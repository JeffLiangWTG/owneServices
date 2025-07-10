namespace Enterprise.Customs.GUI
{
	partial class TransportDetailsUserControl
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
			this.PortOfFirstArrivalUserControl = new Enterprise.Customs.GUI.TransportDetailsPortOfFirstArrivalUserControl();
			this.IATALoadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfLoadingUserControl = new Enterprise.Customs.GUI.TransportDetailsPortOfLoadingUserControl();
			this.PortOfDischargeUserControl = new Enterprise.Customs.GUI.TransportDetailsPortOfDischargeUserControl();
			this.FlightUserControl = new Enterprise.Customs.GUI.TransportDetailsFlightUserControl();
			this.VehicleRegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VoyageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OverrideValuesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MasterBillTextBox = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.OceanBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportIDAndNationalityUserControl = new Enterprise.Customs.GUI.TransportIDAndNationalityUserControl();
			this.FlightAndNationalityUserControl = new Enterprise.Customs.GUI.FlightAndNationalityUserControl();
			this.InlandModeOfTransportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VoyageAndNationalityUserControl = new Enterprise.Customs.GUI.VoyageAndNationalityUserControl();
			this.TransportDetailsPortOfLoadingWithIATAUserControl = new Enterprise.Customs.GUI.TransportDetailsPortOfLoadingWithIATAUserControl();
			this.TransportInlandSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.TransportInlandModeAndTypeOfIdUserControl = new Enterprise.Customs.GUI.TransportInlandModeAndTypeOfIdUserControl();
			this.TransportInlandIDAndNationalityUserControl = new Enterprise.Customs.GUI.TransportInlandIDAndNationalityUserControl();
			this.TransportInlandRoadUserControl = new Enterprise.Customs.GUI.TransportInlandRoadUserControl();
			this.TransportInlandAirUserControl = new Enterprise.Customs.GUI.TransportInlandAirUserControl();
			this.TransportInlandInlandWaterwaysUserControl = new Enterprise.Customs.GUI.TransportInlandInlandWaterwaysUserControl();
			this.TransportInlandOwnPropulsionUserControl = new Enterprise.Customs.GUI.TransportInlandOwnPropulsionUserControl();
			this.TransportInlandRailUserControl = new Enterprise.Customs.GUI.TransportInlandRailUserControl();
			this.TransportInlandSeaUserControl = new Enterprise.Customs.GUI.TransportInlandSeaUserControl();
			this.GoodsDestinationCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.UCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SubLocationOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportMeansDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsLoadPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortOfFirstArrivalUserControl.SuspendLayout();
			this.IATALoadPortCodeFindBox.SuspendLayout();
			this.PortOfLoadingUserControl.SuspendLayout();
			this.PortOfDischargeUserControl.SuspendLayout();
			this.FlightUserControl.SuspendLayout();
			this.MasterBillTextBox.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.TransportIDAndNationalityUserControl.SuspendLayout();
			this.FlightAndNationalityUserControl.SuspendLayout();
			this.InlandModeOfTransportDropEdit.SuspendLayout();
			this.VoyageAndNationalityUserControl.SuspendLayout();
			this.TransportDetailsPortOfLoadingWithIATAUserControl.SuspendLayout();
			this.TransportInlandSeparatorUserControl.SuspendLayout();
			this.TransportInlandModeAndTypeOfIdUserControl.SuspendLayout();
			this.TransportInlandIDAndNationalityUserControl.SuspendLayout();
			this.TransportInlandRoadUserControl.SuspendLayout();
			this.TransportInlandAirUserControl.SuspendLayout();
			this.TransportInlandInlandWaterwaysUserControl.SuspendLayout();
			this.TransportInlandOwnPropulsionUserControl.SuspendLayout();
			this.TransportInlandRailUserControl.SuspendLayout();
			this.TransportInlandSeaUserControl.SuspendLayout();
			this.GoodsDestinationCountryCodeFindBox.SuspendLayout();
			this.TransportMeansDropEdit.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.CustomsLoadPortCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// PortOfFirstArrivalUserControl
			// 
			this.PortOfFirstArrivalUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfFirstArrivalUserControl, ".");
			this.PortOfFirstArrivalUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 152, true);
			this.PortOfFirstArrivalUserControl.Name = "PortOfFirstArrivalUserControl";
			this.PortOfFirstArrivalUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.PortOfFirstArrivalUserControl.TabIndex = 10;
			// 
			// IATALoadPortCodeFindBox
			// 
			this.IATALoadPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IATALoadPortCodeFindBox, "JE_IATALoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_IATALoadPort)));
			this.IATALoadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 102, true);
			this.IATALoadPortCodeFindBox.Name = "IATALoadPortCodeFindBox";
			this.IATALoadPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.IATALoadPortCodeFindBox.ParentType = null;
			this.IATALoadPortCodeFindBox.PreBoundMaxLength = 3;
			this.IATALoadPortCodeFindBox.ShowDescriptionBox = false;
			this.IATALoadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.IATALoadPortCodeFindBox.TabIndex = 9;
			// 
			// PortOfLoadingUserControl
			// 
			this.PortOfLoadingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingUserControl, ".");
			this.PortOfLoadingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 126, true);
			this.PortOfLoadingUserControl.Name = "PortOfLoadingUserControl";
			this.PortOfLoadingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.PortOfLoadingUserControl.TabIndex = 6;
			// 
			// PortOfDischargeUserControl
			// 
			this.PortOfDischargeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeUserControl, ".");
			this.PortOfDischargeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 179, true);
			this.PortOfDischargeUserControl.Name = "PortOfDischargeUserControl";
			this.PortOfDischargeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.PortOfDischargeUserControl.TabIndex = 7;
			// 
			// FlightUserControl
			// 
			this.FlightUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FlightUserControl, ".");
			this.FlightUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 206, true);
			this.FlightUserControl.Name = "FlightUserControl";
			this.FlightUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.FlightUserControl.TabIndex = 8;
			// 
			// VehicleRegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleRegistrationNumberTextBox, "JE_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VoyageFlightNo)));
			this.VehicleRegistrationNumberTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2D9A9130-C8A3-4A23-9022-FEDB31ADF3F9", "Registration");
			this.VehicleRegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 283, true);
			this.VehicleRegistrationNumberTextBox.Name = "VehicleRegistrationNumberTextBox";
			this.VehicleRegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.VehicleRegistrationNumberTextBox.TabIndex = 4;
			// 
			// VoyageNumberTextBox
			// 
			this.VoyageNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.VoyageNumberTextBox, "JE_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VoyageFlightNo)));
			this.VoyageNumberTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7672B557-F8EC-48AD-86EC-4122503B4C29", "Voyage");
			this.VoyageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 102, true);
			this.VoyageNumberTextBox.Name = "VoyageNumberTextBox";
			this.VoyageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.VoyageNumberTextBox.TabIndex = 5;
			// 
			// OverrideValuesCheckBox
			// 
			this.OverrideValuesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideValuesCheckBox, "JE_OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_OverrideFreightDefaults)));
			this.OverrideValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 1, true);
			this.OverrideValuesCheckBox.Name = "OverrideValuesCheckBox";
			this.OverrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 17, true);
			this.OverrideValuesCheckBox.TabIndex = 0;
			this.OverrideValuesCheckBox.UseVisualStyleBackColor = false;
			// 
			// MasterBillTextBox
			// 
			this.MasterBillTextBox.AllowAlphaInMAWP = false;
			this.MasterBillTextBox.AllowDrop = true;
			this.MasterBillTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MasterBillTextBox, "JE_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MasterBill)));
			this.MasterBillTextBox.FormattedMasterBill = "";
			this.MasterBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 24, true);
			this.MasterBillTextBox.Name = "MasterBillTextBox";
			this.MasterBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.MasterBillTextBox.TabIndex = 1;
			// 
			// OceanBillTextBox
			// 
			this.OceanBillTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OceanBillTextBox, "JE_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MasterBill)));
			this.OceanBillTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("738E82C9-5970-4CDC-A7A4-C6E13BDA6C0F", "Ocean Bill");
			this.OceanBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 50, true);
			this.OceanBillTextBox.Name = "OceanBillTextBox";
			this.OceanBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.OceanBillTextBox.TabIndex = 2;
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "JE_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VesselName)));
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 76, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VesselCodeFindBox.ParentType = null;
			this.VesselCodeFindBox.PreBoundMaxLength = 35;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.VesselCodeFindBox.TabIndex = 3;
			// 
			// TransportIDAndNationalityUserControl
			// 
			this.TransportIDAndNationalityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportIDAndNationalityUserControl, ".");
			this.TransportIDAndNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 232, true);
			this.TransportIDAndNationalityUserControl.Name = "TransportIDAndNationalityUserControl";
			this.TransportIDAndNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.TransportIDAndNationalityUserControl.TabIndex = 9;
			// 
			// FlightAndNationalityUserControl
			// 
			this.FlightAndNationalityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FlightAndNationalityUserControl, ".");
			this.FlightAndNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 259, true);
			this.FlightAndNationalityUserControl.Name = "FlightAndNationalityUserControl";
			this.FlightAndNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.FlightAndNationalityUserControl.TabIndex = 10;
			// 
			// InlandModeOfTransportDropEdit
			// 
			this.InlandModeOfTransportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandModeOfTransportDropEdit, "JE_TransportModeInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportModeInland)));
			this.InlandModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 309, true);
			this.InlandModeOfTransportDropEdit.Name = "InlandModeOfTransportDropEdit";
			this.InlandModeOfTransportDropEdit.PreBoundMaxLength = 2;
			this.InlandModeOfTransportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.InlandModeOfTransportDropEdit.TabIndex = 16;
			// 
			// VoyageAndNationalityUserControl
			// 
			this.VoyageAndNationalityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VoyageAndNationalityUserControl, ".");
			this.VoyageAndNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 332, true);
			this.VoyageAndNationalityUserControl.Name = "VoyageAndNationalityUserControl";
			this.VoyageAndNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.VoyageAndNationalityUserControl.TabIndex = 17;
			// 
			// TransportDetailsPortOfLoadingWithIATAUserControl
			// 
			this.TransportDetailsPortOfLoadingWithIATAUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDetailsPortOfLoadingWithIATAUserControl, ".");
			this.TransportDetailsPortOfLoadingWithIATAUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 361, true);
			this.TransportDetailsPortOfLoadingWithIATAUserControl.Name = "TransportDetailsPortOfLoadingWithIATAUserControl";
			this.TransportDetailsPortOfLoadingWithIATAUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 23, true);
			this.TransportDetailsPortOfLoadingWithIATAUserControl.TabIndex = 18;
			// 
			// TransportInlandSeparatorUserControl
			// 
			this.TransportInlandSeparatorUserControl.AllowDrop = true;
			this.TransportInlandSeparatorUserControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("37abf365-b5b6-4ea9-a832-bbf898034d11", "Transport Inland");
			this.TransportInlandSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 503, true);
			this.TransportInlandSeparatorUserControl.Name = "TransportInlandSeparatorUserControl";
			this.TransportInlandSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 15, true);
			this.TransportInlandSeparatorUserControl.TabIndex = 22;
			// 
			// TransportInlandModeAndTypeOfIdUserControl
			// 
			this.TransportInlandModeAndTypeOfIdUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandModeAndTypeOfIdUserControl, ".");
			this.TransportInlandModeAndTypeOfIdUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 523, true);
			this.TransportInlandModeAndTypeOfIdUserControl.Name = "TransportInlandModeAndTypeOfIdUserControl";
			this.TransportInlandModeAndTypeOfIdUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 22, true);
			this.TransportInlandModeAndTypeOfIdUserControl.TabIndex = 23;
			// 
			// TransportInlandIDAndNationalityUserControl
			// 
			this.TransportInlandIDAndNationalityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandIDAndNationalityUserControl, ".");
			this.TransportInlandIDAndNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 523, true);
			this.TransportInlandIDAndNationalityUserControl.Name = "TransportInlandIDAndNationalityUserControl";
			this.TransportInlandIDAndNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 22, true);
			this.TransportInlandIDAndNationalityUserControl.TabIndex = 24;
			// 
			// TransportInlandRoadUserControl
			// 
			this.TransportInlandRoadUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandRoadUserControl, ".");
			this.TransportInlandRoadUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 523, true);
			this.TransportInlandRoadUserControl.Name = "TransportInlandRoadUserControl";
			this.TransportInlandRoadUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 43, true);
			this.TransportInlandRoadUserControl.TabIndex = 25;
			// 
			// TransportInlandAirUserControl
			// 
			this.TransportInlandAirUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandAirUserControl, ".");
			this.TransportInlandAirUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 571, true);
			this.TransportInlandAirUserControl.Name = "TransportInlandAirUserControl";
			this.TransportInlandAirUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 44, true);
			this.TransportInlandAirUserControl.TabIndex = 26;
			// 
			// TransportInlandInlandWaterwaysUserControl
			// 
			this.TransportInlandInlandWaterwaysUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandInlandWaterwaysUserControl, ".");
			this.TransportInlandInlandWaterwaysUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 621, true);
			this.TransportInlandInlandWaterwaysUserControl.Name = "TransportInlandInlandWaterwaysUserControl";
			this.TransportInlandInlandWaterwaysUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 22, true);
			this.TransportInlandInlandWaterwaysUserControl.TabIndex = 27;
			// 
			// TransportInlandOwnPropulsionUserControl
			// 
			this.TransportInlandOwnPropulsionUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandOwnPropulsionUserControl, ".");
			this.TransportInlandOwnPropulsionUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 648, true);
			this.TransportInlandOwnPropulsionUserControl.Name = "TransportInlandOwnPropulsionUserControl";
			this.TransportInlandOwnPropulsionUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 44, true);
			this.TransportInlandOwnPropulsionUserControl.TabIndex = 28;
			// 
			// TransportInlandRailUserControl
			// 
			this.TransportInlandRailUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandRailUserControl, ".");
			this.TransportInlandRailUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 697, true);
			this.TransportInlandRailUserControl.Name = "TransportInlandRailUserControl";
			this.TransportInlandRailUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 44, true);
			this.TransportInlandRailUserControl.TabIndex = 29;
			// 
			// TransportInlandSeaUserControl
			// 
			this.TransportInlandSeaUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandSeaUserControl, ".");
			this.TransportInlandSeaUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 746, true);
			this.TransportInlandSeaUserControl.Name = "TransportInlandSeaUserControl";
			this.TransportInlandSeaUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.TransportInlandSeaUserControl.TabIndex = 30;
			// 
			// GoodsDestinationCountryCodeFindBox
			// 
			this.GoodsDestinationCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsDestinationCountryCodeFindBox, "JE_GoodsDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_GoodsDestination)));
			this.GoodsDestinationCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 390, true);
			this.GoodsDestinationCountryCodeFindBox.Name = "GoodsDestinationCountryCodeFindBox";
			this.GoodsDestinationCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsDestinationCountryCodeFindBox.ParentType = null;
			this.GoodsDestinationCountryCodeFindBox.PreBoundMaxLength = 2;
			this.GoodsDestinationCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.GoodsDestinationCountryCodeFindBox.TabIndex = 19;
			// 
			// UCRTextBox
			// 
			this.BindingSource.SetBindingMember(this.UCRTextBox, "JE_UCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_UCR)));
			this.UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 416, true);
			this.UCRTextBox.Name = "UCRTextBox";
			this.UCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.UCRTextBox.TabIndex = 20;
			// 
			// SubLocationOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubLocationOfGoodsTextBox, "JE_SubLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_SubLocationOfGoods)));
			this.SubLocationOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 442, true);
			this.SubLocationOfGoodsTextBox.Name = "SubLocationOfGoodsTextBox";
			this.SubLocationOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.SubLocationOfGoodsTextBox.TabIndex = 21;
			// 
			// TransportMeansDropEdit
			// 
			this.TransportMeansDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportMeansDropEdit, "JE_TransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportMeans)));
			this.TransportMeansDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 469, true);
			this.TransportMeansDropEdit.Name = "TransportMeansDropEdit";
			this.TransportMeansDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.TransportMeansDropEdit.TabIndex = 31;
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 785, true);
			this.CustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeCodeFindBox.ParentType = null;
			this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 35;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 3;
			// 
			// CustomsLoadPortCodeFindBox
			// 
			this.CustomsLoadPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsLoadPortCodeFindBox, "JE_CustomsLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_CustomsLoadPort)));
			this.CustomsLoadPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 823, true);
			this.CustomsLoadPortCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsLoadPortCodeFindBox.Name = "CustomsLoadPortCodeFindBox";
			this.CustomsLoadPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsLoadPortCodeFindBox.ParentType = null;
			this.CustomsLoadPortCodeFindBox.PreBoundMaxLength = 10;
			this.CustomsLoadPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.CustomsLoadPortCodeFindBox.TabIndex = 32;
			// 
			// TransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsLoadPortCodeFindBox);
			this.Controls.Add(this.TransportMeansDropEdit);
			this.Controls.Add(this.TransportInlandSeaUserControl);
			this.Controls.Add(this.TransportInlandRailUserControl);
			this.Controls.Add(this.TransportInlandOwnPropulsionUserControl);
			this.Controls.Add(this.TransportInlandInlandWaterwaysUserControl);
			this.Controls.Add(this.TransportInlandAirUserControl);
			this.Controls.Add(this.TransportInlandRoadUserControl);
			this.Controls.Add(this.TransportInlandSeparatorUserControl);
			this.Controls.Add(this.TransportInlandModeAndTypeOfIdUserControl);
			this.Controls.Add(this.TransportInlandIDAndNationalityUserControl);
			this.Controls.Add(this.TransportDetailsPortOfLoadingWithIATAUserControl);
			this.Controls.Add(this.VoyageAndNationalityUserControl);
			this.Controls.Add(this.FlightUserControl);
			this.Controls.Add(this.VehicleRegistrationNumberTextBox);
			this.Controls.Add(this.VoyageNumberTextBox);
			this.Controls.Add(this.PortOfLoadingUserControl);
			this.Controls.Add(this.PortOfFirstArrivalUserControl);
			this.Controls.Add(this.OverrideValuesCheckBox);
			this.Controls.Add(this.MasterBillTextBox);
			this.Controls.Add(this.OceanBillTextBox);
			this.Controls.Add(this.VesselCodeFindBox);
			this.Controls.Add(this.TransportIDAndNationalityUserControl);
			this.Controls.Add(this.FlightAndNationalityUserControl);
			this.Controls.Add(this.PortOfDischargeUserControl);
			this.Controls.Add(this.IATALoadPortCodeFindBox);
			this.Controls.Add(this.InlandModeOfTransportDropEdit);
			this.Controls.Add(this.GoodsDestinationCountryCodeFindBox);
			this.Controls.Add(this.UCRTextBox);
			this.Controls.Add(this.SubLocationOfGoodsTextBox);
			this.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 906, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortOfFirstArrivalUserControl.ResumeLayout(true);
			this.PortOfFirstArrivalUserControl.PerformLayout();
			this.IATALoadPortCodeFindBox.ResumeLayout(true);
			this.IATALoadPortCodeFindBox.PerformLayout();
			this.PortOfLoadingUserControl.ResumeLayout(true);
			this.PortOfLoadingUserControl.PerformLayout();
			this.PortOfDischargeUserControl.ResumeLayout(true);
			this.PortOfDischargeUserControl.PerformLayout();
			this.FlightUserControl.ResumeLayout(true);
			this.FlightUserControl.PerformLayout();
			this.MasterBillTextBox.ResumeLayout(true);
			this.MasterBillTextBox.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.TransportIDAndNationalityUserControl.ResumeLayout(true);
			this.TransportIDAndNationalityUserControl.PerformLayout();
			this.FlightAndNationalityUserControl.ResumeLayout(true);
			this.FlightAndNationalityUserControl.PerformLayout();
			this.InlandModeOfTransportDropEdit.ResumeLayout(true);
			this.InlandModeOfTransportDropEdit.PerformLayout();
			this.VoyageAndNationalityUserControl.ResumeLayout(true);
			this.VoyageAndNationalityUserControl.PerformLayout();
			this.TransportDetailsPortOfLoadingWithIATAUserControl.ResumeLayout(true);
			this.TransportDetailsPortOfLoadingWithIATAUserControl.PerformLayout();
			this.TransportInlandSeparatorUserControl.ResumeLayout(true);
			this.TransportInlandSeparatorUserControl.PerformLayout();
			this.TransportInlandModeAndTypeOfIdUserControl.ResumeLayout(true);
			this.TransportInlandModeAndTypeOfIdUserControl.PerformLayout();
			this.TransportInlandIDAndNationalityUserControl.ResumeLayout(true);
			this.TransportInlandIDAndNationalityUserControl.PerformLayout();
			this.TransportInlandRoadUserControl.ResumeLayout(true);
			this.TransportInlandRoadUserControl.PerformLayout();
			this.TransportInlandAirUserControl.ResumeLayout(true);
			this.TransportInlandAirUserControl.PerformLayout();
			this.TransportInlandInlandWaterwaysUserControl.ResumeLayout(true);
			this.TransportInlandInlandWaterwaysUserControl.PerformLayout();
			this.TransportInlandOwnPropulsionUserControl.ResumeLayout(true);
			this.TransportInlandOwnPropulsionUserControl.PerformLayout();
			this.TransportInlandRailUserControl.ResumeLayout(true);
			this.TransportInlandRailUserControl.PerformLayout();
			this.TransportInlandSeaUserControl.ResumeLayout(true);
			this.TransportInlandSeaUserControl.PerformLayout();
			this.GoodsDestinationCountryCodeFindBox.ResumeLayout(true);
			this.GoodsDestinationCountryCodeFindBox.PerformLayout();
			this.TransportMeansDropEdit.ResumeLayout(true);
			this.TransportMeansDropEdit.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.CustomsLoadPortCodeFindBox.ResumeLayout(true);
			this.CustomsLoadPortCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCheckBox OverrideValuesCheckBox;
		internal Enterprise.ZArchitecture.ZMasterBillControl MasterBillTextBox;
		internal Enterprise.ZArchitecture.ZTextBox OceanBillTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox VehicleRegistrationNumberTextBox;
		internal Enterprise.ZArchitecture.ZTextBox VoyageNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox IATALoadPortCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit InlandModeOfTransportDropEdit;
		internal TransportDetailsPortOfLoadingUserControl PortOfLoadingUserControl;
		internal TransportDetailsPortOfDischargeUserControl PortOfDischargeUserControl;
		internal TransportDetailsFlightUserControl FlightUserControl;
		internal TransportIDAndNationalityUserControl TransportIDAndNationalityUserControl;
		internal FlightAndNationalityUserControl FlightAndNationalityUserControl;
		internal TransportDetailsPortOfFirstArrivalUserControl PortOfFirstArrivalUserControl;
		internal VoyageAndNationalityUserControl VoyageAndNationalityUserControl;
		internal TransportDetailsPortOfLoadingWithIATAUserControl TransportDetailsPortOfLoadingWithIATAUserControl;
		internal ZArchitecture.GUI.SeparatorUserControl TransportInlandSeparatorUserControl;
		internal TransportInlandModeAndTypeOfIdUserControl TransportInlandModeAndTypeOfIdUserControl;
		internal TransportInlandIDAndNationalityUserControl TransportInlandIDAndNationalityUserControl;
		internal TransportInlandRoadUserControl TransportInlandRoadUserControl;
		internal TransportInlandAirUserControl TransportInlandAirUserControl;
		internal TransportInlandInlandWaterwaysUserControl TransportInlandInlandWaterwaysUserControl;
		internal TransportInlandOwnPropulsionUserControl TransportInlandOwnPropulsionUserControl;
		internal TransportInlandRailUserControl TransportInlandRailUserControl;
		internal TransportInlandSeaUserControl TransportInlandSeaUserControl;
		internal Enterprise.ZArchitecture.ZTextBox UCRTextBox;
		internal Enterprise.ZArchitecture.ZTextBox SubLocationOfGoodsTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox GoodsDestinationCountryCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit TransportMeansDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox CustomsLoadPortCodeFindBox;
	}
}
