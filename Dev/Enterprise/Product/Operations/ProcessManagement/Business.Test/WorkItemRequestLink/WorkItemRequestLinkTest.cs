using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemRequestLink))]
	class WorkItemRequestLinkTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return ProcessMgmtTestHelper.CreateWorkItemRequestLink(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return ProcessMgmtTestHelper.CreateWorkItemRequestLink(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return ProcessMgmtTestHelper.CreateWorkItemRequestLink(Factory);
		}
	}
}
