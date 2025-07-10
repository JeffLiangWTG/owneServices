using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateJZ_OH_Supplier_TIN()
	{
		AssertSupplierHasRequiredNumber("Selected Organization doesn't have a valid TIN number.", OrgCusCode.PolandCodeTypes.TIN, true);
	}

	public void TestValidateJZ_OH_Supplier_EOR()
	{
		AssertSupplierHasRequiredNumber("Selected Organization doesn't have a valid EOR number.", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, false);
	}

	void AssertSupplierHasRequiredNumber(string messageError, string codeType, bool numberMustBePL)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		var invoiceHeader = declaration.Invoices.AddNew();

		var supplier = Factory.New<OrgHeader>();
		var supplierAddress = supplier.Addresses.AddNew();
		supplierAddress.OA_Address1 = "Some street";

		invoiceHeader.JZ_OH_Supplier = supplier.PK;

		var cusCode = supplierAddress.CustomsCodes.AddNew();
		CombineAssertions(() =>
		{
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageError("Invalid code type and empty number", invoiceHeader.JZ_OH_SupplierInfo, messageError);

			cusCode.OK_CodeType = codeType;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageError("Valid code type but empty number", invoiceHeader.JZ_OH_SupplierInfo, messageError);

			if (numberMustBePL)
			{
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;
				cusCode.OK_CustomsRegNo = "12345";
				invoiceHeader.Validation.ValidateJZ_OH_Supplier();
				AssertHasMessageError("Not PL", invoiceHeader.JZ_OH_SupplierInfo, messageError);

				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
				invoiceHeader.Validation.ValidateJZ_OH_Supplier();
				AssertNoMessageError("Valid PL Number", invoiceHeader.JZ_OH_SupplierInfo, messageError);
			}
			else
			{
				cusCode.OK_CustomsRegNo = "12345";
				invoiceHeader.Validation.ValidateJZ_OH_Supplier();
				AssertNoMessageError("Valid", invoiceHeader.JZ_OH_SupplierInfo, messageError);
			}

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Import", invoiceHeader.JZ_OH_SupplierInfo, messageError);
		});
	}
}
