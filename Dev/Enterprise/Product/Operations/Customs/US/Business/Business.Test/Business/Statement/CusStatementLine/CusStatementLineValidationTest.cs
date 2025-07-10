using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusStatementLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWhenReconDeclaration()
		{
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration2.US_EntryFilerCode = "XJ6";
			declaration2.ReleaseStatus = CRLReleaseStatusList.Codes.CAN;
			var entry = declaration2.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "2";
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ6";
			statementLine.B3_EntryNum = "2";

			AssertNotNull(statementLine.Declaration);
			statementLine.Validation.ValidateAll();
			AssertEquals("A CusStatementLine associated with a Recon Declaration has not error message", false, statementLine.ReleaseStatusInfo.Notifications.HasMessageErrors());
		}

		public void TestCheckB3_CustomsFeesTotalWhenNoEntryNumber()
		{
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.CAN;
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ6";
			statementLine.B3_EntryNum = "1";

			AssertNull(statementLine.Declaration);
			statementLine.Validation.ValidateAll();
			AssertEquals(false, statementLine.ReleaseStatusInfo.Notifications.HasMessageErrors());
		}

		public void TestCheckB3_CustomsFeesTotalNoEntryNumber()
		{
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var mockLine = Factory.NewMoq<CusStatementLine>();
			mockLine.Object.B3_EntryNum = "3";
			mockLine.Object.B3_EntryFilerCode = "XJ5";
			mockLine.Object.B3_B2 = statement.PK;

			TestQuery query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.ARPostedAmount = 1000m;
			query.Result.ARUnPostedAmount = 0m;
			mockLine.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query);
			mockLine.Object.B3_CustomsFeesTotal = 1000m;
			mockLine.Object.Validation.ValidateB3_CustomsFeesTotal();
			AssertNull(mockLine.Object.Declaration);
			AssertEquals(false, mockLine.Object.B3_CustomsFeesTotalInfo.Notifications.HasMessageErrors());
			AssertEquals(false, mockLine.Object.ReleaseStatusInfo.Notifications.HasMessageErrors());
		}

		public void TestCheckB3_CustomsFeesTotalWhenNotReleaseStatus()
		{
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.CAN;
			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "1";

			AssertNotNull(statementLine.Declaration);
			statementLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(statementLine.ReleaseStatusInfo, CusStatementLineValidation.NotReleaseStatus);

			statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;
			statementLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(statementLine.ReleaseStatusInfo, CusStatementLineValidation.NotReleaseStatus);
		}

		public void TestCheckReleaseStatusWhenEntryTypeIs3138()
		{
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.NRL;

			var statement = Factory.New<CusStatementHeader>();
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "1";

			statementLine.B3_EntryType = EntryTypeList.Codes.WarehouseFTZ;
			statementLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(statementLine.ReleaseStatusInfo, CusStatementLineValidation.NotReleaseStatus);

			declaration.US_ConsolACE = true;
			statementLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(statementLine.ReleaseStatusInfo, CusStatementLineValidation.NotReleaseStatus);

			declaration.US_ConsolACE = false;
			statementLine.B3_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			statementLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(statementLine.ReleaseStatusInfo, CusStatementLineValidation.NotReleaseStatus);

			statementLine.B3_EntryType = EntryTypeList.Codes.WarehouseWithdrawalQuota;
			statementLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(statementLine.ReleaseStatusInfo, CusStatementLineValidation.NotReleaseStatus);

			statementLine.B3_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			statementLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(statementLine.ReleaseStatusInfo, CusStatementLineValidation.NotReleaseStatus);

			statementLine.B3_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVDQuotaVisa;
			statementLine.Validation.ValidateAll();
			AssertNoMessageErrorContaining(statementLine.ReleaseStatusInfo, CusStatementLineValidation.NotReleaseStatus);
		}

		public void TestCheckB3_CustomsFeesTotalWhenNotMatchAmount()
		{
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var mockLine = Factory.NewMoq<CusStatementLine>();
			mockLine.Object.B3_EntryNum = "1";
			mockLine.Object.B3_EntryFilerCode = "XJ5";
			mockLine.Object.B3_B2 = statement.PK;

			TestQuery query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.ARPostedAmount = 1000m;
			query.Result.ARUnPostedAmount = 500m;
			mockLine.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query);
			mockLine.Object.B3_CustomsFeesTotal = 2000m;

			AssertEquals("ARTotalAmount", 1500m, mockLine.Object.ARTotalAmount);
			mockLine.Object.Validation.ValidateB3_CustomsFeesTotal();
			AssertHasMessageErrorContaining(mockLine.Object.B3_CustomsFeesTotalInfo, CusStatementLineValidation.CustomsFeeTotal);

			mockLine.Object.B3_Status = StatementLineStatusList.Codes.Deleted;
			mockLine.Object.Validation.ValidateB3_CustomsFeesTotal();
			AssertNoMessageErrorContaining(mockLine.Object.B3_CustomsFeesTotalInfo, CusStatementLineValidation.CustomsFeeTotal);
		}

		public void TestCheckB3_CustomsFeesTotalHasAnUnpostedARAmount()
		{
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var mockLine = Factory.NewMoq<CusStatementLine>();
			mockLine.Object.B3_EntryNum = "1";
			mockLine.Object.B3_EntryFilerCode = "XJ5";
			mockLine.Object.B3_B2 = statement.PK;

			TestQuery query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.ARPostedAmount = 1000m;
			query.Result.ARUnPostedAmount = 500m;
			mockLine.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query);
			mockLine.Object.B3_CustomsFeesTotal = 1500m;
			mockLine.Object.Validation.ValidateB3_CustomsFeesTotal();

			AssertEquals("ARUnPostedAmount", 500m, mockLine.Object.ARUnPostedAmount);
			AssertHasWarningContaining(mockLine.Object.B3_CustomsFeesTotalInfo, CusStatementLineValidation.ARPostedArAmount);

			mockLine.Object.B3_Status = StatementLineStatusList.Codes.Deleted;
			mockLine.Object.Validation.ValidateB3_CustomsFeesTotal();
			AssertNoWarningContaining(mockLine.Object.B3_CustomsFeesTotalInfo, CusStatementLineValidation.ARPostedArAmount);
		}

		public void TestCheckB3_CustomsFeesTotalNoErrorMessage()
		{
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var mockLine = Factory.NewMoq<CusStatementLine>();
			mockLine.Object.B3_EntryNum = "1";
			mockLine.Object.B3_EntryFilerCode = "XJ5";
			mockLine.Object.B3_B2 = statement.PK;

			TestQuery query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.ARPostedAmount = 1000m;
			query.Result.ARUnPostedAmount = 0m;
			mockLine.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query);
			mockLine.Object.B3_CustomsFeesTotal = 1000m;
			mockLine.Object.Validation.ValidateB3_CustomsFeesTotal();
			AssertEquals(false, mockLine.Object.B3_CustomsFeesTotalInfo.Notifications.HasMessageErrors());
			AssertEquals(false, mockLine.Object.ReleaseStatusInfo.Notifications.HasMessageErrors());
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "1";
			Factory.Save();
		}
	}
}
