using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.TR.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorService))]
	public class MessageProcessorServiceTest : BranchMessageProcessorServiceTest<MessageProcessorService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "TRP", hostedServiceAttribute.Code);
				AssertEquals("Description", "TR Incoming Message Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "TRC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Turkey, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(MessageProcessorService).GetMethod(nameof(MessageProcessorService.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			CertificateRequirementChecker.ResetForTesting();
			AssertEquals("There is no Certificate configured in Turkey.", MessageProcessorService.IsRequired());
			GlbExternalPasswordHelperTest.SetupGlbExternalPassword_TRK(Factory);
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(string.Empty, MessageProcessorService.IsRequired());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						Messaging.ServiceTaskApplicationCodeList.Descriptions.TRP,
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
			var message = factory.Load<TRManifestMessage>(testData.MessagePK);
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		protected override MessageProcessorService CreateServiceTask() => new MessageProcessorService();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var factory = Factory;
			var header = factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			((BusinessObject)header).FillWithValidTestData();
			header.AMA_JobReference = "ULU-2019/00002345";
			header.AMA_ManifestType = "DENIHR";

			var requestInterchange = ServiceTaskTestHelper.CreateEDIInterchange(TRMessageTypes.Codes.TRO, EDIInterchange.Status.Received, EDIInterchange.Direction.Transmit, factory);
			var requestMessage = ServiceTaskTestHelper.CreateEdiMessage("Body Text", EDIInterchange.Status.Received, EDIInterchange.Direction.Transmit, factory);
			requestMessage.EM_LinkedObject = (BusinessObject)header;
			requestMessage.EM_EI = requestInterchange.PK;

			var messageText = TRMessageTestHelper.GetFileText("Manifest.Incoming.TROSucceed.xml");
			var responseInterchange = ServiceTaskTestHelper.CreateEDIInterchange(TRMessageTypes.Codes.TRO, EDIInterchange.Status.Queued, EDIInterchange.Direction.Receive, factory);
			var responseMessage = ServiceTaskTestHelper.CreateEdiMessage(messageText, EDIInterchange.Status.Queued, EDIInterchange.Direction.Receive, factory);
			responseMessage.EM_EI = responseInterchange.PK;

			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = responseMessage.PK
			};
		}
	}
}

