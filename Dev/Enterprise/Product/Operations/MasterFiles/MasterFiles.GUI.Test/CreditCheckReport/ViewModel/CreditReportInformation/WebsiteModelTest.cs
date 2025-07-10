using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class WebsiteModelTest : TestCase
	{
		public void TestConstructor()
		{
			var model = new WebsiteModel("Wisetech99.com", "Wisetech.com");

			CombineAssertions(() =>
			{
				AssertEquals("Wisetech99.com", model.OriginalMainUrl);
				AssertEquals("Wisetech.com", model.NewUrl);
			});
		}

		public void TestDefaultMergeAction()
		{
			var model1 = new WebsiteModel("Wisetech99.com", "Wisetech.com");
			var model2 = new WebsiteModel("Wisetech.com", "Wisetech.com");

			CombineAssertions(() =>
			{
				AssertEquals(MergeAction.Codes.Add, model1.SelectedMergeAction);
				AssertEquals(MergeAction.Codes.Ignore, model2.SelectedMergeAction);
			});
		}

		public void TestMergeActions()
		{
			var expectedActions1 = new List<string>() { "Add", "Ignore" };
			var expectedActions2 = new List<string>() { "Ignore" };

			var model1 = new WebsiteModel("Wisetech99.com", "Wisetech.com");
			var model2 = new WebsiteModel("Wisetech.com", "Wisetech.com");

			CombineAssertions(() =>
			{
				AssertSequencesEqual(expectedActions1, model1.MergeActions.Select(m => m.Value));
				AssertSequencesEqual(expectedActions2, model2.MergeActions.Select(m => m.Value));
			});
		}
	}
}
