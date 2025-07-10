using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRHiringRequest))]
	sealed class HRHiringRequestWorkflowProviderTest : WorkflowProviderTest<HRHiringRequest, HRHiringRequestProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.HRHiringRequestDescriptorCode; }
		}

		protected override string GetPropertyNameForReleaseGroupRulesTest(BusinessObject job) => $"{nameof(HRHiringRequest.JobApplicant)}.{nameof(HRHiringRequest.JobApplicant.Email)}";

		protected override void SetPropertyValueForReleaseGroupRulesTest(BusinessObject job, string propertyName, string value)
		{
			((HRHiringRequest)job).JobApplicant.HA_EmailAddress = value;
			job.HasChanges = true;
		}
	}
}
