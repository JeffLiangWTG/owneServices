using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class GlobalTariffsHeaderUserControl
	{
		private ZArchitecture.ZCalcEdit GlobalRateLevelCalcEdit;
		private ZArchitecture.ZTranslatableTextControl GlobalRateDescriptionTextBox;
		private ZDropEdit DiscountTypeDropEdit;
		private ZArchitecture.ZCalcEdit DiscountCalcEdit;
		private ZArchitecture.ZLabel DiscountPercentSignLabel;
		private ZCodeFindBox ServiceLevelCodeFindBox;

		private void InitializeComponent()
		{
			this.GlobalRateLevelCalcEdit = new ZArchitecture.ZCalcEdit();
			this.GlobalRateDescriptionTextBox = new ZArchitecture.ZTranslatableTextControl();
			this.DiscountTypeDropEdit = new ZDropEdit();
			this.DiscountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.DiscountPercentSignLabel = new ZArchitecture.ZLabel();
			this.ServiceLevelCodeFindBox = new ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CompanyTariff);
			// 
			// GlobalRateLevelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GlobalRateLevelCalcEdit, "TH_GlobalRateLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CompanyTariff)(null)).TH_GlobalRateLevel);
			this.GlobalRateLevelCalcEdit.DecimalPlaces = 0;
			this.GlobalRateLevelCalcEdit.Decimals = 0;
			this.GlobalRateLevelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 3, true);
			this.GlobalRateLevelCalcEdit.Name = "GlobalRateLevelCalcEdit";
			this.GlobalRateLevelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.GlobalRateLevelCalcEdit.TabIndex = 1;
			this.GlobalRateLevelCalcEdit.Text = "0";
			this.GlobalRateLevelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GlobalRateDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GlobalRateDescriptionTextBox, "TH_GlobalRateDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CompanyTariff)(null)).TH_GlobalRateDescription);
			this.GlobalRateDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GlobalRateDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 26, true);
			this.GlobalRateDescriptionTextBox.Name = "GlobalRateDescriptionTextBox";
			this.GlobalRateDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.GlobalRateDescriptionTextBox.TabIndex = 3;
			// 
			// DiscountTypeDropEdit
			// 
			this.DiscountTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DiscountTypeDropEdit, "DiscountType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CompanyTariff)(null)).DiscountType);
			this.DiscountTypeDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("GlobalTariffsHeaderUserControl|f2b45dc2-9878-44a1-8080-6c951acb14c2", "Discount", "Discount for non Fees and Charges Rate Lines");
			this.DiscountTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 49, true);
			this.DiscountTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DiscountTypeDropEdit.Name = "DiscountTypeDropEdit";
			this.DiscountTypeDropEdit.PreBoundMaxLength = 3;
			this.DiscountTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.DiscountTypeDropEdit.TabIndex = 10;
			// 
			// DiscountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DiscountCalcEdit, "Discount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CompanyTariff)(null)).Discount);
			this.DiscountCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("GlobalTariffsHeaderUserControl|2b56c87d-5cf9-4306-857e-005466d1fca9", "", "Discount Percentage to be applied to non Fees and Charges Rate Lines");
			this.DiscountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 72, true);
			this.DiscountCalcEdit.DecimalPlaces = 2;
			this.DiscountCalcEdit.Name = "DiscountCalcEdit";
			this.DiscountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.DiscountCalcEdit.TabIndex = 12;
			this.DiscountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DiscountPercentSignLabel
			// 
			this.DiscountPercentSignLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 72, true);
			this.DiscountPercentSignLabel.Name = "DiscountPercentSignLabel";
			this.DiscountPercentSignLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 21, true);
			this.DiscountPercentSignLabel.TabIndex = 13;
			this.DiscountPercentSignLabel.Text = "%";
			// 
			// ServiceLevelCodeFindBox
			// 
			this.ServiceLevelCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelCodeFindBox, "DiscountServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CompanyTariff)(null)).DiscountServiceLevel);
			this.ServiceLevelCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("GlobalTariffsHeaderUserControl|dfb3b672-100e-44f9-8b18-cefa54f245fb", "Service Level");
			this.ServiceLevelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 72, true);
			this.ServiceLevelCodeFindBox.Name = "ServiceLevelCodeFindBox";
			this.ServiceLevelCodeFindBox.PreBoundMaxLength = 6;
			this.ServiceLevelCodeFindBox.ShowDescriptionBox = false;
			this.ServiceLevelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ServiceLevelCodeFindBox.TabIndex = 11;
			// 
			// GlobalTariffsHeaderUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ServiceLevelCodeFindBox);
			this.Controls.Add(this.DiscountPercentSignLabel);
			this.Controls.Add(this.DiscountCalcEdit);
			this.Controls.Add(this.DiscountTypeDropEdit);
			this.Controls.Add(this.GlobalRateLevelCalcEdit);
			this.Controls.Add(this.GlobalRateDescriptionTextBox);
			this.Name = "GlobalTariffsHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 95, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
