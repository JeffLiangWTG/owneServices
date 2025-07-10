using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Rating.DataTransfer.TACT
{
	public class TACTData150FixedWidthDataFormat : FixedWidthFlatFileFormat
	{
		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotSupportedException("Export to IATA TACT format is not supported");
		}

		public override FlatFileDataRow ConvertToRow(ZString rawDataRow)
		{
			var result = new TACTDataDataRow(Constants.RowLength);
			result.SetField(0, rawDataRow.SubstringSafe(0, Constants.RowLength));

			result.CategoryValue = GetValue(Constants.Position.Category, Constants.Length.Category, rawDataRow);
			result.OriginCountryCodeValue = GetValue(Constants.Position.OriginCountry, Constants.Length.OriginCountry, rawDataRow);
			result.OriginCityCodeValue = GetValue(Constants.Position.OriginPort, Constants.Length.OriginPort, rawDataRow);
			result.DestinationCountryCodeValue = GetValue(Constants.Position.DestinationCountry, Constants.Length.DestinationCountry, rawDataRow);
			result.DestinationCityCodeValue = GetValue(Constants.Position.DestinationPort, Constants.Length.DestinationPort, rawDataRow);
			result.UniqueNoteValue = GetValue(Constants.Position.UniqueNote, Constants.Length.UniqueNote, rawDataRow);
			result.CarrierCodeValue = GetValue(Constants.Position.CarrierCode, Constants.Length.CarrierCode, rawDataRow);
			result.WeightBreakValue = GetValue(Constants.Position.WeightBreak, Constants.Length.WeightBreak, rawDataRow);
			result.WeightBreakUnitValue = GetValue(Constants.Position.WeightBreakUnit, Constants.Length.WeightBreakUnit, rawDataRow);
			result.CurrencyValue = GetValue(Constants.Position.Currency, Constants.Length.Currency, rawDataRow);
			result.RateDecimalPlaceValue = GetValue(Constants.Position.DecimalPlace, Constants.Length.DecimalPlace, rawDataRow);
			result.RateValue = GetValue(Constants.Position.Rate, Constants.Length.Rate, rawDataRow);
			result.StartDateValue = GetStartDate(rawDataRow);
			result.EndDateValue = GetValue(Constants.Position.EndDate, Constants.Length.EndDate, rawDataRow);

			var actionString = GetValue(Constants.Position.ActionCode, Constants.Length.ActionCode, rawDataRow);
			result.ActionCode = actionString.Length == 1 ? actionString[0] : TACTDataDataRow.Constants.ActionCodes.None;

			return result;
		}

		ZString GetStartDate(ZString rawDataRow)
		{
			var result = ZString.Empty;

			var governmentStatus = GetValue(Constants.Position.GovernmentStatus, Constants.Length.GovernmentStatus, rawDataRow);
			switch (governmentStatus)
			{
				case "P":
					result = GetValue(Constants.Position.IntendedStartDate, Constants.Length.IntendedStartDate, rawDataRow);
					break;

				case "A":
					result = GetValue(Constants.Position.ActualStartDate, Constants.Length.ActualStartDate, rawDataRow);
					break;
			}

			return result;
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.None; }
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		public string GetOrigin(string row)
			=> GetValue(Constants.Position.OriginPort, Constants.Length.OriginPort, row);

		public string GetDestination(string row)
			=> GetValue(Constants.Position.DestinationPort, Constants.Length.DestinationPort, row);

		protected override string GetClientSpecificFileExtension()
		{
			return Constants.FileFormat;
		}

		public string FileExtension
		{
			get { return GetClientSpecificFileExtension(); }
		}

		#region Constants

		public static class Constants
		{
			public const int RowLength = 150;
			public const string FileFormat = "054";

			public static class Position
			{
				public const int Category = 0;
				public const int Area = 2;
				public const int BlankField1 = 4;
				public const int IdentificationCode = 7;
				public const int OriginPortNumeric = 13;
				public const int OriginPort = 18;
				public const int OriginCountry = 21;
				public const int DestinationPortNumeric = 23;
				public const int DestinationPort = 28;
				public const int DestinationCountry = 31;
				public const int UniqueNote = 33;
				public const int CarrierCode = 37;
				public const int DirectionCode = 40;
				public const int BlankField2 = 41;
				public const int ProportionalActionCode = 42;
				public const int SCRItemNumber = 43;
				public const int ULDRatingType = 51;
				public const int ULDChargeCode = 55;
				public const int WeightBreak = 56;
				public const int WeightBreakUnit = 61;
				public const int ChangeIndicators = 62;
				public const int BlankField3 = 68;
				public const int Currency = 95;
				public const int DecimalPlace = 98;
				public const int Rate = 99;
				public const int IntendedStartDate = 108;
				public const int ActualStartDate = 116;
				public const int EndDate = 124;
				public const int GovernmentStatus = 132;
				public const int OriginGateWay = 133;
				public const int DestinationGateWay = 136;
				public const int UniqueAddOnAreaCode = 139;
				public const int Source = 142;
				public const int ActionCode = 147;
				public const int ConstructionAllowedIndicator = 148;
				public const int CategorySortIndicator = 149;
			}

			public static class Length
			{
				public const int Category = 2;
				public const int Area = 2;
				public const int BlankField1 = 3;
				public const int IdentificationCode = 6;
				public const int OriginPortNumeric = 5;
				public const int OriginPort = 3;
				public const int OriginCountry = 2;
				public const int DestinationPortNumeric = 5;
				public const int DestinationPort = 3;
				public const int DestinationCountry = 2;
				public const int UniqueNote = 4;
				public const int CarrierCode = 3;
				public const int DirectionCode = 1;
				public const int BlankField2 = 1;
				public const int ProportionalActionCode = 1;
				public const int SCRItemNumber = 8;
				public const int ULDRatingType = 4;
				public const int ULDChargeCode = 1;
				public const int WeightBreak = 5;
				public const int WeightBreakUnit = 1;
				public const int ChangeIndicators = 6;
				public const int BlankField3 = 27;
				public const int Currency = 3;
				public const int DecimalPlace = 1;
				public const int Rate = 9;
				public const int IntendedStartDate = 8;
				public const int ActualStartDate = 8;
				public const int EndDate = 8;
				public const int GovernmentStatus = 1;
				public const int OriginGateWay = 3;
				public const int DestinationGateWay = 3;
				public const int UniqueAddOnAreaCode = 3;
				public const int Source = 5;
				public const int ActionCode = 1;
				public const int ConstructionAllowedIndicator = 1;
				public const int CategorySortIndicator = 1;
			}
		}

		#endregion
	}
}

