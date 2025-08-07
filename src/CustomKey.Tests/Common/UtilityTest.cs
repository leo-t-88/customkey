using CustomKey.Common;

namespace CustomKey.Tests
{
    [TestFixture]
    public class UtilityTests
    {
        private bool _eventTriggered;

        [SetUp]
        public void Setup()
        {
            // Reset state before each test
            Utility.IsShiftPending = false;
            Utility.IsInputEnabled = true;
            _eventTriggered = false;
        }

        [Test]
        public void IsShift_SetSameValue_DoesNotTriggerEvent()
        {
            Utility.IsShiftChanged += () => _eventTriggered = true;

            Utility.IsShift = false;
            Assert.IsFalse(_eventTriggered);
        }

        [Test]
        public void IsShift_SetDifferentValue_TriggersEvent()
        {
            Utility.IsShiftChanged += () => _eventTriggered = true;

            Utility.IsShift = true;
            Assert.IsTrue(_eventTriggered);
        }

        [Test]
        public void IsInputEnabled_CanBeModified()
        {
            Utility.IsInputEnabled = false;
            Assert.IsFalse(Utility.IsInputEnabled);

            Utility.IsInputEnabled = true;
            Assert.IsTrue(Utility.IsInputEnabled);
        }

        [Test]
        public void OpenURL_DoesNotThrow()
        {
            // Test that make sure the method doesn't crash.
            Assert.DoesNotThrow(() => Utility.OpenURL("https://example.com"));
        }
    }
}
