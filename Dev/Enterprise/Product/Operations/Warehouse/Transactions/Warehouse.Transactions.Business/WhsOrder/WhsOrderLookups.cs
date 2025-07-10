using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderLookups : WhsPickableDocketLookups
	{
		public WhsOrderLookups(WhsOrder parent)
			: base(parent)
		{
		}

		#region SubTypes

		protected override CodeDescriptionPairList SubTypesCore => new CodeLists.OrderType();

		#endregion

		#region Forwarders

		public override OrgHeaderCollection Forwarders
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region INCOTerms

		protected override CodeDescriptionPairList INCOTermsCore
		{
			get { return Parent.IsDomesticFreight ? DomesticPaymentTermsList : InternationalPaymentTermsList; }
		}

		CodeDescriptionPairList InternationalPaymentTermsList
		{
			get { return Factory.GetCachedValue("WhsOrderLookups|InternationalPaymentTermsList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)); }
		}

		#endregion

		#region CrossDockLocations

		public WhsLocationCollection CrossDockLocations
		{
			get
			{
				return Factory.GetCachedValue("WhsDocketLookups|CrossDockLocations|" + Parent.WD_WW_Whs, () =>
				{
					var warehouse = Parent.Warehouse;
					return warehouse != null
						? new WhsLocationCollection(Parent.Warehouse, dockDoorLocationsOnly: true)
						: new WhsLocationCollection(Factory, dockDoorLocationsOnly: true);
				});
			}
		}

		#endregion

		#region SalesChannels

		public WhsSalesChannelCollection SalesChannels => Factory.GetCachedValue("WhsDocketLookups|SalesChannels", () => new WhsSalesChannelCollection(Factory));

		#endregion
	}
}
