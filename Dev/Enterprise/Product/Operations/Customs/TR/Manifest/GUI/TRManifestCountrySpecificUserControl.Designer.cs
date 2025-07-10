namespace Enterprise.Customs.TR.Manifest.GUI
{
	partial class TRManifestCountrySpecificUserControl
	{
		void InitializeComponent()
		{
			this.TransportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TempStorageDueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TempStorageStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.InspectionClerkTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InternalInspectionNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManifestDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TIRNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PresentationCustomsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GlobalManifestStampDutyValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MasterBillStampDutyValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalStampDutyValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportTypeDropEdit.SuspendLayout();
			this.TempStorageDueDateEdit.SuspendLayout();
			this.TempStorageStartDateEdit.SuspendLayout();
			this.PresentationCustomsOfficeDropEdit.SuspendLayout();
			this.RegistrationDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader);
			// 
			// TransportTypeDropEdit
			// 
			this.TransportTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportTypeDropEdit, "TransportType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).TransportType)));
			this.TransportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 2, true);
			this.TransportTypeDropEdit.Name = "TransportTypeDropEdit";
			this.TransportTypeDropEdit.PreBoundMaxLength = 2;
			this.TransportTypeDropEdit.ShouldResizeByMaxLength = true;
			this.TransportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransportTypeDropEdit.TabIndex = 0;
			// 
			// TempStorageDueDateEdit
			// 
			this.TempStorageDueDateEdit.AllowDrop = true;
			this.TempStorageDueDateEdit.AutoCompleteMonthThreshold = 1;
			this.TempStorageDueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TempStorageDueDateEdit, "TemporaryStorageDueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).TemporaryStorageDueDate)));
			this.TempStorageDueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 69, true);
			this.TempStorageDueDateEdit.Name = "TempStorageDueDateEdit";
			this.TempStorageDueDateEdit.TabIndex = 5;
			// 
			// TempStorageStartDateEdit
			// 
			this.TempStorageStartDateEdit.AllowDrop = true;
			this.TempStorageStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.TempStorageStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TempStorageStartDateEdit, "TemporaryStorageStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).TemporaryStorageStartDate)));
			this.TempStorageStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 47, true);
			this.TempStorageStartDateEdit.Name = "TempStorageStartDateEdit";
			this.TempStorageStartDateEdit.TabIndex = 4;
			// 
			// InspectionClerkTextBox
			// 
			this.BindingSource.SetBindingMember(this.InspectionClerkTextBox, "AMA_InspectionClerk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).AMA_InspectionClerk)));
			this.InspectionClerkTextBox.CaptionResourceString = null;
			this.InspectionClerkTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 3, true);
			this.InspectionClerkTextBox.Name = "InspectionClerkTextBox";
			this.InspectionClerkTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.InspectionClerkTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.InspectionClerkTextBox.TabIndex = 2;
			// 
			// InternalInspectionNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.InternalInspectionNoTextBox, "ManifestInternalInspectionNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).ManifestInternalInspectionNo)));
			this.InternalInspectionNoTextBox.CaptionResourceString = null;
			this.InternalInspectionNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 25, true);
			this.InternalInspectionNoTextBox.Name = "InternalInspectionNoTextBox";
			this.InternalInspectionNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.InternalInspectionNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.InternalInspectionNoTextBox.TabIndex = 3;
			// 
			// ManifestDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ManifestDescriptionTextBox, "AMA_ManifestDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).AMA_ManifestDescription)));
			this.ManifestDescriptionTextBox.CaptionResourceString = null;
			this.ManifestDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 25, true);
			this.ManifestDescriptionTextBox.Multiline = true;
			this.ManifestDescriptionTextBox.Name = "ManifestDescriptionTextBox";
			this.ManifestDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ManifestDescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ManifestDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 43, true);
			this.ManifestDescriptionTextBox.TabIndex = 1;
			// 
			// TIRNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TIRNumberTextBox, "TIRNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).TIRNumber)));
			this.TIRNumberTextBox.CaptionResourceString = null;
			this.TIRNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 94, true);
			this.TIRNumberTextBox.Name = "TIRNumberTextBox";
			this.TIRNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.TIRNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TIRNumberTextBox.TabIndex = 6;
			// 
			// PresentationCustomsOfficeDropEdit
			// 
			this.PresentationCustomsOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PresentationCustomsOfficeDropEdit, "TR_GM_PresentationCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).TR_GM_PresentationCustomsOffice)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PresentationCustomsOfficeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.PresentationCustomsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 71, true);
			this.PresentationCustomsOfficeDropEdit.Name = "PresentationCustomsOfficeDropEdit";
			this.PresentationCustomsOfficeDropEdit.ShouldResizeByMaxLength = true;
			this.PresentationCustomsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.PresentationCustomsOfficeDropEdit.TabIndex = 22;
			// 
			// GlobalManifestStampDutyValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GlobalManifestStampDutyValueCalcEdit, "GlobalManifestStampDutyValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).GlobalManifestStampDutyValue)));
			this.GlobalManifestStampDutyValueCalcEdit.CaptionResourceString = null;
			this.GlobalManifestStampDutyValueCalcEdit.DecimalPlaces = 2;
			this.GlobalManifestStampDutyValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 91, true);
			this.GlobalManifestStampDutyValueCalcEdit.Name = "GlobalManifestStampDutyValueCalcEdit";
			this.GlobalManifestStampDutyValueCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.GlobalManifestStampDutyValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.GlobalManifestStampDutyValueCalcEdit.TabIndex = 108;
			this.GlobalManifestStampDutyValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MasterBillStampDutyValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MasterBillStampDutyValueCalcEdit, "MasterBillStampDutyValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).MasterBillStampDutyValue)));
			this.MasterBillStampDutyValueCalcEdit.CaptionResourceString = null;
			this.MasterBillStampDutyValueCalcEdit.DecimalPlaces = 2;
			this.MasterBillStampDutyValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 114, true);
			this.MasterBillStampDutyValueCalcEdit.Name = "MasterBillStampDutyValueCalcEdit";
			this.MasterBillStampDutyValueCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.MasterBillStampDutyValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.MasterBillStampDutyValueCalcEdit.TabIndex = 108;
			this.MasterBillStampDutyValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalStampDutyValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalStampDutyValueCalcEdit, "TotalStampDutyValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader)(null)).TotalStampDutyValue)));
			this.TotalStampDutyValueCalcEdit.CaptionResourceString = null;
			this.TotalStampDutyValueCalcEdit.DecimalPlaces = 2;
			this.TotalStampDutyValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 136, true);
			this.TotalStampDutyValueCalcEdit.Name = "TotalStampDutyValueCalcEdit";
			this.TotalStampDutyValueCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.TotalStampDutyValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TotalStampDutyValueCalcEdit.TabIndex = 108;
			this.TotalStampDutyValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RegistrationDateEdit
			// 
			this.RegistrationDateEdit.AllowDrop = true;
			this.RegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RegistrationDateEdit, "RegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).RegistrationDate)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.RegistrationDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.RegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 380, true);
			this.RegistrationDateEdit.Name = "RegistrationDateEdit";
			this.RegistrationDateEdit.TabIndex = 30;
			this.RegistrationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			// 
			// TRManifestCountrySpecificUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PresentationCustomsOfficeDropEdit);
			this.Controls.Add(this.TIRNumberTextBox);
			this.Controls.Add(this.ManifestDescriptionTextBox);
			this.Controls.Add(this.InternalInspectionNoTextBox);
			this.Controls.Add(this.InspectionClerkTextBox);
			this.Controls.Add(this.TransportTypeDropEdit);
			this.Controls.Add(this.TempStorageDueDateEdit);
			this.Controls.Add(this.TempStorageStartDateEdit);
			this.Controls.Add(this.GlobalManifestStampDutyValueCalcEdit);
			this.Controls.Add(this.MasterBillStampDutyValueCalcEdit);
			this.Controls.Add(this.TotalStampDutyValueCalcEdit);
			this.Controls.Add(this.RegistrationDateEdit);
			this.Name = "TRManifestCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportTypeDropEdit.ResumeLayout(true);
			this.TransportTypeDropEdit.PerformLayout();
			this.TempStorageDueDateEdit.ResumeLayout(true);
			this.TempStorageDueDateEdit.PerformLayout();
			this.TempStorageStartDateEdit.ResumeLayout(true);
			this.TempStorageStartDateEdit.PerformLayout();
			this.PresentationCustomsOfficeDropEdit.ResumeLayout(true);
			this.PresentationCustomsOfficeDropEdit.PerformLayout();
			this.RegistrationDateEdit.ResumeLayout(true);
			this.RegistrationDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit PresentationCustomsOfficeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TransportTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit TempStorageDueDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit TempStorageStartDateEdit;
		internal Enterprise.ZArchitecture.ZTextBox InspectionClerkTextBox;
		internal Enterprise.ZArchitecture.ZTextBox InternalInspectionNoTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ManifestDescriptionTextBox;
		internal Enterprise.ZArchitecture.ZTextBox TIRNumberTextBox;
		internal Enterprise.ZArchitecture.ZCalcEdit GlobalManifestStampDutyValueCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit MasterBillStampDutyValueCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit TotalStampDutyValueCalcEdit;
		internal ZArchitecture.GUI.ZDateEdit RegistrationDateEdit;
	}
}
