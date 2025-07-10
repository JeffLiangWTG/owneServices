//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgWhsClientAccountAssociationLookups
//
//    This class should be used for overriding collections in AutoOrgWhsClientAccountAssociationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgWhsClientAccountAssociationLookups : AutoOrgWhsClientAccountAssociationLookups
	{
		public OrgWhsClientAccountAssociationLookups(AutoOrgWhsClientAccountAssociation parent)
			: base(parent)
		{
		}

		#region Parent

		public new OrgWhsClientAccountAssociation Parent => (OrgWhsClientAccountAssociation)base.Parent;

		#endregion

		#region Warehouses

		public IWhsWarehouseCollection Warehouses
			=> Factory.GetCachedValue("OrgWhsClientAccountAssociationLookups|Warehouses", () => ObjectFactory.Get<IWhsWarehouseCollection>("IWhsWarehouseCollection", Factory));

		#endregion

		#region SalesChannels

		public IWhsSalesChannelCollection SalesChannels
			=> Factory.GetCachedValue("OrgWhsClientAccountAssociationLookups|SalesChannels", () => ObjectFactory.Get<IWhsSalesChannelCollection>("IWhsSalesChannelCollection", Factory));

		#endregion

		public CodeDescriptionPairList BillingTypesList
		{
			get
			{
				return new CodeDescriptionPairList()
				{
					new CodeDescriptionPair(CarrierBillingType.BillReceiver, Res.GetString("MasterFiles|BillingTypesList|BillReceiver", "Bill Receiver")),
					new CodeDescriptionPair(CarrierBillingType.BillSender, Res.GetString("MasterFiles|BillingTypesList|BillSender", "Bill Sender")),
					new CodeDescriptionPair(CarrierBillingType.BillThirdParty, Res.GetString("MasterFiles|BillingTypesList|BillThirdParty", "Bill Third Party"))
				};
			}
		}

		public override OrgCarrierAccountCollection BillToCarrierAccounts => GetOrgCarrierAccountCollectionByCarrier();

		public override OrgCarrierAccountCollection DutyBillToCarrierAccounts => GetOrgCarrierAccountCollectionByCarrier();

		OrgCarrierAccountCollection GetOrgCarrierAccountCollectionByCarrier()
		{
			var carrier = Parent.CarrierAccount?.Carrier;
			return carrier != null
				? new OrgCarrierAccountCollection(carrier)
				: new OrgCarrierAccountCollection(Factory, ZQuery.NoResultQuery);
		}
	}
}
