using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.OceanCarrier.Business.CarrierVoyagePortCallData),
	Enterprise.Core.Constants.DocManagerCodes.CarrierVoyagePortCall)]

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyagePortCallData : AssemblyData
	{
		protected override Type CollectionType => typeof(CarrierVoyagePortCallCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("CarrierVoyagePortCallData|HumanReadableName", "Carrier Voyage Port Call Data");

		public override Type BusinessObjectType => typeof(CarrierVoyagePortCall);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
