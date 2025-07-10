using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESLocationOfGoodsProviderTest : DataProviderTestCase<AESLocationOfGoodsProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null GoodsLocation", "Value cannot be null.\r\nParameter name: cusGoodsLocation",
				() => new AESLocationOfGoodsProvider(null));
		});
	}

	public void TestTypeOfLocation()
	{
		entryInstruction.GoodsLocation.CGL_Type = "2";
		AssertEquals("2", Provider.TypeOfLocation);
	}

	public void TestQualifierOfIdentification()
	{
		entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.W;
		AssertEquals(QualifierOfTheIdentificationList.Codes.W, Provider.QualifierOfIdentification);
	}

	public void TestAuthorisationNumber()
	{
		CombineAssertions(() =>
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.Y;
			entryInstruction.GoodsLocation.Address.AuthorisationNumber = "BGS";
			AssertEquals("QualifierOfIdentification is Y", "BGS", GetProvider().AuthorisationNumber);
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.W;
			AssertEquals("QualifierOfIdentification is not Y", null, GetProvider().AuthorisationNumber);
		});
	}

	public void TestAdditionalIdentifier()
	{
		CombineAssertions(() =>
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.X;
			AssertEquals("QualifierOfIdentification is X but GoodsLocation.AdditionalIdentifier is empty", null, GetProvider().AdditionalIdentifier);

			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.Y;
			AssertEquals("QualifierOfIdentification is Y but GoodsLocation.AdditionalIdentifier is empty", null, GetProvider().AdditionalIdentifier);

			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.X;
			entryInstruction.GoodsLocation.AdditionalIdentifier = "SLG";
			AssertEquals("QualifierOfIdentification is X", "SLG", GetProvider().AdditionalIdentifier);

			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.Y;
			entryInstruction.GoodsLocation.AdditionalIdentifier = "SLG";
			AssertEquals("QualifierOfIdentification is Y", "SLG", GetProvider().AdditionalIdentifier);

			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.W;
			AssertEquals("QualifierOfIdentification is other than X,Y", null, GetProvider().AdditionalIdentifier);
		});
	}

	public void TestUNLocode()
	{
		CombineAssertions(() =>
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.U;
			entryInstruction.GoodsLocation.Unlocode = "LOG";
			AssertEquals("QualifierOfIdentification is U", "LOG", GetProvider().UNLocode);
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.W;
			AssertEquals("QualifierOfIdentification is not U", null, GetProvider().UNLocode);
		});
	}

	public void TestCustomsOffice()
	{
		CombineAssertions(() =>
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.V;
			entryInstruction.GoodsLocation.CGL_CustomsOffice = "LOG";
			AssertEquals("QualifierOfIdentification is V", "LOG", GetProvider().CustomsOffice.ReferenceNumber);
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.W;
			AssertEquals("QualifierOfIdentification is not V", null, GetProvider().CustomsOffice);
		});
	}

	public void TestGNSS()
	{
		var unloco = Factory.New<RefUNLOCO>();
		unloco.RL_GeoLocation = new ZGeography("20, 10");
		CombineAssertions(() =>
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.W;
			entryInstruction.GoodsLocation.Address.E2_GeoLocation = unloco.RL_GeoLocation;
			AssertNotNull("QualifierOfIdentification is W", GetProvider().GNSS);
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.V;
			AssertNull("QualifierOfIdentification is not W", GetProvider().GNSS);
		});
	}

	public void TestEconomicOperator()
	{
		CombineAssertions(() =>
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.X;
			AssertNotNull("QualifierOfIdentification is X", GetProvider().EconomicOperator);
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.V;
			AssertNull("QualifierOfIdentification is not X", GetProvider().EconomicOperator);
		});
	}

	public void TestAddress()
	{
		CombineAssertions(() =>
		{
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.Z;
			var goodsLocationAddress = entryInstruction.GoodsLocation.Address;
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.OA_PostCode = "11-200";
			goodsLocationAddress.E2_OA_Address = address.PK;
			AssertEquals("Address not overriden", "11-200", GetProvider().Address.PostCode);
			goodsLocationAddress.E2_AddressOverride = true;
			goodsLocationAddress.E2_Postcode = "20-200";
			AssertEquals("Address overriden", "20-200", GetProvider().Address.PostCode);
			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.W;
			AssertNull("QualifierOfIdentification is not Z", GetProvider().Address);
		});
	}

	public void TestContactPerson()
	{
		CombineAssertions(() =>
		{
			var address = entryInstruction.GoodsLocation.Address;
			address.E2_Contact = "AgentName";
			address.E2_Phone = "12345";
			address.E2_Email = "agent@name.com";

			AssertEquals("Mapped from cus agent", "AgentName", GetProvider().ContactPerson.Name);

			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.V;
			address.E2_Contact = "AgentName";
			address.E2_Phone = "12345";
			AssertNull("'V' CGL_Qualifier", GetProvider().ContactPerson);

			entryInstruction.GoodsLocation.CGL_Qualifier = QualifierOfTheIdentificationList.Codes.W;
			address.E2_Contact = "AgentName";
			address.E2_Phone = "12345";
			AssertNotNull("Not 'V' CGL_Qualifier", GetProvider().ContactPerson);

			address.E2_Contact  = string.Empty;
			AssertNull("Null if name is empty", GetProvider().ContactPerson);

			address.E2_Contact = "AgentName";
			address.E2_Phone = string.Empty;
			AssertNull("Null if phone is empty", GetProvider().ContactPerson);
		});
	}

	public void TestPostcodeAddress() => AssertNull(Provider.PostcodeAddress);

	protected override AESLocationOfGoodsProvider GetProvider() => new AESLocationOfGoodsProvider(entryInstruction.GoodsLocation);

	protected override void SetUp()
	{
		base.SetUp();
		entryInstruction = Factory.New<CusEntryInstruction>();
	}

	CusEntryInstruction entryInstruction;
}
