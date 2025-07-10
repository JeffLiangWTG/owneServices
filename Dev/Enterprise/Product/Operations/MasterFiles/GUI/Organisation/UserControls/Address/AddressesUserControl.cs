using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;
using static Enterprise.Freight.Integration.Forwarding;
using ISupportWebAddressValidation = Enterprise.MasterFiles.Business.ISupportWebAddressValidation;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AddressesUserControl : OrganisationSecurityContainerControl, IReadOnlyToggleControl, ISupportWebAddressValidationControl
	{
		public AddressesUserControl()
		{
			InitializeComponent();
			AddCopyAddressForAnalysisMenuItem();

			if (!DesignModeFinder.IsDesigning)
			{
				SetCharacterCasing();

				var supplyChainSecurityConfiguration = ObjectFactory.New<ISupplyChainSecurityConfigurationHelper>().GetConfiguration();

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedStates)
				{
					USKnownShipperTabPage.TabVisible = false;
				}

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore
					|| !supplyChainSecurityConfiguration.IsAddressLevelScheme)
				{
					GenericKnownShipperTabPage.TabVisible = false;
				}

				if (GenericKnownShipperTabPage.TabVisible)
				{
					var licenceCountryCode = supplyChainSecurityConfiguration.LicenceEconomicGroupingCode.IsEmpty
						? GlbCompany.CurrentCompany.GC_RN_NKCountryCode
						: supplyChainSecurityConfiguration.LicenceEconomicGroupingCode;

					GenericKnownShipperTabPage.Text = Res.GetString("23d0767b-581e-4789-9322-e5ded838987d", "Supply Chain Security ({0})", licenceCountryCode);
				}

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
					ValidateAddressButton.ReadOnly = ClearFieldsButton.ReadOnly = !Env.Security.OrgAddressModify.IsAllowed;
					HookResizeEvent();
				}
				else
				{
					ValidateAddressButton.Visible = false;
					ClearFieldsButton.Visible = false;
				}

				if (DataRegistry.Instance.ProductivityWiseModeEnabled)
				{
					SetUpControlForProductivityWiseMode();
				}
			}
			ValidationJustForced = false;

			USKnownShipperTabPage.Leave += new EventHandler(KnownShipperTabPage_Leave);
			USKnownShipperTabPage.Enter += new EventHandler(KnownShipperTabPage_Enter);

			GenericKnownShipperTabPage.Leave += new EventHandler(KnownShipperTabPage_Leave);
			GenericKnownShipperTabPage.Enter += new EventHandler(KnownShipperTabPage_Enter);

			OrgAddressBoundGrid.AfterBind += new EventHandler(OrgAddressBoundGrid_AfterBind);
			OrgAddressBoundGrid.SelectedRowsChangedInMouseDown += new EventHandler(OrgAddressBoundGrid_SelectedRowsChangedInMouseDown);

			InitializeTimeTablesAndRadioButtonEnabledStatus(SelectedOrgAddress?.TimetablesRangeType);
			MissingResourceStringChecker.ExcludeFromTest(OA_JobLoadingDurationCalcEdit);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public const string YaheiFontName = "Microsoft YaHei";

		void HookResizeEvent()
		{
			Resize += AddressesUserControl_Resize;
		}

		protected virtual void AddressesUserControl_Resize(object sender, EventArgs e)
		{
			var suggestionControl = SupportWebAddressValidationControlHelper.FindAddressSuggestionControl(this);
			if (suggestionControl != null && suggestionControl.Visible)
			{
				SupportWebAddressValidationControlHelper.ShowAddressSuggestionControl(this, 0, MaxHeight, true);
			}
		}

		void SetUpControlForProductivityWiseMode()
		{
			ExtraDetailsTabControl.Visible = false;
			ControlDpiScalingHelper.SetWidth(AddressDetailsGroupBox, ExtraDetailsTabControl.Width + AddressDetailsGroupBox.Width, false);
		}

		void SelectLocalAddressDropEdit_Validated(object sender, EventArgs e)
		{
			UpdateControlsEnabledStatus();
		}

		void OrgAddressBoundGrid_AfterBind(object sender, EventArgs e)
		{
			if (OrgAddressBoundGrid.ListManager != null)
			{
				OrgAddressBoundGrid.ListManager.CurrentChanged += new EventHandler(OrgAddressBoundGridListManager_CurrentChanged);
				RefreshCustomsAddressSecurityLabel();
				HookValidationStatusChangeEvent();
				UpdateControlsEnabledStatus();
			}
		}

		void OrgAddressBoundGrid_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
		{
			UpdateContextMenu();
		}

		void OrgAddressBoundGridListManager_CurrentChanged(object sender, EventArgs e)
		{
			RefreshCustomsAddressSecurityLabel();
			HookValidationStatusChangeEvent();
			CloseSuggestionForms();
			InitializeTimeTablesAndRadioButtonEnabledStatus(SelectedOrgAddress?.TimetablesRangeType);
		}

		const int numberOfAddressesToCombine = 2;

		ZMenuItem combineMenuItem;
		CombineTranslatedAddressForm addressCombinerForm;
		void UpdateContextMenu()
		{
			if (combineMenuItem != null)
			{
				OrgAddressBoundGrid.ContextMenu.MenuItems.Remove(combineMenuItem);
			}

			var addresses = OrgAddressBoundGrid.GetSelectedElements<OrgAddress>();

			if (addresses.Length == numberOfAddressesToCombine)
			{
				var addressCombiner = new AddressCombiner(Org.Factory, addresses[0], addresses[1]);

				if (addressCombiner.CanCombine())
				{
					var combine = Res.GetString("94866FC1-1A06-4D0E-AC45-8406B4FF4A7A", "Combine Addresses");
					combineMenuItem = new ZMenuItem(combine, delegate
					{
						if (Env.Security.OrgAddressCombineAddresses.IsAllowed)
						{
							addressCombinerForm = new CombineTranslatedAddressForm(addressCombiner);
							ZFormModaliser.Show(addressCombinerForm, FindForm());
						}
						else
						{
							Env.Security.OrgAddressCombineAddresses.ShowError();
						}
					});
					OrgAddressBoundGrid.ContextMenu.MenuItems.Add(4, combineMenuItem);
				}
			}
		}

		public void OnCopyAddressForAnalysis_Click(object sender, EventArgs e)
		{
			SafeClipboard.SetText(OrgAddress.ConvertAddressToAnalysisText(SelectedOrgAddress));
		}

		void OA_LanguageBoundDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			var toBeFixedControls = new Control[] { OrgAddressBoundGrid, OA_CompanyNameOverrideBoundTextBox, OA_Address1BoundTextBox, OA_Address2BoundTextBox, OA_CityBoundTextBox, OA_LoadingUnloadingConstraintsTextBox, OA_OtherWarehouseFacilitiesTextBox, AddressAdditionalInfoUserControl, TranslatedAddressAdditionalInfoUserControl };
			var languageDropEdit = (ZDropEdit)sender;
			if (languageDropEdit.Text == Core.SharedConstants.Languages.ChineseSimplified && OA_CompanyNameOverrideBoundTextBox.Font.Name != YaheiFontName)
			{
				toBeFixedControls.ForEach(control =>
				{
					control.Font = new Font(YaheiFontName, languageDropEdit.Font.Size);
				});
			}

			if (languageDropEdit.Text != Core.SharedConstants.Languages.ChineseSimplified && OA_CompanyNameOverrideBoundTextBox.Font.Name == YaheiFontName)
			{
				toBeFixedControls.ForEach(control =>
				{
					control.Font = languageDropEdit.Font;
				});
			}
		}

		void AddressTextBox_TextChanged(object sender, EventArgs e)
		{
			if (SuggestionWindowParentControl != null)
			{
				AddressSuggestionControlHelper.CloseSuggestionForm(SuggestionWindowParentControl.Controls);
			}
		}

		void AddCopyAddressForAnalysisMenuItem()
		{
			var menuItem = new ZMenuItem
			{
				Caption = ResString.GetMultilingualString("1083E54B-8C95-42A8-95F6-7F6D2FE5A812", "Copy Address for Analysis"),
				Name = (NoResString)"Copy Address for Analysis"
			};
			menuItem.Click += OnCopyAddressForAnalysis_Click;

			var deleteMenuItem = OrgAddressBoundGrid.DeleteMenuItem;
			if (deleteMenuItem != null)
			{
				OrgAddressBoundGrid.ContextMenu.MenuItems.Add(deleteMenuItem.Index + 1, menuItem);
			}
			else
			{
				OrgAddressBoundGrid.ContextMenu.MenuItems.Add(menuItem);
			}
		}

		void RefreshCustomsAddressSecurityLabel()
		{
			if (SelectedOrgAddress != null)
			{
				CustomsAddressSecurityLabel.CaptionResourceString = SelectedOrgAddress.IsCustomsAddress && !Env.Security.OrgAddressCustomsAddressModify.IsAllowed ?
												Res.GetData("AddressesUserControl|97f7acfa-3d32-4723-831a-82108ec0f631", "You do not have security access to modify Customs Address.") :
																	(SelectedOrgAddress.IsEUCustomsAddress && !Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed ?
												Res.GetData("AddressesUserControl|2DC93F8E-BD0B-4C3F-A283-34B2C7F25667", "You do not have security access to modify EU Customs Address.") : ResourceStringData.Empty);
				CustomsAddressSecurityLabel.Visible = (SelectedOrgAddress.IsCustomsAddress && !Env.Security.OrgAddressCustomsAddressModify.IsAllowed) || (SelectedOrgAddress.IsEUCustomsAddress && !Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed);
			}
		}

		#region Timetable

		void RangeTypeRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			var currentRadioButton = sender as ZRadioButton;
			if (currentRadioButton.Checked)
			{
				if (!RemoveUnSavedTimetableRows(currentRadioButton))
				{
					SelectPreviousRadioButton(currentRadioButton);
				}
			}
		}

		void TimetableGrid_AfterBind(object sender, EventArgs e)
		{
			InitializeTimeTablesAndRadioButtonEnabledStatus(SelectedOrgAddress?.TimetablesRangeType);
		}

		void HookDeleteLastTimeTableEvent()
		{
			if (SelectedOrgAddress != null && SelectedOrgAddress.Timetables != null)
			{
				SelectedOrgAddress.Timetables.OnTriedToDeleteLastTimeTable -= Timetables_OnTriedToDeleteLastTimeTable;
				SelectedOrgAddress.Timetables.OnTriedToDeleteLastTimeTable += Timetables_OnTriedToDeleteLastTimeTable;
			}
		}

		void Timetables_OnTriedToDeleteLastTimeTable(object sender, EventArgs e)
		{
			var message = Res.GetString("51c719c3-a9cb-44f6-b004-6c0a40942f3d", "The last Pickup or Delivery Time can not be deleted!");
			var caption = Res.GetString("AC68BD59-8DE1-4A7C-AA6B-1EA6709DCDE5", "Delete Pickup and Delivery Times");
			Globals.Message.ShowError(message, caption);
		}

		void InitializeTimeTablesAndRadioButtonEnabledStatus(OrgTimeTableRangeType? rangeType)
		{
			if (rangeType.HasValue)
			{
				UnHookRadioButtonCheckedChangedEvent();
				DefaultRadioButton.Checked = false;
				WeekdayRadioButton.Checked = false;
				AdvancedRadioButton.Checked = false;
				NotApplicableRadioButton.Checked = false;

				switch (rangeType.Value)
				{
					case OrgTimeTableRangeType.Default:
						if (!DefaultRadioButton.Checked)
						{
							DefaultRadioButton.Checked = true;
						}
						TimetableGrid.AddToAvailableColumns("DayOfWeek");
						TimetableGrid.ReadOnly = true;
						previousRadioButton = DefaultRadioButton;
						break;
					case OrgTimeTableRangeType.Weekday:
						if (!WeekdayRadioButton.Checked)
						{
							WeekdayRadioButton.Checked = true;
						}
						TimetableGrid.RemoveFromAvailableColumns("DayOfWeek");
						TimetableGrid.ReadOnly = false;
						previousRadioButton = WeekdayRadioButton;
						break;
					case OrgTimeTableRangeType.Advanced:
						if (!AdvancedRadioButton.Checked)
						{
							AdvancedRadioButton.Checked = true;
						}
						TimetableGrid.AddToAvailableColumns("DayOfWeek");
						TimetableGrid.ReadOnly = false;
						previousRadioButton = AdvancedRadioButton;
						break;
					case OrgTimeTableRangeType.NotApplicable:
						if (!NotApplicableRadioButton.Checked)
						{
							NotApplicableRadioButton.Checked = true;
						}
						TimetableGrid.RemoveFromAvailableColumns("DayOfWeek");
						TimetableGrid.ReadOnly = true;
						previousRadioButton = NotApplicableRadioButton;
						break;
				}

				if (SelectedOrgAddress != null && SelectedOrgAddress.Header != null && !SelectedOrgAddress.Header.SecurityProvider.HasModifyAddressAdditionalDetailsSecurity)
				{
					DefaultRadioButton.Enabled = false;
					WeekdayRadioButton.Enabled = false;
					AdvancedRadioButton.Enabled = false;
					NotApplicableRadioButton.Enabled = false;
				}
				else
				{
					DefaultRadioButton.Enabled = true;
					WeekdayRadioButton.Enabled = true;
					AdvancedRadioButton.Enabled = true;
					NotApplicableRadioButton.Enabled = true;
				}

				HookRadioButtonCheckedChangedEvent();
				HookDeleteLastTimeTableEvent();
			}
		}

		void SelectPreviousRadioButton(ZRadioButton currentRadioButton)
		{
			UnHookRadioButtonCheckedChangedEvent();

			currentRadioButton.Checked = false;
			if (previousRadioButton != null)
			{
				previousRadioButton.Checked = true;
				previousRadioButton.Focus();
			}

			HookRadioButtonCheckedChangedEvent();
		}

		ZRadioButton previousRadioButton;

		bool RemoveUnSavedTimetableRows(ZRadioButton radioButton)
		{
			var rangeType = OrgTimeTableRangeType.Default;
			if (radioButton == AdvancedRadioButton)
			{
				rangeType = OrgTimeTableRangeType.Advanced;
			}
			else if (radioButton == NotApplicableRadioButton)
			{
				rangeType = OrgTimeTableRangeType.NotApplicable;
			}
			else if (radioButton == WeekdayRadioButton)
			{
				rangeType = OrgTimeTableRangeType.Weekday;
			}

			var shouldRemove = false;
			if (previousRadioButton != null && (previousRadioButton == DefaultRadioButton || previousRadioButton == NotApplicableRadioButton))
			{
				shouldRemove = true;
			}
			else if (SelectedOrgAddress.TimetablesHasChanges)
			{
				var message = string.Format(CultureInfo.CurrentCulture, (NoResString)@"By resetting to {0} Pickup and Delivery Times all Pickup and Delivery Times entered against other settings will be lost.
Are you sure you want to continue?", radioButton.CaptionResourceString.Caption);
				shouldRemove = Globals.Message.Show(message, Res.GetString("AC68BD59-8DE1-4A7C-AA5B-1EA6709DCDE5", @"Resetting Pickup and Delivery Times"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;
			}
			else
			{
				shouldRemove = true;
			}

			if (shouldRemove)
			{
				InitializeTimeTablesAndRadioButtonEnabledStatus(rangeType);
				SelectedOrgAddress.SetTimetablesRangeType(rangeType);
			}

			return shouldRemove;
		}

		void UnHookRadioButtonCheckedChangedEvent()
		{
			DefaultRadioButton.CheckedChanged -= RangeTypeRadioButton_CheckedChanged;
			WeekdayRadioButton.CheckedChanged -= RangeTypeRadioButton_CheckedChanged;
			AdvancedRadioButton.CheckedChanged -= RangeTypeRadioButton_CheckedChanged;
			NotApplicableRadioButton.CheckedChanged -= RangeTypeRadioButton_CheckedChanged;
		}

		void HookRadioButtonCheckedChangedEvent()
		{
			DefaultRadioButton.CheckedChanged += RangeTypeRadioButton_CheckedChanged;
			WeekdayRadioButton.CheckedChanged += RangeTypeRadioButton_CheckedChanged;
			AdvancedRadioButton.CheckedChanged += RangeTypeRadioButton_CheckedChanged;
			NotApplicableRadioButton.CheckedChanged += RangeTypeRadioButton_CheckedChanged;
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
				ClearFieldsButton.ReadOnly = value;
			}
		}
		bool readOnly;

		#endregion

		#region Known Shipper

#if DEBUG
		internal
#endif
		void KnownShipperTabPage_Enter(object sender, EventArgs e)
		{
			if (OrgAddressBoundGrid.ListManager != null && OrgAddressBoundGrid.ListManager.Position > -1)
			{
				OrgAddress currentAddress = (OrgAddress)OrgAddressBoundGrid.ListManager.GetCurrent();
				if (currentAddress.Header.SecurityProvider.HasModifyConsignorExporterSchemeSecurity &&
					currentAddress.KnownShipperDetails.Count == 0)
				{
					OrgCountryData newData = currentAddress.Header.CountryDataCollectionForThisCompany.AddNew();
					using (newData.SuspendSettingHasChanges())
					{
						newData.OV_OA_ApprovedLocation = currentAddress.PK;
					}
					currentAddress.KnownShipperDetails.Add(newData);
				}
			}
		}

#if DEBUG
		internal
#endif
		void KnownShipperTabPage_Leave(object sender, EventArgs e)
		{
			if (OrgAddressBoundGrid.ListManager != null && OrgAddressBoundGrid.ListManager.Position > -1)
			{
				OrgAddress currentAddress = (OrgAddress)OrgAddressBoundGrid.ListManager.GetCurrent();
				if (currentAddress.Header.SecurityProvider.HasModifyConsignorExporterSchemeSecurity &&
					!((IBusinessObjectCollection)currentAddress.KnownShipperDetails).HasChanges &&
					currentAddress.KnownShipperDetails.Count > 0 &&
					!currentAddress.KnownShipperDetails[0].IsInDatabase)
				{
					using (currentAddress.SuspendSettingHasChanges())
					{
						currentAddress.KnownShipperDetails.DeleteAll();
						((IBusinessObjectCollectionInternals)currentAddress.KnownShipperDetails).HasChangesFromDelete = false;
						currentAddress.Header.CountryDataCollectionForThisCompany.HasChanges = false;
					}
				}
			}
		}

		#endregion

		#region GUI Setup

		void SetCharacterCasing()
		{
			OA_Address1BoundTextBox.CharacterCasing = RequiredCasing;
			OA_Address2BoundTextBox.CharacterCasing = RequiredCasing;
			OA_CityBoundTextBox.CharacterCasing = RequiredCasing;
			OA_StateBoundDropEdit.CharacterCasing = RequiredCasing;
			OA_CompanyNameOverrideBoundTextBox.CharacterCasing = RequiredCasing;

			foreach (object styleInfo in OrgAddressBoundGrid.ColumnStyles)
			{
				if (styleInfo is ZTextBoxColumnStyleInfo)
				{
					((ZTextBoxColumnStyleInfo)styleInfo).CharacterCasing = RequiredCasing;
				}
			}

			OA_EmailTextBox.CharacterCasing = CharacterCasing.Normal;
		}

		CharacterCasing RequiredCasing
		{
			get { return Env.Registry.OrgAllowMixedCase ? CharacterCasing.Normal : CharacterCasing.Upper; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (cancellationToken != null)
				{
					cancellationToken.Cancel();
					cancellationToken.Dispose();
					cancellationToken = null;
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Organisation

		OrgHeader fOrg;
		OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = (OrgHeader)((ZForm)ParentForm).BusinessEntity;
				}
				return fOrg;
			}
		}

		OrgAddress SelectedOrgAddress
		{
			get
			{
				var listManager = OrgAddressBoundGrid.ListManager;
				return listManager != null ? (OrgAddress)listManager.GetCurrent() : null;
			}
		}

		#endregion

		#region Address Validation

		CancellationTokenSource cancellationToken;

		#region ProcessCmdKey
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (ShouldValidateAddress() && ProcessManuallyVerifyShortCutKey(ref msg, keyData))
			{
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		public bool ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData)
		{
			if (SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, ReadOnly, this, AddressForValidation))
			{
				return true;
			}
			return false;
		}
		#endregion

		void HookISupportWebAddressValidationControlChangeFocusEvents()
		{
			SupportWebAddressValidationControlHelper.HookISupportWebAddressValidationControlChangeFocusEvents(this, ISupportWebAddressValidationControl_GotFocus, ISupportWebAddressValidationControl_LostFocus);
		}

		protected void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			var modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities = Org.SecurityProvider.ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities(SelectedOrgAddress);
			if (modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities.Any())
			{
				Env.Security.ShowError(modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities);
			}
			else if (SelectedOrgAddress.IsMainAddress)
			{
				if (Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed)
				{
					SupportWebAddressValidationControlHelper.ClearFields(AddressForValidation, this);
				}
				else
				{
					Env.Security.OrgDetailsModifyNameAndAddress.ShowError();
				}
			}
			else
			{
				if (Org.IsInDatabase)
				{
					if (Env.Security.OrgAddressDetailsModify.IsAllowed)
					{
						SupportWebAddressValidationControlHelper.ClearFields(AddressForValidation, this);
					}
					else
					{
						Env.Security.OrgAddressDetailsModify.ShowError();
					}
				}
				else
				{
					if (Env.Security.OrgAddressDetailsNew.IsAllowed)
					{
						SupportWebAddressValidationControlHelper.ClearFields(AddressForValidation, this);
					}
					else
					{
						Env.Security.OrgAddressDetailsNew.ShowError();
					}
				}
			}
		}

		void ISupportWebAddressValidationControl_GotFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(sender, AddressForValidation, this, 0, MaxHeight);
		}

		void ISupportWebAddressValidationControl_LostFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.HideSuggestionControlsUponLostFocus(this);
		}

		void CloseSuggestionForms()
		{
			AddressSuggestionControlHelper.CloseSuggestionForm(ParentForm.Controls);
			CityTownSuggestionControlHelper.CloseSuggestionForm(AddressDetailsGroupBox.Controls);
		}

		int MaxHeight => AddressDetailsGroupBox.Height - ValidateButton.Location.Y;

		public ISupportWebAddressValidation AddressForValidation
		{
			get
			{
				if (SelectedOrgAddress != null)
				{
					return SelectedOrgAddress.SelectedTranslatedAddress;
				}
				return null;
			}
		}

		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		protected async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			var modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities = Org.SecurityProvider.ModifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities(SelectedOrgAddress);
			if (modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities.Any())
			{
				Env.Security.ShowError(modifyAddressDetailsBlockedSecuritiesAsPerAddressCapabilities);
			}
			else if (SelectedOrgAddress.IsMainAddress)
			{
				if (Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed)
				{
					ValidationJustForced = true;
					await ValidateAddress();
				}
				else
				{
					Env.Security.OrgDetailsModifyNameAndAddress.ShowError();
				}
			}
			else
			{
				if (Org.IsInDatabase)
				{
					if (Env.Security.OrgAddressDetailsModify.IsAllowed)
					{
						ValidationJustForced = true;
						await ValidateAddress();
					}
					else
					{
						Env.Security.OrgAddressDetailsModify.ShowError();
					}
				}
				else
				{
					if (Env.Security.OrgAddressDetailsNew.IsAllowed)
					{
						ValidationJustForced = true;
						await ValidateAddress();
					}
					else
					{
						Env.Security.OrgAddressDetailsNew.ShowError();
					}
				}
			}
		}

		async Task ValidateAddress()
		{
			if (IsOnCurrentSelectedTab() && AddressValidationService.IsAddressNeedValidation(AddressForValidation))
			{
				CleanseAction cleanseAction;

				if (!ValidationJustForced)
				{
					AddressSuggestionControlHelper.CloseSuggestionForm(ParentForm.Controls);
					cleanseAction = CleanseAction.QuickValidate;
				}
				else
				{
					cleanseAction = CleanseAction.ValidateAndSuggest;
					ValidationJustForced = false;
				}

				await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, AddressForValidation, this, ParentForm, Controls, RefreshValidationStatus, null, 0, cleanseAction, null, false, MaxHeight);
			}
		}

		bool IsOnCurrentSelectedTab()
		{
			if (this.Parent != null && this.Parent.Parent as ZTabControl != null)
			{
				return ((ZTabControl)this.Parent.Parent).SelectedTab == this.Parent;
			}
			return false;
		}

		protected async Task GetCityTownAsync()
		{
			if (IsOnCurrentSelectedTab() && (!string.IsNullOrEmpty(AddressForValidation.City) || !string.IsNullOrEmpty(AddressForValidation.Postcode)))
			{
				var maxWidth = OA_Address1BoundTextBox.Width;
				ValidationJustForced = true;
				await CityTownSuggestionControlHelper.GetCityTownAsync(cancellationToken, AddressForValidation, this, ParentForm, Controls, async () => { ValidationJustForced = true; await ValidateAddress(); ValidationJustForced = false; }, maxWidth);
			}
		}

		void HookValidationStatusChangeEvent()
		{
			if (!DesignModeFinder.IsDesigning && Env.Instance.Registry.EnableAddressValidationWebService)
			{
				if (AddressForValidation != null)
				{
					AddressForValidation.AddressValidationStatusChanged += AddressForValidation_AddressValidationStatusChanged;
					AddressForValidation_AddressValidationStatusChanged(null, EventArgs.Empty);
					AddressSuggestionControlHelper.RegisterPropertyChangedEvent(AddressForValidation, ValidateAddress);
					CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(AddressForValidation, GetCityTownAsync);
				}
			}
		}

#if DEBUG
		internal
#endif
		void RefreshValidationStatus()
		{
			if (ShouldValidateAddress())
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, AddressForValidation.ValidationStatus, AddressForValidation.IsErrorSuppressed);
				ValidateAddressButton.ReadOnly = SelectedOrgAddress.ReadOnly;
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryFindBox, AddressForValidation.ValidationStatus);
				AddressDetailsGroupBox.Invalidate();

				ValidateAddressButton.Visible = true;
				ClearFieldsButton.Visible = true;
			}
			else
			{
				AddressValidationUIHelper.ResetAddressFieldState(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryFindBox);

				ValidateAddressButton.Visible = false;
				ClearFieldsButton.Visible = false;
			}
		}

		void AddressForValidation_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			RefreshValidationStatus();

			if (AddressValidationService.IsAddressInValidStatus(AddressForValidation))
			{
				AddressSuggestionControlHelper.CloseSuggestionForm(Controls);
			}
		}

		bool ShouldValidateAddress()
		{
			return
				this.SelectedOrgAddress != null &&
				this.SelectedOrgAddress.Country != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(this.SelectedOrgAddress.Country.PK.ToGuid(), SelectedOrgAddress.ValidationSection);
		}

		public ZTextBox CityControl
		{
			get { return OA_CityBoundTextBox; }
		}

		public ZTextBox PostcodeControl
		{
			get { return OA_PostCodeBoundTextBox; }
		}

		public ZDropEdit StateControl
		{
			get { return OA_StateBoundDropEdit; }
		}

		public ZTextBox Address1Control
		{
			get { return OA_Address1BoundTextBox; }
		}

		public ZTextBox Address2Control
		{
			get { return OA_Address2BoundTextBox; }
		}

		public ZCodeFindBox CountryControl
		{
			get { return CountryFindBox; }
		}

		public ZButton ValidateButton
		{
			get { return ValidateAddressButton; }
		}

		public Control SuggestionWindowParentControl
		{
			get { return this; }
		}

		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public bool ValidationJustForced { get; set; }
		#endregion

		#region Multiple language

		void RemoveLocalAddressButton_Click(object sender, EventArgs e)
		{
			if (SelectedOrgAddress != null)
			{
				var localAddress = SelectedOrgAddress.SelectedTranslatedAddress as OrgTranslatedAddress;
				if (localAddress != null)
				{
					var dialogResult = Globals.Message.Show(Res.GetString("AC68BD59-8DE2-4A7C-AA5B-1EA6709DCDE4", "Are you sure to delete this translated address?"), Res.GetString("0CAB7C15-071F-4D77-A0B3-A38A3175C44A", "Delete Translated Address"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					if (dialogResult == DialogResult.Yes)
					{
						SelectedOrgAddress.DeleteTranslatedAddress(localAddress);
						UpdateControlsEnabledStatus();
					}
				}
			}
		}

#if DEBUG
		internal
#endif
		void AddLocalAddressButton_Click(object sender, EventArgs e)
		{
			if (SelectedOrgAddress != null)
			{
				SelectedOrgAddress.AddNewTranslatedAddress();
				UpdateControlsEnabledStatus();
				OA_Address1BoundTextBox.Focus();
			}
		}

		void SelectLocalAddressDropEdit_BoundValueCommitted(object sender, EventArgs e)
		{
			HookValidationStatusChangeEvent();
			UpdateControlsEnabledStatus();
			CloseSuggestionForms();
		}

#if DEBUG
		internal
#endif
		void UpdateControlsEnabledStatus()
		{
			try
			{
				bool isTranslatedAddress = SelectedOrgAddress != SelectedOrgAddress?.SelectedTranslatedAddress;
				var canModify = ParentForm != null && (bool)Org?.IsInDatabase ? Env.Security.OrganisationModify.IsAllowed : Env.Security.OrganisationNew.IsAllowed;
				RemoveLocalAddressButton_Exposed.Enabled = isTranslatedAddress && canModify;
				CountryFindBox.Enabled = !isTranslatedAddress;
				RelatedPortFindBox.Enabled = !isTranslatedAddress;
				AddressAdditionalInfoUserControl.Visible = !isTranslatedAddress;
				TranslatedAddressAdditionalInfoUserControl.Visible = isTranslatedAddress;
				RefreshValidationStatus();
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This should be fixed by WI00169774, please contact the ROPE team. The NullReferenceException was caught from UpdateControlsEnabledStatus().", e);
			}
		}

		#endregion
	}

	#region AddressZGrid class

	public class AddressZGrid : ZGrid
	{
		protected override int HandleDelete(int clickedRow)
		{
			var indices = GetSelectedRowsIndices(clickedRow);
			var orgAddresses = GetBusinessObjects(indices);
			var associatedAddresses = orgAddresses
				.Where(orgAddress => orgAddress.Factory.Exists(typeof(OrgCusCode), new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, orgAddress.PK)))
				.Cast<OrgAddress>();

			var result = true;
			if (associatedAddresses.Any())
			{
				var dialogResult = Globals.Message.Show(
					Res.GetString("7BDB5354-A68B-43CF-A279-1BDF997FBE74", "The address(es): {0} are associated with Registration Numbers / Codes. If you delete the address(es), the associated premises address will be set to empty. Are you sure to delete the address(es)?", string.Join(", ", associatedAddresses.Select(orgAddress => orgAddress.Address1))),
					Res.GetString("CBD992C5-A592-426A-85D2-C18DD783EBBB", "Delete Address"),
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Warning);

				if (dialogResult == DialogResult.OK)
				{
					var associatedAddressPKs = associatedAddresses.Select(t => t.PK);
					((OrgHeader)DataSource).CustomsCodes.Cast<OrgCusCode>()
						.Where(t => associatedAddressPKs.Contains(t.OK_OA_PremisesAddress))
						.ForEach(t => t.OK_OA_PremisesAddress = ZGuid.Empty);
				}
				else
				{
					result = false;
				}
			}

			return result ? base.HandleDelete(clickedRow) : 0;
		}

		IEnumerable<BusinessObject> GetBusinessObjects(List<int> indices)
		{
			List<BusinessObject> result = null;
			if (List is IBusinessObjectCollection)
			{
				result = new List<BusinessObject>(indices.Count);
				foreach (var index in indices)
				{
					result.Add((BusinessObject)ListManager.List[index]);
				}
			}
			return result ?? Enumerable.Empty<BusinessObject>();
		}

		List<int> GetSelectedRowsIndices(int currentRow)
		{
			var result = new List<int>();
			for (var index = DataGridRowsLength - 1; index >= 0; index--)
			{
				if (IsSelected(index) && index < ListManager.Count)
				{
					result.Add(index);
				}
			}
			if (result.Count == 0 &&
				currentRow >= 0 &&
				currentRow < DataGridRowsLength &&
				currentRow < ListManager.Count)
			{
				result.Add(currentRow);
			}
			return result;
		}
	}

	#endregion
}
