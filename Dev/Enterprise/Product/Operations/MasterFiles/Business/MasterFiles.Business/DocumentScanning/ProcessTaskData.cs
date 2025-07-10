using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(ProcessTaskData),
	Enterprise.Core.Constants.DocManagerCodes.ProcessTask)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	public class ProcessTaskData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ProcessTask); } }
		protected override Type CollectionType
		{
			get { return typeof(ProcessTaskCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ProcessTaskCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ProcessTasks; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("201d2c6a-e57b-4455-a9f4-dc8374759332", "Task"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
