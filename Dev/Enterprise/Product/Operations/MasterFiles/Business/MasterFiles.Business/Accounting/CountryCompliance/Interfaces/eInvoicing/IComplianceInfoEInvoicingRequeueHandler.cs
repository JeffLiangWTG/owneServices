using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceInfoEInvoicingRequeueHandler
	{
		bool IsPivotRequeueRestricted(ZString pivotState, out string warningMessage);
	}
}
