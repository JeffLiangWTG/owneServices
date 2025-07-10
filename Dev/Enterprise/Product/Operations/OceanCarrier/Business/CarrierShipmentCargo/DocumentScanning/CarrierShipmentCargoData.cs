using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.OceanCarrier.Business.CarrierShipmentCargoData),
	Enterprise.Core.Constants.DocManagerCodes.CarrierShipmentCargo)]

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentCargoData : AssemblyData
	{
		protected override Type CollectionType => typeof(CarrierShipmentCargoCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("CarrierShipmentCargoData|HumanReadableName", "Carrier Shipment Cargo Data");

		public override Type BusinessObjectType => typeof(CarrierShipmentCargo);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
