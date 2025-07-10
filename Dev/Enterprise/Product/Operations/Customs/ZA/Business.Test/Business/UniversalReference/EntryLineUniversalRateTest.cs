using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class TestEntryLineUniversalRate : TestCaseWithFactory
	{
		public void TestValueForDutyCalculateFromFormula()
		{
			var testCusEntryLine = Factory.NewWithValidTestData<CusEntryLine>();
			testCusEntryLine.CL_CustomsValue = 1000m;
			var tester = new EntryLineUniversalRate(testCusEntryLine, testCusEntryLine.CL_CustomsValue);
			AssertEquals(1000m, tester.CustomsValue);
			AssertEquals(1000m, tester.ValueForDuty);
			AssertEquals(ZString.Empty, tester.CustomsValueFormula);

			tester.CustomsValueFormula = "CV / 2";
			AssertEquals(1000m, tester.CustomsValue);
			AssertEquals(500m, tester.ValueForDuty);
			AssertEquals("CV / 2", tester.CustomsValueFormula);

			tester.CountrySpecificValueListUpdateOrAddNew("1P1", 200);
			tester.CountrySpecificValueListUpdateOrAddNew("3P1", 100);
			tester.CustomsValueFormula = "(CV * 1.15) + 1P1 - 3P1";
			AssertEquals(1000m, tester.CustomsValue);
			AssertEquals(1250m, tester.ValueForDuty);
			AssertEquals("(CV * 1.15) + 1P1 - 3P1", tester.CustomsValueFormula);

			tester.CustomsValueFormula = "";
			AssertEquals(1000m, tester.CustomsValue);
			AssertEquals(1000m, tester.ValueForDuty);
			AssertEquals(ZString.Empty, tester.CustomsValueFormula);

			tester = new EntryLineUniversalRate(testCusEntryLine, testCusEntryLine.CL_CustomsValue);
			tester.CountrySpecificValueListUpdateOrAddNew("1P1", 200.50);
			tester.CountrySpecificValueListUpdateOrAddNew("3P1", 100);
			tester.CustomsValueFormula = "(CV * 1.15) + 1P1 - 3P1";
			AssertEquals(1000m, tester.CustomsValue);
			AssertEquals(1250m, tester.ValueForDuty);
			AssertEquals("(CV * 1.15) + 1P1 - 3P1", tester.CustomsValueFormula);

			tester = new EntryLineUniversalRate(testCusEntryLine, testCusEntryLine.CL_CustomsValue);
			tester.CountrySpecificValueListUpdateOrAddNew("1P1", 200.51);
			tester.CountrySpecificValueListUpdateOrAddNew("3P1", 100);
			tester.CustomsValueFormula = "(CV * 1.15) + 1P1 - 3P1";
			AssertEquals(1000m, tester.CustomsValue);
			AssertEquals(1251m, tester.ValueForDuty);
			AssertEquals("(CV * 1.15) + 1P1 - 3P1", tester.CustomsValueFormula);

			testCusEntryLine.CL_CustomsValue = 1481m;
			tester = new EntryLineUniversalRate(testCusEntryLine, testCusEntryLine.CL_CustomsValue);
			tester.CountrySpecificValueListUpdateOrAddNew("1P1", 222.15);
			tester.CountrySpecificValueListUpdateOrAddNew("3P1", 0);
			tester.CustomsValueFormula = "(CV * 1.15) + 1P1 - 3P1";
			AssertEquals(1481m, tester.CustomsValue);
			AssertEquals(1925m, tester.ValueForDuty);
			AssertEquals("(CV * 1.15) + 1P1 - 3P1", tester.CustomsValueFormula);
		}

		public void TestAggregationRounding()
		{
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			var invoiceLine3 = Factory.New<JobComInvoiceLine>();

			invoiceLine1.JI_CustomsUnitQty = "A";
			invoiceLine1.JI_CustomsQuantity = 1.0015m;
			invoiceLine1.JI_CustomsSecondUnitQty = "A";
			invoiceLine1.JI_CustomsSecondQuantity = 1.0015m;
			invoiceLine1.JI_CustomsThirdUnitQty = "B";
			invoiceLine1.JI_CustomsThirdQuantity = 1.002m;

			invoiceLine2.JI_CustomsUnitQty = "A";
			invoiceLine2.JI_CustomsQuantity = 1.0015;
			invoiceLine2.JI_CustomsSecondUnitQty = "B";
			invoiceLine2.JI_CustomsSecondQuantity = 1.0024m;
			invoiceLine2.JI_CustomsThirdUnitQty = "C";
			invoiceLine2.JI_CustomsThirdQuantity = 1.0025m;

			invoiceLine3.JI_CustomsUnitQty = "A";
			invoiceLine3.JI_CustomsQuantity = 1.0005m;
			invoiceLine3.JI_CustomsSecondUnitQty = "C";
			invoiceLine3.JI_CustomsSecondQuantity = 1.0025m;
			invoiceLine3.JI_CustomsThirdUnitQty = "D";
			invoiceLine3.JI_CustomsThirdQuantity = 1.00499m;

			var cusEntryLine = Factory.NewWithValidTestData<CusEntryLine>();
			cusEntryLine.InvoiceLines.Add(invoiceLine1);
			cusEntryLine.InvoiceLines.Add(invoiceLine2);
			cusEntryLine.InvoiceLines.Add(invoiceLine3);

			var tester = new EntryLineUniversalRate(cusEntryLine, 1000m);

			var uomDict = (tester as IUniversalRateCalcData).UnitOfMeasureValueList;
			AssertEquals(4, uomDict.Count);
			CombineAssertions(() =>
			{
				AssertEquals(4.01m, uomDict["A"]);
				AssertEquals(2.00m, uomDict["B"]);
				AssertEquals(2.01m, uomDict["C"]);
				AssertEquals(1.00m, uomDict["D"]);
			});
		}
	}
}
