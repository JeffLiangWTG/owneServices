using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TRBranchCustomsApplicationTypeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestLinkRequestMessageByTrackingId()
		{
			var factory = Factory;
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "DENIHR";
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

			var sessionGUID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");
			var messageText = "Body Text";
			var requestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, sessionGUID, "Test Interchange message");
			var requestMessage = MessageTestHelper.CreateEdiMessage<TRManifestMessage>(TRMessageTypes.Codes.TRO, "Body Text", EDIMessage.Status.Queued, EDIInterchange.Direction.Transmit, AsycudaManifestHeaderSchema.Constants.TableName, factory);
			requestMessage.EM_LinkUniqueID = header.PK;
			requestMessage.EM_EI = requestInterchange.PK;
			
			var responseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued, sessionGUID, messageText);
			var responseMessage = MessageTestHelper.CreateEdiMessage<TRManifestMessage>(TRMessageTypes.Codes.TRO, messageText, EDIMessage.Status.Queued, EDIInterchange.Status.Received, string.Empty, factory);
			responseMessage.EM_EI = responseInterchange.PK;

			Factory.Save();

			var processor = new TRBranchCustomsMessageProcessor { Logger = new LoggingInformation() };

			processor.ExecuteBatch();
			responseMessage.Reload();
			requestMessage.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(requestMessage.EM_LinkUniqueID, responseMessage.EM_LinkUniqueID);
				AssertEquals(requestMessage.EM_LinkTable, responseMessage.EM_LinkTable);
			});
		}

		public void TestPreProcessMessageFailedForLinkedObjectIsNull()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var factory = Factory;
				var header = factory.New<AsycudaManifestHeader>();
				header.AMA_ManifestType = "DENIHR";
				header.AMA_GB = GlbBranch.CurrentBranch.PK;
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

				var sessionGUID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");
				var messageText = "Body Text";

				var requestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, sessionGUID, "Test Interchange message");
				var requestMessage = MessageTestHelper.CreateEdiMessage<TRManifestMessage>(TRMessageTypes.Codes.TRO, "Body Text", EDIMessage.Status.Queued, EDIInterchange.Direction.Transmit, string.Empty, factory);
				requestMessage.EM_EI = requestInterchange.PK;

				var responseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued, sessionGUID, messageText);
				var responseMessage = MessageTestHelper.CreateEdiMessage<TRManifestMessage>(TRMessageTypes.Codes.TRO, messageText, EDIMessage.Status.Queued, EDIInterchange.Status.Received, string.Empty, factory);
				responseMessage.EM_EI = responseInterchange.PK;

				Factory.Save();

				var processor = new TRBranchCustomsMessageProcessor { Logger = new LoggingInformation() };

				processor.ExecuteBatch();

				responseMessage.Reload();

				AssertEquals(EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			}
		}

		public void TestPreProcessMessageFailedDoesNotHaveOriginalMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var factory = Factory;
				var header = factory.New<AsycudaManifestHeader>();
				header.AMA_ManifestType = "DENIHR";
				header.AMA_GB = GlbBranch.CurrentBranch.PK;
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

				var sessionGUID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");
				var messageText = "Body Text";

				var responseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued, sessionGUID, messageText);
				var responseMessage = MessageTestHelper.CreateEdiMessage<TRManifestMessage>(TRMessageTypes.Codes.TRO, messageText, EDIMessage.Status.Queued, EDIInterchange.Status.Received, string.Empty, factory);

				Factory.Save();

				var processor = new TRBranchCustomsMessageProcessor { Logger = new LoggingInformation() };

				processor.ExecuteBatch();

				responseMessage.Reload();

				AssertEquals(EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			}
		}

		public void TestProcessMessageReceived()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var factory = Factory;

				var header = factory.New<AsycudaManifestHeader>();
				header.AMA_ManifestType = "DENIHR";
				header.AMA_GB = GlbBranch.CurrentBranch.PK;
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

				var sessionGUID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");
				var messageText = TRMessageTestHelper.GetFileText("RequestResult.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");

				var requestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, sessionGUID, "Test Interchange message");
				var requestMessage = MessageTestHelper.CreateEdiMessage<TRManifestMessage>(TRMessageTypes.Codes.TRO, "Body Text", EDIMessage.Status.Queued, EDIInterchange.Direction.Transmit, AsycudaManifestHeaderSchema.Constants.TableName, factory);
				requestMessage.EM_LinkUniqueID = header.PK;
				requestMessage.EM_EI = requestInterchange.PK;

				var responseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued, sessionGUID, messageText);
				var responseMessage = MessageTestHelper.CreateEdiMessage<TRManifestMessage>(TRMessageTypes.Codes.TRO, messageText, EDIMessage.Status.Queued, EDIInterchange.Status.Received, string.Empty, factory);
				responseMessage.EM_EI = responseInterchange.PK;

				Factory.Save();

				var processor = new TRBranchCustomsMessageProcessor { Logger = new LoggingInformation() };

				processor.ExecuteBatch();

				responseMessage.Reload();

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, responseMessage.EM_Status);
			}
		}
	}
}
