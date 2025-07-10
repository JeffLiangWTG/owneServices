using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class MergeActionTest : TestCase
	{
		public void TestGetMergeActions()
		{
			var expectedAddActions = new List<string>() { "Add", "Ignore" };
			var addActions = MergeAction.GetAddActions();

			var updateActions = MergeAction.GetUpdateActions();
			var expectedUpdateActions = new List<string>() { "Update", "Ignore" };

			var ignoreActions = MergeAction.GetIgnoreActions();
			var expectedIgnoreActions = new List<string>() { "Ignore" };

			var addAndUpdateActions = MergeAction.GetAddAndUpdateActions();
			var expectedAddAndUpdateActions = new List<string>() { "Add", "Update", "Ignore" };

			CombineAssertions(() =>
			{
				AssertEquals("Add", addActions["ADD"]);
				AssertEquals("Ignore", addActions["IGN"]);

				AssertEquals("Update", updateActions["UPD"]);
				AssertEquals("Ignore", updateActions["IGN"]);

				AssertEquals("Ignore", ignoreActions["IGN"]);

				AssertEquals("Add", addAndUpdateActions["ADD"]);
				AssertEquals("Update", addAndUpdateActions["UPD"]);
				AssertEquals("Ignore", addAndUpdateActions["IGN"]);

				AssertSequencesEqual(expectedAddActions, addActions.Select(a => a.Value));
				AssertSequencesEqual(expectedUpdateActions, updateActions.Select(a => a.Value));
				AssertSequencesEqual(expectedIgnoreActions, ignoreActions.Select(a => a.Value));
				AssertSequencesEqual(expectedAddAndUpdateActions, addAndUpdateActions.Select(a => a.Value));
			});
		}
	}
}
