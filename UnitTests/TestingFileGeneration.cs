using SupCom2ModPackager;
using SupCom2ModPackager.Collections;
using SupCom2ModPackager.Models;
using SupCom2ModPackager.Utility;

namespace UnitTests
{
    public class TestingFileGeneration
    {
        private static readonly string TestPathFull = @"D:\SC2MODS\Testing\Full";
        private static readonly string TestPathStubs = @"D:\SC2MODS\Testing\Stubs";
        private readonly IServiceProvider _serviceProvider;

        public TestingFileGeneration()
        {
            _serviceProvider = TestServiceLocator.ConfigureServices();
        }
        [Fact]
        public async Task GenerateStubs()
        {
            if (!Directory.Exists(TestPathFull))
                return;
            //if (Directory.Exists(TestPathStubs))
            //    Directory.Delete(TestPathStubs, true);
            Directory.CreateDirectory(TestPathStubs);
            var items = TestServiceLocator.GetService<DisplayItemCollection>() ?? throw new InvalidOperationException("");
            var packager = TestServiceLocator.GetService<SC2ModPackager>() ?? throw new InvalidOperationException("");
            var progress = new Progress<PackProgressArgs>(args =>
            {
            });
            foreach (var file in Directory.GetFiles(TestPathFull, "*"))
            {
                items.Add(new FileInfo(file));
            }

            foreach (var item in items.Where(i => i is DisplayItemFile).Cast<DisplayItemFile>())
            {
                if (!Directory.Exists(item.UnpackDirectoryPath))
                {
                    await packager.UnpackAsync(item, false, progress);
                }
            }
            foreach (var directory in Directory.GetDirectories(TestPathFull, "*"))
            {
                Directory.Move(directory, Path.Combine(TestPathStubs, Path.GetFileName(directory)));
            }
        }

    }
}