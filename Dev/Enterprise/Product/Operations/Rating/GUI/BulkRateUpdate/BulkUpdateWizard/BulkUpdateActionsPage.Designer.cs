namespace Enterprise.Rating.GUI
{
	public partial class BulkUpdateActionsPage
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.NewEntryEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.NewEntryStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CreateNewEntryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeleteRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.IncreaseChargeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ReplaceRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AddChargeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.rateLineControl = new Enterprise.Rating.GUI.RateLineControl();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BothRatesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AgentRatesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.StandardRatesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NewEntryEndDateEdit.SuspendLayout();
			this.NewEntryStartDateEdit.SuspendLayout();
			this.rateLineControl.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.BulkRateUpdater);
			// 
			// NewEntryEndDateEdit
			// 
			this.NewEntryEndDateEdit.AllowDrop = true;
			this.NewEntryEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.NewEntryEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.NewEntryEndDateEdit, "NewEntryEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).NewEntryEndDate)));
			this.NewEntryEndDateEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|970db8ef-b040-4af1-97db-9f379eaa92c9", "End Date");
			this.NewEntryEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 77, true);
			this.NewEntryEndDateEdit.Name = "NewEntryEndDateEdit";
			this.NewEntryEndDateEdit.Visible = CreateNewEntryCheckBox.Checked;
			this.NewEntryEndDateEdit.TabIndex = 19;
			// 
			// NewEntryStartDateEdit
			// 
			this.NewEntryStartDateEdit.AllowDrop = true;
			this.NewEntryStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.NewEntryStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.NewEntryStartDateEdit, "NewEntryStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).NewEntryStartDate)));
			this.NewEntryStartDateEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|1002b32a-cc13-48bc-a791-d807419baee1", "Start Date");
			this.NewEntryStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 51, true);
			this.NewEntryStartDateEdit.Name = "NewEntryStartDateEdit";
			this.NewEntryStartDateEdit.Visible = CreateNewEntryCheckBox.Checked;
			this.NewEntryStartDateEdit.TabIndex = 18;
			// 
			// CreateNewEntryCheckBox
			// 
			this.CreateNewEntryCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CreateNewEntryCheckBox, "CreateNewEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).CreateNewEntry)));
			this.CreateNewEntryCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|b45fdb85-8e04-4cc2-89c4-1292a5b64d3d", "Apply Changes for Specified Dates Only");
			this.CreateNewEntryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CreateNewEntryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 16, true);
			this.CreateNewEntryCheckBox.Name = "CreateNewEntryCheckBox";
			this.CreateNewEntryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 17, true);
			this.CreateNewEntryCheckBox.TabIndex = 17;
			// 
			// DeleteRadioButton
			// 
			this.DeleteRadioButton.AutoCheck = false;
			this.DeleteRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DeleteRadioButton, "DeleteCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).DeleteCharge)));
			this.DeleteRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|4eb59ef6-0348-4d0d-8bae-9199d3895534", "Delete Charge");
			this.DeleteRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeleteRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 179, true);
			this.DeleteRadioButton.Name = "DeleteRadioButton";
			this.DeleteRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 17, true);
			this.DeleteRadioButton.TabIndex = 15;
			// 
			// IncreaseChargeRadioButton
			// 
			this.IncreaseChargeRadioButton.AutoCheck = false;
			this.IncreaseChargeRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncreaseChargeRadioButton, "IncreaseDecreaseCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).IncreaseDecreaseCharge)));
			this.IncreaseChargeRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|1137e39a-3fc1-45ba-893e-6e6fc8c193b2", "Increase / Decrease Charge", "Increase / Decrease Charge. Please note this functionality is incompatible with rate lines using the Cost Based Calculator (CST) or Company Tariff Based Calculator (CTB).");
			this.IncreaseChargeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncreaseChargeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 72, true);
			this.IncreaseChargeRadioButton.Name = "IncreaseChargeRadioButton";
			this.IncreaseChargeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 17, true);
			this.IncreaseChargeRadioButton.TabIndex = 12;
			// 
			// ReplaceRadioButton
			// 
			this.ReplaceRadioButton.AutoCheck = false;
			this.ReplaceRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReplaceRadioButton, "ReplaceCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ReplaceCharge)));
			this.ReplaceRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|cc6341ac-eefd-454e-94fc-29d0e1f9f4d1", "Replace Existing Charge");
			this.ReplaceRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReplaceRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 40, true);
			this.ReplaceRadioButton.Name = "ReplaceRadioButton";
			this.ReplaceRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 17, true);
			this.ReplaceRadioButton.TabIndex = 11;
			// 
			// AddChargeRadioButton
			// 
			this.AddChargeRadioButton.AutoCheck = false;
			this.AddChargeRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AddChargeRadioButton, "AddOrReplaceCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).AddOrReplaceCharge)));
			this.AddChargeRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|09f6c4e2-fbf1-4d2e-8701-cd1a13569587", "Add or Replace Charge");
			this.AddChargeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AddChargeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 8, true);
			this.AddChargeRadioButton.Name = "AddChargeRadioButton";
			this.AddChargeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.AddChargeRadioButton.TabIndex = 10;
			// 
			// rateLineControl
			// 
			this.rateLineControl.AllowDrop = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Rating.Business.RateLine)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).ActionsLine)));
			this.rateLineControl.CalculatorPanelAgentRatesCheckBoxVisible = false;
			this.rateLineControl.CalculatorPanelCalculatorDropEditVisible = false;
			this.rateLineControl.CalculatorPanelVisible = false;
			this.rateLineControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 8, true);
			this.rateLineControl.Name = "rateLineControl";
			this.rateLineControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 405, true);
			this.rateLineControl.TabIndex = 16;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.BothRatesRadioButton);
			this.zPanel1.Controls.Add(this.AgentRatesRadioButton);
			this.zPanel1.Controls.Add(this.StandardRatesRadioButton);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 96, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 74, true);
			this.zPanel1.TabIndex = 13;
			// 
			// BothRatesRadioButton
			// 
			this.BothRatesRadioButton.AutoCheck = false;
			this.BothRatesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BothRatesRadioButton, "UpdateBothRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).UpdateBothRates)));
			this.BothRatesRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|8c7faf68-0b02-4f82-982f-d46f2de8bb23", "Both Rates", "Increase / Decrease Both Standard Rates And Agent Rates");
			this.BothRatesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BothRatesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.BothRatesRadioButton.Name = "BothRatesRadioButton";
			this.BothRatesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.BothRatesRadioButton.TabIndex = 2;
			this.BothRatesRadioButton.TabStop = true;
			this.BothRatesRadioButton.UseVisualStyleBackColor = true;
			// 
			// AgentRatesRadioButton
			// 
			this.AgentRatesRadioButton.AutoCheck = false;
			this.AgentRatesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AgentRatesRadioButton, "UpdateAgentRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).UpdateAgentRates)));
			this.AgentRatesRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|fcb7489a-e381-49c2-8c71-5a48c5c61bba", "Agent Rates", "Increase / Decrease Agent Rates");
			this.AgentRatesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AgentRatesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 5, true);
			this.AgentRatesRadioButton.Name = "AgentRatesRadioButton";
			this.AgentRatesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
			this.AgentRatesRadioButton.TabIndex = 1;
			this.AgentRatesRadioButton.TabStop = true;
			this.AgentRatesRadioButton.UseVisualStyleBackColor = true;
			// 
			// StandardRatesRadioButton
			// 
			this.StandardRatesRadioButton.AutoCheck = false;
			this.StandardRatesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.StandardRatesRadioButton, "UpdateStandardRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.BulkRateUpdater)(null)).UpdateStandardRates)));
			this.StandardRatesRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("BulkUpdateActionsPage|503cc875-7a15-4837-a203-5b1a325b597c", "Standard Rates", "Increase / Decrease Standard Rates");
			this.StandardRatesRadioButton.Checked = true;
			this.StandardRatesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.StandardRatesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, -19, true);
			this.StandardRatesRadioButton.Name = "StandardRatesRadioButton";
			this.StandardRatesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 17, true);
			this.StandardRatesRadioButton.TabIndex = 0;
			this.StandardRatesRadioButton.TabStop = true;
			this.StandardRatesRadioButton.UseVisualStyleBackColor = true;
			// 
			// BulkUpdateActionsPage
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.NewEntryEndDateEdit);
			this.Controls.Add(this.NewEntryStartDateEdit);
			this.Controls.Add(this.CreateNewEntryCheckBox);
			this.Controls.Add(this.DeleteRadioButton);
			this.Controls.Add(this.IncreaseChargeRadioButton);
			this.Controls.Add(this.ReplaceRadioButton);
			this.Controls.Add(this.AddChargeRadioButton);
			this.Controls.Add(this.rateLineControl);
			this.Name = "BulkUpdateActionsPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 475, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NewEntryEndDateEdit.ResumeLayout(true);
			this.NewEntryEndDateEdit.PerformLayout();
			this.NewEntryStartDateEdit.ResumeLayout(true);
			this.NewEntryStartDateEdit.PerformLayout();
			this.rateLineControl.ResumeLayout(true);
			this.rateLineControl.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZDateEdit NewEntryEndDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit NewEntryStartDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CreateNewEntryCheckBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton DeleteRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton IncreaseChargeRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ReplaceRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton AddChargeRadioButton;
		private RateLineControl rateLineControl;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZRadioButton AgentRatesRadioButton;
		private ZArchitecture.GUI.ZRadioButton StandardRatesRadioButton;
		private ZArchitecture.GUI.ZRadioButton BothRatesRadioButton;
	}
}
