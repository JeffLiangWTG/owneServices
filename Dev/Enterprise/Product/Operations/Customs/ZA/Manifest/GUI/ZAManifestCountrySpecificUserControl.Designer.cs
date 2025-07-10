namespace Enterprise.Customs.ZA.Manifest.GUI
{
	partial class ZAManifestCountrySpecificUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PlaceOfEntryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PlaceOfExitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DateAtCustomsOfficeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RadioCallSignCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EstLoadDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MasterCarrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VoyageFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TssVesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TssRadioCallSignCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TssCargoCarrierCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DateOfDepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SeparatorTextUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.CaseNumberManifestHeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CaseNumberGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CallPurposeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PlaceOfEntryDropEdit.SuspendLayout();
			this.PlaceOfExitDropEdit.SuspendLayout();
			this.DateAtCustomsOfficeDateEdit.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.RadioCallSignCodeFindBox.SuspendLayout();
			this.EstLoadDateEdit.SuspendLayout();
			this.TssVesselCodeFindBox.SuspendLayout();
			this.TssRadioCallSignCodeFindBox.SuspendLayout();
			this.TssCargoCarrierCodeCodeFindBox.SuspendLayout();
			this.DateOfDepartureDateEdit.SuspendLayout();
			this.SeparatorTextUserControl.SuspendLayout();
			this.CaseNumberManifestHeaderGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CaseNumberGrid)).BeginInit();
			this.CaseNumberGrid.SuspendLayout();
			this.CallPurposeCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader);
			// 
			// PlaceOfEntryDropEdit
			// 
			this.PlaceOfEntryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfEntryDropEdit, "PlaceOfEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).PlaceOfEntry)));
			this.PlaceOfEntryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 32, true);
			this.PlaceOfEntryDropEdit.Name = "PlaceOfEntryDropEdit";
			this.PlaceOfEntryDropEdit.PreBoundMaxLength = 3;
			this.PlaceOfEntryDropEdit.ShouldResizeByMaxLength = true;
			this.PlaceOfEntryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.PlaceOfEntryDropEdit.TabIndex = 5;
			// 
			// PlaceOfExitDropEdit
			// 
			this.PlaceOfExitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfExitDropEdit, "PlaceOfExit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).PlaceOfExit)));
			this.PlaceOfExitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 80, true);
			this.PlaceOfExitDropEdit.Name = "PlaceOfExitDropEdit";
			this.PlaceOfExitDropEdit.PreBoundMaxLength = 3;
			this.PlaceOfExitDropEdit.ShouldResizeByMaxLength = true;
			this.PlaceOfExitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.PlaceOfExitDropEdit.TabIndex = 6;
			// 
			// DateAtCustomsOfficeDateEdit
			// 
			this.DateAtCustomsOfficeDateEdit.AllowDrop = true;
			this.DateAtCustomsOfficeDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateAtCustomsOfficeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateAtCustomsOfficeDateEdit, "AMA_DateAtCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).AMA_DateAtCustomsOffice)));
			this.DateAtCustomsOfficeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 128, true);
			this.DateAtCustomsOfficeDateEdit.Name = "DateAtCustomsOfficeDateEdit";
			this.DateAtCustomsOfficeDateEdit.TabIndex = 4;
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "AMA_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).AMA_VesselName)));
			this.VesselCodeFindBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 75, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VesselCodeFindBox.ParentType = null;
			this.VesselCodeFindBox.PreBoundMaxLength = 35;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.VesselCodeFindBox.TabIndex = 14;
			// 
			// RadioCallSignCodeFindBox
			// 
			this.RadioCallSignCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RadioCallSignCodeFindBox, "AMA_RadioCallSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).AMA_RadioCallSign)));
			this.RadioCallSignCodeFindBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.RadioCallSignCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 152, true);
			this.RadioCallSignCodeFindBox.Name = "RadioCallSignCodeFindBox";
			this.RadioCallSignCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.RadioCallSignCodeFindBox.ParentType = null;
			this.RadioCallSignCodeFindBox.PreBoundMaxLength = 10;
			this.RadioCallSignCodeFindBox.ShowDescriptionBox = false;
			this.RadioCallSignCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.RadioCallSignCodeFindBox.TabIndex = 15;
			// 
			// EstLoadDateEdit
			// 
			this.EstLoadDateEdit.AllowDrop = true;
			this.EstLoadDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstLoadDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstLoadDateEdit, "EstimatedTimeOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).EstimatedTimeOfLoading)));
			this.EstLoadDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EstLoadDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 56, true);
			this.EstLoadDateEdit.Name = "EstLoadDateEdit";
			this.EstLoadDateEdit.TabIndex = 2;
			// 
			// MasterCarrierCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterCarrierCodeTextBox, "MasterCarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).MasterCarrierCode)));
			this.MasterCarrierCodeTextBox.CaptionResourceString = null;
			this.MasterCarrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 8, true);
			this.MasterCarrierCodeTextBox.Name = "MasterCarrierCodeTextBox";
			this.MasterCarrierCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MasterCarrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.MasterCarrierCodeTextBox.TabIndex = 0;
			// 
			// VoyageFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageFlightTextBox, "TSS_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).TSS_VoyageFlight)));
			this.VoyageFlightTextBox.CaptionResourceString = null;
			this.VoyageFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 104, true);
			this.VoyageFlightTextBox.Name = "VoyageFlightTextBox";
			this.VoyageFlightTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.VoyageFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.VoyageFlightTextBox.TabIndex = 9;
			// 
			// TssVesselCodeFindBox
			// 
			this.TssVesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TssVesselCodeFindBox, "TSS_Vessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).TSS_Vessel)));
			this.TssVesselCodeFindBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.TssVesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 75, true);
			this.TssVesselCodeFindBox.Name = "TssVesselCodeFindBox";
			this.TssVesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TssVesselCodeFindBox.ParentType = null;
			this.TssVesselCodeFindBox.PreBoundMaxLength = 35;
			this.TssVesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.TssVesselCodeFindBox.TabIndex = 10;
			// 
			// TssRadioCallSignCodeFindBox
			// 
			this.TssRadioCallSignCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TssRadioCallSignCodeFindBox, "TSS_RadioCallSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).TSS_RadioCallSign)));
			this.TssRadioCallSignCodeFindBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.TssRadioCallSignCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 152, true);
			this.TssRadioCallSignCodeFindBox.Name = "TssRadioCallSignCodeFindBox";
			this.TssRadioCallSignCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TssRadioCallSignCodeFindBox.ParentType = null;
			this.TssRadioCallSignCodeFindBox.PreBoundMaxLength = 10;
			this.TssRadioCallSignCodeFindBox.ShowDescriptionBox = false;
			this.TssRadioCallSignCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.TssRadioCallSignCodeFindBox.TabIndex = 11;
			// 
			// TssCargoCarrierCodeCodeFindBox
			// 
			this.TssCargoCarrierCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TssCargoCarrierCodeCodeFindBox, "TSS_CargoCarrierPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).TSS_CargoCarrierPK)));
			this.TssCargoCarrierCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 224, true);
			this.TssCargoCarrierCodeCodeFindBox.Name = "TssCargoCarrierCodeCodeFindBox";
			this.TssCargoCarrierCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TssCargoCarrierCodeCodeFindBox.ParentType = null;
			this.TssCargoCarrierCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.TssCargoCarrierCodeCodeFindBox.TabIndex = 12;
			// 
			// DateOfDepartureDateEdit
			// 
			this.DateOfDepartureDateEdit.AllowDrop = true;
			this.DateOfDepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfDepartureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfDepartureDateEdit, "TSS_DateOfDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).TSS_DateOfDeparture)));
			this.DateOfDepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 200, true);
			this.DateOfDepartureDateEdit.Name = "DateOfDepartureDateEdit";
			this.DateOfDepartureDateEdit.TabIndex = 13;
			// 
			// SeparatorTextUserControl
			// 
			this.SeparatorTextUserControl.AllowDrop = true;
			this.SeparatorTextUserControl.CaptionResourceString = Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("ZAManifest|TSSSeparator", "Transhipment Details");
			this.SeparatorTextUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 8, true);
			this.SeparatorTextUserControl.Name = "SeparatorTextUserControl";
			this.SeparatorTextUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 15, true);
			this.SeparatorTextUserControl.TabIndex = 15;
			// 
			// CaseNumberManifestHeaderGroupBox
			// 
			this.CaseNumberManifestHeaderGroupBox.Controls.Add(this.CaseNumberGrid);
			this.CaseNumberManifestHeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 27, true);
			this.CaseNumberManifestHeaderGroupBox.Name = "CaseNumberManifestHeaderGroupBox";
			this.CaseNumberManifestHeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 222, true);
			this.CaseNumberManifestHeaderGroupBox.TabIndex = 7;
			this.CaseNumberManifestHeaderGroupBox.TabStop = false;
			this.CaseNumberManifestHeaderGroupBox.CaptionResourceString = Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("7B8754DF-4DD6-4D8D-9910-1DED5771D9E2", "Supporting Documents Cases");
			// 
			// CaseNumberGrid
			// 
			this.CaseNumberGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CaseNumberGrid, "CaseNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).CaseNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CaseNumber)(((System.Collections.IList)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).CaseNumbers)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CaseNumber)(((System.Collections.IList)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).CaseNumbers)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CaseNumber)(((System.Collections.IList)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).CaseNumbers)).SyncRoot)).Description)));
			this.CaseNumberGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("BC8240E7-FB41-41FA-942A-4F407C7B0383", "Case Number");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("EF286758-7AD5-4019-BCD2-4145CFF50AAB", "Status");
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Code";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.Manifest.GUI.Res.GetData("E5762E4B-4EA6-42F3-AC1B-8C07D95AFD97", "Status Description");
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			this.CaseNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CaseNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CaseNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CaseNumberGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CaseNumberGrid.GridId = "009DC144-0360-44A8-97BD-EEB73A606521";
			this.CaseNumberGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CaseNumberGrid.LayoutKey = "CaseNumberGrid";
			this.CaseNumberGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CaseNumberGrid.Name = "CaseNumberGrid";
			this.CaseNumberGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 203, true);
			this.CaseNumberGrid.TabIndex = 8;
			// 
			// CallPurposeCodeDropEdit
			// 
			this.CallPurposeCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CallPurposeCodeDropEdit, "CallPurposeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader)(null)).CallPurposeCode)));
			this.CallPurposeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 250, true);
			this.CallPurposeCodeDropEdit.Name = "CallPurposeCodeDropEdit";
			this.CallPurposeCodeDropEdit.PreBoundMaxLength = 3;
			this.CallPurposeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CallPurposeCodeDropEdit.TabIndex = 16;
			// 
			// ZAManifestCountrySpecificUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CallPurposeCodeDropEdit);
			this.Controls.Add(this.SeparatorTextUserControl);
			this.Controls.Add(this.VoyageFlightTextBox);
			this.Controls.Add(this.TssVesselCodeFindBox);
			this.Controls.Add(this.TssRadioCallSignCodeFindBox);
			this.Controls.Add(this.TssCargoCarrierCodeCodeFindBox);
			this.Controls.Add(this.DateOfDepartureDateEdit);
			this.Controls.Add(this.CaseNumberManifestHeaderGroupBox);
			this.Controls.Add(this.MasterCarrierCodeTextBox);
			this.Controls.Add(this.PlaceOfExitDropEdit);
			this.Controls.Add(this.PlaceOfEntryDropEdit);
			this.Controls.Add(this.DateAtCustomsOfficeDateEdit);
			this.Controls.Add(this.VesselCodeFindBox);
			this.Controls.Add(this.RadioCallSignCodeFindBox);
			this.Controls.Add(this.EstLoadDateEdit);
			this.Name = "ZAManifestCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 278, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PlaceOfEntryDropEdit.ResumeLayout(true);
			this.PlaceOfEntryDropEdit.PerformLayout();
			this.PlaceOfExitDropEdit.ResumeLayout(true);
			this.PlaceOfExitDropEdit.PerformLayout();
			this.DateAtCustomsOfficeDateEdit.ResumeLayout(true);
			this.DateAtCustomsOfficeDateEdit.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.RadioCallSignCodeFindBox.ResumeLayout(true);
			this.RadioCallSignCodeFindBox.PerformLayout();
			this.EstLoadDateEdit.ResumeLayout(true);
			this.EstLoadDateEdit.PerformLayout();
			this.TssVesselCodeFindBox.ResumeLayout(true);
			this.TssVesselCodeFindBox.PerformLayout();
			this.TssRadioCallSignCodeFindBox.ResumeLayout(true);
			this.TssRadioCallSignCodeFindBox.PerformLayout();
			this.TssCargoCarrierCodeCodeFindBox.ResumeLayout(true);
			this.TssCargoCarrierCodeCodeFindBox.PerformLayout();
			this.DateOfDepartureDateEdit.ResumeLayout(true);
			this.DateOfDepartureDateEdit.PerformLayout();
			this.SeparatorTextUserControl.ResumeLayout(true);
			this.SeparatorTextUserControl.PerformLayout();
			this.CaseNumberManifestHeaderGroupBox.ResumeLayout(false);
			this.CaseNumberManifestHeaderGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CaseNumberGrid)).EndInit();
			this.CaseNumberGrid.ResumeLayout(false);
			this.CaseNumberGrid.PerformLayout();
			this.CallPurposeCodeDropEdit.ResumeLayout(true);
			this.CallPurposeCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit PlaceOfEntryDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit PlaceOfExitDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit DateAtCustomsOfficeDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox RadioCallSignCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit EstLoadDateEdit;
		internal Enterprise.ZArchitecture.ZTextBox MasterCarrierCodeTextBox;
		internal Enterprise.ZArchitecture.ZTextBox VoyageFlightTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TssVesselCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TssRadioCallSignCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox TssCargoCarrierCodeCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit DateOfDepartureDateEdit;
		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl SeparatorTextUserControl;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox CaseNumberManifestHeaderGroupBox;
		Enterprise.ZArchitecture.ZGrid CaseNumberGrid;
		internal ZArchitecture.GUI.ZDropEdit CallPurposeCodeDropEdit;
	}
}
