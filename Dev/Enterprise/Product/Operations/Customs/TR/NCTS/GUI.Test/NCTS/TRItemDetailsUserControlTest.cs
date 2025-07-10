using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	class TRItemDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDeclerationNumberField()
		{
			using (var control = new TRItemDetailsUserControl())
			{
				var declerationNumberTextBox = control.Controls.Find("ExportDeclarationNumberTextBox", true).FirstOrDefault();
				CombineAssertions(() =>
				{
					AssertType<ZTextBox>(declerationNumberTextBox);
					AssertEquals("ExportDeclarationNumberTextBox", true, declerationNumberTextBox.Visible);
				});
			}
		}

		public void TestTypeField()
		{
			using (var control = new TRItemDetailsUserControl())
			{
				var exportDeclarationTypeDropEdit = control.Controls.Find("ExportDeclarationTypeDropEdit", true).FirstOrDefault();
				CombineAssertions(() =>
				{
					AssertType<ZDropEdit>(exportDeclarationTypeDropEdit);
					AssertEquals("ExportDeclarationTypeCodeBox", true, exportDeclarationTypeDropEdit.Visible);
				});
			}
		}

		public void TestIsPartialField()
		{
			using (var control = new TRItemDetailsUserControl())
			{
				var isDeclarationPartialCheckBox = (ZCheckBox)control.Controls.Find("IsDeclarationPartialCheckBox", true).First();
				CombineAssertions(() =>
				{
					AssertType<ZCheckBox>(isDeclarationPartialCheckBox);
					AssertEquals("isDeclarationPartialCheckBox", true, isDeclarationPartialCheckBox.Visible);
					AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 15, true), isDeclarationPartialCheckBox.Size);
					AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 424, true), isDeclarationPartialCheckBox.Location);
				});
			}
		}

		public void TestGetTariffType()
		{
			using (var control = new TRItemDetailsUserControl())
			{
				var commodityCodeTariffFindBox = control.FindSingle<TariffFindBox>("CommodityCodeTariffFindBox");
				AssertEquals("HSN", commodityCodeTariffFindBox.TariffType);
			}
		}

		public void TestCaptionRenderingEnabled()
		{
			using (var control = new TRItemDetailsUserControl())
			{
				AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
			}
		}

		public void TestCustomsValueDropEditCaption()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.Turkey))
			using (var control = new TRItemDetailsUserControl())
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				control.SetDataBinding(header, ZString.Empty);
				var customsValueCaption = control.FindSingle<ZCalcDropEdit>("CustomsValueDropEdit").GetExtension<ILabelCaptionRenderer>().Caption;
				AssertEquals("Statistical Value in USD", customsValueCaption);
			}
		}
	}
}


