using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Testing
{
	[TestedType(typeof(InformationHyerLinkForm))]
	sealed class InformationHyerLinkFormTest : ZFormBasherTest
	{
		public void TestOKButton()
		{
			using (var form = new InformationHyerLinkFormTestForTesting())
			{
				form.Show();
				Application.DoEvents();
				form.OKButton.PerformClick();

				AssertEquals(DialogResult.OK, form.DialogResult);

				form.Close();
			}
		}

		#region Implement

		protected override Form GetFormToBashCore() => new InformationHyerLinkForm("test", "test");

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		#endregion

		class InformationHyerLinkFormTestForTesting : InformationHyerLinkForm
		{
			public InformationHyerLinkFormTestForTesting() : base("test", "test")
			{
			}

			public new Button OKButton => base.OKButton;
		}
	}
}
