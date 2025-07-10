using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclarationAccountingIntegrationTest : TestCaseWithFactory
	{
		public void TestCargoReleaseEntriesAreExcludedForIntegration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			Factory.Save();
			AssertEquals("PreCondition", 2, declaration.ActiveEntryHeaders.Count);

			var dataProvider = (IAccIntegrationDataProvider)new JobDeclarationIAccIntegrationDataProvider(declaration);
			AssertEquals("Only formal entries", 1, dataProvider.InvDataProviders.Length);
		}

		public void TestWhenAutoRateAndReviewOnSendingIsSet()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_OH_Importer = testHelper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();//Need to save first

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();//should have autorated

			var job = new JobHeader.Loader(declaration).Load();
			AssertNotNull("PreCondition", job);

			var charges = job.Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals(1, charges.Length);
			AssertEquals("PreCondition:Not posted", false, charges[0].IsCostPosted);
			AssertEquals("PreCondition:Not posted", false, charges[0].IsRevenuePosted);

			option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			option.PreApprovalBillingJob = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			declaration.US_JobReadyForPost = false;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryReplace;
			Factory.Save();//should not post anything as it is not ready

			var factory2 = new BusinessObjectFactory();
			charges = factory2.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals(1, charges.Length);
			AssertEquals("Should NOT have been posted", false, charges[0].IsCostPosted);
			AssertEquals("Should NOT have been posted", false, charges[0].IsRevenuePosted);

			declaration.US_JobReadyForPost = true;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace;
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryReplace;
			Factory.Save();//now job is ready for posting

			AssertEquals("Should be set back to no after integration is successful", false, declaration.US_JobReadyForPost);

			factory2 = new BusinessObjectFactory();
			charges = factory2.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals(1, charges.Length);
			AssertEquals("Should have been posted", true, charges[0].IsCostPosted);
			AssertEquals("Should have been posted", true, charges[0].IsRevenuePosted);
			AssertEquals("Correct Revenue Recognition Type should have been set", RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, charges[0].ARLine.AL_RevRecognitionType);
			AssertEquals("Correct ReverseDate should have been set", charges[0].ARLine.AL_PostDate, charges[0].ARLine.AL_ReverseDate);
		}

		public void TestAutoRateDuplicateAPException_CS00109632()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = Core.Constants.Groups.PostMastersGroupPK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
			Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK).Staff[0].GS_EmailAddress = "test@cargowise.com";

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_OH_Importer = testHelper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader crl = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			CusEntryHeader entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();//Need to save first

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			entry2.CH_TotalPaid = 10m;
			crl.CH_TotalPaid = 11m;
			Factory.Save();//should have autorated

			var job = new JobHeader.Loader(declaration).Load();
			AssertNotNull("PreCondition", job);

			var charges = job.Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
			AssertEquals(1, charges.Length);
			AssertEquals(true, charges[0].IsCostPosted);
			AssertEquals(false, charges[0].IsRevenuePosted);
		}

		public void TestTriggerAccountingIntegrationPCS()
		{
			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();
			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			option.PreApprovalBillingJob = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			var declaration = new DeclarationTestHelper().GetMergedDutiableDeclarationForACE(Factory);
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_OH_Importer = testHelper.Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_PSC = true;
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			var job = new JobHeader.Loader(declaration).Load();
			AssertNull("If PSC is ticked, Job is not created", job);

			declaration.US_PSC = false;
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			job = new JobHeader.Loader(declaration).Load();
			AssertNotNull("If PSC is not ticked, Job is created", job);
		}

		public void TestUS_DRWRejectedMerchandiseReasonForACEDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			AssertEquals(string.Empty, declaration.US_DRWRejectedMerchandiseReason);

			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._03;
			AssertEquals(DrawbackRejectedMerchandiseReasonList.Codes.NCS, declaration.US_DRWRejectedMerchandiseReason);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._04;
			AssertEquals(DrawbackRejectedMerchandiseReasonList.Codes.SWC, declaration.US_DRWRejectedMerchandiseReason);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._05;
			AssertEquals(DrawbackRejectedMerchandiseReasonList.Codes.DTI, declaration.US_DRWRejectedMerchandiseReason);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._06;
			AssertEquals(string.Empty, declaration.US_DRWRejectedMerchandiseReason);

			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._17;
			AssertEquals(DrawbackRejectedMerchandiseReasonList.Codes.NCS, declaration.US_DRWRejectedMerchandiseReason);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._18;
			AssertEquals(DrawbackRejectedMerchandiseReasonList.Codes.SWC, declaration.US_DRWRejectedMerchandiseReason);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._19;
			AssertEquals(DrawbackRejectedMerchandiseReasonList.Codes.DTI, declaration.US_DRWRejectedMerchandiseReason);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._20;
			AssertEquals(string.Empty, declaration.US_DRWRejectedMerchandiseReason);
		}

		public void TestUS_SPNIDType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableSPN = true;
			AssertEquals(string.Empty, declaration.US_SPNIDType);
			AssertEquals(false, declaration.IsACEBLNStandAlonePriorNotice);
			AssertEquals(false, declaration.IsACEENTStandAlonePriorNotice);

			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			AssertEquals(true, declaration.IsACEBLNStandAlonePriorNotice);
			AssertEquals(false, declaration.IsACEENTStandAlonePriorNotice);

			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			AssertEquals(false, declaration.IsACEBLNStandAlonePriorNotice);
			AssertEquals(true, declaration.IsACEENTStandAlonePriorNotice);

			declaration.US_EnableSPN = false;
			AssertEquals(string.Empty, declaration.US_SPNIDType);
			AssertEquals(false, declaration.IsACEBLNStandAlonePriorNotice);
			AssertEquals(false, declaration.IsACEENTStandAlonePriorNotice);

			declaration.US_EnableSPN = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals(false, declaration.US_EnableSPN);
			AssertEquals(string.Empty, declaration.US_SPNIDType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.O;
			AssertEquals(false, declaration.IsFTZFTZStandAlonePriorNotice);
			AssertEquals(false, declaration.IsFTZBLNStandAlonePriorNotice);

			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			AssertEquals(false, declaration.IsFTZFTZStandAlonePriorNotice);
			AssertEquals(false, declaration.IsFTZBLNStandAlonePriorNotice);

			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.FTZ;
			AssertEquals(true, declaration.IsFTZFTZStandAlonePriorNotice);
			AssertEquals(false, declaration.IsFTZBLNStandAlonePriorNotice);

			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			AssertEquals(false, declaration.IsFTZFTZStandAlonePriorNotice);
			AssertEquals(true, declaration.IsFTZBLNStandAlonePriorNotice);
		}

		public void TestIsACSCargoCertificationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			AssertEquals(CargoReleaseTypeList.Codes.ACE, declaration.US_CargoReleaseType);
			AssertEquals(false, declaration.IsACSCargoCertificationMode);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertEquals(true, declaration.IsACSCargoCertificationMode);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals(true, declaration.IsACSCargoCertificationMode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
