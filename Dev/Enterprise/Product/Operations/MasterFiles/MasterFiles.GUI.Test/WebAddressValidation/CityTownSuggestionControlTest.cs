using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CityTownSuggestionControlTest : TestCaseWithFactory
	{
		public void TestConstructor_WhenGettingEnoughSpace_ShouldOpenToBottomByDefault()
		{
			// Arrange.

			var cityTown = new CandidateCityTown
			{
				City = "[_MOCK_CITY_]",
				Postcode = "[_MOCK_POSTCODE_]",
				State = "[_MOCK_STATE_]"
			};

			var address = Factory.NewWithValidTestData<OrgAddress>();

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var buttonLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var buttonSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			// Act.

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var validationButton = new Button { Location = buttonLocation, Size = buttonSize })
			{
				parentControl.Controls.Add(validationButton);
				applicationForm.Controls.Add(parentControl);

				var suggestionControl = new CityTownSuggestionControl(
					address,
					new[] { cityTown },
					"[_ENTERED_CITY_]",
					"[_ENTERED_STATE_]",
					"[_ENTERED_POSTCODE_]",
					applicationForm,
					parentControl,
					validationButton);

				using (suggestionControl)
				{
					// Assert.

					AssertEquals(
						0,
						ControlDpiScalingHelper.UnscaleFromCurrentDpiX(suggestionControl.Location.X));

					AssertEquals(
						100 + 1,
						ControlDpiScalingHelper.UnscaleFromCurrentDpiY(suggestionControl.Location.Y));
				}
			}
		}

		public void TestUpdateFilterShouldHaveCorrectCountOfMatchItems()
		{
			// Arrange.

			var candidateCityTowns = PrepareCandidateCityTowns();
			AssertEquals("Precondition", 1000, candidateCityTowns.Length);

			// Act.

			using (var suggestionControl =
				new CityTownSuggestionControl(null, candidateCityTowns, "Sydney", "", "", null, null, null))
			{
				suggestionControl.UpdateFilter("Sydney", "NSW", "0");

				// Assert.

				AssertEquals("The count of items in CityTownListView should be 1000.", 1000, suggestionControl.CityTownListView.Items.Count);
#if !WINZOR //TODO fix groups for winzor
				AssertEquals("The count of CityTownListView's group should be 2: 'Exact Matches' and 'Partial Matches'.", 2, suggestionControl.CityTownListView.Groups.Count);
				AssertEquals("The count of items in exact matches group should be 999.", 999, suggestionControl.CityTownListView.Groups[0].Items.Count);
#endif
			}
		}

		public void TestUpdateFilterShouldHaveCorrectCountOfPartialMatchItems()
		{
			// Arrange.

			var candidateCityTowns = PrepareCandidateCityTowns();
			AssertEquals("Precondition", 1000, candidateCityTowns.Length);

			// Act.

			using (var suggestionControl =
				new CityTownSuggestionControl(null, candidateCityTowns, "Sydney", "", "", null, null, null))
			{
				suggestionControl.UpdateFilter("Sydney", "NSW", "0001");

				// Assert.

				AssertEquals("The count of items in CityTownListView should be 1000.", 1000, suggestionControl.CityTownListView.Items.Count);
#if !WINZOR //TODO fix groups for winzor
				AssertEquals("The count of CityTownListView's group should be 2: 'Exact Matches' and 'Partial Matches'.", 2, suggestionControl.CityTownListView.Groups.Count);
				AssertEquals("The count of items in partial matches group should be 999.", 999, suggestionControl.CityTownListView.Groups[1].Items.Count);
#endif
			}
		}

		public void TestSetupListView()
		{
			// Arrange.

			var candidateCityTowns = PrepareCandidateCityTowns();

			// Act.

			using (var suggestionControl = new CityTownSuggestionControl(null, candidateCityTowns, "Sydney", "", "", null, null, null))
			{
				// Assert.

				AssertEquals("The count of CityTownListView's items should be 1000", 1000, suggestionControl.CityTownListView.Items.Count);
			}
		}

		public void TestConstructListWhenStateProvinceValidationRuleIsMustNotBeEnteredOrNot()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = "AU";

			var candidateCityTowns = PrepareCandidateCityTowns(1);
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;
			AssertSuggestionValue(orgAddress, candidateCityTowns, "0001, Sydney");

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			AssertSuggestionValue(orgAddress, candidateCityTowns, "0001, Sydney, NSW");

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			AssertSuggestionValue(orgAddress, candidateCityTowns, "0001, Sydney, NSW");
		}

		void AssertSuggestionValue(OrgAddress orgAddress, CandidateCityTown[] candidateCityTowns, string expectedValue)
		{
			using (var suggestionControl = new CityTownSuggestionControl(orgAddress, candidateCityTowns, "Sydney", "", "", null, null, null))
			{
				AssertEquals("The count of CityTownListView's items should be 1", 1, suggestionControl.CityTownListView.Items.Count);
				AssertEquals("The text should be " + expectedValue, expectedValue, suggestionControl.CityTownListView.Items[0].Text);
			}
		}

		CandidateCityTown[] PrepareCandidateCityTowns(int maxCount = 1000)
		{
			var cityTownSuggestions = new CandidateCityTown[maxCount];

			for (int i = 1; i < maxCount + 1; i++)
			{
				var cityTown = new CandidateCityTown
				{
					City = "Sydney",
					Postcode = i.ToString().PadLeft(4, '0'),
					State = "NSW",
				};

				cityTownSuggestions[i - 1] = cityTown;
			}

			return cityTownSuggestions;
		}

		public void TestNoDuplicates()
		{
			var cityTownSuggestions = new CandidateCityTown[3];

			var cityTown0 = new CandidateCityTown
			{
				City = "Sydney",
				Postcode = "2000",
				State = "NSW",
			};

			var cityTown1 = new CandidateCityTown
			{
				City = "Adelaide",
				Postcode = "5000",
				State = "SA",
			};

			var cityTown2 = new CandidateCityTown
			{
				City = "Melbourne",
				Postcode = "3000",
				State = "VIC",
			};

			cityTownSuggestions[0] = cityTown0;
			cityTownSuggestions[1] = cityTown1;
			cityTownSuggestions[2] = cityTown2;

			var address = Factory.NewWithValidTestData<OrgAddress>();

			using (var suggestionControl = new CityTownSuggestionControl(address, cityTownSuggestions, "Sydney", "NSW", "2000", null, null, null))
			{
				CombineAssertions(() =>
				{
					AssertEquals("The count of CityTownListView's items should be 3", 3, suggestionControl.CityTownListView.Items.Count);

					AssertEquals("The text should be '2000, Sydney, NSW'", cityTown0.Postcode + ", " + cityTown0.City + ", " + cityTown0.State, suggestionControl.CityTownListView.Items[0].Text);
					AssertEquals("The text should be '5000, Adelaide, SA'", cityTown1.Postcode + ", " + cityTown1.City + ", " + cityTown1.State, suggestionControl.CityTownListView.Items[1].Text);
					AssertEquals("The text should be '3000, Melbourne, VIC'", cityTown2.Postcode + ", " + cityTown2.City + ", " + cityTown2.State, suggestionControl.CityTownListView.Items[2].Text);
				});
			}
		}

		public void TestDuplicatesRemoved()
		{
			var cityTownSuggestions = new CandidateCityTown[6];

			var cityTown0 = new CandidateCityTown
			{
				City = "Sydney",
				Postcode = "2000",
				State = "NSW",
			};

			var cityTown1 = new CandidateCityTown
			{
				City = "Sydney",
				Postcode = "2000",
				State = "NSW",
			};

			var cityTown2 = new CandidateCityTown
			{
				City = "Melbourne",
				Postcode = "3000",
				State = "VIC",
			};

			var cityTown3 = new CandidateCityTown
			{
				City = "Melbourne",
				Postcode = "3000",
				State = "VIC",
			};

			var cityTown4 = new CandidateCityTown
			{
				City = "Melbourne",
				Postcode = "3000",
				State = "VIC",
			};

			var cityTown5 = new CandidateCityTown
			{
				City = "Melbourne",
				Postcode = "3000",
				State = "VIC",
			};

			cityTownSuggestions[0] = cityTown0;
			cityTownSuggestions[1] = cityTown1;
			cityTownSuggestions[2] = cityTown2;

			cityTownSuggestions[3] = cityTown3;
			cityTownSuggestions[4] = cityTown4;
			cityTownSuggestions[5] = cityTown5;

			var address = Factory.NewWithValidTestData<OrgAddress>();

			using (var suggestionControl = new CityTownSuggestionControl(address, cityTownSuggestions, "Sydney", "NSW", "2000", null, null, null))
			{
				CombineAssertions(() =>
				{
					AssertEquals("The count of CityTownListView's items should be 2", 2, suggestionControl.CityTownListView.Items.Count);

					AssertEquals("The text should be '2000, Sydney, NSW'", cityTown1.Postcode + ", " + cityTown1.City + ", " + cityTown1.State, suggestionControl.CityTownListView.Items[0].Text);
					AssertEquals("The text should be '3000, Melbourne, VIC'", cityTown3.Postcode + ", " + cityTown3.City + ", " + cityTown3.State, suggestionControl.CityTownListView.Items[1].Text);
				});
			}
		}

		public void TestCityTownSelectedHandlerBeenUnregisteredAfterDisposed()
		{
			var candidateCityTowns = new CandidateCityTown[1] { new CandidateCityTown() };
			using (var control = new CityTownSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), candidateCityTowns, null, null, null, null, null, null))
			{
				var fieldInfo = typeof(CityTownSuggestionControl).GetField("CityTownSelected", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
				var cityTownSelected = fieldInfo.GetValue(control);

				var cityTownSelectedMethods = (cityTownSelected as Delegate).GetInvocationList().Select(x => x.Method.Name).ToArray();
				AssertEquals(1, cityTownSelectedMethods.Length);
				Assert(cityTownSelectedMethods.Any(x => x.Equals("HandleCityTownSelected")));

				control.Dispose();
				cityTownSelected = fieldInfo.GetValue(control);
				AssertNull(cityTownSelected);
			}
		}
	}
}
