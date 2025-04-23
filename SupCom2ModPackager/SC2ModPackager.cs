
using SupCom2ModPackager.Collections;
using SupCom2ModPackager.Extensions;
using SupCom2ModPackager.Models;
using SupCom2ModPackager.Utility;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;

namespace SupCom2ModPackager;

public class SC2ModPackager
{
    public static readonly SC2ModPackager Empty = new(DisplayItemCollection.Empty);
    private readonly DisplayItemCollection _items;

    public SC2ModPackager(DisplayItemCollection items)
    {
        this._items = items;
    }

    public async Task UnpackAsync(DisplayItemFile itemFile, bool overWrite, IProgress<PackProgressArgs> progress)
    {
        Guard.Requires(itemFile.FullPath.IsCompressedFile(), $"File {itemFile.Name} extension is not one of [{string.Join(',', GeneralExtensions.CompressedExtensions)}]");

        if (Directory.Exists(itemFile.UnpackDirectoryPath))
        {
            if (!overWrite)
                throw new InvalidOperationException("Handle overWrite error");
            progress.Report(new() { Value = 0, Text = $"Removing {Path.GetFileName(itemFile.UnpackDirectoryPath)} ..." });
            await Task.Run(() =>
            {
                Directory.Delete(itemFile.UnpackDirectoryPath, true);
            });
        }

        Directory.CreateDirectory(itemFile.UnpackDirectoryPath);

        progress.Report(new() { Value = 0, Text = $"Scanning {itemFile.Name} ..." });
        var count = await Task.Run(() => GetCountOfItemsToExtract(itemFile.FullPath));
        progress.Report(new() { Value = 0, Maximum = count, Text = $"Extracting ..." });

        // Perform the extraction
        await Task.Run(() => UnpackAsyncInternal(itemFile.FullPath, progress));
    }



    private static async Task UnpackAsyncInternal(string path, IProgress<PackProgressArgs> progress)
    {
        Guard.Requires(!string.IsNullOrEmpty(path), $"Invalid path [{path.GetNullOrEmpty()}]");
        Guard.Requires(path.IsCompressedFile(), path.GetCompressedFileNameExtensionError());

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

                progress.Report(new() { Text = $"Extracting {entry.FullName}" });

                if (filePath.IsCompressedFile())
                {
                    // Recursively unpack nested archives
                    await UnpackAsyncInternal(filePath, progress);
                }
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
