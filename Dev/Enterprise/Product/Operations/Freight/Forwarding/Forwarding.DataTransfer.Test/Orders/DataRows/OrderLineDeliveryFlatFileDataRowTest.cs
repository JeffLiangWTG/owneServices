using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderLineDeliveryFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string csvOrderLineDelivery = "PLD,,,AUSYD,ADDRESS_SYD,10,,,Attr1,Attr2,Attr3,Attr4,Attr5,20090101,20090202,20090303,20090404,20090505,Y,N,Y,N,Y,1.00,2.00,3.00,4.00,5.00";

			OrderLineDeliveryFlatFileDataRow orderRow = new OrderLineDeliveryFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderLineDelivery).FieldValues));
			AssertNotNull(orderRow);
			AssertEquals(RecordTypes.PLD, orderRow.RecordType);
			AssertEquals("AUSYD", orderRow.DeliveryPort);
			AssertEquals("ADDRESS_SYD", orderRow.DeliveryPointAddressShortCode.ToString());
			AssertEquals(10, orderRow.QtyDelivered);

			AssertEquals("Attr1", orderRow.CustomAttribute1);
			AssertEquals("Attr2", orderRow.CustomAttribute2);
			AssertEquals("Attr3", orderRow.CustomAttribute3);
			AssertEquals("Attr4", orderRow.CustomAttribute4);
			AssertEquals("Attr5", orderRow.CustomAttribute5);

			AssertEquals(new ZDateTime(2009, 1, 1), orderRow.CustomDate1);
			AssertEquals(new ZDateTime(2009, 2, 2), orderRow.CustomDate2);
			AssertEquals(new ZDateTime(2009, 3, 3), orderRow.CustomDate3);
			AssertEquals(new ZDateTime(2009, 4, 4), orderRow.CustomDate4);
			AssertEquals(new ZDateTime(2009, 5, 5), orderRow.CustomDate5);

			AssertEquals(true, orderRow.CustomFlag1);
			AssertEquals(false, orderRow.CustomFlag2);
			AssertEquals(true, orderRow.CustomFlag3);
			AssertEquals(false, orderRow.CustomFlag4);
			AssertEquals(true, orderRow.CustomFlag5);

			AssertEquals((ZDecimal)1.00, orderRow.CustomNumber1);
			AssertEquals((ZDecimal)2.00, orderRow.CustomNumber2);
			AssertEquals((ZDecimal)3.00, orderRow.CustomNumber3);
			AssertEquals((ZDecimal)4.00, orderRow.CustomNumber4);
			AssertEquals((ZDecimal)5.00, orderRow.CustomNumber5);
		}
	}
}
