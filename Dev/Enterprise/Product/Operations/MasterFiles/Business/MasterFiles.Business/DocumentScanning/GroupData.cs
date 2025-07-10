using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GroupData),
	Enterprise.Core.Constants.DocManagerCodes.Group)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class GroupData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GlbGroup); } }
		protected override Type CollectionType
		{
			get { return typeof(GlbGroupCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new GlbGroupCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GlbGroup; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("a8aec189-58af-49d3-b170-9201ae13fba2", "Group"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
