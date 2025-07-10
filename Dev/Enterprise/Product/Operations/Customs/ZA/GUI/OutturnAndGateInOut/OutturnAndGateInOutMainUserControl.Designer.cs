using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutMainUserControl
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
			if (disposing)
			{
				zCodeFindBoxVessel.PopupSelected -= ZCodeFindBoxVessel_PopupSelected;
			}
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
		void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.zGroupBoxHeader = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zGroupBoxCargoReportingInstructions = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zCodeFindBoxOutturnProvider = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zDropEditWithFixedWidthCustomsOffice = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zDateEditUnpacked = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.DeConsolidatorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.TerminalAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.zDropEditWithFixedWidthManifestType = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zDropGateInOutMessageType = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zDropEditWithFixedWidthCustomsStatus = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zDateEditGateInOutDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.GateInOutStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zDropEditWithFixedWidthExcessIndicator = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zDateEditFullyLoadedUnloadedDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zGroupBoxTransportDocumentDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zCodeFindBoxPortOfLoading = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zDropEditWithFixedWidthTransportMode = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zDropEditWithFixedWidthAgentType = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zTextBoxVoyage = new Enterprise.ZArchitecture.ZTextBox();
            this.zCodeFindBoxVessel = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zCodeFindBoxPortOfDischarge = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zDropEditWithFixedWidthNature = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zTextBoxManifestNumber = new Enterprise.ZArchitecture.ZTextBox();
            this.zDropEditWithFixedWidthContainerMode = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.BookingNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.zTextBoxParentBill = new Enterprise.ZArchitecture.ZTextBox();
            this.zDateEditDep = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zDateEditArv = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zDateEditIssueDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zAddressControlCarrierZAddress = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.zTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.zTabPageBill = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.outturnAndGateInOutBillUserControl = new Enterprise.Customs.ZA.GUI.OutturnAndGateInOutBillUserControl();
            this.zTabPageContainer = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.outturnAndGateInOutContainerUserControl = new Enterprise.Customs.ZA.GUI.OutturnAndGateInOutContainerUserControl();
            this.zTabPagePack = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.outturnAndGateInOutPackUserControl = new Enterprise.Customs.ZA.GUI.OutturnAndGateInOutPackUserControl();
            this.zTabPageMessages = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.outturnAndGateInOutMessageUserControl = new Enterprise.Customs.ZA.GUI.OutturnAndGateInOutMessageUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zGroupBoxHeader.SuspendLayout();
            this.zGroupBoxCargoReportingInstructions.SuspendLayout();
            this.zCodeFindBoxOutturnProvider.SuspendLayout();
            this.zDropEditWithFixedWidthCustomsOffice.SuspendLayout();
            this.zDateEditUnpacked.SuspendLayout();
            this.DeConsolidatorAddressControl.SuspendLayout();
            this.TerminalAddressControl.SuspendLayout();
            this.zDropEditWithFixedWidthManifestType.SuspendLayout();
            this.zDropGateInOutMessageType.SuspendLayout();
            this.zDropEditWithFixedWidthCustomsStatus.SuspendLayout();
            this.zDateEditGateInOutDate.SuspendLayout();
            this.GateInOutStatusDropEdit.SuspendLayout();
            this.zDropEditWithFixedWidthExcessIndicator.SuspendLayout();
            this.zDateEditFullyLoadedUnloadedDate.SuspendLayout();
            this.zGroupBoxTransportDocumentDetails.SuspendLayout();
            this.zCodeFindBoxPortOfLoading.SuspendLayout();
            this.zDropEditWithFixedWidthTransportMode.SuspendLayout();
            this.zDropEditWithFixedWidthAgentType.SuspendLayout();
            this.zCodeFindBoxVessel.SuspendLayout();
            this.zCodeFindBoxPortOfDischarge.SuspendLayout();
            this.zDropEditWithFixedWidthNature.SuspendLayout();
            this.zDropEditWithFixedWidthContainerMode.SuspendLayout();
            this.zDateEditDep.SuspendLayout();
            this.zDateEditArv.SuspendLayout();
            this.zDateEditIssueDate.SuspendLayout();
            this.zAddressControlCarrierZAddress.SuspendLayout();
            this.zTabControl.SuspendLayout();
            this.zTabPageBill.SuspendLayout();
            this.outturnAndGateInOutBillUserControl.SuspendLayout();
            this.zTabPageContainer.SuspendLayout();
            this.outturnAndGateInOutContainerUserControl.SuspendLayout();
            this.zTabPagePack.SuspendLayout();
            this.outturnAndGateInOutPackUserControl.SuspendLayout();
            this.zTabPageMessages.SuspendLayout();
            this.outturnAndGateInOutMessageUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.AsycudaManifestHeader);
            // 
            // zGroupBoxHeader
            // 
            this.zGroupBoxHeader.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("8bb2ba1f-b520-4ae4-9be5-61b90f5867c9", "Outturn & Gate In/Out");
            this.zGroupBoxHeader.Controls.Add(this.zGroupBoxCargoReportingInstructions);
            this.zGroupBoxHeader.Controls.Add(this.zGroupBoxTransportDocumentDetails);
            this.zGroupBoxHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.zGroupBoxHeader.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zGroupBoxHeader.Name = "zGroupBoxHeader";
            this.zGroupBoxHeader.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 373, true);
            this.zGroupBoxHeader.TabIndex = 0;
            this.zGroupBoxHeader.TabStop = false;
            // 
            // zGroupBoxCargoReportingInstructions
            // 
            this.zGroupBoxCargoReportingInstructions.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("5b2f9848-2748-4ef8-a57a-d51c7c03d04b", "Cargo Reporting Instructions");
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.zCodeFindBoxOutturnProvider);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.zDropEditWithFixedWidthCustomsOffice);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.zDateEditUnpacked);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.DeConsolidatorAddressControl);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.TerminalAddressControl);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.zDropEditWithFixedWidthManifestType);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.zDropGateInOutMessageType);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.zDropEditWithFixedWidthCustomsStatus);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.zDateEditGateInOutDate);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.GateInOutStatusDropEdit);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.zDropEditWithFixedWidthExcessIndicator);
            this.zGroupBoxCargoReportingInstructions.Controls.Add(this.zDateEditFullyLoadedUnloadedDate);
            this.zGroupBoxCargoReportingInstructions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 17, true);
            this.zGroupBoxCargoReportingInstructions.Name = "zGroupBoxCargoReportingInstructions";
            this.zGroupBoxCargoReportingInstructions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 352, true);
            this.zGroupBoxCargoReportingInstructions.TabIndex = 35;
            this.zGroupBoxCargoReportingInstructions.TabStop = false;
            // 
            // zCodeFindBoxOutturnProvider
            // 
            this.zCodeFindBoxOutturnProvider.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBoxOutturnProvider, "OutturnProvider");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).OutturnProvider)));
            this.zCodeFindBoxOutturnProvider.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 17, true);
            this.zCodeFindBoxOutturnProvider.Name = "zCodeFindBoxOutturnProvider";
            this.zCodeFindBoxOutturnProvider.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBoxOutturnProvider.ParentType = null;
            this.zCodeFindBoxOutturnProvider.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zCodeFindBoxOutturnProvider.TabIndex = 1;
            // 
            // zDropEditWithFixedWidthCustomsOffice
            // 
            this.zDropEditWithFixedWidthCustomsOffice.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthCustomsOffice, "AMA_CustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_CustomsOffice)));
            this.zDropEditWithFixedWidthCustomsOffice.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 43, true);
            this.zDropEditWithFixedWidthCustomsOffice.Name = "zDropEditWithFixedWidthCustomsOffice";
            this.zDropEditWithFixedWidthCustomsOffice.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthCustomsOffice.TabIndex = 2;
            // 
            // zDateEditUnpacked
            // 
            this.zDateEditUnpacked.AllowDrop = true;
            this.zDateEditUnpacked.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEditUnpacked, "UnpackedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).UnpackedDate)));
            this.zDateEditUnpacked.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.zDateEditUnpacked.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 181, true);
            this.zDateEditUnpacked.Name = "zDateEditUnpacked";
            this.zDateEditUnpacked.TabIndex = 7;
            // 
            // DeConsolidatorAddressControl
            // 
            this.DeConsolidatorAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DeConsolidatorAddressControl, "AMA_OA_DeconsolidateAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_OA_DeconsolidateAddress)));
            this.DeConsolidatorAddressControl.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("fea8a32c-10f4-4b6c-acf6-ebe967d004ca", "Depot");
            this.DeConsolidatorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 68, true);
            this.DeConsolidatorAddressControl.Name = "DeConsolidatorAddressControl";
            this.DeConsolidatorAddressControl.PopupCaption = "";
            this.DeConsolidatorAddressControl.ShowAddress = false;
            this.DeConsolidatorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 17, true);
            this.DeConsolidatorAddressControl.TabIndex = 3;
            // 
            // TerminalAddressControl
            // 
            this.TerminalAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TerminalAddressControl, "AMA_OA_DischargeTerminalAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_OA_DischargeTerminalAddress)));
            this.TerminalAddressControl.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("8b1a7ddc-6253-4eb4-a2b3-607f469227c8", "Terminal");
            this.TerminalAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 93, true);
            this.TerminalAddressControl.Name = "TerminalAddressControl";
            this.TerminalAddressControl.PopupCaption = "";
            this.TerminalAddressControl.ShowAddress = false;
            this.TerminalAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 17, true);
            this.TerminalAddressControl.TabIndex = 4;
            // 
            // zDropEditWithFixedWidthManifestType
            // 
            this.zDropEditWithFixedWidthManifestType.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthManifestType, "AMA_ManifestType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_ManifestType)));
            this.zDropEditWithFixedWidthManifestType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 131, true);
            this.zDropEditWithFixedWidthManifestType.Name = "zDropEditWithFixedWidthManifestType";
            this.zDropEditWithFixedWidthManifestType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthManifestType.TabIndex = 5;
            // 
            // zDropGateInOutMessageType
            // 
            this.zDropGateInOutMessageType.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropGateInOutMessageType, "GateInOutMessageType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).GateInOutMessageType)));
            this.zDropGateInOutMessageType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 270, true);
            this.zDropGateInOutMessageType.Name = "zDropGateInOutMessageType";
            this.zDropGateInOutMessageType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropGateInOutMessageType.TabIndex = 10;
            // 
            // zDropEditWithFixedWidthCustomsStatus
            // 
            this.zDropEditWithFixedWidthCustomsStatus.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthCustomsStatus, "RegistrationStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).RegistrationStatus)));
            this.zDropEditWithFixedWidthCustomsStatus.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("5bfee477-5c20-4886-80e4-f5ee9fba4558", "Inturn/Outturn Status");
            this.zDropEditWithFixedWidthCustomsStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 233, true);
            this.zDropEditWithFixedWidthCustomsStatus.Name = "zDropEditWithFixedWidthCustomsStatus";
            this.zDropEditWithFixedWidthCustomsStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthCustomsStatus.TabIndex = 9;
            // 
            // zDateEditGateInOutDate
            // 
            this.zDateEditGateInOutDate.AllowDrop = true;
            this.zDateEditGateInOutDate.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEditGateInOutDate, "GateInOutDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).GateInOutDate)));
            this.zDateEditGateInOutDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.zDateEditGateInOutDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 296, true);
            this.zDateEditGateInOutDate.Name = "zDateEditGateInOutDate";
            this.zDateEditGateInOutDate.TabIndex = 11;
            // 
            // GateInOutStatusDropEdit
            // 
            this.GateInOutStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GateInOutStatusDropEdit, "GateInOutCustomsStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).GateInOutCustomsStatus)));
            this.GateInOutStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 322, true);
            this.GateInOutStatusDropEdit.Name = "GateInOutStatusDropEdit";
            this.GateInOutStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.GateInOutStatusDropEdit.TabIndex = 12;
            // 
            // zDropEditWithFixedWidthExcessIndicator
            // 
            this.zDropEditWithFixedWidthExcessIndicator.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthExcessIndicator, "ExcessIndicator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).ExcessIndicator)));
            this.zDropEditWithFixedWidthExcessIndicator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 207, true);
            this.zDropEditWithFixedWidthExcessIndicator.Name = "zDropEditWithFixedWidthExcessIndicator";
            this.zDropEditWithFixedWidthExcessIndicator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthExcessIndicator.TabIndex = 8;
            // 
            // zDateEditFullyLoadedUnloadedDate
            // 
            this.zDateEditFullyLoadedUnloadedDate.AllowDrop = true;
            this.zDateEditFullyLoadedUnloadedDate.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEditFullyLoadedUnloadedDate, "FullyLoadedUnloadedDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).FullyLoadedUnloadedDate)));
            this.zDateEditFullyLoadedUnloadedDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.zDateEditFullyLoadedUnloadedDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 156, true);
            this.zDateEditFullyLoadedUnloadedDate.Name = "zDateEditFullyLoadedUnloadedDate";
            this.zDateEditFullyLoadedUnloadedDate.TabIndex = 6;
            // 
            // zGroupBoxTransportDocumentDetails
            // 
            this.zGroupBoxTransportDocumentDetails.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("a113343b-5856-4c8a-b1f4-626733c67cf4", "Transport Document Details");
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zCodeFindBoxPortOfLoading);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zDropEditWithFixedWidthTransportMode);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zDropEditWithFixedWidthAgentType);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zTextBoxVoyage);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zCodeFindBoxVessel);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zCodeFindBoxPortOfDischarge);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zDropEditWithFixedWidthNature);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zTextBoxManifestNumber);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zDropEditWithFixedWidthContainerMode);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.BookingNumberTextBox);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zTextBoxParentBill);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zDateEditDep);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zDateEditArv);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zDateEditIssueDate);
            this.zGroupBoxTransportDocumentDetails.Controls.Add(this.zAddressControlCarrierZAddress);
            this.zGroupBoxTransportDocumentDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 17, true);
            this.zGroupBoxTransportDocumentDetails.Name = "zGroupBoxTransportDocumentDetails";
            this.zGroupBoxTransportDocumentDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 352, true);
            this.zGroupBoxTransportDocumentDetails.TabIndex = 34;
            this.zGroupBoxTransportDocumentDetails.TabStop = false;
            // 
            // zCodeFindBoxPortOfLoading
            // 
            this.zCodeFindBoxPortOfLoading.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBoxPortOfLoading, "MasterBill.ABL_RL_NKPortOfLoading");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).MasterBill.ABL_RL_NKPortOfLoading)));
            this.zCodeFindBoxPortOfLoading.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 181, true);
            this.zCodeFindBoxPortOfLoading.Name = "zCodeFindBoxPortOfLoading";
            this.zCodeFindBoxPortOfLoading.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBoxPortOfLoading.ParentType = null;
            this.zCodeFindBoxPortOfLoading.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zCodeFindBoxPortOfLoading.TabIndex = 6;
            // 
            // zDropEditWithFixedWidthTransportMode
            // 
            this.zDropEditWithFixedWidthTransportMode.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthTransportMode, "AMA_TransportMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_TransportMode)));
            this.zDropEditWithFixedWidthTransportMode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 17, true);
            this.zDropEditWithFixedWidthTransportMode.Name = "zDropEditWithFixedWidthTransportMode";
            this.zDropEditWithFixedWidthTransportMode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthTransportMode.TabIndex = 0;
            // 
            // zDropEditWithFixedWidthAgentType
            // 
            this.zDropEditWithFixedWidthAgentType.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthAgentType, "AMA_AgentType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_AgentType)));
            this.zDropEditWithFixedWidthAgentType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 43, true);
            this.zDropEditWithFixedWidthAgentType.Name = "zDropEditWithFixedWidthAgentType";
            this.zDropEditWithFixedWidthAgentType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthAgentType.TabIndex = 1;
            // 
            // zTextBoxVoyage
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxVoyage, "AMA_Voyage");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_Voyage)));
            this.zTextBoxVoyage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 155, true);
            this.zTextBoxVoyage.Name = "zTextBoxVoyage";
            this.zTextBoxVoyage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zTextBoxVoyage.TabIndex = 5;
            // 
            // zCodeFindBoxVessel
            // 
            this.zCodeFindBoxVessel.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBoxVessel, "AMA_VesselName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_VesselName)));
            this.zCodeFindBoxVessel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 130, true);
            this.zCodeFindBoxVessel.Name = "zCodeFindBoxVessel";
            this.zCodeFindBoxVessel.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBoxVessel.ParentType = null;
            this.zCodeFindBoxVessel.ShowDescriptionBox = false;
            this.zCodeFindBoxVessel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
            this.zCodeFindBoxVessel.TabIndex = 4;
            // 
            // zCodeFindBoxPortOfDischarge
            // 
            this.zCodeFindBoxPortOfDischarge.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBoxPortOfDischarge, "MasterBill.ABL_RL_NKPortOfDischarge");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).MasterBill.ABL_RL_NKPortOfDischarge)));
            this.zCodeFindBoxPortOfDischarge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 206, true);
            this.zCodeFindBoxPortOfDischarge.Name = "zCodeFindBoxPortOfDischarge";
            this.zCodeFindBoxPortOfDischarge.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBoxPortOfDischarge.ParentType = null;
            this.zCodeFindBoxPortOfDischarge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zCodeFindBoxPortOfDischarge.TabIndex = 8;
            // 
            // zDropEditWithFixedWidthNature
            // 
            this.zDropEditWithFixedWidthNature.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthNature, "AMA_Nature");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_Nature)));
            this.zDropEditWithFixedWidthNature.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 68, true);
            this.zDropEditWithFixedWidthNature.Name = "zDropEditWithFixedWidthNature";
            this.zDropEditWithFixedWidthNature.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthNature.TabIndex = 2;
            // 
            // zTextBoxManifestNumber
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxManifestNumber, "AMA_MasterBill");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_MasterBill)));
            this.zTextBoxManifestNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 268, true);
            this.zTextBoxManifestNumber.Name = "zTextBoxManifestNumber";
            this.zTextBoxManifestNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zTextBoxManifestNumber.TabIndex = 11;
            // 
            // zDropEditWithFixedWidthContainerMode
            // 
            this.zDropEditWithFixedWidthContainerMode.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthContainerMode, "AMA_ContainerMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_ContainerMode)));
            this.zDropEditWithFixedWidthContainerMode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 319, true);
            this.zDropEditWithFixedWidthContainerMode.Name = "zDropEditWithFixedWidthContainerMode";
            this.zDropEditWithFixedWidthContainerMode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthContainerMode.TabIndex = 14;
            // 
            // BookingNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.BookingNumberTextBox, "BookingNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).BookingNumber)));
            this.BookingNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 243, true);
            this.BookingNumberTextBox.Name = "BookingNumberTextBox";
            this.BookingNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.BookingNumberTextBox.TabIndex = 10;
            // 
            // zTextBoxParentBill
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxParentBill, "ParentBill");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).ParentBill)));
            this.zTextBoxParentBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 293, true);
            this.zTextBoxParentBill.Name = "zTextBoxParentBill";
            this.zTextBoxParentBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zTextBoxParentBill.TabIndex = 13;
            // 
            // zDateEditDep
            // 
            this.zDateEditDep.AllowDrop = true;
            this.zDateEditDep.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEditDep, "MasterBill.ABL_E_DEP");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).MasterBill.ABL_E_DEP)));
            this.zDateEditDep.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.zDateEditDep.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 181, true);
            this.zDateEditDep.Name = "zDateEditDep";
            this.zDateEditDep.TabIndex = 7;
            // 
            // zDateEditArv
            // 
            this.zDateEditArv.AllowDrop = true;
            this.zDateEditArv.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEditArv, "MasterBill.ABL_E_ARV");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).MasterBill.ABL_E_ARV)));
            this.zDateEditArv.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.zDateEditArv.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 206, true);
            this.zDateEditArv.Name = "zDateEditArv";
            this.zDateEditArv.TabIndex = 9;
            // 
            // zDateEditIssueDate
            // 
            this.zDateEditIssueDate.AllowDrop = true;
            this.zDateEditIssueDate.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEditIssueDate, "AMA_IssueDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_IssueDate)));
            this.zDateEditIssueDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 268, true);
            this.zDateEditIssueDate.Name = "zDateEditIssueDate";
            this.zDateEditIssueDate.TabIndex = 12;
            // 
            // zAddressControlCarrierZAddress
            // 
            this.zAddressControlCarrierZAddress.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zAddressControlCarrierZAddress, "AMA_OA_Carrier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).AMA_OA_Carrier)));
            this.zAddressControlCarrierZAddress.BindToOrgList = "Lookups+CarrierList";
            this.zAddressControlCarrierZAddress.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f7be1a92-e53e-43d7-919b-075fefd673ae", "Carrier");
            this.zAddressControlCarrierZAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 105, true);
            this.zAddressControlCarrierZAddress.Name = "zAddressControlCarrierZAddress";
            this.zAddressControlCarrierZAddress.PopupCaption = "";
            this.zAddressControlCarrierZAddress.ShowAddress = false;
            this.zAddressControlCarrierZAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 17, true);
            this.zAddressControlCarrierZAddress.TabIndex = 3;
            // 
            // zTabControl
            // 
            this.zTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.zTabControl.Controls.Add(this.zTabPageBill);
            this.zTabControl.Controls.Add(this.zTabPageContainer);
            this.zTabControl.Controls.Add(this.zTabPagePack);
            this.zTabControl.Controls.Add(this.zTabPageMessages);
            this.zTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 373, true);
            this.zTabControl.Name = "zTabControl";
            this.zTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 304, true);
            this.zTabControl.TabIndex = 1;
            // 
            // zTabPageBill
            // 
            this.zTabPageBill.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("16e9eb0a-2aab-4c86-b976-aaf2305b5d87", "Bill");
            this.zTabPageBill.Controls.Add(this.outturnAndGateInOutBillUserControl);
            this.zTabPageBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.zTabPageBill.Name = "zTabPageBill";
            this.zTabPageBill.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPageBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1145, 282, true);
            this.zTabPageBill.TabIndex = 0;
            this.zTabPageBill.UseVisualStyleBackColor = true;
            // 
            // outturnAndGateInOutBillUserControl
            // 
            this.outturnAndGateInOutBillUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.outturnAndGateInOutBillUserControl, ".");
            this.outturnAndGateInOutBillUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outturnAndGateInOutBillUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.outturnAndGateInOutBillUserControl.Name = "outturnAndGateInOutBillUserControl";
            this.outturnAndGateInOutBillUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1139, 277, true);
            this.outturnAndGateInOutBillUserControl.TabIndex = 0;
            // 
            // zTabPageContainer
            // 
            this.zTabPageContainer.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("fa117e31-bf3c-4b7d-bfdb-1da5983aa6d4", "Container");
            this.zTabPageContainer.Controls.Add(this.outturnAndGateInOutContainerUserControl);
            this.zTabPageContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.zTabPageContainer.Name = "zTabPageContainer";
            this.zTabPageContainer.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPageContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1145, 282, true);
            this.zTabPageContainer.TabIndex = 1;
            this.zTabPageContainer.UseVisualStyleBackColor = true;
            // 
            // outturnAndGateInOutContainerUserControl
            // 
            this.outturnAndGateInOutContainerUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.outturnAndGateInOutContainerUserControl, ".");
            this.outturnAndGateInOutContainerUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outturnAndGateInOutContainerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.outturnAndGateInOutContainerUserControl.Name = "outturnAndGateInOutContainerUserControl";
            this.outturnAndGateInOutContainerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1139, 277, true);
            this.outturnAndGateInOutContainerUserControl.TabIndex = 0;
            // 
            // zTabPagePack
            // 
            this.zTabPagePack.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("8b3dda6b-f2ad-4199-bb3a-b2066817690a", "Packs");
            this.zTabPagePack.Controls.Add(this.outturnAndGateInOutPackUserControl);
            this.zTabPagePack.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.zTabPagePack.Name = "zTabPagePack";
            this.zTabPagePack.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1145, 282, true);
            this.zTabPagePack.TabIndex = 2;
            // 
            // outturnAndGateInOutPackUserControl
            // 
            this.outturnAndGateInOutPackUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.outturnAndGateInOutPackUserControl, ".");
            this.outturnAndGateInOutPackUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outturnAndGateInOutPackUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.outturnAndGateInOutPackUserControl.Name = "outturnAndGateInOutPackUserControl";
            this.outturnAndGateInOutPackUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1145, 282, true);
            this.outturnAndGateInOutPackUserControl.TabIndex = 0;
            // 
            // zTabPageMessages
            // 
            this.zTabPageMessages.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f1e74fe3-4f79-448a-8983-aee1c363ba4c", "Messages");
            this.zTabPageMessages.Controls.Add(this.outturnAndGateInOutMessageUserControl);
            this.zTabPageMessages.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.zTabPageMessages.Name = "zTabPageMessages";
            this.zTabPageMessages.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPageMessages.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1145, 282, true);
            this.zTabPageMessages.TabIndex = 3;
            this.zTabPageMessages.UseVisualStyleBackColor = true;
            // 
            // outturnAndGateInOutMessageUserControl
            // 
            this.outturnAndGateInOutMessageUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.outturnAndGateInOutMessageUserControl, ".");
            this.outturnAndGateInOutMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outturnAndGateInOutMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.outturnAndGateInOutMessageUserControl.Name = "outturnAndGateInOutMessageUserControl";
            this.outturnAndGateInOutMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1139, 277, true);
            this.outturnAndGateInOutMessageUserControl.TabIndex = 0;
            // 
            // OutturnAndGateInOutMainUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.zTabControl);
            this.Controls.Add(this.zGroupBoxHeader);
            this.Name = "OutturnAndGateInOutMainUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 677, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zGroupBoxHeader.ResumeLayout(false);
            this.zGroupBoxHeader.PerformLayout();
            this.zGroupBoxCargoReportingInstructions.ResumeLayout(false);
            this.zGroupBoxCargoReportingInstructions.PerformLayout();
            this.zCodeFindBoxOutturnProvider.ResumeLayout(true);
            this.zCodeFindBoxOutturnProvider.PerformLayout();
            this.zDropEditWithFixedWidthCustomsOffice.ResumeLayout(true);
            this.zDropEditWithFixedWidthCustomsOffice.PerformLayout();
            this.zDateEditUnpacked.ResumeLayout(true);
            this.zDateEditUnpacked.PerformLayout();
            this.DeConsolidatorAddressControl.ResumeLayout(true);
            this.DeConsolidatorAddressControl.PerformLayout();
            this.TerminalAddressControl.ResumeLayout(true);
            this.TerminalAddressControl.PerformLayout();
            this.zDropEditWithFixedWidthManifestType.ResumeLayout(true);
            this.zDropEditWithFixedWidthManifestType.PerformLayout();
            this.zDropGateInOutMessageType.ResumeLayout(true);
            this.zDropGateInOutMessageType.PerformLayout();
            this.zDropEditWithFixedWidthCustomsStatus.ResumeLayout(true);
            this.zDropEditWithFixedWidthCustomsStatus.PerformLayout();
            this.zDateEditGateInOutDate.ResumeLayout(true);
            this.zDateEditGateInOutDate.PerformLayout();
            this.GateInOutStatusDropEdit.ResumeLayout(true);
            this.GateInOutStatusDropEdit.PerformLayout();
            this.zDropEditWithFixedWidthExcessIndicator.ResumeLayout(true);
            this.zDropEditWithFixedWidthExcessIndicator.PerformLayout();
            this.zDateEditFullyLoadedUnloadedDate.ResumeLayout(true);
            this.zDateEditFullyLoadedUnloadedDate.PerformLayout();
            this.zGroupBoxTransportDocumentDetails.ResumeLayout(false);
            this.zGroupBoxTransportDocumentDetails.PerformLayout();
            this.zCodeFindBoxPortOfLoading.ResumeLayout(true);
            this.zCodeFindBoxPortOfLoading.PerformLayout();
            this.zDropEditWithFixedWidthTransportMode.ResumeLayout(true);
            this.zDropEditWithFixedWidthTransportMode.PerformLayout();
            this.zDropEditWithFixedWidthAgentType.ResumeLayout(true);
            this.zDropEditWithFixedWidthAgentType.PerformLayout();
            this.zCodeFindBoxVessel.ResumeLayout(true);
            this.zCodeFindBoxVessel.PerformLayout();
            this.zCodeFindBoxPortOfDischarge.ResumeLayout(true);
            this.zCodeFindBoxPortOfDischarge.PerformLayout();
            this.zDropEditWithFixedWidthNature.ResumeLayout(true);
            this.zDropEditWithFixedWidthNature.PerformLayout();
            this.zDropEditWithFixedWidthContainerMode.ResumeLayout(true);
            this.zDropEditWithFixedWidthContainerMode.PerformLayout();
            this.zDateEditDep.ResumeLayout(true);
            this.zDateEditDep.PerformLayout();
            this.zDateEditArv.ResumeLayout(true);
            this.zDateEditArv.PerformLayout();
            this.zDateEditIssueDate.ResumeLayout(true);
            this.zDateEditIssueDate.PerformLayout();
            this.zAddressControlCarrierZAddress.ResumeLayout(true);
            this.zAddressControlCarrierZAddress.PerformLayout();
            this.zTabControl.ResumeLayout(false);
            this.zTabControl.PerformLayout();
            this.zTabPageBill.ResumeLayout(false);
            this.zTabPageBill.PerformLayout();
            this.outturnAndGateInOutBillUserControl.ResumeLayout(true);
            this.outturnAndGateInOutBillUserControl.PerformLayout();
            this.zTabPageContainer.ResumeLayout(false);
            this.zTabPageContainer.PerformLayout();
            this.outturnAndGateInOutContainerUserControl.ResumeLayout(true);
            this.outturnAndGateInOutContainerUserControl.PerformLayout();
            this.zTabPagePack.ResumeLayout(false);
            this.zTabPagePack.PerformLayout();
            this.outturnAndGateInOutPackUserControl.ResumeLayout(true);
            this.outturnAndGateInOutPackUserControl.PerformLayout();
            this.zTabPageMessages.ResumeLayout(false);
            this.zTabPageMessages.PerformLayout();
            this.outturnAndGateInOutMessageUserControl.ResumeLayout(true);
            this.outturnAndGateInOutMessageUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		ZGroupBox zGroupBoxHeader;
		ZGroupBox zGroupBoxCargoReportingInstructions;
		ZGroupBox zGroupBoxTransportDocumentDetails;
		ZTabControl zTabControl;
		ZTabPage zTabPageBill;
		internal ZTabPage zTabPageContainer;
		ZTabPage zTabPagePack;
		OutturnAndGateInOutBillUserControl outturnAndGateInOutBillUserControl;
		OutturnAndGateInOutContainerUserControl outturnAndGateInOutContainerUserControl;
		OutturnAndGateInOutPackUserControl outturnAndGateInOutPackUserControl;
		ZArchitecture.ZTextBox zTextBoxParentBill;
		ZArchitecture.ZTextBox zTextBoxManifestNumber;
		ZCodeFindBox zCodeFindBoxOutturnProvider;
		ZCodeFindBox zCodeFindBoxPortOfDischarge;
		internal ZArchitecture.ZTextBox zTextBoxVoyage;
		internal ZDateEdit zDateEditFullyLoadedUnloadedDate;
		internal ZDropEditWithFixedWidth zDropEditWithFixedWidthExcessIndicator;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthNature;
		ZDateEdit zDateEditUnpacked;
		ZDateEdit zDateEditArv;
		ZDateEdit zDateEditDep;
		ZDateEdit zDateEditIssueDate;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthManifestType;
		internal ZDropEditWithFixedWidth zDropEditWithFixedWidthContainerMode;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthAgentType;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthCustomsOffice;
		ZAddressControl zAddressControlCarrierZAddress;
		internal ZCodeFindBox zCodeFindBoxVessel;
		ZTabPage zTabPageMessages;
		OutturnAndGateInOutMessageUserControl outturnAndGateInOutMessageUserControl;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthTransportMode;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthCustomsStatus;
		internal ZDateEdit zDateEditGateInOutDate;
		internal ZDropEditWithFixedWidth zDropGateInOutMessageType;
		private ZCodeFindBox zCodeFindBoxPortOfLoading;
		private ZAddressControl TerminalAddressControl;
		private ZAddressControl DeConsolidatorAddressControl;
		private ZDropEditWithFixedWidth GateInOutStatusDropEdit;
		private ZArchitecture.ZTextBox BookingNumberTextBox;
	}
}
