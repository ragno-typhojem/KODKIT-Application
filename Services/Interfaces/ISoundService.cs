using System.Collections.Generic;
using System.Threading.Tasks;

namespace KodKit.Services.Interfaces
{
    public interface ISoundService
    {
        // Temel Ses İşlemleri
        Task PlayNoteAsync(string note, double duration);
        Task PlayDrumAsync(string drumType, double duration);
        Task PlaySoundAsync(string soundName);
        Task WaitAsync(double seconds);

        // Ses Kontrolü
        Task StopAllSoundsAsync();
        Task StopSoundAsync(string soundId);
        void SetVolume(double volume);

        // Ses Durumu
        bool IsPlaying(string soundId);
        double GetCurrentVolume();

        // Enstrüman Yönetimi
        void SetInstrument(string instrumentName);
        string GetCurrentInstrument();
        IEnumerable<string> GetAvailableInstruments();

        // Özel Ses İşlemleri
        Task PlayCustomSoundAsync(string soundPath);
        Task<bool> RecordSoundAsync(double duration);
        Task<string> SaveRecordedSoundAsync(string name);
    }
}
