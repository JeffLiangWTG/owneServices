using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobServiceLink))]
	public sealed class JobServiceLinkTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var jobServiceLink = factory.NewWithValidTestData<JobServiceLink>();
			jobServiceLink.ESL_ParentTableCode = "WPS";
			return jobServiceLink;
		}
	}
}
