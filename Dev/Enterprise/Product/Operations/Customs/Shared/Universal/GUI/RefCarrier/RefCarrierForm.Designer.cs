using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	partial class RefCarrierForm
	{
		private ZGroupBox AttributesGroupBox;
		private ZGroupBox BasicGroupBox;
		private ZCodeFindBox ZZ4_CountryOrGroupingCodeFindBox;
		private ZArchitecture.ZTextBox ZZ4_DescriptionTextBox;
		private ZPanel CodeTransportModesPanel;
		private ZArchitecture.ZLabel TransportModesLabel;
		private ZArchitecture.ZGrid AttributesGrid;
		private ZCheckBox ZZ4_IsSystemCheckBox;
		private ZArchitecture.ZTextBox ZZ4_CodeTextBox;

		#region Windows Form Designer generated code
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AttributesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttributesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BasicGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ZZ4_IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ZZ4_CountryOrGroupingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ZZ4_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodeTransportModesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportModesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ZZ4_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AttributesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesGrid)).BeginInit();
			this.AttributesGrid.SuspendLayout();
			this.BasicGroupBox.SuspendLayout();
			this.ZZ4_CountryOrGroupingCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1255, 275, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.AttributesGroupBox);
			this.MainTabPage.Controls.Add(this.BasicGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1247, 248, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1247, 248, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1247, 248, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1255, 275, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1255, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.ZZRefCarrierCombined);
			// 
			// AttributesGroupBox
			// 
			this.AttributesGroupBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("E3EECDDD-0EC4-4178-AF55-42CD58E01417", "Attributes");
			this.AttributesGroupBox.Controls.Add(this.AttributesGrid);
			this.AttributesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 0, true);
			this.AttributesGroupBox.Name = "AttributesGroupBox";
			this.AttributesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 248, true);
			this.AttributesGroupBox.TabIndex = 1;
			this.AttributesGroupBox.TabStop = false;
			// 
			// AttributesGrid
			// 
			this.AttributesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AttributesGrid, "Attributes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCarrierCombined)(null)).Attributes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCarrierAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCarrierCombined)(null)).Attributes)).SyncRoot)).ZZG_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCarrierAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCarrierCombined)(null)).Attributes)).SyncRoot)).ZZG_Value)));
			this.AttributesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "ZZG_Name";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo1.ColumnName = "ZZG_Value";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.AttributesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AttributesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGrid.GridId = "14737437-ef5f-477c-96ce-0cee1d35d7d1";
			this.AttributesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AttributesGrid.LayoutKey = "AttributesGrid";
			this.AttributesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AttributesGrid.Name = "AttributesGrid";
			this.AttributesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 229, true);
			this.AttributesGrid.TabIndex = 0;
			// 
			// BasicGroupBox
			// 
			this.BasicGroupBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("05A25BAF-CD7F-4455-9D31-68EC9E080AAD", "Basic Information");
			this.BasicGroupBox.Controls.Add(this.ZZ4_IsSystemCheckBox);
			this.BasicGroupBox.Controls.Add(this.ZZ4_CountryOrGroupingCodeFindBox);
			this.BasicGroupBox.Controls.Add(this.ZZ4_DescriptionTextBox);
			this.BasicGroupBox.Controls.Add(this.CodeTransportModesPanel);
			this.BasicGroupBox.Controls.Add(this.TransportModesLabel);
			this.BasicGroupBox.Controls.Add(this.ZZ4_CodeTextBox);
			this.BasicGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.BasicGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BasicGroupBox.Name = "BasicGroupBox";
			this.BasicGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 248, true);
			this.BasicGroupBox.TabIndex = 0;
			this.BasicGroupBox.TabStop = false;
			// 
			// ZZ4_IsSystemCheckBox
			// 
			this.ZZ4_IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ZZ4_IsSystemCheckBox, "ZZ4_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.ZZRefCarrierCombined)(null)).ZZ4_IsSystem)));
			this.ZZ4_IsSystemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ZZ4_IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ZZ4_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 182, true);
			this.ZZ4_IsSystemCheckBox.Name = "ZZ4_IsSystemCheckBox";
			this.ZZ4_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ZZ4_IsSystemCheckBox.TabIndex = 5;
			this.ZZ4_IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// ZZ4_CountryOrGroupingCodeFindBox
			// 
			this.ZZ4_CountryOrGroupingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZZ4_CountryOrGroupingCodeFindBox, "ZZ4_CountryOrGrouping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCarrierCombined)(null)).ZZ4_CountryOrGrouping)));
			this.ZZ4_CountryOrGroupingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 46, true);
			this.ZZ4_CountryOrGroupingCodeFindBox.Name = "ZZ4_CountryOrGroupingCodeFindBox";
			this.ZZ4_CountryOrGroupingCodeFindBox.PreBoundMaxLength = 2;
			this.ZZ4_CountryOrGroupingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ZZ4_CountryOrGroupingCodeFindBox.TabIndex = 0;
			// 
			// ZZ4_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZZ4_DescriptionTextBox, "ZZ4_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCarrierCombined)(null)).ZZ4_Description)));
			this.ZZ4_DescriptionTextBox.CaptionResourceString = null;
			this.ZZ4_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 98, true);
			this.ZZ4_DescriptionTextBox.Multiline = true;
			this.ZZ4_DescriptionTextBox.Name = "ZZ4_DescriptionTextBox";
			this.ZZ4_DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ZZ4_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 49, true);
			this.ZZ4_DescriptionTextBox.TabIndex = 2;
			// 
			// CodeTransportModesPanel
			// 
			this.CodeTransportModesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 153, true);
			this.CodeTransportModesPanel.Name = "CodeTransportModesPanel";
			this.CodeTransportModesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 23, true);
			this.CodeTransportModesPanel.TabIndex = 4;
			// 
			// TransportModesLabel
			// 
			this.TransportModesLabel.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("A5A18A3A-E31E-413B-AA82-F93B0A72C19E", "Transport Mode");
			this.TransportModesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TransportModesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 153, true);
			this.TransportModesLabel.Name = "TransportModesLabel";
			this.TransportModesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.TransportModesLabel.TabIndex = 3;
			this.TransportModesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ZZ4_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZZ4_CodeTextBox, "ZZ4_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCarrierCombined)(null)).ZZ4_Code)));
			this.ZZ4_CodeTextBox.CaptionResourceString = null;
			this.ZZ4_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 72, true);
			this.ZZ4_CodeTextBox.Name = "ZZ4_CodeTextBox";
			this.ZZ4_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.ZZ4_CodeTextBox.TabIndex = 1;
			// 
			// RefCarrierForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("C12CAF7F-AB5C-4A20-BC2C-5974FFC475EA", "Global Carrier");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1255, 331, true);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Universal.ZZRefCarrierCombined);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "RefCarrierForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "RefCarrierForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AttributesGroupBox.ResumeLayout(false);
			this.AttributesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesGrid)).EndInit();
			this.AttributesGrid.ResumeLayout(false);
			this.AttributesGrid.PerformLayout();
			this.BasicGroupBox.ResumeLayout(false);
			this.BasicGroupBox.PerformLayout();
			this.ZZ4_CountryOrGroupingCodeFindBox.ResumeLayout(true);
			this.ZZ4_CountryOrGroupingCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
