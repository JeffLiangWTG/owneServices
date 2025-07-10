using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class LocationOfGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null CusGoodsLocation", "Value cannot be null.\r\nParameter name: cusGoodsLocation", () => new LocationOfGoodsProvider(null));
	}

	public void TestTypeOfLocation() => AssertEquals("A", Provider.TypeOfLocation);

	public void TestQualifierOfIdentification() => AssertEquals("B", Provider.QualifierOfIdentification);

	public void TestAuthorisationNumber()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			cusGoodsLocation.Address.AuthorisationNumber = "test";
			AssertEquals("AuthorizationNumber Qualifier", "test", GetProvider().AuthorisationNumber);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			cusGoodsLocation.Address.AuthorisationNumber = "test";
			AssertEquals("Not AuthorizationNumber Qualifier", string.Empty, GetProvider().AuthorisationNumber);
		});
	}

	public void TestAdditionalIdentifier()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			cusGoodsLocation.CGL_AdditionalIdentifier = "Something";
			AssertEquals("AuthorizationNumber Qualifier", "Something", GetProvider().AdditionalIdentifier);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			cusGoodsLocation.CGL_AdditionalIdentifier = "Something";
			AssertEquals("EoriNumber Qualifier", "Something", GetProvider().AdditionalIdentifier);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			cusGoodsLocation.CGL_AdditionalIdentifier = "Something";
			AssertEquals("Other Qualifier", string.Empty, GetProvider().AdditionalIdentifier);
		});
	}

	public void TestUNLocode()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			cusGoodsLocation.CGL_AdditionalIdentifier = "Something";
			AssertEquals("UnLocode Qualifier", "Something", GetProvider().UNLocode);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			cusGoodsLocation.CGL_AdditionalIdentifier = "Something";
			AssertEquals("Not UnLocode Qualifier", string.Empty, GetProvider().UNLocode);
		});
	}

	public void TestCustomsOffice()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			cusGoodsLocation.CGL_CustomsOffice = "Something";
			AssertEquals("CustomsOfficeIdentifier Qualifier", "Something", GetProvider().CustomsOffice);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			cusGoodsLocation.CGL_CustomsOffice = "Something";
			AssertEquals("Not CustomsOfficeIdentifier Qualifier", string.Empty, GetProvider().CustomsOffice);
		});
	}

	public void TestGNSS()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			AssertNotNull("GnssCoordinates Qualifier", GetProvider().GNSS);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertNull("Not GnssCoordinates Qualifier", GetProvider().GNSS);
		});
	}

	public void TestEconomicOperator()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			cusGoodsLocation.Address.E2_GovRegNum = "ABC";
			AssertEquals("EoriNumber Qualifier with OrgHeader", "ABC", GetProvider().EconomicOperator);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			AssertEquals("Not EoriNumber Qualifier", string.Empty, GetProvider().EconomicOperator);
		});
	}

	public void TestAddress()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertNotNull("Address Qualifier", GetProvider().Address);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertNull("Not Address Qualifier", GetProvider().Address);
		});
	}

	public void TestPostcodeAddress()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
			AssertNotNull("PostcodeAddress Qualifier", GetProvider().PostcodeAddress);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertNull("Not PostcodeAddress Qualifier", GetProvider().PostcodeAddress);
		});
	}

	public void TestContactPerson()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty E2_Contact", GetProvider().ContactPerson);

			cusGoodsLocation.Address.E2_Contact = "ABC";
			AssertNull("Empty E2_Phone", GetProvider().ContactPerson);

			cusGoodsLocation.Address.E2_Phone = "CDE";
			AssertNotNull("Not empty E2_Phone and E2_Contact", GetProvider().ContactPerson);

			cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			AssertNull("No contact person in this qualifier", GetProvider().ContactPerson);
		});
	}

	protected override LocationOfGoodsProvider GetProvider() => new LocationOfGoodsProvider(cusGoodsLocation);

	protected override void SetUp()
	{
		base.SetUp();
		cusGoodsLocation = Factory.New<CusGoodsLocation>();
		cusGoodsLocation.CGL_Type = "A";
		cusGoodsLocation.CGL_Qualifier = "B";
		cusGoodsLocation.CGL_AdditionalIdentifier = "Something";
	}
	CusGoodsLocation cusGoodsLocation;
}
