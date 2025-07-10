using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class SourceListNamesWinModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CommonTestDataHelper.CreateComplianceList(Factory, "AAA");
			CommonTestDataHelper.CreateComplianceList(Factory, "BBB");
			CommonTestDataHelper.CreateComplianceList(Factory, "CCC", isExcluded: true);
			CommonTestDataHelper.CreateComplianceList(Factory, "DDD", isExcluded: true);
			Factory.Save();

			var model = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA", "BBB", "CCC" }));

			AssertEquals("Included list contains 2 entry", 2, model.IncludedSourceListNames.Count);
			AssertContainsExactElementsInAnyOrder(new[] { ("AAA Name", "AAA Description"), ("BBB Name", "BBB Description") }, model.IncludedSourceListNames.Select(u => (u.Name, u.Description)));

			AssertEquals("Excluded list contains 1 entry", 1, model.ExcludedSourceListNames.Count);
			AssertContainsExactElementsInAnyOrder(new[] { ("CCC Name", "CCC Description") }, model.ExcludedSourceListNames.Select(u => (u.Name, u.Description)));

			model.IsInnerExpanderExpanded = true;
			AssertEquals("Hide Excluded (1)", model.InnerExpanderTitle);

			model.IsInnerExpanderExpanded = false;
			AssertEquals("Show Excluded (1)", model.InnerExpanderTitle);

			model = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA", "BBB", "CCC" }, true));

			AssertEquals("Included list contains 3 entry", 3, model.IncludedSourceListNames.Count);
			AssertContainsExactElementsInAnyOrder(new[] { ("AAA Name", "AAA Description"), ("BBB Name", "BBB Description"), ("CCC Name", "CCC Description") }, model.IncludedSourceListNames.Select(u => (u.Name, u.Description)));

			AssertEquals("Excluded list contains 0 entry", 0, model.ExcludedSourceListNames.Count);
		}

		public void TestIncludedNotImportedSourceListCodes()
		{
			CommonTestDataHelper.CreateComplianceList(Factory, "AAA");
			CommonTestDataHelper.CreateComplianceList(Factory, "BBB", isExcluded: true);
			Factory.Save();

			var model = new SourceListNamesWinModel(new SourceListNamesModel(Factory, new[] { "AAA", "BBB", "CCC" }));

			AssertEquals("Included list contains 2 entry", 2, model.IncludedSourceListNames.Count);
			AssertContainsExactElementsInAnyOrder(new[] { ("AAA Name", "AAA Description"), ("CCC", string.Empty) }, model.IncludedSourceListNames.Select(u => (u.Name, u.Description)));
		}

		public void TestConstructorArgumentNull()
		{
			AssertExceptionThrown<ArgumentException>(() => _ = new SourceListNamesWinModel(null));
		}

		public void TestExpanderWinModel()
		{
			var model = new SourceListNamesWinModel(new SourceListNamesModel(Factory, Array.Empty<string>()));
			CombineAssertions(() =>
			{
				AssertEquals("Source List Names", model.ExpanderTitle);
				AssertEquals(string.Empty, model.ExpanderDescription);
				AssertEquals(true, model.IsExpanderEnabled);
			});
		}
	}
}
