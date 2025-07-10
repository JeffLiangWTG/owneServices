using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Module
{
	public class AgentsOfAllocationRouteFilter : ModuleGuidPivotFilter
	{
		readonly ZString parentPKColumn;
		public AgentsOfAllocationRouteFilter(ZString description, IBusinessObjectCollection collection, Type parentType, ZString parentPKColumn)
			: base(description,
				  ModuleIDs.Organisation,
				  AllocationRouteAgentPivotSchema.ARA_OH_Agent,
				  AllocationRouteAgentPivotSchema.ARA_RCA_AllocationLine,
				  collection,
				  parentType,
				  typeof(AllocationRouteAgentPivot))
		{
			this.parentPKColumn = parentPKColumn;
		}

		protected override string ParentPkColumnNameForAllMatch => parentPKColumn;
	}
}
