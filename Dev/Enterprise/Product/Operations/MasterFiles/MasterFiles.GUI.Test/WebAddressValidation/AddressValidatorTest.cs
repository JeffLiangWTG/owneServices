using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class AddressValidatorTest : TestCaseWithFactory
	{
		public void TestDefaultConfig()
		{
			using (var form = new Form())
			using (var parentControl = new ZDocAddressControl())
			{
				form.Controls.Add(parentControl);

				var addressValidator = new AddressValidatorForTest(parentControl);

				AssertEquals(0, addressValidator.ConfigExposed.MaxHeightGetter.Invoke());
			}
		}

		public void TestCustomConfig()
		{
			var config = new AddressValidatorConfiguration()
				.WithCustomSuggestionWindowMaxHeightGetter(() => 1);

			using (var form = new Form())
			using (var parentControl = new ZDocAddressControl())
			{
				form.Controls.Add(parentControl);

				var addressValidator = new AddressValidatorForTest(parentControl, config);

				AssertEquals(1, addressValidator.ConfigExposed.MaxHeightGetter.Invoke());
			}
		}

		public void TestThrowExceptionWhenParentControlIsNotISupportWebAddressValidationControl()
		{
			AssertExceptionThrown(typeof(ArgumentException), () =>
			{
				new AddressValidatorForTest(new Control());
			});
		}

		public void TestGetParentForm()
		{
			using (var temporaryOrganisationsPopup = new TemporaryOrganisationsPopup(new OrgHeaderCollection(Factory), Factory.New<OrgHeader>()))
			{
				AssertGetParentForm(temporaryOrganisationsPopup, temporaryOrganisationsPopup);
			}

			using (var glbStaffForm = new GlbStaffForm(Factory.New<GlbStaff>()))
			{
				AssertGetParentForm(glbStaffForm, glbStaffForm);
			}

			using (var form1 = new Form())
			using (var zDocAddressControl = new ZDocAddressControl())
			{
				form1.Controls.Add(zDocAddressControl);
				AssertGetParentForm(form1, zDocAddressControl);
			}

			using (var form2 = new Form())
			using (var addressesUserControl = new AddressesUserControl())
			{
				form2.Controls.Add(addressesUserControl);
				AssertGetParentForm(form2, addressesUserControl);
			}
		}

		public void TestNoValidationRequired()
		{
			var validated = false;

			var config = new AddressValidatorConfiguration()
				.WithCustomValidateAddress(GetCustomValidateAddress);

			using (var form = new Form())
			using (var parentControl = new ControlSupportAddressValidationForTest())
			{
				form.Controls.Add(parentControl);

				var addressValidator = new AddressValidatorForTest(parentControl, config);

				parentControl.ValidateButton.PerformClick();

				AssertEquals(false, validated);
			}

			Func<Task> GetCustomValidateAddress(Func<Task> core) => async () =>
			{
				validated = true;
				await Task.Delay(1);
			};
		}

		public void TestControlResize()
		{
			var addressItems = new List<ValidationResultItem> { new ValidationResultItem { Address1 = "Add01", Address2 = "Add02", City = "City0", Country = "AU", Group = "", Postcode = "P1", State = "NSW" } };

			var address = Factory.New<OrgAddress>();

			var config = new AddressValidatorConfiguration()
				.WithCustomSuggestionWindowMaxHeightGetter(() => 600);

			var locationUpdated = false;

			using (var form = new ZForm())
			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var suggestionControl = new AddressSuggestionControl(address, addressItems, addressItems.First(), form, parentControl, (parentControl as ISupportWebAddressValidationControl).ValidateButton))
			{
				parentControl.SuggestionWindowParentControl.Controls.Add(suggestionControl);
				form.Controls.Add(parentControl);
				form.Size = new Size(1500, 1500);
				form.Show();

				suggestionControl.LocationChanged += (object sender, EventArgs e) => locationUpdated = true;

				var addressValidator = new AddressValidator(parentControl, config);

				suggestionControl.Hide();
				parentControl.Size = new Size(1000, 1000);
				AssertEquals(false, locationUpdated);
				AssertEquals(false, suggestionControl.Visible);

				suggestionControl.Show();
				parentControl.Size = new Size(800, 900);
				Assert(locationUpdated);
				Assert(suggestionControl.Visible);
			}
		}

		public void TestShowAddressSuggestionControlWhenControlGotFocus()
		{
			AssertSuggestionControl(GetAllControls, (parentControl, control) => AssertShowSuggestionControlWithStatuses(
				expectedAddressSuggestionControlVisible: true,
				expectedCityTownSuggestionControlVisible: false,
				parentControl,
				control,
				UnverifiedStatuses));
		}

		public void TestShowCityTownSuggestionControlWhenControlGotFocus_AddressSuggestionControlInvisible()
		{
			using (new DisposableAction(() => ForceAddressSuggestionControlShowFailedForTest = false))
			{
				ForceAddressSuggestionControlShowFailedForTest = true;

				AssertSuggestionControl(GetCityTownControls, (parentControl, control) => AssertShowSuggestionControlWithStatuses(
					expectedAddressSuggestionControlVisible: false,
					expectedCityTownSuggestionControlVisible: true,
					parentControl,
					control,
					UnverifiedStatuses));
			}
		}

		public void TestDoNotShowSuggestionControlWhenControlsGotFocus_AddressIsNull()
		{
			using (new DisposableAction(() => UseNullAsAddressForTest = false))
			{
				UseNullAsAddressForTest = true;

				AssertSuggestionControl(GetAllControls, (parentControl, control) => AssertShowSuggestionControlCore(
					expectedAddressSuggestionControlVisible: false,
					expectedCityTownSuggestionControlVisible: false,
					parentControl,
					control));
			}
		}

		public void TestDoNotShowSuggestionControlWhenControlsGotFocus_AddressVerified()
		{
			AssertSuggestionControl(GetAllControls, (parentControl, control) => AssertShowSuggestionControlWithStatuses(
				expectedAddressSuggestionControlVisible: false,
				expectedCityTownSuggestionControlVisible: false,
				parentControl,
				control,
				VerifiedStatuses));
		}

		public void TestDoNotShowCityTownSuggestionControlWhenControlsGotFocus_AddressControlsFocused()
		{
			using (new DisposableAction(() => ForceAddressSuggestionControlShowFailedForTest = false))
			{
				ForceAddressSuggestionControlShowFailedForTest = true;

				AssertSuggestionControl(GetAddressControls, (parentControl, control) => AssertShowSuggestionControlWithStatuses(
					expectedAddressSuggestionControlVisible: false,
					expectedCityTownSuggestionControlVisible: false,
					parentControl,
					control,
					UnverifiedStatuses));
			}
		}

		[RequiresSTA]
		public void TestHideSuggestionControlsWhenControlLostFocus()
		{
			AssertSuggestionControl((control) => new Control[] { null }, (parentControl, control) => AssertHideSuggestionControlCore(
				expectedAddressSuggestionControlVisible: false,
				expectedCityTownSuggestionControlVisible: false,
				parentControl,
				control));
		}

		public void TestDoNotHideSuggestionControlWhenControlLostFocus_FocusCityTownFields()
		{
			AssertSuggestionControl(GetCityTownControls, (parentControl, control) => AssertHideSuggestionControlCore(
				expectedAddressSuggestionControlVisible: true,
				expectedCityTownSuggestionControlVisible: true,
				parentControl,
				control));
		}

		public void TestDoNotHideCityTownSuggestionControlWhenControlLostFocus_MouseWithinBoundsOfCityTownControls()
		{
			using (new DisposableAction(() => MoveMouseForSuggestionControl = null))
			{
				MoveMouseForSuggestionControl = (parentControl) => MoveMouseIntoSuggestionControl(parentControl, GetCityTownSuggestionControl(parentControl));

				AssertSuggestionControl((control) => new Control[] { null }, (parentControl, control) => AssertHideSuggestionControlCore(
					expectedAddressSuggestionControlVisible: false,
					expectedCityTownSuggestionControlVisible: true,
					parentControl,
					control));
			}
		}

		public void TestDoNotHideAddressSuggestionControlWhenControlLostFocus_FocusAddressFields()
		{
			AssertSuggestionControl(GetAddressControls, (parentControl, control) => AssertHideSuggestionControlCore(
				expectedAddressSuggestionControlVisible: true,
				expectedCityTownSuggestionControlVisible: false,
				parentControl,
				control));
		}

		public void TestDoNotHideAddressSuggestionControlWhenControlLostFocus_MouseWithinBoundsOfAddressControls()
		{
			using (new DisposableAction(() => MoveMouseForSuggestionControl = null))
			{
				MoveMouseForSuggestionControl = (parentControl) => MoveMouseIntoSuggestionControl(parentControl, GetAddressSuggestionControl(parentControl));

				AssertSuggestionControl((control) => new Control[] { null }, (parentControl, control) => AssertHideSuggestionControlCore(
					expectedAddressSuggestionControlVisible: true,
					expectedCityTownSuggestionControlVisible: false,
					parentControl,
					control));
			}
		}

		public void TestStatePostcodeRequiredBehaviour()
		{
			var config = new AddressValidatorConfiguration()
				.WithCustomValidateAddress(GetCustomValidateAddress)
				.WithStateAndPostcodeRequiredWhenButtonClick();

			var staff = Factory.New<GlbStaffForTestStatePostcodeRequiredBehaviour>();

			var testCases = new (bool expectedResult, string status)[]
			{
				(true, AddressValidationStatus.Unverifiable),
				(true, AddressValidationStatus.CountryNotAvailable),
				(false, AddressValidationStatus.Verified),
				(false, AddressValidationStatus.ManuallyVerified),
				(false, AddressValidationStatus.VerifiedToStreet),
				(false, AddressValidationStatus.ToBeVerified),
				(false, AddressValidationStatus.Invalid),
				(false, AddressValidationStatus.ExcludeBackgroundValidation),
				(false, AddressValidationStatus.NotRequired)
			};

			foreach (var (expectedResult, status) in testCases)
			{
				staff.ValidationStatus = status;

				using (var form = new Form())
				using (var parentControl = new ControlSupportAddressValidationForTest(staff))
				{
					form.Controls.Add(parentControl);
					form.Show();

					var addressValidator = new AddressValidatorForTest(parentControl, config);

					parentControl.ValidateButton.PerformClick();

					AssertEquals(expectedResult, staff.StateAndPostcodeValidated);

					staff.ResetStateAndPostcodeValidated();
				}
			}

			Func<Task> GetCustomValidateAddress(Func<Task> core) => async () =>
			{
				await Task.Delay(1);
			};
		}

		public void TestSecuritiesCheckOfButtonClick()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.Address1 = "#2";

			using (var form = new Form())
			using (var parentControl = new ControlSupportAddressValidationForTest(org.MainAddress))
			{
				form.Controls.Add(parentControl);
				form.Show();

				AssertSecurityCheckOfButtonClick(parentControl, org.MainAddress, Env.Security.OrgAddressDetailsNonARAPNew);
				AssertSecurityCheckOfButtonClick(parentControl, org.MainAddress, Env.Security.OrgDetailsModifyNameAndAddress);
				AssertSecurityCheckOfButtonClick(parentControl, address, Env.Security.OrgAddressDetailsNew);
				Factory.Save();
				AssertSecurityCheckOfButtonClick(parentControl, address, Env.Security.OrgAddressDetailsModify);
			}
		}

		public void TestClearFields()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GeoLocation = ZGeography.CreatePoint(12, 21);
			staff.Postcode = "postcode";
			staff.City = "city";
			staff.StateCode = "state code";
			staff.Address1 = "address 1";
			staff.Address2 = "address 2";

			using (var form = new ZForm())
			using (var parentControl = new ControlSupportAddressValidationForTest(staff))
			{
				form.Controls.Add(parentControl);
				form.Show();

				var addressValidator = new AddressValidatorForTest(parentControl);

				AssertEquals("PreCondition", false, parentControl.Address1Control.Focused);

				parentControl.ClearAddressFieldsButton.PerformClick();

				AssertNull(staff.GeoLocation.Latitude);
				AssertNull(staff.GeoLocation.Longitude);
				AssertNullOrEmpty(staff.Address1);
				AssertNullOrEmpty(staff.Address2);
				AssertNullOrEmpty(staff.Postcode);
				AssertNullOrEmpty(staff.City);
				AssertNullOrEmpty(staff.StateCode);
				Assert(parentControl.Address1Control.Focused);
			}
		}

		public void TestCustomValidateAddressButtonClick()
		{
			var validated = false;
			var customButtonClickLogicCalled = false;
			var runCore = false;

			var config = new AddressValidatorConfiguration()
				.WithCustomValidateAddress(GetCustomValidateAddress)
				.WithCustomValidateButtonClick((core) =>
				{
					if (runCore)
					{
						core.Invoke();
					}
					customButtonClickLogicCalled = true;
				});

			using (var form = new Form())
			using (var parentControl = new ControlSupportAddressValidationForTest(Factory.New<JobDocAddress>()))
			{
				form.Controls.Add(parentControl);
				form.Show();

				var addressValidator = new AddressValidatorForTest(parentControl, config);

				parentControl.ValidateButton.PerformClick();

				AssertEquals(false, validated);
				Assert(customButtonClickLogicCalled);

				customButtonClickLogicCalled = false;
				runCore = true;

				parentControl.ValidateButton.PerformClick();

				Assert(validated);
				Assert(customButtonClickLogicCalled);
			}

			Func<Task> GetCustomValidateAddress(Func<Task> core) => async () =>
			{
				validated = true;
				await Task.Delay(1);
			};
		}

		public void TestCityTownSelectedAction()
		{
			using (var form = new ZForm())
			using (var control = new ControlSupportAddressValidationForTest(Factory.NewWithValidTestData<OrgAddress>()))
			{
				form.Controls.Add(control);
				form.Show();

				var addressValidator = new AddressValidatorForTest(control);

				AssertNotNull(addressValidator.ConfigExposed.SelectCityTownAction);

				var addressValidated = false;
				addressValidator.ConfigExposed.WithCustomValidateAddress(core => async () =>
				{
					addressValidated = true;
					await Task.CompletedTask;
				});

				addressValidator.ConfigExposed.SelectCityTownAction.Invoke();

				Assert(addressValidated);
			}
		}

		public void TestOverrideRefreshValidationStatus()
		{
			using (var form = new ZForm())
			using (var control = new ControlSupportAddressValidationForTest(Factory.NewWithValidTestData<OrgAddress>()))
			{
				AssertNotEquals("Precondition: ", "123", control.Name);
				var config = new AddressValidatorConfiguration().WithRefreshValidationStatus(
					() => control.Name = "123",
					default,
					default,
					null,
					null);
				form.Controls.Add(control);
				form.Show();

				new AddressValidatorForTest(control, config).RefreshValidationStatus_Exposed();
				AssertEquals("123", control.Name);
			}
		}

		public void TestRefreshValidationStatus_ShouldValidate_ValidationStatusNotSpecified()
		{
			using (var form = new ZForm())
			using (var control = new ControlSupportAddressValidationForTest(Factory.NewWithValidTestData<OrgAddress>()))
			{
				control.ValidateButton.Visible = false;
				control.ClearAddressFieldsButton.Visible = false;

				CombineAssertions("Preconditions: ", () =>
				{
					AssertEquals(AddressValidationStatus.ToBeVerified, control.AddressForValidation.ValidationStatus);
					AssertEquals(false, control.ValidateButton.Visible);
					AssertEquals(false, control.ClearAddressFieldsButton.Visible);
					AssertNotEquals("123", control.Name);
				});

				var config = new AddressValidatorConfiguration().WithRefreshValidationStatus(
					null,
					null,
					null,
					() => control.Name = "123",
					() => control.Name = "456");
				form.Controls.Add(control);
				form.Show();

				new AddressValidatorForTest(control, config).RefreshValidationStatus_Exposed();
				CombineAssertions(() =>
				{
					AssertEquals(true, control.ValidateButton.Visible);
					AssertEquals(true, control.ClearAddressFieldsButton.Visible);
					AssertEquals("123", control.Name);
					AssertEquals("This address needs to be verified.", control.ValidateButton.ToolTipCaption);
					AssertEquals(Color.FromArgb(255, 224, 179), control.Address1Control.BackColor);
				});
			}
		}

		public void TestRefreshValidationStatus_ShouldValidate_ValidationStatusSpecified()
		{
			using (var form = new ZForm())
			using (var control = new ControlSupportAddressValidationForTest(Factory.NewWithValidTestData<OrgAddress>()))
			{
				control.ValidateButton.Visible = false;
				control.ClearAddressFieldsButton.Visible = false;

				CombineAssertions("Preconditions: ", () =>
				{
					AssertEquals(false, control.ValidateButton.Visible);
					AssertEquals(false, control.ClearAddressFieldsButton.Visible);
					AssertNotEquals("123", control.Name);
				});

				var config = new AddressValidatorConfiguration().WithRefreshValidationStatus(
					null,
					() => true,
					() => AddressValidationStatus.VerifiedToStreet,
					() => control.Name = "123",
					() => control.Name = "456");
				form.Controls.Add(control);
				form.Show();

				new AddressValidatorForTest(control, config).RefreshValidationStatus_Exposed();
				CombineAssertions(() =>
				{
					AssertEquals(true, control.ValidateButton.Visible);
					AssertEquals(true, control.ClearAddressFieldsButton.Visible);
					AssertEquals("123", control.Name);
					AssertEquals("This address is verified to street number.", control.ValidateButton.ToolTipCaption);
					AssertEquals(Color.FromArgb(198, 236, 198), control.Address1Control.BackColor);
				});
			}
		}

		public void TestRefreshValidationStatus_ShouldNotValidate()
		{
			using (var form = new ZForm())
			using (var control = new ControlSupportAddressValidationForTest(Factory.NewWithValidTestData<OrgAddress>()))
			{
				control.ValidateButton.Visible = true;
				control.ClearAddressFieldsButton.Visible = true;

				CombineAssertions("Preconditions: ", () =>
				{
					AssertEquals(true, control.ValidateButton.Visible);
					AssertEquals(true, control.ClearAddressFieldsButton.Visible);
					AssertNotEquals("456", control.Name);
				});

				var config = new AddressValidatorConfiguration().WithRefreshValidationStatus(
					null,
					() => false,
					() => AddressValidationStatus.VerifiedToStreet,
					() => control.Name = "123",
					() => control.Name = "456");
				form.Controls.Add(control);
				form.Show();

				new AddressValidatorForTest(control, config).RefreshValidationStatus_Exposed();
				CombineAssertions(() =>
				{
					AssertEquals(false, control.ValidateButton.Visible);
					AssertEquals(false, control.ClearAddressFieldsButton.Visible);
					AssertEquals("456", control.Name);
					AssertNull(control.ValidateButton.ToolTipCaption);
					AssertEquals(SystemColors.Window, control.Address1Control.BackColor);
				});
			}
		}

		public void TestHookAndUnhookAddressValidationStatusChangedEvent()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			using (var form = new ZForm())
			using (var control = new ControlSupportAddressValidationForTest(address))
			using (var suggestionControl = new AddressSuggestionControl() { Name = AddressSuggestionControlHelper.AddressSuggestionControlName })
			{
				control.SuggestionWindowParentControl.Controls.Add(suggestionControl);
				form.Controls.Add(control);
				var config = new AddressValidatorConfiguration().WithRefreshValidationStatus(
					() => control.Name = "123",
					default,
					default,
					null,
					null);
				form.Controls.Add(control);
				form.Show();

				var validator = new AddressValidatorForTest(control, config);
				AssertNotEquals("Precondition: ", "123", control.Name);
				AssertEquals(true, control.Controls.Find(AddressSuggestionControlHelper.AddressSuggestionControlName, true).Any());

				validator.HookAddressValidationStatusChangedEvent_Exposed();
				validator.UnhookAddressValidationStatusChangedEvent_Exposed();
				address.OA_ValidationStatus = "MAN";
				AssertNotEquals("Hook then Unhook. Set OA_ValidationStatus to raise AddressValidationStatusChanged event.", "123", control.Name);
				AssertEquals(true, control.Controls.Find(AddressSuggestionControlHelper.AddressSuggestionControlName, true).Any());

				validator.HookAddressValidationStatusChangedEvent_Exposed();
				address.OA_ValidationStatus = "MAN";
				AssertEquals("Just hook. Set OA_ValidationStatus to raise AddressValidationStatusChanged event.", "123", control.Name);
				AssertEquals(false, control.Controls.Find(AddressSuggestionControlHelper.AddressSuggestionControlName, true).Any());
			}
		}

		#region Implementation

		void AssertGetParentForm(Form expectedForm, Control control)
		{
			AssertEquals(expectedForm, new AddressValidatorForTest(control).ParentFormExposed);
		}

		void AssertSecurityCheckOfButtonClick(Control control, OrgAddress address, SecurityCheckpoint checkpoint)
		{
			var validated = false;

			var parentControl = control as ISupportWebAddressValidationControl;

			var config = new AddressValidatorConfiguration()
				.WithCustomValidateAddress(GetCustomValidateAddress)
				.WithOrgAddressSecuritiesCheckWhenButtonClick(() => address);

			var addressValidator = new AddressValidatorForTest(control, config);

			checkpoint.IsAllowed = false;

			UnitTestUserNotification.Instance.ClearMessages();
			parentControl.ValidateButton.PerformClick();
			AssertEquals(checkpoint.ErrorMessageForNotAllowed.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, validated);

			UnitTestUserNotification.Instance.ClearMessages();
			parentControl.ClearAddressFieldsButton.PerformClick();
			AssertEquals(checkpoint.ErrorMessageForNotAllowed.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);

			checkpoint.IsAllowed = true;

			UnitTestUserNotification.Instance.ClearMessages();

			parentControl.ValidateButton.PerformClick();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			Assert(validated);

			parentControl.ClearAddressFieldsButton.PerformClick();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			Func<Task> GetCustomValidateAddress(Func<Task> core) => async () =>
			{
				validated = true;
				await Task.Delay(1);
			};
		}

		void AssertSuggestionControl(
			Func<ISupportWebAddressValidationControl, Control[]> controlsGetter,
			Action<ControlSupportAddressValidationForTest, Control> core)
		{
			using (var form = new ZForm())
			using (var parentControl = UseNullAsAddressForTest
				? new ControlSupportAddressValidationForTest()
				: new ControlSupportAddressValidationForTest(Factory.NewWithValidTestData<OrgAddress>()))
			{
				form.Controls.Add(parentControl);
				form.Show();

				var candidateCityTowns = new CandidateCityTown[1] { new CandidateCityTown() };
				var addressSuggestionControl = new AddressSuggestionControl() { Name = AddressSuggestionControlHelper.AddressSuggestionControlName };
				var cityTownSuggestionControl = new CityTownSuggestionControl(null, candidateCityTowns, null, null, null, null, null, null) { Name = CityTownSuggestionControlHelper.CityTownSuggestionControlName };

				if (ForceAddressSuggestionControlShowFailedForTest)
				{
					addressSuggestionControl.VisibleChanged += (object sender, EventArgs e) => addressSuggestionControl.Visible = false;
				}

				var enableAddressValidationWebService = Env.Instance.Registry.EnableAddressValidationWebService;

				using (new DisposableAction(() =>
				{
					Env.Instance.Registry.EnableAddressValidationWebService = enableAddressValidationWebService;

					parentControl.SuggestionWindowParentControl.Controls.Remove(addressSuggestionControl);
					parentControl.SuggestionWindowParentControl.Controls.Remove(cityTownSuggestionControl);
					addressSuggestionControl.Dispose();
					cityTownSuggestionControl.Dispose();
				}))
				{
					Env.Instance.Registry.EnableAddressValidationWebService = true;

					parentControl.SuggestionWindowParentControl.Controls.Add(addressSuggestionControl);
					parentControl.SuggestionWindowParentControl.Controls.Add(cityTownSuggestionControl);

					new AddressValidatorForTest(parentControl);

					foreach (var control in controlsGetter.Invoke(parentControl))
					{
						core.Invoke(parentControl, control);
					}
				}
			}
		}

		void AssertShowSuggestionControlWithStatuses(
			bool expectedAddressSuggestionControlVisible,
			bool expectedCityTownSuggestionControlVisible,
			ControlSupportAddressValidationForTest parentControl,
			Control control,
			string[] statuses)
		{
			foreach (var status in statuses)
			{
				parentControl.AddressForValidation.ValidationStatus = status;

				AssertShowSuggestionControlCore(
					expectedAddressSuggestionControlVisible,
					expectedCityTownSuggestionControlVisible,
					parentControl,
					control);
			}
		}

		void AssertShowSuggestionControlCore(
			bool expectedAddressSuggestionControlVisible,
			bool expectedCityTownSuggestionControlVisible,
			ControlSupportAddressValidationForTest parentControl,
			Control control)
		{
			var addressSuggestionControl = GetAddressSuggestionControl(parentControl);
			var cityTownSuggestionControl = GetCityTownSuggestionControl(parentControl);

			addressSuggestionControl.Hide();
			cityTownSuggestionControl.Hide();

			parentControl.ActiveControl = null;

			AssertEquals("PreCondition", false, addressSuggestionControl.Visible);
			AssertEquals("PreCondition", false, cityTownSuggestionControl.Visible);

			control.Focus();

			AssertEquals(expectedAddressSuggestionControlVisible, addressSuggestionControl.Visible);
			AssertEquals(expectedCityTownSuggestionControlVisible, cityTownSuggestionControl.Visible);
		}

		void AssertHideSuggestionControlCore(
			bool expectedAddressSuggestionControlVisible,
			bool expectedCityTownSuggestionControlVisible,
			ControlSupportAddressValidationForTest parentControl,
			Control control)
		{
			var addressSuggestionControl = GetAddressSuggestionControl(parentControl);
			var cityTownSuggestionControl = GetCityTownSuggestionControl(parentControl);

			parentControl.AddressForValidation.ValidationStatus = AddressValidationStatus.Verified;

			foreach (var preFocusedControl in GetAllControls(parentControl))
			{
				if (preFocusedControl == control || preFocusedControl?.Parent == control || preFocusedControl == control?.Parent)
				{
					continue;
				}

				parentControl.ActiveControl = null;

				addressSuggestionControl.Show();
				cityTownSuggestionControl.Show();

				addressSuggestionControl.Location = new Point(1, 1);
				cityTownSuggestionControl.Location = new Point(addressSuggestionControl.Width + 2, 1);

				preFocusedControl.Focus();

				Assert("PreCondition", addressSuggestionControl.Visible);
				Assert("PreCondition", cityTownSuggestionControl.Visible);

				(MoveMouseForSuggestionControl ?? MoveMouseOutOfSuggestionControl).Invoke(parentControl);

				parentControl.ActiveControl = control;

				AssertEquals(expectedAddressSuggestionControlVisible, addressSuggestionControl.Visible);
				AssertEquals(expectedCityTownSuggestionControlVisible, cityTownSuggestionControl.Visible);
			}
		}

		void MoveMouseIntoSuggestionControl(ControlSupportAddressValidationForTest parentControl, Control suggestionControl)
		{
			Cursor.Position = parentControl.PointToScreen(new Point(suggestionControl.Location.X + 1, suggestionControl.Location.Y + 1));

			Assert("PreCondition", suggestionControl.Bounds.Contains(parentControl.SuggestionWindowParentControl.PointToClient(Control.MousePosition)));
		}

		void MoveMouseOutOfSuggestionControl(ControlSupportAddressValidationForTest parentControl)
		{
			var addressSuggestionControl = GetAddressSuggestionControl(parentControl);
			var cityTownSuggestionControl = GetCityTownSuggestionControl(parentControl);

			Cursor.Position = parentControl.PointToScreen(new Point(0, 0));

			AssertEquals("PreCondition", false, addressSuggestionControl.Bounds.Contains(parentControl.SuggestionWindowParentControl.PointToClient(Control.MousePosition)));
			AssertEquals("PreCondition", false, cityTownSuggestionControl.Bounds.Contains(parentControl.SuggestionWindowParentControl.PointToClient(Control.MousePosition)));
		}

		AddressSuggestionControl GetAddressSuggestionControl(ISupportWebAddressValidationControl control) => control.SuggestionWindowParentControl.Controls.Find(AddressSuggestionControlHelper.AddressSuggestionControlName, searchAllChildren: true).First() as AddressSuggestionControl;

		CityTownSuggestionControl GetCityTownSuggestionControl(ISupportWebAddressValidationControl control) => control.SuggestionWindowParentControl.Controls.Find(CityTownSuggestionControlHelper.CityTownSuggestionControlName, searchAllChildren: true).First() as CityTownSuggestionControl;

		Control[] GetAllControls(ISupportWebAddressValidationControl control) => GetAddressControls(control)
			.Concat(GetCityTownControls(control))
			.Append(control.ValidateButton)
			.ToArray();

		Control[] GetAddressControls(ISupportWebAddressValidationControl control) => new Control[]
		{
			control.Address1Control,
			control.Address2Control
		};

		Control[] GetCityTownControls(ISupportWebAddressValidationControl control) => new Control[]
		{
			control.CityControl,
			control.PostcodeControl,
			control.CountryControl,
			control.CountryControl.CodeBox,
			control.StateControl,
			control.StateControl.CodeBox
		};

		string[] VerifiedStatuses { get; } = new[]
		{
			AddressValidationStatus.Verified,
			AddressValidationStatus.VerifiedToStreet,
			AddressValidationStatus.ManuallyVerified
		};

		string[] UnverifiedStatuses { get; } = new[]
		{
			AddressValidationStatus.Unverifiable,
			AddressValidationStatus.ToBeVerified,
			AddressValidationStatus.Invalid,
			AddressValidationStatus.CountryNotAvailable,
			AddressValidationStatus.ExcludeBackgroundValidation,
			AddressValidationStatus.NotRequired
		};

		bool UseNullAsAddressForTest { get; set; }

		bool ForceAddressSuggestionControlShowFailedForTest { get; set; }

		Action<ControlSupportAddressValidationForTest> MoveMouseForSuggestionControl { get; set; }

		#endregion Implementation
	}

	#region Derived Class For Test

	class AddressValidatorForTest : AddressValidator
	{
		public AddressValidatorForTest(Control parentControl)
			: base(parentControl)
		{
		}

		public AddressValidatorForTest(Control parentControl, AddressValidatorConfiguration config)
			: base(parentControl, config)
		{
		}

		public void RefreshValidationStatus_Exposed() => RefreshValidationStatus();

		public void HookAddressValidationStatusChangedEvent_Exposed() => HookAddressValidationStatusChangedEvent();

		public void UnhookAddressValidationStatusChangedEvent_Exposed() => UnhookAddressValidationStatusChangedEvent();

		private protected override void ForcedValidateAddress()
		{
			ValidationForced = true;
			AsyncTaskSynchronizer.Run(ConfigExposed.ValidateAddress(ValidateAddressCore));
		}

		public Form ParentFormExposed => ParentForm;
		public AddressValidatorConfiguration ConfigExposed => config;
	}

	class GlbStaffForTestStatePostcodeRequiredBehaviour : GlbStaff, ISupportWebAddressValidation
	{
		public GlbStaffForTestStatePostcodeRequiredBehaviour(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new void ValidatePostcodeAndStateForAddress()
		{
			StateAndPostcodeValidated = true;
		}

		public void ResetStateAndPostcodeValidated() => StateAndPostcodeValidated = false;

		public bool StateAndPostcodeValidated { get; private set; }
	}

	#endregion Derived Class For Test
}
