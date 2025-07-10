using System;
using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business.DocumentScanning;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(JobCampaignData),
	Enterprise.Core.Constants.DocManagerCodes.JobCampaign)]

namespace Enterprise.Recruiter.Business.DocumentScanning
{
	public class JobCampaignData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(HRRecruitmentJobCampaign); } }
		protected override Type CollectionType
		{
			get { return typeof(HRRecruitmentJobCampaignCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new HRRecruitmentJobCampaignCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.HRJobOpenings; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("85c7c7de-6352-45a4-afca-45808548ca78", "Job Openings"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
