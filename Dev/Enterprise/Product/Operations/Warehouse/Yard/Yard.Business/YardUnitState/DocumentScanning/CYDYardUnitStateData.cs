using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CYDYardUnitStateData),
	Enterprise.Core.Constants.DocManagerCodes.CYDYardUnitState)]

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardUnitStateData : CYDAssemblyData<CYDYardUnitState>
	{
		protected override Type CollectionType => typeof(CYDYardUnitStateCollection);

		public override MultilingualString HumanReadableName
		{
			get
			{
				return ResString.GetMultilingualString("CYDYardUnitState|HumanReadableName", "Yard Unit State");
			}
		}
	}
}
