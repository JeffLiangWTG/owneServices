using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Cache;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.PaymentBasisExtensions;
using DotNetCache = System.Runtime.Caching;

namespace Enterprise.MasterFiles.Business
{
	public struct PaymentBasis : IEquatable<PaymentBasis>
	{
		public PaymentBasis(Quantity chargeable, RateInfo rateInfo, AdapterType adapterType, ZString adapterID, ZString? chargeableUnitDescription = null, ZString? chargeableDescription = null)
			: this(chargeable, rateInfo, adapterType, adapterID, chargeableUnitDescription, chargeableDescription, null)
		{
		}

		internal PaymentBasis(Quantity chargeable, RateInfo rateInfo, AdapterType adapterType, ZString adapterID, ZString? chargeableUnitDescription, ZString? chargeableDescription, ZString? percentageBaseDescription)
		{
			if (rateInfo.IsEmpty)
			{
				throw new InvalidOperationException("RateInfo should be not empty");
			}

			if (chargeable.IsEmpty && rateInfo.PerUnitRate.HasValue)
			{
				throw new InvalidOperationException("Chargeable should be not empty for per unit rate");
			}

			Chargeable = chargeable;
			UnroundedChargeable = default;
			ChargeableDescription = chargeableDescription;
			PercentageBaseDescription = percentageBaseDescription;
			ChargeableUnitDescription = chargeableUnitDescription;
			RateInfo = rateInfo;
			AdapterType = adapterType;
			AdapterID = adapterID;
			isNotEmpty = true;

			Amount = Calculate(chargeable, rateInfo);
		}

		internal PaymentBasis(PaymentBasis originalBasis, string description)
			: this(originalBasis.Chargeable, originalBasis.RateInfo, originalBasis.AdapterType, originalBasis.AdapterID, originalBasis.ChargeableUnitDescription, description, originalBasis.PercentageBaseDescription)
		{
		}

		PaymentBasis(PaymentBasis originalBasis, RateInfo percentageInfo, ZString percentageBaseDescription)
			: this(new Quantity(originalBasis.Amount, originalBasis.Currency), percentageInfo, originalBasis.AdapterType, originalBasis.AdapterID, originalBasis.ChargeableUnitDescription, originalBasis.ChargeableDescription, percentageBaseDescription)
		{
		}

		internal PaymentBasis(PaymentBasis originalBasis, Quantity updatedChargeable)
			: this(updatedChargeable, originalBasis.RateInfo, originalBasis.AdapterType, originalBasis.AdapterID, originalBasis.ChargeableUnitDescription, originalBasis.ChargeableDescription, originalBasis.PercentageBaseDescription)
		{
		}

		public static PaymentBasis PercentageFromOriginal(PaymentBasis originalBasis, RateInfo percentageInfo)
		{
			if (percentageInfo.IsPercentage && percentageInfo.PerUnitRate != 100)
			{
				return new PaymentBasis(originalBasis, percentageInfo, originalBasis.ToString());
			}

			return originalBasis;
		}

		public static PaymentBasis GetChanged(PaymentBasis paymentBasis, ZDecimal fraction)
		{
			if (fraction != 1)
			{
				return new PaymentBasis(paymentBasis, RateInfo.GetChanged(paymentBasis.RateInfo, fraction), paymentBasis.ToString());
			}

			return paymentBasis;
		}

		static decimal Calculate(Quantity chargeable, RateInfo rateInfo)
		{
			var result = 0m;
			if (rateInfo.PerUnitRate.HasValue)
			{
				result = chargeable.Amount * rateInfo.PerUnitRate.Value;
			}

			if (rateInfo.IsPercentage)
			{
				result = result / 100;
			}

			if (rateInfo.FlatRate.HasValue)
			{
				result += rateInfo.FlatRate.Value;
			}

			return result;
		}

		public ZDecimal Amount { get; }
		public ZString Currency => RateInfo.Currency;
		public Quantity Chargeable { get; }
		public Quantity UnroundedChargeable { get; set; }
		public ZString? ChargeableDescription { get; }
		public ZString? PercentageBaseDescription { get; }
		public ZString? ChargeableUnitDescription { get; }
		public RateInfo RateInfo { get; }
		public AdapterType AdapterType { get; }
		public ZString AdapterID { get; }

		public bool IsEmpty => !isNotEmpty;
		readonly bool isNotEmpty;

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj)
					 && obj is PaymentBasis
					 && Equals((PaymentBasis)obj);
		}

		public bool Equals(PaymentBasis other)
		{
			return isNotEmpty.Equals(other.isNotEmpty)
					 && Chargeable.Equals(other.Chargeable)
					 && RateInfo.Equals(other.RateInfo)
					 && AdapterType.Equals(other.AdapterType)
					 && AdapterID.Equals(other.AdapterID);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = isNotEmpty.GetHashCode();
				hashCode = (hashCode * 397) ^ Chargeable.GetHashCode();
				hashCode = (hashCode * 397) ^ RateInfo.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(PaymentBasis obj1, PaymentBasis obj2) => obj1.Equals(obj2);
		public static bool operator !=(PaymentBasis obj1, PaymentBasis obj2) => !obj1.Equals(obj2);

		public override string ToString()
		{
			return ToFormattedString(true);
		}

		string ToFormattedString(bool showRateInfo)
		{
			if (Chargeable.IsEmpty)
			{
				if (string.IsNullOrEmpty(ChargeableDescription))
				{
					return RateInfo.ToString();
				}

				if (RateInfo.Type == RateInfo.RateInfoType.FLT && RateInfo.FlatRate == 0)
				{
					return ChargeableDescription;
				}

				return Invariant($"{RateInfo} {ChargeableDescription}");
			}

			var chargeableUnitDescription = ChargeableUnitDescription ?? Chargeable.Unit;
			var chargeableLabel = Chargeable.Label.IsEmpty ? string.Empty : Chargeable.Label + " ";
			var chargeableAmountDescription = chargeableLabel + GetFormattedChargeableAmount() + " " + chargeableUnitDescription;
			var additionalChargeableDescription = ChargeableDescription.HasValue && !ChargeableDescription.Value.IsEmpty
				? " " + String.Format(CultureInfo.InvariantCulture, "({0})", ChargeableDescription.Value)
				: string.Empty;

			if (RateInfo.IsPercentage)
			{
				string percentageBaseDesc;
				if (PercentageBaseDescription.HasValue)
				{
					percentageBaseDesc = PercentageBaseDescription.Value;
				}
				else
				{
					var elements = new string[] { ChargeableUnitDescription, Chargeable.Unit, FormatDecimal(Chargeable.Amount), additionalChargeableDescription };
					percentageBaseDesc = string.Join(" ", elements.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
				}

				return Res.GetString("ECC5CBC0-27B8-4662-BF3F-FF5BCA1080AC", "{0} of ({1})", RateInfo, percentageBaseDesc);
			}

			var rateInfoDescription = showRateInfo ? " @ " + RateInfo : string.Empty;

			if (Chargeable.Unit == Currency)
			{
				if (Chargeable.Amount == 0)
				{
					return RateInfo + additionalChargeableDescription;
				}

				chargeableAmountDescription = Currency + " " + FormatDecimal(Chargeable.Amount);

				if (RateInfo.Type == RateInfo.RateInfoType.FLT && Chargeable.Amount == RateInfo.FlatRate.GetValueOrDefault())
				{
					return chargeableAmountDescription + additionalChargeableDescription;
				}
			}

			return chargeableAmountDescription + additionalChargeableDescription + rateInfoDescription;
		}

		ZString GetFormattedChargeableAmount()
		{
			var formattedChargeableAmount = Chargeable.Unit == Volume.TeaChest  //Needed for specific Tea Chest description
				? Chargeable.Amount.ToString("G26", Culture.CurrentCompanyCountryCulture)
				: Utilities.Round(Chargeable.Amount, 4).ToString("G26", Culture.CurrentCompanyCountryCulture);

			var decimals = Math.Abs((Chargeable.Amount - Math.Truncate(Chargeable.Amount)) * (int)Math.Pow(10, 4));

			if (decimals > 0 && decimals < 1)
			{
				formattedChargeableAmount = Res.GetString("8ed6e1e8-0e18-4e42-9032-3f82e69d1777", "More than {0}", Chargeable.Amount.ToString("f4", Culture.CurrentCompanyCountryCulture));
			}
			else if (decimals > (int)Math.Pow(10, 4) - 1)
			{
				formattedChargeableAmount = Res.GetString("a5d3d316-7efe-4ed8-a4fc-5be006033ca8", "Less than {0}", Chargeable.Amount.ToString("f4", Culture.CurrentCompanyCountryCulture));
			}

			return formattedChargeableAmount;
		}
	}

	public static class PaymentBasisExtensions
	{
		public static string Str(string messageTemplate, params object[] args)
		{
			for (var i = 0; i < args.Length; i++)
			{
				if (args[i] is decimal dec)
				{
					args[i] = FormatDecimal(dec);
				}

				if (args[i] is ZDecimal zdec)
				{
					args[i] = FormatDecimal(zdec);
				}
			}

			return string.Format(CultureInfo.InvariantCulture, messageTemplate, args);
		}

		public static string FormatDecimal(decimal amount)
		{
			var format = "f4";
			if (amount.ToString(format, Culture.CurrentCompanyCountryCulture).EndsWith("00", StringComparison.OrdinalIgnoreCase))
			{
				format = "f2";
			}
			else if (amount.ToString(format, Culture.CurrentCompanyCountryCulture).EndsWith("0", StringComparison.OrdinalIgnoreCase))
			{
				format = "f3";
			}

			return amount.ToString(format, Culture.CurrentCompanyCountryCulture);
		}

		public static PaymentBasis UpdateChargeableDescription(this PaymentBasis paymentBasis, string newDescription)
		{
			return new PaymentBasis(paymentBasis, newDescription);
		}

		public static IEnumerable<PaymentBasis> GetWithUpdatedChargeableDescription(this IEnumerable<PaymentBasis> paymentBases, string newDescription)
		{
			return paymentBases.Select(x => x.UpdateChargeableDescription(newDescription));
		}

		public static IEnumerable<PaymentBasis> SetCartageZoneDescription(this IEnumerable<PaymentBasis> paymentBases, string cartageZoneDescription)
		{
			PaymentBasis NewBasisSelector(PaymentBasis originalBasis)
			{
				var rateInfo = RateInfo.GetWithCartageZoneDescription(originalBasis.RateInfo, cartageZoneDescription);
				return UpdateRateInfo(originalBasis, rateInfo);
			}

			return paymentBases.Select(NewBasisSelector);
		}

		public static PaymentBasis UpdateRateInfo(this PaymentBasis originalBasis, RateInfo rateInfo)
		{
			return new PaymentBasis(originalBasis.Chargeable, rateInfo, originalBasis.AdapterType, originalBasis.AdapterID, originalBasis.ChargeableUnitDescription, originalBasis.ChargeableDescription, originalBasis.PercentageBaseDescription);
		}

		public static bool TryCreateFlatBasisFromMinimum(this IEnumerable<PaymentBasis> paymentBases, out PaymentBasis flatPaymentBasis)
		{
			flatPaymentBasis = default;

			if (paymentBases.Any(x => x.RateInfo.Type == RateInfo.RateInfoType.MIN))
			{
				var calculation = paymentBases.Calculate();
				if (calculation.amount == calculation.minimum && calculation.amount != decimal.MinValue)
				{
					var minBasis = paymentBases.FirstOrDefault(x => x.RateInfo.Type == RateInfo.RateInfoType.MIN && x.RateInfo.MinRate == calculation.minimum);
					flatPaymentBasis = new PaymentBasis(default, RateInfo.CreateFLT(calculation.amount, minBasis.Currency, rateReference: nameof(RateInfo.RateInfoType.MIN)), minBasis.AdapterType, minBasis.AdapterID, minBasis.ChargeableUnitDescription, minBasis.ChargeableDescription, minBasis.PercentageBaseDescription);
					return true;
				}
			}

			return false;
		}

		public static bool TryCreateFlatBasisFromMaximum(this IEnumerable<PaymentBasis> paymentBases, out PaymentBasis flatPaymentBasis)
		{
			flatPaymentBasis = default;

			if (paymentBases.Any(x => x.RateInfo.Type == RateInfo.RateInfoType.MAX))
			{
				var calculation = paymentBases.Calculate();
				if (calculation.amount == calculation.maximum && calculation.amount != decimal.MaxValue)
				{
					var maxBasis = paymentBases.FirstOrDefault(x => x.RateInfo.Type == RateInfo.RateInfoType.MAX && x.RateInfo.MaxRate == calculation.maximum);
					flatPaymentBasis = new PaymentBasis(default, RateInfo.CreateFLT(calculation.amount, maxBasis.Currency, rateReference: nameof(RateInfo.RateInfoType.MAX)), maxBasis.AdapterType, maxBasis.AdapterID, maxBasis.ChargeableUnitDescription, maxBasis.ChargeableDescription, maxBasis.PercentageBaseDescription);
					return true;
				}
			}

			return false;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static (decimal amount, decimal minimum, decimal maximum) Calculate(this IEnumerable<PaymentBasis> bases)
		{
			bases = bases.ToList();
			if (bases.Any())
			{
				if (bases.Count() == 1)
				{
					var rate = bases.First().RateInfo;
					if (rate.Type == RateInfo.RateInfoType.MIN)
					{
						return (rate.MinRate, rate.MinRate, decimal.MaxValue);
					}
					if (rate.Type == RateInfo.RateInfoType.MAX)
					{
						return (rate.MaxRate, decimal.MinValue, rate.MaxRate);
					}
				}

				if (!bases.AllSame(x => x.Currency))
				{
					throw new InvalidOperationException("Currencies should be same to perform operation on payment bases");
				}

				var useMinMax = bases.Any(x => x.Chargeable.Amount > 0) || bases.Any(x => x.RateInfo.FlatRate.HasValue && x.RateInfo.FlatRate > 0);
				var biggestMinimum = useMinMax ? bases.Max(x => x.RateInfo.MinRate) : decimal.MinValue;
				var smallestMaximum = useMinMax ? bases.Min(x => x.RateInfo.MaxRate) : decimal.MaxValue;

				var amount = biggestMinimum == 0 ? Math.Min(bases.Sum(x => x.Amount), smallestMaximum) : Math.Max(Math.Min(bases.Sum(x => x.Amount), smallestMaximum), biggestMinimum);
				var minUsed = amount == biggestMinimum && amount != 0 ? biggestMinimum : decimal.MinValue;
				var maxUsed = amount == smallestMaximum && amount != 0 ? smallestMaximum : decimal.MaxValue;

				return (amount, minUsed, maxUsed);
			}

			return (ZDecimal.Zero, decimal.MinValue, decimal.MaxValue);
		}

		[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1008:OpeningParenthesisMustBeSpacedCorrectly", Justification = "Update the analyzers to support latest C# language.")]
		public static (decimal amount, decimal minimum, decimal maximum) CalculateRounded(this IEnumerable<PaymentBasis> bases, int decimals)
		{
			var unrounded = bases.Calculate();
			var amount = Utilities.Round(unrounded.amount, decimals);
			var minimum = unrounded.minimum != decimal.MinValue ? Utilities.Round(unrounded.minimum, decimals) : decimal.MinValue;
			var maximum = unrounded.maximum != decimal.MaxValue ? Utilities.Round(unrounded.maximum, decimals) : decimal.MaxValue;

			return (amount, minimum, maximum);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static string GetDescription(this IEnumerable<PaymentBasis> bases)
		{
			bases = bases.ToArray();

			if (!bases.Any())
			{
				return string.Empty;
			}

			var nonMinMaxBases = bases.ExcludingMinOrMax().ToArray();
			if (nonMinMaxBases.Length == 0)
			{
				return bases.First().RateInfo.ToString();
			}

			if (nonMinMaxBases.Length < bases.Count() && TryGetDescriptionIfMinimumOrMaximum(bases, out string minMaxDescription))
			{
				return minMaxDescription;
			}

			if (nonMinMaxBases.Length == 1)
			{
				return nonMinMaxBases[0].ToString();
			}

			var baseRateBases = nonMinMaxBases.ExcludingZeroValues().Where(x => x.RateInfo.Type == RateInfo.RateInfoType.FLT).ToArray();
			if (baseRateBases.Any() && baseRateBases.Length == 1)
			{
				var baseRateBasisDescription = baseRateBases[0].ToString();
				var otherRates = nonMinMaxBases.ExcludingZeroValues().Where(b => !baseRateBases.Contains(b)).ToArray();
				if (otherRates.Any())
				{
					return baseRateBasisDescription + " + " + string.Join(" + ", otherRates.Select(x => x.ToString()));
				}

				return baseRateBasisDescription;
			}

			var basesForDescription = nonMinMaxBases.Length == 1 ? nonMinMaxBases : nonMinMaxBases.ExcludingZeroValues().ToArray();

			var groupedDescriptions = GroupSimilarChargeableAndRateInfoDescriptions(basesForDescription);
			if (!string.IsNullOrEmpty(groupedDescriptions))
			{
				return groupedDescriptions;
			}

			var summarizedPerUnitDescriptions = SumUpSimilarPerUnitBasesForDescription(basesForDescription);
			if (!string.IsNullOrEmpty(summarizedPerUnitDescriptions))
			{
				return summarizedPerUnitDescriptions;
			}

			return string.Join(" + ", basesForDescription);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static string GroupSimilarChargeableAndRateInfoDescriptions(IEnumerable<PaymentBasis> bases)
		{
			if (bases.Count() <= 1)
			{
				return string.Empty;
			}

			var commonChargeableDescription = string.Empty;
			var commonRateInfoDescription = string.Empty;

			if (bases.All(x => !string.IsNullOrWhiteSpace(x.ChargeableDescription)))
			{
				var chargeableDescriptions = bases.Select(x => x.ChargeableDescription).Distinct().ToArray();
				if (chargeableDescriptions.Length == 1)
				{
					commonChargeableDescription = chargeableDescriptions[0];
				}
			}

			if (bases.All(x => !string.IsNullOrWhiteSpace(x.RateInfo.RateInfoDescription)))
			{
				var rateInfoDescriptions = bases.Select(x => x.RateInfo.RateInfoDescription).Distinct().ToArray();
				if (rateInfoDescriptions.Length == 1)
				{
					commonRateInfoDescription = rateInfoDescriptions[0];
				}
			}

			if (string.IsNullOrEmpty(commonChargeableDescription) && string.IsNullOrEmpty(commonRateInfoDescription))
			{
				return string.Empty;
			}

			var basesAsStrings = bases.Select(x => x.ToString());
			var modifiedDescriptions = new List<string>();

			foreach (var basis in basesAsStrings)
			{
				var modifiedDescription = basis;
				if (!string.IsNullOrEmpty(commonChargeableDescription))
				{
					modifiedDescription = modifiedDescription.Replace(" (" + commonChargeableDescription + ")", string.Empty);
				}

				if (!string.IsNullOrEmpty(commonRateInfoDescription))
				{
					modifiedDescription = modifiedDescription.Replace(" (" + commonRateInfoDescription + ")", string.Empty);
				}

				modifiedDescriptions.Add(modifiedDescription);
			}

			return string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}",
				string.IsNullOrEmpty(commonChargeableDescription) ? string.Empty : commonChargeableDescription + " ",
				string.Join(" + ", modifiedDescriptions),
				string.IsNullOrEmpty(commonRateInfoDescription) ? string.Empty : " (" + commonRateInfoDescription + ")");
		}

		static string SumUpSimilarPerUnitBasesForDescription(IEnumerable<PaymentBasis> bases)
		{
			var result = new List<string>();

			if (bases.Any(x => x.RateInfo.Type == RateInfo.RateInfoType.UNT))
			{
				var nonPerUnitBases = bases.Where(x => x.RateInfo.Type != RateInfo.RateInfoType.UNT).ToArray();
				if (nonPerUnitBases.Any())
				{
					result.Add(nonPerUnitBases.GetDescription());
				}

				var perUnitBases = bases
					.Where(x => x.RateInfo.Type == RateInfo.RateInfoType.UNT)
					.GroupBy(x => x.RateInfo.PerUnitRate)
					.OrderByDescending(x => x.Key)
					.ToArray();

				foreach (var group in perUnitBases)
				{
					if (group.All(x => x.ToString() == group.First().ToString()))
					{
						var firstInGroup = group.First();
						var newChargeable = new Quantity(firstInGroup.Chargeable.Amount * group.Count(), firstInGroup.Chargeable.Unit);

						result.Add(new PaymentBasis(firstInGroup, newChargeable).ToString());
					}
					else
					{
						result.AddRange(group.Select(x => x.ToString()));
					}
				}
			}

			if (result.Any())
			{
				return string.Join(" + ", result);
			}

			return string.Empty;
		}

		static bool TryGetDescriptionIfMinimumOrMaximum(IEnumerable<PaymentBasis> bases, out string description)
		{
			var result = bases.Calculate();
			var currencyCode = bases.SameOrDefault(x => x.Currency);
			description = string.Empty;

			if (RatingDataRegistry.Instance.EnableLongChargeCalculationDescription.Value)
			{
				if (result.minimum > decimal.MinValue || result.amount != 0 && !bases.GetMaxMinimumBasis().IsEmpty)
				{
					var minimumValue = result.minimum > decimal.MinValue ? result.minimum : bases.GetMaxMinimumBasis().RateInfo.MinRate;
					description = ResString.GetMultilingualString("184318c0-0b64-44aa-a606-60b7c7f734a7", "Greater of (Min Rate {0} {1}, {2})", currencyCode, FormatDecimal(minimumValue), bases.ExcludingMinOrMax().GetDescription());
				}

				if (result.maximum < decimal.MaxValue || !bases.GetMinMaximumBasis().IsEmpty)
				{
					var maximumValue = result.maximum < decimal.MaxValue ? result.maximum : bases.GetMinMaximumBasis().RateInfo.MaxRate;
					var descriptionToAppendWithMaxRate = string.IsNullOrWhiteSpace(description) ? bases.ExcludingMinOrMax().GetDescription() : description;
					description = ResString.GetMultilingualString("0d0c2c32-9c2f-494e-bb84-0ec5410c0c34", "Lesser of (Max Rate {0} {1}, {2})", currencyCode, FormatDecimal(maximumValue), descriptionToAppendWithMaxRate);
				}
			}
			else
			{
				if (result.minimum > decimal.MinValue)
				{
					description = ResString.GetMultilingualString("34126630-2b70-4ec4-849f-5fc93f97cfb3", "Minimum {0} {1}", currencyCode, FormatDecimal(result.minimum));
				}

				if (result.maximum < decimal.MaxValue)
				{
					description = ResString.GetMultilingualString("694cbb99-07a2-4665-88ed-b35522e43977", "Maximum {0} {1}", currencyCode, FormatDecimal(result.maximum));
				}
			}

			if (!string.IsNullOrWhiteSpace(description))
			{
				return true;
			}

			return false;
		}

		public static ZDecimal ChargeableAmount(this IEnumerable<PaymentBasis> bases)
		{
			return bases.Sum(x => x.Chargeable.Amount);
		}

		public static ZDecimal UnroundedChargeableAmount(this IEnumerable<PaymentBasis> bases)
		{
			return bases.Sum(x => x.UnroundedChargeable.Amount);
		}

		public static ZString? CartageZoneDescription(this IEnumerable<PaymentBasis> bases)
		{
			return bases.Where(x => x.RateInfo.CartageZoneDescription.HasValue).Select(x => x.RateInfo.CartageZoneDescription).FirstOrDefault();
		}

		public static IEnumerable<PaymentBasis> ExcludingMinOrMax(this IEnumerable<PaymentBasis> bases)
		{
			return bases.Where(x => x.RateInfo.MinRate == decimal.MinValue && x.RateInfo.MaxRate == decimal.MaxValue);
		}

		public static IEnumerable<PaymentBasis> MinOrMax(this IEnumerable<PaymentBasis> bases)
		{
			return bases.Where(x => x.RateInfo.MinRate != decimal.MinValue || x.RateInfo.MaxRate != decimal.MaxValue);
		}

		public static IEnumerable<PaymentBasis> ExcludingZeroValues(this IEnumerable<PaymentBasis> bases)
		{
			return bases.Where(x => x.Amount != 0);
		}

		public static PaymentBasis GetMaxMinimumBasis(this IEnumerable<PaymentBasis> bases) => bases.MinOrMax().Where(x => x.RateInfo.Type == RateInfo.RateInfoType.MIN).OrderByDescending(x => x.RateInfo.MinRate).SingleOrDefault();

		public static PaymentBasis GetMinMaximumBasis(this IEnumerable<PaymentBasis> bases) => bases.MinOrMax().Where(x => x.RateInfo.Type == RateInfo.RateInfoType.MAX).OrderBy(x => x.RateInfo.MaxRate).SingleOrDefault();
	}

	public struct RateInfo : IEquatable<RateInfo>
	{
		RateInfo(decimal? perUnitRate, decimal? flatRate, ZString? unit, decimal? minRate, decimal? maxRate, decimal? unitMultiplier, ZString currency, RateInfoType rateInfoType, ZString? unitDescription, ZString? rateInfoDescription, ZString? cartageZoneDescription, ZString? rateReference = null, bool isPartThereof = false)
		{
			if (currency.IsEmpty)
			{
				throw new InvalidOperationException("Currency should be not empty, for percentage calc pass chargeable currency (temp workaround)");
			}

			PerUnitRate = perUnitRate;
			FlatRate = flatRate;
			Unit = unit;
			MinRate = minRate ?? decimal.MinValue;
			MaxRate = maxRate ?? decimal.MaxValue;
			Currency = currency;
			Type = rateInfoType;
			isNotEmpty = true;
			UnitDescription = unitDescription;
			RateInfoDescription = rateInfoDescription;
			CartageZoneDescription = cartageZoneDescription;
			UnitMultiplier = unitMultiplier;
			RateReference = rateReference.HasValue && !rateReference.Value.IsEmpty ? rateReference.Value : (ZString)rateInfoType.ToString();
			IsPartThereof = isPartThereof;
			MeasurementBasis = string.Empty;
			ReportIssueIfCurrencyIsInvalid();
		}

		void ReportIssueIfCurrencyIsInvalid()
		{
			var policy = new DotNetCache.CacheItemPolicy();
			var currencies = DotNetCache.MemoryCache.Default.GetOrAdd("ListOfAllCurrenceis", GetCurrencies, policy);

			if (currencies.Contains((string)Currency))
			{
				return;
			}

			var msg = Invariant($"Invalid currency: {Currency}. RateInfo Description: {RateInfoDescription}, Rate Reference: {RateReference}, Unit Description: {UnitDescription} ,Type: {Type.ToString()}.");
			ErrorReporter.ReportOnce("WI00233077_InvalidCurrency", msg);
		}

		List<string> GetCurrencies()
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			return new RefCurrencyCollection(factory).Select(c => c.Code).ToList();
		}

		public static RateInfo GetChanged(RateInfo info, ZDecimal fraction)
		{
			var newPerUnit = info.PerUnitRate * fraction;
			var newFlat = info.FlatRate * fraction;
			var newMin = info.MinRate != decimal.MinValue ? info.MinRate * fraction : decimal.MinValue;
			var newMax = info.MaxRate != decimal.MaxValue ? info.MaxRate * fraction : decimal.MaxValue;

			return new RateInfo(newPerUnit, newFlat, info.Unit, newMin, newMax, info.UnitMultiplier, info.Currency, info.Type, info.UnitDescription, info.RateInfoDescription, info.CartageZoneDescription, nameof(RateInfoType.PER));
		}

		public static RateInfo GetWithNewRateInfoDescription(RateInfo rateInfo, ZString description)
		{
			return new RateInfo(rateInfo.PerUnitRate, rateInfo.FlatRate, rateInfo.Unit, rateInfo.MinRate, rateInfo.MaxRate, rateInfo.UnitMultiplier, rateInfo.Currency, rateInfo.Type, rateInfo.UnitDescription, description, rateInfo.CartageZoneDescription);
		}

		public static RateInfo GetWithConvertedCurrency(RateInfo rateInfo, ZDecimal? convertedPerUnitRate, ZDecimal? convertedFlatRate, ZDecimal? convertedMinRate, ZDecimal? convertedMaxRate, ZString newCurrency)
		{
			if (rateInfo.Currency != newCurrency)
			{
				return new RateInfo(convertedPerUnitRate, convertedFlatRate, rateInfo.Unit, convertedMinRate, convertedMaxRate, rateInfo.UnitMultiplier, newCurrency, rateInfo.Type, rateInfo.UnitDescription, rateInfo.RateInfoDescription, rateInfo.CartageZoneDescription);
			}

			return rateInfo;
		}

		public static RateInfo GetWithCartageZoneDescription(RateInfo rateInfo, ZString cartageZoneDescription)
		{
			return new RateInfo(rateInfo.PerUnitRate, rateInfo.FlatRate, rateInfo.Unit, rateInfo.MinRate, rateInfo.MaxRate, rateInfo.UnitMultiplier, rateInfo.Currency, rateInfo.Type, rateInfo.UnitDescription, rateInfo.RateInfoDescription, cartageZoneDescription);
		}

		public static RateInfo CreateMIN(decimal minRate, ZString currency, ZString? rateInfoDescription = null, ZString? cartageZoneDescription = null)
		{
			return new RateInfo(null, null, null, minRate, null, null, currency, RateInfoType.MIN, null, rateInfoDescription, cartageZoneDescription);
		}

		public static RateInfo CreateMAX(decimal maxRate, ZString currency, ZString? rateInfoDescription = null, ZString? cartageZoneDescription = null)
		{
			return new RateInfo(null, null, null, null, maxRate, null, currency, RateInfoType.MAX, null, rateInfoDescription, cartageZoneDescription);
		}

		public static RateInfo CreateFLT(decimal flatRate, ZString currency, ZString? rateInfoDescription = null, ZString? cartageZoneDescription = null, ZString? rateReference = null)
		{
			return new RateInfo(null, flatRate, null, null, null, null, currency, RateInfoType.FLT, null, rateInfoDescription, cartageZoneDescription, rateReference);
		}

		public static RateInfo CreateUNT(decimal perUnitRate, ZString unit, ZString currency, ZString? unitDescription = null, ZString? rateInfoDescription = null, ZString? cartageZoneDescription = null, decimal? unitMultiplier = null, bool isPartThereof = false)
		{
			return new RateInfo(perUnitRate, null, unit, null, null, unitMultiplier, currency, RateInfoType.UNT, unitDescription, rateInfoDescription, cartageZoneDescription, isPartThereof: isPartThereof);
		}

		public static RateInfo CreatePER(decimal percentage, ZString chargeableCurrency)
		{
			return CreateUNT(percentage, PercentageUnit, chargeableCurrency);
		}

		public bool IsPercentage => !IsEmpty && Type == RateInfoType.UNT && Unit.HasValue && Unit.Value == PercentageUnit;

		const string PercentageUnit = "100";

		public enum RateInfoType
		{
			FLT,
			MIN,
			MAX,
			UNT,
			PER
		}

		public bool IsEmpty => !isNotEmpty;
		readonly bool isNotEmpty;

		public RateInfoType Type { get; }

		public decimal? PerUnitRate { get; }
		public decimal? FlatRate { get; }
		public ZString? MeasurementBasis { get; set; }
		public ZString? Unit { get; }
		public ZString? UnitDescription { get; set; }
		public decimal MinRate { get; }
		public decimal MaxRate { get; }
		public ZString Currency { get; }
		public ZString? RateInfoDescription { get; }
		public ZString? CartageZoneDescription { get; }
		public decimal? UnitMultiplier { get; }
		public ZString? RateReference { get; }
		public bool IsPartThereof { get; }

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj) && obj is RateInfo && Equals((RateInfo)obj);
		}

		public bool Equals(RateInfo other)
		{
			return isNotEmpty.Equals(other.isNotEmpty)
					 && PerUnitRate.Equals(other.PerUnitRate)
					 && FlatRate.Equals(other.FlatRate)
					 && Unit.Equals(other.Unit)
					 && UnitMultiplier.Equals(other.UnitMultiplier)
					 && MinRate.Equals(other.MinRate)
					 && MaxRate.Equals(other.MaxRate)
					 && Currency.Equals(other.Currency);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = isNotEmpty.GetHashCode();
				hashCode = (hashCode * 397) ^ Unit.GetHashCode();
				hashCode = (hashCode * 397) ^ Currency.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(RateInfo left, RateInfo right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(RateInfo left, RateInfo right)
		{
			return !left.Equals(right);
		}
		public override string ToString()
		{
			if (!IsEmpty)
			{
				var perUnitRateWithMultiplier = UnitMultiplier.HasValue ? UnitMultiplier.Value * PerUnitRate : PerUnitRate;

				switch (Type)
				{
					case RateInfoType.FLT:
						return Str("{0} {1} {2}", RatingDataRegistry.Instance.BaseRateText.Value, Currency, FlatRate);

					case RateInfoType.MIN:
						return Str((NoResString)"MIN {0} {1}", Currency, MinRate);

					case RateInfoType.MAX:
						return Str((NoResString)"MAX {0} {1}", Currency, MaxRate);

					case RateInfoType.UNT:
						if (IsPercentage)
						{
							return Str("{0}%", PerUnitRate);
						}

						if (Unit.HasValue && Unit.Value == Currency && RateInfoDescription.HasValue)
						{
							return RateInfoDescription.Value;
						}
						return Str("{0} {1}/{2}", Currency, perUnitRateWithMultiplier, UnitDescription ?? Unit);
				}
			}

			return string.Empty;
		}

		string Str(string messageTemplate, params object[] args)
		{
			if (RateInfoDescription.HasValue && !RateInfoDescription.Value.IsEmpty)
			{
				return PaymentBasisExtensions.Str(messageTemplate, args) + " " + string.Format(CultureInfo.InvariantCulture, "({0})", RateInfoDescription.Value);
			}

			return PaymentBasisExtensions.Str(messageTemplate, args);
		}
	}

	public enum AdapterType
	{
		BillOfLading,
		Booking,
		LoadList,
		CFSShipment,
		CFSContainer,
		ContainerDetention,
		ContainerMovement,
		CustomsResponseMessage,
		GatePass,
		Order,
		PortTransport,
		Shipment,
		TransportConsignment,
		TransitWarehouse,
		ConsolidatedBooking,
		TransportBooking,
		BookingWithQuote,
		OneOffQuote,
		Consolidation,
		RunSheet,
		WarehouseInvoice,
		WarehouseReceipt,
		WarehouseOrder,
		WarehouseAdjustment,
		Warehouse,
		WarehouseAdHocService,
		HVLVShipment,
		Protest,
		ReconDeclaration,
		CustomsDeclaration,
		LVSDeclaration,
		RateEntry,
		GatewayShipment,
		ContainerYard
	}
}
