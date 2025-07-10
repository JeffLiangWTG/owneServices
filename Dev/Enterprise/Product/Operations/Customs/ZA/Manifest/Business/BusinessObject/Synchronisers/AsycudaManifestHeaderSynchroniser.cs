using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
	{
		public AsycudaManifestHeaderSynchroniser(AsycudaManifestHeader destination, ForwardingConsol sourceConsol)
			: base(destination, sourceConsol)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			var randomDeclaration = Source.Shipments.Cast<ForwardingShipment>().SelectMany(shipment => shipment.Declarations.OfType<JobDeclaration>()).FirstOrDefault(x => x.JE_CustomsOffice != ZString.Empty);
			if (randomDeclaration != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_CustomsOfficeInfo, randomDeclaration.JE_CustomsOfficeInfo));
			}
			if (Source.JK_AgentType == Core.Constants.AgentType.CoLoad && Source.JK_TransportMode == Core.Constants.TransportModes.Sea)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.AMA_OA_ShippingAgentInfo, Source.JK_OA_CreditorAddressInfo));
			}
		}

		protected override BusinessObjectCollectionSynchroniser GetNewBillCollectionSynchroniser()
		{
			return new AsycudaBillCollectionSynchroniser((AsycudaManifestHeader)Destination);
		}

		protected override void OnSynchronise(SynchroniseEventArgs e)
		{
			var loadPort = Destination.AMA_RL_NKPortOfLoading;
			var dischargePort = Destination.AMA_RL_NKPortOfDischarge;
			base.OnSynchronise(e);
			if (e.Action == SynchroniseAction.Force && (loadPort != Destination.AMA_RL_NKPortOfLoading || dischargePort != Destination.AMA_RL_NKPortOfDischarge))
			{
				((AsycudaManifestHeader)Destination).SetCallPurposeCodeByPorts();
			}
		}
	}
}
