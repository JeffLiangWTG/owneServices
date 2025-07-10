namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCurrencyForm
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

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.RX_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RX_SymbolBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsExcludedCFXCalculationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RX_SubUnitRatioBoundDropDownEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UnitNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnitNameDropButton = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.UnitNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SubUnitLanguageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SubUnitLanguageDropButton = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.SubUnitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LanguageTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LanguageDropButton = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RX_ISOSubUnitRatioBoundDropDownEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExchangeRatesWarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnitNameDropButton)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SubUnitLanguageDropButton)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LanguageDropButton)).BeginInit();
			this.zStmNoteTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 416, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(305);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(305);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCurrency);
			// 
			// RX_CodeBoundTextBox
			// 
			this.RX_CodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RX_CodeBoundTextBox, "RX_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_Code)));
			this.RX_CodeBoundTextBox.CaptionResourceString = null;
			this.RX_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 11, true);
			this.RX_CodeBoundTextBox.Name = "RX_CodeBoundTextBox";
			this.RX_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.RX_CodeBoundTextBox.TabIndex = 0;
			// 
			// RX_SymbolBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RX_SymbolBoundTextBox, "RX_Symbol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_Symbol)));
			this.RX_SymbolBoundTextBox.CaptionResourceString = null;
			this.RX_SymbolBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 11, true);
			this.RX_SymbolBoundTextBox.Name = "RX_SymbolBoundTextBox";
			this.RX_SymbolBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.RX_SymbolBoundTextBox.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 387, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RX_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_IsActive)));
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(38, 100, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.IsActiveCheckBox.TabIndex = 13;
			// 
			// IsSystemCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "RX_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_IsSystem)));
			this.IsSystemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 100, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.IsSystemCheckBox.TabIndex = 14;
			// 
			// IsExcludedCFXCalculationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsExcludedCFXCalculationCheckBox, "RX_IsExcludedCFXCalculation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_IsExcludedCFXCalculation)));
			this.IsExcludedCFXCalculationCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1bebbfc7-6104-4a2f-90af-319e605a8f02", "Exclude from CFX Calculations");
			this.IsExcludedCFXCalculationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsExcludedCFXCalculationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsExcludedCFXCalculationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 100, true);
			this.IsExcludedCFXCalculationCheckBox.Name = "IsExcludedCFXCalculationCheckBox";
			this.IsExcludedCFXCalculationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 22, true);
			this.IsExcludedCFXCalculationCheckBox.TabIndex = 15;
			// 
			// RX_SubUnitRatioBoundDropDownEdit
			// 
			this.BindingSource.SetBindingMember(this.RX_SubUnitRatioBoundDropDownEdit, "RX_SubUnitRatio");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_SubUnitRatio)));
			this.RX_SubUnitRatioBoundDropDownEdit.CaptionResourceString = null;
			this.RX_SubUnitRatioBoundDropDownEdit.DecimalPlaces = 0;
			this.RX_SubUnitRatioBoundDropDownEdit.Decimals = 0;
			this.RX_SubUnitRatioBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 56, true);
			this.RX_SubUnitRatioBoundDropDownEdit.Name = "RX_SubUnitRatioBoundDropDownEdit";
			this.RX_SubUnitRatioBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 18, true);
			this.RX_SubUnitRatioBoundDropDownEdit.TabIndex = 8;
			this.RX_SubUnitRatioBoundDropDownEdit.Text = "0";
			this.RX_SubUnitRatioBoundDropDownEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(771, 377, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCurrencyForm|6c63e234-d501-44b3-a7e5-ac704fe5d27c", "Currency");
			this.MainTabPage.Controls.Add(this.UnitNameLabel);
			this.MainTabPage.Controls.Add(this.UnitNameDropButton);
			this.MainTabPage.Controls.Add(this.UnitNameTextBox);
			this.MainTabPage.Controls.Add(this.SubUnitLanguageLabel);
			this.MainTabPage.Controls.Add(this.SubUnitLanguageDropButton);
			this.MainTabPage.Controls.Add(this.SubUnitTextBox);
			this.MainTabPage.Controls.Add(this.LanguageTextLabel);
			this.MainTabPage.Controls.Add(this.LanguageDropButton);
			this.MainTabPage.Controls.Add(this.DescriptionTextBox);
			this.MainTabPage.Controls.Add(this.RX_ISOSubUnitRatioBoundDropDownEdit);
			this.MainTabPage.Controls.Add(this.IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.ExchangeRatesWarningLabel);
			this.MainTabPage.Controls.Add(this.IsSystemCheckBox);
			this.MainTabPage.Controls.Add(this.IsExcludedCFXCalculationCheckBox);
			this.MainTabPage.Controls.Add(this.RX_SubUnitRatioBoundDropDownEdit);
			this.MainTabPage.Controls.Add(this.RX_CodeBoundTextBox);
			this.MainTabPage.Controls.Add(this.RX_SymbolBoundTextBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 350, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// UnitNameLabel
			// 
			this.UnitNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.UnitNameLabel.BackColor = System.Drawing.SystemColors.Window;
			this.UnitNameLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.UnitNameLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("988E3AB5-FA4B-41DA-9ED1-0411EFBF74B0", "ENG");
			this.UnitNameLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.UnitNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnitNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 34, true);
			this.UnitNameLabel.Name = "UnitNameLabel";
			this.UnitNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.UnitNameLabel.TabIndex = 3;
			// 
			// UnitNameDropButton
			// 
			this.UnitNameDropButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.UnitNameDropButton.IsCaptionOverridden = false;
			this.UnitNameDropButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 34, true);
			this.UnitNameDropButton.Name = "UnitNameDropButton";
			this.UnitNameDropButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UnitNameDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 18, true);
			this.UnitNameDropButton.TabIndex = 4;
			this.UnitNameDropButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UnitNameDropButton.ToolTipCaption = null;
			this.UnitNameDropButton.UseVisualStyleBackColor = true;
			this.UnitNameDropButton.Click += new System.EventHandler(this.UnitNameDropButton_Click);
			// 
			// UnitNameTextBox
			// 
			this.UnitNameTextBox.AllowDrop = true;
			this.UnitNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UnitNameTextBox, "RX_UnitName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_UnitName)));
			this.UnitNameTextBox.CaptionResourceString = null;
			this.UnitNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 34, true);
			this.UnitNameTextBox.Name = "UnitNameTextBox";
			this.UnitNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.UnitNameTextBox.TabIndex = 2;
			// 
			// SubUnitLanguageLabel
			// 
			this.SubUnitLanguageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SubUnitLanguageLabel.BackColor = System.Drawing.SystemColors.Window;
			this.SubUnitLanguageLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SubUnitLanguageLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("D847AB39-A821-4F53-A52C-9ECED76E27FE", "ENG");
			this.SubUnitLanguageLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.SubUnitLanguageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SubUnitLanguageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(694, 35, true);
			this.SubUnitLanguageLabel.Name = "SubUnitLanguageLabel";
			this.SubUnitLanguageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.SubUnitLanguageLabel.TabIndex = 6;
			// 
			// SubUnitLanguageDropButton
			// 
			this.SubUnitLanguageDropButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SubUnitLanguageDropButton.IsCaptionOverridden = false;
			this.SubUnitLanguageDropButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(728, 35, true);
			this.SubUnitLanguageDropButton.Name = "SubUnitLanguageDropButton";
			this.SubUnitLanguageDropButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SubUnitLanguageDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 18, true);
			this.SubUnitLanguageDropButton.TabIndex = 7;
			this.SubUnitLanguageDropButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SubUnitLanguageDropButton.ToolTipCaption = null;
			this.SubUnitLanguageDropButton.UseVisualStyleBackColor = true;
			this.SubUnitLanguageDropButton.Click += new System.EventHandler(this.SubUnitLanguageDropButton_Click);
			// 
			// SubUnitTextBox
			// 
			this.SubUnitTextBox.AllowDrop = true;
			this.SubUnitTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SubUnitTextBox, "RX_SubUnitName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_SubUnitName)));
			this.SubUnitTextBox.CaptionResourceString = null;
			this.SubUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 35, true);
			this.SubUnitTextBox.Name = "SubUnitTextBox";
			this.SubUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.SubUnitTextBox.TabIndex = 5;
			// 
			// LanguageTextLabel
			// 
			this.LanguageTextLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LanguageTextLabel.BackColor = System.Drawing.SystemColors.Window;
			this.LanguageTextLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.LanguageTextLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7636F7D0-CF7B-48A8-9FE0-496289B4BA1B", "ENG");
			this.LanguageTextLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.LanguageTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LanguageTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 78, true);
			this.LanguageTextLabel.Name = "LanguageTextLabel";
			this.LanguageTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.LanguageTextLabel.TabIndex = 11;
			// 
			// LanguageDropButton
			// 
			this.LanguageDropButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LanguageDropButton.IsCaptionOverridden = false;
			this.LanguageDropButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 78, true);
			this.LanguageDropButton.Name = "LanguageDropButton";
			this.LanguageDropButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LanguageDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 18, true);
			this.LanguageDropButton.TabIndex = 12;
			this.LanguageDropButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.LanguageDropButton.ToolTipCaption = null;
			this.LanguageDropButton.UseVisualStyleBackColor = true;
			this.LanguageDropButton.Click += new System.EventHandler(this.LanguageDropButton_Click);
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.AllowDrop = true;
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "RX_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_Desc)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 78, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 18, true);
			this.DescriptionTextBox.TabIndex = 10;
			// 
			// RX_ISOSubUnitRatioBoundDropDownEdit
			// 
			this.BindingSource.SetBindingMember(this.RX_ISOSubUnitRatioBoundDropDownEdit, "RX_ISOSubUnitRatio");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefCurrency)(null)).RX_ISOSubUnitRatio)));
			this.RX_ISOSubUnitRatioBoundDropDownEdit.CaptionResourceString = null;
			this.RX_ISOSubUnitRatioBoundDropDownEdit.DecimalPlaces = 0;
			this.RX_ISOSubUnitRatioBoundDropDownEdit.Decimals = 0;
			this.RX_ISOSubUnitRatioBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 56, true);
			this.RX_ISOSubUnitRatioBoundDropDownEdit.Name = "RX_ISOSubUnitRatioBoundDropDownEdit";
			this.RX_ISOSubUnitRatioBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.RX_ISOSubUnitRatioBoundDropDownEdit.TabIndex = 9;
			this.RX_ISOSubUnitRatioBoundDropDownEdit.Text = "0";
			this.RX_ISOSubUnitRatioBoundDropDownEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExchangeRatesWarningLabel
			//
			this.ExchangeRatesWarningLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5211c277-88f9-47f8-9ba3-de58874730fb", "Maintenance of Exchange Rates is now done from the Exchange Rate module, this can be found in Maintain>Reference Files>Exchange Rates");
			this.ExchangeRatesWarningLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExchangeRatesWarningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ExchangeRatesWarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 171, true);
			this.ExchangeRatesWarningLabel.Name = "ExchangeRatesWarningLabel";
			this.ExchangeRatesWarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 43, true);
			this.ExchangeRatesWarningLabel.TabIndex = 9;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 350, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 350, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// RefCurrencyForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCurrencyForm|df13ed49-3a46-481d-bf50-5f6fb93fc4b5", "Currency");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 440, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCurrency);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 475, true);
			this.Name = "RefCurrencyForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnitNameDropButton)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SubUnitLanguageDropButton)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LanguageDropButton)).EndInit();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel LanguageTextLabel;
		private Enterprise.ZArchitecture.GUI.ZDropButtonOnly LanguageDropButton;
		private Enterprise.ZArchitecture.ZLabel SubUnitLanguageLabel;
		private Enterprise.ZArchitecture.GUI.ZDropButtonOnly SubUnitLanguageDropButton;
		private Enterprise.ZArchitecture.ZTextBox SubUnitTextBox;
		private Enterprise.ZArchitecture.ZLabel UnitNameLabel;
		private Enterprise.ZArchitecture.GUI.ZDropButtonOnly UnitNameDropButton;
		private Enterprise.ZArchitecture.ZTextBox UnitNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		private Enterprise.ZArchitecture.ZLabel ExchangeRatesWarningLabel;
		private Enterprise.ZArchitecture.ZTextBox RX_CodeBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox RX_SymbolBoundTextBox;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsSystemCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsExcludedCFXCalculationCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit RX_SubUnitRatioBoundDropDownEdit;
		private Enterprise.ZArchitecture.ZCalcEdit RX_ISOSubUnitRatioBoundDropDownEdit;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
	}
}
