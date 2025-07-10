using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.PL;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExitSummaryAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_CountryOfDestination()
	{
		invoiceLine.Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Australia;

		CombineAssertions(() =>
		{
			invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.NewZealand;
			AssertNoNotifications("Country of Destination is not the same as Goods Destionation", invoiceLine.ZG_CountryOfDestinationInfo);

			invoiceLine.ZG_CountryOfDestination = ZString.Empty;
			AssertNoNotifications("Empty Country of Destination", invoiceLine.ZG_CountryOfDestinationInfo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
	}

	JobComInvoiceLine invoiceLine;
}
