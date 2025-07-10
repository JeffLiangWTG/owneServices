using System.Collections.ObjectModel;
using Enterprise.DeniedPartyScreening.Business;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class GenericMatchWinModelTest : TestCase
	{
		public void TestBasicProperty()
		{
			var model = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", new ObservableCollection<ScreenedDeniedItemWinModel>());
			CombineAssertions(() =>
			{
				AssertEquals("Other Screened Party", model.OtherScreenedPartyHeader);
				AssertEquals("Other Denied Party", model.OtherDeniedPartyHeader);
				AssertEquals("Screened Party", model.ScreenedPartyHeader);
				AssertEquals("Denied Party", model.DeniedPartyHeader);
				AssertEquals("Show More (0)", model.InnerExpanderTitle);
				AssertEquals(0, model.ScreenedDeniedItemsCount);
				AssertEquals(0, model.OtherScreenedDeniedItemsCount);
				AssertEquals(0, model.ScreenedDeniedItems.Count);
				AssertEquals(0, model.OtherScreenedDeniedItems.Count);
				AssertEquals("Address", model.OddName);
				AssertEquals("Addresses", model.PluralName);
				AssertEquals(false, model.IsExpanderEnabled);
				AssertEquals(false, model.IsExpanderExpanded);
				AssertEquals(false, model.IsInnerExpanderExpanded);
				AssertEquals("No Addresses Matched", model.ExpanderDescription);
			});
		}

		public void TestOnlyHaveHighMatch()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("Test", "Test", "Exact", ScoreGrades.High),
			};

			var model = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", new ObservableCollection<ScreenedDeniedItemWinModel>(matches));
			CombineAssertions(() =>
			{
				AssertEquals("Show More (0)", model.InnerExpanderTitle);
				AssertEquals(1, model.ScreenedDeniedItemsCount);
				AssertEquals(0, model.OtherScreenedDeniedItemsCount);
				AssertEquals(1, model.ScreenedDeniedItems.Count);
				AssertEquals(0, model.OtherScreenedDeniedItems.Count);
				AssertEquals(true, model.IsExpanderEnabled);
				AssertEquals("1 Address Matched", model.ExpanderDescription);
				AssertEquals(true, model.IsExpanderExpanded);
			});
		}

		public void TestOnlyHaveLowMatch()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("Test", "Test", "None", ScoreGrades.Low),
			};

			var model = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", new ObservableCollection<ScreenedDeniedItemWinModel>(matches));
			CombineAssertions(() =>
			{
				AssertEquals("Show More (1)", model.InnerExpanderTitle);
				AssertEquals(0, model.ScreenedDeniedItemsCount);
				AssertEquals(1, model.OtherScreenedDeniedItemsCount);
				AssertEquals(0, model.ScreenedDeniedItems.Count);
				AssertEquals(1, model.OtherScreenedDeniedItems.Count);
				AssertEquals(true, model.IsExpanderEnabled);
				AssertEquals("No Addresses Matched", model.ExpanderDescription);
				AssertEquals(false, model.IsExpanderExpanded);
			});

			model.IsInnerExpanderExpanded = true;
			AssertEquals("Show Less (1)", model.InnerExpanderTitle);
		}

		public void TestHaveHighLowMatch()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("Test", "Test", string.Empty, ScoreGrades.High),
				new ScreenedDeniedItemWinModel("Test", "Test", string.Empty, ScoreGrades.Low),
			};

			var model = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", new ObservableCollection<ScreenedDeniedItemWinModel>(matches));
			CombineAssertions(() =>
			{
				AssertEquals("Show More (1)", model.InnerExpanderTitle);
				AssertEquals(1, model.ScreenedDeniedItemsCount);
				AssertEquals(1, model.OtherScreenedDeniedItemsCount);
				AssertEquals(1, model.ScreenedDeniedItems.Count);
				AssertEquals(1, model.OtherScreenedDeniedItems.Count);
				AssertEquals(true, model.IsExpanderEnabled);
				AssertEquals("1 Address Matched", model.ExpanderDescription);
				AssertEquals(true, model.IsExpanderExpanded);
			});

			model.IsInnerExpanderExpanded = true;
			AssertEquals("Show Less (1)", model.InnerExpanderTitle);
		}

		public void TestBottomLineVisibility()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("Test1", "Test", string.Empty, ScoreGrades.High),
				new ScreenedDeniedItemWinModel("Test2", "Test", string.Empty, ScoreGrades.High),
				new ScreenedDeniedItemWinModel("Test3", "Test", string.Empty, ScoreGrades.Low),
				new ScreenedDeniedItemWinModel("Test4", "Test", string.Empty, ScoreGrades.Low),
			};

			var model = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", new ObservableCollection<ScreenedDeniedItemWinModel>(matches));
			CombineAssertions(() =>
			{
				AssertEquals(2, model.ScreenedDeniedItemsCount);
				AssertEquals(true, model.ScreenedDeniedItems[0].BottomLineVisibility);
				AssertEquals(false, model.ScreenedDeniedItems[1].BottomLineVisibility);

				AssertEquals(2, model.OtherScreenedDeniedItemsCount);
				AssertEquals(true, model.OtherScreenedDeniedItems[0].BottomLineVisibility);
				AssertEquals(false, model.OtherScreenedDeniedItems[1].BottomLineVisibility);
			});
		}

		public void TestOnlyShowHighestScoreMatches()
		{
			var matches = new ObservableCollection<ScreenedDeniedItemWinModel>()
			{
				new ScreenedDeniedItemWinModel("party1", "party1a", string.Empty, ScoreGrades.High, 100),
				new ScreenedDeniedItemWinModel("party1", "party1b", string.Empty, ScoreGrades.High, 100),
				new ScreenedDeniedItemWinModel("party1", "party1c", string.Empty, ScoreGrades.Medium, 80),
				new ScreenedDeniedItemWinModel("party1", "party1c", string.Empty, ScoreGrades.Medium, 78),
				new ScreenedDeniedItemWinModel("party2", "Test", string.Empty, ScoreGrades.Medium, 80),
				new ScreenedDeniedItemWinModel("party3", "Test", string.Empty, ScoreGrades.Low, 10),
			};

			var model = new GenericMatchWinModelForTest("Test Title", "Address", "Addresses", new ObservableCollection<ScreenedDeniedItemWinModel>(matches));
			CombineAssertions(() =>
			{
				AssertEquals(2, model.ScreenedDeniedItemsCount);
				AssertOnlyShowHighestScoreMatches("party1", 100, model.ScreenedDeniedItems[0]);
				AssertOnlyShowHighestScoreMatches("party2", 80, model.ScreenedDeniedItems[1]);

				AssertEquals(4, model.OtherScreenedDeniedItemsCount);
				AssertOnlyShowHighestScoreMatches("party1", 100, model.OtherScreenedDeniedItems[0]);
				AssertOnlyShowHighestScoreMatches("party1", 80, model.OtherScreenedDeniedItems[1]);
				AssertOnlyShowHighestScoreMatches("party1", 78, model.OtherScreenedDeniedItems[2]);
				AssertOnlyShowHighestScoreMatches("party3", 10, model.OtherScreenedDeniedItems[3]);
			});
		}

		void AssertOnlyShowHighestScoreMatches(string expectedPartyName, int expectedScore, ScreenedDeniedItemWinModel model)
		{
			AssertEquals(expectedPartyName, model.ScreenedParty);
			AssertEquals(expectedScore, model.Score);
		}
	}

	public class GenericMatchWinModelForTest : GenericMatchWinModel
	{
		public GenericMatchWinModelForTest(string expanderTitle, string oddName, string pluralName, ObservableCollection<ScreenedDeniedItemWinModel> totalScreenedDeniedItems)
		{
			ExpanderTitle = expanderTitle;
			OddName = oddName;
			PluralName = pluralName;
			TotalScreenedDeniedItems = totalScreenedDeniedItems;
		}

		public override string OddName { get; }

		public override string PluralName { get; }

		public override string ExpanderTitle { get; }

		public override ObservableCollection<ScreenedDeniedItemWinModel> TotalScreenedDeniedItems { get; }
	}
}
