using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLDetailCollection))]
	public class CusSeaManOBLDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSeaManOBLDetailCollection(Factory.New<CusSeaManOBLHeader>());
		}
	}
}
