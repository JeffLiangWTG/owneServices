using CargoWise.Types;
namespace Enterprise.Freight.Confirmations.GUI
{
	partial class QuickPODForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.QuickPODGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EditSelectedShipmentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.QuickPODGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 409, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Confirmations.Business.QuickPODs);
			// 
			// QuickPODGrid
			// 
			this.QuickPODGrid.AllowNavigation = false;
			this.QuickPODGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QuickPODGrid, "QuickPODsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).ShipmentID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryConfirm.EU_GoodsSignForBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryConfirm.EU_PickupDeliveryTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryConfirm.TotalBookedPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).PacksDelivered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryConfirm.TotalBookedWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryConfirm.TotalWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryConfirm.TotalBookedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryConfirm.TotalVolumeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).DeliveryConfirm.EU_OA_TransportProvider_ZAddress.OrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).Charge.JR_RX_NKCostCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).Charge.JR_LocalCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).Charge.JR_OH_CostAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).Charge.JR_RX_NKSellCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).Charge.JR_LocalSellAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Confirmations.Business.QuickPOD)(((System.Collections.IList)(((Enterprise.Freight.Confirmations.Business.QuickPODs)(null)).QuickPODsCollection)).SyncRoot)).Charge.JR_InvoiceType)));
			this.QuickPODGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|cb815891-7a2c-43d9-a7cd-a5de3a6dee83", "House Bill");
			zTextBoxColumnStyleInfo1.ColumnName = "HouseBill";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.ToolTip = "Please enter a housebill of the shipment you wish to do a POD for.";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|417f1bbf-9476-4fc6-9ee2-1e43c1926ff7", "Shipment ID");
			zTextBoxColumnStyleInfo2.ColumnName = "ShipmentID";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.ToolTip = "Please enter the ID of the shipment you wish to do a POD for.";
			zTextBoxColumnStyleInfo3.ColumnName = "DeliveryConfirm+EU_GoodsSignForBy";
			zTextBoxColumnStyleInfo3.ToolTip = "Please enter who the goods were received by.";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zDateEditColumnStyleInfo1.ColumnName = "DeliveryConfirm+EU_PickupDeliveryTime";
			zDateEditColumnStyleInfo1.ToolTip = "Please enter or click to select a delivery time of the goods.";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|274fcffd-4d99-4a4e-a2be-80d892e064c9", "Delivered On");
			zCalcEditColumnStyleInfo1.ColumnName = "DeliveryConfirm+TotalBookedPackages";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.ToolTip = "Total booked packages.";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(38);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|33375489-ea46-4568-a9c4-c977738b759f", "Pkgs. Dlvd.", "Packages Delivered.");
			zCalcEditColumnStyleInfo2.ColumnName = "PacksDelivered";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.ToolTip = "Please enter the packages delivered.";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|db99f097-57cb-434f-b78b-308cfe799961", "Weight");
			zCalcEditColumnStyleInfo3.ColumnName = "DeliveryConfirm+TotalBookedWeight";
			zCalcEditColumnStyleInfo3.Decimals = 3;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.ToolTip = "Total booked weight.";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|8e4170e4-0792-4de3-add3-fbae582a8781", "Dlvd. Weight");
			zCalcEditColumnStyleInfo4.ColumnName = "DeliveryWeight";
			zCalcEditColumnStyleInfo4.Decimals = 3;
			zCalcEditColumnStyleInfo4.ToolTip = "Please enter the delivered weight.";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|f0f76cd6-66c0-40f8-8fed-ce8c2c2fb754", "UW");
			zTextBoxColumnStyleInfo4.ColumnName = "DeliveryConfirm+TotalWeightUnit";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ToolTip = "Unit of Weight.";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|f7fcf631-5525-429a-ba5f-23938150b541", "Volume", "Volume\r\n.");
			zCalcEditColumnStyleInfo5.ColumnName = "DeliveryConfirm+TotalBookedVolume";
			zCalcEditColumnStyleInfo5.Decimals = 3;
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.ToolTip = "Total booked volume.";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|ba6f9ce2-1ba6-4b04-bf5e-8d95d73c8348", "Dlvd. Volume");
			zCalcEditColumnStyleInfo6.ColumnName = "DeliveryVolume";
			zCalcEditColumnStyleInfo6.Decimals = 3;
			zCalcEditColumnStyleInfo6.ToolTip = "Please enter the delivered volume.";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|4e435cf4-2bfd-49b6-b080-4a7cc241eb84", "UV");
			zTextBoxColumnStyleInfo5.ColumnName = "DeliveryConfirm+TotalVolumeUnit";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.ToolTip = "Unit of Volume.";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|bade96c2-67db-4012-b29f-0f734fa3919b", "Transport Co", "Transport Company", "");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "DeliveryConfirm+EU_OA_TransportProvider_ZAddress+OrgPK";
			zOrganisationFindBoxColumnStyleInfo1.ToolTip = "Please enter or click to select the Transport Company.";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|fe42c3a6-f1e1-4bf7-ac78-df55dade8a2c", "Charge");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCode";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.ToolTip = "Please enter or click to select a charge code.";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Charge+JR_RX_NKCostCurrency";
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Please enter or click to select the currency of the cost amount.";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "Charge+JR_LocalCostAmt";
			zCalcEditColumnStyleInfo7.ToolTip = "Please enter the local cost amount here if it is in local currency.";
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "Charge+JR_OH_CostAccount";
			zOrganisationFindBoxColumnStyleInfo2.ToolTip = "Please enter or click to select a creditor where applicable. This field is applic" +
				"able when you enter an Actual Cost or want to indicate the expected Credit" +
				"or for the estimated Cost or Accrual.";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Charge+JR_RX_NKSellCurrency";
			zCodeFindBoxColumnStyleInfo2.ToolTip = "Please enter or click to select the currency of the sell amount.";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "Charge+JR_LocalSellAmt";
			zCalcEditColumnStyleInfo8.ToolTip = "Please enter the local sell amount here if it is in local currency.";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zDropEditColumnStyleInfo1.ColumnName = "Charge+JR_InvoiceType";
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.QuickPODGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.QuickPODGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.QuickPODGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.QuickPODGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.QuickPODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.QuickPODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.QuickPODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.QuickPODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.QuickPODGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.QuickPODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.QuickPODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.QuickPODGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.QuickPODGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.QuickPODGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.QuickPODGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.QuickPODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.QuickPODGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.QuickPODGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.QuickPODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.QuickPODGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.QuickPODGrid.GridId = "7a29bde5-e3ba-486a-9bf9-c86be9ef496d";
			this.QuickPODGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuickPODGrid.LayoutKey = "QuickPODGrid";
			this.QuickPODGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuickPODGrid.Name = "QuickPODGrid";
			this.QuickPODGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 378, true);
			this.QuickPODGrid.TabIndex = 1;
			// 
			// EditSelectedShipmentButton
			// 
			this.EditSelectedShipmentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EditSelectedShipmentButton.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|8ce3f736-4e83-4808-8d56-751445aa37f8", "Edit Selected Shipments");
			this.EditSelectedShipmentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 380, true);
			this.EditSelectedShipmentButton.Name = "EditSelectedShipmentButton";
			this.EditSelectedShipmentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 23, true);
			this.EditSelectedShipmentButton.TabIndex = 2;
			this.EditSelectedShipmentButton.UseVisualStyleBackColor = true;
			this.EditSelectedShipmentButton.Click += new System.EventHandler(this.EditSelectedShipmentButton_Click);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(748, 380, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 3;
			// 
			// QuickPODForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 433, true);
			this.CaptionResourceString = Enterprise.Freight.Confirmations.GUI.Res.GetData("QuickPODForm|67b9275a-0448-468d-924c-4e544a375a91", "Quick POD");
			this.Controls.Add(this.QuickPODGrid);
			this.Controls.Add(this.EditSelectedShipmentButton);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Confirmations.Business.QuickPODs);
			this.DataSourceTypeName = "Enterprise.Freight.Confirmations.Business.QuickPODs";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 290, true);
			this.Name = "QuickPODForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.EditSelectedShipmentButton, 0);
			this.Controls.SetChildIndex(this.QuickPODGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.QuickPODGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid QuickPODGrid;
		private Enterprise.ZArchitecture.GUI.ZButton EditSelectedShipmentButton;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
	}
}
