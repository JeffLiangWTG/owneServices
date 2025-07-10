using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CustomsContainerData),
	Enterprise.Core.Constants.DocManagerCodes.CustomsContainer)]

namespace Enterprise.Customs.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;

	public class CustomsContainerData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(BaseCusContainer); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("ff59b074-2695-4d72-96eb-b79eacd66337", "Container"); } }
	}
}
