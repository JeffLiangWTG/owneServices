using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVConsignmentCollection))]
	public class CusUSLVConsignmentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusUSLVConsignmentCollection(Factory.New<CusUSLVClearance>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusUSLVConsignment>();
		}
	}
}
