using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManSlotOrgCollection))]
	public class CusSeaManSlotOrgCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSeaManSlotOrgCollection(Factory.New<CusSeaManTranHead>());
		}
	}
}
