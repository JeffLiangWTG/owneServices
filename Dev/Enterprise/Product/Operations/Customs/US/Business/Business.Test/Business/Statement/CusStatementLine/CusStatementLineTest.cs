using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusStatementLine))]
	sealed class CusStatementLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAccountingIntegrationForReconDeclaration()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_IsDebtor = true;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DisbursementCreditor.PK.ToGuid());
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupNotification);

			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";

			var reconIWrapper = new ReconDeclarationIReconciliation(declaration);
			new ReconMessageManager(reconIWrapper, US.Messaging.Business.UpdateActionCode.Add).PopulateMessage();

			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "4321089";
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statement.B2_StatementAmount = 25m;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = declaration.ReconEntryNumber;
			line.B3_CustomsFeesTotal = 25m;

			var customsJob = ((IAccInvoiceDataProvider)line).CustomsJob;
			AssertEquals(declaration, customsJob);
			Assert("isimport", customsJob.IsImport());
			AssertNull("null PaymentTerm", customsJob.GetFirstAdapter().PaymentTerm);

			var charge1 = line.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge1.B4_ChargeAmount = 25m;

			var job = new JobHeader.Loader(declaration.ReconWrappedJobDeclaration).Load();
			AssertNull(job);

			IAccInvoiceDataProvider dataProvider = line;

			statement.PostARInvoices = true;
			statement.PerformAccIntegration();
			job = new JobHeader.Loader(declaration.ReconWrappedJobDeclaration).Load();
			AssertNotNull(job);

			var charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("charge lines", 1, charges.Length);

			var charge = charges[0];
			AssertEquals("Debtor defaulted from ReconDeclaration.Importer", importer.PK, charge.JR_OH_SellAccount);
			AssertEquals("Amount", 25m, charge.JR_OSSellAmt);
		}

		OrgHeader DisbursementCreditor
		{
			get { return disbursementCreditor ?? (disbursementCreditor = CreateCreditorOrgHeader("~o~")); }
		}
		OrgHeader disbursementCreditor;

		OrgHeader CreateCreditorOrgHeader(string code)
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "Test Company Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";
			header.OH_Code = "Z" + code;

			header.OH_IsCreditor = true;
			header.CompanyData.SetAPTaxApplicable(true);
			header.MiscServ.OM_APWHTApplicable = true;

			Factory.Save();
			return header;
		}

		public void TestEntryForReconEntry()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EntryFilerCode = "XJ5";
			reconDeclaration.ReconEntry.GetEntry().EntryNumber = "1234";
			Factory.Save();
			AssertEquals("PreCondition", "1234", reconDeclaration.ReconEntryNumber);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = "1234";
			Factory.Save();
			AssertEquals(reconDeclaration, line.Declaration);

			var factory2 = new BusinessObjectFactory();
			var statementLineLoaded = factory2.Load<CusStatementLine>(line.PK);
			AssertEquals(reconDeclaration.PK, statementLineLoaded.Declaration.PK);
		}

		public void TestStatusSetToActive()
		{
			CusStatementLine statementLine = Factory.New<CusStatementLine>();
			AssertEquals(StatementLineStatusList.Codes.Active, statementLine.B3_Status);
		}

		public void TestStatusWhenSaveFails()
		{
			var header = Factory.New<CusStatementHeader>();
			var line = Factory.New<CusStatementLineWithException>();
			line.B3_B2 = header.PK;

			Factory.Save();
			AssertEquals(StatementLineStatusList.Codes.Active, statementLine.B3_Status);

			line.ThrowException = true;
			line.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			try
			{
				Factory.Save();
			}
			catch
			{
			}

			AssertEquals(StatementLineStatusList.Codes.Active, line.B3_Status);
		}

		class CusStatementLineWithException : CusStatementLine
		{
			public CusStatementLineWithException(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ThrowException;
			public override void OnSaving()
			{
				base.OnSaving();
				if (ThrowException)
				{
					throw new Exception("Blah");
				}
			}
		}

		public void TestCustomsCharges()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			CusStatementLine line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = "1";

			CusStatementLineCharge charge1 = line.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge1.B4_ChargeAmount = 25m;

			IAccInvoiceDataProvider dataProvider = line;
			CustomsCharge[] charges = dataProvider.CustomsCharges[0].GetCustomsCharges(null);

			AssertEquals(1, charges.Length);
			AssertEquals(25m, charges[0].Amount);
			AssertEquals(true, charges[0].IsPaidByBroker);
		}

		public void TestCustomsChargesForInformationOnly()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;

			CusStatementLine line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = "1";

			CusStatementLineCharge charge1 = line.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge1.B4_ChargeAmount = 25m;

			IAccInvoiceDataProvider dataProvider = line;
			CustomsCharge[] charges = dataProvider.CustomsCharges[0].GetCustomsCharges(null);

			AssertEquals(1, charges.Length);
			AssertEquals(25m, charges[0].Amount);
			AssertEquals(false, charges[0].IsPaidByBroker);
			AssertEquals(true, charges[0].IsInformationOnly);
		}

		public void TestAccountingIntegrationForWarehouseEntries()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;

			CusStatementLine line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryType = EntryTypeList.Codes.Warehouse;

			CusStatementLineCharge charge1 = line.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge1.B4_ChargeAmount = 25m;

			CusStatementLineCharge charge2 = line.Charges.AddNew();
			charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.HMF;
			charge2.B4_ChargeAmount = 10m;

			CusStatementLineCharge charge3 = line.Charges.AddNew();
			charge3.B4_ChargeType = "DTY";
			charge3.B4_ChargeAmount = 100m;

			IAccInvoiceDataProvider dataProvider = line;
			CustomsCharge[] charges = dataProvider.CustomsCharges[0].GetCustomsCharges(null);

			AssertEquals(3, charges.Length);

			var charge499 = charges.First(x => x.Description.Contains("499"));
			var charge501 = charges.First(x => x.Description.Contains("501"));
			var chargeDty = charges.First(x => x.Description.Contains("Duty"));

			AssertEquals(100m, chargeDty.Amount);
			AssertEquals(false, chargeDty.IsPaidByBroker);
			AssertEquals(true, chargeDty.IsInformationOnly);

			AssertEquals(25m, charge499.Amount);
			AssertEquals(false, charge499.IsPaidByBroker);
			AssertEquals(true, charge499.IsInformationOnly);

			AssertEquals(10m, charge501.Amount);
			AssertEquals(true, charge501.IsPaidByBroker);
			AssertEquals(false, charge501.IsInformationOnly);
		}

		[TestDate(2009, 1, 3)]
		public void TestIAccInvoiceDataProvider()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			statement.B2_PaymentAuthorizationDate = ZDateTime.Today.AddDays(-4);

			var line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = "1";

			var charge1 = line.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge1.B4_ChargeAmount = 25m;

			IAccInvoiceDataProvider dataProvider = line;

			AssertEquals("UniqueNumber", "1", dataProvider.UniqueNumber);
			AssertEquals(ZDateTime.Today.AddDays(-1), dataProvider.InvoiceDate);
			AssertEquals("No entry exists in the system with this entry number.", dataProvider.ReasonForUnbillability);
			AssertEquals(false, dataProvider.IsBillable);
			AssertNull(dataProvider.CustomsJob);
			statement.B2_PaymentAuthorizationDate = ZDateTime.Invalid;
			AssertEquals(ZDateTime.Today.AddDays(-1), dataProvider.InvoiceDate);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "1";
			Factory.Save();

			AssertEquals("", dataProvider.ReasonForUnbillability);
			AssertEquals(true, dataProvider.IsBillable);
			AssertEquals(declaration, dataProvider.CustomsJob);
		}

		[TestDate(2009, 1, 2)]
		public void TestInvoiceDateShouldTodayMinusOneBusinessDay()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());

			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			statement.B2_PaymentAuthorizationDate = ZDateTime.Today;

			CusStatementLine line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = "1";

			CusStatementLineCharge charge1 = line.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge1.B4_ChargeAmount = 25m;

			IAccInvoiceDataProvider dataProvider = line;
			AssertEquals(new ZDateTime(2008, 12, 31), dataProvider.InvoiceDate);

			CusStatementLine lineWithoutHeader = Factory.New<CusStatementLine>();
			lineWithoutHeader.B3_EntryFilerCode = "XJ5";
			lineWithoutHeader.B3_EntryNum = "1";
			var charge = lineWithoutHeader.Charges.AddNew();
			charge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge.B4_ChargeAmount = 22m;

			IAccInvoiceDataProvider dataProviderNoHeader = lineWithoutHeader;
			AssertEquals(new ZDateTime(2008, 12, 31), dataProviderNoHeader.InvoiceDate);
		}

		[TestDate(2009, 1, 10)]
		public void TestIAccInvoiceDataProviderDueDateForPeriodicStatement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PeriodicStatementMM = "01";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "1";
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "123456";
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statement.B2_PrintDate = new ZDateTime(2008, 12, 15);

			var line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = "1";

			var charge1 = line.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge1.B4_ChargeAmount = 25m;

			IAccInvoiceDataProvider dataProvider = line;
			AssertEquals(new DateTime(2009, 1, 22), dataProvider.APDueDate);

			statement.B2_PrintDate = new ZDateTime(2009, 1, 2);
			AssertEquals(new DateTime(2009, 1, 22), dataProvider.APDueDate);
		}

		public void TestIAccInvoiceDataProviderDueDateForDailyStatement()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "1";

			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "123456";
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statement.B2_PrintDate = new ZDateTime(2008, 12, 15);

			CusStatementLine line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = "1";

			CusStatementLineCharge charge1 = line.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge1.B4_ChargeAmount = 25m;

			IAccInvoiceDataProvider dataProvider = line;
			AssertEquals(ZDateTime.Today, dataProvider.APDueDate.Date);

			statement.B2_PrintDate = ZDateTime.Today.AddDays(1);
			AssertEquals(statement.B2_PrintDate, dataProvider.APDueDate.Date);

			statement.B2_PaymentAuthorizationDate = ZDateTime.Today.AddDays(2);
			AssertEquals(statement.B2_PaymentAuthorizationDate, dataProvider.APDueDate.Date);
		}

		public void TestGetAccountingAP_ARInvoiceAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "1";

			var statement = Factory.New<CusStatementHeader>();
			var mockLine = Factory.NewMoq<CusStatementLine>();
			mockLine.Object.B3_EntryNum = "1";
			mockLine.Object.B3_EntryFilerCode = "XJ5";
			mockLine.Object.B3_B2 = statement.PK;
			Factory.Save();

			var query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.APPostedAmount = 10m;
			query.Result.APUnPostedAmount = 20m;
			query.Result.ARPostedAmount = 40m;
			query.Result.ARUnPostedAmount = 80m;
			query.Result.APFullyPaid = true;

			mockLine.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query);

			AssertEquals("APPostedAmount", 10m, mockLine.Object.APPostedAmount);
			AssertEquals("APUnPostedAmount", 20m, mockLine.Object.APUnPostedAmount);
			AssertEquals("ARPostedAmount", 40m, mockLine.Object.ARPostedAmount);
			AssertEquals("ARUnPostedAmount", 80m, mockLine.Object.ARUnPostedAmount);
			AssertEquals("APFullyPaid", true, mockLine.Object.APFullyPaid);
		}

		public void TestHasDiscrepancyBetweenAccountingInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "1";
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			var mockLine = Factory.NewMoq<CusStatementLine>();
			mockLine.Object.B3_EntryNum = "1";
			mockLine.Object.B3_EntryFilerCode = "XJ5";
			mockLine.Object.B3_B2 = statement.PK;

			var query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.APPostedAmount = 10m;
			query.Result.APUnPostedAmount = 20m;
			query.Result.ARPostedAmount = 10m;
			query.Result.ARUnPostedAmount = 20m;

			mockLine.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query);

			var header = Factory.New<CusStatementHeader>();
			header.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			mockLine.Object.B3_B2 = header.PK;

			mockLine.Object.B3_CustomsFeesTotal = 30m;

			AssertEquals("DifferenceBetweenAPInvoiceAndCustomsAmount", 0m, mockLine.Object.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("DifferenceBetweenARInvoiceAndCustomsAmount", 0m, mockLine.Object.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", false, mockLine.Object.HasDiscrepancyBetweenInvoicesAndCustomsAmount);

			query.Result.ARPostedAmount = 40m;
			query.Result.ARUnPostedAmount = 80m;
			mockLine.Object.RefreshAccountingAP_ARInvoiceQueryResult();
			AssertEquals("DifferenceBetweenAPInvoiceAndCustomsAmount", 0m, mockLine.Object.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("DifferenceBetweenARInvoiceAndCustomsAmount", 90m, mockLine.Object.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", true, mockLine.Object.HasDiscrepancyBetweenInvoicesAndCustomsAmount);

			query.Result.APPostedAmount = 20m;
			mockLine.Object.RefreshAccountingAP_ARInvoiceQueryResult();
			AssertEquals("DifferenceBetweenAPInvoiceAndCustomsAmount", 10m, mockLine.Object.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("DifferenceBetweenARInvoiceAndCustomsAmount", 90m, mockLine.Object.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", true, mockLine.Object.HasDiscrepancyBetweenInvoicesAndCustomsAmount);
		}

		public void TestHasDiscrepancyBetweenAccountingInvoicesForImporterPayment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "1";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			var mockLine = Factory.NewMoq<CusStatementLine>();
			mockLine.Object.B3_EntryNum = "1";
			mockLine.Object.B3_EntryFilerCode = "XJ5";
			mockLine.Object.B3_B2 = statement.PK;

			var query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.APPostedAmount = 0m;
			query.Result.APUnPostedAmount = 0m;
			query.Result.ARPostedAmount = 0m;
			query.Result.ARUnPostedAmount = 0m;

			mockLine.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query);

			var header = Factory.New<CusStatementHeader>();
			mockLine.Object.B3_B2 = header.PK;
			mockLine.Object.B3_CustomsFeesTotal = 30m;

			AssertEquals("AP & AR should be compared with BrokerPaymentAmount", 0m, mockLine.Object.DifferenceBetweenAPInvoiceAndCustomsAmount);
			AssertEquals("AP & AR should be compared with BrokerPaymentAmount", 0m, mockLine.Object.DifferenceBetweenARInvoiceAndCustomsAmount);
			AssertEquals("HasDiscrepancy", false, mockLine.Object.HasDiscrepancyBetweenInvoicesAndCustomsAmount);
		}

		public void TestUserFees()
		{
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, 1m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, 2m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Duty, 3m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, 4m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, 5m);

			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Beef, 6m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Blueberry, 7m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Cotton, 8m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.DutiableMail, 9m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Avocado, 10m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Honey, 11m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, 12m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.FreshLimes, 13m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Mango, 14m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, 15m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 16m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Mushroom, 17m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Raspberry, 18m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Pork, 19m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Potato, 20m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.SoftwoodLumber, 21m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Sugar, 22m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Sorghum, 23m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Watermelon, 24m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.HMF, 25m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 26m);

			AssertEquals(336m, statementLine.UserFees);

			statementLine.B3_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(25m, statementLine.UserFees);
		}

		public void TestEstimatedDuty()
		{
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Duty, 3m);
			AssertEquals(3m, statementLine.EstimatedDuty);

			statementLine.B3_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(0m, statementLine.EstimatedDuty);
		}

		public void TestIsPaid()
		{
			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			CusStatementLine statementLine = statement.StatementLines.AddNew();
			AssertEquals(false, statementLine.IsPaid);

			//payment is attempted
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;
			AssertEquals(false, statementLine.IsPaid);

			//payment message is responded
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			AssertEquals(true, statementLine.IsPaid);

			//final statement is issued
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			AssertEquals(true, statementLine.IsPaid);
		}

		public void TestIsActive()
		{
			statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;
			AssertEquals(false, statementLine.IsActive);
			AssertEquals(true, statementLine.IsStatusDeleted);

			statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;
			AssertEquals(false, statementLine.IsActive);
			AssertEquals(false, statementLine.IsStatusDeleted);
			AssertEquals(true, statementLine.IsDeletionPending);

			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			AssertEquals(true, statementLine.IsActive);
			AssertEquals(false, statementLine.IsStatusDeleted);
		}

		public void TestHasAmountsToPay()
		{
			statementLine.B3_CustomsFeesTotal = 0m;
			AssertEquals(false, statementLine.HasAmountsToPay);

			statementLine.B3_CustomsFeesTotal = 10m;
			AssertEquals(true, statementLine.HasAmountsToPay);
		}

		public void TestEntry()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ6");
			statementHeader.B2_ProcessPort = "8888";

			statementLine.B3_EntryProcessPort = "8887";
			statementLine.B3_EntryFilerCode = "XJ6";
			statementLine.B3_EntryNum = "1234567";

			AssertNull(statementLine.Declaration);

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_EnableENS = false;

			dec1.JE_GB = GlbBranch.CurrentBranch.PK;
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry1.EntryNumber = "1234566";

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_EnableENS = false;
			dec2.US_EntryFilerCode = "XJ6";
			dec2.US_CertifyCargoRelease = false;

			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.EntryNumber = "1234567";
			Factory.Save();
			AssertEquals(entry2.Declaration, statementLine.Declaration);

			dec2.JE_DeclarationReference = "B000033";
			AssertEquals("B000033", statementLine.JobDeclarationBrokerReferenceNumber);
			AssertEquals(ZString.Empty, statementLine.ReleaseStatus);
			AssertEquals(ZString.Empty, statementLine.ReleaseStatusDescription);

			dec2.JE_EntryAuthorisationDate = ZDateTime.Today;
			AssertEquals(CRLReleaseStatusList.Codes.REL, statementLine.ReleaseStatus);
			AssertEquals(CRLReleaseStatusList.Descriptions.REL, statementLine.ReleaseStatusDescription);

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EntryFilerCode = "XJ5";
			var entry = reconDeclaration.ReconEntry.GetEntry();
			entry.EntryNumber = "71005051";
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "4321089";
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statement.B2_StatementAmount = 25m;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = reconDeclaration.ReconEntryNumber;
			line.B3_CustomsFeesTotal = 25m;
			AssertEquals(CRLReleaseStatusList.Codes.NRT, line.ReleaseStatus);
			AssertEquals(CRLReleaseStatusList.Descriptions.NRT, line.ReleaseStatusDescription);
		}

		public void TestEstimatedTaxesAndIndicators()
		{
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, 11.2m);

			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Duty, 15m);

			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 12m);

			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, 20.5m);

			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, 25m);

			AssertEquals(11.2m, statementLine.EstimatedCVD);
			AssertEquals(15m, statementLine.EstimatedDuty);
			AssertEquals("I", statementLine.InterestForReconciliationIndicator);
			AssertEquals(20.5m, statementLine.EstimatedADD);
			AssertEquals(25m, statementLine.EstimatedTax);
			AssertEquals(false, statementLine.IsDeferredTaxIndicator);

			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, 0);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, 5m);
			AssertEquals("This is used for doc printing and 'D' is printed along with the amount to indicate it is deferred", 5m, statementLine.EstimatedTax);

			statementLine.B3_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(5m, statementLine.EstimatedTax);
			AssertEquals(true, statementLine.IsDeferredTaxIndicator);
		}

		public void TestProperties()
		{
			statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;
			AssertEquals(StatementLineStatusList.Descriptions.Deleted, statementLine.LineStatusDescription);

			statementLine.B3_Team = "456";
			AssertEquals("456", statementLine.B3_Team);
		}

		public void TestEntryStatusDescription()
		{
			statementLine.B3_EntryStatus = StatementEntryStatus.Codes.Census;
			AssertEquals(StatementEntryStatus.Descriptions.Census, statementLine.EntryStatusDescription);

			statementLine.B3_EntryStatus = StatementEntryStatus.Codes.Paperless;
			AssertEquals(StatementEntryStatus.Descriptions.Paperless, statementLine.EntryStatusDescription);

			statementLine.B3_EntryStatus = ZString.Empty;
			AssertEquals(StatementEntryStatus.Descriptions.Paperless, statementLine.EntryStatusDescription);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.ImportEntryNumber = "1234567";
			declaration.US_EntryFilerCode = "XJ6";
			Factory.Save();

			statementLine.B3_EntryFilerCode = "XJ6";
			statementLine.B3_EntryNum = "1234567";
			AssertEquals("Documents Required", statementLine.EntryStatusDescription);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(StatementEntryStatus.Descriptions.Paperless, statementLine.EntryStatusDescription);
		}

		public void TestFormattedEntryNumber()
		{
			statementLine.B3_EntryFilerCode = "ABC";
			statementLine.B3_EntryNum = "12345678";

			AssertEquals("Formatted Entry No", "ABC-1234567-8", statementLine.FormattedEntryNumber);
		}

		public void TestICustomsChargeEntry()
		{
			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statementHeader.B2_DueDate = new ZDateTime(2009, 6, 1);

			statementLine.B3_EntryNum = "12345678";
			statementLine.B3_EntryFilerCode = "XJ5";

			IUSCustomsChargeEntry usChargeEntry = statementLine;
			ICustomsChargeEntry chargeEntry = statementLine;
			AssertNull(chargeEntry.Job);
			AssertEquals(ZString.Empty, chargeEntry.UniqueNumber);
			AssertEquals(ZString.Empty, chargeEntry.PreviousUniqueNumber);
			AssertEquals("PaymentType", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, usChargeEntry.PaymentType);
			AssertEquals("DueDate", new ZDateTime(2009, 6, 1).Date, usChargeEntry.DueDate);
			AssertEquals(false, usChargeEntry.IsPaidByImporter);

			statementHeader.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			AssertEquals(true, usChargeEntry.IsPaidByImporter);
		}

		public void TestICustomsChargeEntryCreditorPK()
		{
			OrgHeader creditor = Factory.New<OrgHeader>();
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			CusStatementHeader header = Factory.New<CusStatementHeader>();
			CusStatementLine statementLine = header.StatementLines.AddNew();
			ICustomsChargeEntry chargeEntry = statementLine;
			AssertEquals(creditor.PK, chargeEntry.CreditorPK);
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, statementLine.PK.ToGuid());
			AssertEquals(ZGuid.Empty, chargeEntry.CreditorPK);
		}

		public void TestICustomsChargeEntryHasBeenWithdrawn()
		{
			statementLine.B3_EntryNum = "12345678";
			statementLine.B3_EntryFilerCode = "XJ5";

			IUSCustomsChargeEntry usChargeEntry = statementLine;
			ICustomsChargeEntry chargeEntry = statementLine;

			AssertEquals(false, chargeEntry.HasBeenWithdrawn);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			entry.EntryNumber = "12345678";
			Factory.Save();

			AssertNotNull("PreCondition", statementLine.Declaration);

			AssertEquals(false, statementLine.HasEntryBeenWithdrawn);
			AssertEquals("HasBeenWithdrawn", false, chargeEntry.HasBeenWithdrawn);

			entry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			AssertEquals(true, statementLine.HasEntryBeenWithdrawn);
			AssertEquals("PreCondition", true, statementLine.Declaration.HasEntryBeenWithdrawn);
			AssertEquals("HasBeenWithdrawn", true, chargeEntry.HasBeenWithdrawn);
		}

		public void TestIsElectronicInvoiceRequired()
		{
			statementLine.B3_EIIndicator = YesNoDefaultList.Codes.No;
			AssertEquals("Electronic Invoice not required", false, statementLine.IsElectronicInvoiceRequired);

			statementLine.B3_EIIndicator = YesNoDefaultList.Codes.Yes;
			AssertEquals("Electronic Invoice required", true, statementLine.IsElectronicInvoiceRequired);
		}

		public void TestDeclarationProvider()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_EntryFilerCode = "XJ5";
			reconDeclaration.ReconEntry.GetEntry().EntryNumber = "1234";
			Factory.Save();
			AssertEquals("PreCondition", "1234", reconDeclaration.ReconEntryNumber);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryFilerCode = "XJ5";
			line.B3_EntryNum = "1234";

			AssertEquals(reconDeclaration, line.Declaration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			statementHeader = Factory.New<CusStatementHeader>();
			statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
		}

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;

		public void TestIStatementDeleteAndAddEntity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_IsInvoiceByRequest = true;
			declaration.US_EntryFilerCode = "YXY";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "09784511";
			Factory.Save();

			statementLine.B3_EntryFilerCode = "YXY";
			statementLine.B3_EntryNum = "09784511";
			statementLine.B3_B2 = statementHeader.PK;
			statementLine.B3_EntryProcessPort = "8887";
			statementLine.StatementHeader.B2_ProcessPort = "8888";
			statementLine.B3_Status = Enterprise.Customs.US.Business.StatementLineStatusList.Codes.Active;

			AssertNotNull("PreCondition", statementLine.Declaration);

			IStatementDeleteTransaction iEntity = statementLine;

			AssertEquals("EntryNumber", "09784511", iEntity.EntryNumber);
			AssertEquals("EntryNumber", "YXY-0978451-1", statementLine.FormattedEntryNumber);
			AssertEquals("EntryFilerCode", "YXY", iEntity.EntryFilerCode);
			AssertEquals("ProcessingPort", "8888", iEntity.ProcessingPort);
			AssertEquals("PortOfEntry", "8887", iEntity.PortOfEntry);
			AssertEquals("Factory", statementLine.Factory, iEntity.Factory);
			AssertEquals("Branch", entry.Declaration.Branch, iEntity.Branch);
			AssertEquals("Branch", Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), iEntity.Branch);
			AssertEquals("Release Date", ZDateTime.Empty, iEntity.ReleaseDate);
			AssertEquals("ShouldGenerateACEStatementMessage", true, iEntity.ShouldGenerateACEStatementMessage);
			AssertEquals("IsStatementUpdateMessagePending", false, iEntity.IsStatementUpdateMessagePending);

			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = "3901";
			declaration.US_PreparerOfficeCode = "22";
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.StatementUpdate;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_MessageNum = "HYEDUSCMT_000001";
			declaration.Messages.Add(message);

			AssertEquals("3901", iEntity.PreparerPort);
			AssertEquals("22", iEntity.PreparerOfficeCode);
			AssertEquals(true, iEntity.ShouldPopulatePreparerSite);
			AssertEquals("ShouldGenerateACEStatementMessage", true, iEntity.ShouldGenerateACEStatementMessage);
			AssertEquals("IsStatementUpdateMessagePending", true, iEntity.IsStatementUpdateMessagePending);
		}

		public void TestFetchHints()
		{
			statementLine.B3_EntryNum = "12345678";
			statementLine.B3_EntryFilerCode = "XJ5";

			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, 11.2m);
			statementLine.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Duty, 15m);

			var statementLine2 = statementHeader.StatementLines.AddNew();
			statementLine2.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine2.B3_EntryNum = "70000452";
			statementLine2.B3_EntryFilerCode = "XJ5";
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.DairyFee, 21.50m);
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Duty, 15m);
			statementLine2.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 25m);

			var statementLine3 = statementHeader.StatementLines.AddNew();
			statementLine3.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine3.B3_EntryNum = "80000409";
			statementLine3.B3_EntryFilerCode = "SV9";
			statementLine3.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, 11.2m);
			statementLine3.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.Duty, 15m);
			statementLine3.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, 12m);
			statementLine3.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, 20.5m);
			statementLine3.Charges.UpdateLineChargeFor(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, 25m);

			var strategy = statementLine3.FetchStrategy;
			var fetchCount = Factory.ActiveTableFetchHints;
			strategy.FetchForLoadChildEditableObjects();
			AssertEquals("hints should be added", fetchCount + 1, Factory.ActiveTableFetchHints);
		}

		public void TestMatchCustomsChargesToClear()
		{
			var provider = statementLine as IAccInvoiceDataProvider;

			CombineAssertions(() =>
			{
				AssertEquals("Empty", true, provider.MatchCustomsChargesToClear("", ""));
				AssertEquals("InvoiceNum", true, provider.MatchCustomsChargesToClear("INV987654321", ""));
				AssertEquals("Description", true, provider.MatchCustomsChargesToClear("", "Some random description"));
				AssertEquals("Both filled", true, provider.MatchCustomsChargesToClear("INV987654321", "Another descriptoin"));
			});
		}

		public void TestIsAutoBillingDueDateFromPaymentTerms()
		{
			CustomsDataRegistry.Instance.AutoBillingDueDateFromPaymentTerms.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var provider = statementLine as IAccInvoiceDataProvider;
			AssertEquals("The provider's IsAutoBillingDueDateFromPaymentTerms should reflect the explicitly set configuration value.", true, provider.IsAutoBillingDueDateFromPaymentTerms);
		}
	}
}
