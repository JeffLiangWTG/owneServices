using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ScheduleTransportLegBusinessObjectFinder))]
	sealed class ScheduleTransportLegBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ScheduleTransportLegBusinessObjectFinder(null, null));

			var voyage = Factory.New<JobVoyage>();

			AssertExceptionThrown(typeof(ArgumentNullException), () => new ScheduleTransportLegBusinessObjectFinder(null, voyage));

			var dataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.PortOfLoading = new UNLOCO { Code = "NZAKL" };

			AssertExceptionThrown(typeof(ArgumentNullException), () => new ScheduleTransportLegBusinessObjectFinder(dataObject, null));

			var origin1 = voyage.Origins.AddNew("AUSYD");
			var origin2 = voyage.Origins.AddNew("NZAKL");
			var origin3 = voyage.Origins.AddNew("USSFO");

			var destination1 = voyage.Destinations.AddNew("NZAKL");
			var destination2 = voyage.Destinations.AddNew("USSFO");
			var destination3 = voyage.Destinations.AddNew("USMIA");

			origin1.JA_E_DEP = ZDate.Today.AddDays(1);
			destination1.JB_A_ARV = ZDate.Today.AddDays(2);
			origin2.JA_E_DEP = ZDate.Today.AddDays(3);
			destination2.JB_A_ARV = ZDate.Today.AddDays(4);
			origin3.JA_E_DEP = ZDate.Today.AddDays(5);
			destination3.JB_A_ARV = ZDate.Today.AddDays(6);

			voyage.GenerateSailings();

			var finder = new ScheduleTransportLegBusinessObjectFinder(dataObject, voyage);
			AssertEquals(null, finder.Find(voyage.Sailings.Cast<JobSailing>()));

			dataObject.PortOfDischarge = new UNLOCO { Code = "USSFO" };

			finder = new ScheduleTransportLegBusinessObjectFinder(dataObject, voyage);
			var expectedSailing = voyage.Sailings.Cast<JobSailing>().FirstOrDefault(s => s.JX_JA_RL_NKPortOfLoading == "NZAKL" && s.JX_JB_RL_NKPortOfDischarge == "USSFO");
			AssertEquals(expectedSailing, finder.Find(voyage.Sailings.Cast<JobSailing>()));
		}
	}
}
