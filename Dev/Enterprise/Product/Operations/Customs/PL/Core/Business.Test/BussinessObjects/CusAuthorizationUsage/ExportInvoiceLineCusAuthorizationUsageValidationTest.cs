using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ExportInvoiceLineCusAuthorizationUsageValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAGC_OH_Owner()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var authorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
		var propertyInfo = authorizationUsage.AGC_OH_OwnerInfo;
		var validation = authorizationUsage.Validation;

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("default values", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			authorizationUsage.AGC_Code = "321";
			validation.ValidateAGC_OH_Owner();
			AssertHasMessageErrorContaining("Owner can't be empty", propertyInfo, MandatoryValidation.YouHaveNotEntered);

			authorizationUsage.AGC_OH_Owner = ZGuid.NewZGuid();
			AssertNoMessageErrorContaining("Onwer is not empty", propertyInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}
}
