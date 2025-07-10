using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(JobShipmentPrePlanningData),
	Enterprise.Core.Constants.DocManagerCodes.JobShipmentPrePlanning)]

namespace Enterprise.Freight.Forwarding.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Freight.Forwarding.Orders.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	class JobShipmentPrePlanningData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(JobShipmentPreplanning); } }
		protected override Type CollectionType
		{
			get { return typeof(JobShipmentPreplanningCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new JobShipmentPreplanningCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.JobShipmentPreplanning; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("d2443f7d-d61e-4d6a-9d36-113217851a6e", "Shipment Pre Advice"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
