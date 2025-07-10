using System;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI.WebAddressValidation;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class AddressValidatorConfigurationTest : TestCaseWithFactory
	{
		public void TestMaxWidthGetter()
		{
			AssertEquals(0, new AddressValidatorConfiguration().MaxWidthGetter.Invoke());
			AssertEquals(1, new AddressValidatorConfiguration().WithCustomSuggestionWindowMaxWidthGetter(() => 1).MaxWidthGetter.Invoke());
		}

		public void TestMaxHeightGetter()
		{
			AssertEquals(0, new AddressValidatorConfiguration().MaxHeightGetter.Invoke());
			AssertEquals(1, new AddressValidatorConfiguration().WithCustomSuggestionWindowMaxHeightGetter(() => 1).MaxHeightGetter.Invoke());
		}

		public void TestShouldHookControlResize()
		{
			var config1 = new AddressValidatorConfiguration();
			var config2 = new AddressValidatorConfiguration();

			var customGetter = new Func<int>(() => throw new NotImplementedException());

			AssertEquals(false, config1.ShouldHookControlResize);
			AssertEquals(false, config2.ShouldHookControlResize);

			config1.WithCustomSuggestionWindowMaxHeightGetter(customGetter);
			Assert(config1.ShouldHookControlResize);

			config2.WithCustomSuggestionWindowMaxWidthGetter(customGetter);
			Assert(config2.ShouldHookControlResize);
		}

		public void TestValidateAddress()
		{
			AsyncTaskSynchronizer.Run(async () =>
			{
				var addressValidated = false;
				var fakeValidateAddress = new Func<Task>(async () =>
				{
					await Task.CompletedTask;
					addressValidated = true;
				});

				await new AddressValidatorConfiguration().ValidateAddress.Invoke(fakeValidateAddress).Invoke();

				Assert(addressValidated);

				var customLogicExecuted = false;
				var customValidateAddress = new Func<Task>(async () =>
				{
					await Task.CompletedTask;
					customLogicExecuted = true;
				});
				addressValidated = false;

				await new AddressValidatorConfiguration()
					.WithCustomValidateAddress((core) => customValidateAddress)
					.ValidateAddress.Invoke(fakeValidateAddress)
					.Invoke();

				AssertEquals(false, addressValidated);
				Assert(customLogicExecuted);
			});
		}

		public void TestGetCityTownAsync()
		{
			AsyncTaskSynchronizer.Run(async () =>
			{
				var cityTownGot = false;
				var fakeGetCityTownAsync = new Func<Task>(async () =>
				{
					await Task.CompletedTask;
					cityTownGot = true;
				});

				await new AddressValidatorConfiguration().ValidateAddress.Invoke(fakeGetCityTownAsync).Invoke();

				var customLogicExecuted = false;
				var customGetCityTownAsync = new Func<Task>(async () =>
				{
					await Task.CompletedTask;
					customLogicExecuted = true;
				});
				cityTownGot = false;

				await new AddressValidatorConfiguration()
					.WithCustomGetCityTownAsync((core) => customGetCityTownAsync)
					.GetCityTownAsync.Invoke(fakeGetCityTownAsync)
					.Invoke();

				AssertEquals(false, cityTownGot);
				Assert(customLogicExecuted);
			});
		}

		public void TestValidateButtonClick()
		{
			var clicked = false;
			var fakeClickCore = new Action(() => clicked = true);

			new AddressValidatorConfiguration().ValidateButtonClick.Invoke(fakeClickCore);

			Assert(clicked);

			var customLogicExecuted = false;
			clicked = false;
			new AddressValidatorConfiguration()
				.WithCustomValidateButtonClick((core) => customLogicExecuted = true)
				.ValidateButtonClick
				.Invoke(fakeClickCore);

			AssertEquals(false, clicked);
			Assert(customLogicExecuted);
		}

		public void TestCurrentOrgAddressGetter()
		{
			AssertNull(new AddressValidatorConfiguration().CurrentOrgAddressGetter);

			var config = new AddressValidatorConfiguration().WithOrgAddressSecuritiesCheckWhenButtonClick(() => null);
			AssertNotNull(config.CurrentOrgAddressGetter);
			AssertNull(config.CurrentOrgAddressGetter.Invoke());
		}

		public void TestButtonClickWithStateAndPostcodeRequired()
		{
			AssertEquals(false, new AddressValidatorConfiguration().ButtonClickWithStateAndPostcodeRequired);
			Assert(new AddressValidatorConfiguration().WithStateAndPostcodeRequiredWhenButtonClick().ButtonClickWithStateAndPostcodeRequired);
		}

		public void TestCurrentSelectedTabCheck()
		{
			Assert(new AddressValidatorConfiguration().CurrentSelectedTabCheck.Invoke());
			AssertEquals(false, new AddressValidatorConfiguration().WithCurrentSelectedTabCheck(() => false).CurrentSelectedTabCheck.Invoke());
		}

		public void TestRefreshValidationStatus()
		{
			var config = new AddressValidatorConfiguration();
			CombineAssertions(() =>
			{
				AssertNull(config.OverrideRefreshValidationStatus);
				AssertNull(config.ShouldValidate);
				AssertNull(config.ValidationStatus);
				AssertNull(config.RunWhenRefreshValidationStatus_ShouldValidate);
				AssertNull(config.RunWhenRefreshValidationStatus_ShouldNotValidate);
			});

			var statusOverrided = false;
			var executed1 = false;
			var executed2 = false;
			config.WithRefreshValidationStatus(
				() => statusOverrided = true,
				() => true,
				() => "INV",
				() => executed1 = true,
				() => executed2 = true);

			CombineAssertions(() =>
			{
				AssertNotNull(config.OverrideRefreshValidationStatus);
				AssertNotNull(config.ShouldValidate);
				AssertNotNull(config.ValidationStatus);
				AssertNotNull(config.RunWhenRefreshValidationStatus_ShouldValidate);
				AssertNotNull(config.RunWhenRefreshValidationStatus_ShouldNotValidate);
			});

			config.OverrideRefreshValidationStatus.Invoke();
			config.RunWhenRefreshValidationStatus_ShouldValidate.Invoke();
			config.RunWhenRefreshValidationStatus_ShouldNotValidate.Invoke();

			CombineAssertions(() =>
			{
				Assert(statusOverrided);
				Assert(config.ShouldValidate.Invoke());
				AssertEquals("INV", config.ValidationStatus.Invoke());
				Assert(executed1);
				Assert(executed2);
			});
		}

		public void TestFirstLoadCheck()
		{
			AssertEquals(false, new AddressValidatorConfiguration().CheckIfControlFirstLoad.Invoke());
			Assert(new AddressValidatorConfiguration().WithCheckIfControlFirstLoad(() => true).CheckIfControlFirstLoad.Invoke());
		}

		public void TestReferenceControlGetter()
		{
			using (var control = new ControlSupportAddressValidationForTest())
			{
				AssertEquals(control.ValidateButton, new AddressValidatorConfiguration().ReferenceControlGetter.Invoke(control));
				AssertEquals(control.Address1Control, new AddressValidatorConfiguration().WithReferenceControlGetter((x) => x.Address1Control).ReferenceControlGetter.Invoke(control));
			}
		}

		public void TestAddressSelectedAction()
		{
			AssertNull(new AddressValidatorConfiguration().AddressSelectedAction);

			var addressSelected = false;

			var config = new AddressValidatorConfiguration().WithAddressSelectedAction(() => addressSelected = true);
			AssertNotNull(config.AddressSelectedAction);

			config.AddressSelectedAction.Invoke();
			Assert(addressSelected);
		}

		public void TestDefaultHandleDestroyedAction()
		{
			AssertNull(new AddressValidatorConfiguration().HandleDestroyedAction);

			var destroyed = false;

			var config = new AddressValidatorConfiguration().WithHandleDestroyedAction(() => destroyed = true);
			AssertNotNull(config.HandleDestroyedAction);

			config.HandleDestroyedAction.Invoke();
			Assert(destroyed);
		}
	}
}
