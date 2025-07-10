using System;
using System.Collections.ObjectModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Rating.DataTransfer.TACT
{
	public class TACTDataDataRow : FlatFileDataRow
	{
		public TACTDataDataRow(int rowLength)
			: base(rowLength)
		{
		}

		#region Key

		public ZString Key
		{
			get
			{
				ZString result = OriginCountryCode + OriginCityCode + "|" + DestinationCountryCode + DestinationCityCode + "|" + CarrierCode;
				if (IsGeneralRate)
				{
					result += "|G"; // This should not smell at all
				}

				if (!string.IsNullOrEmpty(UniqueNote))
				{
					result += "|" + UniqueNote; // This should not smell as well
				}

				return result;
			}
		}

		#endregion

		#region Category

		public ZString Category => CategoryValue;

		internal ZString CategoryValue;

		public static readonly ReadOnlyCollection<string> CargoRates = new ReadOnlyCollection<string>(
			new[]
			{
				Constants.Category.SpecifiedGeneralCargoRate,
				Constants.Category.ConstructedGeneralCargoRate,
				Constants.Category.ProportionalRatePerKilogram
			});

		public static readonly ReadOnlyCollection<string> RatesPerKilogram = new ReadOnlyCollection<string>(
			new[]
			{
				Constants.Category.SpecifiedRatePerKilogram,
				Constants.Category.ConstructedRatePerKilogram,
				Constants.Category.ProportionalRatePerKilogram
			});

		public bool IsGeneralRate => Category == Constants.Category.SpecifiedMinimumCharge
									 || Category == Constants.Category.SpecifiedBasicCharge
									 || CargoRates.Contains(Category)
									 || RatesPerKilogram.Contains(Category);

		#endregion

		public ZString OriginCountryCode
		{
			get { return OriginCountryCodeValue; }
		}

		internal ZString OriginCountryCodeValue;

		public ZString OriginCityCode
		{
			get { return OriginCityCodeValue; }
		}

		internal ZString OriginCityCodeValue;

		public ZString DestinationCountryCode
		{
			get { return DestinationCountryCodeValue; }
		}

		internal ZString DestinationCountryCodeValue;

		public ZString DestinationCityCode
		{
			get { return DestinationCityCodeValue; }
		}

		internal ZString DestinationCityCodeValue;

		public ZDecimal WeightBreak
		{
			get
			{
				ZDecimal result;
				ZDecimal.TryParse(WeightBreakValue, out result);

				return result;
			}
		}

		internal ZString WeightBreakValue;

		public ZString WeightBreakUnit
		{
			get
			{
				var unit = ZString.Empty;

				switch (WeightBreakUnitValue)
				{
					case "K":
						unit = "KG";
						break;

					case "L":
						unit = "LB";
						break;
				}

				return unit;
			}
		}

		internal ZString WeightBreakUnitValue;

		public ZString Currency
		{
			get { return CurrencyValue; }
		}

		internal ZString CurrencyValue;

		public ZDecimal Rate
		{
			get
			{
				ZDecimal result;
				ZInt decimalPlace;

				ZInt.TryParse(RateDecimalPlaceValue, out decimalPlace);
				ZDecimal.TryParse(RateValue, out result);

				if (!result.IsEmpty && !decimalPlace.IsEmpty)
				{
					result /= (decimal)Math.Pow(10, decimalPlace);
				}

				return result;
			}
		}

		internal ZString RateValue;
		internal ZString RateDecimalPlaceValue;

		public ZDate StartDate
		{
			get
			{
				ZDateTime result;
				ZDateTime.TryParseExact(StartDateValue, out result, "yyyyMMdd");

				return result.IsValid ? new ZDate(result) : ZDate.Empty;
			}
		}

		internal string StartDateValue;

		public ZDate EndDate
		{
			get
			{
				ZDateTime result;
				ZDateTime.TryParseExact(EndDateValue, out result, "yyyyMMdd");

				return result.IsValid ? new ZDate(result) : ZDate.Empty;
			}
		}

		internal string EndDateValue;

		public ZString CarrierCode
		{
			get { return CarrierCodeValue.Trim(); }
		}

		internal ZString CarrierCodeValue;

		public ZString UniqueNote
		{
			get { return UniqueNoteValue.Trim(); }
		}

		internal ZString UniqueNoteValue;

		public char ActionCode { get; set; } = Constants.ActionCodes.None;

		#region Constants

		public static class Constants
		{
			public static class Category
			{
				public const string SpecifiedMinimumCharge = "MC";
				public const string SpecifiedBasicCharge = "BS";
				public const string SpecifiedGeneralCargoRate = "GS";
				public const string SpecifiedRatePerKilogram = "KS";
				public const string SpecifiedSpecificCommodityRate = "SS";
				public const string SpecifiedULDGeneralCargoRate = "US";
				public const string SpecifiedULDSpecificCommodityRate = "CS";

				public const string ConstructedGeneralCargoRate = "GC";
				public const string ConstructedRatePerKilogram = "KC";
				public const string ConstructedSpecificCommodityRate = "SC";
				public const string ConstructedULDGeneralCargoRate = "UC";
				public const string ConstructedULDSpecificCommodityRate = "CC";

				public const string ProportionalGeneralCargoRate = "GP";
				public const string ProportionalRatePerKilogram = "KP";
				public const string ProportionalSpecificCommodityRate = "SP";
				public const string ProportionalULDGeneralCargoRate = "UP";
				public const string ProportionalULDSpecificCommodityRate = "CP";
			}

			public static class ActionCodes
			{
				public const char Add = 'A';
				public const char Change = 'C';
				public const char Delete = 'D';
				public const char None = ' ';
			}
		}

		#endregion
	}
}
