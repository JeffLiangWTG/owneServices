using CargoWise.Types;

namespace Enterprise.TransportConsignment.Integration
{
	public interface IDtbLinehaulManifest
	{
		ZString LHM_ManifestID { get; set; }
		ZGuid PK { get; }
	}
}
