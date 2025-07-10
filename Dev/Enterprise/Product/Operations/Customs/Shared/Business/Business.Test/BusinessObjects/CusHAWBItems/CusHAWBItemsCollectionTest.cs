using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusHAWBItemsCollection))]
	public class CusHAWBItemsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusHAWBItemsCollection(Factory.New<CusHAWB>());
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusHAWBItems>();
	}
}
