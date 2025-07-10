using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Customs.UY.Manifest.Business;
using Enterprise.Customs.UY.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.ServiceTasks.Testing
{
	[TestedType(typeof(MessageProcessorService))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class MessageProcessorServiceTaskTest : BranchMessageProcessorServiceTest<MessageProcessorService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "UYP", hostedServiceAttribute.Code);
				AssertEquals("Description", "UY Customs Incoming Message Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "UYC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Uruguay, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.UYP,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UYCustoms),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, BranchMessageProcessorServiceTestHelperData testData, MessageProcessorService serviceTask)
		{
			var message = factory.Load<UYMessage>(testData.MessagePK);
			AssertEquals(UYMessage.Status.ProcessedOK, message.EM_Status);
		}

		protected override MessageProcessorService CreateServiceTask() => new MessageProcessorService();

		protected override BranchMessageProcessorServiceTestHelperData SetupDataForTesting()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "8AZ8493";

			var bodyText = DAETestingHelper.GetExpectedMessageXML(Path.Combine(BaseSourcePath, DAETestingConstants.BodyText));
			var sessionGUID = new ZGuid("DEC667EC-3847-4169-8376-87856D8FD014");
			var requestInterchange = CreateInterchange(sessionGUID, EDIInterchange.Direction.Transmit, EDIInterchangeStatusList.Codes.Received);
			var requestMessage = CreateMessage(bodyText, requestInterchange.PK, header.PK, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Received);
			var responseInterchange = CreateInterchange(requestInterchange.EI_SessionGUID, EDIInterchange.Direction.Receive, EDIInterchangeStatusList.Codes.Queued);
			var responseMessage = CreateMessage(bodyText, responseInterchange.PK, header.PK, EDIMessage.Direction.Receive, EDIMessageStatusList.Codes.Queued);

			return new BranchMessageProcessorServiceTestHelperData()
			{
				MessagePK = responseMessage.PK
			};
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN0000001";
			header.AMA_Voyage = "LH8264";
			return header;
		}

		UYCInterchange CreateInterchange(ZGuid sessionGUID, ZString direction, ZString status)
		{
			var interchange = Factory.New<UYCInterchange>();
			interchange.EI_Status = status;
			interchange.EI_IsActive = true;
			interchange.EI_ReceiveTransmit = direction;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UYCustoms;
			interchange.EI_InterchangeType = MessageTypes.Codes.UYC;
			interchange.EI_From = UYMessageConstants.InterchangeToTest;
			interchange.EI_To = "eHub";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_SessionGUID = sessionGUID;

			Factory.Save();
			return interchange;
		}

		UYMessage CreateMessage(ZString file, ZGuid interchangePK, ZGuid headerPK, ZString direction, ZString status)
		{
			var message = Factory.New<UYMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UYCustoms;
			message.EM_ApplicationReference = "MAN0000001";
			message.EM_MessageNum = "0070";
			message.EM_MessageType = MessageTypes.Codes.UYC;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = file;
			message.EM_EI = interchangePK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = headerPK;

			Factory.Save();
			return message;
		}
	}
}
