using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.TACT.Testing
{
	public class TACTData80FixedWidthDataFormatTest : TestCase
	{
		public void TestFileExtensionForImport()
		{
			var testClass = new TACTData80FixedWidthDataFormat();

			AssertEquals(FileExtensionType.ClientSpecific, testClass.FileExtensionForImport);
			AssertEquals("146", TACTData80FixedWidthDataFormat.Constants.FileFormat);
		}

		public void TestConvertToRow()
		{
			var rawDataRow = "GS123456PARFRBOMING724AF  000007N02BGA00300KUSD200000605019990101199906301234512";

			var dataFormat = new TACTData80FixedWidthDataFormat();
			var result = dataFormat.ConvertToRow(rawDataRow) as TACTDataDataRow;

			AssertNotNull(result);
			AssertEquals("GS", result.Category);
			AssertEquals("FR", result.OriginCountryCode);
			AssertEquals("PAR", result.OriginCityCode);
			AssertEquals("IN", result.DestinationCountryCode);
			AssertEquals("BOM", result.DestinationCityCode);
			AssertEquals("G724", result.UniqueNote);
			AssertEquals(300m, result.WeightBreak);
			AssertEquals("KG", result.WeightBreakUnit);
			AssertEquals("USD", result.Currency);
			AssertEquals(60.50m, result.Rate);
			AssertEquals(new ZDateTime(1999, 1, 1), result.StartDate);
			AssertEquals(new ZDateTime(1999, 6, 30), result.EndDate);
		}
	}
}
