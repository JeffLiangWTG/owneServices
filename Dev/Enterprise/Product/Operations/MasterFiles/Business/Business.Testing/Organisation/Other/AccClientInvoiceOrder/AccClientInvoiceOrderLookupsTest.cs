using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccClientInvoiceOrderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeCodes()
		{
			AccClientInvoiceOrder invoiceOrder = Factory.New<AccClientInvoiceOrder>();
			AccClientInvoiceOrderLookups invoiceOrderLookups = new AccClientInvoiceOrderLookups(invoiceOrder);
			invoiceOrderLookups.ChargeCodes.Load();
			Assert("Collection must have at least one element", invoiceOrderLookups.ChargeCodes.Count > 0);
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK));
			AssertCollectionContains(chargeCode, invoiceOrderLookups.ChargeCodes);

			chargeCode.AC_IsActive = false;

			invoiceOrderLookups = new AccClientInvoiceOrderLookups(invoiceOrder);
			invoiceOrderLookups.ChargeCodes.Load();
			Assert("Collection must have at least one element", invoiceOrderLookups.ChargeCodes.Count > 0);
			AssertCollectionNotContains(chargeCode, invoiceOrderLookups.ChargeCodes);

			CreateChargeCode("MJA");

			invoiceOrderLookups = new AccClientInvoiceOrderLookups(invoiceOrder);
			invoiceOrderLookups.ChargeCodes.Load();

			AccChargeCode chargeCodeToMatch = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "MJA"));
			AssertCollectionContains("Charge Lookup contains MJA type charge", chargeCodeToMatch, invoiceOrderLookups.ChargeCodes);
		}

		AccChargeCode CreateChargeCode(string chargeType)
		{
			AccChargeCode chargeCode = new BusinessObjectFactory().NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "ZZ" + chargeType;
			chargeCode.AC_ChargeType = chargeType;

			chargeCode.Factory.Save();

			return chargeCode;
		}
	}
}
