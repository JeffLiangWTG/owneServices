using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GovernmentUniformInvoiceDataValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Government Uniform Invoice Number.";
			governmentUniformInvoiceData.Validation.ValidateCY_Code();
			AssertHasMessageError(governmentUniformInvoiceData.CY_CodeInfo, messageError);
			governmentUniformInvoiceData.CY_Code = "XXX11";
			AssertNoMessageError(governmentUniformInvoiceData.CY_CodeInfo, messageError);
			var governmentUniformInvoiceData2 = declaration.GovernmentUniformInvoices.AddNew();
			governmentUniformInvoiceData2.CY_Code = "XXX11";
			AssertHasMessageErrorContaining(governmentUniformInvoiceData2.CY_CodeInfo, "This Number is duplicated.");
		}

		public void TestCheckCY_Data()
		{
			string messageError = MandatoryValidation.YouHaveNotEntered + " an Amount.";
			governmentUniformInvoiceData.Validation.ValidateCY_Data();
			AssertHasMessageError(governmentUniformInvoiceData.CY_DataInfo, messageError);
			governmentUniformInvoiceData.CY_Data = "12345";
			AssertNoMessageError(governmentUniformInvoiceData.CY_DataInfo, messageError);
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			governmentUniformInvoiceData = declaration.GovernmentUniformInvoices.AddNew();
			base.SetUp();
		}

		GovernmentUniformInvoiceData governmentUniformInvoiceData;
		JobDeclaration declaration;
	}
}
