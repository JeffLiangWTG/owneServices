using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorService))]
	class ETradeErrorMessageProcessorServiceTest : BranchMessageProcessorServiceTest<MessageProcessorService>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.TRP,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.TRCustoms,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, MessageProcessorService serviceTask)
		{
			var message = factory.Load<ETradeEDIMessage>(testData.MessagePK);
			var header = factory.Load<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>(message.EM_LinkUniqueID);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("AMA_MessageStatus", header.AMA_MessageStatus, TRMessageStatusCodeList.Codes.Error);
			});
		}

		protected override MessageProcessorService CreateServiceTask() => new MessageProcessorService();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var factory = Factory;
			var header = factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG00010";

			var messageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");

			var requestInterchange = ServiceTaskTestHelper.CreateEDIInterchange(TRMessageTypes.Codes.TRE, EDIInterchange.Status.Queued, EDIInterchange.Direction.Transmit, factory);
			var requestMessage = ServiceTaskTestHelper.CreateEdiMessage<ETradeEDIMessage>(TRMessageTypes.Codes.TRE, "Body Text", EDIInterchange.Status.Queued, EDIInterchange.Direction.Transmit, factory);
			requestMessage.EM_LinkedObject = (BusinessObject)header;
			requestMessage.EM_EI = requestInterchange.PK;

			var responseInterchange = ServiceTaskTestHelper.CreateEDIInterchange(TRMessageTypes.Codes.TRE, EDIInterchange.Status.Queued, EDIInterchange.Direction.Receive, factory);
			var responseMessage = ServiceTaskTestHelper.CreateEdiMessage<ETradeEDIMessage>(TRMessageTypes.Codes.TRE, messageText, EDIInterchange.Status.Queued, EDIInterchange.Direction.Receive, factory);
			responseMessage.EM_EI = responseInterchange.PK;
			responseMessage.EM_LinkedObject = (BusinessObject)header;

			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = responseMessage.PK
			};
		}
	}
}

