using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class TraderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTraderTypeList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair("BUY", "Buyer"),
				new CodeDescriptionPair("SEL", "Seller"),
			}, Lookups.TraderTypeList);
		}

		TraderLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					var trader = Factory.New<Trader>();
					lookups = new TraderLookups(trader);
				}
				return lookups;
			}
		}
		TraderLookups lookups;
	}
}
