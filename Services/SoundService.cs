using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KodKit.Services.Interfaces;

namespace KodKit.Services
{
    public class SoundService : ISoundService
    {
        private readonly Dictionary<string, double> _noteFrequencies;
        private readonly Dictionary<string, string> _soundEffects;
        private readonly HashSet<string> _playingSounds;
        private readonly object _lockObject;
        private string _currentInstrument;
        private double _volume;
        private bool _isRecording;

        private const double MinVolume = 0.0;
        private const double MaxVolume = 1.0;
        private const int DefaultSoundDuration = 1000; // milisaniye

        public SoundService()
        {
            _noteFrequencies = new Dictionary<string, double>
            {
                {"C4", 261.63}, {"D4", 293.66}, {"E4", 329.63},
                {"F4", 349.23}, {"G4", 392.00}, {"A4", 440.00},
                {"B4", 493.88}
            };

            _soundEffects = new Dictionary<string, string>();
            _playingSounds = new HashSet<string>();
            _lockObject = new object();
            _currentInstrument = "piano";
            _volume = 1.0;
            _isRecording = false;
        }

        public async Task PlayNoteAsync(string note, double duration)
        {
            if (string.IsNullOrEmpty(note))
                throw new ArgumentNullException(nameof(note));

            if (duration <= 0)
                throw new ArgumentException("Süre 0'dan büyük olmalıdır.", nameof(duration));

            if (_noteFrequencies.ContainsKey(note))
            {
                var soundId = $"note_{note}_{DateTime.Now.Ticks}";
                try
                {
                    lock (_lockObject)
                    {
                        _playingSounds.Add(soundId);
                    }
                    await Task.Delay((int)(duration * 1000));
                }
                finally
                {
                    lock (_lockObject)
                    {
                        _playingSounds.Remove(soundId);
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Geçersiz nota: {note}", nameof(note));
            }
        }

        public async Task PlayDrumAsync(string drumType, double duration)
        {
            if (string.IsNullOrEmpty(drumType))
                throw new ArgumentNullException(nameof(drumType));

            if (duration <= 0)
                throw new ArgumentException("Süre 0'dan büyük olmalıdır.", nameof(duration));

            var soundId = $"drum_{drumType}_{DateTime.Now.Ticks}";
            try
            {
                lock (_lockObject)
                {
                    _playingSounds.Add(soundId);
                }
                await Task.Delay((int)(duration * 1000));
            }
            finally
            {
                lock (_lockObject)
                {
                    _playingSounds.Remove(soundId);
                }
            }
        }

        public async Task PlaySoundAsync(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
                throw new ArgumentNullException(nameof(soundName));

            var soundId = $"sound_{soundName}_{DateTime.Now.Ticks}";
            try
            {
                lock (_lockObject)
                {
                    _playingSounds.Add(soundId);
                }
                await Task.Delay(DefaultSoundDuration);
            }
            finally
            {
                lock (_lockObject)
                {
                    _playingSounds.Remove(soundId);
                }
            }
        }

        public async Task WaitAsync(double seconds)
        {
            if (seconds <= 0)
                throw new ArgumentException("Süre 0'dan büyük olmalıdır.", nameof(seconds));

            await Task.Delay((int)(seconds * 1000));
        }

        public async Task StopAllSoundsAsync()
        {
            lock (_lockObject)
            {
                _playingSounds.Clear();
            }
            await Task.CompletedTask;
        }

        public async Task StopSoundAsync(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                throw new ArgumentNullException(nameof(soundId));

            lock (_lockObject)
            {
                _playingSounds.Remove(soundId);
            }
            await Task.CompletedTask;
        }

        public void SetVolume(double volume)
        {
            if (volume < MinVolume || volume > MaxVolume)
                throw new ArgumentOutOfRangeException(nameof(volume),
                    $"Ses seviyesi {MinVolume} ile {MaxVolume} arasında olmalıdır.");

            _volume = volume;
        }

        public bool IsPlaying(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                throw new ArgumentNullException(nameof(soundId));

            lock (_lockObject)
            {
                return _playingSounds.Contains(soundId);
            }
        }

        public double GetCurrentVolume()
        {
            return _volume;
        }

        public void SetInstrument(string instrumentName)
        {
            if (string.IsNullOrEmpty(instrumentName))
                throw new ArgumentNullException(nameof(instrumentName));

            if (!GetAvailableInstruments().Contains(instrumentName))
                throw new ArgumentException($"Desteklenmeyen enstrüman: {instrumentName}", nameof(instrumentName));

            _currentInstrument = instrumentName;
        }

        public string GetCurrentInstrument()
        {
            return _currentInstrument;
        }

        public IEnumerable<string> GetAvailableInstruments()
        {
            return new[] { "piano", "guitar", "violin", "flute" };
        }

        public async Task PlayCustomSoundAsync(string soundPath)
        {
            if (string.IsNullOrEmpty(soundPath))
                throw new ArgumentNullException(nameof(soundPath));

            if (!File.Exists(soundPath))
                throw new FileNotFoundException("Ses dosyası bulunamadı.", soundPath);

            var soundId = $"custom_{Path.GetFileName(soundPath)}_{DateTime.Now.Ticks}";
            try
            {
                lock (_lockObject)
                {
                    _playingSounds.Add(soundId);
                }
                await Task.Delay(DefaultSoundDuration);
            }
            finally
            {
                lock (_lockObject)
                {
                    _playingSounds.Remove(soundId);
                }
            }
        }

        public async Task<bool> RecordSoundAsync(double duration)
        {
            if (duration <= 0)
                throw new ArgumentException("Süre 0'dan büyük olmalıdır.", nameof(duration));

            if (_isRecording)
                throw new InvalidOperationException("Kayıt zaten devam ediyor.");

            try
            {
                _isRecording = true;
                await Task.Delay((int)(duration * 1000));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _isRecording = false;
            }
        }

        public async Task<string> SaveRecordedSoundAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"recorded_{safeName}_{DateTime.Now.Ticks}.wav";

            try
            {
                await Task.Delay(100);
                return fileName;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ses kaydı kaydedilemedi.", ex);
            }
        }
    }
}
