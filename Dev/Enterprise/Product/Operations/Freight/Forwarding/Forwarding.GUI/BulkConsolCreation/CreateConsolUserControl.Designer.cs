using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class CreateConsolUserControl
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
            this.CreateConsolTabControl = new ZTemplateTabControl();
            this.CreateNewConsolTabPage = new ZTabPage();
            this.CreateConsolFromTemplatesTabPage = new ZTabPage();
			this.MawbConsolsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsolidationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsolsPerFlightEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalConsolsEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PreAllocatedValuesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VolumeDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.WeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServiceLevelDropEditFromTemplate = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AllocateNeutralMasterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AllocateNeutralMasterCheckBoxFromTemplates = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AirlinePrefixEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.ChargeableEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ShipmentsEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CTOCutOffEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.CFSCutOffEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.CTOCutOffLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CFSCutOffLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TemplateGrid = new ZArchitecture.ZGrid();
			var ConsolTemplateColumnStyleInfo = new ConsolCreationTemplateColumnStyleInfo();
			var perFlightColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			var totalConsolsColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.CreateConsolTabControl.SuspendLayout();
			this.ConsolidationDetailsGroupBox.SuspendLayout();
			this.VolumeDropEdit.SuspendLayout();
			this.WeightDropEdit.SuspendLayout();
			this.ServiceLevelDropEdit.SuspendLayout();
			this.ServiceLevelDropEditFromTemplate.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TemplateGrid)).BeginInit();
			this.TemplateGrid.SuspendLayout();
            this.SuspendLayout();
			// 
			// MawbConsolsLabel
			// 
			this.MawbConsolsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|B9C89973-6926-4601-A2F3-5CE3D3A3A217", "Specify the number of MAWB/Consols to be created for the selected flight");
			this.MawbConsolsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.MawbConsolsLabel.IsFontBold = true;
			this.MawbConsolsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.MawbConsolsLabel.Name = "MawbConsolsLabel";
			this.MawbConsolsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 49, true);
			this.MawbConsolsLabel.TabIndex = 1;
			// 
			// ConsolidationDetailsGroupBox
			// 
			this.ConsolidationDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|{79B70EE7-9455-4E59-BAC7-A78719CE0940}", "Consolidation Details");
			this.ConsolidationDetailsGroupBox.Controls.Add(this.VolumeDropEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.WeightDropEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.AllocateNeutralMasterCheckBox);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.ServiceLevelDropEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.AirlinePrefixEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.ChargeableEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.ShipmentsEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.PreAllocatedValuesLabel);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.ConsolsPerFlightEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.TotalConsolsEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.CTOCutOffEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.CFSCutOffEdit);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.CTOCutOffLabel);
			this.ConsolidationDetailsGroupBox.Controls.Add(this.CFSCutOffLabel);
			this.ConsolidationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 49, true);
			this.ConsolidationDetailsGroupBox.Name = "ConsolidationDetailsGroupBox";
			this.ConsolidationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 211, true);
			this.ConsolidationDetailsGroupBox.TabIndex = 2;
			this.ConsolidationDetailsGroupBox.TabStop = false;
			// 
			// CTOCutOffEdit
			// 
			this.BindingSource.SetBindingMember(this.CTOCutOffEdit, "ConsolDetails.CTOCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.CTOCutOff)));
			this.CTOCutOffEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|0EB75594-C951-4B8B-823A-A695454C8BAD", "CTO Cut Off");
			this.CTOCutOffEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 167, true);
			this.CTOCutOffEdit.Name = "CTOCutOffEdit";
			this.CTOCutOffEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.CTOCutOffEdit.TabIndex = 6;
			// 
			// CFSCutOffEdit
			// 
			this.BindingSource.SetBindingMember(this.CFSCutOffEdit, "ConsolDetails.CFSCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.CFSCutOff)));
			this.CFSCutOffEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|3A18A5DE-A698-4D45-95D0-AC743715EF93", "CFS Cut Off");
			this.CFSCutOffEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 167, true);
			this.CFSCutOffEdit.Name = "CFSCutOffEdit";
			this.CFSCutOffEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.CFSCutOffEdit.TabIndex = 11;
			// 
			// CTOCutOffLabel
			//
			this.CTOCutOffLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|158438BA-E76B-446E-9A59-93F9337D5AED", "(hours before departure)");
			this.CTOCutOffLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 187, true);
			this.CTOCutOffLabel.Name = "CTOCutOffLabel";
			this.CTOCutOffLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.CTOCutOffLabel.TabIndex = 0;
			this.CTOCutOffLabel.ForeColor = System.Drawing.Color.Gray;
			// 
			// CFSCutOffLabel
			//
			this.CFSCutOffLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|158438BA-E76B-446E-9A59-93F9337D5AED", "(hours before departure)");
			this.CFSCutOffLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 187, true);
			this.CFSCutOffLabel.Name = "CFSCutOffLabel";
			this.CFSCutOffLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.CFSCutOffLabel.TabIndex = 0;
			this.CFSCutOffLabel.ForeColor = System.Drawing.Color.Gray;
			// 
			// ConsolsPerFlightEdit
			// 
			this.BindingSource.SetBindingMember(this.ConsolsPerFlightEdit, "ConsolDetails.ConsolsPerFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.ConsolsPerFlight)));
			this.ConsolsPerFlightEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|625B582B-EC14-46B3-BEE6-4E66DA959C80", "Consols Per Flight");
			this.ConsolsPerFlightEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsolsPerFlightEdit.DecimalPlaces = 2;
			this.ConsolsPerFlightEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 20, true);
			this.ConsolsPerFlightEdit.Name = "ConsolsPerFlightEdit";
			this.ConsolsPerFlightEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.ConsolsPerFlightEdit.TabIndex = 0;
			this.ConsolsPerFlightEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalConsolsEdit
			//
			this.BindingSource.SetBindingMember(this.TotalConsolsEdit, "ConsolDetails.TotalConsols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.TotalConsols)));
			this.TotalConsolsEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|1da5258e-1a62-4934-9371-89dfb61a3621", "Total Consols");
			this.TotalConsolsEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TotalConsolsEdit.DecimalPlaces = 2;
			this.TotalConsolsEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 20, true);
			this.TotalConsolsEdit.Name = "TotalConsolsEdit";
			this.TotalConsolsEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.TotalConsolsEdit.TabIndex = 1;
			this.TotalConsolsEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;			
			// 
			// PreAllocatedValuesLabel
			// 
			this.PreAllocatedValuesLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|A2707193-7A7A-4D23-A01D-85F4546BF540", "Pre-Allocated Values");
			this.PreAllocatedValuesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)(((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)
				| Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PreAllocatedValuesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 52, true);
			this.PreAllocatedValuesLabel.Name = "PreAllocatedValuesLabel";
			this.PreAllocatedValuesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 15, true);
			this.PreAllocatedValuesLabel.TabIndex = 2;
			// 
			// VolumeDropEdit
			// 
			this.VolumeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.VolumeUnit)));
			this.VolumeDropEdit.BindToAmount = "ConsolDetails.Volume";
			this.VolumeDropEdit.BindToUnit = "ConsolDetails.VolumeUnit";
			this.VolumeDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|5275921C-4890-4E2C-BBC6-9EFF84F2DC7B", "Volume");
			this.VolumeDropEdit.Decimals = 3;
			this.VolumeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 107, true);
			this.VolumeDropEdit.Name = "VolumeDropEdit";
			this.VolumeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.VolumeDropEdit.TabIndex = 8;
			// 
			// WeightDropEdit
			// 
			this.WeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.WeightUnit)));
			this.WeightDropEdit.BindToAmount = "ConsolDetails.Weight";
			this.WeightDropEdit.BindToUnit = "ConsolDetails.WeightUnit";
			this.WeightDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|F3DCC481-37EC-484F-A042-E33DCD7136F7", "Weight");
			this.WeightDropEdit.Decimals = 3;
			this.WeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 74, true);
			this.WeightDropEdit.Name = "WeightDropEdit";
			this.WeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.WeightDropEdit.TabIndex = 7;
			// 
			// AllocateNeutralMasterCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AllocateNeutralMasterCheckBox, "ConsolDetails.AllocateNeutralMaster");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.AllocateNeutralMaster)));
			this.AllocateNeutralMasterCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|E5582929-C123-404A-A9F2-53CE2D75CC1D", "Allocate Neutral Master Automatically");
			this.AllocateNeutralMasterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllocateNeutralMasterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 139, true);
			this.AllocateNeutralMasterCheckBox.Name = "AllocateNeutralMasterCheckBox";
			this.AllocateNeutralMasterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 17, true);
			this.AllocateNeutralMasterCheckBox.TabIndex = 9;
			this.AllocateNeutralMasterCheckBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ServiceLevelDropEdit
			// 
			this.ServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelDropEdit, "ConsolDetails.ServiceLevel");
			this.ServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 137, true);
			this.ServiceLevelDropEdit.Name = "ServiceLevelDropEdit";
			this.ServiceLevelDropEdit.PreBoundMaxLength = 3;
			this.ServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.ServiceLevelDropEdit.TabIndex = 10;
			// 
			// AirlinePrefixEdit
			// 
			this.BindingSource.SetBindingMember(this.AirlinePrefixEdit, "ConsolDetails.AirlinePrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.AirlinePrefix)));
			this.AirlinePrefixEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|9093B077-EB7B-4D1C-81CA-AC50B9043805", "Airline Prefix");
			this.AirlinePrefixEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 137, true);
			this.AirlinePrefixEdit.Name = "AirlinePrefixEdit";
			this.AirlinePrefixEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.AirlinePrefixEdit.TabIndex = 5;
			// 
			// ChargeableEdit
			// 
			this.BindingSource.SetBindingMember(this.ChargeableEdit, "ConsolDetails.Chargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.Chargeable)));
			this.ChargeableEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|8329552C-B1F4-405B-A094-13AB3E61FF13", "Chargeable");
			this.ChargeableEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ChargeableEdit.DecimalPlaces = 2;
			this.ChargeableEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 107, true);
			this.ChargeableEdit.Name = "ChargeableEdit";
			this.ChargeableEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.ChargeableEdit.TabIndex = 4;
			this.ChargeableEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentsEdit
			// 
			this.ShipmentsEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ShipmentsEdit, "ConsolDetails.Shipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.Shipments)));
			this.ShipmentsEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|A9670EF3-4E25-43FB-BB15-235C594FEB42", "Shipments");
			this.ShipmentsEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ShipmentsEdit.DecimalPlaces = 2;
			this.ShipmentsEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 74, true);
			this.ShipmentsEdit.Name = "ShipmentsEdit";
			this.ShipmentsEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.ShipmentsEdit.TabIndex = 3;
			this.ShipmentsEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
            // CreateConsolTabControl
            // 
            this.CreateConsolTabControl.Controls.Add(this.CreateNewConsolTabPage);
            this.CreateConsolTabControl.Controls.Add(this.CreateConsolFromTemplatesTabPage);
            this.CreateConsolTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CreateConsolTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CreateConsolTabControl.Name = "CreateConsolTabControl";
            this.CreateConsolTabControl.SelectedIndex = 0;
            this.CreateConsolTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 238, true);
            this.CreateConsolTabControl.TabIndex = 0;
            // 
            // CreateNewConsolTabPage
            // 
			this.CreateNewConsolTabPage.Controls.Add(this.MawbConsolsLabel);
			this.CreateNewConsolTabPage.Controls.Add(this.ConsolidationDetailsGroupBox);			
            this.CreateNewConsolTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
            this.CreateNewConsolTabPage.Name = MultiDaysSelection.CreateNewConsolsTabName;
            this.CreateNewConsolTabPage.Size =  CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 252, true);
            this.CreateNewConsolTabPage.TabIndex = 0;
			this.CreateNewConsolTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|555eedf4-0eec-44f7-a561-03134095fd2c", "Create New Consols");
			//
			// TemplateGrid
			//
			this.TemplateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TemplateGrid, "ConsolTemplateDetails.ConsolTemplates");
			this.TemplateGrid.CaptionVisible = false;
			ConsolTemplateColumnStyleInfo.ColumnName = "ConsolTemplateReferenceId";
			ConsolTemplateColumnStyleInfo.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobConsol;
			ConsolTemplateColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("652e5acc-5061-4ef3-9dc5-4079f2c7582e", "Consolidation Template");
			ConsolTemplateColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			ConsolTemplateColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			perFlightColumnStyleInfo.ColumnName = "ConsolsPerFlight";
			perFlightColumnStyleInfo.BindToDecimalPlaces = null;
			perFlightColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("49285e33-a05f-41ac-9425-e644012df991", "Per Flight");
			perFlightColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);		
			totalConsolsColumnStyleInfo.ColumnName = "TotalConsols";
			totalConsolsColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("02dca336-cc8a-40fb-9fbe-9e1618210f06", "Total Consols");
			totalConsolsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TemplateGrid.ColumnStyles.Add(ConsolTemplateColumnStyleInfo);
			this.TemplateGrid.ColumnStyles.Add(perFlightColumnStyleInfo);
			this.TemplateGrid.ColumnStyles.Add(totalConsolsColumnStyleInfo);
			this.TemplateGrid.GridId = "eb9c0df9-f8b8-4ce3-bf0d-f4ad3e497372";
			this.TemplateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TemplateGrid.LayoutKey = "TemplateGrid";
			this.TemplateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.TemplateGrid.Name = "TemplateGrid";
			this.TemplateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 230, true);
			this.TemplateGrid.TabIndex = 1;
			// 
			// AllocateNeutralMasterCheckBoxFromTemplates
			// 
			this.BindingSource.SetBindingMember(this.AllocateNeutralMasterCheckBoxFromTemplates, "ConsolTemplateDetails.AllocateNeutralMaster");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.MultiDaysSelection)(null)).ConsolDetails.AllocateNeutralMaster)));
			this.AllocateNeutralMasterCheckBoxFromTemplates.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|6f0fb824-5645-43bc-9105-f5649a686279", "Allocate Neutral Master Automatically");
			this.AllocateNeutralMasterCheckBoxFromTemplates.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllocateNeutralMasterCheckBoxFromTemplates.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 245, true);
			this.AllocateNeutralMasterCheckBoxFromTemplates.Name = "AllocateNeutralMasterCheckBoxFromTemplates";
			this.AllocateNeutralMasterCheckBoxFromTemplates.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 17, true);
			this.AllocateNeutralMasterCheckBoxFromTemplates.TabIndex = 2;
			this.AllocateNeutralMasterCheckBoxFromTemplates.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ServiceLevelDropEditFromTemplate
			// 
			this.ServiceLevelDropEditFromTemplate.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelDropEditFromTemplate, "ConsolTemplateDetails.ServiceLevel");
			this.ServiceLevelDropEditFromTemplate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 243, true);
			this.ServiceLevelDropEditFromTemplate.Name = "ServiceLevelDropEditFromTemplate";
			this.ServiceLevelDropEditFromTemplate.PreBoundMaxLength = 3;
			this.ServiceLevelDropEditFromTemplate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.ServiceLevelDropEditFromTemplate.TabIndex = 3;
            // 
            // CreateConsolFromTemplatesTabPage
            //
			this.CreateConsolFromTemplatesTabPage.Controls.Add(this.AllocateNeutralMasterCheckBoxFromTemplates);
			this.CreateConsolFromTemplatesTabPage.Controls.Add(this.ServiceLevelDropEditFromTemplate);
			this.CreateConsolFromTemplatesTabPage.Controls.Add(this.TemplateGrid);
            this.CreateConsolFromTemplatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
            this.CreateConsolFromTemplatesTabPage.Name = MultiDaysSelection.CreateFromConsolTemplatesTabName;
            this.CreateConsolFromTemplatesTabPage.Size =  CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 252, true);
            this.CreateConsolFromTemplatesTabPage.TabIndex = 1;
			this.CreateConsolFromTemplatesTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CreateConsolUserControl|77fa6f52-51cc-4354-9dce-b65b22ace7bd", "Create Consols from Templates");
			// 
            // CreateConsolUserControl
            // 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CreateConsolTabControl);
            this.Name = "CreateConsolUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 328, true);
			this.ConsolidationDetailsGroupBox.ResumeLayout(false);
			this.ConsolidationDetailsGroupBox.PerformLayout();
			this.VolumeDropEdit.ResumeLayout(true);
			this.VolumeDropEdit.PerformLayout();
			this.WeightDropEdit.ResumeLayout(true);
			this.WeightDropEdit.PerformLayout();
			this.ServiceLevelDropEdit.ResumeLayout(true);
			this.ServiceLevelDropEdit.PerformLayout();
			this.ServiceLevelDropEditFromTemplate.ResumeLayout(true);
			this.ServiceLevelDropEditFromTemplate.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TemplateGrid)).EndInit();
			this.TemplateGrid.ResumeLayout(false);
			this.TemplateGrid.PerformLayout();
			this.CreateNewConsolTabPage.ResumeLayout(false);
			this.CreateConsolFromTemplatesTabPage.ResumeLayout(false);
			this.CreateConsolTabControl.ResumeLayout(false);
            this.ResumeLayout(false);
		}

		#endregion

		internal ZTemplateTabControl CreateConsolTabControl;
		ZTabPage CreateNewConsolTabPage;
		ZTabPage CreateConsolFromTemplatesTabPage;
		internal ZArchitecture.ZLabel MawbConsolsLabel;
		ZArchitecture.GUI.ZGroupBox ConsolidationDetailsGroupBox;
		ZArchitecture.ZCalcEdit ConsolsPerFlightEdit;
		ZArchitecture.ZCalcEdit TotalConsolsEdit;
		ZArchitecture.ZLabel PreAllocatedValuesLabel;
		ZArchitecture.GUI.ZCalcDropEdit VolumeDropEdit;
		ZArchitecture.GUI.ZCalcDropEdit WeightDropEdit;
		ZArchitecture.GUI.ZCheckBox AllocateNeutralMasterCheckBox;
		ZArchitecture.GUI.ZCheckBox AllocateNeutralMasterCheckBoxFromTemplates;
		ZArchitecture.ZTextBox AirlinePrefixEdit;
		ZArchitecture.ZCalcEdit ChargeableEdit;
		ZArchitecture.ZCalcEdit ShipmentsEdit;
		ZArchitecture.GUI.ZTimeEdit CTOCutOffEdit;
		ZArchitecture.GUI.ZTimeEdit CFSCutOffEdit;
		ZArchitecture.ZLabel CTOCutOffLabel;
		ZArchitecture.ZLabel CFSCutOffLabel;
		ZArchitecture.ZGrid TemplateGrid;
		ZArchitecture.GUI.ZDropEdit ServiceLevelDropEdit;
		ZArchitecture.GUI.ZDropEdit ServiceLevelDropEditFromTemplate;
	}
}
