namespace TravelTogether.Domain.Common;

internal static class Guard
{
    public static string RequiredText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{fieldName} cannot be longer than {maxLength} characters.");
        }

        return trimmed;
    }

    public static string? OptionalText(string? value, int maxLength, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : RequiredText(value, maxLength, fieldName);
    }
}
