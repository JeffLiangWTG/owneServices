using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class CombineBookingsLookups : ZLookups
	{
		public CombineBookingsLookups(CombineBookings parent)
			: base(parent) { }

		public AgencyBookingCollection Bookings
		{
			get
			{
				ZQuery additionalFilter = new ZQuery();
				additionalFilter.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, Parent.MasterBooking.PK);

				AgencyShipmentDefaultFilterProvider provider = new AgencyShipmentDefaultFilterProvider();

				AgencyShipment shipment = Parent.MasterBooking;
				if (shipment != null)
				{
					additionalFilter.AddToFilter(JobShipmentSchema.JS_PackingMode, shipment.JS_PackingMode);
					additionalFilter.AddToFilter(JobShipmentSchema.JS_IsCancelled, false);
					provider.ContainerMode = shipment.JS_PackingMode;

					if (shipment.BookingParty != null)
					{
						provider.BookingParty = shipment.BookingParty.PK;
						additionalFilter.AddToFilter(GetOrganisationFilter(DocAddressTypes.Codes.BookingPartyDocumentaryAddress, shipment.BookingParty.PK));
					}

					if (shipment.ConsignorPK.IsValid)
					{
						provider.Consignor = shipment.ConsignorPK;
						additionalFilter.AddToFilter(GetOrganisationFilter(DocAddressTypes.Codes.ConsignorDocumentaryAddress, shipment.ConsignorPK));
					}

					if (shipment.ConsigneePK.IsValid)
					{
						provider.Consignee = shipment.ConsigneePK;
						additionalFilter.AddToFilter(GetOrganisationFilter(DocAddressTypes.Codes.ConsigneeDocumentaryAddress, shipment.ConsigneePK));
					}

					if (!shipment.JS_RL_NKOrigin.IsEmpty)
					{
						provider.OriginPort = shipment.JS_RL_NKOrigin;
						additionalFilter.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, shipment.JS_RL_NKOrigin);
					}

					if (!shipment.JS_RL_NKDestination.IsEmpty)
					{
						provider.DestinationPort = shipment.JS_RL_NKDestination;
						additionalFilter.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, shipment.JS_RL_NKDestination);
					}

					JobSailing sailing = shipment.Sailing;
					if (sailing != null)
					{
						additionalFilter.AddToFilter(JobShipmentSchema.JS_JX, sailing.PK);
						provider.LoadPort = sailing.JX_JA_RL_NKPortOfLoading;
						provider.DischargePort = sailing.JX_JB_RL_NKPortOfDischarge;
						provider.Vessel = sailing.JX_JV_NKVessel;
						provider.Voyage = sailing.JX_JV_VoyageFlight;
					}
				}

				AgencyBookingCollection bookings = new AgencyBookingCollection(Factory);
				bookings.AdditionalFilter = additionalFilter;
				provider.SetDefaultFilters(bookings);

				return bookings;
			}
		}

		#region Implementation

		ZQuery GetOrganisationFilter(ZString addressType, ZGuid organisation)
		{
			ZDBOnlySubQuery docAddress = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, !organisation.IsValid);
			docAddress.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			docAddress.AddToFilter(JobDocAddressSchema.E2_AddressOverride, false);

			if (organisation.IsValid)
			{
				ZDBOnlySubQuery orgAddress = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				orgAddress.AddToFilter(OrgAddressSchema.OA_OH, organisation);

				docAddress.AddSubQuery(orgAddress, JoinCondition.And);
			}

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AgencyBooking));
			result.AddSubQuery(docAddress, JoinCondition.And);

			return result;
		}

		protected new CombineBookings Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (CombineBookings)base.Parent; }
		}

		#endregion
	}
}
