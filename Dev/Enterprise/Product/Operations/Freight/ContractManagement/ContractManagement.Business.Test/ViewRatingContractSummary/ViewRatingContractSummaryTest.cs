using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Test
{
	[TestedType(typeof(ViewRatingContractSummary))]
	public class ViewRatingContractSummaryTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var ratingContract = Factory.New<RatingContract>();
			var viewRatingContractSummary = Factory.New<ViewRatingContractSummary>();
			viewRatingContractSummary.RCV_RCT_RatingContract = ratingContract.PK;

			return viewRatingContractSummary;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot save nor delete view", true);
		}
	}
}
