using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs.NZ;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.NewZealand })]
	[ShipmentTransportMode(new[] { TransportModes.Air })]
	[ShipmentDirection(new[] { Directions.Import, Directions.Export })]
	public class NZAirCargoReportCommand : CustomsJobCommand
	{
		public NZAirCargoReportCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("e936dc38-0b58-45c5-9fcc-fe55d09c0b32", "HVLV AirCargo {0}",
			Shipment.JobDirection == Directions.Import ? "ICR" : "CRE");

		protected override Type RelatedCustomsJobType => typeof(ICusMAWB);

		public override CustomsRelatedBusinessObjectConverter Converter => new NZAirCargoReportConverter(this);

		public override string UsageCode => Shipment.JobDirection == Directions.Import ? UsageCodes.NZAirICR : UsageCodes.NZAirCRE;
	}
}
