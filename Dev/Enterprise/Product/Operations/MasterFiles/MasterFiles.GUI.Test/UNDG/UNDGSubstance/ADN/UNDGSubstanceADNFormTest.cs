using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UNDGSubstanceADNForm))]
	sealed class UNDGSubstanceADNFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new UNDGSubstanceADNForm(Factory.New<UNDGSubstanceADN>());
		}

		#endregion

		[RequiresSTA]
		public void TestAllowNew()
		{
			using (var form = GetFormToBash())
			{
				Assert("Not allowed New display mode", !((IPostingButtonsProvider)form).AllowNew);
			}
		}

		[RequiresSTA]
		public void TestBorderPanel()
		{
			using var control = new UNDGSubstanceADNControl();
			var horizontalDivider = control.Controls.Find("DG_BorderPanel", searchAllChildren: true)?.FirstOrDefault() as ZPanel;
			AssertNotNull(horizontalDivider);
			AssertEquals("DG_BorderPanel", horizontalDivider.Name);
			AssertEquals(new Size(820, 1), horizontalDivider.Size);
			AssertEquals(new Point(60, 40), horizontalDivider.Location);
			AssertEquals(Color.DarkGray, horizontalDivider.BackColor);
		}
	}
}
