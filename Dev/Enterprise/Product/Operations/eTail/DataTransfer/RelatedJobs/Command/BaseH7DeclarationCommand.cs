using System;
using CargoWise.Definitions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.eTail.DataTransfer
{
	[ShipmentDirection(new[] { Directions.Import })]
	[ShipmentTransportMode(new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road })]
	[RequiredFeatureControlCode(LicenceFeatureCodeList.Codes.EcommerceH7Feature)]
	public abstract class BaseH7DeclarationCommand : CustomsJobCommand
	{
		public BaseH7DeclarationCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("3d54f5a7-08dd-4a8a-866d-e09e872131d9", "Low Value (H7)");
		protected override Type RelatedCustomsJobType => typeof(Enterprise.Integration.Customs.ASYCUDA.EUManifest.IAsycudaManifestHeader);
		public override string UsageCode => UsageCodes.EUH7Declaration;
	}
}
