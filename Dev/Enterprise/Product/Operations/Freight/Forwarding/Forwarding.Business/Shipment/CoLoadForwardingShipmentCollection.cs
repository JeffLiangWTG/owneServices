using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CoLoadForwardingShipmentCollection : CoLoadShipmentCollection
	{
		public CoLoadForwardingShipmentCollection(ForwardingShipment master, BusinessObjectFactory factory)
			: base(master, factory)
		{
			AllowAddNew = Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed || !master.HasConsolPastCutOffDate;
		}

		public ForwardingShipment ParentForwardingShipment => Master as ForwardingShipment;

		public new ForwardingShipment this[int i]
		{
			get { return (ForwardingShipment)base[i]; }
		}

		public new ForwardingShipment AddNew()
		{
			return (ForwardingShipment)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(ForwardingShipment);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (ParentForwardingShipment != null && !IsLoading && !IsUpdatingByDataRefreshBus)
			{
				SetBookingPartyDocumentaryAddressChildOnMaster(bizOAdded);
				ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartAddToBizO(ParentForwardingShipment, ParentForwardingShipment, bizOAdded);
			}
		}

		protected override void OnRemoved(BusinessObject bizORemoved)
		{
			base.OnRemoved(bizORemoved);

			if (!IsUpdatingByDataRefreshBus && ParentForwardingShipment != null)
			{
				SetBookingPartyDocumentaryAddressChildOnMaster(bizORemoved);
				ScreeningStatusUpdater.SetShouldUpdateScreeningStatusWhenPartRemovedFromBizO(ParentForwardingShipment, ParentForwardingShipment);
			}
		}

		void SetBookingPartyDocumentaryAddressChildOnMaster(BusinessObject bizO)
		{
			if (ParentForwardingShipment?.BookingParty == null && bizO is ForwardingShipment)
			{
				var coLoadShipments = ParentForwardingShipment.CoLoadShipments?.Cast<ForwardingShipment>();
				if (coLoadShipments.Any() && coLoadShipments.AllSame(s => s.BookingParty))
				{
					ParentForwardingShipment.BookingPartyDocumentaryAddress.CopyPersistentValuesFrom(coLoadShipments.FirstOrDefault().BookingPartyDocumentaryAddress);
				}
			}
		}
	}
}
