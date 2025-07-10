using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class TransferEntryForm
	{
		private ZTemplateTabControl TabControl;
		private ZButton FinaliseButton;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZLogsTabPage zEventTabPage1;
		private ZWorkflowTabPage WorkflowTabPage;
		private ZTabPage CustomFieldsTabPage;
		private CustomLabelsUserControl customLabelsUserControl1;
		private ZTabPage EntryTabPage;
		private KPanel bottomPanel;
		private KSplitter splitter1;
		private KPanel topPanel;
		private ZOrganisationControl OrgWhsFindControl;
		private ZGroupBox DetailsGroupBox;
		private ZDropEdit SubTypeDropEdit;
		private ZTextBox StatusTextBox;
		private ZTextBox ExtRefTextBox;
		private ZCalcEdit ExtRefSplitCalcEdit;
		private ZGuidFindBox zGuidFindBox1;
		private TransferDocketLinesGridUserControl TransferDocketLinesGridUserControl;
		private RelatedJobsTabPage RelatedJobsTabPage;
		private ZPanel zPanel1;
		private ZPanel zPanel2;
		private ZPanel zPanel3;
		private ZPanel zPanel4;
		private ZLabel TransferTaskPlanningStatusPromptLabel;
		private IContainer components;
		private ZGuidFindBox PickForReplenishmentBox;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new Container();
			this.TabControl = new ZTemplateTabControl();
			this.EntryTabPage = new ZTabPage();
			this.topPanel = new KPanel();
			this.PickForReplenishmentBox = new ZGuidFindBox();
			this.OrgWhsFindControl = new ZOrganisationControl();
			this.DetailsGroupBox = new ZGroupBox();
			this.SubTypeDropEdit = new ZDropEdit();
			this.StatusTextBox = new ZTextBox();
			this.ExtRefTextBox = new ZTextBox();
			this.ExtRefSplitCalcEdit = new ZCalcEdit();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.TransferDocketLinesGridUserControl = new TransferDocketLinesGridUserControl();
			this.splitter1 = new KSplitter();
			this.bottomPanel = new KPanel();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.CustomFieldsTabPage = new ZTabPage();
			this.customLabelsUserControl1 = new CustomLabelsUserControl();
			this.RelatedJobsTabPage = new RelatedJobsTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.FinaliseButton = new ZButton();
			this.zPanel1 = new ZPanel();
			this.zPanel2 = new ZPanel();
			this.zPanel3 = new ZPanel();
			this.zPanel4 = new ZPanel();
			this.TransferTaskPlanningStatusPromptLabel = new ZLabel();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.EntryTabPage.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.PickForReplenishmentBox.SuspendLayout();
			this.OrgWhsFindControl.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.SubTypeDropEdit.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.TransferDocketLinesGridUserControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.customLabelsUserControl1.SuspendLayout();
			this.RelatedJobsTabPage.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.TransferTaskPlanningStatusPromptLabel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 552, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1083, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(348);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(349);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsTransfer);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.EntryTabPage);
			this.TabControl.Controls.Add(this.WorkflowTabPage);
			this.TabControl.Controls.Add(this.CustomFieldsTabPage);
			this.TabControl.Controls.Add(this.RelatedJobsTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 549, true);
			this.TabControl.TabIndex = 0;
			// 
			// EntryTabPage
			// 
			this.EntryTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|a411cce9-0043-4201-9b79-e224224c96f0", "Entry");
			this.EntryTabPage.Controls.Add(this.topPanel);
			this.EntryTabPage.Controls.Add(this.splitter1);
			this.EntryTabPage.Controls.Add(this.bottomPanel);
			this.EntryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryTabPage.Name = "EntryTabPage";
			this.EntryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 522, true);
			this.EntryTabPage.TabIndex = 0;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.PickForReplenishmentBox);
			this.topPanel.Controls.Add(this.OrgWhsFindControl);
			this.topPanel.Controls.Add(this.DetailsGroupBox);
			this.topPanel.Controls.Add(this.zGuidFindBox1);
			this.topPanel.Controls.Add(this.TransferDocketLinesGridUserControl);
			this.topPanel.Controls.Add(this.TransferTaskPlanningStatusPromptLabel);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 297, true);
			this.topPanel.TabIndex = 13;
			// 
			// PickForReplenishmentBox
			// 
			this.PickForReplenishmentBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickForReplenishmentBox, "WD_WP_PickBeingReplenished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsTransfer)(null)).WD_WP_PickBeingReplenished)));
			this.PickForReplenishmentBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 80, true);
			this.PickForReplenishmentBox.Name = "PickForReplenishmentBox";
			this.PickForReplenishmentBox.PreBoundMaxLength = 12;
			this.PickForReplenishmentBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.PickForReplenishmentBox.TabIndex = 3;
			// 
			// OrgWhsFindControl
			// 
			this.OrgWhsFindControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgWhsFindControl, "WD_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsTransfer)(null)).WD_OH_Client)));
			this.OrgWhsFindControl.BindToOrganisations = "Lookups+Clients";
			this.OrgWhsFindControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Client");
			this.OrgWhsFindControl.Captions = new string[] { "Client" };
			this.OrgWhsFindControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.FullName;
			this.OrgWhsFindControl.IsCaptionOverridden = false;
			this.OrgWhsFindControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrgWhsFindControl.Name = "OrgWhsFindControl";
			this.OrgWhsFindControl.PopupCaption = "";
			this.OrgWhsFindControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 52, true);
			this.OrgWhsFindControl.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|5eb4263a-1d6a-498b-9f73-a7ecef1d67a8", "Details");
			this.DetailsGroupBox.Controls.Add(this.SubTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.StatusTextBox);
			this.DetailsGroupBox.Controls.Add(this.ExtRefTextBox);
			this.DetailsGroupBox.Controls.Add(this.ExtRefSplitCalcEdit);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 96, true);
			this.DetailsGroupBox.TabIndex = 3;
			this.DetailsGroupBox.TabStop = false;
			// 
			// SubTypeDropEdit
			// 
			this.SubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubTypeDropEdit, "DocketSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsTransfer)(null)).DocketSubType)));
			this.SubTypeDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|02217f1a-7f9c-4451-9077-40f0d08f5a05", "Transfer Type");
			this.SubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 43, true);
			this.SubTypeDropEdit.Name = "SubTypeDropEdit";
			this.SubTypeDropEdit.ShouldResizeByMaxLength = true;
			this.SubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.SubTypeDropEdit.TabIndex = 3;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "WD_DocketStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsTransfer)(null)).WD_DocketStatusDescription)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|6f8b77d4-3b72-46e8-b7c3-4812af4cb557", "Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 68, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.StatusTextBox.TabIndex = 5;
			// 
			// ExtRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExtRefTextBox, "WD_ExternalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsTransfer)(null)).WD_ExternalReference)));
			this.ExtRefTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|b0c40872-b4c1-4404-aa6e-6e8ad80a87d4", "Reference");
			this.ExtRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 18, true);
			this.ExtRefTextBox.Name = "ExtRefTextBox";
			this.ExtRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 20, true);
			this.ExtRefTextBox.TabIndex = 1;
			// 
			// ExtRefSplitCalcEdit
			// 
			this.ExtRefSplitCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ExtRefSplitCalcEdit, "WD_ExternalReferenceSplit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsTransfer)(null)).WD_ExternalReferenceSplit)));
			this.ExtRefSplitCalcEdit.CaptionResourceString = null;
			this.ExtRefSplitCalcEdit.DecimalPlaces = 0;
			this.ExtRefSplitCalcEdit.Decimals = 0;
			this.ExtRefSplitCalcEdit.IsCalculatorEnabled = false;
			this.ExtRefSplitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 18, true);
			this.ExtRefSplitCalcEdit.Name = "ExtRefSplitCalcEdit";
			this.ExtRefSplitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 20, true);
			this.ExtRefSplitCalcEdit.TabIndex = 9;
			this.ExtRefSplitCalcEdit.Text = "0";
			this.ExtRefSplitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "WD_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsTransfer)(null)).WD_WW_Whs)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 57, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PreBoundMaxLength = 3;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.zGuidFindBox1.TabIndex = 2;
			// 
			// TransferDocketLinesGridUserControl
			// 
			this.TransferDocketLinesGridUserControl.AllowDrop = true;
			this.TransferDocketLinesGridUserControl.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransferDocketLinesGridUserControl, ".");
			this.TransferDocketLinesGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 103, true);
			this.TransferDocketLinesGridUserControl.Name = "TransferDocketLinesGridUserControl";
			this.TransferDocketLinesGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 190, true);
			this.TransferDocketLinesGridUserControl.TabIndex = 4;
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.SystemColors.ControlDark;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.DoNotSaveSplitterLayout = false;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 293, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 6, true);
			this.splitter1.TabIndex = 14;
			this.splitter1.TabStop = false;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 299, true);
			this.bottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 90, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 223, true);
			this.bottomPanel.TabIndex = 15;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 522, true);
			this.WorkflowTabPage.TabIndex = 1;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|edf7b194-b550-4d74-a0fb-30ca523ae8e9", "Additional Info");
			this.CustomFieldsTabPage.Controls.Add(this.customLabelsUserControl1);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 522, true);
			this.CustomFieldsTabPage.TabIndex = 4;
			// 
			// customLabelsUserControl1
			// 
			this.customLabelsUserControl1.AllowDrop = true;
			this.customLabelsUserControl1.BindToMember = "";
			this.customLabelsUserControl1.CustomLabelsProvider = null;
			this.customLabelsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customLabelsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customLabelsUserControl1.Name = "customLabelsUserControl1";
			this.customLabelsUserControl1.PropertyNamesToExclude = Array.Empty<string>();
			this.customLabelsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 522, true);
			this.customLabelsUserControl1.TabIndex = 0;
			// 
			// RelatedJobsTabPage
			// 
			this.RelatedJobsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|88b1fec6-067f-4a9a-a3e5-364e0f9c145a", "Related Jobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.RelatedJobCollection)(((WhsTransfer)(null)).RelatedJobs)));
			this.RelatedJobsTabPage.ExcludeFromBindingOnSave = true;
			this.RelatedJobsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedJobsTabPage.Name = "RelatedJobsTabPage";
			this.RelatedJobsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 522, true);
			this.RelatedJobsTabPage.TabIndex = 5;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 522, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 522, true);
			this.zEventTabPage1.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 1, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// FinaliseButton
			// 
			this.FinaliseButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|fc939121-0a0f-4782-b1e0-f4f82f5e8d8c", "Finalize");
			this.FinaliseButton.IsCaptionOverridden = false;
			this.FinaliseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 1, true);
			this.FinaliseButton.Name = "FinaliseButton";
			this.FinaliseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.FinaliseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.FinaliseButton.TabIndex = 2;
			this.FinaliseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.FinaliseButton.ToolTipCaption = null;
			this.FinaliseButton.Click += new EventHandler(this.FinaliseButton_Click);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zPanel2);
			this.zPanel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 552, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1083, 28, true);
			this.zPanel1.TabIndex = 0;
			// 
			// zPanel2
			// 
			this.zPanel2.AutoSize = true;
			this.zPanel2.Controls.Add(this.zPanel3);
			this.zPanel2.Controls.Add(this.zPanel4);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(755, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 28, true);
			this.zPanel2.TabIndex = 0;
			// 
			// zPanel3
			// 
			this.zPanel3.Controls.Add(this.FinaliseButton);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 28, true);
			this.zPanel3.TabIndex = 0;
			// 
			// zPanel4
			// 
			this.zPanel4.AutoSize = true;
			this.zPanel4.Controls.Add(this.PostingButtonsUserControl);
			this.zPanel4.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 0, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 28, true);
			this.zPanel4.TabIndex = 0;
			//
			// TransferTaskPlanningStatusPromptLabel
			// 
			this.TransferTaskPlanningStatusPromptLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.TransferTaskPlanningStatusPromptLabel, "TaskPlanningStatusPrompt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsTransfer)(null)).TaskPlanningStatusPrompt)));
			this.TransferTaskPlanningStatusPromptLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TransferTaskPlanningStatusPromptLabel.ForeColor = System.Drawing.Color.Red;
			this.TransferTaskPlanningStatusPromptLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TransferTaskPlanningStatusPromptLabel, false);
			this.TransferTaskPlanningStatusPromptLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 3, true);
			this.TransferTaskPlanningStatusPromptLabel.Name = "TransferTaskPlanningStatusPromptLabel";
			this.TransferTaskPlanningStatusPromptLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 29, true);
			this.TransferTaskPlanningStatusPromptLabel.TabIndex = 1;
			this.TransferTaskPlanningStatusPromptLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TransferEntryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferEntryForm|9b85016c-b18a-4931-ad64-572093f4a648", "Pick To Replenish");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1083, 604, true);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.TabControl);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(WhsTransfer);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsTransfer";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 450, true);
			this.Name = "TransferEntryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.EntryTabPage.ResumeLayout(false);
			this.EntryTabPage.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.TransferTaskPlanningStatusPromptLabel.ResumeLayout(false);
			this.TransferTaskPlanningStatusPromptLabel.PerformLayout();
			this.PickForReplenishmentBox.ResumeLayout(true);
			this.PickForReplenishmentBox.PerformLayout();
			this.OrgWhsFindControl.ResumeLayout(true);
			this.OrgWhsFindControl.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.SubTypeDropEdit.ResumeLayout(true);
			this.SubTypeDropEdit.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.TransferDocketLinesGridUserControl.ResumeLayout(true);
			this.TransferDocketLinesGridUserControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.customLabelsUserControl1.ResumeLayout(true);
			this.customLabelsUserControl1.PerformLayout();
			this.RelatedJobsTabPage.ResumeLayout(false);
			this.RelatedJobsTabPage.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
