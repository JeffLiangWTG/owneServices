using Enterprise.MasterFiles.Business;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationLookups : Common.DtbBookingConsolidationLookups
	{
		public DtbBookingConsolidationLookups(DtbBookingConsolidation parent)
			: base(parent)
		{
		}

		protected new DtbBookingConsolidation Parent
		{
			get { return (DtbBookingConsolidation)base.Parent; }
		}

		public DtbBookingCollectionForFindBox BookingsFindBoxList
		{
			get { return new DtbBookingCollectionForFindBox(Parent.Factory, Parent.Address.Organisation); }
		}

		public OrgHeaderCollection LocalTransportOrganisations
		{
			get { return BindToLists.LocalTransportOrganisations; }
		}

		public BindToLists BindToLists
		{
			get { return Factory.GetCachedValue("TransportBookings|BindToLists", () => new BindToLists(Factory)); }
		}

		public DtbBookingCollectionForSubBookingsFindBox SubBookingsFindBoxList
		{
			get { return new DtbBookingCollectionForSubBookingsFindBox(Parent.Factory, Parent.Address.Address, true); }
		}
	}
}
