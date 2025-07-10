using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class PortHubSelectionTestDataCreator
	{
		public PortHubSelectionTestDataCreator(BusinessObjectFactory inputFactory)
		{
			Argument.NotNull(inputFactory, nameof(inputFactory));

			this.factory = inputFactory;
		}

		readonly BusinessObjectFactory factory;

		public OrgHeader GenerateOrganisation(string headerCode, string headerFullName)
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = headerCode;
			orgHeader.OH_FullName = headerFullName;
			orgHeader.OH_IsShippingProvider = true;

			return orgHeader;
		}

		public OrgAddress GenerateAddress(string addressCode, string headerCode, string headerFullName)
		{
			var orgHeader = GenerateOrganisation(headerCode, headerFullName);
			var orgAddress = factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_Code = addressCode;
			orgAddress.OA_OH = orgHeader.PK;

			return orgAddress;
		}

		public ZGuid CreatePortAndDepotSelectionWithUndgClass(ZGuid depotAddressPK, ZGuid dispatchDepotAddress, string serviceLevel, string direction, string ratingFreightMode, string packType, string undgClass)
		{
			var portHubSelection = factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection.TY_Direction = direction;
			portHubSelection.TY_RS_NKServiceLevel = serviceLevel;
			portHubSelection.TY_RatingFreightMode = ratingFreightMode;
			portHubSelection.TY_OA_DepotAddress = depotAddressPK;
			portHubSelection.TY_F3_NKPackType = packType;
			portHubSelection.TY_UndgClass = undgClass;

			if (!dispatchDepotAddress.IsEmpty)
			{
				portHubSelection.TY_OA_DispatchDepotAddress = dispatchDepotAddress;
			}

			return portHubSelection.PK;
		}

		public ZGuid AddZone(ZGuid portHubSelectionPK, string zoneName, ZGuid carrierPK, string serviceLevel, string countryCode = "UA")
		{
			var provider = factory.NewWithValidTestData<RateTransportProvider>();
			provider.TP_RN_NKCountry = countryCode;
			provider.TP_OH_RelatedParty = carrierPK;

			var zone = factory.NewWithValidTestData<RateTransportZone>();
			zone.TZ_ZoneName = zoneName;
			zone.TZ_TP = provider.PK;

			var portHubZonePivot = factory.New<PortHubZonePivot>();
			portHubZonePivot.TX_TY_Hub = portHubSelectionPK;
			portHubZonePivot.TX_TZ_Zone = zone.PK;
			portHubZonePivot.TX_PL_NKCarrierServiceLevel = serviceLevel;
			portHubZonePivot.TX_CarrierAccountNumber = "777888999";

			return zone.PK;
		}

		public void AddZoneItem(ZGuid zonePK, string city, string state, string country)
		{
			var suburb = factory.NewWithValidTestData<RefCityTown>();
			suburb.R9_InternationalName = city;
			suburb.R9_RW_NKState = state;
			suburb.R9_RN_NKCountry = country;

			var zoneItem = factory.NewWithValidTestData<RateTransportZoneItem>();
			zoneItem.TQ_TZ_DomesticZone = zonePK;
			zoneItem.TQ_R9_CityTown = suburb.PK;
		}

		public static void ClearUpAllZoneItems(BusinessObjectFactory factory)
		{
			factory.Load<RateTransportZoneItem>(new ZQuery()).ForEach(item => { item.Delete(); });
			factory.Save();
		}
	}
}
