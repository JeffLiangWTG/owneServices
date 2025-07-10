using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

class CusExitReportUcc6ValidationDecider : ICusExitReportUcc6ValidationDecider
{
	public bool ValidateCER_CXC_ConsignmentMrnIsAlreadyBeingUsed => true;

	public bool ValidateCER_LocationLookups => false;
}
