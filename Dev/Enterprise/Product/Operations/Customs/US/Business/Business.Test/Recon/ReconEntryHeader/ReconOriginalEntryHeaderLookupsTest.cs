using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconOriginalEntryHeaderLookupsTest : TestCaseWithFactory
	{
		public void TestEntries()
		{
			OrgHeader importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "Z1Z2Z3Z4";
			OrgHeader importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "Z4Z3Z2Z1";
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.IOROrgPK = importer1.PK;
			reconDec.US_SuretyCode = "891";
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			reconDec.US_ImportEntrySource = ReconciliationImportEntrySourceList.Codes.PuertoRico;
			ReconOriginalEntryHeader reconOriginalEntry = reconDec.OriginalEntries.AddNew();
			reconOriginalEntry.CH_OrigEntryReference = "XJ500000020";
			ModuleEntryHeaderCollection entries = reconOriginalEntry.Lookups.Entries;
			AssertEquals("00000020", entries.FilterBusinessObjectDefaults["Entry Number (ENS):Property"].Value);
			AssertEquals(ReconIssueCodeList.Codes.ValueRecon, entries.FilterBusinessObjectDefaults["Recon Issue:Property"].Value);
			AssertEquals("891", entries.FilterBusinessObjectDefaults["Surety Code:Property"].Value);
			AssertEquals(reconDec.IOROrgPK, entries.FilterBusinessObjectDefaults["Importer of Record:Property"].Value);
			AssertEquals(true, entries.FilterBusinessObjectDefaults["Exclude IOR Filing Their Own Recon:Property0"].Value);
			AssertEquals(true, entries.FilterBusinessObjectDefaults["Reconciliation:Property0"].Value);
			AssertEquals(ReconciliationImportEntrySourceList.Codes.PuertoRico, entries.FilterBusinessObjectDefaults["Import Source:Property"].Value);
			JobDeclaration dec1 = Factory.New<JobDeclaration>();
			dec1.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			dec1.US_NAFTAReconIndicator = ZBool.True;
			dec1.US_SuretyCode = "891";
			dec1.IOROrgPK = importer1.PK;
			dec1.US_FileTheirOwnRecon = ZBool.False;
			CusEntryHeader entrySummary = dec1.CustomsEntryHeaders.AddNew();
			entrySummary.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			dec1.US_SchDEntry = "4963";
			Factory.Save();
			AssertEquals(true, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			dec1.US_OtherReconIndicator = ReconIssueCodeList.Codes.ValueClassRecon;
			Factory.Save();
			AssertEquals(false, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.ValueClassRecon;
			Factory.Save();
			AssertEquals(true, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			dec1.US_SuretyCode = "695";
			Factory.Save();
			AssertEquals(false, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			reconDec.US_SuretyCode = "695";
			Factory.Save();
			AssertEquals(true, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			dec1.IOROrgPK = importer2.PK;
			Factory.Save();
			AssertEquals(false, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			reconDec.IOROrgPK = importer2.PK;
			Factory.Save();
			AssertEquals("importer2 new org record now defaults with NA issue code on creation", false, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			var countryData = importer2.CountryData;
			var wrapper = OrgHeaderWrapper.New(importer2);
			AssertEquals("Pre-condition: ZO_OtherReconIndicator by default on creation of new Organisation should be Not Applicable", ReconIssueCodeList.Codes.NotApplicable, wrapper.ZO_OtherReconIndicator);
			wrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueClassRecon;
			reconDec.IOROrgPK = ZGuid.Empty;
			reconDec.IOROrgPK = importer2.PK;
			Factory.Save();
			AssertEquals("importer2 updated with VC issue code", true, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			dec1.US_FileTheirOwnRecon = ZBool.True;
			Factory.Save();
			AssertEquals(false, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			dec1.US_FileTheirOwnRecon = ZBool.False;
			Factory.Save();
			AssertEquals(true, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			reconDec.US_IssueCode = ZString.Empty;
			dec1.US_OtherReconIndicator = ZString.Empty;
			dec1.US_NAFTAReconIndicator = ZBool.False;
			Factory.Save();
			AssertEquals(false, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			dec1.US_NAFTAReconIndicator = ZBool.True;
			Factory.Save();
			AssertEquals(true, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			dec1.US_SchDEntry = "5123";
			Factory.Save();
			AssertEquals(false, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			reconDec.US_ImportEntrySource = ReconciliationImportEntrySourceList.Codes.VirginIslands;
			Factory.Save();
			AssertEquals(true, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			dec1.US_SchDEntry = "2908";
			Factory.Save();
			AssertEquals(false, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			reconDec.US_ImportEntrySource = ReconciliationImportEntrySourceList.Codes.FiftyStates;
			Factory.Save();
			AssertEquals(true, entrySummary.MatchesFilter(reconOriginalEntry.Lookups.Entries.AdditionalFilter));
			reconOriginalEntry.CH_OrigEntryReference = "XJ5";
			Factory.Save();
			AssertEquals(false, reconOriginalEntry.Lookups.Entries.FilterBusinessObjectDefaults.ContainsDefaultFor("Entry #:Property"));
		}

		public void TestEntryTypeList()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader reconOriginalEntry = reconDec.OriginalEntries.AddNew();
			CodeDescriptionPairList entryTypeList = reconOriginalEntry.Lookups.EntryTypeList;
			AssertEquals("InBond related types are removed", false, entryTypeList.ContainsCode(EntryTypeList.Codes.TransportationExportation));
			AssertEquals("Ex-Warehousing related types are removed", false, entryTypeList.ContainsCode(EntryTypeList.Codes.WarehouseWithdrawalADDCVD));
			AssertEquals("Drawback related types are removed", false, entryTypeList.ContainsCode(EntryTypeList.Codes.SubstitutionUnusedMerchandiseDrawback));
			AssertEquals("Sorted", EntryTypeList.Codes.ConsumptionFreeDutiable, entryTypeList[0].Code);
		}

		public void TestYesNoList()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader reconOriginalEntry = reconDec.OriginalEntries.AddNew();
			AssertEquals(typeof(YesNoDefaultList), reconOriginalEntry.Lookups.YesNoList.GetType());
			Assert("Should contain No", reconOriginalEntry.Lookups.YesNoList.ContainsCode(YesNoDefaultList.Codes.No));
			Assert("Should contain Yes", reconOriginalEntry.Lookups.YesNoList.ContainsCode(YesNoDefaultList.Codes.Yes));
			Assert("Should not contain Default", !reconOriginalEntry.Lookups.YesNoList.ContainsCode(YesNoDefaultList.Codes.Default));
		}

		public void TestPendingActionTypeList()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader reconOriginalEntry = reconDec.OriginalEntries.AddNew();
			AssertEquals(typeof(PendingActionIDTypeList), reconOriginalEntry.Lookups.PendingActionTypeList.GetType());
			Assert("Should contain A", reconOriginalEntry.Lookups.PendingActionTypeList.ContainsCode(PendingActionIDTypeList.Codes.A));
			Assert("Should contain C", reconOriginalEntry.Lookups.PendingActionTypeList.ContainsCode(PendingActionIDTypeList.Codes.C));
			Assert("Should contain P", reconOriginalEntry.Lookups.PendingActionTypeList.ContainsCode(PendingActionIDTypeList.Codes.P));
		}
	}
}
