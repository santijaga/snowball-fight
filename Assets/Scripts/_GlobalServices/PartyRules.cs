using UnityEngine;

public static class PartyRules
{
    public const int MinimumStat = 1;
    public const int MaximumStat = 99;
    public const int MaximumLevel = 99;
    public const int MaximumMembers = 3;

    public static int ClampStat(int value)
    {

        return Mathf.Clamp(value, MinimumStat, MaximumStat);
    }

    public static int CalculateMaximumHealth(int strength)
    {
        double offset = ClampStat(strength) - 5;
        // Continue the curve below 5 without making lower strength increase health.
        double health = 15 + 9984 * offset * System.Math.Abs(offset) / (94 * 94);

        return Mathf.Clamp((int)System.Math.Round(health, System.MidpointRounding.AwayFromZero), 1, 9999);
    }

    public static int CalculateMaximumMana(int intellect)
    {
        double value = ClampStat(intellect);
        double mana = 999 * value * value / (99 * 99);

        return Mathf.Clamp((int)System.Math.Round(mana, System.MidpointRounding.AwayFromZero), 0, 999);
    }
}
