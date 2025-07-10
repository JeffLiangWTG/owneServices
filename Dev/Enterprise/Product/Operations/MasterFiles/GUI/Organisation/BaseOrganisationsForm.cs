using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class BaseOrganisationsForm : ZForm, ISupportViewDpsLogsTab
	{
		public BaseOrganisationsForm()
		{
		}

		public BaseOrganisationsForm(OrgHeader organisation)
			: base(organisation)
		{
			organisation.RegenerateCodeWhenOrgTypesChange = true;
			this.Organisation = organisation;
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			SetupEventHandlers();

			AddScreeningLogsTabPage();
			new DeniedPartyScreeningPresentationManager().CreateMenusForScreeningEntity(this);
			ObjectFactory.Get<IMasterDataProviderGUI>().CreateOrgMenu(this, organisation);
			recalculatePatternsInitializer = new RecalculatePatternsInitializer(organisation);
			recalculatePatternsInitializer.CreateRecalculateMenuItem(ActionsMenuItem, (o, e) => { organisation.RegeneratePatternTables(); });
			ControlDpiScalingHelper.SetWidth(ref AddedInfoPanelSplitContainer, ClientSize.Width, false);
			ControlDpiScalingHelper.SetHeight(ref AddedInfoPanelSplitContainer, ButtonsUserControl.Top, false);
			new CopyAddressForAnalysisManager().CreateOrgMenu(this, organisation);
			((IBusinessObjectInternals)organisation).Validate(organisation.OH_ScreeningStatusInfo); // Trigger the validation before form show
			InitialiseAddedInfoPanel();
			ContactsTabPage.SetupSecurity(Env.Security.OrgContactView);
		}

		void ContactsControlOnAfterFirstBinding(object sender, EventArgs args)
		{
			ContactsControl.InitialContactToSelect = InitialContactToSelect;
			if (recalculatePatternsInitializer != null)
			{
				ContactsControl.AddRecalculatePatternTablesMenuItem(recalculatePatternsInitializer);
			}
		}

		public OrgAddress OrgAddressForAnalysis
		{
			get
			{
				return OrganisationsTabControl.SelectedTab == AddressesTabPage ? (OrgAddress)AddressesPageControl2?.OrgAddressBoundGrid.GetCurrent() : Organisation?.MainAddress;
			}
		}

		public OrgContact InitialContactToSelect
		{
			get;
			set;
		}

		#region Denied Party Screening

		string DpsLogsTabName => Res.GetString("01140254-885E-495E-A778-97402C79416D", "Denied Party Screening Logs");

		void AddScreeningLogsTabPage()
		{
			var screeningLogControl = new StmEntityScreeningLogControl();
			screeningLogControl.SetBindingMember("ScreeningLogCollection");
			zLogsTabPage1.AddAdditionalTab(DpsLogsTabName, screeningLogControl);
		}

		public void SwitchToDpsLogsTab()
		{
			if (zLogsTabPage1 != null)
			{
				SetInitialTabPage(zLogsTabPage1);
				OrganisationsTabControl.SelectedTab = zLogsTabPage1;
				zLogsTabPage1.SetSelectedTab(DpsLogsTabName);
			}
		}

		#endregion

		#region Implementation

		readonly RecalculatePatternsInitializer recalculatePatternsInitializer;

		public override string FormCaption
		{
			get
			{
				string result = "";
				if (!string.IsNullOrEmpty(Organisation?.OH_Code))
				{
					result = string.Format(" - {0} / {1}", Organisation.OH_Code, Organisation.OH_FullName);
				}
				return result;
			}
		}

		protected internal string OrganisationTypeNotSelectedMessage
		{
			get
			{
				return !this.IsDesignMode() ? Res.GetString("e0c07442-3445-4eb2-908a-cc4451640378", @"You must select an Organization Type from the right-side of the Organization Details Screen. 
EG: Consignee/Consignor, Receivables/Payables etc....") : "DESIGN_MODE_STRING_ONLY";
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = ContinueWithSave.Yes;
			var callBase = true;

			if (!Organisation.OrganisationTypeIsSelected && !IsProductivityWiseModeEnabled)
			{
				Globals.Message.ShowError(OrganisationTypeNotSelectedMessage, Res.GetString("3b4f70ca-2c90-4b79-9164-362b00d05c59", "No Organization Type Selected"));
				return ContinueWithSave.No;
			}

			if (!OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value && Organisation.IsLikelyDuplicate(false))
			{
				callBase = false;
				result = ShowDuplicationForm();
			}

			if (result == ContinueWithSave.Yes)
			{
				if (Organisation.RequiresSecurityOverrideToUpdateCountry)
				{
					callBase = false;
					result = ShowSecurityLoginForm();
				}

				if (Organisation.ShouldAskToCreateEnglishMainAddress)
				{
					ShowCreateEnglishMainAddressForm();
				}
			}

			if (callBase)
			{
				return base.ShowPreSaveDialogs();
			}
			else
			{
				return result;
			}
		}

		protected bool IsProductivityWiseModeEnabled
		{
			get
			{
				if (!isProductivityWiseModeEnabled.HasValue)
				{
					isProductivityWiseModeEnabled = DataRegistry.Instance.ProductivityWiseModeEnabled;
				}

				return isProductivityWiseModeEnabled.Value;
			}
		}
		bool? isProductivityWiseModeEnabled;

		protected virtual ContinueWithSave ShowDuplicationForm()
		{
			ContinueWithSave result = ContinueWithSave.No;

			using (DuplicateOrgForm duplicateForm = new DuplicateOrgForm(Organisation))
			{
				ZFormModaliser.ShowDialogWithoutDispose(duplicateForm);
				result = duplicateForm.UserDecision;
			}

			return result;
		}

#if DEBUG
		protected virtual
#endif
 ContinueWithSave ShowSecurityLoginForm()
		{
			var result = ContinueWithSave.Yes;
			var args = GetSecurityLoginEventArgs();

			var loginProvider = (ISecurityLoginProvider)Activator.CreateInstance(ObjectFactory.GetType("ISecurityLoginProvider"), Res.GetString("7b224547-30b6-4d91-ae4f-d8ae6675aa6c", "Security restriction"));
			loginProvider.ShowDocumentLoginForDocuments(args);

			if (!args.IsAllowedToProceed)
			{
				result = ContinueWithSave.No;
				Globals.Message.ShowError(args.Message);
			}
			else
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Organisation.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, string.Format("Organization country security override granted by user {0}", args.AuthorisingStaffLogin));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			return result;
		}

		protected internal SecurityLoginEventArgs GetSecurityLoginEventArgs()
		{
			var errorMessage = ResString.GetMultilingualString("22cc9add-7f6c-45b1-9d66-620f6870bdc9", "You do not have sufficient security rights to create an organization for a country/region outside your current login country/region.");
			var loginMessage = MultilingualString.Join(string.Empty, errorMessage, (NoResString)System.Environment.NewLine, ResString.GetMultilingualString("26317dc9-305d-4f77-8e78-17fcf8aefb00", "Your local administrator or user(s) within your company that have the appropriate rights can override this setting by entering their login credentials.\r\nDo you wish to continue?"), (NoResString)System.Environment.NewLine);
			var args = new SecurityLoginEventArgs(loginMessage, errorMessage, security => security.OrgDetailsNewAllowCreationOutsideLoginCountry)
			{
				HideApprovalRequestButton = true
			};

			return args;
		}

		void SetupEventHandlers()
		{
			OrganisationsTabControl.SelectedIndexChanged += new EventHandler(ChangeTabPage);
			DetailsTabPage.RunWhenBindingOrFirstShown(delegate
			{
				DetailsControl.DetailsTabControl.SelectedIndexChanged += new EventHandler(ChangeTabPage);
			});
			Organisation.CompanyData.CannotModifyAROrAPFlag += new OrgCompanyData.CannotModifyAROrAPFlagEventHandler(CannotModifyAROrAPFlag);
			Organisation.CompanyData.SecurityAccessDenied += new OrgCompanyData.CannotModifyAROrAPFlagEventHandler(SecurityAccessDenied);
			Organisation.Addresses.OnTriedToRemoveMainAddress += new EventHandler(Addresses_OnTriedToRemoveMainAddress);
			Organisation.OnCheckIfNeedToEmptyRelatedCollections += new EventHandler(Organisation_OnCheckIfNeedToEmptyRelatedCollections);

			Organisation.OH_IsDebtorInfo.ValueChanged += new EventHandler(OrganisationTypeChanged);
			Organisation.OH_IsConsignorInfo.ValueChanged += new EventHandler(OrganisationTypeChanged);
			Organisation.OH_IsConsigneeInfo.ValueChanged += new EventHandler(OrganisationTypeChanged);
			Organisation.OH_IsSalesLeadInfo.ValueChanged += new EventHandler(OrganisationTypeChanged);
			Organisation.OH_IsShippingProviderInfo.ValueChanged += new EventHandler(OrganisationTypeChanged);
		}

		protected virtual void UnsubscribeHandlers()
		{
			if (OrganisationsTabControl != null)
			{
				OrganisationsTabControl.SelectedIndexChanged -= ChangeTabPage;
			}
			if (DetailsControl != null)
			{
				DetailsControl.DetailsTabControl.SelectedIndexChanged -= ChangeTabPage;
			}
			if (Organisation != null)
			{
				Organisation.CompanyData.CannotModifyAROrAPFlag -= CannotModifyAROrAPFlag;
				Organisation.CompanyData.SecurityAccessDenied -= SecurityAccessDenied;
				Organisation.Addresses.OnTriedToRemoveMainAddress -= Addresses_OnTriedToRemoveMainAddress;
				Organisation.OnCheckIfNeedToEmptyRelatedCollections -= Organisation_OnCheckIfNeedToEmptyRelatedCollections;

				Organisation.OH_IsDebtorInfo.ValueChanged -= OrganisationTypeChanged;
				Organisation.OH_IsConsignorInfo.ValueChanged -= OrganisationTypeChanged;
				Organisation.OH_IsConsigneeInfo.ValueChanged -= OrganisationTypeChanged;
				Organisation.OH_IsSalesLeadInfo.ValueChanged -= OrganisationTypeChanged;
				Organisation.OH_IsShippingProviderInfo.ValueChanged -= OrganisationTypeChanged;
			}
		}

		void Addresses_OnTriedToRemoveMainAddress(object sender, EventArgs e)
		{
			Globals.Message.ShowWarning(Res.GetString("742186b3-accd-4705-bc67-9c258f8d7914", "The Main Address for this Organization cannot be removed."), Res.GetString("2013ccfd-1068-4c8d-8d42-449222752bf6", "Cannot Remove Main Address"));
		}

		void SecurityAccessDenied(object sender, OrgCompanyData.CannotModifyAROrAPFlagEventArgs e)
		{
			string caption = Res.GetString("7a8ce9a6-6c6c-4043-9df1-7df90dbd1db1", "Cannot Change {0} Flag", e.AROrAPCode);
			string message = Res.GetString("da266620-1568-4a32-ab23-d5e6e139c833", "You don't have the security permissions to access to change the {0} flag. Please see your system administrator if you require access to modify this flag.", e.AROrAPCode);
			Globals.Message.ShowError(message, caption);
		}

		internal void CannotModifyAROrAPFlag(object sender, OrgCompanyData.CannotModifyAROrAPFlagEventArgs e)
		{
			var errorMessage = Res.GetString("b7230bde-65dc-42a2-a5aa-a7bfd3048569", "You cannot change the {0} flag because active {1} transactions still exist for this Organization", e.AROrAPCode, e.AROrAPCode);
			var errorCaption = Res.GetString("459edf35-1a29-4a32-bcdf-86fbb790a93d", "Cannot change {0} flag", e.AROrAPCode);
			if (Organisation.IsChangingByWorkflowTrigger)
			{
				throw new ZCannotSaveException(errorMessage, errorCaption);
			}
			else 
			{
				Globals.Message.ShowError(errorMessage, errorCaption);
			}
		}

		void Organisation_OnCheckIfNeedToEmptyRelatedCollections(object sender, EventArgs e)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("536e8a71-8965-422f-82af-3b943c664695", "Changing this organization type will remove the existing data for this organization type. Would you like to continue?"), Res.GetString("21fcb561-6740-4f03-a30a-ab30ea129cd8", "Existing Data"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			Organisation.EmptyRelatedCollections = result == DialogResult.OK;
		}

		void OrganisationTypeChanged(object sender, EventArgs e)
		{
			bool showControls = Organisation.OH_IsDebtor || Organisation.OH_IsConsignee || Organisation.OH_IsConsignor || Organisation.OH_IsSalesLead || Organisation.OH_IsShippingProvider;

			DetailsTabPage.RunWhenBindingOrFirstShown(delegate
			{
				if (DetailsControl.organisationRatingUserControl1 != null)
				{
					DetailsControl.organisationRatingUserControl1.Visible = showControls;
				}
				if (DetailsControl.AutoRatingAndCompanyTariffNotAvailableLabel != null)
				{
					DetailsControl.AutoRatingAndCompanyTariffNotAvailableLabel.Visible = !showControls;
				}
			});
		}

		void ShowCreateEnglishMainAddressForm()
		{
			var mainAddress = Organisation.MainAddress;
			var englishMainAddress = Organisation.CreateEnglishEquivalentAddress(mainAddress);
			var addressMapper = new AddressMapper(mainAddress, englishMainAddress);

			using (var createTranslatedAAddressForm = new CreateTranslatedAddressForm(addressMapper, ShouldValidateAddress()))
			{
				var result = ZFormModaliser.ShowDialogAndDispose(createTranslatedAAddressForm, this);
				if (result != DialogResult.OK)
				{
					englishMainAddress.ParentAddress.AddressLanguagePack.Remove(englishMainAddress);
					englishMainAddress.Delete();
				}
				else
				{
					SwitchAddressInfo(addressMapper, englishMainAddress, mainAddress);
				}
			}
		}

		internal void SwitchAddressInfo(AddressMapper addressMapper, OrgTranslatedAddress translatedAddress, OrgAddress mainAddress)
		{
			if (addressMapper.Address1.EntityPK != mainAddress.EntityPK)
			{
				var mainAddress1 = addressMapper.Address1.Address1;
				var mainAddress2 = addressMapper.Address1.Address2;
				var mainLanguage = addressMapper.Address1.Language;
				var mainCity = addressMapper.Address1.City;
				var mainPostcode = addressMapper.Address1.Postcode;
				var mainCountry = addressMapper.Address1.CountryCodeISO2;

				var translatedAddress1 = addressMapper.Address2.Address1;
				var translatedAddress2 = addressMapper.Address2.Address2;
				var translatedLanguage = addressMapper.Address2.Language;
				var translatedCity = addressMapper.Address2.City;
				var translatedPostcode = addressMapper.Address2.Postcode;
				var translatedCountry = addressMapper.Address2.CountryCodeISO2;

				translatedAddress.ParentAddress.AddressLanguagePack.Remove(translatedAddress);
				translatedAddress.Delete();

				Organisation.OH_Language = mainLanguage;
				mainAddress.Address1 = mainAddress1;
				mainAddress.Address2 = mainAddress2;
				mainAddress.Language = mainLanguage;
				mainAddress.City = mainCity;
				mainAddress.Postcode = mainPostcode;
				mainAddress.OA_RN_NKCountryCode = mainCountry;

				var newTranslatedAddress = mainAddress.TranslatedAddresses.AddNew();
				mainAddress.AddressLanguagePack.Add(newTranslatedAddress);
				newTranslatedAddress.OTA_Language = translatedLanguage;
				newTranslatedAddress.OTA_Address1 = translatedAddress1;
				newTranslatedAddress.OTA_Address2 = translatedAddress2;
				newTranslatedAddress.OTA_City = translatedCity;
				newTranslatedAddress.OTA_PostCode = translatedPostcode;
				newTranslatedAddress.CountryCodeISO2 = translatedCountry;
			}
		}

		bool ShouldValidateAddress()
		{
			var mainAddress = Organisation.MainAddress;
			return
				Organisation != null &&
				mainAddress != null &&
				mainAddress.Country != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(mainAddress.Country.PK.ToGuid(), mainAddress.ValidationSection);
		}

		#region Change Tab Page

		protected virtual void ChangeTabPage(object sender, EventArgs e)
		{
			TabControlsAlreadySet = false;
			bool setControls = true;
			bool showControls = true;
			ZLabel selectedLabel = null;

			TabPage selectedTab = ((TabControl)sender).SelectedTab;

			if (selectedTab != null)
			{
				switch (selectedTab.Name)
				{
					case "DetailsTabPage":
						showControls = true;
						break;

					case "AutoRatingAndCompanyTariffTabPage":
						showControls = Organisation.OH_IsDebtor || Organisation.OH_IsConsignee || Organisation.OH_IsConsignor || Organisation.OH_IsSalesLead || Organisation.OH_IsShippingProvider;
						DetailsTabPage.RunWhenBindingOrFirstShown(delegate
						{
							selectedLabel = DetailsControl.AutoRatingAndCompanyTariffNotAvailableLabel;
						});
						break;

					case "AddressesTabPage":
						showControls = true;
						break;

					case "ContactsTabPage":
						showControls = true;
						break;

					default:
						setControls = false;
						break;
				}

				if (setControls)
				{
					SetTabPageControls(selectedTab, selectedLabel, showControls);
				}
			}
		}

		protected void SetTabPageControls(TabPage page, ZLabel label, bool showControls)
		{
			foreach (Control ctrl in page.Controls)
			{
				ctrl.Visible = showControls;
			}

			if (label != null)
			{
				label.Visible = !showControls;
			}

			TabControlsAlreadySet = true;
		}

		protected ZBool TabControlsAlreadySet;

		#endregion

		public OrgHeader Organisation { get; }

		protected override void HandleSaveException(Exception ex)
		{
			if (ex is ZSaveConcurrencyException concurrencyException)
			{
				// When saving organization while it was deleted somewhere else, ZSaveConcurrencyException is thrown.
				// Don't try to resolve with ConcurrencyResolver.Resolve because:
				// 1. There is no point to show merge message and resolve the changes because the org. was deleted.
				// 2. With the resolving, organization will be deleted leading to changed events to occur
				//   and all kinds of reports about accessing properties on deleted business objects like OrgAddress, CompanyData,...
				var changes = concurrencyException.Factory.GetChanges();
				if (changes.GetChangedObjects().Any(x => !x.IsExistsInDatabase && x.SessionInstance.PK == Organisation.PK))
				{
					// same message in ZForm but it needs new key because of different assemblies
					Globals.Message.ShowError(Res.GetString("9d6cf40a-4382-405f-a101-e6fca332a1ad", "Because this {0} has been deleted while you were editing it, this form will be closed.", BusinessEntity.HumanReadableName));
					this.ForceClose();
					return;
				}

				ZExceptionReporting.HandleZSaveConcurrencyException(concurrencyException, FormNotificationHandler, true);
				return;
			}

			if (ex.InnerException?.InnerException is SqlException sqlEx)
			{
				var match = new DbErrorMatch(sqlEx);
				Organisation.Factory.ClearQueryCache();
				if (match.ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey && Organisation.CheckCodeIsDuplicated())
				{
					SaveInternal();
					return;
				}
			}

			base.HandleSaveException(ex);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (initialTabPage != null)
			{
				OrganisationsTabControl.SelectedTab = initialTabPage;
			}
		}

		public void SetInitialTabPage(ZTabPage tabPage)
		{
			initialTabPage = tabPage;
		}

		ZTabPage initialTabPage;

		#region Added Info Panel

		void InitialiseAddedInfoPanel()
		{
			AddedInfoPanelSplitContainer.Panel1Collapsed = true;
		}

		protected override ZTabControl TopLevelTabControl => OrganisationsTabControl;

		#endregion

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
			try
			{
				if (isDisposing)
				{
					UnsubscribeHandlers();

					if (components != null)
					{
						components.Dispose();
					}
				}
			}
			finally
			{
				base.Dispose(isDisposing);
			}
		}

		#endregion
	}
}
