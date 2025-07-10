using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation.Testing
{
	sealed class CityTownSuggestionControlHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestShowSuggestedCityTownsParentControlIsDisposed()
		{
			using (var applicationForm = new Form())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				var cityTowns = new CandidateCityTown[] { new CandidateCityTown() };
				var address = Factory.New<OrgAddress>();
				var parentControl = new ZDocAddressControl();
				Action handleSelectAction = () => { };

				parentControl.Dispose();

				var methodInfo = typeof(CityTownSuggestionControlHelper).GetMethod("ShowSuggestedCityTowns", BindingFlags.NonPublic | BindingFlags.Static);
				methodInfo.Invoke(null, new object[] { cityTowns, address, parentControl, applicationForm, contactDetailsGroupBox.Controls, handleSelectAction, 0, true });
			}
		}

		[ExpectNoExceptions]
		public void TestGetCityTownAsyncWithNullCandidates()
		{
			using (var applicationForm = new Form())
			{
				var address = new Mock<ISupportWebAddressValidation>
				{
					Object =
					{
						IsUpdatingCityTown = false,
						ValidationStatus = AddressValidationStatus.ToBeVerified
					}
				};
				address
					.Setup(m => m.GetCityTownAsync(It.IsAny<CancellationTokenSource>()))
					.Returns(Task.FromResult<CandidateCityTown[]>(null));
				CityTownSuggestionControlHelper.GetCityTownAsync(null, address.Object, null, applicationForm,
					new Control.ControlCollection(null), null).Wait();
				address.Verify(m => m.GetCityTownAsync(It.IsAny<CancellationTokenSource>()), Times.AtLeastOnce());
			}
		}

		public void TestSuggestionControlHelperFunctionsDoNotThrowWhenSuggestionControlAndBetterListViewAreAlreadyDisposed()
		{
			var address = new Mock<ISupportWebAddressValidation>();

			var cityTowns = new CandidateCityTown[1];
			var cityTown = new CandidateCityTown
			{
				City = "Sydney",
				Postcode = "2000",
				State = "NSW",
			};
			cityTowns[0] = cityTown;

			using (var applicationForm = new Form())
			using (var parentControl = new ZDocAddressControl())
			using (var control = new TextBox())
			using (var suggestionControl = new CityTownSuggestionControl(address.Object, cityTowns, "Sydney", "", "", null, null, null))
			{
				AssertNoExceptionThrown(() =>
				{
					CityTownSuggestionControlHelper.HookTextControl(address.Object, control, suggestionControl, parentControl);
					suggestionControl.CityTownListView.Dispose();

					control.Text = "changed";
				});
			}
		}

		public void TestShouldNotThrowExceptionAndCloseSuggestionControl_WhenDeleteAddress()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();

			var cityTowns = new CandidateCityTown[1];
			var cityTown = new CandidateCityTown
			{
				City = "Sydney",
				Postcode = "2000",
				State = "NSW",
			};
			cityTowns[0] = cityTown;

			using (var parentControl = new ZDocAddressControl())
			using (var control = new TextBox())
			using (var suggestionControl = new CityTownSuggestionControl(address, cityTowns, "Sydney", "", "", null, null, null))
			{
				CityTownSuggestionControlHelper.HookTextControl(address, control, suggestionControl, parentControl);
				suggestionControl.CityTownListView.Dispose();

				address.Delete();
				control.Text = "changed";
				Assert(suggestionControl.IsDisposed);
			}
		}

		[ExpectNoExceptions]
		public void TestGetCityTownUpdateSuggestedStateWithNullStateInCandidateCityTown()
		{
			using (var applicationForm = new Form())
			{
				var testCountry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "HK"));
				var address = new Mock<ISupportWebAddressValidation>();
				address.Setup(m => m.Country).Returns(testCountry);
				var candidate = new CandidateCityTown();

				CityTownSuggestionControlHelper.UpdateSuggestedState(candidate, address.Object);
			}
		}

		public void TestGetCityTownUpdateSuggestedStateWithPunctuationsNormalized()
		{
			var code = "95";
			var query = new ZQuery(RefCountryStatesSchema.RW_Code, code)
				.AddToFilter(RefCountryStatesSchema.RW_Description, "Val-d'Oise")
				.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, "FR");

			if (!Factory.Exists(typeof(RefCountryStates), query))
			{
				var refCountryStates = Factory.NewWithValidTestData<RefCountryStates>();
				refCountryStates.RW_Code = "95";
				refCountryStates.RW_Description = "Val-d'Oise";
				refCountryStates.RW_RN_NKCountryCode = "FR";
			}

			var cityTown = new CandidateCityTown
			{
				State = "Val-d’Oise"
			};

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "FR";
			CityTownSuggestionControlHelper.UpdateSuggestedState(cityTown, address);
			AssertEquals(code, cityTown.State);
		}

		[ExpectNoExceptions]
		public void TestGetCityTownAsyncWithDeletedOrDetachedAddress()
		{
			var cityTown = new CandidateCityTown
			{
				City = "Sydney",
				Postcode = "2000",
				State = "NSW",
			};

			Action<ISupportWebAddressValidation> myAddressHandler = delegate(ISupportWebAddressValidation address)
			{
				var bo = address as BusinessObject;
				AssertNotNull(bo);
				bo.Delete();
			};

			CityTownSuggestionControlHelper.AddressHandlerForTest += myAddressHandler;

			CityTownSuggestionControlHelper.FakeResult = new CandidateCityTown[] { cityTown };

			try
			{
				using (var applicationForm = new Form())
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.IsUpdatingCityTown = false;
					address.ValidationStatus = AddressValidationStatus.ToBeVerified;
					Factory.Save();
					var t = CityTownSuggestionControlHelper.GetCityTownAsync(new CancellationTokenSource(), address, null, applicationForm, applicationForm.Controls, null);
				}

				using (var applicationForm = new Form())
				{
					var address = Factory.NewWithValidTestData<JobDocAddress>();
					address.IsUpdatingCityTown = false;
					address.ValidationStatus = AddressValidationStatus.ToBeVerified;
					var t = CityTownSuggestionControlHelper.GetCityTownAsync(new CancellationTokenSource(), address, null, applicationForm, applicationForm.Controls, null);
				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				CityTownSuggestionControlHelper.AddressHandlerForTest -= myAddressHandler;
				CityTownSuggestionControlHelper.FakeResult = null;
			}
		}

		public void TestFillStateWithSuggestion()
		{
			var exactMatch = true;
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;
			AssertStateValue(!exactMatch, String.Empty, String.Empty);
			AssertStateValue(!exactMatch, "ACT", "ACT");
			AssertStateValue(exactMatch, String.Empty, String.Empty);
			AssertStateValue(exactMatch, "ACT", String.Empty);

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
			AssertStateValue(!exactMatch, String.Empty, "NSW");
			AssertStateValue(!exactMatch, "ACT", "ACT");
			AssertStateValue(exactMatch, String.Empty, "NSW");
			AssertStateValue(exactMatch, "ACT", "NSW");

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			AssertStateValue(!exactMatch, String.Empty, "NSW");
			AssertStateValue(!exactMatch, "ACT", "ACT");
			AssertStateValue(exactMatch, String.Empty, "NSW");
			AssertStateValue(exactMatch, "ACT", "NSW");
		}

		[ExpectNoExceptions]
		public void TestGetCityTownAsync_WhenAddressIsDeleted_ShouldNotThrowException()
		{
			// Arrange.

			CityTownSuggestionControlHelper.FakeResult = new[]
			{
				new CandidateCityTown
				{
					City = "ALEXANDRIA",
					Postcode = "2015",
					State = "NSW"
				}
			};

			var organization = Factory.New<OrgHeader>();
			var resetEvent = new AutoResetEvent(false);

			using (var form = new ZChildForm(organization))
			using (var addressControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(addressControl);
				form.Show();

				var address = organization.Addresses.AddNew();
				address.OA_Address1 = "72 O'RIORDAN STREET";
				address.OA_Address2 = "WISETECH GLOBAL";
				address.OA_PostCode = "2015";
				address.OA_City = "ALEXANDRIA";
				address.OA_State = "NSW";
				address.OA_RN_NKCountryCode = "AU";

				address.Delete();

				// Act & Assert.

				Task.Factory.StartNew(async () =>
				{
					try
					{
						await CityTownSuggestionControlHelper.GetCityTownAsync(
							new CancellationTokenSource(),
							address,
							addressControl,
							form,
							addressControl.AddressFieldControls,
							() => { });
					}
					finally
					{
						resetEvent.Set();
					}
				});
			}

			resetEvent.WaitOne();
		}

		void AssertStateValue(bool exactMatch, string preValue, string expectedValue)
		{
			var cityTownSuggestions = new CandidateCityTown[1] { new CandidateCityTown { City = "Sydney", Postcode = "0001", State = "NSW" } };
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_State = preValue;
			AssertEquals("Precondition", preValue, address.OA_State);

			if (exactMatch)
			{
				CityTownSuggestionControlHelper.FillAddressWithExactMatchSuggestion(address, cityTownSuggestions);
			}
			else
			{
				CityTownSuggestionControlHelper.AutoFillStateIfPossible(cityTownSuggestions, address);
			}
			AssertEquals(expectedValue, address.OA_State);
		}
	}
}
