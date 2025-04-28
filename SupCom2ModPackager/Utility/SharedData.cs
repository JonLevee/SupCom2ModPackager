using System.ComponentModel;
using System.IO.Packaging;

namespace SupCom2ModPackager.Utility
{
    public class SharedData
    {
        public event EventHandler? CurrentPathChanged;

        private string _currentPath = string.Empty;
        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                if (!EqualityComparer<string>.Default.Equals(_currentPath, value))
                {
                    _currentPath = value;
                    CurrentPathChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
