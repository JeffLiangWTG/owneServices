using System;
using System.Configuration;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.DataAccess.Integration;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class UpdateStatusEventTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEverything()
		{
			string track = string.Empty;
			string send = string.Empty;
			string rec = string.Empty;
			var accessor = MockRepository.GenerateMock<IOutboxAccessor>();
			accessor.Expect(x => x.UpdateMessageDistributionStatus("1111", "sender", "recipient")).
				Do(new Action<string, string, string>((string trackingID, string sender, string recipient) =>
				{
					track = trackingID;
					send = sender;
					rec = recipient;
				})).Repeat.Once(); 

			var statusEvent = new UpdateStatusEvent("1111", "sender", "recipient", accessor);
			Assert.IsNull(statusEvent.ParentRecord);
			Assert.AreEqual(typeof(UpdateStatusEvent), statusEvent.BatchType);
			statusEvent.PersistQueryable(null, null, 0);
			Assert.AreEqual("1111", track);
			Assert.AreEqual("sender", send);
			Assert.AreEqual("recipient", rec);

			accessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UpdateStatusEvent_RetriesFail()
		{
			var exception = new TimeoutException();
			var accessor = MockRepository.GenerateMock<IOutboxAccessor>();
			accessor.Stub(x => x.UpdateMessageDistributionStatus(Guid.Empty.ToString(), "SENDER", "RECIPIENT")).Throw(exception);

			var logger = MockRepository.GeneratePartialMock<TraceLogger>(false, "UpdateStatusEventTest", LogLevel.All, true, false, false, "s");

			var statusEvent = new UpdateStatusEvent(Guid.Empty.ToString(), "SENDER", "RECIPIENT", accessor) { Logger = logger };

			statusEvent.PersistQueryableInternal(null, null, 0);

			logger.AssertWasCalled(x => x.DebugFormat(Arg.Text.StartsWith("Starting update"), Arg<object[]>.Is.Anything), x => x.Repeat.Once());
			logger.AssertWasCalled(x => x.DebugFormat(Arg.Text.StartsWith("Error. Will be retried."), Arg.Is(exception), Arg<object[]>.Is.Anything), x => x.Repeat.Times(5));
			logger.AssertWasCalled(x => x.WarnFormat(Arg.Text.StartsWith("UpdateMessageDistributionStatus error. Will be retried."), Arg.Is(exception), Arg<object[]>.Is.Anything), x => x.Repeat.Times(4));
			logger.AssertWasCalled(x => x.ErrorFormat(Arg.Text.StartsWith("UpdateMessageDistributionStatus permanently failed after multiple retries."), Arg.Is(exception), Arg<object[]>.Is.Anything), x => x.Repeat.Once());
			logger.AssertWasNotCalled(x => x.DebugFormat(Arg.Text.StartsWith("Completed update"), Arg<object[]>.Is.Anything));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UpdateStatusEvent_RetriesSuccess()
		{
			var exception = new TimeoutException();
			var accessor = MockRepository.GenerateMock<IOutboxAccessor>();
			accessor.Stub(x => x.UpdateMessageDistributionStatus(Guid.Empty.ToString(), "SENDER", "RECIPIENT")).Throw(exception).Repeat.Twice();

			var logger = MockRepository.GeneratePartialMock<TraceLogger>(false, "UpdateStatusEventTest", LogLevel.All, true, false, false, "s");

			var statusEvent = new UpdateStatusEvent(Guid.Empty.ToString(), "SENDER", "RECIPIENT", accessor) { Logger = logger };

			statusEvent.PersistQueryableInternal(null, null, 0);

			logger.AssertWasCalled(x => x.DebugFormat(Arg.Text.StartsWith("Starting update"), Arg<object[]>.Is.Anything), x => x.Repeat.Once());
			logger.AssertWasCalled(x => x.DebugFormat(Arg.Text.StartsWith("Error. Will be retried."), Arg.Is(exception), Arg<object[]>.Is.Anything), x => x.Repeat.Twice());
			logger.AssertWasNotCalled(x => x.WarnFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything));
			logger.AssertWasNotCalled(x => x.ErrorFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything));
			logger.AssertWasCalled(x => x.DebugFormat(Arg.Text.StartsWith("Completed update"), Arg<object[]>.Is.Anything), x => x.Repeat.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UpdateStatusEvent_ShouldNotRetry()
		{
			var exception = new TimeoutException();
			var accessor = MockRepository.GenerateMock<IOutboxAccessor>();
			string trackingID = null;
			var sender = "SENDER";
			var recipient = "RECIPIENT";
			accessor.Expect(x => x.UpdateMessageDistributionStatus(trackingID, sender, recipient)).IgnoreArguments().Repeat.Never();

			var logger = MockRepository.GeneratePartialMock<TraceLogger>(false, "UpdateStatusEventTest", LogLevel.All, true, false, false, "s");

			var statusEvent = new UpdateStatusEvent(trackingID, sender, recipient, accessor) { Logger = logger };
			statusEvent.PersistQueryableInternal(null, null, 0);
			logger.AssertWasCalled(x => x.WarnFormat("Unexpected message going through UpdateMessageDistributionStatus which has one of following properties which is empty or null: [MessageTrackingID: {0}, SenderID: {1}, RecipientID: {2}", trackingID, sender, recipient));

			logger = MockRepository.GeneratePartialMock<TraceLogger>(false, "UpdateStatusEventTest", LogLevel.All, true, false, false, "s");
			trackingID = Guid.NewGuid().ToString();
			sender = null;
			statusEvent = new UpdateStatusEvent(trackingID, sender, recipient, accessor) { Logger = logger };
			statusEvent.PersistQueryableInternal(null, null, 0);
			logger.AssertWasCalled(x => x.WarnFormat("Unexpected message going through UpdateMessageDistributionStatus which has one of following properties which is empty or null: [MessageTrackingID: {0}, SenderID: {1}, RecipientID: {2}", trackingID, sender, recipient));

			logger = MockRepository.GeneratePartialMock<TraceLogger>(false, "UpdateStatusEventTest", LogLevel.All, true, false, false, "s");
			sender = "SENDER";
			recipient = null;
			statusEvent = new UpdateStatusEvent(trackingID, sender, recipient, accessor) { Logger = logger };
			statusEvent.PersistQueryableInternal(null, null, 0);
			logger.AssertWasCalled(x => x.WarnFormat("Unexpected message going through UpdateMessageDistributionStatus which has one of following properties which is empty or null: [MessageTrackingID: {0}, SenderID: {1}, RecipientID: {2}", trackingID, sender, recipient));

			accessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UpdateStatusEvent_SafeGetTimeSpanConfig_GetDefaultValues()
		{
			ConfigurationManager.AppSettings["UpdateStatusEventWaitQuick"] = null;
			ConfigurationManager.AppSettings["UpdateStatusEventMaxQuick"] = null;
			ConfigurationManager.AppSettings["UpdateStatusEventWaitLong"] = "Invalid Format";
			ConfigurationManager.AppSettings["UpdateStatusEventMaxLong"] = "Invalid Format";

			var logger = MockRepository.GeneratePartialMock<TraceLogger>(false, "UpdateStatusEventTest", LogLevel.All, true, false, false, "s");
			var accessor = MockRepository.GenerateMock<IOutboxAccessor>();
			var statusEvent = new UpdateStatusEvent(Guid.Empty.ToString(), "SENDER", "RECIPIENT", accessor) { Logger = logger };

			Assert.AreEqual(TimeSpan.FromSeconds(5), statusEvent.WaitQuick);
			Assert.AreEqual(TimeSpan.FromMinutes(1), statusEvent.MaxQuick);
			Assert.AreEqual(TimeSpan.FromMinutes(1), statusEvent.WaitLong);
			Assert.AreEqual(TimeSpan.FromDays(1), statusEvent.MaxLong);
			logger.AssertWasCalled(x => x.WarnFormat("Expected <appSettings><add key=\"{0}\" value=\"{1}\"/></appSettings> but not found any in Biztalk's config. Using default value ({1} seconds) instead.", "UpdateStatusEventWaitQuick", TimeSpan.FromSeconds(5).TotalSeconds));
			logger.AssertWasCalled(x => x.WarnFormat("Expected <appSettings><add key=\"{0}\" value=\"{1}\"/></appSettings> but not found any in Biztalk's config. Using default value ({1} seconds) instead.", "UpdateStatusEventMaxQuick", TimeSpan.FromMinutes(1).TotalSeconds));
			logger.AssertWasCalled(x => x.WarnFormat("Expected \"{0}\" config's value is a number but it is \"{1}\". Using default value ({2} seconds) instead.", "UpdateStatusEventWaitLong", "Invalid Format", TimeSpan.FromMinutes(1).TotalSeconds));
			logger.AssertWasCalled(x => x.WarnFormat("Expected \"{0}\" config's value is a number but it is \"{1}\". Using default value ({2} seconds) instead.", "UpdateStatusEventMaxLong", "Invalid Format", TimeSpan.FromDays(1).TotalSeconds));
		}
	}
}
