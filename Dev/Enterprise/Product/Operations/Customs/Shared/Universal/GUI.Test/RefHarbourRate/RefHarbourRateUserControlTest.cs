using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI.Testing;

sealed class RefHarbourRateUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using var userControl = new RefHarbourRateUserControl();
		AssertControl("ZXF_TypeTextBox", "ZXF_Type");
		AssertControl("ZXF_PortTextBox", "ZXF_Port");
		AssertControl("ZXF_ModeDropEdit", "ZXF_Mode");
		AssertControl("ZXF_CommodityTextBox", "ZXF_Commodity");
		AssertControl("ZXF_PortTaxTypeTextBox", "ZXF_PortTaxType");
		AssertControl("ZXF_RateFormulaTextBox", "ZXF_RateFormula");
		AssertControl("ZXF_ZZZ_NKDataGroupingCodeFindBox", "ZXF_ZZZ_NKDataGrouping");
		AssertControl("ZXF_StartDateEdit", "ZXF_StartDate");
		AssertControl("ZXF_EndDateEdit", "ZXF_EndDate");

		void AssertControl(string controlName, string binding)
		{
			CombineAssertions(() =>
			{
				if (controlName.EndsWith("TextBox"))
				{
					var control = userControl.FindSingle<ZTextBox>(controlName);
					AssertEquals("Binding", binding, control.BindTo);
				}
				else if (controlName.EndsWith("DropEdit"))
				{
					var control = userControl.FindSingle<ZDropEdit>(controlName);
					AssertEquals("Binding", binding, control.BindTo);
				}
				else if (controlName.EndsWith("DateEdit"))
				{
					var control = userControl.FindSingle<ZDateEdit>(controlName);
					AssertEquals("Binding", binding, control.BindTo);
				}
				else if (controlName.EndsWith("CodeFindBox"))
				{
					var control = userControl.FindSingle<ZCodeFindBox>(controlName);
					AssertEquals("Binding", binding, control.BindTo);
				}
				else if (controlName.EndsWith("CheckBox"))
				{
					var control = userControl.FindSingle<ZCheckBox>(controlName);
					AssertEquals("Binding", binding, control.BindTo);
				}
			});
		}
	}
}
