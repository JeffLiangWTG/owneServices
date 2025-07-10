using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WTG.Logging;
using static Enterprise.Rating.Business.UrsConstants;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(SystemLogProperties))]
namespace Enterprise.Rating.Business.WiseRates;

public class UrsWiseRatesClientWithRatesComparison(IWiseRatesClient wiseRatesClient, IWiseRatesClient ursClient)
		: IWiseRatesClient
{
	readonly IWiseRatesClient wiseRatesClient = wiseRatesClient ?? throw new ArgumentNullException(nameof(wiseRatesClient));
	readonly IWiseRatesClient ursClient = ursClient ?? throw new ArgumentNullException(nameof(ursClient));
	readonly IWiseRatesClient masterClient = RatingFeatureHelper.Urs.IsEnabledForLegacy ? ursClient : wiseRatesClient;

	public string ServiceURL => masterClient.ServiceURL;
	public string AccessToken => masterClient.AccessToken;
	public string LastRequest => masterClient.LastRequest;

	public void Dispose()
	{
		wiseRatesClient.Dispose();
		ursClient.Dispose();
	}

	public async Task<RatesSearchResponse> SearchAsync(RatesSearchRequest request, string correlationID = null, CancellationToken cancellationToken = new CancellationToken())
	{
		var ratesServiceTask = wiseRatesClient.SearchAsync(request, correlationID, cancellationToken).ConfigureAwait(false);
		var ursResult = await ursClient.SearchAsync(request, correlationID, cancellationToken).ConfigureAwait(false);

		try
		{
			var ratesServiceResult = await ratesServiceTask;
			SendCorrelationReport(request.RatesQuery, ratesServiceResult, ursResult, correlationID);
		}
		catch (Exception)
		{
			// Nothing to do, we were calling the secondary service just for correlation report
		}

		return ursResult;
	}

	public RefChargeCode[] GetChargeCodes(string correlationID = null)
	{
		return masterClient.GetChargeCodes(correlationID);
	}

	public Task<RefChargeCode[]> GetChargeCodesAsync(string correlationID = null)
	{
		return masterClient.GetChargeCodesAsync(correlationID);
	}

	public RefCommodityGroup[] GetCommodityGroups(string correlationID = null)
	{
		return masterClient.GetCommodityGroups(correlationID);
	}

	public Task<RefCommodityGroup[]> GetCommodityGroupsAsync(string correlationID = null)
	{
		return masterClient.GetCommodityGroupsAsync(correlationID);
	}

	public RefServiceLevel[] GetServiceLevels(string correlationID = null)
	{
		return masterClient.GetServiceLevels(correlationID);
	}

	public Task<RefServiceLevel[]> GetServiceLevelsAsync(string correlationID = null)
	{
		return masterClient.GetServiceLevelsAsync(correlationID);
	}

	public RatesServiceConfiguration GetConfiguration(string correlationID = null)
	{
		return masterClient.GetConfiguration(correlationID);
	}

	public Task<RatesServiceConfiguration> GetConfigurationAsync(string correlationID = null)
	{
		return masterClient.GetConfigurationAsync(correlationID);
	}

	public ChargeCodeWithMappingInfo[] GetChargeCodesWithMappingInfo(string correlationID = null)
	{
		return masterClient.GetChargeCodesWithMappingInfo(correlationID);
	}

	public Task<ChargeCodeWithMappingInfo[]> GetChargeCodesWithMappingInfoAsync(string correlationID = null)
	{
		return masterClient.GetChargeCodesWithMappingInfoAsync(correlationID);
	}

	public ChargeCodeWithMappingInfo[] GetAllChargeCodesWithMappingInfo(string correlationID = null)
	{
		return masterClient.GetAllChargeCodesWithMappingInfo(correlationID);
	}

	public Task<ChargeCodeWithMappingInfo[]> GetAllChargeCodesWithMappingInfoAsync(string correlationID = null)
	{
		return masterClient.GetAllChargeCodesWithMappingInfoAsync(correlationID);
	}

	public Task SendKafkaMessageAsync(IEnumerable<string> kafkaMessages)
	{
		throw new NotImplementedException();
	}

	public HashSet<string> GetNamedAccounts(string correlationID = null)
	{
		return masterClient.GetNamedAccounts(correlationID);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is only for logs")]
	void SendCorrelationReport(RatesQuery query, RatesSearchResponse ratesServiceResult, RatesSearchResponse ursResult, string correlationId)
	{
		var wiseRates = GenerateKeys(query, ratesServiceResult.Rates, correlationId);
		var ursRates = GenerateKeys(query, ursResult.Rates, correlationId);
		if (wiseRates == null || ursRates == null)
		{
			return;
		}

		var missingRates = wiseRates.Keys.Except(ursRates.Keys).ToList();
		var nonExpectedRates = ursRates.Keys.Except(wiseRates.Keys).ToList();

		var correlationReport = new UrsCorrelationReport
		{
			RatesServiceRatesCount = ratesServiceResult.Rates.Length,
			UrsRatesCount = ursResult.Rates.Length,
			MissingRatesCount = missingRates.Count,
			NonExpectedRatesCount = nonExpectedRates.Count
		};

		if (missingRates.Any())
		{
			correlationReport.MissingRates = string.Join("\n", missingRates.OrderBy(x => x));
		}

		if (nonExpectedRates.Any())
		{
			correlationReport.NonExpectedRates = string.Join("\n", nonExpectedRates.OrderBy(x => x));
		}

		var notes = new List<string>();

		foreach (var pair in ursRates)
		{
			if (!wiseRates.TryGetValue(pair.Key, out var rate))
			{
				continue;
			}

			var wiseCharges = rate.Charges.ToDictionary(charge => GetKey(charge, rate.TransportMode));
			var ursCharges = pair.Value.Charges.ToDictionary(charge => GetKey(charge, pair.Value.TransportMode));

			var wiseChargeKeysOrdered = wiseCharges.Keys.OrderBy(k => k);
			var ursChargeKeysOrdered = ursCharges.Keys.OrderBy(k => k);

			if (wiseCharges.Count != ursCharges.Count || !wiseChargeKeysOrdered.SequenceEqual(ursChargeKeysOrdered))
			{
				var sb = new StringBuilder();
				sb.AppendLine($"URS Rate {pair.Value.Id} doesn't match {rate.Id} Rates Service rate");
				sb.AppendLine();
				sb.AppendLine("Rates Service Charges:");
				wiseChargeKeysOrdered.ForEach(k => sb.AppendLine(k));
				sb.AppendLine();
				sb.AppendLine("URS Charges:");
				ursChargeKeysOrdered.ForEach(k => sb.AppendLine(k));

				correlationReport.InvalidRatesCount++;
				notes.Add(sb.ToString());
			}
		}

		if (notes.Any())
		{
			correlationReport.Notes = string.Join("\r\n============================\r\n", notes);
		}

		SendCorrelationReport(query, correlationId, correlationReport);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is only for logs")]
	Dictionary<string, Rate> GenerateKeys(RatesQuery query, IEnumerable<Rate> rates, string correlationId, bool isUrs = false)
	{
		var groupedRates = rates
			.GroupBy(GetKey)
			.ToDictionary(g => g.Key, g => g.ToList());

		var duplicatedKeys = groupedRates.Where(kvp => kvp.Value.Count > 1).Select(kvp => kvp.Key).ToList();
		if (duplicatedKeys.Any())
		{
			var provider = isUrs ? "URS" : "Rates Service";
			var sb = new StringBuilder();
			sb.AppendLine($"{provider} rates have duplicated keys:");
			duplicatedKeys.ForEach(k => sb.AppendLine(k));
			sb.AppendLine();
			sb.AppendLine("Please update keys generation logic to avoid duplicates, otherwise, it is impossible to correlate rates.");

			var report = new UrsCorrelationReport { Notes = sb.ToString() };

			if (isUrs)
			{
				report.UrsRatesCount = rates.Count();
			}
			else
			{
				report.RatesServiceRatesCount = rates.Count();
			}

			SendCorrelationReport(query, correlationId, report);

			return null;
		}

		return groupedRates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Single());
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This is only for logs")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Doesn't matter here")]
	void SendCorrelationReport(RatesQuery query, string traceID, UrsCorrelationReport correlation)
	{
		var registration = ObjectFactory.Get<IProductRegistration>().Key;
		var errorsCount = correlation.MissingRatesCount + correlation.NonExpectedRatesCount + correlation.InvalidRatesCount;

		var result = errorsCount > 0 ? "failed" : string.IsNullOrEmpty(correlation.Notes) ? "succeeded" : "unknown";

		var fields = new Dictionary<string, object>
		{
			{ SystemLogProperties.TraceId, traceID },
			{ SystemLogProperties.SourceContext, "UrsIntegration" },
			{ SystemLogProperties.UserSystem, registration.EnterpriseCode },
			{ "Result", result },
			{ "RatesServiceUrl", wiseRatesClient.ServiceURL },
			{ "UrsUrl", ursClient.ServiceURL },
			{ "Correlation", correlation },
			{ "RatesQuery", query }
		};

		// Tyring to maintain serilog convention for consistency, although it is not necessary
		var msgData = new Dictionary<string, object>
		{
			{ "timestamp", DateTime.UtcNow },
			{ "messageTemplate", "Correlation between urs rates and rates service rates {Result}" },
			{ "message", $"Correlation between urs rates and rates service rates {result}" },
			{ "fields", fields },
		};

		var msg = JsonConvert.SerializeObject(msgData);
		wiseRatesClient.SendKafkaMessageAsync([msg]).GetAwaiter().GetResult();
	}

	static string GetKey(Rate rate)
	{
		var includedCommodities = string.Join(",", rate.CarrierCommodityInfo?.IncludedCommodities ?? Array.Empty<string>());
		var productCode = rate.ProviderCustomFields.FirstOrDefault(c => c.Code == Rate.CustomFields.Cargoguide.ProductCode)?.Value.ToString() ?? string.Empty;
		var cgReference = rate.ProviderCustomFields.FirstOrDefault(c => c.Code == Rate.CustomFields.Cargoguide.Reference)?.Value.ToString() ?? string.Empty;

		var carrierCommodityInfoGroupType = rate.CarrierCommodityInfo?.GroupType == CommodityCategory.NonHazardous
			? (NoResString)"General cargo"
			: rate.CarrierCommodityInfo?.GroupType ?? string.Empty;

		var keyValues = new[]
		{
			rate.TransportMode,
			rate.ContainerMode,
			rate.Origin,
			rate.Destination,
			rate.Carrier,
			rate.ServiceLevel,
			rate.StartDate.ToString(),
			rate.ExpiryDate?.ToString() ?? string.Empty,
			rate.ContractNumber,
			rate.Commodity,
			rate.CarrierCommodityInfo?.GroupName ?? string.Empty,
			rate.TransportMode == WRConstants.TransportModes.SEA ? carrierCommodityInfoGroupType : string.Empty,
			includedCommodities,
			productCode,
			cgReference,
			rate.Container?.Code,
			string.Join(",", rate.NamedAccounts),
		};

		var key = string.Join("|", keyValues);
		return key;
	}

	static string GetKey(Charge charge, string transportMode)
	{
		// This was done, because WiseRates returns None, and URS was altered to now return Freight done in WI00864654, however comparison is meant to pass.
		var chargeType = charge.ChargeType.Equals(ChargeType.Freight) ? ChargeType.None : charge.ChargeType;

		var keyValues = new object[]
		{
			charge.ChargeCode,
			chargeType,
			charge.FlatRate,
			charge.PerUnitRate,
			charge.MinRate,
			charge.MaxRate,
			charge.MinChargeable,
			transportMode == WRConstants.TransportModes.SEA ? charge.Currency : string.Empty,
			charge.Unit,
			charge.Applicability,
			charge.BreakOperator,
			transportMode == WRConstants.TransportModes.SEA ? charge.BreakUnit : string.Empty,
			charge.Break,
			charge.IsHigherBreakLowerRate,
		};

		return string.Join("|", keyValues);
	}

	public class UrsCorrelationReport
	{
		public int RatesServiceRatesCount { get; set; }
		public int UrsRatesCount { get; set; }

		/// <summary>
		///		Number of rates that are missing in the Urs response comparing to RatesService response
		/// </summary>
		public int MissingRatesCount { get; set; }
		public string MissingRates { get; set; }

		/// <summary>
		///		 Number of rates that are not expected in the Urs response comparing to RatesService response
		/// </summary>
		public int NonExpectedRatesCount { get; set; }
		public string NonExpectedRates { get; set; }

		/// <summary>
		///		 Number of rates that exist in both responses but prices are different in Urs ones.
		/// </summary>
		public int InvalidRatesCount { get; set; }
		public string Notes { get; set; }
	}
}
