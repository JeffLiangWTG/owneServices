using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test.Organisation.OrgMatchApproval
{
	public class SimilarOrgMatchesModuleButtonGridTest : TestCaseWithDummyOrgMatchApproval
	{
		public void TestModuleID()
		{
			using (SimilarOrgMatchesModuleButtonGrid grid = new SimilarOrgMatchesModuleButtonGrid())
			{
				AssertEquals("ModuleID", ModuleIDs.Organisation, grid.ModuleID);
			}
		}

		public void TestNameOfAGridElement()
		{
			using (SimilarOrgMatchesModuleButtonGrid grid = new SimilarOrgMatchesModuleButtonGrid())
			{
				AssertEquals("NameOfAGridElement", "Similar Organization", grid.NameOfAGridElement.Caption);
			}
		}

		public void TestEdit()
		{
			OrgHeader similarOrgMatch = CreateSimilarOrgMatch();
			Factory.Save();
			AssertEquals("There should be 1 similar organisation for the test", 1, DummyMatchApproval.SimilarOrgMatchesSortedByRank.Count);

			using (ZForm form = new ZForm(DummyMatchApproval))
			using (TestSimilarOrgMatchesModuleButtonGrid grid = new TestSimilarOrgMatchesModuleButtonGrid())
			{
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();
				grid.InnerGrid.SetDataBinding(DummyMatchApproval, "SimilarOrgMatchesSortedByRank");

				grid.Edit(null, null); // expect no exception
				grid.Edit(DummyMatchApproval.SimilarOrgMatchesSortedByRank[0], null);
				AssertNotNull("Should show the edit form successfully", grid.LastShownZForm);
				grid.LastShownZForm.Dispose();
			}
		}

		public void TestColumnsNotSerializedByDesignerForContainingControl()
		{
			AssertEquals(true, typeof(SimilarOrgMatchesModuleButtonGrid).IsSubclassOf(typeof(ZModuleButtonGridWithoutColumnStylesSerialisation)));
		}

		class TestSimilarOrgMatchesModuleButtonGrid : SimilarOrgMatchesModuleButtonGrid
		{
			public new void Edit(BusinessObject selected, object sender)
			{
				base.Edit(selected, sender);
			}
		}
	}

	[TestedType(typeof(SimilarOrgMatchesModuleButtonGrid))]
	class SimilarOrgMatchesModuleButtonGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
	{
	}
}
