using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestNameEmptyError()
	{
		const string errorMessage = "Phone number is present. However, Contact Name is missing which is required. Hence, Contact information will be skipped in Customs Edi message.";

		AssertNamePhoneEmpty(() => cusGoodsLocationAddress.E2_Contact = ZString.Empty, cusGoodsLocationAddress.E2_ContactInfo, errorMessage);
	}

	public void TestPhoneEmptyError()
	{
		const string errorMessage = "Contact Name is present. However, Phone number is missing which is required. Hence, Contact information will be skipped in Customs Edi message.";

		AssertNamePhoneEmpty(() => cusGoodsLocationAddress.E2_Phone = ZString.Empty, cusGoodsLocationAddress.E2_PhoneInfo, errorMessage);
	}

	void AssertNamePhoneEmpty(Action createErrorConditions, ZPropertyInfo propertyInfo, string errorMessage)
	{
		cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;

		CombineAssertions(() =>
		{
			cusGoodsLocationAddress.E2_Contact = ZString.Empty;
			cusGoodsLocationAddress.E2_Phone = ZString.Empty;
			AssertNoMessageError("E2_Contact and E2_Phone are empty", propertyInfo, errorMessage);

			cusGoodsLocationAddress.E2_Contact = "Test";
			cusGoodsLocationAddress.E2_Phone = "123";
			AssertNoMessageError("E2_Contact and E2_Phone are not empty", propertyInfo, errorMessage);

			createErrorConditions();
			var conditions = $"E2_Contact is {(cusGoodsLocationAddress.E2_Contact.IsEmpty ? "empty" : "not empty")}, " +
					$"E2_Phone is {(cusGoodsLocationAddress.E2_Phone.IsEmpty ? "empty" : "not empty")}";
			AssertHasMessageError(conditions, propertyInfo, errorMessage);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertNoMessageError(conditions + ", CGL_Qualifier is CustomsOfficeIdentifier", propertyInfo, errorMessage);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNoMessageError(conditions + ", NCTS4", propertyInfo, errorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		cusGoodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		cusGoodsLocationAddress = cusGoodsLocation.Address;
	}

	NctsHeader nctsHeader;
	CusGoodsLocation cusGoodsLocation;
	CusGoodsLocationAddress cusGoodsLocationAddress;
}
