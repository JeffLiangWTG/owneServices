using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckDeclarationGoodsOriginWillBeUsed() => CombineAssertions(() =>
	{
		const string warningMessage = "The Declaration/[15] Dispatch country will be used.";

		declaration.JE_GoodsOrigin = ZString.Empty;
		invoiceLine.ZG_CountryOfSupply = ZString.Empty;
		AssertNoWarning("Empty ZG_CountryOfSupply and JE_GoodsOrigin", invoiceLine.ZG_CountryOfSupplyInfo, warningMessage);

		invoiceLine.ZG_CountryOfSupply = CountryCodes.Poland;
		AssertNoWarning("Empty JE_GoodsOrigin", invoiceLine.ZG_CountryOfSupplyInfo, warningMessage);

		declaration.JE_GoodsOrigin = CountryCodes.Poland;
		invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
		AssertNoWarning("Same ZG_CountryOfSupply and JE_GoodsOrigin", invoiceLine.ZG_CountryOfSupplyInfo, warningMessage);

		invoiceLine.ZG_CountryOfSupply = ZString.Empty;
		AssertHasWarning("Empty ZG_CountryOfSupply and JE_GoodsOrigin is not empty", invoiceLine.ZG_CountryOfSupplyInfo, warningMessage);
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
}
