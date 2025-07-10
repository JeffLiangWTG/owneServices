using System;
using System.Linq;
using System.Windows;
using CargoWise.Common;
using Enterprise.Rating.Business;
using NUnit.Framework;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools.Enums;

namespace Enterprise.Rating.GUI.Testing
{
	public class WiseRatesSearchViewModelTest : TransactionedTestCase
	{
		public void TestPopulatePieModel()
		{
			var searchResponse = new RatesSearchResponseDTO
			{
				RatesSearchResponse = new RatesSearchResponse
				{
					Rates = Array.Empty<Rate>(),
					Providers = Array.Empty<ProviderResult>()
				}
			};

			var model = new WiseRatesSearchResultViewModel(searchResponse, Enumerable.Empty<string>());

			AssertEquals(0, model.RatesServiceProviderOutcomes.Count);

			searchResponse = new RatesSearchResponseDTO
			{
				RatesSearchResponse = new RatesSearchResponse
				{
					Rates = new[]
					{
						new Rate { Provider = WRConstants.RateProviders.WTG },
						new Rate { Provider = WRConstants.RateProviders.WTG },
						new Rate { Provider = WRConstants.RateProviders.CargoSphere },
						new Rate { Provider = WRConstants.RateProviders.CargoSphere },
						new Rate { Provider = WRConstants.RateProviders.CargoSphere },
						new Rate { Provider = WRConstants.RateProviders.CargoGuide },
						new Rate { Provider = WRConstants.RateProviders.CargoGuide },
						new Rate { Provider = WRConstants.RateProviders.CargoGuide },
						new Rate { Provider = WRConstants.RateProviders.CargoGuide }
					},
					Providers = new[]
					{
						new ProviderResult
						{
							ProviderCode = WRConstants.RateProviders.WTG,
							ProviderName = "WiseTech Global",
							ConnectionResult = ConnectionResult.Success,
							ElapsedMilliseconds = 1000,
						},
						new ProviderResult
						{
							ProviderCode = WRConstants.RateProviders.CargoSphere,
							ProviderName = "CargoSphere",
							ConnectionResult = ConnectionResult.PartialSuccess,
							ElapsedMilliseconds = 2000,
						},
						new ProviderResult
						{
							ProviderCode = WRConstants.RateProviders.CargoGuide,
							ProviderName = "Cargoguide",
							ConnectionResult = ConnectionResult.Success,
							ElapsedMilliseconds = 505,
						},
						new ProviderResult
						{
							ProviderCode = "CGVM",
							ProviderName = "CarGoVroom",
							ConnectionResult = ConnectionResult.Failure,
							ElapsedMilliseconds = 30000,
						},
					}
				}
			};

			model = new WiseRatesSearchResultViewModel(searchResponse, Enumerable.Empty<string>());

			AssertEquals("2 rates (1.00 s)", model.RatesServiceProviderOutcomes[0].RatesAndSpeed);
			AssertEquals("3 rates (2.00 s)", model.RatesServiceProviderOutcomes[1].RatesAndSpeed);
			AssertEquals("4 rates (0.51 s)", model.RatesServiceProviderOutcomes[2].RatesAndSpeed);
			AssertEquals("0 rates (30.00 s)", model.RatesServiceProviderOutcomes[3].RatesAndSpeed);

			AssertEquals("WiseTech Global", model.RatesServiceProviderOutcomes[0].ProviderName);
			AssertEquals("CargoSphere", model.RatesServiceProviderOutcomes[1].ProviderName);
			AssertEquals("Cargoguide", model.RatesServiceProviderOutcomes[2].ProviderName);
			AssertEquals("CarGoVroom", model.RatesServiceProviderOutcomes[3].ProviderName);
		}

		public void TestIncludesRawDataRegistryOption()
		{
			var searchResponse = new RatesSearchResponseDTO
			{
				RatesSearchResponse = new RatesSearchResponse
				{
					Rates = new[]
					{
						new Rate { Provider = WRConstants.RateProviders.WTG },
						new Rate { Provider = WRConstants.RateProviders.CargoGuide }
					},
					Providers = new[]
					{
						new ProviderResult
						{
							ConnectionResult = ConnectionResult.Success,
							ElapsedMilliseconds = 1000,
							ProviderCode = WRConstants.RateProviders.WTG,
							ProviderName = "WiseTech Global"
						},
						new ProviderResult
						{
							ConnectionResult = ConnectionResult.Success,
							ElapsedMilliseconds = 505,
							ProviderName = "CargoGuide",
							ProviderCode = WRConstants.RateProviders.CargoGuide,
						},
					},
				},
				RawResponse = "...RawResponse"
			};

			WiseRatesSearchResultViewModel model;

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				model = new WiseRatesSearchResultViewModel(searchResponse, Enumerable.Empty<string>());
				AssertEquals(searchResponse.RawResponse, model.RawData);
				AssertEquals(Visibility.Visible, model.IncludeRawData);
			}

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				model = new WiseRatesSearchResultViewModel(searchResponse, Enumerable.Empty<string>());
				AssertEquals(Visibility.Hidden, model.IncludeRawData);
			}
		}

		public void TestIncludesRawDataIsHiddenWhenEmpty()
		{
			var searchResponse = new RatesSearchResponseDTO
			{
				RatesSearchResponse = new RatesSearchResponse
				{
					Rates = new[]
					{
						new Rate { Provider = WRConstants.RateProviders.WTG },
						new Rate { Provider = WRConstants.RateProviders.CargoGuide }
					},
					Providers = new[]
					{
						new ProviderResult
						{
							ConnectionResult = ConnectionResult.Success,
							ElapsedMilliseconds = 1000,
							ProviderCode = WRConstants.RateProviders.WTG,
							ProviderName = "WiseTech Global"
						},
						new ProviderResult
						{
							ConnectionResult = ConnectionResult.Success,
							ElapsedMilliseconds = 505,
							ProviderName = "CargoGuide",
							ProviderCode = WRConstants.RateProviders.CargoGuide,
						},
					}
				}
			};

			WiseRatesSearchResultViewModel model;
			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				model = new WiseRatesSearchResultViewModel(searchResponse, Enumerable.Empty<string>());
				AssertNullOrEmpty(model.RawData);
				AssertEquals(Visibility.Hidden, model.IncludeRawData);
			}
		}

		public void TestDuplicatedRates()
		{
			var searchResponse = new RatesSearchResponseDTO
			{
				RatesSearchResponse = new RatesSearchResponse
				{
					Rates = Array.Empty<Rate>(),
					Providers = Array.Empty<ProviderResult>()
				}
			};

			var rates = new[]
			{
				new Rate { Provider = WRConstants.RateProviders.WTG },
				new Rate { Provider = WRConstants.RateProviders.WTG },
				new Rate { Provider = WRConstants.RateProviders.CargoSphere },
				new Rate { Provider = WRConstants.RateProviders.CargoSphere },
				new Rate { Provider = WRConstants.RateProviders.CargoSphere },
			};

			var providers = new[]
			{
				new ProviderResult { ConnectionResult = ConnectionResult.Success, ElapsedMilliseconds = 1000, ProviderCode = WRConstants.RateProviders.WTG, ProviderName = "WiseTech Global" },
				new ProviderResult { ConnectionResult = ConnectionResult.Success, ElapsedMilliseconds = 2000, ProviderCode = WRConstants.RateProviders.CargoSphere, ProviderName = "CargoSphere" },
				new ProviderResult { ConnectionResult = ConnectionResult.Success, ElapsedMilliseconds = 505, ProviderCode = WRConstants.RateProviders.CargoSphere, ProviderName = "CargoSphere" },
			};

			searchResponse.RatesSearchResponse.Rates = rates;
			searchResponse.RatesSearchResponse.Providers = providers;

			var model = new WiseRatesSearchResultViewModel(searchResponse, Enumerable.Empty<string>());

			AssertEquals("Duplicate provider CGSP", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			providers[1].ProviderCode = WRConstants.RateProviders.IATA;
			providers[1].ProviderName = "IATA";

			model = new WiseRatesSearchResultViewModel(searchResponse, Enumerable.Empty<string>());

			AssertEquals("WiseTech Global", model.RatesServiceProviderOutcomes[0].ProviderName);
			AssertEquals("IATA", model.RatesServiceProviderOutcomes[1].ProviderName);
			AssertEquals("CargoSphere", model.RatesServiceProviderOutcomes[2].ProviderName);
		}

		public void TestNullResponseWithWarnings()
		{
			var warnings = new[] { "warning0", "warning1" };
			WiseRatesSearchResultViewModel model = new WiseRatesSearchResultViewModel(null, warnings);
			AssertEquals(warnings, model.ErrorsOrWarnings);
		}
	}
}
