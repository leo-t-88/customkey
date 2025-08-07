using CustomKey.Common;

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
            Assert.AreEqual("Vc1", result);
        }

        [Test]
        public void ConvertToVcKey_Null_If_Not_Found()
        {
            string result = LayoutLoader.ConvertToVcKey("UnknownKey");
            Assert.IsNull(result);
        }

        [Test]
        public void GetChar_BaseChar_When_Shift_Off()
        {
            LayoutLoader.KeyVal["Key1"] = ("a", "A", "id1");
            Utility.IsShift = false;

            string result = LayoutLoader.GetChar("Key1");
            Assert.AreEqual("a", result);
        }

        [Test]
        public void GetChar_ShiftChar_When_Shift_On()
        {
            LayoutLoader.KeyVal["Key1"] = ("a", "A", "id1");
            Utility.IsShift = true;

            string result = LayoutLoader.GetChar("Key1");
            Assert.AreEqual("A", result);
        }

        [Test]
        public void GetChar_Uppercase_If_ShiftChar_Is_ReturnCode()
        {
            LayoutLoader.KeyVal["Key1"] = ("b", "\r", "id1");
            Utility.IsShift = true;

            string result = LayoutLoader.GetChar("Key1");
            Assert.AreEqual("B", result);
        }

        [Test]
        public void GetChar_Empty_If_Key_Not_Found()
        {
            Utility.IsShift = false;
            string result = LayoutLoader.GetChar("Key999");
            Assert.AreEqual("", result);
        }

        [Test]
        public void GetChar_Empty_If_Key_Is_Null_Or_Whitespace()
        {
            string result1 = LayoutLoader.GetChar(null);
            string result2 = LayoutLoader.GetChar("   ");

            Assert.AreEqual("", result1);
            Assert.AreEqual("", result2);
        }

        [Test]
        public void GetChar_ShiftSuffix_Properly_When_Shift_Off()
        {
            LayoutLoader.KeyVal["Key1"] = ("x", "Y", "id1");
            Utility.IsShift = false;

            string result = LayoutLoader.GetChar("Key1Shift");
            Assert.AreEqual("Y", result);
        }

        [Test]
        public void GetChar_Empty_For_ShiftSuffix_When_Shift_On()
        {
            LayoutLoader.KeyVal["Key1"] = ("x", "Y", "id1");
            Utility.IsShift = true;

            string result = LayoutLoader.GetChar("Key1Shift");
            Assert.AreEqual("", result);
        }

        [Test]
        public void GetJsonFileName_Null_If_Not_Found()
        {
            string result = LayoutLoader.GetJsonFileName("UnknownLayout");
            Assert.IsNull(result);
        }
    }
}
