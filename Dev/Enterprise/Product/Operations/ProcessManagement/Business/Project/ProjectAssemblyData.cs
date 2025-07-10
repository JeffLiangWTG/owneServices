using System;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ProjectAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.Project)]

namespace Enterprise.ProcessManagement.Business
{
	class ProjectAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(Project); }
		}

		protected override Type CollectionType
		{
			get { return typeof(ProjectCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ProjectCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Project; }
		}

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return Project.SingularName; }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
