using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal class PackageLineCompanyTariffAutoRateCalculatorTest : PackageLineAutoRateCalculatorTest
	{
		#region Implementation

		protected override RatingHeader RatingHeader => ratingHeader ?? (ratingHeader = Helper.NewCompanyTariff());
		RatingHeader ratingHeader;

		protected override CostSell CostSell => CostSell.Revenue;

		protected override void Save() => RatingHeader.Factory.Save();

		protected override OrgHeader LocalClient => localClient ?? (localClient = Helper.NewOrgHeader(1));
		OrgHeader localClient;

		#endregion
	}
}
