using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnergyCo.Api.Json;

public sealed class UtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("Date/time value is required.");
        }

        if (HasExplicitOffset(value))
        {
            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
        }

        var unspecifiedDateTime = DateTime.Parse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces);

        return new DateTimeOffset(DateTime.SpecifyKind(unspecifiedDateTime, DateTimeKind.Utc));
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
    }

    private static bool HasExplicitOffset(string value)
    {
        if (value.EndsWith('Z') || value.EndsWith('z'))
        {
            return true;
        }

        var timeSeparatorIndex = value.IndexOf('T');
        if (timeSeparatorIndex < 0)
        {
            timeSeparatorIndex = value.IndexOf(' ');
        }

        if (timeSeparatorIndex < 0)
        {
            return false;
        }

        return value.IndexOf('+', timeSeparatorIndex) >= 0 ||
            value.IndexOf('-', timeSeparatorIndex) >= 0;
    }
}
