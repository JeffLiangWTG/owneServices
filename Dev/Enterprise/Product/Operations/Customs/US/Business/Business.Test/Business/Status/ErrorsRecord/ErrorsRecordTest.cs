using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ErrorsRecord))]
	sealed class ErrorsRecordTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateEntrySummaryStatusNotificationDetails()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ENSStatusDispositionCode, "UCDSP", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INoFurtherActionRequired, "INoFurtherActionRequired", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IActionRequiredDespiteActionID, "IActionRequiredDespiteActionID", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1", "1 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "2", "2 DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3", "3 DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "4", "4 DESC", startDate, endDate);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "5", "5 DESC", startDate, endDate);
			var code6 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "6", "6 DESC", startDate, endDate);
			var code7 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "7", "7 DESC", startDate, endDate);
			var code8 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "8", "8 DESC", startDate, endDate);
			var code9 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "9", "9 DESC", startDate, endDate);
			var codeE = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "E", "E DESC", startDate, endDate);
			var codeP = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "P", "P DESC", startDate, endDate);
			var codeQ = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Q", "Q DESC", startDate, endDate);
			var codeR = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "R", "R DESC", startDate, endDate);

			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName2.ZXE_Name, "Y");
			var attributeE1 = helper.CreateNewOrGetExistingCusCodeListAttribute(codeE.PK, attributeName2.ZXE_Name, "Y");
			var attribute61 = helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, attributeName1.ZXE_Name, "Y");
			var attribute71 = helper.CreateNewOrGetExistingCusCodeListAttribute(code7.PK, attributeName1.ZXE_Name, "Y");
			var attribute81 = helper.CreateNewOrGetExistingCusCodeListAttribute(code8.PK, attributeName1.ZXE_Name, "Y");
			var attributeQ1 = helper.CreateNewOrGetExistingCusCodeListAttribute(codeQ.PK, attributeName1.ZXE_Name, "Y");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E171333   090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";

			var record = new ErrorsRecord(message);
			record.UpdateEntrySummaryStatusNotificationDetails();

			AssertEquals("7", record.DispositionCode);
			AssertEquals("7 DESC", record.DispositionDescription);

			AssertEquals("1", record.SourceOfActionRequest);
			AssertEquals("Manual Request", record.SourceOfActionRequestDesc);
			AssertEquals("333", record.ImportSpecialistTeam);
			AssertEquals(new ZDateTime(2010, 9, 1), record.StatusDate);
			AssertEquals("", record.LineNumber);
			AssertEquals("CHRIS SMITH (Ph:5555555555)", record.CBPStaffContactDetails);
			AssertEquals("REQUESTED DOCUMENTS RECEIVED", record.BlockText);

			message.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.Authorised, "REVIEWED");
			AssertEquals("REVIEWED", record.ReferenceOnAction);
		}

		public void TestSO20CMTCargoReleaseStatusResponseAction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			declaration.ImportEntryNumber = "00941598";

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.EntryNumber = "00941598";

			Factory.Save();

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ApplicationReference = ReferenceIdentifierQualifierCodeList.Codes.CMT;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message.EM_MessageText =
			"B003002906SO                                                                    " +
			"SO103002906  00941598 0113-611944100KKLUYM MATURITY         41E  040217         " +
			"SO20CMTTRANSFER FOR EXAM TO CES MERCER  PLEASE UPLOAD ENT                       " +
			"SO20CMTRY DOCS TO DIS                                                           " +
			"SO20CR B00101688                                                                " +
			"SO40RKKLUNB3706038                                         00001960     00001960" +
			"SO50040317105895BILL ARRIVED                                                    " +
			"SO60040317105822RELEASE DATE UPDATE                     04031701                " +
			"SO60040317105898RELEASED                                04031701                " +
			"SO60040317105801ONE USG                                                         " +
			"Y  3002906SO00000";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();

			var cusEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			cusEntry.Messages.Add(message);
			var result = cusEntry.Messages.Find(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus);
			AssertNotNull(result);

			declaration.Reload();
			var errRecords = declaration.EntryStatusesAndErrors.ACECargoRelReferenceData;
			AssertEquals("ACE CargoRelease Reference Data", 2, declaration.ACECargoRelReferenceData.Count);

			ErrorsRecord oneCMT = errRecords.OfType<ErrorsRecord>().First(x => x.ErrorMessageIdentifier == ReferenceIdentifierQualifierCodeList.Codes.CMT);
			AssertNotNull(oneCMT);
			AssertEquals(EM_ActionStatusList.Codes.Incomplete, oneCMT.ReferenceOnAction);
			AssertEquals(ReferenceIdentifierQualifierCodeList.Codes.CMT, oneCMT.ErrorMessageIdentifier);
			AssertEquals("TRANSFER FOR EXAM TO CES MERCER  PLEASE UPLOAD ENTRY DOCS TO DIS", oneCMT.NarrativeMessage);

			message.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.Authorised, "AAABBB", new ZDateTimeOffset(2011, 08, 14, 22, 08, 50));
			AssertEquals("AAABBB", "AAABBB", oneCMT.ReferenceOnAction);
			AssertEquals("Log Event Time", "14-Aug-11 22:08", oneCMT.ActionLogEventTime.ToLongTimeString());
		}

		public void TestCBPStaffContactDetailsMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E171333   090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITHSDFSDFSDFSDFSDFSDFSDF5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";

			var record = new ErrorsRecord(message);
			AssertNoExceptionThrown(() => record.UpdateEntrySummaryStatusNotificationDetails());
		}

		public void TestQuotaInformations()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E141333022090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"E4TA0Q01                                  10025       KG 8          KG          " +
"E4TA1Q02                                  99          G  100.11     MG          " +
"E4TA2Q03                                  78412252.13 OT 89         OT          " +
"E4TA3Q04                                  18          MC 877777.23  TL          " +
"E4TA4Q05                                  1.25        LT 78945614783T           " +
"Y  8888XJ5UC00003";

			var record = new ErrorsRecord(message);
			record.UpdateEntrySummaryStatusNotificationDetails();

			AssertEquals(5, record.QuotaInformations.Count);
			var quotaInformation = record.QuotaInformations.Cast<QuotaInformation>().FirstOrDefault(x => x.LineItemIdentifier == "TA0");
			AssertNotNull(quotaInformation);
			AssertEquals(quotaInformation.LineItemIdentifier, "TA0");
			AssertEquals(quotaInformation.QuotaLineStatusCode, "Q01");
			AssertEquals(quotaInformation.QuotaLineStatusDescription, "Quota Processed / Accepted");
			AssertEquals(quotaInformation.ReservedQuotaQuantity, 0.08m);
			AssertEquals(quotaInformation.ReservedQuotaQuantityUQ, "KG");
			AssertEquals(quotaInformation.RequestedQuotaQuantity, 100.25m);
			AssertEquals(quotaInformation.RequestedQuotaQuantityUQ, "KG");
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return ErrorsRecord;
		}

		ErrorsRecord ErrorsRecord
		{
			get { return errorsRecord ?? (errorsRecord = new ErrorsRecord()); }
		}
		ErrorsRecord errorsRecord;

		#endregion
	}
}
