using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class InnerExpanderWinModelTest : TestCase
	{
		public void TestInnerExpanderWinModel()
		{
			var expanderWinModelForTest = new InnerExpanderWinModelForTest("Test", "Test Description", true, "InnerTitle");

			CombineAssertions(() =>
			{
				AssertEquals("Test", expanderWinModelForTest.ExpanderTitle);
				AssertEquals("Test Description", expanderWinModelForTest.ExpanderDescription);
				AssertEquals(true, expanderWinModelForTest.IsExpanderEnabled);
				AssertEquals("InnerTitle", expanderWinModelForTest.InnerExpanderTitle);
			});
		}
	}

	public class InnerExpanderWinModelForTest : InnerExpanderWinModel<InnerExpanderWinModelForTest>
	{
		public InnerExpanderWinModelForTest(string expanderTitle, string expanderDescription, bool isEnabled, string innerExpanderTitle)
		{
			ExpanderTitle = expanderTitle;
			ExpanderDescription = expanderDescription;
			IsExpanderEnabled = isEnabled;
			InnerExpanderTitle = innerExpanderTitle;
		}

		public override string ExpanderTitle { get; }
		public override string ExpanderDescription { get; }
		public override bool IsExpanderEnabled { get; }
		public override string InnerExpanderTitle { get; }
	}
}
