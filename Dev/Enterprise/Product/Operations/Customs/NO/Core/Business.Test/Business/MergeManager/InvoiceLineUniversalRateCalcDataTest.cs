using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(InvoiceLineUniversalRateCalcData))]
sealed class InvoiceLineUniversalRateCalcDataTest : TestCaseWithFactory
{
	public void TestUnitOfMeasureValueList()
	{
		var invoiceLine = CreateNewInvoiceLine(12, "KG", 22, "NMB");
		invoiceLine.JI_CustomsThirdQuantity = 100;
		invoiceLine.JI_CustomsThirdUnitQty = "PCS";

		invoiceLine.JI_CustomsFourthQuantity = 52;
		invoiceLine.JI_CustomsFourthUnitQty = "NMB";

		invoiceLine.JI_CustomsFifthQuantity = 5;
		invoiceLine.JI_CustomsFifthUnitQty = "LTR";

		var data = new InvoiceLineUniversalRateCalcData(invoiceLine, Factory.New<RateView>());
		var uomQtyDictionary = data.UnitOfMeasureValueList;

		AssertContainsExactElementsInAnyOrder("Qty Values", new KeyValuePair<string, decimal>[] {
			new ("KG", 12),
			new ("KGM", 12),
			//Automatically added by conversion
			new ("GRM", 12000),
			new ("TNE", 0.012m),
			new ("DTN", 0.12m),

			new ("LTR", 5),
			//Automatically added by conversion
			new ("KLT", 0.005m),
			new ("HLT", 0.05m),

			new ("NMB", 74),
			new ("PCS", 100),

			//Automatically added by conversion
			new ("FLAT", 1)
		}, uomQtyDictionary);
	}

	JobComInvoiceLine CreateNewInvoiceLine(ZDecimal customsQty, ZString customsUnit, ZDecimal customsSecondQty, ZString customsSecondUnit)
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_CustomsQuantity = customsQty;
		invoiceLine.JI_CustomsUnitQty = customsUnit;
		invoiceLine.JI_CustomsSecondQuantity = customsSecondQty;
		invoiceLine.JI_CustomsSecondUnitQty = customsSecondUnit;
		return invoiceLine;
	}
}
