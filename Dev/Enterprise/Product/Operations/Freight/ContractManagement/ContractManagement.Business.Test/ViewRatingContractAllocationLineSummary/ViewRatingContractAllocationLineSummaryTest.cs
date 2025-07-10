using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Test
{
	[TestedType(typeof(ViewRatingContractAllocationLineSummary))]
	public class ViewRatingContractAllocationLineSummaryTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var allocationLine = Factory.New<RatingContractAllocationLine>();
			var viewRatingContractAllocationLineSummary = Factory.New<ViewRatingContractAllocationLineSummary>();
			viewRatingContractAllocationLineSummary.RAV_RCA_AllocationLine = allocationLine.PK;

			return viewRatingContractAllocationLineSummary;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot save nor delete view", true);
		}
	}
}
