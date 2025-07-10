using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class TasksModuleFilter : ProcessTasksModuleFilter
	{
		protected TasksModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public TasksModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, ModuleIDs.ProcessTasks, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
		}

		public TasksModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, ModuleIDs.ProcessTasks, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
		}

		protected override FilterStripBusinessObject GetNewSelectedFilters(StmModuleFilter layoutToLoad)
		{
			var filterBizo = (ProcessTaskFilterBusinessObject)base.GetNewSelectedFilters(layoutToLoad);
			filterBizo.ShouldAddNonTemplateFilter = ParentBusinessObjectType != typeof(ProcessTaskTemplate);
			return filterBizo;
		}
	}
}
