using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class SailingTemplateCopyDialogBasherTest : BaseFreightTest
	{
		#region TestGetTemplateCopy

		public void TestGetTemplateCopy()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			JobVoyage voyageCopy;
			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			voyageCopy = SailingTemplateCopyDialog.GetTemplateCopy(voyage);

			AssertNotNull("Should have shown a dialog", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should have shown the correct form", typeof(SailingTemplateCopyDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertNull("Canceled, should return null", voyageCopy);
			AssertEquals("Existing voyage should have no changes", false, voyage.HasChanges);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			voyageCopy = SailingTemplateCopyDialog.GetTemplateCopy(voyage);

			AssertNotNull("Should have shown a dialog", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should have shown the correct form", typeof(SailingTemplateCopyDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertNotNull("Should have returned a BusnissObject", voyageCopy);
			AssertNotEquals("The voyage returned should be distinct from the voyage passed in", voyage.PK, voyageCopy.PK);
			AssertEquals("The voyage returned should not be saved yet", false, voyageCopy.IsInDatabase);
			AssertEquals("Existing voyage should have no changes", false, voyage.HasChanges);
		}

		#endregion
	}
}
