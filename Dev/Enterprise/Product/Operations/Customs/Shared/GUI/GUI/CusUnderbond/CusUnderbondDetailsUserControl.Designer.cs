namespace Enterprise.Customs.GUI
{
	partial class CusUnderbondDetailsUserControl
	{

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		internal void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CusUnderbondDetailsUserControl));
			this.UnderbondDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZDetailsTabPage();
			this.TranshipmentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TranshipmentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.EstablishmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DestinationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DischargeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DischargeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DestinationIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OriginIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DestinationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OriginLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OriginAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DischargeIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsMoveFromDischargeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PartShipmentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PiecesTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PiecesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FlightNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FlightNoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ArrivalDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BySeaTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ResponsiblePartyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VesselFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VoyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ResponsiblePartyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VoyageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VesselLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RequestReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnderbondForDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UnderbondForLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MovementModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SendersReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ModeOfMovementLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReferenceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OutturnTabPage = new Enterprise.ZArchitecture.GUI.ZDetailsTabPage();
			this.NilOutturnButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZDetailsTabPage();
			this.MessageUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnderbondDetailsPanel.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.TranshipmentCodeFindBox.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.EstablishmentsTabPage.SuspendLayout();
			this.DestinationAddressControl.SuspendLayout();
			this.DischargeAddressControl.SuspendLayout();
			this.OriginAddressControl.SuspendLayout();
			this.PartShipmentTabPage.SuspendLayout();
			this.ArrivalDateEdit.SuspendLayout();
			this.BySeaTabPage.SuspendLayout();
			this.VesselFindBox.SuspendLayout();
			this.RequestReasonDropEdit.SuspendLayout();
			this.UnderbondForDropEdit.SuspendLayout();
			this.MovementModeDropEdit.SuspendLayout();
			this.OutturnTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessageUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusUnderbond);
			// 
			// UnderbondDetailsPanel
			// 
			this.UnderbondDetailsPanel.Controls.Add(this.MainTabControl);
			this.UnderbondDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnderbondDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnderbondDetailsPanel.Name = "UnderbondDetailsPanel";
			this.UnderbondDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 171, true);
			this.UnderbondDetailsPanel.TabIndex = 4;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.DetailsTabPage);
			this.MainTabControl.Controls.Add(this.OutturnTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 171, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 171, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.AdditionalText = "Details";
			this.DetailsTabPage.Controls.Add(this.TranshipmentCodeFindBox);
			this.DetailsTabPage.Controls.Add(this.TranshipmentLabel);
			this.DetailsTabPage.Controls.Add(this.DetailsTabControl);
			this.DetailsTabPage.Controls.Add(this.CustomsStatusTextBox);
			this.DetailsTabPage.Controls.Add(this.CustomsStatusLabel);
			this.DetailsTabPage.Controls.Add(this.MessageStatusTextBox);
			this.DetailsTabPage.Controls.Add(this.MessageStatusLabel);
			this.DetailsTabPage.Controls.Add(this.RequestReasonDropEdit);
			this.DetailsTabPage.Controls.Add(this.RequestReasonLabel);
			this.DetailsTabPage.Controls.Add(this.UnderbondForDropEdit);
			this.DetailsTabPage.Controls.Add(this.UnderbondForLabel);
			this.DetailsTabPage.Controls.Add(this.MovementModeDropEdit);
			this.DetailsTabPage.Controls.Add(this.SendersReferenceTextBox);
			this.DetailsTabPage.Controls.Add(this.ModeOfMovementLabel);
			this.DetailsTabPage.Controls.Add(this.ReferenceLabel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.Text = "Underbond Details";
			// 
			// TranshipmentCodeFindBox
			// 
			this.TranshipmentCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TranshipmentCodeFindBox, "C4_RL_NKTranshipDestPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_RL_NKTranshipDestPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusUnderbond)(null)).Lookups.TranshipDestPorts)));
			this.TranshipmentCodeFindBox.BindToList = "Lookups+TranshipDestPorts";
			this.TranshipmentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 75, true);
			this.TranshipmentCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.TranshipmentCodeFindBox.Name = "TranshipmentCodeFindBox";
			this.TranshipmentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TranshipmentCodeFindBox.ParentType = null;
			this.TranshipmentCodeFindBox.ShowDescriptionBox = false;
			this.TranshipmentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.TranshipmentCodeFindBox.TabIndex = 4;
			// 
			// TranshipmentLabel
			// 
			this.TranshipmentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TranshipmentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 75, true);
			this.TranshipmentLabel.Name = "TranshipmentLabel";
			this.TranshipmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 16, true);
			this.TranshipmentLabel.TabIndex = 14;
			this.TranshipmentLabel.Text = "Transhipment Port:";
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.EstablishmentsTabPage);
			this.DetailsTabControl.Controls.Add(this.PartShipmentTabPage);
			this.DetailsTabControl.Controls.Add(this.BySeaTabPage);
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 5, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 136, true);
			this.DetailsTabControl.TabIndex = 12;
			// 
			// EstablishmentsTabPage
			// 
			this.EstablishmentsTabPage.Controls.Add(this.DestinationAddressControl);
			this.EstablishmentsTabPage.Controls.Add(this.DischargeAddressControl);
			this.EstablishmentsTabPage.Controls.Add(this.DischargeLabel);
			this.EstablishmentsTabPage.Controls.Add(this.DestinationIDTextBox);
			this.EstablishmentsTabPage.Controls.Add(this.OriginIDTextBox);
			this.EstablishmentsTabPage.Controls.Add(this.DestinationLabel);
			this.EstablishmentsTabPage.Controls.Add(this.OriginLabel);
			this.EstablishmentsTabPage.Controls.Add(this.OriginAddressControl);
			this.EstablishmentsTabPage.Controls.Add(this.DischargeIDTextBox);
			this.EstablishmentsTabPage.Controls.Add(this.IsMoveFromDischargeCheckBox);
			this.EstablishmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EstablishmentsTabPage.Name = "EstablishmentsTabPage";
			this.EstablishmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 109, true);
			this.EstablishmentsTabPage.TabIndex = 0;
			this.EstablishmentsTabPage.Text = "Establishments";
			// 
			// DestinationAddressControl
			// 
			this.DestinationAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationAddressControl, "C4_OA_DestinationAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_OA_DestinationAddress)));
			this.DestinationAddressControl.BindToOrgList = "Lookups+CustomsControlledPremisesList";
			this.DestinationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 32, true);
			this.DestinationAddressControl.Name = "DestinationAddressControl";
			this.DestinationAddressControl.PopupCaption = "";
			this.DestinationAddressControl.ShowAddress = false;
			this.DestinationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.DestinationAddressControl.TabIndex = 4;
			// 
			// DischargeAddressControl
			// 
			this.DischargeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DischargeAddressControl, "C4_OA_DischargeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_OA_DischargeAddress)));
			this.DischargeAddressControl.BindToOrgList = "Lookups+CustomsControlledPremisesList";
			this.DischargeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 56, true);
			this.DischargeAddressControl.Name = "DischargeAddressControl";
			this.DischargeAddressControl.PopupCaption = "";
			this.DischargeAddressControl.ShowAddress = false;
			this.DischargeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.DischargeAddressControl.TabIndex = 7;
			// 
			// DischargeLabel
			// 
			this.DischargeLabel.AutoSize = true;
			this.DischargeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 56, true);
			this.DischargeLabel.Name = "DischargeLabel";
			this.DischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.DischargeLabel.TabIndex = 6;
			this.DischargeLabel.Text = "Discharge:";
			// 
			// DestinationIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationIDTextBox, "C4_DestinationPremiseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_DestinationPremiseID)));
			this.DestinationIDTextBox.CaptionResourceString = null;
			this.DestinationIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 32, true);
			this.DestinationIDTextBox.Name = "DestinationIDTextBox";
			this.DestinationIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.DestinationIDTextBox.TabIndex = 5;
			// 
			// OriginIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.OriginIDTextBox, "C4_OriginPremiseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_OriginPremiseID)));
			this.OriginIDTextBox.CaptionResourceString = null;
			this.OriginIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 8, true);
			this.OriginIDTextBox.Name = "OriginIDTextBox";
			this.OriginIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OriginIDTextBox.TabIndex = 2;
			// 
			// DestinationLabel
			// 
			this.DestinationLabel.AutoSize = true;
			this.DestinationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DestinationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 32, true);
			this.DestinationLabel.Name = "DestinationLabel";
			this.DestinationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.DestinationLabel.TabIndex = 3;
			this.DestinationLabel.Text = "Destination:";
			// 
			// OriginLabel
			// 
			this.OriginLabel.AutoSize = true;
			this.OriginLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OriginLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.OriginLabel.Name = "OriginLabel";
			this.OriginLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 13, true);
			this.OriginLabel.TabIndex = 0;
			this.OriginLabel.Text = "Origin:";
			// 
			// OriginAddressControl
			// 
			this.OriginAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginAddressControl, "C4_OA_OriginAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_OA_OriginAddress)));
			this.OriginAddressControl.BindToOrgList = "Lookups+CustomsControlledPremisesList";
			this.OriginAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 8, true);
			this.OriginAddressControl.Name = "OriginAddressControl";
			this.OriginAddressControl.PopupCaption = "";
			this.OriginAddressControl.ShowAddress = false;
			this.OriginAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.OriginAddressControl.TabIndex = 1;
			// 
			// DischargeIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.DischargeIDTextBox, "C4_DischargePremiseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_DischargePremiseID)));
			this.DischargeIDTextBox.CaptionResourceString = null;
			this.DischargeIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 56, true);
			this.DischargeIDTextBox.Name = "DischargeIDTextBox";
			this.DischargeIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.DischargeIDTextBox.TabIndex = 8;
			// 
			// IsMoveFromDischargeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsMoveFromDischargeCheckBox, "C4_IsMoveFromDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_IsMoveFromDischarge)));
			this.IsMoveFromDischargeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 81, true);
			this.IsMoveFromDischargeCheckBox.Name = "IsMoveFromDischargeCheckBox";
			this.IsMoveFromDischargeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 16, true);
			this.IsMoveFromDischargeCheckBox.TabIndex = 9;
			this.IsMoveFromDischargeCheckBox.Text = "Is Move From Discharge?";
			// 
			// PartShipmentTabPage
			// 
			this.PartShipmentTabPage.Controls.Add(this.ArrivalDateEdit);
			this.PartShipmentTabPage.Controls.Add(this.PiecesTextBox);
			this.PartShipmentTabPage.Controls.Add(this.PiecesLabel);
			this.PartShipmentTabPage.Controls.Add(this.FlightNoTextBox);
			this.PartShipmentTabPage.Controls.Add(this.FlightNoLabel);
			this.PartShipmentTabPage.Controls.Add(this.ArrivalDateLabel);
			this.PartShipmentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PartShipmentTabPage.Name = "PartShipmentTabPage";
			this.PartShipmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 109, true);
			this.PartShipmentTabPage.TabIndex = 1;
			this.PartShipmentTabPage.Text = "Part Shipment";
			// 
			// ArrivalDateEdit
			// 
			this.ArrivalDateEdit.AllowDrop = true;
			this.ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ArrivalDateEdit, "C4_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_ArrivalDate)));
			this.ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 32, true);
			this.ArrivalDateEdit.Name = "ArrivalDateEdit";
			this.ArrivalDateEdit.TabIndex = 3;
			// 
			// PiecesTextBox
			// 
			this.BindingSource.SetBindingMember(this.PiecesTextBox, "C4_PiecesManifested");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_PiecesManifested)));
			this.PiecesTextBox.CaptionResourceString = null;
			this.PiecesTextBox.DecimalPlaces = 2;
			this.PiecesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 56, true);
			this.PiecesTextBox.Name = "PiecesTextBox";
			this.PiecesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.PiecesTextBox.TabIndex = 5;
			this.PiecesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PiecesLabel
			// 
			this.PiecesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PiecesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.PiecesLabel.Name = "PiecesLabel";
			this.PiecesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 16, true);
			this.PiecesLabel.TabIndex = 4;
			this.PiecesLabel.Text = "Pieces:";
			// 
			// FlightNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNoTextBox, "C4_FlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_FlightNo)));
			this.FlightNoTextBox.CaptionResourceString = null;
			this.FlightNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 8, true);
			this.FlightNoTextBox.Name = "FlightNoTextBox";
			this.FlightNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.FlightNoTextBox.TabIndex = 1;
			// 
			// FlightNoLabel
			// 
			this.FlightNoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FlightNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.FlightNoLabel.Name = "FlightNoLabel";
			this.FlightNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.FlightNoLabel.TabIndex = 0;
			this.FlightNoLabel.Text = "Flight No:";
			// 
			// ArrivalDateLabel
			// 
			this.ArrivalDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ArrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.ArrivalDateLabel.Name = "ArrivalDateLabel";
			this.ArrivalDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 16, true);
			this.ArrivalDateLabel.TabIndex = 2;
			this.ArrivalDateLabel.Text = "Arrival Date:";
			// 
			// BySeaTabPage
			// 
			this.BySeaTabPage.Controls.Add(this.ResponsiblePartyLabel);
			this.BySeaTabPage.Controls.Add(this.VesselFindBox);
			this.BySeaTabPage.Controls.Add(this.VoyageTextBox);
			this.BySeaTabPage.Controls.Add(this.ResponsiblePartyTextBox);
			this.BySeaTabPage.Controls.Add(this.VoyageLabel);
			this.BySeaTabPage.Controls.Add(this.VesselLabel);
			this.BySeaTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BySeaTabPage.Name = "BySeaTabPage";
			this.BySeaTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 109, true);
			this.BySeaTabPage.TabIndex = 2;
			this.BySeaTabPage.Text = "Underbond by Sea";
			// 
			// ResponsiblePartyLabel
			// 
			this.ResponsiblePartyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ResponsiblePartyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ResponsiblePartyLabel.Name = "ResponsiblePartyLabel";
			this.ResponsiblePartyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.ResponsiblePartyLabel.TabIndex = 0;
			this.ResponsiblePartyLabel.Text = "Responsible Party:";
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselFindBox, "C4_UnderbondBySeaVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_UnderbondBySeaVessel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusUnderbond)(null)).Lookups.UnderbondBySeaVessels)));
			this.VesselFindBox.BindToList = "Lookups+UnderbondBySeaVessels";
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.VesselFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
			this.VesselFindBox.Name = "VesselFindBox";
			this.VesselFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VesselFindBox.ParentType = null;
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.VesselFindBox.TabIndex = 3;
			// 
			// VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageTextBox, "C4_UnderbondBySeaVoyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_UnderbondBySeaVoyage)));
			this.VoyageTextBox.CaptionResourceString = null;
			this.VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 56, true);
			this.VoyageTextBox.Name = "VoyageTextBox";
			this.VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.VoyageTextBox.TabIndex = 5;
			// 
			// ResponsiblePartyTextBox
			// 
			this.BindingSource.SetBindingMember(this.ResponsiblePartyTextBox, "C4_ResponsiblePartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_ResponsiblePartyID)));
			this.ResponsiblePartyTextBox.CaptionResourceString = null;
			this.ResponsiblePartyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.ResponsiblePartyTextBox.Name = "ResponsiblePartyTextBox";
			this.ResponsiblePartyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ResponsiblePartyTextBox.TabIndex = 1;
			// 
			// VoyageLabel
			// 
			this.VoyageLabel.AutoSize = true;
			this.VoyageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VoyageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.VoyageLabel.Name = "VoyageLabel";
			this.VoyageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 13, true);
			this.VoyageLabel.TabIndex = 4;
			this.VoyageLabel.Text = "Voyage:";
			// 
			// VesselLabel
			// 
			this.VesselLabel.AutoSize = true;
			this.VesselLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VesselLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.VesselLabel.Name = "VesselLabel";
			this.VesselLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.VesselLabel.TabIndex = 2;
			this.VesselLabel.Text = "Vessel:";
			// 
			// CustomsStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsStatusTextBox, "ApprovalStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).ApprovalStatus)));
			this.CustomsStatusTextBox.CaptionResourceString = null;
			this.CustomsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 121, true);
			this.CustomsStatusTextBox.Name = "CustomsStatusTextBox";
			this.CustomsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.CustomsStatusTextBox.TabIndex = 6;
			// 
			// CustomsStatusLabel
			// 
			this.CustomsStatusLabel.AutoSize = true;
			this.CustomsStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomsStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 121, true);
			this.CustomsStatusLabel.Name = "CustomsStatusLabel";
			this.CustomsStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 13, true);
			this.CustomsStatusLabel.TabIndex = 10;
			this.CustomsStatusLabel.Text = "Customs Status:";
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "UnderbondStatus+Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).UnderbondStatus.Description)));
			this.MessageStatusTextBox.CaptionResourceString = null;
			this.MessageStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 98, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.MessageStatusTextBox.TabIndex = 5;
			// 
			// MessageStatusLabel
			// 
			this.MessageStatusLabel.AutoSize = true;
			this.MessageStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 98, true);
			this.MessageStatusLabel.Name = "MessageStatusLabel";
			this.MessageStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.MessageStatusLabel.TabIndex = 8;
			this.MessageStatusLabel.Text = "Message Status:";
			// 
			// RequestReasonDropEdit
			// 
			this.RequestReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestReasonDropEdit, "C4_MovementReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_MovementReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusUnderbond)(null)).Lookups.RequestReasonList)));
			this.RequestReasonDropEdit.BindToList = "Lookups+RequestReasonList";
			this.RequestReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 29, true);
			this.RequestReasonDropEdit.Name = "RequestReasonDropEdit";
			this.RequestReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.RequestReasonDropEdit.TabIndex = 1;
			// 
			// RequestReasonLabel
			// 
			this.RequestReasonLabel.AutoSize = true;
			this.RequestReasonLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RequestReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 29, true);
			this.RequestReasonLabel.Name = "RequestReasonLabel";
			this.RequestReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.RequestReasonLabel.TabIndex = 2;
			this.RequestReasonLabel.Text = "Request Reason:";
			// 
			// UnderbondForDropEdit
			// 
			this.UnderbondForDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnderbondForDropEdit, "LinkedObjectStringRepresentation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusUnderbond)(null)).LinkedObjectStringRepresentation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusUnderbond)(null)).Lookups.UnderbondForList)));
			this.UnderbondForDropEdit.BindToList = "Lookups+UnderbondForList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UnderbondForDropEdit, false);
			this.UnderbondForDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 6, true);
			this.UnderbondForDropEdit.Name = "UnderbondForDropEdit";
			this.UnderbondForDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.UnderbondForDropEdit.TabIndex = 0;
			// 
			// UnderbondForLabel
			// 
			this.UnderbondForLabel.AutoSize = true;
			this.UnderbondForLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnderbondForLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.UnderbondForLabel.Name = "UnderbondForLabel";
			this.UnderbondForLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 13, true);
			this.UnderbondForLabel.TabIndex = 0;
			this.UnderbondForLabel.Text = "Underbond For:";
			// 
			// MovementModeDropEdit
			// 
			this.MovementModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MovementModeDropEdit, "C4_ModeOfMovement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_ModeOfMovement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusUnderbond)(null)).Lookups.ModeOfTransportList)));
			this.MovementModeDropEdit.BindToList = "Lookups+ModeOfTransportList";
			this.MovementModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 52, true);
			this.MovementModeDropEdit.Name = "MovementModeDropEdit";
			this.MovementModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.MovementModeDropEdit.TabIndex = 2;
			// 
			// SendersReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.SendersReferenceTextBox, "C4_SendersMessageReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusUnderbond)(null)).C4_SendersMessageReference)));
			this.SendersReferenceTextBox.CaptionResourceString = null;
			this.SendersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 75, true);
			this.SendersReferenceTextBox.Name = "SendersReferenceTextBox";
			this.SendersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.SendersReferenceTextBox.TabIndex = 3;
			// 
			// ModeOfMovementLabel
			// 
			this.ModeOfMovementLabel.AutoSize = true;
			this.ModeOfMovementLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ModeOfMovementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 52, true);
			this.ModeOfMovementLabel.Name = "ModeOfMovementLabel";
			this.ModeOfMovementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 13, true);
			this.ModeOfMovementLabel.TabIndex = 4;
			this.ModeOfMovementLabel.Text = "Mode:";
			// 
			// ReferenceLabel
			// 
			this.ReferenceLabel.AutoSize = true;
			this.ReferenceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ReferenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 75, true);
			this.ReferenceLabel.Name = "ReferenceLabel";
			this.ReferenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.ReferenceLabel.TabIndex = 6;
			this.ReferenceLabel.Text = "Reference:";
			// 
			// OutturnTabPage
			// 
			this.OutturnTabPage.AdditionalText = "Details";
			this.OutturnTabPage.Controls.Add(this.NilOutturnButton);
			this.OutturnTabPage.Controls.Add(this.CoveringLabel);
			this.OutturnTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OutturnTabPage.Name = "OutturnTabPage";
			this.OutturnTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.OutturnTabPage.TabIndex = 1;
			this.OutturnTabPage.Text = "Outturn";
			// 
			// NilOutturnButton
			// 
			this.NilOutturnButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.NilOutturnButton.IsCaptionOverridden = true;
			this.NilOutturnButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 3, true);
			this.NilOutturnButton.Name = "NilOutturnButton";
			this.NilOutturnButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 21, true);
			this.NilOutturnButton.TabIndex = 11;
			this.NilOutturnButton.Text = "Nil Outturn";
			this.NilOutturnButton.ToolTipCaption = null;
			this.NilOutturnButton.Visible = false;
			this.NilOutturnButton.Click += new System.EventHandler(this.NilOutturnButton_Click);
			// 
			// coveringLabel
			// 
			this.CoveringLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CoveringLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CoveringLabel.IsFontBold = true;
			this.CoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CoveringLabel.Name = "coveringLabel";
			this.CoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.CoveringLabel.TabIndex = 12;
			this.CoveringLabel.Text = resources.GetString("coveringLabel.Text");
			this.CoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.AdditionalText = "Details";
			this.MessagesTabPage.Controls.Add(this.MessageUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.MessagesTabPage.TabIndex = 2;
			this.MessagesTabPage.Text = "Underbond Messages";
			// 
			// MessageUserControl
			// 
			this.MessageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.Business.CusUnderbond)(null)))));
			this.MessageUserControl.BindPrepend = "";
			this.MessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageUserControl.Name = "MessageUserControl";
			this.MessageUserControl.ShowChangingBlueMessageHeading = false;
			this.MessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.MessageUserControl.TabIndex = 1;
			// 
			// CusUnderbondDetailsUserControl
			// 
			this.Controls.Add(this.UnderbondDetailsPanel);
			this.Name = "CusUnderbondDetailsUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 171, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnderbondDetailsPanel.ResumeLayout(false);
			this.UnderbondDetailsPanel.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.TranshipmentCodeFindBox.ResumeLayout(true);
			this.TranshipmentCodeFindBox.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.EstablishmentsTabPage.ResumeLayout(false);
			this.EstablishmentsTabPage.PerformLayout();
			this.DestinationAddressControl.ResumeLayout(true);
			this.DestinationAddressControl.PerformLayout();
			this.DischargeAddressControl.ResumeLayout(true);
			this.DischargeAddressControl.PerformLayout();
			this.OriginAddressControl.ResumeLayout(true);
			this.OriginAddressControl.PerformLayout();
			this.PartShipmentTabPage.ResumeLayout(false);
			this.PartShipmentTabPage.PerformLayout();
			this.ArrivalDateEdit.ResumeLayout(true);
			this.ArrivalDateEdit.PerformLayout();
			this.BySeaTabPage.ResumeLayout(false);
			this.BySeaTabPage.PerformLayout();
			this.VesselFindBox.ResumeLayout(true);
			this.VesselFindBox.PerformLayout();
			this.RequestReasonDropEdit.ResumeLayout(true);
			this.RequestReasonDropEdit.PerformLayout();
			this.UnderbondForDropEdit.ResumeLayout(true);
			this.UnderbondForDropEdit.PerformLayout();
			this.MovementModeDropEdit.ResumeLayout(true);
			this.MovementModeDropEdit.PerformLayout();
			this.OutturnTabPage.ResumeLayout(false);
			this.OutturnTabPage.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessageUserControl.ResumeLayout(true);
			this.MessageUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		protected internal Enterprise.ZArchitecture.ZLabel CoveringLabel;
		internal Enterprise.ZArchitecture.ZTextBox MessageStatusTextBox;
		protected internal Enterprise.ZArchitecture.ZLabel MessageStatusLabel;
		protected internal Enterprise.ZArchitecture.ZLabel CustomsStatusLabel;
		internal Enterprise.ZArchitecture.ZTextBox CustomsStatusTextBox;
		internal Enterprise.ZArchitecture.GUI.ZTabPage EstablishmentsTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage PartShipmentTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage BySeaTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl DetailsTabControl;
		internal Enterprise.ZArchitecture.ZLabel TranshipmentLabel;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TranshipmentCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox ResponsiblePartyTextBox;
		private Enterprise.ZArchitecture.ZLabel ResponsiblePartyLabel;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl DischargeAddressControl;
		internal Enterprise.ZArchitecture.ZTextBox DischargeIDTextBox;
		internal Enterprise.ZArchitecture.ZLabel DischargeLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel UnderbondDetailsPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.ZLabel FlightNoLabel;
		private Enterprise.ZArchitecture.ZTextBox FlightNoTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ArrivalDateEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PiecesTextBox;
		private Enterprise.ZArchitecture.ZLabel PiecesLabel;
		private Enterprise.ZArchitecture.ZLabel ArrivalDateLabel;
		internal Enterprise.ZArchitecture.GUI.ZDetailsTabPage DetailsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit RequestReasonDropEdit;
		protected internal Enterprise.ZArchitecture.ZLabel RequestReasonLabel;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit UnderbondForDropEdit;
		protected internal Enterprise.ZArchitecture.ZLabel UnderbondForLabel;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MovementModeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselFindBox;
		internal Enterprise.ZArchitecture.ZTextBox VoyageTextBox;
		internal Enterprise.ZArchitecture.ZTextBox SendersReferenceTextBox;
		private Enterprise.ZArchitecture.ZLabel VoyageLabel;
		private Enterprise.ZArchitecture.ZLabel VesselLabel;
		protected internal Enterprise.ZArchitecture.ZLabel ModeOfMovementLabel;
		protected Enterprise.ZArchitecture.ZLabel ReferenceLabel;
		internal Enterprise.ZArchitecture.ZTextBox DestinationIDTextBox;
		internal Enterprise.ZArchitecture.ZTextBox OriginIDTextBox;
		private Enterprise.ZArchitecture.ZLabel DestinationLabel;
		private Enterprise.ZArchitecture.ZLabel OriginLabel;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl OriginAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl DestinationAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox IsMoveFromDischargeCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZDetailsTabPage OutturnTabPage;
		protected internal Enterprise.Customs.GUI.CusOutturnUserControl OutturnUserControl;
		protected internal Enterprise.ZArchitecture.GUI.ZDetailsTabPage MessagesTabPage;
		internal Enterprise.Messaging.GUI.EDIMessageUserControl MessageUserControl;
		public Enterprise.ZArchitecture.GUI.ZButton NilOutturnButton;
		private System.ComponentModel.IContainer components;
	}
}
