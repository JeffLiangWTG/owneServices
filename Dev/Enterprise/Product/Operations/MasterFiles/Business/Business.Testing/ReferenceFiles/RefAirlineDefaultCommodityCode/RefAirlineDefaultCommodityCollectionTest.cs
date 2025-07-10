using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefAirlineDefaultCommodityCodeCollection))]
	public class RefAirlineDefaultCommodityCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefAirlineDefaultCommodityCodeCollection(Factory.NewWithValidTestData<RefAirline>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<RefAirlineDefaultCommodityCode>();
		}
	}
}
