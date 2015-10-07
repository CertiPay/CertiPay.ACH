using CertiPay.Common.Testing;
using NUnit.Framework;

namespace CertiPay.ACH.Tests
{
    public class StringExtensionsTests
    {
        // TODO Test Cases:

        // Null string

        // Truncates long string

        // Pads left and right

        [Test, Unit]
        public void Handles_Empty_String()
        {
            Assert.AreEqual("    ", string.Empty.TrimAndPadLeft(4));
        }

        [Test, Unit]
        public void Handles_Null_String()
        {
            string test = null;

            Assert.AreEqual("      ", test.TrimAndPadLeft(6));
        }

        [Test, Unit]
        public void Truncates_Long_String()
        {
            string test = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            Assert.AreEqual("ABCD", test.TrimAndPadLeft(4, '0'));
        }

        [Test, Unit]
        public void Pads_Left()
        {
            string test = "ABC";

            Assert.AreEqual("ABC   ", test.TrimAndPadRight(6));
        }

        [Test, Unit]
        public void Pads_Right()
        {
            string test = "ABC";

            Assert.AreEqual("   ABC", test.TrimAndPadLeft(6, ' '));
        }
    }
}