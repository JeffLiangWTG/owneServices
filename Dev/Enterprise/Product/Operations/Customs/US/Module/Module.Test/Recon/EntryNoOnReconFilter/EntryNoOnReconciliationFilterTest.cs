using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(EntryNoOnReconciliationFilter))]
	sealed class EntryNoOnReconciliationFilterTest : ModuleTextFilterTest
	{
		public void TestEntryNoOnReconciliationFilter()
		{
			var reconDeclaration1 = GetReconDeclaration("B00001003", "00000048", "00000123");
			var reconDeclaration2 = GetReconDeclaration("B00001004", "00000051", "00000456");
			var reconDeclaration3 = GetReconDeclaration("B00001005", "00000055", "00000789");
			var declarationShouldBeReconciled = Factory.New<JobDeclaration>();
			declarationShouldBeReconciled.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationShouldBeReconciled.JE_DeclarationReference = "B00001006";
			var entryOnReconciliation = declarationShouldBeReconciled.ActiveEntryHeaders.AddNew();
			entryOnReconciliation.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			reconDeclaration3.ImportLines(new JobDeclaration[] { declarationShouldBeReconciled });
			var reconOriginalEntry = reconDeclaration3.OriginalEntries[1];
			reconOriginalEntry.CH_OrigEntryReference = ZString.Empty;
			Factory.Save();
			var filter = new ReconFilterStripBusinessObject();
			var textFilter = (EntryNoOnReconciliationFilter)filter[ReconFilterStripBusinessObject.Schema.EntryNumberOnReconciliation];
			textFilter.EntryFilerCode = "XJ5";
			textFilter.Property = "00000051";
			textFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("Declaration 2 should be found", reconDeclaration2.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			textFilter.EntryFilerCode = ZString.Empty;
			textFilter.Property = "00000048";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("Declaration 1 should be found", reconDeclaration1.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			textFilter.Property = "00000055";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("Declaration 3 should be found", reconDeclaration3.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			textFilter.Property = ZString.Empty;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals("all declarations should be found", 3, coll.Count);
		}

		ReconDeclaration GetReconDeclaration(ZString declarationShouldBeReconciledNumber, ZString entryOnReconciliationNumber, ZString reconWrappedDeclarationJobNumber)
		{
			var declarationShouldBeReconciled = Factory.New<JobDeclaration>();
			declarationShouldBeReconciled.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationShouldBeReconciled.JE_DeclarationReference = declarationShouldBeReconciledNumber;
			declarationShouldBeReconciled.US_EntryFilerCode = "XJ5";
			var entryOnReconciliation = declarationShouldBeReconciled.ActiveEntryHeaders.AddNew();
			entryOnReconciliation.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryOnReconciliation.EntryNumber = entryOnReconciliationNumber;
			var reconWrappedDeclaration = Factory.New<JobDeclaration>();
			reconWrappedDeclaration.JE_DeclarationReference = reconWrappedDeclarationJobNumber;
			var reconDeclaration = new ReconDeclaration(reconWrappedDeclaration);
			var reconOriginalEntry = reconWrappedDeclaration.CustomsEntryHeaders.AddNew();
			reconOriginalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			var reconOriginalEntryFound = reconDeclaration.OriginalEntries[0];
			reconOriginalEntryFound.CH_OrigEntryReference = "XJ5" + entryOnReconciliationNumber;
			Factory.Save();
			return reconDeclaration;
		}

		protected override ModuleTextFilter GetNewModuleFilter() => new EntryNoOnReconciliationFilter("moo");
	}
}
