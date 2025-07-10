using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentArrivalDepartureTest : BaseFreightTest
	{
		#region TestArrivalConsol

		public void TestArrivalConsol()
		{
			AssertEquals(ArrivalConsol, Shipment.ArrivalConsol);
		}

		#endregion

		#region TestDepartureConsol

		public void TestDepartureConsol()
		{
			AssertEquals(DepartureConsol, Shipment.DepartureConsol);
		}

		#endregion

		#region TestMostInterestingDepartureConsol

		public void TestMostInterestingDepartureConsol()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USNYC";

			CommonConsol consolA = shipment.Consols.AddNew();
			consolA.JK_TransportMode = Constants.TransportModes.Road;
			consolA.JK_RL_NKLoadPort = "AUSYD";
			consolA.JK_RL_NKDischargePort = "AUBNE";

			CommonConsol consolB = shipment.Consols.AddNew();
			consolB.JK_TransportMode = Constants.TransportModes.Air;
			consolB.JK_RL_NKLoadPort = "AUBNE";
			consolB.JK_RL_NKDischargePort = "USNYC";

			AssertEquals(shipment.MostInterestingDepartureConsol, consolB);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals(shipment.MostInterestingDepartureConsol, consolA);

			CommonConsol consolC = shipment.Consols.AddNew();
			consolC.JK_TransportMode = Constants.TransportModes.Road;
			consolC.JK_RL_NKLoadPort = "NZAKL";
			consolC.JK_RL_NKDischargePort = "NZZQN";

			shipment.Consols.Remove(consolA);

			AssertEquals(shipment.MostInterestingDepartureConsol, consolB);

			shipment.Consols.Add(consolA);

			AssertEquals(shipment.MostInterestingDepartureConsol, consolA);

			shipment.JS_TransportMode = Constants.TransportModes.Mail;
			Assert(shipment.MostInterestingDepartureConsol == consolA || shipment.MostInterestingDepartureConsol == consolB);

			shipment.JS_TransportMode = Constants.TransportModes.AirSea;
			AssertEquals(shipment.MostInterestingDepartureConsol, consolB);

			CommonConsol consolD = shipment.Consols.AddNew();
			consolD.JK_TransportMode = Constants.TransportModes.Sea;
			consolD.JK_RL_NKLoadPort = "AUBNE";
			consolD.JK_RL_NKDischargePort = "NZAKL";

			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			AssertEquals(shipment.MostInterestingDepartureConsol, consolD);
		}

		#endregion

		#region TestCurrentBranchAirDepartureConsol

		[ExpectNoExceptions]
		public void TestCurrentBranchAirDepartureConsol()
		{
			AssertEquals("Prerequisite", "AU", GlbBranch.CurrentBranch.Country.Code);

			var shipment = Factory.New<CommonShipment>();

			AssertEquals(null, shipment.CurrentBranchDepartureAirConsol);

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "SGSIN";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			AssertEquals(null, shipment.CurrentBranchDepartureAirConsol);

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "USCHI";

			AssertEquals(consol2, shipment.CurrentBranchDepartureAirConsol);

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol3.JK_RL_NKLoadPort = "USCHI";
			consol3.JK_RL_NKDischargePort = "USLAX";

			AssertEquals(null, shipment.CurrentBranchDepartureAirConsol);

			var consol4 = shipment.Consols.AddNew();
			consol4.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol4.JK_RL_NKLoadPort = "AUSYD";
			consol4.JK_RL_NKDischargePort = "NZAKL";

			AssertEquals(consol4, shipment.CurrentBranchDepartureAirConsol);

			RefUNLOCO newUNLOCO = Factory.New<RefUNLOCO>();
			newUNLOCO.RL_Code = "AU";
			newUNLOCO.RL_PortName = "Country-less UNLOCO";
			newUNLOCO.RL_RN_NKCountryCode = ZString.Empty;
			Factory.Save();

			consol4.JK_RL_NKLoadPort = "AU";

			AssertNull("Since the consol has a load port without a valid country we expect not to match to any consol nor throw an exception", shipment.CurrentBranchDepartureAirConsol);
		}

		#endregion

		#region AddDepartureContainersToTestTheyDontGetIntoTheArrivalContainersList

		void AddDepartureContainersToTestTheyDontGetIntoTheArrivalContainersList()
		{
			Shipment.OuterPackLines.CurrentConsol = DepartureConsol;
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerBeta);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerAlpha);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerAlpha);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerBeta);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerGamma);
		}

		#endregion

		#region AddArrivalContainersToTestTheyDontGetIntoTheDepartureContainersList

		void AddArrivalContainersToTestTheyDontGetIntoTheDepartureContainersList()
		{
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerBeta);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerAlpha);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerAlpha);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerBeta);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerGamma);
		}

		#endregion

		#region TestArrivalContainersAll

		public void TestArrivalContainersAll()
		{
			AssertEquals("Pre-condition: Expecting CommonShipment to have no arrival containers.", 0, Shipment.ArrivalContainers.Count);
			AddDepartureContainersToTestTheyDontGetIntoTheArrivalContainersList();
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, null);

			CommonContainerCollection containers = Shipment.ArrivalContainers;
			AssertEquals("Count", 0, containers.Count);
		}

		#endregion

		#region TestArrivalContainersNone

		public void TestArrivalContainersNone()
		{
			AddDepartureContainersToTestTheyDontGetIntoTheArrivalContainersList();
			CommonContainerCollection containers = Shipment.ArrivalContainers;
			AssertEquals("Count", 0, containers.Count);
		}

		#endregion

		#region TestArrivalContainersSpecific

		public void TestArrivalContainersSpecific()
		{
			AddDepartureContainersToTestTheyDontGetIntoTheArrivalContainersList();
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerBeta);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerGamma);

			CommonContainerCollection containers = Shipment.ArrivalContainers;
			AssertEquals("Count", 2, containers.Count);
			AssertEquals("Beta", ArrivalContainerBeta, containers[0]);
			AssertEquals("Gamma", ArrivalContainerGamma, containers[1]);
		}

		#endregion

		#region TestArrivalContainersDoNotDuplicate

		public void TestArrivalContainersDoNotDuplicate()
		{
			AddDepartureContainersToTestTheyDontGetIntoTheArrivalContainersList();
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerBeta);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerGamma);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, ArrivalContainerGamma);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(ArrivalConsol, null);

			CommonContainerCollection containers = Shipment.ArrivalContainers;
			AssertEquals("Count", 2, containers.Count);
			AssertEquals("Beta", ArrivalContainerBeta, containers[0]);
			AssertEquals("Gamma", ArrivalContainerGamma, containers[1]);
		}

		#endregion

		#region TestDepartureContainersAll

		public void TestDepartureContainersAll()
		{
			AddArrivalContainersToTestTheyDontGetIntoTheDepartureContainersList();
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, null);

			CommonContainerCollection containers = Shipment.DepartureContainers;
			AssertEquals("Count", 0, containers.Count);
		}

		#endregion

		#region TestDepartureContainersNone

		public void TestDepartureContainersNone()
		{
			AddArrivalContainersToTestTheyDontGetIntoTheDepartureContainersList();
			CommonContainerCollection containers = Shipment.DepartureContainers;
			AssertEquals("Count", 0, containers.Count);
		}

		#endregion

		#region TestDepartureContainersSpecific

		public void TestDepartureContainersSpecific()
		{
			AddArrivalContainersToTestTheyDontGetIntoTheDepartureContainersList();
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerBeta);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerGamma);

			CommonContainerCollection containers = Shipment.DepartureContainers;
			AssertEquals("Count", 2, containers.Count);
			AssertEquals("Beta", DepartureContainerBeta, containers[0]);
			AssertEquals("Gamma", DepartureContainerGamma, containers[1]);
		}

		#endregion

		#region TestDepartureContainersDoNotDuplicate

		public void TestDepartureContainersDoNotDuplicate()
		{
			AddArrivalContainersToTestTheyDontGetIntoTheDepartureContainersList();
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerBeta);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerGamma);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, DepartureContainerGamma);
			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(DepartureConsol, null);

			CommonContainerCollection containers = Shipment.DepartureContainers;
			AssertEquals("Count", 2, containers.Count);
			AssertEquals("Beta", DepartureContainerBeta, containers[0]);
			AssertEquals("Gamma", DepartureContainerGamma, containers[1]);
		}

		#endregion

		#region TestBusinessObjectsWithRelatedEvents

		public void TestBusinessObjectsWithRelatedEvents()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			AssertEquals("Must see the JobDocsAndCartage logs", true, ((IList)shipment.BusinessObjectsWithRelatedEvents).Contains(shipment.DocsAndCartage));
		}

		#endregion

		#region TestSaving_ChangeConsolToCFSNoException

		public void TestSaving_ChangeConsolToCFSNoException_SetIsCFSBeforeSaveAttachedConsol()
		{
			var factory1 = new BusinessObjectFactory();
			var shipment = factory1.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = true;

			var factory2 = new BusinessObjectFactory();
			var consol = factory2.NewWithValidTestData<CommonConsol>();
			consol.JK_IsForwarding = true;
			factory2.Save();

			var consolInAnotherFactory = factory1.Load<CommonConsol>(consol.PK);
			shipment.Consols.Add(consolInAnotherFactory);

			consol.JK_IsCFS = true;
			factory2.Save();

			Assert("Not expecting Shipment to be CFS Registered.", !shipment.JS_IsCFSRegistered);
			Assert("Expecting Consol to be CFS Registered.", consol.JK_IsCFS);
			AssertNoExceptionThrown("Expect not throw exception when factory is saved", factory1.Save);
			Assert("Expecting Shipment to be CFS Registered.", shipment.JS_IsCFSRegistered);
		}

		public void TestSaving_ChangeConsolToCFSNoException_SetIsCFSAfterSaveAttachedConsol()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var shipment = factory1.NewWithValidTestData<CommonShipment>();
			shipment.JS_IsForwardRegistered = true;

			var consol = factory1.NewWithValidTestData<CommonConsol>();
			consol.JK_IsForwarding = true;
			factory1.Save();

			var shipmentInAnotherFactory = factory2.Load<CommonShipment>(shipment.PK);
			var consolInAnotherFactory = factory2.Load<CommonConsol>(consol.PK);

			shipment.Consols.Add(consol);

			consolInAnotherFactory.JK_IsCFS = true;

			factory1.Save();
			factory2.Save();

			Assert("Not expecting Shipment to be CFS Registered.", !shipment.JS_IsCFSRegistered);
			Assert("Expecting Consol to be CFS Registered.", consol.JK_IsCFS);

			shipment.JS_ActualVolume = 111;
			AssertNoExceptionThrown("Expect not throw exception when factory is saved", factory1.Save);
			Assert("Expecting Shipment to be CFS Registered.", shipment.JS_IsCFSRegistered);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			Shipment = CommonShipment.New(Factory);
			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.JS_RL_NKOrigin = "USNYC";

			ArrivalConsol = Shipment.Consols.AddNew();
			ArrivalConsol.JK_RL_NKDischargePort = "AUMEL";
			ArrivalConsol.JK_RL_NKLoadPort = "BOGUS";

			// Add them in the non-alphabetical order to test they get sorted
			ArrivalContainerBeta = ArrivalConsol.Containers.AddNew();
			ArrivalContainerBeta.JC_ContainerNum = "ArvBeta";
			ArrivalContainerAlpha = ArrivalConsol.Containers.AddNew();
			ArrivalContainerAlpha.JC_ContainerNum = "ArvAlpha";
			ArrivalContainerGamma = ArrivalConsol.Containers.AddNew();
			ArrivalContainerGamma.JC_ContainerNum = "ArvGamma";

			DepartureConsol = Shipment.Consols.AddNew();
			DepartureConsol.JK_RL_NKDischargePort = "BOGUS";
			DepartureConsol.JK_RL_NKLoadPort = "USLAX";

			// Add them in non-alphabetical order to test they get sorted
			DepartureContainerAlpha = DepartureConsol.Containers.AddNew();
			DepartureContainerAlpha.JC_ContainerNum = "DepAlpha";
			DepartureContainerBeta = DepartureConsol.Containers.AddNew();
			DepartureContainerBeta.JC_ContainerNum = "DepBeta";
			DepartureContainerGamma = DepartureConsol.Containers.AddNew();
			DepartureContainerGamma.JC_ContainerNum = "DepGamma";

			Shipment.OuterPackLines.RemoveAll(); // Get rid of automatically added pack lines - we will configure our own for unit test purposes
		}

		CommonShipment Shipment;
		CommonConsol ArrivalConsol, DepartureConsol;
		CommonContainer ArrivalContainerAlpha, ArrivalContainerBeta, ArrivalContainerGamma, DepartureContainerAlpha, DepartureContainerBeta, DepartureContainerGamma;

		#endregion
	}
}
