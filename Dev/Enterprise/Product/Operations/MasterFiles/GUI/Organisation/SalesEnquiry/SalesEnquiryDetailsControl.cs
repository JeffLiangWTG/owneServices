using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesEnquiryDetailsControl : ZUserControl, ISupportWebAddressValidationControl
	{
		public SalesEnquiryDetailsControl()
		{
			InitializeComponent();
			SetCharacterCasing();

			if (!DesignModeFinder.IsDesigning)
			{
				if (Env.Instance.Registry.EnableAddressValidationWebService)
				{
					cancellationToken = new CancellationTokenSource();
					this.HandleCreated += (o, e) =>
					{
						if (this.ParentForm != null)
						{
							this.ParentForm.FormClosed += (x, y) =>
							{
								if (cancellationToken != null)
								{
									cancellationToken.Cancel();
								}
							};
						}
					};
					HookISupportWebAddressValidationControlChangeFocusEvents();
				}
				else
				{
					ValidateAddressButton.Visible = false;
					ClearFieldsButton.Visible = false;
				}
			}
			ValidationJustForced = false;
		}

		public void Initialize(SalesEnquiry enquiry)
		{
			Enquiry = enquiry;

			this.relatedCommunicationGrid.InnerGrid.ColorContextKey = RelatedCommunicationForm.RelatedCommunicationColorContextKey;
			SetupContactControls();
			UpdateButtonStates();
			UpdateControlVisibility();

			enquiry.HasChangesChanged += enquiry_HasChangesChanged;
			enquiry.O1_LeadStatusInfo.ValueChanged += O1_LeadStatusInfo_ValueChanged;
			enquiry.OrgPkInfo.ValueChanged += OrgPk_ValueChanged;
			EnquiryCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("0D1C02BB-F89D-4524-A410-FD9AF1A26A6B", "To make use of this tab, please setup inquiry custom fields in Workflow Manager.");

			CreateSalesOpportunityButton.Click += CreateSalesOpportunityButton_Click;
			salesRelationControl.FormShowingForNewActivity += salesRelationControl_FormShowingForNewActivity;
			salesRelationControl.FormShownForNewActivity += salesRelationControl_FormShownForNewActivity;

			enquiry.OrgPkInfo.ValueChanged += OrgPkInfo_ValueChanged;
			enquiry.O1_OA_LinkedAddressInfo.ValueChanged += O1_OA_LinkedAddressInfo_ValueChanged;
		}

		protected SalesEnquiry Enquiry;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				UpdateOpportunityButtonsVisibility();
				SetOverallDispositionLabelColor();
				UpdateCustomFieldsControlReadOnly();
				ISupportWebAddressValidationControlInitialise();
			}

			//so the Enquiry.ReadOnly is not set automatically and sequentially.
			//we have to handle the 'ParentForm.Shown' event to read Enquiry.ReadOnly correctly.
			if (ParentForm != null)
			{
				ParentForm.Shown += (sender, evt) =>
				{
					if (Enquiry.ReadOnly)
					{
						CloseEnquiryButton.Enabled = CloseReasonDropEdit.Visible && !CloseReasonDropEdit.ReadOnly;
						LinkOrgButton.Enabled = CreateSalesOpportunityButton.Enabled = false;
					}
				};
			}
		}

		void OrgPk_ValueChanged(object sender, EventArgs e)
		{
			UpdateControlVisibility();
		}

		void UpdateControlVisibility()
		{
			bool hasOrg = Enquiry.Header != null;

			ContactDropEdit.Visible = hasOrg;
			ContactNameTextBox.Visible = !hasOrg;

			CompanyAddressCodePanel.Visible = hasOrg;
		}

		void O1_LeadStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateButtonStates();
			UpdateOpportunityButtonsVisibility();
		}

		void SetCloseReasonDropEditVisibility(bool isClosed)
		{
			if (CloseReasonDropEdit.Visible != isClosed)
			{
				CloseReasonDropEdit.Visible = isClosed;
				var labelWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
				var margin = ControlDpiScalingHelper.ScaleToCurrentDpiX(16);
				using (var graphics = CreateGraphics())
				{
					var textLength = (int)graphics.MeasureString(CloseReasonDropEdit.CaptionResourceString.Caption, CloseReasonDropEdit.Font).Width + margin;
					labelWidth = Math.Min(textLength, labelWidth);
				}

				var totalWidth = CloseReasonDropEdit.Width + labelWidth;
				var leftValue = isClosed ?
					StatusButtonsGroupBox.Left - totalWidth :
					StatusButtonsGroupBox.Left + totalWidth;
				ControlDpiScalingHelper.SetLeft(StatusButtonsGroupBox, leftValue, false);
			}
		}

		void SetOverallDispositionLabelColor()
		{
			if (!Enquiry.IsNotOpen)
			{
				StatusDescriptionLabel.BackColor = System.Drawing.Color.LimeGreen;
			}
			else
			{
				StatusDescriptionLabel.BackColor = System.Drawing.Color.Red;
			}
		}

		void enquiry_HasChangesChanged(object sender, EventArgs e)
		{
			if (Enquiry != null && !Enquiry.IsDeleted)
			{
				UpdateButtonStates();
				SetOverallDispositionLabelColor();
				UpdateCustomFieldsControlReadOnly();
			}
		}

		void UpdateButtonStates()
		{
			bool inDb = Enquiry.IsInDatabase;
			AdditionalEnquiryButtonsGroupBox.Visible = StatusButtonsGroupBox.Visible = inDb;
			if (inDb)
			{
				if (!Enquiry.IsNotOpen)
				{
					CloseEnquiryButton.Enabled = Enquiry.CanDoClose;
					CloseEnquiryButton.Text = Res.GetString("5B940093-BADC-4706-B2B0-95131366A81F", "Mark as Closed");
				}
				else if (!Enquiry.IsConvertedToOpportunity)
				{
					CloseEnquiryButton.Text = Res.GetString("71C38E08-4CE0-41F8-8913-FAFAEE253319", "Re-open");
				}

				CreateSalesOpportunityButton.Enabled = Enquiry.CanConvertToOpportunity;

				LinkOrgButton.Enabled = Enquiry.CanDoClose;
				OrgSearchButton.Enabled = Enquiry.CanDoClose;
				ViewSalesOpportunityButton.Enabled = Enquiry.Opportunity != null;
				SetCloseReasonDropEditVisibility(Enquiry.O1_LeadStatus == SalesEnquiryStatusCodeList.Codes.Closed);
			}
		}

		void UpdateOpportunityButtonsVisibility()
		{
			if (Enquiry.IsConvertedToOpportunity)
			{
				CreateSalesOpportunityButton.Visible = false;
				ViewSalesOpportunityButton.Visible = true;
			}
			else
			{
				CreateSalesOpportunityButton.Visible = true;
				ViewSalesOpportunityButton.Visible = false;
			}
		}

		void UpdateCustomFieldsControlReadOnly()
		{
			EnquiryCustomFieldsControl.SetReadOnly(Enquiry.ReadOnly);
		}

		void CloseEnquiryButton_Click(object sender, EventArgs e)
		{
			if (!Enquiry.IsNotOpen)
			{
				CloseEnquiry();
			}
			else if (!Enquiry.IsConvertedToOpportunity)
			{
				ReopenEnquiry();
			}
		}

		void CloseEnquiry()
		{
			if (Env.Security.InquiryManagerClose.IsAllowed)
			{
				Enquiry.DoClose();
			}
			else
			{
				Env.Security.InquiryManagerClose.ShowError();
			}
		}

		void ReopenEnquiry()
		{
			if (Env.Security.InquiryManagerReopen.IsAllowed)
			{
				Enquiry.Reopen();
			}
			else
			{
				Env.Security.InquiryManagerReopen.ShowError();
			}
		}

		#region Contact Controls

		void SetupContactControls()
		{
			PhoneNumberControl.SetDialling(ContactNumberDialler_Dialling);
			PhoneNumberControl.SetDiallingRelatedCommunicationContactDeciding(ContactNumberDialler_DiallingRelatedCommunicationContactDeciding);
			MobilePhoneNumberControl.SetDialling(ContactNumberDialler_Dialling);
			MobilePhoneNumberControl.SetDiallingRelatedCommunicationContactDeciding(ContactNumberDialler_DiallingRelatedCommunicationContactDeciding);
		}

		void ContactNumberDialler_Dialling(object sender, PhoneDiallerUserControl.DiallingEventArgs e)
		{
			e.CreateRelatedCommunication = OrganisationsDataRegistry.Instance.CreateCommunicationOnInquiryContactCall.Value;
		}

		void ContactNumberDialler_DiallingRelatedCommunicationContactDeciding(object sender, PhoneDiallerUserControl.DiallingRelatedCommunicationContactDecidingArgs e)
		{
			e.ContactPk = Enquiry != null ? Enquiry.O1_OC_LinkedContact : ZGuid.Empty;
		}

		#endregion

		#region Company Name Text Box Event Handlers

		protected void CompanyNameTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.F3 || e.KeyCode == Keys.F4 || e.KeyCode == Keys.Oemplus)
				&& Enquiry.HasContactErrors)
			{
				ShowCannotLinkToOrgError();
				return;
			}

			if (e.KeyCode == Keys.F3)
			{
				var f3Controller = e.Modifiers == Keys.Alt ? ControllerIDs.Organisation : ControllerIDs.ClientIntelligence;
				HandleF3KeyDown(f3Controller);
			}
			else if (e.KeyCode == Keys.F4)
			{
				HandleF4KeyDown();
			}
			else if (e.KeyCode == Keys.Oemplus) // Key "="
			{
				HandleEqualKeyDown();
			}
		}

		void HandleF3KeyDown(ControllerID f3Controller)
		{
			ZString orgName = CompanyNameTextBox.Text.Trim();

			ZQuery query = new ZQuery(OrgHeaderSchema.OH_FullName, orgName);
			int matchedOrgCount = Enquiry.Factory.GetDatabaseCount(typeof(OrgHeader), query);

			ZController controller = ZControllerFactory.Create(f3Controller);
			controller.SetFormsModalTo(FindForm());

			if (Enquiry.Header != null)
			{
				controller.ShowEditForm(Enquiry.Header);
			}
			else if (orgName.IsEmpty)
			{
				if (!ShowErrorForMissingSecurity())
				{
					OrgHeader org = Enquiry.CreateOrg(new BusinessObjectFactory());
					org.OrgSaved += (s, e) => { Enquiry.OrgPk = org.PK; };
					controller.ShowFormForNewEntity(org);
				}
			}
			else if (matchedOrgCount == 0)
			{
				if (!ShowErrorForMissingSecurity() && Globals.Message.Show(Res.GetString("092c14aa-d40a-4b14-a401-15b009668f4e", "The organization \"{0}\" does not exist. Would you like to create a new organization?", orgName), Res.GetString("48a34157-d40c-4136-bdb2-23e851be5c71", "Create Organization"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					OrgHeader org = Enquiry.CreateOrg(new BusinessObjectFactory());
					org.OH_FullName = !Env.Registry.OrgAllowMixedCase ? orgName.ToUpper() : orgName;
					org.OrgSaved += (s, e) => { Enquiry.OrgPk = org.PK; };
					controller.ShowFormForNewEntity(org);
				}
			}
			else if (matchedOrgCount == 1)
			{
				OrgHeader org = Enquiry.Factory.Load<OrgHeader>(query)[0];
				Enquiry.OrgPk = org.PK;
				controller.ShowEditForm(org);
			}
			else
			{
				SelectOrg(SelectOrgPopupOption.SingleOrgNameFilter);
			}
		}

		void HandleF4KeyDown()
		{
			ZString orgName = CompanyNameTextBox.Text.Trim();
			if (orgName.IsEmpty)
			{
				SelectOrg();
			}
			else
			{
				SelectOrg(SelectOrgPopupOption.DefaultFiltersWithOrgNameFilter);
			}
		}

		void HandleEqualKeyDown()
		{
			ZString orgName = CompanyNameTextBox.Text.Trim();
			FindBoxListProvider listProvider = new FindBoxListProvider(new OrgHeaderCollection(new BusinessObjectFactory()));
			ZString autocompleteName = listProvider.NearestDescriptionMatch(orgName, true);
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_FullName, autocompleteName);
			var matchedOrgs = Enquiry.Factory.Load<OrgHeader>(query);
			if (matchedOrgs.Length > 0)
			{
				Enquiry.OrgPk = matchedOrgs[0].PK;
				Enquiry.RefreshBinding();
			}
			else
			{
				Enquiry.OrgPk = ZGuid.Empty;
				CompanyNameTextBox.Text = autocompleteName;
			}

			CompanyNameTextBox.Focus();
			if (autocompleteName.Length > orgName.Length)
			{
				CompanyNameTextBox.Select(orgName.Length, autocompleteName.Length - orgName.Length);
			}
		}

		void CompanyNameTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '=')
			{
				e.Handled = true;
			}
		}

		void ShowCannotLinkToOrgError()
		{
			ZFormModaliser.ShowDialogAndDispose(new ZErrorMessageBox(Enquiry, Enquiry.HumanReadableName,
							Res.GetString("7c99c85e-0568-4678-a0d2-87081928a058", "link"),
							Res.GetString("8cf65bae-8c15-49db-94e9-d1cc045ca8c0", "linked to an organization")));
		}

		#endregion

		#region Select Org

		void OrgSearchButton_Click(object sender, EventArgs e)
		{
			if (Enquiry.HasContactErrors)
			{
				ShowCannotLinkToOrgError();
				return;
			}
			SelectOrg(SelectOrgPopupOption.DefaultFiltersWithOrgNameFilter);
		}

		void orgPopup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects.Length >= 1)
			{
				OrgHeader org = (OrgHeader)e.SelectedBusinessObjects[0];
				Enquiry.OrgPk = org.PK;
				Enquiry.RefreshBinding();
			}
		}

		void SelectOrg(SelectOrgPopupOption option = SelectOrgPopupOption.DefaultFilters)
		{
			ZFilterGridModule orgModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation);

			ZString orgName = CompanyNameTextBox.Text.Trim();
			if (option == SelectOrgPopupOption.SingleOrgNameFilter)
			{
				orgModule.FilterBusinessObject.SetInitialCodeForSearch(orgName, OrgHeaderSchema.Constants.OH_FullName);
			}
			else if (option == SelectOrgPopupOption.DefaultFiltersWithOrgNameFilter)
			{
				var defaults = new FilterBusinessObjectDefaults();
				var orgNameFilterDefault = new FilterBusinessObjectDefault("Name", "Property", orgName);
				defaults.Add(orgNameFilterDefault);
				orgModule.FilterBusinessObject.SetExternalDefaults(defaults);
			}

			var orgPopup = new EmbeddedModulePopup(orgModule);
			var findBox = new ZSimpleFindBox(new SalesEnquiryLookups(Enquiry).OrganisationsList, orgPopup);
			var provider = new PopupModuleDecisionProvider(findBox);
			orgModule.OverrideModuleDecisionProvider(provider);
			orgPopup.EmbeddedModulePopupOKButtonStrategy = provider;
			orgPopup.Selected += new EmbeddedModulePopup.SelectedEventHandler(orgPopup_Selected);
			ZFormModaliser.Show(orgPopup, ParentForm);

			if (option == SelectOrgPopupOption.SingleOrgNameFilter)
			{
				var filterControl = orgModule.EmbeddedControl as ZFilterStripCommonControl;
				if (filterControl != null)
				{
					filterControl.FirePerformSearch();
				}
			}
		}

		enum SelectOrgPopupOption
		{
			DefaultFilters,
			DefaultFiltersWithOrgNameFilter,
			SingleOrgNameFilter,
		}

		#endregion

		#region Link to Another Org

		void LinkOrgButton_Click(object sender, EventArgs e)
		{
			// Only if the enquiry is not linked to any org, otherwise will open an org module popup
			if (Enquiry.Header != null)
			{
				SelectOrg();
			}
			else if (EnsureSaved())
			{
				ZForm findOrgForm = new SalesEnquiryFindOrgForm(Enquiry, CreateEnquiryOrgFinder(), false);
				ZFormModaliser.ShowDialogAndDispose(findOrgForm);
			}
		}

		EnquiryOrgFinder CreateEnquiryOrgFinder()
		{
			return Enquiry.CreateEnquiryOrgFinder();
		}

		#endregion

		#region Convert to Opportunity

		void CreateSalesOpportunityButton_Click(object sender, EventArgs e)
		{
			SalesRelationsTabPage.Show();
			salesRelationControl.InvokeNewActivityMenuItem(RelatableActivityTypeList.Codes.OpportunityManager);
		}

		void salesRelationControl_FormShowingForNewActivity(object sender, SalesRelationControl.FormShowingForActivityArgs e)
		{
			if (e.Activity.ActivityType == RelatableActivityTypeList.Codes.Quotations || e.Activity.ActivityType == RelatableActivityTypeList.Codes.OneOffQuotes)
			{
				PromptUserToSetClientIntelligenceForNewActivity(e.Activity);
			}
		}

		void salesRelationControl_FormShownForNewActivity(object sender, SalesRelationControl.FormShownForActivityArgs e)
		{
			var opportunity = e.Form.BusinessEntity as OrgOpportunity;
			if (opportunity != null && !Enquiry.IsConvertedToOpportunity)
			{
				e.Form.FormClosed += OpportunityFormClosed;
			}
		}

		bool EnsureSaved()
		{
			bool result = true;
			if (Enquiry.HasChanges)
			{
				result = false;
				var parentSalesEnquiryForm = ParentForm as SalesEnquiryForm;
				if (parentSalesEnquiryForm != null)
				{
					result = parentSalesEnquiryForm.PromptUserForSave() == DialogResult.OK;
				}
			}

			return result;
		}

		protected virtual void OpportunityFormClosed(object sender, EventArgs e)
		{
			ZForm form = (ZForm)sender;
			form.FormClosed -= OpportunityFormClosed;
			var opportunity = (OrgOpportunity)form.BusinessEntity;
			if (opportunity != null && Enquiry != null)
			{
				if (Enquiry.IsConvertedToOpportunity && opportunity.IsInDatabase)
				{
					DialogResult result = Globals.Message.Show(
						Res.GetString("6C51472F-1146-4837-A4D8-8B5DFA71A5C4", "Inquiry has been converted to an opportunity"),
						Res.GetString("92CFF14B-EAF8-4684-A52A-DA01572CE355", "Create Opportunity Complete"),
						MessageBoxButtons.OK, MessageBoxIcon.Information);

					if (ParentForm != null)
					{
						ParentForm.Close();
					}
				}
			}
		}

		#endregion

		#region View Opportunity

		void ViewSalesOpportunityButton_Click(object sender, EventArgs e)
		{
			OrgOpportunity opp = Enquiry.Opportunity;
			if (opp != null)
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.Opportunity);
				var editSecurity = Env.Security.OpportunityManagementEdit;
				var viewSecurity = Env.Security.OpportunityManagementView;

				if (editSecurity.IsAllowed)
				{
					controller.ShowEditForm(opp);
				}
				else if (viewSecurity.IsAllowed)
				{
					controller.ShowViewForm(opp);
				}
				else
				{
					viewSecurity.ShowError();
				}
			}
		}

		#endregion

		#region RelatableActivity

		void PromptUserToSetClientIntelligenceForNewActivity(IRelatableActivity activity)
		{
			if (activity.Client == null && Enquiry.Header == null)
			{
				var dialogResult = Globals.Message.Show(
					ResString.GetMultilingualString("55f89001-0a3d-4a20-a783-3ffd5de9b80b", @"A Client could not be set on the related {0} as one does not exist on this inquiry.
Would you like to set the Client Intelligence before continuing with the {0}?", activity.HumanReadableName),
					ResString.GetMultilingualString("24077573-40e0-4a0a-98dd-14ee0740654a", "New {0}", activity.HumanReadableName),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (dialogResult == DialogResult.Yes)
				{
					if (EnsureSaved())
					{
						ZForm findOrgForm = new SalesEnquiryFindOrgForm(Enquiry, CreateEnquiryOrgFinder(), true);
						ZFormModaliser.ShowDialogAndDispose(findOrgForm);
					}
				}
			}
		}

		#endregion

		#region WebAddressValidation

		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public ISupportWebAddressValidation Address
		{
			get
			{
				return Enquiry;
			}
		}

		public ZTextBox Address1Control
		{
			get
			{
				return CompanyAddress1TextBox;
			}
		}

		public ZTextBox Address2Control
		{
			get
			{
				return CompanyAddress2TextBox;
			}
		}

		public ZTextBox CityControl
		{
			get
			{
				return CompanyCityTextBox;
			}
		}

		public ZTextBox PostcodeControl
		{
			get
			{
				return CompanyPostCodeTextBox;
			}
		}

		public ZDropEdit StateControl
		{
			get
			{
				return CompanyStateDropEdit;
			}
		}

		public ZCodeFindBox CountryControl
		{
			get
			{
				return CompanyPortCountryCodeFindBox;
			}
		}

		ZButton ISupportWebAddressValidationControl.ValidateButton
		{
			get
			{
				return ValidateAddressButton;
			}
		}

		public Control SuggestionWindowParentControl
		{
			get
			{
				return this;
			}
		}

		void RefreshValidationStatus()
		{
			var validationStatus = Enquiry.LinkedAddress != null ? Enquiry.LinkedAddress.ValidationStatus : Address.ValidationStatus;
			if (Address != null)
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, validationStatus, Address.IsErrorSuppressed);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl, validationStatus);
				CompanyGroupPanel3.Invalidate();
			}
		}

		async void ISupportWebAddressValidationControlInitialise()
		{
			if (Env.Instance.Registry.EnableAddressValidationWebService && Address != null)
			{
				Enquiry.PreValidationForAddressValidationService();
				var formDisplayMode = (FindForm() as ZForm)?.DisplayMode;

				if (formDisplayMode != null && formDisplayMode != ODisplayMode.ReadOnly && formDisplayMode != ODisplayMode.Delete && !string.IsNullOrEmpty(Address.Address1) && Address.ValidationStatus != AddressValidationStatus.ManuallyVerified && !Address.Address1Info.ReadOnly)
				{
					using (Enquiry.SuspendSettingHasChanges())
					{
						await ValidateAddress();
					}
				}

				RefreshValidationStatus();
				Address.AddressValidationStatusChanged += Address_AddressValidationStatusChanged;
				AddressSuggestionControlHelper.RegisterPropertyChangedEvent(Address, ValidateAddress);
				CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(Address, GetCityTownAsync);

				if (formDisplayMode != null && formDisplayMode == ODisplayMode.Delete)
				{
					ValidateAddressButton.ReadOnly = ClearFieldsButton.ReadOnly = true;
				}
				else
				{
					ValidateAddressButton.ReadOnly = ClearFieldsButton.ReadOnly = Address.Address1Info.ReadOnly;
				}
			}
		}

		void Address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			if (Address != null)
			{
				RefreshValidationStatus();
				if (AddressValidationService.IsAddressInValidStatus(Address))
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
				}
			}
		}

		protected virtual async Task ValidateAddress()
		{
			if (AddressValidationService.IsAddressNeedValidation(Address))
			{
				CleanseAction cleanseAction;

				if (!ValidationJustForced)
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(SuggestionWindowParentControl.Controls);
					cleanseAction = CleanseAction.QuickValidate;
				}
				else
				{
					cleanseAction = CleanseAction.ValidateAndSuggest;
					ValidationJustForced = false;
				}

				await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, Address, this, this.ParentForm, SuggestionWindowParentControl.Controls, RefreshValidationStatus, null, 0, cleanseAction);
			}
		}

		async Task GetCityTownAsync()
		{
			if ((!string.IsNullOrEmpty(Address.City) || !string.IsNullOrEmpty(Address.Postcode)))
			{
				var maxWidth = StateControl.Width;
				await CityTownSuggestionControlHelper.GetCityTownAsync(cancellationToken, Address, this, ParentForm, SuggestionWindowParentControl.Controls, async () => { ValidationJustForced = true; await ValidateAddress(); ValidationJustForced = false; }, maxWidth);
			}
		}

		void HookISupportWebAddressValidationControlChangeFocusEvents()
		{
			SupportWebAddressValidationControlHelper.HookISupportWebAddressValidationControlChangeFocusEvents(this, ISupportWebAddressValidationControl_GotFocus, ISupportWebAddressValidationControl_LostFocus);
		}

		void ISupportWebAddressValidationControl_GotFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(sender, Address, this);
		}

		void ISupportWebAddressValidationControl_LostFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.HideSuggestionControlsUponLostFocus(this);
		}

		async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			if (Enquiry != null)
			{
				if (Enquiry.OrgFieldsReadOnly)
				{
					var controller = ZControllerFactory.Create(ControllerIDs.Organisation);
					var form = ((IOrganisationController)controller).ShowForm(Enquiry.Header, OrganisationTabPages.Address, FormAction.Edit);
					var orgForm = form as ZOrganisationsForm;
					orgForm.AddressesPageControl2.OrgAddressBoundGrid.SelectSingleElementByPK(Enquiry.O1_OA_LinkedAddress);
					form.Closed += form_Closed;
				}
				else
				{
					ValidationJustForced = true;
					await ValidateAddress();
				}
			}
		}

		void form_Closed(object sender, EventArgs e)
		{
			RefreshValidationStatus();
		}

		void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ClearFields(Address, this);
		}

		void O1_OA_LinkedAddressInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshValidationStatus();
		}

		void OrgPkInfo_ValueChanged(object sender, EventArgs e)
		{
			ClearFieldsButton.ReadOnly = Enquiry.OrgFieldsReadOnly;
		}

		public bool ValidationJustForced { get; set; }

		public ISupportWebAddressValidation AddressForValidation
		{
			get { return Enquiry; }
		}

		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		CancellationTokenSource cancellationToken;

		#endregion

		#region ProcessCmdKey
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ProcessManuallyVerifyShortCutKey(ref msg, keyData))
			{
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		public bool ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData)
		{
			if (SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, this.GetReadOnly(), this, Address))
			{
				return true;
			}
			return false;
		}
		#endregion

		#region Character Case on Org Fields

		void SetCharacterCasing()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var requiredCasing = Env.Registry.OrgAllowMixedCase ? CharacterCasing.Normal : CharacterCasing.Upper;

				ContactNameTextBox.CharacterCasing = requiredCasing;
				ContactDropEdit.CharacterCasing = requiredCasing;
				CompanyNameTextBox.CharacterCasing = requiredCasing;
				ReferringContactDropEdit.CharacterCasing = requiredCasing;
				ReferToContactDropEdit.CharacterCasing = requiredCasing;
			}
		}

		#endregion

		#region Security

		bool ShowErrorForMissingSecurity()
		{
			if (!Env.Security.OrgContactNew.IsAllowed)
			{
				Env.Security.OrgContactNew.ShowError();
				return true;
			}
			else if (!Env.Security.ClientIntelligenceModify.IsAllowed)
			{
				Env.Security.ClientIntelligenceModify.ShowError();
				return true;
			}
			else
			{
				return false;
			}
		}

		#endregion
	}
}
