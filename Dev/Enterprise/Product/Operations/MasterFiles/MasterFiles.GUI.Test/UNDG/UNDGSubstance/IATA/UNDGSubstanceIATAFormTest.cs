using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UNDGSubstanceIATAForm))]
	sealed class UNDGSubstanceIATAFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new UNDGSubstanceIATAForm(Factory.New<UNDGSubstance>());
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

		public void TestBorderPanel()
		{
			var substance = Factory.New<UNDGSubstance>();
			using var control = new UNDGSubstanceIATAControl(substance);
			var horizontalDivider = control.Controls.Find("DG_BorderPanel", searchAllChildren: true)?.FirstOrDefault() as ZPanel;
			AssertNotNull(horizontalDivider);
			AssertEquals("DG_BorderPanel", horizontalDivider.Name);
			AssertEquals(new Size(910, 1), horizontalDivider.Size);
			AssertEquals(new Point(60, 40), horizontalDivider.Location);
			AssertEquals(Color.DarkGray, horizontalDivider.BackColor);
		}
	}
}
