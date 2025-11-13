using CustomKey.Common;
using NUnit.Framework;

namespace CustomKey.Tests.Common
{
    [TestFixture]
    public class LayoutLoaderTest
    {
        [SetUp]
        public void Setup()
        {
            LayoutLoader.KeyVal.Clear();
        }

        [Test]
        public void ConvertToVcKey_Correct_Value()
        {
            string result = LayoutLoader.ConvertToVcKey("D1");
            Assert.That(result, Is.EqualTo("Vc1"));
        }

        [Test]
        public void ConvertToVcKey_Null_If_Not_Found()
        {
            string result = LayoutLoader.ConvertToVcKey("UnknownKey");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetChar_BaseChar_When_Shift_Off()
        {
            LayoutLoader.KeyVal["Key1"] = ("a", "A", "id1");
            Utility.IsShift = false;

            string result = LayoutLoader.GetChar("Key1");
            Assert.That(result, Is.EqualTo("a"));
        }

        [Test]
        public void GetChar_ShiftChar_When_Shift_On()
        {
            LayoutLoader.KeyVal["Key1"] = ("a", "A", "id1");
            Utility.IsShift = true;

            string result = LayoutLoader.GetChar("Key1");
            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void GetChar_Uppercase_If_ShiftChar_Is_ReturnCode()
        {
            LayoutLoader.KeyVal["Key1"] = ("b", "\r", "id1");
            Utility.IsShift = true;

            string result = LayoutLoader.GetChar("Key1");
            Assert.That(result, Is.EqualTo("B"));
        }

        [Test]
        public void GetChar_Empty_If_Key_Not_Found()
        {
            Utility.IsShift = false;
            string result = LayoutLoader.GetChar("Key999");
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void GetChar_Empty_If_Key_Is_Null_Or_Whitespace()
        {
            Assert.That(LayoutLoader.GetChar("   "), Is.EqualTo(""));
        }

        [Test]
        public void GetChar_ShiftSuffix_Properly_When_Shift_Off()
        {
            LayoutLoader.KeyVal["Key1"] = ("x", "Y", "id1");
            Utility.IsShift = false;

            string result = LayoutLoader.GetChar("Key1Shift");
            Assert.That(result, Is.EqualTo("Y"));
        }

        [Test]
        public void GetChar_Empty_For_ShiftSuffix_When_Shift_On()
        {
            LayoutLoader.KeyVal["Key1"] = ("x", "Y", "id1");
            Utility.IsShift = true;

            string result = LayoutLoader.GetChar("Key1Shift");
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void GetJsonFileName_Null_If_Not_Found()
        {
            string result = LayoutLoader.GetJsonFileName("UnknownLayout");
            Assert.That(result, Is.Null);
        }
    }
}
