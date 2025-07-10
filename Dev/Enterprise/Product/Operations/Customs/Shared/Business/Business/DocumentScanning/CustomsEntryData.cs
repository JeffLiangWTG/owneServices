using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CustomsEntryData),
	Enterprise.Core.Constants.DocManagerCodes.CustomsEntry)]

namespace Enterprise.Customs.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;

	public class CustomsEntryData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CusEntryHeader); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("8e651c08-7ddb-4d8d-a700-43370d015a18", "Customs Entry"); } }
	}
}
