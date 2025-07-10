using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbPersonAddressControl : ZUserControl, ISupportWebAddressValidationControl
	{
		public GlbPersonAddressControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				if (Env.Instance.Registry.EnableAddressValidationWebService)
				{
					cancellationToken = new CancellationTokenSource();
					HandleCreated += (o, e) =>
					{
						if (ParentForm != null)
						{
							ParentForm.FormClosed += (x, y) =>
							{
								cancellationToken?.Cancel();
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

		readonly CancellationTokenSource cancellationToken;

		void HookISupportWebAddressValidationControlChangeFocusEvents()
		{
			SupportWebAddressValidationControlHelper.HookISupportWebAddressValidationControlChangeFocusEvents(this, ISupportWebAddressValidationControl_GotFocus, ISupportWebAddressValidationControl_LostFocus);
		}

		void ISupportWebAddressValidationControl_GotFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(sender, AddressForValidation, this);
		}

		void ISupportWebAddressValidationControl_LostFocus(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.HideSuggestionControlsUponLostFocus(this);
		}

		#region ISupportWebAddressValidationControl
		public ZTextBox AddressCodeControl => null;
		public ZTextBox AdditionalAddressInformationControl => null;
		public ZTextBox Address1Control => Address1BoundTextBox;
		public ZTextBox Address2Control => Address2BoundTextBox;
		public ZTextBox CityControl => CityBoundTextBox;
		public ZTextBox PostcodeControl => PostCodeBoundTextBox;
		public ZDropEdit StateControl => StateBoundDropEdit;
		public ZCodeFindBox CountryControl => CountryFindBox;
		public ZButton ValidateButton => ValidateAddressButton;
		public bool ValidationJustForced { get; set; }
		public Control SuggestionWindowParentControl => ParentForm;

		public bool ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData)
		{
			if (SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, this.GetReadOnly(), this, Person))
			{
				return true;
			}
			return false;
		}

		public ISupportWebAddressValidation AddressForValidation => Person;
		public ZButton ClearAddressFieldsButton => ClearFieldsButton;
		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		public GlbPerson Person => (GlbPerson)CurrentDataItem;

		#endregion

		void ClearFieldsButton_Click(object sender, EventArgs e)
		{
			SupportWebAddressValidationControlHelper.ClearFields(AddressForValidation, this);
		}

		async void ValidateAddressButton_Click(object sender, EventArgs e)
		{
			ValidationJustForced = true;
			await ValidateAddress();
		}

		async Task ValidateAddress()
		{
			if (AddressValidationService.IsAddressNeedValidation(AddressForValidation))
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

				await AddressSuggestionControlHelper.ValidateAddressAsync(cancellationToken, AddressForValidation, this, ParentForm, SuggestionWindowParentControl.Controls, RefreshValidationStatus, null, 0, cleanseAction);
			}
		}

		void RefreshValidationStatus()
		{
			if (AddressForValidation != null)
			{
				AddressValidationUIHelper.SetButtonValidationStatus(ValidateAddressButton, AddressForValidation.ValidationStatus, AddressForValidation.IsErrorSuppressed);
				AddressValidationUIHelper.SetAddressFieldValidationStatus(Address1Control, Address2Control, CityControl, PostcodeControl, StateControl, CountryControl, AddressForValidation.ValidationStatus);
				ParentForm.Invalidate();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ISupportWebAddressValidationControlInitialise();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			var visible = true;
			if (Env.Security != null && Person != null && Person.IsInDatabase)
			{
				visible = Env.Security.PersonIntelligenceViewHomeAddress.IsAllowed;
			}

			ValidateAddressButton.Visible = visible;
			ClearFieldsButton.Visible = visible;
		}

		async void ISupportWebAddressValidationControlInitialise()
		{
			if (Env.Instance.Registry.EnableAddressValidationWebService && AddressForValidation != null)
			{
				Person.PreValidationForAddressValidationService();
				var topLevelZForm = this.TopLevelControl as ZForm;
				if (topLevelZForm != null && topLevelZForm.DisplayMode != ODisplayMode.ReadOnly && !string.IsNullOrEmpty(AddressForValidation.Address1) && AddressForValidation.ValidationStatus != AddressValidationStatus.ManuallyVerified && !AddressForValidation.Address1Info.ReadOnly)
				{
					using (Person.SuspendSettingHasChanges())
					{
						await ValidateAddress();
					}
				}

				RefreshValidationStatus();
			}
		}
	}
}
