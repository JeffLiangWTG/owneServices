using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(Order.MilestoneCollectionViewConstantSize))]
	sealed class MilestoneCollectionViewConstantSizeTest : BusinessObjectCollectionViewTestCase<Order.MilestoneCollectionViewConstantSize>
	{
		protected override Order.MilestoneCollectionViewConstantSize GetCollectionToTest()
		{
			var taskCollection = new ProcessTaskCollection(Factory);
			return new Order.MilestoneCollectionViewConstantSize(taskCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ProcessTask result = Factory.New<ProcessTask>();
			result.IsMilestone = true;
			return result;
		}
	}
}
