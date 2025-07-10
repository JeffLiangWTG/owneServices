using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingLookups : Common.DtbBookingLookups
	{
		public DtbBookingLookups(DtbBooking parent)
			: base(parent)
		{
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Carrier)]
		public OrgHeaderCollection LocalTransportOrganisations
		{
			get { return BindToLists.LocalTransportOrganisations; }
		}

		public CodeDescriptionPairList RatingFreightModes
		{
			get { return BindToLists.RatingFreightModes.List; }
		}

		public override OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get { return Factory.GetCachedValue($"TransportBookings|BindToLists|CarrierServiceLevels|{Booking.PK}", () => new CachedProperty<OrgCarrierServiceLevelCollection>(Factory, GetCarrierServiceLevels)).Value; }
		}

		OrgCarrierServiceLevelCollection GetCarrierServiceLevels()
		{
			var carrierAddress = Booking.Address;
			var carrier = carrierAddress != null ? carrierAddress.Organisation : null;
			var result = carrier != null ? new OrgCarrierServiceLevelCollection(carrier.MiscServ) : new OrgCarrierServiceLevelCollection(Factory);
			result.Load();

			return result;
		}

		public override OrgCarrierAccountCollection CarrierAccounts
		{
			get { return Factory.GetCachedValue($"TransportBookings|BindToLists|CarrierAccounts|{Booking.PK}", () => new CachedProperty<OrgCarrierAccountCollection>(Factory, GetCarrierAccounts)).Value; }
		}

		OrgCarrierAccountCollection GetCarrierAccounts()
		{
			var carrierAddress = Booking.Address;
			var carrier = carrierAddress != null ? carrierAddress.Organisation : null;
			return carrier != null ? new OrgCarrierAccountCollection(carrier) : new OrgCarrierAccountCollection(Factory, ZQuery.NoResultQuery);
		}

		public BindToLists BindToLists
		{
			get { return Factory.GetCachedValue("TransportBookings|BindToLists", () => new BindToLists(Factory)); }
		}

		DtbBooking Booking
		{
			get { return (DtbBooking)Parent; }
		}

		public DtbBookingTmplCollection BookingTemplates
		{
			get { return BindToLists.BookingTemplates; }
		}

		public DtbBookingCollectionForSubBookingsFindBox SubBookingsFindBoxList
		{
			get { return new DtbBookingCollectionForSubBookingsFindBox(Booking, false); }
		}

		public CodeDescriptionPairList BookingTransportModes
		{
			get { return BindToLists.BookingTransportModes.List; }
		}
	}
}
