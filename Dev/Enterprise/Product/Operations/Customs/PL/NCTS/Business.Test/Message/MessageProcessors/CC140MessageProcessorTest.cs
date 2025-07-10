using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC140MessageProcessor))]
sealed class CC140MessageProcessorTest : BaseNctsMessageProcessorTestCase<CC140MessageProcessor, IIE140>
{
	protected override Type ExpectedMessageInterpreterType => typeof(CC140CMessageInterpreter);

	protected override string ExpectedMessageFriendlyName => MessageNameList.Descriptions.IE140;

	protected override bool ExpectedIsFailureNotification => false;

	public void TestProcessMessage()
	{
		const string testMrn = "1234567ABC";

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var message = Factory.New<EDIMessage>();
		message.EM_LinkTable = "CusInBondMoveHeader";
		message.EM_LinkUniqueID = testNctsHeader.MovementHeader.PK;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Linked object BM_MessageStatus", expected: NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry, testNctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("Linked object BM_MessageStatus", expected: Common.EU.LogicalStatusList.Codes.Accepted, testNctsHeader.MovementHeader.BM_MessageStatus);
			AssertEquals("Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	[TestDate(2024, 10, 15, 01, 00, 05)]
	public void TestProcessMessage_NctsHeaderLog_SameDay()
	{
		const string testMrn = "1234567ABC";
		var requestOnNonArrivedMovementDate = new DateTime(2024, 10, 15);
		var limitForResponseDate = new DateTime(2024, 10, 16);

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader.MovementHeader;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		mockTransitOperation.Setup(x => x.RequestOnNonArrivedMovementDate).Returns(requestOnNonArrivedMovementDate);
		mockTransitOperation.Setup(x => x.LimitForResponseDate).Returns(limitForResponseDate);

		MessageProcessor.ProcessMessage(message);

		var movementHeader = testNctsHeader.MovementHeader;
		var mostRecentLog = movementHeader.Logs.MostRecentLog;

		AssertNotNull("New log should be created", mostRecentLog);

		CombineAssertions(() =>
		{
			AssertEquals("NctsHeader should have a single StmAlog with CIP event", AutoEvents.CustomsImpedimentReceivedCode, mostRecentLog.SL_SE_NKEvent);
			AssertEquals("SL_EventTime should be equal to current UTC time", ZDateTime.Now, mostRecentLog.SL_EventTime);
			AssertEquals("SL_GB_NKBranch should be equal to branch ID of a currently logged in user", Env.CurrentBranch.Code, mostRecentLog.SL_GB_NKBranch);
			AssertEquals("SL_GE_NKDepartment should be equal to departure ID of a currently logged in user", Env.CurrentDepartment.Code, mostRecentLog.SL_GE_NKDepartment);
			AssertEquals("SL_Parent should be equal to current NctsDepartureMovementHeader ID", movementHeader.PK, mostRecentLog.SL_Parent);
			AssertEquals("SL_Table should be equal to 'CusInBondMoveHeader'", AutoCusInBondMoveHeader.Schema.TableName, mostRecentLog.SL_Table);
			AssertEquals("SL_Reference should be equal to '<BM_CustomsStatus>-Limit Date To Response:<CY_Date>, where CY_Code is 'ENQ' and CY_Type is 'EUO'", "ENQ-Limit Date To Response:2024-10-16", mostRecentLog.SL_Reference);
		});
	}

	public void TestProcessMessage_NctsHeaderLog_NextDay()
	{
		const string testMrn = "1234567ABC";
		var requestOnNonArrivedMovementDate = new DateTime(2024, 10, 15);
		var limitForResponseDate = new DateTime(2024, 10, 16);

		var testNctsHeader = Factory.New<NctsHeader>();
		testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = testNctsHeader.MovementHeader;
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		dataProviderMock.Setup(x => x.MRN).Returns(testMrn);
		mockTransitOperation.Setup(x => x.RequestOnNonArrivedMovementDate).Returns(requestOnNonArrivedMovementDate);
		mockTransitOperation.Setup(x => x.LimitForResponseDate).Returns(limitForResponseDate);

		MessageProcessor.ProcessMessage(message);

		var movementHeader = testNctsHeader.MovementHeader;
		var mostRecentLog = movementHeader.Logs.MostRecentLog;

		AssertNotNull("New log should be created", mostRecentLog);

		CombineAssertions(() =>
		{
			AssertEquals("NctsHeader should have a single StmAlog with CIP event", AutoEvents.CustomsImpedimentReceivedCode, mostRecentLog.SL_SE_NKEvent);
			AssertEquals("SL_EventTime should be equal to /IE140PL/CC140C/TransitOperation/requestOnNonArrivedMovementDate", requestOnNonArrivedMovementDate.Date, mostRecentLog.SL_EventTime.Date);
			AssertEquals("SL_GB_NKBranch should be equal to branch ID of a currently logged in user", Env.CurrentBranch.Code, mostRecentLog.SL_GB_NKBranch);
			AssertEquals("SL_GE_NKDepartment should be equal to departure ID of a currently logged in user", Env.CurrentDepartment.Code, mostRecentLog.SL_GE_NKDepartment);
			AssertEquals("SL_Parent should be equal to current NctsDepartureMovementHeader ID", movementHeader.PK, mostRecentLog.SL_Parent);
			AssertEquals("SL_Table should be equal to 'CusInBondMoveHeader'", AutoCusInBondMoveHeader.Schema.TableName, mostRecentLog.SL_Table);
			AssertEquals("SL_Reference should be equal to '<BM_CustomsStatus>-Limit Date To Response:<CY_Date>, where CY_Code is 'ENQ' and CY_Type is 'EUO'", "ENQ-Limit Date To Response:2024-10-16", mostRecentLog.SL_Reference);
		});
	}

	public void TestProcessMessage_CreatesCusCodeData_WithCustomsOfficeOfEnquiryAtDeparture()
	{
		const string mrn = "1234567ABC";
		const string customsOfficeOfEnquiryAtDeparture = "reference1";
		var limitForResponseDate = new DateTime(year: 2024, month: 10, day: 15, hour: 00, minute: 00, second: 00, kind: DateTimeKind.Utc);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = nctsHeader.MovementHeader;
		message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
		dataProviderMock.Setup(x => x.CustomsOfficeOfEnquiryAtDeparture).Returns(customsOfficeOfEnquiryAtDeparture);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		mockTransitOperation.Setup(x => x.LimitForResponseDate).Returns(limitForResponseDate);

		MessageProcessor.ProcessMessage(message);

		var movementHeader = nctsHeader.MovementHeader;

		CombineAssertions(() =>
		{
			Assert("No CustomsOffice were created", movementHeader.CustomsOffices.Any());
			var customsOffice = movementHeader.CustomsOffices.Last();
			AssertEquals("CY_Data should be equal to /IE140PL/CC140C/CustomsOfficeOfEnquiryAtDeparture/referenceNumber", customsOfficeOfEnquiryAtDeparture, customsOffice.CY_Data);
			AssertEquals("CY_Code should be equal to 'ENQ'", "ENQ", customsOffice.CY_Code);
			AssertEquals("CY_Type should be equal to 'EUO'", "EUO", customsOffice.CY_Type);
			AssertEquals("CY_ParentTable should be equal to 'BM'", "BM", customsOffice.CY_ParentTableCode);
			AssertEquals("CY_ParentID should be equal to 'CusInBondMoveHeader.BM_PK'", movementHeader.PK, customsOffice.CY_ParentID);
			AssertEquals("CY_Date should be equal to /IE140PL/CC140C/TransitOperation/limitForResponseDate", limitForResponseDate, customsOffice.CY_Date);
		});
	}

	public void TestEmail()
	{
		const string transmitMessageNum = "24IE01500123";
		const string mrn = "TEST_MRN";
		var correlationIdentifier = transmitMessageNum;

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(NctsMovementType.Codes.Departure, expectedSubjectPrefix: $"IE140_Search_Procedure_({mrn})", transmitMessageNum: transmitMessageNum, mrn: null, lrn: null);
	}

	protected override void SetUp()
	{
		base.SetUp();

		mockTransitOperation = new Mock<ICC140CTransitOperation>();
		dataProviderMock.Setup(x => x.TransitOperation).Returns(mockTransitOperation.Object);
	}

	Mock<ICC140CTransitOperation> mockTransitOperation;
}
