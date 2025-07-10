using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class AddressSuggestionControlUtilTest : TestCaseWithFactory
	{
		public void TestValidateAddress()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddressForValidationIsNull()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				parentControl.AddressForValidation.IsValidatingAddress = true;

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				AssertEquals(false, suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestIsValidatingAddress()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest())
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				AssertEquals(false, suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuickValidate()
		{
			AssertQuickValidateCore(AddressValidationStatus.Verified, true);
			AssertQuickValidateCore(AddressValidationStatus.VerifiedToStreet, true);
			AssertQuickValidateCore(AddressValidationStatus.ManuallyVerified, false);
			AssertQuickValidateCore(AddressValidationStatus.Unverifiable, false);
			AssertQuickValidateCore(AddressValidationStatus.ToBeVerified, false);
			AssertQuickValidateCore(AddressValidationStatus.Invalid, false);
			AssertQuickValidateCore(AddressValidationStatus.CountryNotAvailable, false);
			AssertQuickValidateCore(AddressValidationStatus.ExcludeBackgroundValidation, false);
			AssertQuickValidateCore(AddressValidationStatus.NotRequired, false);
		}

		public void TestQuickValidate_RowDeletedOrDetachedOrNull()
		{
			var mock = new Mock<NonPersistentBusinessObject>();
			var mockedAddress = mock.As<ISupportWebAddressValidation>().Object;

			using (var parentControl = new ControlSupportAddressValidationForTest(mockedAddress))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);
				suggestionControlUtil.AdditionalValidateAction = (address, result) => address.ValidationStatus = AddressValidationStatus.Verified;

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.QuickValidate);

				AssertEquals(true, suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestValidateAndSuggestDifferentAddresses()
		{
			var address = GetNewAddressForTesting();

			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				suggestionControlUtil.CloseSuggestionControlIfExists();

				address.Address1 = "Another Address 1";
				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowValidationResultFromCache()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				suggestionControlUtil.CloseSuggestionControlIfExists();
				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				AssertEquals(false, suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				AssertEquals(false, suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddressHasPreValidationError()
		{
			var expectedMessage = "Please fix errors on address before running validation.";

			AssertAddressHasPreValidationErrorCore(CleanseAction.ValidateAndSuggest, isFirstLoad: false, expectedMessage);
			AssertAddressHasPreValidationErrorCore(CleanseAction.QuickValidate, isFirstLoad: false, expectedMessage);
			AssertAddressHasPreValidationErrorCore(CleanseAction.ValidateAndSuggest, isFirstLoad: true, null);
			AssertAddressHasPreValidationErrorCore(CleanseAction.QuickValidate, isFirstLoad: true, null);

			Factory.Saving += (factory) =>
			{
				AssertAddressHasPreValidationErrorCore(CleanseAction.ValidateAndSuggest, isFirstLoad: false, null);
				AssertAddressHasPreValidationErrorCore(CleanseAction.QuickValidate, isFirstLoad: false, null);
				AssertAddressHasPreValidationErrorCore(CleanseAction.ValidateAndSuggest, isFirstLoad: true, null);
				AssertAddressHasPreValidationErrorCore(CleanseAction.QuickValidate, isFirstLoad: true, null);
			};
			Factory.Save();
		}

		public void TestShowValidationMessage()
		{
			AssertShowValidationMessageCore("This message should be shown.", (address, result) => result.Message = "This message should be shown.");
			AssertShowValidationMessageCore("This error message should be shown.", (address, result) => result.ResultAddress.ErrorMessage = "This error message should be shown.");
			AssertShowValidationMessageCore("This result message should be shown.", (address, result) =>
			{
				result.Message = "This result message should be shown.";
				result.ResultAddress.ErrorMessage = "This error message should not be shown.";
			});
		}

		public void TestShowValidationResult()
		{
			AssertShowValidationResultCore(AddressValidationStatus.Invalid, true);
			AssertShowValidationResultCore(AddressValidationStatus.Verified, false);
			AssertShowValidationResultCore(AddressValidationStatus.VerifiedToStreet, false);
			AssertShowValidationResultCore(AddressValidationStatus.ManuallyVerified, false);
			AssertShowValidationResultCore(AddressValidationStatus.Unverifiable, false);
			AssertShowValidationResultCore(AddressValidationStatus.ToBeVerified, false);
			AssertShowValidationResultCore(AddressValidationStatus.CountryNotAvailable, false);
			AssertShowValidationResultCore(AddressValidationStatus.ExcludeBackgroundValidation, false);
			AssertShowValidationResultCore(AddressValidationStatus.NotRequired, false);
		}

		public void TestDoNotShowValidationResult_RowDeletedOrDetachedOrNull()
		{
			var mock = new Mock<NonPersistentBusinessObject>();
			var mockedAddress = mock.As<ISupportWebAddressValidation>().Object;

			using (var parentControl = new ControlSupportAddressValidationForTest(mockedAddress))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCloseSuggestionControlBeforeCreateAndShow()
		{
			var address = GetNewAddressForTesting();

			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var preControl = suggestionControlUtil.FindSuggestionControl();

				address.Address1 = "Another Address 1";
				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNoExceptionThrown(() => suggestionControlUtil.FindSuggestionControl());
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertNotEquals(preControl, suggestionControlUtil.FindSuggestionControl());

				preControl?.Dispose();
			}
		}

		public void TestDoNotShowSuggestionControl_ParentControlIsDisposed()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);
				parentControl.Dispose();

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

#if !WINZOR // WI00914184 - [BetterListView] CS - ListView-SelectedItems refactor
		public void TestSuggestionControlAddressSelected()
		{
			var addressSelected = false;
			var config = new AddressValidatorConfiguration().WithAddressSelectedAction(() => addressSelected = true);

			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var suggestionControl = suggestionControlUtil.FindSuggestionControl();
				suggestionControl.SelectAddressAndClose();

				Assert(addressSelected);

				suggestionControl?.Dispose();
			}
		}
#endif

		public void TestSuggestionControlDestroyedAction()
		{
			var destroyedActionInvoked = false;
			var config = new AddressValidatorConfiguration().WithHandleDestroyedAction(() => destroyedActionInvoked = true);

			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);
				parentForm.Show();
				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				suggestionControlUtil.CloseSuggestionControlIfExists();

				Assert(destroyedActionInvoked);
			}
		}

		public void TestRegisterPropertyChangedEvent_AddressIsNull()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest())
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				AssertNoExceptionThrown(() => suggestionControlUtil.RegisterPropertyChangedEvent(() => Task.CompletedTask));
			}
		}

		public void TestRegisterPropertyChangedEvent()
		{
			var handlerClearedBeforeRegistering = false;
			var registeredTaskCalled = false;

			var mock = new Mock<ISupportWebAddressValidation>();
			mock.Setup(m => m.ClearWebAddressValidationHandler()).Callback(() => handlerClearedBeforeRegistering = true);
			var address = mock.Object;

			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				suggestionControlUtil.RegisterPropertyChangedEvent(() =>
				{
					registeredTaskCalled = true;
					return Task.CompletedTask;
				});

				Assert(handlerClearedBeforeRegistering);

				mock.Setup(m => m.NeedValidation).Returns(true);
				mock.Raise(m => m.TriggerWebAddressValidation += null, mock.Object, EventArgs.Empty);

				Assert(registeredTaskCalled);

				registeredTaskCalled = false;
				mock.Setup(m => m.NeedValidation).Returns(false);
				mock.Raise(m => m.TriggerWebAddressValidation += null, mock.Object, EventArgs.Empty);

				AssertEquals(false, registeredTaskCalled);
			}
		}

		public void TestEventHandlerBeenUnregisteredAfterDisposed()
		{
			var config = new AddressValidatorConfiguration().WithAddressSelectedAction(() => { }).WithHandleDestroyedAction(() => { });

			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config))
			{
				parentForm.Controls.Add(parentControl);
				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				var control = suggestionControlUtil.FindSuggestionControl();

				var propertyInfo = typeof(Control).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
				var eventHandlerList = propertyInfo.GetValue(control, Array.Empty<object>()) as EventHandlerList;
				var fieldInfo = typeof(Control).GetField("Event" + "HandleDestroyed", BindingFlags.NonPublic | BindingFlags.Static);
				var handleDestroyedKey = fieldInfo.GetValue(control);
				var handleDestroyed = eventHandlerList[handleDestroyedKey];

				var addressSelectedFieldInfo = typeof(AddressSuggestionControl).GetField("AddressSelected", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
				var addressSelected = addressSelectedFieldInfo.GetValue(control);

				var addressSelectedMethods = (addressSelected as Delegate).GetInvocationList().Select(x => x.Method.Name).ToArray();
				AssertEquals(2, addressSelectedMethods.Length);
				Assert(addressSelectedMethods.Any(x => x.Equals("HandleAddressSelected")));
				Assert(addressSelectedMethods.Any(x => x.StartsWith("<CreateNewSuggestionControl>")));
				var handleDestroyedMethods = handleDestroyed.GetInvocationList().Select(x => x.Method.Name).ToArray();
				AssertEquals(1, handleDestroyedMethods.Length);
				Assert(handleDestroyedMethods.Any(x => x.StartsWith("<CreateNewSuggestionControl>")));

				control.Dispose();
				addressSelected = addressSelectedFieldInfo.GetValue(control);
				handleDestroyed = eventHandlerList[handleDestroyedKey];
				AssertNull(addressSelected);
				AssertNull(handleDestroyed);
			}
		}

		#region Implementation

		void AssertQuickValidateCore(string validationStatus, bool expectedResult)
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);
				suggestionControlUtil.AdditionalValidateAction = (address, result) => address.ValidationStatus = validationStatus;

				suggestionControlUtil.ValidateAddressAsync(CleanseAction.QuickValidate);

				AssertEquals(true, suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(expectedResult, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertAddressHasPreValidationErrorCore(CleanseAction cleanseAction, bool isFirstLoad, string expectedMessage)
		{
			var config = new AddressValidatorConfiguration();
			if (isFirstLoad)
			{
				config.WithCheckIfControlFirstLoad(() => true);
			}

			using (var parentControl = new ControlSupportAddressValidationForTest(Factory.NewWithValidTestData<OrgAddress>()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.ValidateAddressAsync(cleanseAction);

				AssertEquals(false, suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertShowValidationMessageCore(string expectedMessage, Action<ISupportWebAddressValidation, WebAddressValidationResult> validationAction)
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.AdditionalValidateAction = validationAction;
				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertShowValidationResultCore(string validationStatus, bool expectedResult)
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(GetNewAddressForTesting()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new AddressSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			using (MonitorUserNotification)
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.AdditionalValidateAction = (address, result) => address.ValidationStatus = validationStatus;
				suggestionControlUtil.ValidateAddressAsync(CleanseAction.ValidateAndSuggest);

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertEquals(expectedResult, suggestionControlUtil.FindSuggestionControl() != null);
				AssertEquals(false, suggestionControlUtil.ClosestPortHasBeenSet);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		ISupportWebAddressValidation GetNewAddressForTesting()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>() as ISupportWebAddressValidation;

			address.Address1 = "UNIT 2 4 LEE AVENUE";
			address.CountryCodeISO2 = "AU";
			address.StateCode = "NSW";
			address.Postcode = "2627";

			return address;
		}

		DisposableAction MonitorUserNotification { get; } = new DisposableAction(UnitTestUserNotification.Instance.ClearMessages, UnitTestUserNotification.Instance.ClearMessages);

		#endregion Implementation
	}

	class AddressSuggestionControlUtilForTest : AddressSuggestionControlUtil, IDisposable
	{
		public AddressSuggestionControlUtilForTest(Control parentControl, Form parentForm, CancellationTokenSource cancellationTokenSource, AddressValidatorConfiguration config)
			: base(parentControl, parentForm, cancellationTokenSource, config)
		{
		}

		#region Validation

		public new void ValidateAddressAsync(CleanseAction cleanseAction)
		{
			ServiceHasBeenRequested = false;
			ClosestPortHasBeenSet = false;

			AsyncTaskSynchronizer.Run(async () => await base.ValidateAddressAsync(cleanseAction));
		}

		protected override async Task<WebAddressValidationResult> ValidateAddressCore(CleanseAction cleanseAction)
		{
			ServiceHasBeenRequested = true;

			using (Env.Registry.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			using (SetFakeWebService())
			{
				var result = await base.ValidateAddressCore(cleanseAction);
				AdditionalValidateAction?.Invoke(validationControl.AddressForValidation, result);
				return result;
			}
		}

		protected override void SetClosestPortCore() => ClosestPortHasBeenSet = true;

		#endregion Validation

		#region Properties

		public Action<ISupportWebAddressValidation, WebAddressValidationResult> AdditionalValidateAction { private get; set; }

		public bool ServiceHasBeenRequested { get; private set; }

		public bool ClosestPortHasBeenSet { get; private set; }

		#endregion Properties

		#region Implementaion

		public new void CloseSuggestionControlIfExists()
		{
			var suggestionControl = FindSuggestionControl();
			suggestionControl?.Close();
			suggestionControl?.Dispose();
		}

		public void Dispose() => CloseSuggestionControlIfExists();

		DisposableAction SetFakeWebService()
		{
			var serviceAddress = string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort());
			var serviceUri = new Uri(serviceAddress + AddressValidationService.Constants.ValidationServiceName + "/");
			var validationServiceForTest = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "POST" },
				Processor = (_, request) => new Tuple<int, string>(200, @"
					{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""02292eb7-db6c-40e2-a1e7-f63f9de5ca7c"",""AddressSourceTable"":""OA"",""Addressee"":null,""Address1"":""4 LEE AVENUE"",""Address2"":"""",""City"":""JINDABYNE"",""Locality"":null,""County"":""SNOWY MONARO REGIONAL"",""State"":""NSW"",""Postcode"":""2627"",""PostcodeAddOn"":null,""PostcodeBase"":""2627"",""Country"":""AU"",""CompanyName"":null,""Apartment"":null,""UnparsedAddressInformation"":"""",""UnmatchedApartmentPrefix"":""UNIT 2"",""UnmatchedApartmentSuffix"":null,""StreetNumber"":""4"",""Street"":""LEE AVENUE"",""Latitude"":-36.435914441157529,""Longitude"":148.60532438287689,""MatchCode"":""S5HPNTSCZAS"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":1,""ResultStatusCode"":""PCL"",""ServiceType"":1,""QueryType"":1,""AvailableData"":4,""AddressType"":8,""ErrorMessage"":null,""Group"":null,""HouseNumberInput"":""4"",""StreetPrefixInput"":"""",""StreetNameInput"":""LEE"",""StreetSuffixInput"":""AVENUE"",""PostalCodeBaseInput"":""2627"",""PostalCodeAddOnInput"":"""",""CityInput"":""JINDABYNE"",""StateProvinceInput"":""NSW"",""LandParcelPolygon"":null},""ProviderServiceCalls"":[],""Suggestions"":[],""DiagnosticMessage"":null,""ProviderCodeUsed"":""PBO"",""ValidateStartDateTimeUTC"":""2023-02-16T01:57:23.7350176Z"",""ValidateFinishDateTimeUTC"":""2023-02-16T01:57:23.7662663Z"",""SuggestStartDateTimeUTC"":""0001-01-01T00:00:00"",""SuggestFinishDateTimeUTC"":""0001-01-01T00:00:00""}],""DiagnosticMessage"":null}"),
				Uri = serviceUri,
				ContentType = "application/json"
			};

			return new DisposableAction(() =>
			{
				AddressValidationService.SetAvailableWebServiceAddress(serviceAddress);
				validationServiceForTest.Start();
			},
			() =>
			{
				if (validationServiceForTest.IsStarted)
				{
					validationServiceForTest.Stop();
				}
				AddressValidationService.SetAvailableWebServiceAddress();
			});
		}

		#endregion Implementaion
	}
}
