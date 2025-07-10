using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	public sealed class DiscardedMessageUserControlTest : TestCaseWithFactory
	{
		public void TestMessageTextBoundTextBox()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var userControl = new DiscardedMessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var textBox = userControl.FindSingle<ZTextBox>("MessageTextBoundTextBox");
				CombineAssertions(() =>
				{
					AssertEquals("HideSelection", false, textBox.HideSelection);
					AssertEquals("EnableFindDialog", true, textBox.EnableFindDialog);
				});
			}
		}
	}
}
