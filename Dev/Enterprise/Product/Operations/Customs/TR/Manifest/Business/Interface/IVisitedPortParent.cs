using CargoWise.Types;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public interface IVisitedPortParent
	{
		VisitedPortCollection VisitedPorts { get; }
		ZBool SupportsCustomsPorts { get; }
		AsycudaManifestHeader ManifestHeader { get; }
	}
}
