using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.NO.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(DMOMessagePacker))]
sealed class DMOMessagePackerTest : UniversalCustomsEDIMessagePackerTest<DMOMessagePacker>
{
	public void TestPack_Parameters() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When message is null", () => MessagePacker.Pack(null, interchange, loggingInformation));
		AssertExceptionThrown<ArgumentNullException>("When interchange is null", () => MessagePacker.Pack(message, null, loggingInformation));
		AssertExceptionThrown<ArgumentNullException>("When logger is null", () => MessagePacker.Pack(message, interchange, null));
	});

	public void TestPack_InvalidBranchPK() => CombineAssertions(() =>
	{
		message.EM_GB = ZGuid.NewZGuid();
		var result = MessagePacker.Pack(message, interchange, loggingInformation);
		AssertEquals("Invalid Branch PK", $"EDIMessage with PK: [{message.PK}], failed to identify compnay with branch PK [{message.EM_GB}].", result);

		message.EM_GB = branch.PK;
		branch.GB_GC = ZGuid.NewZGuid();
		result = MessagePacker.Pack(message, interchange, loggingInformation);
		AssertEquals("Invalid Company PK linked to the Current Branch", $"EDIMessage with PK: [{message.PK}], failed to identify compnay with branch PK [{message.EM_GB}].", result);
	});

	public void TestPack_ForNewMessageAtHeaderLevel()
	{
		SetDataAndAssertInterchange(TransportTypeList.Codes.Road, NODMOInterchangeTypeList.Codes.RDT);
		SetDataAndAssertInterchange(TransportTypeList.Codes.Air, NODMOInterchangeTypeList.Codes.ART);
		SetDataAndAssertInterchange(TransportTypeList.Codes.Rail, NODMOInterchangeTypeList.Codes.RLT);

		void SetDataAndAssertInterchange(string transportMode, string expectedInterchangeType)
			=> SetDataAndAssertInterchangeForNewMessage(transportMode, NODMOEDIMessageTypeList.Codes.TRA, expectedInterchangeType, (header, _) => header);
	}

	public void TestPack_ForNewMessageAtBillLevel()
	{
		SetDataAndAssertInterchangeForNewMessage(transportMode: TransportTypeList.Codes.Road,
			messageType: NODMOEDIMessageTypeList.Codes.HCS,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.RDH,
			getParent: (_, bill) => bill);

		SetDataAndAssertInterchangeForNewMessage(transportMode: TransportTypeList.Codes.Road,
			messageType: NODMOEDIMessageTypeList.Codes.MCS,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.RDM,
			getParent: (_, bill) => bill);

		SetDataAndAssertInterchangeForNewMessage(transportMode: TransportTypeList.Codes.Air,
			messageType: NODMOEDIMessageTypeList.Codes.HCS,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.ARH,
			getParent: (_, bill) => bill);
		SetDataAndAssertInterchangeForNewMessage(transportMode: TransportTypeList.Codes.Air,
			messageType: NODMOEDIMessageTypeList.Codes.MCS,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.ARM,
			getParent: (_, bill) => bill);

		SetDataAndAssertInterchangeForNewMessage(transportMode: TransportTypeList.Codes.Rail,
			messageType: NODMOEDIMessageTypeList.Codes.HCS,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.RLH,
			getParent: (_, bill) => bill);
		SetDataAndAssertInterchangeForNewMessage(transportMode: TransportTypeList.Codes.Rail,
			messageType: NODMOEDIMessageTypeList.Codes.MCS,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.RLM,
			getParent: (_, bill) => bill);
	}

	void SetDataAndAssertInterchangeForNewMessage(string transportMode,
		string messageType,
		string expectedInterchangeType,
		Func<AsycudaManifestHeader, AsycudaBill, BusinessObject> getParent)
	{
		var (header, bill) = CreateHeaderWithBill(transportMode);
		var parent = getParent(header, bill);
		UpdateMessageData(NODMOMessageFunctionList.Codes.NEW, messageType, parent);
		var result = MessagePacker.Pack(message, interchange, loggingInformation);

		CombineAssertions($"TransportMode: {transportMode}, MessageType: {messageType}", () =>
		{
			AssertNullOrEmpty("Output String", result);
			AssertEquals("EI_InterchangeType", expectedInterchangeType, interchange.EI_InterchangeType);
			AssertEquals("EI_ApplicationCode", "NOD", interchange.EI_ApplicationCode);
			AssertEquals("EI_HeaderText", $"{{\"custom.MessageSubType\":\"{message.EM_MessageSubType}\",\"custom.NO.Issuer\":\"{password.GP_UserID}\"}}", interchange.EI_HeaderText);
			AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_From", Env.CurrentCompany.GetLicenceCode(), interchange.EI_From);
		});
	}

	public void TestPack_ForUpdateMessage()
	{
		SetDataAndAssertInterchangeForMessageRequiringMRN(transportMode: TransportTypeList.Codes.Road,
			messageType: NODMOEDIMessageTypeList.Codes.TRA,
			messageSubType: NODMOMessageFunctionList.Codes.UPD,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.RDT,
			getParent: (header, _) => header);
		SetDataAndAssertInterchangeForMessageRequiringMRN(transportMode: TransportTypeList.Codes.Rail,
			messageType: NODMOEDIMessageTypeList.Codes.HCS,
			messageSubType: NODMOMessageFunctionList.Codes.UPD,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.RLH,
			getParent: (_, bill) => bill);
	}

	public void TestPack_ForDeleteMessage()
	{
		SetDataAndAssertInterchangeForMessageRequiringMRN(transportMode: TransportTypeList.Codes.Road,
			messageType: NODMOEDIMessageTypeList.Codes.TRA,
			messageSubType: NODMOMessageFunctionList.Codes.DEL,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.RDT,
			getParent: (header, _) => header);
		SetDataAndAssertInterchangeForMessageRequiringMRN(transportMode: TransportTypeList.Codes.Rail,
			messageType: NODMOEDIMessageTypeList.Codes.HCS,
			messageSubType: NODMOMessageFunctionList.Codes.DEL,
			expectedInterchangeType: NODMOInterchangeTypeList.Codes.RLH,
			getParent: (_, bill) => bill);
	}

	void SetDataAndAssertInterchangeForMessageRequiringMRN(string transportMode,
		string messageType,
		string messageSubType,
		string expectedInterchangeType,
		Func<AsycudaManifestHeader, AsycudaBill, BusinessObject> getParent)
	{
		var (header, bill) = CreateHeaderWithBill(transportMode);
		var parent = getParent(header, bill);
		UpdateMessageData(messageSubType, messageType, parent);
		var movementReferenceNumberSupporter = (IMovementReferenceNumberSupporter)parent;
		movementReferenceNumberSupporter.MovementReferenceNumber = "12345";

		var result = MessagePacker.Pack(message, interchange, loggingInformation);
		
		CombineAssertions($"TransportMode: {transportMode}, MessageType: {messageType}", () =>
		{
			AssertNullOrEmpty("Output String", result);
			AssertEquals("EI_InterchangeType", expectedInterchangeType, interchange.EI_InterchangeType);
			AssertEquals("EI_ApplicationCode", "NOD", interchange.EI_ApplicationCode);
			AssertEquals("EI_HeaderText", $"{{\"custom.MessageSubType\":\"{message.EM_MessageSubType}\",\"custom.ReferenceNumber\":\"12345\",\"custom.NO.Issuer\":\"{password.GP_UserID}\"}}", interchange.EI_HeaderText);
			AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_From", Env.CurrentCompany.GetLicenceCode(), interchange.EI_From);
			AssertEquals("EI_GP", password.PK, interchange.EI_GP);
		});
	}

	public void TestPack_ForMessageWithNoInterchangeTypeMapping()
	{
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.UPD;
		message.EM_MessageType = "XYZ";
		var (header, _) = CreateHeaderWithBill(TransportTypeList.Codes.Road);
		message.EM_LinkUniqueID = header.PK;
		message.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;

		var result = MessagePacker.Pack(message, interchange, loggingInformation);
		CombineAssertions(() =>
		{
			AssertEquals("Output String", "Unable to find mapping for Transport Mode: ROA and Message Type (Custom Level): XYZ", result);
			AssertEquals("EI_Status", EDIInterchange.Status.Discarded, interchange.EI_Status);
		});
	}

	[ExpectNoExceptions]
	public void TestLoggedInformation_WithCompleteInformation()
	{
		message.EM_MessageSubType = NODMOMessageFunctionList.Codes.NEW;
		message.EM_MessageType = NODMOEDIMessageTypeList.Codes.TRA;
		var (header, _) = CreateHeaderWithBill(TransportTypeList.Codes.Road);
		message.EM_LinkUniqueID = header.PK;
		message.EM_LinkTable = AsycudaManifestHeader.Schema.TableName;
		header.MovementReferenceNumber = "12345";

		var loggerMock = new Mock<LoggingInformation>();
		_ = MessagePacker.Pack(message, interchange, loggerMock.Object);

		CombineAssertions(() =>
		{
			loggerMock
				.Verify(l => l.Log(It.Is<LogType>(t => t == LogType.Information), It.Is<string>(m => m == $"Started Processing EDIMessage with PK: [{message.PK}]")),
				Times.Once);

			loggerMock
				.Verify(l => l.Log(It.Is<LogType>(t => t == LogType.Information), It.Is<string>(m => m == $"Message Information Packed successfully for EDIMessage with PK: [{message.PK}]")),
					Times.Once);
		});
	}

	[ExpectNoExceptions]
	public void TestLoggedInformation_WithIncompleteMessageInformation()
	{
		message.EM_MessageText = ZString.Empty;
		var loggerMock = new Mock<LoggingInformation>();
		_ = MessagePacker.Pack(message, interchange, loggerMock.Object);

		CombineAssertions(() =>
		{
			loggerMock
				.Verify(l => l.Log(It.Is<LogType>(t => t == LogType.Information), It.Is<string>(m => m == $"Started Processing EDIMessage with PK: [{message.PK}]")),
					Times.Once);

			loggerMock
				.Verify(l => l.LogError(It.Is<string>(s => s == $"Failed to process EDIMessage with PK: [{message.PK}]")),
					Times.Once);
		});
	}

	protected override string ApplicationCode => Messaging.Integration.ApplicationCodeList.Codes.NOCustomsDMO;

	protected override void SetUp()
	{
		base.SetUp();

		var messageNumberStrategy = Mock.Of<IMessageNumberStrategy>(s => s.GetMessageReferenceNumber() == "123");
		message = Factory.NewWithValidTestData<EDIMessage>();
		message.MessageNumberStrategy = messageNumberStrategy;
		message.EM_MessageText = "SOME_SAMPLE_MESSAGE_TEXT";

		var company = Factory.NewWithValidTestData<GlbCompany>();
		branch = company.Branches.AddNew();
		message.EM_GB = branch.PK;

		password = CreateExternalPasswaord();
		password.GP_GC = company.PK;

		interchange = Factory.New<EDIInterchange>();
		loggingInformation = Mock.Of<LoggingInformation>();
	}

	EDIMessage message;
	EDIInterchange interchange;
	LoggingInformation loggingInformation;
	GlbExternalPassword_NOD password;
	GlbBranch branch;

	IUniversalCustomsEDIMessagePacker MessagePacker => messagePacker ??= new DMOMessagePacker();
	IUniversalCustomsEDIMessagePacker messagePacker;

	void UpdateMessageData(string messageSubType, string messageType, BusinessObject parent)
	{
		message.EM_MessageSubType = messageSubType;
		message.EM_MessageType = messageType;
		message.EM_LinkUniqueID = parent.PK;
		message.EM_LinkedObject = parent;
	}

	(AsycudaManifestHeader ManifestHeader, AsycudaBill Bill) CreateHeaderWithBill(ZString transportMode)
	{
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_TransportMode = transportMode;
		var bill = header.Bills.AddNew();
		return (header, bill);
	}

	GlbExternalPassword_NOD CreateExternalPasswaord()
	{
		var password = Factory.NewWithValidTestData<GlbExternalPassword_NOD>();
		password.GP_CurrentPassword = "3V6njpfnLSrfrpsUA66llg==";
		return password;
	}
}
