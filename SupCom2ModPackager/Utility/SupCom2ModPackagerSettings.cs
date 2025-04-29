using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupCom2ModPackager.Extensions;

namespace SupCom2ModPackager.Utility
{
    public class SupCom2ModPackagerSettings
    {
        public string AppName { get; set; } = "SupCom2ModPackager";
        public string AppVersion { get; set; } = "1.0.0";
        public string ApplicationDataFolder { get; set; } = string.Empty;
        public string ApplicationRootFolder => Path.Combine(ApplicationDataFolder, AppVersion);

        public string UserSettingsFile => Path.Combine(ApplicationRootFolder, "userSettings.json");

        public string ModInstalledFolder => Path.Combine(ApplicationRootFolder, "ModInstalled");
        public string ModLibraryFolder => Path.Combine(ApplicationRootFolder, "ModLibrary");
        public string ModEditingFolder => Path.Combine(ApplicationRootFolder, "ModEditing");
    }
}
