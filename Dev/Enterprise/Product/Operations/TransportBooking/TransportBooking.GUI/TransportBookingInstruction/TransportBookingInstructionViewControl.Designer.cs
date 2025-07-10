namespace Enterprise.TransportBookings.GUI
{
	partial class TransportBookingInstructionViewControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransportBookingInstructionViewControl));
			this.InstructionsGroupBox = new Enterprise.Packing.GUI.ZGroupBoxWithoutCaption();
			this.TopLevelSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.GridsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.InstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackageDivotGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackagesToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.MoveUpButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.MoveDownButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.AssignPackagesButton = new Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton();
			this.AssignToSelectedInstructionMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.AssignToAllInstructionsMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.AssignAllOuterPackagesToAllInstructionsMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.AssignAllContainersToAllInstructionsMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.UnassignPackagesButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InstructionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopLevelSplitContainer)).BeginInit();
			this.TopLevelSplitContainer.Panel1.SuspendLayout();
			this.TopLevelSplitContainer.Panel2.SuspendLayout();
			this.TopLevelSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GridsSplitContainer)).BeginInit();
			this.GridsSplitContainer.Panel1.SuspendLayout();
			this.GridsSplitContainer.Panel2.SuspendLayout();
			this.GridsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InstructionsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackageDivotGrid)).BeginInit();
			this.PackagesToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBooking);
			// 
			// InstructionsGroupBox
			// 
			this.InstructionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InstructionsGroupBox.Controls.Add(this.TopLevelSplitContainer);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InstructionsGroupBox, false);
			this.InstructionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, -5, true);
			this.InstructionsGroupBox.Name = "InstructionsGroupBox";
			this.InstructionsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.InstructionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 105, true);
			this.InstructionsGroupBox.TabIndex = 0;
			this.InstructionsGroupBox.TabStop = false;
			// 
			// TopLevelSplitContainer
			// 
			this.TopLevelSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TopLevelSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.TopLevelSplitContainer.IsSplitterFixed = true;
			this.TopLevelSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.TopLevelSplitContainer.Name = "TopLevelSplitContainer";
			this.TopLevelSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// TopLevelSplitContainer.Panel1
			// 
			this.TopLevelSplitContainer.Panel1.Controls.Add(this.GridsSplitContainer);
			// 
			// TopLevelSplitContainer.Panel2
			// 
			this.TopLevelSplitContainer.Panel2.Controls.Add(this.PackagesToolStrip);
			this.TopLevelSplitContainer.Panel2MinSize = 31;
			this.TopLevelSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 92, true);
			this.TopLevelSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.TopLevelSplitContainer.SplitterWidth = 1;
			this.TopLevelSplitContainer.TabIndex = 3;
			// 
			// GridsSplitContainer
			// 
			this.GridsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GridsSplitContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GridsSplitContainer.Name = "GridsSplitContainer";
			// 
			// GridsSplitContainer.Panel1
			// 
			this.GridsSplitContainer.Panel1.Controls.Add(this.InstructionsGrid);
			// 
			// GridsSplitContainer.Panel2
			// 
			this.GridsSplitContainer.Panel2.Controls.Add(this.PackageDivotGrid);
			this.GridsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 60, true);
			this.GridsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(521);
			this.GridsSplitContainer.TabIndex = 1;
			// 
			// InstructionsGrid
			// 
			this.InstructionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InstructionsGrid, "Instructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).KN_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).KN_InstructionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).OrganisationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).Address.OrganisationNameOrPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).Address.OrganisationDataFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).Address.E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).Address.Lookups.SelectableDocAddressType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).KN_DropMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).KN_RQ_Equipment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).KN_ServiceInstruction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).ConNoteNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).KN_IsContainerRateable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).KN_IsLooseRateable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).Address.E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).KN_IsAuthorisedToLeave)));
			this.InstructionsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "KN_Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(15);
			zDropEditColumnStyleInfo1.ColumnName = "KN_InstructionType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.ColumnName = "OrganisationType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingInstructionViewControl|190eed05-f18e-404b-bd9b-411edfc4cfd3", "Org.", "Organization", "");
			zMultiControlColumnStyleInfo1.ColumnName = "Address+OrganisationNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "Address+OrganisationDataFieldType";
			zGuidDropEditColumnStyleInfo1.ColumnName = "Address+E2_OA_Address";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);			
			zDropEditColumnStyleInfo3.ColumnName = "KN_DropMode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "KN_RQ_Equipment";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zMultiLineTextBoxColumnInfo1.ColumnName = "KN_ServiceInstruction";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.ColumnName = "ConNoteNo";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingInstructionViewControl|2d1a27b8-6f6b-4af2-9d8d-ebe476539d7e", "Status");
			zTextBoxColumnStyleInfo2.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("12b2d466-81fa-459a-a6e4-0391de97109c", "RC?", "Rate Container?", "Is Container Rateable?", "");
			zCheckBoxColumnStyleInfo1.ColumnName = "KN_IsContainerRateable";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("4ac56ed5-cdb2-41af-9f07-d9d313152b8a", "RL?", "Rate LCL?", "Is LCL Rateable?", "");
			zCheckBoxColumnStyleInfo2.ColumnName = "KN_IsLooseRateable";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("d98dd60c-ed44-4097-9025-8a292e4da57f", "Org. Name", "Org. Full Name", "Organization Full Name", "");
			zTextBoxColumnStyleInfo3.ColumnName = "Address+E2_CompanyName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "KN_IsAuthorisedToLeave";			
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.InstructionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.InstructionsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.InstructionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InstructionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.InstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InstructionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.InstructionsGrid.CopySelectedRowsAllowed = true;
			this.InstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InstructionsGrid.GridId = "B7BD45CE-ABAF-4F5B-8889-2E8882FC1EC9";
			this.InstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InstructionsGrid.LayoutKey = "InstructionsGrid";
			this.InstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InstructionsGrid.Name = "InstructionsGrid";
			this.InstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 60, true);
			this.InstructionsGrid.TabIndex = 2;
			// 
			// PackageDivotGrid
			// 
			this.PackageDivotGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackageDivotGrid, "Instructions.PackageDivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)).SyncRoot)).KD_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)).SyncRoot)).Package.KP_PackageID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)).SyncRoot)).PackageDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)).SyncRoot)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)).SyncRoot)).Package.KP_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)).SyncRoot)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)).SyncRoot)).Package.KP_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)).SyncRoot)).Package.Container.ContainerType.RC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingInstruction)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Instructions)).SyncRoot)).PackageDivots)).SyncRoot)).Package.KP_GoodsDescription)));
			this.PackageDivotGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "KD_Quantity";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingInstructionViewControl|dc6b6b69-1cdd-4ce1-8da8-1f1d4782fb21", "Package");
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.ColumnName = "Package+KP_PackageID";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingInstructionViewControl|3232f7f2-0e38-40f7-94f6-f9ed2e58ecf8", "Description");
			zTextBoxColumnStyleInfo5.ColumnName = "PackageDescription";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Weight";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingInstructionViewControl|884e4897-a50f-496b-b7d0-9008fc4bf238", "Weight and Unit");
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.ColumnName = "Package+KP_WeightUQ";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingInstructionViewControl|884e4897-a50f-496b-b7d0-9008fc4bf238", "Weight and Unit");
			zDropEditColumnStyleInfo4.IsReadOnly = true;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "Volume";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingInstructionViewControl|e2c085bb-1b72-428c-a928-9704614fa273", "Volume and Unit");
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo5.ColumnName = "Package+KP_VolumeUQ";
			zDropEditColumnStyleInfo5.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingInstructionViewControl|e2c085bb-1b72-428c-a928-9704614fa273", "Volume and Unit");
			zDropEditColumnStyleInfo5.IsReadOnly = true;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo6.ColumnName = "Package+Container+ContainerType+RC_Code";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.ColumnName = "Package+KP_GoodsDescription";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			this.PackageDivotGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackageDivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackageDivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PackageDivotGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackageDivotGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PackageDivotGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PackageDivotGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.PackageDivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PackageDivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PackageDivotGrid.CopySelectedRowsAllowed = true;
			this.PackageDivotGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageDivotGrid.GridId = "6B3E4592-BFEA-4B2F-AF87-33BD11F58945";
			this.PackageDivotGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackageDivotGrid.LayoutKey = "PackageDivotGrid";
			this.PackageDivotGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageDivotGrid.Name = "PackageDivotGrid";
			this.PackageDivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 60, true);
			this.PackageDivotGrid.TabIndex = 2;
			// 
			// PackagesToolStrip
			// 
			this.PackagesToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.PackagesToolStrip.ImageScalingSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.PackagesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MoveUpButton,
            this.MoveDownButton,
            this.toolStripSeparator2,
            this.AssignPackagesButton,
            this.UnassignPackagesButton});
			this.PackagesToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.PackagesToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesToolStrip.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 29, true);
			this.PackagesToolStrip.Name = "PackagesToolStrip";
			this.PackagesToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 31, true);
			this.PackagesToolStrip.TabIndex = 1;
			this.PackagesToolStrip.Text = "toolStrip1";
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.Image = ((System.Drawing.Image)(resources.GetObject("MoveUpButton.Image")));
			this.MoveUpButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 28, true);
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.Image = ((System.Drawing.Image)(resources.GetObject("MoveDownButton.Image")));
			this.MoveDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 28, true);
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 31, true);
			// 
			// AssignPackagesButton
			// 
			this.AssignPackagesButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("4FC5BA96-9DEE-49DE-BACD-E8BA0DCB3E63", "Assign Packages");
			this.AssignPackagesButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AssignToSelectedInstructionMenuItem,
            this.AssignToAllInstructionsMenuItem,
            this.toolStripMenuItem1,
            this.AssignAllOuterPackagesToAllInstructionsMenuItem,
            this.AssignAllContainersToAllInstructionsMenuItem});
			this.AssignPackagesButton.Image = global::Enterprise.TransportBookings.GUI.Properties.Resources.AddPack;
			this.AssignPackagesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.AssignPackagesButton.Name = "AssignPackagesButton";
			this.AssignPackagesButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 0, 0, 0, true);
			this.AssignPackagesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 28, true);
			// 
			// AssignToSelectedInstructionMenuItem
			// 
			this.AssignToSelectedInstructionMenuItem.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("FD9913C1-78BF-4A29-B105-12B4CEE30442", "...to Selected Instructions");
			this.AssignToSelectedInstructionMenuItem.Name = "AssignToSelectedInstructionMenuItem";
			this.AssignToSelectedInstructionMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 22, true);
			this.AssignToSelectedInstructionMenuItem.Click += new System.EventHandler(this.AssignToSelectedInstructionMenuItem_Click);
			// 
			// AssignToAllInstructionsMenuItem
			// 
			this.AssignToAllInstructionsMenuItem.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("58388EBE-5B81-4DB6-92FD-676D8FA6B47F", "...to All Instructions");
			this.AssignToAllInstructionsMenuItem.Name = "AssignToAllInstructionsMenuItem";
			this.AssignToAllInstructionsMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 22, true);
			this.AssignToAllInstructionsMenuItem.Click += new System.EventHandler(this.AssignToAllInstructionsMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 6, true);
			// 
			// AssignAllOuterPackagesToAllInstructionsMenuItem
			// 
			this.AssignAllOuterPackagesToAllInstructionsMenuItem.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("7052EC33-80B9-460C-B08C-EF44841A44BD", "Assign All Outer Packages to All Instructions");
			this.AssignAllOuterPackagesToAllInstructionsMenuItem.Name = "AssignAllOuterPackagesToAllInstructionsMenuItem";
			this.AssignAllOuterPackagesToAllInstructionsMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 22, true);
			this.AssignAllOuterPackagesToAllInstructionsMenuItem.Click += new System.EventHandler(this.AssignAllOuterPackagesToAllInstructionsMenuItem_Click);
			// 
			// AssignAllContainersToAllInstructionsMenuItem
			// 
			this.AssignAllContainersToAllInstructionsMenuItem.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("A82E6853-61C9-4BCE-A7AA-DF59561B07A9", "Assign All Containers to All Instructions");
			this.AssignAllContainersToAllInstructionsMenuItem.Name = "AssignAllContainersToAllInstructionsMenuItem";
			this.AssignAllContainersToAllInstructionsMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 22, true);
			this.AssignAllContainersToAllInstructionsMenuItem.Click += new System.EventHandler(this.AssignAllContainersToAllInstructionsMenuItem_Click);
			// 
			// UnassignPackagesButton
			// 
			this.UnassignPackagesButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("18D93BF2-B261-41AD-8695-456409321C07", "Un-assign Packages");
			this.UnassignPackagesButton.Image = global::Enterprise.TransportBookings.GUI.Properties.Resources.Unpack;
			this.UnassignPackagesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.UnassignPackagesButton.Name = "UnassignPackagesButton";
			this.UnassignPackagesButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 0, 0, 0, true);
			this.UnassignPackagesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 28, true);
			this.UnassignPackagesButton.Click += new System.EventHandler(this.UnassignPackagesButton_Click);
			// 
			// TransportBookingInstructionViewControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InstructionsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 100, true);
			this.Name = "TransportBookingInstructionViewControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InstructionsGroupBox.ResumeLayout(false);
			this.TopLevelSplitContainer.Panel1.ResumeLayout(false);
			this.TopLevelSplitContainer.Panel2.ResumeLayout(false);
			this.TopLevelSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopLevelSplitContainer)).EndInit();
			this.TopLevelSplitContainer.ResumeLayout(false);
			this.GridsSplitContainer.Panel1.ResumeLayout(false);
			this.GridsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GridsSplitContainer)).EndInit();
			this.GridsSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InstructionsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PackageDivotGrid)).EndInit();
			this.PackagesToolStrip.ResumeLayout(false);
			this.PackagesToolStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer TopLevelSplitContainer;
		internal CargoWise.Windows.UI.KSplitContainer GridsSplitContainer;
		internal ZArchitecture.ZGrid InstructionsGrid;
		private ZArchitecture.ZGrid PackageDivotGrid;
#if DEBUG
		internal
#endif
		Enterprise.ZArchitecture.GUI.ZToolStripButton MoveUpButton;
#if DEBUG
		internal
#endif
		Enterprise.ZArchitecture.GUI.ZToolStripButton MoveDownButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
#if DEBUG
		internal
#endif
		Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton AssignPackagesButton;
		private Enterprise.ZArchitecture.GUI.ZToolStripMenuItem AssignToSelectedInstructionMenuItem;
		private Enterprise.ZArchitecture.GUI.ZToolStripMenuItem AssignToAllInstructionsMenuItem;

#if DEBUG
		internal
#endif
		Enterprise.ZArchitecture.GUI.ZToolStripButton UnassignPackagesButton;
		private Packing.GUI.ZGroupBoxWithoutCaption InstructionsGroupBox;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;

#if DEBUG
		internal
#endif
		Enterprise.ZArchitecture.GUI.ZToolStripMenuItem AssignAllOuterPackagesToAllInstructionsMenuItem;

#if DEBUG
		internal
#endif
		Enterprise.ZArchitecture.GUI.ZToolStripMenuItem AssignAllContainersToAllInstructionsMenuItem;
		private ZArchitecture.GUI.ZToolStrip PackagesToolStrip;
	}
}
