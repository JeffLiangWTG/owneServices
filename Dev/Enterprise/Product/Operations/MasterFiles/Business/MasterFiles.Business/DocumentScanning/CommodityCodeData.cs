using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(CommodityCodeData),
	Enterprise.Core.Constants.DocManagerCodes.CommodityCode)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class CommodityCodeData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefCommodityCode); } }
		protected override Type CollectionType
		{
			get { return typeof(RefCommodityCodeCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefCommodityCodeCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefCommodityCode; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("f9eca5ef-b9b8-45c8-8dcb-c58c0edf2417", "Commodity Code"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
