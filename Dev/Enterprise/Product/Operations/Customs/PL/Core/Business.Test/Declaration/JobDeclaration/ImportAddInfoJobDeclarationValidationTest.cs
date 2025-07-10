using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportAddInfoJobDeclarationValidationTest : TestCaseWithFactory
{
	public void TestCheckZG_ExciseCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			declaration.ZG_ExciseCode = ZString.Empty;
			AssertHasMessageErrorContaining("Empty Payment Method", declaration.ZG_ExciseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_ExciseCode = "1";
			AssertHasMessageErrorContaining("Invalid Payment Method", declaration.ZG_ExciseCodeInfo, ListValidation.InvalidCodeMessageError.ToString());

			declaration.ZG_ExciseCode = PLMethodOfPaymentList.Codes.A;
			AssertNoMessageErrorContaining("Valid Payment Method", declaration.ZG_ExciseCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Valid Payment Method", declaration.ZG_ExciseCodeInfo, ListValidation.InvalidCodeMessageError.ToString());
		});
	}

	public void TestCheckZG_VATDeferType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			declaration.ZG_VATDeferType = ZString.Empty;
			AssertHasMessageErrorContaining("Empty Payment Method", declaration.ZG_VATDeferTypeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_VATDeferType = "1";
			AssertHasMessageErrorContaining("Invalid Payment Method", declaration.ZG_VATDeferTypeInfo, ListValidation.InvalidCodeMessageError.ToString());

			declaration.ZG_VATDeferType = PLMethodOfPaymentList.Codes.A;
			AssertNoMessageErrorContaining("Valid Payment Method", declaration.ZG_VATDeferTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Valid Payment Method", declaration.ZG_VATDeferTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
		});
	}
}
