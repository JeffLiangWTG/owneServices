namespace Enterprise.MasterFiles.GUI
{
	partial class RecipientSelectionAdvancedSearchForm
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
		new void InitializeComponent()
		{
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RoleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ResetFiltersButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 205, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RecipientSelection);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("19748ac7-07e7-41f4-b9da-47a7325dd426", "Advanced Search");
			this.zGroupBox1.Controls.Add(this.RoleDropEdit);
			this.zGroupBox1.Controls.Add(this.LocationTextBox);
			this.zGroupBox1.Controls.Add(this.PhoneTextBox);
			this.zGroupBox1.Controls.Add(this.TitleTextBox);
			this.zGroupBox1.Controls.Add(this.NameTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 159, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// RoleDropEdit
			// 
			this.RoleDropEdit.AllowDrop = true;
			this.RoleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RoleDropEdit, "RoleQuery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).RoleQuery)));
			this.RoleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9c315947-7d96-4155-ad39-265076f62891", "Role");
			this.RoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 123, true);
			this.RoleDropEdit.Name = "RoleDropEdit";
			this.RoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.RoleDropEdit.TabIndex = 4;
			// 
			// LocationTextBox
			// 
			this.LocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LocationTextBox, "LocationQuery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).LocationQuery)));
			this.LocationTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2815c8bb-562e-496f-8a16-c3ab0a29da53", "Location");
			this.LocationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 97, true);
			this.LocationTextBox.Name = "LocationTextBox";
			this.LocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.LocationTextBox.TabIndex = 3;
			// 
			// PhoneTextBox
			// 
			this.PhoneTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PhoneTextBox, "PhoneQuery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).PhoneQuery)));
			this.PhoneTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1e268b7f-be5f-4202-8e00-281a4d21a7a6", "Phone");
			this.PhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 71, true);
			this.PhoneTextBox.Name = "PhoneTextBox";
			this.PhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.PhoneTextBox.TabIndex = 2;
			// 
			// TitleTextBox
			// 
			this.TitleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TitleTextBox, "TitleQuery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).TitleQuery)));
			this.TitleTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("598d5281-4d13-4dcb-8013-9cd286b3e3e4", "Title");
			this.TitleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 45, true);
			this.TitleTextBox.Name = "TitleTextBox";
			this.TitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.TitleTextBox.TabIndex = 1;
			// 
			// NameTextBox
			// 
			this.NameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NameTextBox, "NameQuery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RecipientSelection)(null)).NameQuery)));
			this.NameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("264b0209-37e0-4460-89b7-64ddd484645b", "Name");
			this.NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 19, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.NameTextBox.TabIndex = 0;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("75d7e81d-566c-48fa-8cc8-6982e8fcc616", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 168, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// NewCancelButton
			// 
			this.NewCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NewCancelButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e133a45c-3938-4a8c-87bd-d85cf0d4002f", "Cancel");
			this.NewCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.NewCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 168, true);
			this.NewCancelButton.Name = "NewCancelButton";
			this.NewCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NewCancelButton.TabIndex = 4;
			this.NewCancelButton.UseVisualStyleBackColor = true;
			this.NewCancelButton.Click += new System.EventHandler(this.NewCancelButton_Click);
			// 
			// ResetFiltersButton
			// 
			this.ResetFiltersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ResetFiltersButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ba0eada9-6b39-4a52-8cd0-0a4d62470534", "Reset filters");
			this.ResetFiltersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 168, true);
			this.ResetFiltersButton.Name = "ResetFiltersButton";
			this.ResetFiltersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ResetFiltersButton.TabIndex = 2;
			this.ResetFiltersButton.UseVisualStyleBackColor = true;
			this.ResetFiltersButton.Click += new System.EventHandler(this.ResetFiltersButton_Click);
			// 
			// RecipientSelectionAdvancedSearchForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.NewCancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2a6d6e6b-f6cb-4251-80ed-ef44a86fcf01", "Advanced Search");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 229, true);
			this.Controls.Add(this.ResetFiltersButton);
			this.Controls.Add(this.NewCancelButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.zGroupBox1);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RecipientSelection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 260, true);
			this.Name = "RecipientSelectionAdvancedSearchForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.NewCancelButton, 0);
			this.Controls.SetChildIndex(this.ResetFiltersButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZTextBox PhoneTextBox;
		private ZArchitecture.ZTextBox TitleTextBox;
		private ZArchitecture.ZTextBox NameTextBox;
		private ZArchitecture.GUI.ZDropEdit RoleDropEdit;
		private ZArchitecture.ZTextBox LocationTextBox;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton NewCancelButton;
		private ZArchitecture.GUI.ZButton ResetFiltersButton;

	}
}
