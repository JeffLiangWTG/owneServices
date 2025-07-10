using System.Linq;
using Enterprise.Core.DialogDefault;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(DialogDefaultFilterBusinessObject))]
	sealed class DialogDefaultFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DialogDefaultFilterBusinessObject();
		}

		//Fails when you have any other defaults in your DB
		public void TestAppliesToSimilarDialogs()
		{
			AssertEquals("PRE: Needs empty StmDialogDefault table. If this fails locally that is ok", 0, Factory.GetDatabaseCount(typeof(StmDialogDefault)));
			var withContext = Factory.NewWithValidTestData<StmDialogDefault>();
			withContext.SDD_Context = new byte[] { 1, 2, 3 };

			var without = Factory.NewWithValidTestData<StmDialogDefault>();
			without.SDD_Context = null;

			Factory.Save();

			var filterBusinessObject = (DialogDefaultFilterBusinessObject)GetNewFilterStripBusinessObject();
			var appliesToSimilarFilter = (ModuleFlagsFilter)(filterBusinessObject.ModuleFilters["Applies to Similar Dialogs"]);

			CombineAssertions(() =>
			{
				appliesToSimilarFilter.Property0 = false;
				var matchingItems = Factory.Load<StmDialogDefault>(appliesToSimilarFilter.Query);
				AssertEquals("There should only be the one with a context", 1, matchingItems.Length);
				AssertEquals("Should get the default with a context", withContext.PK, matchingItems.Single().PK);

				appliesToSimilarFilter.Property0 = true;
				matchingItems = Factory.Load<StmDialogDefault>(appliesToSimilarFilter.Query);
				AssertEquals("Should should only be one without a context", 1, matchingItems.Length);
				AssertEquals("Should get the default with no context", without.PK, matchingItems.Single().PK);
			});
		}
	}
}
