using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class MilestonesModuleFilter : ProcessTasksModuleFilter
	{
		protected MilestonesModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public MilestonesModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, ModuleIDs.WorkflowMilestones, primaryKeyColumn, ProcessTasksSchema.P9_ParentID, list, parentBusinessObjectType)
		{
		}

		public MilestonesModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, BusinessObjectFactory factory, Type parentBusinessObjectType)
			: base(description, ModuleIDs.WorkflowMilestones, primaryKeyColumn, ProcessTasksSchema.P9_ParentID, () => new ProcessTaskCollection(factory), parentBusinessObjectType)
		{
		}
	}
}
