using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SecondaryTariffCreatorTest : TestCaseWithFactory
	{
		public void TestAddSecondaryTariffsFromSecondaryTariffRules_MultipleSecondaryRules()
		{
			var tariffRule = AddSecondaryTariffRule("0000000000", "1111111111", "1111111112");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "0000000000";
			AssertEquals("Secondary tariff lines are created", 2, invoiceLine.SecondaryTariffLines.Count());
			AssertEquals("Tariff is added in that order", "1111111111", declaration.InvoiceLines[1].JI_Tariff);
			AssertEquals("Tariff is added in that order", "1111111112", declaration.InvoiceLines[2].JI_Tariff);

			var secondaryTariff2 = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff2.U3_DateFrom = ZDateTime.BrettsBirthday;
			secondaryTariff2.U3_TariffFrom = "1111111113";
			secondaryTariff2.U3_Tariff2 = "1111111114";

			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000000";
			AssertEquals("No secondary tariffs cannot be added as there are two possibilities", 0, invoiceLine.SecondaryTariffLines.Count());
		}

		public void TestAddSecondaryTariffsAccordingToRuleWhenParentIsuncommitted()
		{
			var tariffRule = AddSecondaryTariffRule("0000000000", "1111111111", "1111111112");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = (JobComInvoiceLine)((System.ComponentModel.IBindingList)declaration.InvoiceLines).AddNew();//to add an uncommitted row
			AssertNoExceptionThrown(() => invoiceLine.JI_Tariff = "0000000000");

			AssertEquals("2 secondary lines are added", 2, invoiceLine.SecondaryTariffLines.Count());
		}

		//Watch repair
		public void TestAddSecondaryTariffsWhenSuplementaryTariffExists()
		{
			AddSecondaryTariffRule("0000000000", "1111111111", "1111111112");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802004040";    // repairs
			invoiceLine.JI_Tariff = "0000000000";//primary tariff

			AssertEquals("2 secondary tariff lines", 2, invoiceLine.SecondaryTariffLines.Count());
			AssertEquals((ZShort)1, invoiceLine.JI_LineNo);

			invoiceLine = invoice.JobComInvoiceLines.GetByLineNo(2);
			AssertEquals("1111111111", invoiceLine.JI_Tariff);
			AssertEquals("9802004040", invoiceLine.US_SupTariff);

			invoiceLine = invoice.JobComInvoiceLines.GetByLineNo(3);
			AssertEquals("1111111112", invoiceLine.JI_Tariff);
			AssertEquals("9802004040", invoiceLine.US_SupTariff);
		}

		public void TestAddSecondaryTariffsLineNumbers()
		{
			AddSecondaryTariffRule("2222222221", "2222222222", "2222222223");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2222222221";
			AssertEquals("2 secondary tariff lines", 2, invoiceLine1.SecondaryTariffLines.Count());
			AssertEquals("Line Number 1", (short)1, invoiceLine1.JI_LineNo);
			AssertEquals("Line Number 2", (short)2, invoiceLine1.SecondaryTariffLines.ElementAt(0).JI_LineNo);
			AssertEquals("Line Number 3", (short)3, invoiceLine1.SecondaryTariffLines.ElementAt(1).JI_LineNo);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2222222221";
			AssertEquals("2 secondary tariff lines", 2, invoiceLine2.SecondaryTariffLines.Count());
			AssertEquals("Line Number 4", (short)4, invoiceLine2.JI_LineNo);
			AssertEquals("Line Number 5", (short)5, invoiceLine2.SecondaryTariffLines.ElementAt(0).JI_LineNo);
			AssertEquals("Line Number 6", (short)6, invoiceLine2.SecondaryTariffLines.ElementAt(1).JI_LineNo);

			AddSecondaryTariffRule("3333333331", "3333333332", "3333333333", "3333333334");

			invoiceLine1.JI_Tariff = "3333333331";
			AssertEquals("3 secondary tariff lines", 3, invoiceLine1.SecondaryTariffLines.Count());
			AssertEquals("Line Number 1", (short)1, invoiceLine2.JI_LineNo);
			AssertEquals("Line Number 2", (short)2, invoiceLine2.SecondaryTariffLines.ElementAt(0).JI_LineNo);
			AssertEquals("Line Number 3", (short)3, invoiceLine2.SecondaryTariffLines.ElementAt(1).JI_LineNo);
			AssertEquals("Line Number 4", (short)4, invoiceLine1.JI_LineNo);
			AssertEquals("Line Number 5", (short)5, invoiceLine1.SecondaryTariffLines.ElementAt(0).JI_LineNo);
			AssertEquals("Line Number 6", (short)6, invoiceLine1.SecondaryTariffLines.ElementAt(1).JI_LineNo);
			AssertEquals("Line Number 7", (short)7, invoiceLine1.SecondaryTariffLines.ElementAt(2).JI_LineNo);
		}

		public void TestAddSecondaryTariffsForTIB()
		{
			AddSecondaryTariffRule("0000000000", "1111111111", "1111111112");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9813000520";
			invoiceLine.JI_Tariff = "0000000000";

			AssertEquals("2 secondary tariff lines", 2, invoiceLine.SecondaryTariffLines.Count());
			AssertEquals((ZShort)1, invoiceLine.JI_LineNo);

			invoiceLine = invoice.JobComInvoiceLines.GetByLineNo(2);
			AssertEquals("1111111111", invoiceLine.JI_Tariff);
			AssertEquals("", invoiceLine.US_SupTariff);

			invoiceLine = invoice.JobComInvoiceLines.GetByLineNo(3);
			AssertEquals("1111111112", invoiceLine.JI_Tariff);
			AssertEquals("", invoiceLine.US_SupTariff);
		}

		USCTariffRule AddSecondaryTariffRule(ZString parentTariffNumber, params ZString[] secondaryTariffNumbers)
		{
			CreateUSCTariffRecord(parentTariffNumber);

			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_Tariff = parentTariffNumber;

			var secondaryTariff = tariffRule.SecondaryTariffs.AddNew();
			secondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;

			if (secondaryTariffNumbers.Length > 0)
			{
				secondaryTariff.U3_TariffFrom = secondaryTariffNumbers[0];
				CreateUSCTariffRecord(secondaryTariffNumbers[0]);
			}

			if (secondaryTariffNumbers.Length > 1)
			{
				secondaryTariff.U3_Tariff2 = secondaryTariffNumbers[1];
				CreateUSCTariffRecord(secondaryTariffNumbers[1]);
			}

			if (secondaryTariffNumbers.Length > 2)
			{
				secondaryTariff.U3_Tariff3 = secondaryTariffNumbers[2];
				CreateUSCTariffRecord(secondaryTariffNumbers[2]);
			}

			return tariffRule;
		}

		USCTariff CreateUSCTariffRecord(ZString tariffNumber)
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			return tariff;
		}
	}
}
