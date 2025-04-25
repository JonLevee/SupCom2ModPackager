using System.IO;
using System.Collections;
using System.Reflection;

namespace SupCom2ModPackager.Extensions
{
    public static class GeneralExtensions
    {
        public static readonly string[] CompressedExtensions = [".zip", ".rar", ".sc2", ".scd"];
        public static readonly string[] SupCom2Files = ["mod.zip", "meta.txt", "profile.yml"];
        private static readonly IList EmptyList = new List<object>();

        public static bool IsCompressedFile(this string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension != null && CompressedExtensions.Contains(extension);
        }

        public static bool IsSupCom2Directory(this string directory)
        {
            return true;
        }

        public static string? GetCompressedFileName(this string directory)
        {
            var compressedFile = CompressedExtensions
                .Select(ext => directory + ext)
                .FirstOrDefault(File.Exists);
            return compressedFile;
        }

        public static bool HasSupCom2CompressedFile(this string directory)
        {
            if (CompressedExtensions.Select(ext => directory + ext).Any(File.Exists))
            {
                if (SupCom2Files.All(file => File.Exists(Path.Combine(directory, file))))
                    return true;
            }
            return false;
        }

        public static IList GetListOrDefault(this IList? list)
        {
            return list ?? EmptyList;
        }

        public static string GetNullOrEmpty(this string? text)
        {
            return text == null ? "(null)" : text == string.Empty ? "(empty)" : text;
        }

        public static string[] GetValidDrives()
        {
            return DriveInfo
                .GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => d.Name)
                .ToArray();
        }

        public static FieldInfo? GetInheritedField(this Type? type, string fieldName, BindingFlags bindingFlags = BindingFlags.Default)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, bindingFlags);
                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }

    }
}
