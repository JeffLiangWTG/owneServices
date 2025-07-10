using System;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class HRRecruitmentJobCampaignWorkflowDescriptor : WorkflowDescriptor
	{
		public const string WorkflowTypeCode = "HRJ";

		public override string Code
			=> WorkflowTypeCode;

		public override IMultilingualString Description
			=> ResString.GetMultilingualString("MasterFiles|HRRecruitmentJobCampaignWorkflowDescriptor|Description", "Human Resources Job Opening");

		public override bool RequiresClient
			=> false;

		public override bool AreTasksCompanySpecific
			=> false;

		public override Type WorkflowProviderType
			=> ObjectFactory.GetType<IHRRecruitmentJobCampaign>();

		public override ControllerID ControllerID
			=> ControllerIDs.HRJobOpenings;

		public override bool SupportsBufferManagement
			=> false;
	}
}
