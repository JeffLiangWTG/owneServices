using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(StowPlanMessageIssueCollection))]
	class StowPlanMessageIssueCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StowPlanMessageIssueCollection>
	{
		protected override StowPlanMessageIssueCollection GetCollectionToTest()
		{
			return new StowPlanMessageIssueCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StowPlanMessageIssue(ZGuid.Empty, "", "", "", "", NotificationType.Warning);
		}
	}
}
