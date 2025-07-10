using CargoWise.Common;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string csvOrderHeader = "POH,aaaa,bbbbb,cccc,111111,01012005,vvvv,,,,,,,,,";
			string csvOrderLine = "POL,aaaa,bbbbb,cccc,111111,01012005,vvvv,,,,,,,,,";
			string csvOrderInvalid = "LOH,aaaa,bbbbb,cccc,111111,01012005,vvvv,,,,,,,,,";

			OrderFlatFileDataRow orderRow = new OrderFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderHeader).FieldValues));
			AssertNotNull(orderRow);
			AssertEquals(RecordTypes.POH, orderRow.RecordType);

			orderRow = new OrderFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderLine).FieldValues));
			AssertNotNull(orderRow);
			AssertEquals(RecordTypes.POL, orderRow.RecordType);

			orderRow = new OrderFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderInvalid).FieldValues));
			AssertNotNull(orderRow);
			AssertEquals(RecordTypes.INV, orderRow.RecordType);
		}
	}
}
