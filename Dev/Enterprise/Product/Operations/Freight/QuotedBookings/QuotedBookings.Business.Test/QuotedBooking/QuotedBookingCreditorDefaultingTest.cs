using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business.Test;

public class QuotedBookingCreditorDefaultingTest : TestCaseWithFactory
{
	public void TestDefaultingCreditor_DefaultsToSPCRelatedParty()
	{
		//Arrange
		var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
		quotedBooking.TransportMode = Constants.TransportModes.Sea;
		quotedBooking.ContainerMode = Constants.ContainerModes.FCL;
		quotedBooking.Origin = "AUSYD";
		quotedBooking.Destination = "AUSYD";

		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		carrier.OH_IsCreditor = true;

		var org1 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FCL, companyLevel: CompanyLevelList.Codes.COM);
		var org2 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FTL, companyLevel: CompanyLevelList.Codes.COM);
		var org3 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FCL, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Act
		quotedBooking.OH_Carrier = carrier.PK;

		//Assert
		AssertEquals(org1.PK, quotedBooking.Creditor);
	}

	public void TestDefaultingCreditor_DoesNotDefault_WhenCreditorIsNotEmpty()
	{
		//Arrange
		var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
		quotedBooking.TransportMode = Constants.TransportModes.Sea;
		quotedBooking.ContainerMode = Constants.ContainerModes.FCL;
		quotedBooking.Origin = "AUSYD";
		quotedBooking.Destination = "AUSYD";

		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		var creditor = Factory.NewWithValidTestData<OrgHeader>();
		creditor.OH_IsCreditor = true;

		var org1 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FCL, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Act
		quotedBooking.OH_Carrier = carrier.PK;

		//Assert
		AssertEquals(org1.PK, quotedBooking.Creditor);

		//Act
		quotedBooking.Creditor = creditor.PK;
		quotedBooking.OH_Carrier = carrier.PK;

		//Assert
		AssertEquals(creditor.PK, quotedBooking.Creditor);
	}

	public void TestDefaultingCreditor_DefaultsToCarrier_WhenNoSuitableMatchFound_AndCarrierIsCreditor()
	{
		//Arrange
		var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
		quotedBooking.TransportMode = Constants.TransportModes.Truck;
		quotedBooking.ContainerMode = Constants.ContainerModes.FCL;

		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		carrier.OH_IsCreditor = true;

		var org1 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FCL, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Act
		quotedBooking.OH_Carrier = carrier.PK;

		//Assert
		AssertEquals(carrier.PK, quotedBooking.Creditor);
	}

	#region Implementation

	static OrgHeader CreateOrgSPCRelatedParty(BusinessObjectFactory factory, OrgHeader org, string direction, string mode, string location, string containerMode = "", string companyLevel = CompanyLevelList.Codes.COM, bool isCreditor = true)
	{
		var orgRelated = factory.NewWithValidTestData<OrgHeader>();
		var relatedParty = org.AllRelatedParties.AddNew();
		relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
		relatedParty.PR_OH_RelatedParty = orgRelated.PK;
		relatedParty.PR_FreightDirection = direction;
		relatedParty.PR_FreightTransportMode = mode;
		relatedParty.PR_Location = location;
		relatedParty.PR_FreightContainerMode = containerMode;
		relatedParty.CompanyLevel = companyLevel;

		OrgCompanyData orgCompanyData;
		orgRelated.CompanyDataCollection.Load(new (OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK));
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

	#endregion Implementation
}
