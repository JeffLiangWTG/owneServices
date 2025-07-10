using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentHeaderGenPivotCollection))]
	public class HVLVConsignmentHeaderGenPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<HVLVConsignmentHeaderGenPivotCollection>
	{
		protected override HVLVConsignmentHeaderGenPivotCollection GetCollectionToTest()
		{
			var consignmentHeader = Factory.New<HVLVConsignmentHeader>();
			return new HVLVConsignmentHeaderGenPivotCollection(consignmentHeader);
		}

		public void TestOnlyLoadRelatedRecords()
		{
			var setupFactory = new BusinessObjectFactory();

			var consignmentHeader1 = setupFactory.NewWithValidTestData<HVLVConsignmentHeaderForTest>();
			var consignmentHeader2 = setupFactory.NewWithValidTestData<HVLVConsignmentHeaderForTest>();
			var dummyRecord1 = setupFactory.NewWithValidTestData<DummyBaseBusinessObject>();
			var dummyRecord2 = setupFactory.NewWithValidTestData<DummyBaseBusinessObject>();

			consignmentHeader1.GenPivotCollection.AddRelatedIfNotExist(dummyRecord1);
			consignmentHeader2.GenPivotCollection.AddRelatedIfNotExist(dummyRecord2);

			setupFactory.Save();

			consignmentHeader1 = Factory.Load<HVLVConsignmentHeaderForTest>(consignmentHeader1.PK);

			CombineAssertions("The collection should only load GenPivot related to current record.", () =>
			{
				AssertEquals(1, consignmentHeader1.GenPivotCollection.Count);
				AssertEquals(dummyRecord1.PK, consignmentHeader1.GenPivotCollection[0].Relation2ID);
			});
		}
	}
}
