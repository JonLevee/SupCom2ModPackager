using System.ComponentModel;

namespace SupCom2ModPackager.Utility
{
    public class SharedPropertyOrchestrator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string CurrentDirectory { get; set; } = string.Empty;


    }
}

