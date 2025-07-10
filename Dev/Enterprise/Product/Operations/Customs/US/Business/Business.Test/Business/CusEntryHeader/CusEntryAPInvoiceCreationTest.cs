using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class APInvoiceCreationTest : TestCaseWithFactory
	{
		public void TestIssue00211941AutoBillingForTruckDeclarationInRoadShipment()
		{
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Freight.Forwarding.Business.ForwardingShipment shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = testHelper.Importer.MainAddress.PK;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_OH_Importer = testHelper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_JS = shipment.PK;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertNoExceptionThrown(delegate
			{
				Factory.Save();
			});
		}

		public void TestClearBillHold()
		{
			var bill = declaration.Bills.AddNew();
			bill.CU_MessageStatus = "51";
			CusEntryHeader entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entryHeader.EntryNumber = "B0320";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Assert(bill.CU_MessageStatus.IsEmpty);
		}

		public void TestAccountingIntegrationForS_Jobs()
		{
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "B0320";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			entryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 100m);
			entryHeader.MergedLines.AddNew();
			Factory.Save(); // entries created
							//when census warning override message is processed
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save(); // entry cleared
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			var newFactory = new BusinessObjectFactory();
			var job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			AssertNotNull("Auto rated and attached to the shipment", job);
			var charge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertNotNull("Should have one charge row in the job", charge);
			AssertEquals("charge.JR_LocalCostAmt", 100m, charge.JR_LocalCostAmt);
			AssertEquals("Charge Code is correctly set", testHelper.DisbursementChargeCode.PK, charge.JR_AC);
			AssertEquals("Should not have posted though", false, charge.IsCostPosted);
			var transaction = newFactory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, entryHeader.EntryNumber).AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
			AssertNull("In registry, US opt not to post AP invoice", transaction);
		}

		public void TestCensusWarningOverride()
		{
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "B0320";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			entryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 100m);
			entryHeader.MergedLines.AddNew();
			Factory.Save(); // entries created
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			//when census warning override message is processed
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save(); // entry cleared
			var newFactory = new BusinessObjectFactory();
			var job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull("Auto rated", job);
			var charge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertNotNull("Should have one charge row in the job", charge);
			AssertEquals("charge.JR_LocalCostAmt", 100m, charge.JR_LocalCostAmt);
			AssertEquals("Charge Code is correctly set", testHelper.DisbursementChargeCode.PK, charge.JR_AC);
			AssertEquals("Should not have posted though", false, charge.IsCostPosted);
			var transaction = newFactory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, entryHeader.EntryNumber).AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC));
			AssertNull("In registry, US opt not to post AP invoice", transaction);
		}

		public void TestWIPsAreDeletedWhenEntryIsWithdrawn()
		{
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = false;
			option.ARPostDSB = false;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			CusEntryHeader entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "B0320";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			entryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 100m);
			entryHeader.MergedLines.AddNew();
			Factory.Save();
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save(); // message cleared
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobHeader job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull("Auto rated", job);
			JobCharge charge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertNotNull("Should have one charge row in the job", charge);
			AssertEquals("charge.JR_LocalCostAmt", 100m, charge.JR_LocalCostAmt);
			AssertEquals("Charge Code is correctly set", testHelper.DisbursementChargeCode.PK, charge.JR_AC);
			AssertEquals("Should not have posted though", false, charge.IsCostPosted && charge.IsRevenuePosted);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete;
			Factory.Save(); // entry withdrawn
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			Factory.Save(); // entry withdrawn
			newFactory = new BusinessObjectFactory();
			charge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals("Unposted WIP for disbursement", 0m, charge.JR_LocalCostAmt);
		}

		public void TestSendEmailWhenChargesPostedForWithdrawnEntries()
		{
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			GlbGroup dummyEmailGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "teststaff@abc.com";
			dummyEmailGroup.Staff.Add(currentStaff);
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = false;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			CusEntryHeader entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "B0320";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			entryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 100m);
			entryHeader.MergedLines.AddNew();
			Factory.Save();
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save(); // message cleared
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobHeader job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull("Auto rated", job);
			JobCharge charge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertNotNull("Should have one charge row in the job", charge);
			AssertEquals("charge.JR_LocalCostAmt", 100m, charge.JR_LocalCostAmt);
			AssertEquals("Charge Code is correctly set", testHelper.DisbursementChargeCode.PK, charge.JR_AC);
			AssertEquals("Should have posted Cost", true, charge.IsCostPosted);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete;
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			Factory.Save(); // entry withdrawn
			newFactory = new BusinessObjectFactory();
			charge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertNotNull("Posted disbursement charge should not has been deleted", charge);
			Assert(charge.IsCostPosted);
			AssertEquals("One email is created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestInvoicesNotPostedWhenClearedWithCensusWarning()
		{
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			CusEntryHeader entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "B0320";
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			entryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Duty, 100m);
			entryHeader.MergedLines.AddNew();
			Factory.Save(); // entries created
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			Factory.Save(); // census warning
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobHeader job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNull("AutoBilling not attempted", job);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
			Factory.Save();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings;
			Factory.Save(); //census warning again
			job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNull("AutoBilling not attempted", job);
			USCustomsDataRegistry.Instance.SuppressAutoBillingDisbursementWhenCensusWarningExists.SetValue(declaration.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			entryHeader.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
			Factory.Save();
			entryHeader.CH_Status = ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings;
			Factory.Save(); //census warning again
			job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull("AutoBilling is attempted", job);
			var jobCharge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertNotNull("Contain charge code", jobCharge);
		}

		public void TestAccountingIntegrationForWarehouseEntries()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6302.21.9020";
			invoiceLine.JI_LinePrice = 5484.82m;
			invoiceLine.JI_CustomsQuantity = 2140m;
			invoiceLine.JI_CustomsSecondQuantity = 578m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save(); // entries created
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			declaration.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save(); // entry cleared
			AssertEquals("PreCondition", 6.86m, declaration.CustomsEntryHeaders[0].HMFAmountForEntry);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobHeader job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull("Auto rated", job);
			var charges = newFactory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			var chargeToPay = charges.FirstOrDefault(x => x.JR_AC == testHelper.DisbursementChargeCode.PK);
			AssertNotNull("Should have one charge row in the job", chargeToPay);
			AssertEquals("charge.JR_LocalCostAmt should be only HMF for warehouse entries", 6.86m, chargeToPay.JR_LocalCostAmt);
			AssertEquals("Should have posted AP", true, chargeToPay.IsCostPosted);
		}

		[TestDate(2009, 1, 1)]
		public void TestAccountingIntegrationForDeferredTax()
		{
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2204215030";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_SPI = "AU"; // MPF is exempt
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax; // deferred
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertNotEquals("Tax is there", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalEstimatedTax);
			AssertEquals("But payable amount is only duty", 63m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
			AssertEquals(63m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_TotalPaid);
			declaration.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save(); // entries created
			declaration.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save(); // entry cleared
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobHeader job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull("Auto rated", job);
			JobCharge charge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertNotNull("Should have one charge row in the job", charge);
			AssertEquals("charge.JR_LocalCostAmt should be only duty excluding deferred tax", 63m, charge.JR_LocalCostAmt);
			AssertEquals("Charge Code is correctly set", testHelper.DisbursementChargeCode.PK, charge.JR_AC);
			AssertEquals("Should have posted AP", true, charge.IsCostPosted);
		}

		public void TestWhenImporterIsToPayButInformationOnlyRegistryIsTurnedOn()
		{
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertEquals("PreCondition", false, PaymentTypeList.IsPaidByBroker(declaration.US_PaymentType));
			AssertEquals("PreCondition", false, declaration.IsPaidByBroker);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6302.21.9020";
			invoiceLine.JI_LinePrice = 5484.82m;
			invoiceLine.JI_CustomsQuantity = 2140m;
			invoiceLine.JI_CustomsSecondQuantity = 578m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save(); // entries created
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			declaration.CustomsEntryHeaders[0].CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save(); // entry cleared
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobHeader job = newFactory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK).AddToFilter(JobHeaderSchema.JH_GC, declaration.CompanyPK));
			AssertNotNull("Auto rated", job);
			JobCharge charge = newFactory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertNotNull("Should have one charge row in the job", charge);
			AssertEquals("Not paid by broker, but information only", 0m, charge.JR_LocalCostAmt);
			AssertEquals("Charge Code is correctly set", testHelper.DeferredChargeCode.PK, charge.JR_AC);
		}

		JobDeclaration declaration;
		Customs.Business.Testing.InvoicingTestHelper testHelper;
		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			AccountingIntegrationOptions option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = false;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();
			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_OH_Importer = testHelper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		}

		protected override void TearDown()
		{
			base.TearDown();
			ObjectFactory.Get<IAccounting>().SetWIPMustHaveDebtorCode(GlbCompany.CurrentCompany.PK.ToGuid(), false);
		}
	}
}
