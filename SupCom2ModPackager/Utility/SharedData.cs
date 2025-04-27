using System.ComponentModel;

namespace SupCom2ModPackager.Utility
{
    public class SharedData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _currentPath = string.Empty;
        public string CurrentPath { 
            get => _currentPath;
            set
            {
                if (!EqualityComparer<string>.Default.Equals(_currentPath, value))
                {
                    _currentPath = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPath)));
                }
            }
        }
    }
}
