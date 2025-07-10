using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS.Testing
{
	sealed class CBPRequestValueProviderTest : TestCaseWithFactory
	{
		public void TestDetails()
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
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "221";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var cbpRequests = declaration.EntryStatusesAndErrors.ENSStatusNotifications;
			declaration.ImportEntryNumber = "36194291";

			var request1 = cbpRequests.AddNew();
			request1.DispositionCode = ENSStatusDispositionCodeList._1;
			request1.ActionIDNumber = "1";
			request1.StatusDate = new ZDateTime(2013, 1, 10);

			var request2 = cbpRequests.AddNew();
			request2.ActionIDNumber = "2";
			request2.StatusDate = new ZDateTime(2013, 1, 20);
			request2.DispositionCode = ENSStatusDispositionCodeList._5;

			var request3 = cbpRequests.AddNew();
			request3.ActionIDNumber = "3";
			request3.StatusDate = new ZDateTime(2013, 1, 21);
			request3.DispositionCode = ENSStatusDispositionCodeList._6;
			Assert("PreCondition", !ENSStatusDispositionCodeListLoader.IsFurtherActionRequired(Factory, request3.DispositionCode));

			var request4 = cbpRequests.AddNew();
			request4.ActionIDNumber = "";
			request4.StatusDate = new ZDateTime(2013, 1, 22);
			request4.DispositionCode = ENSStatusDispositionCodeList._5;

			var disCBPRequests = new CBPRequestValueProvider(declaration).CBPRequests;
			AssertEquals(2, disCBPRequests.Count());

			var disCBPRequest1 = disCBPRequests.ElementAt(0);
			var disCBPRequest2 = disCBPRequests.ElementAt(1);

			AssertEquals("1", disCBPRequest1.ID);
			AssertEquals(new ZDateTime(2013, 1, 10), disCBPRequest1.RequestDate);
			AssertEquals("Requested on 01-10-13 (1 DESC)", disCBPRequest1.Description);
			AssertEquals(CBPRequestType.ACEActionNumber, disCBPRequest1.Type);

			AssertEquals("2", disCBPRequest2.ID);
			AssertEquals(new ZDateTime(2013, 1, 20), disCBPRequest2.RequestDate);
			AssertEquals("Requested on 01-20-13 (5 DESC)", disCBPRequest2.Description);
			AssertEquals(CBPRequestType.ACEActionNumber, disCBPRequest2.Type);
		}

		public void TestGetDISCBPRequestListFromSO60Message()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "221";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "36194291";

			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_MessageNum = "1";
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message1.EM_MessageText = "B003001221SO                                                                    " +
				"SO103002221  36194291 0193-108633000HDMUHYUNDAI FAITH       44E  022415         " +
				"SO20CR X00019645                                                                " +
				"SO40RHDMUQSWB4396229                                       00000633CT   00000633" +
				"SO50031115131395BILL ARRIVED                                                    " +
				"SO60031115131303PENDING INTENSIVE EXAM                                          " +
				"SO60031115131384DOC REQUIRED FOR CORRECTION REQUEST                             " +
				"SO60031115131396DOCUMENT REQUIRED                               01              " +
				"Y  3001221SO00000";
			declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(message1);

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageNum = "2";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message2.EM_MessageText = "B003001221SO                                                                    " +
				"SO103002221  36194291 0193-108633000HDMUHYUNDAI FAITH       44E  022415         " +
				"SO20CR X00019645                                                                " +
				"SO40RHDMUQSWB4396229                                       00000633CT   00000633" +
				"SO50031115131395BILL ARRIVED                                                    " +
				"SO60031115131303PENDING INTENSIVE EXAM                                          " +
				"SO60031115131396DOCUMENT REQUIRED                               02              " +
				"Y  3001221SO00000";
			declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(message2);

			var message3 = Factory.New<MQEDIMessage>();
			message3.EM_MessageNum = "3";
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message3.EM_MessageText = "B003001221SO                                                                    " +
				"SO103002221  36194291 0193-108633000HDMUHYUNDAI FAITH       44E  022415         " +
				"SO20CR X00019645                                                                " +
				"SO40RHDMUQSWB4396229                                       00000633CT   00000633" +
				"SO50031115131395BILL ARRIVED                                                    " +
				"SO60031115131303PENDING INTENSIVE EXAM                                          " +
				"SO60031115131396DOCUMENT REQUIRED                               02              " +
				"Y  3001221SO00000";
			declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(message3);

			var disCBPRequests = new CBPRequestValueProvider(declaration).CBPRequests;
			AssertEquals(3, disCBPRequests.Count());

			var disCBPRequest1 = disCBPRequests.ElementAt(0);
			var disCBPRequest2 = disCBPRequests.ElementAt(1);
			var disCBPRequest3 = disCBPRequests.ElementAt(2);

			CombineAssertions(() =>
			{
				AssertEquals("9602-031115", disCBPRequest1.ID);
				AssertEquals(new ZDateTime(2015, 3, 11), disCBPRequest1.RequestDate);
				AssertEquals("Requested on 03-11-15 (DOCUMENT REQUIRED - Invoice)", disCBPRequest1.Description);
				AssertEquals(CBPRequestType.ACEActionNumber, disCBPRequest1.Type);

				AssertEquals("84-031115", disCBPRequest2.ID);
				AssertEquals(new ZDateTime(2015, 3, 11), disCBPRequest2.RequestDate);
				AssertEquals("Requested on 03-11-15 (DOC REQUIRED FOR CORRECTION REQUEST)", disCBPRequest2.Description);
				AssertEquals(CBPRequestType.ACEActionNumber, disCBPRequest2.Type);

				AssertEquals("9601-031115", disCBPRequest3.ID);
				AssertEquals(new ZDateTime(2015, 3, 11), disCBPRequest3.RequestDate);
				AssertEquals("Requested on 03-11-15 (DOCUMENT REQUIRED - Packing List)", disCBPRequest3.Description);
				AssertEquals(CBPRequestType.ACEActionNumber, disCBPRequest3.Type);
			});
		}

		public void TestGetDISCBPRequestListFromSO70Message()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "221";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "36194291";

			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_MessageNum = "1";
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message1.EM_MessageText = "B003001221SO                                                                    " +
				"SO103002221  36194291 0193-108633000HDMUHYUNDAI FAITH       44E  022415         " +
				"SO20CR X00019645                                                                " +
				"SO40RHDMUQSWB4396229                                       00000633CT   00000633" +
				"SO50031115131395BILL ARRIVED                                                    " +
				"SO70ACE000123013143910DOCUMENTS REQUIRED            02        2  0101000 01     " +
				"Y  3001221SO00000";
			declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(message1);

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageNum = "2";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message2.EM_MessageText = "B003001221SO                                                                    " +
				"SO103002221  36194291 0193-108633000HDMUHYUNDAI FAITH       44E  022415         " +
				"SO20CR X00019645                                                                " +
				"SO40RHDMUQSWB4396229                                       00000633CT   00000633" +
				"SO50031115131395BILL ARRIVED                                                    " +
				"SO70ACE000123013143910DOCUMENTS REQUIRED            02        2  0101000 01     " +
				"SO70ACE000123013143910DOCUMENTS REQUIRED            01        2  0101000 11     " +
				"Y  3001221SO00000";
			declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(message2);

			var message3 = Factory.New<MQEDIMessage>();
			message3.EM_MessageNum = "3";
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message3.EM_MessageText = "B003001221SO                                                                    " +
				"SO103002221  36194291 0193-108633000HDMUHYUNDAI FAITH       44E  022415         " +
				"SO20CR X00019645                                                                " +
				"SO40RHDMUQSWB4396229                                       00000633CT   00000633" +
				"SO50123113131395BILL ARRIVED                                                    " +
				"SO60071916132696DOCUMENT REQUIRED                               CBP03           " +
				"Y  3001221SO00000";
			declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.Add(message3);

			var disCBPRequests = new CBPRequestValueProvider(declaration).CBPRequests.OrderBy(r => r.ID);
			AssertEquals(3, disCBPRequests.Count());

			var disCBPRequest = disCBPRequests.ElementAt(0);
			AssertEquals("0111-123013", disCBPRequest.ID);
			AssertEquals(new ZDateTime(2013, 12, 30), disCBPRequest.RequestDate);
			AssertEquals("Requested on 123013 (DOCUMENTS REQUIRED - Ingredient list)", disCBPRequest.Description);
			AssertEquals(CBPRequestType.ACEActionNumber, disCBPRequest.Type);

			disCBPRequest = disCBPRequests.ElementAt(1);
			AssertEquals("0201-123013", disCBPRequest.ID);
			AssertEquals(new ZDateTime(2013, 12, 30), disCBPRequest.RequestDate);
			AssertEquals("Requested on 123013 (DOCUMENTS REQUIRED - Packing List)", disCBPRequest.Description);
			AssertEquals(CBPRequestType.ACEActionNumber, disCBPRequest.Type);

			disCBPRequest = disCBPRequests.ElementAt(2);
			AssertEquals("96CBP03-071916", disCBPRequest.ID);
		}
	}
}
