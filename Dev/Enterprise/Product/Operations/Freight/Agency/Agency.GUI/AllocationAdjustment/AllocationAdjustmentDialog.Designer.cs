namespace Enterprise.Freight.Agency.GUI
{
	partial class AllocationAdjustmentDialog
	{
		new void InitializeComponent()
		{
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.messageBodyLabel = new Enterprise.ZArchitecture.ZLabel();
			loginBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			passwordBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			authorizationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			bodyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			allocationDisplay = new Enterprise.Freight.Agency.GUI.AllocationAdjustmentDisplayTable();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			authorizationGroupBox.SuspendLayout();
			bodyPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 300, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 1;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails);
			// 
			// loginBoundTextBox
			// 
			loginBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(loginBoundTextBox, "Login");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).Login)));
			loginBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			loginBoundTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDialog|63ab9735-3356-457a-a4a9-e2769f3aed84", "Login Name", "The login name of a staff member with permissions to adjust allocation details.\r\n\r\nThe required security right is:\r\nOperations -> Schedules -> Sailing Schedule -> Allocation -> Edit.");
			loginBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 16, true);
			loginBoundTextBox.Name = "loginBoundTextBox";
			loginBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			loginBoundTextBox.TabIndex = 1;
			// 
			// passwordBoundTextBox
			// 
			passwordBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(passwordBoundTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails)(null)).Password)));
			passwordBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			passwordBoundTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDialog|88747af5-2409-4544-a9f7-ed687b71cf98", "Password", "The password corresponding to the login name.");
			passwordBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 16, true);
			passwordBoundTextBox.Name = "passwordBoundTextBox";
			passwordBoundTextBox.PasswordChar = '*';
			passwordBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			passwordBoundTextBox.TabIndex = 3;
			// 
			// authorizationGroupBox
			// 
			authorizationGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDialog|56c35313-9edd-4caa-a048-bc7b0f91d21e", "Authorization");
			authorizationGroupBox.Controls.Add(this.cancelButton);
			authorizationGroupBox.Controls.Add(this.okButton);
			authorizationGroupBox.Controls.Add(passwordBoundTextBox);
			authorizationGroupBox.Controls.Add(loginBoundTextBox);
			authorizationGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			authorizationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 225, true);
			authorizationGroupBox.Name = "authorizationGroupBox";
			authorizationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 70, true);
			authorizationGroupBox.TabIndex = 2;
			authorizationGroupBox.TabStop = false;
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDialog|1ade0e39-a7c0-4c69-99a5-df10ba0f6484", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 40, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelZButton_Click);
			// 
			// okButton
			// 
			this.okButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDialog|d810f152-0fd1-414c-a94f-cdbed4d079ac", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 40, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 4;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OkZButton_Click);
			// 
			// bodyPanel
			// 
			bodyPanel.Controls.Add(allocationDisplay);
			bodyPanel.Controls.Add(this.messageBodyLabel);
			bodyPanel.Controls.Add(authorizationGroupBox);
			bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			bodyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			bodyPanel.Name = "bodyPanel";
			bodyPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			bodyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 300, true);
			bodyPanel.TabIndex = 0;
			// 
			// allocationDisplay
			// 
			this.BindingSource.SetBindingMember(allocationDisplay, ".");
			allocationDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			allocationDisplay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 80, true);
			allocationDisplay.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 127, true);
			allocationDisplay.Name = "allocationDisplay";
			allocationDisplay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 145, true);
			allocationDisplay.TabIndex = 1;
			// 
			// messageBodyLabel
			// 
			this.messageBodyLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.messageBodyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.messageBodyLabel.Name = "messageBodyLabel";
			this.messageBodyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 75, true);
			this.messageBodyLabel.TabIndex = 0;
			this.messageBodyLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// AllocationAdjustmentDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 324, true);
			this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AllocationAdjustmentDialog|21f109ef-5ac9-47a1-942c-44a07eb0a46d", "Allocation Adjustment");
			this.Controls.Add(bodyPanel);
			this.DataSourceAssemblyName = "Enterprise.Freight.Agency.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails);
			this.DataSourceTypeName = "Enterprise.Freight.Agency.Business.AllocationAdjustmentDetails";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 350, true);
			this.MinimizeBox = false;
			this.Name = "AllocationAdjustmentDialog";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "AllocationAdjustmentDialog";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bodyPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			authorizationGroupBox.ResumeLayout(false);
			authorizationGroupBox.PerformLayout();
			bodyPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZLabel messageBodyLabel;
		Enterprise.ZArchitecture.GUI.ZButton okButton;
		Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		Enterprise.ZArchitecture.ZTextBox loginBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox passwordBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox authorizationGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel bodyPanel;
		Enterprise.Freight.Agency.GUI.AllocationAdjustmentDisplayTable allocationDisplay;

	}
}
