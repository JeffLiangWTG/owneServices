using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportStatQtyValidatorTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_Unit2 = "LTR";
			tariff.UE_DateFrom = new ZDateTime(2008, 09, 11);
			tariff.UE_DateTo = ZDateTime.Now;

			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			invoiceLine.JI_CustomsSecondQuantity = -1m;
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ValidationConstants.NegativeAmountNotAllowed);

			invoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertNoWarning(invoiceLine.JI_CustomsSecondQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);
			AssertHasMessageError(invoiceLine.JI_CustomsSecondQuantityInfo, ValidationConstants.StatQTYRequired);

			invoiceLine.JI_CustomsSecondQuantity = 40m;
			AssertNoWarnings(invoiceLine.JI_CustomsSecondQuantityInfo);
			AssertNoMessageErrors(invoiceLine.JI_CustomsSecondQuantityInfo);

			var subInvoiceLine = invoice.InvoiceLines.AddNew();
			subInvoiceLine.JI_ParentID = invoiceLine.PK;
			subInvoiceLine.JI_CustomsSecondUnitQty = "KG";
			subInvoiceLine.JI_Tariff = tariff.UE_Tariff;

			AssertNoMessageErrorContaining(subInvoiceLine.JI_CustomsSecondQuantityInfo, ValidationConstants.StatQTYRequired);
			AssertHasWarning(subInvoiceLine.JI_CustomsSecondQuantityInfo, ValidationConstants.WarnIfStatQTYisZero);
		}
	}
}
