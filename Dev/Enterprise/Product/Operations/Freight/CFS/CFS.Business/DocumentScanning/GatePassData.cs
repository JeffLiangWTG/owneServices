using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GatePassData),
	Enterprise.Core.Constants.DocManagerCodes.GatePass)]

namespace Enterprise.Freight.CFS.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	class GatePassData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GatePassShipment); } }
		protected override Type CollectionType
		{
			get { return typeof(GatePassShipmentList); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new GatePassShipmentList(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ShipmentGatePass; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("4dc449a7-d852-45bd-8a16-0da2eeb5065a", "Gate Pass"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
