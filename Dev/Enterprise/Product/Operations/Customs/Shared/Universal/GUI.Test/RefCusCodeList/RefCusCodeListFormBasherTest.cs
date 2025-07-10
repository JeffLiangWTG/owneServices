using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(RefCusCodeListForm))]
	sealed class RefCusCodeListFormBasherTest : ZFormBasherTest
	{
		public void TestTransportModeCheckBoxes()
		{
			var code = Factory.New<ZZRefCusCodeListCombined>();
			using (var form = new RefCusCodeListForm(code))
			{
				form.Show();
				var codeTransportModesPanel = (ZPanel)form.Controls.Find("CodeTransportModesPanel", searchAllChildren: true)[0];
				var attrTransportModesPanel = (ZPanel)form.Controls.Find("AttrTransportModesPanel", searchAllChildren: true)[0];
				AssertEquals(RefTransportModesHelper.GetList(Factory).Count, codeTransportModesPanel.Controls.Cast<ZCheckBox>().Count());
				AssertEquals(RefTransportModesHelper.GetList(Factory).Count, attrTransportModesPanel.Controls.Cast<ZCheckBox>().Count());
			}
		}

		public void TestColumnStyleInfos()
		{
			var code = Factory.New<ZZRefCusCodeListCombined>();
			using (var form = new RefCusCodeListForm(code))
			{
				form.Show();
				var attributesGrid = form.Controls.Find("AttributesGrid", searchAllChildren: true)[0] as ZGrid;
				CombineAssertions(() =>
				{
					Assert(attributesGrid.ColumnStyles.OfType<ZMultiControlColumnStyleInfo>().Any(info => info.ColumnName == "ZZE_Value"));
					Assert(attributesGrid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Any(info => info.ColumnName == "DescriptionOfZZE_Value"));
					Assert(attributesGrid.ColumnStyles.OfType<ZDateEditColumnStyleInfo>().Any(info => info.ColumnName == "ZZE_StartDate"));
					Assert(attributesGrid.ColumnStyles.OfType<ZDateEditColumnStyleInfo>().Any(info => info.ColumnName == "ZZE_EndDate"));
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var code = factory.New<ZZRefCusCodeListCombined>();
			return new RefCusCodeListForm(code);
		}
	}
}
