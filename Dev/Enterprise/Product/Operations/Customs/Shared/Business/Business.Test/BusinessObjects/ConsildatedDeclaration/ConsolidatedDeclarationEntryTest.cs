using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationEntry))]
	sealed class ConsolidatedDeclarationEntryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(GlbBranch.CurrentBranch.PK, consolidatedDeclarationEntry.CRE_GB_Branch);
			AssertEquals(ConsolidatedDeclarationEntry.EntryTypeForConsolidateDeclaration, consolidatedDeclarationEntry.CRE_EntryType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consolidatedDeclarationEntry = CreateConsolidatedDeclarationEntry(Factory);
		}

		protected override BusinessObject GetNewBusinessObject() => consolidatedDeclarationEntry;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => consolidatedDeclarationEntry;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateConsolidatedDeclarationEntry(factory);

		ConsolidatedDeclarationEntry CreateConsolidatedDeclarationEntry(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			consolidatedDeclarationEntry = Factory.New<ConsolidatedDeclarationEntry>();
			consolidatedDeclarationEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			consolidatedDeclarationEntry.CRE_EntryDate = ZDate.Today;
			consolidatedDeclarationEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			return consolidatedDeclarationEntry;
		}

		ConsolidatedDeclarationEntry consolidatedDeclarationEntry;
	}
}
