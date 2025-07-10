using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class ExpanderWinModelTest : TestCase
	{
		public void TestExpanderWinModel()
		{
			var expanderWinModelForTest = new ExpanderWinModelForTest("Test", "Test Description", true, false);

			CombineAssertions(() =>
			{
				AssertEquals("Test", expanderWinModelForTest.ExpanderTitle);
				AssertEquals("Test Description", expanderWinModelForTest.ExpanderDescription);
				AssertEquals(true, expanderWinModelForTest.IsExpanderEnabled);
				AssertEquals(false, expanderWinModelForTest.IsExpanderExpanded);
			});
		}
	}

	public class ExpanderWinModelForTest : ExpanderWinModel<ExpanderWinModelForTest>
	{
		public ExpanderWinModelForTest(string expanderTitle, string expanderDescription, bool isEnabled, bool isExpanderExpanded)
		{
			ExpanderTitle = expanderTitle;
			ExpanderDescription = expanderDescription;
			IsExpanderEnabled = isEnabled;
			IsExpanderExpanded = isExpanderExpanded;
		}

		public override string ExpanderTitle { get; }

		public override string ExpanderDescription { get; }

		public override bool IsExpanderEnabled { get; }

		public override sealed bool IsExpanderExpanded { get; set; }
	}
}
