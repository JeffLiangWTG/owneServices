using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	internal sealed class ViewQuotedBookingFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.ResetDatabaseLoadCount();
			ViewQuotedBooking[] viewQuotedBookings = factory.Load<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.PK, CreateViewQuotedBookings()));

			string hit = "";
			foreach (ViewQuotedBooking viewQuotedBooking in viewQuotedBookings)
			{
				QuotedBooking quotedBooking = QuotedBooking.New(viewQuotedBooking.VB_TH, viewQuotedBooking.VB_JS, factory);

				hit = quotedBooking.ConsignorDocumentaryAddress.Address.OA_Code;
				hit = quotedBooking.ConsigneeDocumentaryAddress.Address.OA_Code;
				if (quotedBooking.Booking != null)
				{
					hit = quotedBooking.QuotedBookingContainers[0].JC_ContainerCode;
				}
			}

			//JobDocAddress: 22
			//JobDeclaration: 21
			//OrgAddress: 21
			//OrgContact: 20
			//JobShipment: 2
			//RateOneOffContainers: 2
			//RateOneOffPackLine: 2
			//JobConShipLink: 1
			//JobContainer: 1
			//JobPackLines: 1
			//OrgAddressCapability: 1
			//OrgHeader: 1
			//RateOneOfShipment: 1
			//RatingHeader: 1
			//ViewQuotedBooking: 1

			//Hits: 75/75

			AssertMaxDbHits(98, factory);
		}

		public void TestFetchForView()
		{
			List<ZGuid> viewQuotedBookingGuids = CreateViewQuotedBookings();

			CombineAssertions(() =>
			{
				CheckDBHitCount(viewQuotedBookingGuids, "QuotedBooking+Booking+NumbersAsString", 1, true);
				CheckDBHitCount(viewQuotedBookingGuids, "QuotedBooking+Booking+NumbersAsString", 117, false);
			});
		}

		#region Implementation

		static List<ZGuid> CreateViewQuotedBookings()
		{
			List<ZGuid> result = new List<ZGuid>();
			BusinessObjectFactory factory = new BusinessObjectFactory();

			//Quoted Booking
			for (int i = 0; i < 10; i++)
			{
				Quote quote = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
				ForwardingShipment booking = QuotedBooking.CreateNewBooking(factory);
				QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, factory);

				OrgHeader cnr = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader cne = factory.NewWithValidTestData<OrgHeader>();

				var vessel = factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "vessel" + i;

				JobVoyage voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();

				quotedBooking.Mode = Core.Constants.RateMode.FCL;
				quotedBooking.Origin = "AUBNE";
				quotedBooking.Destination = "NLAMS";
				quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = cnr.MainAddress.PK;
				quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = cne.MainAddress.PK;
				quotedBooking.Booking.JS_JX = voyage.Sailings[0].PK;
				quotedBooking.QuotedBookingContainers.AddNew();
				quotedBooking.TryLoadOrCreateJob();

				result.Add(quotedBooking.PK);
			}

			//Quote Only
			for (int i = 0; i < 10; i++)
			{
				Quote quoteOnly = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted);
				QuotedBooking quotedBooking_QuoteOnly = QuotedBooking.New(quoteOnly.PK, ZGuid.Empty, factory);

				OrgHeader cnr = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader cne = factory.NewWithValidTestData<OrgHeader>();

				quotedBooking_QuoteOnly.Mode = Core.Constants.RateMode.FCL;
				quotedBooking_QuoteOnly.Origin = "AUBNE";
				quotedBooking_QuoteOnly.Destination = "NZAKL";
				quotedBooking_QuoteOnly.ConsignorDocumentaryAddress.E2_OA_Address = cnr.MainAddress.PK;
				quotedBooking_QuoteOnly.ConsigneeDocumentaryAddress.E2_OA_Address = cne.MainAddress.PK;
				quotedBooking_QuoteOnly.TryLoadOrCreateJob();

				result.Add(quotedBooking_QuoteOnly.PK);
			}

			//Booking Only
			for (int i = 0; i < 10; i++)
			{
				ForwardingShipment bookingOnly = QuotedBooking.CreateNewBooking(factory);
				QuotedBooking quotedBooking_BookingOnly = QuotedBooking.New(ZGuid.Empty, bookingOnly.PK, factory);

				var vessel = RefVessel.LookupVesselByName("vessel" + i, factory).First();

				JobVoyage voyage = factory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "voyage" + i;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
				voyage.GenerateSailings();

				OrgHeader cnr = factory.NewWithValidTestData<OrgHeader>();
				OrgHeader cne = factory.NewWithValidTestData<OrgHeader>();

				quotedBooking_BookingOnly.Mode = Core.Constants.RateMode.FCL;
				quotedBooking_BookingOnly.Origin = "AUBNE";
				quotedBooking_BookingOnly.Destination = "NZAKL";
				quotedBooking_BookingOnly.ConsignorDocumentaryAddress.E2_OA_Address = cnr.MainAddress.PK;
				quotedBooking_BookingOnly.ConsigneeDocumentaryAddress.E2_OA_Address = cne.MainAddress.PK;
				quotedBooking_BookingOnly.Booking.JS_JX = voyage.Sailings[0].PK;
				quotedBooking_BookingOnly.QuotedBookingContainers.AddNew();
				quotedBooking_BookingOnly.TryLoadOrCreateJob();

				result.Add(quotedBooking_BookingOnly.PK);
			}

			factory.Save();

			return result;
		}

		void CheckDBHitCount(List<ZGuid> viewQuotedBookingGuids, string columnName, int maxDbHits, bool fetchForView)
		{
			var factory = new BusinessObjectFactory();
			var viewQuotedBookings = factory.Load<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.PK, viewQuotedBookingGuids));

			if (fetchForView)
			{
				foreach (var viewQuotedBooking in viewQuotedBookings)
				{
					viewQuotedBooking.FetchStrategy.FetchForView(new[] { new TableColumn("", columnName) });
				}
			}

			factory.ResetDatabaseLoadCount();

			foreach (var viewQuotedBooking in viewQuotedBookings)
			{
				object value = viewQuotedBooking[columnName];
			}

			var message = ZString.Format("column name: {0}, max db hit count: {1}, actual count: {2}", columnName, maxDbHits, factory.DatabaseLoadCount);

			Assert(message, factory.DatabaseLoadCount <= maxDbHits);
		}

		#endregion
	}
}
