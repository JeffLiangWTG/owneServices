namespace Enterprise.MasterFiles.GUI
{
	partial class CreateTranslatedAddressForm
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
			this.AddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddressDetailsControlWithLanguage = new Enterprise.MasterFiles.GUI.AddressDetailsControlWithLanguage();
			this.AddressGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddressDetailsControlWithLanguage2 = new Enterprise.MasterFiles.GUI.AddressDetailsControlWithLanguage();
			this.tableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.tableLayoutPanel2 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.tableLayoutPanel3 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.SwitchButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.NoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.YesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LabelInformation = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AddressGroupBox.SuspendLayout();
			this.AddressDetailsControlWithLanguage.SuspendLayout();
			this.AddressGroupBox2.SuspendLayout();
			this.AddressDetailsControlWithLanguage2.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 334, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AddressMapper);
			// 
			// AddressGroupBox
			// 
			this.AddressGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AE61CD91-37F1-498E-88CA-B180583D22F3", "Primary Address");
			this.AddressGroupBox.Controls.Add(this.AddressDetailsControlWithLanguage);
			this.AddressGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AddressGroupBox.Name = "AddressGroupBox";
			this.AddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 257, true);
			this.AddressGroupBox.TabIndex = 2;
			this.AddressGroupBox.TabStop = false;
			// 
			// AddressDetailsControlWithLanguage
			// 
			this.AddressDetailsControlWithLanguage.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressDetailsControlWithLanguage, "Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ISupportWebAddressValidation)(((Enterprise.MasterFiles.Business.AddressMapper)(null)).Address1)));
			this.AddressDetailsControlWithLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressDetailsControlWithLanguage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AddressDetailsControlWithLanguage.Name = "AddressDetailsControlWithLanguage";
			this.AddressDetailsControlWithLanguage.ReadOnly = false;
			this.AddressDetailsControlWithLanguage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 238, true);
			this.AddressDetailsControlWithLanguage.TabIndex = 3;
			this.AddressDetailsControlWithLanguage.ValidationJustForced = false;
			// 
			// AddressGroupBox2
			// 
			this.AddressGroupBox2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4A14816F-6368-4BE9-944A-4F4AB6887EF8", "Translated Address");
			this.AddressGroupBox2.Controls.Add(this.AddressDetailsControlWithLanguage2);
			this.AddressGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 3, true);
			this.AddressGroupBox2.Name = "AddressGroupBox2";
			this.AddressGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 257, true);
			this.AddressGroupBox2.TabIndex = 6;
			this.AddressGroupBox2.TabStop = false;
			// 
			// AddressDetailsControlWithLanguage2
			// 
			this.AddressDetailsControlWithLanguage2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressDetailsControlWithLanguage2, "Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ISupportWebAddressValidation)(((Enterprise.MasterFiles.Business.AddressMapper)(null)).Address2)));
			this.AddressDetailsControlWithLanguage2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressDetailsControlWithLanguage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AddressDetailsControlWithLanguage2.Name = "AddressDetailsControlWithLanguage2";
			this.AddressDetailsControlWithLanguage2.ReadOnly = false;
			this.AddressDetailsControlWithLanguage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 238, true);
			this.AddressDetailsControlWithLanguage2.TabIndex = 7;
			this.AddressDetailsControlWithLanguage2.ValidationJustForced = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.LabelInformation, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(35)));
			this.tableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 334, true);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.AddressGroupBox, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.AddressGroupBox2, 2, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 33, true);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 263, true);
			this.tableLayoutPanel2.TabIndex = 1;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.SwitchButton, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 3, true);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200)));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 257, true);
			this.tableLayoutPanel3.TabIndex = 4;
			// 
			// SwitchButton
			// 
			this.SwitchButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.SwitchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SwitchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 77, true);
			this.SwitchButton.Name = "SwitchButton";
			this.SwitchButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SwitchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 46, true);
			this.SwitchButton.TabIndex = 5;
			this.SwitchButton.Text = "<    >";
			this.SwitchButton.ToolTipCaption = null;
			this.SwitchButton.Click += new System.EventHandler(this.SwitchButton_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.NoButton);
			this.panel1.Controls.Add(this.YesButton);
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(831, 302, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 29, true);
			this.panel1.TabIndex = 8;
			// 
			// NoButton
			// 
			this.NoButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9adac99a-e0b5-4cd5-a1a9-710a0c2d488d", "No");
			this.NoButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.NoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 3, true);
			this.NoButton.Name = "NoButton";
			this.NoButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NoButton.TabIndex = 10;
			this.NoButton.ToolTipCaption = null;
			// 
			// YesButton
			// 
			this.YesButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CreateTranslatedAddressForm|D5C159E6-6D29-49CD-B3FA-6AEA16F93DA2", "Yes");
			this.YesButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.YesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.YesButton.Name = "YesButton";
			this.YesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.YesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.YesButton.TabIndex = 9;
			this.YesButton.ToolTipCaption = null;
			// 
			// LabelInformation
			// 
			this.LabelInformation.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.LabelInformation.AutoSize = true;
			this.LabelInformation.BackColor = System.Drawing.SystemColors.Control;
			this.LabelInformation.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f1abc9ce-44e6-416f-b8b8-7c70c93fbc9a", "We have detected non-English characters in the main address for this organization. Please select which address you would like to set as Primary/Translated address.");
			this.LabelInformation.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelInformation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 8, true);
			this.LabelInformation.Name = "LabelInformation";
			this.LabelInformation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 13, true);
			this.LabelInformation.TabIndex = 2;
			// 
			// CreateTranslatedAddressForm
			// 
			this.AcceptButton = this.YesButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.NoButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 358, true);
			this.Controls.Add(this.tableLayoutPanel1);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AddressMapper);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 358, true);
			this.Name = "CreateTranslatedAddressForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AddressGroupBox.ResumeLayout(false);
			this.AddressGroupBox.PerformLayout();
			this.AddressDetailsControlWithLanguage.ResumeLayout(true);
			this.AddressDetailsControlWithLanguage.PerformLayout();
			this.AddressGroupBox2.ResumeLayout(false);
			this.AddressGroupBox2.PerformLayout();
			this.AddressDetailsControlWithLanguage2.ResumeLayout(true);
			this.AddressDetailsControlWithLanguage2.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel1;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel2;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel3;
		private CargoWise.Windows.UI.KPanel panel1;
		protected ZArchitecture.GUI.ZButton YesButton;
		protected ZArchitecture.GUI.ZButton NoButton;
		protected ZArchitecture.GUI.ZButton SwitchButton;
		private ZArchitecture.ZLabel LabelInformation;
		protected AddressDetailsControlWithLanguage AddressDetailsControlWithLanguage2;
		protected AddressDetailsControlWithLanguage AddressDetailsControlWithLanguage;
		protected ZArchitecture.GUI.ZGroupBox AddressGroupBox;
		protected ZArchitecture.GUI.ZGroupBox AddressGroupBox2;
	}
}