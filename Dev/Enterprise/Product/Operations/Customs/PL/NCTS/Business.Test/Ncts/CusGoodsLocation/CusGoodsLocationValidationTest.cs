using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCGL_Qualifier() => CombineAssertions(()
		=> AssertCGL_QualifierOrCheckCGL_TypeEmpty(cusGoodsLocation.CGL_QualifierInfo, "Qualifier code is required for Location of Goods.",
			(qualifier, type) => qualifier.IsEmpty && !type.IsEmpty));

	public void TestCheckCheckCGL_Type() => CombineAssertions(()
		=> AssertCGL_QualifierOrCheckCGL_TypeEmpty(cusGoodsLocation.CGL_TypeInfo, "Type code is required for Location of Goods.",
			(qualifier, type) => type.IsEmpty && !qualifier.IsEmpty));

	void AssertCGL_QualifierOrCheckCGL_TypeEmpty(ZPropertyInfo propertyInfo, string expectedErrorMessage, Func<ZString, ZString, bool> mustFailGetFunc)
	{
		cusGoodsLocation.CGL_Qualifier = ZString.Empty;
		cusGoodsLocation.CGL_Type = ZString.Empty;
		AssertErrorMessage();

		cusGoodsLocation.CGL_Qualifier = "T";
		cusGoodsLocation.Validation.ValidateCGL_Type();
		AssertErrorMessage();

		cusGoodsLocation.CGL_Type = "A";
		cusGoodsLocation.Validation.ValidateCGL_Qualifier();
		AssertErrorMessage();

		cusGoodsLocation.CGL_Qualifier = ZString.Empty;
		cusGoodsLocation.Validation.ValidateCGL_Type();
		AssertErrorMessage();

		void AssertErrorMessage()
		{
			var message = new StringBuilder()
				.Append(cusGoodsLocation.CGL_Qualifier.IsEmpty ? "CGL_Qualifier empty, " : "CGL_Qualifier not empty, ")
				.Append(cusGoodsLocation.CGL_Type.IsEmpty ? "CGL_Type empty" : "CGL_Type not empty")
				.ToString();
			if (mustFailGetFunc(cusGoodsLocation.CGL_Qualifier, cusGoodsLocation.CGL_Type))
			{
				AssertHasMessageError(message, propertyInfo, expectedErrorMessage);
			}
			else
			{
				AssertNoMessageError(message, propertyInfo, expectedErrorMessage);
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		cusGoodsLocation = nctsHeader.MovementHeader.GoodsLocation;
	}

	NctsHeader nctsHeader;
	CusGoodsLocation cusGoodsLocation;
}
