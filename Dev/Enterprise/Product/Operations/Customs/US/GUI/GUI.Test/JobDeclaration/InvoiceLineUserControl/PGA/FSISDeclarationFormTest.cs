using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(FSISDeclarationForm))]
	sealed class FSISDeclarationFormTest : ZFormBasherTest
	{
		public void TestOKButtonClick()
		{
			using (var form = new FSISDeclarationForm(Factory.New<JobDeclaration>(), ""))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			var form = new FSISDeclarationForm(declaration, "123456");
			form.FSISCertificatesGrid.ColumnStyles.RemoveAt(form.FSISCertificatesGrid.ColumnStyles.Count - 2); //Remove SealNumberColumnStyle to avoid prompt to [List] tag it.
			return form;
		}
	}
}
