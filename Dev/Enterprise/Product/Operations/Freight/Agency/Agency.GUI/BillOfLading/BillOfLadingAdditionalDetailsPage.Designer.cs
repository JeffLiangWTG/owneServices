using CargoWiseOne.ResourceStrings;

namespace Enterprise.Freight.Agency.GUI
{
	partial class BillOfLadingAdditionalDetailsPage
	{
		void InitializeComponent()
		{
			this.goodsValueBoundCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.orderItemsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.orderItemsEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.customsDetailPanel = new Enterprise.Freight.Agency.GUI.CustomsDetailsControl();
			this.placeOfReceiptCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.placeOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			referenceNumbersGroupBox = new Enterprise.MasterFiles.GUI.NumbersControl();
			sendingAgentAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			receivingAgentAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.tabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.detailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.customfieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.customFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tabControl.SuspendLayout();
			this.detailsTabPage.SuspendLayout();
			this.customfieldsTabPage.SuspendLayout();
			this.customFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BillOfLading);
			// 
			// referenceNumbersGroupBox
			// 
			referenceNumbersGroupBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(referenceNumbersGroupBox, "Numbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).Numbers)));
			referenceNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			referenceNumbersGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 200, true);
			referenceNumbersGroupBox.Name = "referenceNumbersGroupBox";
			referenceNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 328, true);
			referenceNumbersGroupBox.TabIndex = 57;
			referenceNumbersGroupBox.TabStop = false;
			// 
			// sendingAgentAddressControl
			// 
			sendingAgentAddressControl.AllowDrop = true;
			sendingAgentAddressControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.BindingSource.SetBindingMember(sendingAgentAddressControl, "SendingAgentAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).SendingAgentAddressPK)));
			sendingAgentAddressControl.BindToOrgList = "BindToLists+Organisations";
			sendingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 12, true);
			sendingAgentAddressControl.Name = "sendingAgentAddressControl";
			sendingAgentAddressControl.PopupCaption = "";
			sendingAgentAddressControl.ShowAddress = false;
			sendingAgentAddressControl.ShowOrganisationName = true;
			sendingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			sendingAgentAddressControl.TabIndex = 58;
			// 
			// receivingAgentAddressControl
			// 
			receivingAgentAddressControl.AllowDrop = true;
			receivingAgentAddressControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.BindingSource.SetBindingMember(receivingAgentAddressControl, "ReceivingAgentAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).ReceivingAgentAddressPK)));
			receivingAgentAddressControl.BindToOrgList = "BindToLists+Organisations";
			receivingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 39, true);
			receivingAgentAddressControl.Name = "receivingAgentAddressControl";
			receivingAgentAddressControl.PopupCaption = "";
			receivingAgentAddressControl.ShowAddress = false;
			receivingAgentAddressControl.ShowOrganisationName = true;
			receivingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 21, true);
			receivingAgentAddressControl.TabIndex = 59;
			// 
			// customsDetailPanel
			// 
			this.customsDetailPanel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.customsDetailPanel, ".");
			this.customsDetailPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 117, true);
			this.customsDetailPanel.Name = "customsDetailPanel";
			this.customsDetailPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 56, true);
			this.customsDetailPanel.TabIndex = 56;
			// 
			// placeOfReceiptCodeFindBox
			// 
			this.placeOfReceiptCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.placeOfReceiptCodeFindBox, "JS_RL_NKPlaceOfReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_RL_NKPlaceOfReceipt)));
			this.placeOfReceiptCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 66, true);
			this.placeOfReceiptCodeFindBox.Name = "placeOfReceiptCodeFindBox";
			this.placeOfReceiptCodeFindBox.PopupCaption = "Select Place Of Receipt";
			this.placeOfReceiptCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.placeOfReceiptCodeFindBox.CaptionResourceString = Res.GetData("4259a6fd-3cbb-4bbf-aba2-c38d7769c41f", "Place Of Receipt");
			// 
			// placeOfDischargeCodeFindBox
			// 
			this.placeOfDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.placeOfDischargeCodeFindBox, "JS_RL_NKPlaceOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).JS_RL_NKPlaceOfDischarge)));
			this.placeOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 91, true);
			this.placeOfDischargeCodeFindBox.Name = "placeOfDischargeCodeFindBox";
			this.placeOfDischargeCodeFindBox.PopupCaption = "Select Place Of Delivery";
			this.placeOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			placeOfDischargeCodeFindBox.CaptionResourceString = Res.GetData("d619ffe7-83ce-49e0-8c6f-4f80beece4ef", "Place Of Delivery");
			// 
			// orderItemsTextBox
			// 
			this.orderItemsTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.orderItemsTextBox, "DocsAndCartage.JP_OrderItemsAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).DocsAndCartage.JP_OrderItemsAsString)));
			this.orderItemsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 350, true);
			this.orderItemsTextBox.Name = "orderItemsTextBox";
			this.orderItemsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.orderItemsTextBox.CaptionResourceString = Res.GetData("fa127540-dea8-473a-87f2-c117100c71dc", "Order Refs");
			// 
			// orderItemsEditButton
			// 
			this.orderItemsEditButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("366d0b4b-9f1f-41b4-83c8-19154c5ba8a0", "More...");
			this.orderItemsEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 350, true);
			this.orderItemsEditButton.Name = "orderItemsEditButton";
			this.orderItemsEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 20, true);
			this.orderItemsEditButton.Click += new System.EventHandler(this.OrderReferencesButton_Click);
			// 
			// goodsValueBoundCurrencyCodeFindBox
			// 
			this.goodsValueBoundCurrencyCodeFindBox.AllowDrop = true;
			this.goodsValueBoundCurrencyCodeFindBox.BindToAmount = "JS_GoodsValue";
			this.goodsValueBoundCurrencyCodeFindBox.BindToUnit = "JS_RX_NKGoodsValueCurr";
			this.goodsValueBoundCurrencyCodeFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.goodsValueBoundCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 375, true);
			this.goodsValueBoundCurrencyCodeFindBox.Name = "goodsValueBoundCurrencyCodeFindBox";
			this.goodsValueBoundCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			// 
			// tabControl
			// 
			this.tabControl.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.tabControl.Controls.Add(this.detailsTabPage);
			this.tabControl.Controls.Add(this.customfieldsTabPage);
			this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 8, true);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 210, true);
			this.tabControl.TabIndex = 55;
			// 
			// detailsTabPage
			// 
			this.detailsTabPage.AutoScroll = true;
			this.detailsTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("6d587ab9-4031-4a06-a46b-3f9fb107ad8e", "Details");
			this.detailsTabPage.Controls.Add(this.sendingAgentAddressControl);
			this.detailsTabPage.Controls.Add(this.receivingAgentAddressControl);
			this.detailsTabPage.Controls.Add(this.placeOfReceiptCodeFindBox);
			this.detailsTabPage.Controls.Add(this.placeOfDischargeCodeFindBox);
			this.detailsTabPage.Controls.Add(this.customsDetailPanel);
			this.detailsTabPage.Name = "detailsTabPage";
			// customFieldsControl
			this.customFieldsControl.AllowDrop = true;
			this.customFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customFieldsControl.Name = "CustomFieldsControl";
			this.customFieldsControl.NothingSetupMessageLabelText = Res.GetString("5feba27e-66ce-4153-8243-1c98cb7583ab", "To make use of this tab, please setup Shipment custom field for L&&A Shipment in Workflow Manager.");
			this.customFieldsControl.TabIndex = 0;
			// customfieldsTabPage
			this.customfieldsTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ac3db837-4675-4319-be64-7b0ed11f2824", "Custom Fields");
			this.customfieldsTabPage.Controls.Add(this.customFieldsControl);
			this.customfieldsTabPage.Name = "customfieldsTabPage";
			this.customfieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8);
			// 
			// BillOfLadingAdditionalDetailsPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.orderItemsTextBox);
			this.Controls.Add(this.orderItemsEditButton);
			this.Controls.Add(this.goodsValueBoundCurrencyCodeFindBox);
			this.Controls.Add(referenceNumbersGroupBox);
			this.Controls.Add(this.tabControl);
			this.Name = "BillOfLadingAdditionalDetailsPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 597, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tabControl.ResumeLayout(false);
			this.detailsTabPage.ResumeLayout(false);
			this.customfieldsTabPage.ResumeLayout(false);
			this.customFieldsControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		CustomsDetailsControl customsDetailPanel;
		ZArchitecture.GUI.ZCodeFindBox placeOfReceiptCodeFindBox;
		ZArchitecture.GUI.ZCodeFindBox placeOfDischargeCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCalcFindBox goodsValueBoundCurrencyCodeFindBox;
		Enterprise.ZArchitecture.ZTextBox orderItemsTextBox;
		Enterprise.ZArchitecture.GUI.ZButton orderItemsEditButton;
		Enterprise.MasterFiles.GUI.NumbersControl referenceNumbersGroupBox;
		Enterprise.ZArchitecture.GUI.ZAddressControl sendingAgentAddressControl;
		Enterprise.ZArchitecture.GUI.ZAddressControl receivingAgentAddressControl;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl tabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage detailsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage customfieldsTabPage;
		Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl customFieldsControl;
	}
}
