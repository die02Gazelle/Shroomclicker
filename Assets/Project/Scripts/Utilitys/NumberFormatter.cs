using UnityEngine;

public class NumberFormatter : MonoBehaviour
{
    public NumberFormat numberFormat = NumberFormat.Normal;

    public enum NumberFormat
    {
        Normal,
        Short
    }
    string FormatNumber(float num)
    {
        if (numberFormat == NumberFormat.Normal)
            return num.ToString("N0");

        // kurze Notation
        if (num >= 1_000_000_000_000) return (num / 1_000_000_000_000f).ToString("0.#") + "T";
        if (num >= 1_000_000_000) return (num / 1_000_000_000f).ToString("0.#") + "B";
        if (num >= 1_000_000) return (num / 1_000_000f).ToString("0.#") + "M";
        if (num >= 10_000) return (num / 1_000f).ToString("0.#") + "K";
        return num.ToString("N0");
    }
}
