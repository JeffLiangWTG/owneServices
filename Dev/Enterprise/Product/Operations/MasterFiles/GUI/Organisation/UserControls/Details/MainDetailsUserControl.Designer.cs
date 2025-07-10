using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.MasterFiles.GUI
{
	public partial class MainDetailsUserControl
	{

		#region Component Designer generated code

		protected Enterprise.ZArchitecture.GUI.ZGroupBox OrgTypeGroupBox;
		protected Enterprise.ZArchitecture.ZLabel OrgTypeSplitterLabel1;
		protected Enterprise.ZArchitecture.ZLabel OrgTypeSplitterLabel2;
		protected Enterprise.ZArchitecture.ZLabel OrgTypeSplitterLabel3;
		protected Enterprise.ZArchitecture.ZLabel OrgTypeSplitterLabel4;
		protected Enterprise.ZArchitecture.ZLabel OrgTypeSplitterLabel5;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsCompetitorBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsMiscFreightServicesBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsSalesLeadBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsWarehouseClientBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsTransportClientBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsBrokerBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsForwarderBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsShippingProviderBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsConsignorBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsConsigneeBoundCheckEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsCreditorBoundCheckEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsDebtorBoundCheckEdit;
		internal CargoWise.Windows.UI.KFlowLayoutPanel orgTypesflowLayoutPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel MainDetailsPanel;
		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl DetailsTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		public NameAndAddressDetailsUserControl NameAndAddressDetailsControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage StaffAssignmentsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage SecurityTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage AutoRatingAndCompanyTariffTabPage;
		private StaffAssignmentsUserControl staffAssignmentsUserControl1;
		private RelatedPartiesUserControl relatedPartiesUserControl;
		private WebURLsUserControl webURLsUserControl;
		private WebSecurityUserControl webSecurityUserControl1;
		private CustomFieldsUserControl customFieldsUserControl1;
		internal Enterprise.MasterFiles.GUI.Organisation.UserControls.OrganisationRatingUserControl organisationRatingUserControl1;
		internal Enterprise.ZArchitecture.ZLabel AutoRatingAndCompanyTariffNotAvailableLabel;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsNationalAccountBoundCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsActiveBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsTempAccountBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsGlobalAccountCheckbox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsControllingAgentBoundCheckEdit;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OH_IsControllingCustomerBoundCheckEdit;
		internal Enterprise.ZArchitecture.GUI.ZTabPage ConfigTabPage;
		internal ConfigUserControl ConfigUserControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage RelatedPartiesTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage OrgWebURLsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.OrgTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.orgTypesflowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.OH_IsActiveBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsNationalAccountBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsGlobalAccountCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsTempAccountBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OrgTypeSplitterLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.OH_IsDebtorBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsCreditorBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OrgTypeSplitterLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.OH_IsConsignorBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsConsigneeBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsTransportClientBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsWarehouseClientBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OrgTypeSplitterLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.OH_IsShippingProviderBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsForwarderBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsBrokerBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsMiscFreightServicesBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsCompetitorBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OrgTypeSplitterLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.OH_IsSalesLeadBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OrgTypeSplitterLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.OH_IsControllingCustomerBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OH_IsControllingAgentBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StaffAssignmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecurityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AutoRatingAndCompanyTariffTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConfigTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RelatedPartiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OrgWebURLsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrgTypeGroupBox.SuspendLayout();
			this.orgTypesflowLayoutPanel.SuspendLayout();
			this.MainDetailsPanel.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// OrgTypeGroupBox
			// 
			this.OrgTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
			this.OrgTypeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|2258818e-308a-4082-8bbb-5a69e545ca8c", "Organization Type");
			this.OrgTypeGroupBox.Controls.Add(this.orgTypesflowLayoutPanel);
			this.OrgTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(594, 3, true);
			this.OrgTypeGroupBox.Name = "OrgTypeGroupBox";
			this.OrgTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 597, true);
			this.OrgTypeGroupBox.TabIndex = 0;
			this.OrgTypeGroupBox.TabStop = false;
			// 
			// orgTypesflowLayoutPanel
			// 
			this.orgTypesflowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsActiveBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsNationalAccountBoundCheckBox);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsGlobalAccountCheckbox);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsTempAccountBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OrgTypeSplitterLabel1);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsDebtorBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsCreditorBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OrgTypeSplitterLabel2);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsConsignorBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsConsigneeBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsTransportClientBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsWarehouseClientBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OrgTypeSplitterLabel3);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsShippingProviderBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsForwarderBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsBrokerBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsMiscFreightServicesBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsCompetitorBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OrgTypeSplitterLabel4);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsSalesLeadBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OrgTypeSplitterLabel5);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsControllingCustomerBoundCheckEdit);
			this.orgTypesflowLayoutPanel.Controls.Add(this.OH_IsControllingAgentBoundCheckEdit);
			this.orgTypesflowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.orgTypesflowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.orgTypesflowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 2, true);
			this.orgTypesflowLayoutPanel.Name = "orgTypesflowLayoutPanel";
			this.orgTypesflowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 574, true);
			this.orgTypesflowLayoutPanel.TabIndex = 34;
			this.orgTypesflowLayoutPanel.WrapContents = false;
			this.orgTypesflowLayoutPanel.AutoScroll = true;
			this.orgTypesflowLayoutPanel.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 560, true);
			// 
			// OH_IsActiveBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsActiveBoundCheckEdit, "OH_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsActive)));
			this.OH_IsActiveBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsActiveBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsActiveBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 8, true);
			this.OH_IsActiveBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsActiveBoundCheckEdit.Name = "OH_IsActiveBoundCheckEdit";
			this.OH_IsActiveBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsActiveBoundCheckEdit.TabIndex = 0;
			this.OH_IsActiveBoundCheckEdit.UseVisualStyleBackColor = false;
			// 
			// OH_IsNationalAccountBoundCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsNationalAccountBoundCheckBox, "OH_IsNationalAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsNationalAccount)));
			this.OH_IsNationalAccountBoundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsNationalAccountBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsNationalAccountBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 36, true);
			this.OH_IsNationalAccountBoundCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsNationalAccountBoundCheckBox.Name = "OH_IsNationalAccountBoundCheckBox";
			this.OH_IsNationalAccountBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsNationalAccountBoundCheckBox.TabIndex = 1;
			// 
			// OH_IsGlobalAccountCheckbox
			// 
			this.BindingSource.SetBindingMember(this.OH_IsGlobalAccountCheckbox, "OH_IsGlobalAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsGlobalAccount)));
			this.OH_IsGlobalAccountCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsGlobalAccountCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsGlobalAccountCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 64, true);
			this.OH_IsGlobalAccountCheckbox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsGlobalAccountCheckbox.Name = "OH_IsGlobalAccountCheckbox";
			this.OH_IsGlobalAccountCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsGlobalAccountCheckbox.TabIndex = 2;
			// 
			// OH_IsTempAccountBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsTempAccountBoundCheckEdit, "OH_IsTempAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsTempAccount)));
			this.OH_IsTempAccountBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsTempAccountBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsTempAccountBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 92, true);
			this.OH_IsTempAccountBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsTempAccountBoundCheckEdit.Name = "OH_IsTempAccountBoundCheckEdit";
			this.OH_IsTempAccountBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsTempAccountBoundCheckEdit.TabIndex = 3;
			// 
			// OrgTypeSplitterLabel1
			// 
			this.OrgTypeSplitterLabel1.BackColor = System.Drawing.SystemColors.Control;
			this.OrgTypeSplitterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.OrgTypeSplitterLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrgTypeSplitterLabel1, false);
			this.OrgTypeSplitterLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 120, true);
			this.OrgTypeSplitterLabel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OrgTypeSplitterLabel1.Name = "OrgTypeSplitterLabel1";
			this.OrgTypeSplitterLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 3, true);
			this.OrgTypeSplitterLabel1.TabIndex = 27;
			// 
			// OH_IsDebtorBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsDebtorBoundCheckEdit, "OH_IsDebtor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsDebtor)));
			this.OH_IsDebtorBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsDebtorBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsDebtorBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 131, true);
			this.OH_IsDebtorBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsDebtorBoundCheckEdit.Name = "OH_IsDebtorBoundCheckEdit";
			this.OH_IsDebtorBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsDebtorBoundCheckEdit.TabIndex = 4;
			// 
			// OH_IsCreditorBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsCreditorBoundCheckEdit, "OH_IsCreditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsCreditor)));
			this.OH_IsCreditorBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsCreditorBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsCreditorBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 159, true);
			this.OH_IsCreditorBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsCreditorBoundCheckEdit.Name = "OH_IsCreditorBoundCheckEdit";
			this.OH_IsCreditorBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsCreditorBoundCheckEdit.TabIndex = 5;
			// 
			// OrgTypeSplitterLabel2
			// 
			this.OrgTypeSplitterLabel2.BackColor = System.Drawing.SystemColors.Control;
			this.OrgTypeSplitterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.OrgTypeSplitterLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrgTypeSplitterLabel2, false);
			this.OrgTypeSplitterLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 187, true);
			this.OrgTypeSplitterLabel2.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OrgTypeSplitterLabel2.Name = "OrgTypeSplitterLabel2";
			this.OrgTypeSplitterLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 3, true);
			this.OrgTypeSplitterLabel2.TabIndex = 28;
			// 
			// OH_IsConsignorBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsConsignorBoundCheckEdit, "OH_IsConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsConsignor)));
			this.OH_IsConsignorBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsConsignorBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsConsignorBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 198, true);
			this.OH_IsConsignorBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsConsignorBoundCheckEdit.Name = "OH_IsConsignorBoundCheckEdit";
			this.OH_IsConsignorBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsConsignorBoundCheckEdit.TabIndex = 6;
			// 
			// OH_IsConsigneeBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsConsigneeBoundCheckEdit, "OH_IsConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsConsignee)));
			this.OH_IsConsigneeBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsConsigneeBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsConsigneeBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 226, true);
			this.OH_IsConsigneeBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsConsigneeBoundCheckEdit.Name = "OH_IsConsigneeBoundCheckEdit";
			this.OH_IsConsigneeBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsConsigneeBoundCheckEdit.TabIndex = 7;
			// 
			// OH_IsTransportClientBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsTransportClientBoundCheckEdit, "OH_IsTransportClient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsTransportClient)));
			this.OH_IsTransportClientBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsTransportClientBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsTransportClientBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 254, true);
			this.OH_IsTransportClientBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsTransportClientBoundCheckEdit.Name = "OH_IsTransportClientBoundCheckEdit";
			this.OH_IsTransportClientBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsTransportClientBoundCheckEdit.TabIndex = 8;
			// 
			// OH_IsWarehouseClientBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsWarehouseClientBoundCheckEdit, "OH_IsWarehouseClient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsWarehouseClient)));
			this.OH_IsWarehouseClientBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsWarehouseClientBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsWarehouseClientBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 282, true);
			this.OH_IsWarehouseClientBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsWarehouseClientBoundCheckEdit.Name = "OH_IsWarehouseClientBoundCheckEdit";
			this.OH_IsWarehouseClientBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsWarehouseClientBoundCheckEdit.TabIndex = 9;
			if (WarehouseDataRegistry.Instance.EnableContainerYard.Value)
			{
				this.OH_IsWarehouseClientBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|fc573309-4d40-4ad7-9ef3-dbbf4db02dad", "Whs/Facility");
			}
			else
			{
				this.OH_IsWarehouseClientBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|1f823735-7107-4aa1-a822-5e56d8f7b4f2", "Warehouse");
			}
			// 
			// OrgTypeSplitterLabel3
			// 
			this.OrgTypeSplitterLabel3.BackColor = System.Drawing.SystemColors.Control;
			this.OrgTypeSplitterLabel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.OrgTypeSplitterLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrgTypeSplitterLabel3, false);
			this.OrgTypeSplitterLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 310, true);
			this.OrgTypeSplitterLabel3.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OrgTypeSplitterLabel3.Name = "OrgTypeSplitterLabel3";
			this.OrgTypeSplitterLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 3, true);
			this.OrgTypeSplitterLabel3.TabIndex = 29;
			// 
			// OH_IsShippingProviderBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsShippingProviderBoundCheckEdit, "OH_IsShippingProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsShippingProvider)));
			this.OH_IsShippingProviderBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsShippingProviderBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsShippingProviderBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 321, true);
			this.OH_IsShippingProviderBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsShippingProviderBoundCheckEdit.Name = "OH_IsShippingProviderBoundCheckEdit";
			this.OH_IsShippingProviderBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsShippingProviderBoundCheckEdit.TabIndex = 10;
			// 
			// OH_IsForwarderBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsForwarderBoundCheckEdit, "OH_IsForwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsForwarder)));
			this.OH_IsForwarderBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsForwarderBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsForwarderBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 349, true);
			this.OH_IsForwarderBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsForwarderBoundCheckEdit.Name = "OH_IsForwarderBoundCheckEdit";
			this.OH_IsForwarderBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsForwarderBoundCheckEdit.TabIndex = 11;
			// 
			// OH_IsBrokerBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsBrokerBoundCheckEdit, "OH_IsBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsBroker)));
			this.OH_IsBrokerBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsBrokerBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsBrokerBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 377, true);
			this.OH_IsBrokerBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsBrokerBoundCheckEdit.Name = "OH_IsBrokerBoundCheckEdit";
			this.OH_IsBrokerBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsBrokerBoundCheckEdit.TabIndex = 12;
			// 
			// OH_IsMiscFreightServicesBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsMiscFreightServicesBoundCheckEdit, "OH_IsMiscFreightServices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsMiscFreightServices)));
			this.OH_IsMiscFreightServicesBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsMiscFreightServicesBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsMiscFreightServicesBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 405, true);
			this.OH_IsMiscFreightServicesBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsMiscFreightServicesBoundCheckEdit.Name = "OH_IsMiscFreightServicesBoundCheckEdit";
			this.OH_IsMiscFreightServicesBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsMiscFreightServicesBoundCheckEdit.TabIndex = 13;
			// 
			// OH_IsCompetitorBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsCompetitorBoundCheckEdit, "OH_IsCompetitor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsCompetitor)));
			this.OH_IsCompetitorBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsCompetitorBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsCompetitorBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 433, true);
			this.OH_IsCompetitorBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsCompetitorBoundCheckEdit.Name = "OH_IsCompetitorBoundCheckEdit";
			this.OH_IsCompetitorBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsCompetitorBoundCheckEdit.TabIndex = 14;
			// 
			// OrgTypeSplitterLabel4
			// 
			this.OrgTypeSplitterLabel4.BackColor = System.Drawing.SystemColors.Control;
			this.OrgTypeSplitterLabel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.OrgTypeSplitterLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrgTypeSplitterLabel4, false);
			this.OrgTypeSplitterLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 461, true);
			this.OrgTypeSplitterLabel4.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OrgTypeSplitterLabel4.Name = "OrgTypeSplitterLabel4";
			this.OrgTypeSplitterLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 3, true);
			this.OrgTypeSplitterLabel4.TabIndex = 30;
			// 
			// OH_IsSalesLeadBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsSalesLeadBoundCheckEdit, "OH_IsSalesLead");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsSalesLead)));
			this.OH_IsSalesLeadBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsSalesLeadBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsSalesLeadBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 472, true);
			this.OH_IsSalesLeadBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsSalesLeadBoundCheckEdit.Name = "OH_IsSalesLeadBoundCheckEdit";
			this.OH_IsSalesLeadBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsSalesLeadBoundCheckEdit.TabIndex = 15;
			// 
			// OrgTypeSplitterLabel5
			// 
			this.OrgTypeSplitterLabel5.BackColor = System.Drawing.SystemColors.Control;
			this.OrgTypeSplitterLabel5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.OrgTypeSplitterLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrgTypeSplitterLabel5, false);
			this.OrgTypeSplitterLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 500, true);
			this.OrgTypeSplitterLabel5.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OrgTypeSplitterLabel5.Name = "OrgTypeSplitterLabel5";
			this.OrgTypeSplitterLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 3, true);
			this.OrgTypeSplitterLabel5.TabIndex = 31;
			// 
			// OH_IsControllingCustomerBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsControllingCustomerBoundCheckEdit, "OH_IsControllingCustomer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsControllingCustomer)));
			this.OH_IsControllingCustomerBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsControllingCustomerBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsControllingCustomerBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 511, true);
			this.OH_IsControllingCustomerBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsControllingCustomerBoundCheckEdit.Name = "OH_IsControllingCustomerBoundCheckEdit";
			this.OH_IsControllingCustomerBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsControllingCustomerBoundCheckEdit.TabIndex = 32;
			// 
			// OH_IsControllingAgentBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.OH_IsControllingAgentBoundCheckEdit, "OH_IsControllingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OH_IsControllingAgent)));
			this.OH_IsControllingAgentBoundCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OH_IsControllingAgentBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OH_IsControllingAgentBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 539, true);
			this.OH_IsControllingAgentBoundCheckEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(35, 8, 0, 0, true);
			this.OH_IsControllingAgentBoundCheckEdit.Name = "OH_IsControllingAgentBoundCheckEdit";
			this.OH_IsControllingAgentBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.OH_IsControllingAgentBoundCheckEdit.TabIndex = 33;
			// 
			// MainDetailsPanel
			// 
			this.MainDetailsPanel.Controls.Add(this.DetailsTabControl);
			this.MainDetailsPanel.Controls.Add(this.OrgTypeGroupBox);
			this.MainDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.MainDetailsPanel.Name = "MainDetailsPanel";
			this.MainDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 603, true);
			this.MainDetailsPanel.TabIndex = 15;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Controls.Add(this.DetailsTabPage);
			this.DetailsTabControl.Controls.Add(this.StaffAssignmentsTabPage);
			this.DetailsTabControl.Controls.Add(this.SecurityTabPage);
			this.DetailsTabControl.Controls.Add(this.AutoRatingAndCompanyTariffTabPage);
			this.DetailsTabControl.Controls.Add(this.ConfigTabPage);
			this.DetailsTabControl.Controls.Add(this.RelatedPartiesTabPage);
			this.DetailsTabControl.Controls.Add(this.OrgWebURLsTabPage);
			this.DetailsTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 597, true);
			this.DetailsTabControl.TabIndex = 1;
			this.DetailsTabControl.SelectedIndexChanged += new System.EventHandler(this.DetailsTabControl_SelectedIndexChanged);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|c43bc3ae-5cb5-49f6-8a84-ee4209bea178", "Details");
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 570, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// 
			// StaffAssignmentsTabPage
			// 
			this.StaffAssignmentsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|9263345e-5527-4219-bd03-923bee78c08c", "Staff Assignments");
			this.StaffAssignmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StaffAssignmentsTabPage.Name = "StaffAssignmentsTabPage";
			this.StaffAssignmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 570, true);
			this.StaffAssignmentsTabPage.TabIndex = 1;
			this.StaffAssignmentsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.StaffAssignmentsTabPage_InitializeTab));
			// 
			// SecurityTabPage
			// 
			this.SecurityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|cad9d73f-a584-4893-8400-a15c7d26ac0d", "Web Security");
			this.SecurityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SecurityTabPage.Name = "SecurityTabPage";
			this.SecurityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 570, true);
			this.SecurityTabPage.TabIndex = 2;
			this.SecurityTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.SecurityTabPage_InitializeTab));
			// 
			// AutoRatingAndCompanyTariffTabPage
			// 
			this.AutoRatingAndCompanyTariffTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|5089d664-2c83-457b-ae4a-49cf561585e7", "Rating");
			this.AutoRatingAndCompanyTariffTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AutoRatingAndCompanyTariffTabPage.Name = "AutoRatingAndCompanyTariffTabPage";
			this.AutoRatingAndCompanyTariffTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 570, true);
			this.AutoRatingAndCompanyTariffTabPage.TabIndex = 3;
			this.AutoRatingAndCompanyTariffTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AutoRatingAndCompanyTariffTabPage_InitializeTab));
			// 
			// ConfigTabPage
			// 
			this.ConfigTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|36a3be39-7d64-42fd-a856-64a86fa71570", "Config");
			this.ConfigTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConfigTabPage.Name = "ConfigTabPage";
			this.ConfigTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 570, true);
			this.ConfigTabPage.TabIndex = 4;
			this.ConfigTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ConfigTabPage_InitializeTab));
			// 
			// RelatedPartiesTabPage
			// 
			this.RelatedPartiesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|66c6ceef-631d-4c32-abe0-9a1b80b7cec2", "Related Parties");
			this.RelatedPartiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedPartiesTabPage.Name = "RelatedPartiesTabPage";
			this.RelatedPartiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RelatedPartiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 570, true);
			this.RelatedPartiesTabPage.TabIndex = 5;
			this.RelatedPartiesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RelatedPartiesTabPage_InitializeTab));
			// 
			// OrgWebURLsTabPage
			// 
			this.OrgWebURLsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|eb2be7ab-b1dd-495a-be31-ee58e1ae9c55", "Web");
			this.OrgWebURLsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrgWebURLsTabPage.Name = "OrgWebURLsTabPage";
			this.OrgWebURLsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OrgWebURLsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 570, true);
			this.OrgWebURLsTabPage.TabIndex = 6;
			this.OrgWebURLsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.OrgWebURLsTabPage_InitializeTab));
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|5c508251-4c27-4597-aa44-ad9c1d52feca", "Custom Fields");
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 570, true);
			this.CustomFieldsTabPage.TabIndex = 7;
			this.CustomFieldsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CustomFieldsTabPage_InitializeTab));
			// 
			// MainDetailsUserControl
			// 
			this.Controls.Add(this.MainDetailsPanel);
			this.IsModifyConfig = true;
			this.IsModifyConfigBrandsAndCompanyNames = true;
			this.IsModifyConfigEDICodeMapping = true;
			this.IsModifyConfigGeneral = true;
			this.IsModifyConfigRegistrationNumbers = true;
			this.IsModifyDetails = true;
			this.IsModifyDetailsCustomFields = true;
			this.IsModifyDetailsNameAndAddress = true;
			this.IsModifyDetailsOrganisationType = true;
			this.IsModifyDetailsPhFaxWebDetails = true;
			this.IsModifyDetailsRatingAndTariffs = true;
			this.IsModifyDetailsStaffAssignments = true;
			this.IsModifyDetailsWebSecurity = true;
			this.IsNewDetailsWebSecurity = true;
			this.Name = "MainDetailsUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 627, true);
			this.Load += new System.EventHandler(this.MainDetailsUserControl_Load);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.MainDetailsPanel, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrgTypeGroupBox.ResumeLayout(false);
			this.OrgTypeGroupBox.PerformLayout();
			this.orgTypesflowLayoutPanel.ResumeLayout(false);
			this.orgTypesflowLayoutPanel.PerformLayout();
			this.MainDetailsPanel.ResumeLayout(false);
			this.MainDetailsPanel.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void DetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NameAndAddressDetailsControl = new Enterprise.MasterFiles.GUI.NameAndAddressDetailsUserControl();
			this.DetailsTabPage.SuspendLayout();
			this.NameAndAddressDetailsControl.SuspendLayout();
			this.DetailsTabPage.Controls.Add(this.NameAndAddressDetailsControl);
			// 
			// NameAndAddressDetailsControl
			// 
			this.NameAndAddressDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NameAndAddressDetailsControl, ".");
			this.NameAndAddressDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NameAndAddressDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NameAndAddressDetailsControl.Name = "NameAndAddressDetailsControl";
			this.NameAndAddressDetailsControl.ReadOnly = false;
			this.NameAndAddressDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.NameAndAddressDetailsControl.TabIndex = 0;
			this.NameAndAddressDetailsControl.ValidationJustForced = false;
			this.DetailsTabPage.PerformLayout();
			this.NameAndAddressDetailsControl.ResumeLayout(true);
			this.NameAndAddressDetailsControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(true);
		}

		private void StaffAssignmentsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.staffAssignmentsUserControl1 = new Enterprise.MasterFiles.GUI.StaffAssignmentsUserControl();
			this.StaffAssignmentsTabPage.SuspendLayout();
			this.staffAssignmentsUserControl1.SuspendLayout();
			this.StaffAssignmentsTabPage.Controls.Add(this.staffAssignmentsUserControl1);
			// 
			// staffAssignmentsUserControl1
			// 
			this.staffAssignmentsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.staffAssignmentsUserControl1, ".");
			this.staffAssignmentsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.staffAssignmentsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.staffAssignmentsUserControl1.Name = "staffAssignmentsUserControl1";
			this.staffAssignmentsUserControl1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.staffAssignmentsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.staffAssignmentsUserControl1.TabIndex = 0;
			this.StaffAssignmentsTabPage.PerformLayout();
			this.staffAssignmentsUserControl1.ResumeLayout(true);
			this.staffAssignmentsUserControl1.PerformLayout();
			this.StaffAssignmentsTabPage.ResumeLayout(true);
		}

		private void OrgWebURLsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.webURLsUserControl = new Enterprise.MasterFiles.GUI.WebURLsUserControl();
			this.OrgWebURLsTabPage.SuspendLayout();
			this.webURLsUserControl.SuspendLayout();
			this.OrgWebURLsTabPage.Controls.Add(this.webURLsUserControl);
			// 
			// webURLsUserControl
			// 
			this.webURLsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.webURLsUserControl, ".");
			this.webURLsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webURLsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.webURLsUserControl.Name = "webURLsUserControl";
			this.webURLsUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.webURLsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 67, true);
			this.webURLsUserControl.TabIndex = 0;
			this.OrgWebURLsTabPage.PerformLayout();
			this.webURLsUserControl.ResumeLayout(true);
			this.webURLsUserControl.PerformLayout();
			this.OrgWebURLsTabPage.ResumeLayout(true);
		}

		private void RelatedPartiesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.relatedPartiesUserControl = new Enterprise.MasterFiles.GUI.RelatedPartiesUserControl();
			this.RelatedPartiesTabPage.SuspendLayout();
			this.relatedPartiesUserControl.SuspendLayout();
			this.RelatedPartiesTabPage.Controls.Add(this.relatedPartiesUserControl);
			// 
			// relatedPartiesUserControl
			// 
			this.relatedPartiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.relatedPartiesUserControl, ".");
			this.relatedPartiesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.relatedPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.relatedPartiesUserControl.Name = "relatedPartiesUserControl";
			this.relatedPartiesUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.relatedPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 67, true);
			this.relatedPartiesUserControl.TabIndex = 0;
			this.RelatedPartiesTabPage.PerformLayout();
			this.relatedPartiesUserControl.ResumeLayout(true);
			this.relatedPartiesUserControl.PerformLayout();
			this.RelatedPartiesTabPage.ResumeLayout(true);
		}

		private void SecurityTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.webSecurityUserControl1 = new Enterprise.MasterFiles.GUI.WebSecurityUserControl();
			this.SecurityTabPage.SuspendLayout();
			this.webSecurityUserControl1.SuspendLayout();
			this.SecurityTabPage.Controls.Add(this.webSecurityUserControl1);
			// 
			// webSecurityUserControl1
			// 
			this.webSecurityUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.webSecurityUserControl1, ".");
			this.webSecurityUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webSecurityUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.webSecurityUserControl1.Name = "webSecurityUserControl1";
			this.webSecurityUserControl1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.webSecurityUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.webSecurityUserControl1.TabIndex = 0;
			this.SecurityTabPage.PerformLayout();
			this.webSecurityUserControl1.ResumeLayout(true);
			this.webSecurityUserControl1.PerformLayout();
			this.SecurityTabPage.ResumeLayout(true);
		}

		private void CustomFieldsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.customFieldsUserControl1 = new Enterprise.MasterFiles.GUI.CustomFieldsUserControl();
			this.CustomFieldsTabPage.SuspendLayout();
			this.customFieldsUserControl1.SuspendLayout();
			this.CustomFieldsTabPage.Controls.Add(this.customFieldsUserControl1);
			// 
			// customFieldsUserControl1
			// 
			this.customFieldsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.customFieldsUserControl1, ".");
			this.customFieldsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customFieldsUserControl1.IsModifyAddress = false;
			this.customFieldsUserControl1.IsModifyCarrier = false;
			this.customFieldsUserControl1.IsModifyCompetitor = false;
			this.customFieldsUserControl1.IsModifyConfig = false;
			this.customFieldsUserControl1.IsModifyConfigBrandsAndCompanyNames = false;
			this.customFieldsUserControl1.IsModifyConfigEDICodeMapping = false;
			this.customFieldsUserControl1.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.customFieldsUserControl1.IsModifyConfigGeneral = false;
			this.customFieldsUserControl1.IsModifyConfigRegistrationNumbers = false;
			this.customFieldsUserControl1.IsModifyConsignee = false;
			this.customFieldsUserControl1.IsModifyConsigneeDetails = false;
			this.customFieldsUserControl1.IsModifyConsigneeLandedCosting = false;
			this.customFieldsUserControl1.IsModifyConsigneeRelationships = false;
			this.customFieldsUserControl1.IsModifyConsignor = false;
			this.customFieldsUserControl1.IsModifyConsignorDetails = false;
			this.customFieldsUserControl1.IsModifyConsignorExporterScheme = false;
			this.customFieldsUserControl1.IsModifyConsignorRelationships = false;
			this.customFieldsUserControl1.IsModifyContact = false;
			this.customFieldsUserControl1.IsModifyContactContactDetails = false;
			this.customFieldsUserControl1.IsModifyContactDocDeliveryDetails = false;
			this.customFieldsUserControl1.IsModifyContactPersonalInformation = false;
			this.customFieldsUserControl1.IsModifyCustom = false;
			this.customFieldsUserControl1.IsModifyDetails = false;
			this.customFieldsUserControl1.IsModifyDetailsCustomFields = true;
			this.customFieldsUserControl1.IsModifyDetailsNameAndAddress = false;
			this.customFieldsUserControl1.IsModifyDetailsOrganisationType = false;
			this.customFieldsUserControl1.IsModifyDetailsPhFaxWebDetails = false;
			this.customFieldsUserControl1.IsModifyDetailsRatingAndTariffs = false;
			this.customFieldsUserControl1.IsModifyDetailsStaffAssignments = false;
			this.customFieldsUserControl1.IsModifyDetailsWebSecurity = false;
			this.customFieldsUserControl1.IsModifyForwarder = false;
			this.customFieldsUserControl1.IsModifyForwarderDetails = false;
			this.customFieldsUserControl1.IsModifyForwarderProfitShare = false;
			this.customFieldsUserControl1.IsModifyPayables = false;
			this.customFieldsUserControl1.IsModifyReceivables = false;
			this.customFieldsUserControl1.IsModifyReceivablesConfig = false;
			this.customFieldsUserControl1.IsModifyReceivablesInvoicing = false;
			this.customFieldsUserControl1.IsModifySales = false;
			this.customFieldsUserControl1.IsModifySalesClientRelationship = false;
			this.customFieldsUserControl1.IsModifySalesClientSummary = false;
			this.customFieldsUserControl1.IsModifySalesOpportunityManagement = false;
			this.customFieldsUserControl1.IsModifySalesTradeProfile = false;
			this.customFieldsUserControl1.IsModifyServices = false;
			this.customFieldsUserControl1.IsModifyWarehouse = false;
			this.customFieldsUserControl1.IsNewConfigModifyFinancialRegistrationNosSecurity = false;
			this.customFieldsUserControl1.IsNewDetailsWebSecurity = false;
			this.customFieldsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.customFieldsUserControl1.Name = "customFieldsUserControl1";
			this.customFieldsUserControl1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.customFieldsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 564, true);
			this.customFieldsUserControl1.TabIndex = 0;
			this.CustomFieldsTabPage.PerformLayout();
			this.customFieldsUserControl1.ResumeLayout(true);
			this.customFieldsUserControl1.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(true);
		}

		private void AutoRatingAndCompanyTariffTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.AutoRatingAndCompanyTariffNotAvailableLabel = new Enterprise.ZArchitecture.ZLabel();
			this.organisationRatingUserControl1 = new Enterprise.MasterFiles.GUI.Organisation.UserControls.OrganisationRatingUserControl();
			this.AutoRatingAndCompanyTariffTabPage.SuspendLayout();
			this.organisationRatingUserControl1.SuspendLayout();
			this.AutoRatingAndCompanyTariffTabPage.Controls.Add(this.organisationRatingUserControl1);
			this.AutoRatingAndCompanyTariffTabPage.Controls.Add(this.AutoRatingAndCompanyTariffNotAvailableLabel);
			// 
			// AutoRatingAndCompanyTariffNotAvailableLabel
			// 
			this.AutoRatingAndCompanyTariffNotAvailableLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("MainDetailsUserControl|0c76ba91-ed27-4070-b750-6530996a4570", "You must select an Organization type of Receivables, Consignee, Consignor, Sales or Carrier from the Details page to use this page.");
			this.AutoRatingAndCompanyTariffNotAvailableLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoRatingAndCompanyTariffNotAvailableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 232, true);
			this.AutoRatingAndCompanyTariffNotAvailableLabel.Name = "AutoRatingAndCompanyTariffNotAvailableLabel";
			this.AutoRatingAndCompanyTariffNotAvailableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 40, true);
			this.AutoRatingAndCompanyTariffNotAvailableLabel.TabIndex = 18;
			this.AutoRatingAndCompanyTariffNotAvailableLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// organisationRatingUserControl1
			// 
			this.organisationRatingUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.organisationRatingUserControl1, ".");
			this.organisationRatingUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.organisationRatingUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.organisationRatingUserControl1.Name = "organisationRatingUserControl1";
			this.organisationRatingUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.organisationRatingUserControl1.TabIndex = 19;
			this.AutoRatingAndCompanyTariffTabPage.PerformLayout();
			this.organisationRatingUserControl1.ResumeLayout(true);
			this.organisationRatingUserControl1.PerformLayout();
			this.AutoRatingAndCompanyTariffTabPage.ResumeLayout(true);
		}

		private void ConfigTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ConfigUserControl = new Enterprise.MasterFiles.GUI.ConfigUserControl();
			this.ConfigTabPage.SuspendLayout();
			this.ConfigUserControl.SuspendLayout();
			this.ConfigTabPage.Controls.Add(this.ConfigUserControl);
			// 
			// ConfigUserControl
			// 
			this.ConfigUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConfigUserControl, ".");
			this.ConfigUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigUserControl.IsModifyAddress = false;
			this.ConfigUserControl.IsModifyCarrier = false;
			this.ConfigUserControl.IsModifyCompetitor = false;
			this.ConfigUserControl.IsModifyConfig = true;
			this.ConfigUserControl.IsModifyConfigBrandsAndCompanyNames = true;
			this.ConfigUserControl.IsModifyConfigEDICodeMapping = true;
			this.ConfigUserControl.IsModifyConfigFinancialRegistrationNumbersSecurity = false;
			this.ConfigUserControl.IsModifyConfigGeneral = true;
			this.ConfigUserControl.IsModifyConfigRegistrationNumbers = true;
			this.ConfigUserControl.IsModifyConsignee = false;
			this.ConfigUserControl.IsModifyConsigneeDetails = false;
			this.ConfigUserControl.IsModifyConsigneeLandedCosting = false;
			this.ConfigUserControl.IsModifyConsigneeRelationships = false;
			this.ConfigUserControl.IsModifyConsignor = false;
			this.ConfigUserControl.IsModifyConsignorDetails = false;
			this.ConfigUserControl.IsModifyConsignorExporterScheme = false;
			this.ConfigUserControl.IsModifyConsignorRelationships = false;
			this.ConfigUserControl.IsModifyContact = false;
			this.ConfigUserControl.IsModifyContactContactDetails = false;
			this.ConfigUserControl.IsModifyContactDocDeliveryDetails = false;
			this.ConfigUserControl.IsModifyContactPersonalInformation = false;
			this.ConfigUserControl.IsModifyCustom = false;
			this.ConfigUserControl.IsModifyDetails = false;
			this.ConfigUserControl.IsModifyDetailsCustomFields = false;
			this.ConfigUserControl.IsModifyDetailsNameAndAddress = false;
			this.ConfigUserControl.IsModifyDetailsOrganisationType = false;
			this.ConfigUserControl.IsModifyDetailsPhFaxWebDetails = false;
			this.ConfigUserControl.IsModifyDetailsRatingAndTariffs = false;
			this.ConfigUserControl.IsModifyDetailsStaffAssignments = false;
			this.ConfigUserControl.IsModifyDetailsWebSecurity = false;
			this.ConfigUserControl.IsModifyForwarder = false;
			this.ConfigUserControl.IsModifyForwarderDetails = false;
			this.ConfigUserControl.IsModifyForwarderProfitShare = false;
			this.ConfigUserControl.IsModifyPayables = false;
			this.ConfigUserControl.IsModifyReceivables = false;
			this.ConfigUserControl.IsModifyReceivablesConfig = false;
			this.ConfigUserControl.IsModifyReceivablesInvoicing = false;
			this.ConfigUserControl.IsModifySales = false;
			this.ConfigUserControl.IsModifySalesClientRelationship = false;
			this.ConfigUserControl.IsModifySalesClientSummary = false;
			this.ConfigUserControl.IsModifySalesOpportunityManagement = false;
			this.ConfigUserControl.IsModifySalesTradeProfile = false;
			this.ConfigUserControl.IsModifyServices = false;
			this.ConfigUserControl.IsModifyWarehouse = false;
			this.ConfigUserControl.IsNewConfigModifyFinancialRegistrationNosSecurity = false;
			this.ConfigUserControl.IsNewDetailsWebSecurity = false;
			this.ConfigUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfigUserControl.Name = "ConfigUserControl";
			this.ConfigUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.ConfigUserControl.TabIndex = 0;
			this.ConfigTabPage.PerformLayout();
			this.ConfigUserControl.ResumeLayout(true);
			this.ConfigUserControl.PerformLayout();
			this.ConfigTabPage.ResumeLayout(true);
		}

		#endregion

	}
}
