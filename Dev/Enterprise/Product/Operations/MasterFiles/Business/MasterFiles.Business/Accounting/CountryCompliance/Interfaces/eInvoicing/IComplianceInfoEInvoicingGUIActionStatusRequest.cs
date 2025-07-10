using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceInfoEInvoicingGUIActionStatusRequest
	{
		ZString StatusRequestMenuName { get; }

		ZString StatusRequestActionInformation { get; }
	}
}
