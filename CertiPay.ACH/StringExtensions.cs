using System;

namespace CertiPay.ACH
{
    public static class StringExtensions
    {
        public static String TrimAndPadLeft(this String original, int length, char paddingChar = ' ')
        {
            return
                original
                .NotNull()
                .ToUpper()
                .PadLeft(length, paddingChar)
                .Substring(0, length);
        }

        public static String TrimAndPadRight(this String original, int length, char paddingChar = ' ')
        {
            return
                original
                .NotNull()
                .ToUpper()
                .PadRight(length, paddingChar)
                .Substring(0, length);
        }

        public static String NotNull(this String original)
        {
            return String.IsNullOrWhiteSpace(original) ? String.Empty : original;
        }
    }
}