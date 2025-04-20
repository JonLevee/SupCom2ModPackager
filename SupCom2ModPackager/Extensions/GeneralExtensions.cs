using System.IO;

namespace SupCom2ModPackager.Extensions
{
    public static class GeneralExtensions
    {
        private static readonly string[] CompressedExtensions = [".zip", ".rar", ".sc2", ".scd"];
        private static readonly string[] SupCom2Files = ["mod.zip", "meta.txt", "profile.yml"];

        public static bool IsCompressedFile(this string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension != null && CompressedExtensions.Contains(extension);
        }

        public static string? GetCompressedFileName(this string directory)
        {
            var compressedFile = CompressedExtensions
                .Select(ext => directory + ext)
                .FirstOrDefault(File.Exists);
            return compressedFile;
        }

        public static bool IsSupCom2Directory(this string directory)
        {
            if (CompressedExtensions.Select(ext => directory + ext).Any(File.Exists))
            {
                if (SupCom2Files.All(file => File.Exists(Path.Combine(directory, file))))
                    return true;
            }
            return false;
        }
    }
}
