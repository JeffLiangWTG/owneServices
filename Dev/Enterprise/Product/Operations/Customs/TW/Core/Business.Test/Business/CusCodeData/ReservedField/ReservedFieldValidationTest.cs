using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ReservedFieldValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			var declaration = Factory.New<JobDeclaration>();
			ReservedField reservedField = declaration.ReservedFields.AddNew();
			AssertCY_Code(reservedField);

			ReservedField reservedField2 = declaration.ReservedFields.AddNew();
			CheckDuplicateCode(reservedField, reservedField2);

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			reservedField = invoiceLine.ReservedFields.AddNew();
			AssertCY_Code(reservedField);

			reservedField2 = invoiceLine.ReservedFields.AddNew();
			CheckDuplicateCode(reservedField, reservedField2);
		}

		[ExpectNoExceptions]
		public void TestCheckCY_Data()
		{
			var declaration = Factory.New<JobDeclaration>();
			ReservedField reservedField = declaration.ReservedFields.AddNew();
			reservedField.CY_Data = ZString.Empty;
			AssertHasMessageErrorContaining(reservedField.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			reservedField.CY_Data = "A1你好";
			NUnit.Framework.Assert.That(reservedField.CY_Data, NUnit.Framework.Is.EqualTo("A1你好").Using(CustomComparers.TypeComparison));
			AssertNoMessageErrorContaining(reservedField.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			reservedField = invoiceLine.ReservedFields.AddNew();
			reservedField.CY_Data = ZString.Empty;
			AssertHasMessageErrorContaining(reservedField.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			reservedField.CY_Data = "A1你好";
			NUnit.Framework.Assert.That(reservedField.CY_Data, NUnit.Framework.Is.EqualTo("A1你好").Using(CustomComparers.TypeComparison));
			AssertNoMessageErrorContaining(reservedField.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void CheckDuplicateCode(ReservedField reservedField, ReservedField reservedField2)
		{
			reservedField.CY_Code = "2";
			reservedField2.CY_Code = "2";
			AssertHasMessageErrorContaining(reservedField2.CY_CodeInfo, ValidationConstants.CusCodeData.ReservedFieldDuplicated);
		}

		void AssertCY_Code(ReservedField reservedField)
		{
			string messageError = MandatoryValidation.MustBeEntered;
			reservedField.Validation.ValidateCY_Code();
			AssertHasErrorContaining(reservedField.CY_CodeInfo, messageError);

			reservedField.CY_Code = "你";
			AssertNoErrorContaining(reservedField.CY_CodeInfo, messageError);
			AssertHasMessageError(reservedField.CY_CodeInfo, ValidationConstants.CusCodeData.CodeIsLettersAndNumbersOnly);

			reservedField.CY_Code = "a";
			AssertHasMessageError(reservedField.CY_CodeInfo, ValidationConstants.CusCodeData.CodeIsLettersAndNumbersOnly);

			reservedField.CY_Code = "A";
			AssertNoMessageErrorContaining(reservedField.CY_CodeInfo, ValidationConstants.CusCodeData.CodeIsLettersAndNumbersOnly);
			reservedField.CY_Code = "1";
			AssertNoMessageErrorContaining(reservedField.CY_CodeInfo, ValidationConstants.CusCodeData.CodeIsLettersAndNumbersOnly);
		}
	}
}
