using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.PL;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(AddInfoJobComInvoiceLine))]
sealed class AddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
{
	public void TestGetNewValidation() =>
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertType<AddInfoJobComInvoiceLineValidation>("Export Declaration", invoiceLine.AddInfo.Validation);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertType<ImportAddInfoJobComInvoiceLineValidation>("Import Declaration", invoiceLine.AddInfo.Validation);

			declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
			AssertType<EU.Business.Declaration.AddInfoJobComInvoiceLineValidation>("Exit Summary Declaration", invoiceLine.AddInfo.Validation);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<EU.Business.Declaration.AddInfoJobComInvoiceLineValidation>("Miscellaneous Declaration", invoiceLine.AddInfo.Validation);
		});

	public void TestZG_CountryOfSupplyMaxLength() => AssertEquals(4, invoiceLine.ZG_CountryOfSupplyInfo.MaxLength);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;

	protected override BusinessObject GetNewBusinessObject() => new AddInfoJobComInvoiceLine(Factory.New<JobComInvoiceLine>().JI_AddInfoInfo);
}
