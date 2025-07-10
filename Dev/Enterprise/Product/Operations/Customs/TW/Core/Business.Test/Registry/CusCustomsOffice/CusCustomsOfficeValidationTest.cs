using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusCustomsOfficeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			currentElement.Validation.ValidateAll();
			AssertHasErrorContaining(currentElement.CustomsOfficeCodeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateCustomsOfficeCode()
		{
			var targetInfo = currentElement.CustomsOfficeCodeInfo;
			currentElement.Validation.ValidateCustomsOfficeCode();
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			currentElement.CustomsOfficeCode = "CE";
			AssertNoErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			currentElement.CustomsOfficeCode = "?";
			AssertHasErrorContaining(targetInfo, ListValidation.InvalidCodeError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateCustomsOffice();
			currentElement = new CusCustomsOffice(new FallbackLevel(Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		}

		CusCustomsOffice currentElement;
	}
}
