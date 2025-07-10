using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module
{
	internal sealed class CustomModuleFilterControlKludge : WorkflowFilterStrip
	{
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			var voyageVesselModuleFilter = currentModuleFilter as VoyageVesselModuleFilter;
			if (voyageVesselModuleFilter != null)
			{
				var control = new VoyageVesselModuleFilterControl(this, FilterControlBindingSource);
				control.SetMaxLength(voyageVesselModuleFilter);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				result = new Control[] { control };
			}
			else if (currentModuleFilter is EntryNumberModuleFilter)
			{
				result = GetEntryNumberFilterControls(this, FilterControlBindingSource);
			}
			else if (currentModuleFilter is OrgRelatedPartiesModuleFilter)
			{
				result = GetOrgRelatedPartiesModuleFilterControl();
			}
			else if (currentModuleFilter is ChargeModuleFilter)
			{
				result = GetChargeModuleFilter(this, FilterControlBindingSource);
			}
			else if (currentModuleFilter is ReferenceNumberFilter)
			{
				result = ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
			}
			else if (currentModuleFilter is IJobManagementAmountFilter)
			{
				result = new Control[] { (Control)((IJobManagementAmountFilter)currentModuleFilter).GetFilterControl() };
			}
			else
			{
				result = null;
			}

			if (result != null)
			{
				CalculatePreferedHeight(result);
				return result;
			}
			else
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}

		void CalculatePreferedHeight(Control[] controls)
		{
			PreferredHeight = controls.Select(control => control.Bottom).Aggregate(PreferredHeight, Math.Max);
		}

		static Control[] GetEntryNumberFilterControls(ZFilterStrip filterStripControl, ZBindingSource bindingSource)
		{
			ZDropEdit operatorDropEdit = new ZDropEdit();
			operatorDropEdit.Name = "operatorDropEdit";
			operatorDropEdit.CharacterCasing = CharacterCasing.Lower;
			operatorDropEdit.CodeBox.TextAlign = HorizontalAlignment.Center;
			operatorDropEdit.CodeBox.Font = filterStripControl.ComparisonOperatorFont;
			operatorDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
			operatorDropEdit.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref operatorDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref operatorDropEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(filterStripControl.FilterControlsBox1Start - ZFilterStrip.SpaceBetweenLabelAndControl * 2) - operatorDropEdit.Width, false);
			operatorDropEdit.BindToList = "ComparisonOperator_List";
			operatorDropEdit.TabIndex = 0;
			bindingSource.SetBindingMember(operatorDropEdit, "ComparisonOperator");

			ZDropEdit entryTypeDropEdit = new ZDropEdit();
			entryTypeDropEdit.Name = "entryTypeDropEdit";
			entryTypeDropEdit.CharacterCasing = CharacterCasing.Upper;
			entryTypeDropEdit.PreBoundMaxLength = 4;
			entryTypeDropEdit.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref entryTypeDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref entryTypeDropEdit, filterStripControl.FilterControlsBox1Start, true);
			entryTypeDropEdit.BindTo = "EntryType";
			entryTypeDropEdit.TabIndex = 1;
			bindingSource.SetBindingMember(entryTypeDropEdit, "EntryType");

			ZTextBox entryNumberTextBox = new ZTextBox();
			entryNumberTextBox.Name = "entryNumberTextBox";
			ControlDpiScalingHelper.SetWidth(ref entryNumberTextBox, ControlDpiScalingHelper.ScaleToCurrentDpiX(filterStripControl.FilterControlBoxWidth - ZFilterStrip.SpaceBetweenLabelAndControl) - entryTypeDropEdit.Width, false);
			ControlDpiScalingHelper.SetTop(ref entryNumberTextBox, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref entryNumberTextBox, entryTypeDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(ZFilterStrip.SpaceBetweenLabelAndControl), false);
			entryNumberTextBox.TabIndex = 2;
			entryNumberTextBox.BindTo = "Property";
			bindingSource.SetBindingMember(entryNumberTextBox, "Property");

			return new Control[] { operatorDropEdit, entryTypeDropEdit, entryNumberTextBox };
		}
		static Control[] GetOrgRelatedPartiesModuleFilterControl()
		{
			return new Control[] { new OrgRelatedPartiesFilterControl() };
		}

		static Control[] GetChargeModuleFilter(ZFilterStrip filterStripControl, ZBindingSource bindingSource)
		{
			ZLabel chargeGroupLabel = new ZLabel();
			chargeGroupLabel.Name = "chargeGroupLabel";
			chargeGroupLabel.Text = Res.GetString("677e81d0-89d8-40a0-ba2b-889d855f6108", "Group:");
			chargeGroupLabel.Size = chargeGroupLabel.PreferredSize;
			ControlDpiScalingHelper.SetWidth(chargeGroupLabel, chargeGroupLabel.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), false);
			ControlDpiScalingHelper.SetTop(ref chargeGroupLabel, filterStripControl.LabelTop, false);
			ControlDpiScalingHelper.SetLeft(ref chargeGroupLabel, filterStripControl.Label1Start(chargeGroupLabel), false);
			chargeGroupLabel.TabIndex = 0;

			ZDropEdit chargeGroupDropEdit = new ZDropEdit();
			chargeGroupDropEdit.Name = "chargeGroupDropEdit";
			chargeGroupDropEdit.CharacterCasing = CharacterCasing.Upper;
			chargeGroupDropEdit.ShowDescriptionBox = true;
			ControlDpiScalingHelper.SetTop(ref chargeGroupDropEdit, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref chargeGroupDropEdit, filterStripControl.FilterControlsBox1Start, true);
			chargeGroupDropEdit.TabIndex = 1;
			bindingSource.SetBindingMember(chargeGroupDropEdit, ChargeModuleFilter.Schema.ChargeGroup);

			int row2start = filterStripControl.FilterControlTop + chargeGroupDropEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(3);
			int row2labelStart = filterStripControl.LabelTop + chargeGroupDropEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(3);

			ZCheckBox useLowerBoundCheckBox = new ZCheckBox();
			useLowerBoundCheckBox.Name = "useLowerBoundCheckBox";
			useLowerBoundCheckBox.Size = useLowerBoundCheckBox.PreferredSize;
			ControlDpiScalingHelper.SetTop(ref useLowerBoundCheckBox, row2labelStart, false);
			ControlDpiScalingHelper.SetLeft(ref useLowerBoundCheckBox, filterStripControl.FilterControlsBox1Start, true);
			useLowerBoundCheckBox.TabIndex = 3;
			bindingSource.SetBindingMember(useLowerBoundCheckBox, ChargeModuleFilter.Schema.UseLowerBound);

			ZCalcEdit lowerBoundCalcEdit = new ZCalcEdit();
			lowerBoundCalcEdit.Name = "lowerBoundCalcEdit";
			lowerBoundCalcEdit.DecimalPlaces = GlbCompany.CurrentCompany.LocalCurrency.Decimals;
			ControlDpiScalingHelper.SetTop(ref lowerBoundCalcEdit, row2start, false);
			ControlDpiScalingHelper.SetLeft(ref lowerBoundCalcEdit, useLowerBoundCheckBox.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
			ControlDpiScalingHelper.SetWidth(lowerBoundCalcEdit, lowerBoundCalcEdit.Width - (useLowerBoundCheckBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3)), false);
			lowerBoundCalcEdit.TabIndex = 4;
			bindingSource.SetBindingMember(lowerBoundCalcEdit, ChargeModuleFilter.Schema.LowerBound);

			ZLabel lowerBoundLabel = new ZLabel();
			lowerBoundLabel.Name = "lowerBoundLabel";
			lowerBoundLabel.Text = Res.GetString("b065a5a3-4edf-44e3-b190-0a1508add698", "From:");
			lowerBoundLabel.Size = lowerBoundLabel.PreferredSize;
			ControlDpiScalingHelper.SetTop(ref lowerBoundLabel, row2labelStart, false);
			ControlDpiScalingHelper.SetLeft(ref lowerBoundLabel, filterStripControl.Label1Start(lowerBoundLabel), false);
			lowerBoundLabel.TabIndex = 2;

			ZCalcEdit upperBoundCalcEdit = new ZCalcEdit();
			upperBoundCalcEdit.Name = "upperBoundCalcEdit";
			ControlDpiScalingHelper.SetTop(ref upperBoundCalcEdit, row2start, false);
			ControlDpiScalingHelper.SetLeft(ref upperBoundCalcEdit, filterStripControl.FilterControlsBox2Start(upperBoundCalcEdit), true);
			upperBoundCalcEdit.TabIndex = 7;
			bindingSource.SetBindingMember(upperBoundCalcEdit, ChargeModuleFilter.Schema.UpperBound);

			ZCheckBox useUpperBoundCheckBox = new ZCheckBox();
			useUpperBoundCheckBox.Name = "useUpperBoundCheckBox";
			useUpperBoundCheckBox.Size = useUpperBoundCheckBox.PreferredSize;
			ControlDpiScalingHelper.SetTop(ref useUpperBoundCheckBox, row2labelStart, false);
			ControlDpiScalingHelper.SetLeft(ref useUpperBoundCheckBox, upperBoundCalcEdit.Left, false);
			useUpperBoundCheckBox.TabIndex = 6;
			bindingSource.SetBindingMember(useUpperBoundCheckBox, ChargeModuleFilter.Schema.UseUpperBound);

			ControlDpiScalingHelper.SetWidth(upperBoundCalcEdit, upperBoundCalcEdit.Width - (useUpperBoundCheckBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3)), false);
			ControlDpiScalingHelper.SetLeft(upperBoundCalcEdit, upperBoundCalcEdit.Left + (useUpperBoundCheckBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3)), false);

			ZLabel upperBoundLabel = new ZLabel();
			upperBoundLabel.Name = "upperBoundLabel";
			upperBoundLabel.Text = Res.GetString("f29fed5a-980e-4a91-93cf-71157f648542", "To:");
			upperBoundLabel.Size = upperBoundLabel.PreferredSize;
			ControlDpiScalingHelper.SetTop(ref upperBoundLabel, row2labelStart, false);
			ControlDpiScalingHelper.SetLeft(ref upperBoundLabel, useUpperBoundCheckBox.Left - upperBoundLabel.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
			upperBoundLabel.TabIndex = 5;

			return new Control[]
			{
				chargeGroupLabel,
				chargeGroupDropEdit,
				lowerBoundLabel,
				useLowerBoundCheckBox,
				lowerBoundCalcEdit,
				upperBoundLabel,
				useUpperBoundCheckBox,
				upperBoundCalcEdit,
			};
		}
	}
}


