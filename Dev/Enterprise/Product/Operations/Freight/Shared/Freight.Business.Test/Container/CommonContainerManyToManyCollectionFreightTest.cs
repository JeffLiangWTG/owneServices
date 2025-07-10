using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerManyToManyCollectionFreightTest : BaseFreightTest
	{
		public void TestAddingAnInnerPackLineToAContainer()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packLine = shipment.InnerPackLines.AddNew();
			CommonContainer container = Factory.New<CommonContainer>();

			packLine.Containers.Add(container);
			AssertEquals("Should not have added the container", false, packLine.Containers.Contains(container));
		}

		public void TestPacklineAddedToTwoContainers()
		{
			CommonConsol consol = GetExportConsol(typeof(CommonConsol));
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "3";
			transport.JW_ETA = ZDateTime.Today.AddDays(10);
			transport.JW_ETD = ZDateTime.Today;

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "1111";
			container1.JC_RC = RC_20GP_PK;
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "2222";
			container2.JC_RC = RC_20GP_PK;

			Factory.Save();

			BusinessObjectFactory shipmentFactory = new BusinessObjectFactory();

			CommonShipment shipment = CommonShipment.New(shipmentFactory);
			CommonConsol consolInShipmentFactory = shipmentFactory.Load<CommonConsol>(consol.PK);
			shipment.Consols.Add(consolInShipmentFactory);

			AssertEquals(0, shipment.OuterPackLines.Count);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consignor = shipmentFactory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			shipment.ConsignorPK = consignor.PK;

			shipmentFactory.Save();

			AssertEquals(1, consol.Shipments.Count);

			AssertEquals(0, container1.PackLines.Count);
			AssertEquals(0, container2.PackLines.Count);

			PackLine line = shipment.OuterPackLines.AddNew();
			line.CurrentConsol = consolInShipmentFactory;

			AssertEquals(1, line.Containers.Count);
			AssertEquals(1, shipment.Containers.Count());

			line.Containers.Load();

			AssertEquals(1, line.Containers.Count);
			AssertEquals(1, shipment.Containers.Count());

			shipmentFactory.Save();

			int sfactorycount = 0;
			foreach (BusinessObject bO in ((IBusinessObjectFactoryInternals)shipmentFactory).AllBusinessObjects)
			{
				if (bO is JobContainerPackPivot)
				{
					sfactorycount++;
				}
			}

			AssertEquals(1, sfactorycount);

			int cfactorycount = 0;
			foreach (BusinessObject bO in ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects)
			{
				if (bO is JobContainerPackPivot)
				{
					cfactorycount++;
				}
			}

			AssertEquals(1, cfactorycount);

			line.Containers.Load();
			AssertEquals(1, line.Containers.Count);
			AssertEquals(1, consol.Shipments[0].OuterPackLines[0].Containers.Count);
			AssertEquals(1, container1.PackLines.Count + container2.PackLines.Count);
		}

		public void TestPackLineCurrentConsolResetsWhenConsolRemovedFromShipment()
		{
			var consol1 = GetExportConsol(typeof(CommonConsol));
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";
			container1.JC_RC = RC_20GP_PK;

			var consol2 = GetExportConsol(typeof(CommonConsol));
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "Container2";
			container2.JC_RC = RC_20GP_PK;

			Factory.Save();

			var shipment = CommonShipment.New(Factory);
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 1;
			packline.CurrentConsol = consol1;
			shipment.Consols.Add(consol1);
			Assert(packline.Containers.Contains(container1));

			shipment.Consols.Add(consol2);
			shipment.Consols.Remove(consol1);

			AssertEquals(consol2.PK, packline.CurrentConsol.PK);
		}

		public void TestContainersAvailableResetWhenConsolIsRemovedFromShipment()
		{
			var consol1 = GetExportConsol(typeof(CommonConsol));
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";
			container1.JC_RC = RC_20GP_PK;

			var consol2 = GetExportConsol(typeof(CommonConsol));
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "Container2";
			container2.JC_RC = RC_20GP_PK;

			Factory.Save();

			var shipment = CommonShipment.New(Factory);
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 1;
			packline.CurrentConsol = consol1;
			shipment.Consols.Add(consol1);
			Assert(packline.Containers.Contains(container1));

			shipment.Consols.Add(consol2);
			shipment.Consols.Remove(consol1);

			AssertEquals(1, packline.AllContainersOnShipment_List.Count);
			AssertEquals(container2.PK, ((CommonContainer)packline.AllContainersOnShipment_List[0]).PK);
		}

		public void TestPackLineContainersResetWhenConsolRemovedFromShipment()
		{
			var consol1 = GetExportConsol(typeof(CommonConsol));
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";
			container1.JC_RC = RC_20GP_PK;

			var consol2 = GetExportConsol(typeof(CommonConsol));
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "Container2";
			container2.JC_RC = RC_20GP_PK;

			Factory.Save();

			var shipment = CommonShipment.New(Factory);
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 1;

			Factory.Save();

			packline.CurrentConsol = consol1;
			shipment.Consols.Add(consol1);
			Assert(packline.Containers.Contains(container1));

			shipment.Consols.Add(consol2);
			shipment.Consols.Remove(consol1);

			AssertEquals(1, packline.Containers.Count);
			AssertEquals(1, container2.PackLines.Count);
			AssertEquals(0, container1.PackLines.Count);
			Assert(packline.Containers.Contains(container2));
		}

		public void TestUpdateByDataRefresh_RemoveContainersFromTheSameConsol()
		{
			var consol1 = Factory.New<CommonConsol>();

			var containerA = consol1.Containers.AddNew();
			containerA.JC_ContainerNum = "AAA";
			containerA.JC_RC = RC_20GP_PK;

			var containerB = consol1.Containers.AddNew();
			containerB.JC_ContainerNum = "BBB";
			containerB.JC_RC = RC_20GP_PK;

			var shipment = Factory.New<CommonShipment>();
			shipment.Consols.Add(consol1);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(containerA.PK);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { containerA }, packLine.Containers);

			var anotherFactory = new BusinessObjectFactory();
			var packLineReloaded = anotherFactory.Load<PackLine>(packLine.PK);
			var containerBReloaded = anotherFactory.Load<CommonContainer>(containerB.PK);

			packLineReloaded.Containers.Add(containerBReloaded);

			anotherFactory.Save();

			AssertContainsExactElementsInAnyOrder("Container from the same consol was removed", new[] { containerB }, packLine.Containers);
			AssertContainsExactElementsInAnyOrder("Container A packs no packlines", System.Array.Empty<PackLine>(), containerA.PackLines);
			AssertContainsExactElementsInAnyOrder("Container B packs one packline", new[] { packLine }, containerB.PackLines);
		}
	}
}
