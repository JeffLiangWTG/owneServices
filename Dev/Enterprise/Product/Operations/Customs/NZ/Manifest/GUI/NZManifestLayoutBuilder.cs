using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.NZ.Manifest.Business;

namespace Enterprise.Customs.NZ.Manifest.GUI
{
	public class NZManifestLayoutBuilder : ManifestLayoutBuilder<AsycudaManifestHeader>
	{
		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			bool IsSeaICR(AsycudaManifestHeader h) => h.IsSea && h.AMA_ManifestType == NZManifestTypes.Codes.ICR;
			SetVisibility(CommonBag.RadioCallSignTextBox, IsSeaICR, h => h.AMA_TransportModeInfo, h => h.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.ConveyanceCountryCodeFindBox, IsSeaICR, h => h.AMA_TransportModeInfo, h => h.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.MastersNameTextBox, IsSeaICR, h => h.AMA_TransportModeInfo, h => h.AMA_ManifestTypeInfo);

			bool IsICR(AsycudaManifestHeader h) => h.AMA_ManifestType == NZManifestTypes.Codes.ICR;
			SetVisibility(CommonBag.AgentTypeDropEdit, IsICR, h => h.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.ContainerModeDropEdit, IsICR, h => h.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.CustomsOfficeDropEdit, IsICR, h => h.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.CarrierCodeTextBox, IsICR, h => h.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.ShippingAgentAddressControl, IsICR, h => h.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.DeconsolidateAddressControl, IsICR, h => h.AMA_ManifestTypeInfo);

			bool IsNotICR(AsycudaManifestHeader h) => h.AMA_ManifestType != NZManifestTypes.Codes.ICR;
			SetVisibility(CommonBag.RegistrationDateEdit, IsNotICR, h => h.AMA_ManifestTypeInfo);
			SetVisibility(CommonBag.CustomsStatusDropEdit, IsNotICR, h => h.AMA_ManifestTypeInfo);
		}
	}
}
