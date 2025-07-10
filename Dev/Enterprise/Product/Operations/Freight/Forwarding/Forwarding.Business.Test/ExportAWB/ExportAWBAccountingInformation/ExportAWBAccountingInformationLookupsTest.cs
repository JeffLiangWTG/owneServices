using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ExportAWBAccountingInformationLookupsTest : Forwarding.AWB.Business.Testing.ExportAWBAccountingInformationLookupsTest
	{
		public void TestAccountingCodesIncludingItalian()
		{
			ExportAWBAccountingInformation accountingInformation = Factory.New<ExportAWBAccountingInformation>();
			AssertEquals(OLookUpEditType.AWBAccountingCodes, accountingInformation.Lookups.AccountingCodes.LookupEditType);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "IT";

			AssertCollectionContains("SIV", accountingInformation.Lookups.AccountingCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
			AssertCollectionContains("IIV", accountingInformation.Lookups.AccountingCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
			AssertEquals(OLookUpEditType.AWBAccountingCodes, accountingInformation.Lookups.AccountingCodes.LookupEditType);
		}

		public void TestAccountingCodesExcludesACASCode()
		{
			var awbAccountingCodes = new CodeDescriptionPairList(OLookUpEditType.AWBAccountingCodes);
			AssertEquals(OLookUpEditType.AWBAccountingCodes, awbAccountingCodes.LookupEditType);
			AssertEquals(21, awbAccountingCodes.Count);

			ExportAWBAccountingInformation accountingInformation = Factory.New<ExportAWBAccountingInformation>();
			AssertEquals(OLookUpEditType.AWBAccountingCodes, accountingInformation.Lookups.AccountingCodes.LookupEditType);
			AssertEquals(6, accountingInformation.Lookups.AccountingCodes.Count);

			AssertCollectionContains("GBL", accountingInformation.Lookups.AccountingCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
			AssertCollectionContains("RET", accountingInformation.Lookups.AccountingCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
			AssertCollectionContains("GEN", accountingInformation.Lookups.AccountingCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
			AssertCollectionContains("MCO", accountingInformation.Lookups.AccountingCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
			AssertCollectionContains("SRN", accountingInformation.Lookups.AccountingCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
			AssertCollectionContains("STL", accountingInformation.Lookups.AccountingCodes.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}
	}
}
