using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.UnitedStates }, allowLoginToDifferentCountryRegistry: nameof(HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies))]
	[ShipmentDestinationCountry(CountryCodes.UnitedStates)]
	[ShipmentTransportMode(new[] { TransportModes.Air })]
	public class USAirAMSCommand : BaseHVLVRelatedJobCommand
	{
		public USAirAMSCommand(ForwardingShipment shipment) : base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("61f7dc40-f0fa-4a37-be16-9a45a4a40925", "US Air AMS (Import)");

		protected override Type RelatedCustomsJobType => typeof(Enterprise.Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new USAirAMSConverter(this);

		public override bool ShouldValidateWaybill => true;

		public override string UsageCode => UsageCodes.USAirAMS;
	}
}
