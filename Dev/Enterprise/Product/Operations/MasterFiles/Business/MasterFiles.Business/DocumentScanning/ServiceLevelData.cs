using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(ServiceLevelData),
	Enterprise.Core.Constants.DocManagerCodes.ServiceLevel)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class ServiceLevelData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefServiceLevel); } }
		protected override Type CollectionType
		{
			get { return typeof(RefServiceLevelCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefServiceLevelCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ServiceLevel; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("4c7a4c8f-d874-4ced-97fc-eabbe25988c2", "Service Levels"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
