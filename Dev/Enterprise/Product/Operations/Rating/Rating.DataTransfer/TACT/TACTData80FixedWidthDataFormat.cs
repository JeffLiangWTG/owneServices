using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Rating.DataTransfer.TACT
{
	public class TACTData80FixedWidthDataFormat : FixedWidthFlatFileFormat
	{
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
			result.CarrierCodeValue = GetValue(Constants.Position.Carrier, Constants.Length.Carrier, rawDataRow);
			result.WeightBreakValue = GetValue(Constants.Position.WeightBreak, Constants.Length.WeightBreak, rawDataRow);
			result.WeightBreakUnitValue = GetValue(Constants.Position.WeightBreakUnit, Constants.Length.WeightBreakUnit, rawDataRow);
			result.CurrencyValue = GetValue(Constants.Position.Currency, Constants.Length.Currency, rawDataRow);
			result.RateDecimalPlaceValue = GetValue(Constants.Position.DecimalPlace, Constants.Length.DecimalPlace, rawDataRow);
			result.RateValue = GetValue(Constants.Position.Rate, Constants.Length.Rate, rawDataRow);
			result.StartDateValue = GetValue(Constants.Position.StartDate, Constants.Length.StartDate, rawDataRow);
			result.EndDateValue = GetValue(Constants.Position.EndDate, Constants.Length.EndDate, rawDataRow);

			return result;
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		protected override string GetClientSpecificFileExtension()
		{
			return Constants.FileFormat;
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Txt; }
		}

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotSupportedException("Export to IATA TACT format is not supported");
		}

		public string GetOrigin(string row)
			=> GetValue(Constants.Position.OriginPort, Constants.Length.OriginPort, row);

		public string GetDestination(string row)
			=> GetValue(Constants.Position.DestinationPort, Constants.Length.DestinationPort, row);

		#region Constants

		public static class Constants
		{
			public const int RowLength = 80;
			public const string FileFormat = "146";

			public static class Position
			{
				public const int Category = 0;
				public const int InternalUse1 = 2;
				public const int OriginPort = 8;
				public const int OriginCountry = 11;
				public const int DestinationPort = 13;
				public const int DestinationCountry = 16;
				public const int UniqueNote = 18;
				public const int Carrier = 22;
				public const int SCRItemNumber = 25;
				public const int ULDRatingType = 33;
				public const int ULDChargeCode = 37;
				public const int WeightBreak = 38;
				public const int WeightBreakUnit = 43;
				public const int Currency = 44;
				public const int DecimalPlace = 47;
				public const int Rate = 48;
				public const int StartDate = 57;
				public const int EndDate = 65;
				public const int Source = 73;
				public const int InternalUse2 = 78;
			}

			public static class Length
			{
				public const int Category = 2;
				public const int InternalUse1 = 6;
				public const int OriginPort = 3;
				public const int OriginCountry = 2;
				public const int DestinationPort = 3;
				public const int DestinationCountry = 2;
				public const int UniqueNote = 4;
				public const int Carrier = 3;
				public const int SCRItemNumber = 8;
				public const int ULDRatingType = 4;
				public const int ULDChargeCode = 1;
				public const int WeightBreak = 5;
				public const int WeightBreakUnit = 1;
				public const int Currency = 3;
				public const int DecimalPlace = 1;
				public const int Rate = 9;
				public const int StartDate = 8;
				public const int EndDate = 8;
				public const int Source = 5;
				public const int InternalUse2 = 2;
			}
		}

		#endregion
	}
}

