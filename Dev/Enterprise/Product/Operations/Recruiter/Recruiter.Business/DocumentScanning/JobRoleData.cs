using System;
using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business.DocumentScanning;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(JobRoleData),
	Enterprise.Core.Constants.DocManagerCodes.JobRole)]

namespace Enterprise.Recruiter.Business.DocumentScanning
{
	public class JobRoleData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(HRJobRole); } }
		protected override Type CollectionType
		{
			get { return typeof(HRJobRoleCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new HRJobRoleCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.HRJobRole; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("827bdca6-8eee-4942-88ff-bec93adb039f", "Job Role"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
