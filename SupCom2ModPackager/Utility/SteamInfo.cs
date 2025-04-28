using Microsoft.Win32;
using System.IO;
using ValveKeyValue;

namespace SupCom2ModPackager.Utility
{

    public class SteamApp
    {
        public string AppId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string InstallDir { get; set; } = string.Empty;
    }
    public class SteamApps : Dictionary<string, SteamApp>
    {
    }
    public class SteamInfo
    {
        private readonly SteamApps steamApps = [];
        public string GetRoot(string appName)
        {
            if (!steamApps.Any())
            {
                var regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var steamPath = regKey?.GetValue("SteamPath")?.ToString()
                    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
                var steamLibraryFolderFile = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
                var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
                SteamLibraryFolder[] steamLibraryFolders = Array.Empty<SteamLibraryFolder>();
                using (var stream = File.OpenRead(steamLibraryFolderFile))
                {
                    steamLibraryFolders = kv.Deserialize<SteamLibraryFolder[]>(stream);

                    foreach (var steamLibraryFolder in steamLibraryFolders)
                    {
                        if (steamLibraryFolder.Path != null)
                        {
                            var libraryPath = Path.Combine(steamLibraryFolder.Path, "steamapps");
                            foreach (var manifest in Directory.GetFiles(libraryPath, "*.acf"))
                            {
                                using (var manifestStream = File.OpenRead(manifest))
                                {
                                    var app = kv.Deserialize<SteamLibraryApp>(manifestStream);
                                    if (steamLibraryFolder.Apps.ContainsKey(app.AppId))
                                    {
                                        var steamApp = new SteamApp
                                        {
                                            AppId = app.AppId,
                                            Name = app.Name,
                                            InstallDir = Path.Combine(
                                                steamLibraryFolder.Path.Replace("\\\\", "\\"),
                                                "steamapps",
                                                "common", 
                                                app.InstallDir)
                                        };
                                        steamApps.Add(app.Name, steamApp);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var requestedApp = steamApps[appName];
            var root = requestedApp.InstallDir;

            return root;
        }
        private class SteamLibraryFolder
        {
            public string Path { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
            public long ContentId { get; set; } = 0;
            public long TotalSize { get; set; } = 0;
            public Dictionary<string, string> Apps { get; set; } = [];
        }
        private class SteamLibraryApp
        {
            public string AppId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string InstallDir { get; set; } = string.Empty;
        }
        private class SteamLibraryApps : Dictionary<int, SteamLibraryApp>
        {
        }
    }

}
