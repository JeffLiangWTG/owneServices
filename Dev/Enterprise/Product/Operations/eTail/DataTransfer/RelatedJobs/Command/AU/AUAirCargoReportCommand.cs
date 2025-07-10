using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.Australia })]
	[ShipmentTransportMode(new[] { TransportModes.Air })]
	[ShipmentDirection(new[] { Directions.Import })]
	public class AUAirCargoReportCommand : CustomsJobCommand
	{
		public AUAirCargoReportCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("22ede14a-0fd4-4c64-918d-66cbdb883e52", "HVLV AirCargo Report");

		protected override Type RelatedCustomsJobType => typeof(ICusMAWB);

		public override CustomsRelatedBusinessObjectConverter Converter => new AirManifestConverter(this);

		public override string UsageCode => UsageCodes.AUAirCargoReport;
	}
}
