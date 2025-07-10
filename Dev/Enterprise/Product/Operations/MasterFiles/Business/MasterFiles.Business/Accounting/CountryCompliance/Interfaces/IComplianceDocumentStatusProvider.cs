using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceDocumentStatusProvider
	{
		ICodeDescriptionPairList GetComplianceDocumentStatusTypes();
		string GetComplianceDocumentStatus(string statusType);
	}
}
