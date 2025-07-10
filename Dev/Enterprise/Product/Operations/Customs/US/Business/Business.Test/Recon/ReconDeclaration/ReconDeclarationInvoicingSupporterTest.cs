using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconDeclarationInvoicingSupporter))]
	sealed class ReconDeclarationInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestIJobInvoicingPlugIn_DefaultChargeGroup()
		{
			IJobInvoicingPlugIn reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, reconDec.InvoicingSupporter.DefaultChargeGroup);
		}

		public void TestIJobInvoicingPlugIn_OverriddenDepartmentPK()
		{
			IJobInvoicingPlugIn reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			IJobInvoicingPlugIn jobInvoicingPlugIn = reconDec;
			AssertEquals(ObjectFactory.Get<IAccounting>().CustomsOther, jobInvoicingPlugIn.InvoicingSupporter.OverriddenDepartmentPK);
		}

		public void TestTemplateReconDeclarationCopyCore()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_Comment = "TestComment";
			var result = (JobDeclaration)reconDec.TemplateReconDeclarationCopyCore(CloneType.TemplateCopy);
			AssertNotNull(result);
			AssertEquals(ZString.Empty, result.US_Comment);
		}

		public void TestGetOperationsSignificantDate()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var jobInvoicingPlugIn = (IJobInvoicingPlugIn)reconDec;

			AssertEquals(ZDateTime.Empty, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals(ZDateTime.Empty, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDateByDirection(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, ""));

			var log = reconDec.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(2008, 9, 11));

			AssertEquals(log.SL_EventTime, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals(log.SL_EventTime, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDateByDirection(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, ""));

			log = reconDec.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(2009, 9, 11));

			AssertEquals(log.SL_EventTime, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals(log.SL_EventTime, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDateByDirection(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, ""));
		}

		public void TestConsignee()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			IJobInvoicingPlugIn jobInvoicingPlugIn = reconDec;

			reconDec.JE_OH_Importer = Factory.New<OrgHeader>().PK;

			AssertEquals(reconDec.Importer, jobInvoicingPlugIn.InvoicingSupporter.Consignee);
		}

		public void TestCreateAccountingJobOnSavingOfOperationsJob()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "IMP@!12";
			org.OH_FullName = "IMPORTERFULLNAME";
			var dec = Factory.New<JobDeclaration>();
			dec.SuspendAddingWorkflow = true;
			dec.JE_OH_Importer = org.PK;
			dec.JE_OH_Supplier = org.PK;
			var reconDec = new ReconDeclaration(dec);
			var jobInvoicingPlugIn = (IJobInvoicingPlugIn)reconDec;
			AssertEquals(true, jobInvoicingPlugIn.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
			Factory.Save();
			var addressPK = org.MainAddress.PK;
			dec.DocAddresses.OfType<JobDocAddress>().Where(x => !x.IsInDatabase).ForEach(x => x.E2_OA_Address = addressPK);
			Factory.Save();
			AssertEquals(false, jobInvoicingPlugIn.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		public void TestDefaultNotifyParty()
		{
			var dec = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(dec);
			var ior = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(ior);
			var party4811 = Factory.New<OrgHeader>();
			wrapper.ZO_OH_NP = party4811.PK;
			reconDec.IOROrgPK = ior.PK;
			AssertEquals(party4811.PK, reconDec.JE_OH_NotifyParty);
		}

		public void TestPriorDisclosureIndicator()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec1.US_EnableENS = true;
			dec1.US_PriorDisclosure = true;
			dec1.Invoices.AddNew();
			dec1.InvoiceLines.AddNew();
			dec1.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			DeclarationTestHelper.SetEntryFilerCode("XJ6");
			JobDeclaration dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec2.US_EnableENS = true;
			dec2.US_PriorDisclosure = false;
			dec2.Invoices.AddNew();
			dec2.InvoiceLines.AddNew();
			dec2.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			CusEntryHeader ensEntry1 = dec1.CustomsEntryHeaders[0];
			CusEntryHeader ensEntry2 = dec2.CustomsEntryHeaders[0];

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			ReconOriginalEntryHeader reconOriginalEntry1 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry1.CH_OrigEntryReference = "XJ5" + ensEntry1.EntryNumber;
			Assert(reconOriginalEntry1.US_PriorDisclosure);

			ReconOriginalEntryHeader reconOriginalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry2.CH_OrigEntryReference = "XJ6" + ensEntry2.EntryNumber;
			Assert(!reconOriginalEntry2.US_PriorDisclosure);

			Assert(reconDeclaration.PriorDisclosureIndicator);

			DeclarationTestHelper.SetEntryFilerCode("XJ3");
			JobDeclaration dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec3.US_EnableENS = true;
			dec3.US_PriorDisclosure = false;
			dec3.Invoices.AddNew();
			dec3.InvoiceLines.AddNew();
			dec3.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			DeclarationTestHelper.SetEntryFilerCode("XJ4");
			JobDeclaration dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec4.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec4.US_EnableENS = true;
			dec4.US_PriorDisclosure = false;
			dec4.Invoices.AddNew();
			dec4.InvoiceLines.AddNew();
			dec4.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			CusEntryHeader ensEntry3 = dec3.CustomsEntryHeaders[0];
			CusEntryHeader ensEntry4 = dec4.CustomsEntryHeaders[0];

			ReconDeclaration reconDeclaration2 = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			ReconOriginalEntryHeader reconOriginalEntry3 = reconDeclaration2.OriginalEntries.AddNew();
			reconOriginalEntry3.CH_OrigEntryReference = "XJ3" + ensEntry3.EntryNumber;
			Assert(!reconOriginalEntry3.US_PriorDisclosure);

			ReconOriginalEntryHeader reconOriginalEntry4 = reconDeclaration2.OriginalEntries.AddNew();
			reconOriginalEntry4.CH_OrigEntryReference = "XJ4" + ensEntry4.EntryNumber;
			Assert(!reconOriginalEntry4.US_PriorDisclosure);

			Assert(!reconDeclaration2.PriorDisclosureIndicator);
		}

		public void TestNAFTA303ClaimStatement()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec1.US_EnableENS = true;
			dec1.US_NAFTAClaimStat = true;
			dec1.Invoices.AddNew();
			dec1.InvoiceLines.AddNew();
			dec1.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			DeclarationTestHelper.SetEntryFilerCode("XJ6");
			JobDeclaration dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec2.US_EnableENS = true;
			dec2.US_NAFTAClaimStat = false;
			dec2.Invoices.AddNew();
			dec2.InvoiceLines.AddNew();
			dec2.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			CusEntryHeader ensEntry1 = dec1.CustomsEntryHeaders[0];
			CusEntryHeader ensEntry2 = dec2.CustomsEntryHeaders[0];

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			ReconOriginalEntryHeader reconOriginalEntry1 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry1.CH_OrigEntryReference = "XJ5" + ensEntry1.EntryNumber;
			Assert(reconOriginalEntry1.US_NAFTAClaimStat);

			ReconOriginalEntryHeader reconOriginalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry2.CH_OrigEntryReference = "XJ6" + ensEntry2.EntryNumber;
			Assert(!reconOriginalEntry2.US_NAFTAClaimStat);

			Assert(reconDeclaration.NAFTA303ClaimStatement);

			DeclarationTestHelper.SetEntryFilerCode("XJ3");
			JobDeclaration dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec3.US_EnableENS = true;
			dec3.US_NAFTAClaimStat = false;
			dec3.Invoices.AddNew();
			dec3.InvoiceLines.AddNew();
			dec3.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			DeclarationTestHelper.SetEntryFilerCode("XJ4");
			JobDeclaration dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec4.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec4.US_EnableENS = true;
			dec4.US_NAFTAClaimStat = false;
			dec4.Invoices.AddNew();
			dec4.InvoiceLines.AddNew();
			dec4.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			CusEntryHeader ensEntry3 = dec3.CustomsEntryHeaders[0];
			CusEntryHeader ensEntry4 = dec4.CustomsEntryHeaders[0];

			ReconDeclaration reconDeclaration2 = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			ReconOriginalEntryHeader reconOriginalEntry3 = reconDeclaration2.OriginalEntries.AddNew();
			reconOriginalEntry3.CH_OrigEntryReference = "XJ3" + ensEntry3.EntryNumber;
			Assert(!reconOriginalEntry3.US_NAFTAClaimStat);

			ReconOriginalEntryHeader reconOriginalEntry4 = reconDeclaration2.OriginalEntries.AddNew();
			reconOriginalEntry4.CH_OrigEntryReference = "XJ4" + ensEntry4.EntryNumber;
			Assert(!reconOriginalEntry4.US_NAFTAClaimStat);

			Assert(!reconDeclaration2.NAFTA303ClaimStatement);
		}

		public void TestProtestOrPetitionFiledStatement()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec1.US_EnableENS = true;
			dec1.US_ProtestStat = true;
			dec1.Invoices.AddNew();
			dec1.InvoiceLines.AddNew();
			dec1.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			DeclarationTestHelper.SetEntryFilerCode("XJ6");
			JobDeclaration dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec2.US_EnableENS = true;
			dec2.US_ProtestStat = false;
			dec2.Invoices.AddNew();
			dec2.InvoiceLines.AddNew();
			dec2.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			CusEntryHeader ensEntry1 = dec1.CustomsEntryHeaders[0];
			CusEntryHeader ensEntry2 = dec2.CustomsEntryHeaders[0];

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			ReconOriginalEntryHeader reconOriginalEntry1 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry1.CH_OrigEntryReference = "XJ5" + ensEntry1.EntryNumber;
			Assert(reconOriginalEntry1.US_ProtestStat);

			ReconOriginalEntryHeader reconOriginalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			reconOriginalEntry2.CH_OrigEntryReference = "XJ6" + ensEntry2.EntryNumber;
			Assert(!reconOriginalEntry2.US_ProtestStat);

			Assert(reconDeclaration.ProtestOrPetitionFiledStatement);

			DeclarationTestHelper.SetEntryFilerCode("XJ3");
			JobDeclaration dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec3.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec3.US_EnableENS = true;
			dec3.US_ProtestStat = false;
			dec3.Invoices.AddNew();
			dec3.InvoiceLines.AddNew();
			dec3.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			DeclarationTestHelper.SetEntryFilerCode("XJ4");
			JobDeclaration dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec4.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec4.US_EnableENS = true;
			dec4.US_ProtestStat = false;
			dec4.Invoices.AddNew();
			dec4.InvoiceLines.AddNew();
			dec4.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			CusEntryHeader ensEntry3 = dec3.CustomsEntryHeaders[0];
			CusEntryHeader ensEntry4 = dec4.CustomsEntryHeaders[0];

			ReconDeclaration reconDeclaration2 = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			ReconOriginalEntryHeader reconOriginalEntry3 = reconDeclaration2.OriginalEntries.AddNew();
			reconOriginalEntry3.CH_OrigEntryReference = "XJ3" + ensEntry3.EntryNumber;
			Assert(!reconOriginalEntry3.US_ProtestStat);

			ReconOriginalEntryHeader reconOriginalEntry4 = reconDeclaration2.OriginalEntries.AddNew();
			reconOriginalEntry4.CH_OrigEntryReference = "XJ4" + ensEntry4.EntryNumber;
			Assert(!reconOriginalEntry4.US_ProtestStat);

			Assert(!reconDeclaration2.ProtestOrPetitionFiledStatement);
		}

		public void TestDocRecipientRegistrationNumberResultIsDefaulted()
		{
			var organisation = Factory.New<OrgHeader>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.SummaryDocRecipientAddress.OrganisationPK = organisation.PK;
			reconDeclaration.SummaryDocRecipientAddress.E2_AddressOverride = true;
			AssertEquals("", reconDeclaration.SummaryDocRecipientAddress.E2_GovRegNum);

			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-123456700");

			reconDeclaration.SummaryDocRecipientAddress.E2_AddressOverride = false;
			reconDeclaration.SummaryDocRecipientAddress.E2_AddressOverride = true;
			AssertEquals("12-123456700", reconDeclaration.SummaryDocRecipientAddress.E2_GovRegNum);
			reconDeclaration.SummaryDocRecipientAddress.E2_AddressOverride = false;
			AssertEquals("12-123456700", reconDeclaration.SummaryDocRecipientAddress.E2_GovRegNum);
		}

		public void TestClaimantRegistrationNumberResultIsDefaulted()
		{
			var organisation = Factory.New<OrgHeader>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.ClaimantAddress.OrganisationPK = organisation.PK;
			reconDeclaration.ClaimantAddress.E2_AddressOverride = true;
			AssertEquals("", reconDeclaration.ClaimantAddress.E2_GovRegNum);

			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "654-73-1975");

			reconDeclaration.ClaimantAddress.E2_AddressOverride = false;
			reconDeclaration.ClaimantAddress.E2_AddressOverride = true;
			AssertEquals("654-73-1975", reconDeclaration.ClaimantAddress.E2_GovRegNum);
			reconDeclaration.ClaimantAddress.E2_AddressOverride = false;
			AssertEquals("654-73-1975", reconDeclaration.ClaimantAddress.E2_GovRegNum);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var reconDeclaration = new ReconDeclaration(jobDeclaration);
			return reconDeclaration;
		}

		protected override ZString TestingCountry => Core.Constants.CountryCodes.UnitedStates;
	}
}
