using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefTransitTime))]
	sealed class RefTransitTimeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTransitDaysAndHours()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime.TransitDays = 2;
			transitTime.TransitHours = 18;

			AssertEquals(66, transitTime.RTT_TransitHours);

			transitTime.RTT_TransitHours = 83;
			Factory.Save();

			transitTime = new BusinessObjectFactory().Load<RefTransitTime>(transitTime.PK);
			AssertEquals(3, transitTime.TransitDays);
			AssertEquals(11, transitTime.TransitHours);
		}

		public void TestHumanReadableName()
		{
			var transitTime = Factory.New<RefTransitTime>();
			transitTime.RTT_RS_NKServiceLevel = "STD";
			transitTime.RTT_FZ_OriginInternationalZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUEC")).PK;
			transitTime.RTT_FZ_DestinationInternationalZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USEC")).PK;

			AssertEquals("STD Zone Transit: AUEC - USEC", transitTime.HumanReadableName);
		}

		public void TestTransitTimeFormatted()
		{
			var transitTime = Factory.New<RefTransitTime>();
			transitTime.TransitDays = 1;
			transitTime.TransitHours = 15;

			AssertEquals("1 day 15 hours", transitTime.TransitTimeFormatted);

			transitTime.TransitDays = 4;
			transitTime.TransitHours = 1;

			AssertEquals("4 days 1 hour", transitTime.TransitTimeFormatted);
		}

		public void TestRefTransitTimeDetails()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			var transitTimeDetails = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetails.RTD_RTT_Parent = transitTime.PK;

			Factory.Save();

			transitTime = new BusinessObjectFactory().Load<RefTransitTime>(transitTime.PK);

			var transitTimeDetailAfterLoad = transitTime.RefTransitTimeDetails.FirstOrDefault();

			AssertEquals(transitTimeDetailAfterLoad.PK, transitTimeDetails.PK);
		}
	}
}
