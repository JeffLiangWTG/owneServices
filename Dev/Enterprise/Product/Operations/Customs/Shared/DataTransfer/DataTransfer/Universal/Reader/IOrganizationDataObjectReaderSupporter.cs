using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public interface IOrganisationDataObjectReaderSupporter
	{
		OrganisationDataObjectReader CreateNewReader(OrganizationAddress addressData);
	}
}
