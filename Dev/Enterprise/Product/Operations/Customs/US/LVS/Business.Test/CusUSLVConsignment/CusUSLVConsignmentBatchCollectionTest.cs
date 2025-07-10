using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.LVS.Business.Testing;

public class CusUSLVConsignmentBatchCollectionTest : TestCaseWithFactory
{
	public void TestGeneratingBatches_BasicBatching()
	{
		var consignments = new List<CusUSLVConsignment>();
		for (var i = 0; i < 1000; i++)
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			_ = consignment.CusUSLVItems.AddNew();
			consignments.Add(consignment);
		}

		var batchCollection = new CusUSLVConsignmentBatchCollection(consignments);
		Assert(batchCollection.MoveNextBatch());
		AssertEquals(999, batchCollection.CurrentBatch.Count);
		AssertEquals(999, batchCollection.CurrentBatch.Sum(c => c.CusUSLVItems.Count));
		Assert(batchCollection.MoveNextBatch());
		AssertEquals(1, batchCollection.CurrentBatch.Count);
		AssertEquals(1, batchCollection.CurrentBatch.Sum(c => c.CusUSLVItems.Count));
		Assert(!batchCollection.MoveNextBatch());
	}

	public void TestGeneratingBatches_BatchConsignmentAsAWhole()
	{
		var consignments = new List<CusUSLVConsignment>();
		for (var i = 0; i < 499; i++)
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			_ = consignment.CusUSLVItems.AddNew();
			_ = consignment.CusUSLVItems.AddNew();
			consignments.Add(consignment);
		}

		var consignment1 = Factory.NewWithValidTestData<CusUSLVConsignment>();
		_ = consignment1.CusUSLVItems.AddNew();
		_ = consignment1.CusUSLVItems.AddNew();
		_ = consignment1.CusUSLVItems.AddNew();
		consignments.Add(consignment1);

		var batchCollection = new CusUSLVConsignmentBatchCollection(consignments);
		Assert(batchCollection.MoveNextBatch());
		AssertEquals(499, batchCollection.CurrentBatch.Count);
		AssertEquals(998, batchCollection.CurrentBatch.Sum(c => c.CusUSLVItems.Count));
		Assert(batchCollection.MoveNextBatch());
		AssertEquals(1, batchCollection.CurrentBatch.Count);
		AssertEquals(3, batchCollection.CurrentBatch.Sum(c => c.CusUSLVItems.Count));
		Assert(!batchCollection.MoveNextBatch());
	}

	public void TestGeneratingBatches_ConsignmentWith1000Lines()
	{
		var consignments = new List<CusUSLVConsignment>();
		var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
		for (var i = 0; i < 1000; i++)
		{
			_ = consignment.CusUSLVItems.AddNew();
		}
		consignments.Add(consignment);

		consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
		_ = consignment.CusUSLVItems.AddNew();
		consignments.Add(consignment);

		var batchCollection = new CusUSLVConsignmentBatchCollection(consignments);
		Assert(batchCollection.MoveNextBatch());
		AssertEquals(1, batchCollection.CurrentBatch.Count);
		AssertEquals(1000, batchCollection.CurrentBatch.Sum(c => c.CusUSLVItems.Count));
		Assert(batchCollection.MoveNextBatch());
		AssertEquals(1, batchCollection.CurrentBatch.Count);
		AssertEquals(1, batchCollection.CurrentBatch.Sum(c => c.CusUSLVItems.Count));
		Assert(!batchCollection.MoveNextBatch());
	}
}
