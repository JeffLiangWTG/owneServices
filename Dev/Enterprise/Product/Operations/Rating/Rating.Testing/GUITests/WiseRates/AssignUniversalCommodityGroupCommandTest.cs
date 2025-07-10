using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Rating.GUI.Test
{
	public class AssignUniversalCommodityGroupCommandTest : RatingTestCase
	{
		public void TestAssign_WhenMappedCommodityGroup_DoNotShowQuestionDialog()
		{
			var refCommodityCodeCollection = new RefCommodityCodeCollection(Factory);
			var testRefCommodityCode = refCommodityCodeCollection.First(x => x.RH_UniversalCommodityGroup.IsEmpty);
			testRefCommodityCode.RH_UniversalCommodityGroup = "AAAA";
			Factory.Save();

			var command = new AssignUniversalCommodityGroupCommand(null);
			var assignment = command.Assign("AAAA");

			AssertNull("No dialog to show.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Universal Commodity Group has been mapped.", true, assignment);
		}

		public void TestAssign_WhenUnmappedCommodityGroup_ShowQuestionDialog_AnswerNo()
		{
			var refCommodityCodeCollection = new RefCommodityCodeCollection(Factory);
			AssertCollectionNotContains(
				"Non-existence code for test",
				"AAAA",
				refCommodityCodeCollection.Select(x => x.RH_UniversalCommodityGroup)
			);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			var command = new AssignUniversalCommodityGroupCommand(null);
			var assignmentAnswerNo = command.Assign("AAAA");

			var expectedMessage = @"The Universal Commodity Group 'AAAA' has NOT been assigned to any CW1 Commodity.
Would you like to complete the assignment?";
			AssertEquals(
				"Expected dialog message did not match",
				expectedMessage,
				UnitTestUserNotification.Instance.LastMessage.Text
			);

			Assert("No commodity assignment but rate applying should continue.", assignmentAnswerNo);
		}

		public void TestAssign_WhenUnmappedCommodityGroup_SecurityCheck()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var checkpoint = Env.Security.CommodityModify;
			checkpoint.IsAllowed = false;

			var command = new AssignUniversalCommodityGroupCommand(null);
			var assignment = command.Assign("AAAA");

			AssertEquals(
				@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Reference Files -> Commodities -> Edit",
				UnitTestUserNotification.Instance.LastMessage.Text
			);
			AssertEquals("Security check is not passed", false, assignment);
		}

		public void TestAssign_WhenUnmappedCommodityGroup_ShowRefCommodityModule()
		{
			var refCommodityCodeCollection = new RefCommodityCodeCollection(Factory);
			var testRefCommodity = refCommodityCodeCollection.First(x => x.RH_UniversalCommodityGroup.IsEmpty);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(delegate(object dialog)
			{
				var decisionProvider = ((EmbeddedModulePopup)dialog).Module_ForTest.ModuleDecisionProvider;
				// testRefCommodity is returned as a selected item from module for mapping
				decisionProvider.HandleDefaultAction(new[] { testRefCommodity });
			});

			var command = new AssignUniversalCommodityGroupCommand(null);
			var assignment = command.Assign("AAAA");

			AssertEquals("Assignment completed.", true, assignment);
			AssertEquals("Commodity group assignment failed.", "AAAA", testRefCommodity.RH_UniversalCommodityGroup);
		}

		public void TestAssign_WhenUnmappedCommodityGroup_SaveLayout()
		{
			var refCommodityCodeCollection = new RefCommodityCodeCollection(Factory);
			var commoditiesWithoutUniversal = refCommodityCodeCollection.Where(x => x.RH_UniversalCommodityGroup.IsEmpty);
			var testRefCommodity1 = commoditiesWithoutUniversal.First();
			var testRefCommodity2 = commoditiesWithoutUniversal.Skip(1).First();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(delegate(object dialog)
			{
				var refCommodityCodeModule = ((EmbeddedModulePopup)dialog).Module_ForTest;
				var decisionProvider = refCommodityCodeModule.ModuleDecisionProvider;

				decisionProvider.HandleDefaultAction(new[] { testRefCommodity1 });
				AssertEquals("Expected three active module filters after handling default action", 3, refCommodityCodeModule.FilterBusinessObject.ActiveModuleFilters.Count);
				refCommodityCodeModule.FilterBusinessObject.SaveLayout("SavedLayoutName");
				AssertEquals("Expected three active module filters after saving layout", 3, refCommodityCodeModule.FilterBusinessObject.ActiveModuleFilters.Count);
			});

			var command = new AssignUniversalCommodityGroupCommand(null);
			var assignment = command.Assign("AAAA");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(delegate(object dialog)
			{
				var refCommodityCodeModule = ((EmbeddedModulePopup)dialog).Module_ForTest;
				var decisionProvider = refCommodityCodeModule.ModuleDecisionProvider;

				decisionProvider.HandleDefaultAction(new[] { testRefCommodity1 });
				AssertEquals("Expected three active module filters after handling default action", 3, refCommodityCodeModule.FilterBusinessObject.ActiveModuleFilters.Count);
			});

			command = new AssignUniversalCommodityGroupCommand(null);
			assignment = command.Assign("BBBB");
		}
	}
}
