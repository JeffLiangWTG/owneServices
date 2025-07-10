using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.NL.ServiceTasks.Testing;

[TestedType(typeof(MessageSenderServiceTask))]
sealed class MessageSenderServiceTaskTest : ServiceTaskTestCase<MessageSenderServiceTask>
{
	public void TestProcess()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";

		var nlmessage = Factory.New<NLEDIMessage>();
		nlmessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NLCustoms;
		nlmessage.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		nlmessage.EM_MessageSubType = ExportSendMessageTypes.Codes.PRE;
		nlmessage.EM_MessageNum = "1";
		nlmessage.EM_ReceiveTransmit = NLEDIMessage.Direction.Transmit;
		nlmessage.EM_Status = NLEDIMessage.Status.Queued;
		nlmessage.EM_IsActive = true;
		nlmessage.EM_MessageText = "<?xml version='1.0' encoding='utf - 8'?><MetaData xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Declaration:1'><WCOTypeCode>CC432A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>EH00001</ApplicationReferenceID><CommunicationsAgreementID>18</CommunicationsAgreementID><PreparationDateTime formatCode='304'>20220224154823Z</PreparationDateTime><Recipient><ID>DMS.NL</ID></Recipient><Sender><ID>NL56785678</ID></Sender></CommunicationMetaData><Declaration><FunctionalReferenceID>EH00001</FunctionalReferenceID><ID>MRN123</ID><DeclarationOffice><ID>NL55566677</ID></DeclarationOffice><Agent><ID>NL43434343</ID><FunctionCode>DIR</FunctionCode></Agent><Declarant><Name>Declarant Full Name</Name><ID>NL56785678</ID><Address><CityName>Decapolis</CityName><CountryCode>NL</CountryCode><Line>Declarantenstraat 30</Line><PostcodeID>5890DW</PostcodeID></Address></Declarant><GoodsShipment><SequenceNumeric>1</SequenceNumeric><Consignment><GoodsLocation><TypeCode>A</TypeCode><IdentificationTypeCode>T</IdentificationTypeCode><Address><CountryCode>NL</CountryCode><PostcodeID>1234AB</PostcodeID><StreetNumberID>Kerkstraat 1</StreetNumberID></Address></GoodsLocation><TransportEquipment><SequenceNumeric>1</SequenceNumeric><ID>APLU8521458</ID><GoodsReference><SequenceNumeric>1</SequenceNumeric><GoodsItemNumeric>1</GoodsItemNumeric></GoodsReference><GoodsReference><SequenceNumeric>2</SequenceNumeric><GoodsItemNumeric>1</GoodsItemNumeric></GoodsReference></TransportEquipment><TransportEquipment><SequenceNumeric>2</SequenceNumeric><ID>MSCU0051257</ID><GoodsReference><SequenceNumeric>1</SequenceNumeric><GoodsItemNumeric>1</GoodsItemNumeric></GoodsReference><GoodsReference><SequenceNumeric>2</SequenceNumeric><GoodsItemNumeric>1</GoodsItemNumeric></GoodsReference></TransportEquipment></Consignment></GoodsShipment></Declaration></MetaData>";
		nlmessage.EM_MessageOwner = "CW1_Test";
		Factory.Save();

		var service = new MessageSenderServiceTask();
		InitialiseTaskSchedule(service);
		using (Env.Instance.TemporaryServiceTaskContext(MessageSenderServiceTask.Code, canRunInAnyBranch: true))
		{
			AssertNoExceptionThrown(service.RunTask);
		}

		var factory = new BusinessObjectFactory();
		var messages = factory.Load<NLEDIMessage>(nlmessage.PK);
		var interchange = messages.Interchange;
		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Interchange No", "1", messages.EM_InterchangeNumber);
			AssertEquals("EDI Interchange - Status", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EDI Interchange - Header Text", $"{{\"custom.Subject\":\"[v=.05][a=DMS.NL][k={interchange.eHubID.KeepAlphanumericCharacters()}][s=0]\"}}", interchange.EI_HeaderText);
			AssertEquals("EDI Interchange - Body Text", messages.EM_MessageText, interchange.EI_BodyText);
			AssertNullOrEmpty("EDI Interchange - Footer Text", interchange.EI_FooterText);
			AssertEquals("EDI Interchange - Direction", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EDI Interchange - Retry Count", 0, interchange.EI_RetryCount);
			AssertEquals("EDI Interchange - Transport Type", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
			AssertEquals("EDI Interchange - From", nlmessage.Company.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals("EDI Interchange - To", "NLCustomsDMS", interchange.EI_To);
		});
	}

	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		CombineAssertions(() =>
		{
			var hostedServiceAttribute = hostedServiceAttributes.Single();
			AssertEquals("Code", "NLS", hostedServiceAttribute.Code);
			AssertEquals("Description", "NL Customs Message Sender", hostedServiceAttribute.Description);
			AssertEquals("Category", "NLC", hostedServiceAttribute.Category);
			AssertEquals("MinimumPeriod", "60Seconds", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Netherlands, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("DefaultScheduleRunEvery", "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
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
					"NL Customs Message Sender",
					EDIMessageSchema.Constants.EM_IsActive + "=Y",
					EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
					EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
					EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.NLCustoms,
					EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
			};
		}
	}
}
