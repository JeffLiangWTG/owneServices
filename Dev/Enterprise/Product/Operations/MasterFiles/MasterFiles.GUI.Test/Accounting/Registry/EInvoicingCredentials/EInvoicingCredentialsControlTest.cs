using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EInvoicingCredentialsControl))]
	sealed class EInvoicingCredentialsControlTest : RegistryZUserControlTestCase
	{
		#region Fields

		const string APIKeyTextBox = "APIKeyTextBox";
		const string CopyAPIKeyButton = "CopyAPIKeyButton";
		const string GenerateAPIKeyButton = "GenerateAPIKeyButton";

		IAPIKeyGeneratorStrategy APIKeyGeneratorStrategy;

		#endregion

		protected override IBusiness GetNewBusinessEntity() => new EInvoicingCredentials();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;

		public void TestViewButtonForClientSecretVisible()
		{
			void TestViewButtonVisible(bool viewButtonVisible)
			{
				var behavior = viewButtonVisible
					? EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar
					: EInvoicingCredentialsRegistryItem.Behavior.Default;

				using (var control = new EInvoicingCredentialsControl(behavior))
				{
					var viewButton = control.FindSingle<ZButton>("ViewButtonForClientSecret");
					AssertEquals("Visible viewButton", viewButtonVisible, viewButton.Visible);

					var clientSecretTextBox = control.FindSingle<ZTextBox>("ClientSecretTextBox");
					AssertEquals("Visible *", viewButtonVisible ? '*' : '\0', clientSecretTextBox.PasswordChar);
				}
			}

			TestViewButtonVisible(true);
			TestViewButtonVisible(false);

			var user = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			Factory.Save();
			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Guid.Empty, Guid.Empty))
			using (var control = new EInvoicingCredentialsControl(EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar))
			{
				var viewButton = control.FindSingle<ZButton>("ViewButtonForClientSecret");
				AssertEquals("Visible viewButton", false, viewButton.Visible);

				var clientSecretTextBox = control.FindSingle<ZTextBox>("ClientSecretTextBox");
				AssertEquals("Visible *", '*', clientSecretTextBox.PasswordChar);
			}
		}

		public void TestViewButtonClickActions()
		{
			using (var control = new DummyClientSecretControl())
			{
				var viewButton = control.FindSingle<ZButton>("ViewButtonForClientSecret");
				var clientSecretTextBox = control.FindSingle<ZTextBox>("ClientSecretTextBox");
				control.Show();
				clientSecretTextBox.Text = "ThisIsClientSecret";

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				viewButton.PerformClick();
				AssertNull("No message should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				control.LoginPassword = "IAmHH1";
				viewButton.PerformClick();
				AssertEquals("An error message should be shown.", "Incorrect Developer Password", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.LoginPassword = User.MasterPassword;
				viewButton.PerformClick();
				AssertEquals("The password should be displayed.", "ThisIsClientSecret", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEInvoicingCredentialsControl_WhenIncorrectParameters_ThrowsException()
		{
			var fallbackLevel = new FallbackLevel(companyPK: GlbCompany.CurrentCompany.PK.ToGuid(), branchPK: Guid.Empty, departmentPK: Guid.Empty);
			var wrongFallbackLevel = new FallbackLevel(companyPK: Guid.Empty, branchPK: Guid.Empty, departmentPK: Guid.Empty);

			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				using var control = new EInvoicingCredentialsControl(apiKeyGenerator: null, EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey, Factory, fallbackLevel);
			});

			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				using var control = new EInvoicingCredentialsControl(APIKeyGeneratorStrategy, EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey, factory: null, fallbackLevel);
			});

			AssertExceptionThrown<ArgumentException>("FallbackLevel", "To use API Key field, fallback level should be 'company'.", () =>
			{
				using var control = new EInvoicingCredentialsControl(APIKeyGeneratorStrategy, EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey, Factory, fallbackLevel: null);
			});

			AssertExceptionThrown<ArgumentException>("FallbackLevel", "To use API Key field, fallback level should be 'company'.", () =>
			{
				using var control = new EInvoicingCredentialsControl(APIKeyGeneratorStrategy, EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey, Factory, wrongFallbackLevel);
			});
		}

		public void TestAPIKeyControls_Visibility()
		{
			AssertControlIsVisible<ZButton>(EInvoicingCredentialsRegistryItem.Behavior.Default, CopyAPIKeyButton, isVisible: false);
			AssertControlIsVisible<ZButton>(EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar, CopyAPIKeyButton, isVisible: false);
			AssertControlIsVisible<ZButton>(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey, CopyAPIKeyButton, isVisible: true);
			AssertControlIsVisible<ZButton>(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey | EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar, CopyAPIKeyButton, isVisible: true);

			AssertControlIsVisible<ZButton>(EInvoicingCredentialsRegistryItem.Behavior.Default, GenerateAPIKeyButton, isVisible: false);
			AssertControlIsVisible<ZButton>(EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar, GenerateAPIKeyButton, isVisible: false);
			AssertControlIsVisible<ZButton>(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey, GenerateAPIKeyButton, isVisible: true);
			AssertControlIsVisible<ZButton>(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey | EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar, GenerateAPIKeyButton, isVisible: true);

			AssertControlIsVisible<ZTextBox>(EInvoicingCredentialsRegistryItem.Behavior.Default, APIKeyTextBox, isVisible: false);
			AssertControlIsVisible<ZTextBox>(EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar, APIKeyTextBox, isVisible: false);
			AssertControlIsVisible<ZTextBox>(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey, APIKeyTextBox, isVisible: true);
			AssertControlIsVisible<ZTextBox>(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey | EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar, APIKeyTextBox, isVisible: true);
		}

		public void TestAPIKeyButtons_IsEnabled()
		{
			using (var eInvoicingCredentialsControl = CreateEInvoicingCredentialsControl(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey))
			{
				eInvoicingCredentialsControl.ReadOnly = true;
				AssertControlIsEnabled<ZButton>(eInvoicingCredentialsControl, CopyAPIKeyButton, isEnabled: true);
				AssertControlIsEnabled<ZButton>(eInvoicingCredentialsControl, GenerateAPIKeyButton, isEnabled: false);

				eInvoicingCredentialsControl.ReadOnly = false;
				AssertControlIsEnabled<ZButton>(eInvoicingCredentialsControl, CopyAPIKeyButton, isEnabled: true);
				AssertControlIsEnabled<ZButton>(eInvoicingCredentialsControl, GenerateAPIKeyButton, isEnabled: true);
			}
		}

		public void TestGenerateApiKeyButton_ShowsConfirmationMessage()
		{
			using (var form = new ZChildForm())
			using (var eInvoicingCredentialsControl = CreateEInvoicingCredentialsControl(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey))
			{
				form.Controls.Add(eInvoicingCredentialsControl);
				form.Show();

				var generateApiKeyButton = eInvoicingCredentialsControl.FindSingle<ZButton>(GenerateAPIKeyButton);
				eInvoicingCredentialsControl.ReadOnly = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				generateApiKeyButton.PerformClick();
				generateApiKeyButton.PerformClick();
				AssertEquals("When you generate a new API Key, you must also update it on the ETA e-Invoicing portal. Otherwise, the most recent status of the e-invoices will not be obtained. Would you like to continue?",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCopyAPIKeyButtonButton_ShowsInformationMessage()
		{
			using (var form = new ZChildForm())
			using (var eInvoicingCredentialsControl = CreateEInvoicingCredentialsControl(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey))
			{
				form.Controls.Add(eInvoicingCredentialsControl);
				form.Show();

				var generateApiKeyButton = eInvoicingCredentialsControl.FindSingle<ZButton>(GenerateAPIKeyButton);
				var copyApiKeyButton = eInvoicingCredentialsControl.FindSingle<ZButton>(CopyAPIKeyButton);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				eInvoicingCredentialsControl.BoundBusinessObject.HasChanges = true;

				copyApiKeyButton.PerformClick();
				AssertEquals("To copy the API Key, please save the registry.", UnitTestUserNotification.Instance.LastMessage.Text);

				eInvoicingCredentialsControl.BoundBusinessObject.HasChanges = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				copyApiKeyButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCopyAPIKeyButtonButton_SetsClipboard()
		{
			using (var form = new ZChildForm())
			using (var eInvoicingCredentialsControl = CreateEInvoicingCredentialsControl(EInvoicingCredentialsRegistryItem.Behavior.HasAPIKey))
			{
				form.Controls.Add(eInvoicingCredentialsControl);
				form.Show();

				var copyApiKeyButton = eInvoicingCredentialsControl.FindSingle<ZButton>(CopyAPIKeyButton);
				var apiKeyTextBox = eInvoicingCredentialsControl.FindSingle<ZTextBox>(APIKeyTextBox);

				var expectedApiKey = apiKeyTextBox.Text;
				eInvoicingCredentialsControl.ReadOnly = true;
				eInvoicingCredentialsControl.ClipboardValue = null;

				copyApiKeyButton.PerformClick();

				AssertEquals(expectedApiKey, eInvoicingCredentialsControl.ClipboardValue);
			}
		}

		void AssertControlIsEnabled<T>(EInvoicingCredentialsControl eInvoicingCredentialsControl, string controlName, bool isEnabled) where T : Control
		{
			var control = eInvoicingCredentialsControl.FindSingle<T>(controlName);

			AssertEquals($"Enabled {controlName}", isEnabled, control.Enabled);
		}

		void AssertControlIsVisible<T>(EInvoicingCredentialsRegistryItem.Behavior behavior, string controlName, bool isVisible) where T : Control
		{
			using (var eInvoicingCredentialsControl = CreateEInvoicingCredentialsControl(behavior))
			{
				var control = eInvoicingCredentialsControl.FindSingle<T>(controlName);

				AssertEquals($"Visible {controlName}", isVisible, control.Visible);
			}
		}

		EInvoicingCredentialsControlForTest CreateEInvoicingCredentialsControl(EInvoicingCredentialsRegistryItem.Behavior behavior)
		{
			var eInvoicingCredentialsControl = new EInvoicingCredentialsControlForTest(APIKeyGeneratorStrategy, behavior, Factory,
				new FallbackLevel(companyPK: GlbCompany.CurrentCompany.PK.ToGuid(), branchPK: Guid.Empty, departmentPK: Guid.Empty), UnitTestUserNotification.Instance);

			eInvoicingCredentialsControl.SetDataBinding(new EInvoicingCredentials()
			{
				APIKey = "dummy-api-key"
			}, dataMember: "");
			eInvoicingCredentialsControl.Dock = DockStyle.Fill;

			return eInvoicingCredentialsControl;
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (APIKeyGeneratorStrategy == null)
			{
				var apiKeyGeneratorStrategyMock = new Mock<IAPIKeyGeneratorStrategy>();
				apiKeyGeneratorStrategyMock
					.Setup(x => x.GenerateAPIKey(It.IsAny<GlbCompany>()))
					.Returns("1234567890");

				APIKeyGeneratorStrategy = apiKeyGeneratorStrategyMock.Object;
			}
		}

		class EInvoicingCredentialsControlForTest : EInvoicingCredentialsControl
		{
			public string ClipboardValue { get; set; }

			public EInvoicingCredentialsControlForTest(IAPIKeyGeneratorStrategy apiKeyGenerator,
				EInvoicingCredentialsRegistryItem.Behavior behavior,
				BusinessObjectFactory factory,
				FallbackLevel fallbackLevel,
				IUserNotification userNotification = null) : base(apiKeyGenerator, behavior, factory, fallbackLevel, userNotification)
			{
			}

			protected override void SafeClipboardSetText(string value)
			{
				ClipboardValue = value;
			}
		}
	}
}
