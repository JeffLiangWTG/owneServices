using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Model;
using static WiseRates.Api.Model.RatesSearchRequest;

namespace Enterprise.Rating.Business
{
	public class WiseRatingHeaderView : NonPersistentBusinessObject, IRatingHeader, INeedCodeMappings
	{
		public WiseRatingHeaderView(WiseRatesProvider provider, BusinessObjectFactory factory, ElementaryLogger logger) : base(factory)
		{
			this.Logger = logger;
			this.Provider = provider;
			this.WiseEntryViews = new WiseEntryViewsCollection();
		}

		/// <summary>
		/// This is the listing of WiseEntry items that are displayed for the results in the
		/// multi modal search form.
		/// </summary>
		public WiseEntryViewsCollection WiseEntryViews { get; }

		public ElementaryLogger Logger { get; }
		public WiseRatesProvider Provider { get; }

		public OrgHeader Header => GlbCompany.CurrentCompany.OrgProxy;

		public ZString TH_QuoteNumber => ZString.Empty;

		public ZByte TH_GlobalRateLevel => ZByte.Zero;

		public ZGuid TH_OH => GlbCompany.CurrentCompany.OrgProxy.PK;

		public GlbCompany Company => GlbCompany.CurrentCompany;

		public ZString TH_GlobalRateDescription => ZString.Empty;

		public MultilingualString TH_GlobalRateDescriptionMultilingual => ResString.GetMultilingualString("f8a3f0c7-83d5-4114-92a8-e03f209ff1e2", "Wise Rates");

		public ZString RatingHeaderTypeDescription => ZString.Empty;
		public ZString DisplayInfo() => TH_GlobalRateDescriptionMultilingual;

		public ZBool TH_OneTimeQuote => false;

		public IEnumerable<IRateEntry> ChildRateEntries => WiseEntryViews.Cast<IRateEntry>();

		public ZString TH_RateType => "WRC";

		public string InvalidReason => string.Empty;

		#region SuppressResourceStringsCheckRegion

		public void SendRatesRequest(List<RatesQuery> ratesQueries)
		{
			var contextOperation = Operation.WiseRateSearch;

			RatesSearchResponseDTO totalResponse = null;
			var allConvertedEntries = new List<WiseEntry>();

			foreach (var ratesQuery in ratesQueries)
			{
				var convertedEntries = Provider.GetRates(ratesQuery, null, contextOperation, out var response);

				if (response != null)
				{
					totalResponse = MergeResponseDto(totalResponse, response);
				}

				if (convertedEntries != null && convertedEntries.Any())
				{
					allConvertedEntries.AddRange(convertedEntries);
				}
			}

			SearchResponse = totalResponse;
			PopulateWiseEntryViews(allConvertedEntries);
		}

		RatesSearchResponseDTO MergeResponseDto(RatesSearchResponseDTO resA, RatesSearchResponseDTO resB)
		{
			var rawResponses = string.Concat(resA?.RawResponse, resB?.RawResponse);

			if (resA?.RatesSearchResponse == null)
			{
				resB.RawResponse = rawResponses;
				return resB;
			}

			if (resB?.RatesSearchResponse == null)
			{
				resA.RawResponse = rawResponses;
				return resA;
			}

			var result = new RatesSearchResponseDTO();

			result.RatesSearchResponse = new RatesSearchResponse();
			result.RatesSearchResponse.ChargeCodes = resA.RatesSearchResponse.ChargeCodes.Union(resB.RatesSearchResponse.ChargeCodes).ToArray();
			result.RatesSearchResponse.CargoSphereContracts = resA.RatesSearchResponse.CargoSphereContracts.Union(resB.RatesSearchResponse.CargoSphereContracts).ToArray();
			result.RatesSearchResponse.CargoSphereNamedAccounts = resA.RatesSearchResponse.CargoSphereNamedAccounts.Union(resB.RatesSearchResponse.CargoSphereNamedAccounts).ToArray();
			result.RatesSearchResponse.Carriers = resA.RatesSearchResponse.Carriers.Union(resB.RatesSearchResponse.Carriers).ToArray();
			result.RatesSearchResponse.Providers = resA.RatesSearchResponse.Providers.Union(resB.RatesSearchResponse.Providers).ToArray();
			result.RatesSearchResponse.Rates = resA.RatesSearchResponse.Rates.Union(resB.RatesSearchResponse.Rates).ToArray();
			result.RatesSearchResponse.ServiceLevels = resA.RatesSearchResponse.ServiceLevels.Union(resB.RatesSearchResponse.ServiceLevels).ToArray();
			result.RatesSearchResponse.Warnings = resA.RatesSearchResponse.Warnings.Union(resB.RatesSearchResponse.Warnings).ToArray();
			result.RawResponse = rawResponses;
			result.TraceID = resA.TraceID + ", " + resB.TraceID;

			return result;
		}

		void PopulateWiseEntryViews(IEnumerable<WiseEntry> convertedEntries)
		{
			WiseEntryViews.RemoveAndDeleteAll();
			WiseEntryViews.AddRange(convertedEntries.Select(x => new WiseEntryView(x, SearchResponse.RatesSearchResponse)));
			this.RunPreSaveValidation();
		}

		public void RecalculateWiseEntryViewsFromLastResponse()
		{
			var convertedEntries = Provider.ConvertRates(SearchResponse, null);
			PopulateWiseEntryViews(convertedEntries);
		}

		#endregion

		public RatesSearchResponseDTO SearchResponse { get; private set; }

		public IEnumerable<IRateEntry> LoadRateEntriesForAutoRater(ZQuery odFilter)
		{
			return ChildRateEntries;
		}

		public WiseRatingHeaderViewValidation Validation => new WiseRatingHeaderViewValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			this.Validation.ValidateAll();
		}

		List<UnmappedForeignCode> INeedCodeMappings.UnmappedCodes => unmappedCodes;
		readonly List<UnmappedForeignCode> unmappedCodes = new List<UnmappedForeignCode>();

		void INeedCodeMappings.SetUnmappedCodes(IEnumerable<UnmappedForeignCode> codesNeedsMapping)
		{
			unmappedCodes.AddRange(codesNeedsMapping);
		}

		bool INeedCodeMappings.NeedsCodeMapping => false;

		ZGuid INeedCodeMappings.CarrierOrgHeaderPK => ZGuid.Empty;
	}
}
