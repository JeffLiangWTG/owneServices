using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal class PackageLineCostingAutoRateCalculatorTest : PackageLineAutoRateCalculatorTest
	{
		#region Implementation

		protected override RatingHeader RatingHeader => ratingHeader ?? (ratingHeader = Helper.NewCosting(NewClient));
		RatingHeader ratingHeader;

		protected override CostSell CostSell => CostSell.Cost;

		protected override void Save() => Factory.Save();

		protected override OrgHeader LocalClient => NewClient;

		#endregion
	}
}
