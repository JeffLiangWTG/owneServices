using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ProcessTasksModuleFilterForTest : ProcessTasksModuleFilter
	{
		public ProcessTasksModuleFilterForTest(FilterCategory category, ModuleFilterCollection parentCollection) : base(category, parentCollection)
		{
		}

		public ProcessTasksModuleFilterForTest(ZString description, ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType) : base(description, moduleID, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
		}

		public ProcessTasksModuleFilterForTest(ZString description, ModuleIdentifier moduleID, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType) : base(description, moduleID, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
		}
	}
}
