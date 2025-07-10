using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.PortHubs.Business
{
	public class PortHubSelectionLookups : AutoPortHubSelectionLookups
	{
		public PortHubSelectionLookups(AutoPortHubSelection parent) : base(parent)
		{
		}

		public CodeDescriptionPairList DirectionList => Factory.GetCachedValue<PortHubSelectionDirectionList>();

		public CodeDescriptionPairList DGClassList
		{
			get
			{
				return Factory.GetCachedValue("PortHubSelectionLookups.DGClassList", delegate
				{
					var list = new CodeDescriptionPairList(UNDGDataItemLookups.GetDGClassList(Factory));
					list.AddPair(PortHubSelection.All, Res.GetString("2c45dd28-c4bd-4b17-a268-171ce1508658", "All"));
					return list;
				});
			}
		}

		public CodeDescriptionPairList FreightModeList => Factory.GetCachedValue<TransportModeList>();

		public CodeDescriptionPairList PackModeList => Factory.GetCachedValue<PortHubSelectionPackModeList>();

		public CodeDescriptionPairList ProcessTypeList => Factory.GetCachedValue<PortHubSelectionProcessTypeList>();

		public CodeDescriptionPairList VolumeUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public virtual DepotCollection Depots => new DepotCollection(Factory);

		public OrganisationsFindBoxCollection ShipperOrgs => new ConsignorCollection(Factory);

		public CarrierCollection CarrierCollection => new CarrierCollection(Factory);

		public RateTransportZonesCollection TransportZonesList => new RateTransportZonesCollection(Factory);

		public virtual RefUNLOCOCollection RefUNLOCO_List => new RefUNLOCOCollection(Factory);
	}
}
