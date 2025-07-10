using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ForwardingShipmentCustomsStatusProviderTest : TestCaseWithFactory
	{
		public void TestStatusIsEmptyWhenNoDeclarationExists()
		{
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", "", statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.CustomsMessageStatus()", "", statusProvider.CustomsMessageStatus());
		}

		public void TestStatusFromImportDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = true;
			var eNSentry = declaration.CustomsEntryHeaders.AddNew();
			eNSentry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var cRLentry = declaration.CustomsEntryHeaders.AddNew();
			cRLentry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			cRLentry.CH_Status = ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal;
			declaration.JE_JS = Shipment.PK;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", ImportMessageStatusList.Descriptions.ClearBorderCargoReleaseOriginal, statusProvider.CustomsCargoStatus());
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			cRLentry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			cRLentry.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseReplace;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", ImportMessageStatusList.Descriptions.AwaitingACECargoReleaseReplace, statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.SEBillStatus()", ZString.Empty, statusProvider.SEBillStatus());
			AssertEquals("statusProvider.HLDOrEXMStatus()", ZString.Empty, statusProvider.HLDOrEXMStatus());
			cRLentry.CH_Status = ZString.Empty;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", ImportMessageStatusList.Descriptions.NotSent, statusProvider.CustomsCargoStatus());
			eNSentry.CH_Status = ZString.Empty;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", ImportMessageStatusList.Descriptions.NotSent, statusProvider.CustomsMessageStatus());

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode, "SO50D", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code93 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "93", "BILL ON FILE", startDate, endDate);
			Factory.Save();

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "Test1";
			bill.Declaration.simplifiedEntryBillStatus = null;
			bill.Declaration.simplifiedEntryBillStatusDescription = null;
			bill.DispositionCodes.AddNewIfNotExist("93", ZDateTime.Today.AddDays(-1), BillDispositionSourceList.Codes.SO);
			SetupStatusProvider();
			AssertEquals("description contains Transfer of liability for container", true, statusProvider.SEBillStatus().Contains(code93.ZZD_Description));
		}

		public void TestStatusFromImportDeclarationACECargoRelease()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_JS = Shipment.PK;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var cRLentry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			cRLentry.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseReplace;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", ImportMessageStatusList.Descriptions.AwaitingACECargoReleaseReplace, statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.HLDOrEXMStatus()", ZString.Empty, statusProvider.HLDOrEXMStatus());
			var eNSentry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			cRLentry.CH_Status = ZString.Empty;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", ImportMessageStatusList.Descriptions.NotSent, statusProvider.CustomsCargoStatus());
			eNSentry.CH_Status = ZString.Empty;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsMessageStatus()", ImportMessageStatusList.Descriptions.NotSent, statusProvider.CustomsMessageStatus());
		}

		public void TestStatusFromExportDeclaration()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader eXPentry = declaration.CustomsEntryHeaders.AddNew();
			eXPentry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			eXPentry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			declaration.JE_JS = Shipment.PK;
			SetupStatusProvider();
			AssertEquals("statusProvider.CustomsCargoStatus()", "", statusProvider.CustomsCargoStatus());
			AssertEquals("statusProvider.CustomsMessageStatus()", AESDirectCustomsEntryStatus.Descriptions.OriginalSEDClear, statusProvider.CustomsMessageStatus());
		}

		void SetupStatusProvider()
		{
			Factory.Save();
			statusProvider = new ForwardingShipmentCustomsStatusProvider(Shipment);
		}
		ForwardingShipmentCustomsStatusProvider statusProvider;

		ForwardingShipment shipment;
		ForwardingShipment Shipment => shipment ?? (shipment = Factory.NewWithValidTestData<ForwardingShipment>());
	}
}
