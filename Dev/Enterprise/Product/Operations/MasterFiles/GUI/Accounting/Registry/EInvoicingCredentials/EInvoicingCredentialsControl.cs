using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EInvoicingCredentialsControl : RegistryZUserControl
	{
		readonly IUserNotification userNotification;
		readonly IAPIKeyGeneratorStrategy apiKeyGenerator;
		readonly BusinessObjectFactory factory;
		readonly FallbackLevel fallbackLevel;

		GlbCompany FallbackCompany
		{
			get
			{
				if (fallbackCompany != null)
				{
					return fallbackCompany;
				}

				var companyPK = fallbackLevel.CompanyPK(returnEmptyIfBranchPKIsPresent: false);
				fallbackCompany = factory.Load<GlbCompany>(companyPK);

				return fallbackCompany;
			}
		}
		GlbCompany fallbackCompany;

		public EInvoicingCredentialsControl()
		{
			InitializeComponent();
			this.AfterFirstBinding += EInvoicingCredentialsControl_AfterFirstBinding;
		}
		public EInvoicingCredentialsControl(EInvoicingCredentialsRegistryItem.Behavior behavior)
			: this(apiKeyGenerator: null, behavior, factory: null, fallbackLevel: null)
		{
		}

		public EInvoicingCredentialsControl(IAPIKeyGeneratorStrategy apiKeyGenerator,
			EInvoicingCredentialsRegistryItem.Behavior behavior,
			BusinessObjectFactory factory,
			FallbackLevel fallbackLevel,
			IUserNotification userNotification = null) : this()
		{
			ValidateParameters(apiKeyGenerator, behavior, factory, fallbackLevel);

			this.apiKeyGenerator = apiKeyGenerator;
			this.factory = factory;
			this.fallbackLevel = fallbackLevel;
			this.userNotification = userNotification ?? Globals.Message;

			ArrangeClientSecretControls(behavior);
			ArrangeAPIKeyControls(behavior);
		}

		void EInvoicingCredentialsControl_AfterFirstBinding(object sender, EventArgs e)
		{
			var eInvoicingCredentials = BoundBusinessObject as EInvoicingCredentials;
			if (eInvoicingCredentials != null && eInvoicingCredentials.HasChanges)
			{
				eInvoicingCredentials.HasChanges = false;
			}
		}

		void ValidateParameters(IAPIKeyGeneratorStrategy apiKeyGenerator, EInvoicingCredentialsRegistryItem.Behavior behavior, BusinessObjectFactory factory, FallbackLevel fallbackLevel)
		{
			if (behavior.HasAPIKeyFlag())
			{
				try
				{
					if (apiKeyGenerator == null)
					{
						throw new ArgumentNullException(nameof(apiKeyGenerator));
					}

					if (factory == null)
					{
						throw new ArgumentNullException(nameof(factory));
					}

					if (fallbackLevel == null || fallbackLevel.Level != Enterprise.Integration.RegistryStorageFlags.Company)
					{
						throw new ArgumentException(Res.GetString("3f7e2d72-6984-4e5f-8b84-58ded4f3e7c7", "To use API Key field, fallback level should be 'company'."));
					}
				}
				catch
				{
					Dispose(); // to avoid memory leaks first dispose then throw the original exception.
					throw;
				}
			}
		}

		void ArrangeClientSecretControls(EInvoicingCredentialsRegistryItem.Behavior behavior)
		{
			if (behavior.SetPasswordCharFlag())
			{
				ClientSecretTextBox.PasswordChar = '*';
				ViewButtonForClientSecret.Visible = GlbStaff.CurrentUser.GS_IsDeveloper || GlbStaff.CurrentUser.GS_IsController;
			}
		}

		void ArrangeAPIKeyControls(EInvoicingCredentialsRegistryItem.Behavior behavior)
		{
			if (!behavior.HasAPIKeyFlag())
			{
				HideAPIKeyControls();
			}
			else
			{
				ArrangeVerticalPositionOfAPIKeyControls();
			}
		}

		void HideAPIKeyControls()
		{
			ApiKeyTextBox.Visible = false;
			CopyApiKeyButton.Visible = false;
			GenerateAPIKeyButton.Visible = false;
		}

		void ArrangeVerticalPositionOfAPIKeyControls()
		{
			if (!ViewButtonForClientSecret.Visible)
			{
				var emptySpaceLength = ApiKeyTextBox.Top - ViewButtonForClientSecret.Top;
				ApiKeyTextBox.Top -= emptySpaceLength;
				GenerateAPIKeyButton.Top -= emptySpaceLength;
				CopyApiKeyButton.Top -= emptySpaceLength;
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			ClientIdTextBox.ReadOnly = readOnly;
			ClientSecretTextBox.ReadOnly = readOnly;
			GenerateAPIKeyButton.Enabled = !readOnly;
		}

		void ViewButton_Click(object sender, EventArgs e)
		{
			ViewPassword();
		}

		void ViewPassword()
		{
			var user = EnvProxy.Instance.CurrentUser;
			if (user.IsDeveloper && !String.IsNullOrEmpty(ClientSecretTextBox.Text))
			{
				using (DeveloperLoginForm loginForm = new DeveloperLoginForm())
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(loginForm) == DialogResult.OK)
					{
						if (IsValidPassword(loginForm))
						{
							Globals.Message.ShowInformation(ClientSecretTextBox.Text, Res.GetString("53e2e5a7-9db3-4f1a-897f-4b370999a8b2", "Password"));
						}
						else
						{
							loginForm.ShowIncorrectPasswordMessage();
						}
					}
				}
			}
			else if (user.IsController && !String.IsNullOrEmpty(ClientSecretTextBox.Text))
			{
				Globals.Message.ShowInformation(ClientSecretTextBox.Text, Res.GetString("53e2e5a7-9db3-4f1a-897f-4b370999a8b2", "Password"));
			}
		}

#if DEBUG
		protected virtual
#endif
		bool IsValidPassword(DeveloperLoginForm loginForm)
		{
			return loginForm.IsValidPassword;
		}

		void GenerateAPIKeyButton_Click(object sender, EventArgs e)
		{
			if (!IsUserConfirmationNeeded || IsUserConfirmedToOverwriteAPIKey())
			{
				var eInvoicingCredentials = (EInvoicingCredentials)this.DataSource;
				eInvoicingCredentials.APIKey = apiKeyGenerator.GenerateAPIKey(FallbackCompany);
			}
		}

		bool IsUserConfirmationNeeded =>
			!ApiKeyTextBox.Text.IsNullOrEmpty();

		bool IsUserConfirmedToOverwriteAPIKey()
		{
			var confirmationResult = userNotification.ShowConfirmation(
				Res.GetString("4ffb740b-a116-443b-abb2-243db8681025", @"When you generate a new API Key, you must also update it on the ETA e-Invoicing portal. Otherwise, the most recent status of the e-invoices will not be obtained. Would you like to continue?"),
				Res.GetString("621c719c-5cd6-4e5c-8e50-c6f6451c5ac0", "API Key Overwrite Confirmation"),
				Res.GetString("b7020797-5f97-49ae-b3b0-811b5795e878", "Yes"),
				MessageBoxIcon.Question);

			return confirmationResult == DialogResult.OK;
		}

		void CopyAPIKeyButton_Click(object sender, EventArgs e)
		{
			if ((BoundBusinessObject as EInvoicingCredentials)?.HasChanges ?? false)
			{
				userNotification.ShowInformation(Res.GetString("b0ce1415-193d-499d-88b6-fcc2059444f6", "To copy the API Key, please save the registry."));
				return;
			}

			var apiKey = ApiKeyTextBox.Text;
			if (!string.IsNullOrEmpty(apiKey))
			{
				SafeClipboardSetText(apiKey);
			}
		}

		/// <summary>
		/// This method is to make clipboard operation testable.
		/// </summary>
		protected virtual void SafeClipboardSetText(string value)
			=> SafeClipboard.SetText(value);
	}
}
