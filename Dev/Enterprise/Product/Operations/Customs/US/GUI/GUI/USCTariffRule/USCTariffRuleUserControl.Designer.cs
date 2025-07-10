namespace Enterprise.Customs.US.GUI
{
	partial class USCTariffRuleUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.RuleCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RuleCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RuleCodeDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TariffCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateFromLabel = new Enterprise.ZArchitecture.ZLabel();
			this.U1_DateFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.U1_DateToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExceptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExceptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.FormattedTariffToCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RuleCodeDropEdit.SuspendLayout();
			this.TariffCodeFindBox.SuspendLayout();
			this.U1_DateFromDateEdit.SuspendLayout();
			this.U1_DateToDateEdit.SuspendLayout();
			this.ExceptionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExceptionsGrid)).BeginInit();
			this.ExceptionsGrid.SuspendLayout();
			this.FormattedTariffToCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.USCTariffRule);
			// 
			// RuleCodeLabel
			// 
			this.RuleCodeLabel.AutoSize = true;
			this.RuleCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RuleCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 14, true);
			this.RuleCodeLabel.Name = "RuleCodeLabel";
			this.RuleCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.RuleCodeLabel.TabIndex = 0;
			this.RuleCodeLabel.Text = "Rule Code:";
			// 
			// RuleCodeDropEdit
			// 
			this.RuleCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RuleCodeDropEdit, "U1_RuleCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).U1_RuleCode)));
			this.RuleCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 10, true);
			this.RuleCodeDropEdit.Name = "RuleCodeDropEdit";
			this.RuleCodeDropEdit.PreBoundMaxLength = 3;
			this.RuleCodeDropEdit.ShowDescriptionBox = false;
			this.RuleCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.RuleCodeDropEdit.TabIndex = 0;
			// 
			// RuleCodeDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.RuleCodeDescriptionTextBox, "RuleCodeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).RuleCodeDescription)));
			this.RuleCodeDescriptionTextBox.CaptionResourceString = null;
			this.RuleCodeDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RuleCodeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 32, true);
			this.RuleCodeDescriptionTextBox.Multiline = true;
			this.RuleCodeDescriptionTextBox.Name = "RuleCodeDescriptionTextBox";
			this.RuleCodeDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.RuleCodeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 96, true);
			this.RuleCodeDescriptionTextBox.TabIndex = 1;
			// 
			// TariffLabel
			// 
			this.TariffLabel.AutoSize = true;
			this.TariffLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TariffLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 134, true);
			this.TariffLabel.Name = "TariffLabel";
			this.TariffLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.TariffLabel.TabIndex = 3;
			this.TariffLabel.Text = "Tariff From:";
			// 
			// TariffCodeFindBox
			// 
			this.TariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).FormattedTariff)));
			this.TariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 130, true);
			this.TariffCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.PreBoundMaxLength = 12;
			this.TariffCodeFindBox.ShouldResize = true;
			this.TariffCodeFindBox.ShowDescriptionBox = false;
			this.TariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.TariffCodeFindBox.TabIndex = 2;
			// 
			// DateFromLabel
			// 
			this.DateFromLabel.AutoSize = true;
			this.DateFromLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DateFromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 157, true);
			this.DateFromLabel.Name = "DateFromLabel";
			this.DateFromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 13, true);
			this.DateFromLabel.TabIndex = 5;
			this.DateFromLabel.Text = "Effective From:";
			// 
			// U1_DateFromDateEdit
			// 
			this.U1_DateFromDateEdit.AllowDrop = true;
			this.U1_DateFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.U1_DateFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.U1_DateFromDateEdit, "U1_DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).U1_DateFrom)));
			this.U1_DateFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 153, true);
			this.U1_DateFromDateEdit.Name = "U1_DateFromDateEdit";
			this.U1_DateFromDateEdit.TabIndex = 4;
			// 
			// U1_DateToDateEdit
			// 
			this.U1_DateToDateEdit.AllowDrop = true;
			this.U1_DateToDateEdit.AutoCompleteMonthThreshold = 1;
			this.U1_DateToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.U1_DateToDateEdit, "U1_DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).U1_DateTo)));
			this.U1_DateToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 153, true);
			this.U1_DateToDateEdit.Name = "U1_DateToDateEdit";
			this.U1_DateToDateEdit.TabIndex = 5;
			// 
			// DateToLabel
			// 
			this.DateToLabel.AutoSize = true;
			this.DateToLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DateToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 157, true);
			this.DateToLabel.Name = "DateToLabel";
			this.DateToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 13, true);
			this.DateToLabel.TabIndex = 7;
			this.DateToLabel.Text = "To:";
			// 
			// ExceptionsGroupBox
			// 
			this.ExceptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ExceptionsGroupBox.Controls.Add(this.ExceptionsGrid);
			this.ExceptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 179, true);
			this.ExceptionsGroupBox.Name = "ExceptionsGroupBox";
			this.ExceptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 159, true);
			this.ExceptionsGroupBox.TabIndex = 6;
			this.ExceptionsGroupBox.TabStop = false;
			this.ExceptionsGroupBox.Text = "Exceptions";
			// 
			// ExceptionsGrid
			// 
			this.ExceptionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExceptionsGrid, "RuleExceptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).RuleExceptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCTariffRuleException)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).RuleExceptions)).SyncRoot)).FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCTariffRuleException)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).RuleExceptions)).SyncRoot)).FormattedTariffTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.USCTariffRuleException)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).RuleExceptions)).SyncRoot)).U2_DateFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.USCTariffRuleException)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).RuleExceptions)).SyncRoot)).U2_DateTo)));
			this.ExceptionsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.Caption = "Tariff From";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "FormattedTariff";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCodeFindBoxColumnStyleInfo2.Caption = "Tariff To";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "FormattedTariffTo";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo1.Caption = "Date From";
			zDateEditColumnStyleInfo1.ColumnName = "U2_DateFrom";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Date To";
			zDateEditColumnStyleInfo2.ColumnName = "U2_DateTo";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsMandatory = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ExceptionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ExceptionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ExceptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExceptionsGrid.GridId = "8053b13a-7ef7-407e-ace3-fe7c3d78c54c";
			this.ExceptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExceptionsGrid.LayoutKey = "ExceptionsGrid";
			this.ExceptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ExceptionsGrid.Name = "ExceptionsGrid";
			this.ExceptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 140, true);
			this.ExceptionsGrid.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 157, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 13, true);
			this.zLabel1.TabIndex = 10;
			this.zLabel1.Text = "(Leave \'To\' empty if unknown)";
			// 
			// FormattedTariffToCodeFindBox
			// 
			this.FormattedTariffToCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FormattedTariffToCodeFindBox, "FormattedTariffTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCTariffRule)(null)).FormattedTariffTo)));
			this.FormattedTariffToCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 130, true);
			this.FormattedTariffToCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			this.FormattedTariffToCodeFindBox.Name = "FormattedTariffToCodeFindBox";
			this.FormattedTariffToCodeFindBox.PreBoundMaxLength = 12;
			this.FormattedTariffToCodeFindBox.ShouldResize = true;
			this.FormattedTariffToCodeFindBox.ShowDescriptionBox = false;
			this.FormattedTariffToCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.FormattedTariffToCodeFindBox.TabIndex = 3;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 133, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 13, true);
			this.zLabel2.TabIndex = 12;
			this.zLabel2.Text = "To:";
			// 
			// USCTariffRuleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.FormattedTariffToCodeFindBox);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ExceptionsGroupBox);
			this.Controls.Add(this.U1_DateToDateEdit);
			this.Controls.Add(this.DateToLabel);
			this.Controls.Add(this.U1_DateFromDateEdit);
			this.Controls.Add(this.DateFromLabel);
			this.Controls.Add(this.TariffCodeFindBox);
			this.Controls.Add(this.TariffLabel);
			this.Controls.Add(this.RuleCodeDescriptionTextBox);
			this.Controls.Add(this.RuleCodeDropEdit);
			this.Controls.Add(this.RuleCodeLabel);
			this.Name = "USCTariffRuleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 338, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RuleCodeDropEdit.ResumeLayout(true);
			this.RuleCodeDropEdit.PerformLayout();
			this.TariffCodeFindBox.ResumeLayout(true);
			this.TariffCodeFindBox.PerformLayout();
			this.U1_DateFromDateEdit.ResumeLayout(true);
			this.U1_DateFromDateEdit.PerformLayout();
			this.U1_DateToDateEdit.ResumeLayout(true);
			this.U1_DateToDateEdit.PerformLayout();
			this.ExceptionsGroupBox.ResumeLayout(false);
			this.ExceptionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExceptionsGrid)).EndInit();
			this.ExceptionsGrid.ResumeLayout(false);
			this.ExceptionsGrid.PerformLayout();
			this.FormattedTariffToCodeFindBox.ResumeLayout(true);
			this.FormattedTariffToCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel RuleCodeLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RuleCodeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox RuleCodeDescriptionTextBox;
		private Enterprise.ZArchitecture.ZLabel TariffLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox TariffCodeFindBox;
		private Enterprise.ZArchitecture.ZLabel DateFromLabel;
		private Enterprise.ZArchitecture.GUI.ZDateEdit U1_DateFromDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit U1_DateToDateEdit;
		private Enterprise.ZArchitecture.ZLabel DateToLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ExceptionsGroupBox;
		private Enterprise.ZArchitecture.ZGrid ExceptionsGrid;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox FormattedTariffToCodeFindBox;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
	}
}
