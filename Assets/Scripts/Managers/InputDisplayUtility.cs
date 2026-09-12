using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDisplayUtility : MonoBehaviour
{
    // Dictionary to map KeyCodes to display strings
    private static readonly Dictionary<KeyCode, string> keyDisplayNames = new Dictionary<KeyCode, string>
    {
        // Numbers
        { KeyCode.Alpha0, "0" }, { KeyCode.Alpha1, "1" }, { KeyCode.Alpha2, "2" },
        { KeyCode.Alpha3, "3" }, { KeyCode.Alpha4, "4" }, { KeyCode.Alpha5, "5" },
        { KeyCode.Alpha6, "6" }, { KeyCode.Alpha7, "7" }, { KeyCode.Alpha8, "8" },
        { KeyCode.Alpha9, "9" },
        // Numpad Numbers (Optional, if you use them)
        { KeyCode.Keypad0, "Num 0" }, { KeyCode.Keypad1, "Num 1" }, { KeyCode.Keypad2, "Num 2" },
        { KeyCode.Keypad3, "Num 3" }, { KeyCode.Keypad4, "Num 4" }, { KeyCode.Keypad5, "Num 5" },
        { KeyCode.Keypad6, "Num 6" }, { KeyCode.Keypad7, "Num 7" }, { KeyCode.Keypad8, "Num 8" },
        { KeyCode.Keypad9, "Num 9" },
        // Special Characters
        { KeyCode.Semicolon, ";" },
        { KeyCode.Period, "." },
        { KeyCode.Comma, "," },
        { KeyCode.Slash, "/" },
        { KeyCode.Backslash, "\\" },
        { KeyCode.Quote, "'" },
        { KeyCode.Minus, "-" },
        { KeyCode.Equals, "=" },
        { KeyCode.LeftBracket, "[" },
        { KeyCode.RightBracket, "]" },
        // Add more as needed...
        // For letters, KeyCode.ToString() is usually fine (e.g., "A", "B", "C")
        // but you could override them too if you wanted lowercase, for example.
        // { KeyCode.A, "a" },
    };

    public static string GetDisplayString(KeyCode keyCode)
    {
        if (keyDisplayNames.TryGetValue(keyCode, out string displayName))
        {
            return displayName;
        }
        return keyCode.ToString(); // Fallback
    }
}
