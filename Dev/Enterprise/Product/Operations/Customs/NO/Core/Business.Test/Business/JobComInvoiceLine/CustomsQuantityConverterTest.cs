using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class CustomsQuantityConverterTest : BaseCustomsQuantityConverterTest
	{
		public void TestCalculateFromNetWeightToCustomsQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var customsQuantityConverter = invoiceLine.CustomsQuantityConverter;

			CombineAssertions(() =>
			{
				invoiceLine.JI_NetWeight = 234m;
				invoiceLine.JI_CustomsUnitQty = "KGM";

				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("KG -> KGM", 234m, customsQuantityConverter.CalculateFromNetWeightToCustomsQtyCore());

				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Milligrams;
				AssertEquals("MG -> KGM", 0.000234m, customsQuantityConverter.CalculateFromNetWeightToCustomsQtyCore());

				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Ounces;
				AssertEquals("OZ -> KGM", 6.63378841125m, customsQuantityConverter.CalculateFromNetWeightToCustomsQtyCore(), delta: 0.000001m);

				invoiceLine.JI_CustomsUnitQty = "MG";
				invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("KG -> MG", 234000000m, customsQuantityConverter.CalculateFromNetWeightToCustomsQtyCore());
			});
		}
	}
}
