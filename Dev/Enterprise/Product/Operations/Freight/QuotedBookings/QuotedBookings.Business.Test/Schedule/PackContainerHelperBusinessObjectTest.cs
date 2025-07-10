using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(PackContainerHelper))]
	public class PackContainerHelperBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBuildConsol()
		{
			JobSailing sailing = GetSailing();

			QuotedBooking booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			booking.Booking.JS_A_BKD = new ZDateTime(2004, 1, 3);
			booking.Booking.JS_RL_NKOrigin = "AUSYD";
			booking.Booking.JS_RL_NKDestination = "USLAX";
			booking.Booking.JS_ActualVolume = 4m;
			booking.Booking.JS_ActualWeight = 300m;
			booking.Booking.JS_OuterPacks = 6;
			booking.Booking.JS_GoodsDescription = "SDFSDFSDF";
			booking.Booking.JS_A_RCV = ZDateTime.Today; //IsReceived

			PackLine packline1 = booking.Booking.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 1;
			packline1.JL_ActualVolume = 1m;
			packline1.JL_ActualWeight = 1m;
			PackLine packline2 = booking.Booking.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 2;
			packline2.JL_ActualVolume = 2m;
			packline2.JL_ActualWeight = 2m;
			PackLine packline3 = booking.Booking.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_ActualVolume = 3m;
			packline3.JL_ActualWeight = 3m;
			PackLine packline4 = booking.Booking.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 4;
			packline4.JL_ActualVolume = 4m;
			packline4.JL_ActualWeight = 4m;

			sailing.Shipments.Add(booking.Booking);
			Factory.Save();

			PackContainerHelper packContainerHelper = new PackContainerHelper(sailing);
			AssertEquals("Should have 0 containers", 0, packContainerHelper.Containers.Count);
			AssertEquals("Should have 5 unallocated packlines", 5, sailing.UnAllocatedPackLines.Count);

			ForwardingContainer container = packContainerHelper.AddBookingContainer();
			container.AddPackLines(sailing.UnAllocatedPackLines.ToArray());
			AssertEquals("Should have 5 packlines", 5, container.PackLines.Count);

			Factory.Save();

			BuildConsolHelper buildConsolHelper = new BuildConsolHelper();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ForwardingConsol newConsol = newFactory.New<ForwardingConsol>();
			buildConsolHelper.BuildConsolFromPackContainers(newConsol, new CommonContainer[] { container }, sailing);
			newFactory.Save();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PackContainerHelper(GetSailing());
		}

		JobSailing GetSailing()
		{
			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "234";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_E_DEP = new ZDateTime(2004, 1, 20);
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_E_ARV = new ZDateTime(2004, 2, 20);
			destination.JB_RL_NKPortOfDischarge = "USLAX";

			JobSailing sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_DepotCutOff = new ZDateTime(2004, 1, 18);
			sailing.JX_DepotReceivalCommences = new ZDateTime(2004, 1, 1);
			sailing.Origin.JA_ReceivalCommences = new ZDateTime(2004, 1, 1);
			sailing.Origin.JA_CutOff = new ZDateTime(2004, 1, 19);

			Factory.Save();

			return sailing;
		}

		#endregion
	}
}
