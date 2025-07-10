using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConfigUserControl
	{

		#region Component Designer generated code

		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl ConfigTabControl;
		internal Enterprise.ZArchitecture.GUI.ZTabPage RegistrationCodesTabPage;
		Enterprise.ZArchitecture.GUI.ZGroupBox CusCodesGroupBox;
		internal ZCusCodesGrid CusCodesGrid;
		internal Enterprise.ZArchitecture.GUI.ZMenuItem ExportCodeListToExcelMenuItem;
		internal Enterprise.ZArchitecture.GUI.ZMenuItem MarkVerifiedMenuItem;
		internal Enterprise.ZArchitecture.GUI.ZMenuItem MarkNotVerifiedMenuItem;
		Enterprise.ZArchitecture.GUI.ZTabPage EDICodeMappingTabPage;
		Enterprise.ZArchitecture.GUI.ZGroupBox ImportMappingGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel EDIMappingPanel;
		Enterprise.ZArchitecture.ZGrid OrgPatternMatchOverrideBoundGrid;
		Enterprise.ZArchitecture.GUI.ZTabPage BrandsAndCompanyNames;
		Enterprise.ZArchitecture.GUI.ZGroupBox BrandNamesGroupBox;
		Enterprise.ZArchitecture.ZGrid BrandsOrRelatedNamesGrid;
		Enterprise.ZArchitecture.GUI.ZTabPage EDICommsTabPage;
		Enterprise.ZArchitecture.GUI.ZPanel CommModeDetailsPanel;
		Enterprise.ZArchitecture.ZGrid EdiCommsGrid;
		Enterprise.ZArchitecture.GUI.ZPanel EdiCommGridPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox EDICommunicationModesGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox XmlUniversalGroupBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox sendInternalMilestonesCheckBox;
		Enterprise.ZArchitecture.GUI.ZTabPage NumberFountainsTabPage;
		NumberRangesUserControl NumberRangesUserControl;
		Enterprise.ZArchitecture.GUI.ZGroupBox CommunicationSettingsBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit ModuleDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit CommTransportDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit CommDirectionDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox FileInformationBox;
		Enterprise.ZArchitecture.ZTextBox EmailSubjectTextBox;
		Enterprise.ZArchitecture.ZTextBox FileNameTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit FileFormatDropEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox ReceiverBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox EdiPartyFindBox;
		Enterprise.ZArchitecture.ZTextBox DestinationTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit RecipientRoleDropEdit;
		Enterprise.ZArchitecture.ZTextBox ReceiverVANIDTextBox;
		Enterprise.ZArchitecture.ZTextBox SenderVANIDTextBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox MessageVANFindBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox FTPGroupBox;
		Enterprise.ZArchitecture.ZTextBox FTPLockingMethodTextBox;
		Enterprise.ZArchitecture.ZCalcEdit FTPPortNumberCalcEdit;
		Enterprise.ZArchitecture.ZTextBox FTPLoginNameTextBox;
		Enterprise.ZArchitecture.ZTextBox FTPPasswordTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox MessageInfotmationBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit EventCodeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit EventReferenceConditionDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit PurposeCodeDropEdit;

		System.ComponentModel.IContainer components;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ConfigTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.RegistrationCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EDICodeMappingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BrandsAndCompanyNames = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EDICommsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NumberFountainsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfigTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ConfigTabControl
			// 
			this.ConfigTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ConfigTabControl.Controls.Add(this.RegistrationCodesTabPage);
			this.ConfigTabControl.Controls.Add(this.EDICodeMappingTabPage);
			this.ConfigTabControl.Controls.Add(this.BrandsAndCompanyNames);
			this.ConfigTabControl.Controls.Add(this.EDICommsTabPage);
			this.ConfigTabControl.Controls.Add(this.NumberFountainsTabPage);
			this.ConfigTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.ConfigTabControl.Name = "ConfigTabControl";
			this.ConfigTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 520, true);
			this.ConfigTabControl.TabIndex = 15;
			// 
			// RegistrationCodesTabPage
			// 
			this.RegistrationCodesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|fdad8ef6-038e-4b12-8c44-2b8ed0f591eb", "Registration Numbers / Codes");
			this.RegistrationCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RegistrationCodesTabPage.Name = "RegistrationCodesTabPage";
			this.RegistrationCodesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.RegistrationCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 493, true);
			this.RegistrationCodesTabPage.TabIndex = 0;
			this.RegistrationCodesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RegistrationCodesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).OK_RN_NKCodeCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).OK_CodeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).SecuredCustomsRegNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).CustomsRegNoFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).OK_OA_PremisesAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).IsCurrentCompanyCodeTypePrimary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).VerificationStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).LastVerifiedTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).VerificationAuthority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CustomsCodes)).SyncRoot)).VerificationAuthorityFieldType)));
			// 
			// EDICodeMappingTabPage
			// 
			this.EDICodeMappingTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|82d9416a-56e2-4db1-aee5-b5de822c8483", "EDI Code Mapping");
			this.EDICodeMappingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EDICodeMappingTabPage.Name = "EDICodeMappingTabPage";
			this.EDICodeMappingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.EDICodeMappingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 493, true);
			this.EDICodeMappingTabPage.TabIndex = 1;
			this.EDICodeMappingTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.EDICodeMappingTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PatternMatchOverrides_ForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PatternMatchOverrides_ForBinding)).SyncRoot)).OO_ForeignCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PatternMatchOverrides_ForBinding)).SyncRoot)).OO_ForeignCodeFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PatternMatchOverrides_ForBinding)).SyncRoot)).OO_Relationship)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PatternMatchOverrides_ForBinding)).SyncRoot)).OrgCoNameorGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PatternMatchOverrides_ForBinding)).SyncRoot)).OrgCoFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPatternMatchOverride)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).PatternMatchOverrides_ForBinding)).SyncRoot)).OO_Context)));
			// 
			// BrandsAndCompanyNames
			// 
			this.BrandsAndCompanyNames.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|e24c438b-3454-423b-99ce-512dbf3f9718", "Brands && Company Names");
			this.BrandsAndCompanyNames.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BrandsAndCompanyNames.Name = "BrandsAndCompanyNames";
			this.BrandsAndCompanyNames.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.BrandsAndCompanyNames.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 493, true);
			this.BrandsAndCompanyNames.TabIndex = 2;
			this.BrandsAndCompanyNames.RunWhenBindingOrFirstShown(new System.EventHandler(this.BrandsAndCompanyNames_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).BrandsOrRelatedNames)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgBrandOrRelatedName)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).BrandsOrRelatedNames)).SyncRoot)).P1_RelatedName)));
			// 
			// EDICommsTabPage
			// 
			this.EDICommsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|c7cbc00e-dc1d-4670-830d-12529987946c", "EDI Communications");
			this.EDICommsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EDICommsTabPage.Name = "EDICommsTabPage";
			this.EDICommsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.EDICommsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 493, true);
			this.EDICommsTabPage.TabIndex = 3;
			this.EDICommsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.EDICommsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_CommsDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_FileFormat)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_MessagePurpose)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_CommunicationsTransport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_ServerAddressSubject)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_Filename)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_LocalPartyVanID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_RelatedPartyVanID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_OH_MessageVAN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_FtpLockingMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_PortNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_LoginName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_Password)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_RecipientRole)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_EventCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_EventReferenceConditionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_EventReferenceConditionValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).CommunicationParty)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_CommunicationsTransport)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_ServerAddressSubject)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_Filename)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_Destination)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).CommunicationParty)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_LocalPartyVanID)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_RelatedPartyVanID)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_FtpLockingMethod)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_Password)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_PortNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_LoginName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.EDICommunicationsMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).EDICommunicationsModes)).SyncRoot)).EK_PublishInternalMilestones)));
			// 
			// NumberFountainsTabPage
			// 
			this.NumberFountainsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|0c0cb876-fd43-4cd0-a3da-867f25848c92", "Number Ranges");
			this.NumberFountainsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NumberFountainsTabPage.Name = "NumberFountainsTabPage";
			this.NumberFountainsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.NumberFountainsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 493, true);
			this.NumberFountainsTabPage.TabIndex = 4;
			this.NumberFountainsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NumberFountainsTabPage_InitializeTab));
			// 
			// ConfigUserControl
			// 
			this.Controls.Add(this.ConfigTabControl);
			this.IsModifyConfig = true;
			this.IsModifyConfigBrandsAndCompanyNames = true;
			this.IsModifyConfigEDICodeMapping = true;
			this.IsModifyConfigGeneral = true;
			this.IsModifyConfigRegistrationNumbers = true;
			this.Name = "ConfigUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 544, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.ConfigTabControl, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfigTabControl.ResumeLayout(false);
			this.ConfigTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void RegistrationCodesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.CusCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CusCodesGrid = new Enterprise.MasterFiles.GUI.ZCusCodesGrid();
			this.RegistrationCodesTabPage.SuspendLayout();
			this.CusCodesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusCodesGrid)).BeginInit();
			this.CusCodesGrid.SuspendLayout();
			this.RegistrationCodesTabPage.Controls.Add(this.CusCodesGroupBox);
			// 
			// CusCodesGroupBox
			// 
			this.CusCodesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|20683a99-215b-4247-8503-31e0ae8458c0", "Registration Numbers / Codes");
			this.CusCodesGroupBox.Controls.Add(this.CusCodesGrid);
			this.CusCodesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.CusCodesGroupBox.Name = "CusCodesGroupBox";
			this.CusCodesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.CusCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(878, 483, true);
			this.CusCodesGroupBox.TabIndex = 2;
			this.CusCodesGroupBox.TabStop = false;
			// 
			// CusCodesGrid
			// 
			this.CusCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CusCodesGrid, "CustomsCodes");
			this.CusCodesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OK_RN_NKCodeCountry";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Select Country";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "OK_CodeType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.ColumnName = "SecuredCustomsRegNo";
			zMultiControlColumnStyleInfo1.DefaultCollectionIndex = 0;
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CustomsRegNoFieldType";
			zMultiControlColumnStyleInfo1.IsMandatory = true;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zGuidDropEditColumnStyleInfo1.ColumnName = "OK_OA_PremisesAddress";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.ToolTip = "Select a physical address that this registration number relates to.";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d847f5a9-1f5a-4c24-b1ac-e7375aa52873", "Primary Code Type");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsCurrentCompanyCodeTypePrimary";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "VerificationStatus";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDateEditColumnStyleInfo1.ColumnName = "LastVerifiedTime";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiControlColumnStyleInfo2.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo2.ColumnName = "VerificationAuthority";
			zMultiControlColumnStyleInfo2.DefaultCollectionIndex = 0;
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "VerificationAuthorityFieldType";
			zMultiControlColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.CusCodesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CusCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CusCodesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.CusCodesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.CusCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CusCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CusCodesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CusCodesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.CusCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusCodesGrid.GridId = "1408e3b5-3235-40d4-8d8f-43f089d10b7b";
			this.CusCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusCodesGrid.LayoutKey = "CusCodesGrid";
			this.CusCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 18, true);
			this.CusCodesGrid.Name = "CusCodesGrid";
			this.CusCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 460, true);
			this.CusCodesGrid.TabIndex = 1;
			this.RegistrationCodesTabPage.PerformLayout();
			this.CusCodesGroupBox.ResumeLayout(false);
			this.CusCodesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusCodesGrid)).EndInit();
			this.CusCodesGrid.ResumeLayout(false);
			this.CusCodesGrid.PerformLayout();
			this.RegistrationCodesTabPage.ResumeLayout(true);
		}

		private void EDICodeMappingTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ImportMappingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EDIMappingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrgPatternMatchOverrideBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EDICodeMappingTabPage.SuspendLayout();
			this.ImportMappingGroupBox.SuspendLayout();
			this.EDIMappingPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgPatternMatchOverrideBoundGrid)).BeginInit();
			this.OrgPatternMatchOverrideBoundGrid.SuspendLayout();
			this.EDICodeMappingTabPage.Controls.Add(this.ImportMappingGroupBox);
			// 
			// ImportMappingGroupBox
			// 
			this.ImportMappingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|5e396747-bb50-42f1-a451-89ebbeaa9df5", "EDI Mapping");
			this.ImportMappingGroupBox.Controls.Add(this.EDIMappingPanel);
			this.ImportMappingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportMappingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.ImportMappingGroupBox.Name = "ImportMappingGroupBox";
			this.ImportMappingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(878, 483, true);
			this.ImportMappingGroupBox.TabIndex = 3;
			this.ImportMappingGroupBox.TabStop = false;
			// 
			// EDIMappingPanel
			// 
			this.EDIMappingPanel.Controls.Add(this.OrgPatternMatchOverrideBoundGrid);
			this.EDIMappingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EDIMappingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EDIMappingPanel.Name = "EDIMappingPanel";
			this.EDIMappingPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.EDIMappingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 464, true);
			this.EDIMappingPanel.TabIndex = 13;
			// 
			// OrgPatternMatchOverrideBoundGrid
			// 
			this.OrgPatternMatchOverrideBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgPatternMatchOverrideBoundGrid, "PatternMatchOverrides_ForBinding");
			this.OrgPatternMatchOverrideBoundGrid.CaptionVisible = false;
			zMultiControlColumnStyleInfo3.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo3.ColumnName = "OO_ForeignCode";
			zMultiControlColumnStyleInfo3.DefaultCollectionIndex = 0;
			zMultiControlColumnStyleInfo3.FieldTypeColumnName = "OO_ForeignCodeFieldType";
			zMultiControlColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(275);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "OO_Relationship";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.ToolTip = "Type of relationship that is to be mapped to the EDI codes.";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo4.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo4.ColumnName = "OrgCoNameorGuid";
			zMultiControlColumnStyleInfo4.DefaultCollectionIndex = 0;
			zMultiControlColumnStyleInfo4.FieldTypeColumnName = "OrgCoFieldType";
			zMultiControlColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.ColumnName = "OO_Context";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrgPatternMatchOverrideBoundGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo3);
			this.OrgPatternMatchOverrideBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OrgPatternMatchOverrideBoundGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo4);
			this.OrgPatternMatchOverrideBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OrgPatternMatchOverrideBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgPatternMatchOverrideBoundGrid.GridId = "b3015997-df13-4688-96cd-414ee1f04fa8";
			this.OrgPatternMatchOverrideBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgPatternMatchOverrideBoundGrid.LayoutKey = "OrgPatternMatchOverrideBoundGrid";
			this.OrgPatternMatchOverrideBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.OrgPatternMatchOverrideBoundGrid.Name = "OrgPatternMatchOverrideBoundGrid";
			this.OrgPatternMatchOverrideBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 454, true);
			this.OrgPatternMatchOverrideBoundGrid.TabIndex = 0;
			this.EDICodeMappingTabPage.PerformLayout();
			this.ImportMappingGroupBox.ResumeLayout(false);
			this.ImportMappingGroupBox.PerformLayout();
			this.EDIMappingPanel.ResumeLayout(false);
			this.EDIMappingPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgPatternMatchOverrideBoundGrid)).EndInit();
			this.OrgPatternMatchOverrideBoundGrid.ResumeLayout(false);
			this.OrgPatternMatchOverrideBoundGrid.PerformLayout();
			this.EDICodeMappingTabPage.ResumeLayout(true);
		}

		private void BrandsAndCompanyNames_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BrandNamesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BrandsOrRelatedNamesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BrandsAndCompanyNames.SuspendLayout();
			this.BrandNamesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BrandsOrRelatedNamesGrid)).BeginInit();
			this.BrandsOrRelatedNamesGrid.SuspendLayout();
			this.BrandsAndCompanyNames.Controls.Add(this.BrandNamesGroupBox);
			// 
			// BrandNamesGroupBox
			// 
			this.BrandNamesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|be788de7-4775-4833-b726-85476066f502", "Brand Names Or Related Company Names");
			this.BrandNamesGroupBox.Controls.Add(this.BrandsOrRelatedNamesGrid);
			this.BrandNamesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BrandNamesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.BrandNamesGroupBox.Name = "BrandNamesGroupBox";
			this.BrandNamesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(878, 483, true);
			this.BrandNamesGroupBox.TabIndex = 6;
			this.BrandNamesGroupBox.TabStop = false;
			// 
			// BrandsOrRelatedNamesGrid
			// 
			this.BrandsOrRelatedNamesGrid.AllowNavigation = false;
			this.BrandsOrRelatedNamesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BrandsOrRelatedNamesGrid, "BrandsOrRelatedNames");
			this.BrandsOrRelatedNamesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "P1_RelatedName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.BrandsOrRelatedNamesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BrandsOrRelatedNamesGrid.GridId = "2963be5b-fac9-4fc2-836a-fab4f03bc30d";
			this.BrandsOrRelatedNamesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BrandsOrRelatedNamesGrid.LayoutKey = "CusCodesGrid";
			this.BrandsOrRelatedNamesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.BrandsOrRelatedNamesGrid.Name = "BrandsOrRelatedNamesGrid";
			this.BrandsOrRelatedNamesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 459, true);
			this.BrandsOrRelatedNamesGrid.TabIndex = 0;
			this.BrandsAndCompanyNames.PerformLayout();
			this.BrandNamesGroupBox.ResumeLayout(false);
			this.BrandNamesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BrandsOrRelatedNamesGrid)).EndInit();
			this.BrandsOrRelatedNamesGrid.ResumeLayout(false);
			this.BrandsOrRelatedNamesGrid.PerformLayout();
			this.BrandsAndCompanyNames.ResumeLayout(true);
		}

		private void EDICommsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.EDICommunicationModesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EdiCommGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EdiCommsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CommModeDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CommunicationSettingsBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommTransportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommDirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RecipientRoleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EventCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EventReferenceConditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PurposeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FileFormatDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EmailSubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DestinationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EdiPartyFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MessageVANFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.FileInformationBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReceiverBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageInfotmationBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SenderVANIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceiverVANIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FTPGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FTPLockingMethodTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FTPPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FTPPortNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FTPLoginNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.XmlUniversalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.sendInternalMilestonesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EDICommsTabPage.SuspendLayout();
			this.EDICommunicationModesGroupBox.SuspendLayout();
			this.EdiCommGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EdiCommsGrid)).BeginInit();
			this.EdiCommsGrid.SuspendLayout();
			this.CommModeDetailsPanel.SuspendLayout();
			this.CommunicationSettingsBox.SuspendLayout();
			this.CommTransportDropEdit.SuspendLayout();
			this.CommDirectionDropEdit.SuspendLayout();
			this.EventCodeDropEdit.SuspendLayout();
			this.EventReferenceConditionDropEdit.SuspendLayout();
			this.PurposeCodeDropEdit.SuspendLayout();
			this.FileFormatDropEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.ModuleDropEdit.SuspendLayout();
			this.EdiPartyFindBox.SuspendLayout();
			this.MessageVANFindBox.SuspendLayout();
			this.FileInformationBox.SuspendLayout();
			this.ReceiverBox.SuspendLayout();
			this.MessageInfotmationBox.SuspendLayout();
			this.FTPGroupBox.SuspendLayout();
			this.XmlUniversalGroupBox.SuspendLayout();
			this.EDICommsTabPage.Controls.Add(this.EDICommunicationModesGroupBox);
			// 
			// EDICommunicationModesGroupBox
			// 
			this.EDICommunicationModesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|b3de019b-b020-4968-a474-6383210fb555", "EDI Communication Modes");
			this.EDICommunicationModesGroupBox.Controls.Add(this.EdiCommGridPanel);
			this.EDICommunicationModesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EDICommunicationModesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.EDICommunicationModesGroupBox.Name = "EDICommunicationModesGroupBox";
			this.EDICommunicationModesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(878, 483, true);
			this.EDICommunicationModesGroupBox.TabIndex = 7;
			this.EDICommunicationModesGroupBox.TabStop = false;
			// 
			// EdiCommGridPanel
			// 
			this.EdiCommGridPanel.Controls.Add(this.EdiCommsGrid);
			this.EdiCommGridPanel.Controls.Add(this.CommModeDetailsPanel);
			this.EdiCommGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EdiCommGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EdiCommGridPanel.Name = "EdiCommGridPanel";
			this.EdiCommGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 464, true);
			this.EdiCommGridPanel.TabIndex = 7;
			// 
			// EdiCommsGrid
			// 
			this.EdiCommsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EdiCommsGrid, "EDICommunicationsModes");
			this.EdiCommsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.ColumnName = "EK_Module";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDropEditColumnStyleInfo6.ColumnName = "EK_CommsDirection";
			zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo7.ColumnName = "EK_FileFormat";
			zDropEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zDropEditColumnStyleInfo8.ColumnName = "EK_MessagePurpose";
			zDropEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDropEditColumnStyleInfo9.ColumnName = "EK_CommunicationsTransport";
			zDropEditColumnStyleInfo9.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo9.IsVisible = false;
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo2.ColumnName = "EK_ServerAddressSubject";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "EK_Filename";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "EK_Destination";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(238);
			zTextBoxColumnStyleInfo5.ColumnName = "EK_LocalPartyVanID";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "EK_RelatedPartyVanID";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "EK_OH_MessageVAN";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo7.ColumnName = "EK_FtpLockingMethod";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|e004aefc-6898-4134-a66f-433ebc673f0e", "FTP");
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "EK_PortNumber";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|e004aefc-6898-4134-a66f-433ebc673f0e", "FTP");
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo8.ColumnName = "EK_LoginName";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|e004aefc-6898-4134-a66f-433ebc673f0e", "FTP");
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "EK_Password";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|e004aefc-6898-4134-a66f-433ebc673f0e", "FTP");
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo10.ColumnName = "EK_TransportMode";
			zDropEditColumnStyleInfo10.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo10.IsVisible = false;
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo11.ColumnName = "EK_RecipientRole";
			zDropEditColumnStyleInfo11.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo11.IsVisible = false;
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo12.ColumnName = "EK_EventCode";
			zDropEditColumnStyleInfo12.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo12.IsVisible = false;
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo13.ColumnName = "EK_EventReferenceConditionType";
			zDropEditColumnStyleInfo13.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo13.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("a70c0feb-f789-4e37-8559-c70aa8401d2e", "Event Reference Condition");
			zDropEditColumnStyleInfo13.IsVisible = false;
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "EK_EventReferenceConditionValue";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("a70c0feb-f789-4e37-8559-c70aa8401d2e", "Event Reference Condition");
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("818E04E2-6DA4-40BF-8B7B-55BB71A5FA23", "EDI Client");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CommunicationParty";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Messaging.EDICommunicationParty;
			this.EdiCommsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.EdiCommsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.EdiCommsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.EdiCommsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.EdiCommsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.EdiCommsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EdiCommsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EdiCommsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EdiCommsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EdiCommsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EdiCommsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.EdiCommsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EdiCommsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EdiCommsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.EdiCommsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.EdiCommsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.EdiCommsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.EdiCommsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.EdiCommsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.EdiCommsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			if (ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.EAdaptorNextFeature) != null)
			{
				this.EdiCommsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			}
			this.EdiCommsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EdiCommsGrid.GridId = "46000bf4-171c-4f94-94d4-aae225594450";
			this.EdiCommsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EdiCommsGrid.LayoutKey = "EdiCommsGrid";
			this.EdiCommsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EdiCommsGrid.Name = "EdiCommsGrid";
			this.EdiCommsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 464, true);
			this.EdiCommsGrid.TabIndex = 0;
			this.EdiCommsGrid.AfterBind += new System.EventHandler(this.EdiCommsGrid_AfterBind);
			this.EdiCommsGrid.CurrentCellChanged += new System.EventHandler(this.EdiCommsGrid_CurrentCellChanged);
			this.EdiCommsGrid.Disposed += new System.EventHandler(this.EdiCommsGrid_Disposed);
			// 
			// CommModeDetailsPanel
			// 
			this.CommModeDetailsPanel.Controls.Add(this.XmlUniversalGroupBox);
			this.CommModeDetailsPanel.Controls.Add(this.MessageInfotmationBox);
			this.CommModeDetailsPanel.Controls.Add(this.FTPGroupBox);
			this.CommModeDetailsPanel.Controls.Add(this.ReceiverBox);
			this.CommModeDetailsPanel.Controls.Add(this.FileInformationBox);
			this.CommModeDetailsPanel.Controls.Add(this.CommunicationSettingsBox);
			this.CommModeDetailsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.CommModeDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 0, true);
			this.CommModeDetailsPanel.Name = "CommModeDetailsPanel";
			this.CommModeDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0, true);
			this.CommModeDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 464, true);
			this.CommModeDetailsPanel.TabIndex = 6;
			this.CommModeDetailsPanel.AutoScroll = true;
			// 
			// Communication Settings
			// 
			this.CommunicationSettingsBox.AutoSize = true;
			this.CommunicationSettingsBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|af17bad9-5d94-49f6-86ab-e3d9465e6bab", "Communication Settings");
			this.CommunicationSettingsBox.Controls.Add(this.ModuleDropEdit);
			this.CommunicationSettingsBox.Controls.Add(this.CommTransportDropEdit);
			this.CommunicationSettingsBox.Controls.Add(this.CommDirectionDropEdit);
			this.CommunicationSettingsBox.Controls.Add(this.TransportModeDropEdit);
			this.CommunicationSettingsBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CommunicationSettingsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.CommunicationSettingsBox.Name = "CommunicationSettingsBox";
			this.CommunicationSettingsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 90, true);
			this.CommunicationSettingsBox.TabIndex = 34;
			this.CommunicationSettingsBox.TabStop = false;
			// 
			// ModuleDropEdit
			// 
			this.ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModuleDropEdit, "EDICommunicationsModes.EK_Module");
			this.ModuleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 20, true);
			this.ModuleDropEdit.Name = "ModuleDropEdit";
			this.ModuleDropEdit.PreBoundMaxLength = 5;
			this.ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.ModuleDropEdit.TabIndex = 0;
			// 
			// CommTransportDropEdit
			// 
			this.CommTransportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommTransportDropEdit, "EDICommunicationsModes.EK_CommunicationsTransport");
			this.CommTransportDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 42, true);
			this.CommTransportDropEdit.Name = "CommTransportDropEdit";
			this.CommTransportDropEdit.PreBoundMaxLength = 5;
			this.CommTransportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.CommTransportDropEdit.TabIndex = 1;
			// 
			// CommDirectionDropEdit
			// 
			this.CommDirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommDirectionDropEdit, "EDICommunicationsModes.EK_CommsDirection");
			this.CommDirectionDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommDirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 64, true);
			this.CommDirectionDropEdit.Name = "CommDirectionDropEdit";
			this.CommDirectionDropEdit.PreBoundMaxLength = 5;
			this.CommDirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.CommDirectionDropEdit.TabIndex = 2;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "EDICommunicationsModes.EK_TransportMode");
			this.TransportModeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 86, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 5;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.TransportModeDropEdit.TabIndex = 3;
			// 
			// FileInformationBox
			// 
			this.FileInformationBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|738b0ae4-ab43-4ab4-b8ad-a62c616d29d1", "File Information");
			this.FileInformationBox.Controls.Add(this.EmailSubjectTextBox);
			this.FileInformationBox.Controls.Add(this.FileNameTextBox);
			this.FileInformationBox.Controls.Add(this.FileFormatDropEdit);
			this.FileInformationBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FileInformationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 135, true);
			this.FileInformationBox.Name = "FileInformationBox";
			this.FileInformationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 90, true);
			this.FileInformationBox.TabIndex = 35;
			this.FileInformationBox.TabStop = false;
			// 
			// EmailSubjectTextBox
			// 
			this.EmailSubjectTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EmailSubjectTextBox, "EDICommunicationsModes.EK_ServerAddressSubject");
			this.EmailSubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EmailSubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 20, true);
			this.EmailSubjectTextBox.Name = "EmailSubjectTextBox";
			this.EmailSubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.EmailSubjectTextBox.TabIndex = 4;
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FileNameTextBox, "EDICommunicationsModes.EK_Filename");
			this.FileNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 42, true);
			this.FileNameTextBox.Name = "FileNameTextBox";
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.FileNameTextBox.TabIndex = 5;
			// 
			// FileFormatDropEdit
			// 
			this.FileFormatDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FileFormatDropEdit, "EDICommunicationsModes.EK_FileFormat");
			this.FileFormatDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FileFormatDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 64, true);
			this.FileFormatDropEdit.Name = "FileFormatDropEdit";
			this.FileFormatDropEdit.PreBoundMaxLength = 5;
			this.FileFormatDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.FileFormatDropEdit.TabIndex = 6;
			// 
			// ReceiverBox
			// 
			this.ReceiverBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|738b0ae4-ab43-4ab4-b8ad-a62c616d29d2", "Receiver");
			this.ReceiverBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ReceiverBox.Controls.Add(this.EdiPartyFindBox);
			this.ReceiverBox.Controls.Add(this.DestinationTextBox);
			this.ReceiverBox.Controls.Add(this.RecipientRoleDropEdit);
			this.ReceiverBox.Controls.Add(this.SenderVANIDTextBox);
			this.ReceiverBox.Controls.Add(this.ReceiverVANIDTextBox);
			this.ReceiverBox.Controls.Add(this.MessageVANFindBox);
			this.ReceiverBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 220, true);
			this.ReceiverBox.Name = "ReceiverBox";
			this.ReceiverBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 153, true);
			this.ReceiverBox.TabIndex = 36;
			this.ReceiverBox.TabStop = false;
			// 
			// EdiPartyFindBox
			// 
			this.EdiPartyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EdiPartyFindBox, "EDICommunicationsModes.CommunicationParty");
			this.EdiPartyFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|6DF51088-6C49-459D-ABD4-C5E63A59A43B", "EDI Client");
			this.EdiPartyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 18, true);
			this.EdiPartyFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Messaging.EDICommunicationParty;
			this.EdiPartyFindBox.Name = "EdiPartyFindBox";
			this.EdiPartyFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.EdiPartyFindBox.ParentType = null;
			this.EdiPartyFindBox.ShowDescriptionBox = false;
			this.EdiPartyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.EdiPartyFindBox.TabIndex = 7;
			this.EdiPartyFindBox.Visible = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.EAdaptorNextFeature) != null;
			// 
			// DestinationTextBox
			// 
			this.DestinationTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DestinationTextBox, "EDICommunicationsModes.EK_Destination");
			this.DestinationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DestinationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 40, true);
			this.DestinationTextBox.Name = "DestinationTextBox";
			this.DestinationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.DestinationTextBox.TabIndex = 8;
			// 
			// RecipientRoleDropEdit
			// 
			this.RecipientRoleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RecipientRoleDropEdit, "EDICommunicationsModes.EK_RecipientRole");
			this.RecipientRoleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RecipientRoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 62, true);
			this.RecipientRoleDropEdit.Name = "RecipientRoleDropEdit";
			this.RecipientRoleDropEdit.PreBoundMaxLength = 5;
			this.RecipientRoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.RecipientRoleDropEdit.TabIndex = 9;
			// 
			// SenderVANIDTextBox
			// 
			this.SenderVANIDTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SenderVANIDTextBox, "EDICommunicationsModes.EK_LocalPartyVanID");
			this.SenderVANIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SenderVANIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 84, true);
			this.SenderVANIDTextBox.Name = "SenderVANIDTextBox";
			this.SenderVANIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.SenderVANIDTextBox.TabIndex = 10;
			// 
			// ReceiverVANIDTextBox
			// 
			this.ReceiverVANIDTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReceiverVANIDTextBox, "EDICommunicationsModes.EK_RelatedPartyVanID");
			this.ReceiverVANIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReceiverVANIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 106, true);
			this.ReceiverVANIDTextBox.Name = "ReceiverVANIDTextBox";
			this.ReceiverVANIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.ReceiverVANIDTextBox.TabIndex = 11;
			// 
			// MessageVANFindBox
			// 
			this.MessageVANFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageVANFindBox, "EDICommunicationsModes.EK_OH_MessageVAN");
			this.MessageVANFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|6DF51088-6C49-459D-ABD4-C5E63A58A43B", "Message VAN");
			this.MessageVANFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 128, true);
			this.MessageVANFindBox.Name = "MessageVANFindBox";
			this.MessageVANFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.MessageVANFindBox.TabIndex = 12;
			// 
			// FTPGroupBox
			// 
			this.FTPGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|1c6a99ea-9981-4a67-9382-a281607aef58", "FTP");
			this.FTPGroupBox.Controls.Add(this.FTPLockingMethodTextBox);
			this.FTPGroupBox.Controls.Add(this.FTPPortNumberCalcEdit);
			this.FTPGroupBox.Controls.Add(this.FTPLoginNameTextBox);
			this.FTPGroupBox.Controls.Add(this.FTPPasswordTextBox);
			this.FTPGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FTPGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 350, true);
			this.FTPGroupBox.Name = "FTPGroupBox";
			this.FTPGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 108, true);
			this.FTPGroupBox.TabIndex = 37;
			this.FTPGroupBox.TabStop = false;
			// 
			// FTPLockingMethodTextBox
			// 
			this.FTPLockingMethodTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FTPLockingMethodTextBox, "EDICommunicationsModes.EK_FtpLockingMethod");
			this.FTPLockingMethodTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FTPLockingMethodTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 18, true);
			this.FTPLockingMethodTextBox.Name = "FTPLockingMethodTextBox";
			this.FTPLockingMethodTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.FTPLockingMethodTextBox.TabIndex = 13;
			// 
			// FTPPasswordTextBox
			// 
			this.FTPPasswordTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FTPPasswordTextBox, "EDICommunicationsModes.EK_Password");
			this.FTPPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FTPPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 40, true);
			this.FTPPasswordTextBox.Name = "FTPPasswordTextBox";
			this.FTPPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.FTPPasswordTextBox.TabIndex = 14;
			// 
			// FTPPortNumberCalcEdit
			// 
			this.FTPPortNumberCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FTPPortNumberCalcEdit, "EDICommunicationsModes.EK_PortNumber");
			this.FTPPortNumberCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FTPPortNumberCalcEdit.DecimalPlaces = 0;
			this.FTPPortNumberCalcEdit.Decimals = 0;
			this.FTPPortNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 62, true);
			this.FTPPortNumberCalcEdit.Name = "FTPPortNumberCalcEdit";
			this.FTPPortNumberCalcEdit.ShowGroupSeparators = false;
			this.FTPPortNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.FTPPortNumberCalcEdit.TabIndex = 15;
			this.FTPPortNumberCalcEdit.Text = "0";
			this.FTPPortNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.FTPPortNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// FTPLoginNameTextBox
			// 
			this.FTPLoginNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FTPLoginNameTextBox, "EDICommunicationsModes.EK_LoginName");
			this.FTPLoginNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FTPLoginNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 84, true);
			this.FTPLoginNameTextBox.Name = "FTPLoginNameTextBox";
			this.FTPLoginNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.FTPLoginNameTextBox.TabIndex = 16;
			// 
			// MessageInfotmationBox
			// 
			this.MessageInfotmationBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|738b0ae4-ab43-4ab4-b8ad-a62c616d29d3", "Message Information");
			this.MessageInfotmationBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageInfotmationBox.Controls.Add(this.EventCodeDropEdit);
			this.MessageInfotmationBox.Controls.Add(this.EventReferenceConditionDropEdit);
			this.MessageInfotmationBox.Controls.Add(this.PurposeCodeDropEdit);
			this.MessageInfotmationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 470, true);
			this.MessageInfotmationBox.Name = "MessageInfotmationBox";
			this.MessageInfotmationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 90, true);
			this.MessageInfotmationBox.TabIndex = 38;
			this.MessageInfotmationBox.TabStop = false;
			// 
			// EventCodeDropEdit
			// 
			this.EventCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EventCodeDropEdit, "EDICommunicationsModes.EK_EventCode");
			this.EventCodeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EventCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 18, true);
			this.EventCodeDropEdit.Name = "EventCodeDropEdit";
			this.EventCodeDropEdit.PreBoundMaxLength = 5;
			this.EventCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.EventCodeDropEdit.TabIndex = 17;
			// 
			// EventReferenceConditionDropEdit
			// 
			this.EventReferenceConditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EventReferenceConditionDropEdit, "EDICommunicationsModes.EK_EventReferenceConditionType");
			this.EventReferenceConditionDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EventReferenceConditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 42, true);
			this.EventReferenceConditionDropEdit.Name = "EventReferenceConditionDropEdit";
			this.EventReferenceConditionDropEdit.PreBoundMaxLength = 5;
			this.EventReferenceConditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.EventReferenceConditionDropEdit.TabIndex = 18;
			// 
			// PurposeCodeDropEdit
			// 
			this.PurposeCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PurposeCodeDropEdit, "EDICommunicationsModes.EK_MessagePurpose");
			this.PurposeCodeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PurposeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 64, true);
			this.PurposeCodeDropEdit.Name = "PurposeCodeDropEdit";
			this.PurposeCodeDropEdit.PreBoundMaxLength = 5;
			this.PurposeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.PurposeCodeDropEdit.TabIndex = 19;
			// 
			// XmlUniversalGroupBox
			// 
			this.XmlUniversalGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConfigUserControl|f220ef33-b1ef-4e8d-9286-7a1210f8ce1b", "XML Universal Shipment");
			this.XmlUniversalGroupBox.Controls.Add(this.sendInternalMilestonesCheckBox);
			this.XmlUniversalGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.XmlUniversalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 550, true);
			this.XmlUniversalGroupBox.Name = "XmlUniversalGroupBox";
			this.XmlUniversalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 47, true);
			this.XmlUniversalGroupBox.TabIndex = 39;
			this.XmlUniversalGroupBox.TabStop = false;
			// 
			// sendInternalMilestonesCheckBox
			// 
			this.sendInternalMilestonesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.sendInternalMilestonesCheckBox, "EDICommunicationsModes.EK_PublishInternalMilestones");
			this.sendInternalMilestonesCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("02e8e8cf-1b19-458e-b523-01614cfd3557", "Send Internal Milestones");
			this.sendInternalMilestonesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.sendInternalMilestonesCheckBox.Name = "sendInternalMilestonesCheckBox";
			this.sendInternalMilestonesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 17, true);
			this.sendInternalMilestonesCheckBox.TabIndex = 0;
			this.EDICommsTabPage.PerformLayout();
			this.EDICommunicationModesGroupBox.ResumeLayout(false);
			this.EDICommunicationModesGroupBox.PerformLayout();
			this.EdiCommGridPanel.ResumeLayout(false);
			this.EdiCommGridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EdiCommsGrid)).EndInit();
			this.EdiCommsGrid.ResumeLayout(false);
			this.EdiCommsGrid.PerformLayout();
			this.CommModeDetailsPanel.ResumeLayout(false);
			this.CommModeDetailsPanel.PerformLayout();
			//
			//CommunicationSettingsBox
			//
			this.CommunicationSettingsBox.ResumeLayout(false);
			this.CommunicationSettingsBox.PerformLayout();
			this.ModuleDropEdit.ResumeLayout(true);
			this.ModuleDropEdit.PerformLayout();
			this.CommTransportDropEdit.ResumeLayout(true);
			this.CommTransportDropEdit.PerformLayout();
			this.CommDirectionDropEdit.ResumeLayout(true);
			this.CommDirectionDropEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			//
			//FileInformationBox
			//
			this.FileInformationBox.ResumeLayout(false);
			this.FileInformationBox.PerformLayout();
			this.EmailSubjectTextBox.ResumeLayout(false);
			this.EmailSubjectTextBox.PerformLayout();
			this.FileNameTextBox.ResumeLayout(false);
			this.FileNameTextBox.PerformLayout();
			this.FileFormatDropEdit.ResumeLayout(false);
			this.FileFormatDropEdit.PerformLayout();
			//
			//ReceiverBox
			//
			this.ReceiverBox.ResumeLayout(false);
			this.ReceiverBox.PerformLayout();
			this.EdiPartyFindBox.ResumeLayout(true);
			this.EdiPartyFindBox.PerformLayout();
			this.DestinationTextBox.ResumeLayout(true);
			this.DestinationTextBox.PerformLayout();
			this.RecipientRoleDropEdit.ResumeLayout(true);
			this.RecipientRoleDropEdit.PerformLayout();
			this.SenderVANIDTextBox.ResumeLayout(true);
			this.SenderVANIDTextBox.PerformLayout();
			this.ReceiverVANIDTextBox.ResumeLayout(true);
			this.ReceiverVANIDTextBox.PerformLayout();
			this.MessageVANFindBox.ResumeLayout(true);
			this.MessageVANFindBox.PerformLayout();
			//
			//FTPGroupBox
			//
			this.FTPGroupBox.ResumeLayout(false);
			this.FTPGroupBox.PerformLayout();
			this.FTPLockingMethodTextBox.ResumeLayout(false);
			this.FTPLockingMethodTextBox.PerformLayout();
			this.FTPPortNumberCalcEdit.ResumeLayout(false);
			this.FTPPortNumberCalcEdit.PerformLayout();
			this.FTPLoginNameTextBox.ResumeLayout(false);
			this.FTPLoginNameTextBox.PerformLayout();
			this.FTPPasswordTextBox.ResumeLayout(false);
			this.FTPPasswordTextBox.PerformLayout();
			//
			//MessageInfotmationBox
			//
			this.MessageInfotmationBox.ResumeLayout(false);
			this.MessageInfotmationBox.PerformLayout();
			this.EventCodeDropEdit.ResumeLayout(false);
			this.EventCodeDropEdit.PerformLayout();
			this.EventReferenceConditionDropEdit.ResumeLayout(false);
			this.EventReferenceConditionDropEdit.PerformLayout();
			this.PurposeCodeDropEdit.ResumeLayout(false);
			this.PurposeCodeDropEdit.PerformLayout();

			this.XmlUniversalGroupBox.ResumeLayout(false);
			this.XmlUniversalGroupBox.PerformLayout();
			this.EDICommsTabPage.ResumeLayout(true);
		}

		private void NumberFountainsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NumberRangesUserControl = new Enterprise.MasterFiles.GUI.NumberRangesUserControl();
			this.NumberFountainsTabPage.SuspendLayout();
			this.NumberRangesUserControl.SuspendLayout();
			this.NumberFountainsTabPage.Controls.Add(this.NumberRangesUserControl);
			// 
			// NumberRangesUserControl
			// 
			this.NumberRangesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NumberRangesUserControl, ".");
			this.NumberRangesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumberRangesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.NumberRangesUserControl.Name = "NumberRangesUserControl";
			this.NumberRangesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(878, 483, true);
			this.NumberRangesUserControl.TabIndex = 0;
			this.NumberFountainsTabPage.PerformLayout();
			this.NumberRangesUserControl.ResumeLayout(true);
			this.NumberRangesUserControl.PerformLayout();
			this.NumberFountainsTabPage.ResumeLayout(true);

		}

		#endregion

	}
}
