namespace Enterprise.MasterFiles.GUI
{
	public partial class OpportunityManagementDetailsControl
	{

		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.GUI.ZGroupBox LeadSourceGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox P8_GCGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit P8_OpportunityTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit P8_StatusDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit P8_StageDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox P8_GS_NKPrimarySalesPersonCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DateClosedDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl OpportunityTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage OppNotesTabPage;
		private Enterprise.ZArchitecture.GUI.ZRichTextBox zRichTextBox1;
		private Enterprise.ZArchitecture.GUI.ZGuidSearchEdit ContactsDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OpportunityClientGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth OpportunitySourceDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OutcomesDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcFindBox EstimatedValueCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AssignedOrgPKGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidSearchEdit P8_OC_AssignedOfficeContactGuidSearchEdit;
		private Enterprise.ZArchitecture.ZCalcEdit DiscountCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit P8_PackageTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ClosedReasonDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox P8_OHGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit P8_EstimatedCloseDateDateEdit;
		private Enterprise.ZArchitecture.ZCalcEdit RentalMultiplierCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit LastQuotedDateEdit;
		private Enterprise.ZArchitecture.ZTextBox UNLOCOTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidDropEdit P8_OA_AssignedOfficeGuidDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CloseDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OrganizationDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ContactDetailsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox JobCategoryTextBox;
		private Enterprise.ZArchitecture.ZTextBox ContactClosestPortTextBox;
		private Enterprise.ZArchitecture.ZTextBox ContactTimeZoneTextBox;
		protected Enterprise.ZArchitecture.ZLabel DeliveryStatusLabel;
		protected Enterprise.ZArchitecture.GUI.ZImageButton DeliveryStatusButton;
		private Enterprise.ZArchitecture.ZTextBox SourceDetailsTextBox;
		public Enterprise.ZArchitecture.ZLabel OverallDispositionLabel;
		private Enterprise.ZArchitecture.GUI.ZDateEdit P8_RecallDateDateEdit;
		private Enterprise.ZArchitecture.ZLabel CloseCertaintyPercentageLabel;
		private Enterprise.ZArchitecture.ZLabel CloseCertaintyDescLabel;
		private CargoWise.Windows.UI.KTrackBar CloseCertaintyTrackBar;
		private Enterprise.ZArchitecture.GUI.ZDropEdit SourceDetailsDropEdit;
		private Enterprise.ZArchitecture.ZLabel CreatedFromInquiryLabel;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ReferringOrgGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidSearchEdit ReferringContactSearchEdit;
		private Enterprise.ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		private Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl OpportunityCustomFieldsControl;
		internal ContactPhoneDiallerUserControl ContactPhoneDiallerUserControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox relatedCommunicationGroupBox;
		private RelatedCommunicationGrid relatedCommunicationGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage SalesRelationsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage CommissionAgreementTabPage;
		internal OpportunityCommissionAgreementsTab CommissionAgreementsControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MonetaryValueGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit CommittedValueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit UnsuccessfulValueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PipelineValueCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZTabPage StageProgressTabPage;
		private OpportunityStageProgressControl opportunityStageProgressControl;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CreatedFromInquiryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RentalMultiplierCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.P8_PackageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DiscountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.P8_OpportunityTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EstimatedValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.P8_OA_AssignedOfficeGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.P8_OC_AssignedOfficeContactGuidSearchEdit = new Enterprise.ZArchitecture.GUI.ZGuidSearchEdit();
			this.AssignedOrgPKGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.P8_GS_NKPrimarySalesPersonCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.P8_EstimatedCloseDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClosedReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OutcomesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DateClosedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.P8_StageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.P8_StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OpportunityTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.OppNotesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zRichTextBox1 = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OpportunityCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.CommissionAgreementTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CommissionAgreementsControl = new Enterprise.MasterFiles.GUI.OpportunityCommissionAgreementsTab();
			this.SalesRelationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StageProgressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.opportunityStageProgressControl = new OpportunityStageProgressControl();
			this.ContactsDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidSearchEdit();
			this.OpportunityClientGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.P8_GCGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ReferringContactSearchEdit = new Enterprise.ZArchitecture.GUI.ZGuidSearchEdit();
			this.ReferringOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OpportunitySourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.SourceDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SourceDetailsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UNLOCOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LastQuotedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.P8_OHGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.relatedCommunicationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.relatedCommunicationGrid = new Enterprise.MasterFiles.GUI.RelatedCommunicationGrid();
			this.CloseDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CloseCertaintyPercentageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseCertaintyDescLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseCertaintyTrackBar = new CargoWise.Windows.UI.KTrackBar();
			this.P8_RecallDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OverallDispositionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OrganizationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactPhoneDiallerUserControl = new Enterprise.MasterFiles.GUI.ContactPhoneDiallerUserControl();
			this.JobCategoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactClosestPortTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactTimeZoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliveryStatusButton = new Enterprise.ZArchitecture.GUI.ZImageButton();
			this.LeadSourceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MonetaryValueGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UnsuccessfulValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CommittedValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PipelineValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.P8_PackageTypeDropEdit.SuspendLayout();
			this.P8_OpportunityTypeDropEdit.SuspendLayout();
			this.EstimatedValueCalcFindBox.SuspendLayout();
			this.P8_OA_AssignedOfficeGuidDropEdit.SuspendLayout();
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.SuspendLayout();
			this.AssignedOrgPKGuidFindBox.SuspendLayout();
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.SuspendLayout();
			this.P8_EstimatedCloseDateDateEdit.SuspendLayout();
			this.ClosedReasonDropEdit.SuspendLayout();
			this.OutcomesDropEdit.SuspendLayout();
			this.DateClosedDateEdit.SuspendLayout();
			this.P8_StageDropEdit.SuspendLayout();
			this.P8_StatusDropEdit.SuspendLayout();
			this.OpportunityTabControl.SuspendLayout();
			this.OppNotesTabPage.SuspendLayout();
			this.zRichTextBox1.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.OpportunityCustomFieldsControl.SuspendLayout();
			this.CommissionAgreementTabPage.SuspendLayout();
			this.CommissionAgreementsControl.SuspendLayout();
			this.SalesRelationsTabPage.SuspendLayout();
			this.StageProgressTabPage.SuspendLayout();
			this.opportunityStageProgressControl.SuspendLayout();
			this.ContactsDropEdit.SuspendLayout();
			this.OpportunityClientGroupBox.SuspendLayout();
			this.P8_GCGuidFindBox.SuspendLayout();
			this.ReferringContactSearchEdit.SuspendLayout();
			this.ReferringOrgGuidFindBox.SuspendLayout();
			this.OpportunitySourceDropEdit.SuspendLayout();
			this.SourceDetailsDropEdit.SuspendLayout();
			this.LastQuotedDateEdit.SuspendLayout();
			this.P8_OHGuidFindBox.SuspendLayout();
			this.relatedCommunicationGroupBox.SuspendLayout();
			this.relatedCommunicationGrid.SuspendLayout();
			this.CloseDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CloseCertaintyTrackBar)).BeginInit();
			this.P8_RecallDateDateEdit.SuspendLayout();
			this.OrganizationDetailsGroupBox.SuspendLayout();
			this.ContactDetailsGroupBox.SuspendLayout();
			this.ContactPhoneDiallerUserControl.SuspendLayout();
			this.LeadSourceGroupBox.SuspendLayout();
			this.MonetaryValueGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgOpportunity);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|b8e9697b-0dd2-4c28-9d91-3ebb2c107e8a", "Opportunity Details");
			this.DetailsGroupBox.Controls.Add(this.CreatedFromInquiryLabel);
			this.DetailsGroupBox.Controls.Add(this.RentalMultiplierCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.P8_PackageTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.DiscountCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.P8_OpportunityTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 135, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// CreatedFromInquiryLabel
			// 
			this.CreatedFromInquiryLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CreatedFromInquiryLabel, "Enquiry.O1_LeadUniqueReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).Enquiry.O1_LeadUniqueReference)));
			this.CreatedFromInquiryLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.CreatedFromInquiryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CreatedFromInquiryLabel.ForeColor = System.Drawing.Color.Blue;
			this.CreatedFromInquiryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 0, true);
			this.CreatedFromInquiryLabel.Name = "CreatedFromInquiryLabel";
			this.CreatedFromInquiryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.CreatedFromInquiryLabel.TabIndex = 8;
			this.CreatedFromInquiryLabel.Text = "<Inquiry ID>";
			this.CreatedFromInquiryLabel.Click += new System.EventHandler(this.CreatedFromInquiryLabel_Click);
			// 
			// RentalMultiplierCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RentalMultiplierCalcEdit, "P8_RentalMultiplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_RentalMultiplier)));
			this.RentalMultiplierCalcEdit.DecimalPlaces = 0;
			this.RentalMultiplierCalcEdit.Decimals = 0;
			this.RentalMultiplierCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(279, 99, true);
			this.RentalMultiplierCalcEdit.MaxValue = new decimal(new int[] {
			-727379969,
			232,
			0,
			0 });
			this.RentalMultiplierCalcEdit.Name = "RentalMultiplierCalcEdit";
			this.RentalMultiplierCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 17, true);
			this.RentalMultiplierCalcEdit.TabIndex = 6;
			this.RentalMultiplierCalcEdit.Text = "0";
			this.RentalMultiplierCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// P8_PackageTypeDropEdit
			// 
			this.P8_PackageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.P8_PackageTypeDropEdit, "P8_PackageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).ProductTypeDescription)));
			this.P8_PackageTypeDropEdit.BindToForDescription = "ProductTypeDescription";
			this.P8_PackageTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|53deb24b-244a-4909-91e9-f777ec9330fb", "Product");
			this.P8_PackageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 43, true);
			this.P8_PackageTypeDropEdit.Name = "P8_PackageTypeDropEdit";
			this.P8_PackageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.P8_PackageTypeDropEdit.TabIndex = 2;
			// 
			// DiscountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DiscountCalcEdit, "P8_DiscountAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_DiscountAmount)));
			this.DiscountCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|8fa11b1d-3448-40f9-bb00-53b74e678345", "Total Discount");
			this.DiscountCalcEdit.DecimalPlaces = 0;
			this.DiscountCalcEdit.Decimals = 0;
			this.DiscountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 99, true);
			this.DiscountCalcEdit.MaxValue = new decimal(new int[] {
			-727379969,
			232,
			0,
			0 });
			this.DiscountCalcEdit.Name = "DiscountCalcEdit";
			this.DiscountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 17, true);
			this.DiscountCalcEdit.TabIndex = 5;
			this.DiscountCalcEdit.Text = "0";
			this.DiscountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// P8_OpportunityTypeDropEdit
			// 
			this.P8_OpportunityTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.P8_OpportunityTypeDropEdit, "P8_OpportunityType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OpportunityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).TypeDescription)));
			this.P8_OpportunityTypeDropEdit.BindToForDescription = "TypeDescription";
			this.P8_OpportunityTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|85d25922-811a-4b9d-807f-01f6d500f956", "Sales Type");
			this.P8_OpportunityTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 69, true);
			this.P8_OpportunityTypeDropEdit.Name = "P8_OpportunityTypeDropEdit";
			this.P8_OpportunityTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.P8_OpportunityTypeDropEdit.TabIndex = 3;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "P8_OpportunityDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OpportunityDescription)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|9d071f64-d289-4615-bff5-c7b70c2a512d", "Description", "Opportunity Description.");
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 17, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// EstimatedValueCalcFindBox
			// 
			this.EstimatedValueCalcFindBox.AllowDrop = true;
			this.EstimatedValueCalcFindBox.BindToAmount = "P8_EstimatedValue";
			this.EstimatedValueCalcFindBox.BindToUnit = "P8_RX_NKEstimatedValueCurrency";
			this.EstimatedValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.EstimatedValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 22, true);
			this.EstimatedValueCalcFindBox.Name = "EstimatedValueCalcFindBox";
			this.EstimatedValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.EstimatedValueCalcFindBox.TabIndex = 4;
			// 
			// P8_OA_AssignedOfficeGuidDropEdit
			// 
			this.P8_OA_AssignedOfficeGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.P8_OA_AssignedOfficeGuidDropEdit, "P8_OA_AssignedOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OA_AssignedOffice)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.P8_OA_AssignedOfficeGuidDropEdit, false);
			this.P8_OA_AssignedOfficeGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 93, true);
			this.P8_OA_AssignedOfficeGuidDropEdit.Name = "P8_OA_AssignedOfficeGuidDropEdit";
			this.P8_OA_AssignedOfficeGuidDropEdit.PreBoundMaxLength = 41;
			this.P8_OA_AssignedOfficeGuidDropEdit.ShowDescriptionBox = false;
			this.P8_OA_AssignedOfficeGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.P8_OA_AssignedOfficeGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 17, true);
			this.P8_OA_AssignedOfficeGuidDropEdit.TabIndex = 3;
			// 
			// P8_OC_AssignedOfficeContactGuidDropEdit
			// 
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.P8_OC_AssignedOfficeContactGuidSearchEdit, "P8_OC_AssignedOfficeContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OC_AssignedOfficeContact)));
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 117, true);
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.Name = "P8_OC_AssignedOfficeContactGuidSearchEdit";
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.PreBoundMaxLength = 35;
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.MaximumRows = 50;
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 17, true);
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.TabIndex = 4;
			// 
			// AssignedOrgPKGuidFindBox
			// 
			this.AssignedOrgPKGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AssignedOrgPKGuidFindBox, "AssignedOrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).AssignedOrgPK)));
			this.AssignedOrgPKGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|4cbd0c5e-c563-4075-aa37-1a9b0629c2aa", "Assigned Office");
			this.AssignedOrgPKGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.AssignedOrgPKGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 69, true);
			this.AssignedOrgPKGuidFindBox.Name = "AssignedOrgPKGuidFindBox";
			this.AssignedOrgPKGuidFindBox.ShouldResize = true;
			this.AssignedOrgPKGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.AssignedOrgPKGuidFindBox.TabIndex = 2;
			// 
			// P8_GS_NKPrimarySalesPersonCodeFindBox
			// 
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.P8_GS_NKPrimarySalesPersonCodeFindBox, "P8_GS_NKPrimarySalesPerson");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_GS_NKPrimarySalesPerson)));
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|cadc566b-1615-4eb7-bd32-70c15af96ab4", "Sales Person", "Opportunity Primary Sales Person.");
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 21, true);
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.Name = "P8_GS_NKPrimarySalesPersonCodeFindBox";
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.ShouldResize = true;
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.TabIndex = 0;
			// 
			// P8_EstimatedCloseDateDateEdit
			// 
			this.P8_EstimatedCloseDateDateEdit.AllowDrop = true;
			this.P8_EstimatedCloseDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.P8_EstimatedCloseDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.P8_EstimatedCloseDateDateEdit, "P8_EstimatedCloseDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_EstimatedCloseDateLocal)));
			this.P8_EstimatedCloseDateDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|572a9e8b-9cde-4507-826e-e7d965463eef", "Est. Closed Date");
			this.P8_EstimatedCloseDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 141, true);
			this.P8_EstimatedCloseDateDateEdit.Name = "P8_EstimatedCloseDateDateEdit";
			this.P8_EstimatedCloseDateDateEdit.TabIndex = 5;
			// 
			// ClosedReasonDropEdit
			// 
			this.ClosedReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClosedReasonDropEdit, "P8_LostReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_LostReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).CloseReasonDescription)));
			this.ClosedReasonDropEdit.BindToForDescription = "CloseReasonDescription";
			this.ClosedReasonDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|27f34715-5de1-4649-b1f2-9a1dbe453e6b", "Close Reason");
			this.ClosedReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 167, true);
			this.ClosedReasonDropEdit.Name = "ClosedReasonDropEdit";
			this.ClosedReasonDropEdit.PreBoundMaxLength = 3;
			this.ClosedReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.ClosedReasonDropEdit.TabIndex = 7;
			// 
			// OutcomesDropEdit
			// 
			this.OutcomesDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OutcomesDropEdit, "P8_Outcome");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_Outcome)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).OutcomeDescription)));
			this.OutcomesDropEdit.BindToForDescription = "OutcomeDescription";
			this.OutcomesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 21, true);
			this.OutcomesDropEdit.Name = "OutcomesDropEdit";
			this.OutcomesDropEdit.PreBoundMaxLength = 3;
			this.OutcomesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.OutcomesDropEdit.TabIndex = 0;
			// 
			// DateClosedDateEdit
			// 
			this.DateClosedDateEdit.AllowDrop = true;
			this.DateClosedDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateClosedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateClosedDateEdit, "P8_ClosedDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_ClosedDateLocal)));
			this.DateClosedDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|72ffae33-9d76-46a6-ad0a-de7566205475", "Close Date");
			this.DateClosedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 143, true);
			this.DateClosedDateEdit.Name = "DateClosedDateEdit";
			this.DateClosedDateEdit.TabIndex = 6;
			// 
			// P8_StageDropEdit
			// 
			this.P8_StageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.P8_StageDropEdit, "P8_Stage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_Stage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).StageDescription)));
			this.P8_StageDropEdit.BindToForDescription = "StageDescription";
			this.P8_StageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 47, true);
			this.P8_StageDropEdit.Name = "P8_StageDropEdit";
			this.P8_StageDropEdit.PreBoundMaxLength = 3;
			this.P8_StageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.P8_StageDropEdit.TabIndex = 2;
			// 
			// P8_StatusDropEdit
			// 
			this.P8_StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.P8_StatusDropEdit, "P8_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).StatusDescription)));
			this.P8_StatusDropEdit.BindToForDescription = "StatusDescription";
			this.P8_StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 73, true);
			this.P8_StatusDropEdit.Name = "P8_StatusDropEdit";
			this.P8_StatusDropEdit.PreBoundMaxLength = 3;
			this.P8_StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
			this.P8_StatusDropEdit.TabIndex = 3;
			// 
			// OpportunityTabControl
			// 
			this.OpportunityTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.OpportunityTabControl.Controls.Add(this.OppNotesTabPage);
			this.OpportunityTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.OpportunityTabControl.Controls.Add(this.CommissionAgreementTabPage);
			this.OpportunityTabControl.Controls.Add(this.SalesRelationsTabPage);
			this.OpportunityTabControl.Controls.Add(this.StageProgressTabPage);
			this.OpportunityTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 363, true);
			this.OpportunityTabControl.Name = "OpportunityTabControl";
			this.OpportunityTabControl.SelectedIndex = 0;
			this.OpportunityTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 237, true);
			this.OpportunityTabControl.TabIndex = 6;
			this.OpportunityTabControl.TabStop = false;
			// 
			// OppNotesTabPage
			// 
			this.OppNotesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|886eed34-e6ba-4eaf-9196-f2fa14531d5f", "Details", "Details", "Details", "");
			this.OppNotesTabPage.Controls.Add(this.zRichTextBox1);
			this.OppNotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OppNotesTabPage.Name = "OppNotesTabPage";
			this.OppNotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 215, true);
			this.OppNotesTabPage.TabIndex = 0;
			// 
			// zRichTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zRichTextBox1, "P8_OpportunityNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OpportunityNotes)));
			this.zRichTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zRichTextBox1, false);
			this.zRichTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zRichTextBox1.MaxLength = 10000000;
			this.zRichTextBox1.Name = "zRichTextBox1";
			this.zRichTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 45, true);
			this.zRichTextBox1.TabIndex = 0;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cf3ec703-78b8-406c-9b60-5ba14d26b558", "Custom Fields");
			this.CustomFieldsTabPage.Controls.Add(this.OpportunityCustomFieldsControl);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 7, 3, 3, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 215, true);
			this.CustomFieldsTabPage.TabIndex = 5;
			// 
			// OpportunityCustomFieldsControl
			// 
			this.OpportunityCustomFieldsControl.AllowDrop = true;
			this.OpportunityCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OpportunityCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.OpportunityCustomFieldsControl.Name = "OpportunityCustomFieldsControl";
			this.OpportunityCustomFieldsControl.NothingSetupMessageLabelText = "To make use of this tab, please setup Transport Booking Instruction custom fields in Workflow Manager.";
			this.OpportunityCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 35, true);
			this.OpportunityCustomFieldsControl.TabIndex = 0;
			// 
			// CommissionAgreementTabPage
			// 
			this.CommissionAgreementTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a3e302c2-fb76-4ad9-bd99-c7ba3978f75b", "Commission Agreement");
			this.CommissionAgreementTabPage.Controls.Add(this.CommissionAgreementsControl);
			this.CommissionAgreementTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CommissionAgreementTabPage.Name = "CommissionAgreementTabPage";
			this.CommissionAgreementTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 215, true);
			this.CommissionAgreementTabPage.TabIndex = 8;
			// 
			// CommissionAgreementsControl
			// 
			this.CommissionAgreementsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommissionAgreementsControl, ".");
			this.CommissionAgreementsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommissionAgreementsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommissionAgreementsControl.Name = "CommissionAgreementsControl";
			this.CommissionAgreementsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 215, true);
			this.CommissionAgreementsControl.TabIndex = 0;
			// 
			// SalesRelationsTabPage
			// 
			this.SalesRelationsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b8970ae9-bd00-4111-a059-8af5a94237c2", "Sales Relations");
			this.SalesRelationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.SalesRelationsTabPage.Name = "SalesRelationsTabPage";
			this.SalesRelationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 215, true);
			this.SalesRelationsTabPage.TabIndex = 7;
			//
			// StageProgressTabPage
			// 
			this.StageProgressTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f989841d-860e-4dee-8038-d975996eee17", "Stage Progress");
			this.StageProgressTabPage.Controls.Add(this.opportunityStageProgressControl);
			this.StageProgressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.StageProgressTabPage.Name = "StageProgressTabPage";
			this.StageProgressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 215, true);
			this.StageProgressTabPage.TabIndex = 8;
			// 
			// opportunityStageProgressControl
			// 
			this.opportunityStageProgressControl.AllowDrop = true;
			this.opportunityStageProgressControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.opportunityStageProgressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.opportunityStageProgressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.opportunityStageProgressControl.Name = "opportunityStageProgressControl";
			this.opportunityStageProgressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 215, true);
			this.opportunityStageProgressControl.TabIndex = 0;
			this.opportunityStageProgressControl.CaptionRenderingEnabled = true;
			// 
			// ContactsDropEdit
			// 
			this.ContactsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactsDropEdit, "P8_OC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OC)));
			this.ContactsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 21, true);
			this.ContactsDropEdit.Name = "ContactsDropEdit";
			this.ContactsDropEdit.PreBoundMaxLength = 25;
			this.ContactsDropEdit.MaximumRows = 50;
			this.ContactsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.ContactsDropEdit.TabIndex = 0;
			// 
			// OpportunityClientGroupBox
			// 
			this.OpportunityClientGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|e1e8c452-69a4-4b82-93b3-6a96d1cc2c62", "Internal Details");
			this.OpportunityClientGroupBox.Controls.Add(this.P8_GCGuidFindBox);
			this.OpportunityClientGroupBox.Controls.Add(this.P8_OA_AssignedOfficeGuidDropEdit);
			this.OpportunityClientGroupBox.Controls.Add(this.P8_OC_AssignedOfficeContactGuidSearchEdit);
			this.OpportunityClientGroupBox.Controls.Add(this.P8_GS_NKPrimarySalesPersonCodeFindBox);
			this.OpportunityClientGroupBox.Controls.Add(this.AssignedOrgPKGuidFindBox);
			this.OpportunityClientGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 215, true);
			this.OpportunityClientGroupBox.Name = "OpportunityClientGroupBox";
			this.OpportunityClientGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 142, true);
			this.OpportunityClientGroupBox.TabIndex = 3;
			this.OpportunityClientGroupBox.TabStop = false;
			// 
			// P8_GCGuidFindBox
			// 
			this.P8_GCGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.P8_GCGuidFindBox, "P8_GC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_GC)));
			this.P8_GCGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.P8_GCGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 45, true);
			this.P8_GCGuidFindBox.Name = "P8_GCGuidFindBox";
			this.P8_GCGuidFindBox.ShouldResize = true;
			this.P8_GCGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.P8_GCGuidFindBox.TabIndex = 1;
			// 
			// ReferringContactSearchEdit
			// 
			this.ReferringContactSearchEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferringContactSearchEdit, "P8_OC_ReferringContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OC_ReferringContact)));
			this.ReferringContactSearchEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5485ced4-004f-4bcc-9a46-a037358f5a1e", "Referring Contact");
			this.ReferringContactSearchEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferringContactSearchEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 99, true);
			this.ReferringContactSearchEdit.Name = "ReferringContactSearchEdit";
			this.ReferringContactSearchEdit.PreBoundMaxLength = 39;
			this.ReferringContactSearchEdit.MaximumRows = 50;
			this.ReferringContactSearchEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 17, true);
			this.ReferringContactSearchEdit.TabIndex = 7;
			// 
			// ReferringOrgGuidFindBox
			// 
			this.ReferringOrgGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferringOrgGuidFindBox, "P8_OH_ReferringOrganisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OH_ReferringOrganisation)));
			this.ReferringOrgGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d2afd525-4ff9-4d9b-840e-2a22a58fe520", "Referring Organization");
			this.ReferringOrgGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ReferringOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 73, true);
			this.ReferringOrgGuidFindBox.Name = "ReferringOrgGuidFindBox";
			this.ReferringOrgGuidFindBox.ShouldResize = true;
			this.ReferringOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.ReferringOrgGuidFindBox.TabIndex = 6;
			// 
			// OpportunitySourceDropEdit
			// 
			this.OpportunitySourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OpportunitySourceDropEdit, "P8_Source");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).SourceDescription)));
			this.OpportunitySourceDropEdit.BindToForDescription = "SourceDescription";
			this.OpportunitySourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 21, true);
			this.OpportunitySourceDropEdit.Name = "OpportunitySourceDropEdit";
			this.OpportunitySourceDropEdit.PreBoundMaxLength = 5;
			this.OpportunitySourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.OpportunitySourceDropEdit.TabIndex = 4;
			// 
			// SourceDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.SourceDetailsTextBox, "P8_SourceDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_SourceDetails)));
			this.SourceDetailsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|4d29263a-ba01-43a2-ab32-6e658baf4fe0", "Source Details");
			this.SourceDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SourceDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 47, true);
			this.SourceDetailsTextBox.Name = "SourceDetailsTextBox";
			this.SourceDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.SourceDetailsTextBox.TabIndex = 5;
			// 
			// SourceDetailsDropEdit
			// 
			this.SourceDetailsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SourceDetailsDropEdit, "SourceDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).SourceDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).SourceDetailsDescription)));
			this.SourceDetailsDropEdit.BindToForDescription = "SourceDetailsDescription";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SourceDetailsDropEdit, false);
			this.SourceDetailsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 47, true);
			this.SourceDetailsDropEdit.Name = "SourceDetailsDropEdit";
			this.SourceDetailsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 17, true);
			this.SourceDetailsDropEdit.TabIndex = 7;
			this.SourceDetailsDropEdit.Visible = false;
			// 
			// UNLOCOTextBox
			// 
			this.BindingSource.SetBindingMember(this.UNLOCOTextBox, "Address+OA_RL_NKRelatedPortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).Address.OA_RL_NKRelatedPortCode)));
			this.UNLOCOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 47, true);
			this.UNLOCOTextBox.Name = "UNLOCOTextBox";
			this.UNLOCOTextBox.ReadOnly = true;
			this.UNLOCOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.UNLOCOTextBox.TabIndex = 8;
			this.UNLOCOTextBox.TabStop = false;
			// 
			// LastQuotedDateEdit
			// 
			this.LastQuotedDateEdit.AllowDrop = true;
			this.LastQuotedDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastQuotedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastQuotedDateEdit, "Header+LastQuotedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).Header.LastQuotedDate)));
			this.LastQuotedDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|3ae90939-b0c3-4ec8-a52e-30585265da8f", "Last Quoted Date", "Client Last Quoted Date.");
			this.LastQuotedDateEdit.Enabled = false;
			this.LastQuotedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 47, true);
			this.LastQuotedDateEdit.Name = "LastQuotedDateEdit";
			this.LastQuotedDateEdit.TabIndex = 1;
			this.LastQuotedDateEdit.TabStop = false;
			// 
			// P8_OHGuidFindBox
			// 
			this.P8_OHGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.P8_OHGuidFindBox, "P8_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OH)));
			this.P8_OHGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|ec3c0156-42a8-4745-806e-3e7e63a4d811", "Organization");
			this.P8_OHGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.P8_OHGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 21, true);
			this.P8_OHGuidFindBox.Name = "P8_OHGuidFindBox";
			this.P8_OHGuidFindBox.ShouldResize = true;
			this.P8_OHGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 17, true);
			this.P8_OHGuidFindBox.TabIndex = 0;
			// 
			// relatedCommunicationGroupBox
			// 
			this.relatedCommunicationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.relatedCommunicationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9f8dc0f0-4c41-4544-a376-e3c52328fb4a", "Related Communication");
			this.relatedCommunicationGroupBox.Controls.Add(this.relatedCommunicationGrid);
			this.relatedCommunicationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(785, 239, true);
			this.relatedCommunicationGroupBox.Name = "relatedCommunicationGroupBox";
			this.relatedCommunicationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 359, true);
			this.relatedCommunicationGroupBox.TabIndex = 7;
			this.relatedCommunicationGroupBox.TabStop = false;
			// 
			// relatedCommunicationGrid
			// 
			this.relatedCommunicationGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.relatedCommunicationGrid, "RelatedCommunicationCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgSalesCallCollection)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).RelatedCommunicationCollection)));
			this.relatedCommunicationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.relatedCommunicationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.relatedCommunicationGrid.Name = "relatedCommunicationGrid";
			this.relatedCommunicationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 342, true);
			this.relatedCommunicationGrid.TabIndex = 0;
			// 
			// CloseDetailsGroupBox
			// 
			this.CloseDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CloseDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|2436d3f1-458a-495e-a7de-2c1e87b2a62b", "Close Details");
			this.CloseDetailsGroupBox.Controls.Add(this.CloseCertaintyPercentageLabel);
			this.CloseDetailsGroupBox.Controls.Add(this.CloseCertaintyDescLabel);
			this.CloseDetailsGroupBox.Controls.Add(this.CloseCertaintyTrackBar);
			this.CloseDetailsGroupBox.Controls.Add(this.P8_RecallDateDateEdit);
			this.CloseDetailsGroupBox.Controls.Add(this.OutcomesDropEdit);
			this.CloseDetailsGroupBox.Controls.Add(this.ClosedReasonDropEdit);
			this.CloseDetailsGroupBox.Controls.Add(this.P8_EstimatedCloseDateDateEdit);
			this.CloseDetailsGroupBox.Controls.Add(this.P8_StatusDropEdit);
			this.CloseDetailsGroupBox.Controls.Add(this.DateClosedDateEdit);
			this.CloseDetailsGroupBox.Controls.Add(this.P8_StageDropEdit);
			this.CloseDetailsGroupBox.Controls.Add(this.OverallDispositionLabel);
			this.CloseDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(784, 3, true);
			this.CloseDetailsGroupBox.Name = "CloseDetailsGroupBox";
			this.CloseDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 230, true);
			this.CloseDetailsGroupBox.TabIndex = 5;
			this.CloseDetailsGroupBox.TabStop = false;
			// 
			// CloseCertaintyPercentageLabel
			// 
			this.BindingSource.SetBindingMember(this.CloseCertaintyPercentageLabel, "CloseCertaintyAsPercentageString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).CloseCertaintyAsPercentageString)));
			this.CloseCertaintyPercentageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CloseCertaintyPercentageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 105, true);
			this.CloseCertaintyPercentageLabel.Name = "CloseCertaintyPercentageLabel";
			this.CloseCertaintyPercentageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 23, true);
			this.CloseCertaintyPercentageLabel.TabIndex = 13;
			// 
			// CloseCertaintyDescLabel
			// 
			this.CloseCertaintyDescLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|fb3e4f76-1d9f-4f1b-8253-cba9f20fbd42", "Close Certainty:");
			this.CloseCertaintyDescLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CloseCertaintyDescLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 110, true);
			this.CloseCertaintyDescLabel.Name = "CloseCertaintyDescLabel";
			this.CloseCertaintyDescLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.CloseCertaintyDescLabel.TabIndex = 12;
			// 
			// CloseCertaintyTrackBar
			// 
			this.CloseCertaintyTrackBar.AutoSize = false;
			this.CloseCertaintyTrackBar.BackColor = System.Drawing.SystemColors.Control;
			this.CloseCertaintyTrackBar.LargeChange = 1;
			this.CloseCertaintyTrackBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 99, true);
			this.CloseCertaintyTrackBar.Maximum = 20;
			this.CloseCertaintyTrackBar.Name = "CloseCertaintyTrackBar";
			this.CloseCertaintyTrackBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 33, true);
			this.CloseCertaintyTrackBar.TabIndex = 4;
			this.CloseCertaintyTrackBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
			this.CloseCertaintyTrackBar.Scroll += new System.EventHandler(this.CloseCertaintyTrackBar_Scroll);
			// 
			// P8_RecallDateDateEdit
			// 
			this.P8_RecallDateDateEdit.AllowDrop = true;
			this.P8_RecallDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.P8_RecallDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.P8_RecallDateDateEdit, "P8_RecallDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_RecallDateLocal)));
			this.P8_RecallDateDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|e671d0d8-0e41-42cc-900f-93263d0631f4", "Recall Date");
			this.P8_RecallDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 193, true);
			this.P8_RecallDateDateEdit.Name = "P8_RecallDateDateEdit";
			this.P8_RecallDateDateEdit.TabIndex = 8;
			this.P8_RecallDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			// 
			// OverallDispositionLabel
			// 
			this.BindingSource.SetBindingMember(this.OverallDispositionLabel, "OverallDispositionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).OverallDispositionDescription)));
			this.OverallDispositionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OverallDispositionLabel.ForeColor = System.Drawing.Color.White;
			this.OverallDispositionLabel.IsFontBold = true;
			this.OverallDispositionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 73, true);
			this.OverallDispositionLabel.Name = "OverallDispositionLabel";
			this.OverallDispositionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.OverallDispositionLabel.TabIndex = 14;
			this.OverallDispositionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// OrganizationDetailsGroupBox
			// 
			this.OrganizationDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|1b24ff76-2e3b-4b61-a85e-58f7fcf891b2", "Organization Details");
			this.OrganizationDetailsGroupBox.Controls.Add(this.UNLOCOTextBox);
			this.OrganizationDetailsGroupBox.Controls.Add(this.P8_OHGuidFindBox);
			this.OrganizationDetailsGroupBox.Controls.Add(this.LastQuotedDateEdit);
			this.OrganizationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 3, true);
			this.OrganizationDetailsGroupBox.Name = "OrganizationDetailsGroupBox";
			this.OrganizationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 75, true);
			this.OrganizationDetailsGroupBox.TabIndex = 1;
			this.OrganizationDetailsGroupBox.TabStop = false;
			// 
			// ContactDetailsGroupBox
			// 
			this.ContactDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|6ea73ec1-d0ba-4863-a739-701e5432903f", "Contact Details");
			this.ContactDetailsGroupBox.Controls.Add(this.ContactsDropEdit);
			this.ContactDetailsGroupBox.Controls.Add(this.ContactPhoneDiallerUserControl);
			this.ContactDetailsGroupBox.Controls.Add(this.JobCategoryTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.ContactClosestPortTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.ContactTimeZoneTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.DeliveryStatusLabel);
			this.ContactDetailsGroupBox.Controls.Add(this.DeliveryStatusButton);
			this.ContactDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 84, true);
			this.ContactDetailsGroupBox.Name = "ContactDetailsGroupBox";
			this.ContactDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 127, true);
			this.ContactDetailsGroupBox.TabIndex = 2;
			this.ContactDetailsGroupBox.TabStop = false;
			// 
			// ContactPhoneDiallerUserControl
			// 
			this.ContactPhoneDiallerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactPhoneDiallerUserControl, "P8_OC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_OH)));
			this.ContactPhoneDiallerUserControl.BindToOrg = "P8_OH";
			this.ContactPhoneDiallerUserControl.CurrentOrg = null;
			this.ContactPhoneDiallerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 20, true);
			this.ContactPhoneDiallerUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.ContactPhoneDiallerUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.ContactPhoneDiallerUserControl.Name = "ContactPhoneDiallerUserControl";
			this.ContactPhoneDiallerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.ContactPhoneDiallerUserControl.TabIndex = 1;
			// 
			// JobCategoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobCategoryTextBox, "Contact+JobCategoryDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).Contact.JobCategoryDescription)));
			this.JobCategoryTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|31a1490e-2f83-4867-a8a4-05bcc4a90107", "Job Category");
			this.JobCategoryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JobCategoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 47, true);
			this.JobCategoryTextBox.Name = "JobCategoryTextBox";
			this.JobCategoryTextBox.ReadOnly = true;
			this.JobCategoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 17, true);
			this.JobCategoryTextBox.TabIndex = 2;
			this.JobCategoryTextBox.TabStop = false;
			// 
			// ContactClosestPortTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactClosestPortTextBox, "Contact+OrgClosestPort+NameAndCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).Contact.OrgClosestPort.NameAndCountry)));
			this.ContactClosestPortTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|558ac683-c6b0-4fdc-af65-16aaa89cbf14", "Location");
			this.ContactClosestPortTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactClosestPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 73, true);
			this.ContactClosestPortTextBox.Name = "ContactClosestPortTextBox";
			this.ContactClosestPortTextBox.ReadOnly = true;
			this.ContactClosestPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 17, true);
			this.ContactClosestPortTextBox.TabIndex = 3;
			this.ContactClosestPortTextBox.TabStop = false;
			// 
			// ContactTimeZoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactTimeZoneTextBox, "Contact+OrgClosestPort+TimeZoneDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).Contact.OrgClosestPort.TimeZoneDescription)));
			this.ContactTimeZoneTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|8436f4fb-26d3-492e-803f-e7afa6beee93", "Time Zone");
			this.ContactTimeZoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactTimeZoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 99, true);
			this.ContactTimeZoneTextBox.Name = "ContactTimeZoneTextBox";
			this.ContactTimeZoneTextBox.ReadOnly = true;
			this.ContactTimeZoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 17, true);
			this.ContactTimeZoneTextBox.TabIndex = 4;
			this.ContactTimeZoneTextBox.TabStop = false;
			// 
			// DeliveryStatusLabel
			// 
			this.DeliveryStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right))));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryStatusLabel, false);
			this.DeliveryStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 125, true);
			this.DeliveryStatusLabel.Name = "DeliveryStatusLabel";
			this.DeliveryStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 17, true);
			this.DeliveryStatusLabel.TabIndex = 6;
			this.DeliveryStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DeliveryStatusLabel.Visible = false;
			// 
			// DeliveryStatusButton
			// 
			this.DeliveryStatusButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right))));
			this.DeliveryStatusButton.BackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.Unverified;
			this.DeliveryStatusButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.DeliveryStatusButton.DisplayFocusCues = false;
			this.DeliveryStatusButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 125, true);
			this.DeliveryStatusButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DeliveryStatusButton.Name = "DeliveryStatusButton";
			this.DeliveryStatusButton.NormalBackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.Unverified;
			this.DeliveryStatusButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 17, true);
			this.DeliveryStatusButton.TabIndex = 7;
			this.DeliveryStatusButton.TabStop = false;
			this.DeliveryStatusButton.UseVisualStyleBackColor = true;
			this.DeliveryStatusButton.BringToFront();
			this.DeliveryStatusButton.Visible = false;
			// 
			// LeadSourceGroupBox
			// 
			this.LeadSourceGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8bb765bd-3588-45eb-8d12-6eb311d7f35d", "Lead Source");
			this.LeadSourceGroupBox.Controls.Add(this.ReferringContactSearchEdit);
			this.LeadSourceGroupBox.Controls.Add(this.ReferringOrgGuidFindBox);
			this.LeadSourceGroupBox.Controls.Add(this.OpportunitySourceDropEdit);
			this.LeadSourceGroupBox.Controls.Add(this.SourceDetailsTextBox);
			this.LeadSourceGroupBox.Controls.Add(this.SourceDetailsDropEdit);
			this.LeadSourceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 215, true);
			this.LeadSourceGroupBox.Name = "LeadSourceGroupBox";
			this.LeadSourceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 142, true);
			this.LeadSourceGroupBox.TabIndex = 4;
			this.LeadSourceGroupBox.TabStop = false;
			// 
			// MonetaryValueGroupBox
			// 
			this.MonetaryValueGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fe1a792d-66cd-48fc-8ac4-06e7a754067f", "Monetary Value (p.a)");
			this.MonetaryValueGroupBox.Controls.Add(this.UnsuccessfulValueCalcEdit);
			this.MonetaryValueGroupBox.Controls.Add(this.CommittedValueCalcEdit);
			this.MonetaryValueGroupBox.Controls.Add(this.PipelineValueCalcEdit);
			this.MonetaryValueGroupBox.Controls.Add(this.EstimatedValueCalcFindBox);
			this.MonetaryValueGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 139, true);
			this.MonetaryValueGroupBox.Name = "MonetaryValueGroupBox";
			this.MonetaryValueGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 72, true);
			this.MonetaryValueGroupBox.TabIndex = 8;
			this.MonetaryValueGroupBox.TabStop = false;
			// 
			// UnsuccessfulValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.UnsuccessfulValueCalcEdit, "P8_Calc_UnsuccessfulValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_Calc_UnsuccessfulValue)));
			this.UnsuccessfulValueCalcEdit.DecimalPlaces = 2;
			this.UnsuccessfulValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 48, true);
			this.UnsuccessfulValueCalcEdit.Name = "UnsuccessfulValueCalcEdit";
			this.UnsuccessfulValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.UnsuccessfulValueCalcEdit.TabIndex = 8;
			this.UnsuccessfulValueCalcEdit.Text = "0.00";
			this.UnsuccessfulValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommittedValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CommittedValueCalcEdit, "P8_Calc_CommittedValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_Calc_CommittedValue)));
			this.CommittedValueCalcEdit.DecimalPlaces = 2;
			this.CommittedValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 48, true);
			this.CommittedValueCalcEdit.Name = "CommittedValueCalcEdit";
			this.CommittedValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.CommittedValueCalcEdit.TabIndex = 7;
			this.CommittedValueCalcEdit.Text = "0.00";
			this.CommittedValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PipelineValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PipelineValueCalcEdit, "P8_Calc_PipelineValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).P8_Calc_PipelineValue)));
			this.PipelineValueCalcEdit.DecimalPlaces = 2;
			this.PipelineValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 22, true);
			this.PipelineValueCalcEdit.Name = "PipelineValueCalcEdit";
			this.PipelineValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.PipelineValueCalcEdit.TabIndex = 6;
			this.PipelineValueCalcEdit.Text = "0.00";
			this.PipelineValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OpportunityManagementDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementForm|a69cb205-ab2d-4e6f-8de8-cd677c1535af", "Opportunity");
			this.Controls.Add(this.MonetaryValueGroupBox);
			this.Controls.Add(this.LeadSourceGroupBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.OrganizationDetailsGroupBox);
			this.Controls.Add(this.ContactDetailsGroupBox);
			this.Controls.Add(this.CloseDetailsGroupBox);
			this.Controls.Add(this.OpportunityClientGroupBox);
			this.Controls.Add(this.OpportunityTabControl);
			this.Controls.Add(this.relatedCommunicationGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1173, 575, true);
			this.Name = "OpportunityManagementDetailsControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1173, 605, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.P8_PackageTypeDropEdit.ResumeLayout(true);
			this.P8_PackageTypeDropEdit.PerformLayout();
			this.P8_OpportunityTypeDropEdit.ResumeLayout(true);
			this.P8_OpportunityTypeDropEdit.PerformLayout();
			this.EstimatedValueCalcFindBox.ResumeLayout(true);
			this.EstimatedValueCalcFindBox.PerformLayout();
			this.P8_OA_AssignedOfficeGuidDropEdit.ResumeLayout(true);
			this.P8_OA_AssignedOfficeGuidDropEdit.PerformLayout();
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.ResumeLayout(true);
			this.P8_OC_AssignedOfficeContactGuidSearchEdit.PerformLayout();
			this.AssignedOrgPKGuidFindBox.ResumeLayout(true);
			this.AssignedOrgPKGuidFindBox.PerformLayout();
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.ResumeLayout(true);
			this.P8_GS_NKPrimarySalesPersonCodeFindBox.PerformLayout();
			this.P8_EstimatedCloseDateDateEdit.ResumeLayout(true);
			this.P8_EstimatedCloseDateDateEdit.PerformLayout();
			this.ClosedReasonDropEdit.ResumeLayout(true);
			this.ClosedReasonDropEdit.PerformLayout();
			this.OutcomesDropEdit.ResumeLayout(true);
			this.OutcomesDropEdit.PerformLayout();
			this.DateClosedDateEdit.ResumeLayout(true);
			this.DateClosedDateEdit.PerformLayout();
			this.P8_StageDropEdit.ResumeLayout(true);
			this.P8_StageDropEdit.PerformLayout();
			this.P8_StatusDropEdit.ResumeLayout(true);
			this.P8_StatusDropEdit.PerformLayout();
			this.OpportunityTabControl.ResumeLayout(false);
			this.OpportunityTabControl.PerformLayout();
			this.OppNotesTabPage.ResumeLayout(false);
			this.OppNotesTabPage.PerformLayout();
			this.zRichTextBox1.ResumeLayout(true);
			this.zRichTextBox1.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.OpportunityCustomFieldsControl.ResumeLayout(true);
			this.OpportunityCustomFieldsControl.PerformLayout();
			this.CommissionAgreementTabPage.ResumeLayout(false);
			this.CommissionAgreementTabPage.PerformLayout();
			this.CommissionAgreementsControl.ResumeLayout(true);
			this.CommissionAgreementsControl.PerformLayout();
			this.SalesRelationsTabPage.ResumeLayout(false);
			this.SalesRelationsTabPage.PerformLayout();
			this.StageProgressTabPage.ResumeLayout(false);
			this.StageProgressTabPage.PerformLayout();
			this.opportunityStageProgressControl.ResumeLayout(false);
			this.opportunityStageProgressControl.PerformLayout();
			this.ContactsDropEdit.ResumeLayout(true);
			this.ContactsDropEdit.PerformLayout();
			this.OpportunityClientGroupBox.ResumeLayout(false);
			this.OpportunityClientGroupBox.PerformLayout();
			this.P8_GCGuidFindBox.ResumeLayout(true);
			this.P8_GCGuidFindBox.PerformLayout();
			this.ReferringContactSearchEdit.ResumeLayout(true);
			this.ReferringContactSearchEdit.PerformLayout();
			this.ReferringOrgGuidFindBox.ResumeLayout(true);
			this.ReferringOrgGuidFindBox.PerformLayout();
			this.OpportunitySourceDropEdit.ResumeLayout(true);
			this.OpportunitySourceDropEdit.PerformLayout();
			this.SourceDetailsDropEdit.ResumeLayout(true);
			this.SourceDetailsDropEdit.PerformLayout();
			this.LastQuotedDateEdit.ResumeLayout(true);
			this.LastQuotedDateEdit.PerformLayout();
			this.P8_OHGuidFindBox.ResumeLayout(true);
			this.P8_OHGuidFindBox.PerformLayout();
			this.relatedCommunicationGroupBox.ResumeLayout(false);
			this.relatedCommunicationGroupBox.PerformLayout();
			this.relatedCommunicationGrid.ResumeLayout(true);
			this.relatedCommunicationGrid.PerformLayout();
			this.CloseDetailsGroupBox.ResumeLayout(false);
			this.CloseDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CloseCertaintyTrackBar)).EndInit();
			this.P8_RecallDateDateEdit.ResumeLayout(true);
			this.P8_RecallDateDateEdit.PerformLayout();
			this.OrganizationDetailsGroupBox.ResumeLayout(false);
			this.OrganizationDetailsGroupBox.PerformLayout();
			this.ContactDetailsGroupBox.ResumeLayout(false);
			this.ContactDetailsGroupBox.PerformLayout();
			this.ContactPhoneDiallerUserControl.ResumeLayout(true);
			this.ContactPhoneDiallerUserControl.PerformLayout();
			this.LeadSourceGroupBox.ResumeLayout(false);
			this.LeadSourceGroupBox.PerformLayout();
			this.MonetaryValueGroupBox.ResumeLayout(false);
			this.MonetaryValueGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
