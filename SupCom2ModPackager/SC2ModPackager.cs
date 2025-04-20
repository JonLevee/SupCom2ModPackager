
using SupCom2ModPackager.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Shapes;

namespace SupCom2ModPackager;

public class SC2ModPackager
{
    private readonly DisplayItemCollection _items;
    private readonly ProgressReporter progressReporter;

    public SC2ModPackager(DisplayItemCollection items, ProgressReporter progressReporter)
    {
        this._items = items;
        this.progressReporter = progressReporter;
    }

    public async Task Unpack(DisplayItem item)
    {
        using (progressReporter.CreateReporter())
        {
            progressReporter.Visibility = Visibility.Visible;
            progressReporter.Minimum = progressReporter.Maximum = progressReporter.Value = 0;
            progressReporter.Text = string.Empty;

            var directory = Path.ChangeExtension(item.Path, null);
            if (Directory.Exists(directory))
            {
                //var result = MessageBox.Show("Overwrite directory?", "Unpack will overwrite existing directory", MessageBoxButton.OKCancel);
                //if (result != MessageBoxResult.OK)
                //{
                //    return;
                //}
                progressReporter.Report("Removing folder ...");
                await Task.Run(() => Directory.Delete(directory, true));

            }

            var targetDirectory = _items.FirstOrDefault(dirItem => string.Equals(directory, dirItem.Path, StringComparison.OrdinalIgnoreCase));
            if (targetDirectory == null)
            {
                Directory.CreateDirectory(directory);
                targetDirectory = _items.Add(DisplayItemType.Directory, directory);

                await Task.CompletedTask;
            }


            progressReporter.Report("Scanning ...");
            var count = await Task.Run(() => GetCountOfItemsToExtract(item.Path));
            progressReporter.Maximum = count;


            // Perform the extraction
            if (!item.Path.IsCompressedFile())
            {

            }
            await Task.Run(() => UnpackAsync(item.Path, progress));

            progressReporter.Visibility = Visibility.Hidden;
        }
    }



    private static async Task UnpackAsync(string path, IProgress<string> progress)
    {
        if (!path.IsCompressedFile())
        {
            return;
        }

        var directory = Path.ChangeExtension(path, null);
        Directory.CreateDirectory(directory);

        using (var zipFile = ZipFile.OpenRead(path))
        {
            foreach (var entry in zipFile.Entries)
            {
                if (entry.FullName.EndsWith("/"))
                {
                    continue;
                }

                var filePath = Path.Combine(directory, entry.FullName);
                var fileDirectory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(fileDirectory) && !Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                // Extract the file
                entry.ExtractToFile(filePath, overwrite: true);

                progress.Report($"Extracting {entry.FullName}");

                // Recursively unpack nested archives
                await UnpackAsync(filePath, progress);
            }
        }
    }

    private static async Task<int> GetCountOfItemsToExtract(string path)
    {
        if (!path.IsCompressedFile())
        {
            return await Task.FromResult(0); // Use Task.FromResult to return a completed task with a result
        }

        return await Task.Run(() => // Use Task.Run to perform CPU-bound work on a background thread
        {
            using var zipFile = ZipFile.OpenRead(path);
            return GetCountForAllEntries(zipFile);
        });
    }

    private static async Task<int> GetCountForAllEntries(ZipArchive archive)
    {
        int count = 0;
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith("/"))
            {
                continue;
            }
            ++count;

            // Check if the entry is a nested zip file
            if (entry.FullName.IsCompressedFile())
            {
                using var nestedStream = entry.Open();
                using var nestedArchive = new ZipArchive(nestedStream, ZipArchiveMode.Read);
                count += await GetCountForAllEntries(nestedArchive);
            }
        }
        return count;
    }

}
