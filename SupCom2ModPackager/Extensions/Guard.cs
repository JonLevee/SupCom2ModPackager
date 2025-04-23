using System.Reflection.Metadata;

namespace SupCom2ModPackager.Extensions
{
    public static class Guard
    {
        public static string GetCompressedFileNameExtensionError(this string path)
        {
            return $"File {path} extension is not one of [{string.Join(',', GeneralExtensions.CompressedExtensions)}]";
        }

        public static void Requires(bool condition)
        {
            Requires(condition, "Condition is not met.");
        }
        public static void Requires(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
