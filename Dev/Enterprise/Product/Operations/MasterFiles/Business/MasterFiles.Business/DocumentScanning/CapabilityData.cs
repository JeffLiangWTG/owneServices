using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CapabilityData),
	Enterprise.Core.Constants.DocManagerCodes.Capability)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class CapabilityData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GlbCapability); } }
		protected override Type CollectionType
		{
			get { return typeof(GlbCapabilityCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new GlbCapabilityCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GlbCapability; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("463357f4-ec79-40f7-a90b-2756526f083c", "Capability"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
