using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.PortHubs.Business
{
	public class PortHubZonePivotLookups : AutoPortHubZonePivotLookups
	{
		public PortHubZonePivotLookups(AutoPortHubZonePivot parent)
			: base(parent)
		{
		}

		public OrgHeaderCollection RelatedParties
		{
			get { return Factory.GetCachedValue("PortHubZonePivot.RelatedParties_Generic", delegate { return new OrgHeaderCollection(Factory); }); }
		}

		public RateTransportZonesCollection TransportZones
		{
			get { return new RateTransportZonesCollection(Factory); }
		}

		public override OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get
			{
				OrgCarrierServiceLevelCollection carrierServiceLevels;

				if (TransportZoneOwner != null && TransportZoneOwner.OH_IsShippingProvider)
				{
					carrierServiceLevels = new OrgCarrierServiceLevelCollection(TransportZoneOwner.MiscServ);
				}
				else
				{
					carrierServiceLevels = new OrgCarrierServiceLevelCollection(Factory, false, false);
				}

				carrierServiceLevels.Load();

				return carrierServiceLevels;
			}
		}

		public OrgCarrierAccountCollection CarrierAccounts
		{
			get
			{
				if (TransportZoneOwner != null)
				{
					if (TransportZoneOwner.OH_IsShippingProvider)
					{
						return TransportZoneOwner.CarrierAccounts;
					}
					return new OrgCarrierAccountCollection(TransportZoneOwner);
				}
				var orgCarrierAccountCollection = new OrgCarrierAccountCollection(Factory);
				orgCarrierAccountCollection.DeleteAll();
				return orgCarrierAccountCollection;
			}
		}

		public OrgHeader TransportZoneOwner
		{
			get
			{
				if (transportZoneOwner == null)
				{
					transportZoneOwner = (((PortHubZonePivot)Parent).TransportZone != null
						&& ((PortHubZonePivot)Parent).TransportZone.TransportProvider != null
						&& !((PortHubZonePivot)Parent).TransportZone.TransportProvider.TP_OH_RelatedParty.IsEmpty)
							? Factory.Load<OrgHeader>(((PortHubZonePivot)Parent).TransportZone.TransportProvider.TP_OH_RelatedParty)
							: null;
				}

				return transportZoneOwner;
			}
		}

		OrgHeader transportZoneOwner;
	}
}
