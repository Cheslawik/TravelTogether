namespace TravelTogether.Domain.Entities;

public enum SplitType
{
    /// <summary>Every participant in the split pays the same amount.</summary>
    Equal = 0,

    /// <summary>The amount is divided proportionally to each participant's shares (e.g. 2:1:1).</summary>
    Shares = 1,
}
