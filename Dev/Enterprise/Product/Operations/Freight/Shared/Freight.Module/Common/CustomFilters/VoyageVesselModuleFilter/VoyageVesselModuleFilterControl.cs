using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module
{
	public partial class VoyageVesselModuleFilterControl : ZUserControl
	{
		public VoyageVesselModuleFilterControl(ZFilterStrip filterStripControl, ZBindingSource bindingSource, bool hideArchivedFlag = false)
		{
			InitializeComponent();
			this.VesselFindBox.CodeBox.Size = ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			SetupOperatorDropEdit(filterStripControl, bindingSource);

			ControlDpiScalingHelper.SetLeft(ref VoyageFlightTextBox, operatorDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(4), false);
			ControlDpiScalingHelper.SetLeft(ref IncludeArchivedCheckBox, operatorDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(4), false);

			if (hideArchivedFlag)
			{
				ControlDpiScalingHelper.SetHeight(this, IncludeArchivedCheckBox.Height, false);
			}
		}

		void SetupOperatorDropEdit(ZFilterStrip filterStripControl, ZBindingSource bindingSource)
		{
			operatorDropEdit.CharacterCasing = CharacterCasing.Lower;
			operatorDropEdit.CodeBox.TextAlign = HorizontalAlignment.Center;
			operatorDropEdit.CodeBox.Font = filterStripControl.ComparisonOperatorFont;
			operatorDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
			ControlDpiScalingHelper.SetLeft(ref operatorDropEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(filterStripControl.FilterControlsBox1Start - ZFilterStrip.SpaceBetweenLabelAndControl) - operatorDropEdit.Width, false);
			operatorDropEdit.BindToList = "ComparisonOperator_List";
			bindingSource.SetBindingMember(operatorDropEdit, "ComparisonOperator");
		}

		public void SetMaxLength(VoyageVesselModuleFilter filter)
		{
			VesselFindBox.ShouldResize = false;
			VoyageFlightTextBox.MaxLength = filter.MaxLength;
			VesselFindBox.CodeBox.MaxLength = filter.VesselMaxLength;
		}
	}
}
