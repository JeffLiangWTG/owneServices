using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CFSShipmentReceivalData),
	Enterprise.Core.Constants.DocManagerCodes.CFSShipmentReceival)]

namespace Enterprise.Freight.CFS.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	class CFSShipmentReceivalData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CFSShipment); } }
		protected override Type CollectionType
		{
			get { return typeof(CFSShipmentList); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CFSShipmentList(factory);
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ShipmentReceival; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("94a35039-d313-49cf-aab2-9a0400e6ce5c", "CFS Shipment Receival"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
