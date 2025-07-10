namespace Enterprise.Freight.Agency.GUI
{
	public partial class BillOfLadingMainPage
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.JS_PackingModeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JS_RL_NKDestinationBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JS_RL_NKOriginBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JS_HBLAWBChargesDisplayBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JS_NoCopyBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JS_NoOriginalBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JS_HouseBillIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_ShippedOnBoardDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JS_ReleaseTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JS_ShippedOnBoardDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.NotifyPartyDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.JS_BookingReferenceBoundZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_CFSReferenceBoundZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_INCOBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsEntryNumberTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsEntryNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_HouseBillBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_GoodsDescriptionTextBoxBoundTextBox = new Enterprise.Freight.GUI.GoodsDescriptionTextBox();
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText = new Enterprise.Freight.GUI.ZStmNotePopupEditWithBindableText();
			this.ConsignorDocumentaryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsigneeDocumentaryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.JS_E_ARVBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_E_DEPBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_ShipmentStatusBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JS_ActualVolumeBoundZCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JS_OuterPacksBoundZCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JS_ActualWeightBoundCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SailingControl = new Enterprise.Freight.Agency.GUI.SailingUserControl();
			this.ServiceLevel = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JS_RL_NKHouseBillIssuePlaceFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BillOfLading);
			// 
			// JS_PackingModeBoundDropEdit
			// 
			this.JS_PackingModeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_PackingModeBoundDropEdit, "JS_PackingMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_PackingMode)));
			this.JS_PackingModeBoundDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|ca684e5b-e319-4ff3-af52-08f628a5198a", "Cargo Type", "The type of cargo to be transported.");
			this.JS_PackingModeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 8, true);
			this.JS_PackingModeBoundDropEdit.Name = "JS_PackingModeBoundDropEdit";
			this.JS_PackingModeBoundDropEdit.PreBoundMaxLength = 3;
			this.JS_PackingModeBoundDropEdit.ShowDescriptionBox = false;
			this.JS_PackingModeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JS_PackingModeBoundDropEdit.TabIndex = 1;
			// 
			// JS_RL_NKDestinationBoundCodeFindBox
			// 
			this.JS_RL_NKDestinationBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKDestinationBoundCodeFindBox, "JS_RL_NKDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_RL_NKDestination)));
			this.JS_RL_NKDestinationBoundCodeFindBox.CaptionResourceString = null;
			this.JS_RL_NKDestinationBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 226, true);
			this.JS_RL_NKDestinationBoundCodeFindBox.Name = "JS_RL_NKDestinationBoundCodeFindBox";
			this.JS_RL_NKDestinationBoundCodeFindBox.PopupCaption = "Select Destination";
			this.JS_RL_NKDestinationBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.JS_RL_NKDestinationBoundCodeFindBox.TabIndex = 14;
			// 
			// JS_RL_NKOriginBoundCodeFindBox
			// 
			this.JS_RL_NKOriginBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKOriginBoundCodeFindBox, "JS_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_RL_NKOrigin)));
			this.JS_RL_NKOriginBoundCodeFindBox.CaptionResourceString = null;
			this.JS_RL_NKOriginBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 202, true);
			this.JS_RL_NKOriginBoundCodeFindBox.Name = "JS_RL_NKOriginBoundCodeFindBox";
			this.JS_RL_NKOriginBoundCodeFindBox.PopupCaption = "Select Origin";
			this.JS_RL_NKOriginBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.JS_RL_NKOriginBoundCodeFindBox.TabIndex = 12;
			// 
			// JS_HBLAWBChargesDisplayBoundDropEdit
			// 
			this.JS_HBLAWBChargesDisplayBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_HBLAWBChargesDisplayBoundDropEdit, "JS_HBLAWBChargesDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_HBLAWBChargesDisplay)));
			this.JS_HBLAWBChargesDisplayBoundDropEdit.CaptionResourceString = null;
			this.JS_HBLAWBChargesDisplayBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 378, true);
			this.JS_HBLAWBChargesDisplayBoundDropEdit.Name = "JS_HBLAWBChargesDisplayBoundDropEdit";
			this.JS_HBLAWBChargesDisplayBoundDropEdit.PreBoundMaxLength = 3;
			this.JS_HBLAWBChargesDisplayBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.JS_HBLAWBChargesDisplayBoundDropEdit.TabIndex = 26;
			// 
			// JS_NoCopyBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JS_NoCopyBillsCalcEdit, "JS_NoCopyBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_NoCopyBills)));
			this.JS_NoCopyBillsCalcEdit.CaptionResourceString = null;
			this.JS_NoCopyBillsCalcEdit.DecimalPlaces = 2;
			this.JS_NoCopyBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 404, true);
			this.JS_NoCopyBillsCalcEdit.Name = "JS_NoCopyBillsCalcEdit";
			this.JS_NoCopyBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.JS_NoCopyBillsCalcEdit.TabIndex = 49;
			this.JS_NoCopyBillsCalcEdit.Text = "0";
			this.JS_NoCopyBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JS_NoOriginalBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JS_NoOriginalBillsCalcEdit, "JS_NoOriginalBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_NoOriginalBills)));
			this.JS_NoOriginalBillsCalcEdit.CaptionResourceString = null;
			this.JS_NoOriginalBillsCalcEdit.DecimalPlaces = 2;
			this.JS_NoOriginalBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 404, true);
			this.JS_NoOriginalBillsCalcEdit.Name = "JS_NoOriginalBillsCalcEdit";
			this.JS_NoOriginalBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.JS_NoOriginalBillsCalcEdit.TabIndex = 47;
			this.JS_NoOriginalBillsCalcEdit.Text = "0";
			this.JS_NoOriginalBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JS_HouseBillIssueDateEdit
			// 
			this.JS_HouseBillIssueDateEdit.AllowDrop = true;
			this.JS_HouseBillIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_HouseBillIssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_HouseBillIssueDateEdit, "JS_HouseBillIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_HouseBillIssueDate)));
			this.JS_HouseBillIssueDateEdit.CaptionResourceString = null;
			this.JS_HouseBillIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 356, true);
			this.JS_HouseBillIssueDateEdit.Name = "JS_HouseBillIssueDateEdit";
			this.JS_HouseBillIssueDateEdit.TabIndex = 42;
			// 
			// JS_RL_NKHouseBillIssuePlaceFindBox
			// 
			this.JS_RL_NKHouseBillIssuePlaceFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKHouseBillIssuePlaceFindBox, "JS_RL_NKHouseBillIssuePlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_RL_NKHouseBillIssuePlace)));
			this.JS_RL_NKHouseBillIssuePlaceFindBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|3e9f6a47-141e-4693-8db1-c40c2aec43dc", "Place of Issue");
			this.JS_RL_NKHouseBillIssuePlaceFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(615, 356, true);
			this.JS_RL_NKHouseBillIssuePlaceFindBox.Name = "JS_RL_NKHouseBillIssuePlaceFindBox";
			this.JS_RL_NKHouseBillIssuePlaceFindBox.ShowDescriptionBox = false;
			this.JS_RL_NKHouseBillIssuePlaceFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JS_RL_NKHouseBillIssuePlaceFindBox.TabIndex = 43;
			// 
			// JS_ShippedOnBoardDropEdit
			// 
			this.JS_ShippedOnBoardDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_ShippedOnBoardDropEdit, "JS_ShippedOnBoard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_ShippedOnBoard)));
			this.JS_ShippedOnBoardDropEdit.CaptionResourceString = null;
			this.JS_ShippedOnBoardDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 380, true);
			this.JS_ShippedOnBoardDropEdit.Name = "JS_ShippedOnBoardDropEdit";
			this.JS_ShippedOnBoardDropEdit.PreBoundMaxLength = 3;
			this.JS_ShippedOnBoardDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.JS_ShippedOnBoardDropEdit.TabIndex = 45;
			// 
			// JS_ReleaseTypeBoundDropEdit
			// 
			this.JS_ReleaseTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_ReleaseTypeBoundDropEdit, "JS_ReleaseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_ReleaseType)));
			this.JS_ReleaseTypeBoundDropEdit.CaptionResourceString = null;
			this.JS_ReleaseTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 402, true);
			this.JS_ReleaseTypeBoundDropEdit.Name = "JS_ReleaseTypeBoundDropEdit";
			this.JS_ReleaseTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.JS_ReleaseTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.JS_ReleaseTypeBoundDropEdit.TabIndex = 28;
			// 
			// JS_ShippedOnBoardDateEdit
			// 
			this.JS_ShippedOnBoardDateEdit.AllowDrop = true;
			this.JS_ShippedOnBoardDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_ShippedOnBoardDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_ShippedOnBoardDateEdit, "JS_ShippedOnBoardDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_ShippedOnBoardDate)));
			this.JS_ShippedOnBoardDateEdit.CaptionResourceString = null;
			this.JS_ShippedOnBoardDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 380, true);
			this.JS_ShippedOnBoardDateEdit.Name = "JS_ShippedOnBoardDateEdit";
			this.JS_ShippedOnBoardDateEdit.TabIndex = 44;
			// 
			// NotifyPartyDocAddressControl
			// 
			this.NotifyPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyDocAddressControl, "NotifyPartyDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).NotifyPartyDocumentaryAddress)));
			this.NotifyPartyDocAddressControl.BindToOrganisations = "Lookups.NotifyParty_List";
			this.NotifyPartyDocAddressControl.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|030bd527-c1da-457e-92cb-69570707e054", "Notify Party");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NotifyPartyDocAddressControl, false);
			this.NotifyPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 392, true);
			this.NotifyPartyDocAddressControl.Name = "NotifyPartyDocAddressControl";
			this.NotifyPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.NotifyPartyDocAddressControl.TabIndex = 53;
			// 
			// JS_BookingReferenceBoundZTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_BookingReferenceBoundZTextBox, "JS_BookingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_BookingReference)));
			this.JS_BookingReferenceBoundZTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|3d18f92d-0893-437d-895d-244f3c8ba8bf", "Shippers Ref", "The shippers reference.");
			this.JS_BookingReferenceBoundZTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JS_BookingReferenceBoundZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 282, true);
			this.JS_BookingReferenceBoundZTextBox.Name = "JS_BookingReferenceBoundZTextBox";
			this.JS_BookingReferenceBoundZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.JS_BookingReferenceBoundZTextBox.TabIndex = 18;
			// 
			// JS_CFSReferenceBoundZTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_CFSReferenceBoundZTextBox, "JS_CFSReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_CFSReference)));
			this.JS_CFSReferenceBoundZTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|487c5a18-d4d6-473d-9a6e-5945bf2ebc94", "Booking Ref", "The booking reference or release number.");
			this.JS_CFSReferenceBoundZTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JS_CFSReferenceBoundZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 258, true);
			this.JS_CFSReferenceBoundZTextBox.Name = "JS_CFSReferenceBoundZTextBox";
			this.JS_CFSReferenceBoundZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.JS_CFSReferenceBoundZTextBox.TabIndex = 16;
			// 
			// JS_INCOBoundDropEdit
			// 
			this.JS_INCOBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_INCOBoundDropEdit, "JS_INCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_INCO)));
			this.JS_INCOBoundDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|df3e530c-ab8a-43fe-85eb-c70228a22e3e", "Payment Term", "Specifies if the FREIGHT is prepaid or collect.");
			this.JS_INCOBoundDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JS_INCOBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 354, true);
			this.JS_INCOBoundDropEdit.Name = "JS_INCOBoundDropEdit";
			this.JS_INCOBoundDropEdit.PreBoundMaxLength = 3;
			this.JS_INCOBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.JS_INCOBoundDropEdit.TabIndex = 24;
			// 
			// CustomsEntryNumberTypeBoundDropEdit
			// 
			this.CustomsEntryNumberTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberTypeBoundDropEdit, "CustomsEntryNumberType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).CustomsEntryNumberType)));
			this.CustomsEntryNumberTypeBoundDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|8b439804-9a65-472a-83ba-eabc9dd0940d", "Customs Entry Type");
			this.CustomsEntryNumberTypeBoundDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsEntryNumberTypeBoundDropEdit, false);
			this.CustomsEntryNumberTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 256, true);
			this.CustomsEntryNumberTypeBoundDropEdit.Name = "CustomsEntryNumberTypeBoundDropEdit";
			this.CustomsEntryNumberTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.CustomsEntryNumberTypeBoundDropEdit.ShowDescriptionBox = false;
			this.CustomsEntryNumberTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CustomsEntryNumberTypeBoundDropEdit.TabIndex = 33;
			// 
			// CustomsEntryNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberBoundTextBox, "CustomsEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).CustomsEntryNumber)));
			this.CustomsEntryNumberBoundTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|774e7f49-069a-453f-85bb-ec83eb7cce19", "Customs Entry Number");
			this.CustomsEntryNumberBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsEntryNumberBoundTextBox, false);
			this.CustomsEntryNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 256, true);
			this.CustomsEntryNumberBoundTextBox.Name = "CustomsEntryNumberBoundTextBox";
			this.CustomsEntryNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CustomsEntryNumberBoundTextBox.TabIndex = 34;
			// 
			// JS_HouseBillBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JS_HouseBillBoundTextBox, "JS_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_HouseBill)));
			this.JS_HouseBillBoundTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|198dfc5b-767b-4416-bfd5-acc2f65deb5a", "OBL", "Bill Of Lading", "The ocean bill number is from an internal range of numbers that your organization uses to identify your shipments.");
			this.JS_HouseBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 8, true);
			this.JS_HouseBillBoundTextBox.Name = "JS_HouseBillBoundTextBox";
			this.JS_HouseBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.JS_HouseBillBoundTextBox.TabIndex = 5;
			// 
			// JS_GoodsDescriptionTextBoxBoundTextBox
			// 
			this.JS_GoodsDescriptionTextBoxBoundTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_GoodsDescriptionTextBoxBoundTextBox, "JS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_GoodsDescription)));
			this.JS_GoodsDescriptionTextBoxBoundTextBox.ButtonText = "Detail";
			this.JS_GoodsDescriptionTextBoxBoundTextBox.CaptionResourceString = null;
			this.JS_GoodsDescriptionTextBoxBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 306, true);
			this.JS_GoodsDescriptionTextBoxBoundTextBox.Name = "JS_GoodsDescriptionTextBoxBoundTextBox";
			this.JS_GoodsDescriptionTextBoxBoundTextBox.NoteTypeDescription = "Detailed Goods Description";
			this.JS_GoodsDescriptionTextBoxBoundTextBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 1, true);
			this.JS_GoodsDescriptionTextBoxBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 23, true);
			this.JS_GoodsDescriptionTextBoxBoundTextBox.TabIndex = 20;
			// 
			// JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText
			// 
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText, "JS_MarksAndNumbersShort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_MarksAndNumbersShort)));
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.ButtonText = "Detail";
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|8d269c22-585e-4600-8e71-bb576e906b25", "Marks and Numbers");
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 330, true);
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.Name = "JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText";
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.NoteTypeDescription = "Marks & Numbers";
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 1, true);
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 23, true);
			this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText.TabIndex = 22;
			// 
			// ConsignorDocumentaryDocAddressControl
			// 
			this.ConsignorDocumentaryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorDocumentaryDocAddressControl, "ConsignorDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).ConsignorDocumentaryAddress)));
			this.ConsignorDocumentaryDocAddressControl.BindToContacts = "Lookups.ConsignorContacts_List";
			this.ConsignorDocumentaryDocAddressControl.BindToOrganisations = "Lookups.Consignor_List";
			this.ConsignorDocumentaryDocAddressControl.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|6b031135-d8fa-4748-8040-17c476c3c147", "Consignor");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsignorDocumentaryDocAddressControl, false);
			this.ConsignorDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 8, true);
			this.ConsignorDocumentaryDocAddressControl.Name = "ConsignorDocumentaryDocAddressControl";
			this.ConsignorDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsignorDocumentaryDocAddressControl.TabIndex = 51;
			this.ConsignorDocumentaryDocAddressControl.Enter += new System.EventHandler(this.ConsignorDocumentaryDocAddressControl_Enter);
			// 
			// ConsigneeDocumentaryDocAddressControl
			// 
			this.ConsigneeDocumentaryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocumentaryDocAddressControl, "ConsigneeDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).ConsigneeDocumentaryAddress)));
			this.ConsigneeDocumentaryDocAddressControl.BindToContacts = "Lookups.ConsigneeContacts_List";
			this.ConsigneeDocumentaryDocAddressControl.BindToOrganisations = "Lookups.Consignee_List";
			this.ConsigneeDocumentaryDocAddressControl.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|b5fa8851-e516-4a46-827b-6111089c6465", "Consignee");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeDocumentaryDocAddressControl, false);
			this.ConsigneeDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 200, true);
			this.ConsigneeDocumentaryDocAddressControl.Name = "ConsigneeDocumentaryDocAddressControl";
			this.ConsigneeDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsigneeDocumentaryDocAddressControl.TabIndex = 52;
			this.ConsigneeDocumentaryDocAddressControl.Enter += new System.EventHandler(this.ConsigneeDocumentaryDocAddressControl_Enter);
			// 
			// JS_E_ARVBoundDateEdit
			// 
			this.JS_E_ARVBoundDateEdit.AllowDrop = true;
			this.JS_E_ARVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_E_ARVBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_E_ARVBoundDateEdit, "JS_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_E_ARV)));
			this.JS_E_ARVBoundDateEdit.CaptionResourceString = null;
			this.JS_E_ARVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 226, true);
			this.JS_E_ARVBoundDateEdit.Name = "JS_E_ARVBoundDateEdit";
			this.JS_E_ARVBoundDateEdit.TabIndex = 32;
			// 
			// JS_E_DEPBoundDateEdit
			// 
			this.JS_E_DEPBoundDateEdit.AllowDrop = true;
			this.JS_E_DEPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_E_DEPBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_E_DEPBoundDateEdit, "JS_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_E_DEP)));
			this.JS_E_DEPBoundDateEdit.CaptionResourceString = null;
			this.JS_E_DEPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 202, true);
			this.JS_E_DEPBoundDateEdit.Name = "JS_E_DEPBoundDateEdit";
			this.JS_E_DEPBoundDateEdit.TabIndex = 30;
			// 
			// JS_ShipmentStatusBoundDropEdit
			// 
			this.JS_ShipmentStatusBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_ShipmentStatusBoundDropEdit, "JS_ShipmentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_ShipmentStatus)));
			this.JS_ShipmentStatusBoundDropEdit.CaptionResourceString = null;
			this.JS_ShipmentStatusBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 8, true);
			this.JS_ShipmentStatusBoundDropEdit.Name = "JS_ShipmentStatusBoundDropEdit";
			this.JS_ShipmentStatusBoundDropEdit.PreBoundMaxLength = 3;
			this.JS_ShipmentStatusBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.JS_ShipmentStatusBoundDropEdit.TabIndex = 3;
			// 
			// JS_ActualVolumeBoundZCalcDropEdit
			// 
			this.JS_ActualVolumeBoundZCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_ActualVolumeBoundZCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_UnitOfVolume)));
			this.JS_ActualVolumeBoundZCalcDropEdit.BindToAmount = "JS_ActualVolume";
			this.JS_ActualVolumeBoundZCalcDropEdit.BindToUnit = "JS_UnitOfVolume";
			this.JS_ActualVolumeBoundZCalcDropEdit.CaptionResourceString = null;
			this.JS_ActualVolumeBoundZCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 332, true);
			this.JS_ActualVolumeBoundZCalcDropEdit.Name = "JS_ActualVolumeBoundZCalcDropEdit";
			this.JS_ActualVolumeBoundZCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JS_ActualVolumeBoundZCalcDropEdit.TabIndex = 40;
			this.JS_ActualVolumeBoundZCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JS_OuterPacksBoundZCalcDropEdit
			// 
			this.JS_OuterPacksBoundZCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_OuterPacksBoundZCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_F3_NKPackType)));
			this.JS_OuterPacksBoundZCalcDropEdit.BindToAmount = "JS_OuterPacks";
			this.JS_OuterPacksBoundZCalcDropEdit.BindToUnit = "JS_F3_NKPackType";
			this.JS_OuterPacksBoundZCalcDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingMainPage|4108898f-d473-46f4-b167-1ae4301c3513", "Packs");
			this.JS_OuterPacksBoundZCalcDropEdit.Decimals = 2;
			this.JS_OuterPacksBoundZCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 284, true);
			this.JS_OuterPacksBoundZCalcDropEdit.Name = "JS_OuterPacksBoundZCalcDropEdit";
			this.JS_OuterPacksBoundZCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JS_OuterPacksBoundZCalcDropEdit.TabIndex = 36;
			this.JS_OuterPacksBoundZCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JS_ActualWeightBoundCalcDropEdit
			// 
			this.JS_ActualWeightBoundCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_ActualWeightBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_UnitOfWeight)));
			this.JS_ActualWeightBoundCalcDropEdit.BindToAmount = "JS_ActualWeight";
			this.JS_ActualWeightBoundCalcDropEdit.BindToUnit = "JS_UnitOfWeight";
			this.JS_ActualWeightBoundCalcDropEdit.CaptionResourceString = null;
			this.JS_ActualWeightBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 308, true);
			this.JS_ActualWeightBoundCalcDropEdit.Name = "JS_ActualWeightBoundCalcDropEdit";
			this.JS_ActualWeightBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JS_ActualWeightBoundCalcDropEdit.TabIndex = 38;
			this.JS_ActualWeightBoundCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// SailingControl
			// 
			this.SailingControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SailingControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyShipment)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)))));
			this.SailingControl.CaptionResourceString = null;
			this.SailingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 34, true);
			this.SailingControl.Name = "SailingControl";
			this.SailingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 156, true);
			this.SailingControl.TabIndex = 10;
			// 
			// ServiceLevel
			// 
			this.ServiceLevel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevel, "JS_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_RS_NKServiceLevel)));
			this.ServiceLevel.CaptionResourceString = null;
			this.ServiceLevel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 425, true);
			this.ServiceLevel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ServiceLevel.Name = "ServiceLevel";
			this.ServiceLevel.PopupCaption = null;
			this.ServiceLevel.PreBoundMaxLength = 3;
			this.ServiceLevel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.ServiceLevel.TabIndex = 54;
			// 
			// BillOfLadingMainPage
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ServiceLevel);
			this.Controls.Add(this.CustomsEntryNumberTypeBoundDropEdit);
			this.Controls.Add(this.JS_NoCopyBillsCalcEdit);
			this.Controls.Add(this.JS_HBLAWBChargesDisplayBoundDropEdit);
			this.Controls.Add(this.JS_NoOriginalBillsCalcEdit);
			this.Controls.Add(this.JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText);
			this.Controls.Add(this.JS_GoodsDescriptionTextBoxBoundTextBox);
			this.Controls.Add(this.JS_ReleaseTypeBoundDropEdit);
			this.Controls.Add(this.JS_ShipmentStatusBoundDropEdit);
			this.Controls.Add(this.JS_PackingModeBoundDropEdit);
			this.Controls.Add(this.JS_RL_NKDestinationBoundCodeFindBox);
			this.Controls.Add(this.JS_RL_NKOriginBoundCodeFindBox);
			this.Controls.Add(this.JS_E_DEPBoundDateEdit);
			this.Controls.Add(this.JS_E_ARVBoundDateEdit);
			this.Controls.Add(this.JS_HouseBillIssueDateEdit);
			this.Controls.Add(this.JS_RL_NKHouseBillIssuePlaceFindBox);
			this.Controls.Add(this.JS_ShippedOnBoardDropEdit);
			this.Controls.Add(this.JS_ActualVolumeBoundZCalcDropEdit);
			this.Controls.Add(this.JS_OuterPacksBoundZCalcDropEdit);
			this.Controls.Add(this.JS_ShippedOnBoardDateEdit);
			this.Controls.Add(this.JS_ActualWeightBoundCalcDropEdit);
			this.Controls.Add(this.JS_BookingReferenceBoundZTextBox);
			this.Controls.Add(this.JS_CFSReferenceBoundZTextBox);
			this.Controls.Add(this.NotifyPartyDocAddressControl);
			this.Controls.Add(this.JS_INCOBoundDropEdit);
			this.Controls.Add(this.CustomsEntryNumberBoundTextBox);
			this.Controls.Add(this.JS_HouseBillBoundTextBox);
			this.Controls.Add(this.ConsigneeDocumentaryDocAddressControl);
			this.Controls.Add(this.ConsignorDocumentaryDocAddressControl);
			this.Controls.Add(this.SailingControl);
			this.Name = "BillOfLadingMainPage";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 587, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_PackingModeBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JS_RL_NKDestinationBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JS_RL_NKOriginBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JS_ShippedOnBoardDateEdit;
		private Enterprise.ZArchitecture.ZCalcEdit JS_NoCopyBillsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit JS_NoOriginalBillsCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JS_HouseBillIssueDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_ShippedOnBoardDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit JS_ActualVolumeBoundZCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit JS_ActualWeightBoundCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit JS_OuterPacksBoundZCalcDropEdit;
		private Enterprise.ZArchitecture.ZTextBox JS_CFSReferenceBoundZTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_ReleaseTypeBoundDropEdit;
		private Enterprise.ZArchitecture.ZTextBox JS_BookingReferenceBoundZTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_INCOBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CustomsEntryNumberTypeBoundDropEdit;
		private Enterprise.ZArchitecture.ZTextBox CustomsEntryNumberBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox JS_HouseBillBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_HBLAWBChargesDisplayBoundDropEdit;
		private Enterprise.Freight.GUI.GoodsDescriptionTextBox JS_GoodsDescriptionTextBoxBoundTextBox;
		private Enterprise.Freight.GUI.ZStmNotePopupEditWithBindableText JS_MarksAndNumbersShortBoundStmNotePopupEditWithBindableText;
		protected internal Enterprise.MasterFiles.GUI.ZDocAddressControl ConsignorDocumentaryDocAddressControl;
		protected internal Enterprise.MasterFiles.GUI.ZDocAddressControl ConsigneeDocumentaryDocAddressControl;
		protected internal Enterprise.MasterFiles.GUI.ZDocAddressControl NotifyPartyDocAddressControl;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JS_E_ARVBoundDateEdit;
		private SailingUserControl SailingControl;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JS_E_DEPBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_ShipmentStatusBoundDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox ServiceLevel;
		private ZArchitecture.GUI.ZCodeFindBox JS_RL_NKHouseBillIssuePlaceFindBox;
	}
}
