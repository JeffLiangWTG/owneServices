namespace Enterprise.Customs.PL.ExitControl.Business;

class ExitControlConfiguration : EU.ExitControl.Business.ExitControlConfiguration
{
	protected override EU.ExitControl.Business.CusExitReportConfiguration GetNewCusExitReportConfiguration() => new CusExitReportConfiguration();
}
