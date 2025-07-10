using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketLookups : AutoWhsDocketLookups
	{
		public WhsDocketLookups(AutoWhsDocket parent)
			: base(parent)
		{
		}

		#region Parent

		new protected WhsDocket Parent
		{
			get { return (WhsDocket)base.Parent; }
		}

		#endregion

		#region Clients

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.WarehouseClient)]
		public override OrgHeaderCollection Clients
		{
			get
			{
				WarehouseClientCollectionWithSecurityCheck result = new WarehouseClientCollectionWithSecurityCheck(Factory);
				return result;
			}
		}

		#endregion

		#region DistributionCentres

		public OrgHeaderCollection DistributionCentres
		{
			get { return new DistributionCentreCollection(Factory); }
		}

		#endregion

		#region Warehouses

		public virtual WhsWarehouseCollection Warehouses
		{
			get { return new WhsWarehouseCollectionWithSecurityCheck(Factory); }
		}

		#endregion

		#region TransportZones

		public virtual RateTransportZonesCollection TransportZones
		{
			get { return new RateTransportZonesCollection(Factory); }
		}

		#endregion

		#region PackingTasks

		public override ProcessTaskCollection PackingTasks => Factory.GetCachedValue("WhsDocketLookups|PackingTasks", () => new ProcessTaskCollection(Factory, ZQuery.NoResultQuery));

		#endregion

		#region Pickups

		public OrgHeaderCollection PickUps
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region DropOffs

		public OrgHeaderCollection DropOffs
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region Consignees

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public OrgHeaderCollection Consignees
		{
			get
			{
				var result = new ConsigneeCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
					"Consignee - Related Consignor"
					, "Property"
					, delegate
					{
						var client = Parent.Client;
						return (client != null && client.BuyerLinks.Count > 0 && client.OH_IsConsignor) ? Parent.WD_OH_Client : ZGuid.Empty;
					}
				));
				return result;
			}
		}

		#endregion

		#region Suppliers

		public OrgHeaderCollection Suppliers
		{
			get { return new ConsignorCollection(Factory); }
		}

		#endregion

		#region Debtors

		public OrgHeaderCollection Debtors
		{
			get { return new DebtorCollection(Factory); }
		}

		#endregion

		#region TransportCos

		public OrgHeaderCollection TransportCos
		{
			get { return new ShippingProviderCollection(Factory); }
		}

		#endregion

		#region ProductUQ

		public CodeDescriptionPairList ProductUQ
		{
			get { return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits(); }
		}

		#endregion

		#region WeightUnitTypes

		public CodeDescriptionPairList WeightUnitTypes
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region CubicUnitTypes

		public CodeDescriptionPairList CubicUnitTypes
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region WhsOrderFulfillmentRules

		public WhsOrderFulfillmentRuleList WhsOrderFulfillmentRules
		{
			get { return new WhsOrderFulfillmentRuleList(); }
		}

		#endregion

		#region PickOptions

		public WhsPickOption PickOptions
		{
			get { return new WhsPickOption(); }
		}

		#endregion

		#region DropModes

		public CodeDescriptionPairList DropModes
		{
			get { return Parent.Containers.Count > 0 ? FCLDropModes : LCLDropModes; }
		}

		#endregion

		#region FCLDropModes

		CodeDescriptionPairList FCLDropModes
		{
			get { return new FCLEquipmentNeededList(true); }
		}

		#endregion

		#region LCLDropModes

		CodeDescriptionPairList LCLDropModes
		{
			get { return new LCLAIREquipmentNeededList(true); }
		}

		#endregion

		#region SubTypes

		public CodeDescriptionPairList SubTypes => SubTypesCore;

		protected virtual CodeDescriptionPairList SubTypesCore => null;

		#endregion

		#region ShipperCODPaymentTypes

		public ReadOnlyCodeDescriptionPairList ShipperCODPaymentTypes
		{
			get { return Factory.GetCachedValue("WhsDocketLookups|ShipperCODPaymentTypes", () => FreightDataRegistry.Instance.ShipperCODPaymentTypes.Value); }
		}

		#endregion

		#region INCOTerms

		public CodeDescriptionPairList INCOTerms
		{
			get { return INCOTermsCore; }
		}

		protected virtual CodeDescriptionPairList INCOTermsCore
		{
			get { return DomesticPaymentTermsList; }
		}

		protected CodeDescriptionPairList DomesticPaymentTermsList
		{
			get { return Factory.GetCachedValue("WhsDocketLookups|DomesticPaymentTermsList", () => new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms)); }
		}

		#endregion

		#region TransportModes

		public CodeDescriptionPairList TransportModes
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.TransportType); }
		}

		#endregion

		#region ContainerModes

		public CodeDescriptionPairList ContainerModes
		{
			get { return ContainerModesFromTransportMode(Parent.WD_TransportMode); }
		}

		public CodeDescriptionPairList ContainerModesFromTransportMode(ZString transportMode)
		{
			return Factory.GetCachedValue("WhsDocketLookups|ContainerModes_" + transportMode,
				() => ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(transportMode));
		}

		#endregion

		#region ScreeningStatuses

		public CodeDescriptionPairList ScreeningStatusesList => Factory.GetCachedValue<ScreeningStatusesList>();

		#endregion

		#region PicksForReplenishment

		public WhsPickCollectionForTransfers PicksForReplenishment => PicksForReplenishmentCore;

		protected virtual WhsPickCollectionForTransfers PicksForReplenishmentCore => null;

		#endregion
	}
}
