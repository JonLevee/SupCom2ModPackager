using System.Reflection.Metadata;

namespace SupCom2ModPackager.Extensions
{
    public static class ContractHelperExtensions
    {
        public static string GetCompressedFileNameExtensionError(this string path)
        {
            return $"File {path} extension is not one of [{string.Join(',', GeneralExtensions.CompressedExtensions)}]";
        }
    }
}
