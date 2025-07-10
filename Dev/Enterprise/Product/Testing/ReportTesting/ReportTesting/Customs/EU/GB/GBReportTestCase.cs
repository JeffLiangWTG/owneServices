using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	public abstract class GBReportTestCase : ReportTestCase
	{
		protected override void SetUp()
		{
			temporaryLogin = GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedKingdom);
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			temporaryLogin?.Dispose();
			temporaryLogin = null;
		}

		System.IDisposable temporaryLogin;
	}
}
