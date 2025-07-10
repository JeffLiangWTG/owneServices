using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusCodeListForm
	{
		private ZGroupBox AttributesGroupBox;
		private ZArchitecture.ZGrid AttributesGrid;
		private ZGroupBox BasicGroupBox;
		private ZCheckBox ZZD_IsSystemCheckBox;
		private ZCodeFindBox ZZD_CountryOrGroupingCodeFindBox;
		private ZDateEdit ZZD_EndDateDateEdit;
		private ZDateEdit ZZD_StartDateDateEdit;
		private ZArchitecture.ZTextBox ZZD_DescriptionTextBox;
		private ZDropEdit ZZD_CodeTypeDropEdit;
		private ZPanel CodeTransportModesPanel;
		private ZArchitecture.ZTextBox ZZD_CodeTextBox;
		private ZGroupBox AttributeTransportModesGroupBox;
		private ZPanel AttrTransportModesPanel;
		private ZArchitecture.ZLabel TransportModesLabel;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AttributesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttributesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AttributeTransportModesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttrTransportModesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BasicGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ZZD_IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ZZD_CountryOrGroupingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ZZD_EndDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ZZD_StartDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ZZD_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZZD_CodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CodeTransportModesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportModesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ZZD_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LanguagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LanguagesGrid = new Enterprise.ZArchitecture.ZGrid();
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
			this.AttributeTransportModesGroupBox.SuspendLayout();
			this.BasicGroupBox.SuspendLayout();
			this.ZZD_CountryOrGroupingCodeFindBox.SuspendLayout();
			this.ZZD_EndDateDateEdit.SuspendLayout();
			this.ZZD_StartDateDateEdit.SuspendLayout();
			this.ZZD_CodeTypeDropEdit.SuspendLayout();
			this.LanguagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LanguagesGrid)).BeginInit();
			this.LanguagesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1255, 331, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.AttributesGroupBox);
			this.MainTabPage.Controls.Add(this.LanguagesGroupBox);
			this.MainTabPage.Controls.Add(this.BasicGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1247, 304, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1247, 304, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1247, 304, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1255, 331, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1255, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.ZZRefCusCodeListCombined);
			// 
			// AttributesGroupBox
			// 
			this.AttributesGroupBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("fc8e7753-1ff6-4fc6-b532-594daad9c7b4", "Attributes");
			this.AttributesGroupBox.Controls.Add(this.AttributesGrid);
			this.AttributesGroupBox.Controls.Add(this.AttributeTransportModesGroupBox);
			this.AttributesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 112, true);
			this.AttributesGroupBox.Name = "AttributesGroupBox";
			this.AttributesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 192, true);
			this.AttributesGroupBox.TabIndex = 4;
			this.AttributesGroupBox.TabStop = false;
			// 
			// AttributesGrid
			// 
			this.AttributesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AttributesGrid, "Attributes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)).SyncRoot)).CodeListAttributeNamePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)).SyncRoot)).NameDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)).SyncRoot)).ZZE_ValueDecimalPlaces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)).SyncRoot)).ZZE_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)).SyncRoot)).DescriptionOfZZE_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)).SyncRoot)).ZZE_ValueDataFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)).SyncRoot)).ZZE_TransportModes)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)).SyncRoot)).ZZE_StartDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Attributes)).SyncRoot)).ZZE_EndDate)));
			this.AttributesGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("9115fddd-65c1-48ad-b104-5b8a564ef116", "Name");
			zGuidDropEditColumnStyleInfo1.ColumnName = "CodeListAttributeNamePK";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("808186f4-89ae-46b0-9414-e10fe6d3530f", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "NameDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = "ZZE_ValueDecimalPlaces";
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("a511528a-5498-4701-afcc-d6b59af60b8f", "Value");
			zMultiControlColumnStyleInfo1.ColumnName = "ZZE_Value";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ZZE_ValueDataFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.ColumnName = "DescriptionOfZZE_Value";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("686717aa-f124-4e85-b04a-2a0ea2fec759", "Transport Modes");
			zTextBoxColumnStyleInfo2.ColumnName = "ZZE_TransportModes";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            zDateEditColumnStyleInfo1.ColumnName = "ZZE_StartDate";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo2.ColumnName = "ZZE_EndDate";
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AttributesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AttributesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AttributesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.AttributesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.AttributesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.AttributesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGrid.GridId = "14737437-ef5f-477c-96ce-0cee1d35d7d1";
			this.AttributesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AttributesGrid.LayoutKey = "AttributesGrid";
			this.AttributesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AttributesGrid.Name = "AttributesGrid";
			this.AttributesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 104, true);
			this.AttributesGrid.TabIndex = 0;
			// 
			// AttributeTransportModesGroupBox
			// 
			this.AttributeTransportModesGroupBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("8ee97350-c61b-4605-b7de-77e740456513", "Attribute Transport Mode");
			this.AttributeTransportModesGroupBox.Controls.Add(this.AttrTransportModesPanel);
			this.AttributeTransportModesGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AttributeTransportModesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 120, true);
			this.AttributeTransportModesGroupBox.Name = "AttributeTransportModesGroupBox";
			this.AttributeTransportModesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 69, true);
			this.AttributeTransportModesGroupBox.TabIndex = 4;
			this.AttributeTransportModesGroupBox.TabStop = false;
			// 
			// AttrTransportModesPanel
			// 
			this.AttrTransportModesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AttrTransportModesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 17, true);
			this.AttrTransportModesPanel.Name = "AttrTransportModesPanel";
			this.AttrTransportModesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 49, true);
			this.AttrTransportModesPanel.TabIndex = 4;
			// 
			// BasicGroupBox
			// 
			this.BasicGroupBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("a5070bd5-5606-4371-add8-aa2c950cd51b", "Basic Information");
			this.BasicGroupBox.Controls.Add(this.ZZD_IsSystemCheckBox);
			this.BasicGroupBox.Controls.Add(this.ZZD_CountryOrGroupingCodeFindBox);
			this.BasicGroupBox.Controls.Add(this.ZZD_EndDateDateEdit);
			this.BasicGroupBox.Controls.Add(this.ZZD_StartDateDateEdit);
			this.BasicGroupBox.Controls.Add(this.ZZD_DescriptionTextBox);
			this.BasicGroupBox.Controls.Add(this.ZZD_CodeTypeDropEdit);
			this.BasicGroupBox.Controls.Add(this.CodeTransportModesPanel);
			this.BasicGroupBox.Controls.Add(this.TransportModesLabel);
			this.BasicGroupBox.Controls.Add(this.ZZD_CodeTextBox);
			this.BasicGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.BasicGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BasicGroupBox.Name = "BasicGroupBox";
			this.BasicGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 304, true);
			this.BasicGroupBox.TabIndex = 2;
			this.BasicGroupBox.TabStop = false;
			// 
			// ZZD_IsSystemCheckBox
			// 
			this.ZZD_IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ZZD_IsSystemCheckBox, "ZZD_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_IsSystem)));
			this.ZZD_IsSystemCheckBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("47eb0ee9-5d40-4e2c-9f43-b7fd82e67a31", "Is System");
			this.ZZD_IsSystemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ZZD_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 252, true);
			this.ZZD_IsSystemCheckBox.Name = "ZZD_IsSystemCheckBox";
			this.ZZD_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ZZD_IsSystemCheckBox.TabIndex = 7;
			this.ZZD_IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// ZZD_CountryOrGroupingCodeFindBox
			// 
			this.ZZD_CountryOrGroupingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZZD_CountryOrGroupingCodeFindBox, "ZZD_CountryOrGrouping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_CountryOrGrouping)));
			this.ZZD_CountryOrGroupingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 19, true);
			this.ZZD_CountryOrGroupingCodeFindBox.Name = "ZZD_CountryOrGroupingCodeFindBox";
			this.ZZD_CountryOrGroupingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ZZD_CountryOrGroupingCodeFindBox.ParentType = null;
			this.ZZD_CountryOrGroupingCodeFindBox.PreBoundMaxLength = 2;
			this.ZZD_CountryOrGroupingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ZZD_CountryOrGroupingCodeFindBox.TabIndex = 0;
			// 
			// ZZD_EndDateDateEdit
			// 
			this.ZZD_EndDateDateEdit.AllowDrop = true;
			this.ZZD_EndDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ZZD_EndDateDateEdit, "ZZD_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_EndDate)));
			this.ZZD_EndDateDateEdit.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("16419423-e89d-4416-88b8-fbf3bafa20e6", "End Date");
			this.ZZD_EndDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 227, true);
			this.ZZD_EndDateDateEdit.Name = "ZZD_EndDateDateEdit";
			this.ZZD_EndDateDateEdit.TabIndex = 6;
			// 
			// ZZD_StartDateDateEdit
			// 
			this.ZZD_StartDateDateEdit.AllowDrop = true;
			this.ZZD_StartDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ZZD_StartDateDateEdit, "ZZD_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_StartDate)));
			this.ZZD_StartDateDateEdit.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("ce9e5484-0833-4d11-8932-c4af5b095938", "Start Date");
			this.ZZD_StartDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 202, true);
			this.ZZD_StartDateDateEdit.Name = "ZZD_StartDateDateEdit";
			this.ZZD_StartDateDateEdit.TabIndex = 5;
			// 
			// ZZD_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZZD_DescriptionTextBox, "ZZD_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_Description)));
			this.ZZD_DescriptionTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("4b161c38-c010-4d84-be0c-aded83d896fb", "Description");
			this.ZZD_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 94, true);
			this.ZZD_DescriptionTextBox.Multiline = true;
			this.ZZD_DescriptionTextBox.Name = "ZZD_DescriptionTextBox";
			this.ZZD_DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ZZD_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 49, true);
			this.ZZD_DescriptionTextBox.TabIndex = 3;
			// 
			// ZZD_CodeTypeDropEdit
			// 
			this.ZZD_CodeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZZD_CodeTypeDropEdit, "ZZD_CodeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_CodeType)));
			this.ZZD_CodeTypeDropEdit.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("7e9daac4-f0a8-47d7-a607-5d82687498db", "List Type");
			this.ZZD_CodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 44, true);
			this.ZZD_CodeTypeDropEdit.Name = "ZZD_CodeTypeDropEdit";
			this.ZZD_CodeTypeDropEdit.PreBoundMaxLength = 5;
			this.ZZD_CodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ZZD_CodeTypeDropEdit.TabIndex = 1;
			// 
			// CodeTransportModesPanel
			// 
			this.CodeTransportModesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 148, true);
			this.CodeTransportModesPanel.Name = "CodeTransportModesPanel";
			this.CodeTransportModesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 49, true);
			this.CodeTransportModesPanel.TabIndex = 4;
			// 
			// TransportModesLabel
			// 
			this.TransportModesLabel.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("c628d9a3-ba8e-45a0-a9a5-1f8a6638ae12", "Transport Mode");
			this.TransportModesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TransportModesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 161, true);
			this.TransportModesLabel.Name = "TransportModesLabel";
			this.TransportModesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.TransportModesLabel.TabIndex = 8;
			this.TransportModesLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ZZD_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZZD_CodeTextBox, "ZZD_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_Code)));
			this.ZZD_CodeTextBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("35195d55-bf33-4313-8228-928e9b53f010", "Code");
			this.ZZD_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 69, true);
			this.ZZD_CodeTextBox.Name = "ZZD_CodeTextBox";
			this.ZZD_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.ZZD_CodeTextBox.TabIndex = 2;
			// 
			// LanguagesGroupBox
			// 
			this.LanguagesGroupBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("229a8436-6a7a-4cb8-b893-8ba2aa124380", "Languages");
			this.LanguagesGroupBox.Controls.Add(this.LanguagesGrid);
			this.LanguagesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LanguagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 0, true);
			this.LanguagesGroupBox.Name = "LanguagesGroupBox";
			this.LanguagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 112, true);
			this.LanguagesGroupBox.TabIndex = 3;
			this.LanguagesGroupBox.TabStop = false;
			// 
			// LanguagesGrid
			// 
			this.LanguagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LanguagesGrid, "Languages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Languages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListLanguageCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Languages)).SyncRoot)).ZXA_ZX6_NKLanguage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListLanguageCombined)(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).Languages)).SyncRoot)).ZXA_Description)));
			this.LanguagesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ZXA_ZX6_NKLanguage";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "ZXA_Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.LanguagesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LanguagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LanguagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LanguagesGrid.GridId = "14737437-ef5f-477c-96ce-0cee1d35d7d1";
			this.LanguagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LanguagesGrid.LayoutKey = "LanguagesGrid";
			this.LanguagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LanguagesGrid.Name = "LanguagesGrid";
			this.LanguagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 93, true);
			this.LanguagesGrid.TabIndex = 0;
			// 
			// RefCusCodeListForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("31d91b0f-2094-40fc-8b02-d40a9ec74cab", "Global Code");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1255, 387, true);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Universal.ZZRefCusCodeListCombined);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "RefCusCodeListForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "RefCusCodeListForm";
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
			this.AttributeTransportModesGroupBox.ResumeLayout(false);
			this.AttributeTransportModesGroupBox.PerformLayout();
			this.BasicGroupBox.ResumeLayout(false);
			this.BasicGroupBox.PerformLayout();
			this.ZZD_CountryOrGroupingCodeFindBox.ResumeLayout(true);
			this.ZZD_CountryOrGroupingCodeFindBox.PerformLayout();
			this.ZZD_EndDateDateEdit.ResumeLayout(true);
			this.ZZD_EndDateDateEdit.PerformLayout();
			this.ZZD_StartDateDateEdit.ResumeLayout(true);
			this.ZZD_StartDateDateEdit.PerformLayout();
			this.ZZD_CodeTypeDropEdit.ResumeLayout(true);
			this.ZZD_CodeTypeDropEdit.PerformLayout();
			this.LanguagesGroupBox.ResumeLayout(false);
			this.LanguagesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LanguagesGrid)).EndInit();
			this.LanguagesGrid.ResumeLayout(false);
			this.LanguagesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBox LanguagesGroupBox;
		private ZArchitecture.ZGrid LanguagesGrid;
	}
}
