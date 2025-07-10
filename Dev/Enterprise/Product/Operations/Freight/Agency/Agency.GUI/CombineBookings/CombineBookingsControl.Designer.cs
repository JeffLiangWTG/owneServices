namespace Enterprise.Freight.Agency.GUI
{
	partial class CombineBookingsControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.shipmentsGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			topGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			cargoTypeTextBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			voyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			dischargeTextBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			originTextBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			vesselTextBox = new Enterprise.ZArchitecture.ZTextBox();
			shipmentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			shipmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			topGroupBox.SuspendLayout();
			shipmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.shipmentsGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.CombineBookings);
			// 
			// topGroupBox
			// 
			topGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CombineBookingsControl|5ea2a288-b11f-43cc-8fe3-9d7cc7282c70", "Main Booking");
			topGroupBox.Controls.Add(cargoTypeTextBox);
			topGroupBox.Controls.Add(voyageTextBox);
			topGroupBox.Controls.Add(dischargeTextBox);
			topGroupBox.Controls.Add(originTextBox);
			topGroupBox.Controls.Add(vesselTextBox);
			topGroupBox.Controls.Add(shipmentNumberTextBox);
			topGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			topGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			topGroupBox.Name = "topGroupBox";
			topGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 93, true);
			topGroupBox.TabIndex = 0;
			topGroupBox.TabStop = false;
			// 
			// cargoTypeTextBox
			// 
			this.BindingSource.SetBindingMember(cargoTypeTextBox, "MasterBooking.JS_PackingMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.CombineBookings)(null)).MasterBooking.JS_PackingMode)));
			cargoTypeTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CombineBookingsControl|34f29d00-1898-47d9-ac20-8d6928ef8b94", "Cargo Type", "The type of cargo to be transported.");
			cargoTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 16, true);
			cargoTypeTextBox.Name = "cargoTypeTextBox";
			cargoTypeTextBox.PreBoundMaxLength = 3;
			cargoTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			cargoTypeTextBox.TabIndex = 1;
			// 
			// voyageTextBox
			// 
			this.BindingSource.SetBindingMember(voyageTextBox, "MasterBooking.Sailing.JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.CombineBookings)(null)).MasterBooking.Sailing.JX_JV_VoyageFlight)));
			voyageTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CombineBookingsControl|bd1cc424-a5a8-4564-a37d-b93d56b3f7a4", "Voyage");
			voyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 40, true);
			voyageTextBox.Name = "voyageTextBox";
			voyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			voyageTextBox.TabIndex = 3;
			// 
			// dischargeTextBox
			// 
			this.BindingSource.SetBindingMember(dischargeTextBox, "MasterBooking.Sailing.JX_JB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.CombineBookings)(null)).MasterBooking.Sailing.JX_JB_RL_NKPortOfDischarge)));
			dischargeTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CombineBookingsControl|a6e975e9-dc0c-456f-a279-80fd7ac4a74e", "Disch.", "Port Of Discharge", "The discharge port of the current sailing.");
			dischargeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 64, true);
			dischargeTextBox.Name = "dischargeTextBox";
			dischargeTextBox.PreBoundMaxLength = 5;
			dischargeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			dischargeTextBox.TabIndex = 5;
			// 
			// originTextBox
			// 
			this.BindingSource.SetBindingMember(originTextBox, "MasterBooking.Sailing.JX_JA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.CombineBookings)(null)).MasterBooking.Sailing.JX_JA_RL_NKPortOfLoading)));
			originTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CombineBookingsControl|40451026-ece0-4cd2-baca-f88bb3421985", "Load", "Port Of Loading", "The load port of the current sailing.");
			originTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 64, true);
			originTextBox.Name = "originTextBox";
			originTextBox.PreBoundMaxLength = 5;
			originTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			originTextBox.TabIndex = 4;
			// 
			// vesselTextBox
			// 
			this.BindingSource.SetBindingMember(vesselTextBox, "MasterBooking.Sailing.JX_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.CombineBookings)(null)).MasterBooking.Sailing.JX_JV_NKVessel)));
			vesselTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CombineBookingsControl|8e37b84e-b18f-42a4-a1e8-3cb83abe939c", "Vessel");
			vesselTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
			vesselTextBox.Name = "vesselTextBox";
			vesselTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			vesselTextBox.TabIndex = 2;
			// 
			// shipmentNumberTextBox
			// 
			this.BindingSource.SetBindingMember(shipmentNumberTextBox, "MasterBooking.JS_UniqueConsignRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.CombineBookings)(null)).MasterBooking.JS_UniqueConsignRef)));
			shipmentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			shipmentNumberTextBox.Name = "shipmentNumberTextBox";
			shipmentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			shipmentNumberTextBox.TabIndex = 0;
			// 
			// shipmentsGroupBox
			// 
			shipmentsGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CombineBookingsControl|ab92ee5a-f077-4964-b2d2-f391d185b020", "Bookings to Merge Into the Main Booking");
			shipmentsGroupBox.Controls.Add(this.shipmentsGrid);
			shipmentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			shipmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 96, true);
			shipmentsGroupBox.Name = "shipmentsGroupBox";
			shipmentsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 5, true);
			shipmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 333, true);
			shipmentsGroupBox.TabIndex = 1;
			shipmentsGroupBox.TabStop = false;
			// 
			// shipmentsGrid
			// 
			this.shipmentsGrid.AttachButtonText = Enterprise.Freight.Agency.GUI.Res.GetData("BA8DEBAD-AB4A-4945-9DE8-8DA38F340E47", "Add");
			this.BindingSource.SetBindingMember(this.shipmentsGrid, "OtherBookings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.CombineBookings)(null)).OtherBookings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.CombineBookings)(null)).Lookups.Bookings)));
			this.shipmentsGrid.BindToFindBoxList = "Lookups.Bookings";
			zTextBoxColumnStyleInfo1.ColumnName = "JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ZModuleButtonGrid|11ffc35f-cd80-4065-ab3d-0aee00d51c8f", "Booking Party");
			zMultiControlColumnStyleInfo1.ColumnName = "BookingPartyNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "BookingPartyFieldType";
			zMultiControlColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ZModuleButtonGrid|dd032a9e-8f0f-4c37-9803-56bbe15d96a3", "Consignor");
			zMultiControlColumnStyleInfo2.ColumnName = "ConsignorNameOrPK";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "ConsignorFieldType";
			zMultiControlColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ZModuleButtonGrid|947b9236-ed37-4d39-8e43-732e980bc600", "Consignee");
			zMultiControlColumnStyleInfo3.ColumnName = "ConsigneeNameOrPK";
			zMultiControlColumnStyleInfo3.FieldTypeColumnName = "ConsigneeFieldType";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ZModuleButtonGrid|faacd42b-b3f6-4003-8290-f22437c42e1a", "Cargo Type", "The type of cargo to be transported.");
			zTextBoxColumnStyleInfo2.ColumnName = "JS_PackingMode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ZModuleButtonGrid|b0be8b44-364e-4656-8fa1-c918a7cb334f", "Vessel");
			zTextBoxColumnStyleInfo3.ColumnName = "Sailing+JX_JV_NKVessel";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ZModuleButtonGrid|a01c9f2f-e381-459a-9acc-3eeee2229442", "Voyage");
			zTextBoxColumnStyleInfo4.ColumnName = "Sailing+JX_JV_VoyageFlight";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ZModuleButtonGrid|72f80301-b4f7-4bae-b4c1-34774f1c278e", "Load Port", "The port where the goods are expected to be loaded.");
			zTextBoxColumnStyleInfo5.ColumnName = "JS_NKLoadPort";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ZModuleButtonGrid|ad97118f-7542-4e58-80a8-2c7af29588c1", "Disch. Port", "Discharge Port", "The port where the goods are expected to be unloaded.");
			zTextBoxColumnStyleInfo6.ColumnName = "JS_NKDischargePort";
			this.shipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.shipmentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.shipmentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.shipmentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo3);
			this.shipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.shipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.shipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.shipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.shipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.shipmentsGrid.GridId = "aa015662-70a7-417d-b23b-db0d09f7743b";
			this.shipmentsGrid.DetachButtonText = Enterprise.Freight.Agency.GUI.Res.GetData("352E0B8C-5C08-4A13-AF87-CCFA926412FB", "Remove");
			this.shipmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.shipmentsGrid.InnerGrid.AllowNavigation = false;
			this.shipmentsGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.shipmentsGrid.InnerGrid.CaptionVisible = false;
			this.shipmentsGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.shipmentsGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.shipmentsGrid.InnerGrid.LayoutKey = "Grid";
			this.shipmentsGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.shipmentsGrid.InnerGrid.Name = "Grid";
			this.shipmentsGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 274, true);
			this.shipmentsGrid.InnerGrid.TabIndex = 0;
			this.shipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 16, true);
			this.shipmentsGrid.Name = "shipmentsGrid";
			this.shipmentsGrid.NameOfAGridElement = Enterprise.Freight.Agency.GUI.Res.GetData("46FD57C7-9415-4C46-B66E-A5C90D0C24EE", "Booking");
			this.shipmentsGrid.ReadOnly = false;
			this.shipmentsGrid.ShowEditButton = false;
			this.shipmentsGrid.ShowNewButton = false;
			this.shipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 312, true);
			this.shipmentsGrid.TabIndex = 0;
			// 
			// CombineBookingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(shipmentsGroupBox);
			this.Controls.Add(topGroupBox);
			this.Name = "CombineBookingsControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 432, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			topGroupBox.ResumeLayout(false);
			topGroupBox.PerformLayout();
			shipmentsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.shipmentsGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZModuleButtonGrid shipmentsGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox topGroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit cargoTypeTextBox;
		Enterprise.ZArchitecture.ZTextBox voyageTextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox dischargeTextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox originTextBox;
		Enterprise.ZArchitecture.ZTextBox vesselTextBox;
		Enterprise.ZArchitecture.ZTextBox shipmentNumberTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox shipmentsGroupBox;
	}
}
