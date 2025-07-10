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
	[ShipmentTransportMode(new[] { TransportModes.Sea })]
	[ShipmentDirection(new[] { Directions.Import, Directions.Export })]
	public class NZSeaCargoReportCommand : CustomsJobCommand
	{
		public NZSeaCargoReportCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("82872045-95d2-425a-b9c0-177e3931d31d", "HVLV SeaCargo {0}",
			Shipment.JobDirection == Directions.Import ? "ICR" : "CRE");

		protected override Type RelatedCustomsJobType => typeof(ICusSCAOceanBill);

		public override CustomsRelatedBusinessObjectConverter Converter => new NZSeaCargoReportConverter(this);

		public override string UsageCode => Shipment.JobDirection == Directions.Import ? UsageCodes.NZSeaICR : UsageCodes.NZSeaCRE;

		public override bool ShouldValidateItemContainer => true;
	}
}
