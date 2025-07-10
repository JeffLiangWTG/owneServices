using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderLineFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string csvOrderLine = "POL,1001,1111,SuppPrdCode,SuppPrdDesc,LocPrdCode,LocPrdDesc,INC,,20060301,,10.12,UQ,10.10,11.11,9.09,,,,365.1,,,prtattr1,prtattr2,prtattr3,custattr1,custattr2,custattr3,custattr4,custattr5,custattr6,custtextblob,custflag1,custflag2,custflag3,custflag4,custflag5,20050101,20050102,20050103,20050104,20050105,1.01,1.02,1.03,1.04,1.05,,,,,,,,,10,3,1000.001,200.002,10.123,M3,123.01,KG,,,,,,,,,special instractions,additional information";

			OrderLineFlatFileDataRow orderRow = new OrderLineFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderLine).FieldValues));
			AssertNotNull(orderRow);
			AssertEquals(RecordTypes.POL, orderRow.RecordType);
			AssertEquals(1001.ToString(), orderRow.ClientOrderLineNumber.ToString());
			AssertEquals(1111.ToString(), orderRow.ClientOrderSubLineNumber.ToString());
			AssertEquals("SuppPrdCode", orderRow.SupplierProductCode);
			AssertEquals("SuppPrdDesc", orderRow.SupplierProductDescription);
			AssertEquals("LocPrdCode", orderRow.LocalProductCode);
			AssertEquals("LocPrdDesc", orderRow.LocalProductDescription);
			AssertEquals("INC", orderRow.LineStatus);
			AssertEquals(new ZDateTime(2006, 3, 1), orderRow.LineDropDate);
			AssertEquals(10.12m, orderRow.ProductQuantityOrdered);
			AssertEquals("UQ", orderRow.ProductUQ);
			AssertEquals(10.10m, orderRow.InnerPacks);
			AssertEquals(11.11m, orderRow.OuterPacks);
			AssertEquals(9.09m, orderRow.ProductUnitPrice);
			AssertEquals(365.1m, orderRow.ProductLinePrice);
			AssertEquals("prtattr1", orderRow.PartAttribute1);
			AssertEquals("prtattr2", orderRow.PartAttribute2);
			AssertEquals("prtattr3", orderRow.PartAttribute3);
			AssertEquals("custattr1", orderRow.CustomAttribute1);
			AssertEquals("custattr2", orderRow.CustomAttribute2);
			AssertEquals("custattr3", orderRow.CustomAttribute3);
			AssertEquals("custattr4", orderRow.CustomAttribute4);
			AssertEquals("custattr5", orderRow.CustomAttribute5);
			AssertEquals("custattr6", orderRow.CustomAttribute6);
			AssertEquals("custtextblob", orderRow.CustomTextBlob);
			AssertEquals(false, orderRow.CustomFlag1);
			AssertEquals(false, orderRow.CustomFlag2);
			AssertEquals(false, orderRow.CustomFlag3);
			AssertEquals(false, orderRow.CustomFlag4);
			AssertEquals(false, orderRow.CustomFlag5);
			AssertEquals(new ZDateTime(2005, 1, 1), orderRow.CustomDate1);
			AssertEquals(new ZDateTime(2005, 1, 2), orderRow.CustomDate2);
			AssertEquals(new ZDateTime(2005, 1, 3), orderRow.CustomDate3);
			AssertEquals(new ZDateTime(2005, 1, 4), orderRow.CustomDate4);
			AssertEquals(new ZDateTime(2005, 1, 5), orderRow.CustomDate5);
			AssertEquals(1.01m, orderRow.CustomDecimal1);
			AssertEquals(1.02m, orderRow.CustomDecimal2);
			AssertEquals(1.03m, orderRow.CustomDecimal3);
			AssertEquals(1.04m, orderRow.CustomDecimal4);
			AssertEquals(1.05m, orderRow.CustomDecimal5);
			AssertEquals("10", orderRow.ContainerNumber);
			AssertEquals(3, orderRow.ContainerPuckingOrder);
			AssertEquals(1000.001m, orderRow.UnitQtyInvoiced);
			AssertEquals(200.002m, orderRow.UnitQuantityReceived);
			AssertEquals(10.123m, orderRow.ActualVolume);
			AssertEquals("M3", orderRow.VolumeUnit);
			AssertEquals(123.01m, orderRow.ActualWeight);
			AssertEquals("KG", orderRow.WeightUnit);
			AssertEquals("special instractions", orderRow.SpecialInstractions);
			AssertEquals("additional information", orderRow.AdditionalInformation);
		}
	}
}
