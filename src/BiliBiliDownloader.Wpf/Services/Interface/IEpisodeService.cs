using BiliBiliDownloader.Wpf.Models.Response;
using BiliBiliDownloader.Wpf.Models.Services;

namespace BiliBiliDownloader.Wpf.Services.Interface
{
    public interface IEpisodeService
    {
        Task<List<VideoSaveInfo>> SaveVideoEpisode(EpisodeResponse episode, string savePath, Predicate<Video>? predicate = null);
        Task<List<AudioSaveInfo>> SaveAudioEpisode(EpisodeResponse episode, string savePath, Predicate<Audio>? predicate = null);
        Task<EpisodeSaveInfo> SaveEpisode(EpisodeResponse episode, string savePath, Predicate<Video>? videoPredicate = null, Predicate<Audio>? audioPredicate = null);
        Task<EpisodeResponse?> GetEpisode(long episodeId);
        Task<string> GetEpisodeName(long episodeId, long serieId);
    }
}
