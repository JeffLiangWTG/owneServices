using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WTG.AddressCleansing.Common;

#pragma warning disable IDE0001 // Simplify names. Designer requires fully qualified names to correctly deserialize properties

namespace Enterprise.Recruiter.GUI
{
	public partial class HRJobApplicantForm : Enterprise.ZArchitecture.GUI.ZTemplateForm, ISupportWebAddressValidationControl
	{
		public HRJobApplicantForm(HRJobApplicant applicant)
			: base(applicant)
		{
			applicant.Applications.SetOverrideAllowNew(true);

			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("70ABC86F-88C8-496A-AD61-44069AB0ED09", "Create New Staff"), CreateStaff));

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
			SetEditPersonButtonAvailability();
			Saved += (sender, e) => { SetEditPersonButtonAvailability(); };
		}

		void SetEditPersonButtonAvailability()
		{
			EditPersonButton.Available = Applicant.IsInDatabase && Applicant.Person != null && SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.Value;
		}

		public HRJobApplicant Applicant
		{
			get { return BusinessEntity; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			AddAuditColumns(BusinessEntity.Applications);
			ApplicationsTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.HRJobApplicationView });
			if (!Env.Security.HRJobApplicationEdit.IsAllowed)
			{
				foreach (ZGridColumnInfo column in ApplicationsGrid.ColumnStyles)
				{
					column.IsReadOnly = true;
				}
			}
		}

		public new HRJobApplicant BusinessEntity
		{
			get { return (HRJobApplicant)base.BusinessEntity; }
		}

		void AddAuditColumns(IActiveBusinessObjectCollection applications)
		{
			if (applications != null)
			{
				FilterStripAuditDetails.AddAuditDetailsColumns(ApplicationsGrid, applications.TableName, typeof(HRJobApplication));
			}
		}

		#region Controllers

		protected ZController JobCampaignController
		{
			get
			{
				if (contactController == null)
				{
					contactController = ZControllerFactory.Create(ControllerIDs.HRJobOpenings);
				}
				return contactController;
			}
		}
		ZController contactController;

		protected ZController JobApplicationController
		{
			get
			{
				if (jobApplicationController == null)
				{
					jobApplicationController = ZControllerFactory.Create(ControllerIDs.HRJobApplication);
				}
				return jobApplicationController;
			}
		}
		ZController jobApplicationController;

		protected void ResetJobApplicationController()
		{
			jobApplicationController = null;
		}

		#endregion

		protected void ShowControllerEditForm()
		{
			if (!Applicant.IsInDatabase || Applicant.HasChanges)
			{
				Globals.Message.ShowError(
					Res.GetString("631e220c-6313-4c52-94d2-33b69653b7a0", "Cannot edit application values until the applicant is saved."));

				return;
			}

			HRJobApplication selectedApplication = (HRJobApplication)ApplicationsGrid.ListManager.GetCurrent();
			if (selectedApplication != null && BusinessEntity != null && selectedApplication.HP_HV.IsValid)
			{
				ResetJobApplicationController();
				JobApplicationController.ShowEditForm(selectedApplication);
			}
			else
			{
				Globals.Message.Show(Res.GetString("71efaf02-fadb-432f-a16e-df090645d27b", "Ensure that a valid campaign and a valid application is selected."));
			}
		}

		protected void EditPersonButton_Click(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.GlbPerson);
			controller.SetFormsModalTo(this);
			controller.ShowEditForm(Applicant.Person);
		}

		void ApplicationsGrid_DoubleClick(object sender, EventArgs e)
		{
			ShowControllerEditForm();
		}

		void ApplicationEditButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.Applications.Cast<HRJobApplication>().Any())
			{
				ShowControllerEditForm();
			}
			else
			{
				Globals.Message.Show(Res.GetString("20a01d61-928d-4cf6-8c34-5c070a0499a4", "Please select an application to edit"));
			}
		}

		public override string FormCaption
		{
			get
			{
				string captionPrefix = Res.GetString("Recruiter|HRJobApplicantForm|FormCaption", "Applicant");
				return Applicant != null && !Applicant.HA_FullName.IsEmpty ? captionPrefix + " : " + Applicant.HA_FullName : captionPrefix;
			}
		}

		#region WebAddressValidation

		public ZTextBox AddressCodeControl => null;

		public ZTextBox AdditionalAddressInformationControl => null;

		public ISupportWebAddressValidation Address
		{
			get
			{
				return CurrentDataItem as ISupportWebAddressValidation;
			}
		}

		public ZTextBox Address1Control
		{
			get
			{
				return HA_UserAddress1BoundText;
			}
		}

		public ZTextBox Address2Control
		{
			get
			{
				return HA_UserAddress2BoundText;
			}
		}

		public ZTextBox CityControl
		{
			get
			{
				return HA_CityBoundText;
			}
		}

		public ZTextBox PostcodeControl
		{
			get
			{
				return HA_PostcodeBoundText;
			}
		}

		public ZDropEdit StateControl
		{
			get
			{
				return HA_StateBoundDropEdit;
			}
		}

		public ZCodeFindBox CountryControl
		{
			get
			{
				return HA_CountryFindBox;
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
				return MainTabPage;
			}
		}

		void RefreshValidationStatus()
		{
			if (Address != null)
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, Address.ValidationStatus, Address.IsErrorSuppressed);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl, Address.ValidationStatus);
				PersonalInfoGroupBox.Invalidate();
			}
		}

		async void ISupportWebAddressValidationControlInitialise()
		{
			if (Env.Instance.Registry.EnableAddressValidationWebService && Address != null)
			{
				Applicant.PreValidationForAddressValidationService();
				var topLevelZForm = this.TopLevelControl as ZForm;
				if (topLevelZForm != null && topLevelZForm.DisplayMode != ODisplayMode.ReadOnly && !string.IsNullOrEmpty(Address.Address1) && Address.ValidationStatus != AddressValidationStatus.ManuallyVerified && !Address.Address1Info.ReadOnly)
				{
					await ValidateAddress();
				}
				RefreshValidationStatus();

				//The form may have closed or the binding may have changed by the time the address has been validated, nothing to do if null.
				if (Address != null)
				{
					Address.AddressValidationStatusChanged += Address_AddressValidationStatusChanged;
					AddressSuggestionControlHelper.RegisterPropertyChangedEvent(Address, ValidateAddress);
					CityTownSuggestionControlHelper.RegisterPropertyChangedEvent(Address, GetCityTownAsync);

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

		async Task ValidateAddress()
		{
			//Due to race conditions, we need a non-getter provided reference to the control
			var suggestionWindowParentControl = SuggestionWindowParentControl;
			try
			{
				if (AddressValidationService.IsAddressNeedValidation(Address) && suggestionWindowParentControl != null)
				{
					CleanseAction cleanseAction;

					if (!ValidationJustForced)
					{
						AddressSuggestionControlHelper.CloseSuggestionForm(suggestionWindowParentControl.Controls);
						cleanseAction = CleanseAction.QuickValidate;
					}
					else
					{
						cleanseAction = CleanseAction.ValidateAndSuggest;
						ValidationJustForced = false;
					}

					await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, Address, this, this, suggestionWindowParentControl.Controls, RefreshValidationStatus, null, 0, cleanseAction);
				}
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This should be fixed by WI00176748, please contact the ROPE team. The NullReferenceException was caught from ValidateAddress().", e);
			}
		}

		async Task GetCityTownAsync()
		{
			if ((!string.IsNullOrEmpty(Address.City) || !string.IsNullOrEmpty(Address.Postcode)))
			{
				var location = SuggestionWindowParentControl.PointToClient(PersonalInfoGroupBox.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(StateControl.Left, StateControl.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false)));
				int maxWidth = StateControl.Width;
				await CityTownSuggestionControlHelper.GetCityTownAsync(cancellationToken, Address, this, this, SuggestionWindowParentControl.Controls, async () => { ValidationJustForced = true; await ValidateAddress(); ValidationJustForced = false; }, maxWidth);
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
			ValidationJustForced = true;
			await ValidateAddress();
		}

		void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ClearFields(Address, this);
		}

		public bool ValidationJustForced { get; set; }

		public ISupportWebAddressValidation AddressForValidation
		{
			get { return BusinessEntity; }
		}

		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		CancellationTokenSource cancellationToken;

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

		void HRJobApplicantForm_Load(object sender, EventArgs e)
		{
			ISupportWebAddressValidationControlInitialise();
		}

		GlbStaffCollection CollectionForDefaults { get; set; }

		void CreateStaff(object sender, EventArgs e)
		{
			if (Env.Security.Staff.IsAllowed)
			{
				if (BusinessEntity.Person.StaffCollection.Count == 0)
				{
					CollectionForDefaults = new GlbStaffCollection(BusinessEntity.Person, new ZQuery());

					var controller = ZControllerFactory.Create(ControllerIDs.GlbStaff);
					controller.SetCollectionForDefaultsAndValidation(CollectionForDefaults);
					var form = controller.ShowNewForm() as ZForm;
					form.BusinessEntity.HasChanges = true;
				}
				else
				{
					Globals.Message.Show(Res.GetString("AD099164-740A-4693-A2CC-9DFEEEC0B444", "There is already a Staff for this Person."));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("2834BFFD-BE84-4834-B714-1C1BD7DCA8F2", "You don't have enough security rights to create a Staff record."));
			}
		}
	}
}
