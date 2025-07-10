using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(StaffData),
	Enterprise.Core.Constants.DocManagerCodes.Staff)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class StaffData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GlbStaff); } }
		protected override Type CollectionType
		{
			get { return typeof(GlbStaffCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new GlbStaffCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GlbStaff; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("a0b2aabd-ec59-463e-b94d-f26da798d6ba", "Staff"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
