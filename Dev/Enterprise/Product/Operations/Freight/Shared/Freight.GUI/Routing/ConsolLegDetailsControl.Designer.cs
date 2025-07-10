using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.GUI
{
	partial class ConsoLegDetailsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TransportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsDomesticCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsCargoOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JW_ATDBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_ATABoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_RL_NKDiscPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JW_ETABoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JW_IsCharterBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JW_ETDBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JW_RL_NKLoadPortBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RoadPanel = new CargoWise.Windows.UI.KPanel();
			this.JW_VesselBoundTextBox_Road = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_VoyageFlightBoundTextBox_Road = new Enterprise.ZArchitecture.ZTextBox();
			this.RailPanel = new CargoWise.Windows.UI.KPanel();
			this.JW_VesselBoundTextBox_Rail = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_VoyageFlightBoundTextBox_Rail = new Enterprise.ZArchitecture.ZTextBox();
			this.SeaPanel = new CargoWise.Windows.UI.KPanel();
			this.JW_VesselBoundTextBox_Sea = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JW_VoyageFlightBoundTextBox_Sea = new Enterprise.ZArchitecture.ZTextBox();
			this.AirPanel = new CargoWise.Windows.UI.KPanel();
			this.JW_JX_JV_RegistrationNoBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_AircraftTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JW_VoyageFlightBoundTextBox_Air = new Enterprise.ZArchitecture.ZTextBox();
			this.FlightStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportGroupBox.SuspendLayout();
			this.RoadPanel.SuspendLayout();
			this.RailPanel.SuspendLayout();
			this.SeaPanel.SuspendLayout();
			this.AirPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommonConsol);
			// 
			// TransportGroupBox
			// 
			this.TransportGroupBox.Controls.Add(this.IsDomesticCheckBox);
			this.TransportGroupBox.Controls.Add(this.IsCargoOnlyCheckBox);
			this.TransportGroupBox.Controls.Add(this.FlightStatusLabel);
			this.TransportGroupBox.Controls.Add(this.WarningLabel);
			this.TransportGroupBox.Controls.Add(this.JW_ATDBoundDateEdit);
			this.TransportGroupBox.Controls.Add(this.JW_ATABoundDateEdit);
			this.TransportGroupBox.Controls.Add(this.JW_RL_NKDiscPortBoundCodeFindBox);
			this.TransportGroupBox.Controls.Add(this.JW_ETABoundDateEdit);
			this.TransportGroupBox.Controls.Add(this.zCheckBox1);
			this.TransportGroupBox.Controls.Add(this.JW_IsCharterBoundCheckBox);
			this.TransportGroupBox.Controls.Add(this.JW_ETDBoundDateEdit);
			this.TransportGroupBox.Controls.Add(this.JW_RL_NKLoadPortBoundCodeFindBox);
			this.TransportGroupBox.Controls.Add(this.RoadPanel);
			this.TransportGroupBox.Controls.Add(this.RailPanel);
			this.TransportGroupBox.Controls.Add(this.SeaPanel);
			this.TransportGroupBox.Controls.Add(this.AirPanel);
			this.TransportGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportGroupBox.Name = "TransportGroupBox";
			this.TransportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 115, true);
			this.TransportGroupBox.TabIndex = 0;
			this.TransportGroupBox.TabStop = false;
			// 
			// IsDomesticCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsDomesticCheckBox, "MostInterestingTransportForBinding.IsDomestic");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).IsDomestic)));
			this.IsDomesticCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ConsoLegDetailsControl|8e7a0138-af37-4c79-bf40-90fc0a06f128", "Is Domestic");
			this.IsDomesticCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDomesticCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 88, true);
			this.IsDomesticCheckBox.Name = "IsDomesticCheckBox";
			this.IsDomesticCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.IsDomesticCheckBox.TabIndex = 8;
			// 
			// IsCargoOnlyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsCargoOnlyCheckBox, "MostInterestingTransportForBinding.JW_IsCargoOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_IsCargoOnly)));
			this.IsCargoOnlyCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ConsoLegDetailsControl|bd82d2dc-4def-4482-ba44-a2bfaa935b9b", "Is Cargo Only");
			this.IsCargoOnlyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsCargoOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 90, true);
			this.IsCargoOnlyCheckBox.Name = "IsCargoOnlyCheckBox";
			this.IsCargoOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 17, true);
			this.IsCargoOnlyCheckBox.TabIndex = 11;
			this.IsCargoOnlyCheckBox.UseVisualStyleBackColor = true;
			// 
			// WarningLabel
			// 
			this.WarningLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ConsoLegDetailsControl|97e23f06-a93a-4f91-b162-64fd642f8c9c", "See the routing tab for the other legs.");
			this.WarningLabel.ForeColor = System.Drawing.Color.Red;
			this.WarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 0, true);
			this.WarningLabel.Name = "WarningLabel";
			this.WarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 16, true);
			this.WarningLabel.TabIndex = 0;
			// 
			// JW_ATDBoundDateEdit
			// 
			this.JW_ATDBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_ATDBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_ATDBoundDateEdit, "MostInterestingTransportForBinding.JW_ATDForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_ATDForBinding)));
			this.JW_ATDBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_ATDBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 43, true);
			this.JW_ATDBoundDateEdit.Name = "JW_ATDBoundDateEdit";
			this.JW_ATDBoundDateEdit.TabIndex = 6;
			// 
			// JW_ATABoundDateEdit
			// 
			this.JW_ATABoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_ATABoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_ATABoundDateEdit, "MostInterestingTransportForBinding.JW_ATAForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_ATAForBinding)));
			this.JW_ATABoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_ATABoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 66, true);
			this.JW_ATABoundDateEdit.Name = "JW_ATABoundDateEdit";
			this.JW_ATABoundDateEdit.TabIndex = 7;
			// 
			// JW_RL_NKDiscPortBoundCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.JW_RL_NKDiscPortBoundCodeFindBox, "MostInterestingTransportForBinding.JW_RL_NKDiscPortForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_RL_NKDiscPortForBinding)));
			this.JW_RL_NKDiscPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 66, true);
			this.JW_RL_NKDiscPortBoundCodeFindBox.Name = "JW_RL_NKDiscPortBoundCodeFindBox";
			this.JW_RL_NKDiscPortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JW_RL_NKDiscPortBoundCodeFindBox.ShowDescriptionBox = false;
			this.JW_RL_NKDiscPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JW_RL_NKDiscPortBoundCodeFindBox.TabIndex = 3;
			// 
			// JW_ETABoundDateEdit
			// 
			this.JW_ETABoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_ETABoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_ETABoundDateEdit, "MostInterestingTransportForBinding.JW_ETAForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_ETAForBinding)));
			this.JW_ETABoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_ETABoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 66, true);
			this.JW_ETABoundDateEdit.Name = "JW_ETABoundDateEdit";
			this.JW_ETABoundDateEdit.TabIndex = 5;
			// 
			// zCheckBox1
			// 
			this.BindingSource.SetBindingMember(this.zCheckBox1, "MostInterestingTransportForBinding.JW_IsLinked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_IsLinked)));
			this.zCheckBox1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ConsoLegDetailsControl|4fb2e119-5e93-4291-a60e-a6af0f4eb7cc", "Is Linked");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 88, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.zCheckBox1.TabIndex = 9;
			// 
			// JW_IsCharterBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.JW_IsCharterBoundCheckBox, "MostInterestingTransportForBinding.JW_IsCharterForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_IsCharterForBinding)));
			this.JW_IsCharterBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JW_IsCharterBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 88, true);
			this.JW_IsCharterBoundCheckBox.Name = "JW_IsCharterBoundCheckBox";
			this.JW_IsCharterBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.JW_IsCharterBoundCheckBox.TabIndex = 10;
			// 
			// JW_ETDBoundDateEdit
			// 
			this.JW_ETDBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JW_ETDBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JW_ETDBoundDateEdit, "MostInterestingTransportForBinding.JW_ETDForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_ETDForBinding)));
			this.JW_ETDBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JW_ETDBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 43, true);
			this.JW_ETDBoundDateEdit.Name = "JW_ETDBoundDateEdit";
			this.JW_ETDBoundDateEdit.TabIndex = 4;
			// 
			// JW_RL_NKLoadPortBoundCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.JW_RL_NKLoadPortBoundCodeFindBox, "MostInterestingTransportForBinding.JW_RL_NKLoadPortForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_RL_NKLoadPortForBinding)));
			this.JW_RL_NKLoadPortBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 43, true);
			this.JW_RL_NKLoadPortBoundCodeFindBox.Name = "JW_RL_NKLoadPortBoundCodeFindBox";
			this.JW_RL_NKLoadPortBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JW_RL_NKLoadPortBoundCodeFindBox.ShowDescriptionBox = false;
			this.JW_RL_NKLoadPortBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JW_RL_NKLoadPortBoundCodeFindBox.TabIndex = 2;
			// 
			// RoadPanel
			// 
			this.RoadPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RoadPanel.Controls.Add(this.JW_VesselBoundTextBox_Road);
			this.RoadPanel.Controls.Add(this.JW_VoyageFlightBoundTextBox_Road);
			this.RoadPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.RoadPanel.Name = "RoadPanel";
			this.RoadPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 26, true);
			this.RoadPanel.TabIndex = 0;
			this.RoadPanel.Visible = false;
			// 
			// JW_VesselBoundTextBox_Road
			// 
			this.BindingSource.SetBindingMember(this.JW_VesselBoundTextBox_Road, "MostInterestingTransportForBinding.JW_VesselForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_VesselForBinding)));
			this.JW_VesselBoundTextBox_Road.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 3, true);
			this.JW_VesselBoundTextBox_Road.Name = "JW_VesselBoundTextBox_Road";
			this.JW_VesselBoundTextBox_Road.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.JW_VesselBoundTextBox_Road.TabIndex = 5;
			// 
			// JW_VoyageFlightBoundTextBox_Road
			// 
			this.BindingSource.SetBindingMember(this.JW_VoyageFlightBoundTextBox_Road, "MostInterestingTransportForBinding.JW_VoyageFlightForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_VoyageFlightForBinding)));
			this.JW_VoyageFlightBoundTextBox_Road.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 3, true);
			this.JW_VoyageFlightBoundTextBox_Road.Name = "JW_VoyageFlightBoundTextBox_Road";
			this.JW_VoyageFlightBoundTextBox_Road.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JW_VoyageFlightBoundTextBox_Road.TabIndex = 1;
			// 
			// RailPanel
			// 
			this.RailPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RailPanel.Controls.Add(this.JW_VesselBoundTextBox_Rail);
			this.RailPanel.Controls.Add(this.JW_VoyageFlightBoundTextBox_Rail);
			this.RailPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.RailPanel.Name = "RailPanel";
			this.RailPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 26, true);
			this.RailPanel.TabIndex = 0;
			this.RailPanel.Visible = false;
			// 
			// JW_VesselBoundTextBox_Rail
			// 
			this.BindingSource.SetBindingMember(this.JW_VesselBoundTextBox_Rail, "MostInterestingTransportForBinding.JW_VesselForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_VesselForBinding)));
			this.JW_VesselBoundTextBox_Rail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 3, true);
			this.JW_VesselBoundTextBox_Rail.Name = "JW_VesselBoundTextBox_Rail";
			this.JW_VesselBoundTextBox_Rail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.JW_VesselBoundTextBox_Rail.TabIndex = 1;
			// 
			// JW_VoyageFlightBoundTextBox_Rail
			// 
			this.BindingSource.SetBindingMember(this.JW_VoyageFlightBoundTextBox_Rail, "MostInterestingTransportForBinding.JW_VoyageFlightForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_VoyageFlightForBinding)));
			this.JW_VoyageFlightBoundTextBox_Rail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 3, true);
			this.JW_VoyageFlightBoundTextBox_Rail.Name = "JW_VoyageFlightBoundTextBox_Rail";
			this.JW_VoyageFlightBoundTextBox_Rail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JW_VoyageFlightBoundTextBox_Rail.TabIndex = 3;
			// 
			// SeaPanel
			// 
			this.SeaPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SeaPanel.Controls.Add(this.JW_VesselBoundTextBox_Sea);
			this.SeaPanel.Controls.Add(this.JW_VoyageFlightBoundTextBox_Sea);
			this.SeaPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.SeaPanel.Name = "SeaPanel";
			this.SeaPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 26, true);
			this.SeaPanel.TabIndex = 1;
			this.SeaPanel.Visible = false;
			// 
			// JW_VesselBoundTextBox_Sea
			// 
			this.BindingSource.SetBindingMember(this.JW_VesselBoundTextBox_Sea, "MostInterestingTransportForBinding.JW_VesselForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_VesselForBinding)));
			this.JW_VesselBoundTextBox_Sea.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 4, true);
			this.JW_VesselBoundTextBox_Sea.Name = "JW_VesselBoundTextBox_Sea";
			this.JW_VesselBoundTextBox_Sea.PreBoundMaxLength = 35;
			this.JW_VesselBoundTextBox_Sea.ShowDescriptionBox = false;
			this.JW_VesselBoundTextBox_Sea.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 21, true);
			this.JW_VesselBoundTextBox_Sea.TabIndex = 3;
			// 
			// JW_VoyageFlightBoundTextBox_Sea
			// 
			this.BindingSource.SetBindingMember(this.JW_VoyageFlightBoundTextBox_Sea, "MostInterestingTransportForBinding.JW_VoyageFlightForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_VoyageFlightForBinding)));
			this.JW_VoyageFlightBoundTextBox_Sea.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 4, true);
			this.JW_VoyageFlightBoundTextBox_Sea.Name = "JW_VoyageFlightBoundTextBox_Sea";
			this.JW_VoyageFlightBoundTextBox_Sea.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JW_VoyageFlightBoundTextBox_Sea.TabIndex = 1;
			// 
			// AirPanel
			// 
			this.AirPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.AirPanel.Controls.Add(this.JW_JX_JV_RegistrationNoBoundTextBox);
			this.AirPanel.Controls.Add(this.JW_VoyageFlightBoundTextBox_Air);
			this.AirPanel.Controls.Add(this.JW_AircraftTypeTextBox);
			this.AirPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.AirPanel.Name = "AirPanel";
			this.AirPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 26, true);
			this.AirPanel.TabIndex = 0;
			this.AirPanel.Visible = false;
			// 
			// JW_JX_JV_RegistrationNoBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JW_JX_JV_RegistrationNoBoundTextBox, "MostInterestingTransportForBinding.JW_JX_JV_RegistrationNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_JX_JV_RegistrationNo)));
			this.JW_JX_JV_RegistrationNoBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ConsoLegDetailsControl|4b8ce929-45d1-4f21-981c-83d41ef30daa", "Aircraft Reg.", "Aircraft Registration Number", "The aircraft registration number for a chartered flight.");
			this.JW_JX_JV_RegistrationNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 4, true);
			this.JW_JX_JV_RegistrationNoBoundTextBox.Name = "JW_JX_JV_RegistrationNoBoundTextBox";
			this.JW_JX_JV_RegistrationNoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.JW_JX_JV_RegistrationNoBoundTextBox.TabIndex = 3;
			// 
			// JW_AircraftTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.JW_AircraftTypeTextBox, "MostInterestingTransportForBinding.JW_AircraftTypeForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_AircraftTypeForBinding)));
			this.JW_AircraftTypeTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ConsoLegDetailsControl|b2f20f8e-72ec-4d03-85d0-1e68a13c54c9", "Aircraft Type", "Aircraft Type", "The aircraft type for a flight.");
			this.JW_AircraftTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 4, true);
			this.JW_AircraftTypeTextBox.Name = "JW_AircraftTypeTextBox";
			this.JW_AircraftTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.JW_AircraftTypeTextBox.TabIndex = 2;
			// 
			// JW_VoyageFlightBoundTextBox_Air
			// 
			this.BindingSource.SetBindingMember(this.JW_VoyageFlightBoundTextBox_Air, "MostInterestingTransportForBinding.JW_VoyageFlightForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).JW_VoyageFlightForBinding)));
			this.JW_VoyageFlightBoundTextBox_Air.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 4, true);
			this.JW_VoyageFlightBoundTextBox_Air.Name = "JW_VoyageFlightBoundTextBox_Air";
			this.JW_VoyageFlightBoundTextBox_Air.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JW_VoyageFlightBoundTextBox_Air.TabIndex = 1;
			// 
			// FlightStatusLabel
			// 
			this.BindingSource.SetBindingMember(this.FlightStatusLabel, "MostInterestingTransportForBinding.OnlineScheduleStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.CommonConsol)(null)).MostInterestingTransportForBinding)).SyncRoot)).OnlineScheduleStatusDescription)));
			this.FlightStatusLabel.Font = new System.Drawing.Font(this.FlightStatusLabel.Font, System.Drawing.FontStyle.Bold);
			this.FlightStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 90);
			this.FlightStatusLabel.Name = "FlightStatusLabel";
			this.FlightStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 18);
			this.FlightStatusLabel.TextChanged += FlightStatusLabel_TextChanged;
			this.FlightStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ConsoLegDetailsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportGroupBox);
			this.Name = "ConsoLegDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportGroupBox.ResumeLayout(false);
			this.RoadPanel.ResumeLayout(false);
			this.RoadPanel.PerformLayout();
			this.RailPanel.ResumeLayout(false);
			this.RailPanel.PerformLayout();
			this.SeaPanel.ResumeLayout(false);
			this.SeaPanel.PerformLayout();
			this.AirPanel.ResumeLayout(false);
			this.AirPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox TransportGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JW_ATDBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JW_ETDBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JW_ATABoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JW_RL_NKLoadPortBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JW_RL_NKDiscPortBoundCodeFindBox;
		private CargoWise.Windows.UI.KPanel AirPanel;
		private Enterprise.ZArchitecture.ZTextBox JW_VoyageFlightBoundTextBox_Air;
		private CargoWise.Windows.UI.KPanel RoadPanel;
		private Enterprise.ZArchitecture.ZTextBox JW_VoyageFlightBoundTextBox_Road;
		private CargoWise.Windows.UI.KPanel RailPanel;
		private Enterprise.ZArchitecture.ZTextBox JW_VesselBoundTextBox_Rail;
		private Enterprise.ZArchitecture.ZTextBox JW_VoyageFlightBoundTextBox_Rail;
		private CargoWise.Windows.UI.KPanel SeaPanel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JW_VesselBoundTextBox_Sea;
		private Enterprise.ZArchitecture.ZTextBox JW_VoyageFlightBoundTextBox_Sea;
		private Enterprise.ZArchitecture.ZLabel WarningLabel;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JW_ETABoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox JW_IsCharterBoundCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private Enterprise.ZArchitecture.ZTextBox JW_JX_JV_RegistrationNoBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox JW_AircraftTypeTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsCargoOnlyCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsDomesticCheckBox;
		private Enterprise.ZArchitecture.ZTextBox JW_VesselBoundTextBox_Road;
		private Enterprise.ZArchitecture.ZLabel FlightStatusLabel;
	}
}
