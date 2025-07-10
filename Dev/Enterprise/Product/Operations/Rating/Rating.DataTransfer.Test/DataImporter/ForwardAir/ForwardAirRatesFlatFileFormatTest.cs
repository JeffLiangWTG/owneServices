using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.ForwardAir.Testing
{
	public class ForwardAirRatesFlatFileFormatTest : TestCase
	{
		public void TestConvertToRow()
		{
			const string RawDataRow = ",ABQ,ALB,1530,900,3,Th,Fr,Mo,Mo,Mo,,,31.17,29.75,28.33,26.9,25.49,24.08,34.39";

			ForwardAirRatesFlatFileFormat dataFormat = new ForwardAirRatesFlatFileFormat(new BusinessObjectFactory());
			ForwardAirRatesFlatFileDataRow result = (ForwardAirRatesFlatFileDataRow)dataFormat.ConvertToRow(RawDataRow);

			AssertEquals("USABQ", result.Origin);
			AssertEquals("USALB", result.Destination);
			AssertEquals(3, result.Days);
			AssertEquals(31.17m, result.W100);
			AssertEquals(29.75m, result.W500);
			AssertEquals(28.33m, result.W1000);
			AssertEquals(26.9m, result.W3000);
			AssertEquals(25.49m, result.W5000);
			AssertEquals(24.08m, result.W7500);
			AssertEquals(34.39m, result.Minimum);
		}
	}
}
