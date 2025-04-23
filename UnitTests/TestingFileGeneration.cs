using SupCom2ModPackager;
using SupCom2ModPackager.Collections;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Models;
using SupCom2ModPackager.Utility;

namespace UnitTests
{
    public class TestingFileGeneration
    {
        private readonly IServiceProvider _serviceProvider;

        public TestingFileGeneration()
        {
            _serviceProvider = TestServiceLocator.ConfigureServices();
        }
        [Fact]
        public async Task GenerateStubs()
        {
            var testPath = DriveInfo
                .GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => $@"{d.Name}SC2MODS\Testing")
                .First(Directory.Exists);
            var testPathFull = $@"{testPath}\Full";
            var testPathStubs = $@"{testPath}\Stubs";
            if (!Directory.Exists(testPathFull))
                return;
            //if (Directory.Exists(TestPathStubs))
            //    Directory.Delete(TestPathStubs, true);
            Directory.CreateDirectory(testPathStubs);
            var items = TestServiceLocator.GetService<DisplayItemCollection>() ?? throw new InvalidOperationException("");
            var packager = TestServiceLocator.GetService<SC2ModPackager>() ?? throw new InvalidOperationException("");
            var progress = new Progress<PackProgressArgs>(args =>
            {
            });
            var collection = TestServiceLocator.GetService<DisplayItemCollection>() ?? throw new InvalidOperationException("");
            var monitor = TestServiceLocator.GetService<DisplayItemCollectionMonitor>() ?? throw new InvalidOperationException("");
            PropertySyncManager.Sync(collection, monitor, instance => instance.Path, instance => instance.Path);
            collection.Path = testPathFull;
            

            foreach (var item in collection.Files.ToList())
            {
                if (!Directory.Exists(item.UnpackDirectoryPath))
                {
                    await packager.UnpackAsync(item, false, progress);
                }
            }
            foreach (var item in collection.Directories.ToList())
            {
                var target = Path.Combine(testPathStubs, item.Name);
                Directory.Move(item.FullPath, target);
            }
        }

    }
}