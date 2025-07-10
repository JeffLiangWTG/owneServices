using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class CityTownSuggestionControlUtilTest : TestCaseWithFactory
	{
		public void TestDoNotGetCityTownAsync()
		{
			AssertDoNotGetCityTownAsyncCore(null);

			var address1 = GetAddressForTest();
			address1.IsUpdatingCityTown = true;
			AssertDoNotGetCityTownAsyncCore(address1);

			var mock = new Mock<NonPersistentBusinessObject>().As<ISupportWebAddressValidation>();
			mock.Setup(x => x.City).Returns("City");
			mock.Setup(x => x.Postcode).Returns("P0");
			mock.Setup(x => x.IsUpdatingCityTown).Returns(false);
			mock.Setup(x => x.ValidationStatus).Returns(AddressValidationStatus.Invalid);
			var address2 = mock.Object;

			AssertDoNotGetCityTownAsyncCore(address2);

			var address3 = GetAddressForTest();
			address3.ValidationStatus = AddressValidationStatus.Verified;
			AssertDoNotGetCityTownAsyncCore(address3);

			var address4 = GetAddressForTest();
			address4.City = string.Empty;
			address4.Postcode = string.Empty;
			AssertDoNotGetCityTownAsyncCore(address4);
		}

		public void TestDoNotGetCityTownAsync_WhenCurrentTabIsNotSelected()
		{
			var config = new AddressValidatorConfiguration().WithCurrentSelectedTabCheck(() => false);

			using (var parentControl = new ControlSupportAddressValidationForTest(GetAddressForTest()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config))
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.GetCityTownAsync();

				AssertEquals(false, suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
			}
		}

		public void TestServiceResultIsEmpty()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(GetAddressForTest()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.ServiceResultForTesting = "[]";
				suggestionControlUtil.GetCityTownAsync();

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
			}
		}

		public void TestRegisterPropertyChangedEvent()
		{
			var handlerClearedBeforeRegistering = false;
			var registeredTaskCalled = false;

			var mock = new Mock<ISupportWebAddressValidation>();
			mock.Setup(m => m.ClearWebGetCityTownHandler()).Callback(() => handlerClearedBeforeRegistering = true);
			var address = mock.Object;

			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				suggestionControlUtil.RegisterPropertyChangedEvent(() =>
				{
					registeredTaskCalled = true;
					return Task.CompletedTask;
				});

				Assert(handlerClearedBeforeRegistering);

				mock.Setup(m => m.NeedValidation).Returns(true);
				mock.Raise(m => m.TriggerWebGetCityTown += null, mock.Object, EventArgs.Empty);

				Assert(registeredTaskCalled);
			}
		}

		public void TestRegisterPropertyChangedEvent_AddressIsNull()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest())
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				AssertNoExceptionThrown(() => suggestionControlUtil.RegisterPropertyChangedEvent(() => Task.CompletedTask));
			}
		}

		public void TestHasExactMatch()
		{
			var config = new AddressValidatorConfiguration();

			using (var parentControl = new ControlSupportAddressValidationForTest(GetAddressForTest()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config))
			{
				parentForm.Controls.Add(parentControl);
				parentControl.Address1Control.Select();

				var cityTownSelected = false;
				config.WithSelectCityTownAction(() => cityTownSelected = true);
				parentControl.StateControl.Text = "STATE FOR TEST2";
				parentControl.CityControl.Text = "CITY FOR TEST2";
				parentControl.PostcodeControl.Text = "5678";

				suggestionControlUtil.ServiceResultForTesting = @"
				[{
					""State"": ""State For Test1"",
					""City"": ""City For Test1"",
					""Postcode"": ""1234"",
					""DiagnosticMessage"": null
				}, {
					""State"": ""State For Test2"",
					""City"": ""City For Test2"",
					""Postcode"": ""5678"",
					""DiagnosticMessage"": null
				}]";

				suggestionControlUtil.GetCityTownAsync();

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals("STATE FOR TEST2", parentControl.AddressForValidation.State);
				AssertEquals("CITY FOR TEST2", parentControl.AddressForValidation.City);
				AssertEquals("5678", parentControl.AddressForValidation.Postcode);
				Assert(cityTownSelected);
			}
		}

		public void TestFillAddressWithSuggestion()
		{
			var config = new AddressValidatorConfiguration();
			var address = GetAddressForTest();
			address.City = string.Empty;

			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config))
			{
				parentForm.Controls.Add(parentControl);
				parentControl.Address1Control.Select();

				var cityTownSelected = false;
				config.WithSelectCityTownAction(() => cityTownSelected = true);

				suggestionControlUtil.ServiceResultForTesting = @"
				[{
					""State"": ""State For Test"",
					""City"": ""City For Test"",
					""Postcode"": ""1234"",
					""DiagnosticMessage"": null
				}]";

				suggestionControlUtil.GetCityTownAsync();

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals("STATE FOR TEST", parentControl.AddressForValidation.State);
				AssertEquals("CITY FOR TEST", parentControl.AddressForValidation.City);
				AssertEquals("1234", parentControl.AddressForValidation.Postcode);
				Assert(cityTownSelected);

				parentControl.AddressForValidation.State = string.Empty;
				parentControl.AddressForValidation.CountryCodeISO2 = "AU";
				parentControl.AddressForValidation.Country.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;

				suggestionControlUtil.GetCityTownAsync();

				AssertEquals(string.Empty, parentControl.AddressForValidation.State);
			}
		}

		public void TestCreateAndShowSuggestionControl()
		{
			var config = new AddressValidatorConfiguration();

			using (var parentControl = new ControlSupportAddressValidationForTest(GetAddressForTest()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config))
			{
				parentForm.Controls.Add(parentControl);
				parentControl.Address1Control.Select();

				parentControl.AddressForValidation.State = string.Empty;

				suggestionControlUtil.ServiceResultForTesting = @"
				[{
					""State"": ""State"",
					""City"": ""City1"",
					""Postcode"": ""1234"",
					""DiagnosticMessage"": null
				}, {
					""State"": ""State"",
					""City"": ""City2"",
					""Postcode"": ""1234"",
					""DiagnosticMessage"": null
				}]";

				parentControl.AddressForValidation.CountryCodeISO2 = string.Empty;
				suggestionControlUtil.GetCityTownAsync();

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals("STATE", parentControl.AddressForValidation.State);
				suggestionControlUtil.CloseSuggestionControlIfExists();

				parentControl.AddressForValidation.State = string.Empty;
				parentControl.AddressForValidation.CountryCodeISO2 = "AU";
				suggestionControlUtil.GetCityTownAsync();

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals("STATE", parentControl.AddressForValidation.State);
				suggestionControlUtil.CloseSuggestionControlIfExists();

				parentControl.AddressForValidation.State = string.Empty;
				parentControl.AddressForValidation.Country.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;
				suggestionControlUtil.GetCityTownAsync();

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNotNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(string.Empty, parentControl.AddressForValidation.State);
			}
		}

		public void TestDoNotUpdatingCityTown()
		{
			var config = new AddressValidatorConfiguration();

			using (var parentControl = new ControlSupportAddressValidationForTest(GetAddressForTest()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, config))
			{
				parentForm.Controls.Add(parentControl);
				parentControl.ActiveControl = null;

				var cityTownSelected = false;
				config.WithSelectCityTownAction(() => cityTownSelected = true);

				suggestionControlUtil.ServiceResultForTesting = @"
				[{
					""State"": ""State"",
					""City"": ""City"",
					""Postcode"": ""1234"",
					""DiagnosticMessage"": null
				}]";

				suggestionControlUtil.GetCityTownAsync();

				Assert(suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
				AssertEquals(false, cityTownSelected);
			}
		}

		public void TestDoNotUpdateSuggestedState()
		{
			var mock = new Mock<ISupportWebAddressValidation>();
			var address = mock.Object;

			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				mock.Setup(m => m.GetCityTownAsync(It.IsAny<CancellationTokenSource>()))
					.Returns(Task.FromResult(new[] { new CandidateCityTown { State = "NSW" } }));
				mock.As<ISupportWebAddressValidation>().Setup(m => m.State_MaxLength).Returns(2);
				mock.Setup(m => m.Country).Returns(default(RefCountry));

				var candidates = suggestionControlUtil.GetCityTownCoreExposed();

				AssertEquals("NSW", candidates.Single().State);
			}
		}

		public void TestUpdateSuggestedState()
		{
			var mock = new Mock<ISupportWebAddressValidation>();
			var address = mock.Object;

			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				mock.Setup(m => m.GetCityTownAsync(It.IsAny<CancellationTokenSource>()))
					.Returns(Task.FromResult(new[] { new CandidateCityTown { State = "NSW" } }));
				mock.Setup(m => m.Country)
					.Returns(Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, "AU") as RefCountry);
				mock.As<ISupportWebAddressValidation>().Setup(m => m.State_MaxLength).Returns(2);
				var candidates = suggestionControlUtil.GetCityTownCoreExposed();
				AssertEquals("NS", candidates.Single().State);
			}
		}

		public void TestUpdateSuggestedState_MatchDescription()
		{
			var mock = new Mock<ISupportWebAddressValidation>();
			var address = mock.Object;

			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				mock.Setup(m => m.GetCityTownAsync(It.IsAny<CancellationTokenSource>()))
					.Returns(Task.FromResult(new[]
					{
						new CandidateCityTown { State = "Ajman", DiagnosticMessage = "1" },
						new CandidateCityTown { State = "Ra’s al Khaymah", DiagnosticMessage = "2" }
					}));
				mock.Setup(m => m.Country)
					.Returns(Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, "AE") as RefCountry);
				mock.As<ISupportWebAddressValidation>().Setup(m => m.State_MaxLength).Returns(2);
				var candidates = suggestionControlUtil.GetCityTownCoreExposed();
				AssertEquals("AJ", candidates.Single(x => x.DiagnosticMessage == "1").State);
				AssertEquals("RK", candidates.Single(x => x.DiagnosticMessage == "2").State);
			}
		}

		public void TestEventHandlerBeenUnregisteredAfterDisposed()
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(GetAddressForTest()))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				parentForm.Controls.Add(parentControl);
				parentControl.Address1Control.Select();

				parentControl.AddressForValidation.State = string.Empty;
				suggestionControlUtil.ServiceResultForTesting = @"
				[{
					""State"": ""State"",
					""City"": ""City1"",
					""Postcode"": ""1234"",
					""DiagnosticMessage"": null
				}, {
					""State"": ""State"",
					""City"": ""City2"",
					""Postcode"": ""1234"",
					""DiagnosticMessage"": null
				}]";
				parentControl.AddressForValidation.CountryCodeISO2 = string.Empty;
				suggestionControlUtil.GetCityTownAsync();

				var control = suggestionControlUtil.FindSuggestionControl();

				var cityTownSelectedFieldInfo = typeof(CityTownSuggestionControl).GetField("CityTownSelected", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
				var cityTownSelected = cityTownSelectedFieldInfo.GetValue(control);

				var cityTownSelectedMethods = (cityTownSelected as Delegate).GetInvocationList().Select(x => x.Method.Name).ToArray();
				AssertEquals(2, cityTownSelectedMethods.Length);
				Assert(cityTownSelectedMethods.Any(x => x.Equals("HandleCityTownSelected")));
				Assert(cityTownSelectedMethods.Any(x => x.StartsWith("<CreateNewSuggestionControl>")));

				control.Dispose();
				cityTownSelected = cityTownSelectedFieldInfo.GetValue(control);
				AssertNull(cityTownSelected);
			}
		}

		#region Implementation

		void AssertDoNotGetCityTownAsyncCore(ISupportWebAddressValidation address)
		{
			using (var parentControl = new ControlSupportAddressValidationForTest(address))
			using (var parentForm = new Form())
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var suggestionControlUtil = new CityTownSuggestionControlUtilForTest(parentControl, parentForm, cancellationTokenSource, new AddressValidatorConfiguration()))
			{
				parentForm.Controls.Add(parentControl);

				suggestionControlUtil.GetCityTownAsync();

				AssertEquals(false, suggestionControlUtil.ServiceHasBeenRequested);
				AssertNull(suggestionControlUtil.FindSuggestionControl());
			}
		}

		ISupportWebAddressValidation GetAddressForTest()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.ValidationStatus = AddressValidationStatus.Invalid;
			orgAddress.City = "City";
			orgAddress.Postcode = "P0";

			return orgAddress;
		}

		#endregion Implementaion
	}

	class CityTownSuggestionControlUtilForTest : CityTownSuggestionControlUtil, IDisposable
	{
		public CityTownSuggestionControlUtilForTest(Control parentControl, Form parentForm, CancellationTokenSource cancellationTokenSource, AddressValidatorConfiguration config)
			: base(parentControl, parentForm, cancellationTokenSource, config)
		{
		}

		public new void GetCityTownAsync()
		{
			ServiceHasBeenRequested = false;
			CityTownCandidates = null;
			AsyncTaskSynchronizer.Run(base.GetCityTownAsync);
		}

		public CandidateCityTown[] GetCityTownCoreExposed() => AsyncTaskSynchronizer.Run(GetCityTownCore);

		protected override async Task<CandidateCityTown[]> GetCityTownCore()
		{
			ServiceHasBeenRequested = true;
			using (SetFakeWebService())
			{
				CityTownCandidates = await base.GetCityTownCore();
			}

			return CityTownCandidates;
		}

		public CandidateCityTown[] CityTownCandidates { get; private set; }

		public bool ServiceHasBeenRequested { get; private set; }

		public void Dispose() => CloseSuggestionControlIfExists();

		public string ServiceResultForTesting { private get; set; }

		DisposableAction SetFakeWebService()
		{
			var serviceAddress = string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort());
			var serviceUri = new Uri(serviceAddress + "GetCandidateCityTowns/");
			var validationServiceForTest = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "GET" },
				Processor = (_, request) => new Tuple<int, string>(200, ServiceResultForTesting),
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
	}
}
