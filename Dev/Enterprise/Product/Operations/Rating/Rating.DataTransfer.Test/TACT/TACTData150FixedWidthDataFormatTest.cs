using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.TACT.Testing
{
	class TACTData150FixedWidthDataFormatTest : TestCase
	{
		public void TestFileExtensionForImport()
		{
			var tactClass = new TACTData150FixedWidthDataFormat();
			AssertEquals(FileExtensionType.ClientSpecific, tactClass.FileExtensionForImport);
			AssertEquals("054", TACTData150FixedWidthDataFormat.Constants.FileFormat);
		}

		public void TestConvertToRow()
		{
			var rawDataRow = "GS5L   CAR  N11770BFNZA64570OSAJPK436JL                 00045K                                 ZAR20000042402011100120111001        A          JL     ";

			var dataFormat = new TACTData150FixedWidthDataFormat();
			var result = dataFormat.ConvertToRow(rawDataRow) as TACTDataDataRow;

			AssertNotNull(result);
			AssertEquals("GS", result.Category);
			AssertEquals("ZA", result.OriginCountryCode);
			AssertEquals("BFN", result.OriginCityCode);
			AssertEquals("JP", result.DestinationCountryCode);
			AssertEquals("OSA", result.DestinationCityCode);
			AssertEquals("K436", result.UniqueNote);
			AssertEquals("JL", result.CarrierCode);
			AssertEquals(45m, result.WeightBreak);
			AssertEquals("KG", result.WeightBreakUnit);
			AssertEquals("ZAR", result.Currency);
			AssertEquals(42.4m, result.Rate);
			AssertEquals(new ZDateTime(2011, 10, 1), result.StartDate);
			AssertEquals(ZDateTime.Empty, result.EndDate);
		}

		public void TestDates()
		{
			var rowWithGovernmentStatusP = "MC4C   501  A16220CPTZA72830RDUUS                       00000K                                 ZAR2000040000201212122012010120130101P   ATL   21602   ";
			var rowWithGovernmentStatusA = "MC4C   501  A16220CPTZA72830RDUUS                       00000K                                 ZAR20000400002012121220120101        A   ATL   21602   ";
			var rowWithGovernmentStatusANoActualDate = "MC4C   501  A16220CPTZA72830RDUUS                       00000K                                 ZAR200004000020121212                A   ATL   21602   ";

			var dataFormat = new TACTData150FixedWidthDataFormat();
			var result = dataFormat.ConvertToRow(rowWithGovernmentStatusP) as TACTDataDataRow;

			AssertEquals("Expected to use the intended start date when gov status is P as per IATA TACT specifications", new ZDateTime(2012, 12, 12), result.StartDate);
			AssertEquals(new ZDateTime(2013, 01, 01), result.EndDate);

			result = dataFormat.ConvertToRow(rowWithGovernmentStatusA) as TACTDataDataRow;

			AssertEquals("Expected to use the actual start date when gov status is A as per IATA TACT specifications", new ZDateTime(2012, 01, 01), result.StartDate);
			AssertEquals(ZDateTime.Empty, result.EndDate);

			result = dataFormat.ConvertToRow(rowWithGovernmentStatusANoActualDate) as TACTDataDataRow;

			AssertEquals("Expected NOT to fall back to the intended date if we can't find the actual date", ZDateTime.Empty, result.StartDate);
		}

		public void TestActionCode()
		{
			var rowWithQuestionMarkForActionCode = "GC1A   550  A60650BNAUS38760IQTPE                       00001K                                 USD20000008472020030120200301        AATL           ?  ";
			var dataFormat = new TACTData150FixedWidthDataFormat();

			var actualNone = (TACTDataDataRow)dataFormat.ConvertToRow(rowWithQuestionMarkForActionCode.Replace('?', TACTDataDataRow.Constants.ActionCodes.None));
			var actualAdd = (TACTDataDataRow)dataFormat.ConvertToRow(rowWithQuestionMarkForActionCode.Replace('?', TACTDataDataRow.Constants.ActionCodes.Add));
			var actualChange = (TACTDataDataRow)dataFormat.ConvertToRow(rowWithQuestionMarkForActionCode.Replace('?', TACTDataDataRow.Constants.ActionCodes.Change));
			var actualDelete = (TACTDataDataRow)dataFormat.ConvertToRow(rowWithQuestionMarkForActionCode.Replace('?', TACTDataDataRow.Constants.ActionCodes.Delete));
			var actualMissing = (TACTDataDataRow)dataFormat.ConvertToRow(rowWithQuestionMarkForActionCode.Replace("?  ", ""));

			AssertEquals(TACTDataDataRow.Constants.ActionCodes.None, actualNone.ActionCode);
			AssertEquals(TACTDataDataRow.Constants.ActionCodes.Add, actualAdd.ActionCode);
			AssertEquals(TACTDataDataRow.Constants.ActionCodes.Change, actualChange.ActionCode);
			AssertEquals(TACTDataDataRow.Constants.ActionCodes.Delete, actualDelete.ActionCode);
			AssertEquals(TACTDataDataRow.Constants.ActionCodes.None, actualMissing.ActionCode);
		}
	}
}
