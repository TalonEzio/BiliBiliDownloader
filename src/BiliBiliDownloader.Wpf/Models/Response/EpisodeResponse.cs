using BiliBiliDownloader.Wpf.Common;
using System.Text.Json.Serialization;

namespace BiliBiliDownloader.Wpf.Models.Response
{
    public record Audio
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("quality")]
        public long Quality { get; set; }

        [JsonPropertyName("bandwidth")]
        public long Bandwidth { get; set; }

        [JsonPropertyName("codec_id")]
        public long CodecId { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("md5")]
        public string Md5 { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("backup_url")] public object BackupUrl { get; set; } = new();

        [JsonPropertyName("container")]
        public long Container { get; set; }

        [JsonPropertyName("start_with_sap")]
        public long StartWithSap { get; set; }

        [JsonPropertyName("codecs")]
        public string Codecs { get; set; } = string.Empty;

        [JsonPropertyName("sar")]
        public string Sar { get; set; } = string.Empty;

        [JsonPropertyName("frame_rate")]
        public string FrameRate { get; set; } = string.Empty;

        [JsonPropertyName("segment_base")] public SegmentBase SegmentBase { get; set; } = new();

        [JsonPropertyName("width")]
        public long Width { get; set; }

        [JsonPropertyName("height")]
        public long Height { get; set; }

        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = string.Empty;
    }

    public record EpisodeData
    {
        [JsonPropertyName("playurl")] public Playurl Playurl { get; set; } = new();

        [JsonPropertyName("watermark")] public Watermark Watermark { get; set; } = new();

        [JsonPropertyName("in_stream_ad")] public object InStreamAd { get; set; } = new();
    }

    public record Playurl
    {
        [JsonPropertyName("quality")]
        public long Quality { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("expire_at")]
        public long ExpireAt { get; set; }

        [JsonPropertyName("video")] public List<Video> Videos { get; set; } = [];

        [JsonPropertyName("audio_resource")] public List<Audio> Audios { get; set; } = [];
    }

    public record EpisodeResponse
    {
        [JsonPropertyName("code")]
        public long Code { get; init; }

        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("ttl")]
        public long Ttl { get; init; }

        [JsonPropertyName("data")] public EpisodeData EpisodeData { get; init; } = new();

        public string? Name { get; set; }
    }

    public record SegmentBase
    {
        [JsonPropertyName("range")]
        public string Range { get; set; } = string.Empty;

        [JsonPropertyName("index_range")]
        public string IndexRange { get; set; } = string.Empty;
    }

    public record StreamInfo
    {
        [JsonPropertyName("quality")]
        public VideoQuality Quality { get; set; }

        [JsonPropertyName("desc_text")]
        public string DescText { get; set; } = string.Empty;

        [JsonPropertyName("desc_words")]
        public string DescWords { get; set; } = string.Empty;

        [JsonPropertyName("longact")]
        public bool Intact { get; set; }

        [JsonPropertyName("no_rexcode")]
        public bool NoRexcode { get; set; }
    }

    public record Video
    {
        [JsonPropertyName("video_resource")] public VideoResource VideoResource { get; set; } = new();

        [JsonPropertyName("stream_info")] public StreamInfo StreamInfo { get; set; } = new();

        [JsonPropertyName("audio_quality")]
        public long AudioQuality { get; set; }
    }

    public record VideoResource
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("quality")]
        public VideoQuality Quality { get; set; }

        [JsonPropertyName("bandwidth")]
        public long Bandwidth { get; set; }

        [JsonPropertyName("codec_id")]
        public long CodecId { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("md5")]
        public string Md5 { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("backup_url")]
        public string? BackupUrl { get; set; } = string.Empty;

        [JsonPropertyName("container")]
        public long Container { get; set; }

        [JsonPropertyName("start_with_sap")]
        public long StartWithSap { get; set; }

        [JsonPropertyName("codecs")]
        public string Codecs { get; set; } = string.Empty;

        [JsonPropertyName("sar")]
        public string Sar { get; set; } = string.Empty;

        [JsonPropertyName("frame_rate")]
        public string FrameRate { get; set; } = string.Empty;

        [JsonPropertyName("segment_base")] public SegmentBase SegmentBase { get; set; } = new();

        [JsonPropertyName("width")]
        public long Width { get; set; }

        [JsonPropertyName("height")]
        public long Height { get; set; }

        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = string.Empty;
    }

    public record Watermark
    {
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

}
