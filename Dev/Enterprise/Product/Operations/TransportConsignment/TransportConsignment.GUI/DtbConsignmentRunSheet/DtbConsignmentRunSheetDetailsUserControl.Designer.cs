namespace Enterprise.TransportConsignment.GUI
{
	partial class DtbConsignmentRunSheetDetailsUserControl
	{
		System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo41 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ActionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConfirmationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConfirmationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ActionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DetailsTabControl.SuspendLayout();
            this.ConfirmationsTabPage.SuspendLayout();
			this.ActionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConfirmationsGrid)).BeginInit();
            this.ConfirmationsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ActionsGrid)).BeginInit();
            this.ActionsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet);
            // 
            // DetailsTabControl
            // 
            this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.DetailsTabControl.Controls.Add(this.ConfirmationsTabPage);
            this.DetailsTabControl.Controls.Add(this.ActionsTabPage);
            this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DetailsTabControl.Name = "DetailsTabControl";
            this.DetailsTabControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 150, true);
            this.DetailsTabControl.TabIndex = 0;
            // 
            // ConfirmationsTabPage
            // 
            this.ConfirmationsTabPage.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("5fb1ea33-705f-435e-a9a0-79fae6a081d5", "Consignments");
            this.ConfirmationsTabPage.Controls.Add(this.ConfirmationsGrid);
			this.ConfirmationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.ConfirmationsTabPage.Name = "ConfirmationsTabPage";
            this.ConfirmationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConfirmationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 120, true);
			this.ConfirmationsTabPage.TabIndex = 0;
            this.ConfirmationsTabPage.UseVisualStyleBackColor = true;
            // 
            // ConfirmationsGrid
            // 
            this.ConfirmationsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ConfirmationsGrid, "RunSheetInstructions.Confirmations");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).KK_ConfirmationType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).BookingID)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignmentID)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalPackages)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalWeightUnit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalVolume)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalVolumeUnit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).KK_RequiredFrom)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).KK_RequiredTo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).IsHazardous)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).RequiresRefrigeration)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorAddressAsSingleLine)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorCity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorPostcode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorState)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeAddressAsSingleLine)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeCity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneePostcode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeState)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).BillToPartyCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).Instruction.Booking.KM_TransportReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).Instruction.Booking.KM_RS_NKServiceLevel)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ReceivedBy)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).HasSignature)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentConfirmation)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).RunSheetInstruction.K1_FailureReason)));
			this.ConfirmationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "KK_ConfirmationType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo3.ColumnName = "ConsignmentID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TotalWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.TransportConsignment.GUI.Res.GetData("DtbConsignmentConfirmation|TotalWeightGroup", "Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo4.ColumnName = "TotalWeightUnit";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.TransportConsignment.GUI.Res.GetData("DtbConsignmentConfirmation|TotalWeightGroup", "Weight");
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDateEditColumnStyleInfo1.ColumnName = "KK_RequiredFrom";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "KK_RequiredTo";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo6.ColumnName = "ConsignorName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.ColumnName = "ConsignorAddressAsSingleLine";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo8.ColumnName = "ConsignorCity";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo9.ColumnName = "ConsignorPostcode";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo10.ColumnName = "ConsignorState";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo11.ColumnName = "ConsigneeName";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo12.ColumnName = "ConsigneeAddressAsSingleLine";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo13.ColumnName = "ConsigneeCity";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo14.ColumnName = "ConsigneePostcode";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo15.ColumnName = "ConsigneeState";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo16.ColumnName = "ConsignorReference";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo17.ColumnName = "ConsigneeReference";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo18.ColumnName = "BillToPartyCode";
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.ColumnName = "Instruction+Booking+KM_TransportReference";
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Instruction+Booking+KM_RS_NKServiceLevel";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ConfirmationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ConfirmationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ConfirmationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.ConfirmationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.ConfirmationsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ConfirmationsGrid.CopySelectedRowsAllowed = true;
			this.ConfirmationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfirmationsGrid.GridId = "2d22df03-00d2-413f-aea7-c3fa76e853e9";
			this.ConfirmationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConfirmationsGrid.IsWholeRowSelectedOnClick = true;
			this.ConfirmationsGrid.LayoutKey = "ConsignmentsGrid";
			this.ConfirmationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ConfirmationsGrid.Name = "ConfirmationsGrid";
			this.ConfirmationsGrid.ReadOnly = true;
			this.ConfirmationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 113, true);
			this.ConfirmationsGrid.TabIndex = 2;
			// 
			// ActionsTabPage
			// 
			this.ActionsTabPage.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("5fb1ea33-705f-435e-a9a0-79fae6a081d5", "Consignments");
			this.ActionsTabPage.Controls.Add(this.ActionsGrid);
			this.ActionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.ActionsTabPage.Name = "ActionsTabPage";
			this.ActionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ActionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 120, true);
			this.ActionsTabPage.TabIndex = 0;
			this.ActionsTabPage.UseVisualStyleBackColor = true;
			// 
			// ActionsGrid
			// 
			this.ActionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ActionsGrid, "RunSheetInstructions.Actions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Actions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).LTA_ActionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).BookingID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignmentID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).TotalVolumeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).LTA_RequiredFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).LTA_RequiredTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).IsHazardous)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).RequiresRefrigeration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorAddressAsSingleLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorCity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorPostcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeAddressAsSingleLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeCity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneePostcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignorReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsigneeReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).BillToPartyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignmentAddress.Consignment.LTC_ConnoteNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ConsignmentAddress.Consignment.LTC_RS_NKServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).ReceivedBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).HasSignature)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentAction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Confirmations)).SyncRoot)).RunSheetInstruction.K1_FailureReason)));
			this.ActionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo41.ColumnName = "LTA_ActionType";
			zTextBoxColumnStyleInfo41.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo3.ColumnName = "ConsignmentID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TotalWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.TransportConsignment.GUI.Res.GetData("DtbConsignmentAction|TotalWeightGroup", "Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo4.ColumnName = "TotalWeightUnit";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.TransportConsignment.GUI.Res.GetData("DtbConsignmentAction|TotalWeightGroup", "Weight");
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDateEditColumnStyleInfo1.ColumnName = "KK_RequiredFrom";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "KK_RequiredTo";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo6.ColumnName = "ConsignorName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.ColumnName = "ConsignorAddressAsSingleLine";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo8.ColumnName = "ConsignorCity";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo9.ColumnName = "ConsignorPostcode";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo10.ColumnName = "ConsignorState";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo11.ColumnName = "ConsigneeName";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo12.ColumnName = "ConsigneeAddressAsSingleLine";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo13.ColumnName = "ConsigneeCity";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo14.ColumnName = "ConsigneePostcode";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo15.ColumnName = "ConsigneeState";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo16.ColumnName = "ConsignorReference";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo17.ColumnName = "ConsigneeReference";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo18.ColumnName = "BillToPartyCode";
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.ColumnName = "Instruction+Booking+KM_TransportReference";
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Instruction+Booking+KM_RS_NKServiceLevel";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo41);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.ActionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.ActionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.ActionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
            this.ActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
            this.ActionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ActionsGrid.CopySelectedRowsAllowed = true;
			this.ActionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActionsGrid.GridId = "23a0a1e5-aa52-4c1c-8483-25a23a2d5ef3";
            this.ActionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ActionsGrid.IsWholeRowSelectedOnClick = true;
            this.ActionsGrid.LayoutKey = "ActionsGrid";
            this.ActionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.ActionsGrid.Name = "ActionsGrid";
            this.ActionsGrid.ReadOnly = true;
			this.ActionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 113, true);
			this.ActionsGrid.TabIndex = 1;
            // 
            // DtbConsignmentRunSheetDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.DetailsTabControl);
            this.Name = "DtbConsignmentRunSheetDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 150, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DetailsTabControl.ResumeLayout(false);
            this.DetailsTabControl.PerformLayout();
            this.ConfirmationsTabPage.ResumeLayout(false);
            this.ConfirmationsTabPage.PerformLayout();
            this.ActionsTabPage.ResumeLayout(false);
            this.ActionsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConfirmationsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ActionsGrid)).EndInit();
			this.ConfirmationsGrid.ResumeLayout(false);
			this.ConfirmationsGrid.PerformLayout();
			this.ActionsGrid.ResumeLayout(false);
            this.ActionsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZTabControl DetailsTabControl;
		internal ZArchitecture.GUI.ZTabPage ConfirmationsTabPage;
		internal ZArchitecture.GUI.ZTabPage ActionsTabPage;
		internal ZArchitecture.ZGrid ConfirmationsGrid;
		internal ZArchitecture.ZGrid ActionsGrid;
	}
}
