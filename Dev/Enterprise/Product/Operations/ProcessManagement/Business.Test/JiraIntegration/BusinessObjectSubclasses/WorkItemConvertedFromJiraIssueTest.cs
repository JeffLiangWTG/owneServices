using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkItemConvertedFromJiraIssue))]
	class WokrItemConvertedFromJiraIssueTest : EnterpriseBusinessObjectTestCase
	{
	}

	[TestedType(typeof(WorkItemConvertedFromJiraIssue))]
	class WorkItemConvertedFromJiraIssueRelatedItemSourceTest : WorkTaskRelatedItemSourceTestCase
	{
	}

	[TestedType(typeof(WorkItemConvertedFromJiraIssue))]
	class WorkItemConvertedFromJiraIssueRelatedItemTest : WorkItemRelatedItemTestCase
	{
	}
}
