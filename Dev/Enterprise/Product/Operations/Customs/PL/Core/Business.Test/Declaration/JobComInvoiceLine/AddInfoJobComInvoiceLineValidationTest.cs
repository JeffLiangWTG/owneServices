using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class AddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCustomsUQListNoDeclaration()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertEquals("No Declaration falls back to logged in company and Todays date", "PL1, PL2", invoiceLine.Lookups.CustomsUQList.CodesAsString);
	}

	void SetupCustomsUQReferenceData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "PL1", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

		helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "PL2", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
		Factory.Save();
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetupCustomsUQReferenceData();
		declaration = Factory.New<JobDeclaration>();
		invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
}
