using System.IO;

namespace SupCom2ModPackager.Extensions
{
    public static class GeneralExtensions
    {
        private static readonly string[] CompressedExtensions = [".zip", ".rar", ".sc2", ".scd"];

        public static bool IsCompressedFile(this string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension != null && CompressedExtensions.Contains(extension);
        }

        public static string? GetCompressedFileName(this string directory)
        {
            return CompressedExtensions
                .Select(ext => Path.ChangeExtension(directory, ext))
                .FirstOrDefault(File.Exists);
        }
    }
}
