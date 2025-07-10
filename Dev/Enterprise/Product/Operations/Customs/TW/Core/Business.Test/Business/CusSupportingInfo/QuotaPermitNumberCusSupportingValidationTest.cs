using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class QuotaPermitNumberCusSupportingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			var checkIfNotEnteredWhenIsTariffQuotaMessageError = "The invoice line is applying for a tariff quota. Please enter a 'Quota Permit Number'";
			invoiceLine.JI_Tariff = "98";
			invoiceLine.JI_ConcessionOrder = Constants.ConcessionOrder.Quota;
			invoiceLine.QuotaPermitNumber = "123";
			var targetInfo = invoiceLine.QuotaPermitNumberInfo;
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.QuotaPermitNumber = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.JI_Tariff = "97";
			invoiceLine.JI_ConcessionOrder = Constants.ConcessionOrder.Quota;
			invoiceLine.QuotaPermitNumber = "123";
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.QuotaPermitNumber = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.JI_Tariff = "98";
			invoiceLine.JI_ConcessionOrder = ZString.Empty;
			invoiceLine.QuotaPermitNumber = "123";
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.QuotaPermitNumber = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.JI_Tariff = "97";
			invoiceLine.JI_ConcessionOrder = ZString.Empty;
			invoiceLine.QuotaPermitNumber = "123";
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.QuotaPermitNumber = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
		}

		public void TestCheckCheckCSI_LineNo()
		{
			var checkIfNotEnteredWhenIsTariffQuotaMessageError = "The invoice line is applying for a tariff quota. Please enter a 'Quota Permit Line Number'.";
			invoiceLine.JI_Tariff = "98";
			invoiceLine.JI_ConcessionOrder = Constants.ConcessionOrder.Quota;
			invoiceLine.QuotaPermitNumberItemNumber = 0;
			var targetInfo = invoiceLine.QuotaPermitNumberItemNumberInfo;
			AssertHasMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.QuotaPermitNumberItemNumber = 1;
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.JI_Tariff = "97";
			invoiceLine.JI_ConcessionOrder = Constants.ConcessionOrder.Quota;
			invoiceLine.QuotaPermitNumberItemNumber = 0;
			AssertHasMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.QuotaPermitNumberItemNumber = 1;
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.JI_Tariff = "98";
			invoiceLine.JI_ConcessionOrder = ZString.Empty;
			invoiceLine.QuotaPermitNumberItemNumber = 0;
			AssertHasMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.QuotaPermitNumberItemNumber = 1;
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.JI_Tariff = "97";
			invoiceLine.JI_ConcessionOrder = ZString.Empty;
			invoiceLine.QuotaPermitNumberItemNumber = 0;
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
			invoiceLine.QuotaPermitNumberItemNumber = 1;
			AssertNoMessageErrorContaining(targetInfo, checkIfNotEnteredWhenIsTariffQuotaMessageError);
		}

		#region Implementation
		JobDeclaration jobDeclaration;
		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			invoiceLine = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		}
		#endregion
	}
}
