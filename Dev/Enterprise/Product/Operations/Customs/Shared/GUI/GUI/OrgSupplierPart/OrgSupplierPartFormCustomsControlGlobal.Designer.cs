using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class OrgSupplierPartFormCustomsControlGlobal
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
			if (disposing && components != null)
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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.detailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CI_UsageCommentTextBox = new LongTextControl();
			this.ClassificationDescriptionTextBox = new LongTextControl();
			this.AttributesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AttributesLeftSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.Attributes1GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Attributes1Grid = new Enterprise.ZArchitecture.ZGrid();
			this.AttributesRightSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.Attributes2GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Attributes2Grid = new Enterprise.ZArchitecture.ZGrid();
			this.Attributes3GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Attributes3Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).BeginInit();
			this.PivotGrid.SuspendLayout();
			this.detailsPanel.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.AttributesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).BeginInit();
			this.AttributesLeftSplitContainer.Panel1.SuspendLayout();
			this.AttributesLeftSplitContainer.Panel2.SuspendLayout();
			this.AttributesLeftSplitContainer.SuspendLayout();
			this.Attributes1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).BeginInit();
			this.Attributes1Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).BeginInit();
			this.AttributesRightSplitContainer.Panel1.SuspendLayout();
			this.AttributesRightSplitContainer.Panel2.SuspendLayout();
			this.AttributesRightSplitContainer.SuspendLayout();
			this.Attributes2GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).BeginInit();
			this.Attributes2Grid.SuspendLayout();
			this.Attributes3GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).BeginInit();
			this.Attributes3Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.OrgSupplierPart);
			// 
			// PivotGrid
			// 
			this.PivotGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PivotGrid, "PivotsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_ChildType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_ChildTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_UsageComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_DateStart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_DateEnd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_LastAuditedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).LastAuditedUserFullName)));			
			this.PivotGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|8369BA3D-77AA-4E70-AEBF-F987D9EC72E5", "Type Desc.", "Type Description");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CI_ChildTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|C566F1E7-FE55-41D3-B919-A65BD22C2474", "Organization");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CI_OH";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|BCA7D580-AC54-4437-B890-1B3A1A8C6A65", "Lookup", "Class. Lookup", "Classification Lookup");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "CI_CC";
			zGuidFindBoxColumnStyleInfo2.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|DB4B0E6D-B410-4513-95A1-EF9E877BADC6", "Usage Comment");
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.ColumnName = "CI_UsageComment";
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|90DB073D-E4F9-4A22-AB5E-2217B95439F6", "Start Date");
			zDateEditColumnStyleInfo1.ColumnName = "CI_DateStart";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|6083C3DE-491F-43E2-98E2-CBE0DA605CCE", "End Date");
			zDateEditColumnStyleInfo2.ColumnName = "CI_DateEnd";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PivotGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PivotGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.PivotGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.PivotGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PivotGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PivotGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.PivotGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PivotGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 90, true);
			this.PivotGrid.TabIndex = 9;
			this.PivotGrid.AfterBind += new System.EventHandler(this.PivotGrid_AfterBind);
			// 
			// detailsPanel
			// 
			this.detailsPanel.Controls.Add(this.DetailTabControl);
			this.detailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.detailsPanel.Name = "detailsPanel";
			this.detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 172, true);
			this.detailsPanel.TabIndex = 11;
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailTabControl.Controls.Add(this.DetailsTabPage);
			this.DetailTabControl.Controls.Add(this.AttributesTabPage);
			this.DetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailTabControl.Name = "DetailTabControl";
			this.DetailTabControl.SelectedIndex = 0;
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 172, true);
			this.DetailTabControl.TabIndex = 2;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|81FDA738-68C2-4AF6-A051-71F81B2A3F33", "Details");
			this.DetailsTabPage.Controls.Add(this.CI_UsageCommentTextBox);
			this.DetailsTabPage.Controls.Add(this.ClassificationDescriptionTextBox);
			this.DetailsTabPage.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 115, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// CI_UsageCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.CI_UsageCommentTextBox, "PivotsForBinding.CI_UsageComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_UsageComment)));
			this.CI_UsageCommentTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|8F9715F0-AE5F-4A2A-B91F-3E225438AF4E", "Usage Comment");
			this.CI_UsageCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 8, true);
			this.CI_UsageCommentTextBox.Name = "CI_UsageCommentTextBox";
			this.CI_UsageCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.CI_UsageCommentTextBox.TabIndex = 5;
			// 
			// classificationDescriptionTextBox
			//
			this.ClassificationDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ClassificationDescriptionTextBox, "PivotsForBinding.CI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_Description)));
			this.ClassificationDescriptionTextBox.Name = "classificationDescriptionTextBox";
			this.ClassificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 54, true);
			this.ClassificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 20, true);
			this.ClassificationDescriptionTextBox.TabIndex = 6;
			// 
			// AttributesTabPage
			// 
			this.AttributesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|924AB46E-C836-4206-B8B4-538A0DA04817", "Applies to");
			this.AttributesTabPage.Controls.Add(this.AttributesLeftSplitContainer);
			this.AttributesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AttributesTabPage.Name = "AttributesTabPage";
			this.AttributesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AttributesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 115, true);
			this.AttributesTabPage.TabIndex = 7;
			// 
			// AttributesLeftSplitContainer
			// 
			this.AttributesLeftSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesLeftSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AttributesLeftSplitContainer.Name = "AttributesLeftSplitContainer";
			// 
			// AttributesLeftSplitContainer.Panel1
			// 
			this.AttributesLeftSplitContainer.Panel1.Controls.Add(this.Attributes1GroupBox);
			this.AttributesLeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 109, true);
			this.AttributesLeftSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			// 
			// AttributesLeftSplitContainer.Panel2
			// 
			this.AttributesLeftSplitContainer.Panel2.Controls.Add(this.AttributesRightSplitContainer);
			this.AttributesLeftSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(324);
			this.AttributesLeftSplitContainer.TabIndex = 3;
			// 
			// Attributes1GroupBox
			// 
			this.Attributes1GroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|C5A189AA-3773-4C98-AEFF-2E8F7F678B27", "Attribute 1");
			this.Attributes1GroupBox.Controls.Add(this.Attributes1Grid);
			this.Attributes1GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Attributes1GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Attributes1GroupBox.Name = "Attributes1GroupBox";
			this.Attributes1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 109, true);
			this.Attributes1GroupBox.TabIndex = 1;
			this.Attributes1GroupBox.TabStop = false;
			// 
			// Attributes1Grid
			// 
			this.Attributes1Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Attributes1Grid, "PivotsForBinding.Attributes1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAttributeFilter)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes1)).SyncRoot)).BG_AttributeValue1)));
			this.Attributes1Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "BG_AttributeValue1";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.Attributes1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.Attributes1Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Attributes1Grid.GridId = "AE6DE903-E635-4A14-9F78-D3C3FE2E9D2A";
			this.Attributes1Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Attributes1Grid.LayoutKey = "zGrid1";
			this.Attributes1Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.Attributes1Grid.Name = "Attributes1Grid";
			this.Attributes1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 90, true);
			this.Attributes1Grid.TabIndex = 4;
			// 
			// AttributesRightSplitContainer
			// 
			this.AttributesRightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesRightSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AttributesRightSplitContainer.Name = "AttributesRightSplitContainer";
			// 
			// AttributesRightSplitContainer.Panel1
			// 
			this.AttributesRightSplitContainer.Panel1.Controls.Add(this.Attributes2GroupBox);
			this.AttributesRightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 109, true);
			this.AttributesRightSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			// 
			// AttributesRightSplitContainer.Panel2
			// 
			this.AttributesRightSplitContainer.Panel2.Controls.Add(this.Attributes3GroupBox);
			this.AttributesRightSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(238);
			this.AttributesRightSplitContainer.TabIndex = 0;
			// 
			// Attributes2GroupBox
			// 
			this.Attributes2GroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|9788EC08-C73A-4371-9B4C-C263155D8209", "Attribute 2");
			this.Attributes2GroupBox.Controls.Add(this.Attributes2Grid);
			this.Attributes2GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Attributes2GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Attributes2GroupBox.Name = "Attributes2GroupBox";
			this.Attributes2GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 109, true);
			this.Attributes2GroupBox.TabIndex = 2;
			this.Attributes2GroupBox.TabStop = false;
			// 
			// Attributes2Grid
			// 
			this.Attributes2Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Attributes2Grid, "PivotsForBinding.Attributes2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAttributeFilter)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes2)).SyncRoot)).BG_AttributeValue1)));
			this.Attributes2Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "BG_AttributeValue1";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.Attributes2Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.Attributes2Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Attributes2Grid.GridId = "493C3A1D-26D6-4B33-BC45-C8E09D334B2F";
			this.Attributes2Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Attributes2Grid.LayoutKey = "zGrid1";
			this.Attributes2Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.Attributes2Grid.Name = "Attributes2Grid";
			this.Attributes2Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 90, true);
			this.Attributes2Grid.TabIndex = 4;
			// 
			// Attributes3GroupBox
			// 
			this.Attributes3GroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|25D380D0-A8AC-4A81-83B6-30DB9465EC51", "Attribute 3");
			this.Attributes3GroupBox.Controls.Add(this.Attributes3Grid);
			this.Attributes3GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Attributes3GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Attributes3GroupBox.Name = "Attributes3GroupBox";
			this.Attributes3GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 109, true);
			this.Attributes3GroupBox.TabIndex = 3;
			this.Attributes3GroupBox.TabStop = false;
			// 
			// Attributes3Grid
			// 
			this.Attributes3Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Attributes3Grid, "PivotsForBinding.Attributes3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAttributeFilter)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).Attributes3)).SyncRoot)).BG_AttributeValue1)));
			this.Attributes3Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "BG_AttributeValue1";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.Attributes3Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.Attributes3Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Attributes3Grid.GridId = "B4A3DEFA-E960-4E26-9C1B-F97823A3BD6C";
			this.Attributes3Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Attributes3Grid.LayoutKey = "zGrid1";
			this.Attributes3Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.Attributes3Grid.Name = "Attributes3Grid";
			this.Attributes3Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 90, true);
			this.Attributes3Grid.TabIndex = 4;
			// 
			// OrgSupplierPartFormCustomsControlGlobal
			// 
			this.Controls.Add(this.detailsPanel);
			this.Controls.Add(this.PivotGrid);
			this.Name = "OrgSupplierPartFormCustomsControlGlobal";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 272, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).EndInit();
			this.PivotGrid.ResumeLayout(false);
			this.PivotGrid.PerformLayout();
			this.detailsPanel.ResumeLayout(false);
			this.detailsPanel.PerformLayout();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.AttributesTabPage.ResumeLayout(false);
			this.AttributesTabPage.PerformLayout();
			this.AttributesLeftSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).EndInit();
			this.AttributesLeftSplitContainer.ResumeLayout(false);
			this.AttributesLeftSplitContainer.PerformLayout();
			this.Attributes1GroupBox.ResumeLayout(false);
			this.Attributes1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).EndInit();
			this.Attributes1Grid.ResumeLayout(false);
			this.Attributes1Grid.PerformLayout();
			this.AttributesRightSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesRightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).EndInit();
			this.AttributesRightSplitContainer.ResumeLayout(false);
			this.AttributesRightSplitContainer.PerformLayout();
			this.Attributes2GroupBox.ResumeLayout(false);
			this.Attributes2GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).EndInit();
			this.Attributes2Grid.ResumeLayout(false);
			this.Attributes2Grid.PerformLayout();
			this.Attributes3GroupBox.ResumeLayout(false);
			this.Attributes3GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).EndInit();
			this.Attributes3Grid.ResumeLayout(false);
			this.Attributes3Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZPanel detailsPanel;
		protected ZTabControl DetailTabControl;

		protected ZTabPage DetailsTabPage;
		protected LongTextControl CI_UsageCommentTextBox;
		protected LongTextControl ClassificationDescriptionTextBox;

		protected ZTabPage AttributesTabPage;
		protected CargoWise.Windows.UI.KSplitContainer AttributesLeftSplitContainer;
		protected ZGroupBox Attributes1GroupBox;
		protected ZGrid Attributes1Grid;
		protected CargoWise.Windows.UI.KSplitContainer AttributesRightSplitContainer;
		protected ZGroupBox Attributes2GroupBox;
		protected ZGrid Attributes2Grid;
		protected ZGroupBox Attributes3GroupBox;
		protected ZGrid Attributes3Grid;
	}
}
