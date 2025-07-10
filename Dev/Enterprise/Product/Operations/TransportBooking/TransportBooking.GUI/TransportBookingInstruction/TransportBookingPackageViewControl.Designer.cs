namespace Enterprise.TransportBookings.GUI
{
	partial class TransportBookingPackageViewControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PackageViewGroupBox = new Enterprise.Packing.GUI.ZGroupBoxWithoutCaption();
			this.TopLevelSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PackageGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackageViewGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopLevelSplitContainer)).BeginInit();
			this.TopLevelSplitContainer.Panel1.SuspendLayout();
			this.TopLevelSplitContainer.Panel2.SuspendLayout();
			this.TopLevelSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackageGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InstructionsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBooking);
			// 
			// PackageViewGroupBox
			// 
			this.PackageViewGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PackageViewGroupBox.Controls.Add(this.TopLevelSplitContainer);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PackageViewGroupBox, false);
			this.PackageViewGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, -5, true);
			this.PackageViewGroupBox.Name = "PackageViewGroupBox";
			this.PackageViewGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.PackageViewGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 155, true);
			this.PackageViewGroupBox.TabIndex = 1;
			this.PackageViewGroupBox.TabStop = false;
			// 
			// TopLevelSplitContainer
			// 
			this.TopLevelSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TopLevelSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.TopLevelSplitContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TopLevelSplitContainer.Name = "TopLevelSplitContainer";
			// 
			// TopLevelSplitContainer.Panel1
			// 
			this.TopLevelSplitContainer.Panel1.Controls.Add(this.PackageGrid);
			// 
			// TopLevelSplitContainer.Panel2
			// 
			this.TopLevelSplitContainer.Panel2.Controls.Add(this.InstructionsGrid);
			this.TopLevelSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 143, true);
			this.TopLevelSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(227);
			this.TopLevelSplitContainer.TabIndex = 1;
			// 
			// PackageGrid
			// 
			this.PackageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackageGrid, "Packages_PackageView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).Package.KP_PackageID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).PackageDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).Package.Container.ContainerType.RC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).Package.KP_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).Package.KP_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).Package.KP_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).Package.KP_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).Package.KP_GoodsDescription)));
			this.PackageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Package+KP_PackageID";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "PackageDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "Package+Container+ContainerType+RC_Code";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Package+KP_Weight";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|cd150843-5b50-4144-b3f3-d4badb8fbe46", "Weight and Unit");
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.ColumnName = "Package+KP_WeightUQ";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|cd150843-5b50-4144-b3f3-d4badb8fbe46", "Weight and Unit");
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Package+KP_Volume";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|9a1c646f-3d98-4f7b-aa69-b7fcfca96e4f", "Volume and Unit");
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.ColumnName = "Package+KP_VolumeUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|9a1c646f-3d98-4f7b-aa69-b7fcfca96e4f", "Volume and Unit");
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo4.ColumnName = "Package+KP_GoodsDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			this.PackageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackageGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackageGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackageGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackageGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackageGrid.CopySelectedRowsAllowed = true;
			this.PackageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageGrid.GridId = "23743ce9-7946-45a8-8021-fdfdc00c281f";
			this.PackageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackageGrid.LayoutKey = "PackageGrid";
			this.PackageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageGrid.Name = "PackageGrid";
			this.PackageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 143, true);
			this.PackageGrid.TabIndex = 1;
			// 
			// InstructionsGrid
			// 
			this.InstructionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InstructionsGrid, "Packages_PackageView.InstructionDivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.KN_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).KD_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Package.KP_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Package.KP_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.KN_InstructionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.OrganisationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.Address.OrganisationNameOrPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.Address.OrganisationDataFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.Address.E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.KN_DropMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.KN_RQ_Equipment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.KN_ServiceInstruction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionPkgDivot)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingPackage_PackageView)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Packages_PackageView)).SyncRoot)).InstructionDivots)).SyncRoot)).Instruction.StatusDescription)));
			this.InstructionsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Instruction+KN_Sequence";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "KD_Quantity";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "Weight";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|cd150843-5b50-4144-b3f3-d4badb8fbe46", "Weight and Unit");
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.ColumnName = "Package+KP_WeightUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|cd150843-5b50-4144-b3f3-d4badb8fbe46", "Weight and Unit");
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "Volume";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|9a1c646f-3d98-4f7b-aa69-b7fcfca96e4f", "Volume and Unit");
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo4.ColumnName = "Package+KP_VolumeUQ";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|9a1c646f-3d98-4f7b-aa69-b7fcfca96e4f", "Volume and Unit");
			zDropEditColumnStyleInfo4.IsReadOnly = true;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDropEditColumnStyleInfo5.ColumnName = "Instruction+KN_InstructionType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo6.ColumnName = "Instruction+OrganisationType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|190eed05-f18e-404b-bd9b-411edfc4cfd3", "Org. Code", "Organization Code", "");
			zMultiControlColumnStyleInfo1.ColumnName = "Instruction+Address+OrganisationNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "Instruction+Address+OrganisationDataFieldType";
			zAddressDropEditColumnStyleInfo1.ColumnName = "Instruction+Address+E2_OA_Address";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo7.ColumnName = "Instruction+KN_DropMode";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Instruction+KN_RQ_Equipment";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zMultiLineTextBoxColumnInfo1.ColumnName = "Instruction+KN_ServiceInstruction";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingPackageViewControl|ec63c446-7ade-4d4b-888d-eab5d2b79c5a", "Status");
			zTextBoxColumnStyleInfo5.ColumnName = "Instruction+StatusDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.InstructionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.InstructionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.InstructionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.InstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.InstructionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.InstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.InstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.InstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.InstructionsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.InstructionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.InstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InstructionsGrid.CopySelectedRowsAllowed = true;
			this.InstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InstructionsGrid.GridId = "062d58c1-a058-4bff-88ec-bdcafb791682";
			this.InstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InstructionsGrid.LayoutKey = "InstructionsGridOnPackingView";
			this.InstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InstructionsGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.InstructionsGrid.Name = "InstructionsGrid";
			this.InstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 143, true);
			this.InstructionsGrid.TabIndex = 1;
			// 
			// TransportBookingPackageViewControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackageViewGroupBox);
			this.Name = "TransportBookingPackageViewControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackageViewGroupBox.ResumeLayout(false);
			this.TopLevelSplitContainer.Panel1.ResumeLayout(false);
			this.TopLevelSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TopLevelSplitContainer)).EndInit();
			this.TopLevelSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackageGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InstructionsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer TopLevelSplitContainer;
		internal ZArchitecture.ZGrid PackageGrid;
		private ZArchitecture.ZGrid InstructionsGrid;
		private Packing.GUI.ZGroupBoxWithoutCaption PackageViewGroupBox;
	}
}
