using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class DocumentNumbersUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new DocumentNumbersUserControl())
			{
				var ctrControl1 = control.Controls.Find("DocumentNumberTextBox", true).FirstOrDefault();
				AssertNotNull(ctrControl1);
				var ctrControl2 = control.Controls.Find("DocumentNumbersEditButton", true).FirstOrDefault();
				AssertNotNull(ctrControl2);
			}
		}
	}
}
