using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Testing
{
	[TestedType(typeof(AlertHyperLinkForm))]
	sealed class AlertHyperLinkFormTest : ZFormBasherTest
	{
		public void TestOKButton()
		{
			using (var form = new AlertHyperLinkFormForTesting())
			{
				form.Show();
				Application.DoEvents();
				form.OKButtonForTesting.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				form.Close();
				form.Dispose();
			}
		}

		public void TestCancelBtn()
		{
			using (var form = new AlertHyperLinkFormForTesting())
			{
				form.Show();
				Application.DoEvents();
				form.CancelBtnForTesting.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
				form.Close();
				form.Dispose();
			}
		}

		#region Implement

		protected override Form GetFormToBashCore() => new AlertHyperLinkForm();

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		#endregion

		class AlertHyperLinkFormForTesting : AlertHyperLinkForm
		{
			public AlertHyperLinkFormForTesting()
				: base()
			{
			}

			public Button OKButtonForTesting => OKButton;
			public Button CancelBtnForTesting => CancelBtn;

			public Label Label1 => ShowLabel1;
			public ZLinkLabel LinkLabel1 => HyperlinkLabel;
			public Label Label2 => ShowLabel2;
		}
	}
}
