using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Receivables.Testing
{
	[TestedType(typeof(TermsUserControlFormForBash))]
	sealed class TermsUserControlBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			Factory.Save();

			return new TermsUserControlFormForBash(companyData);
		}

		internal class TermsUserControlFormForBash : ZForm
		{
			internal TermsUserControlFormForBash(OrgCompanyData companyData)
					: base(companyData)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				Controls.Add(TermsUserControl);
				BindingSource.SetBindingMember(TermsUserControl, ".");
				CaptionRenderingEnabled = true;
			}

			readonly TermsUserControl TermsUserControl = new TermsUserControl();
		}
		public void TestMultipleInstallmentGridVisibility()
		{
			using (var form = (TermsUserControlFormForBash)GetFormToBashCore())
			{
				form.Show();
				var grid = (ZGroupBox)form.Controls.Find("ARTermsInstallmentGroupBox", true)[0];
				AssertEquals("Multiple Installments grid is shown", grid.Visible, true);
			}
		}
	}
}
