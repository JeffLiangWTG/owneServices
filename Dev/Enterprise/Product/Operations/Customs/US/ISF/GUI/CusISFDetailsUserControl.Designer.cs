namespace Enterprise.Customs.US.ISF.GUI
{
	partial class CusISFDetailsUserControl
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

			if (isf5UserControl != null)
			{
				isf5UserControl.Dispose();
			}

			if (isf10UserControl != null)
			{
				isf10UserControl.Dispose();
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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;


			this.ISFTabContainerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ISFTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.zTabISFType = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zTabCustom = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ISFCustomFieldsControl1 = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainShipToPartyDocAddressControl = new Enterprise.Customs.US.ISF.GUI.ISFDocAddressControl();
			this.ReferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MasterBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OceanBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HouseBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainCustomsStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BF_LastAcceptedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BF_FirstAcceptedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BF_CustomsReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BF_ActionReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OwnerReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ImporterOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.SCACCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BF_TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BF_ShipmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BF_EntryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BF_JobReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();

			this.ISFTabContainerPanel.SuspendLayout();
			this.ISFTabControl.SuspendLayout();
			this.zTabCustom.SuspendLayout();

			this.TopPanel.SuspendLayout();
			this.ReferencesGroupBox.SuspendLayout();
			this.MainCustomsStatusGroupBox.SuspendLayout();
			this.MainDetailsGroupBox.SuspendLayout();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.CusISFHeader);
			this.Controls.Add(this.ISFTabContainerPanel);
			this.Controls.Add(this.TopPanel);
			// 
			// ISFTabContainerPanel
			// 
			this.ISFTabContainerPanel.Controls.Add(this.ISFTabControl);
			this.ISFTabContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ISFTabContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.ISFTabContainerPanel.Name = "ISFTabContainerPanel";
			this.ISFTabContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 416, true);
			this.ISFTabContainerPanel.TabIndex = 1;
			// 
			// ISFTabControl
			// 
			this.ISFTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ISFTabControl.Controls.Add(this.zTabISFType);
			this.ISFTabControl.Controls.Add(this.zTabCustom);
			this.ISFTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ISFTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ISFTabControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ISFTabControl.Name = "ISFTabControl";
			this.ISFTabControl.SelectedIndex = 0;
			this.ISFTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 416, true);
			this.ISFTabControl.TabIndex = 1;
			// 
			// zTabISFType
			// 
			this.zTabISFType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabISFType.Name = "zTabISFType";
			this.zTabISFType.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabISFType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 389, true);
			this.zTabISFType.TabIndex = 0;
			this.zTabISFType.UseVisualStyleBackColor = false;
			// 
			// zTabCustom
			// 
			this.zTabCustom.Controls.Add(this.ISFCustomFieldsControl1);
			this.zTabCustom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabCustom.Name = "zTabCustom";
			this.zTabCustom.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabCustom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 389, true);
			this.zTabCustom.TabIndex = 1;
			this.zTabCustom.Text = "Custom";
			this.zTabCustom.UseVisualStyleBackColor = false;
			// 
			// shipmentCustomFieldsControl1
			// 
			this.ISFCustomFieldsControl1.AllowDrop = true;
			this.ISFCustomFieldsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ISFCustomFieldsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ISFCustomFieldsControl1.Name = "shipmentCustomFieldsControl1";
			this.ISFCustomFieldsControl1.NothingSetupMessageLabelText = "To make use of this tab, please setup shipment custom field captions and hints in" +
	"...";
			this.ISFCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 383, true);
			this.ISFCustomFieldsControl1.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.ReferencesGroupBox);
			this.TopPanel.Controls.Add(this.MainCustomsStatusGroupBox);
			this.TopPanel.Controls.Add(this.MainShipToPartyDocAddressControl);
			this.TopPanel.Controls.Add(this.MainDetailsGroupBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 182, true);
			this.TopPanel.TabIndex = 0;
			// 
			// ReferencesGroupBox
			// 
			this.ReferencesGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISFForm|8cc77c85-ea16-49f0-8b7c-22252166a837", "References");
			this.ReferencesGroupBox.Controls.Add(this.MasterBillTextBox);
			this.ReferencesGroupBox.Controls.Add(this.OceanBillTextBox);
			this.ReferencesGroupBox.Controls.Add(this.HouseBillTextBox);
			this.ReferencesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 91, true);
			this.ReferencesGroupBox.Name = "ReferencesGroupBox";
			this.ReferencesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 91, true);
			this.ReferencesGroupBox.TabIndex = 2;
			this.ReferencesGroupBox.TabStop = false;
			// 
			// MasterBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterBillTextBox, "BF_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_MasterBill)));
			this.MasterBillTextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISFForm|8fdde9ed-46d3-43d2-ae8d-6e7ff3fcda38", "Master Bill", "Master Bill", "Master Bill", "");
			this.MasterBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 63, true);
			this.MasterBillTextBox.Name = "MasterBillTextBox";
			this.MasterBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 20, true);
			this.MasterBillTextBox.TabIndex = 2;
			// 
			// OceanBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.OceanBillTextBox, "BF_OceanBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_OceanBill)));
			this.OceanBillTextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISFForm|e95a243b-e2a0-415a-b8d7-a43ca299cbdd", "Ocean Bill", "Ocean Bill", "Ocean Bill", "");
			this.OceanBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 17, true);
			this.OceanBillTextBox.Name = "OceanBillTextBox";
			this.OceanBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 20, true);
			this.OceanBillTextBox.TabIndex = 0;
			// 
			// HouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.HouseBillTextBox, "BF_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_HouseBill)));
			this.HouseBillTextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISFForm|1ec6742e-4f5d-4294-8c4c-d11ae4335edd", "House Bill", "House Bill", "House Bill", "");
			this.HouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 40, true);
			this.HouseBillTextBox.Name = "HouseBillTextBox";
			this.HouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 20, true);
			this.HouseBillTextBox.TabIndex = 1;

			// 
			// MainCustomsStatusGroupBox
			// 
			this.MainCustomsStatusGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISFForm|1347bd6d-c072-4210-b7e2-1b0206b886cf", "Customs Det.", "Customs Details", "");
			this.MainCustomsStatusGroupBox.Controls.Add(this.BF_LastAcceptedDateDateEdit);
			this.MainCustomsStatusGroupBox.Controls.Add(this.BF_FirstAcceptedDateDateEdit);
			this.MainCustomsStatusGroupBox.Controls.Add(this.StatusTextBox);
			this.MainCustomsStatusGroupBox.Controls.Add(this.BF_CustomsReferenceTextBox);
			this.MainCustomsStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 0, true);
			this.MainCustomsStatusGroupBox.Name = "MainCustomsStatusGroupBox";
			this.MainCustomsStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 85, true);
			this.MainCustomsStatusGroupBox.TabIndex = 1;
			this.MainCustomsStatusGroupBox.TabStop = false;
			// 
			// BF_LastAcceptedDateDateEdit
			// 
			this.BF_LastAcceptedDateDateEdit.AllowDrop = true;
			this.BF_LastAcceptedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BF_LastAcceptedDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BF_LastAcceptedDateDateEdit, "BF_LastAcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_LastAcceptedDate)));
			this.BF_LastAcceptedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 59, true);
			this.BF_LastAcceptedDateDateEdit.Name = "BF_LastAcceptedDateDateEdit";
			this.BF_LastAcceptedDateDateEdit.TabIndex = 3;
			// 
			// BF_FirstAcceptedDateDateEdit
			// 
			this.BF_FirstAcceptedDateDateEdit.AllowDrop = true;
			this.BF_FirstAcceptedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BF_FirstAcceptedDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BF_FirstAcceptedDateDateEdit, "BF_FirstAcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_FirstAcceptedDate)));
			this.BF_FirstAcceptedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 59, true);
			this.BF_FirstAcceptedDateDateEdit.Name = "BF_FirstAcceptedDateDateEdit";
			this.BF_FirstAcceptedDateDateEdit.TabIndex = 2;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.AcceptsReturn = true;
			this.BindingSource.SetBindingMember(this.StatusTextBox, "BF_CustomsStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_CustomsStatusDescription)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISFForm|5ac78acc-6277-4c42-8fd0-02aaf45c6b11", "Status", "Status", "Status", "");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 38, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 20, true);
			this.StatusTextBox.TabIndex = 1;
			// 
			// BF_CustomsReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.BF_CustomsReferenceTextBox, "BF_CustomsReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_CustomsReference)));
			this.BF_CustomsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 17, true);
			this.BF_CustomsReferenceTextBox.Name = "BF_CustomsReferenceTextBox";
			this.BF_CustomsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.BF_CustomsReferenceTextBox.TabIndex = 0;

			// 
			// MainShipToPartyDocAddressControl
			// 
			this.MainShipToPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MainShipToPartyDocAddressControl, "MainShipToParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).MainShipToParty)));
			this.MainShipToPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISFForm|430bd527-c1da-457e-92cb-69570707e054", "Ship To Party");
			this.MainShipToPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(719, 0, true);
			this.MainShipToPartyDocAddressControl.BindToOrganisations = "Lookups+Organisations";
			this.MainShipToPartyDocAddressControl.Name = "MainShipToPartyDocAddressControl";
			this.MainShipToPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.MainShipToPartyDocAddressControl.TabIndex = 3;
			// 
			// MainDetailsGroupBox
			// 
			this.MainDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISFForm|55a62063-1764-4dd6-aff9-d1a8b007b810", "Main Details");
			this.MainDetailsGroupBox.Controls.Add(this.BF_ActionReasonCodeDropEdit);
			this.MainDetailsGroupBox.Controls.Add(this.OwnerReferenceTextBox);
			this.MainDetailsGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.MainDetailsGroupBox.Controls.Add(this.ImporterOrganisationFindBox);
			this.MainDetailsGroupBox.Controls.Add(this.SCACCodeFindBox);
			this.MainDetailsGroupBox.Controls.Add(this.BF_TransportModeDropEdit);
			this.MainDetailsGroupBox.Controls.Add(this.BF_ShipmentTypeDropEdit);
			this.MainDetailsGroupBox.Controls.Add(this.BF_EntryTypeDropEdit);
			this.MainDetailsGroupBox.Controls.Add(this.BF_JobReferenceTextBox);
			this.MainDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainDetailsGroupBox.Name = "MainDetailsGroupBox";
			this.MainDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 182, true);
			this.MainDetailsGroupBox.TabIndex = 0;
			this.MainDetailsGroupBox.TabStop = false;
			// 
			// BF_ActionReasonCodeDropEdit
			// 
			this.BF_ActionReasonCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BF_ActionReasonCodeDropEdit, "BF_ActionReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_ActionReasonCode)));
			this.BF_ActionReasonCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("ISFForm|8305dd66-197f-4a78-9e08-d0760025dd69", "Act Reason");
			this.BF_ActionReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 155, true);
			this.BF_ActionReasonCodeDropEdit.Name = "BF_ActionReasonCodeDropEdit";
			this.BF_ActionReasonCodeDropEdit.PreBoundMaxLength = 2;
			this.BF_ActionReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.BF_ActionReasonCodeDropEdit.TabIndex = 8;
			// 
			// OwnerReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerReferenceTextBox, "BF_OwnerReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_OwnerReference)));
			this.OwnerReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 132, true);
			this.OwnerReferenceTextBox.Name = "OwnerReferenceTextBox";
			this.OwnerReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.OwnerReferenceTextBox.TabIndex = 7;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "BF_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_GB)));
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 86, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.ShowDescriptionBox = false;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.BranchGuidFindBox.TabIndex = 4;
			// 
			// ImporterOrganisationFindBox
			// 
			this.ImporterOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrganisationFindBox, "BF_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_OH_Importer)));
			this.ImporterOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 109, true);
			this.ImporterOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ImporterOrganisationFindBox.Name = "ImporterOrganisationFindBox";
			this.ImporterOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.ImporterOrganisationFindBox.TabIndex = 6;
			// 
			// SCACCodeFindBox
			// 
			this.SCACCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SCACCodeFindBox, "BF_SCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_SCAC)));
			this.SCACCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 86, true);
			this.SCACCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Carrier;
			this.SCACCodeFindBox.Name = "SCACCodeFindBox";
			this.SCACCodeFindBox.PreBoundMaxLength = 4;
			this.SCACCodeFindBox.ShowDescriptionBox = false;
			this.SCACCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.SCACCodeFindBox.TabIndex = 5;
			// 
			// BF_TransportModeDropEdit
			// 
			this.BF_TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BF_TransportModeDropEdit, "BF_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_TransportMode)));
			this.BF_TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 63, true);
			this.BF_TransportModeDropEdit.Name = "BF_TransportModeDropEdit";
			this.BF_TransportModeDropEdit.PreBoundMaxLength = 2;
			this.BF_TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.BF_TransportModeDropEdit.TabIndex = 3;
			// 
			// BF_ShipmentTypeDropEdit
			// 
			this.BF_ShipmentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BF_ShipmentTypeDropEdit, "BF_ShipmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_ShipmentType)));
			this.BF_ShipmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 40, true);
			this.BF_ShipmentTypeDropEdit.Name = "BF_ShipmentTypeDropEdit";
			this.BF_ShipmentTypeDropEdit.PreBoundMaxLength = 2;
			this.BF_ShipmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.BF_ShipmentTypeDropEdit.TabIndex = 2;
			// 
			// BF_EntryTypeDropEdit
			// 
			this.BF_EntryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BF_EntryTypeDropEdit, "BF_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_EntryType)));
			this.BF_EntryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 17, true);
			this.BF_EntryTypeDropEdit.Name = "BF_EntryTypeDropEdit";
			this.BF_EntryTypeDropEdit.PreBoundMaxLength = 2;
			this.BF_EntryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.BF_EntryTypeDropEdit.TabIndex = 1;
			// 
			// BF_JobReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.BF_JobReferenceTextBox, "BF_JobReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).BF_JobReference)));
			this.BF_JobReferenceTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.BF_JobReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BF_JobReferenceTextBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.BF_JobReferenceTextBox.ForeColor = System.Drawing.Color.MediumBlue;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BF_JobReferenceTextBox, false);
			this.BF_JobReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 0, true);
			this.BF_JobReferenceTextBox.Name = "BF_JobReferenceTextBox";
			this.BF_JobReferenceTextBox.ReadOnly = true;
			this.BF_JobReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 13, true);
			this.BF_JobReferenceTextBox.TabIndex = 0;
			this.BF_JobReferenceTextBox.Text = "<jobreference>";

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ISFTabContainerPanel.ResumeLayout(false);
			this.ISFTabControl.ResumeLayout(false);

			this.zTabCustom.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.ReferencesGroupBox.ResumeLayout(false);
			this.ReferencesGroupBox.PerformLayout();
			this.MainCustomsStatusGroupBox.ResumeLayout(false);
			this.MainCustomsStatusGroupBox.PerformLayout();
			this.MainDetailsGroupBox.ResumeLayout(false);
			this.MainDetailsGroupBox.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel ISFTabContainerPanel;
		private ZArchitecture.GUI.ZTabControl ISFTabControl;
		private ZArchitecture.GUI.ZTabPage zTabISFType;
		private ZArchitecture.GUI.ZTabPage zTabCustom;
		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl ISFCustomFieldsControl1;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private ISFDocAddressControl MainShipToPartyDocAddressControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ReferencesGroupBox;
		private Enterprise.ZArchitecture.ZTextBox MasterBillTextBox;
		private Enterprise.ZArchitecture.ZTextBox OceanBillTextBox;
		private Enterprise.ZArchitecture.ZTextBox HouseBillTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MainCustomsStatusGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit BF_LastAcceptedDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit BF_FirstAcceptedDateDateEdit;
		private Enterprise.ZArchitecture.ZTextBox StatusTextBox;
		private Enterprise.ZArchitecture.ZTextBox BF_CustomsReferenceTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MainDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit BF_ActionReasonCodeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox OwnerReferenceTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox ImporterOrganisationFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox SCACCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit BF_TransportModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit BF_ShipmentTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit BF_EntryTypeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox BF_JobReferenceTextBox;
	}
}
