using System;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobOpeningsController))]
	public class HRJobCampaignControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(HRRecruitmentJobCampaign);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.HRJobOpenings;
		}
	}
}
