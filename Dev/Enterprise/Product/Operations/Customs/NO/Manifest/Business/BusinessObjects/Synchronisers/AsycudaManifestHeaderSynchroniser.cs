using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NO.Manifest.Business;

public sealed class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
{
	public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
		: base(destination, sourceConsol)
	{
	}

	protected override void HookSynchronisers()
	{
		base.HookSynchronisers();
		Synchronisers.Add(new FieldSynchroniser(Destination.AMA_VehicleRegistrationInfo, Source.MostInterestingTransportForBinding[0].JW_VoyageFlightForBindingInfo));
	}
}
