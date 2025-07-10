using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerTest_SailingSupport : BaseFreightTest
	{
		public void TestParentLoadList()
		{
			Assert("Expecting only this sailing to default to true.", Sailing.JX_ShowOnlyThisSailing);
			Assert("Expecting only received shipments to default to true.", Sailing.JX_ShowOnlyReceived);

			AssertEquals("Expecting 2 packlines in the unallocated packlines view.", 2, Sailing.UnAllocatedPackLines.Count);

			Sailing.JX_ShowOnlyReceived = false;
			AssertEquals("Expecting 3 packlines in the unallocated packlines view.", 3, Sailing.UnAllocatedPackLines.Count);

			Sailing.JX_ShowOnlyReceived = true;
			Sailing.JX_ShowOnlyThisSailing = false;
			AssertEquals("Expecting 3 packlines in the unallocated packlines view.", 3, Sailing.UnAllocatedPackLines.Count);

			Sailing.JX_ShowOnlyReceived = false;
			AssertEquals("Expecting 5 packlines in the unallocated packlines view.", 5, Sailing.UnAllocatedPackLines.Count);
		}

		public void TestAddPackLine()
		{
			AssertEquals("Expecting no packlines in the container.", 0, Container.PackLines.Count);

			Container.AddPackLine(Shipment1.OuterPackLines[0]);
			AssertEquals("Expected one packline to be in the container.", 1, Container.PackLines.Count);
			AssertEquals("Expecting packline in the container to be from shipment1.", Shipment1.PK, Container.PackLines[0].JL_JS);
		}

		public void TestRemovePackLine()
		{
			AssertEquals("Expecting no shipments in the container", 0, Container.PackLines.Count);

			Container.AddPackLine(Shipment1.OuterPackLines[0]);
			Container.AddPackLine(Shipment2.OuterPackLines[0]);
			AssertEquals("Expected two packlines to be in the container.", 2, Container.PackLines.Count);

			Container.RemovePackLine(Container.PackLines[0]);
			AssertEquals("Expected one packline to be in the container.", 1, Container.PackLines.Count);
		}

		public void TestAddPackLines()
		{
			Sailing.JX_ShowOnlyReceived = false;
			Sailing.JX_ShowOnlyThisSailing = false;
			AssertEquals("Expecting no PackLines in the container.", 0, Container.PackLines.Count);

			BusinessObject[] packLines = new BusinessObject[2];
			packLines[0] = Shipment1.OuterPackLines[0];
			packLines[1] = Shipment2.OuterPackLines[0];
			Container.AddPackLines(packLines);
			AssertEquals("Expecting two PackLines to be in the container.", 2, Container.PackLines.Count);
		}

		public void TestRemovePackLines()
		{
			AssertEquals("Expecting no PackLines in the container.", 0, Container.PackLines.Count);

			BusinessObject[] packLines = new BusinessObject[3];
			packLines[0] = Shipment1.OuterPackLines[0];
			packLines[1] = Shipment2.OuterPackLines[0];
			packLines[2] = Shipment3.OuterPackLines[0];
			Container.AddPackLines(packLines);
			AssertEquals("Expecting three PackLines to be in the container.", 3, Container.PackLines.Count);

			BusinessObject[] packLinesToBeRemoved = new BusinessObject[2];
			packLinesToBeRemoved[0] = Container.PackLines[0];
			packLinesToBeRemoved[1] = Container.PackLines[1];
			Container.RemovePackLines(packLinesToBeRemoved);
			AssertEquals("Expecting on ePackLine in the container.", 1, Container.PackLines.Count);
		}

		public void TestPacking()
		{
			CommonShipment s1 = Factory.New<CommonShipment>();
			s1.JS_JX = Sailing.PK;
			s1.JS_A_RCV = ZDateTime.Today;

			PackLine line1 = s1.OuterPackLines.AddNew();
			line1.JL_PackageCount = 4;
			line1.JL_F3_NKPackType = Constants.PkgUnit.Case;

			PackLine line2 = s1.OuterPackLines.AddNew();
			line2.JL_PackageCount = 2;
			line2.JL_F3_NKPackType = Constants.PkgUnit.Case;

			AssertEquals(2, s1.OuterPackLines.Count);
			AssertEquals(0, Container.PackLines.Count);

			Container.AddPackLine(line1);

			Sailing.Containers.QueryReJoinPackLines += delegate
			{ };

			AssertEquals(2, s1.OuterPackLines.Count);
			AssertEquals(1, Container.PackLines.Count);

			Container.AddPackLine(line2);
			AssertEquals(1, Container.PackLines.Count);
			AssertEquals(6, Container.PackLines[0].JL_PackageCount);
			AssertEquals(1, s1.OuterPackLines.Count);
		}

		[ExpectNoExceptions()]
		public void TestDeletingFirstSailingContainer()
		{
			JobSailing sailing = Factory.New<JobSailing>();
			CommonContainer container = sailing.Containers.AddNew();
			CommonContainer containerToDelete = sailing.Containers.AddNew();
			containerToDelete.Delete();
			AssertEquals("If a container is deleted, it should be removed from the sailing container collection", 1, sailing.Containers.Count);
		}

		public void TestValidatingPackedContainersDoesNotThrowExceptions()
		{
			CommonContainer sailingContainer = Factory.New<CommonContainer>();
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_OuterPacks = 10;
			JobSailing sailing = CreateExportSailing();
			shipment.JS_JX = sailing.PK;
			CommonContainer container = sailing.Containers.AddNew();
			AssertEquals("Container Collection on Pack line should have count of 0", 0, shipment.OuterPackLines[0].Containers.Count);
			container.AddPackLines(new BusinessObject[] { shipment.OuterPackLines[0] });
			AssertEquals("Pack Line Container Collection should container 1 container now", 1, shipment.OuterPackLines[0].Containers.Count);
			container.AddPackLine(shipment.OuterPackLines[0]);
			AssertNotNull("Sailing Container through the packlines", shipment.OuterPackLines[0].Containers[0]);
		}

		#region Implementation

		CommonShipment Shipment1;
		CommonShipment Shipment2;
		CommonShipment Shipment3;
		CommonShipment Shipment4;
		CommonShipment Shipment5;
		JobSailing Sailing;
		JobVoyage Voyage;
		VoyageDestination Destination;
		VoyageOrigin Origin;
		JobSailing Sailing2;
		JobVoyage Voyage2;
		VoyageDestination Destination2;
		VoyageOrigin Origin2;
		CommonContainer Container;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment1 = Factory.New(typeof(CommonShipment)) as CommonShipment;
			Shipment2 = Factory.New(typeof(CommonShipment)) as CommonShipment;
			Shipment3 = Factory.New(typeof(CommonShipment)) as CommonShipment;
			Shipment4 = Factory.New(typeof(CommonShipment)) as CommonShipment;
			Shipment5 = Factory.New(typeof(CommonShipment)) as CommonShipment;

			Voyage = Factory.New(typeof(JobVoyage)) as JobVoyage;
			Origin = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			Destination = Factory.New(typeof(VoyageDestination)) as VoyageDestination;

			Voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;

			Voyage.Origins.Add(Origin);
			Voyage.Destinations.Add(Destination);
			Origin.JA_RL_NKPortOfLoading = HomePort;
			Origin.JA_E_DEP = ZDateTime.Today;
			Destination.JB_RL_NKPortOfDischarge = OverseasPort3;

			Voyage.GenerateSailings();

			Sailing = Voyage.Sailings[0];
			Sailing.JX_IsPublished = true;

			Voyage2 = Factory.New(typeof(JobVoyage)) as JobVoyage;
			Origin2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			Destination2 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;

			Voyage2.JV_AirSeaRoad = Constants.TransportModes.Sea;

			Voyage2.Origins.Add(Origin2);
			Voyage2.Destinations.Add(Destination2);
			Origin2.JA_RL_NKPortOfLoading = HomePort;
			Origin2.JA_E_DEP = ZDateTime.Today.AddDays(1);
			Destination2.JB_RL_NKPortOfDischarge = OverseasPort3;

			Voyage2.GenerateSailings();

			Sailing2 = Voyage2.Sailings[0];
			Sailing2.JX_IsPublished = true;

			AssertEquals("Should be created and have 0", 0, Sailing.Containers.Count);
			Container = Sailing.AddSailingContainer();
			AssertEquals("Now should have 1", 1, Sailing.Containers.Count);
			Container = Sailing.Containers[0];

			Shipment1.JS_IsBooking = true;
			Shipment1.JS_IsForwardRegistered = false;
			Shipment1.JS_JX = Sailing.PK;
			Shipment1.JS_A_RCV = ZDateTime.Today;
			Shipment1.JS_OuterPacks = 20;
			Shipment1.JS_ActualVolume = new ZDecimal(4.0);
			Shipment1.JS_ActualWeight = new ZDecimal(2000.0);

			Shipment2.JS_IsBooking = true;
			Shipment2.JS_IsForwardRegistered = false;
			Shipment2.JS_JX = Sailing.PK;
			Shipment2.JS_A_RCV = ZDateTime.Today;
			Shipment2.JS_OuterPacks = 10;
			Shipment2.JS_ActualVolume = new ZDecimal(4.0);
			Shipment2.JS_ActualWeight = new ZDecimal(2000.0);

			Shipment3.JS_IsBooking = true;
			Shipment3.JS_IsForwardRegistered = false;
			Shipment3.JS_JX = Sailing.PK;
			Shipment3.JS_OuterPacks = 40;
			Shipment3.JS_ActualVolume = new ZDecimal(4.0);
			Shipment3.JS_ActualWeight = new ZDecimal(2000.0);

			Shipment4.JS_IsBooking = true;
			Shipment4.JS_IsForwardRegistered = false;
			Shipment4.JS_JX = Sailing2.PK;
			Shipment4.JS_A_RCV = ZDateTime.Today;
			Shipment4.JS_OuterPacks = 80;
			Shipment4.JS_ActualVolume = new ZDecimal(4.0);
			Shipment4.JS_ActualWeight = new ZDecimal(2000.0);

			Shipment5.JS_IsBooking = true;
			Shipment5.JS_IsForwardRegistered = false;
			Shipment5.JS_JX = Sailing2.PK;
			Shipment5.JS_OuterPacks = 20;
			Shipment5.JS_ActualVolume = new ZDecimal(4.0);
			Shipment5.JS_ActualWeight = new ZDecimal(2000.0);

			Factory.Save();
		}

		JobSailing CreateExportSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = HomePort;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = OverseasPort;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#endregion
	}
}
