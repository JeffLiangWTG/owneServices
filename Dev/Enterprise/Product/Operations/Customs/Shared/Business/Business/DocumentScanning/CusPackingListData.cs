using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(CusPackingListData), Enterprise.Core.Constants.DocManagerCodes.CustomsPackingList)]
namespace Enterprise.Customs.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;

	public class CusPackingListData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusPackingList);

		protected override Type CollectionType => typeof(CusPackingListCollection);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("88F5EACD-6380-4F89-B8B4-7F803F886219", "Customs Packing List");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
