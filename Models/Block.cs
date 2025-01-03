using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation;

namespace KodKit.Models
{
    public class Block : INotifyPropertyChanged
    {
        private Guid _id = Guid.NewGuid();
        public Guid Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _type = string.Empty;
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                Category = DetermineCategory(_type);
                Parameter = GetDefaultParameter(_type);
                OnPropertyChanged();
            }
        }

        private string _category = "Unknown";
        public string Category
        {
            get => _category;
            private set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        private string _parameter = string.Empty;
        public string Parameter
        {
            get => _parameter;
            set
            {
                _parameter = value;
                OnPropertyChanged();
            }
        }

        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged();
            }
        }

        private bool _isTopLevel;
        public bool IsTopLevel
        {
            get => _isTopLevel;
            set
            {
                _isTopLevel = value;
                OnPropertyChanged();
            }
        }

        private Block? _nextBlock;
        public Block? NextBlock
        {
            get => _nextBlock;
            set
            {
                _nextBlock = value;
                OnPropertyChanged();
            }
        }

        private List<Block> _innerBlocks = new();
        public List<Block> InnerBlocks
        {
            get => _innerBlocks;
            set
            {
                _innerBlocks = value;
                OnPropertyChanged();
            }
        }

        private double _duration = 1.0;
        public double Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, object> _parameters = new();
        public Dictionary<string, object> Parameters
        {
            get => _parameters;
            set
            {
                _parameters = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string DetermineCategory(string blockType)
        {
            return blockType switch
            {
                string type when type.Contains("Move") || type.Contains("Turn") || type.Contains("Position") => "Motion",
                string type when type.Contains("Say") || type.Contains("Sprite") || type.Contains("Costume") => "Looks",
                string type when type.Contains("Sound") || type.Contains("Play") || type.Contains("Note") => "Sound",
                string type when type.Contains("When") => "Events",
                string type when type.Contains("Wait") || type.Contains("Repeat") || type.Contains("If") => "Control",
                string type when type.Contains("Variable") => "Variables",
                _ => "Unknown"
            };
        }

        private string GetDefaultParameter(string blockType)
        {
            return blockType switch
            {
                string type when type.Contains("MoveSteps") => "10",
                string type when type.Contains("TurnRight") || type.Contains("TurnLeft") => "90",
                string type when type.Contains("GoToPosition") => "0, 0",
                string type when type.Contains("SetXPosition") || type.Contains("SetYPosition") => "0",
                string type when type.Contains("SayMessage") => "Merhaba!",
                string type when type.Contains("PlaySound") => "pop",
                string type when type.Contains("PlayNote") => "60",
                string type when type.Contains("PlayDrum") => "1",
                string type when type.Contains("Wait") => "1",
                string type when type.Contains("Repeat") => "10",
                string type when type.Contains("WaitUntil") => "true",
                string type when type.Contains("SetVariable") => "0",
                string type when type.Contains("ChangeVariable") => "1",
                _ => string.Empty
            };
        }

        public T GetParameterValue<T>(string parameterName)
        {
            if (Parameters.TryGetValue(parameterName, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    throw new InvalidCastException($"Cannot convert parameter {parameterName} to type {typeof(T).Name}");
                }
            }
            throw new KeyNotFoundException($"Parameter {parameterName} not found");
        }
    }
}
