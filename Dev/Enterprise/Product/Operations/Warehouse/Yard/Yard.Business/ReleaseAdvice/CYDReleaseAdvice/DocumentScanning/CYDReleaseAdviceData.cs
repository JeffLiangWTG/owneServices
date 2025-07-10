using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CYDReleaseAdviceData),
	Enterprise.Core.Constants.DocManagerCodes.CYDReleaseAdvice)]

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReleaseAdviceData : CYDAssemblyData<CYDReleaseAdvice>
	{
		protected override Type CollectionType => typeof(CYDReleaseAdviceCollection);

		public override MultilingualString HumanReadableName
		{
			get
			{
				return ResString.GetMultilingualString("CYDReleaseAdviceData|HumanReadableName", "Yard Release Advice");
			}
		}
	}
}

