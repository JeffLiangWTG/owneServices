using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DensityValuesControl : ZUserControl
	{
		ZArchitecture.ZCalcEdit ExcessVolumeWeightCalcEdit;
		ZArchitecture.ZLabel ExcessVolumeWeightUnitLabel;
		ZPanel ValuesGroupPanel;
		DensityVisualisationControl DensityVisualisationControl;
		ZGroupBox DensityValuesGroupbox;
		ZArchitecture.ZLabel DensityFactorLabel;

		void InitializeComponent()
		{
			this.ExcessVolumeWeightCalcEdit = new ZArchitecture.ZCalcEdit();
			this.ExcessVolumeWeightUnitLabel = new ZArchitecture.ZLabel();
			this.ValuesGroupPanel = new ZPanel();
			this.DensityValuesGroupbox = new ZGroupBox();
			this.DensityVisualisationControl = new DensityVisualisationControl();
			this.DensityFactorLabel = new ZArchitecture.ZLabel();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ValuesGroupPanel.SuspendLayout();
			this.DensityValuesGroupbox.SuspendLayout();
			this.DensityVisualisationControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ForwardingShipment);
			// 
			// ExcessVolumeWeightCalcEdit
			// 
			this.ExcessVolumeWeightCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExcessVolumeWeightCalcEdit, "JS_Calc_ExcessActualVolumeWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_Calc_ExcessActualVolumeWeight)));
			this.ExcessVolumeWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("005d4d2e-a0b4-4add-b605-b148c61c1324", "Excess Vol. Wgt.", "Excess Volume Weight");
			this.ExcessVolumeWeightCalcEdit.DecimalPlaces = 3;
			this.ExcessVolumeWeightCalcEdit.Decimals = 3;
			this.ExcessVolumeWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 9, true);
			this.ExcessVolumeWeightCalcEdit.Name = "ExcessVolumeWeightCalcEdit";
			this.ExcessVolumeWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 18, true);
			this.ExcessVolumeWeightCalcEdit.TabIndex = 4;
			this.ExcessVolumeWeightCalcEdit.Text = "0.000";
			this.ExcessVolumeWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExcessVolumeWeightUnitLabel
			// 
			this.ExcessVolumeWeightUnitLabel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExcessVolumeWeightUnitLabel, "JS_Calc_ActualVolumeWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_Calc_ActualVolumeWeightUnit)));
			this.ExcessVolumeWeightUnitLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ExcessVolumeWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 10, true);
			this.ExcessVolumeWeightUnitLabel.Name = "ExcessVolumeWeightUnitLabel";
			this.ExcessVolumeWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 17, true);
			this.ExcessVolumeWeightUnitLabel.TabIndex = 5;
			// 
			// ValuesGroupPanel
			// 
			this.ValuesGroupPanel.Controls.Add(this.ExcessVolumeWeightUnitLabel);
			this.ValuesGroupPanel.Controls.Add(this.ExcessVolumeWeightCalcEdit);
			this.ValuesGroupPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 103, true);
			this.ValuesGroupPanel.Name = "ValuesGroupPanel";
			this.ValuesGroupPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 37, true);
			this.ValuesGroupPanel.TabIndex = 0;
			// 
			// DensityValuesGroupbox
			// 
			this.DensityValuesGroupbox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2e647128-fa95-4cd4-942e-b2890a351e0a", "Density/Excess Values");
			this.DensityValuesGroupbox.Controls.Add(this.DensityVisualisationControl);
			this.DensityValuesGroupbox.Controls.Add(this.ValuesGroupPanel);
			this.DensityValuesGroupbox.Controls.Add(this.DensityFactorLabel);
			this.DensityValuesGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DensityValuesGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DensityValuesGroupbox.Name = "DensityValuesGroupbox";
			this.DensityValuesGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 150, true);
			this.DensityValuesGroupbox.TabIndex = 13;
			this.DensityValuesGroupbox.TabStop = false;
			// 
			// DensityVisualisationControl
			// 
			this.DensityVisualisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DensityVisualisationControl, "Density");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Density)(((ForwardingShipment)(null)).Density)));
			this.DensityVisualisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 50, true);
			this.DensityVisualisationControl.Name = "DensityVisualisationControl";
			this.DensityVisualisationControl.ConfigureWidth(263);
			this.DensityVisualisationControl.TabIndex = 1;
			//
			// DensityFactorLabel
			//
			this.DensityFactorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 30, true);
			this.DensityFactorLabel.TabIndex = 2;
			this.DensityFactorLabel.Name = "DensityFactorLabel";
			this.DensityFactorLabel.AutoSize = true;
			this.DensityFactorLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("fb8f06b2-9d8f-4523-8f1a-d1d0bf4a566e", "Density Factor");
			// 
			// DensityValuesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DensityValuesGroupbox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 118, true);
			this.Name = "DensityValuesControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 150, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ValuesGroupPanel.ResumeLayout(false);
			this.ValuesGroupPanel.PerformLayout();
			this.DensityValuesGroupbox.ResumeLayout(false);
			this.DensityValuesGroupbox.PerformLayout();
			this.DensityVisualisationControl.ResumeLayout(true);
			this.DensityVisualisationControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
