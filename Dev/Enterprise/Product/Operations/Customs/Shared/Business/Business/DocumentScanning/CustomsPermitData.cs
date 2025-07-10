using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CustomsPermitData),
	Enterprise.Core.Constants.DocManagerCodes.CustomsPermit)]

namespace Enterprise.Customs.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;

	public class CustomsPermitData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(BaseCusPermitHeader); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("D0FDEB36-8DE1-4963-965D-C28F26FFC7D4", "Permit"); } }
	}
}
