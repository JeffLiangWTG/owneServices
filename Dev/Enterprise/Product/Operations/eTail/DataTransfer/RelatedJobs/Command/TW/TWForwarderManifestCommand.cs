using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs.ASYCUDA.TWManifest;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.Taiwan })]
	[ShipmentTransportMode(new[] { TransportModes.Air, TransportModes.Sea })]
	[ShipmentDirection(new[] { Directions.Import, Directions.Export })]
	public class TWForwarderManifestCommand : CustomsJobCommand
	{
		public TWForwarderManifestCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("9bf8c6ed-0dbd-4329-98f9-81791070eb1b", "TW Forwarder Manifest ({0})",
			Shipment.JobDirection == Directions.Import
			? ResString.GetMultilingualString("d3d43c5c-6a41-4494-a2e8-8b13e2c8803f", "Import")
			: ResString.GetMultilingualString("85563d7f-0f71-44c6-823e-a42ea500cb67", "Export"));

		protected override Type RelatedCustomsJobType => typeof(IAsycudaManifestHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new TWForwarderManifestConverter(this);

		public override string UsageCode => UsageCodes.TWForwarderManifest;
	}
}
