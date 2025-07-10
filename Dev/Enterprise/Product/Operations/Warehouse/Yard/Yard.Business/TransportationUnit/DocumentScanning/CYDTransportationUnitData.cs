using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CYDTransportationUnitData),
	Enterprise.Core.Constants.DocManagerCodes.CYDTransportationUnit)]

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitData : CYDAssemblyData<CYDTransportationUnit>
	{
		protected override Type CollectionType => typeof(CYDTransportationUnitCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("CYDTransportationUnitData|HumanReadableName", "Transportation Unit");
	}
}
