using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.WebCFS.Business
{
	[TestedType(typeof(Fumigation))]
	public class FumigationTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Not required for business objects based on views", true);
		}

		public void TestFumigationDetailsLoadFromJobService()
		{
			ZDateTime dateTimeNow = ZDateTime.Now;
			ZDateTime smallDateTimeNow = new ZDateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0);

			ZDateTime departDate = smallDateTimeNow.AddDays(-7);
			ZDateTime arvDate = smallDateTimeNow.AddDays(-1);
			ZDateTime fumBooked = smallDateTimeNow.AddDays(1);
			ZDateTime fumCompleted = smallDateTimeNow.AddDays(3);

			CreateContainerWithFumigation("OOCL1233450", "SPLATTY ARIANA", "89", "HKHKG", "AUMEL", departDate, arvDate, fumBooked, fumCompleted);

			Factory.Save();

			FumigationCollection fumContainers = new FumigationCollection(Factory);
			fumContainers.Load();
			AssertEquals("Should load one container", 1, fumContainers.Count);
			AssertEquals("ContainerNumber", "OOCL1233450", fumContainers[0].LFV_ContainerNum);
			AssertEquals("EstimatedArrival", arvDate, fumContainers[0].LFV_EstimatedArrivalDate);
			AssertEquals("Vessel", "SPLATTY ARIANA", fumContainers[0].LFV_Vessel);
			AssertEquals("VoyageFlight", "89", fumContainers[0].LFV_VoyageFlight);
			AssertEquals("FumigationBooked", fumBooked, fumContainers[0].LFV_FumigationBooked);
			AssertEquals("FumigationCompleted", fumCompleted, fumContainers[0].LFV_FumigationCompleted);
		}

		public void TestFumigationViewDisplaysOnlyLast30Days()
		{
			ZDateTime dateTimeNow = ZDateTime.Now;
			ZDateTime now = new ZDateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0);

			CreateContainerWithFumigation("OOCL1111112", "SPLATTY ARIANA", "89", "HKHKG", "AUSYD", now.AddDays(-45), now.AddDays(-37), now.AddDays(-35), now.AddDays(-32));
			CreateContainerWithFumigation("OOCL2222229", "SPLATTY ARAFURA", "90", "SGSIN", "AUMEL", now.AddDays(-40), now.AddDays(-33), now.AddDays(-31), now.AddDays(-29));
			CreateContainerWithFumigation("OOCL3333335", "SPLATTY BRIGIT", "91", "JPOSA", "AUFRE", now.AddDays(-33), now.AddDays(-23), now.AddDays(-22), now.AddDays(-20));
			CreateContainerWithFumigation("OOCL4444441", "SPLATTY CONDOR", "92", "JPUKY", "AUBNE", now.AddDays(-29), now.AddDays(-20), now.AddDays(-19), now.AddDays(-17));
			CreateContainerWithFumigation("OOCL5555558", "SPLATTY JABIRU", "93", "THBKK", "AUNTL", now.AddDays(-7), now.AddDays(-1), now.AddDays(1), now.AddDays(2));

			Factory.Save();

			FumigationCollection fumContainers = new FumigationCollection(Factory);

			fumContainers.Load();
			AssertEquals("Should load 3 containers", 3, fumContainers.Count);
			AssertContainsContainer(fumContainers, "OOCL3333335", "SPLATTY BRIGIT", "91", now.AddDays(-23));
			AssertContainsContainer(fumContainers, "OOCL4444441", "SPLATTY CONDOR", "92", now.AddDays(-20));
			AssertContainsContainer(fumContainers, "OOCL5555558", "SPLATTY JABIRU", "93", now.AddDays(-1));
		}

		void AssertContainsContainer(FumigationCollection fumContainers, string containerNum, string vesselName, string voyageFlight, ZDateTime estimatedArv)
		{
			ZQuery bizOQuery = new ZQuery(vw_List_FumigationSchema.LFV_ContainerNum, containerNum);
			bizOQuery.AddToFilter(vw_List_FumigationSchema.LFV_Vessel, vesselName);
			bizOQuery.AddToFilter(vw_List_FumigationSchema.LFV_VoyageFlight, voyageFlight);
			bizOQuery.AddToFilter(vw_List_FumigationSchema.LFV_EstimatedArrivalDate, estimatedArv);

			BusinessObject[] result = fumContainers.Find(bizOQuery);
			Assert(String.Format("Multiple FumigationContainers found while looking for {0}//{1}//{2}//{3}", containerNum, vesselName, voyageFlight, estimatedArv), result.Length < 2);
			AssertEquals(String.Format("Failed to find Fumigation with details {0}//{1}//{2}//{3}", containerNum, vesselName, voyageFlight, estimatedArv), 1, result.Length);
			Fumigation fumCont = result[0] as Fumigation;
			AssertNotNull("BusinessObject should be Fumigation", fumCont);
			AssertEquals("ContainerNumber should match", containerNum, fumCont.LFV_ContainerNum);
			AssertEquals("VesselName should match", vesselName, fumCont.LFV_Vessel);
			AssertEquals("VoyageFlight should match", voyageFlight, fumCont.LFV_VoyageFlight);
			AssertEquals("EstimatedArrival should match", estimatedArv, fumCont.LFV_EstimatedArrivalDate);
		}

		void CreateContainerWithFumigation(string containerNum, string vesselName, string voyageFlight, string loadPort, string dischargePort,
											ZDateTime departDate, ZDateTime arvDate, ZDateTime fumBooked, ZDateTime fumCompleted)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = containerNum;

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = vesselName;

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = voyageFlight;
			voyage.JV_RV_NKVessel = vesselName;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadPort;
			origin.JA_E_DEP = departDate;

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			destination.JB_E_ARV = arvDate;

			Transport transport = consol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;

			JobService fumigation = container.Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Booked = fumBooked;
			fumigation.ES_Completed = fumCompleted;
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected new BusinessObjectFactory Factory
		{
			get { return base.Factory; }
		}
	}
}
