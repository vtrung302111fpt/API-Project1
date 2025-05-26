using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace API_Project1
{
    public class CustomDateTimeConverter : JsonConverter<DateTime?>
    {
        private readonly string _format = "dd/MM/yyyy HH:mm:ss";
        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (DateTime.TryParseExact(value, _format, _culture, DateTimeStyles.None, out var date))
                return date;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(_format));
            else
                writer.WriteNullValue();
        }
    }
}
