using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class BrandsModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var model = new BrandsModel("WiseTech Global");

			AssertEquals("WiseTech Global", model.RelatedBrandOrCompanyName);
		}

		public void TestDefaultMergeAction()
		{
			var model1 = new BrandsModel("WiseTech Global");
			var model2 = new BrandsModel(string.Empty);

			CombineAssertions(() =>
			{
				AssertEquals(MergeAction.Codes.Add, model1.SelectedMergeAction);
				AssertEquals(MergeAction.Codes.Add, model2.SelectedMergeAction);
			});
		}

		public void TestMergeActions()
		{
			var expectedActions = new List<string>() { "Add", "Ignore" };

			var model1 = new BrandsModel("WiseTech Global");
			var model2 = new BrandsModel(string.Empty);

			CombineAssertions(() =>
			{
				AssertSequencesEqual(expectedActions, model1.MergeActions.Select(m => m.Value));
				AssertSequencesEqual(expectedActions, model2.MergeActions.Select(m => m.Value));
			});
		}
	}
}
