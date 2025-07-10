using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISPGAValidationTest : TestCaseWithFactory
	{
		public void TestCheckCode()
		{
			var pga = new DISPGA(Factory);
			pga.Code = "";
			AssertHasMessageErrorContaining(pga.CodeInfo, MandatoryValidation.YouHaveNotEntered);
			pga.Code = "~~";
			AssertNoMessageErrorContaining(pga.CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(pga.CodeInfo, ListValidation.InvalidCodeMessageError);
			pga.Code = PGAList.Codes.CBP;
			AssertNoMessageErrorContaining(pga.CodeInfo, ListValidation.InvalidCodeMessageError);
			pga.Code = "NMF";
			AssertNoMessageErrorContaining(pga.CodeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
