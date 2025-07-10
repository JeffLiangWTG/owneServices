using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CustomConversionFactorForm
	{
		private ZArchitecture.ZCalcEdit PerWeightCalcEdit;
		private ZDropEdit WeightUnitDropEdit;
		private ZArchitecture.ZLabel zLabel3;
		private ZDropEdit VolumeUnitDropEdit;
		private ZArchitecture.ZCalcEdit PerVolumeCalcEdit;
		private ZButton OkButton;
		private ZButton CancelButtonX;
		private ZArchitecture.ZTextBox ConversionFactorTextBox;

		new void InitializeComponent()
		{
			this.PerWeightCalcEdit = new ZArchitecture.ZCalcEdit();
			this.WeightUnitDropEdit = new ZDropEdit();
			this.zLabel3 = new ZArchitecture.ZLabel();
			this.VolumeUnitDropEdit = new ZDropEdit();
			this.PerVolumeCalcEdit = new ZArchitecture.ZCalcEdit();
			this.OkButton = new ZButton();
			this.CancelButtonX = new ZButton();
			this.ConversionFactorTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 178, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 0, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(149);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(149);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CustomConversionFactor);
			// 
			// PerWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PerWeightCalcEdit, "WeightPrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CustomConversionFactor)(null)).WeightPrice);
			this.PerWeightCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CustomConversionFactorForm|7fbf28cf-ebc2-481b-85a9-0f2f9e937646", "$");
			this.PerWeightCalcEdit.DecimalPlaces = 2;
			this.PerWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 15, true);
			this.PerWeightCalcEdit.Name = "PerWeightCalcEdit";
			this.PerWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.PerWeightCalcEdit.TabIndex = 0;
			this.PerWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WeightUnitDropEdit
			// 
			this.BindingSource.SetBindingMember(this.WeightUnitDropEdit, "WeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CustomConversionFactor)(null)).WeightUnit);
			this.WeightUnitDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CustomConversionFactorForm|c2fd6907-fde2-42f7-a048-844835575f23", "per");
			this.WeightUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 15, true);
			this.WeightUnitDropEdit.Name = "WeightUnitDropEdit";
			this.WeightUnitDropEdit.PreBoundMaxLength = 2;
			this.WeightUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.WeightUnitDropEdit.TabIndex = 1;
			// 
			// zLabel3
			// 
			this.zLabel3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CustomConversionFactorForm|583afa7f-0dac-4bae-8617-50c82a7ebe77", "OR");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 45, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 14, true);
			this.zLabel3.TabIndex = 2;
			// 
			// VolumeUnitDropEdit
			// 
			this.BindingSource.SetBindingMember(this.VolumeUnitDropEdit, "VolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CustomConversionFactor)(null)).VolumeUnit);
			this.VolumeUnitDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CustomConversionFactorForm|3d4045c5-9fb8-4501-9814-a4ad0d6747dd", "per");
			this.VolumeUnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 67, true);
			this.VolumeUnitDropEdit.Name = "VolumeUnitDropEdit";
			this.VolumeUnitDropEdit.PreBoundMaxLength = 2;
			this.VolumeUnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.VolumeUnitDropEdit.TabIndex = 4;
			// 
			// PerVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PerVolumeCalcEdit, "VolumePrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CustomConversionFactor)(null)).VolumePrice);
			this.PerVolumeCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CustomConversionFactorForm|70d1b295-9010-4824-af39-d92b1320eeab", "$");
			this.PerVolumeCalcEdit.DecimalPlaces = 2;
			this.PerVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 67, true);
			this.PerVolumeCalcEdit.Name = "PerVolumeCalcEdit";
			this.PerVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.PerVolumeCalcEdit.TabIndex = 3;
			this.PerVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OkButton
			// 
			this.OkButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CustomConversionFactorForm|ac709abb-e98a-40c9-9664-8d9dbf31aacb", "OK");
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 134, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 22, true);
			this.OkButton.TabIndex = 6;
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CustomConversionFactorForm|e35fbd17-a6ed-404f-82f1-3b056fb7cc88", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 134, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 22, true);
			this.CancelButtonX.TabIndex = 7;
			// 
			// ConversionFactorCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ConversionFactorTextBox, "ConversionFactor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CustomConversionFactor)(null)).ConversionFactor);
			this.ConversionFactorTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CustomConversionFactorForm|580b0eb6-052a-4386-8b2b-d1ff25fd8020", "Conversion Factor");
			this.ConversionFactorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 104, true);
			this.ConversionFactorTextBox.Name = "ConversionFactorCalcEdit";
			this.ConversionFactorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ConversionFactorTextBox.TabIndex = 5;
			// 
			// CustomConversionFactorForm
			// 
			this.AcceptButton = this.OkButton;

			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 178, true);
			this.ControlBox = false;
			this.Controls.Add(this.ConversionFactorTextBox);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.VolumeUnitDropEdit);
			this.Controls.Add(this.PerVolumeCalcEdit);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.WeightUnitDropEdit);
			this.Controls.Add(this.PerWeightCalcEdit);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(CustomConversionFactor);
			this.DataSourceTypeName = "Enterprise.Rating.Business.CustomConversionFactor";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this, false);
			this.MinimizeBox = false;
			this.Name = "CustomConversionFactorForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Controls.SetChildIndex(this.PerWeightCalcEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.WeightUnitDropEdit, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.PerVolumeCalcEdit, 0);
			this.Controls.SetChildIndex(this.VolumeUnitDropEdit, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.ConversionFactorTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
