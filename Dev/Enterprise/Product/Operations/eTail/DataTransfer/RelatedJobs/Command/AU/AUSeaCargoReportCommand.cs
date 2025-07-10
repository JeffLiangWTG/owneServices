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
	[ShipmentTransportMode(new[] { TransportModes.Sea })]
	[ShipmentDirection(new[] { Directions.Import })]
	public class AUSeaCargoReportCommand : CustomsJobCommand
	{
		public AUSeaCargoReportCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("217ebff7-d544-4342-a4c5-cb7db0db9631", "HVLV SeaCargo Report");

		protected override Type RelatedCustomsJobType => typeof(ICusSCAOceanBill);

		public override CustomsRelatedBusinessObjectConverter Converter => new SeaOceanBillConverter(this);

		public override string UsageCode => UsageCodes.AUSeaCargoReport;

		public override bool ShouldValidateItemContainer => true;
	}
}
