using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerCollectionTest : BaseFreightTest
	{
		public void TestDataRefreshBusOnAdded()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			CommonShipment shipment = CommonShipment.New(Factory);
			PackLine packLine = shipment.OuterPackLines.AddNew();
			CommonConsol consol = shipment.Consols.AddNew();
			CommonContainer container1 = consol.Containers.AddNew();
			AssertEquals("Consol should have container", 1, consol.Containers.Count);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ChildEditableService.SetState(newFactory, ChildEditableServiceStates.Consol);

			CommonConsol consol_NewFactory = newFactory.Load<CommonConsol>(consol.PK);
			CommonContainer container_NewFactory = consol_NewFactory.Containers.AddNew();
			container_NewFactory.JC_ContainerNum = "C1";

			newFactory.Save();

			AssertEquals("Consol should have 2 containers", 2, consol.Containers.Count);
		}

		public void TestOnAdded()
		{
			var shipment = CommonShipment.New(Factory);

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			packLine1.JL_PackageCount = 1;
			packLine2.JL_PackageCount = 2;

			shipment.Consols.Add(Consol);

			var container1 = Consol.Containers.AddNew();

			AssertEquals("PackLine1 assigned Container", container1, packLine1.GetContainer(Consol));
			AssertEquals("PackLine2 assigned Container", container1, packLine2.GetContainer(Consol));
		}

		public void TestOnAddedSetsNRQ()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			var consignor1 = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor1.PK;
			shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			AssertNotNull("Precondition: Shipment Consignor", shipment.Consignor);
			AssertEquals("Precondition: Shipment is direct shipment", true, shipment.IsDirectShipment);
			AssertEquals("Precondition: VGM set to true", true, shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight);

			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeight = 50;
			consol.Containers.Add(container);

			AssertEquals("Container verification defaults to NRQ", Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, container.JC_GrossWeightVerificationType);
		}

		public void TestOnAddedSetsNON()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			var consignor1 = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor1.PK;
			shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			AssertNotNull("Precondition: Shipment Consignor", shipment.Consignor);
			AssertEquals("Precondition: Shipment is direct shipment", true, shipment.IsDirectShipment);
			AssertEquals("Precondition: VGM set to true", true, shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight);

			var container = Factory.New<CommonContainer>();
			container.JC_GrossWeight = 0;
			consol.Containers.Add(container);

			AssertEquals("Container verification defaults to NON", Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, container.JC_GrossWeightVerificationType);
		}

		public void TestDefaultsOnNewChild()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Consol.JK_RL_NKLoadPort = HomePort;
			Consol.JK_RL_NKDischargePort = OverseasPort;

			Transport transport = Consol.Transports[0];

			transport.JW_Vessel = "SOUTHERN CROSS MARU";
			transport.JW_VoyageFlight = "23";
			transport.JW_ETD = new ZDateTime(2004, 09, 15);
			transport.JW_ETA = new ZDateTime(2004, 09, 23);

			AssertNotNull("JobConsol Schedule", transport.Sailing);
			Assert("Pre-Condition: Sailing is not empty", !transport.JW_JX.IsEmpty);

			Consol.JK_OA_SendingForwarderAddress = (Factory.New<TestLocalSendingForwarder>()).MainAddress.PK;
			CommonContainer container1 = Consol.Containers.AddNew();

			AssertEquals("Sailing should default to Sailing on Consol", transport.JW_JX, container1.Sailing.PK);
			AssertEquals("Container's own sailing reference should be empty.", true, container1.JC_JX.IsEmpty);
			AssertEquals("Client should default from Consol", Consol.SendingForwarderPK, container1.JC_OH_CFSClient);
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", true, container1.JC_TrainWagonNumberInfo.ReadOnly);
			AssertEquals("Delivery mode defaults", container1.JC_DeliveryMode, Constants.DeliveryModes.Codes.CY_CY);

			Consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			CommonContainer container2 = Consol.Containers.AddNew();
			AssertEquals("Delivery mode defaults", container2.JC_DeliveryMode, Constants.DeliveryModes.Codes.CFS_CFS);

			Consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			CommonContainer container3 = Consol.Containers.AddNew();
			AssertEquals("Delivery mode defaults", container3.JC_DeliveryMode, Constants.DeliveryModes.Codes.CFS_CY);

			Consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			CommonContainer container4 = Consol.Containers.AddNew();
			AssertEquals("Delivery mode defaults", container4.JC_DeliveryMode, Constants.DeliveryModes.Codes.CFS_CFS);

			Consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;
			CommonContainer container5 = Consol.Containers.AddNew();
			AssertEquals("Delivery mode defaults", container5.JC_DeliveryMode, Constants.DeliveryModes.Codes.CY_CFS);

			Consol.JK_ConsolMode = "";
			CommonContainer container6 = Consol.Containers.AddNew();
			AssertEquals("Delivery mode defaults", container6.JC_DeliveryMode, "");

			Consol.JK_TransportMode = Constants.TransportModes.Rail;
			CommonContainer containerRail = Consol.Containers.AddNew();
			AssertEquals("JC_TrainWagonNumberInfo.ReadOnly", false, containerRail.JC_TrainWagonNumberInfo.ReadOnly);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;
			CommonContainer container7 = Consol.Containers.AddNew();
			AssertEquals("Container mode defaults", container7.JC_ContainerMode, Constants.ContainerModes.ShippersConsol);

			Consol.JK_TransportMode = Constants.TransportModes.Road;
			Consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;
			CommonContainer container8 = Consol.Containers.AddNew();
			AssertEquals("Container mode defaults", container8.JC_ContainerMode, Constants.ContainerModes.ShippersConsol);

			Consol.JK_TransportMode = Constants.TransportModes.Rail;
			Consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;
			CommonContainer container9 = Consol.Containers.AddNew();
			AssertEquals("Container mode defaults", container9.JC_ContainerMode, Constants.ContainerModes.ShippersConsol);
		}

		public void TestHasContainer()
		{
			string containerNumber = "CRXU1234567";
			Assert(!Containers.HasContainer(containerNumber));
			CommonContainer container = Containers.AddNew();
			container.JC_ContainerNum = containerNumber;
			Assert(Containers.HasContainer(containerNumber));
		}

		public void TestOnAdded_ShipmentWithEmptyConsignorPickupAddress()
		{
			var container = Factory.New<CommonContainer>();
			var shipment = Factory.New<CommonShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var org = Factory.New<OrgHeader>();
			var address = shipment.ConsignorDocumentaryAddress;
			address.OrganisationPK = org.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;

			AssertNotNull("Precondition: Consignor should not be null", shipment.Consignor);
			AssertEquals("Precondition: shipment is direct", true, shipment.IsDirectShipment);
			AssertNull("Precondition: ConsignorPickupAddress.Address is null", shipment.ConsignorPickupAddress.Address);

			AssertNoExceptionThrown(() => consol.Containers.Add(container));
		}

		public void TestRORContainerMode()
		{
			Consol.JK_ConsolMode = Constants.ContainerModes.RollOnRollOff;
			CommonContainer container = Containers.AddNew();
			Assert(container.JC_ContainerMode == Constants.ContainerModes.RollOnRollOff);
		}

		public void TestFindAnyByContainerNumber()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainerCollection collection = new CommonContainerCollection(consol, Factory);
			CommonContainer container1 = collection.AddNew();
			AssertEquals(null, collection.FindAnyByContainerNumber("XX"));
			container1.JC_ContainerNum = "XX";
			AssertEquals(container1, collection.FindAnyByContainerNumber("XX"));
		}

		public void TestAllowNew()
		{
			AssertEquals("Allow new should be false.", false, Sailing.Containers.AllowNew);
		}

		public void TestSetDefaultsForNewChild()
		{
			CommonContainer container = Sailing.Containers.AddNew();
			AssertEquals("ContainerMode should be GRP.", Constants.ContainerModes.Groupage, container.JC_ContainerMode);

			Sailing.Voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			CommonContainer container2 = Sailing.Containers.AddNew();
			AssertEquals("ContainerMode should be ULD.", Constants.ContainerModes.ULD, container2.JC_ContainerMode);
		}

		public void TestSetDefaultContainerModeForAirConsol()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			var container = Containers.AddNew();
			AssertEquals("ContainerMode should be ULD.", Constants.ContainerModes.ULD, container.JC_ContainerMode);
		}

		public void TestSuppressingAutomaticallyUpdatePackLineContainers()
		{
			FreightConfigurationRegistry.Instance.AutoPackFreightContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			var shipment = Factory.New<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;

			consol.Shipments.Add(shipment);
			AssertEquals("Pack Line is allocated", true, container.PackLines.Contains(packLine));

			FreightConfigurationRegistry.Instance.AutoPackFreightContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var consol1 = Factory.New<CommonConsol>();
			var container1 = consol1.Containers.AddNew();
			var shipment1 = Factory.New<CommonShipment>();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;

			consol1.Shipments.Add(shipment1);
			AssertEquals("Registry.Auto-packing Containers was set to false so container should not be packed", false, container1.PackLines.Contains(packLine1));
		}

		#region Implementation

		JobSailing fSailing;
		JobSailing Sailing
		{
			get
			{
				if (fSailing == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew();
					voyage.Origins[0].JA_RL_NKPortOfLoading = "AUSYD";
					voyage.Origins[0].JA_E_DEP = ZDateTime.Today;

					voyage.Destinations.AddNew();
					voyage.Destinations[0].JB_RL_NKPortOfDischarge = "HKHKG";
					voyage.Destinations[0].JB_E_ARV = ZDateTime.Today.AddDays(9);

					fSailing = voyage.Sailings[0];
				}
				return fSailing;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Consol = Factory.New<CommonConsol>();
			Consol.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport = Consol.Transports[0];
			transport.JW_JX = new SailingsForTestClasses(Factory).SydLaxSailing.PK;
			Containers = new CommonContainerCollection(Consol, Factory);
		}

		CommonConsol Consol;
		CommonContainerCollection Containers;

		#endregion
	}
}
