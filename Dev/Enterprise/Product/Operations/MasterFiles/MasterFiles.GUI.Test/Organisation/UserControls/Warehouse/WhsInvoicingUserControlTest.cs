using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test.Organisation.UserControls.Warehouse
{
	public class WhsInvoicingUserControlTest : TestCaseWithFactory
	{
		public void TestSerialNumberIsKeyCheckBox()
		{
			using (var form = new WhsInvoicingTestForm())
			{
				form.Show();
				var autoRatingGroupBoxControl = form.UserControl.FindSingle<ZGroupBox>("AutoRatingGroupBox");
				var serialNumberIsKeyCheckBox = form.UserControl.FindSingle<ZCheckBox>("SerialNumberIsKeyCheckBox");

				AssertEquals(true, serialNumberIsKeyCheckBox.Visible);
			}
		}

		#region TestForm

		protected class WhsInvoicingTestForm : ZForm
		{
			public WhsInvoicingTestForm()
				: base()
			{
			}

			public WhsInvoicingUserControl UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = GetNewOrderEntryUserControl();
				this.Controls.Add(this.UserControl);
			}

			protected WhsInvoicingUserControl GetNewOrderEntryUserControl()
			{
				return new WhsInvoicingUserControl();
			}
		}

		#endregion
	}
}
