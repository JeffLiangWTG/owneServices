using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

public class TranCircumstanceValidationTest : CusCodeDataValidationTest
{
	public new void TestCheckCY_Code()
	{
		var data = Factory.New<TranCircumstance>();
		AssertNoNotifications(data.CY_CodeInfo);

		data.CY_Code = TranCircumstancesList.Codes.A00PL;
		AssertNoNotifications(data.CY_CodeInfo);

		data.CY_Code = "AAAAA";
		AssertHasNotifications(data.CY_CodeInfo);
		AssertHasMessageError(data.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.TranCircumstanceCode1 = TranCircumstancesList.Codes.A00PL;
		var additionalTranCode1 = invoiceHeader.AdditionalTranCircumstanceCodes.AddNew();

		additionalTranCode1.CY_Code = TranCircumstancesList.Codes.B00PL;
		AssertHasMessageError(additionalTranCode1.CY_CodeInfo, "Transaction Circumstances combination is not allowed (B00PL, A00PL)");
		AssertNoMessageError(additionalTranCode1.CY_CodeInfo, "Duplicate transaction circumstance found.");

		additionalTranCode1.CY_Code = TranCircumstancesList.Codes.A00PL;
		AssertHasMessageError(additionalTranCode1.CY_CodeInfo, "Duplicate transaction circumstance found.");
		additionalTranCode1.CY_Code = "asdf";
		AssertHasMessageError(additionalTranCode1.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

		additionalTranCode1.CY_Code = TranCircumstancesList.Codes.A00PL;
		AssertHasMessageError(additionalTranCode1.CY_CodeInfo, "Duplicate transaction circumstance found.");
		additionalTranCode1.CY_Code = ZString.Empty;
		AssertNoNotifications(invoiceHeader.TranCircumstanceCode1Info);

		additionalTranCode1.CY_Code = TranCircumstancesList.Codes.A00PL;
		invoiceHeader.TranCircumstanceCode1 = ZString.Empty;
		AssertNoNotifications(invoiceHeader.TranCircumstanceCode1Info);
		AssertNoNotifications(additionalTranCode1.CY_CodeInfo);
	}
}
