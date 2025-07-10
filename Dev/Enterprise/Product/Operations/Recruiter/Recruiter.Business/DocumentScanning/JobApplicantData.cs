using System;
using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business.DocumentScanning;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(JobApplicantData),
	Enterprise.Core.Constants.DocManagerCodes.JobApplicant)]

namespace Enterprise.Recruiter.Business.DocumentScanning
{
	public class JobApplicantData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(HRJobApplicant); } }
		protected override Type CollectionType
		{
			get { return typeof(HRJobApplicantCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new HRJobApplicantCollection(factory);
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.HRJobApplicant; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("acab8edb-2643-414d-8228-be0fd2a09a6f", "Job Applicant"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
