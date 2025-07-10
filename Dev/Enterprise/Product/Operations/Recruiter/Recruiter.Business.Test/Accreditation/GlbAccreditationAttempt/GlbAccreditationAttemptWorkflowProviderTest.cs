using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationAttempt))]
	sealed class GlbAccreditationAttemptWorkflowProviderTest : WorkflowProviderTest<GlbAccreditationAttempt, GlbAccreditationAttemptProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.GlbAccreditationAttemptWorkflowDescriptorCode; }
		}

		protected override string GetPropertyNameForReleaseGroupRulesTest(BusinessObject job) => $"{nameof(GlbAccreditationAttempt.Person)}.{nameof(GlbAccreditationAttempt.Person.PER_FullName)}";

		protected override void SetPropertyValueForReleaseGroupRulesTest(BusinessObject job, string propertyName, string value)
		{
			((GlbAccreditationAttempt)job).Person.PER_FullName = value;
			job.HasChanges = true; // Wouldn't be necessary if there was a string property to set on this bizo directly...
		}
	}
}
