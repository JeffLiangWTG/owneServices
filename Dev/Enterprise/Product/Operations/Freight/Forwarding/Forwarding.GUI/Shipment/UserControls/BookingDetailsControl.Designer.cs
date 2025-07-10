using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class BookingDetailsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SailingSummarygroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LoadingPortCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DischargePortCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SailingTotalVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DEPBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ARVBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VesselBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VoyageNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BookedShippingLineBoundOrgFindBox = new MasterFiles.GUI.ZOrganisationFindBox();
			this.SailingTotalWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.BookingRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BookingContainersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SailingSummarygroupBox.SuspendLayout();
			this.SailingTotalVolumeCalcDropEdit.SuspendLayout();
			this.DEPBoundReadOnlyDateEdit.SuspendLayout();
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.SuspendLayout();
			this.ARVBoundReadOnlyDateEdit.SuspendLayout();
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.SuspendLayout();
			this.BookedShippingLineBoundOrgFindBox.SuspendLayout();
			this.SailingTotalWeightCalcDropEdit.SuspendLayout();
			this.DetailsPanel.SuspendLayout();
			this.BookingContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// SailingSummarygroupBox
			// 
			this.SailingSummarygroupBox.Controls.Add(this.LoadingPortCodeTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.DischargePortCodeTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.SailingTotalVolumeCalcDropEdit);
			this.SailingSummarygroupBox.Controls.Add(this.DEPBoundReadOnlyDateEdit);
			this.SailingSummarygroupBox.Controls.Add(this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit);
			this.SailingSummarygroupBox.Controls.Add(this.ARVBoundReadOnlyDateEdit);
			this.SailingSummarygroupBox.Controls.Add(this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit);
			this.SailingSummarygroupBox.Controls.Add(this.VesselBoundTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.VoyageNumberBoundTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.BookedShippingLineBoundOrgFindBox);
			this.SailingSummarygroupBox.Controls.Add(this.SailingTotalWeightCalcDropEdit);
			this.SailingSummarygroupBox.Controls.Add(this.BookingRefTextBox);
			this.SailingSummarygroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SailingSummarygroupBox.Name = "SailingSummarygroupBox";
			this.SailingSummarygroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 128, true);
			this.SailingSummarygroupBox.TabIndex = 12;
			this.SailingSummarygroupBox.TabStop = false;
			// 
			// LoadingPortCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoadingPortCodeTextBox, "Sailing.JX_JA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.JX_JA_RL_NKPortOfLoading)));
			this.LoadingPortCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("9bc602bb-bee4-437d-b2ca-cf4f54873999", "Load");
			this.LoadingPortCodeTextBox.Enabled = false;
			this.LoadingPortCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 16, true);
			this.LoadingPortCodeTextBox.Name = "LoadingPortCodeTextBox";
			this.LoadingPortCodeTextBox.ReadOnly = true;
			this.LoadingPortCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.LoadingPortCodeTextBox.TabIndex = 0;
			this.LoadingPortCodeTextBox.TabStop = false;
			// 
			// DischargePortCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.DischargePortCodeTextBox, "Sailing.JX_JB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.JX_JB_RL_NKPortOfDischarge)));
			this.DischargePortCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("866416c2-6266-4145-98ed-af61c534ae49", "Discharge");
			this.DischargePortCodeTextBox.Enabled = false;
			this.DischargePortCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 16, true);
			this.DischargePortCodeTextBox.Name = "DischargePortCodeTextBox";
			this.DischargePortCodeTextBox.ReadOnly = true;
			this.DischargePortCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.DischargePortCodeTextBox.TabIndex = 11;
			this.DischargePortCodeTextBox.TabStop = false;
			// 
			// SailingTotalVolumeCalcDropEdit
			// 
			this.SailingTotalVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SailingTotalVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.TotalVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.TotalVolumeUnit)));
			this.SailingTotalVolumeCalcDropEdit.BindToAmount = "Sailing+TotalVolume";
			this.SailingTotalVolumeCalcDropEdit.BindToUnit = "Sailing+TotalVolumeUnit";
			this.SailingTotalVolumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("62a50cbc-66db-486d-9752-d5b707f02739", "Total Current Volume");
			this.SailingTotalVolumeCalcDropEdit.Decimals = 3;
			this.SailingTotalVolumeCalcDropEdit.Enabled = false;
			this.SailingTotalVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 104, true);
			this.SailingTotalVolumeCalcDropEdit.Name = "SailingTotalVolumeCalcDropEdit";
			this.SailingTotalVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.SailingTotalVolumeCalcDropEdit.TabIndex = 9;
			this.SailingTotalVolumeCalcDropEdit.UnitPreBoundMaxLength = 3;
			this.SailingTotalVolumeCalcDropEdit.Visible = false;
			// 
			// DEPBoundReadOnlyDateEdit
			// 
			this.DEPBoundReadOnlyDateEdit.AllowDrop = true;
			this.DEPBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.DEPBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DEPBoundReadOnlyDateEdit, "Sailing+JX_JA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.JX_JA_E_DEP)));
			this.DEPBoundReadOnlyDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2cefa68c-cc17-43e3-86b8-a41c345d394a", "Estimated Departure Date");
			this.DEPBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DEPBoundReadOnlyDateEdit.Enabled = false;
			this.DEPBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 82, true);
			this.DEPBoundReadOnlyDateEdit.Name = "DEPBoundReadOnlyDateEdit";
			this.DEPBoundReadOnlyDateEdit.TabIndex = 4;
			this.DEPBoundReadOnlyDateEdit.TabStop = false;
			// 
			// JS_Calc_FCLCutOffBoundReadOnlyDateEdit
			// 
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.AllowDrop = true;
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit, "Sailing+JX_JA_CTOCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.JX_JA_CTOCutOff)));
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("0f1e3e8d-3492-4643-9b68-b37cccf6d64e", "CTO Cut Off");
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.Enabled = false;
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 60, true);
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.Name = "JS_Calc_FCLCutOffBoundReadOnlyDateEdit";
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.TabIndex = 3;
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.TabStop = false;
			// 
			// ARVBoundReadOnlyDateEdit
			// 
			this.ARVBoundReadOnlyDateEdit.AllowDrop = true;
			this.ARVBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.ARVBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ARVBoundReadOnlyDateEdit, "Sailing+JX_JB_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.JX_JB_E_ARV)));
			this.ARVBoundReadOnlyDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3092e70b-2eec-4574-b2b9-4678a05715f9", "Estimated Arrival Date");
			this.ARVBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ARVBoundReadOnlyDateEdit.Enabled = false;
			this.ARVBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 82, true);
			this.ARVBoundReadOnlyDateEdit.Name = "ARVBoundReadOnlyDateEdit";
			this.ARVBoundReadOnlyDateEdit.TabIndex = 5;
			this.ARVBoundReadOnlyDateEdit.TabStop = false;
			// 
			// JS_Calc_LCLCutOffBoundReadOnlyDateEdit
			// 
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.AllowDrop = true;
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit, "Sailing+JX_DepotCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.JX_DepotCutOff)));
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("6aa2252d-3d6d-4cd8-aaa1-aca2b5efd70c", "CFS Cut Off");
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.Enabled = false;
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 60, true);
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.Name = "JS_Calc_LCLCutOffBoundReadOnlyDateEdit";
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.TabIndex = 2;
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.TabStop = false;
			// 
			// VesselBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.VesselBoundTextBox, "Sailing.JX_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.JX_JV_NKVessel)));
			this.VesselBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("7c1f6a77-4c68-435f-8e0e-dbd8684ef557", "Vessel");
			this.VesselBoundTextBox.Enabled = false;
			this.VesselBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 38, true);
			this.VesselBoundTextBox.Name = "VesselBoundTextBox";
			this.VesselBoundTextBox.ReadOnly = true;
			this.VesselBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
			this.VesselBoundTextBox.TabIndex = 1;
			this.VesselBoundTextBox.TabStop = false;
			// 
			// VoyageNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageNumberBoundTextBox, "Sailing.JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.JX_JV_VoyageFlight)));
			this.VoyageNumberBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("143676aa-09d1-45be-8c6c-309a057c6a4c", "Voyage No");
			this.VoyageNumberBoundTextBox.Enabled = false;
			this.VoyageNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 38, true);
			this.VoyageNumberBoundTextBox.Name = "VoyageNumberBoundTextBox";
			this.VoyageNumberBoundTextBox.ReadOnly = true;
			this.VoyageNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.VoyageNumberBoundTextBox.TabIndex = 0;
			this.VoyageNumberBoundTextBox.TabStop = false;
			// 
			// BookedShippingLineBoundOrgFindBox
			// 
			this.BookedShippingLineBoundOrgFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BookedShippingLineBoundOrgFindBox, "BookedShippingLinePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookedShippingLinePK)));
			this.BookedShippingLineBoundOrgFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("71a9c384-fadf-41b3-b637-b7397f5c884b", "Carrier:");
			this.BookedShippingLineBoundOrgFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.BookedShippingLineBoundOrgFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 104, true);
			this.BookedShippingLineBoundOrgFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.BookedShippingLineBoundOrgFindBox.Name = "BookedShippingLineBoundOrgFindBox";
			this.BookedShippingLineBoundOrgFindBox.ShouldResize = true;
			this.BookedShippingLineBoundOrgFindBox.ShowDescriptionBox = false;
			this.BookedShippingLineBoundOrgFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.BookedShippingLineBoundOrgFindBox.TabIndex = 6;
			this.BookedShippingLineBoundOrgFindBox.Visible = false;
			// 
			// SailingTotalWeightCalcDropEdit
			// 
			this.SailingTotalWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SailingTotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Sailing.TotalWeightUnit)));
			this.SailingTotalWeightCalcDropEdit.BindToAmount = "Sailing+TotalWeight";
			this.SailingTotalWeightCalcDropEdit.BindToUnit = "Sailing+TotalWeightUnit";
			this.SailingTotalWeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("87805b99-a061-4b40-b75d-448ce222d164", "Total Current Weight");
			this.SailingTotalWeightCalcDropEdit.Decimals = 3;
			this.SailingTotalWeightCalcDropEdit.Enabled = false;
			this.SailingTotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 104, true);
			this.SailingTotalWeightCalcDropEdit.Name = "SailingTotalWeightCalcDropEdit";
			this.SailingTotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.SailingTotalWeightCalcDropEdit.TabIndex = 8;
			this.SailingTotalWeightCalcDropEdit.TabStop = false;
			this.SailingTotalWeightCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// BookingRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingRefTextBox, "JS_CFSReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_CFSReference)));
			this.BookingRefTextBox.CaptionResourceString = null;
			this.BookingRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 104, true);
			this.BookingRefTextBox.Name = "BookingRefTextBox";
			this.BookingRefTextBox.ReadOnly = true;
			this.BookingRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
			this.BookingRefTextBox.TabIndex = 7;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.Controls.Add(this.SailingSummarygroupBox);
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 140, true);
			this.DetailsPanel.TabIndex = 10;
			// 
			// BookingContainersGroupBox
			// 
			this.BookingContainersGroupBox.Controls.Add(this.ContainersGrid);
			this.BookingContainersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BookingContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 140, true);
			this.BookingContainersGroupBox.Name = "BookingContainersGroupBox";
			this.BookingContainersGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.BookingContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 362, true);
			this.BookingContainersGroupBox.TabIndex = 13;
			this.BookingContainersGroupBox.TabStop = false;
			this.BookingContainersGroupBox.Text = "Containers";
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "BookingContainersForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_ContainerCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_RC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_RH_NKContainerCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_ReleaseNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_Calc_DepartureContainerYardAddressOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_Calc_DepartureContainerYardAddressCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_Calc_ArrivalContainerYardAddressOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_Calc_ArrivalContainerYardAddressCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_DepartureEstimatedPickup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_DepartureSlotDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_DepartureSlotReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_EmptyRequired)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_SetPointTemp)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_SetPointTempUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_AirVentFlow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_AirVentFlowRateUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).BookingContainersForBinding)).SyncRoot)).JC_TempRecorderSerialNo)));
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JC_ContainerNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JC_ContainerCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JC_RC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JC_RH_NKContainerCommodityCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|58b840cc-c007-432d-bb3e-fdf293272cb7", "Release");
			zTextBoxColumnStyleInfo2.ColumnName = "JC_ReleaseNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|88bca8ba-0de6-4b29-b771-b8df9d4a7404", "Dep. Container Yard", "Departure Container Yard");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JC_Calc_DepartureContainerYardAddressOrg";
			zGuidFindBoxColumnStyleInfo2.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|b9a813dc-dfb8-4956-a6a7-5845b6646a45", "Dep. Container Yard");
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|15a65e17-a8bc-4929-aeb4-19adc286e1b3", "Dep. Container Yard", "Departure Container Yard");
			zDropEditColumnStyleInfo1.ColumnName = "JC_Calc_DepartureContainerYardAddressCode";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|b9a813dc-dfb8-4956-a6a7-5845b6646a45", "Dep. Container Yard");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|35fa0489-cf45-4871-8661-6124d2f89454", "Arr. Container Yard");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "JC_Calc_ArrivalContainerYardAddressOrg";
			zGuidFindBoxColumnStyleInfo3.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|268f9b56-bc02-49d4-94be-dd796761fca1", "Arr. Container Yard");
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|e714e1e9-9c37-4edc-a5c0-cc76aba25b45", "Arr. Container Yard");
			zDropEditColumnStyleInfo2.ColumnName = "JC_Calc_ArrivalContainerYardAddressCode";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|268f9b56-bc02-49d4-94be-dd796761fca1", "Arr. Container Yard");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|27b9465e-81e3-4b63-b43e-7d22c918d27c", "Full Pickup");
			zDateEditColumnStyleInfo1.ColumnName = "JC_DepartureEstimatedPickup";
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|88cd83d6-0bea-48e4-b97b-64b8588657a6", "Mode");
			zDropEditColumnStyleInfo3.ColumnName = "JC_ContainerMode";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo2.ColumnName = "JC_DepartureSlotDateTime";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|53811535-7381-48c7-9bae-f6a0bdae5b87", "Dep. Slot Ref.", "Departure Slot Ref.");
			zTextBoxColumnStyleInfo3.ColumnName = "JC_DepartureSlotReference";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|78d42a9a-370a-4e25-a8f9-734e86b63178", "Empty Required");
			zDateEditColumnStyleInfo3.ColumnName = "JC_EmptyRequired";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|73ed4bbe-b05d-45e5-a12d-9b99c2312173", "Set Point Temp.");
			zCalcEditColumnStyleInfo2.ColumnName = "JC_SetPointTemp";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|653e3f5c-1bfd-4a20-9b27-f354f1a4a1a1", "Set Point Temp.");
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "JC_SetPointTempUnit";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|653e3f5c-1bfd-4a20-9b27-f354f1a4a1a1", "Set Point Temp.");
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|59ec7b49-a76f-4878-8c9c-029c635bbd55", "Air Vent");
			zCalcEditColumnStyleInfo3.ColumnName = "JC_AirVentFlow";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|46793f67-c473-4942-b37e-972f4ba3879a", "Air Vent");
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|a4edd4c0-37ee-42fa-beb1-11cba9292b7e", "Unit");
			zDropEditColumnStyleInfo5.ColumnName = "JC_AirVentFlowRateUnit";
			zDropEditColumnStyleInfo5.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("BookingDetailsControl|46793f67-c473-4942-b37e-972f4ba3879a", "Air Vent");
			zDropEditColumnStyleInfo5.IsVisible = false;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo4.ColumnName = "JC_TempRecorderSerialNo";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.GridId = "dfd3a3ca-ad49-4873-8c3b-61c2a01fcd3c";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "ContainersGrid";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 16, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.ReadOnly = true;
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 342, true);
			this.ContainersGrid.TabIndex = 11;
			// 
			// BookingDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BookingContainersGroupBox);
			this.Controls.Add(this.DetailsPanel);
			this.Name = "BookingDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 502, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SailingSummarygroupBox.ResumeLayout(false);
			this.SailingSummarygroupBox.PerformLayout();
			this.SailingTotalVolumeCalcDropEdit.ResumeLayout(true);
			this.SailingTotalVolumeCalcDropEdit.PerformLayout();
			this.DEPBoundReadOnlyDateEdit.ResumeLayout(true);
			this.DEPBoundReadOnlyDateEdit.PerformLayout();
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.ResumeLayout(true);
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.PerformLayout();
			this.ARVBoundReadOnlyDateEdit.ResumeLayout(true);
			this.ARVBoundReadOnlyDateEdit.PerformLayout();
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.ResumeLayout(true);
			this.JS_Calc_LCLCutOffBoundReadOnlyDateEdit.PerformLayout();
			this.BookedShippingLineBoundOrgFindBox.ResumeLayout(true);
			this.BookedShippingLineBoundOrgFindBox.PerformLayout();
			this.SailingTotalWeightCalcDropEdit.ResumeLayout(true);
			this.SailingTotalWeightCalcDropEdit.PerformLayout();
			this.DetailsPanel.ResumeLayout(false);
			this.DetailsPanel.PerformLayout();
			this.BookingContainersGroupBox.ResumeLayout(false);
			this.BookingContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainersGrid.ResumeLayout(false);
			this.ContainersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SailingSummarygroupBox;
		private ZArchitecture.GUI.ZCalcDropEdit SailingTotalVolumeCalcDropEdit;
		private ZArchitecture.GUI.ZDateEdit DEPBoundReadOnlyDateEdit;
		private ZArchitecture.GUI.ZDateEdit JS_Calc_FCLCutOffBoundReadOnlyDateEdit;
		private ZArchitecture.GUI.ZDateEdit ARVBoundReadOnlyDateEdit;
		private ZArchitecture.GUI.ZDateEdit JS_Calc_LCLCutOffBoundReadOnlyDateEdit;
		private ZArchitecture.ZTextBox VesselBoundTextBox;
		private ZArchitecture.ZTextBox VoyageNumberBoundTextBox;
		private MasterFiles.GUI.ZOrganisationFindBox BookedShippingLineBoundOrgFindBox;
		private ZArchitecture.GUI.ZCalcDropEdit SailingTotalWeightCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel DetailsPanel;
		private ZArchitecture.ZTextBox LoadingPortCodeTextBox;
		private ZArchitecture.ZTextBox DischargePortCodeTextBox;
		private ZArchitecture.ZTextBox BookingRefTextBox;
		private ZArchitecture.GUI.ZGroupBox BookingContainersGroupBox;
		private ZArchitecture.ZGrid ContainersGrid;
	}
}
