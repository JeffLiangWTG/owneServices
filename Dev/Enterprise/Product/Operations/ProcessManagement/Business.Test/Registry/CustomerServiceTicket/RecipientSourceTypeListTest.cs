using System.Linq;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	class RecipientSourceTypeListTest : TestCase
	{
		public void TestList_ShouldContainValidElementsOnly()
		{
			var list = new RecipientSourceTypeList();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup,
				RecipientSourceTypeList.Codes.LastCompletedTaskResource,
				RecipientSourceTypeList.Codes.NotificationGroup,
			}, list.Cast<ICodeDescription>().Select(x => x.Code));
		}
	}
}
