using CargoWise.Types;

namespace Enterprise.ProcessManagement.Integration
{
	public interface IProject
	{
		ZGuid PK { get; }
		ZGuid ClientOrganisationPK { get; }
		ZGuid WKP_OC_Contact { get; }
		ZString WKP_ProjectNumber { get; set; }
		ZString WKP_Summary { get; set; }
		ZBlob WKP_Details { get; set; }
		ZGuid WKP_P8_Opportunity { get; set; }
	}
}
