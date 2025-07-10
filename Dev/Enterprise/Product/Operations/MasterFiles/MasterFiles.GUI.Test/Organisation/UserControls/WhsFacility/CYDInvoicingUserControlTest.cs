using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing.Organisation.UserControls.WhsFacility
{
	public class CYDInvoicingUserControlTest : TestCaseWithFactory
	{
		public void TestAutoRatingControls()
		{
			using (var form = new CYDInvoicingTestForm())
			{
				form.Show();
				var autoRatingGroupBoxControl = form.UserControl.FindSingle<ZGroupBox>("AutoRatingGroupBox");
				var ratingPeriodDropEdit = form.UserControl.FindSingle<ZDropEdit>("RatingPeriodDropEdit");
				var storageCalcMethodDropEdit = form.UserControl.FindSingle<ZDropEdit>("StorageCalcMethodDropEdit");
				var freeStorageDaysGroupBox = form.UserControl.FindSingle<ZGroupBox>("FreeStorageDaysGroupBox");
				var freeStorageDaysGrid = form.UserControl.FindSingle<ZGrid>("FreeStorageDaysGrid");

				AssertEquals(true, autoRatingGroupBoxControl.Visible);
				AssertEquals(true, ratingPeriodDropEdit.Visible);
				AssertEquals(true, storageCalcMethodDropEdit.Visible);
				AssertEquals(true, freeStorageDaysGroupBox.Visible);
				AssertEquals(true, freeStorageDaysGrid.Visible);
			}
		}

		#region TestForm

		protected class CYDInvoicingTestForm : ZForm
		{
			public CYDInvoicingTestForm()
				: base()
			{
			}

			public CYDInvoicingUserControl UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = GetNewCYDInvoicingUserControl();
				this.Controls.Add(this.UserControl);
			}

			CYDInvoicingUserControl GetNewCYDInvoicingUserControl()
			{
				return new CYDInvoicingUserControl();
			}
		}

		#endregion
	}
}
