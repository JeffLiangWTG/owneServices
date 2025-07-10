using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.OceanCarrier.Business.CarrierVoyageData),
	Enterprise.Core.Constants.DocManagerCodes.CarrierVoyage)]

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyageData : AssemblyData
	{
		protected override Type CollectionType => typeof(CarrierVoyageCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("CarrierVoyageData|HumanReadableName", "Carrier Voyage Data");

		public override Type BusinessObjectType => typeof(CarrierVoyage);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
