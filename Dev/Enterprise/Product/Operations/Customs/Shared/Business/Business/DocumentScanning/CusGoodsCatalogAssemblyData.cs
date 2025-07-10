using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(CusGoodsCatalogAssemblyData), Enterprise.Core.Constants.DocManagerCodes.CusGoodsCatalog)]
namespace Enterprise.Customs.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class CusGoodsCatalogAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(BaseCusGoodsCatalog);

		protected override Type CollectionType => null;

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("e6c12ae5-74d4-4e72-8d36-b2665e1342a9", "Goods Catalog");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
