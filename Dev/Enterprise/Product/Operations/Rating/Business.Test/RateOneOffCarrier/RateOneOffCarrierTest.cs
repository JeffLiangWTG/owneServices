using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test;

[TestedType(typeof(RateOneOffCarrier))]
public class RateOneOffCarrierTest : EnterpriseBusinessObjectTestCase
{
	public void TestDefaultingCreditor_DefaultsToSPCRelatedParty()
	{
		//Arrange
		var oneOffShipment = Factory.New<RateOneOffShipment>();
		oneOffShipment.TT_TransportMode = Constants.TransportModes.Sea;
		oneOffShipment.TT_ContainerMode = Constants.ContainerModes.FCL;
		oneOffShipment.TT_RL_NKReceivalLocation = "AUSYD";
		oneOffShipment.TT_RL_NKDeliveryLocation = "AUSYD";

		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		carrier.OH_IsCreditor = true;

		var org1 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FCL, companyLevel: CompanyLevelList.Codes.COM);
		var org2 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FTL, companyLevel: CompanyLevelList.Codes.COM);
		var org3 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FCL, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Act
		var possibleCarrier = oneOffShipment.PossibleCarriers.AddNew();
		possibleCarrier.TTC_OH_Carrier = carrier.PK;

		//Assert
		AssertEquals(org1.PK, possibleCarrier.TTC_OH_Creditor);
	}

	public void TestDefaultingCreditor_DoesNotDefault_WhenCreditorIsNotEmpty()
	{
		//Arrange
		var oneOffShipment = Factory.New<RateOneOffShipment>();
		oneOffShipment.TT_TransportMode = Constants.TransportModes.Sea;
		oneOffShipment.TT_ContainerMode = Constants.ContainerModes.FCL;
		oneOffShipment.TT_RL_NKReceivalLocation = "AUSYD";
		oneOffShipment.TT_RL_NKDeliveryLocation = "AUSYD";

		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		var creditor = Factory.NewWithValidTestData<OrgHeader>();
		creditor.OH_IsCreditor = true;

		var org1 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FCL, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Act
		var possibleCarrier = oneOffShipment.PossibleCarriers.AddNew();
		possibleCarrier.TTC_OH_Carrier = carrier.PK;

		//Assert
		AssertEquals(org1.PK, possibleCarrier.TTC_OH_Creditor);

		//Act
		possibleCarrier.TTC_OH_Creditor = creditor.PK;
		possibleCarrier.TTC_OH_Carrier = carrier.PK;

		//Assert
		AssertEquals(creditor.PK, possibleCarrier.TTC_OH_Creditor);
	}

	public void TestDefaultingCreditor_DefaultsToCarrier_WhenNoSuitableMatchFound_AndCarrierIsCreditor()
	{
		//Arrange
		var oneOffShipment = Factory.New<RateOneOffShipment>();
		oneOffShipment.TT_TransportMode = Constants.TransportModes.Road;
		oneOffShipment.TT_ContainerMode = Constants.ContainerModes.FCL;

		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		carrier.OH_IsCreditor = true;

		var org1 = CreateOrgSPCRelatedParty(Factory, carrier, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, "AU", containerMode: Constants.ContainerModes.FCL, companyLevel: CompanyLevelList.Codes.COM);

		Factory.Save();

		//Act
		var possibleCarrier = oneOffShipment.PossibleCarriers.AddNew();
		possibleCarrier.TTC_OH_Carrier = carrier.PK;

		//Assert
		AssertEquals(carrier.PK, possibleCarrier.TTC_OH_Creditor);
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
		orgRelated.CompanyDataCollection.Load(new(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK));
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

	protected override BusinessObject GetNewBusinessObject()
	{
		var quote = Factory.New<Quote>();
		var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
		carrierOrg.OH_IsShippingProvider = true;
		var oneOffShipment = quote.OneOffQuote.AddNew();
		var possibleCarrier = oneOffShipment.PossibleCarriers.AddNew();
		possibleCarrier.TTC_OH_Carrier = carrierOrg.PK;
		return possibleCarrier;
	}

	#endregion
}
