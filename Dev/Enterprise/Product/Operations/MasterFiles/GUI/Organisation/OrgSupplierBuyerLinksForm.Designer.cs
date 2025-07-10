namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgSupplierBuyerLinksForm
	{

		#region Windows Form Designer generated code

		Enterprise.ZArchitecture.ZGrid BuyerSupplierGrid;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit MonthsDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox SetShipmentDatesCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		protected Enterprise.ZArchitecture.GUI.ZButton OKButton;

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BuyerSupplierGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.MonthsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SetShipmentDatesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BuyerSupplierGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 272, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkCollectionReadOnlyView);
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierBuyerLinksForm|d1275f51-91cc-48c3-99b9-c4c57380fdab", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(791, 223, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// BuyerSupplierGrid
			// 
			this.BuyerSupplierGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BuyerSupplierGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).SelectedForPrinting)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_RX_NKDefaultCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_ControllingCustomer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OC_NotifyPartyContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_OH_ImportBroker)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OL_InitialShipmentExpected)));
			this.BuyerSupplierGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierBuyerLinksForm|18aeb2f7-b74c-4055-9711-49aa6c7ed7c7", "Include", "Include", "");
			zCheckBoxColumnStyleInfo1.ColumnName = "SelectedForPrinting";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OL_OH_Supplier";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierBuyerLinksForm|bdec0b04-36e9-41a3-a039-aa140d373f56", "Buyer Name", "The Buyer whom which this Consignor/Shipper has a defined relationship with.");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OL_OH_Buyer";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OL_RX_NKDefaultCurrency";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierBuyerLinksForm|2963ad37-894a-496e-97b3-5a037c5a1c13", "Controlling Customer");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "OL_OH_ControllingCustomer";
			zGuidFindBoxColumnStyleInfo3.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo4.ColumnName = "OL_OC_NotifyPartyContact";
			zGuidFindBoxColumnStyleInfo4.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo5.ColumnName = "OL_OH_ImportBroker";
			zGuidFindBoxColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierBuyerLinksForm|a17c1f22-1db6-4960-85de-22c9e6009d99", "Initial Shipment Expected", "The date the first shipment is expected for this relationship.");
			zDateEditColumnStyleInfo1.ColumnName = "OL_InitialShipmentExpected";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			this.BuyerSupplierGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BuyerSupplierGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BuyerSupplierGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.BuyerSupplierGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.BuyerSupplierGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.BuyerSupplierGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.BuyerSupplierGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.BuyerSupplierGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.BuyerSupplierGrid.GridId = "8ad26b64-7714-4c75-bc04-15a986c51e45";
			this.BuyerSupplierGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BuyerSupplierGrid.LayoutKey = "BuyerSupplierGrid";
			this.BuyerSupplierGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 52, true);
			this.BuyerSupplierGrid.Name = "BuyerSupplierGrid";
			this.BuyerSupplierGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 163, true);
			this.BuyerSupplierGrid.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierBuyerLinksForm|ac404402-4b57-4e12-8fd7-c19b11ebfad0", "Please select the parties that you wish to generate this document for.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 15, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 21, true);
			this.zLabel1.TabIndex = 0;
			// 
			// MonthsDropEdit
			// 
			this.BindingSource.SetBindingMember(this.MonthsDropEdit, "ExpectedShipmentMonthsAddition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).ExpectedShipmentMonthsAddition)));
			this.MonthsDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierBuyerLinksForm|cd1e21e0-ace8-4fda-b457-c83fc486c9a2", "First Expected Shipment Date (months from today)");
			this.MonthsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 224, true);
			this.MonthsDropEdit.MaxItemsToShowInDropDown = 12;
			this.MonthsDropEdit.Name = "MonthsDropEdit";
			this.MonthsDropEdit.PreBoundMaxLength = 2;
			this.MonthsDropEdit.ShowDescriptionBox = false;
			this.MonthsDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.MonthsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.MonthsDropEdit.TabIndex = 3;
			// 
			// SetShipmentDatesCheckBox
			// 
			this.SetShipmentDatesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SetShipmentDatesCheckBox, "UpdateShipmentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).UpdateShipmentDate)));
			this.SetShipmentDatesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SetShipmentDatesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 223, true);
			this.SetShipmentDatesCheckBox.Name = "SetShipmentDatesCheckBox";
			this.SetShipmentDatesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SetShipmentDatesCheckBox.TabIndex = 2;
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierBuyerLinksForm|02cd761d-1297-4a02-8fdb-a9488cc3e7ba", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(872, 223, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 5;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelOrderButton_Click);
			// 
			// OrgSupplierBuyerLinksForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 296, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierBuyerLinksForm|dff8c3ee-6891-487d-b7c8-f1dff3e92a7a", "Supplier Links");
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.MonthsDropEdit);
			this.Controls.Add(this.SetShipmentDatesCheckBox);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.BuyerSupplierGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkCollectionReadOnlyView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "OrgSupplierBuyerLinksForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BuyerSupplierGrid, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.SetShipmentDatesCheckBox, 0);
			this.Controls.SetChildIndex(this.MonthsDropEdit, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BuyerSupplierGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
