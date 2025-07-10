using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(StowPlanMessageIssue))]
	class StowPlanMessageIssueTest : NonPersistentBusinessObjectTestCase
	{
		public void TestErrorTypeAndMoreDetail()
		{
			var issue = new StowPlanMessageIssue(ZGuid.Empty, "", "", "", "More details", NotificationType.Warning);
			AssertEquals("Warning", issue.ErrorType);
			AssertEquals("More details", issue.MoreDetail);
			issue = new StowPlanMessageIssue(ZGuid.Empty, "", "", "", "", NotificationType.Error);
			AssertEquals("Message Error", issue.ErrorType);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new StowPlanMessageIssue(ZGuid.Empty, "", "", "", "", NotificationType.Warning);
		}
	}
}
