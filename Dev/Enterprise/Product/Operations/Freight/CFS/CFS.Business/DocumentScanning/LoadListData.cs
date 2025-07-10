using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(LoadListData),
	Enterprise.Core.Constants.DocManagerCodes.LoadList)]

namespace Enterprise.Freight.CFS.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	class LoadListData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CFSLoadListConsol); } }
		protected override Type CollectionType
		{
			get { return typeof(CFSLoadListConsolCollection); }
		}
		public override CargoWise.EntityFramework.IBusinessObjectCollection GetBusinessObjectCollection(CargoWise.EntityFramework.BusinessObjectFactory factory)
		{
			return new CFSLoadListConsolCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.LoadListConsol; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("8fc50ea9-9183-4fdd-93b8-036039debec6", "Load List"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
