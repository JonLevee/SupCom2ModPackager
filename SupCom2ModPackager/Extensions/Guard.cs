using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace SupCom2ModPackager.Extensions
{
    public static class Guard
    {
        public static string GetCompressedFileNameExtensionError(this string path)
        {
            return $"File {path} extension is not one of [{string.Join(',', GeneralExtensions.CompressedExtensions)}]";
        }

        [Conditional("CONTRACTS_FULL")]
        public static void Requires(
            [DoesNotReturnIf(false)]
            bool condition)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Condition is not met.");
            }
        }

        [Conditional("CONTRACTS_FULL")]
        public static void Requires(
            [DoesNotReturnIf(false)]
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
