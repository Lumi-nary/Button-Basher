using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;

public class ObstacleGenerator : MonoBehaviour
{
    [Header("Track Settings")]
    public BezierSpline trackSpline;

    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;
    [Range(5, 100)]
    public int numberOfObstacles = 15;
    public float minDistanceBetweenObstacles = 5f; // Min distance along the spline's path

    [Tooltip("The exact offset from the center spline for the left/right lanes. Should match player lane offsets.")]
    public float laneOffsetAmount = 2f; // e.g., if player lanes are +/- 2 units from center

    [Tooltip("Initial height above the calculated lane point before raycasting.")]
    public float raycastOriginHeightOffset = 2f;
    [Tooltip("The approximate distance from the obstacle's pivot point to its very bottom.")]
    public float obstaclePivotToBottomOffset = 0.5f;
    public LayerMask groundLayer;

    [Header("Generation Settings")]
    public bool randomSeed = true;
    public int seed = 12345;
    public bool generateOnStart = true;

    [Header("Debug")]
    public bool showDebugVisuals = true;

    // Stores the normalizedT and the lane index (0=left, 1=center, 2=right) for occupied spots
    private List<KeyValuePair<float, int>> occupiedLaneSpots = new List<KeyValuePair<float, int>>();
    private Transform obstaclesParent;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateObstacles();
        }
    }

    public void GenerateObstacles()
    {
        ClearObstacles();
        obstaclesParent = new GameObject("Obstacles").transform;
        obstaclesParent.SetParent(transform);

        if (randomSeed) Random.InitState((int)System.DateTime.Now.Ticks);
        else Random.InitState(seed);

        if (trackSpline == null) { Debug.LogError("Track spline is not assigned!"); return; }
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) { Debug.LogError("No obstacle prefabs assigned!"); return; }
        if (groundLayer == 0) { Debug.LogWarning("Ground Layer not set. Raycasting might fail."); }
        if (laneOffsetAmount <= 0) { Debug.LogWarning("Lane Offset Amount is 0 or negative. Obstacles will only spawn at center."); }


        int successfulPlacements = 0;
        int attemptLimit = numberOfObstacles * 30; // Higher attempt limit for more complex placement
        int attempts = 0;

        float splineLength = trackSpline.GetLengthApproximately(0f, 1f); // For distance checks

        while (successfulPlacements < numberOfObstacles && attempts < attemptLimit)
        {
            attempts++;

            float normalizedT = Random.Range(0f, 1f); // Random point along the spline
            int laneIndex = Random.Range(0, 3);    // 0 = Left, 1 = Center, 2 = Right

            Vector3 pointOnSpline = trackSpline.GetPoint(normalizedT);
            Vector3 tangent = trackSpline.GetTangent(normalizedT);

            Vector3 forwardDirectionFlat = tangent;
            forwardDirectionFlat.y = 0;
            if (forwardDirectionFlat.sqrMagnitude < 0.001f) forwardDirectionFlat = transform.forward;
            forwardDirectionFlat.Normalize();
            Vector3 rightDirectionHorizontal = Vector3.Cross(forwardDirectionFlat, Vector3.up).normalized;
            if (rightDirectionHorizontal == Vector3.zero) rightDirectionHorizontal = transform.right;

            float actualLaneOffset;
            if (laneOffsetAmount > 0) // Only apply offset if it's meaningful
            {
                switch (laneIndex)
                {
                    case 0: actualLaneOffset = -laneOffsetAmount; break; // Left
                    case 1: actualLaneOffset = 0; break;             // Center
                    case 2: actualLaneOffset = laneOffsetAmount; break;  // Right
                    default: actualLaneOffset = 0; break;
                }
            }
            else
            {
                actualLaneOffset = 0; // If laneOffsetAmount is 0, all go to center
            }


            Vector3 basePositionAttempt = pointOnSpline + rightDirectionHorizontal * actualLaneOffset;

            Vector3 raycastOrigin = basePositionAttempt + Vector3.up * raycastOriginHeightOffset;
            Vector3 finalPlacementPosition = basePositionAttempt;
            finalPlacementPosition.y = pointOnSpline.y + obstaclePivotToBottomOffset;

            RaycastHit hit;
            float raycastMaxDistance = raycastOriginHeightOffset + 5f;

            if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, raycastMaxDistance, groundLayer))
            {
                finalPlacementPosition = hit.point + Vector3.up * obstaclePivotToBottomOffset;
            }
            else
            {
                Debug.LogWarning($"Obstacle at T={normalizedT}, Lane={laneIndex}: No ground. Using spline height.");
            }

            // --- Proximity Check (More Lane Aware) ---
            bool tooClose = false;
            float currentPointDistanceOnSpline = normalizedT * splineLength;

            foreach (KeyValuePair<float, int> occupiedSpot in occupiedLaneSpots)
            {
                float occupiedSpotDistanceOnSpline = occupiedSpot.Key * splineLength;
                float distanceAlongSpline = Mathf.Abs(currentPointDistanceOnSpline - occupiedSpotDistanceOnSpline);

                // If they are in the same lane OR adjacent lanes, AND too close along the spline's path
                if (Mathf.Abs(occupiedSpot.Value - laneIndex) <= 1 && distanceAlongSpline < minDistanceBetweenObstacles)
                {
                    tooClose = true;
                    break;
                }
                // If they are in opposite outer lanes, the spline distance check is usually enough
                // but you could add a stricter XZ world distance check if needed.
            }
            // --- End Proximity Check ---


            if (!tooClose)
            {
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                GameObject obstacle = Instantiate(prefab, finalPlacementPosition, Quaternion.identity);

                if (forwardDirectionFlat != Vector3.zero)
                {
                    obstacle.transform.rotation = Quaternion.LookRotation(forwardDirectionFlat, Vector3.up);
                }
                obstacle.transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self); // Random Y rotation for variety

                obstacle.transform.SetParent(obstaclesParent);
                occupiedLaneSpots.Add(new KeyValuePair<float, int>(normalizedT, laneIndex));
                successfulPlacements++;
            }
        }
        Debug.Log($"Generated {successfulPlacements} obstacles in {attempts} attempts");
    }

    public void ClearObstacles()
    {
        occupiedLaneSpots.Clear();
        if (obstaclesParent != null)
        {
            if (Application.isPlaying) Destroy(obstaclesParent.gameObject);
            else DestroyImmediate(obstaclesParent.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugVisuals || trackSpline == null || !Application.isEditor) return;

        // It's expensive to draw all occupied spots in Gizmos if there are many,
        // so only draw if not in play mode or if explicitly needed.
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.red;
            // Temporarily generate positions for Gizmos if cleared and not playing
            // This is illustrative; for actual editor preview, you might need a separate list
            // or call Generate without clearing if you want to see the last generated set.
            // For simplicity, this will only draw if occupiedLaneSpots is populated.
            foreach (var spot in occupiedLaneSpots)
            {
                Vector3 point = trackSpline.GetPoint(spot.Key);
                // Simplified drawing at spline height for gizmo clarity
                Gizmos.DrawSphere(point, 0.4f);
            }


            Gizmos.color = Color.yellow;
            float step = 0.05f;
            for (float t = 0; t < 1f; t += step)
            {
                Vector3 position = trackSpline.GetPoint(t);
                Vector3 tangent = trackSpline.GetTangent(t);
                Vector3 up = Vector3.up;
                Vector3 right = Vector3.Cross(tangent, up).normalized;

                if (laneOffsetAmount > 0)
                {
                    Gizmos.DrawLine(position + right * -laneOffsetAmount, position + right * laneOffsetAmount); // Line across lanes
                }
                else
                {
                    Gizmos.DrawSphere(position, 0.1f); // Just draw center line if no offset
                }
            }
        }
    }

    [ContextMenu("Generate Obstacles")]
    public void GenerateObstaclesMenu() { GenerateObstacles(); }

    [ContextMenu("Clear Obstacles")]
    public void ClearObstaclesMenu() { ClearObstacles(); }
}
