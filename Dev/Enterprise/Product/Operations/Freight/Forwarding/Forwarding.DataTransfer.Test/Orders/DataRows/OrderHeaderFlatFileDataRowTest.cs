using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderHeaderFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string csvOrderHeader = "POH,1.0,blah@blah.blah,123456,111111,20050101,SEA,FCL,20060301,20060501,20060401,INC,USD,365,2.33,,INCO,addterms,goodsdesc,3,BLH,,325,M3,,32,KG,1123334,20060410,UA,,,,SYDBLAH,blah blah,Sydney Blah,blah str. Sydney,,Sydney,Blah,04213,AU,Australia,AUSYD,380973181193,mail@mail.com,,,SYDTMP,temp temp,Sydney Temp,Temp str. Sydney,,Sydney,TEMP,09999,AU,Australia,AUSYD,380503345663,temp@temp.com,HouseBill,depvessel,depvoyage,UAIEV,AUSYD,avaliableat,deliveredto,AUBNE,AUMEL,SENDAG,RECAG,,20050101,20050102,custattr1,custattr2,custattr3,custattr4,custattr5,N,Y,N,Y,N,1.01,1.02,1.03,1.04,1.05,contact1,contact2,,GOOOOOOOODs,add notes,special instractions,delivery instractions";

			OrderHeaderFlatFileDataRow orderRow = new OrderHeaderFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderHeader).FieldValues));
			AssertNotNull(orderRow);
			AssertEquals(RecordTypes.POH, orderRow.RecordType);
			AssertEquals("1.0", orderRow.RecordStructureAndVersion);
			AssertEquals("blah@blah.blah", orderRow.SendingEmailURL);
			AssertEquals("123456", orderRow.OrderNumber);
			AssertEquals("111111", orderRow.SupplierInvoiceNumber);
			AssertEquals(new ZDateTime(2005, 1, 1), orderRow.SupplierInvoiceDate);
			AssertEquals("SEA", orderRow.TransportMode);
			AssertEquals("FCL", orderRow.ContainerMode);
			AssertEquals(new ZDateTime(2006, 3, 1), orderRow.OrderDate);
			AssertEquals(new ZDateTime(2006, 5, 1), orderRow.ExWorksRequiredBy);
			AssertEquals(new ZDateTime(2006, 4, 1), orderRow.DeliveryRequiredBy);
			AssertEquals("INC", orderRow.OrderStatus);
			AssertEquals("USD", orderRow.OrderCurrency);
			AssertEquals(365m, orderRow.TotalOrderAmount);
			AssertEquals(2.33m, orderRow.EstimatedExchangeRate);
			AssertEquals("INCO", orderRow.INCOTerm);
			AssertEquals("addterms", orderRow.AdditionalTerms);
			AssertEquals("goodsdesc", orderRow.OrderGoodsDescription);
			AssertEquals(3m, orderRow.TottalPacks);
			AssertEquals("BLH", orderRow.PackType);
			AssertEquals(325m, orderRow.Volume);
			AssertEquals("M3", orderRow.UnitOfVolum);
			AssertEquals(32m, orderRow.Weight);
			AssertEquals("KG", orderRow.UnitOfWeight);
			AssertEquals("1123334", orderRow.ConfirmationNumber);
			AssertEquals(new ZDateTime(2006, 4, 10), orderRow.ConfirmationDate);
			AssertEquals("UA", orderRow.CountryOfOrigin);
			AssertEquals("SYDBLAH", orderRow.SupplierCode);
			AssertEquals("blah blah", orderRow.SupplierContact);
			AssertEquals("Sydney Blah", orderRow.SupplierCompanyName);
			AssertEquals("blah str. Sydney", orderRow.SupplierAddress1);
			AssertEquals("", orderRow.SupplierAddress2);
			AssertEquals("Sydney", orderRow.SupplierCity);
			AssertEquals("Blah", orderRow.SupplierState);
			AssertEquals("04213", orderRow.SupplierPostCode);
			AssertEquals("AU", orderRow.SupplierISOCountryCode);
			AssertEquals("Australia", orderRow.SupplierCountryName);
			AssertEquals("AUSYD", orderRow.SupplierUNLOCO);
			AssertEquals("380973181193", orderRow.SupplierPhone);
			AssertEquals("mail@mail.com", orderRow.SupplierEmailAddress);
			AssertEquals("SYDTMP", orderRow.BuyerCode);
			AssertEquals("temp temp", orderRow.BuyerContact);
			AssertEquals("Sydney Temp", orderRow.BuyerCompanyName);
			AssertEquals("Temp str. Sydney", orderRow.BuyerAddress1);
			AssertEquals("", orderRow.BuyerAddress2);
			AssertEquals("Sydney", orderRow.BuyerCity);
			AssertEquals("TEMP", orderRow.BuyerState);
			AssertEquals("09999", orderRow.BuyerPostCode);
			AssertEquals("AU", orderRow.BuyerISOCountryCode);
			AssertEquals("Australia", orderRow.BuyerCountryName);
			AssertEquals("AUSYD", orderRow.BuyerUNLOCO);
			AssertEquals("380503345663", orderRow.BuyerPhone);
			AssertEquals("temp@temp.com", orderRow.BuyerEmailAddress);
			AssertEquals("HouseBill", orderRow.HouseBill);
			AssertEquals("depvessel", orderRow.DepartureVesselFlight);
			AssertEquals("depvoyage", orderRow.DepartureVoyageFlight);
			AssertEquals("UAIEV", orderRow.GoodsOrigin);
			AssertEquals("AUSYD", orderRow.GoodsDestination);
			AssertEquals("avaliableat", orderRow.GoodsAvaliableAt);
			AssertEquals("deliveredto", orderRow.GoodsDeliveredTo);
			AssertEquals("AUBNE", orderRow.LoadPort);
			AssertEquals("AUMEL", orderRow.DischargePort);
			AssertEquals("SENDAG", orderRow.SendingAgent);
			AssertEquals("RECAG", orderRow.ReceivingAgent);
			AssertEquals(new ZDateTime(2005, 1, 1), orderRow.CustomDate1);
			AssertEquals(new ZDateTime(2005, 1, 2), orderRow.CustomDate2);
			AssertEquals("custattr1", orderRow.CustomAttrib1);
			AssertEquals("custattr2", orderRow.CustomAttrib2);
			AssertEquals("custattr3", orderRow.CustomAttrib3);
			AssertEquals("custattr4", orderRow.CustomAttrib4);
			AssertEquals("custattr5", orderRow.CustomAttrib5);
			AssertEquals(false, orderRow.CustomFlag1);
			AssertEquals(true, orderRow.CustomFlag2);
			AssertEquals(false, orderRow.CustomFlag3);
			AssertEquals(true, orderRow.CustomFlag4);
			AssertEquals(false, orderRow.CustomFlag5);
			AssertEquals(1.01m, orderRow.CustomDecimal1);
			AssertEquals(1.02m, orderRow.CustomDecimal2);
			AssertEquals(1.03m, orderRow.CustomDecimal3);
			AssertEquals(1.04m, orderRow.CustomDecimal4);
			AssertEquals(1.05m, orderRow.CustomDecimal5);
			AssertEquals("contact1", orderRow.CustomContact1);
			AssertEquals("contact2", orderRow.CustomContact2);
			AssertEquals("GOOOOOOOODs", orderRow.GoodsHandlingNotes);
			AssertEquals("add notes", orderRow.DGAdditionalHandlingNotes);
			AssertEquals("special instractions", orderRow.SpecialInstructions);
			AssertEquals("delivery instractions", orderRow.DeliveryInstructions);
		}
	}
}
