using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class DocServicesUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ServicesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.measurementBasisDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.contractorFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.notesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.referenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.durationTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.serviceCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.subLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.serviceLocationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.completedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.bookedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.serviceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.rateAndCurrencyCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ServicesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ServicesGroupBox.SuspendLayout();
			this.measurementBasisDropEdit.SuspendLayout();
			this.contractorFindBox.SuspendLayout();
			this.serviceLocationAddressControl.SuspendLayout();
			this.completedDateEdit.SuspendLayout();
			this.bookedDateEdit.SuspendLayout();
			this.serviceTypeDropEdit.SuspendLayout();
			this.rateAndCurrencyCalcFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).BeginInit();
			this.ServicesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IHaveServices);
			// 
			// ServicesGroupBox
			// 
			this.ServicesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocServicesUserControl|7c5cb182-4cc0-42ae-8d9a-fdc5178bbceb", "Services");
			this.ServicesGroupBox.Controls.Add(this.measurementBasisDropEdit);
			this.ServicesGroupBox.Controls.Add(this.contractorFindBox);
			this.ServicesGroupBox.Controls.Add(this.notesTextBox);
			this.ServicesGroupBox.Controls.Add(this.referenceTextBox);
			this.ServicesGroupBox.Controls.Add(this.durationTimeEdit);
			this.ServicesGroupBox.Controls.Add(this.serviceCountCalcEdit);
			this.ServicesGroupBox.Controls.Add(this.subLocationTextBox);
			this.ServicesGroupBox.Controls.Add(this.serviceLocationAddressControl);
			this.ServicesGroupBox.Controls.Add(this.completedDateEdit);
			this.ServicesGroupBox.Controls.Add(this.bookedDateEdit);
			this.ServicesGroupBox.Controls.Add(this.serviceTypeDropEdit);
			this.ServicesGroupBox.Controls.Add(this.rateAndCurrencyCalcFindBox);
			this.ServicesGroupBox.Controls.Add(this.ServicesGrid);
			this.ServicesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesGroupBox.Name = "ServicesGroupBox";
			this.ServicesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 287, true);
			this.ServicesGroupBox.TabIndex = 0;
			this.ServicesGroupBox.TabStop = false;
			// 
			// measurementBasisDropEdit
			// 
			this.measurementBasisDropEdit.AllowDrop = true;
			this.measurementBasisDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.measurementBasisDropEdit, "Services.ES_MeasurementBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_MeasurementBasis)));
			this.measurementBasisDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.measurementBasisDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 242, true);
			this.measurementBasisDropEdit.Name = "measurementBasisDropEdit";
			this.measurementBasisDropEdit.PreBoundMaxLength = 11;
			this.measurementBasisDropEdit.ShowDescriptionBox = false;
			this.measurementBasisDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			this.measurementBasisDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.measurementBasisDropEdit.TabIndex = 10;
			// 
			// contractorFindBox
			// 
			this.contractorFindBox.AllowDrop = true;
			this.contractorFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.contractorFindBox, "Services.ES_OH_Contractor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_OH_Contractor)));
			this.contractorFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.contractorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 158, true);
			this.contractorFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.contractorFindBox.Name = "contractorFindBox";
			this.contractorFindBox.ShouldResize = true;
			this.contractorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.contractorFindBox.TabIndex = 2;
			// 
			// notesTextBox
			// 
			this.notesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.notesTextBox, "Services.ES_ServiceNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceNote)));
			this.notesTextBox.CaptionResourceString = null;
			this.notesTextBox.IsDynamicMultiline = true;
			this.notesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 263, true);
			this.notesTextBox.Multiline = true;
			this.notesTextBox.Name = "notesTextBox";
			this.notesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 19, true);
			this.notesTextBox.TabIndex = 11;
			// 
			// referenceTextBox
			// 
			this.referenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.referenceTextBox, "Services.ES_References");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_References)));
			this.referenceTextBox.CaptionResourceString = null;
			this.referenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 263, true);
			this.referenceTextBox.Name = "referenceTextBox";
			this.referenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.referenceTextBox.TabIndex = 12;
			// 
			// durationTimeEdit
			// 
			this.durationTimeEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.durationTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.durationTimeEdit, "Services.ES_Duration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Duration)));
			this.durationTimeEdit.CaptionResourceString = null;
			this.durationTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 221, true);
			this.durationTimeEdit.Name = "durationTimeEdit";
			this.durationTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.durationTimeEdit.TabIndex = 8;
			// 
			// serviceCountCalcEdit
			// 
			this.serviceCountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.serviceCountCalcEdit, "Services.ES_ServiceCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceCount)));
			this.serviceCountCalcEdit.CaptionResourceString = null;
			this.serviceCountCalcEdit.DecimalPlaces = 2;
			this.serviceCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 200, true);
			this.serviceCountCalcEdit.Name = "serviceCountCalcEdit";
			this.serviceCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.serviceCountCalcEdit.TabIndex = 6;
			this.serviceCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// subLocationTextBox
			// 
			this.subLocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.subLocationTextBox, "Services.ES_SubLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_SubLocation)));
			this.subLocationTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocServicesUserControl|ABC76D34-CDBF-4D6A-B4E2-A5B93412056D", "Sub Location");
			this.subLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 201, true);
			this.subLocationTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.subLocationTextBox.Name = "subLocationTextBox";
			this.subLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.subLocationTextBox.TabIndex = 5;
			// 
			// serviceLocationAddressControl
			// 
			this.serviceLocationAddressControl.AllowDrop = true;
			this.serviceLocationAddressControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.serviceLocationAddressControl, "Services.ES_OA_Location");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_OA_Location)));
			this.serviceLocationAddressControl.BindToOrgList = "Services.Lookups.ServiceProvider";
			this.serviceLocationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 179, true);
			this.serviceLocationAddressControl.Name = "serviceLocationAddressControl";
			this.serviceLocationAddressControl.PopupCaption = null;
			this.serviceLocationAddressControl.ReadOnly = false;
			this.serviceLocationAddressControl.ShowAddress = false;
			this.serviceLocationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.serviceLocationAddressControl.TabIndex = 3;
			// 
			// completedDateEdit
			// 
			this.completedDateEdit.AllowDrop = true;
			this.completedDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.completedDateEdit.AutoCompleteMonthThreshold = 1;
			this.completedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.completedDateEdit, "Services.ES_Completed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Completed)));
			this.completedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 221, true);
			this.completedDateEdit.Name = "completedDateEdit";
			this.completedDateEdit.TabIndex = 7;
			// 
			// bookedDateEdit
			// 
			this.bookedDateEdit.AllowDrop = true;
			this.bookedDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.bookedDateEdit.AutoCompleteMonthThreshold = 1;
			this.bookedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.bookedDateEdit, "Services.ES_Booked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Booked)));
			this.bookedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 200, true);
			this.bookedDateEdit.Name = "bookedDateEdit";
			this.bookedDateEdit.TabIndex = 4;
			// 
			// serviceTypeDropEdit
			// 
			this.serviceTypeDropEdit.AllowDrop = true;
			this.serviceTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.serviceTypeDropEdit, "Services.ES_ServiceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceCode)));
			this.serviceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 137, true);
			this.serviceTypeDropEdit.Name = "serviceTypeDropEdit";
			this.serviceTypeDropEdit.PreBoundMaxLength = 3;
			this.serviceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 20, true);
			this.serviceTypeDropEdit.TabIndex = 1;
			// 
			// rateAndCurrencyCalcFindBox
			// 
			this.rateAndCurrencyCalcFindBox.AllowDrop = true;
			this.rateAndCurrencyCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.rateAndCurrencyCalcFindBox.BindToAmount = "Services.ES_ServiceRate";
			this.rateAndCurrencyCalcFindBox.BindToUnit = "Services.ES_RX_NKServiceRateCurrency";
			this.rateAndCurrencyCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.rateAndCurrencyCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 242, true);
			this.rateAndCurrencyCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.rateAndCurrencyCalcFindBox.Name = "rateAndCurrencyCalcFindBox";
			this.rateAndCurrencyCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 20, true);
			this.rateAndCurrencyCalcFindBox.TabIndex = 9;
			// 
			// ServicesGrid
			// 
			this.ServicesGrid.AllowNavigation = false;
			this.ServicesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServicesGrid, "Services");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ParentContextID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Calc_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Booked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Completed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_OH_Contractor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ServiceProviderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Calc_LocationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_References)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceNote)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ServiceId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobService)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IHaveServices)(null)).Services)).SyncRoot)).ES_ExternalServiceId)));
			this.ServicesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ES_ServiceCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidDropEditColumnStyleInfo1.ColumnName = "ParentContextID";
			zGuidDropEditColumnStyleInfo1.IsVisible = false;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "ES_Calc_Description";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.ColumnName = "ES_Booked";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "ES_Completed";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ES_OH_Contractor";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ES_ServiceCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ServiceProviderPK";
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDropEditColumnStyleInfo2.ColumnName = "ES_Calc_LocationCode";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
			zTimeEditExColumnStyleInfo1.ColumnName = "ES_Duration";
			zTimeEditExColumnStyleInfo1.IsVisible = false;
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "ES_References";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "ES_ServiceNote";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo4.ColumnName = "ES_ServiceId";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "ES_ExternalServiceId";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ServicesGrid.GridId = "c2aa62eb-d145-4972-87d7-843e48a9221f";
			this.ServicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServicesGrid.LayoutKey = "ServicesGrid";
			this.ServicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 16, true);
			this.ServicesGrid.Name = "ServicesGrid";
			this.ServicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 118, true);
			this.ServicesGrid.TabIndex = 0;
			// 
			// DocServicesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ServicesGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 224, true);
			this.Name = "DocServicesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 287, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ServicesGroupBox.ResumeLayout(false);
			this.ServicesGroupBox.PerformLayout();
			this.measurementBasisDropEdit.ResumeLayout(true);
			this.measurementBasisDropEdit.PerformLayout();
			this.contractorFindBox.ResumeLayout(true);
			this.contractorFindBox.PerformLayout();
			this.serviceLocationAddressControl.ResumeLayout(true);
			this.serviceLocationAddressControl.PerformLayout();
			this.completedDateEdit.ResumeLayout(true);
			this.completedDateEdit.PerformLayout();
			this.bookedDateEdit.ResumeLayout(true);
			this.bookedDateEdit.PerformLayout();
			this.serviceTypeDropEdit.ResumeLayout(true);
			this.serviceTypeDropEdit.PerformLayout();
			this.rateAndCurrencyCalcFindBox.ResumeLayout(true);
			this.rateAndCurrencyCalcFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ServicesGrid)).EndInit();
			this.ServicesGrid.ResumeLayout(false);
			this.ServicesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid ServicesGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ServicesGroupBox;
		private ZArchitecture.GUI.ZCalcFindBox rateAndCurrencyCalcFindBox;
		private Enterprise.ZArchitecture.ZTextBox notesTextBox;
		private Enterprise.ZArchitecture.ZTextBox referenceTextBox;
		private Enterprise.ZArchitecture.GUI.ZTimeEdit durationTimeEdit;
		private Enterprise.ZArchitecture.ZCalcEdit serviceCountCalcEdit;
		private ZAddressControl serviceLocationAddressControl;
		private Enterprise.ZArchitecture.GUI.ZDateEdit completedDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit bookedDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit serviceTypeDropEdit;
		private ZOrganisationFindBox contractorFindBox;
		private ZDropEdit measurementBasisDropEdit;
		private Enterprise.ZArchitecture.ZTextBox subLocationTextBox;
	}
}
