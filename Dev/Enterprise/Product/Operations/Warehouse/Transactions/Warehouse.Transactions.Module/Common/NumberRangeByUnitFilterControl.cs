using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class NumberRangeByUnitFilterControl : ZFilterStrip
	{
		public NumberRangeByUnitFilterControl()
		{
		}

		public Control[] GetFilterControl(Control control, ZBindingSource bindingSource)
		{
			var fromCalcEdit = new ZCalcEdit();
			var toCalcEdit = new ZCalcEdit();

			fromCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("NumberRangeByUnitFilterControl|dcee2031-9bad-4cd6-856c-c7e454d902ba", "From");
			fromCalcEdit.Decimals = 2;
			fromCalcEdit.Location = ControlDpiScalingHelper.NewScaledPoint(275, 0, true);
			fromCalcEdit.BindTo = "Property1";
			fromCalcEdit.TabIndex = 2;
			ControlDpiScalingHelper.SetWidth(ref fromCalcEdit, FilterControlBoxWidthSmall - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStripDateEdit.CalendarButtonWidthZ), false);
			bindingSource.SetBindingMember(fromCalcEdit, fromCalcEdit.BindTo);

			ControlDpiScalingHelper.SetWidth(ref toCalcEdit, FilterControlBoxWidthSmall, false); // must be before calculating Label2Start() below.
			toCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("NumberRangeByUnitFilterControl|0f57e841-dbbb-4f1a-8296-cd5cc112d3aa", "To");
			toCalcEdit.Decimals = 2;
			toCalcEdit.Location = ControlDpiScalingHelper.NewScaledPoint(397, 0, true);
			toCalcEdit.BindTo = "Property2";
			toCalcEdit.TabIndex = 4;
			ControlDpiScalingHelper.SetWidth(ref toCalcEdit, toCalcEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStripDateEdit.CalendarButtonWidthZ), false); // must be after calculating FilterControlsBox2Start()
			bindingSource.SetBindingMember(toCalcEdit, toCalcEdit.BindTo);

			var unitControl = ((ZDropEdit)control);
			unitControl.TabIndex = 5;
			unitControl.Location = ControlDpiScalingHelper.NewScaledPoint(476, 0, true);
			ControlDpiScalingHelper.SetWidth(unitControl.CodeBox, 18, true);
			unitControl.DescriptionBox.Visible = false;
			ControlDpiScalingHelper.SetWidth(ref unitControl, 18, true);

			return new Control[] { unitControl, fromCalcEdit, toCalcEdit };
		}
	}
}
