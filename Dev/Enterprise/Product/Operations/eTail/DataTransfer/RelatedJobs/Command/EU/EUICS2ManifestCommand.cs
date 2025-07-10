using System;
using CargoWise.Application;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ShipmentDestinationCountry(CountryCodes.EuropeanUnion)]
	[ShipmentTransportMode(new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Road })]
	public class EUICS2ManifestCommand : CustomsJobCommand
	{
		public EUICS2ManifestCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("eaf8d4f1-8e24-48b4-a487-7498d8200f12", "EU ICS2 Manifest");

		public override CustomsRelatedBusinessObjectConverter Converter => new EUICS2ManifestConverter(this);

		protected override Type RelatedCustomsJobType => ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader>();

		public override string UsageCode => UsageCodes.EUICS2Manifest;
	}
}
