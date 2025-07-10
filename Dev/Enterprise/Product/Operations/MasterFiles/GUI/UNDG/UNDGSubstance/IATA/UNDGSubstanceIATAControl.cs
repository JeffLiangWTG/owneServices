using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceIATAControl : UNDGSubstanceBaseControl
	{
		readonly UNDGSubstance substance;

		public UNDGSubstanceIATAControl(UNDGSubstance substance)
		{
			this.substance = substance;

			InitializeComponent();

			CrossReferencesGrid.ReadOnly = QualifyingDescriptiveTextGrid.ReadOnly = true;

			SetControlsVisibility(substance);
		}

		string GetUNNOPrefix() => substance.GetUnnoPrefix();

		void SetControlsVisibility(UNDGSubstance substance)
		{
			var haveSecondPackIns = !substance.PaxPackInsSec2.IsEmpty || !substance.CaoPackInsSec2.IsEmpty;
			DG_LQ2OrPaxMaxAmtUQTextBox2.Visible = haveSecondPackIns;
			DG_LQ2OrPaxMaxAmtCalcEdit2.Visible = haveSecondPackIns;
			DG_PaxPackInsTextBox2.Visible = haveSecondPackIns;
			DG_PaxPackInsSecTextBox2.Visible = haveSecondPackIns;
			DG_CargoMaxAmtUQTextBox2.Visible = haveSecondPackIns;
			DG_CargoMaxAmtCalcEdit2.Visible = haveSecondPackIns;
			DG_CargoPackInsTextBox2.Visible = haveSecondPackIns;
			DG_CargoPackInsSecTextBox2.Visible = haveSecondPackIns;

			if (haveSecondPackIns)
			{
				PassengerAndCargoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 154, true);
				CargoOnlyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 154, true);
				UNDGSubstanceTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 451, true);
				DetailsLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 425, true);
			}
			else
			{
				PassengerAndCargoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 104, true);
				CargoOnlyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 104, true);
				UNDGSubstanceTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 401, true);
				DetailsLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 375, true);
			}
		}
	}
}
