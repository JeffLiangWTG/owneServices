using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.OceanCarrier.Business.CarrierShipmentHeaderData),
	Enterprise.Core.Constants.DocManagerCodes.CarrierShipmentHeader)]

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentHeaderData : AssemblyData
	{
		protected override Type CollectionType => typeof(CarrierShipmentHeaderCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("CarrierShipmentHeaderData|HumanReadableName", "Carrier Shipment Data");

		public override Type BusinessObjectType => typeof(CarrierShipmentHeader);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;
	}
}
