using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public abstract partial class BaseCombinedCalculatorControl
	{
		internal ZCheckBox UseAccumulatedCheckbox;
		internal ZCheckBox HigherChargeableLowerRateCheckBox;
		internal ZCheckBox UseInclusiveBreaksCheckBox;
		internal ZDropEditWithFixedWidth BreaksPerDropDown;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.UseAccumulatedCheckbox = new ZCheckBox();
			this.HigherChargeableLowerRateCheckBox = new ZCheckBox();
			this.UseInclusiveBreaksCheckBox = new ZCheckBox();
			this.BreaksPerDropDown = new ZDropEditWithFixedWidth();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DataSourceTypeForBinding;
			// 
			// UseAccumulatedCheckbox
			// 
			this.UseAccumulatedCheckbox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.UseAccumulatedCheckbox.AutoSize = true;
			this.UseAccumulatedCheckbox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|787700b9-b4fd-4546-8c33-4ec01957b160", "Cumulative Breaks");
			this.UseAccumulatedCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseAccumulatedCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.UseAccumulatedCheckbox.Name = "UseAccumulatedCheckbox";
			this.UseAccumulatedCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UseAccumulatedCheckbox.TabIndex = 1;
			// 
			// HigherChargeableLowerRateCheckBox
			// 
			this.HigherChargeableLowerRateCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.HigherChargeableLowerRateCheckBox.AutoSize = true;
			this.HigherChargeableLowerRateCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|125CF98A-FF6B-4BF8-91EA-49CCF9FA6BCA", "Higher Break Lower Rate");
			this.HigherChargeableLowerRateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HigherChargeableLowerRateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 130, true);
			this.HigherChargeableLowerRateCheckBox.Name = "HigherChargeableLowerRateCheckBox";
			this.HigherChargeableLowerRateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.HigherChargeableLowerRateCheckBox.TabIndex = 2;
			// 
			// UseInclusiveBreaksCheckBox
			// 
			this.UseInclusiveBreaksCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.UseInclusiveBreaksCheckBox.AutoSize = true;
			this.UseInclusiveBreaksCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|39232ab0-2a25-47bc-9ef9-2630df6e2052", "Inclusive Breaks");
			this.UseInclusiveBreaksCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseInclusiveBreaksCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 130, true);
			this.UseInclusiveBreaksCheckBox.Name = "UseInclusiveBreaksCheckBox";
			this.UseInclusiveBreaksCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UseInclusiveBreaksCheckBox.TabIndex = 3;
			// 
			// BreaksPer
			//
			this.BreaksPerDropDown.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BreaksPerDropDown.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|807ffe33-b95a-441b-89a3-a16a83630d3c", "Breaks Per");
			this.BreaksPerDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 124, true);
			this.BreaksPerDropDown.Name = "BreaksPerDropDown";
			this.BreaksPerDropDown.PreBoundMaxLength = 3;
			this.BreaksPerDropDown.DescriptionBox.Visible = false;
			this.BreaksPerDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 14, true);
			this.BreaksPerDropDown.TabIndex = 4;
			// 
			// BaseCombinedCalculatorControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HigherChargeableLowerRateCheckBox);
			this.Controls.Add(this.UseAccumulatedCheckbox);
			this.Controls.Add(this.UseInclusiveBreaksCheckBox);
			this.Controls.Add(this.BreaksPerDropDown);
			this.Name = "BaseCombinedCalculatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
