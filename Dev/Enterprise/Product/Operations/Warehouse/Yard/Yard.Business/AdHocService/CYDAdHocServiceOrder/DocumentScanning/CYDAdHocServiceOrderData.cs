using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CYDAdHocServiceOrderData),
	Enterprise.Core.Constants.DocManagerCodes.CYDAdHocServiceOrder)]

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDAdHocServiceOrderData : CYDAssemblyData<CYDAdHocServiceOrder>
	{
		protected override Type CollectionType => typeof(CYDAdHocServiceOrderCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("CYDAdHocServiceOrderData|HumanReadableName", "Ad-Hoc Service Order");
	}
}
