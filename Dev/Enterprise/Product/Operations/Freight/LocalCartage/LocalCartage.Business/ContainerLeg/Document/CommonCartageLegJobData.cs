using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(CommonCartageLegJobData), Constants.DocManagerCodes.LocalTransportLeg)]

namespace Enterprise.Freight.LocalCartage.Business
{
	class CommonCartageLegJobData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CommonCartageLeg); } }
		protected override Type CollectionType
		{
			get { return typeof(CommonCartageLegCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CommonCartageLegCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.CartageLeg; } }
		public override string ReferenceType { get { return Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("09d5e3bb-9e7a-4de7-9767-3f64b7fd4c92", "Port Transport Leg"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
