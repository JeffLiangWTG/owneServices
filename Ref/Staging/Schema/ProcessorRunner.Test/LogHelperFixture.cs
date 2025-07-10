using CargoWise.RefDbRepo.Common.Utils;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner.Test
{
	[TestFixture]
	class LogHelperFixture
	{
		[Test]
		public void TrySetAppContext()
		{
			var logHelper = new LogHelper(logWrapper.Object, "Test");
			Assert.False(logHelper.TrySetAppContext(null, out var message));
			Assert.AreEqual("AppContext cannot be set to NullOrEmpty explicitly in this application [Test].", message);

			Assert.True(logHelper.TrySetAppContext("TestContext", out message));
			Assert.True(string.IsNullOrEmpty(message));

			Assert.False(logHelper.TrySetAppContext("ChangeContext", out message));
			Assert.AreEqual("AppContext is already set for this application [Test].", message);

			logHelper = new LogHelper(logWrapper.Object, "UXML Parser");
			Assert.True(logHelper.TrySetAppContext("TestContext", out message));
			Assert.True(string.IsNullOrEmpty(message));

			Assert.False(logHelper.TrySetAppContext("ChangeContext", out message));
			Assert.AreEqual("AppContext is already set for this application [UXML Parser].", message);
		}

		[Test]
		public void TrySetSubSource()
		{
			var logHelper = new LogHelper(logWrapper.Object, "Test");
			Assert.False(logHelper.TrySetSubSource(null, out var message));
			Assert.AreEqual("SubSource cannot be set to NullOrEmpty explicitly in this application [Test].", message);

			Assert.True(logHelper.TrySetSubSource("SubSource 1", out message));
			Assert.True(string.IsNullOrEmpty(message));

			Assert.False(logHelper.TrySetSubSource("SubSource 2", out message));
			Assert.AreEqual("SubSource is already set for this application [Test].", message);

			logHelper = new LogHelper(logWrapper.Object, "UXML Parser");
			Assert.True(logHelper.TrySetSubSource("SubSource 1", out message));
			Assert.True(string.IsNullOrEmpty(message));

			Assert.False(logHelper.TrySetSubSource("SubSource 2", out message));
			Assert.AreEqual("SubSource is already set for this application [UXML Parser].", message);
		}

		Mock<ILogWrapper> logWrapper;

		[SetUp]
		public void SetUp()
		{
			logWrapper = new Mock<ILogWrapper>();
		}
	}
}
