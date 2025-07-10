using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVConsignmentAdhocEdocsSupportCollection))]
	class CusUSLVConsignmentAdhocEdocsSupportCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusUSLVConsignmentAdhocEdocsSupportCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CusUSLVConsignment>();
			result.ULB_ULH = shipment.PK;
			return result;
		}

		[ExpectNoExceptions]
		public override void TestAddAndDeleteOfElementAsThoughBinding()
		{
			var collection = GetCollectionToTest();
			var element = (CusUSLVConsignment)collection.AddNew();
			element.ULB_ULH = shipment.PK;
			collection.RemoveAndDelete(element);
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.NewWithValidTestData<CusUSLVClearance>();
		}

		CusUSLVClearance shipment;
	}
}
