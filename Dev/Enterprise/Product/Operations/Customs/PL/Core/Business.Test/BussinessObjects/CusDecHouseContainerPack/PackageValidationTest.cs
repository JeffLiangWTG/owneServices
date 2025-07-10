using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

class PackageValidationTest : CusCodeDataValidationTest
{
	public void TestCheckCW_MarksAndNos_ShouldValidateMissingMarksAndNos()
	{
		var declaration = Factory.New<JobDeclaration>();
		var messageError = declaration.PackageMarksAndNumbersAlwaysRequiredValidationMessage;
		var package = declaration.Packages.AddNew();
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var bulkCodes = new string[] { "VQ", "VG", "VL", "VY", "VR", "VO", "VS" };
		CombineAssertions(() =>
		{
			package.Validation.ValidateCW_MarksAndNos();
			AssertHasMessageError("empty Pack", package.CW_MarksAndNosInfo, messageError);

			foreach (var bulkCode in bulkCodes)
			{
				package.CW_PackType = bulkCode;
				package.Validation.ValidateCW_MarksAndNos();
				AssertNoMessageError($"Bulk Code {bulkCode}", package.CW_MarksAndNosInfo, messageError);
			}
			package.CW_PackType = "L0";
			package.Validation.ValidateCW_MarksAndNos();
			AssertHasMessageError($"Not Bulk Code, not used on any invoiceLine", package.CW_MarksAndNosInfo, messageError);

			var line1Package = invoiceLine1.PackagesForInvoiceLinesForBindingOnly.AddNew();
			var line2Package = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.AddNew();
			line1Package.Package = package;
			line2Package.Package = package;

			line1Package.IsLinked = true;
			line2Package.IsLinked = true;
			line1Package.PackQty = 0;
			line2Package.PackQty = 0;
			package.Validation.ValidateCW_MarksAndNos();
			AssertHasMessageError($"Not Bulk Code, used on both invoiceLines with PackQty 0", package.CW_MarksAndNosInfo, messageError);

			line2Package.PackQty = 3;
			package.Validation.ValidateCW_MarksAndNos();
			AssertNoMessageError($"Not Bulk Code, used on both invoiceLines with Total PackQty > 0", package.CW_MarksAndNosInfo, messageError);
		});
	}
	public void TestCheckCW_PackType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var messageError = declaration.PackageMarksAndNumbersAlwaysRequiredValidationMessage;
		var package = declaration.Packages.AddNew();

		CombineAssertions(() =>
		{
			package.Validation.ValidateCW_PackType();
			AssertHasMessageErrorContaining("Empty Pack Type", package.CW_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);

			package.CW_PackType = "A";
			AssertNoMessageErrorContaining("Not empty Pack Type", package.CW_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}
}
