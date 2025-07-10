
namespace Enterprise.eManifest.Testing.Business
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.eManifest.Business;
	using Enterprise.Freight.Business;

	public class ShipmentExtensionTest : TestCaseWithFactory
	{
		public void TestGetCurrencyAtDestination_ShipmentIsNull_ThrowArgumentNullException()
		{
			CommonShipment shipment = null;

			AssertExceptionThrown<ArgumentNullException>(() => shipment.GetCurrencyAtDestination());
		}

		public void TestGetCurrencyAtDestination_DestinationIsNotSpecified_ReturnUSDCurrency()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKDestination = ZString.Empty;

			var currency = shipment.GetCurrencyAtDestination();

			AssertEquals("Currency", "USD", currency.RX_Code);
		}

		public void TestGetCurrencyAtDestination_DestinationIsSpecified_ReturnCurrencyAtDestination()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKDestination = "UAIEV";

			var currency = shipment.GetCurrencyAtDestination();

			AssertEquals("Currency", "UAH", currency.RX_Code);
		}

		public void TestGetSupplierBookingLines_ShipmentIsNull_ThrowArgumentNullException()
		{
			CommonShipment shipment = null;

			AssertExceptionThrown<ArgumentNullException>(() => shipment.GetCurrencyAtDestination());
		}

		public void TestGetSupplierBookingLines_BookingLineOnShipmentExist_ReturnBookingLines()
		{
			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			var shipment2 = Factory.NewWithValidTestData<CommonShipment>();

			var line1 = Factory.NewWithValidTestData<SupplierBookingLine>();
			var line2 = Factory.NewWithValidTestData<SupplierBookingLine>();
			var line3 = Factory.NewWithValidTestData<SupplierBookingLine>();

			line1.DL_JS_ApprovedShipment = shipment1.PK;
			line2.DL_JS_ApprovedShipment = shipment1.PK;
			line3.DL_JS_ApprovedShipment = shipment2.PK;

			var expectedLinePKs = new[] { line1.PK, line2.PK };
			var actualLinePKs = shipment1.GetSupplierBookingLines().Select(l => l.PK);
			AssertContainsExactElementsInAnyOrder("Supplier booking lines", expectedLinePKs, actualLinePKs);

			expectedLinePKs = new[] { line3.PK };
			actualLinePKs = shipment2.GetSupplierBookingLines().Select(l => l.PK);
			AssertContainsExactElementsInAnyOrder("Supplier booking lines", expectedLinePKs, actualLinePKs);
		}
	}
}
