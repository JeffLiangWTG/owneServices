using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceInfoEInvoicingGUIActionDocumentRequest
	{
		ZString DocumentRequestMenuName { get; }

		ZString DocumentRequestActionInformation { get; }
	}
}
