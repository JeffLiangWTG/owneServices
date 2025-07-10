using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgTradeProspectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIncoterms()
		{
			var prospect = Factory.New<OrgTradeProspect>();
			Assert("Incoterms.Count > 0", prospect.Lookups.Incoterms.Count > 0);
			CodeDescriptionPairList domesticTerms = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms);
			foreach (CodeDescriptionPair term in domesticTerms)
			{
				AssertCollectionContains("Ensure Incoterms list contains domestic payment terms", term, prospect.Lookups.Incoterms);
			}
		}

		public void TestExpiryReasons()
		{
			var prospect = Factory.New<OrgTradeProspect>();

			AssertEquals(3, prospect.Lookups.ExpiryReasons.Count);
			Assert(prospect.Lookups.ExpiryReasons.ContainsCode("LST"));
			Assert(prospect.Lookups.ExpiryReasons.ContainsCode("SUP"));
			Assert(prospect.Lookups.ExpiryReasons.ContainsCode("MAN"));

			AssertEquals(1, prospect.Lookups.ManualExpiryReasons.Count);
			Assert(prospect.Lookups.ManualExpiryReasons.ContainsCode("MAN"));
		}
	}
}
