using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AgencyContainerManagerData),
	Enterprise.Core.Constants.DocManagerCodes.AgencyContainerManager)]

namespace Enterprise.Freight.Agency.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	class AgencyContainerManagerData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefContainerStock); } }
		protected override Type CollectionType
		{
			get { return typeof(RefContainerStockCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefContainerStockCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AgencyContainerManager; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("70e4727e-dc75-43fc-b699-15a828042823", "Container (Stock)"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
