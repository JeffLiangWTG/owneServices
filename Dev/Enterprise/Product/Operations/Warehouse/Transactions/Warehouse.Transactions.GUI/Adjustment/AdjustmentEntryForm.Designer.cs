using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class AdjustmentEntryForm
	{
		private ZTabPage EntryTabPage;
		private ZTemplateTabControl TabControl;
		private ZGroupBox DetailsGroupBox;
		private ZTextBox ExtRefTextBox;
		private ZTextBox StatusTextBox;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZLogsTabPage zEventTabPage1;
		private ZGuidFindBox WarehouseFindBox;
		private ZOrganisationControl OrgWhsFindControl;
		private KSplitter splitter1;
		private KPanel bottomPanel;
		private KPanel topPanel;
		private ZTabPage CustomFieldsTabPage;
		private ProcessTemplateCustomFieldsControl customLabelsUserControl1;
		private ZWorkflowTabPage WorkflowTabPage;
		private ZOrganisationControl NewOrgFindControl;
		private ZGroupBox InfoGroupBox;
		private ZTextBox AdjustmentTypeTextBox;
		private AdjustmentDocketLinesGridUserControl adjustmentDocketLinesBondedGridUserControl1;
		private ZPanel zPanel1;
		private ZPanel zPanel2;
		private ZPanel zPanel3;
		private ZButton FinaliseButton;
		private ZPanel zPanel4;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private RelatedJobsTabPage RelatedJobsTabPage;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.TabControl = new ZTemplateTabControl();
			this.EntryTabPage = new ZTabPage();
			this.topPanel = new KPanel();
			this.InfoGroupBox = new ZGroupBox();
			this.AdjustmentTypeTextBox = new ZTextBox();
			this.WarehouseFindBox = new ZGuidFindBox();
			this.NewOrgFindControl = new ZOrganisationControl();
			this.OrgWhsFindControl = new ZOrganisationControl();
			this.DetailsGroupBox = new ZGroupBox();
			this.StatusTextBox = new ZTextBox();
			this.ExtRefTextBox = new ZTextBox();
			this.adjustmentDocketLinesBondedGridUserControl1 = new AdjustmentDocketLinesGridUserControl();
			this.splitter1 = new KSplitter();
			this.bottomPanel = new KPanel();
			this.CustomFieldsTabPage = new ZTabPage();
			this.customLabelsUserControl1 = new ProcessTemplateCustomFieldsControl();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.RelatedJobsTabPage = new RelatedJobsTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.zPanel1 = new ZPanel();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.zPanel4 = new ZPanel();
			this.zPanel3 = new ZPanel();
			this.FinaliseButton = new ZButton();
			this.zPanel2 = new ZPanel();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.EntryTabPage.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.InfoGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 662, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(408);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(409);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsAdjustment);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.EntryTabPage);
			this.TabControl.Controls.Add(this.CustomFieldsTabPage);
			this.TabControl.Controls.Add(this.WorkflowTabPage);
			this.TabControl.Controls.Add(this.RelatedJobsTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1086, 633, true);
			this.TabControl.TabIndex = 0;
			// 
			// EntryTabPage
			// 
			this.EntryTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentEntryForm|d8df9993-e870-42c0-aec9-aa370e47d608", "Entry");
			this.EntryTabPage.Controls.Add(this.topPanel);
			this.EntryTabPage.Controls.Add(this.splitter1);
			this.EntryTabPage.Controls.Add(this.bottomPanel);
			this.EntryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryTabPage.Name = "EntryTabPage";
			this.EntryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 606, true);
			this.EntryTabPage.TabIndex = 0;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.InfoGroupBox);
			this.topPanel.Controls.Add(this.NewOrgFindControl);
			this.topPanel.Controls.Add(this.OrgWhsFindControl);
			this.topPanel.Controls.Add(this.DetailsGroupBox);
			this.topPanel.Controls.Add(this.adjustmentDocketLinesBondedGridUserControl1);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 377, true);
			this.topPanel.TabIndex = 5;
			// 
			// InfoGroupBox
			// 
			this.InfoGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("66669e2d-9870-4ea3-92b6-ba19f226720a", "Info");
			this.InfoGroupBox.Controls.Add(this.AdjustmentTypeTextBox);
			this.InfoGroupBox.Controls.Add(this.WarehouseFindBox);
			this.InfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 3, true);
			this.InfoGroupBox.Name = "InfoGroupBox";
			this.InfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 80, true);
			this.InfoGroupBox.TabIndex = 1;
			this.InfoGroupBox.TabStop = false;
			// 
			// AdjustmentTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdjustmentTypeTextBox, "AdjustmentTypeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsAdjustment)(null)).AdjustmentTypeDescription)));
			this.AdjustmentTypeTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentEntryForm|913221a9-c9f3-4a31-8109-adb0d928bdfc", "Type");
			this.AdjustmentTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 19, true);
			this.AdjustmentTypeTextBox.Name = "AdjustmentTypeTextBox";
			this.AdjustmentTypeTextBox.ReadOnly = true;
			this.AdjustmentTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.AdjustmentTypeTextBox.TabIndex = 0;
			// 
			// WarehouseFindBox
			// 
			this.WarehouseFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseFindBox, "WD_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsAdjustment)(null)).WD_WW_Whs)));
			this.WarehouseFindBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentEntryForm|6dd5ad8b-afc9-4f68-a0b6-4cdb0658d2f4", "Whs.");
			this.WarehouseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 47, true);
			this.WarehouseFindBox.Name = "WarehouseFindBox";
			this.WarehouseFindBox.PreBoundMaxLength = 3;
			this.WarehouseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.WarehouseFindBox.TabIndex = 1;
			// 
			// NewOrgFindControl
			// 
			this.NewOrgFindControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewOrgFindControl, "OwnershipAdjustedClientPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsAdjustment)(null)).OwnershipAdjustedClientPK)));
			this.NewOrgFindControl.BindToOrganisations = "ChildAdjustment+Lookups+Clients";
			this.NewOrgFindControl.Cursor = System.Windows.Forms.Cursors.Default;
			this.NewOrgFindControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.FullName;
			this.NewOrgFindControl.IsCustomHeight = true;
			this.NewOrgFindControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(815, 3, true);
			this.NewOrgFindControl.Name = "NewOrgFindControl";
			this.NewOrgFindControl.PopupCaption = "";
			this.NewOrgFindControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 80, true);
			this.NewOrgFindControl.TabIndex = 3;
			// 
			// OrgWhsFindControl
			// 
			this.OrgWhsFindControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgWhsFindControl, "WD_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsAdjustment)(null)).WD_OH_Client)));
			this.OrgWhsFindControl.BindToOrganisations = "Lookups+Clients";
			this.OrgWhsFindControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentEntryForm|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Client");
			this.OrgWhsFindControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.FullName;
			this.OrgWhsFindControl.IsCustomHeight = true;
			this.OrgWhsFindControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrgWhsFindControl.Name = "OrgWhsFindControl";
			this.OrgWhsFindControl.PopupCaption = "";
			this.OrgWhsFindControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 80, true);
			this.OrgWhsFindControl.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentEntryForm|bcd4f4d9-c80e-4b22-bd8a-500d9188b559", "Details");
			this.DetailsGroupBox.Controls.Add(this.StatusTextBox);
			this.DetailsGroupBox.Controls.Add(this.ExtRefTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 80, true);
			this.DetailsGroupBox.TabIndex = 2;
			this.DetailsGroupBox.TabStop = false;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "WD_DocketStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsAdjustment)(null)).WD_DocketStatusDescription)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentEntryForm|52652a32-ad7b-4e09-94bb-eb29d3e541c6", "Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 48, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.StatusTextBox.TabIndex = 3;
			// 
			// ExtRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExtRefTextBox, "WD_ExternalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsAdjustment)(null)).WD_ExternalReference)));
			this.ExtRefTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentEntryForm|9c97c6bd-8c50-4352-ae53-bb70c63c1015", "Reference");
			this.ExtRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 20, true);
			this.ExtRefTextBox.Name = "ExtRefTextBox";
			this.ExtRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ExtRefTextBox.TabIndex = 1;
			// 
			// adjustmentDocketLinesBondedGridUserControl1
			// 
			this.adjustmentDocketLinesBondedGridUserControl1.AllowDrop = true;
			this.adjustmentDocketLinesBondedGridUserControl1.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.adjustmentDocketLinesBondedGridUserControl1, ".");
			this.adjustmentDocketLinesBondedGridUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 89, true);
			this.adjustmentDocketLinesBondedGridUserControl1.Name = "adjustmentDocketLinesBondedGridUserControl1";
			this.adjustmentDocketLinesBondedGridUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1071, 286, true);
			this.adjustmentDocketLinesBondedGridUserControl1.TabIndex = 4;
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.SystemColors.ControlLight;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 377, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 6, true);
			this.splitter1.TabIndex = 7;
			this.splitter1.TabStop = false;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 383, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 223, true);
			this.bottomPanel.TabIndex = 6;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentEntryForm|262df04a-0a5d-476f-8579-a29c5d4858e8", "Additional Info");
			this.CustomFieldsTabPage.Controls.Add(this.customLabelsUserControl1);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 606, true);
			this.CustomFieldsTabPage.TabIndex = 3;
			// 
			// customLabelsUserControl1
			// 
			this.customLabelsUserControl1.AllowDrop = true;
			this.customLabelsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customLabelsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customLabelsUserControl1.Name = "customLabelsUserControl1";
			this.customLabelsUserControl1.NothingSetupMessageLabelText = "To make use of this tab, please setup Transport Booking Instruction custom fields" +
	" in Workflow Manager.";
			this.customLabelsUserControl1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.customLabelsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 606, true);
			this.customLabelsUserControl1.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 606, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// RelatedJobsTabPage
			// 
			this.RelatedJobsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("1148dccd-283e-474d-9a22-aad191f3072d", "Related Jobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.RelatedJobCollection)(((WhsAdjustment)(null)).RelatedJobs)));
			this.RelatedJobsTabPage.ExcludeFromBindingOnSave = true;
			this.RelatedJobsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedJobsTabPage.Name = "RelatedJobsTabPage";
			this.RelatedJobsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 606, true);
			this.RelatedJobsTabPage.TabIndex = 4;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 606, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 606, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// zPanel1
			// 
			this.zPanel1.AutoSize = true;
			this.zPanel1.Controls.Add(this.zPanel4);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(841, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 28, true);
			this.zPanel1.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// zPanel4
			// 
			this.zPanel4.AutoSize = true;
			this.zPanel4.Controls.Add(this.PostingButtonsUserControl);
			this.zPanel4.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 28, true);
			this.zPanel4.TabIndex = 0;
			// 
			// zPanel3
			// 
			this.zPanel3.Controls.Add(this.FinaliseButton);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 0, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 28, true);
			this.zPanel3.TabIndex = 2;
			// 
			// FinaliseButton
			// 
			this.FinaliseButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentEntryForm|c63e9632-8133-4dfa-b124-9fcf547c8fd5", "Finalize");
			this.FinaliseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
			this.FinaliseButton.Name = "FinaliseButton";
			this.FinaliseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.FinaliseButton.TabIndex = 1;
			this.FinaliseButton.Click += new EventHandler(this.FinaliseButton_Click);
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.zPanel3);
			this.zPanel2.Controls.Add(this.zPanel1);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 634, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 28, true);
			this.zPanel2.TabIndex = 1;
			// 
			// AdjustmentEntryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 686, true);
			this.Controls.Add(this.zPanel2);
			this.Controls.Add(this.TabControl);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(WhsAdjustment);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsAdjustment";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 725, true);
			this.Name = "AdjustmentEntryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel2, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.EntryTabPage.ResumeLayout(false);
			this.topPanel.ResumeLayout(false);
			this.InfoGroupBox.ResumeLayout(false);
			this.InfoGroupBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
