using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public static class ReferenceNumberFilterGUIProvider
	{
		public static Control[] GetReferenceNumberFilterControls(ZFilterStrip filterStripControl, ZBindingSource bindingSource)
		{
			ZDropEdit operatorDropEdit = new ZDropEdit();
			operatorDropEdit.Name = "operatorDropEdit";
			operatorDropEdit.CharacterCasing = CharacterCasing.Lower;
			operatorDropEdit.CodeBox.TextAlign = HorizontalAlignment.Center;
			operatorDropEdit.CodeBox.Font = filterStripControl.ComparisonOperatorFont;
			operatorDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
			operatorDropEdit.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref operatorDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref operatorDropEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(filterStripControl.FilterControlsBox1Start) - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl) - operatorDropEdit.Width, false);
			operatorDropEdit.BindToList = "ComparisonOperator_List";
			operatorDropEdit.TabIndex = 0;
			bindingSource.SetBindingMember(operatorDropEdit, "ComparisonOperator");

			ZDropEdit typePropertyDropEdit = new ZDropEdit();
			typePropertyDropEdit.Name = "typeDropEdit";
			typePropertyDropEdit.CharacterCasing = CharacterCasing.Upper;
			typePropertyDropEdit.PreBoundMaxLength = 3;
			typePropertyDropEdit.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref typePropertyDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref typePropertyDropEdit, filterStripControl.FilterControlsBox2Start(typePropertyDropEdit), true);
			typePropertyDropEdit.TabIndex = 5;
			bindingSource.SetBindingMember(typePropertyDropEdit, "Type");

			ZLabel typeLabel = new ZLabel();
			typeLabel.Name = "typeLabel";
			typeLabel.Text = Res.GetString("0da2b46e-dba6-4ba7-9159-38f88a77861a", "Type:");
			typeLabel.Size = typeLabel.PreferredSize;
			ControlDpiScalingHelper.SetWidth(typeLabel, typeLabel.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
			ControlDpiScalingHelper.SetTop(ref typeLabel, filterStripControl.LabelTop, false);
			ControlDpiScalingHelper.SetLeft(ref typeLabel, filterStripControl.Label2Start(typeLabel, typePropertyDropEdit), false);
			typeLabel.TabIndex = 4;

			ZCodeFindBox countryFindBox = new ZCodeFindBox();
			countryFindBox.Name = "countryFindBox";
			countryFindBox.PreBoundMaxLength = 2;
			countryFindBox.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref countryFindBox, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref countryFindBox, typeLabel.Left - countryFindBox.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl), false);
			countryFindBox.TabIndex = 3;
			bindingSource.SetBindingMember(countryFindBox, "Country");

			ZLabel countryLabel = new ZLabel();
			countryLabel.Name = "countryLabel";
			countryLabel.Text = Res.GetString("be1952b1-d149-4926-860f-83dcd4c726fd", "Country/Region:");
			countryLabel.Size = countryLabel.PreferredSize;
			ControlDpiScalingHelper.SetWidth(countryLabel, countryLabel.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
			ControlDpiScalingHelper.SetTop(ref countryLabel, filterStripControl.LabelTop, false);
			ControlDpiScalingHelper.SetLeft(ref countryLabel, countryFindBox.Left - countryLabel.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl), false);
			countryLabel.TabIndex = 2;

			ZTextBox numberTextBox = new ZTextBox();
			numberTextBox.Name = "numberTextBox";
			ControlDpiScalingHelper.SetTop(ref numberTextBox, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref numberTextBox, filterStripControl.FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref numberTextBox, countryLabel.Left - numberTextBox.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl), false);
			numberTextBox.TabIndex = 1;
			bindingSource.SetBindingMember(numberTextBox, "Property");

			return new Control[]
			{
				operatorDropEdit,
				numberTextBox,
				countryLabel,
				countryFindBox,
				typeLabel,
				typePropertyDropEdit,
			};
		}
	}
}
