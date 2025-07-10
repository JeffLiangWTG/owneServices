using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportLinePriceValidatorTest : TestCaseWithFactory
	{
		public void TestTIBWatchAndValueValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			var invoiceLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			var invoiceLine4 = invoiceLine1.AddSecondaryInvoiceLine();

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLine1.US_SupTariff = "98130020";
				invoiceLine1.JI_Tariff = "9101118010";
				invoiceLine1.JI_LinePrice = 2158m;
				invoiceLine1.JI_CustomsQuantity = 3m;

				invoiceLine2.JI_Tariff = "9101118020";
				invoiceLine2.US_SupTariff = "";
				invoiceLine2.JI_LinePrice = 10062m;
				invoiceLine2.JI_CustomsQuantity = 9m;

				invoiceLine3.JI_Tariff = "9101118030";
				invoiceLine3.US_SupTariff = "";
				invoiceLine3.JI_LinePrice = 292m;
				invoiceLine3.JI_CustomsQuantity = 3m;

				invoiceLine4.JI_Tariff = "9101118040";
				invoiceLine4.US_SupTariff = "";
				invoiceLine4.JI_LinePrice = 16m;
				invoiceLine4.JI_CustomsQuantity = 3m;
			}

			Assert(!invoiceLine1.JI_LinePriceInfo.HasMessageError(ValidationConstants.InvoiceLine.ValueShouldBeDeclaredUnderSecondaryLines));
		}
	}
}
