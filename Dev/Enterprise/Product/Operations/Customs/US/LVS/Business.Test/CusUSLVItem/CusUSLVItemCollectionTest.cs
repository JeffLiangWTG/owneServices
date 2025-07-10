using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVItemCollection))]
	public class CusUSLVItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusUSLVItemCollection(Factory.New<CusUSLVConsignment>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusUSLVItem>();
		}
	}
}
