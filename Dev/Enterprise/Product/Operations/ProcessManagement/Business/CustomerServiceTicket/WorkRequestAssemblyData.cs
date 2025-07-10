using System;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(WorkRequestAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.CustomerServiceTicket)]

namespace Enterprise.ProcessManagement.Business
{
	class WorkRequestAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(WorkRequest);

		protected override Type CollectionType => typeof(WorkRequestCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WorkRequestCollection(factory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.CustomerServiceTicket;

		public override string ReferenceType => Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow;

		public override MultilingualString HumanReadableName => WorkRequest.SingularName;

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
