using System;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CYDReceiveAdviceData),
	Enterprise.Core.Constants.DocManagerCodes.CYDReceiveAdvice)]

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReceiveAdviceData : CYDAssemblyData<CYDReceiveAdvice>
	{
		protected override Type CollectionType => typeof(CYDReceiveAdviceCollection);

		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("CYDReceiveAdviceData|HumanReadableName", "Yard Receive Advice"); } }
	}
}
