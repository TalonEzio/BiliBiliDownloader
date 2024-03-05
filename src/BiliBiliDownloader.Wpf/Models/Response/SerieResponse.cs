using System.Text.Json.Serialization;

namespace BiliBiliDownloader.Wpf.Models.Response
{

    public record SerieResponse
    {
        [JsonPropertyName("code")]
        public long Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("ttl")]
        public long Ttl { get; set; }

        [JsonPropertyName("data")] public SerieData Data { get; set; } = new();
    }

    public record SerieData
    {
        [JsonPropertyName("sections")] public List<SerieSection> Sections { get; set; } = [];
    }

    public record Episode
    {
        [JsonPropertyName("cover")]
        public string Cover { get; set; } = string.Empty;

        [JsonPropertyName("limit")]
        public long Limit { get; set; }

        [JsonPropertyName("limit_text")]
        public string LimitText { get; set; } = string.Empty;

        [JsonPropertyName("episode_id")]
        public string EpisodeId { get; set; } = string.Empty;

        [JsonPropertyName("short_title_display")]
        public string ShortTitleDisplay { get; set; } = string.Empty;

        [JsonPropertyName("long_title_display")]
        public string LongTitleDisplay { get; set; } = string.Empty;

        [JsonPropertyName("title_display")]
        public string TitleDisplay { get; set; } = string.Empty;

        [JsonPropertyName("publish_time")]
        public DateTime PublishTime { get; set; }
    }



    public record SerieSection
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("ep_list_title")]
        public string EpListTitle { get; set; } = string.Empty;

        [JsonPropertyName("episodes")] public List<Episode> Episodes { get; set; } = [];
    }
}
