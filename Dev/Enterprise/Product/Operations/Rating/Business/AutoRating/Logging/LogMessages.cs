using System.Diagnostics.CodeAnalysis;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	#region SuppressResourceStringsCheckRegion

	//Log messages not to translate
	public static class LogMessages
	{
		public const string RequestingAccessTokenMessage = "authenticating the client";

		public const string SendingRequestToRatesServiceMessage = "sending request to Rates Service";
		public const string NotApplicableInRebateMode = "Not applicable in rebate calculation mode";
		public const string OnlyApplicableInRebateMode = "Only applicable in rebate calculation mode";
		public const string ExcludedFromAutocosting = "'Exclude from Autocosting' rates do not apply to job";
		public const string InvalidRateRemoved = "Invalid rates cannot be used for calculation";
		public const string ChargeCodeDoesNotBelongToCompany = "The charge does not belong to this company. Please check and validate your rates data. If the problem persists, please raise an incident.";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string ErrorMoreThanOneChargeCode(int length, string foreignCode) => $"Found {length} WiseChargeCodes for {foreignCode}";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string CalculatedRateLine(string displayInfo, string currency, string clientAmount, string agentAmount) => $"calculated RateLine {displayInfo} as {currency} client  {clientAmount}, agent {agentAmount}";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string AddingChargeableAmount(string product, string value, string measureType) => $"{product}: {value} {measureType}";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string EquipmentTypeIrrelevant(ZString lineEquipment, string partyCartageEquipment, ZString criteriaEquipment)
			=> $"line equipment {lineEquipment} doesn't match {partyCartageEquipment} {criteriaEquipment}";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string MessageSubTypeIrrelevant(ZString lineMessageType, ZString lineMessageSubType, ZString criteriaMessageType, ZString criteriaMessageSubType)
			=> $"line message type/subtype {lineMessageType}/{lineMessageSubType} don't match those of criteria {criteriaMessageType}/{criteriaMessageSubType}";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string OrgDoesntUseGroupClientRates(ZString organizationCode)
	=> $"organization {organizationCode} has been marked to not use group client rates";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string AlwaysCharged(AccChargeCode chargeCode, JobInvoicingConsumerType consumerType)
		{
			var alwaysChargedParty = IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode, consumerType == null || consumerType.OverseasAgentApplicable);
			IMultilingualRegistryItem item;

			switch (alwaysChargedParty)
			{
				case ChargedParty.LocalClient:
					item = IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes;
					break;

				case ChargedParty.Agent:
					item = IncoTermRegistry.Instance.ChargeAgentAlwaysCodes;
					break;

				default:
					return string.Empty;
			}

			var chargeCodeText = chargeCode?.AC_Code ?? "missing charge";

			return item != null
			? $"{chargeCodeText} is always charged per {item.LocationMultilingual}"
			: $"{chargeCodeText} is always charged to {alwaysChargedParty}";
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string PointMismatch(string displayInfo, string product, string measureType, string measureDimension, string expectedValue, string actualValue) => $"RateLine {displayInfo} will not rate {product} by {measureType}	reason: expected '{expectedValue}' {measureDimension} while rate is for '{actualValue}' {measureDimension}"; // log message, subject to change, more for support people as of now

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string CannotFindLocalCodeForForeignCode(string codeType, string foreignCode, bool isUnmapped) => $"Neither code mapping nor local code exist for{(isUnmapped ? " Carrier Specific" : string.Empty)} Rates Service {codeType} - '{foreignCode}'";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string IsHazardousValueIsNotCorrect(string foreignCode, string foreignHaz, string localCode, string localHaz) => $"Rates Service code '{foreignCode}' {foreignHaz} cannot be mapped to CW1 code '{localCode}' {localHaz} due to different IsHazardous setting";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string BizOIsNotActive(string code, string bizOName) => $"{bizOName} '{code}' has been mapped but cannot be used due to being inactive";

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "All the arguments are strings so no need for IFormatProvider")]
		public static string WiseRateMandatoryValueIsEmpty(SchemaColumn column) => $"Mandatory value for {column.Name} is missing on Rates Service";
	}

	public static class LogEventTypes
	{
		public const string RateEntryFiltered = "RateEntry Filtered";
		public const string RatingHeaderFound = "RatingHeader Found";
		public const string RateEntriesFound = "RateEntries Found";
		public const string RateLineFound = "RateLine Found";
		public const string RateLineFiltered = "RateLine Filtered";
		public const string RateLineNotFiltered = "RateLine NOT Filtered";
		public const string ChargeCodeMappingSource = "ChargeCode Mapping Source";
	}

	#endregion
}
