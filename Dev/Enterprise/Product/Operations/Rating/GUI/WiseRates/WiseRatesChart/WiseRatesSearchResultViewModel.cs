using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CargoWise.Common;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.Rating.GUI
{
	public class WiseRatesSearchResultViewModel
	{
		/// <summary>
		///		A constructor for XAML designer. It setups some test data. 
		/// </summary>
		public WiseRatesSearchResultViewModel()
		{
			RatesServiceProviderOutcomes = new Collection<RatesServiceProviderOutcomeRow>(new[]
				{
					new RatesServiceProviderOutcomeRow
					{
						ProviderCode = "WTG",					// Not translatable
						ProviderName = (NoResString)"WiseTech Global",		// Not translatable
						RatesCount = 123,
						ColorBrush = Brushes.Gold,
						ElapsedSeconds = 2.035f
					},
					new RatesServiceProviderOutcomeRow
					{
						ProviderCode = "CGSG",					// Not translatable
						ProviderName = "CargoSphere",			// Not translatable
						RatesCount = 13,
						ColorBrush = Brushes.Green,
						ElapsedSeconds = 66.21f,
					},
					new RatesServiceProviderOutcomeRow
					{
						ProviderCode = "CCGD",					// Not translatable
						ProviderName = (NoResString)"Cargoguide",			// Not translatable
						RatesCount = 66,
						ColorBrush = Brushes.Red,
						ElapsedSeconds = 982.9f,
					}
				});

			ErrorsOrWarnings = new List<string>()
			{
				(NoResString)"Warning 1",					// Not translatable
				(NoResString)"Warning 2",					// Not translatable
			};

			RawData = (NoResString)@"{ 
	""Request"":
	{
		""Content"": """",
		""Method"": ""POST"",
		""URL"":  ""http://localhost/api/v1/rates/search"",
	},

	""Response"":
	{
		""Content"": new[]
		{
			new ResponseInfo
			{
				Request = new HttpRequestInfo
				{
					""Content"" = ""Marco"",
					""Headers"" = {},
					""Method"" = ""GET"",
					""URL"" = ""localhost"",
					""Timestamp"" = ""2018-01-01T00:00:00""
				},
				""Response"":
				{
					""Content"": ""Polo"",
					""Headers"": {},
					""StatusCode"": ""200"",
					""ReasonPhrase"": "",
					""Timestamp"" = ""2018-01-02t00:00:00""
				},
			},
		},
		""StatusCode"": ""OK"",
		""ReasonPhrase"": ""OK"",
		""Timestamp"": ""2018, 1, 2""
	}
}"; // Not translatable

			IncludeRawData = Visibility.Visible;
		}

		public WiseRatesSearchResultViewModel(RatesSearchResponseDTO searchResponse, IEnumerable<string> errorsOrWarnings)
		{
			RatesServiceProviderOutcomes = new Collection<RatesServiceProviderOutcomeRow>();
			ErrorsOrWarnings = errorsOrWarnings;

			if (searchResponse?.RatesSearchResponse != null)
			{
				var providerCodes = new HashSet<string>();

				foreach (var provider in searchResponse.RatesSearchResponse.Providers)
				{
					if (providerCodes.Add(provider.ProviderCode))
					{
						var serviceProviderRates = searchResponse.RatesSearchResponse.Rates?.Where(r => r.Provider == provider.ProviderCode).ToArray() ?? Array.Empty<Rate>();
						var brush = GetColorBrushForProvider(provider.ProviderCode);
						RatesServiceProviderOutcomes.Add(new RatesServiceProviderOutcomeRow(provider, serviceProviderRates, brush));
					}
					else
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"Duplicate provider {provider.ProviderCode}")); // Error log message
					}
				}
			}

			RawData = searchResponse?.RawResponse;
			IncludeRawData = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value && !RawData.IsNullOrEmpty() ? Visibility.Visible : Visibility.Hidden;
		}

		public Collection<RatesServiceProviderOutcomeRow> RatesServiceProviderOutcomes { get; private set; }
		public IEnumerable<string> ErrorsOrWarnings { get; internal set; }
		public string RawData { get; internal set; }
		public Visibility IncludeRawData { get; internal set; }

		public Visibility ShowRatesSearchErrorsOrWarningsLink => ErrorsOrWarnings.Any(errorsOrWarning => !string.IsNullOrEmpty(errorsOrWarning)) ? Visibility.Visible : Visibility.Hidden;

		SolidColorBrush GetColorBrushForProvider(string provider)
		{
			switch (provider)
			{
				case WRConstants.RateProviders.WTG:
					return Brushes.Gold;

				case WRConstants.RateProviders.CargoSphere:
					return Brushes.YellowGreen;

				case WRConstants.RateProviders.CargoGuide:
					return Brushes.Crimson;

				case WRConstants.RateProviders.IATA:
					return Brushes.MediumAquamarine;

				default:
					return Brushes.White;
			}
		}

		public MultilingualString MultilingualNoRatesFound =>
			ResString.GetMultilingualString("c9ed096d-aa3d-4691-a25c-78072f741db3", "No rates found");

		public MultilingualString MultilingualSearchResults =>
			ResString.GetMultilingualString("29a629a6-cb45-4856-b2ba-72247f73dd79", "Search Results");

		public MultilingualString MultilingualRatesSearchErrorsOrWarnings => GetMultilingualRatesSearchErrorsOrWarnings(ErrorsOrWarnings);

		public static MultilingualString GetMultilingualRatesSearchErrorsOrWarnings(IEnumerable<string> errorsOrWarnings)
			=> ResString.GetMultilingualString("ac54b69c-a691-11e8-839a-1c1b0d09faa1", "Rates Search Errors/Warnings ({0})", errorsOrWarnings?.Count() ?? 0);

		public static MultilingualString GetMultilingualRawData()
			=> ResString.GetMultilingualString("B04E02E8-21C2-4729-B279-675C2DA66A4E", "Raw Data");
		public MultilingualString MultilingualRawData => GetMultilingualRawData();
	}

	public class RatesServiceProviderOutcomeRow
	{
		public RatesServiceProviderOutcomeRow()
		{
		}

		public RatesServiceProviderOutcomeRow(ProviderResult provider, Rate[] rates, SolidColorBrush colorBrush)
		{
			ProviderName = provider.ProviderName;
			ProviderCode = provider.ProviderCode;
			ColorBrush = colorBrush;
			RatesCount = rates.Length;
			ElapsedSeconds = provider.ElapsedMilliseconds / 1000f;
		}

		public string ProviderName { get; internal set; }
		public string ProviderCode { get; internal set; }
		public int RatesCount { get; internal set; }
		public SolidColorBrush ColorBrush { get; internal set; }

		public MultilingualString RatesAndSpeed =>
			ResString.GetMultilingualString("4b9e0a49-cb1d-4c9c-8d9b-87ebdd6c5db1", "{0} rates ({1:f2} s)", RatesCount, ElapsedSeconds);

		public float ElapsedSeconds { get; internal set; }
	}
}

