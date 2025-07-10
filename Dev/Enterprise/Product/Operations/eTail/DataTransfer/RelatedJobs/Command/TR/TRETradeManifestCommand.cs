using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs.ASYCUDA.TRETrade;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.Turkey })]
	[ShipmentTransportMode(new[] { TransportModes.Air, TransportModes.Sea })]
	[ShipmentDirection(new[] { Directions.Import, Directions.Export })]
	public class TRETradeManifestCommand : CustomsJobCommand
	{
		public TRETradeManifestCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("c8f0285a-a5e3-4c77-a73c-3338ba4979b1", "TR E-Trade Manifest");

		protected override Type RelatedCustomsJobType => typeof(IAsycudaManifestHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new TRETradeManifestConverter(this);

		public override string UsageCode => UsageCodes.TRETradeManifest;
	}
}
