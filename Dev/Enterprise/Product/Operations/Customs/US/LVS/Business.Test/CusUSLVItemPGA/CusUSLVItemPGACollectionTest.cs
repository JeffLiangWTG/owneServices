using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVItemPGACollection))]
	public class CusUSLVItemPGACollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusUSLVItemPGACollection(Factory.New<CusUSLVItem>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusUSLVItemPGA>();
		}
	}
}
