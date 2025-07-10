
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingConsolidationFilterBusinessObject : FilterStripBusinessObject, IDtbBookingConsolidationFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			// consolidation
			ModuleFilter filter = result.AddGuidFilter(FilterNameConstants.TransportCompany, ModuleIDs.Organisation, QueryHelper.ConsolidationTransportCompanyQuery, BindToLists.LocalTransportOrganisations);
			filter.MultilingualDescription = ResString.GetMultilingualString("20a0450d-9644-4101-9ecb-ff40fcc2ae54", "Transport Company");

			filter = result.AddTextFilter(FilterNameConstants.ConsolidationStatus, DtbBookingConsolidationSchema.KB_Status, BindToLists.BookingConsolidationStatuses);
			filter.MultilingualDescription = ResString.GetMultilingualString("392289b2-e489-4380-980d-b96404a62681", "Consolidation Status");
			filter.Category = FilterCategories.StatusAndFlags;

			// bookings
			filter = result.AddFountainFilter(FilterNameConstants.BookingID, DtbBookingSchema.KM_JobID, "TB");
			filter.MultilingualDescription = ResString.GetMultilingualString("3c3a3c53-9634-43ef-9045-60c9beee0e65", "Booking ID");
			filter.SubGroup = BookingSubGroup;

			var transportReferencFilter = result.AddTextFilter(FilterNameConstants.BookingTransportReference, DtbBookingSchema.KM_TransportReference);
			transportReferencFilter.MultilingualDescription = ResString.GetMultilingualString("a9b38bb5-afd7-417a-b8a1-e6cf9a369327", "Booking Transport Reference");
			transportReferencFilter.Category = FilterCategories.StatusAndFlags;
			transportReferencFilter.SubGroup = BookingSubGroup;

			var carrierAccountFilter = result.AddTextFilter(FilterNameConstants.CarrierAccount, QueryHelper.BookingCarrierAccountQuery, new OrgCarrierAccountCollection(Factory));
			carrierAccountFilter.MultilingualDescription = ResString.GetMultilingualString("22a38bdc-8f8b-417a-7856-a6cf9a3693aa", "Carrier Account");
			carrierAccountFilter.Category = FilterCategories.NumbersAndReferences;
			carrierAccountFilter.SubGroup = BookingSubGroup;

			return result;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var filter = new ModuleFountainFilter(FilterNameConstants.ConsolidationID, DtbBookingConsolidationSchema.KB_JobID, "CB");
			filter.MultilingualDescription = ResString.GetMultilingualString("b258e3b0-8f40-441c-999e-26880f7eae3b", "Consolidation ID");
			return filter;
		}

		DtbBookingSubGroup BookingSubGroup
		{
			get { return bookingSubGroup ?? (bookingSubGroup = new DtbBookingSubGroup()); }
		}

		DtbBookingSubGroup bookingSubGroup;

		/// <summary>
		/// Provides a join from Booking to Consolidation.
		/// </summary>
		class DtbBookingSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(DtbBookingConsolidation));
				var bookingSubQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingSchema.KM_KB_BookingConsolidationMultiJob);
				bookingSubQuery.AddToFilter(filter);
				result.AddSubQuery(bookingSubQuery, JoinCondition.And);

				return result;
			}
		}

		BindToLists BindToLists // TODO: Consider moving this to a Factory method.
		{
			get { return bindToLists ?? (bindToLists = new BindToLists(Factory)); }
		}

		TransportBookingsQueryHelper QueryHelper
		{
			get { return queryHelper ?? (queryHelper = new TransportBookingsQueryHelper()); }
		}

		BindToLists bindToLists;
		TransportBookingsQueryHelper queryHelper;
	}
}
