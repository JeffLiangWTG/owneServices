namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class WhsPickingUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PickingSequenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductionRulesEngineLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.DefaultWarehousePickOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OrdersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WhsOrderDefaultPickPriorityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WhsOrderFulfillmentRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PickPackGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PickPackGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackageWeightToleranceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WhsPackageToleranceEnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WhsPackageWeightToleranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotePackingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WhsEnforceScanOfProductsWhenPackingToteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReleasingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WhsPreventReleaseOfPackageIfNotPickedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PickingSequenceGroupBox.SuspendLayout();
			this.DefaultWarehousePickOptionDropEdit.SuspendLayout();
			this.OrdersGroupBox.SuspendLayout();
			this.WhsOrderFulfillmentRuleDropEdit.SuspendLayout();
			this.PickPackGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PickPackGrid)).BeginInit();
			this.PickPackGrid.SuspendLayout();
			this.PackageWeightToleranceGroupBox.SuspendLayout();
			this.TotePackingGroupBox.SuspendLayout();
			this.ReleasingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams);
			// 
			// PickingSequenceGroupBox
			// 
			this.PickingSequenceGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsPickingUserControl|765fa4fc-e846-4a64-a78a-1f93f314bcbc", "Picking Sequence");
			this.PickingSequenceGroupBox.Controls.Add(this.ProductionRulesEngineLinkLabel);
			this.PickingSequenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 115, true);
			this.PickingSequenceGroupBox.Name = "PickingSequenceGroupBox";
			this.PickingSequenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 67, true);
			this.PickingSequenceGroupBox.TabIndex = 1;
			this.PickingSequenceGroupBox.TabStop = false;
			// 
			// ProductionRulesEngineLinkLabel
			// 
			this.ProductionRulesEngineLinkLabel.AutoSize = true;
			this.ProductionRulesEngineLinkLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsPickingUserControl|d65ec608-0493-4bae-80d6-b01361170b81", "Setup Allocation Rules");
			this.ProductionRulesEngineLinkLabel.IsFontBold = false;
			this.ProductionRulesEngineLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 20, true);
			this.ProductionRulesEngineLinkLabel.Name = "ProductionRulesEngineLinkLabel";
			this.ProductionRulesEngineLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 13, true);
			this.ProductionRulesEngineLinkLabel.TabIndex = 0;
			this.ProductionRulesEngineLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ProductionRulesEngineLinkLabel_LinkClicked);
			// 
			// DefaultWarehousePickOptionDropEdit
			// 
			this.DefaultWarehousePickOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultWarehousePickOptionDropEdit, "Client+MiscServ+OM_IMDefaultWarehousePickOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).Client.MiscServ.OM_IMDefaultWarehousePickOption)));
			this.DefaultWarehousePickOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 19, true);
			this.DefaultWarehousePickOptionDropEdit.Name = "DefaultWarehousePickOptionDropEdit";
			this.DefaultWarehousePickOptionDropEdit.PreBoundMaxLength = 3;
			this.DefaultWarehousePickOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 15, true);
			this.DefaultWarehousePickOptionDropEdit.TabIndex = 1;
			// 
			// OrdersGroupBox
			// 
			this.OrdersGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsPickingUserControl|724dac24-b204-4870-893c-6dec3ec5191d", "Orders");
			this.OrdersGroupBox.Controls.Add(this.WhsOrderDefaultPickPriorityCalcEdit);
			this.OrdersGroupBox.Controls.Add(this.WhsOrderFulfillmentRuleDropEdit);
			this.OrdersGroupBox.Controls.Add(this.DefaultWarehousePickOptionDropEdit);
			this.OrdersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.OrdersGroupBox.Name = "OrdersGroupBox";
			this.OrdersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 106, true);
			this.OrdersGroupBox.TabIndex = 0;
			this.OrdersGroupBox.TabStop = false;
			// 
			// WhsOrderDefaultPickPriorityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WhsOrderDefaultPickPriorityCalcEdit, "Client+MiscServ.OM_WhsOrderDefaultPickPriority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).Client.MiscServ.OM_WhsOrderDefaultPickPriority)));
			this.WhsOrderDefaultPickPriorityCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsPickingUserControl|29EDF81D-61EB-4DF9-B119-A20306856BA5", "Default Pick Priority");
			this.WhsOrderDefaultPickPriorityCalcEdit.DecimalPlaces = 0;
			this.WhsOrderDefaultPickPriorityCalcEdit.Decimals = 0;
			this.WhsOrderDefaultPickPriorityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 71, true);
			this.WhsOrderDefaultPickPriorityCalcEdit.Name = "WhsOrderDefaultPickPriorityCalcEdit";
			this.WhsOrderDefaultPickPriorityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 15, true);
			this.WhsOrderDefaultPickPriorityCalcEdit.TabIndex = 3;
			this.WhsOrderDefaultPickPriorityCalcEdit.Text = "0";
			this.WhsOrderDefaultPickPriorityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.WhsOrderDefaultPickPriorityCalcEdit.TrackDisposedAccess = true;
			// 
			// WhsOrderFulfillmentRuleDropEdit
			// 
			this.WhsOrderFulfillmentRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WhsOrderFulfillmentRuleDropEdit, "Client+MiscServ+OM_WhsOrderFulfillmentRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).Client.MiscServ.OM_WhsOrderFulfillmentRule)));
			this.WhsOrderFulfillmentRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 45, true);
			this.WhsOrderFulfillmentRuleDropEdit.Name = "WhsOrderFulfillmentRuleDropEdit";
			this.WhsOrderFulfillmentRuleDropEdit.PreBoundMaxLength = 3;
			this.WhsOrderFulfillmentRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 15, true);
			this.WhsOrderFulfillmentRuleDropEdit.TabIndex = 2;
			// 
			// PickPackGroupBox
			// 
			this.PickPackGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PickPackGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("cf1c6003-e74f-40ad-a5b4-8f75ea24c9de", "Picking Parameters by Warehouse");
			this.PickPackGroupBox.Controls.Add(this.PickPackGrid);
			this.PickPackGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 188, true);
			this.PickPackGroupBox.Name = "PickPackGroupBox";
			this.PickPackGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1070, 323, true);
			this.PickPackGroupBox.TabIndex = 2;
			this.PickPackGroupBox.TabStop = false;
			// 
			// PickPackGrid
			// 
			this.PickPackGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PickPackGrid, "WarehousePickPackParams");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_WW_Warehouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WarehouseName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_WSH_SalesChannel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_IsPickAndPackEnabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_IsUsingOwnLabel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_NumberOfLabelsToPrintOnNew)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_NumberOfLabelsToPrintOnClose)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_CartoniseByArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_IsUsingCartonSizes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_PromptForWeightAndDimensions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_EnforceScanningForLoad)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_EnforceTransportReferenceForLoad)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_CartonizeByProduct)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_CartonizeByProductCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_CycleCountOnShort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_UseDirectedPackingConsolidation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_EnableAutoPackageCreationOnPicking)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_SplitOrdersFromPartiallyReplenishedPicks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_DetachWavedOrdersWithBlockingShortfall)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_AllowPickDockDoorLocationOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Environment.Business.WhsClientPickPackParamsByWhs)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).WarehousePickPackParams)).SyncRoot)).WPP_AllowPickFinalizationWithUnpackedTotes)));
			this.PickPackGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WPP_WW_Warehouse";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "WarehouseName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "WPP_WSH_SalesChannel";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "WPP_IsPickAndPackEnabled";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCheckBoxColumnStyleInfo2.ColumnName = "WPP_IsUsingOwnLabel";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "WPP_F3_NKPackType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "WPP_NumberOfLabelsToPrintOnNew";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "WPP_NumberOfLabelsToPrintOnClose";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zCheckBoxColumnStyleInfo3.ColumnName = "WPP_CartoniseByArea";
			zCheckBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zCheckBoxColumnStyleInfo4.ColumnName = "WPP_IsUsingCartonSizes";
			zCheckBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zCheckBoxColumnStyleInfo5.ColumnName = "WPP_PromptForWeightAndDimensions";
			zCheckBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zCheckBoxColumnStyleInfo6.ColumnName = "WPP_EnforceScanningForLoad";
			zCheckBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo7.ColumnName = "WPP_EnforceTransportReferenceForLoad";
			zCheckBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zCheckBoxColumnStyleInfo8.ColumnName = "WPP_CartonizeByProduct";
			zCheckBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zCheckBoxColumnStyleInfo9.ColumnName = "WPP_CartonizeByProductCategory";
			zCheckBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zCheckBoxColumnStyleInfo10.ColumnName = "WPP_CycleCountOnShort";
			zCheckBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo11.ColumnName = "WPP_UseDirectedPackingConsolidation";
			zCheckBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo12.ColumnName = "WPP_EnableAutoPackageCreationOnPicking";
			zCheckBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo13.ColumnName = "WPP_SplitOrdersFromPartiallyReplenishedPicks";
			zCheckBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo14.ColumnName = "WPP_AllowPickFinalizationWithUnpackedTotes";
			zCheckBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo15.ColumnName = "WPP_DetachWavedOrdersWithBlockingShortfall";
			zCheckBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo16.ColumnName = "WPP_AllowPickDockDoorLocationOverride";
			zCheckBoxColumnStyleInfo16.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.PickPackGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PickPackGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PickPackGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.PickPackGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PickPackGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PickPackGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo11);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo12);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo13);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo14);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo15);
			this.PickPackGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo16);
			this.PickPackGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickPackGrid.GridId = "04d4fa52-a85d-4f25-8021-4f9767d15d19";
			this.PickPackGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PickPackGrid.LayoutKey = "PickPackGrid";
			this.PickPackGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.PickPackGrid.Name = "PickPackGrid";
			this.PickPackGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1068, 308, true);
			this.PickPackGrid.TabIndex = 0;
			// 
			// PackageWeightToleranceGroupBox
			// 
			this.PackageWeightToleranceGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsPickingUserControl|9FCC84D6-532F-4706-B619-26DEBF61A4F8", "Outbound Check Weight");
			this.PackageWeightToleranceGroupBox.Controls.Add(this.WhsPackageToleranceEnabledCheckBox);
			this.PackageWeightToleranceGroupBox.Controls.Add(this.WhsPackageWeightToleranceCalcEdit);
			this.PackageWeightToleranceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 3, true);
			this.PackageWeightToleranceGroupBox.Name = "PackageWeightToleranceGroupBox";
			this.PackageWeightToleranceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 106, true);
			this.PackageWeightToleranceGroupBox.TabIndex = 4;
			this.PackageWeightToleranceGroupBox.TabStop = false;
			// 
			// WhsPackageToleranceEnabledCheckBox
			// 
			this.WhsPackageToleranceEnabledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WhsPackageToleranceEnabledCheckBox, "Client+MiscServ.OM_WhsPackageToleranceEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).Client.MiscServ.OM_WhsPackageToleranceEnabled)));
			this.WhsPackageToleranceEnabledCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("B8009CCD-432C-45D9-A33D-BCA249E14C81", "Package Weight Tolerance");
			this.WhsPackageToleranceEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 36, true);
			this.WhsPackageToleranceEnabledCheckBox.Name = "WhsPackageToleranceEnabledCheckBox";
			this.WhsPackageToleranceEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 14, true);
			this.WhsPackageToleranceEnabledCheckBox.TabIndex = 0;
			this.WhsPackageToleranceEnabledCheckBox.UseVisualStyleBackColor = true;
			// 
			// WhsPackageWeightToleranceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WhsPackageWeightToleranceCalcEdit, "Client+MiscServ.OM_WhsPackageWeightTolerancePercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).Client.MiscServ.OM_WhsPackageWeightTolerancePercent)));
			this.WhsPackageWeightToleranceCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("EBD629E2-C614-4D07-91A4-6AB3333396E9", "Weight Tolerance +/- %");
			this.WhsPackageWeightToleranceCalcEdit.DecimalPlaces = 2;
			this.WhsPackageWeightToleranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 63, true);
			this.WhsPackageWeightToleranceCalcEdit.Name = "WhsPackageWeightToleranceCalcEdit";
			this.WhsPackageWeightToleranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 15, true);
			this.WhsPackageWeightToleranceCalcEdit.TabIndex = 0;
			this.WhsPackageWeightToleranceCalcEdit.Text = "100.00";
			this.WhsPackageWeightToleranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.WhsPackageWeightToleranceCalcEdit.TrackDisposedAccess = true;
			// 
			// TotePackingGroupBox
			// 
			this.TotePackingGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsPickingUserControl|TotePacking", "Tote Packing");
			this.TotePackingGroupBox.Controls.Add(this.WhsEnforceScanOfProductsWhenPackingToteCheckBox);
			this.TotePackingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 115, true);
			this.TotePackingGroupBox.Name = "TotePackingGroupBox";
			this.TotePackingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 67, true);
			this.TotePackingGroupBox.TabIndex = 5;
			this.TotePackingGroupBox.TabStop = false;
			// 
			// WhsEnforceScanOfProductsWhenPackingToteCheckBox
			// 
			this.WhsEnforceScanOfProductsWhenPackingToteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WhsEnforceScanOfProductsWhenPackingToteCheckBox, "Client.MiscServ.OM_WhsEnforceScanOfProductsWhenPackingTote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).Client.MiscServ.OM_WhsEnforceScanOfProductsWhenPackingTote)));
			this.WhsEnforceScanOfProductsWhenPackingToteCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsPickingUserControl|EnforceScanOfProducts", "Enforce Scan of Products");
			this.WhsEnforceScanOfProductsWhenPackingToteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 28, true);
			this.WhsEnforceScanOfProductsWhenPackingToteCheckBox.Name = "WhsEnforceScanOfProductsWhenPackingToteCheckBox";
			this.WhsEnforceScanOfProductsWhenPackingToteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 14, true);
			this.WhsEnforceScanOfProductsWhenPackingToteCheckBox.TabIndex = 0;
			this.WhsEnforceScanOfProductsWhenPackingToteCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReleasingGroupBox
			// 
			this.ReleasingGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ac496eb3-1756-423b-bdf6-c8965697009f", "Releasing");
			this.ReleasingGroupBox.Controls.Add(this.WhsPreventReleaseOfPackageIfNotPickedCheckBox);
			this.ReleasingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(743, 3, true);
			this.ReleasingGroupBox.Name = "ReleasingGroupBox";
			this.ReleasingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 106, true);
			this.ReleasingGroupBox.TabIndex = 6;
			this.ReleasingGroupBox.TabStop = false;
			// 
			// WhsPreventReleaseOfPackageIfNotPickedCheckBox
			// 
			this.WhsPreventReleaseOfPackageIfNotPickedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WhsPreventReleaseOfPackageIfNotPickedCheckBox, "Client.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Transactions.Business.WhsClientPickingParams)(null)).Client.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked)));
			this.WhsPreventReleaseOfPackageIfNotPickedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 28, true);
			this.WhsPreventReleaseOfPackageIfNotPickedCheckBox.Name = "WhsPreventReleaseOfPackageIfNotPickedCheckBox";
			this.WhsPreventReleaseOfPackageIfNotPickedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 14, true);
			this.WhsPreventReleaseOfPackageIfNotPickedCheckBox.TabIndex = 0;
			this.WhsPreventReleaseOfPackageIfNotPickedCheckBox.UseVisualStyleBackColor = true;
			// 
			// WhsPickingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReleasingGroupBox);
			this.Controls.Add(this.TotePackingGroupBox);
			this.Controls.Add(this.PackageWeightToleranceGroupBox);
			this.Controls.Add(this.PickPackGroupBox);
			this.Controls.Add(this.OrdersGroupBox);
			this.Controls.Add(this.PickingSequenceGroupBox);
			this.Name = "WhsPickingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1081, 524, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PickingSequenceGroupBox.ResumeLayout(false);
			this.PickingSequenceGroupBox.PerformLayout();
			this.DefaultWarehousePickOptionDropEdit.ResumeLayout(true);
			this.DefaultWarehousePickOptionDropEdit.PerformLayout();
			this.OrdersGroupBox.ResumeLayout(false);
			this.OrdersGroupBox.PerformLayout();
			this.WhsOrderFulfillmentRuleDropEdit.ResumeLayout(true);
			this.WhsOrderFulfillmentRuleDropEdit.PerformLayout();
			this.PickPackGroupBox.ResumeLayout(false);
			this.PickPackGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PickPackGrid)).EndInit();
			this.PickPackGrid.ResumeLayout(false);
			this.PickPackGrid.PerformLayout();
			this.PackageWeightToleranceGroupBox.ResumeLayout(false);
			this.PackageWeightToleranceGroupBox.PerformLayout();
			this.TotePackingGroupBox.ResumeLayout(false);
			this.TotePackingGroupBox.PerformLayout();
			this.ReleasingGroupBox.ResumeLayout(false);
			this.ReleasingGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox PickingSequenceGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DefaultWarehousePickOptionDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OrdersGroupBox;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel ProductionRulesEngineLinkLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit WhsOrderFulfillmentRuleDropEdit;
		private ZArchitecture.ZCalcEdit WhsOrderDefaultPickPriorityCalcEdit;
		private ZArchitecture.GUI.ZGroupBox PickPackGroupBox;
		private ZArchitecture.ZGrid PickPackGrid;
		private ZArchitecture.GUI.ZGroupBox PackageWeightToleranceGroupBox;
		private ZArchitecture.ZCalcEdit WhsPackageWeightToleranceCalcEdit;
		private ZArchitecture.GUI.ZCheckBox WhsPackageToleranceEnabledCheckBox;
		private ZArchitecture.GUI.ZGroupBox TotePackingGroupBox;
		private ZArchitecture.GUI.ZCheckBox WhsEnforceScanOfProductsWhenPackingToteCheckBox;
		private ZArchitecture.GUI.ZGroupBox ReleasingGroupBox;
		private ZArchitecture.GUI.ZCheckBox WhsPreventReleaseOfPackageIfNotPickedCheckBox;
	}
}
