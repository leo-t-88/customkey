using CustomKey.Common;
using NUnit.Framework;

namespace CustomKey.Tests.Common
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
            Utility.GlobalRefresh += () => _eventTriggered = true;

            Utility.IsShift = false;
            Assert.That(_eventTriggered, Is.False);
        }

        [Test]
        public void IsShift_SetDifferentValue_TriggersEvent()
        {
            Utility.GlobalRefresh += () => _eventTriggered = true;

            Utility.IsShift = true;
            Assert.That(_eventTriggered, Is.True);
        }

        [Test]
        public void IsInputEnabled_CanBeModified()
        {
            Utility.IsInputEnabled = false;
            Assert.That(Utility.IsInputEnabled, Is.False);

            Utility.IsInputEnabled = true;
            Assert.That(Utility.IsInputEnabled, Is.True);
        }

        [Test]
        public void OpenURL_DoesNotThrow()
        {
            // Test that make sure the method doesn't crash.
            Assert.DoesNotThrow(() => Utility.OpenURL("https://example.com"));
        }
    }
}
