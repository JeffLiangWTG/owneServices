namespace Enterprise.Customs.GUI
{
	partial class SupervisorOverridesForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UserCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.UserDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UserPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UserPasswordLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AllowButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DisallowButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessagesForLogGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ApproveGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UserCodeFindBox.SuspendLayout();
			this.UserDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesForLogGrid)).BeginInit();
			this.MessagesForLogGrid.SuspendLayout();
			this.ApproveGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 361, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.SupervisorOverrides);
			// 
			// UserCodeFindBox
			// 
			this.UserCodeFindBox.AllowDrop = true;
			this.UserCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UserCodeFindBox, "SupervisorName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SupervisorOverrides)(null)).SupervisorName)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UserCodeFindBox, false);
			this.UserCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 21, true);
			this.UserCodeFindBox.Name = "UserCodeFindBox";
			this.UserCodeFindBox.PreBoundMaxLength = 3;
			this.UserCodeFindBox.ShouldResize = true;
			this.UserCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 20, true);
			this.UserCodeFindBox.TabIndex = 1;
			// 
			// UserDetailsGroupBox
			// 
			this.UserDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.UserDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5E7DF866-B20B-4857-A6F5-50B1C7A2D0BF", "Supervisor Login Details");
			this.UserDetailsGroupBox.Controls.Add(this.UserPasswordTextBox);
			this.UserDetailsGroupBox.Controls.Add(this.UserPasswordLabel);
			this.UserDetailsGroupBox.Controls.Add(this.UserNameLabel);
			this.UserDetailsGroupBox.Controls.Add(this.UserCodeFindBox);
			this.UserDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.UserDetailsGroupBox.Name = "UserDetailsGroupBox";
			this.UserDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 72, true);
			this.UserDetailsGroupBox.TabIndex = 0;
			this.UserDetailsGroupBox.TabStop = false;
			// 
			// UserPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.UserPasswordTextBox, "SupervisorPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SupervisorOverrides)(null)).SupervisorPassword)));
			this.UserPasswordTextBox.CaptionResourceString = null;
			this.UserPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UserPasswordTextBox, false);
			this.UserPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 47, true);
			this.UserPasswordTextBox.Name = "UserPasswordTextBox";
			this.UserPasswordTextBox.PasswordChar = '*';
			this.UserPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.UserPasswordTextBox.TabIndex = 3;
			// 
			// UserPasswordLabel
			// 
			this.UserPasswordLabel.AutoSize = true;
			this.UserPasswordLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B1CA77E2-F869-45A3-9CA2-B1E4DD001CF8", "Password:");
			this.UserPasswordLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UserPasswordLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 50, true);
			this.UserPasswordLabel.Name = "UserPasswordLabel";
			this.UserPasswordLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.UserPasswordLabel.TabIndex = 2;
			// 
			// UserNameLabel
			// 
			this.UserNameLabel.AutoSize = true;
			this.UserNameLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5364E732-AFD8-4B1C-9063-BF12763645EE", "User Name:");
			this.UserNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UserNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 24, true);
			this.UserNameLabel.Name = "UserNameLabel";
			this.UserNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.UserNameLabel.TabIndex = 0;
			// 
			// AllowButton
			// 
			this.AllowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AllowButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5B4FCAF2-41A1-4513-AECA-A3F6D3CF875C", "&OK");
			this.AllowButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 331, true);
			this.AllowButton.Name = "AllowButton";
			this.AllowButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AllowButton.TabIndex = 4;
			this.AllowButton.TabStop = false;
			this.AllowButton.UseVisualStyleBackColor = true;
			this.AllowButton.Click += new System.EventHandler(this.AllowButton_Click);
			// 
			// DisallowButton
			// 
			this.DisallowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DisallowButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C8F8231E-A555-46E9-A51E-C55D8C5F9A3A", "&Cancel");
			this.DisallowButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 331, true);
			this.DisallowButton.Name = "DisallowButton";
			this.DisallowButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DisallowButton.TabIndex = 5;
			this.DisallowButton.TabStop = false;
			this.DisallowButton.UseVisualStyleBackColor = true;
			this.DisallowButton.Click += new System.EventHandler(this.DisallowButton_Click);
			// 
			// MessagesForLogGrid
			// 
			this.MessagesForLogGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesForLogGrid, "UnAuthorisedMessagesForLog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.SupervisorOverrides)(null)).UnAuthorisedMessagesForLog)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.MessageLog)(((System.Collections.IList)(((Enterprise.Customs.Business.SupervisorOverrides)(null)).UnAuthorisedMessagesForLog)).SyncRoot)).Message)));
			this.MessagesForLogGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("584D0CB7-0A8D-4335-8CB6-C81A11DD2500", "Message");
			zTextBoxColumnStyleInfo1.ColumnName = "Message";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.MessagesForLogGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesForLogGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesForLogGrid.GridId = "2ee1c5be-8b05-446c-ab18-a152158b471b";
			this.MessagesForLogGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesForLogGrid.LayoutKey = "zGrid1";
			this.MessagesForLogGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesForLogGrid.Name = "MessagesForLogGrid";
			this.MessagesForLogGrid.ReadOnly = true;
			this.MessagesForLogGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 218, true);
			this.MessagesForLogGrid.TabIndex = 0;
			// 
			// ApproveGroupBox
			// 
			this.ApproveGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ApproveGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5B9845E3-1F51-4D69-A976-3A3305D32A2E", "Approve the following Actions");
			this.ApproveGroupBox.Controls.Add(this.MessagesForLogGrid);
			this.ApproveGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 87, true);
			this.ApproveGroupBox.Name = "ApproveGroupBox";
			this.ApproveGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 237, true);
			this.ApproveGroupBox.TabIndex = 1;
			this.ApproveGroupBox.TabStop = false;
			// 
			// SupervisorOverridesForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("AAF6059D-6D9C-40CF-A071-ED8604695171", "Supervisor Overrides");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 385, true);
			this.Controls.Add(this.UserDetailsGroupBox);
			this.Controls.Add(this.ApproveGroupBox);
			this.Controls.Add(this.AllowButton);
			this.Controls.Add(this.DisallowButton);
			this.DataSourceType = typeof(Enterprise.Customs.Business.SupervisorOverrides);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "SupervisorOverridesForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.DisallowButton, 0);
			this.Controls.SetChildIndex(this.AllowButton, 0);
			this.Controls.SetChildIndex(this.ApproveGroupBox, 0);
			this.Controls.SetChildIndex(this.UserDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UserCodeFindBox.ResumeLayout(true);
			this.UserCodeFindBox.PerformLayout();
			this.UserDetailsGroupBox.ResumeLayout(false);
			this.UserDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesForLogGrid)).EndInit();
			this.MessagesForLogGrid.ResumeLayout(false);
			this.MessagesForLogGrid.PerformLayout();
			this.ApproveGroupBox.ResumeLayout(false);
			this.ApproveGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCodeFindBox UserCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox UserDetailsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox UserPasswordTextBox;
		private Enterprise.ZArchitecture.ZLabel UserPasswordLabel;
		private Enterprise.ZArchitecture.ZLabel UserNameLabel;
		private Enterprise.ZArchitecture.GUI.ZButton AllowButton;
		private Enterprise.ZArchitecture.GUI.ZButton DisallowButton;
		private Enterprise.ZArchitecture.ZGrid MessagesForLogGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ApproveGroupBox;
	}
}
