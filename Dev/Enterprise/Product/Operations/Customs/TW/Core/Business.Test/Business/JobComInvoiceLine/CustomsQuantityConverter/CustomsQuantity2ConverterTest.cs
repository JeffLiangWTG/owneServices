using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CustomsQuantity2ConverterTest : BaseCustomsQuantityConverterTest
	{
		protected override BaseCustomsQuantityConverter GetBaseCustomsQuantityConverter(JobComInvoiceLine invoiceLine) => new CustomsQuantity2Converter(invoiceLine);
		protected override ZPropertyInfo GetQuantityPropertyInfo(JobComInvoiceLine invoiceLine) => invoiceLine.JI_CustomsSecondQuantityInfo;
		protected override ZPropertyInfo GetUnitPropertyInfo(JobComInvoiceLine invoiceLine) => invoiceLine.JI_CustomsSecondUnitQtyInfo;

		public override void TestQuantityDecimalPlace()
		{
			var refPacks = Factory.New<CusRefPacks>();
			refPacks.RP_ConversionFactor = 1m;
			refPacks.RP_CustomsPack = "MTK";
			refPacks.RP_CommercialPack = "MTR";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "MTR";
			invoiceLine.JI_CustomsSecondUnitQty = "MTK";

			var converter = GetBaseCustomsQuantityConverter(invoiceLine);
			invoiceLine.JI_InvoiceQuantity = 123.00049;
			converter.CalculateCustomsFactorAndQty();
			AssertEquals(123.0005m, invoiceLine.JI_CustomsSecondQuantity);

			invoiceLine.JI_InvoiceQuantity = 123.000449;
			converter.CalculateCustomsFactorAndQty();
			AssertEquals(123.0004m, invoiceLine.JI_CustomsSecondQuantity);
		}

		public void TestCalculateCustomsSecondQuantityFromClothMeterialsDimetionIsRequired()
		{
			using (UnitConverter.TemporarySetupCachedConvertion(Factory))
			{
				var refPacks = Factory.New<CusRefPacks>();
				refPacks.RP_ConversionFactor = 99m;
				refPacks.RP_CustomsPack = "MTK";
				refPacks.RP_CommercialPack = "MTR";
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var hsnTariffType = helper.CreateNewOrGetExistingTariffType("TW", "HSN");
				var tariffType = helper.CreateTariffType("TW", "TXX");
				Factory.Save();
				var tariff0000000021 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				helper.CreateTariffUOM(tariff0000000021.PK, "CU1", "A");
				helper.CreateTariffUOM(tariff0000000021.PK, "CU2", "MTK");
				var tariff1000000021 = helper.CreateTariff("TW", tariffType.PK, "1000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
				helper.CreateTariffUOM(tariff1000000021.PK, "CU1", "A");
				helper.CreateTariffUOM(tariff1000000021.PK, "CU2", "MTO");
				Factory.Save();
				var declaration = Factory.New<JobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1000000021";
				invoiceLine.JI_InvoiceUQ = "MTR";
				invoiceLine.JI_CustomsSecondUnitQty = "MTK";
				invoiceLine.JI_InvoiceQuantity = 2;
				AssertEquals(198m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidth = 3;
				AssertEquals(198m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidthUQ = "M";
				AssertEquals(198m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_Tariff = "0000000021";
				AssertEquals(6m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidthUQ = ZString.Empty;
				AssertEquals(198m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidth = 0;
				invoiceLine.JI_TextileWidthUQ = "M";
				AssertEquals(198m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_CustomsSecondUnitQty = "MTR";
				invoiceLine.JI_InvoiceQuantity = 2;
				AssertEquals(2m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_CustomsSecondUnitQty = "MTK";
				invoiceLine.JI_InvoiceUQ = "MQH";
				AssertEquals(198m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidth = 1m;
				AssertEquals(198m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidth = 300m;
				invoiceLine.JI_InvoiceUQ = "MTR";
				AssertEquals(600m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidthUQ = "CM";
				AssertEquals(6m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidthUQ = "MM";
				AssertEquals(0.6m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidthUQ = "IN";
				AssertEquals(15.24m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_TextileWidthUQ = "FT";
				AssertEquals(182.88m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_InvoiceUQ = "CMT";
				AssertEquals(1.8288m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_InvoiceUQ = "MMT";
				AssertEquals(0.1829m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_InvoiceUQ = "INH";
				AssertEquals(4.6452m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_InvoiceUQ = "FOT";
				AssertEquals(55.7418m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_InvoiceUQ = "YDS";
				AssertEquals(167.2255m, invoiceLine.JI_CustomsSecondQuantity);
				invoiceLine.JI_InvoiceUQ = "YRD";
				AssertEquals(167.2255m, invoiceLine.JI_CustomsSecondQuantity);
			}
		}
	}
}
