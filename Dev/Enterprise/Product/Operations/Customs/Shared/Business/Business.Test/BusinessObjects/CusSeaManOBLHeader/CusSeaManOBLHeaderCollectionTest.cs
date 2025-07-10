using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderCollection))]
	public class CusSeaManOBLHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSeaManOBLHeaderCollection(Factory.New<CusSeaManTranHead>());
		}
	}
}
