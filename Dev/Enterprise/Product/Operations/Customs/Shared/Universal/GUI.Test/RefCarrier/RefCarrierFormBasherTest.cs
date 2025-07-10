using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(RefCarrierForm))]
	class RefCarrierFormBasherTest : ZFormBasherTest
	{
		public void TestTransportModeCheckBoxes()
		{
			var carrier = new BusinessObjectFactory().New<ZZRefCarrierCombined>();
			using (var form = new RefCarrierForm(carrier))
			{
				form.Show();
				var codeTransportModesPanel = (ZPanel)form.Controls.Find("CodeTransportModesPanel", true)[0];
				AssertEquals(RefCarrierHelper.GetTransportModesList(Factory).Count, codeTransportModesPanel.Controls.Cast<ZCheckBox>().Count());
			}
		}

		public void TestColumnStyleInfos()
		{
			var carrier = new BusinessObjectFactory().New<ZZRefCarrierCombined>();
			using (var form = new RefCarrierForm(carrier))
			{
				form.Show();
				var attributesGrid = form.Controls.Find("AttributesGrid", true).First() as ZGrid;
				Assert(attributesGrid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Any(info => info.ColumnName == "ZZG_Value"));
			}
		}

		protected override Form GetFormToBashCore() => new RefCarrierForm(new BusinessObjectFactory().New<ZZRefCarrierCombined>());
	}
}
