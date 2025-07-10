namespace Enterprise.Customs.US.LVS.GUI
{
	partial class TransportDetailsUserControl
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
			this.groupBoxTransportDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.panelTransportDetailGroup = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.dateEditArrival = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.codeFindBoxEntryPort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.codeFindBoxDischargeUNLOCO = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dateEditDischarge = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.codeFindBoxDischargePort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dropEditDischargePort = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.codeFindBoxLoadingUNLOCO = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dateEditDeparture = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.codeFindBoxLoadingPort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dropEditLoadingPort = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.codeFindBoxCarrierSCAC = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.textBoxFlightNo = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxVoyageNo = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxTripID = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxJourney = new Enterprise.ZArchitecture.ZTextBox();
			this.codeFindBoxVessel = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.masterBillControl = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.textBoxOceanBill = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxMailReference = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxMasterBill = new Enterprise.ZArchitecture.ZTextBox();
			this.codeFindBoxIssuerSCAC = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dropEditContainerMode = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dropEditModeOfTransport = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.textBoxCalculatedModeOfTransport = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBoxTransportDetails.SuspendLayout();
			this.panelTransportDetailGroup.SuspendLayout();
			this.dateEditArrival.SuspendLayout();
			this.codeFindBoxEntryPort.SuspendLayout();
			this.codeFindBoxDischargeUNLOCO.SuspendLayout();
			this.dateEditDischarge.SuspendLayout();
			this.codeFindBoxDischargePort.SuspendLayout();
			this.dropEditDischargePort.SuspendLayout();
			this.codeFindBoxLoadingUNLOCO.SuspendLayout();
			this.dateEditDeparture.SuspendLayout();
			this.codeFindBoxLoadingPort.SuspendLayout();
			this.dropEditLoadingPort.SuspendLayout();
			this.codeFindBoxCarrierSCAC.SuspendLayout();
			this.codeFindBoxVessel.SuspendLayout();
			this.masterBillControl.SuspendLayout();
			this.codeFindBoxIssuerSCAC.SuspendLayout();
			this.dropEditContainerMode.SuspendLayout();
			this.dropEditModeOfTransport.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVClearance);
			// 
			// groupBoxTransportDetails
			// 
			this.groupBoxTransportDetails.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("57632923-254b-4ef0-b5ef-8995cd8bd60f", "Transport Details");
			this.groupBoxTransportDetails.Controls.Add(this.panelTransportDetailGroup);
			this.groupBoxTransportDetails.Controls.Add(this.dropEditContainerMode);
			this.groupBoxTransportDetails.Controls.Add(this.dropEditModeOfTransport);
			this.groupBoxTransportDetails.Controls.Add(this.textBoxCalculatedModeOfTransport);
			this.groupBoxTransportDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxTransportDetails.Name = "groupBoxTransportDetails";
			this.groupBoxTransportDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 216, true);
			this.groupBoxTransportDetails.TabIndex = 0;
			this.groupBoxTransportDetails.TabStop = false;
			// 
			// panelTransportDetailGroup
			// 
			this.panelTransportDetailGroup.Controls.Add(this.dateEditArrival);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxEntryPort);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxDischargeUNLOCO);
			this.panelTransportDetailGroup.Controls.Add(this.dateEditDischarge);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxDischargePort);
			this.panelTransportDetailGroup.Controls.Add(this.dropEditDischargePort);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxLoadingUNLOCO);
			this.panelTransportDetailGroup.Controls.Add(this.dateEditDeparture);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxLoadingPort);
			this.panelTransportDetailGroup.Controls.Add(this.dropEditLoadingPort);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxCarrierSCAC);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxFlightNo);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxVoyageNo);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxTripID);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxJourney);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxVessel);
			this.panelTransportDetailGroup.Controls.Add(this.masterBillControl);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxOceanBill);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxMailReference);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxMasterBill);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxIssuerSCAC);
			this.panelTransportDetailGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.panelTransportDetailGroup.Name = "panelTransportDetailGroup";
			this.panelTransportDetailGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 146, true);
			this.panelTransportDetailGroup.TabIndex = 3;
			// 
			// dateEditArrival
			// 
			this.dateEditArrival.AllowDrop = true;
			this.dateEditArrival.AutoCompleteMonthThreshold = 1;
			this.dateEditArrival.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditArrival, "ULH_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_EntryDate)));
			this.dateEditArrival.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("362b5c78-316f-4a5d-898b-b1eb1c53414a", "Arr.");
			this.dateEditArrival.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 123, true);
			this.dateEditArrival.Name = "dateEditArrival";
			this.dateEditArrival.TabIndex = 21;
			// 
			// codeFindBoxEntryPort
			// 
			this.codeFindBoxEntryPort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxEntryPort, "ULH_PortOfEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PortOfEntry)));
			this.codeFindBoxEntryPort.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("d2a220c1-096f-4b7f-b62e-9a1b4d5ae0fc", "Entry Port");
			this.codeFindBoxEntryPort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 123, true);
			this.codeFindBoxEntryPort.Name = "codeFindBoxEntryPort";
			this.codeFindBoxEntryPort.ShouldResize = true;
			this.codeFindBoxEntryPort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.codeFindBoxEntryPort.TabIndex = 20;
			// 
			// codeFindBoxDischargeUNLOCO
			// 
			this.codeFindBoxDischargeUNLOCO.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxDischargeUNLOCO, "ULH_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_RL_NKPortOfDischarge)));
			this.codeFindBoxDischargeUNLOCO.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 99, true);
			this.codeFindBoxDischargeUNLOCO.Name = "codeFindBoxDischargeUNLOCO";
			this.codeFindBoxDischargeUNLOCO.PreBoundMaxLength = 5;
			this.codeFindBoxDischargeUNLOCO.ShouldResize = true;
			this.codeFindBoxDischargeUNLOCO.ShowDescriptionBox = false;
			this.codeFindBoxDischargeUNLOCO.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.codeFindBoxDischargeUNLOCO.TabIndex = 19;
			// 
			// dateEditDischarge
			// 
			this.dateEditDischarge.AllowDrop = true;
			this.dateEditDischarge.AutoCompleteMonthThreshold = 1;
			this.dateEditDischarge.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditDischarge, "ULH_DischargeDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_DischargeDate)));
			this.dateEditDischarge.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("2f4bf783-d92c-4d46-a5d9-b340e8e1537c", "Arr.");
			this.dateEditDischarge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 99, true);
			this.dateEditDischarge.Name = "dateEditDischarge";
			this.dateEditDischarge.TabIndex = 18;
			// 
			// codeFindBoxDischargePort
			// 
			this.codeFindBoxDischargePort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxDischargePort, "ULH_PortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PortOfDischarge)));
			this.codeFindBoxDischargePort.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("51692669-2705-42fa-b39d-c91df1c22d41", "Discharge");
			this.codeFindBoxDischargePort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 99, true);
			this.codeFindBoxDischargePort.Name = "codeFindBoxDischargePort";
			this.codeFindBoxDischargePort.ShouldResize = true;
			this.codeFindBoxDischargePort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.codeFindBoxDischargePort.TabIndex = 17;
			// 
			// dropEditDischargePort
			// 
			this.dropEditDischargePort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditDischargePort, "ULH_PortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PortOfDischarge)));
			this.dropEditDischargePort.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("51692669-2705-42fa-b39d-c91df1c22d41", "Discharge");
			this.dropEditDischargePort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 99, true);
			this.dropEditDischargePort.Name = "dropEditDischargePort";
			this.dropEditDischargePort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.dropEditDischargePort.TabIndex = 17;
			// 
			// codeFindBoxLoadingUNLOCO
			// 
			this.codeFindBoxLoadingUNLOCO.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxLoadingUNLOCO, "ULH_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_RL_NKPortOfLoading)));
			this.codeFindBoxLoadingUNLOCO.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 75, true);
			this.codeFindBoxLoadingUNLOCO.Name = "codeFindBoxLoadingUNLOCO";
			this.codeFindBoxLoadingUNLOCO.PreBoundMaxLength = 5;
			this.codeFindBoxLoadingUNLOCO.ShouldResize = true;
			this.codeFindBoxLoadingUNLOCO.ShowDescriptionBox = false;
			this.codeFindBoxLoadingUNLOCO.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.codeFindBoxLoadingUNLOCO.TabIndex = 16;
			// 
			// dateEditDeparture
			// 
			this.dateEditDeparture.AllowDrop = true;
			this.dateEditDeparture.AutoCompleteMonthThreshold = 1;
			this.dateEditDeparture.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditDeparture, "ULH_DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_DepartureDate)));
			this.dateEditDeparture.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("cb511dbe-564b-4853-9f31-f7cc6f7a45bf", "Dep.");
			this.dateEditDeparture.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 75, true);
			this.dateEditDeparture.Name = "dateEditDeparture";
			this.dateEditDeparture.TabIndex = 15;
			// 
			// codeFindBoxLoadingPort
			// 
			this.codeFindBoxLoadingPort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxLoadingPort, "ULH_PortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PortOfLoading)));
			this.codeFindBoxLoadingPort.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("94d068f7-ec21-4c3b-82fd-db14636ef50e", "Loading");
			this.codeFindBoxLoadingPort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 75, true);
			this.codeFindBoxLoadingPort.Name = "codeFindBoxLoadingPort";
			this.codeFindBoxLoadingPort.ShouldResize = true;
			this.codeFindBoxLoadingPort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.codeFindBoxLoadingPort.TabIndex = 14;
			// 
			// dropEditLoadingPort
			// 
			this.dropEditLoadingPort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditLoadingPort, "ULH_PortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PortOfLoading)));
			this.dropEditLoadingPort.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("94d068f7-ec21-4c3b-82fd-db14636ef50e", "Loading");
			this.dropEditLoadingPort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 75, true);
			this.dropEditLoadingPort.Name = "dropEditLoadingPort";
			this.dropEditLoadingPort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.dropEditLoadingPort.TabIndex = 14;
			// 
			// codeFindBoxCarrierSCAC
			// 
			this.codeFindBoxCarrierSCAC.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxCarrierSCAC, "ULH_CarrierSCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_CarrierSCAC)));
			this.codeFindBoxCarrierSCAC.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("0ab9722f-3428-4969-a619-db8b83e16ce0", "Carrier SCAC");
			this.codeFindBoxCarrierSCAC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 51, true);
			this.codeFindBoxCarrierSCAC.Name = "codeFindBoxCarrierSCAC";
			this.codeFindBoxCarrierSCAC.PreBoundMaxLength = 4;
			this.codeFindBoxCarrierSCAC.ShouldResize = true;
			this.codeFindBoxCarrierSCAC.ShowDescriptionBox = false;
			this.codeFindBoxCarrierSCAC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.codeFindBoxCarrierSCAC.TabIndex = 13;
			// 
			// textBoxFlightNo
			// 
			this.BindingSource.SetBindingMember(this.textBoxFlightNo, "ULH_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_VoyageFlightNo)));
			this.textBoxFlightNo.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("3f68f470-d611-404c-8f56-4f8ec84715ba", "Flight");
			this.textBoxFlightNo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 51, true);
			this.textBoxFlightNo.Name = "textBoxFlightNo";
			this.textBoxFlightNo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.textBoxFlightNo.TabIndex = 12;
			// 
			// textBoxVoyageNo
			// 
			this.BindingSource.SetBindingMember(this.textBoxVoyageNo, "ULH_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_VoyageFlightNo)));
			this.textBoxVoyageNo.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("e8b2be41-321c-4796-861e-6613cfac8002", "Voyage");
			this.textBoxVoyageNo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 51, true);
			this.textBoxVoyageNo.Name = "textBoxVoyageNo";
			this.textBoxVoyageNo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.textBoxVoyageNo.TabIndex = 10;
			// 
			// textBoxTripID
			// 
			this.BindingSource.SetBindingMember(this.textBoxTripID, "ULH_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_VoyageFlightNo)));
			this.textBoxTripID.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("e79be4a2-be1e-489b-b2d2-66ba5ab4542a", "Trip ID");
			this.textBoxTripID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 51, true);
			this.textBoxTripID.Name = "textBoxTripID";
			this.textBoxTripID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.textBoxTripID.TabIndex = 11;
			// 
			// textBoxJourney
			// 
			this.BindingSource.SetBindingMember(this.textBoxJourney, "ULH_ConveyanceName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_ConveyanceName)));
			this.textBoxJourney.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("3a784b14-f68d-4d6f-8d8c-8bf1e74a0d3b", "Journey");
			this.textBoxJourney.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 27, true);
			this.textBoxJourney.Name = "textBoxJourney";
			this.textBoxJourney.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.textBoxJourney.TabIndex = 9;
			// 
			// codeFindBoxVessel
			// 
			this.codeFindBoxVessel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxVessel, "ULH_ConveyanceName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_ConveyanceName)));
			this.codeFindBoxVessel.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("a59876ad-dbb2-414f-8186-0b6bd0896bf6", "Vessel");
			this.codeFindBoxVessel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 27, true);
			this.codeFindBoxVessel.Name = "codeFindBoxVessel";
			this.codeFindBoxVessel.PreBoundMaxLength = 35;
			this.codeFindBoxVessel.ShouldResize = true;
			this.codeFindBoxVessel.ShowDescriptionBox = false;
			this.codeFindBoxVessel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.codeFindBoxVessel.TabIndex = 8;
			// 
			// masterBillControl
			// 
			this.masterBillControl.AllowAlphaInMAWP = false;
			this.masterBillControl.AllowDrop = true;
			this.masterBillControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.masterBillControl, "ULH_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBill)));
			this.masterBillControl.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("16915439-cf16-4298-adb8-c56d22926bc2", "Master Bill");
			this.masterBillControl.FormattedMasterBill = "";
			this.masterBillControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 3, true);
			this.masterBillControl.Name = "masterBillControl";
			this.masterBillControl.ReadOnly = false;
			this.masterBillControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.masterBillControl.TabIndex = 7;
			// 
			// textBoxOceanBill
			// 
			this.BindingSource.SetBindingMember(this.textBoxOceanBill, "ULH_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBill)));
			this.textBoxOceanBill.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("479ab654-2eb0-4bfa-bb2b-b902cc722195", "Ocean Bill");
			this.textBoxOceanBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 3, true);
			this.textBoxOceanBill.Name = "textBoxOceanBill";
			this.textBoxOceanBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.textBoxOceanBill.TabIndex = 6;
			// 
			// textBoxMailReference
			// 
			this.BindingSource.SetBindingMember(this.textBoxMailReference, "ULH_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBill)));
			this.textBoxMailReference.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("defb7a6f-cea9-47b1-815d-d4f6926f05ba", "Mail Reference");
			this.textBoxMailReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 3, true);
			this.textBoxMailReference.Name = "textBoxMailReference";
			this.textBoxMailReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.textBoxMailReference.TabIndex = 5;
			// 
			// textBoxMasterBill
			// 
			this.BindingSource.SetBindingMember(this.textBoxMasterBill, "ULH_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBill)));
			this.textBoxMasterBill.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("fd3093c7-f754-46c1-a985-803fc6c27924", "Master Bill");
			this.textBoxMasterBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 3, true);
			this.textBoxMasterBill.Name = "textBoxMasterBill";
			this.textBoxMasterBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.textBoxMasterBill.TabIndex = 4;
			// 
			// codeFindBoxIssuerSCAC
			// 
			this.codeFindBoxIssuerSCAC.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxIssuerSCAC, "ULH_MasterBillIssuerSCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBillIssuerSCAC)));
			this.codeFindBoxIssuerSCAC.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("c49e8c82-3d30-4e35-bcaf-330dd3da78b3", "Issuer SCAC");
			this.codeFindBoxIssuerSCAC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 3, true);
			this.codeFindBoxIssuerSCAC.Name = "codeFindBoxIssuerSCAC";
			this.codeFindBoxIssuerSCAC.ShouldResize = true;
			this.codeFindBoxIssuerSCAC.ShowDescriptionBox = false;
			this.codeFindBoxIssuerSCAC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.codeFindBoxIssuerSCAC.TabIndex = 3;
			// 
			// dropEditContainerMode
			// 
			this.dropEditContainerMode.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditContainerMode, "ULH_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_ContainerMode)));
			this.dropEditContainerMode.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("95a58e51-e805-4c51-b11e-f7ef3cb2ca16", "Container");
			this.dropEditContainerMode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 43, true);
			this.dropEditContainerMode.Name = "dropEditContainerMode";
			this.dropEditContainerMode.PreBoundMaxLength = 3;
			this.dropEditContainerMode.ShouldResizeByMaxLength = true;
			this.dropEditContainerMode.ShowDescriptionBox = false;
			this.dropEditContainerMode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.dropEditContainerMode.TabIndex = 2;
			// 
			// dropEditModeOfTransport
			// 
			this.dropEditModeOfTransport.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditModeOfTransport, "ULH_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_TransportMode)));
			this.dropEditModeOfTransport.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("ef7d335d-a8ec-408b-9c89-ab38a1d0e08f", "Mode of Transport");
			this.dropEditModeOfTransport.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 19, true);
			this.dropEditModeOfTransport.Name = "dropEditModeOfTransport";
			this.dropEditModeOfTransport.PreBoundMaxLength = 3;
			this.dropEditModeOfTransport.ShouldResizeByMaxLength = true;
			this.dropEditModeOfTransport.ShowDescriptionBox = false;
			this.dropEditModeOfTransport.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.dropEditModeOfTransport.TabIndex = 0;
			// 
			// textBoxCalculatedModeOfTransport
			// 
			this.BindingSource.SetBindingMember(this.textBoxCalculatedModeOfTransport, "ULH_Calc_USTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_Calc_USTransportMode)));
			this.textBoxCalculatedModeOfTransport.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("3880ead4-952a-4007-82fa-b34a39e698c3", "Calculated MOT");
			this.textBoxCalculatedModeOfTransport.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 19, true);
			this.textBoxCalculatedModeOfTransport.Name = "textBoxCalculatedModeOfTransport";
			this.textBoxCalculatedModeOfTransport.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.textBoxCalculatedModeOfTransport.TabIndex = 1;
			// 
			// TransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.groupBoxTransportDetails);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 216, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBoxTransportDetails.ResumeLayout(false);
			this.groupBoxTransportDetails.PerformLayout();
			this.panelTransportDetailGroup.ResumeLayout(false);
			this.panelTransportDetailGroup.PerformLayout();
			this.dateEditArrival.ResumeLayout(true);
			this.dateEditArrival.PerformLayout();
			this.codeFindBoxEntryPort.ResumeLayout(true);
			this.codeFindBoxEntryPort.PerformLayout();
			this.codeFindBoxDischargeUNLOCO.ResumeLayout(true);
			this.codeFindBoxDischargeUNLOCO.PerformLayout();
			this.dateEditDischarge.ResumeLayout(true);
			this.dateEditDischarge.PerformLayout();
			this.codeFindBoxDischargePort.ResumeLayout(true);
			this.codeFindBoxDischargePort.PerformLayout();
			this.dropEditDischargePort.ResumeLayout(true);
			this.dropEditDischargePort.PerformLayout();
			this.codeFindBoxLoadingUNLOCO.ResumeLayout(true);
			this.codeFindBoxLoadingUNLOCO.PerformLayout();
			this.dateEditDeparture.ResumeLayout(true);
			this.dateEditDeparture.PerformLayout();
			this.codeFindBoxLoadingPort.ResumeLayout(true);
			this.codeFindBoxLoadingPort.PerformLayout();
			this.dropEditLoadingPort.ResumeLayout(true);
			this.dropEditLoadingPort.PerformLayout();
			this.codeFindBoxCarrierSCAC.ResumeLayout(true);
			this.codeFindBoxCarrierSCAC.PerformLayout();
			this.codeFindBoxVessel.ResumeLayout(true);
			this.codeFindBoxVessel.PerformLayout();
			this.masterBillControl.ResumeLayout(true);
			this.masterBillControl.PerformLayout();
			this.codeFindBoxIssuerSCAC.ResumeLayout(true);
			this.codeFindBoxIssuerSCAC.PerformLayout();
			this.dropEditContainerMode.ResumeLayout(true);
			this.dropEditContainerMode.PerformLayout();
			this.dropEditModeOfTransport.ResumeLayout(true);
			this.dropEditModeOfTransport.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox groupBoxTransportDetails;
		private ZArchitecture.ZTextBox textBoxCalculatedModeOfTransport;
		private ZArchitecture.GUI.ZDropEdit dropEditModeOfTransport;
		private ZArchitecture.GUI.ZDropEdit dropEditContainerMode;
		private ZArchitecture.GUI.ZPanel panelTransportDetailGroup;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxIssuerSCAC;
		private ZArchitecture.ZTextBox textBoxMasterBill;
		private ZArchitecture.ZTextBox textBoxMailReference;
		private ZArchitecture.ZTextBox textBoxOceanBill;
		private ZArchitecture.ZMasterBillControl masterBillControl;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxVessel;
		private ZArchitecture.ZTextBox textBoxJourney;
		private ZArchitecture.ZTextBox textBoxTripID;
		private ZArchitecture.ZTextBox textBoxVoyageNo;
		private ZArchitecture.ZTextBox textBoxFlightNo;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxCarrierSCAC;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxLoadingPort;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxLoadingUNLOCO;
		private ZArchitecture.GUI.ZDateEdit dateEditDeparture;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxDischargePort;
		private ZArchitecture.GUI.ZDateEdit dateEditDischarge;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxDischargeUNLOCO;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxEntryPort;
		private ZArchitecture.GUI.ZDateEdit dateEditArrival;
		private ZArchitecture.GUI.ZDropEdit dropEditLoadingPort;
		private ZArchitecture.GUI.ZDropEdit dropEditDischargePort;
	}
}
