using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Spawning Configuration")]
    public GameObject powerUpBoxPrefab;
    public BezierSpline trackSpline;
    public int numberOfBoxesPerRow = 3;
    public float laneOffsetAmount;

    [Tooltip("Distance along the spline between each row of power-up boxes.")]
    public float spawnIntervalDistance = 50f;

    [Tooltip("Offset from the start of the spline to begin spawning.")]
    public float startOffsetDistance = 20f;

    [Tooltip("How high above the spline point the boxes should spawn before potentially raycasting down.")]
    public float spawnHeightOffset = 0.5f;

    [Tooltip("Layer mask for the ground to place boxes accurately.")]
    public LayerMask groundLayer;

    [Header("Spline Search Accuracy")]
    [Tooltip("Higher values are more accurate for finding T at a distance but slower at startup. 100-200 is often a good balance.")]
    public int splineSearchAccuracy = 100;

    [Tooltip("The approximate half-height of the power-up box, from its pivot to its base.")]
    public float boxPivotToBottomOffset = 0.5f;

    [Header("Management")]
    public Transform spawnedBoxesParent;

    void Start()
    {
        if (powerUpBoxPrefab == null)
        {
            Debug.LogError("PowerUpBox Prefab not assigned to PowerUpSpawner!");
            enabled = false;
            return;
        }
        if (trackSpline == null)
        {
            Debug.LogError("Track Spline not assigned to PowerUpSpawner!");
            enabled = false;
            return;
        }
        if (numberOfBoxesPerRow != 3)
        {
            Debug.LogWarning("PowerUpSpawner is designed for 3 boxes per row (lanes). Current setting is " + numberOfBoxesPerRow);
        }
        if (laneOffsetAmount <= 0)
        {
            Debug.LogError("Lane Offset Amount must be greater than 0.");
            enabled = false;
            return;
        }

        GenerateInitialPowerUpBoxes();
    }

    void GenerateInitialPowerUpBoxes()
    {
        // GetLengthApproximately is available and good for this.
        // The third parameter is 'accuracy'; higher is more accurate but slower.
        float splineLength = trackSpline.GetLengthApproximately(0f, 1f, splineSearchAccuracy * 0.1f); // Use a fraction of search for overall length

        if (splineLength <= 0)
        {
            Debug.LogError("Spline length is zero or invalid. Cannot spawn power-up boxes.");
            return;
        }

        float currentDistance = startOffsetDistance;

        while (currentDistance < splineLength - spawnIntervalDistance) // Ensure we don't spawn too close to the end
        {
            // Find the normalized T value that corresponds to currentDistance
            float normalizedT = FindTAtDistance(currentDistance, splineLength);

            if (normalizedT >= 0f) // Check if a valid T was found
            {
                SpawnRowOfBoxes(normalizedT);
            }
            currentDistance += spawnIntervalDistance;
        }
    }

    /// <summary>
    /// Finds the normalized T value on the spline that is closest to the target world distance.
    /// This is a manual search since GetNormalisedTAtDistance isn't directly available.
    /// </summary>
    private float FindTAtDistance(float targetDistance, float estimatedTotalLength)
    {
        if (targetDistance <= 0) return 0f;
        if (targetDistance >= estimatedTotalLength) return 1f;

        float bestT = 0f;
        float closestDistanceFound = float.MaxValue;

        // Iterative search for the T value
        // A simple linear search is often sufficient for setup.
        // For higher accuracy or very long/complex splines, a binary search could be implemented.
        for (int i = 0; i <= splineSearchAccuracy; i++)
        {
            float currentT = (float)i / splineSearchAccuracy;
            // GetLengthApproximately from 0 to currentT
            float distanceAtCurrentT = trackSpline.GetLengthApproximately(0f, currentT, splineSearchAccuracy * 0.05f);

            float diff = Mathf.Abs(distanceAtCurrentT - targetDistance);

            if (diff < closestDistanceFound)
            {
                closestDistanceFound = diff;
                bestT = currentT;
            }

            // Optimization: if we've passed the target distance significantly,
            // and the spline is generally moving forward, we can stop early.
            // This assumes the spline doesn't loop back on itself in terms of distance calculation.
            if (distanceAtCurrentT > targetDistance + spawnIntervalDistance * 0.5f && i > splineSearchAccuracy * 0.1f)
            {
                break;
            }
        }
        return bestT;
    }


    void SpawnRowOfBoxes(float normalizedT)
    {
        if (normalizedT < 0f || normalizedT > 1f) return;

        Vector3 pointOnSplineCenter = trackSpline.GetPoint(normalizedT);
        Vector3 tangent = trackSpline.GetTangent(normalizedT);

        Vector3 forwardDirectionFlat = tangent;
        forwardDirectionFlat.y = 0;
        if (forwardDirectionFlat.sqrMagnitude < 0.001f)
        {
            Vector3 splineNormal = trackSpline.GetNormal(normalizedT);
            forwardDirectionFlat = Vector3.Cross(splineNormal, Vector3.right).normalized;
            if (forwardDirectionFlat.sqrMagnitude < 0.001f) forwardDirectionFlat = transform.forward; // Use spawner's forward
        }
        forwardDirectionFlat.Normalize();
        Vector3 rightDirectionHorizontal = Vector3.Cross(forwardDirectionFlat, Vector3.up).normalized;
        if (rightDirectionHorizontal == Vector3.zero) rightDirectionHorizontal = transform.right; // Fallback if forwardDirectionFlat was vertical

        float[] laneMultipliers = { -1f, 0f, 1f };

        GameObject rowParent = null;
        if (spawnedBoxesParent != null)
        {
            rowParent = new GameObject("PowerUpRow_T" + normalizedT.ToString("F2"));
            rowParent.transform.SetParent(spawnedBoxesParent);
            rowParent.transform.position = pointOnSplineCenter;
        }

        for (int i = 0; i < numberOfBoxesPerRow; i++)
        {
            float offsetMagnitude = laneMultipliers[i] * laneOffsetAmount;
            // Start with the base position on the spline, adjusted for lane
            Vector3 baseLanePosition = pointOnSplineCenter + (rightDirectionHorizontal * offsetMagnitude);

            // Raycast origin: from above the baseLanePosition
            Vector3 raycastOrigin = baseLanePosition + Vector3.up * spawnHeightOffset; // Ensure spawnHeightOffset is sufficient (e.g., 1f or 2f)
            Vector3 finalSpawnPosition = baseLanePosition; // Fallback position

            RaycastHit hit;
            float raycastMaxDistance = spawnHeightOffset + 2f; // Raycast further down than the offset itself

            if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, raycastMaxDistance, groundLayer))
            {
                // Place the box so its bottom (considering pivot) is on the hit point
                finalSpawnPosition = hit.point + Vector3.up * boxPivotToBottomOffset;
            }
            else
            {
                // Fallback if no ground found, place it relative to spline center's Y, adjusted by pivot offset
                finalSpawnPosition.y = pointOnSplineCenter.y + boxPivotToBottomOffset;
                Debug.LogWarning($"No ground found via raycast for power-up box at T={normalizedT}, lane index {i}. Placing relative to spline height.");
            }

            // --- Rotation ---
            Quaternion trackAlignmentRotation = Quaternion.LookRotation(forwardDirectionFlat, Vector3.up);
            Quaternion localXCorrection = Quaternion.Euler(-90f, 0f, 0f);
            Quaternion spawnRotation = trackAlignmentRotation * localXCorrection;

            GameObject boxInstance = Instantiate(powerUpBoxPrefab, finalSpawnPosition, spawnRotation);

            if (rowParent != null)
            {
                boxInstance.transform.SetParent(rowParent.transform);
            }
            else if (spawnedBoxesParent != null)
            {
                boxInstance.transform.SetParent(spawnedBoxesParent);
            }
        }
    }
}
