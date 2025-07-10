using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManArrivalPortCollection))]
	public class CusSeaManArrivalPortCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSeaManArrivalPortCollection(Factory.New<CusSeaManTranHead>());
		}
	}
}
