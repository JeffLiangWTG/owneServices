using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

class CusExitReportConfiguration : EU.ExitControl.Business.CusExitReportConfiguration
{
	protected override ICusExitReportValidationDecider GetUcc6ValidationDecider() => new CusExitReportUcc6ValidationDecider();
}
