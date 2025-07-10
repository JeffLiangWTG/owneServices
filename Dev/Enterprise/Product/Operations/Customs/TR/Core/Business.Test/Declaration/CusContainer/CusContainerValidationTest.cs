using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing;

class CusContainerValidationTest : EU.Business.Declaration.Testing.CusContainerValidationTest<JobDeclaration>
{
	public void TestCheckCO_RN_NKOwnerCountry()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusContainer.CO_RN_NKOwnerCountryInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusContainer = Factory.New<CusContainer>();
	}

	CusContainer cusContainer;
}
