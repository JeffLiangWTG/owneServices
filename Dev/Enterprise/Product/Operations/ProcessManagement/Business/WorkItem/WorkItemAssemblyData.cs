using System;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(WorkItemAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.WorkItem)]

namespace Enterprise.ProcessManagement.Business
{
	class WorkItemAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(WorkItem); }
		}

		protected override Type CollectionType
		{
			get { return typeof(WorkItemCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WorkItemCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WorkItem; }
		}

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return WorkItem.SingularName; }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
