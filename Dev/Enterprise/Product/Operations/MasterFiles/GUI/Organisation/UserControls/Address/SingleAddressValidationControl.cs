using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ISupportWebAddressValidation = Enterprise.MasterFiles.Business.ISupportWebAddressValidation;

namespace Enterprise.MasterFiles.GUI
{
	enum TitleType
	{
		VerifyAll = 1,
		ManuallyVerifyAll = 2,
		ManuallyVerifyAllInDataBase = 3
	}

	public partial class SingleAddressValidationControl : ZUserControl, IReadOnlyToggleControl, ISupportWebAddressValidationControl
	{
		public SingleAddressValidationControl(AdministrationPanelManager manager)
		{
			InitializeComponent();
			Manager = manager;
			if (!DesignModeFinder.IsDesigning)
			{
				InitializeControl();
			}
		}

		#region Initialize

		void InitializeControl()
		{
			ValidationJustForced = false;
			SetupAddressSuggestionControl();

			if (Env.Instance.Registry.EnableAddressValidationWebService)
			{
				CancellationToken = new CancellationTokenSource();
				HandleCreated += (o, e) =>
				{
					if (ParentForm != null)
					{
						ParentForm.FormClosed += (x, y) =>
						{
							CancellationToken?.Cancel();
						};
					}
				};
			}
			else
			{
				ValidateAddressButton.Visible = false;
			}

			SetUIStatus(true, false);
		}

		void SetupAddressSuggestionControl()
		{
			AddressSuggestionControl = new AddressSuggestionControl(true);
			AddressSuggestionControl.ShouldWarnOnManuallyVerify = false;
			AddressSuggestionControl.CaptionRenderingEnabled = true;
			AddressSuggestionControl.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(565);
			AddressSuggestionControl.Name = "SuggestionControl";
			AddressSuggestionControl.AddressSelected += AddressSuggestionControl_AddressSelected;
			MainPanel.Controls.Add(this.AddressSuggestionControl, 0, 1);
		}

		#endregion

		#region Event

		void RecordsNavigator_EnabledChanged(object sender, EventArgs e)
		{
			OpenRecordButton.Enabled = RecordsNavigator.Enabled;
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			SaveSingleRecord(CurrentDataItemView);
		}

		void OpenRecordButton_Click(object sender, EventArgs e)
		{
			OpenFormForSelectedRecord();
		}

		void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ClearFields(AddressForValidation, this);
		}

		void AddressSuggestionControl_AddressSelected(object sender, AddressSuggestionControl.AddressSelectedEventArgs e)
		{
			if (AddressForValidation != null && e != null)
			{
				try
				{
					AddressValidationSuspended = true;
					var originalAdditionalAddress = AddressForValidation.UnrestrictedAdditionalAddressInformation;
					AddressValidationService.SetSuggestedAddressToAddressForValidation(e.SelectedAddress, e.PassAsEntered, AddressForValidation);
					if (AddressValidationService.Check_UnrestrictedAdditionalAddressInformationExceedMaxLength(AddressForValidation))
					{
						Globals.Message.Show(AddressValidationService.Constants.ExceedLengthOfAdditionalAddress);
						AddressForValidation.UnrestrictedAdditionalAddressInformation = originalAdditionalAddress;
					}
				}
				finally
				{
					AddressValidationSuspended = false;
					SetUIStatus(false, false);
					RefreshSuggestionControl(SuggestionControlStatus.Disabled);

					if (e.PassAsEntered || (e.SelectedAddress != null && !e.PassAsEntered))
					{
						AcceptAddress(AddressForValidation);
					}
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			if (CurrentDataItemView != null && BindingCompleted)
			{
				ReBindAddressByType();
				var topLevelZForm = TopLevelControl as ZForm;
				if (topLevelZForm != null && topLevelZForm.DisplayMode != ODisplayMode.ReadOnly)
				{
					if (IsAutoVerifying)
					{
						SetUIStatus(false, true);
					}
					else if (IsAutoVerified)
					{
						SetUIStatus(false, false);
						RefreshSuggestionControl(SuggestionControlStatus.Disabled);
					}
					else
					{
						BeginValidateAddressAsync();
					}
				}

				AddressSuggestionControlHelper.RegisterPropertyChangedEvent(AddressForValidation, ValidateAddressAsync);
				CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(AddressForValidation, GetCityTownAsync);
			}
			else
			{
				SetUIStatus(true, false);
			}

			base.OnCurrentDataItemChanged(e);
		}

		internal void SetUIStatus(bool reset, bool isVerifying)
		{
			var caption = Res.GetString("22CFAD55-71B3-4964-B4E9-C20201EB61E1", "Address Details");

			if (reset)
			{
				IsVerifying = false;
				RefreshMenuItem(false, false);
				SaveButtonPanel.Enabled = false;
				AddressBoundPanel.Enabled = false;
				CityBoundTextBox.DataBindings.Clear();
				CityBoundTextBox.ResetText();
				AddressTypeTextBox.DataBindings.Clear();
				AddressTypeTextBox.ResetText();
				AddressCodeTextBox.DataBindings.Clear();
				AddressCodeTextBox.ResetText();
				Address1BoundTextBox.DataBindings.Clear();
				Address1BoundTextBox.ResetText();
				Address2BoundTextBox.DataBindings.Clear();
				Address2BoundTextBox.ResetText();
				PostCodeBoundTextBox.DataBindings.Clear();
				PostCodeBoundTextBox.ResetText();
				CountryFindBox.CodeBox.DataBindings.Clear();
				CountryFindBox.CodeBox.ResetText();
				OrganisationNameTextbox.DataBindings.Clear();
				OrganisationNameTextbox.ResetText();
				StateBoundDropEdit.CodeBox.DataBindings.Clear();
				StateBoundDropEdit.CodeBox.ResetText();
				OrganisationFindBox.CodeBox.DataBindings.Clear();
				OrganisationFindBox.CodeBox.ResetText();
				AddressSuggestionControl.ClearResultsList();
				StateBoundDropEdit.DescriptionBox.DataBindings.Clear();
				StateBoundDropEdit.DescriptionBox.ResetText();
				OrganisationFindBox.DescriptionBox.DataBindings.Clear();
				OrganisationFindBox.DescriptionBox.ResetText();
				AdditionalAddressInformationBoundTextBox.DataBindings.Clear();
				AdditionalAddressInformationBoundTextBox.ResetText();
				AddressDetailGroupBox.Text = caption;
				RefreshSuggestionControl(SuggestionControlStatus.Disabled);
				CityTownSuggestionControlHelper.CloseSuggestionForm(AddressDetailGroupBox.Controls);
				AddressValidationUIHelper.ResetAddressFieldState(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl);
			}
			else
			{
				var captionForVerifying = Res.GetString("3B2658A4-E175-4DF5-AFF5-B4912B8E6D8A", "Validating Address...");
				var captionForAutoVerifying = Res.GetString("A3CD1C24-3D8B-42A2-A70D-F67C19924C6D", "This address is being auto-verified, you cannot edit it until auto-verify process finished.");
				var captionForAutoVerified = Res.GetString("08349713-7784-4FAD-9F2C-A1972C0FB64E", "Address Details(This address has been auto-verified).");

				IsVerifying = !IsAutoVerifying && isVerifying;
				RefreshValidationStatus();
				ValidationJustForced = isVerifying;
				SaveButtonPanel.Enabled = !isVerifying;
				AddressBoundPanel.Enabled = !isVerifying;
				SaveButton.Enabled = CurrentAddressEntity.HasChanges;

				if (isVerifying)
				{
					RefreshSuggestionControl(SuggestionControlStatus.Disabled);
					CityTownSuggestionControlHelper.CloseSuggestionForm(AddressDetailGroupBox.Controls);
					AddressDetailGroupBox.Text = IsAutoVerifying ? captionForAutoVerifying : captionForVerifying;
				}
				else
				{
					AddressDetailGroupBox.Text = IsAutoVerified ? captionForAutoVerified : caption;
				}
			}
		}

		internal void PerformDataItemChange()
		{
			OnCurrentDataItemChanged(EventArgs.Empty);
		}

		void HookAddressInfoChangedEvent()
		{
			if (CurrentAddressEntity is OrgAddress orgAddress)
			{
				orgAddress.OA_RN_NKCountryCodeInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
				if (orgAddress.PrimaryOrgAddressAdditionalInfo != null)
				{
					orgAddress.OA_AdditionalAddressInformationInfo.ValueChanged += AdditionalAddressPropertyInfo_ValueChanged;
					orgAddress.PrimaryOrgAddressAdditionalInfo.OAI_AdditionalInfoInfo.ValueChanged += AdditionalAddressPropertyInfo_ValueChanged;
				}
				orgAddress.OA_CodeInfo.ValueChanged += AdditionalAddressPropertyInfo_ValueChanged;
				orgAddress.OA_Address1Info.ValueChanged += AddressPropertyInfo_ValueChanged;
				orgAddress.OA_Address2Info.ValueChanged += AddressPropertyInfo_ValueChanged;
				orgAddress.OA_PostCodeInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
				orgAddress.OA_StateInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
				orgAddress.OA_CityInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
			}
			else if (CurrentAddressEntity is JobDocAddress docAddress)
			{
				docAddress.E2_RN_NKCountryCodeInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
				docAddress.E2_Address1Info.ValueChanged += AddressPropertyInfo_ValueChanged;
				docAddress.E2_Address2Info.ValueChanged += AddressPropertyInfo_ValueChanged;
				docAddress.E2_PostcodeInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
				docAddress.E2_StateInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
				docAddress.E2_CityInfo.ValueChanged += AddressPropertyInfo_ValueChanged;
			}
		}

		void AdditionalAddressPropertyInfo_ValueChanged(object sender, EventArgs e)
		{
			SetHasAddressInfoChanges(CurrentDataItemView, true);
		}

		void AddressPropertyInfo_ValueChanged(object sender, EventArgs e)
		{
			if (AddressForValidation != null)
			{
				SetHasAddressInfoChanges(CurrentDataItemView, true);
				RefreshValidationStatus();
			}
		}

		async Task GetCityTownAsync()
		{
			if (!AddressValidationSuspended && IsOnCurrentSelectedTab() && (!string.IsNullOrEmpty(AddressForValidation.City) || !string.IsNullOrEmpty(AddressForValidation.Postcode)))
			{
				var maxWidth = Address1BoundTextBox.Width;
				await CityTownSuggestionControlHelper.GetCityTownAsync(CancellationToken, AddressForValidation, this, ParentForm, AddressDetailGroupBox.Controls, async () =>
				{
					ValidationJustForced = true;
					await ValidateAddressAsync();
					ValidationJustForced = false;
				}, maxWidth);
			}
		}

		#endregion

		#region Short CutKey

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ShouldValidate && ProcessManuallyVerifyShortCutKey(ref msg, keyData))
			{
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		public bool ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.M))
			{
				try
				{
					AddressValidationSuspended = true;
					if (SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, ReadOnly, this, AddressForValidation))
					{
						SetUIStatus(false, false);
						AcceptAddress(AddressForValidation);
						return true;
					}
				}
				finally
				{
					AddressValidationSuspended = false;
				}
			}

			return false;
		}

		#endregion

		#region Open Record

		void OpenFormForSelectedRecord()
		{
			if (CurrentDataItemView != null)
			{
				var parentObj = Factory.Load(CurrentDataItemView.MDM_ParentTableCode, CurrentDataItemView.MDM_ParentID);
				if (parentObj != null)
				{
					var parentObjType = parentObj.GetType();
					var controller = ZControllerFactory.Instance.GetControllerForTypeOrItsBaseTypes(parentObjType) ?? GetTopLevelBizOController(ref parentObj);
					if (controller != null)
					{
						controller.ShowEditForm(parentObj);
					}
					else
					{
						var message = Res.GetString("41e4ab28-dc75-43bd-8d9c-a5b767d23714", "Can't load Controller for type: {0}", parentObjType);
						Globals.Message.ShowError(message);
						ErrorReporter.ReportOnce("AdministrationPanelForm|OpenRecordForAddress|CannotFindController", message + "\r\nParentTableCode: " + CurrentDataItemView.MDM_ParentTableCode + "\r\nCurrent Country Code: " + GlbCompany.CurrentCompany.Country + "\r\nPlease assign this issue to capability MDD.");
					}
				}
			}
		}

		ZController GetTopLevelBizOController(ref BusinessObject bizo)
		{
			if (bizo is ITopLevelBizOProviderForJobDocAddress provider)
			{
				bizo = provider.GetTopBusinessObject();
				return ZControllerFactory.Instance.GetControllerForBizo(bizo);
			}

			return null;
		}

		#endregion

		#region Save

		internal bool SaveSingleRecord(MDMAdminPanelAddressView addressView, bool isAutoVerify = false)
		{
			var success = SaveAddressInfo(addressView, isAutoVerify);

			if (success)
			{
				RefreshGrid?.Invoke(this, new[] { addressView.PK });
			}

			return success;
		}

		void SetHasAddressInfoChanges(MDMAdminPanelAddressView addressView, bool hasChanges)
		{
			if (addressView != null)
			{
				addressView.HasAddressInfoChanges = hasChanges;
				if (addressView.PK == CurrentDataItemView?.PK)
				{
					SaveButton.Enabled = hasChanges;
				}
			}
		}

		internal bool SaveAddressInfo(MDMAdminPanelAddressView addressView, bool isAutoVerify = false)
		{
			var success = false;

			if (addressView != null)
			{
				ISupportWebAddressValidation originalAddress = null;

				var originalAddressFactory = new BusinessObjectFactory { RefreshEnabled = false };

				if (addressView.AddressEntity is JobDocAddress jobDocAddress)
				{
					originalAddress = originalAddressFactory.Load<JobDocAddress>(jobDocAddress.PK);
				}
				else if (addressView.AddressEntity is OrgAddress orgAddress)
				{
					originalAddress = originalAddressFactory.Load<OrgAddress>(orgAddress.PK);
				}
				else
				{
					throw new ArgumentException("Only JobDocAddress and OrgAddress are Supported");
				}

				success = Save(addressView, isAutoVerify);

				if (success)
				{
					ISupportWebAddressValidation updatedAddress = (ISupportWebAddressValidation)addressView.AddressEntity;

					UpdateLog(CurrentDataItemView.MDM_ParentTableCode, CurrentDataItemView.MDM_ParentID, originalAddress, updatedAddress);
				}
			}

			return success;
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
		bool Save(MDMAdminPanelAddressView addressView, bool isAutoVerify = false)
		{
			try
			{
				if (isAutoVerify || !HasErrors(addressView))
				{
					SaveAddressEntity(addressView, isAutoVerify);
					return true;
				}

				var messageBox = new ZErrorMessageBox(addressView.AddressEntity);
				ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
				messageBox.Dispose();
				RefreshSuggestionControl(SuggestionControlStatus.HideListView);
			}
			catch (ZSaveConcurrencyException)
			{
				RefreshCancellationToken();
				Globals.Message.ShowError(ConcurrencyException);
			}

			return false;
		}

		bool HasErrors(MDMAdminPanelAddressView mdmAddress)
		{
			if (mdmAddress.AddressEntity is OrgAddress orgaddress)
			{
				orgaddress.IgnoreValidationStatusError = true;
				orgaddress.IgnorePhoneNumberError = true;
				orgaddress.IgnoreFaxNumberError = true;
				orgaddress.Validation.ValidateOA_ValidationStatus();
				orgaddress.Validation.ValidateOA_Code();
				if (orgaddress.PrimaryOrgAddressAdditionalInfo != null)
				{
					orgaddress.PrimaryOrgAddressAdditionalInfo.Validation.ValidateOAI_AdditionalInfo();
				}
				orgaddress.Validation.ValidateOA_Address1();
				orgaddress.Validation.ValidateOA_Address2();
				orgaddress.Validation.ValidateOA_RN_NKCountryCode();
				orgaddress.Validation.ValidateOA_City();
				orgaddress.Validation.ValidateOA_PostCode();
				orgaddress.Validation.ValidateOA_State();
				orgaddress.Validation.ValidateOA_Phone_Formatted();
				orgaddress.Validation.ValidateOA_Fax_Formatted();
			}
			else if (mdmAddress.AddressEntity is JobDocAddress jobDocAddress)
			{
				jobDocAddress.IgnoreValidationStatusError = true;
				jobDocAddress.Validation.ValidateE2_ValidationStatus();
				jobDocAddress.Validation.ValidateE2_Address1();
				jobDocAddress.Validation.ValidateE2_Address2();
				jobDocAddress.Validation.ValidateE2_RN_NKCountryCode();
				jobDocAddress.Validation.ValidateE2_City();
				jobDocAddress.Validation.ValidateE2_Postcode();
				jobDocAddress.Validation.ValidateE2_State();
			}

			return mdmAddress.AddressEntity.HasErrors;
		}

		void SaveAddressEntity(MDMAdminPanelAddressView mdmView, bool isAutoVerify = false)
		{
			mdmView.SaveAddressEntity();
			SetHasAddressInfoChanges(mdmView, false);
			UpdateAddressView(mdmView, isAutoVerify);
		}

		void UpdateAddressView(MDMAdminPanelAddressView addressView, bool isAutoVerify = false)
		{
			addressView.MDM_AddressCode = ((ISupportWebAddressValidation)addressView.AddressEntity).AddressCode;
			addressView.MDM_Address1 = ((ISupportWebAddressValidation)addressView.AddressEntity).Address1;
			addressView.MDM_Address2 = ((ISupportWebAddressValidation)addressView.AddressEntity).Address2;
			addressView.MDM_Country = ((ISupportWebAddressValidation)addressView.AddressEntity).CountryCodeISO2;
			addressView.MDM_City = ((ISupportWebAddressValidation)addressView.AddressEntity).City;
			addressView.MDM_PostCode = ((ISupportWebAddressValidation)addressView.AddressEntity).Postcode;
			addressView.MDM_State = ((ISupportWebAddressValidation)addressView.AddressEntity).StateCode;
			addressView.MDM_ValidationStatus = ((ISupportWebAddressValidation)addressView.AddressEntity).ValidationStatus;
			addressView.MDM_AdditionalAddressInformation = ((ISupportWebAddressValidation)addressView.AddressEntity).UnrestrictedAdditionalAddressInformation;
			addressView.AutoVerifyState = isAutoVerify ? AddressAutoVerifyState.Verified : AddressAutoVerifyState.Waiting;
		}

		[SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		internal void UpdateLog(string parentTableCode, ZGuid parentId, ISupportWebAddressValidation oldAddress, ISupportWebAddressValidation newAddress)
		{
			var parentObj = Factory.Load(parentTableCode, parentId);
			string logString;
			if (oldAddress.ValidationStatus == newAddress.ValidationStatus)
			{
				logString = Res.GetString("ECB17152-94A2-40AB-AAB0-B421724CD7B6", "Address was edited in MDM Administration from '{0}' to '{1}'.",
					oldAddress.GetFullAddressString(),
					newAddress.GetFullAddressString());
			}
			else
			{
				logString = Res.GetString("B44195ED-F772-4615-B22A-735A83937DEF", "Address was edited in MDM Administration from '{0}' to '{1}'. Validation status changed from {2} to {3}.",
					oldAddress.GetFullAddressString(),
					newAddress.GetFullAddressString(),
					oldAddress.ValidationStatus,
					newAddress.ValidationStatus);
			}
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			parentObj?.GetLogs().AddNew(AutoEvents.EditedARecord, ZString.Format(logString));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();
		}

		protected virtual UpdateChoice GetUpdateChoice(string title, bool showYesButton = true)
		{
			UpdateChoice choice;
			using (var updateForm = new UpdateMultipleJobDocAddressesForm(title, showYesButton))
			{
				ZFormModaliser.ShowDialogWithoutDispose(updateForm, ParentForm);
				choice = updateForm.Choice;
			}

			return choice;
		}

		internal void AcceptAddress(ISupportWebAddressValidation verifiedAddress, bool ignoreCheckMultipleJocDocAddresses = false)
		{
			if (CurrentAddressEntity is JobDocAddress && !ignoreCheckMultipleJocDocAddresses)
			{
				var success = false;
				var hasAddressInfoChanges = CurrentDataItemView.HasAddressInfoChanges;

				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var originalAddress = otherFactory.Load<JobDocAddress>(verifiedAddress.EntityPK);
				var allDuplicatedRecords = originalAddress.AllDuplicatedRecords;

				if (allDuplicatedRecords.Count > 1)
				{
					var messageType = verifiedAddress.ValidationStatus == AddressValidationStatus.Verified ? TitleType.VerifyAll : TitleType.ManuallyVerifyAll;
					var title = GetFormTitle(originalAddress, verifiedAddress, allDuplicatedRecords.Count, messageType);
					var choice = GetUpdateChoice(title, false);
					switch (choice)
					{
						case UpdateChoice.ThisRecord:
							success = SaveSingleRecord(CurrentDataItemView);
							break;
						case UpdateChoice.AllRecords:
							success = SaveMultipleRecords(verifiedAddress, allDuplicatedRecords);
							break;
						case UpdateChoice.Cancel:
							CancelChanges(CurrentDataItemView);
							PerformDataItemChange();
							break;
					}
				}
				else if (allDuplicatedRecords.Count == 1)
				{
					success = SaveSingleRecord(CurrentDataItemView);
				}

				if (success)
				{
					if (verifiedAddress.ValidationStatus == AddressValidationStatus.ManuallyVerified && hasAddressInfoChanges)
					{
						ManuallyVerifyAllInDataBase(verifiedAddress, allDuplicatedRecords);
					}
				}
			}
			else
			{
				SaveSingleRecord(CurrentDataItemView);
			}
		}

		internal void CancelChanges(MDMAdminPanelAddressView addressView)
		{
			addressView.AddressEntity.CancelChanges();
			addressView.AddressEntity.ClearHasChanges();
			addressView.AddressEntity.RefreshBinding();
			SetHasAddressInfoChanges(addressView, false);
		}

		internal void ManuallyVerifyAllInDataBase(ISupportWebAddressValidation verifiedAddress, List<JobDocAddress> updatedAddresses)
		{
			var jobDocAddress = verifiedAddress as JobDocAddress;
			var allDuplicatedRecords = jobDocAddress.AllDuplicatedRecords;
			var duplicatedRecords = allDuplicatedRecords.Except(updatedAddresses, Comparer<JobDocAddress>.Create((x, y) => x.PK.CompareTo(y.PK))).ToList();
			if (duplicatedRecords.Any())
			{
				var title = GetFormTitle(jobDocAddress, verifiedAddress, allDuplicatedRecords.Count, TitleType.ManuallyVerifyAllInDataBase);
				var choice = GetUpdateChoice(title);
				if (choice == UpdateChoice.Yes)
				{
					SaveMultipleRecords(jobDocAddress, duplicatedRecords);
				}
			}
		}

		#endregion

		#region DuplicatedRecords

		internal string GetFormTitle(JobDocAddress originalJobDocAddress, ISupportWebAddressValidation verifiedJobDocAddress, int times, TitleType type)
		{
			var result = string.Empty;
			var originalAddressFull = originalJobDocAddress.AddressFull;
			var replacedAddressFull = (verifiedJobDocAddress as JobDocAddress)?.AddressFull;

			if (type == TitleType.VerifyAll)
			{
				result = string.Format(CultureInfo.CurrentCulture, (NoResString)"The address '{0}' is in the system {1} times. Do you want to update all records to '{2}' ?", originalAddressFull, times, replacedAddressFull);
			}
			else if (type == TitleType.ManuallyVerifyAll)
			{
				result = string.Format(CultureInfo.CurrentCulture, (NoResString)"The address '{0}' is in the system {1} times. Do you want to update all records to '{2}' and manually verify these addresses?", originalAddressFull, times, replacedAddressFull);
			}
			else if (type == TitleType.ManuallyVerifyAllInDataBase)
			{
				result = string.Format(CultureInfo.CurrentCulture, (NoResString)"The address '{0}' is in the system {1} times. Do you want to manually verify all records with the same address '{2}'?", originalAddressFull, times, replacedAddressFull);
			}

			return result;
		}

		internal bool SaveMultipleRecords(ISupportWebAddressValidation verifiedJobDocAddress, List<JobDocAddress> duplicatedAddresses)
		{
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var originalAddress = otherFactory.Load<JobDocAddress>(verifiedJobDocAddress.EntityPK);

			var result = true;
			var pks = new List<ZGuid>();
			try
			{
				using (Manager.AdminPanelAddressCollection.SuspendListChanged())
				using (Manager.AdminPanelProcessedAddressCollection.SuspendListChanged())
				{
					foreach (var address in duplicatedAddresses)
					{
						var pk = address.EntityPK;
						var addressView = GetAddressViewByPk(pk);
						if (addressView != null)
						{
							SetVerifiedInfoToAddressEntity(verifiedJobDocAddress, (ISupportWebAddressValidation)addressView.AddressEntity);
							SaveAddressEntity(addressView);
							UpdateLog(addressView.MDM_ParentTableCode, addressView.MDM_ParentID, originalAddress, verifiedJobDocAddress); //OLD address then NEW address
							pks.Add(pk);
						}
						else
						{
							SetVerifiedInfoToAddressEntity(verifiedJobDocAddress, address);
						}
					}

					Factory.Save();
				}
			}
			catch (ZSaveConcurrencyException)
			{
				result = false;
				RefreshCancellationToken();
				Globals.Message.ShowError(ConcurrencyException);
			}
			finally
			{
				if (pks.Any())
				{
					RefreshGrid?.Invoke(this, pks.ToArray());
				}
			}

			return result;
		}

		MDMAdminPanelAddressView GetAddressViewByPk(ZGuid pk)
		{
			var originalCollection = Manager.AdminPanelAddressCollection;
			var processedCollection = Manager.AdminPanelProcessedAddressCollection;
			return (originalCollection?.FindByPK(pk) ?? processedCollection?.FindByPK(pk)) as MDMAdminPanelAddressView;
		}

		void SetVerifiedInfoToAddressEntity(ISupportWebAddressValidation verifiedJobDocAddress, ISupportWebAddressValidation addressEntity)
		{
			addressEntity.AddressMap = verifiedJobDocAddress.AddressMap;
			addressEntity.GeoLocation = verifiedJobDocAddress.GeoLocation;
			addressEntity.Address1 = verifiedJobDocAddress.Address1;
			addressEntity.Address2 = verifiedJobDocAddress.Address2;
			addressEntity.CountryCodeISO2 = verifiedJobDocAddress.CountryCodeISO2;
			addressEntity.City = verifiedJobDocAddress.City;
			addressEntity.Postcode = verifiedJobDocAddress.Postcode;
			addressEntity.StateCode = verifiedJobDocAddress.StateCode;
			addressEntity.ValidationStatus = verifiedJobDocAddress.ValidationStatus;
		}

		#endregion

		#region UI

		void RefreshValidationStatus()
		{
			RefreshLinkControl();

			if (ShouldValidate)
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, AddressForValidation.ValidationStatus, true);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1BoundTextBox, Address2BoundTextBox, CityBoundTextBox, PostCodeBoundTextBox, StateBoundDropEdit, CountryFindBox, AddressForValidation.ValidationStatus);
				ValidateAddressButton.Enabled = true;
				ClearFieldsButton.Enabled = true;
			}
			else
			{
				AddressValidationUIHelper.ResetAddressFieldState(Address1BoundTextBox, Address2BoundTextBox, CityBoundTextBox, PostCodeBoundTextBox, StateBoundDropEdit, CountryFindBox);
				ValidateAddressButton.Enabled = false;
				ClearFieldsButton.Enabled = false;
				RefreshSuggestionControl(SuggestionControlStatus.Disabled);
			}
		}

		void ReBindAddressByType()
		{
			if (CurrentAddressEntity is OrgAddress currentOrgAddressEntity)
			{
				currentOrgAddressEntity.IsInAdminPanel = true;
				IsMainAddress = currentOrgAddressEntity.IsMainAddress;
				AllowModifyCurrentAddress = currentOrgAddressEntity.AllowModify;
				BindingSource.DataSourceType = typeof(OrgAddress);
				AdditionalAddressInformationBoundTextBox.ReadOnly = false;
				AdditionalAddressInformationBoundTextBox.SetDataBinding(CurrentAddressEntity, nameof(currentOrgAddressEntity.PrimaryOrgAddressAdditionalInfoDetail));
				Address1BoundTextBox.SetDataBinding(CurrentAddressEntity, OrgAddressSchema.OA_Address1.Name);
				Address2BoundTextBox.SetDataBinding(CurrentAddressEntity, OrgAddressSchema.OA_Address2.Name);
				CountryFindBox.SetDataBinding(CurrentAddressEntity, OrgAddressSchema.OA_RN_NKCountryCode.Name);
				CityBoundTextBox.SetDataBinding(CurrentAddressEntity, OrgAddressSchema.OA_City.Name);
				StateBoundDropEdit.SetDataBinding(CurrentAddressEntity, OrgAddressSchema.OA_State.Name);
				PostCodeBoundTextBox.SetDataBinding(CurrentAddressEntity, OrgAddressSchema.OA_PostCode.Name);
				AddressCodeTextBox.ReadOnly = false;
				AddressCodeTextBox.SetDataBinding(CurrentAddressEntity, OrgAddressSchema.OA_Code.Name);
			}
			else if (CurrentAddressEntity is JobDocAddress currentJobDocAddressEntity)
			{
				currentJobDocAddressEntity.IsInAdminPanel = true;
				AllowModifyCurrentAddress = true;
				BindingSource.DataSourceType = typeof(JobDocAddress);
				AdditionalAddressInformationBoundTextBox.ReadOnly = true;
				AdditionalAddressInformationBoundTextBox.SetDataBinding(CurrentDataItemView, MDMAdminPanelAddressViewSchema.MDM_AdditionalAddressInformation.Name);
				Address1BoundTextBox.SetDataBinding(CurrentAddressEntity, JobDocAddressSchema.E2_Address1.Name);
				Address2BoundTextBox.SetDataBinding(CurrentAddressEntity, JobDocAddressSchema.E2_Address2.Name);
				CountryFindBox.SetDataBinding(CurrentAddressEntity, JobDocAddressSchema.E2_RN_NKCountryCode.Name);
				CityBoundTextBox.SetDataBinding(CurrentAddressEntity, JobDocAddressSchema.E2_City.Name);
				StateBoundDropEdit.SetDataBinding(CurrentAddressEntity, JobDocAddressSchema.E2_State.Name);
				PostCodeBoundTextBox.SetDataBinding(CurrentAddressEntity, JobDocAddressSchema.E2_Postcode.Name);
				AddressCodeTextBox.ReadOnly = true;
				AddressCodeTextBox.SetDataBinding(CurrentDataItemView, MDMAdminPanelAddressViewSchema.MDM_AddressCode.Name);
			}

			OrganisationFindBox.ReadOnly = true;
			OrganisationFindBox.SetDataBinding(CurrentDataItemView, MDMAdminPanelAddressViewSchema.MDM_ParentID.Name);
			OrganisationNameTextbox.SetDataBinding(CurrentDataItemView, MDMAdminPanelAddressViewSchema.MDM_CompanyName.Name);
			AddressTypeTextBox.SetDataBinding(CurrentDataItemView, MDMAdminPanelAddressViewSchema.MDM_AddressType.Name);
			HookAddressInfoChangedEvent();
		}

		internal void RefreshLinkControl()
		{
			var enabledForCompany = !string.IsNullOrEmpty(CompanyAndCountryName);
			CopyCompanyInfoLink.Enabled = enabledForCompany;
			SearchCompanyOnlineLink.Enabled = enabledForCompany;

			var enabledForAddress = !string.IsNullOrEmpty(AddressInfo);
			CopyAddressInfoLink.Enabled = enabledForAddress;
			SearchAddressOnlineLink.Enabled = enabledForAddress;
			RefreshMenuItem(enabledForCompany, enabledForAddress);
		}

		void RefreshMenuItem(bool enabledForCompany, bool enabledForAddress)
		{
			bool isOnCurrentSelectedTab = IsOnCurrentSelectedTab();

			if (CopyCompanyInformationMenuItem != null)
			{
				CopyCompanyInformationMenuItem.Enabled = enabledForCompany && isOnCurrentSelectedTab;
			}

			if (SearchCompanyOnlineMenuItem != null)
			{
				SearchCompanyOnlineMenuItem.Enabled = enabledForCompany && isOnCurrentSelectedTab;
			}

			if (CopyAddressInformationMenuItem != null)
			{
				CopyAddressInformationMenuItem.Enabled = enabledForAddress && isOnCurrentSelectedTab;
			}

			if (SearchAddressOnlineMenuItem != null)
			{
				SearchAddressOnlineMenuItem.Enabled = enabledForAddress && isOnCurrentSelectedTab;
			}
		}

		internal void RefreshSuggestionControl(SuggestionControlStatus status, WebAddressValidationResult result = null)
		{
			if (!ShouldValidate)
			{
				status = SuggestionControlStatus.Disabled;
			}

			switch (status)
			{
				case SuggestionControlStatus.Disabled:
					AddressSuggestionControl.ClearResultsList();
					AddressSuggestionControl.Enabled = false;
					break;
				case SuggestionControlStatus.SetUpListView:
					AddressSuggestionControl.SetupListView(AddressForValidation, result);
					AddressSuggestionControl.Enabled = true;
					break;
				case SuggestionControlStatus.HideListView:
					AddressSuggestionControl.HideListView(AddressForValidation);
					AddressSuggestionControl.Enabled = true;
					break;
			}
		}

		internal enum SuggestionControlStatus
		{
			Empty = 0,
			Disabled = 1,
			SetUpListView = 2,
			HideListView = 3
		}

		#endregion

		#region IReadOnlyToggleControl Members

		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				readOnly = value;
				ValidateAddressButton.ReadOnly = value;
			}
		}
		bool readOnly;

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				DisposeCancellationToken();
			}

			base.Dispose(isNotFinalizing);
		}

		void DisposeCancellationToken()
		{
			if (CancellationToken != null)
			{
				CancellationToken.Cancel();
				CancellationToken.Dispose();
				CancellationToken = null;
			}
		}

		public void RefreshCancellationToken()
		{
			DisposeCancellationToken();
			CancellationToken = new CancellationTokenSource();
		}

		#endregion

		#region Address Validation

		public ISupportWebAddressValidation AddressForValidation => (ISupportWebAddressValidation)CurrentAddressEntity;
		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		protected void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			RunAddressValidation = true;
			BeginValidateAddressAsync();
		}

		async void BeginValidateAddressAsync()
		{
			if (CurrentAddressEntity != null)
			{
				if (!CurrentAddressEntity.IsInDatabase || AllowModifyCurrentAddress)
				{
					SetUIStatus(false, true);

					try
					{
						await ValidateAddressAsync();
					}
					finally
					{
						if (!Disposing && !IsDisposed && CurrentAddressEntity != null) //Form closed before validation finished can raise "accessing disposed control" exception
						{
							SetUIStatus(false, false);
						}
					}

					if (AddressForValidation != null)
					{
						if (AddressForValidation.ValidationStatus == AddressValidationStatus.Unverifiable || AddressForValidation.ValidationStatus == AddressValidationStatus.CountryNotAvailable)
						{
							AddressForValidation.ValidatePostcodeAndStateForAddress();
						}
					}
				}
				else
				{
					if (IsMainAddress)
					{
						Env.Security.OrgDetailsModifyNameAndAddress.ShowError();
					}
					else
					{
						Env.Security.OrgAddressDetailsModify.ShowError();
					}

					SetUIStatus(false, false);
					RefreshSuggestionControl(SuggestionControlStatus.Disabled);
				}
			}
		}

		protected virtual Task<WebAddressValidationResult> GetWebAddressValidationResult()
		{
			return AddressForValidation.ValidateAddressAsync(CancellationToken);
		}

		internal async Task ValidateAddressAsync()
		{
			if (!AddressValidationSuspended && IsOnCurrentSelectedTab() && ShouldValidate)
			{
				if (AddressValidationService.IsAddressNeedValidation(AddressForValidation) && (RunAddressValidation || AddressForValidation.ValidationStatus != AddressValidationStatus.ManuallyVerified))
				{
#if DEBUG
					WebAddressValidationStarted = true;
#endif
					var preCurrentItemPk = AddressForValidation.EntityPK;
					var result = await GetWebAddressValidationResult();

					//only show currentItem's info
					if (CurrentAddressEntity != null && CurrentAddressEntity.PK == preCurrentItemPk && result != null)
					{
						if (result.ResultAddress?.ResultStatusCode == ValidationResultStatusCode.PointExact || result.ResultAddress?.ResultStatusCode == ValidationResultStatusCode.PrivateAddressValid)
						{
							AddressSuggestionControl_AddressSelected(this, new AddressSuggestionControl.AddressSelectedEventArgs()
							{
								Address = AddressForValidation,
								PassAsEntered = false,
								SelectedAddress = null
							});
						}
						else
						{
							if (result.SuggestedResults == null && result.TopRecommendedAddress == null)
							{
								RefreshSuggestionControl(SuggestionControlStatus.HideListView);
							}
							else
							{
								RefreshSuggestionControl(SuggestionControlStatus.SetUpListView, result);
							}
						}
					}
				}
				else
				{
					RefreshSuggestionControl(AddressForValidation.ValidationStatus == AddressValidationStatus.ManuallyVerified
						? SuggestionControlStatus.HideListView
						: SuggestionControlStatus.Disabled);

					if (RunAddressValidation && (AddressForValidation.ValidationStatus == AddressValidationStatus.Verified || AddressForValidation.ValidationStatus == AddressValidationStatus.VerifiedToStreet))
					{
						RunAddressValidation = false;
						if (CurrentDataItemView.HasAddressInfoChanges)
						{
							AcceptAddress(AddressForValidation, true);
						}
						else
						{
							RefreshGrid?.Invoke(this, new[] { CurrentAddressEntity.PK });
						}
					}
				}

				RunAddressValidation = false;
			}
		}

#if DEBUG
		public virtual
#endif
		bool IsOnCurrentSelectedTab()
		{
			var parentTabPage = ParentControl?.Parent;
			var parebtTabControl = parentTabPage?.Parent as ZTabControl;
			if (parebtTabControl != null)
			{
				return parebtTabControl.SelectedTab == parentTabPage;
			}

			return false;
		}

		protected bool ShouldValidate => CurrentDataItemView != null &&
							   CurrentAddressEntity != null &&
							   ((ILocation)CurrentAddressEntity).Country != null &&
							   OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(((ILocation)CurrentAddressEntity).Country.PK.ToGuid(), AddressValidationSection.AdminPanel) &&
							   AllowModifyCurrentAddress;

		#endregion

		#region LinkControl

		string CompanyAndCountryName
		{
			get
			{
				var result = string.Empty;
				if (CurrentDataItemView != null)
				{
					var companyName = CurrentDataItemView.MDM_CompanyName;
					var countryName = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, this.CountryFindBox.CurrentCode))?.RN_Desc;
					if (!string.IsNullOrEmpty(companyName))
					{
						result = (companyName + " " + countryName).Trim();
					}
				}

				return result;
			}
		}

		string AddressInfo
		{
			get
			{
				var addressInfo = string.Empty;
				var addressForValidation = AddressForValidation;
				if (addressForValidation != null)
				{
					var countryName = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, CountryFindBox.CurrentCode))?.RN_Desc;
					addressInfo = string.Join(" ", new[] { AddressForValidation.Address1, AddressForValidation.Address2, AddressForValidation.City, AddressForValidation.State, AddressForValidation.Postcode, countryName }.Where(u => !string.IsNullOrEmpty(u)));
				}

				return addressInfo;
			}
		}

		void CopyCompanyInfoLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			CopyCompanyInformation();
		}

		void CopyAddressInfoLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			CopyAddressInformation();
		}

		internal void CopyCompanyInformation()
		{
			SafeClipboard.SetText(CompanyAndCountryName);
		}

		internal void CopyAddressInformation()
		{
			SafeClipboard.SetText(AddressInfo);
		}

		void SearchCompanyOnlineLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SearchCompanyOnline();
		}

		void SearchAddressOnlineLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SearchAddressOnline();
		}

		internal void SearchCompanyOnline()
		{
			WebUrlLauncher.Launch("http://google.com/search?q=" + WebUtility.UrlEncode(CompanyAndCountryName));
		}

		internal void SearchAddressOnline()
		{
			WebUrlLauncher.Launch("http://google.com/maps/search/" + WebUtility.UrlEncode(AddressInfo));
		}

		#endregion

		public ZTextBox AdditionalAddressInformationControl => AdditionalAddressInformationBoundTextBox;

		public ZTextBox Address1Control => Address1BoundTextBox;

		public ZTextBox Address2Control => Address2BoundTextBox;

		public ZCodeFindBox CountryControl => CountryFindBox;

		public ZButton ValidateButton => ValidateAddressButton;

		public ZButton ClearButton => ClearFieldsButton;

		public ZTextBox CityControl => CityBoundTextBox;

		public ZTextBox PostcodeControl => PostCodeBoundTextBox;

		public ZDropEdit StateControl => StateBoundDropEdit;

		public Control SuggestionWindowParentControl => AddressDetailGroupBox;

		public ZLinkLabel CopyCompanyInfoLinkControl => CopyCompanyInfoLink;

		public ZLinkLabel SearchCompanyOnlineControl => SearchCompanyOnlineLink;

		public ZLinkLabel CopyAddressInfoLinkControl => CopyAddressInfoLink;

		public ZLinkLabel SearchAddressOnlineControl => SearchAddressOnlineLink;

		public bool AddressValidationSuspended { get; set; }

		public bool ValidationJustForced { get; set; }

		public ZTextBox AddressCodeControl => AddressCodeTextBox;

		public ZTextBox OrganisationNameControl => OrganisationNameTextbox;

		public ZOrganisationFindBox OrganisationControl => OrganisationFindBox;

		public ZTextBox AddressTypeControl => AddressTypeTextBox;

		CancellationTokenSource CancellationToken { get; set; }

		internal bool BindingCompleted { get; set; }

		bool IsMainAddress { get; set; }

		bool AllowModifyCurrentAddress { get; set; }

		internal bool RunAddressValidation { get; set; }

		internal ZMenuItem CopyCompanyInformationMenuItem { get; set; }

		internal ZMenuItem SearchCompanyOnlineMenuItem { get; set; }

		internal ZMenuItem CopyAddressInformationMenuItem { get; set; }

		internal ZMenuItem SearchAddressOnlineMenuItem { get; set; }

		internal virtual AddressUserControl ParentControl { get; set; }

		internal MDMAdminPanelAddressView CurrentDataItemView => CurrentDataItem as MDMAdminPanelAddressView;

		internal BusinessObject CurrentAddressEntity => CurrentDataItemView?.AddressEntity;

		internal bool IsVerifying { get; set; }

		bool IsAutoVerified => CurrentDataItemView.AutoVerifyState == AddressAutoVerifyState.Verified;

		internal bool IsAutoVerifying => CurrentDataItemView.AutoVerifyState == AddressAutoVerifyState.Verifying || CurrentDataItemView == ParentControl.NextAutoVerifyItem;

		AdministrationPanelManager Manager { get; }

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		readonly string ConcurrencyException = ResString.GetMultilingualString("981C72D6-F869-41CA-AF59-A2C4F831EBF4", "While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please close and open this module to try again.");

		internal event EventHandler<ZGuid[]> RefreshGrid;

#if DEBUG

		internal bool WebAddressValidationStarted;

		protected SingleAddressValidationControl()
		{
			InitializeComponent();
			InitializeControl();
		}
#endif
	}
}
