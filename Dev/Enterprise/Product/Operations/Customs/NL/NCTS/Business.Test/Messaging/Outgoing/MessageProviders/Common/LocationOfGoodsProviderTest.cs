using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(LocationOfGoodsProvider))]
sealed class LocationOfGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new LocationOfGoodsProvider(null));
	}

	public void TestTypeOfLocation_Arrival()
	{
		arrivalHeader.GoodsLocation.CGL_Type = "A";
		AssertEquals("A", arrivalProvider.TypeOfLocation);
	}

	public void TestTypeOfLocation_Departure()
	{
		departureHeader.GoodsLocation.CGL_Type = "D";
		AssertEquals("D", departureProvider.TypeOfLocation);
	}

	public void TestAuthorisationNumber()
	{
		AssertNullOrEmpty(arrivalProvider.AuthorisationNumber);
	}

	public void TestAdditionalIdentifier()
	{
		AssertNullOrEmpty(arrivalProvider.AdditionalIdentifier);
	}

	public void TestQualifierOfIdentification_Arrival()
	{
		arrivalHeader.GoodsLocation.CGL_Qualifier = "V";
		AssertEquals("V", arrivalProvider.QualifierOfIdentification);
	}

	public void TestQualifierOfIdentification_Departure()
	{
		departureHeader.GoodsLocation.CGL_Qualifier = "U";
		AssertEquals("U", departureProvider.QualifierOfIdentification);
	}

	public void TestUnLocode_Arrival()
	{
		AssertNullOrEmpty("UNLocode should be empty", Provider.UnLocode);
	}

	public void TestUnLocode_Departure()
	{
		var nctsDepartureMovementHeader = nctsDepartureHeader.MovementHeader;
		var customsOfficeOfLocationOfGoods = nctsDepartureMovementHeader.CustomsOffices.AddNew();
		customsOfficeOfLocationOfGoods.CY_Code = "LOC";
		customsOfficeOfLocationOfGoods.CY_Data = "DLocID";
		departureHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
		departureHeader.GoodsLocation.CGL_AdditionalIdentifier = "DLocID";

		AssertEquals("DLocID", departureProvider.UnLocode);
	}

	public void TestContactPerson()
	{
		header.ArrivalMovementHeader.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
		var address = header.ArrivalMovementHeader.GoodsLocation.Address;
		address.E2_Contact = "Name";
		AssertNotNull(arrivalProvider.ContactPerson);
	}

	public void TestContactPerson_QualifierNotU()
	{
		var address = header.ArrivalMovementHeader.GoodsLocation.Address;
		address.E2_Contact = "Name";
		AssertNull(arrivalProvider.ContactPerson);
	}

	public void TestCustomsOfficeReferenceNumber_Arrival()
	{
		arrivalHeader.GoodsLocation.CGL_Type = "A";
		arrivalHeader.GoodsLocation.Address.E2_GovRegNum = "123";
		arrivalHeader.GoodsLocation.CGL_Qualifier = "V";

		var arrivalMovementHeader = header.ArrivalMovementHeader;
		arrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "LocID";
		var customsOfficeOfLocationOfGoods = arrivalMovementHeader.CustomsOffices.AddNew();
		customsOfficeOfLocationOfGoods.CY_Code = "LOC";
		customsOfficeOfLocationOfGoods.CY_Data = "LocID";

		AssertEquals("CustomsOfficeReferenceNumber should be filled", "LocID", arrivalProvider.CustomsOfficeReferenceNumber);
	}

	public void TestCustomsOfficeReferenceNumber_Departure()
	{
		departureHeader.GoodsLocation.CGL_Type = "D";
		departureHeader.GoodsLocation.Address.E2_GovRegNum = "D123";
		departureHeader.GoodsLocation.CGL_Qualifier = "V";
		nctsDepartureHeader.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "DLocID";

		var nctsDepartureMovementHeader = nctsDepartureHeader.MovementHeader;
		var customsOfficeOfLocationOfGoods = nctsDepartureMovementHeader.CustomsOffices.AddNew();
		customsOfficeOfLocationOfGoods.CY_Code = "LOC";
		customsOfficeOfLocationOfGoods.CY_Data = "LocID";

		AssertEquals("CustomsOfficeReferenceNumber should be filled", "DLocID", departureProvider.CustomsOfficeReferenceNumber);
	}

	public void TestEconomicOperatorIdentificationNumber()
	{
		AssertNull(arrivalProvider.EconomicOperatorIdentificationNumber);
	}

	public void TestPostCodeAddress()
	{
		AssertType<PostCodeAddressProvider>(arrivalProvider.PostCodeAddress);
	}

	public void TestGNSSLatitute()
	{
		AssertNullOrEmpty(arrivalProvider.GNSSLatitute);
	}

	public void TestGNSSLongitude()
	{
		AssertNullOrEmpty(arrivalProvider.GNSSLongitude);
	}

	public void TestAddress()
	{
		AssertNull(arrivalProvider.Address);
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

		nctsDepartureHeader = Factory.New<NctsHeader>();
		nctsDepartureHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		arrivalHeader = header.ArrivalMovementHeader;
		departureHeader = nctsDepartureHeader.MovementHeader;

		arrivalProvider = new LocationOfGoodsProvider(header);
		departureProvider = new LocationOfGoodsProvider(nctsDepartureHeader);
	}

	protected override LocationOfGoodsProvider GetProvider() => arrivalProvider;

	NctsHeader header;
	NctsHeader nctsDepartureHeader;
	NctsArrivalMovementHeader arrivalHeader;
	NctsDepartureMovementHeader departureHeader;
	LocationOfGoodsProvider departureProvider;
	LocationOfGoodsProvider arrivalProvider;
}
