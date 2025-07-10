using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	partial class AgencyBookingAdditionalDetails
	{
		void InitializeComponent()
		{
			this.ConsigneeDocumentaryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsignorDocumentaryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			sendingAgentAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			receivingAgentAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			referenceNumbersControl = new Enterprise.MasterFiles.GUI.NumbersControl();
			placeOfReceiptCodeFindBox = new ZCodeFindBox();
			placeOfDischargeCodeFindBox = new ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyBooking);
			// 
			// sendingAgentAddressControl
			// 
			sendingAgentAddressControl.AllowDrop = true;
			sendingAgentAddressControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.BindingSource.SetBindingMember(sendingAgentAddressControl, "SendingAgentAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).SendingAgentAddressPK)));
			sendingAgentAddressControl.BindToOrgList = "BindToLists+Organisations";
			sendingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 200, true);
			sendingAgentAddressControl.Name = "sendingAgentAddressControl";
			sendingAgentAddressControl.PopupCaption = "";
			sendingAgentAddressControl.ShowAddress = false;
			sendingAgentAddressControl.ShowOrganisationName = true;
			sendingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			sendingAgentAddressControl.TabIndex = 60;
			// 
			// receivingAgentAddressControl
			// 
			receivingAgentAddressControl.AllowDrop = true;
			receivingAgentAddressControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.BindingSource.SetBindingMember(receivingAgentAddressControl, "ReceivingAgentAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).ReceivingAgentAddressPK)));
			receivingAgentAddressControl.BindToOrgList = "BindToLists+Organisations";
			receivingAgentAddressControl.CaptionResourceString = Res.GetData("BookingDetailsControl|6544c5bf-deb3-41d0-bce7-c708104a10be", "Receiving Agent", "Receiving Agent based on Principal and Destination port of the Shipment.");
			receivingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 226, true);
			receivingAgentAddressControl.Name = "receivingAgentAddressControl";
			receivingAgentAddressControl.PopupCaption = "";
			receivingAgentAddressControl.ShowAddress = false;
			receivingAgentAddressControl.ShowOrganisationName = true;
			receivingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			receivingAgentAddressControl.TabIndex = 61;
			// 
			// ConsigneeDocumentaryDocAddressControl
			// 
			this.ConsigneeDocumentaryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocumentaryDocAddressControl, "ConsigneeDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).ConsigneeDocumentaryAddress)));
			this.ConsigneeDocumentaryDocAddressControl.BindToContacts = "Lookups.ConsigneeContacts_List";
			this.ConsigneeDocumentaryDocAddressControl.BindToOrganisations = "Lookups.Consignee_List";
			this.ConsigneeDocumentaryDocAddressControl.CaptionResourceString = Res.GetData("AgencyBookingAdditionalDetails|e0407485-e2f7-43ea-b6b7-7334d933eae7", "Consignee");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeDocumentaryDocAddressControl, false);
			this.ConsigneeDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 8, true);
			this.ConsigneeDocumentaryDocAddressControl.Name = "ConsigneeDocumentaryDocAddressControl";
			this.ConsigneeDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsigneeDocumentaryDocAddressControl.TabIndex = 59;
			this.ConsigneeDocumentaryDocAddressControl.Enter += new System.EventHandler(this.ConsigneeDocumentaryDocAddressControl_Enter);
			// 
			// ConsignorDocumentaryDocAddressControl
			// 
			this.ConsignorDocumentaryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorDocumentaryDocAddressControl, "ConsignorDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).ConsignorDocumentaryAddress)));
			this.ConsignorDocumentaryDocAddressControl.BindToContacts = "Lookups.ConsignorContacts_List";
			this.ConsignorDocumentaryDocAddressControl.BindToOrganisations = "Lookups.Consignor_List";
			this.ConsignorDocumentaryDocAddressControl.CaptionResourceString = Res.GetData("AgencyBookingAdditionalDetails|00f4bd7f-6659-4493-b938-8002700414fa", "Consignor");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsignorDocumentaryDocAddressControl, false);
			this.ConsignorDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ConsignorDocumentaryDocAddressControl.Name = "ConsignorDocumentaryDocAddressControl";
			this.ConsignorDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsignorDocumentaryDocAddressControl.TabIndex = 58;
			this.ConsignorDocumentaryDocAddressControl.Enter += new System.EventHandler(this.ConsignorDocumentaryDocAddressControl_Enter);
			// 
			// referenceNumbersControl
			// 
			referenceNumbersControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(referenceNumbersControl, "Numbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Common.CusEntryNumAdditionalReferenceCollection)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).Numbers)));
			referenceNumbersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 8, true);
			referenceNumbersControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 200, true);
			referenceNumbersControl.Name = "referenceNumbersControl";
			referenceNumbersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 240, true);
			referenceNumbersControl.TabIndex = 62;
			// 
			// PlaceOfReceiptCodeFindBox
			// 
			placeOfReceiptCodeFindBox.AllowDrop = true;
			BindingSource.SetBindingMember(placeOfReceiptCodeFindBox, "JS_RL_NKPlaceOfReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_RL_NKPlaceOfReceipt)));
			placeOfReceiptCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 252, true);
			placeOfReceiptCodeFindBox.Name = "placeOfReceiptCodeFindBox";
			placeOfReceiptCodeFindBox.PopupCaption = "Select Place Of Receipt";
			placeOfReceiptCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			placeOfReceiptCodeFindBox.CaptionResourceString = Res.GetData("add15eef-b3b2-4664-8544-406c32bfe3be", "Place Of Receipt");
			// 
			// PlaceOfDischargeCodeFindBox
			// 
			placeOfDischargeCodeFindBox.AllowDrop = true;
			BindingSource.SetBindingMember(placeOfDischargeCodeFindBox, "JS_RL_NKPlaceOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).JS_RL_NKPlaceOfDischarge)));
			placeOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 277, true);
			placeOfDischargeCodeFindBox.Name = "placeOfDischargeCodeFindBox";
			placeOfDischargeCodeFindBox.PopupCaption = "Select Place Of Delivery";
			placeOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			placeOfDischargeCodeFindBox.CaptionResourceString = Res.GetData("08e3b17f-17d0-4768-b1db-7e9258313f85", "Place Of Delivery");
			// 
			// AgencyBookingAdditionalDetails
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(referenceNumbersControl);
			this.Controls.Add(sendingAgentAddressControl);
			this.Controls.Add(receivingAgentAddressControl);
			this.Controls.Add(placeOfReceiptCodeFindBox);
			this.Controls.Add(placeOfDischargeCodeFindBox);
			this.Controls.Add(this.ConsigneeDocumentaryDocAddressControl);
			this.Controls.Add(this.ConsignorDocumentaryDocAddressControl);
			this.Name = "AgencyBookingAdditionalDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 306, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		protected internal MasterFiles.GUI.ZDocAddressControl ConsigneeDocumentaryDocAddressControl;
		protected internal MasterFiles.GUI.ZDocAddressControl ConsignorDocumentaryDocAddressControl;
		Enterprise.ZArchitecture.GUI.ZAddressControl sendingAgentAddressControl;
		Enterprise.ZArchitecture.GUI.ZAddressControl receivingAgentAddressControl;
		Enterprise.MasterFiles.GUI.NumbersControl referenceNumbersControl;
		ZCodeFindBox placeOfReceiptCodeFindBox;
		ZCodeFindBox placeOfDischargeCodeFindBox;
	}
}
