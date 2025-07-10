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
	[ShipmentTransportMode(new[] { TransportModes.Sea })]
	public class USSeaAMSCommand : BaseHVLVRelatedJobCommand
	{
		public USSeaAMSCommand(ForwardingShipment shipment) : base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("84bba04d-daf3-44bb-a14b-7522c95c30cf", "US Sea AMS (Import)");

		protected override Type RelatedCustomsJobType => typeof(Enterprise.Integration.Customs.US.USAMS.ICusInBondHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new USSeaAMSConverter(this);

		public override bool ShouldValidateWaybill => true;

		public override string UsageCode => UsageCodes.USSeaAMS;
	}
}
