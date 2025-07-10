using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module
{
	public static class ModuleWarehouseLocationFilterControlHelper
	{
		public static Control[] GetNewWarehouseFilterControls(ZFilterStrip strip, ZBindingSource bindingSource)
		{
			ZGuidDropEdit warehouseControl = new ZGuidDropEdit();
			warehouseControl.CharacterCasing = CharacterCasing.Upper;
			warehouseControl.ShowDescriptionBox = false;
			warehouseControl.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			ControlDpiScalingHelper.SetTop(ref warehouseControl, strip.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref warehouseControl, strip.FilterControlsBox1Start, true);
			warehouseControl.TabIndex = 1;
			bindingSource.SetBindingMember(warehouseControl, ModuleWarehouseLocationFilter.Schema.Warehouse);

			ZTextBox locationControl = new ZTextBox();
			ControlDpiScalingHelper.SetTop(ref locationControl, strip.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref locationControl, strip.FilterControlsBox2Start(warehouseControl), true);
			ControlDpiScalingHelper.SetWidth(ref locationControl, warehouseControl.Width, false);
			locationControl.TabIndex = 2;
			bindingSource.SetBindingMember(locationControl, ModuleWarehouseLocationFilter.Schema.Location);

			return new Control[] { warehouseControl, locationControl };
		}
	}
}
