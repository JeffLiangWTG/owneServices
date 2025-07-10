using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentReturnCollection))]
	public class HVLVConsignmentReturnCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollection_WithFormerConsignmentType_ContainsFormerConsignments()
		{
			var consignmentA = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentB = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentC = Factory.NewWithValidTestData<HVLVConsignment>();

			var pivot1 = Factory.NewWithValidTestData<HVLVReturnPivot>();
			var pivot2 = Factory.NewWithValidTestData<HVLVReturnPivot>();

			pivot1.HVP_HVC_Former = consignmentB.PK;
			pivot1.HVP_HVC_Return = consignmentA.PK;

			pivot2.HVP_HVC_Former = consignmentC.PK;
			pivot2.HVP_HVC_Return = consignmentA.PK;

			Factory.Save();

			var collection = new HVLVConsignmentReturnCollection(consignmentA, HVLVConsignmentReturnCollection.ConsignmentType.Former);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { consignmentB, consignmentC }, collection);
		}

		public void TestCollection_WithReturnConsignmentType_ContainsReturnConsignments()
		{
			var consignmentA = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentB = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentC = Factory.NewWithValidTestData<HVLVConsignment>();

			var pivot1 = Factory.NewWithValidTestData<HVLVReturnPivot>();
			var pivot2 = Factory.NewWithValidTestData<HVLVReturnPivot>();

			pivot1.HVP_HVC_Former = consignmentA.PK;
			pivot1.HVP_HVC_Return = consignmentB.PK;

			pivot2.HVP_HVC_Former = consignmentA.PK;
			pivot2.HVP_HVC_Return = consignmentC.PK;

			Factory.Save();

			var collection = new HVLVConsignmentReturnCollection(consignmentA, HVLVConsignmentReturnCollection.ConsignmentType.Return);
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { consignmentB, consignmentC }, collection);
		}

		public void TestCollection_WithFormerConsignmentType_IsEmptyWhenNoFormerConsignments()
		{
			var consignmentA = Factory.NewWithValidTestData<HVLVConsignment>();

			Factory.Save();

			AssertEquals(0, consignmentA.FormerConsignments.Count);
		}

		public void TestCollection_WithReturnConsignmentType_IsEmptyWhenNoReturnConsignments()
		{
			var consignmentA = Factory.NewWithValidTestData<HVLVConsignment>();

			Factory.Save();

			AssertEquals(0, consignmentA.ReturnConsignments.Count);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HVLVConsignmentReturnCollection(Factory.New<HVLVConsignment>(), HVLVConsignmentReturnCollection.ConsignmentType.Return);
		}

		#endregion
	}
}
