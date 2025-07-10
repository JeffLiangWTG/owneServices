using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class DA63PreSaveDialogStrategyTest : TestCaseWithFactory
	{
		public void TestRunPreSaveAction()
		{
			CombineAssertions("Null Source", () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var tester = new DA63PreSaveDialogStrategy(null);
				AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
				AssertEquals(ContinueWithSave.No, tester.ShowPreSaveDialogs(ContinueWithSave.No));
			});
			CombineAssertions("Dirty:No, PrevAns:Yes", () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var testInput = new DA63DirtyMarkerForTest();
				testInput.DA63NeedsRecalculation = false;
				var tester = new DA63PreSaveDialogStrategy(testInput);
				AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
				AssertEquals(false, testInput.HasBeenRefreshed);
			});
			CombineAssertions("Dirty:No, PrevAns:No", () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var testInput = new DA63DirtyMarkerForTest();
				testInput.DA63NeedsRecalculation = false;
				var tester = new DA63PreSaveDialogStrategy(testInput);
				AssertEquals(ContinueWithSave.No, tester.ShowPreSaveDialogs(ContinueWithSave.No));
				AssertEquals(false, testInput.HasBeenRefreshed);
			});
			CombineAssertions("Dirty:Yes, PrevAns:Yes, Question:No", () =>
			{
				var testNotification = UnitTestUserNotification.Instance;
				testNotification.ClearMessagesAndAnswers();
				testNotification.AddAnswer(DialogResult.No);
				var testInput = new DA63DirtyMarkerForTest();
				testInput.DA63NeedsRecalculation = true;
				var tester = new DA63PreSaveDialogStrategy(testInput);
				AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
				AssertEquals(false, testInput.HasBeenRefreshed);
				AssertEquals(DA63PreSaveDialogStrategy.ReCalculateConfimation, testNotification.LastMessage.Text);
			});
			CombineAssertions("Dirty:Yes, PrevAns:No, Question:No", () =>
			{
				var testNotification = UnitTestUserNotification.Instance;
				testNotification.ClearMessagesAndAnswers();
				testNotification.AddAnswer(DialogResult.No);
				var testInput = new DA63DirtyMarkerForTest();
				testInput.DA63NeedsRecalculation = true;
				var tester = new DA63PreSaveDialogStrategy(testInput);
				AssertEquals(ContinueWithSave.No, tester.ShowPreSaveDialogs(ContinueWithSave.No));
				AssertEquals(false, testInput.HasBeenRefreshed);
			});
			CombineAssertions("Dirty:Yes, PrevAns:Yes, Question:Yes", () =>
			{
				var testNotification = UnitTestUserNotification.Instance;
				testNotification.ClearMessagesAndAnswers();
				testNotification.AddAnswer(DialogResult.Yes);
				var testInput = new DA63DirtyMarkerForTest();
				testInput.DA63NeedsRecalculation = true;
				var tester = new DA63PreSaveDialogStrategy(testInput);
				AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
				AssertEquals(true, testInput.HasBeenRefreshed);
				AssertEquals(DA63PreSaveDialogStrategy.ReCalculateConfimation, testNotification.LastMessage.Text);
			});
			CombineAssertions("Dirty:Yes, PrevAns:No, Question:Yes", () =>
			{
				var testNotification = UnitTestUserNotification.Instance;
				testNotification.ClearMessagesAndAnswers();
				testNotification.AddAnswer(DialogResult.Yes);
				var testInput = new DA63DirtyMarkerForTest();
				testInput.DA63NeedsRecalculation = true;
				var tester = new DA63PreSaveDialogStrategy(testInput);
				AssertEquals(ContinueWithSave.No, tester.ShowPreSaveDialogs(ContinueWithSave.No));
				AssertEquals(false, testInput.HasBeenRefreshed);
			});
		}

		sealed class DA63DirtyMarkerForTest : IDA63ValueRecalculationParent
		{
			public ZBool DA63NeedsRecalculation { get; set; }

			public void RecalculateDA63Values()
			{
				HasBeenRefreshed = true;
			}

			public ZBool HasBeenRefreshed { get; set; }
		}
	}
}
