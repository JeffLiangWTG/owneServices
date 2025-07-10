using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Test
{
	[TestedType(typeof(ViewRatingContractQuantity))]
	public class ViewRatingContractQuantityTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var ratingContract = Factory.New<RatingContract>();
			var viewRatingContractQuantity = Factory.New<ViewRatingContractQuantity>();
			viewRatingContractQuantity.RCQ_RCT_RatingContract = ratingContract.PK;

			return viewRatingContractQuantity;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot save nor delete view", true);
		}
	}
}
