using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs.ASYCUDA.SGAccess;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.Singapore })]
	[ShipmentTransportMode(new[] { TransportModes.Air, TransportModes.Road })]
	[RequiredRegistryItem("SGCustomsDataRegistry", "ACCESSEnable")]
	public class SGCargoReportCommand : CustomsJobCommand
	{
		public SGCargoReportCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("aca7197c-7792-41f1-9bc3-b6684fb42680", "SG ACCESS {0} Manifest",
			Shipment.JobDirection == Directions.Import
			? ResString.GetMultilingualString("ba339195-4e22-48ea-9f31-bc4c324707db", "Import")
			: ResString.GetMultilingualString("86a0a402-1dcb-4df0-b550-eac62cef0e1a", "Export"));

		protected override Type RelatedCustomsJobType => typeof(IAsycudaManifestHeader);

		public override CustomsRelatedBusinessObjectConverter Converter => new SGAccessManifestConverter(this);

		public override string UsageCode => UsageCodes.SGSingaporeACCESS;
	}
}
