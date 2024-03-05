using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace BiliBiliDownloader.Wpf.Models.Response
{
    public record SubtitleJsonResponse
    {
        [JsonProperty("font_size")]
        [JsonPropertyName("font_size")]
        public double FontSize { get; set; }

        [JsonProperty("font_color")]
        [JsonPropertyName("font_color")]
        public string FontColor { get; set; } = string.Empty;

        [JsonProperty("background_alpha")]
        [JsonPropertyName("background_alpha")]
        public double BackgroundAlpha { get; set; }

        [JsonProperty("background_color")]
        [JsonPropertyName("background_color")]
        public string BackgroundColor { get; set; } = string.Empty;

        [JsonProperty("Stroke")]
        [JsonPropertyName("Stroke")]
        public string Stroke { get; set; } = string.Empty;

        [JsonProperty("body")]
        [JsonPropertyName("body")]
        public List<SubtitleJsonBody> Body { get; set; } = [];
    }
    public record SubtitleJsonBody
    {
        [JsonProperty("from")]
        [JsonPropertyName("from")]
        public double From { get; set; }

        [JsonProperty("to")]
        [JsonPropertyName("to")]
        public double To { get; set; }

        [JsonProperty("location")]
        [JsonPropertyName("location")]
        public long Location { get; set; }

        [JsonProperty("content")]
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }


}
