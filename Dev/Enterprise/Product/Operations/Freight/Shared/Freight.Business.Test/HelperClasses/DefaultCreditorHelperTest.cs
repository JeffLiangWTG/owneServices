using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Business.DefaultCreditorHelper;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing;

sealed class DefaultCreditorHelperTest : TestCaseWithFactory
{
	#region RelatedParties

	public void TestSortingRelatedParties()
	{
		var relatedParties = new List<OrgRelatedParty>();

		var org1 = Factory.New<OrgRelatedParty>();
		org1.PR_GC = ZGuid.Empty;
		org1.PR_FreightTransportMode = "ALL";
		relatedParties.Add(org1);

		var org2 = Factory.New<OrgRelatedParty>();
		org2.PR_GC = ZGuid.NewZGuid();
		org2.PR_FreightTransportMode = "ALL";
		relatedParties.Add(org2);

		var sorted = SortRelatedParties(relatedParties).ToList();

		AssertEquals("PR_GC value takes over empty value", org2.PR_GC, sorted[0].PR_GC);

		var org3 = Factory.New<OrgRelatedParty>();
		org3.PR_GC = ZGuid.NewZGuid();
		org3.PR_FreightTransportMode = "ALL";
		org3.PR_Location = "AU";
		relatedParties.Add(org3);

		sorted = SortRelatedParties(relatedParties).ToList();
		AssertEquals("Location value takes over empty value", org3.PR_Location, sorted[0].PR_Location);

		var org4 = Factory.New<OrgRelatedParty>();
		org4.PR_GC = ZGuid.NewZGuid();
		org4.PR_FreightTransportMode = "ALL";
		org4.PR_Location = "AUSYD";
		relatedParties.Add(org4);

		sorted = SortRelatedParties(relatedParties).ToList();
		AssertEquals("Location City takes over country value", org4.PR_Location, sorted[0].PR_Location);

		var org5 = Factory.New<OrgRelatedParty>();
		org5.PR_GC = ZGuid.NewZGuid();
		org5.PR_FreightTransportMode = "ALL";
		org5.PR_FreightContainerMode = "FCL";
		relatedParties.Add(org5);

		sorted = SortRelatedParties(relatedParties).ToList();
		AssertEquals("Location takes over container mode", org4.PR_Location, sorted[0].PR_Location);

		org5.PR_Location = "AUSYD";
		sorted = SortRelatedParties(relatedParties).ToList();
		AssertEquals("Container mode value takes over empty container mode", org5.PR_FreightContainerMode, sorted[0].PR_FreightContainerMode);

		var org6 = Factory.New<OrgRelatedParty>();
		org6.PR_GC = ZGuid.NewZGuid();
		org6.PR_FreightTransportMode = "SEA";
		relatedParties.Add(org6);

		sorted = SortRelatedParties(relatedParties).ToList();
		AssertEquals("Container mode value takes over Transport Mode", org5.PR_FreightContainerMode, sorted[0].PR_FreightContainerMode);

		org6.PR_Location = "AUSYD";
		org6.PR_FreightContainerMode = "FCL";

		sorted = SortRelatedParties(relatedParties).ToList();
		AssertEquals("Transport mode SEA takes over Transport Mode ALL", org6.PR_FreightTransportMode, sorted[0].PR_FreightTransportMode);

		org6.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;

		var org7 = Factory.New<OrgRelatedParty>();
		org7.PR_GC = ZGuid.NewZGuid();
		org7.PR_FreightContainerMode = "FCL";
		org7.PR_FreightTransportMode = "SEA";
		org7.PR_Location = "AUSYD";
		org7.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
		relatedParties.Add(org7);

		sorted = SortRelatedParties(relatedParties).ToList();
		AssertEquals("Direction Delivery takes over Pickup And Delivery", org7.PR_FreightDirection, sorted[0].PR_FreightDirection);

		var org8 = Factory.New<OrgRelatedParty>();
		org8.PR_GC = ZGuid.NewZGuid();
		org8.PR_FreightContainerMode = "FCL";
		org8.PR_FreightTransportMode = "SEA";
		org8.PR_Location = "AUSYD";
		org8.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
		relatedParties.Add(org8);

		sorted = SortRelatedParties(relatedParties).ToList();
		AssertEquals("Direction Pickup takes over Delivery", org8.PR_FreightDirection, sorted[0].PR_FreightDirection);
	}

	public void TestGetRelatedParties()
	{
		//Arrange
		var creditor = Factory.NewWithValidTestData<OrgHeader>();
		var creditorAddress = creditor.Addresses[0];
		var filter = new OrgRelatedPartyFilter()
		{
			CreditorType = CreditorType.DomesticConsol,
			OrgAddress = creditorAddress.PK,
			TransportMode = Constants.TransportModes.Sea,
			ContainerMode = Constants.ContainerModes.Bulk,
			UNLOCO = "Test"
		};
		var pickupRelatedParty = CreateOrgRelatedParty(Factory, creditor, filter, RelatedPartyDirectionList.Codes.Pickup);
		var deliveryRelatedParty = CreateOrgRelatedParty(Factory, creditor, filter, RelatedPartyDirectionList.Codes.Delivery);
		var padRelatedParty = CreateOrgRelatedParty(Factory, creditor, filter, RelatedPartyDirectionList.Codes.PickupAndDelivery);
		var padNonSPCRelatedParty = CreateOrgRelatedParty(Factory, creditor, filter, RelatedPartyDirectionList.Codes.PickupAndDelivery, true, RelatedPartyTypeList.Codes.ClientCFS);
		Factory.Save();

		//Act
		var actualOrgRelatedParties = GetRelatedParties(filter, Factory).Select(org => org.PR_OH_RelatedParty);
		//Assert
		Assert("Domestic consol should have 3 related parties qualified for default creditor", actualOrgRelatedParties.Count() == 3);
		AssertCollectionContains("Pickup should be included for Domestic consol", pickupRelatedParty.PK, actualOrgRelatedParties);
		AssertCollectionContains("Delivery should be included for Domestic consol", deliveryRelatedParty.PK, actualOrgRelatedParties);
		AssertCollectionContains("Pickup & Delivery should be included for Domestic consol", padRelatedParty.PK, actualOrgRelatedParties);

		//Arrange
		filter.CreditorType = CreditorType.ExportConsol;
		//Act
		actualOrgRelatedParties = GetRelatedParties(filter, Factory).Select(org => org.PR_OH_RelatedParty);
		Assert("Export consol should have 2 related parties qualified for default creditor", actualOrgRelatedParties.Count() == 2);
		AssertCollectionNotContains("Delivery should not be included for Export consol", deliveryRelatedParty.PK, actualOrgRelatedParties);
		AssertCollectionContains("Pickup should be included for Export consol", pickupRelatedParty.PK, actualOrgRelatedParties);
		AssertCollectionContains("Pickup & Delivery should be included for Export consol", padRelatedParty.PK, actualOrgRelatedParties);

		//Arrange
		filter.CreditorType = CreditorType.ImportConsol;
		//Act
		actualOrgRelatedParties = GetRelatedParties(filter, Factory).Select(org => org.PR_OH_RelatedParty);
		Assert("Import consol should have 2 related parties qualified for default creditor", actualOrgRelatedParties.Count() == 2);
		AssertCollectionNotContains("Pickup should not be included for Import consol", pickupRelatedParty.PK, actualOrgRelatedParties);
		AssertCollectionContains("Delivery should be included for Import consol", deliveryRelatedParty.PK, actualOrgRelatedParties);
		AssertCollectionContains("Pickup & Delivery should be included for Import consol", padRelatedParty.PK, actualOrgRelatedParties);

		//Arrange
		filter.CreditorType = CreditorType.DomesticConsol;
		pickupRelatedParty.CompanyDataCollection[0].OB_IsCreditor = false;
		Factory.Save();
		//Act
		actualOrgRelatedParties = GetRelatedParties(filter, Factory).Select(org => org.PR_OH_RelatedParty);
		Assert("Domestic consol should have 2 related parties qualified for default creditor", actualOrgRelatedParties.Count() == 2);
		AssertCollectionNotContains("Related party needs to be payable", pickupRelatedParty.PK, actualOrgRelatedParties);
	}

	public void TestGetCreditorOrgHeaderFromOrgRelatedParties_ReturnsMostSpecificRelatedParty()
	{
		//Arrange
		var creditor = Factory.NewWithValidTestData<OrgHeader>();
		var filter = new OrgRelatedPartyFilter()
		{
			CreditorType = CreditorType.DomesticConsol,
			TransportMode = Constants.TransportModes.Sea,
		};

		//Arrange
		var relatedParty1 = CreateOrgRelatedParty(Factory, creditor, new ()
		{
			CreditorType = CreditorType.DomesticConsol,
			TransportMode = Constants.TransportModes.All,
			UNLOCO = "AU"
		}, RelatedPartyDirectionList.Codes.PickupAndDelivery);
		Factory.Save();
		//Assert
		var result = GetCreditorOrgHeaderFromOrgRelatedParties(creditor, filter, Factory);
		AssertEquals("Returns matching related party", relatedParty1.PK, result.PK);

		//Arrange
		var relatedParty2 = CreateOrgRelatedParty(Factory, creditor, new ()
		{
			CreditorType = CreditorType.DomesticConsol,
			TransportMode = Constants.TransportModes.All,
			UNLOCO = "AU"
		}, RelatedPartyDirectionList.Codes.Pickup);
		Factory.Save();
		//Assert
		result = GetCreditorOrgHeaderFromOrgRelatedParties(creditor, filter, Factory);
		AssertEquals("Returns party with more specific direction", relatedParty2.PK, result.PK);

		//Arrange
		var relatedParty3 = CreateOrgRelatedParty(Factory, creditor, new ()
		{
			CreditorType = CreditorType.DomesticConsol,
			TransportMode = Constants.TransportModes.Sea,
			UNLOCO = "AU"
		}, RelatedPartyDirectionList.Codes.PickupAndDelivery);
		Factory.Save();
		//Assert
		result = GetCreditorOrgHeaderFromOrgRelatedParties(creditor, filter, Factory);
		AssertEquals("Returns party with more specific transport mode", relatedParty3.PK, result.PK);

		//Arrange
		var relatedParty4 = CreateOrgRelatedParty(Factory, creditor, new ()
		{
			CreditorType = CreditorType.DomesticConsol,
			TransportMode = Constants.TransportModes.All,
			UNLOCO = "AUSYD"
		}, RelatedPartyDirectionList.Codes.PickupAndDelivery);
		Factory.Save();
		//Assert
		result = GetCreditorOrgHeaderFromOrgRelatedParties(creditor, filter, Factory);
		AssertEquals("Returns party with more specific location", relatedParty4.PK, result.PK);

		//Arrange
		var relatedParty5 = CreateOrgRelatedParty(Factory, creditor, new ()
		{
			CreditorType = CreditorType.DomesticConsol,
			TransportMode = Constants.TransportModes.All,
			UNLOCO = "AU"
		}, RelatedPartyDirectionList.Codes.PickupAndDelivery, companyLevel: "COM");
		Factory.Save();
		//Assert
		result = GetCreditorOrgHeaderFromOrgRelatedParties(creditor, filter, Factory);
		AssertEquals("Returns party with COM company level rather than ENT", relatedParty5.PK, result.PK);
	}

	public void TestGetCreditorOrgHeaderFromOrgRelatedParties_ReturnsNull_WhenNoMatchingParty()
	{
		//Arrange
		var creditor = Factory.NewWithValidTestData<OrgHeader>();
		var filter = new OrgRelatedPartyFilter()
		{
			CreditorType = CreditorType.DomesticConsol,
			TransportMode = Constants.TransportModes.Sea,
		};

		// Assert
		AssertNull(GetCreditorOrgHeaderFromOrgRelatedParties(creditor, filter, Factory));
	}

	#endregion

	#region DefaultCreditor

	public void TestDefaultingCreditor_DefaultsToSPCRelatedParty()
	{
		//Arrange
		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		carrier.OH_IsCreditor = true;

		var org1 = CreateOrgRelatedParty(Factory, carrier, new () { ContainerMode = Constants.ContainerModes.FCL, TransportMode = Constants.TransportModes.Sea, UNLOCO = "AU" }, RelatedPartyDirectionList.Codes.Pickup, companyLevel: CompanyLevelList.Codes.COM);
		var org2 = CreateOrgRelatedParty(Factory, carrier, new() { ContainerMode = Constants.ContainerModes.FTL, TransportMode = Constants.TransportModes.Sea, UNLOCO = "AU" }, RelatedPartyDirectionList.Codes.Pickup, companyLevel: CompanyLevelList.Codes.COM);
		var org3 = CreateOrgRelatedParty(Factory, carrier, new  () { ContainerMode = Constants.ContainerModes.FCL, TransportMode = Constants.TransportModes.Sea, UNLOCO =   "AU" }, RelatedPartyDirectionList.Codes.Delivery, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Assert
		AssertEquals(
			org1.PK,
			GetDefaultCreditor(
				new ImportExportForTest() { JobDirection = Directions.Domestic },
				carrier,
				Constants.TransportModes.Sea,
				Constants.ContainerModes.FCL,
				"AUSYD",
				"AUSYD",
				Factory));
	}

	public void TestDefaultingCreditor_DefaultsToMostSpecificSPCRelatedParty()
	{
		//Arrange
		var carrier = Factory.NewWithValidTestData<OrgHeader>();

		var org1 = CreateOrgRelatedParty(Factory, carrier, new  () { ContainerMode = Constants.ContainerModes.FCL, TransportMode = Constants.TransportModes.Sea, UNLOCO =   "AU" }, RelatedPartyDirectionList.Codes.Pickup, companyLevel: CompanyLevelList.Codes.COM);
		var org2 = CreateOrgRelatedParty(Factory, carrier, new  () { ContainerMode = Constants.ContainerModes.FCL, TransportMode = Constants.TransportModes.Sea, UNLOCO =   "AU" }, RelatedPartyDirectionList.Codes.PickupAndDelivery, companyLevel: CompanyLevelList.Codes.COM);
		var org3 = CreateOrgRelatedParty(Factory, carrier, new  () { ContainerMode = Constants.ContainerModes.FTL, TransportMode = Constants.TransportModes.All, UNLOCO =   "AU" }, RelatedPartyDirectionList.Codes.Pickup, companyLevel: CompanyLevelList.Codes.COM);
		var org4 = CreateOrgRelatedParty(Factory, carrier, new () { ContainerMode = Constants.ContainerModes.FCL, TransportMode = Constants.TransportModes.Sea, UNLOCO = "" }, RelatedPartyDirectionList.Codes.Pickup, companyLevel: CompanyLevelList.Codes.COM);
		var org5 = CreateOrgRelatedParty(Factory, carrier, new  () { ContainerMode = Constants.ContainerModes.FCL, TransportMode = Constants.TransportModes.Sea, UNLOCO =   "AU" }, RelatedPartyDirectionList.Codes.Pickup, companyLevel: CompanyLevelList.Codes.ENT);
		var org6 = CreateOrgRelatedParty(Factory, carrier, new  () { ContainerMode = Constants.ContainerModes.All, TransportMode = Constants.TransportModes.Sea, UNLOCO =   "AU" }, RelatedPartyDirectionList.Codes.Pickup, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Assert
		AssertEquals(
			org1.PK,
			GetDefaultCreditor(
				new ImportExportForTest() { JobDirection = Directions.Domestic },
				carrier,
				Constants.TransportModes.Sea,
				Constants.ContainerModes.FCL,
				"AUSYD",
				"AUSYD",
				Factory));
	}

	public void TestDefaultingCreditor_DoesNotDefault_WhenNoSuitableMatchFound_AndCarrierIsNotCreditor()
	{
		//Arrange
		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		carrier.OH_IsCreditor = false;

		var org1 = CreateOrgRelatedParty(Factory, carrier, new  () { ContainerMode = Constants.ContainerModes.FCL, TransportMode = Constants.TransportModes.Sea, UNLOCO =   "AU" }, RelatedPartyDirectionList.Codes.Delivery, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Assert
		AssertEquals(
			ZGuid.Empty,
			GetDefaultCreditor(
				new ImportExportForTest() { JobDirection = Directions.Domestic },
				carrier,
				Constants.TransportModes.Truck,
				Constants.ContainerModes.FCL,
				"",
				"",
				Factory));
	}

	public void TestDefaultingCreditor_DefaultsToCarrier_WhenNoSuitableMatchFound_AndCarrierIsCreditor()
	{
		//Arrange
		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		carrier.OH_IsCreditor = true;

		var org1 = CreateOrgRelatedParty(Factory, carrier, new  () { ContainerMode = Constants.ContainerModes.FCL, TransportMode = Constants.TransportModes.Sea, UNLOCO =   "AU" }, RelatedPartyDirectionList.Codes.Delivery, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Assert
		AssertEquals(
			carrier.PK,
			DefaultCreditorHelper.GetDefaultCreditor(
				new ImportExportForTest() { JobDirection = Directions.Domestic },
				carrier,
				Constants.TransportModes.Truck,
				Constants.ContainerModes.FCL,
				"",
				"",
				Factory));
	}

	#endregion

	#region Implementation

	static OrgHeader CreateOrgRelatedParty(BusinessObjectFactory factory, OrgHeader org, OrgRelatedPartyFilter filter, string direction, bool isCreditor = true, string partyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor, string companyLevel = "ENT")
	{
		var orgRelated = factory.NewWithValidTestData<OrgHeader>();
		var relatedParty = org.AllRelatedParties.AddNew();
		relatedParty.PR_PartyType = partyType;
		relatedParty.PR_OH_RelatedParty = orgRelated.PK;
		relatedParty.PR_FreightDirection = direction;
		relatedParty.PR_FreightTransportMode = filter.TransportMode;
		relatedParty.PR_Location = filter.UNLOCO;
		relatedParty.PR_FreightContainerMode = filter.ContainerMode;
		relatedParty.CompanyLevel = companyLevel;

		OrgCompanyData orgCompanyData;
		orgRelated.CompanyDataCollection.Load(new ZQuery(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK));
		if (orgRelated.CompanyDataCollection.Count == 0)
		{
			orgCompanyData = factory.New<OrgCompanyData>();
			orgCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			orgCompanyData.OB_OH = orgRelated.PK;
			orgRelated.CompanyDataCollection.Add(orgCompanyData);
		}
		orgCompanyData = orgRelated.CompanyDataCollection[0];
		orgCompanyData.OB_IsCreditor = isCreditor;

		return orgRelated;
	}

	internal class ImportExportForTest : IImportExport
	{
		public Directions JobDirection { get; set; } = Directions.Unknown;
	}

	#endregion
}
