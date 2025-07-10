namespace Enterprise.Customs.US.Module.OperationalActions
{
	partial class SendEntrySummaryOperationActionControl
	{
		void InitializeComponent()
		{
			this.sendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.txtContactPhone = new Enterprise.ZArchitecture.ZTextBox();
			this.txtContactName = new Enterprise.ZArchitecture.ZTextBox();
			this.apportionWeight = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.sendWithErrorsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.sendRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.sendGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator);
			// 
			// SendGroupBox
			// 
			this.sendGroupBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("4a5a835a-1c1c-43a5-adb6-4535b5728066", "Send Messages");
			this.sendGroupBox.Controls.Add(this.txtContactPhone);
			this.sendGroupBox.Controls.Add(this.txtContactName);
			this.sendGroupBox.Controls.Add(this.apportionWeight);
			this.sendGroupBox.Controls.Add(this.sendWithErrorsRadioButton);
			this.sendGroupBox.Controls.Add(this.sendRadioButton);
			this.sendGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.sendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sendGroupBox.Name = "SendGroupBox";
			this.sendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 146, true);
			this.sendGroupBox.TabIndex = 0;
			this.sendGroupBox.TabStop = false;
			// 
			// txtContactPhone
			// 
			this.txtContactPhone.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.txtContactPhone, "ContactPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator)(null)).ContactPhone)));
			this.txtContactPhone.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("05337af0-57a6-4d18-849b-d228611f5748", "Contact Phone");
			this.txtContactPhone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 113, true);
			this.txtContactPhone.Name = "txtContactPhone";
			this.txtContactPhone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.txtContactPhone.TabIndex = 4;
			// 
			// txtContactName
			// 
			this.txtContactName.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.txtContactName, "ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator)(null)).ContactName)));
			this.txtContactName.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("8bf8e5a8-f0d1-40c1-87c4-f8dec579917c", "Contact Name");
			this.txtContactName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 87, true);
			this.txtContactName.Name = "txtContactName";
			this.txtContactName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.txtContactName.TabIndex = 3;
			// 
			// ApportionWeight
			// 
			this.BindingSource.SetBindingMember(this.apportionWeight, "ApportionWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator)(null)).ApportionWeight)));
			this.apportionWeight.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("b916cad9-0e0f-4199-824f-082315cc61de", "Apportion Weight ?");
			this.apportionWeight.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.apportionWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 65, true);
			this.apportionWeight.Name = "ApportionWeight";
			this.apportionWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 16, true);
			this.apportionWeight.TabIndex = 2;
			this.apportionWeight.UseVisualStyleBackColor = true;
			// 
			// SendWithErrorsRadioButton
			// 
			this.sendWithErrorsRadioButton.AutoCheck = false;
			this.sendWithErrorsRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.sendWithErrorsRadioButton, "SendWithMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator)(null)).SendWithMessageErrors)));
			this.sendWithErrorsRadioButton.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("18c69f47-031f-4348-8ec0-15e30a4eaf71", "Send ignoring errors");
			this.sendWithErrorsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.sendWithErrorsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 44, true);
			this.sendWithErrorsRadioButton.Name = "SendWithErrorsRadioButton";
			this.sendWithErrorsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 18, true);
			this.sendWithErrorsRadioButton.TabIndex = 1;
			this.sendWithErrorsRadioButton.UseVisualStyleBackColor = true;
			// 
			// SendRadioButton
			// 
			this.sendRadioButton.AutoCheck = false;
			this.sendRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.sendRadioButton, "Send");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Module.OperationalActions.SendEntrySummaryActionMethodApplicator)(null)).Send)));
			this.sendRadioButton.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("d1fb2e42-00cc-4399-9958-f124030c77f1", "Send (send only where there are no errors)");
			this.sendRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.sendRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 19, true);
			this.sendRadioButton.Name = "SendRadioButton";
			this.sendRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 19, true);
			this.sendRadioButton.TabIndex = 0;
			this.sendRadioButton.UseVisualStyleBackColor = true;
			// 
			// USDeclarationOperationActionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.sendGroupBox);
			this.Name = "USDeclarationOperationActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.sendGroupBox.ResumeLayout(false);
			this.sendGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZRadioButton sendWithErrorsRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton sendRadioButton;
		Enterprise.ZArchitecture.GUI.ZCheckBox apportionWeight;
		internal ZArchitecture.ZTextBox txtContactName;
		internal ZArchitecture.ZTextBox txtContactPhone;
		Enterprise.ZArchitecture.GUI.ZGroupBox sendGroupBox;
	}
}
