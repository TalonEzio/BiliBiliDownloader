using System.Text.Json.Serialization;

namespace BiliBiliDownloader.Wpf.Models.Response
{
    public record Ass
    {
        [JsonPropertyName("subtitle_id")]
        public long SubtitleId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    public record SubtitleResponse
    {
        [JsonPropertyName("code")]
        public long Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("ttl")]
        public long Ttl { get; set; }

        [JsonPropertyName("data")] public SubtitleData SubtitleData { get; init; } = new();
    }
    public record SubtitleData
    {
        [JsonPropertyName("subtitles")] public List<Subtitle> Subtitles { get; set; } = [];

        [JsonPropertyName("video_subtitle")] public List<VideoSubtitle> VideoSubtitles { get; set; } = [];
    }

    public record Srt
    {
        [JsonPropertyName("subtitle_id")]
        public long SubtitleId { get; set; }

        [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    }

    public record Subtitle
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("lang")]
        public string Lang { get; set; } = string.Empty;

        [JsonPropertyName("lang_key")]
        public string LangKey { get; set; } = string.Empty;

        [JsonPropertyName("subtitle_id")]
        public long SubtitleId { get; set; }
    }

    public record VideoSubtitle
    {
        [JsonPropertyName("lang")]
        public string Lang { get; set; } = string.Empty;

        [JsonPropertyName("lang_key")]
        public string LangKey { get; set; } = string.Empty;

        [JsonPropertyName("srt")] public Srt Srt { get; set; } = new();

        [JsonPropertyName("ass")] public Ass Ass { get; set; } = new();
    }
}
