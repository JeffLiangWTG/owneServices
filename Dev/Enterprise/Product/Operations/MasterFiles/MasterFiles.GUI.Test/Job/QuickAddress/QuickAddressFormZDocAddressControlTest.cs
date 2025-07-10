using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture.GUI;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class QuickAddressFormZDocAddressControlTest : TestCaseWithDummy
	{
		public void TestSuggestionControlShouldResizeFormWhenShownUpAndClosed()
		{
			using (var form = new ZForm())
			using (var quickAddressFormZDocAddressControl = new QuickAddressFormZDocAddressControlForTest())
			using (var suggestionControl = new AddressSuggestionControl())
			{
				form.Controls.Add(quickAddressFormZDocAddressControl);
				form.Controls.Add(suggestionControl);
				form.Show();

				var originalLocation = quickAddressFormZDocAddressControl.Location;
				var originalSize = form.Size;

				AsyncTaskSynchronizer.Run(quickAddressFormZDocAddressControl.ValidateAddressForTest);

				AssertNotEquals(originalLocation, suggestionControl.Location);
				AssertNotEquals(originalSize, form.Size);

				quickAddressFormZDocAddressControl.CloseSuggestionFormsForTest();
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(273, 253), form.Size);
			}
		}

		public void TestCityTownControlShouldResizeFormWhenShownUpAndClosed()
		{
			var cityTowns = new[]
			{
				new CandidateCityTown
				{
					City = "Sydney",
					Postcode = "2000",
					State = "NSW",
				}
			};

			using (var form = new ZForm())
			using (var quickAddressFormZDocAddressControl = new QuickAddressFormZDocAddressControlForTest())
			using (var cityTownSuggestionControl = new CityTownSuggestionControl(null, cityTowns, "Sydney", "", "", null, null, null))
			{
				form.Controls.Add(quickAddressFormZDocAddressControl);
				form.Controls.Add(cityTownSuggestionControl);
				form.Show();

				var originalLocation = cityTownSuggestionControl.Location;
				var originalSize = form.Size;

				AsyncTaskSynchronizer.Run(quickAddressFormZDocAddressControl.GetCityTownAsyncForTest);

				AssertNotEquals(originalLocation, cityTownSuggestionControl.Location);
				AssertNotEquals(originalSize, form.Size);

				quickAddressFormZDocAddressControl.CloseSuggestionFormsForTest();
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(273, 253), form.Size);
			}
		}

		public void TestNoExceptionThrownIfParentFormClosedDuringAddressValidation()
		{
			using (var form = new ZForm())
			using (var quickAddressFormZDocAddressControl = new QuickAddressFormZDocAddressControlForTest())
			using (var suggestionControl = new AddressSuggestionControl())
			{
				form.Controls.Add(quickAddressFormZDocAddressControl);
				form.Controls.Add(suggestionControl);
				form.Show();

				var originalLocation = quickAddressFormZDocAddressControl.Location;
				var originalSize = form.Size;

				form.Close();
				AssertNoExceptionThrown(() => AsyncTaskSynchronizer.Run(quickAddressFormZDocAddressControl.ValidateAddressForTest));
			}
		}

		public void TestNoExceptionThrownIfParentFormClosedDuringGettingCityTown()
		{
			var cityTowns = new[]
			{
				new CandidateCityTown
				{
					City = "Sydney",
					Postcode = "2000",
					State = "NSW",
				}
			};

			using (var form = new ZForm())
			using (var quickAddressFormZDocAddressControl = new QuickAddressFormZDocAddressControlForTest())
			using (var cityTownSuggestionControl = new CityTownSuggestionControl(null, cityTowns, "Sydney", "", "", null, null, null))
			{
				form.Controls.Add(quickAddressFormZDocAddressControl);
				form.Controls.Add(cityTownSuggestionControl);
				form.Show();

				var originalLocation = cityTownSuggestionControl.Location;
				var originalSize = form.Size;

				form.Close();
				AssertNoExceptionThrown(() => AsyncTaskSynchronizer.Run(quickAddressFormZDocAddressControl.GetCityTownAsyncForTest));
			}
		}
	}
}
