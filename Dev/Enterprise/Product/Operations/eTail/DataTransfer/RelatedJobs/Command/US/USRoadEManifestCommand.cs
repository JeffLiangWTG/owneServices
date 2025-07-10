using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs.US.eManifest;
using TransportModes = Enterprise.Core.Constants.TransportModes;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.UnitedStates, CountryCodes.Canada }, allowLoginToDifferentCountryRegistry: nameof(HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies))]
	[ShipmentDestinationCountry(CountryCodes.UnitedStates)]
	[ShipmentTransportMode(new[] { TransportModes.Road })]
	public class USRoadEManifestCommand : BaseHVLVRelatedJobCommand
	{
		public USRoadEManifestCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("6131bba9-3091-4ddf-9ba9-6fb4c8744d4b", "HVLV e-Manifest");

		public override bool ShouldValidateWaybill => true;

		public override CustomsRelatedBusinessObjectConverter Converter => new USeManifestConverter(this);

		public override string UsageCode => UsageCodes.USRoadEManifest;

		protected override Type RelatedCustomsJobType => typeof(ICusInBondHeader);
	}
}
