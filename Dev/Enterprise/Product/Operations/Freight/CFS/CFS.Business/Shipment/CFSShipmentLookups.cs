using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentLookups : BaseJobShipmentLookups
	{
		public CFSShipmentLookups(CFSShipment parent)
			: base(parent)
		{
		}

		public new CFSShipment Shipment
		{
			get { return Parent as CFSShipment; }
		}

		public new CFSLoadListConsolCollection Consols_List
		{
			get { return (CFSLoadListConsolCollection)base.Consols_List; }
		}

		public override CodeDescriptionPairList JS_ShipmentType_List
		{
			get
			{
				if (fJS_ShipmentType_List == null)
				{
					fJS_ShipmentType_List = new CodeDescriptionPairList();
					fJS_ShipmentType_List.AddPair(Core.Constants.ShipmentTypes.StandardHouse,
						Core.Constants.ShipmentTypeDescriptions.StandardHouse);
					fJS_ShipmentType_List.AddPair(Core.Constants.ShipmentTypes.BuyersConsolLead,
						Core.Constants.ShipmentTypeDescriptions.BuyersConsolLead);
					fJS_ShipmentType_List.AddPair(Core.Constants.ShipmentTypes.CoLoadMaster,
						Core.Constants.ShipmentTypeDescriptions.CoLoadMaster);
					fJS_ShipmentType_List.AddPair(Core.Constants.ShipmentTypes.BlindCoLoadMaster,
						Core.Constants.ShipmentTypeDescriptions.BlindCoLoadMaster);
					fJS_ShipmentType_List.AddPair(Core.Constants.ShipmentTypes.AssemblyMaster,
						Core.Constants.ShipmentTypeDescriptions.AssemblyMaster);
				}

				return fJS_ShipmentType_List;
			}
		}

		CodeDescriptionPairList fJS_ShipmentType_List;

		public override CodeDescriptionPairList JS_TransportMode_List
		{
			get { return Factory.GetCachedValue<BookingTransportModeCodeDescriptionPairList>(); }
		}

		protected override MainFormConsolCollection GetNewMainFormConsolCollection()
		{
			return new CFSLoadListConsolCollection(Factory);
		}

		protected override void SetFiltersOnMainFormConsolCollection(MainFormConsolCollection collection)
		{
			var provider = new CFSConsolDefaultFilterProvider
			{
				TransportMode = Shipment.JS_TransportMode,
				LoadPort = Shipment.JS_RL_NKOrigin,
				DischargePort = Shipment.JS_RL_NKDestination,
				ETDTo = Shipment.JS_E_DEP,
				ETAFrom = Shipment.JS_E_ARV
			};
			provider.SetDefaultFilters(collection);
		}
	}
}
