using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business.Test
{
	public class DtbBookingConsolidationFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			var viewFactory = new BusinessObjectFactory();
			var consolidations = viewFactory.Load<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.PK, CreateTransportBookingConsolidations().Select(c => c.PK)));
			int beforeFetchForView = viewFactory.DatabaseLoadCount;
			foreach (var consolidation in consolidations)
			{
				consolidation.FetchStrategy.FetchForView(TableColumnsAddedInFetchForView);
			}

			foreach (var consolidation in consolidations)
			{
				var pokes = new object[] {
					consolidation.KB_JobID,
					consolidation.Address,
					consolidation.StatusDescription
				};
			}

			var expetedDbHits = new Dictionary<string, int>();
			expetedDbHits.Add(DtbBookingConsolidationSchema.Constants.TableName, 1);

			AssertDbHits(expetedDbHits, viewFactory);
		}

		TableColumn[] TableColumnsAddedInFetchForView
		{
			get
			{
				return new TableColumn[]
				{
					new TableColumn(DtbBookingConsolidationSchema.Constants.TableName, "KB_JobID"),
					new TableColumn(DtbBookingConsolidationSchema.Constants.TableName, "Address+OrganisationNameOrPK"),
					new TableColumn(DtbBookingConsolidationSchema.Constants.TableName, "StatusDescription")
				};
			}
		}

		IEnumerable<DtbBookingConsolidation> CreateTransportBookingConsolidations()
		{
			var data = new TransportBookingTestData(Factory);
			var bookings = data.CreateTransportBookings();
			return bookings.Select(b => Factory.Load<DtbBookingConsolidation>(b.KM_KB_Booking));
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;
	}
}
