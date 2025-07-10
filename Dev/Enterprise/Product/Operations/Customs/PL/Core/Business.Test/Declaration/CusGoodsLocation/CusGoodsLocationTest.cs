using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidation()
	{
		AssertType<CusGoodsLocationValidation>(cusGoodsLocation.Validation);
	}

	public void TestContactPersonDataVisible() => CombineAssertions(() =>
	{
		Assert(!cusGoodsLocation.ContactPersonDataVisible);

		cusGoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		Assert(!cusGoodsLocation.ContactPersonDataVisible);

		cusGoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		Assert(cusGoodsLocation.ContactPersonDataVisible);
	});

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		cusGoodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
	}

	JobDeclaration declaration;
	CusGoodsLocation cusGoodsLocation;

	protected override BusinessObject GetNewBusinessObject() => cusGoodsLocation;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => cusGoodsLocation;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => cusGoodsLocation;
}
