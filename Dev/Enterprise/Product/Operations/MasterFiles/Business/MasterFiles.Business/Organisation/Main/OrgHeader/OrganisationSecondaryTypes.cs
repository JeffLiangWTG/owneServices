using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class OrganisationSecondaryTypes
	{
		public static ZString None { get { return Res.GetString("Organisation|SecondaryOrgType|None", "None"); } }
		public static ZString ARQualityAssured { get { return Res.GetString("Organisation|SecondaryOrgType|ARQualityAssured", "A/R - Quality Assured"); } }
		public static ZString APQualityAssured { get { return Res.GetString("Organisation|SecondaryOrgType|APQualityAssured", "A/P - Quality Assured"); } }
		public static ZString IncludedInAutoRateUpdate { get { return Res.GetString("Organisation|SecondaryOrgType|IncludedInAutoRateUpdate", "A/R - Included in Automatic Rate Updates"); } }
		public static ZString NotIncludedInAutoRateUpdate { get { return Res.GetString("Organisation|SecondaryOrgType|NotIncludedInAutoRateUpdate", "A/R - Not Included in Automatic Rate Updates"); } }
		public static ZString CreditOnHold { get { return Res.GetString("Organisation|SecondaryOrgType|CreditOnHold", "A/R - Credit On Hold"); } }
		public static ZString HandlesAirFreight { get { return Res.GetString("Organisation|SecondaryOrgType|HandlesAirFreight", "Forwarder - Air Freight"); } }
		public static ZString HandlesSeaFreight { get { return Res.GetString("Organisation|SecondaryOrgType|HandlesSeaFreight", "Forwarder - Sea Freight"); } }
		public static ZString HandlesRoadFreight { get { return Res.GetString("Organisation|SecondaryOrgType|HandlesRoadFreight", "Forwarder - Road Freight"); } }
		public static ZString HandlesRailFreight { get { return Res.GetString("Organisation|SecondaryOrgType|HandlesRailFreight", "Forwarder - Rail Freight"); } }
		public static ZString Depot { get { return Res.GetString("Organisation|SecondaryOrgType|Depot", "Services - Depot"); } }
		public static ZString DistributionCentre { get { return Res.GetString("Organisation|SecondaryOrgType|DistributionCentre", "Services - Distribution Center"); } }
		public static ZString PackingDepot { get { return Res.GetString("Organisation|SecondaryOrgType|PackingDepot", "Services - Packing Depot"); } }
		public static ZString UnpackingDepot { get { return Res.GetString("Organisation|SecondaryOrgType|UnpackingDepot", "Services - Unpacking Depot"); } }
		public static ZString CTO { get { return Res.GetString("Organisation|SecondaryOrgType|CTO", "Services - CTO"); } }
		public static ZString AirCTO { get { return Res.GetString("Organisation|SecondaryOrgType|AirCTO", "Services - Air CTO"); } }
		public static ZString SeaCTO { get { return Res.GetString("Organisation|SecondaryOrgType|SeaCTO", "Services - Sea CTO"); } }
		public static ZString RoadDepotTransitShed { get { return Res.GetString("Organisation|SecondaryOrgType|RoadDepotTransitShed", "Services - Road Depot / Transit Shed"); } }
		public static ZString RailHeadDepot { get { return Res.GetString("Organisation|SecondaryOrgType|RailHeadDepot", "Services - Rail Head / Depot"); } }
		public static ZString FerryWaterTerminal { get { return Res.GetString("Organisation|SecondaryOrgType|FerryWaterTerminal", "Services – Ferry/Inland Water Terminal"); } }
		public static ZString ContainerYard { get { return Res.GetString("Organisation|SecondaryOrgType|ContainerYard", "Services - Container Yard"); } }
		public static ZString FumigationContractor { get { return Res.GetString("Organisation|SecondaryOrgType|FumigationContractor", "Services - Fumigation Contractor"); } }
		public static ZString ContainerLeasingCompany { get { return Res.GetString("Organisation|SecondaryOrgType|ContainerLeasingCompany", "Services - Container Leasing Company"); } }
		public static ZString VGMContractor { get { return Res.GetString("Organisation|SecondaryOrgType|VGMContractor", "Services - VGM Contractor"); } }
		public static ZString LocalTransport { get { return Res.GetString("Organisation|SecondaryOrgType|LocalTransport", "Carrier - Road Transport Provider"); } }
		public static ZString ShippingLine { get { return Res.GetString("Organisation|SecondaryOrgType|ShippingLine", "Carrier - Shipping Line"); } }
		public static ZString Airline { get { return Res.GetString("Organisation|SecondaryOrgType|Airline", "Carrier - Airline"); } }
		public static ZString Rail { get { return Res.GetString("Organisation|SecondaryOrgType|Rail", "Carrier - Rail"); } }
		public static ZString InlandWaterway { get { return Res.GetString("Organisation|SecondaryOrgType|InlandWaterway", "Carrier - Inland Waterways"); } }
		public static ZString AirWholesaler { get { return Res.GetString("Organisation|SecondaryOrgType|AirWholesaler", "Carrier - Air Freight Wholesaler"); } }
		public static ZString SeaWholesaler { get { return Res.GetString("Organisation|SecondaryOrgType|NVOCC", "Carrier - NVOCC"); } }
		public static ZString LineHaul { get { return Res.GetString("Organisation|SecondaryOrgType|LineHaul", "Carrier - Line Haul"); } }
		public static ZString Principal { get { return Res.GetString("Organisation|SecondaryOrgType|Principal", "Carrier - Principal"); } }
		public static ZString VesselConsortium { get { return Res.GetString("Organisation|SecondaryOrgType|VesselConsortium", "Carrier - Vessel Consortium"); } }
	}
}
