using Enterprise.Core.Forms;
using Enterprise.TransportCommon.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI
{
	partial class DtbRoutePlannerForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DtbRoutePlannerForm));
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LinePanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AddressPointsAndConfirmationsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.FiltersPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DirectDetailsUserControl = new Enterprise.TransportConsignment.GUI.DtbRoutePlannerDirectModeDetailsUserControl();
			this.DetailsUserControl = new Enterprise.TransportConsignment.GUI.DtbRoutePlannerDetailsUserControl();
			this.RunSheetsHorizontalSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RunSheetsVerticalSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RunSheetsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RunSheetInstructionsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.RunSheetInstructionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RunSheetInstructionsControl = new Enterprise.TransportConsignment.GUI.DtbConsignmentRunSheetInstructionsUserControl();
			this.MoveBarPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MoveBar = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.AssignAddressPointButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.RemoveAddressPointButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.AssignPartialButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ViewModeToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.NextAvailableButton = new Enterprise.TransportConsignment.GUI.PlannerButton();
			this.PickupsOnlyButton = new Enterprise.TransportConsignment.GUI.PlannerButton();
			this.DeliveriesOnlyButton = new Enterprise.TransportConsignment.GUI.PlannerButton();
			this.DirectButton = new Enterprise.TransportConsignment.GUI.PlannerButton();
			this.ControlBoxToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.FiltersButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ConsignmentDetailsButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.ChangeViewModeButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.RunSheetsArrowButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.RunSheetDetailsButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.RunSheetsDropButton = new Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton();
			this.PlannerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ViewModePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LinePanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.miniToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddressPointsAndConfirmationsSplitContainer)).BeginInit();
			this.AddressPointsAndConfirmationsSplitContainer.Panel1.SuspendLayout();
			this.AddressPointsAndConfirmationsSplitContainer.Panel2.SuspendLayout();
			this.AddressPointsAndConfirmationsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RunSheetsHorizontalSplitContainer)).BeginInit();
			this.RunSheetsHorizontalSplitContainer.Panel1.SuspendLayout();
			this.RunSheetsHorizontalSplitContainer.Panel2.SuspendLayout();
			this.RunSheetsHorizontalSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RunSheetsVerticalSplitContainer)).BeginInit();
			this.RunSheetsVerticalSplitContainer.Panel2.SuspendLayout();
			this.RunSheetsVerticalSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RunSheetsGrid)).BeginInit();
			this.RunSheetInstructionsTabControl.SuspendLayout();
			this.RunSheetInstructionsTabPage.SuspendLayout();
			this.MoveBarPanel.SuspendLayout();
			this.MoveBar.SuspendLayout();
			this.ViewModeToolStrip.SuspendLayout();
			this.ControlBoxToolStrip.SuspendLayout();
			this.PlannerPanel.SuspendLayout();
			this.ViewModePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 609, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbRoutePlannerCollection);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.LinePanel2);
			this.MainSplitContainer.Panel1.Controls.Add(this.AddressPointsAndConfirmationsSplitContainer);
			this.MainSplitContainer.Panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 1, 0, 0, true);
			this.MainSplitContainer.Panel1MinSize = 673;
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.RunSheetsHorizontalSplitContainer);
			this.MainSplitContainer.Panel2.Controls.Add(this.MoveBarPanel);
			this.MainSplitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 1, 0, 0, true);
			this.MainSplitContainer.Panel2MinSize = 344;
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 511, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(680);
			this.MainSplitContainer.SplitterWidth = 3;
			this.MainSplitContainer.TabIndex = 0;
			// 
			// LinePanel2
			// 
			this.LinePanel2.BackColor = System.Drawing.SystemColors.ControlDark;
			this.LinePanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.LinePanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.LinePanel2.Name = "LinePanel2";
			this.LinePanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 1, true);
			this.LinePanel2.TabIndex = 0;
			// 
			// AddressPointsAndConfirmationsSplitContainer
			// 
			this.AddressPointsAndConfirmationsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressPointsAndConfirmationsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.AddressPointsAndConfirmationsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.AddressPointsAndConfirmationsSplitContainer.Name = "AddressPointsAndConfirmationsSplitContainer";
			this.AddressPointsAndConfirmationsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// AddressPointsAndConfirmationsSplitContainer.Panel1
			// 
			this.AddressPointsAndConfirmationsSplitContainer.Panel1.Controls.Add(this.FiltersPanel);
			this.AddressPointsAndConfirmationsSplitContainer.Panel1MinSize = 160;
			// 
			// AddressPointsAndConfirmationsSplitContainer.Panel2
			// 
			this.AddressPointsAndConfirmationsSplitContainer.Panel2.Controls.Add(this.DirectDetailsUserControl);
			this.AddressPointsAndConfirmationsSplitContainer.Panel2.Controls.Add(this.DetailsUserControl);
			this.AddressPointsAndConfirmationsSplitContainer.Panel2MinSize = 191;
			this.AddressPointsAndConfirmationsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 510, true);
			this.AddressPointsAndConfirmationsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(315);
			this.AddressPointsAndConfirmationsSplitContainer.TabIndex = 3;
			// 
			// FiltersPanel
			// 
			this.FiltersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiltersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FiltersPanel.Name = "FiltersPanel";
			this.FiltersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 315, true);
			this.FiltersPanel.TabIndex = 4;
			// 
			// DirectDetailsUserControl
			// 
			this.DirectDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectDetailsUserControl, "AddressPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.TransportConsignment.Business.DtbAddressPoint)(((Enterprise.TransportConsignment.Business.DtbAddressPoint)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).AddressPoints)).SyncRoot)))));
			this.DirectDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DirectDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DirectDetailsUserControl.Name = "DirectDetailsUserControl";
			this.DirectDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 191, true);
			this.DirectDetailsUserControl.TabIndex = 2;
			// 
			// DetailsUserControl
			// 
			this.DetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsUserControl, "AddressPoints");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.TransportConsignment.Business.DtbAddressPoint)(((Enterprise.TransportConsignment.Business.DtbAddressPoint)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).AddressPoints)).SyncRoot)))));
			this.DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsUserControl.Name = "DetailsUserControl";
			this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 191, true);
			this.DetailsUserControl.TabIndex = 1;
			// 
			// RunSheetsHorizontalSplitContainer
			// 
			this.RunSheetsHorizontalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RunSheetsHorizontalSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.RunSheetsHorizontalSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 1, true);
			this.RunSheetsHorizontalSplitContainer.Name = "RunSheetsHorizontalSplitContainer";
			this.RunSheetsHorizontalSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// RunSheetsHorizontalSplitContainer.Panel1
			// 
			this.RunSheetsHorizontalSplitContainer.Panel1.Controls.Add(this.RunSheetsVerticalSplitContainer);
			this.RunSheetsHorizontalSplitContainer.Panel1MinSize = 160;
			// 
			// RunSheetsHorizontalSplitContainer.Panel2
			// 
			this.RunSheetsHorizontalSplitContainer.Panel2.Controls.Add(this.RunSheetInstructionsTabControl);
			this.RunSheetsHorizontalSplitContainer.Panel2MinSize = 191;
			this.RunSheetsHorizontalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 510, true);
			this.RunSheetsHorizontalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(315);
			this.RunSheetsHorizontalSplitContainer.TabIndex = 3;
			// 
			// RunSheetsVerticalSplitContainer
			// 
			this.RunSheetsVerticalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RunSheetsVerticalSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.RunSheetsVerticalSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RunSheetsVerticalSplitContainer.Name = "RunSheetsVerticalSplitContainer";
			this.RunSheetsVerticalSplitContainer.Panel1MinSize = 50;
			// 
			// RunSheetsVerticalSplitContainer.Panel2
			// 
			this.RunSheetsVerticalSplitContainer.Panel2.Controls.Add(this.RunSheetsGrid);
			this.RunSheetsVerticalSplitContainer.Panel2MinSize = 50;
			this.RunSheetsVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 315, true);
			this.RunSheetsVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(180);
			this.RunSheetsVerticalSplitContainer.SplitterWidth = 1;
			this.RunSheetsVerticalSplitContainer.TabIndex = 2;
			// 
			// RunSheetsGrid
			// 
			this.RunSheetsGrid.AllowDrop = true;
			this.RunSheetsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RunSheetsGrid, "RunSheetsFilteredForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_RunSheetNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_AdHocDriversName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_GS_NKTruckDriver)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_RQ_Truck)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_AdHocDriversLicence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_StartTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_EndTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_OH_TransportCo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_AdHocTransportCoName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).KG_AdHocTruckRegistration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).SpecialInstructionsExist)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).HasFailedRunSheetInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).RequiresRefrigeration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)).IsHazardous)));
			this.RunSheetsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "KG_RunSheetNumber";
			zTextBoxColumnStyleInfo2.ColumnName = "KG_AdHocDriversName";
			zTextBoxColumnStyleInfo3.ColumnName = "KG_GS_NKTruckDriver";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "KG_RQ_Truck";
			zTextBoxColumnStyleInfo4.ColumnName = "KG_AdHocDriversLicence";
			zDateEditColumnStyleInfo1.ColumnName = "KG_StartTime";
			zDateEditColumnStyleInfo2.ColumnName = "KG_EndTime";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "KG_OH_TransportCo";
			zTextBoxColumnStyleInfo5.ColumnName = "KG_AdHocTransportCoName";
			zTextBoxColumnStyleInfo6.ColumnName = "KG_AdHocTruckRegistration";
			zCheckBoxColumnStyleInfo1.ColumnName = "SpecialInstructionsExist";
			zCheckBoxColumnStyleInfo2.ColumnName = "HasFailedRunSheetInstructions";
			zCheckBoxColumnStyleInfo3.ColumnName = "RequiresRefrigeration";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "IsHazardous";
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RunSheetsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RunSheetsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RunSheetsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RunSheetsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RunSheetsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RunSheetsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RunSheetsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RunSheetsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.RunSheetsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RunSheetsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RunSheetsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RunSheetsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.RunSheetsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.RunSheetsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.RunSheetsGrid.CopySelectedRowsAllowed = true;
			this.RunSheetsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RunSheetsGrid.GridId = "ddbba7c3-a9e2-4d07-a3bd-f068813811cf";
			this.RunSheetsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RunSheetsGrid.IsWholeRowSelectedOnClick = true;
			this.RunSheetsGrid.LayoutKey = "Grid";
			this.RunSheetsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RunSheetsGrid.Name = "RunSheetsGrid";
			this.RunSheetsGrid.ReadOnly = true;
			this.RunSheetsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 315, true);
			this.RunSheetsGrid.TabIndex = 1;
			// 
			// RunSheetInstructionsTabControl
			// 
			this.RunSheetInstructionsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.RunSheetInstructionsTabControl.Controls.Add(this.RunSheetInstructionsTabPage);
			this.RunSheetInstructionsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RunSheetInstructionsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RunSheetInstructionsTabControl.Name = "RunSheetInstructionsTabControl";
			this.RunSheetInstructionsTabControl.SelectedIndex = 0;
			this.RunSheetInstructionsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 191, true);
			this.RunSheetInstructionsTabControl.TabIndex = 1;
			// 
			// RunSheetInstructionsTabPage
			// 
			this.RunSheetInstructionsTabPage.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("5ba2adbe-528d-48b5-9a94-37d35828bc3f", "Run Sheet Instructions");
			this.RunSheetInstructionsTabPage.Controls.Add(this.RunSheetInstructionsControl);
			this.RunSheetInstructionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RunSheetInstructionsTabPage.Name = "RunSheetInstructionsTabPage";
			this.RunSheetInstructionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RunSheetInstructionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 164, true);
			this.RunSheetInstructionsTabPage.TabIndex = 0;
			this.RunSheetInstructionsTabPage.UseVisualStyleBackColor = true;
			// 
			// RunSheetInstructionsControl
			// 
			this.RunSheetInstructionsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RunSheetInstructionsControl, "RunSheetsFilteredForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbRoutePlanner)(null)).RunSheetsFilteredForBinding)).SyncRoot)))));
			this.RunSheetInstructionsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RunSheetInstructionsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RunSheetInstructionsControl.Name = "RunSheetInstructionsControl";
			this.RunSheetInstructionsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 158, true);
			this.RunSheetInstructionsControl.TabIndex = 0;
			// 
			// MoveBarPanel
			// 
			this.MoveBarPanel.Controls.Add(this.MoveBar);
			this.MoveBarPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.MoveBarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.MoveBarPanel.Name = "MoveBarPanel";
			this.MoveBarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 510, true);
			this.MoveBarPanel.TabIndex = 2;
			// 
			// MoveBar
			// 
			this.MoveBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MoveBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.MoveBar.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.MoveBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AssignAddressPointButton,
            this.RemoveAddressPointButton,
            this.AssignPartialButton});
			this.MoveBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.MoveBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MoveBar.Name = "MoveBar";
			this.MoveBar.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 2, 0, true);
			this.MoveBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 510, true);
			this.MoveBar.TabIndex = 0;
			// 
			// AssignAddressPointButton
			// 
			this.AssignAddressPointButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("19f2a376-3f93-4632-972b-6c595136a3dc", "Assign");
			this.AssignAddressPointButton.Image = global::Enterprise.TransportConsignment.GUI.Properties.Resources.Assign;
			this.AssignAddressPointButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AssignAddressPointButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, 110, 5, 2, true);
			this.AssignAddressPointButton.Name = "AssignAddressPointButton";
			this.AssignAddressPointButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 43, true);
			this.AssignAddressPointButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.AssignAddressPointButton.Click += new System.EventHandler(this.AssignAddressPointButton_Click);
			// 
			// RemoveAddressPointButton
			// 
			this.RemoveAddressPointButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("dee956b1-e843-407e-98e0-c930138d1d57", "Remove");
			this.RemoveAddressPointButton.Image = global::Enterprise.TransportConsignment.GUI.Properties.Resources.Unassign;
			this.RemoveAddressPointButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.RemoveAddressPointButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, 10, 5, 2, true);
			this.RemoveAddressPointButton.Name = "RemoveAddressPointButton";
			this.RemoveAddressPointButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 43, true);
			this.RemoveAddressPointButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			// 
			// AssignPartialButton
			// 
			this.AssignPartialButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.AssignPartialButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("78062c45-a9b2-4fb6-99ba-34284f41cfd2", "Assign");
			this.AssignPartialButton.Image = global::Enterprise.TransportConsignment.GUI.Properties.Resources.AssignBlue;
			this.AssignPartialButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AssignPartialButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, 0, 5, 100, true);
			this.AssignPartialButton.Name = "AssignPartialButton";
			this.AssignPartialButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 43, true);
			this.AssignPartialButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.AssignPartialButton.Click += new System.EventHandler(this.AssignPartialButton_Click);
			// 
			// ViewModeToolStrip
			// 
			this.ViewModeToolStrip.AutoSize = false;
			this.ViewModeToolStrip.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.ViewModeToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ViewModeToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NextAvailableButton,
            this.PickupsOnlyButton,
            this.DeliveriesOnlyButton,
            this.DirectButton});
			this.ViewModeToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.ViewModeToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ViewModeToolStrip.Name = "ViewModeToolStrip";
			this.ViewModeToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 0, 1, 0, true);
			this.ViewModeToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 60, true);
			this.ViewModeToolStrip.Stretch = true;
			this.ViewModeToolStrip.TabIndex = 1;
			// 
			// NextAvailableButton
			// 
			this.NextAvailableButton.AutoSize = false;
			this.NextAvailableButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("c6b57ad8-c8d1-4ce5-b902-4018c97f8596", "Next Available");
			this.NextAvailableButton.CheckOnClick = true;
			this.NextAvailableButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.NextAvailableButton.Image = ((System.Drawing.Image)(resources.GetObject("NextAvailableButton.Image")));
			this.NextAvailableButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.NextAvailableButton.Name = "NextAvailableButton";
			this.NextAvailableButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 57, true);
			this.NextAvailableButton.ViewMode = Enterprise.TransportConsignment.Integration.DtbRoutePlannerViewMode.NextAvailable;
			this.NextAvailableButton.CheckedChanged += new System.EventHandler(this.NextAvailableButton_CheckedChanged);
			// 
			// PickupsOnlyButton
			// 
			this.PickupsOnlyButton.AutoSize = false;
			this.PickupsOnlyButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("4d309803-c039-4dc5-bdfa-8134e8d02df0", "Pickups Only");
			this.PickupsOnlyButton.CheckOnClick = true;
			this.PickupsOnlyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.PickupsOnlyButton.Image = ((System.Drawing.Image)(resources.GetObject("PickupsOnlyButton.Image")));
			this.PickupsOnlyButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.PickupsOnlyButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 0, 2, true);
			this.PickupsOnlyButton.Name = "PickupsOnlyButton";
			this.PickupsOnlyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 57, true);
			this.PickupsOnlyButton.ViewMode = Enterprise.TransportConsignment.Integration.DtbRoutePlannerViewMode.Pickups;
			this.PickupsOnlyButton.CheckedChanged += new System.EventHandler(this.PickupsOnlyButton_CheckedChanged);
			// 
			// DeliveriesOnlyButton
			// 
			this.DeliveriesOnlyButton.AutoSize = false;
			this.DeliveriesOnlyButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("66a91ad0-06a6-435b-98a8-05b678cd1ff6", "Deliveries Only");
			this.DeliveriesOnlyButton.CheckOnClick = true;
			this.DeliveriesOnlyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.DeliveriesOnlyButton.Image = ((System.Drawing.Image)(resources.GetObject("DeliveriesOnlyButton.Image")));
			this.DeliveriesOnlyButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.DeliveriesOnlyButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 0, 2, true);
			this.DeliveriesOnlyButton.Name = "DeliveriesOnlyButton";
			this.DeliveriesOnlyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 57, true);
			this.DeliveriesOnlyButton.ViewMode = Enterprise.TransportConsignment.Integration.DtbRoutePlannerViewMode.Deliveries;
			this.DeliveriesOnlyButton.CheckedChanged += new System.EventHandler(this.DeliveriesOnlyButton_CheckedChanged);
			// 
			// DirectButton
			// 
			this.DirectButton.AutoSize = false;
			this.DirectButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("e9e8fa2e-5d1a-4765-86f2-bc6cfe59054b", "Direct");
			this.DirectButton.CheckOnClick = true;
			this.DirectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.DirectButton.Image = ((System.Drawing.Image)(resources.GetObject("DirectButton.Image")));
			this.DirectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.DirectButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 0, 2, true);
			this.DirectButton.Name = "DirectButton";
			this.DirectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 57, true);
			this.DirectButton.ViewMode = Enterprise.TransportConsignment.Integration.DtbRoutePlannerViewMode.Direct;
			this.DirectButton.CheckedChanged += new System.EventHandler(this.DirectButton_CheckedChanged);
			// 
			// ControlBoxToolStrip
			// 
			this.ControlBoxToolStrip.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ControlBoxToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ControlBoxToolStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.ControlBoxToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FiltersButton,
            this.ConsignmentDetailsButton,
            this.RefreshButton,
            this.toolStripSeparator1,
            this.ChangeViewModeButton,
            this.RunSheetsArrowButton,
            this.toolStripSeparator2,
            this.RunSheetDetailsButton,
            this.RunSheetsDropButton});
			this.ControlBoxToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 67, true);
			this.ControlBoxToolStrip.Name = "ControlBoxToolStrip";
			this.ControlBoxToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 31, true);
			this.ControlBoxToolStrip.TabIndex = 1;
			// 
			// FiltersButton
			// 
			this.FiltersButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("063b2a96-97a9-40f6-8bb1-91dd61ad878d", "Filters");
			this.FiltersButton.CheckOnClick = true;
			this.FiltersButton.Image = global::Enterprise.TransportConsignment.GUI.Properties.Resources.Search;
			this.FiltersButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FiltersButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.FiltersButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 0, 2, true);
			this.FiltersButton.Name = "FiltersButton";
			this.FiltersButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(14, 0, 0, 0, true);
			this.FiltersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 28, true);
			this.FiltersButton.Click += new System.EventHandler(this.FiltersButton_Click);
			// 
			// ConsignmentDetailsButton
			// 
			this.ConsignmentDetailsButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("bdc56aa8-a6b4-47d2-b749-2e3f8058bbc6", "Details");
			this.ConsignmentDetailsButton.CheckOnClick = true;
			this.ConsignmentDetailsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ConsignmentDetailsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ConsignmentDetailsButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 0, 2, true);
			this.ConsignmentDetailsButton.Name = "ConsignmentDetailsButton";
			this.ConsignmentDetailsButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(14, 0, 0, 0, true);
			this.ConsignmentDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 28, true);
			this.ConsignmentDetailsButton.Click += new System.EventHandler(this.ConsignmentDetailsButton_Click);
			// 
			// RefreshButton
			// 
			this.RefreshButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("ddcb5797-5a51-486a-916f-866931bd0dfb", "Refresh");
			this.RefreshButton.Image = global::Enterprise.TransportConsignment.GUI.Properties.Resources.RefreshAll;
			this.RefreshButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RefreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.RefreshButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 0, 2, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(14, 0, 0, 0, true);
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 28, true);
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 1, 0, 0, true);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 30, true);
			// 
			// ChangeViewModeButton
			// 
			this.ChangeViewModeButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("1744b62b-c6c4-4095-bb25-4d4de7fd5de7", "Change View Mode");
			this.ChangeViewModeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.ChangeViewModeButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.ChangeViewModeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ChangeViewModeButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 0, 2, true);
			this.ChangeViewModeButton.Name = "ChangeViewModeButton";
			this.ChangeViewModeButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(15, 0, 0, 0, true);
			this.ChangeViewModeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 28, true);
			this.ChangeViewModeButton.Click += new System.EventHandler(this.ChangeViewModeButton_Click);
			// 
			// RunSheetsArrowButton
			// 
			this.RunSheetsArrowButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.RunSheetsArrowButton.AutoSize = false;
			this.RunSheetsArrowButton.AutoToolTip = false;
			this.RunSheetsArrowButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("e328cdfa-a997-49ab-b91b-3b5416b99a69", "3");
			this.RunSheetsArrowButton.Font = new System.Drawing.Font("Webdings", 11F);
			this.RunSheetsArrowButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.RunSheetsArrowButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 0, 2, true);
			this.RunSheetsArrowButton.Name = "RunSheetsArrowButton";
			this.RunSheetsArrowButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 28, true);
			this.RunSheetsArrowButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.RunSheetsArrowButton.Click += new System.EventHandler(this.RunSheetsArrowButton_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripSeparator2.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 1, 0, 0, true);
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 30, true);
			// 
			// RunSheetDetailsButton
			// 
			this.RunSheetDetailsButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.RunSheetDetailsButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("9e4d5b89-261e-4269-9c70-b445bbb4fdd5", "Details");
			this.RunSheetDetailsButton.CheckOnClick = true;
			this.RunSheetDetailsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RunSheetDetailsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.RunSheetDetailsButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 0, 2, true);
			this.RunSheetDetailsButton.Name = "RunSheetDetailsButton";
			this.RunSheetDetailsButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(14, 0, 0, 0, true);
			this.RunSheetDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 28, true);
			this.RunSheetDetailsButton.Click += new System.EventHandler(this.RunSheetDetailsButton_Click);
			// 
			// RunSheetsDropButton
			// 
			this.RunSheetsDropButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.RunSheetsDropButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("b0244e84-3457-4ca5-a97c-1c92ce412033", "Select Run Sheets...");
			this.RunSheetsDropButton.Image = global::Enterprise.TransportConsignment.GUI.Properties.Resources.Details;
			this.RunSheetsDropButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RunSheetsDropButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.RunSheetsDropButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 0, 2, true);
			this.RunSheetsDropButton.Name = "RunSheetsDropButton";
			this.RunSheetsDropButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(14, 0, 0, 0, true);
			this.RunSheetsDropButton.ShowDropDownArrow = false;
			this.RunSheetsDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 29, true);
			// 
			// PlannerPanel
			// 
			this.PlannerPanel.Controls.Add(this.MainSplitContainer);
			this.PlannerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlannerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 98, true);
			this.PlannerPanel.Name = "PlannerPanel";
			this.PlannerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 511, true);
			this.PlannerPanel.TabIndex = 5;
			// 
			// ViewModePanel
			// 
			this.ViewModePanel.Controls.Add(this.LinePanel1);
			this.ViewModePanel.Controls.Add(this.ViewModeToolStrip);
			this.ViewModePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ViewModePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ViewModePanel.Name = "ViewModePanel";
			this.ViewModePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 64, true);
			this.ViewModePanel.TabIndex = 3;
			// 
			// LinePanel1
			// 
			this.LinePanel1.BackColor = System.Drawing.SystemColors.ControlDark;
			this.LinePanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LinePanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 63, true);
			this.LinePanel1.Name = "LinePanel1";
			this.LinePanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 1, true);
			this.LinePanel1.TabIndex = 2;
			// 
			// miniToolStrip
			// 
			this.miniToolStrip.AutoSize = false;
			this.miniToolStrip.CanOverflow = false;
			this.miniToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.miniToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.miniToolStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.miniToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.miniToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 210, true);
			this.miniToolStrip.Name = "miniToolStrip";
			this.miniToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 2, 0, true);
			this.miniToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 461, true);
			this.miniToolStrip.TabIndex = 0;
			// 
			// DtbRoutePlannerForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 612, true);
			this.Controls.Add(this.PlannerPanel);
			this.Controls.Add(this.ControlBoxToolStrip);
			this.Controls.Add(this.ViewModePanel);
			this.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbRoutePlannerCollection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 650, true);
			this.Name = "DtbRoutePlannerForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.ViewModePanel, 0);
			this.Controls.SetChildIndex(this.ControlBoxToolStrip, 0);
			this.Controls.SetChildIndex(this.PlannerPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.AddressPointsAndConfirmationsSplitContainer.Panel1.ResumeLayout(false);
			this.AddressPointsAndConfirmationsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AddressPointsAndConfirmationsSplitContainer)).EndInit();
			this.AddressPointsAndConfirmationsSplitContainer.ResumeLayout(false);
			this.RunSheetsHorizontalSplitContainer.Panel1.ResumeLayout(false);
			this.RunSheetsHorizontalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RunSheetsHorizontalSplitContainer)).EndInit();
			this.RunSheetsHorizontalSplitContainer.ResumeLayout(false);
			this.RunSheetsVerticalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RunSheetsVerticalSplitContainer)).EndInit();
			this.RunSheetsVerticalSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RunSheetsGrid)).EndInit();
			this.RunSheetInstructionsTabControl.ResumeLayout(false);
			this.RunSheetInstructionsTabPage.ResumeLayout(false);
			this.MoveBarPanel.ResumeLayout(false);
			this.MoveBarPanel.PerformLayout();
			this.MoveBar.ResumeLayout(false);
			this.MoveBar.PerformLayout();
			this.ViewModeToolStrip.ResumeLayout(false);
			this.ViewModeToolStrip.PerformLayout();
			this.ControlBoxToolStrip.ResumeLayout(false);
			this.ControlBoxToolStrip.PerformLayout();
			this.PlannerPanel.ResumeLayout(false);
			this.ViewModePanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZToolStrip ViewModeToolStrip;
		protected PlannerButton NextAvailableButton;
		protected PlannerButton PickupsOnlyButton;
		protected PlannerButton DeliveriesOnlyButton;
		protected PlannerButton DirectButton;
		protected CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private ZArchitecture.GUI.ZToolStrip ControlBoxToolStrip;
		protected ZToolStripButton RunSheetsArrowButton;
		private ZToolStripButton ChangeViewModeButton;
		protected ZArchitecture.ZGrid RunSheetsGrid;
		protected ZToolStripButton FiltersButton;
		private ZPanel PlannerPanel;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private ZPanel ViewModePanel;
		private ZPanel LinePanel1;
		private ZPanel LinePanel2;
		protected ZToolStripDropDownButton RunSheetsDropButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		protected ZToolStripButton ConsignmentDetailsButton;
		protected ZToolStripButton RefreshButton;
		protected CargoWise.Windows.UI.KSplitContainer AddressPointsAndConfirmationsSplitContainer;
		private ZPanel FiltersPanel;
		protected DtbRoutePlannerDetailsUserControl DetailsUserControl;
		private ZPanel MoveBarPanel;
		private ZToolStrip MoveBar;
		protected ZToolStripButton AssignAddressPointButton;
		private ZToolStripButton RemoveAddressPointButton;
		protected ZToolStripButton AssignPartialButton;
		private ZToolStrip miniToolStrip;
		protected ZToolStripButton RunSheetDetailsButton;
		protected CargoWise.Windows.UI.KSplitContainer RunSheetsHorizontalSplitContainer;
		protected CargoWise.Windows.UI.KSplitContainer RunSheetsVerticalSplitContainer;
		protected DtbConsignmentRunSheetInstructionsUserControl RunSheetInstructionsControl;
		private ZTabControl RunSheetInstructionsTabControl;
		private ZTabPage RunSheetInstructionsTabPage;
		protected DtbRoutePlannerDirectModeDetailsUserControl DirectDetailsUserControl;
	}
}
