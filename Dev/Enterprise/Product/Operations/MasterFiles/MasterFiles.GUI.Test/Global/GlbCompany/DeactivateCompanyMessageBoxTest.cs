using System.Linq;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DeactivateCompanyMessageBoxTest : TestCase
	{
		public void TestDeactivateCompanyMessageBox()
		{
			using (var messageBox = new DeactivateCompanyMessageBox())
			{
				var button = (ZButton)messageBox.Controls.Find("YESButton", true).Single();
				AssertEquals("Deactivate", button.Text);
				AssertEquals(System.Drawing.Color.Red, button.BackColor);
			}
		}
	}
}
