using CargoWiseOne.ResourceStrings;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class BookingDetailsControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.orderItemsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.orderItemsEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BookingDetalisPannl = new CargoWise.Windows.UI.KPanel();
			this.ServiceLevelCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.incoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PacksCountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsEntryNumberTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsEntryNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BookingPartyDocumentaryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.JS_GoodsDescriptionBoundTextBox = new Enterprise.Freight.GUI.GoodsDescriptionTextBox();
			this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SailingControl = new Enterprise.Freight.Agency.GUI.SailingUserControl();
			this.JS_ShippingReferenceTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_ActualVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JS_ActualWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JS_PackingModeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JS_RL_NKDestinationBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JS_RL_NKOriginBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JS_E_DEPBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_E_ARVBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_A_BKDBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CFSReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipmentContentTabControl = new Enterprise.Freight.Agency.GUI.BookingContentTabControl();
			this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JS_ShipmentStatusBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			additionalDetailsControl = new Enterprise.Freight.Agency.GUI.AgencyBookingAdditionalDetails();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BookingDetalisPannl.SuspendLayout();
			this.ShipmentContentTabControl.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyBooking);
			// 
			// BookingDetalisPannl
			// 
			this.BookingDetalisPannl.Controls.Add(this.JS_ShipmentStatusBoundDropEdit);
			this.BookingDetalisPannl.Controls.Add(this.ServiceLevelCodeFindBox);
			this.BookingDetalisPannl.Controls.Add(this.incoDropEdit);
			this.BookingDetalisPannl.Controls.Add(this.PacksCountCalcDropEdit);
			this.BookingDetalisPannl.Controls.Add(this.CustomsEntryNumberTypeBoundDropEdit);
			this.BookingDetalisPannl.Controls.Add(this.CustomsEntryNumberBoundTextBox);
			this.BookingDetalisPannl.Controls.Add(this.BookingPartyDocumentaryDocAddressControl);
			this.BookingDetalisPannl.Controls.Add(this.JS_GoodsDescriptionBoundTextBox);
			this.BookingDetalisPannl.Controls.Add(this.orderItemsTextBox);
			this.BookingDetalisPannl.Controls.Add(this.orderItemsEditButton);
			this.BookingDetalisPannl.Controls.Add(this.ConfirmButton);
			this.BookingDetalisPannl.Controls.Add(this.SailingControl);
			this.BookingDetalisPannl.Controls.Add(this.JS_ShippingReferenceTextEdit);
			this.BookingDetalisPannl.Controls.Add(this.JS_ActualVolumeCalcDropEdit);
			this.BookingDetalisPannl.Controls.Add(this.JS_ActualWeightCalcDropEdit);
			this.BookingDetalisPannl.Controls.Add(this.JS_PackingModeBoundDropEdit);
			this.BookingDetalisPannl.Controls.Add(this.JS_RL_NKDestinationBoundCodeFindBox);
			this.BookingDetalisPannl.Controls.Add(this.JS_RL_NKOriginBoundCodeFindBox);
			this.BookingDetalisPannl.Controls.Add(this.JS_E_DEPBoundDateEdit);
			this.BookingDetalisPannl.Controls.Add(this.JS_E_ARVBoundDateEdit);
			this.BookingDetalisPannl.Controls.Add(this.JS_A_BKDBoundReadOnlyDateEdit);
			this.BookingDetalisPannl.Controls.Add(this.CFSReferenceTextBox);
			this.BookingDetalisPannl.Dock = System.Windows.Forms.DockStyle.Top;
			this.BookingDetalisPannl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BookingDetalisPannl.Name = "BookingDetalisPannl";
			this.BookingDetalisPannl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 296, true);
			this.BookingDetalisPannl.TabIndex = 0;
			// 
			// ServiceLevelCodeFindBox
			// 
			this.ServiceLevelCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelCodeFindBox, "JS_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_RS_NKServiceLevel)));
			this.ServiceLevelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 267, true);
			this.ServiceLevelCodeFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ServiceLevelCodeFindBox.Name = "ServiceLevelCodeFindBox";
			this.ServiceLevelCodeFindBox.PopupCaption = null;
			this.ServiceLevelCodeFindBox.PreBoundMaxLength = 3;
			this.ServiceLevelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.ServiceLevelCodeFindBox.TabIndex = 33;
			// 
			// incoDropEdit
			// 
			this.incoDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.incoDropEdit, "JS_INCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_INCO)));
			this.incoDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|df3e530c-ab8a-43fe-85eb-c70228a22e3e", "P/C", "PPD/COL", "Payment Term", "Specifies if the shipment is prepaid or collect.");
			this.incoDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.incoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 244, true);
			this.incoDropEdit.Name = "incoDropEdit";
			this.incoDropEdit.PreBoundMaxLength = 3;
			this.incoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.incoDropEdit.TabIndex = 20;
			// 
			// PacksCountCalcDropEdit
			// 
			this.PacksCountCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PacksCountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_F3_NKPackType)));
			this.PacksCountCalcDropEdit.BindToAmount = "JS_OuterPacks";
			this.PacksCountCalcDropEdit.BindToUnit = "JS_F3_NKPackType";
			this.PacksCountCalcDropEdit.Decimals = 0;
			this.PacksCountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 196, true);
			this.PacksCountCalcDropEdit.Name = "PacksCountCalcDropEdit";
			this.PacksCountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PacksCountCalcDropEdit.TabIndex = 22;
			this.PacksCountCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CustomsEntryNumberTypeBoundDropEdit
			// 
			this.CustomsEntryNumberTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberTypeBoundDropEdit, "CustomsEntryNumberType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).CustomsEntryNumberType)));
			this.CustomsEntryNumberTypeBoundDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|d99e5414-1c16-42e6-bbd8-6aa622e50859", "Customs Entry Number Type");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsEntryNumberTypeBoundDropEdit, false);
			this.CustomsEntryNumberTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(750, 267, true);
			this.CustomsEntryNumberTypeBoundDropEdit.Name = "CustomsEntryNumberTypeBoundDropEdit";
			this.CustomsEntryNumberTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.CustomsEntryNumberTypeBoundDropEdit.ShowDescriptionBox = false;
			this.CustomsEntryNumberTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CustomsEntryNumberTypeBoundDropEdit.TabIndex = 33;
			// 
			// CustomsEntryNumberBoundTextBox
			// 
			this.CustomsEntryNumberBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberBoundTextBox, "CustomsEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).CustomsEntryNumber)));
			this.CustomsEntryNumberBoundTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|140a3329-7439-442c-9726-f372e88e0b1e", "Customs Entry Number");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsEntryNumberBoundTextBox, false);
			this.CustomsEntryNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(817, 267, true);
			this.CustomsEntryNumberBoundTextBox.Name = "CustomsEntryNumberBoundTextBox";
			this.CustomsEntryNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.CustomsEntryNumberBoundTextBox.TabIndex = 34;
			// 
			// BookingPartyDocumentaryDocAddressControl
			// 
			this.BookingPartyDocumentaryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BookingPartyDocumentaryDocAddressControl, "BookingPartyDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).BookingPartyDocumentaryAddress)));
			this.BookingPartyDocumentaryDocAddressControl.BindToOrganisations = "Lookups.Consignor_List";
			this.BookingPartyDocumentaryDocAddressControl.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|030bd527-c1da-457e-92cb-69570707e054", "Booking Party");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BookingPartyDocumentaryDocAddressControl, false);
			this.BookingPartyDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(744, 8, true);
			this.BookingPartyDocumentaryDocAddressControl.Name = "BookingPartyDocumentaryDocAddressControl";
			this.BookingPartyDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.BookingPartyDocumentaryDocAddressControl.TabIndex = 8;
			// 
			// JS_GoodsDescriptionBoundTextBox
			// 
			this.JS_GoodsDescriptionBoundTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_GoodsDescriptionBoundTextBox, "JS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_GoodsDescription)));
			this.JS_GoodsDescriptionBoundTextBox.ButtonText = Res.GetString("7DBD75A3-C537-478C-A237-180BD9528504", "Detail");
			this.JS_GoodsDescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 244, true);
			this.JS_GoodsDescriptionBoundTextBox.Name = "JS_GoodsDescriptionBoundTextBox";
			this.JS_GoodsDescriptionBoundTextBox.NoteTypeDescription = "Detailed Goods Description";
			this.JS_GoodsDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.JS_GoodsDescriptionBoundTextBox.TabIndex = 18;
			//
			// orderItemsTextBox
			// 
			this.BindingSource.SetBindingMember(this.orderItemsTextBox, "DocsAndCartage.JP_OrderItemsAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).DocsAndCartage.JP_OrderItemsAsString)));
			this.orderItemsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(817, 244, true);
			this.orderItemsTextBox.Name = "orderItemsTextBox";
			this.orderItemsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.orderItemsTextBox.CaptionResourceString = Res.GetData("a64a044e-4128-411f-ad81-d0b78a0291ea", "Order Refs");
			this.orderItemsTextBox.TabIndex = 31;
			// 
			// orderItemsEditButton
			// 
			this.orderItemsEditButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("c53abfc8-c777-42b9-a485-849fd07ad0a6", "More...");
			this.orderItemsEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(945, 243, true);
			this.orderItemsEditButton.Name = "orderItemsEditButton";
			this.orderItemsEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 22, true);
			this.orderItemsEditButton.Click += new System.EventHandler(OrderReferencesButton_Click);
			this.orderItemsEditButton.TabIndex = 32;
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|7905b1c8-f70b-4eb8-bec1-bea031f7f4e9", "Confirm");
			this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(594, 165, true);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.ConfirmButton.TabIndex = 7;
			// 
			// SailingControl
			// 
			this.SailingControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SailingControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyShipment)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)))));
			this.SailingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 34, true);
			this.SailingControl.Name = "SailingControl";
			this.SailingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 156, true);
			this.SailingControl.TabIndex = 6;
			// 
			// JS_ShippingReferenceTextEdit
			// 
			this.JS_ShippingReferenceTextEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JS_ShippingReferenceTextEdit, "JS_BookingReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_BookingReference)));
			this.JS_ShippingReferenceTextEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|011ea4ba-28cd-40f1-841c-c59b2bc04050", "Shipper\'s Ref", "The shippers reference.");
			this.JS_ShippingReferenceTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(817, 220, true);
			this.JS_ShippingReferenceTextEdit.Name = "JS_ShippingReferenceTextEdit";
			this.JS_ShippingReferenceTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.JS_ShippingReferenceTextEdit.TabIndex = 30;
			// 
			// JS_ActualVolumeCalcDropEdit
			// 
			this.JS_ActualVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_ActualVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_UnitOfVolume)));
			this.JS_ActualVolumeCalcDropEdit.BindToAmount = "JS_ActualVolume";
			this.JS_ActualVolumeCalcDropEdit.BindToUnit = "JS_UnitOfVolume";
			this.JS_ActualVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 244, true);
			this.JS_ActualVolumeCalcDropEdit.Name = "JS_ActualVolumeCalcDropEdit";
			this.JS_ActualVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.JS_ActualVolumeCalcDropEdit.TabIndex = 26;
			this.JS_ActualVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JS_ActualWeightCalcDropEdit
			// 
			this.JS_ActualWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_ActualWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_UnitOfWeight)));
			this.JS_ActualWeightCalcDropEdit.BindToAmount = "JS_ActualWeight";
			this.JS_ActualWeightCalcDropEdit.BindToUnit = "JS_UnitOfWeight";
			this.JS_ActualWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 220, true);
			this.JS_ActualWeightCalcDropEdit.Name = "JS_ActualWeightCalcDropEdit";
			this.JS_ActualWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.JS_ActualWeightCalcDropEdit.TabIndex = 24;
			this.JS_ActualWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JS_PackingModeBoundDropEdit
			// 
			this.JS_PackingModeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_PackingModeBoundDropEdit, "JS_PackingMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_PackingMode)));
			this.JS_PackingModeBoundDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|ac3aec22-e771-4298-bce1-ba63e8aebf38", "Cargo Type", "The type of cargo to be transported.");
			this.JS_PackingModeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 9, true);
			this.JS_PackingModeBoundDropEdit.Name = "JS_PackingModeBoundDropEdit";
			this.JS_PackingModeBoundDropEdit.PreBoundMaxLength = 3;
			this.JS_PackingModeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.JS_PackingModeBoundDropEdit.TabIndex = 5;
			// 
			// JS_RL_NKDestinationBoundCodeFindBox
			// 
			this.JS_RL_NKDestinationBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKDestinationBoundCodeFindBox, "JS_RL_NKDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_RL_NKDestination)));
			this.JS_RL_NKDestinationBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 220, true);
			this.JS_RL_NKDestinationBoundCodeFindBox.Name = "JS_RL_NKDestinationBoundCodeFindBox";
			this.JS_RL_NKDestinationBoundCodeFindBox.PopupCaption = "Select Destination";
			this.JS_RL_NKDestinationBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JS_RL_NKDestinationBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.JS_RL_NKDestinationBoundCodeFindBox.TabIndex = 14;
			// 
			// JS_RL_NKOriginBoundCodeFindBox
			// 
			this.JS_RL_NKOriginBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKOriginBoundCodeFindBox, "JS_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_RL_NKOrigin)));
			this.JS_RL_NKOriginBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 196, true);
			this.JS_RL_NKOriginBoundCodeFindBox.Name = "JS_RL_NKOriginBoundCodeFindBox";
			this.JS_RL_NKOriginBoundCodeFindBox.PopupCaption = "Select Origin";
			this.JS_RL_NKOriginBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JS_RL_NKOriginBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.JS_RL_NKOriginBoundCodeFindBox.TabIndex = 10;
			// 
			// JS_E_DEPBoundDateEdit
			// 
			this.JS_E_DEPBoundDateEdit.AllowDrop = true;
			this.JS_E_DEPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_E_DEPBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_E_DEPBoundDateEdit, "JS_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_E_DEP)));
			this.JS_E_DEPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 198, true);
			this.JS_E_DEPBoundDateEdit.Name = "JS_E_DEPBoundDateEdit";
			this.JS_E_DEPBoundDateEdit.TabIndex = 12;
			// 
			// JS_E_ARVBoundDateEdit
			// 
			this.JS_E_ARVBoundDateEdit.AllowDrop = true;
			this.JS_E_ARVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_E_ARVBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_E_ARVBoundDateEdit, "JS_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_E_ARV)));
			this.JS_E_ARVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 222, true);
			this.JS_E_ARVBoundDateEdit.Name = "JS_E_ARVBoundDateEdit";
			this.JS_E_ARVBoundDateEdit.TabIndex = 16;
			// 
			// JS_A_BKDBoundReadOnlyDateEdit
			// 
			this.JS_A_BKDBoundReadOnlyDateEdit.AllowDrop = true;
			this.JS_A_BKDBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_A_BKDBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_A_BKDBoundReadOnlyDateEdit, "JS_A_BKD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_A_BKD)));
			this.JS_A_BKDBoundReadOnlyDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|aef57da0-e6c1-44b7-86f0-9833e442b45b", "Booking Date");
			this.JS_A_BKDBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124,9, true);
			this.JS_A_BKDBoundReadOnlyDateEdit.Name = "JS_A_BKDBoundReadOnlyDateEdit";
			this.JS_A_BKDBoundReadOnlyDateEdit.TabIndex = 1;
			// 
			// CFSReferenceTextBox
			// 
			this.CFSReferenceTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CFSReferenceTextBox, "JS_CFSReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_CFSReference)));
			this.CFSReferenceTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|45f19e1a-dcdb-4b4e-bac7-855598929237", "Booking Ref", "The booking reference or release number.");
			this.CFSReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(817, 196, true);
			this.CFSReferenceTextBox.Name = "CFSReferenceTextBox";
			this.CFSReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.CFSReferenceTextBox.TabIndex = 28;
			// 
			// ShipmentContentTabControl
			// 
			this.ShipmentContentTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentContentTabControl.Controls.Add(this.zTabPage1);
			this.ShipmentContentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentContentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 296, true);
			this.ShipmentContentTabControl.Name = "ShipmentContentTabControl";
			this.ShipmentContentTabControl.SelectedIndex = 0;
			this.ShipmentContentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 313, true);
			this.ShipmentContentTabControl.TabIndex = 1;
			// 
			// zTabPage1
			// 
			this.zTabPage1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|80d1307e-4197-41f2-8353-c1cc326a03d4", "Additional Information");
			this.zTabPage1.Controls.Add(additionalDetailsControl);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 286, true);
			this.zTabPage1.TabIndex = 3;
			// 
			// additionalDetailsControl
			// 
			additionalDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(additionalDetailsControl, ".");
			additionalDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			additionalDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			additionalDetailsControl.Name = "additionalDetailsControl";
			additionalDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 286, true);
			additionalDetailsControl.TabIndex = 0;
			// 
			// JS_ShipmentStatusBoundDropEdit
			// 
			this.JS_ShipmentStatusBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_ShipmentStatusBoundDropEdit, "JS_ShipmentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_ShipmentStatus)));
			this.JS_ShipmentStatusBoundDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BookingDetailsControl|c988d9f0-a3a0-4dfc-bdbb-9577629b7708", "Status", "The status of cargo to be transported.");
			this.JS_ShipmentStatusBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 9, true);
			this.JS_ShipmentStatusBoundDropEdit.Name = "JS_ShipmentStatusBoundDropEdit";
			this.JS_ShipmentStatusBoundDropEdit.PreBoundMaxLength = 3;
			this.JS_ShipmentStatusBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.JS_ShipmentStatusBoundDropEdit.TabIndex = 3;
			// 
			// BookingDetailsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShipmentContentTabControl);
			this.Controls.Add(this.BookingDetalisPannl);
			this.Name = "BookingDetailsControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 630, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BookingDetalisPannl.ResumeLayout(false);
			this.BookingDetalisPannl.PerformLayout();
			this.ShipmentContentTabControl.ResumeLayout(false);
			this.zTabPage1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		CargoWise.Windows.UI.KPanel BookingDetalisPannl;
		protected internal Enterprise.MasterFiles.GUI.ZDocAddressControl BookingPartyDocumentaryDocAddressControl;
		Enterprise.ZArchitecture.ZTextBox orderItemsTextBox;
		Enterprise.ZArchitecture.GUI.ZButton orderItemsEditButton;
		Enterprise.Freight.GUI.GoodsDescriptionTextBox JS_GoodsDescriptionBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZButton ConfirmButton;
		Enterprise.ZArchitecture.ZTextBox JS_ShippingReferenceTextEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit JS_PackingModeBoundDropEdit;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox JS_RL_NKDestinationBoundCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox JS_RL_NKOriginBoundCodeFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZDateEdit JS_A_BKDBoundReadOnlyDateEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit CustomsEntryNumberTypeBoundDropEdit;
		Enterprise.ZArchitecture.ZTextBox CustomsEntryNumberBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox CFSReferenceTextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZDateEdit JS_E_ARVBoundDateEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZDateEdit JS_E_DEPBoundDateEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit incoDropEdit;
		Enterprise.ZArchitecture.GUI.ZCalcDropEdit PacksCountCalcDropEdit;
		SailingUserControl SailingControl;
		Enterprise.ZArchitecture.GUI.ZCalcDropEdit JS_ActualVolumeCalcDropEdit;
		Enterprise.ZArchitecture.GUI.ZCalcDropEdit JS_ActualWeightCalcDropEdit;
		BookingContentTabControl ShipmentContentTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage zTabPage1;
		ZArchitecture.GUI.ZCodeFindBox ServiceLevelCodeFindBox;
		Enterprise.Freight.Agency.GUI.AgencyBookingAdditionalDetails additionalDetailsControl;
		ZArchitecture.GUI.ZDropEdit JS_ShipmentStatusBoundDropEdit;
	}
}
