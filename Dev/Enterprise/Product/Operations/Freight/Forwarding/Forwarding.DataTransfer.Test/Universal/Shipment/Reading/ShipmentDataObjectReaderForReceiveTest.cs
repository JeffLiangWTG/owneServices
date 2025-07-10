using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.DataTransfer.Testing.ShipmentDataObjectReaderTest;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentDataObjectReaderForReceiveTest : ShipmentDataObjectReadingHelperTest
	{
		[TestDate(2021, 1, 1)]
		public void TestRemoveContainerWhileSplitPackLineForFirstTime()
		{
			var inx = 0;
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var protypeShipment = GetShipmentProtype(warehouseAddressBO);
				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);
					Factory.SaveForTesting();

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
						Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1))));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When PackLine packline1 has no container and CW1 receives all packages without container, all packages should be assigned to packline1", () =>
					{
						AssertEquals(shipment.PK, newestShipment.PK);
						AssertEquals(1, newestShipment.OuterPackLines.Count);
						AssertEquals(ZString.Empty, newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
						AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
						AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2", "PKG3" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
					});
				}

				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } };
					shipmentDataObject.SetContainerCollection(() => containers);

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
						Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1, 1))));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When PackLine packline1 has no container and CW1 receives all packages with Container CONT0001, all packages should be assigned to packline1 and container of packline1 should be CONT0001", () =>
					{
						AssertEquals(shipment.PK, newestShipment.PK);
						AssertEquals(1, newestShipment.OuterPackLines.Count);
						AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
						AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
						AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2", "PKG3" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
					});
				}

				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" } };
					shipmentDataObject.SetContainerCollection(() => containers);

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1, 1),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG2", shipment.OuterPackLines[0].JL_PackLineId, 2, 1),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG3", shipment.OuterPackLines[0].JL_PackLineId, 3, 2) }));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When PackLine packline1 has no container and CW1 receives Packages(PKG1 and PKG2 with CONT0001 and PKG3 with CONT0002)." +
						" PKG1 and PKG2 should be assigned to packline1 and container of packline1 should be CONT0001, PKG3 would be assigned to the splitted packline whose container should be CONT0002", () =>
						{
							AssertEquals(shipment.PK, newestShipment.PK);
							AssertEquals(2, newestShipment.OuterPackLines.Count);
							AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
							AssertEquals("CONT0002", newestShipment.OuterPackLines[1].JL_Calc_ContainerNumber);
							AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
							AssertEquals("CNF", newestShipment.OuterPackLines[1].JL_OriginTransitWarehouseStatus);
							AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
							AssertArrayEqualsByElements(new ZString[] { "PKG3" }, newestShipment.OuterPackLines[1].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
						});
				}

				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

					var container = shipment.Consols[0].Containers.AddNew();
					container.JC_ContainerNum = "CONT0001";
					shipment.OuterPackLines[0].SetContainer(container.Consol, container);

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
						Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1))));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When Container CONT0001 is assigned to PackLine packline1 and CW1 receives all packages without container, all packages should be assigned to packline1 and container of packline1 should be empty", () =>
					{
						AssertEquals(shipment.PK, newestShipment.PK);
						AssertEquals(1, newestShipment.OuterPackLines.Count);
						AssertEquals(ZString.Empty, newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
						AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
						AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2", "PKG3" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
					});
				}

				using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

					var container = shipment.Consols[0].Containers.AddNew();
					container.JC_ContainerNum = "CONT0001";
					shipment.OuterPackLines[0].SetContainer(container.Consol, container);

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } };
					shipmentDataObject.SetContainerCollection(() => containers);

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
						Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1, 1))));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When Container CONT0001 is assigned to PackLine packline1 and CW1 receives all packages with container CONT0001, all packages should be assigned to packline1 and container of packline1 should have no change", () =>
					{
						AssertEquals(shipment.PK, newestShipment.PK);
						AssertEquals(1, newestShipment.OuterPackLines.Count);
						AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
						AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
						AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2", "PKG3" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
					});
				}

				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

					var container = shipment.Consols[0].Containers.AddNew();
					container.JC_ContainerNum = "CONT0001";
					shipment.OuterPackLines[0].SetContainer(container.Consol, container);

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0002" } };
					shipmentDataObject.SetContainerCollection(() => containers);

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
						Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1, 1))));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When Container CONT0001 is assigned to PackLine packline1 and CW1 receives all packages with container CONT0002, all packages should be assigned to packline1 and container of packline1 should be CONT0002", () =>
					{
						AssertEquals(shipment.PK, newestShipment.PK);
						AssertEquals(1, newestShipment.OuterPackLines.Count);
						AssertEquals("CONT0002", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
						AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
						AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2", "PKG3" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
					});
				}

				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

					var container = shipment.Consols[0].Containers.AddNew();
					container.JC_ContainerNum = "CONT0001";
					shipment.OuterPackLines[0].SetContainer(container.Consol, container);

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" } };
					shipmentDataObject.SetContainerCollection(() => containers);

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1, 1),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG2", shipment.OuterPackLines[0].JL_PackLineId, 2, 1),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG3", shipment.OuterPackLines[0].JL_PackLineId, 3, 2) }));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When Container CONT0001 is assigned to PackLine packline1 and CW1 receives Packages, PKG1,PKG2 with CONT0001 and PKG3 with CONT0002." +
						" PKG1,PKG2 should be assigned to packline1 and container of packline1 should have no change, PKG3 would be assigned to the splitted packline whose container should be CONT0002", () =>
						{
							AssertEquals(shipment.PK, newestShipment.PK);
							AssertEquals(2, newestShipment.OuterPackLines.Count);
							AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
							AssertEquals("CONT0002", newestShipment.OuterPackLines[1].JL_Calc_ContainerNumber);
							AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
							AssertEquals("CNF", newestShipment.OuterPackLines[1].JL_OriginTransitWarehouseStatus);
							AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
							AssertArrayEqualsByElements(new ZString[] { "PKG3" }, newestShipment.OuterPackLines[1].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
						});
				}

				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

					var container = shipment.Consols[0].Containers.AddNew();
					container.JC_ContainerNum = "CONT0001";
					shipment.OuterPackLines[0].SetContainer(container.Consol, container);

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } });

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1, 1),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG2", shipment.OuterPackLines[0].JL_PackLineId, 2, 1) }));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
					Factory.SaveForTesting();

					shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" } });
					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1, 1),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG2", shipment.OuterPackLines[0].JL_PackLineId, 2, 1),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG3", shipment.OuterPackLines[0].JL_PackLineId, 3, 2) }));

					newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When Container CONT0001 is assigned to PackLine packline1 and CW1 receives Packages PKG1,PKG2 with CONT0001 firstly and PKG3 with CONT0002 later." +
						" PKG1,PKG2 should be assigned to packline1 and container of packline1 should have no change, PKG3 would be assigned to splitted packline with container CONT0002", () =>
						{
							AssertEquals(shipment.PK, newestShipment.PK);
							AssertEquals(2, newestShipment.OuterPackLines.Count);
							AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
							AssertEquals("CONT0002", newestShipment.OuterPackLines[1].JL_Calc_ContainerNumber);
							AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
							AssertEquals("CNF", newestShipment.OuterPackLines[1].JL_OriginTransitWarehouseStatus);
							AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
							AssertArrayEqualsByElements(new ZString[] { "PKG3" }, newestShipment.OuterPackLines[1].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
						});
				}

				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

					var container = shipment.Consols[0].Containers.AddNew();
					container.JC_ContainerNum = "CONT0001";
					shipment.OuterPackLines[0].SetContainer(container.Consol, container);

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" }, new Container() { Link = 3, ContainerNumber = "CONT0003" } };
					shipmentDataObject.SetContainerCollection(() => containers);

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1, 1),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG2", shipment.OuterPackLines[0].JL_PackLineId, 2, 2),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG3", shipment.OuterPackLines[0].JL_PackLineId, 3, 3) }));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When Container CONT0001 is assigned to PackLine packline1 and CW1 receives Packages(PKG1 with CONT0001, PKG2 with CONT0002 and PKG3 with CONT0003)." +
						" PKG1 should be assigned to packline1 and container of packline1 should have no change, PKG2 and PKG3 would be seperately splitted to new packlines whose container should be CONT0002 and CONT0003", () =>
						{
							AssertEquals(shipment.PK, newestShipment.PK);
							AssertEquals(3, newestShipment.OuterPackLines.Count);
							AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
							AssertEquals("CONT0002", newestShipment.OuterPackLines[1].JL_Calc_ContainerNumber);
							AssertEquals("CONT0003", newestShipment.OuterPackLines[2].JL_Calc_ContainerNumber);
							AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
							AssertEquals("CNF", newestShipment.OuterPackLines[1].JL_OriginTransitWarehouseStatus);
							AssertEquals("CNF", newestShipment.OuterPackLines[2].JL_OriginTransitWarehouseStatus);
							AssertArrayEqualsByElements(new ZString[] { "PKG1" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
							AssertArrayEqualsByElements(new ZString[] { "PKG2" }, newestShipment.OuterPackLines[1].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
							AssertArrayEqualsByElements(new ZString[] { "PKG3" }, newestShipment.OuterPackLines[2].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
						});
				}

				{
					var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
					shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

					var container = shipment.Consols[0].Containers.AddNew();
					container.JC_ContainerNum = "CONT0001";
					shipment.OuterPackLines[0].SetContainer(container.Consol, container);

					var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
					var containers = new DataObjectList<Container>() { new Container() { Link = 2, ContainerNumber = "CONT0002" }, new Container() { Link = 3, ContainerNumber = "CONT0003" } };
					shipmentDataObject.SetContainerCollection(() => containers);

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1, 2),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG2", shipment.OuterPackLines[0].JL_PackLineId, 2, 2),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG3", shipment.OuterPackLines[0].JL_PackLineId, 3, 3) }));

					var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

					CombineAssertions("When Container CONT0001 is assgined to PackLine packline1 and CW1 receives Packages(PKG1 and PKG2 with CONT0002 and PKG3 with CONT0003)." +
						" PKG1 and PKG2 should be assigned to packline1 and container of packline1 should be CONT0002, PKG3 would be assigned to the splitted packline whose container should be CONT0003", () =>
						{
							AssertEquals(shipment.PK, newestShipment.PK);
							AssertEquals(2, newestShipment.OuterPackLines.Count);
							AssertEquals("CONT0002", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
							AssertEquals("CONT0003", newestShipment.OuterPackLines[1].JL_Calc_ContainerNumber);
							AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
							AssertEquals("CNF", newestShipment.OuterPackLines[1].JL_OriginTransitWarehouseStatus);
							AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
							AssertArrayEqualsByElements(new ZString[] { "PKG3" }, newestShipment.OuterPackLines[1].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
						});
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestRemoveContainerWhileSplitPackLineForNextTime()
		{
			var inx = 0;
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var protypeShipment = GetShipmentProtype(warehouseAddressBO);

				var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
				shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);
				Factory.SaveForTesting();

				var container = shipment.Consols[0].Containers.AddNew();
				container.JC_ContainerNum = "CONT0001";
				shipment.OuterPackLines[0].SetContainer(container.Consol, container);

				var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } });
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
					Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1, 1))));

				var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				CombineAssertions("When PackLine packline1 has Container CONT0001 and CW1 receives all RCV packages with Container CONT0001, all packages should be assigned to packline1", () =>
				{
					AssertEquals(shipment.PK, newestShipment.PK);
					AssertEquals(1, newestShipment.OuterPackLines.Count);
					AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
					AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
					AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2", "PKG3" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
				});

				shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } });
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
					Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForDispatch("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1, 1))));

				newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				CombineAssertions("When PackLine packline1 has Container CONT0001 and CW1 receives all DSP packages with Container CONT0001, all packages should be assigned to packline1", () =>
				{
					AssertEquals(shipment.PK, newestShipment.PK);
					AssertEquals(1, newestShipment.OuterPackLines.Count);
					AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
					AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
					AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2", "PKG3" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
				});

				shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } });
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
					Enumerable.Range(1, 2).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1, 1))));

				AssertExceptionThrown<DataObjectReadFailureException>("Block RCN reads after a DCN read", "RCN xml has not been processed. This update should be done through the DCN.", () => new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject());
			}
		}

		ForwardingShipment GetShipmentProtype(OrgAddress warehouseAddressBO)
		{
			var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, 99999);

			var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
			shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
				Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1))));

			return new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
		}

		ForwardingShipment CreateShipmentForTestRemoveContainerWhileSplitPackLine(OrgAddress warehouseAddressBO, ZInt inx)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "BACON PANCAKES" + inx;
			var consol = shipment.Consols.AddNew();
			consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_RefNumber = "REF001";
			packLine.JL_PackageCount = 3;

			Factory.SaveForTesting();

			return shipment;
		}

		[TestDate(2021, 1, 1)]
		public void TestGetJobHeaderForOrg_JobHeaderIsNull()
		{
			var expectedExceptionMessage = "Could not save Local Client Organization - Failed to create Shipment JobHeader with mutex.";

			var dataObjectShipment = SetupShipment();
			dataObjectShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				new OrganizationAddress()
					{
						AddressType = nameof(DocAddressType.LocalClient),
						Port = new UNLOCO() { Code = "NZDUD", Name = "Dunedin" },
						OrganizationCode = new ZCodeMappedZString() { SourceValue = "TestOrgZz" }
					}
			});

			var readingHelper = new ShipmentDataObjectReadingHelper(dataObjectShipment, logger, Factory);

			var fowardingShipment = Factory.New<ForwardingShipment>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TestOrgZz";

			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => readingHelper.PopulateBusinessObject(fowardingShipment));

			AssertNotNull(fowardingShipment.JobHeader);

			var anotherFactory = new BusinessObjectFactory();
			var shipmentInAnotherFactory = anotherFactory.Load<ForwardingShipment>(fowardingShipment.PK);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedExceptionMessage, () => readingHelper.PopulateBusinessObject(shipmentInAnotherFactory), true);

			fowardingShipment.JobHeader.Dispose();
		}

		public void TestTWReceipt_RenamePackage()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();
				var shipment = GetShipmentProtype(warehouseAddressBO);
				var originalPacklineId = shipment.OuterPackLines[0].JL_PackLineId;

				Factory.SaveForTesting();

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
					Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, originalPacklineId, 1))));

				var packingLine = shipmentDataObject.PackingLineCollection[0];
				packingLine.ReferenceNumber = "PKG5";
				packingLine.SetAddInfoCollection(() => new List<AddInfo>
				{
					new AddInfo() { Key = "PreviousPackageID", Value = "PKG1" }
				});
				LinkShipmentDOToConsol(shipmentDataObject, shipment.Consols[0]);

				var shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				CombineAssertions("still 1 packline, PKG1 renamed to PKG5", () =>
				{
					AssertEquals(1, shipmentRead.OuterPackLines.Count);
					var packLine = shipmentRead.OuterPackLines[0];
					AssertContainsExactElementsInAnyOrder("has correct packages", new[] { "PKG5", "PKG2", "PKG3" }, packLine.PkgPackageCollection.Select(p => p.KP_PackageID));
					AssertContains("Information - Renaming package PKG1 to PKG5.", Logger.Logs);
				});
			}
		}

		public void TestTWReceipt_RenamePackage_NoMatch()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();
				var shipment = GetShipmentProtype(warehouseAddressBO);
				var originalPacklineId = shipment.OuterPackLines[0].JL_PackLineId;
				Factory.SaveForTesting();

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
					Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, originalPacklineId, 1))));
				AddEmptyTRUReferences(shipmentDataObject.PackingLineCollection);

				var packingLine = shipmentDataObject.PackingLineCollection[0];
				packingLine.ReferenceNumber = "PKG5";
				packingLine.SetAddInfoCollection(() => new List<AddInfo>
				{
					new AddInfo() { Key = "PreviousPackageID", Value = "PKG20" }
				});
				LinkShipmentDOToConsol(shipmentDataObject, shipment.Consols[0]);

				var shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				CombineAssertions("default import logic is used: PKG2, PKG3, PK5 are split into a new packline because PKG1 has not arrived yet", () =>
				{
					AssertEquals(2, shipmentRead.OuterPackLines.Count);
					var packLine1 = shipmentRead.OuterPackLines.Cast<ForwardingPackLine>().First(t => t.JL_PackLineId == originalPacklineId);
					var packLine2 = shipmentRead.OuterPackLines.Cast<ForwardingPackLine>().First(t => t.JL_PackLineId != originalPacklineId);
					AssertContainsExactElementsInAnyOrder("packLine1 has correct packages", new[] { "PKG1" }, packLine1.PkgPackageCollection.Select(p => p.KP_PackageID));
					AssertContainsExactElementsInAnyOrder("packLine2 has correct packages", new[] { "PKG2", "PKG3", "PKG5" }, packLine2.PkgPackageCollection.Select(p => p.KP_PackageID));
					AssertContains("should not find package to rename", "Warning - Unable to find package to rename with package id PKG20.", Logger.Logs);
					AssertNotContains("Should not rename package", "Renaming package", Logger.Logs);
				});
			}
		}

		#region TW Events

		void WrapPackingLineWithContainer(UniversalShipment shipmentDataObject, params string[] asnList)
		{
			AssertEquals(shipmentDataObject.PackingLineCollection.Count, asnList.Length);

			var containerLinkLookup = shipmentDataObject.ContainerCollection.ToLookup(container => container.Link);
			var asnShipmentList = new List<UniversalShipment>();

			for (var inx = 0; inx < shipmentDataObject.PackingLineCollection.Count; inx++)
			{
				var packingLine = shipmentDataObject.PackingLineCollection[inx];
				if (packingLine.ReferenceNumberCollection == null)
				{
					packingLine.SetReferenceNumberCollection(() => new List<Reference> { new Reference { Type = new EntryType { Code = "TRA" }, ReferenceNumber = asnList[inx] } });
				}
				else
				{
					packingLine.ReferenceNumberCollection.Add(new Reference { Type = new EntryType { Code = "TRA" }, ReferenceNumber = asnList[inx] });
				}

				if (asnList.Take(inx).All(x => x != asnList[inx]))
				{
					var relatedShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.Instance);
					asnShipmentList.Add(relatedShipment);
					relatedShipment.DataContext = DataContextFactory.New();
					relatedShipment.DataContext.AddDataSource(DataContextType.TransitReceiveASN, asnList[inx]);

					var container = containerLinkLookup[packingLine.ContainerLink].FirstOrDefault();
					if (container != null)
					{
						if (relatedShipment.ContainerCollection == null)
						{
							relatedShipment.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = container.ContainerNumber, LCLUnpack = container.LCLUnpack } });
						}
						else
						{
							relatedShipment.ContainerCollection.Add(new Container { ContainerNumber = container.ContainerNumber, LCLUnpack = container.LCLUnpack });
						}
					}
				}
			}

			if (shipmentDataObject.RelatedShipmentCollection == null)
			{
				shipmentDataObject.SetRelatedShipmentCollection(() => asnShipmentList);
			}
			else
			{
				shipmentDataObject.RelatedShipmentCollection.AddRange(asnShipmentList);
			}
		}

		public void TestTWEvents_Receipt_UPC()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();
				var shipment = GetShipmentProtype(warehouseAddressBO);
				Factory.SaveForTesting();

				var lclUnpackDate11 = new ZDateTime(2020, 1, 21);
				var lclUnpackDate12 = new ZDateTime(2020, 1, 22);
				var lclUnpackDate13 = new ZDateTime(2020, 1, 23);
				var lclUnpackDate21 = new ZDateTime(2020, 1, 24);
				var lclUnpackDate22 = new ZDateTime(2020, 1, 25);
				var lclUnpackDate23 = new ZDateTime(2020, 1, 26);

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				var savedPackLineId = shipment.OuterPackLines[0].JL_PackLineId;

				var containers = new DataObjectList<Container>() {
					new Container() { Link = 1, ContainerNumber = "CONT0001", LCLUnpack = lclUnpackDate11 },
					new Container() { Link = 2, ContainerNumber = "CONT0002", LCLUnpack = lclUnpackDate12 },
					new Container() { Link = 3, ContainerNumber = "CONT0002", LCLUnpack = lclUnpackDate13 }
				};
				shipmentDataObject.SetContainerCollection(() => containers);

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG1", savedPackLineId, 1)),
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG2", savedPackLineId, 2)),
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG3", savedPackLineId, 2)),
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG4", savedPackLineId, 3)),
				}));
				LinkShipmentDOToConsol(shipmentDataObject, shipment.Consols[0]);

				WrapPackingLineWithContainer(shipmentDataObject, "ASN001", "ASN002", "ASN002", "ASN003");

				var shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				CombineAssertions("UPC events exist", () =>
				{
					var upcLogs = shipmentRead.Logs.Find(x => x.SL_SE_NKEvent == Events.UnpackingCompletedCode);
					AssertEquals("0 cancelled", 0, upcLogs.Count(x => x.IsCancelled));

					var upcUncancelledLogs = upcLogs.Where(x => !x.IsCancelled);
					var cont1Logs = upcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0001");
					var cont2Logs = upcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0002");

					AssertEquals("1 CONT0001 log", 1, cont1Logs.Count());
					AssertEquals("1 CONT0002 log", 1, cont2Logs.Count());
					var cont1Log = cont1Logs.ElementAt(0);
					var cont2Log = cont2Logs.ElementAt(0);

					AssertEquals("|EQN=CONT0001|FAC=CFS|LOC=ABCDE|TTL=1|TYP=Container", cont1Log.SL_Reference);
					AssertEquals("|EQN=CONT0002|FAC=CFS|LOC=ABCDE|TTL=3|TYP=Container", cont2Log.SL_Reference);
					AssertEquals("cont1 event date", lclUnpackDate11, cont1Log.SL_EventTime);
					AssertEquals("cont2 event date", lclUnpackDate13, cont2Log.SL_EventTime);

					var propagatedUPCLogs = shipmentRead.Consols[0].Logs.Find(x => x.SL_SE_NKEvent == Events.PackingCompletedCode);
					AssertEquals("No propagated logs to consol", 0, propagatedUPCLogs.Count());
				});

				shipmentDataObject.PackingLineCollection[0].OutturnQty = 2;
				shipmentDataObject.PackingLineCollection[0].PackQty = 2;
				shipmentDataObject.ContainerCollection[0].LCLUnpack = lclUnpackDate21;
				shipmentDataObject.ContainerCollection[1].LCLUnpack = lclUnpackDate22;
				shipmentDataObject.ContainerCollection[2].LCLUnpack = lclUnpackDate23;
				shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				CombineAssertions("UPC events are cancelled and replaced", () =>
				{
					var upcLogs = shipmentRead.Logs.Find(x => x.SL_SE_NKEvent == Events.UnpackingCompletedCode);
					var cancelledLogs = upcLogs.Where(x => x.SL_IsCancelled);

					AssertEquals("2 cancelled", 2, cancelledLogs.Count());

					var upcUncancelledLogs = upcLogs.Where(x => !x.SL_IsCancelled);
					var cont1Logs = upcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0001");
					var cont2Logs = upcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0002");

					AssertEquals("1 CONT0001 log", 1, cont1Logs.Count());
					AssertEquals("1 CONT0002 log", 1, cont2Logs.Count());
					var cont1Log = cont1Logs.ElementAt(0);
					var cont2Log = cont2Logs.ElementAt(0);

					AssertEquals("|EQN=CONT0001|FAC=CFS|LOC=ABCDE|TTL=2|TYP=Container", cont1Log.SL_Reference);
					AssertEquals("|EQN=CONT0002|FAC=CFS|LOC=ABCDE|TTL=3|TYP=Container", cont2Log.SL_Reference);
					AssertEquals("cont1 event date", lclUnpackDate21, cont1Log.SL_EventTime);
					AssertEquals("cont2 event date", lclUnpackDate23, cont2Log.SL_EventTime);

					var propagatedUPCLogs = shipmentRead.Consols[0].Logs.Find(x => x.SL_SE_NKEvent == Events.PackingCompletedCode);
					AssertEquals("No propagated logs to consol", 0, propagatedUPCLogs.Count());
				});

				shipmentDataObject.PackingLineCollection[2].ContainerLink = 1;
				Logger.ClearLogs();
				shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Assert(Logger.GetWarnings().Contains("On packing line ASN002, the expected container CONT0002 and the actual container CONT0001 did not match."));
			}
		}

		public void TestTWEvents_Receipt_UPC_OutturnDoesntMatchHasPTL()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();
				var shipment = GetShipmentProtype(warehouseAddressBO);
				Factory.SaveForTesting();

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				var savedPackLineId = shipment.OuterPackLines[0].JL_PackLineId;

				var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } };
				shipmentDataObject.SetContainerCollection(() => containers);

				var pkg3 = CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG3", savedPackLineId, 1) { OutturnQty = 0, PackQty = 1 });
				pkg3.ContainerLink = null;
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG1", savedPackLineId, 1) { OutturnQty = 5, PackQty = 10 }),
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG2", savedPackLineId, 1) { OutturnQty = 5, PackQty = 10 }),
					pkg3
				}));
				LinkShipmentDOToConsol(shipmentDataObject, shipment.Consols[0]);

				WrapPackingLineWithContainer(shipmentDataObject, "ASN001", "ASN002", "ASN003");
				shipmentDataObject.RelatedShipmentCollection[2].SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "CONT0001" } });

				var shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				CombineAssertions("UPC events exist", () =>
				{
					var upcLogs = shipmentRead.Logs.Find(x => x.SL_SE_NKEvent == Events.UnpackingCompletedCode);
					var cancelledLogs = upcLogs.Where(x => x.SL_IsCancelled);

					AssertEquals("0 cancelled", 0, cancelledLogs.Count());

					var upcUncancelledLogs = upcLogs.Where(x => !x.SL_IsCancelled);
					var cont1Logs = upcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0001");

					AssertEquals("1 CONT0001 log", 1, cont1Logs.Count());
					var cont1Log = cont1Logs.ElementAt(0);

					AssertEquals("|EQN=CONT0001|FAC=CFS|LOC=ABCDE|PTL=10|TTL=21|TYP=Container", cont1Log.SL_Reference);

					var propagatedUPCLogs = shipmentRead.Consols[0].Logs.Find(x => x.SL_SE_NKEvent == Events.PackingCompletedCode);
					AssertEquals("No propagated logs to consol", 0, propagatedUPCLogs.Count());
				});
			}
		}

		[TestTimeZoneUNLOCO("GBGNW")]
		public void TestTWEvents_Receipt_UPC_DoesntReplaceIfSameEventDate()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();
				var shipment = GetShipmentProtype(warehouseAddressBO);
				Factory.SaveForTesting();

				var lclUnpackDate1 = new ZDateTime(2020, 1, 1);

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				var savedPackLineId = shipment.OuterPackLines[0].JL_PackLineId;

				var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001", LCLUnpack = lclUnpackDate1 } };
				shipmentDataObject.SetContainerCollection(() => containers);

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG1", savedPackLineId, 1) { OutturnQty = 5, PackQty = 10 }),
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG2", savedPackLineId, 1) { OutturnQty = 5, PackQty = 10 }),
				}));
				LinkShipmentDOToConsol(shipmentDataObject, shipment.Consols[0]);

				WrapPackingLineWithContainer(shipmentDataObject, "ASN001", "ASN002");

				var shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				CombineAssertions("UPC events exist", () =>
				{
					var upcLogs = shipmentRead.Logs.Find(x => x.SL_SE_NKEvent == Events.UnpackingCompletedCode);
					var cancelledLogs = upcLogs.Where(x => x.SL_IsCancelled);
					AssertEquals("0 cancelled", 0, cancelledLogs.Count());

					var upcUncancelledLogs = upcLogs.Where(x => !x.SL_IsCancelled);
					var cont1Logs = upcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0001");

					AssertEquals("1 CONT0001 log", 1, cont1Logs.Count());
					var cont1Log = cont1Logs.ElementAt(0);

					AssertEquals("|EQN=CONT0001|FAC=CFS|LOC=ABCDE|PTL=10|TTL=20|TYP=Container", cont1Log.SL_Reference);
					AssertEquals("date is correct", lclUnpackDate1, cont1Log.SL_EventTime);

					var propagatedUPCLogs = shipmentRead.Consols[0].Logs.Find(x => x.SL_SE_NKEvent == Events.PackingCompletedCode);
					AssertEquals("No propagated logs to consol", 0, propagatedUPCLogs.Count());
				});

				shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				CombineAssertions("Still zero UPC cancelled", () =>
				{
					var upcLogs = shipmentRead.Logs.Find(x => x.SL_SE_NKEvent == Events.UnpackingCompletedCode);
					var cancelledLogs = upcLogs.Where(x => x.SL_IsCancelled);
					AssertEquals("0 cancelled", 0, cancelledLogs.Count());

					var upcUncancelledLogs = upcLogs.Where(x => !x.SL_IsCancelled);
					var cont1Logs = upcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0001");

					AssertEquals("1 CONT0001 log", 1, cont1Logs.Count());
					var cont1Log = cont1Logs.ElementAt(0);

					AssertEquals("|EQN=CONT0001|FAC=CFS|LOC=ABCDE|PTL=10|TTL=20|TYP=Container", cont1Log.SL_Reference);
					AssertEquals("date is correct", lclUnpackDate1, cont1Log.SL_EventTime);

					var propagatedUPCLogs = shipmentRead.Consols[0].Logs.Find(x => x.SL_SE_NKEvent == Events.PackingCompletedCode);
					AssertEquals("No propagated logs to consol", 0, propagatedUPCLogs.Count());
				});
			}
		}

		public void TestTWEvents_Dispatch_PKC()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();
				var shipment = GetShipmentProtype(warehouseAddressBO);
				Factory.SaveForTesting();

				var packDate11 = new ZDateTime(2020, 1, 1);
				var packDate12 = new ZDateTime(2020, 1, 2);
				var packDate13 = new ZDateTime(2020, 1, 3);
				var packDate21 = new ZDateTime(2020, 1, 4);
				var packDate22 = new ZDateTime(2020, 1, 5);
				var packDate23 = new ZDateTime(2020, 1, 6);

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO, DataContextType.TransitDispatch);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				var savedPackLineId = shipment.OuterPackLines[0].JL_PackLineId;

				var containers = new DataObjectList<Container>() {
					new Container() { Link = 1, ContainerNumber = "CONT0001", PackDate = packDate11 },
					new Container() { Link = 2, ContainerNumber = "CONT0002", PackDate = packDate12 },
					new Container() { Link = 3, ContainerNumber = "CONT0002", PackDate = packDate13 }
				};
				shipmentDataObject.SetContainerCollection(() => containers);

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG1", savedPackLineId, 1) { OutturnQty = 1, PackQty = 2 }),
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG2", savedPackLineId, 2) { OutturnQty = 1, PackQty = 2 }),
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG3", savedPackLineId, 2) { OutturnQty = 1, PackQty = 2 }),
					CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG4", savedPackLineId, 3) { OutturnQty = 1, PackQty = 2 }),
				}));
				LinkShipmentDOToConsol(shipmentDataObject, shipment.Consols[0]);

				var shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				CombineAssertions("PKC events exist", () =>
				{
					var pkcLogs = shipmentRead.Logs.Find(x => x.SL_SE_NKEvent == Events.PackingCompletedCode);
					AssertEquals("0 cancelled", 0, pkcLogs.Count(x => x.IsCancelled));

					var pkcUncancelledLogs = pkcLogs.Where(x => !x.SL_IsCancelled);
					var cont1Logs = pkcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0001");
					var cont2Logs = pkcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0002");

					AssertEquals("2 logs", 2, pkcUncancelledLogs.Count());
					AssertEquals("1 CONT0001 log", 1, cont1Logs.Count());
					AssertEquals("1 CONT0002 log", 1, cont2Logs.Count());
					var cont1Log = cont1Logs.ElementAt(0);
					var cont2Log = cont2Logs.ElementAt(0);

					AssertEquals("|EQN=CONT0001|FAC=CFS|LOC=ABCDE|TTL=1|TYP=Container", cont1Log.SL_Reference);
					AssertEquals("|EQN=CONT0002|FAC=CFS|LOC=ABCDE|TTL=3|TYP=Container", cont2Log.SL_Reference);
					AssertEquals("cont1 date", packDate11, cont1Log.SL_EventTime);
					AssertEquals("cont2 date", packDate13, cont2Log.SL_EventTime);

					var propagatedPKCLogs = shipmentRead.Consols[0].Logs.Find(x => x.SL_SE_NKEvent == Events.PackingCompletedCode);
					AssertEquals("No propagated logs to consol", 0, propagatedPKCLogs.Count());
				});

				shipmentDataObject.PackingLineCollection[0].OutturnQty = 100;
				shipmentDataObject.PackingLineCollection[0].PackQty = 200;
				shipmentDataObject.ContainerCollection[0].PackDate = packDate21;
				shipmentDataObject.ContainerCollection[1].PackDate = packDate22;
				shipmentDataObject.ContainerCollection[2].PackDate = packDate23;
				shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				CombineAssertions("PKC events are cancelled and replaced", () =>
				{
					var pkcLogs = shipmentRead.Logs.Find(x => x.SL_SE_NKEvent == Events.PackingCompletedCode);
					var cancelledLogs = pkcLogs.Where(x => x.SL_IsCancelled);

					AssertEquals("2 cancelled", 2, cancelledLogs.Count());

					var pkcUncancelledLogs = pkcLogs.Where(x => !x.SL_IsCancelled);
					var cont1Logs = pkcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0001");
					var cont2Logs = pkcUncancelledLogs.Where(x => x.Parameters.TryGetValue("EQN", out var val) && val == "CONT0002");

					AssertEquals("1 CONT0001 log", 1, cont1Logs.Count());
					AssertEquals("1 CONT0002 log", 1, cont2Logs.Count());
					var cont1Log = cont1Logs.ElementAt(0);
					var cont2Log = cont2Logs.ElementAt(0);

					AssertEquals("|EQN=CONT0001|FAC=CFS|LOC=ABCDE|TTL=100|TYP=Container", cont1Log.SL_Reference);
					AssertEquals("|EQN=CONT0002|FAC=CFS|LOC=ABCDE|TTL=3|TYP=Container", cont2Log.SL_Reference);
					AssertEquals("cont1 date", packDate21, cont1Log.SL_EventTime);
					AssertEquals("cont2 date", packDate23, cont2Log.SL_EventTime);

					var propagatedPKCLogs = shipmentRead.Consols[0].Logs.Find(x => x.SL_SE_NKEvent == Events.PackingCompletedCode);
					AssertEquals("No propagated logs to consol", 0, propagatedPKCLogs.Count());
				});
			}
		}

		public void TestTWEvents_UPC_Success_WhenReferenceNumberCollectionIsNull()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, 1);

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					CreatePacklineForReceive(
						new PacklineDOOverridesForTest("PKG1", shipment.OuterPackLines[0].JL_PackLineId) { OutturnQty = 1, PackQty = 2 })
				}));

				var readingHelper = new ShipmentDataObjectReadingHelper(shipmentDataObject, logger, Factory);
				AssertNoExceptionThrown(() => readingHelper.PopulateBusinessObject(shipment));
			}
		}

		#endregion

		[TestDate(2021, 1, 1)]
		public void TestLastKnownTransitWarehouseStatusDateTimeShouldBeLatestUnpackDate()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_UnpackDepotAddress = warehouseAddressBO.PK;
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_RefNumber = "REF001";
				packLine.JL_PackageCount = 2;
				Factory.SaveForTesting();

				var receiveShipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(consol, DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				receiveShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2010, 1, 1)));
				receiveShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2010, 1, 1)));
				receiveShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2019, 1, 2)));
				receiveShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2019, 1, 2)));
				receiveShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2021, 1, 2)));
				receiveShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 2)));
				receiveShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU002", DateType.Unpack, ZBool.True, new ZDateTime(2021, 1, 3)));
				receiveShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 3)));
				receiveShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 4)));
				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, packLine.JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, packLine.JL_PackLineId, 1))));

				receiveShipmentDataObject.PackingLineCollection.ForEach(packingLine => packingLine.ReferenceNumberCollection.Clear());
				receiveShipmentDataObject.PackingLineCollection[0].UnloadDate = new ZDateTime(2021, 1, 2);
				receiveShipmentDataObject.PackingLineCollection[0].ReferenceNumberCollection.Add(new Reference { ReferenceNumber = "TRU001", Type = new EntryType { Code = WorkflowDescriptors.TransitReceiveTransportationUnit } });
				receiveShipmentDataObject.PackingLineCollection[1].UnloadDate = new ZDateTime(2021, 1, 3);
				receiveShipmentDataObject.PackingLineCollection[1].ReferenceNumberCollection.Add(new Reference { ReferenceNumber = "TRU002", Type = new EntryType { Code = WorkflowDescriptors.TransitReceiveTransportationUnit } });
				Factory.SaveForTesting();
				var newestShipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals("LastKnownTransitWarehouseStatusDateTime should be the most recent unpack date ", new ZDateTime(2021, 1, 3), newestShipment.OuterPackLines[0].JL_LastKnownTransitWarehouseStatusDateTime);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDepartureTransitWarehouseExcluded_ReceivedAtConsolOrigin()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
				var unpackDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
				var exportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>();
				var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
				shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = packDepotAddress.PK;
				consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.PK;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackLineId = "PackLineId1";
				packLine1.JL_RefNumber = "REF001";
				packLine1.JL_PackageCount = 1;
				packLine1.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
				packLine1.JL_DepartureTransitWarehouseExcluded = true;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackLineId = "PackLineId2";
				packLine2.JL_RefNumber = "REF002";
				packLine2.JL_PackageCount = 1;
				packLine2.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
				packLine2.JL_DepartureTransitWarehouseExcluded = false;

				Factory.SaveForTesting();

				var receiveShipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(consol, DataContextType.TransitReceive, packDepotAddress);
				receiveShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>
				{
					CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2021, 1, 2))
				});
				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packLine2 }.Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i.JL_PackLineId, i.JL_PackLineId, 1, null, null, new ZDateTime(2021, 1, 2)))));
				ShipmentDataObjectReaderForDispatchTest.FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", receiveShipmentDataObject.PackingLineCollection);

				var newestShipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("newestShipment.OuterPackLines.Count", 2, newestShipment.OuterPackLines.Count);
				var newestPackLine1 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId1");
				AssertNotNull("newestPackLine1", newestPackLine1);
				AssertEquals("newestPackLine1.JL_OriginTransitWarehouseStatus", ZString.Empty, newestPackLine1.JL_LastKnownTransitWarehouseStatus);
				var newestPackLine2 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId2");
				AssertNotNull("newestPackLine2", newestPackLine2);
				AssertEquals("newestPackLine2.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, newestPackLine2.JL_LastKnownTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDepartureTransitWarehouseExcluded_ReceivedAtConsolDestination()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
				var unpackDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
				var exportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>();
				var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
				shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = packDepotAddress.PK;
				consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.PK;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackLineId = "PackLineId1";
				packLine1.JL_RefNumber = "REF001";
				packLine1.JL_PackageCount = 1;
				packLine1.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
				packLine1.JL_DepartureTransitWarehouseExcluded = true;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackLineId = "PackLineId2";
				packLine2.JL_RefNumber = "REF002";
				packLine2.JL_PackageCount = 1;
				packLine2.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched;
				packLine2.JL_DepartureTransitWarehouseExcluded = false;

				Factory.SaveForTesting();

				var receiveShipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(consol, DataContextType.TransitReceive, unpackDepotAddress);
				receiveShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>
				{
					CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2021, 1, 2))
				});
				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packLine1, packLine2 }.Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i.JL_PackLineId, i.JL_PackLineId, 1, null, null, new ZDateTime(2021, 1, 2)))));
				ShipmentDataObjectReaderForDispatchTest.FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", receiveShipmentDataObject.PackingLineCollection);

				var newestShipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("newestShipment.OuterPackLines.Count", 2, newestShipment.OuterPackLines.Count);
				var newestPackLine1 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId1");
				AssertNotNull("newestPackLine1", newestPackLine1);
				AssertEquals("newestPackLine1.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, newestPackLine1.JL_LastKnownTransitWarehouseStatus);
				var newestPackLine2 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId2");
				AssertNotNull("newestPackLine2", newestPackLine2);
				AssertEquals("newestPackLine2.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, newestPackLine2.JL_LastKnownTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDepartureTransitWarehouseExcluded_ReceivedAtShipmentOrigin()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
				var unpackDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
				var exportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>();
				var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
				shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = packDepotAddress.PK;
				consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.PK;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackLineId = "PackLineId1";
				packLine1.JL_RefNumber = "REF001";
				packLine1.JL_PackageCount = 1;
				packLine1.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
				packLine1.JL_DepartureTransitWarehouseExcluded = true;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackLineId = "PackLineId2";
				packLine2.JL_RefNumber = "REF002";
				packLine2.JL_PackageCount = 1;
				packLine2.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
				packLine2.JL_DepartureTransitWarehouseExcluded = false;

				Factory.SaveForTesting();

				var receiveShipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(consol, DataContextType.TransitReceive, exportReceivingDepot);
				receiveShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>
				{
					CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2021, 1, 2))
				});
				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packLine2 }.Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i.JL_PackLineId, i.JL_PackLineId, 1, null, null, new ZDateTime(2021, 1, 2)))));
				ShipmentDataObjectReaderForDispatchTest.FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", receiveShipmentDataObject.PackingLineCollection);

				var newestShipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("newestShipment.OuterPackLines.Count", 2, newestShipment.OuterPackLines.Count);
				var newestPackLine1 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId1");
				AssertNotNull("newestPackLine1", newestPackLine1);
				AssertEquals("newestPackLine1.JL_OriginTransitWarehouseStatus", ZString.Empty, newestPackLine1.JL_LastKnownTransitWarehouseStatus);
				var newestPackLine2 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId2");
				AssertNotNull("newestPackLine2", newestPackLine2);
				AssertEquals("newestPackLine2.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, newestPackLine2.JL_LastKnownTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDepartureTransitWarehouseExcluded_ReceivedAtShipmentDestination()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
				var unpackDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
				var exportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>();
				var importReleaseDepot = Factory.NewWithValidTestData<OrgAddress>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
				shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = packDepotAddress.PK;
				consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.PK;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackLineId = "PackLineId1";
				packLine1.JL_RefNumber = "REF001";
				packLine1.JL_PackageCount = 1;
				packLine1.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
				packLine1.JL_DepartureTransitWarehouseExcluded = true;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackLineId = "PackLineId2";
				packLine2.JL_RefNumber = "REF002";
				packLine2.JL_PackageCount = 1;
				packLine2.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched;
				packLine2.JL_DepartureTransitWarehouseExcluded = false;

				Factory.SaveForTesting();

				var receiveShipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(consol, DataContextType.TransitReceive, importReleaseDepot);
				receiveShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>
				{
					CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2021, 1, 2))
				});
				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packLine1, packLine2 }.Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i.JL_PackLineId, i.JL_PackLineId, 1, null, null, new ZDateTime(2021, 1, 2)))));
				ShipmentDataObjectReaderForDispatchTest.FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", receiveShipmentDataObject.PackingLineCollection);

				var newestShipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("newestShipment.OuterPackLines.Count", 2, newestShipment.OuterPackLines.Count);
				var newestPackLine1 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId1");
				AssertNotNull("newestPackLine1", newestPackLine1);
				AssertEquals("newestPackLine1.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, newestPackLine1.JL_LastKnownTransitWarehouseStatus);
				var newestPackLine2 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId2");
				AssertNotNull("newestPackLine2", newestPackLine2);
				AssertEquals("newestPackLine2.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, newestPackLine2.JL_LastKnownTransitWarehouseStatus);
			}
		}

		public void TestDoNotImportTransportLegsFromTransitReceive()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_RefNumber = "REF001";
				packLine.JL_PackageCount = 2;
				var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(consol, DataContextType.TransitReceive, warehouseAddressBO);
				shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
				var transportLegs = shipmentDataObject.TransportLegCollection;
				transportLegs.Add(new TransportLeg { LegOrder = 1, PortOfLoading = new UNLOCO { Code = "Syd" }, PortOfDischarge = new UNLOCO { Code = "sin" } });
				transportLegs.Add(new TransportLeg { LegOrder = 2, PortOfLoading = new UNLOCO { Code = "SIN" }, PortOfDischarge = new UNLOCO { Code = "laX" } });
				AssertNoExceptionThrown(() => new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject());
				AssertNotContains("Populating Transport...", Logger.Logs);
			}
		}

		[ExpectNoExceptions]
		public void TestDoNotMatchContainerFromAnotherConsol()
		{
			var inx = 0;
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var protypeShipment = GetShipmentProtype(warehouseAddressBO);

				var shipment = CreateShipmentForTestRemoveContainerWhileSplitPackLine(warehouseAddressBO, ++inx);
				shipment.OuterPackLines[0].CopyValuesFromPackage(protypeShipment.OuterPackLines[0].PkgPackageCollection[0]);

				var consol1 = shipment.Consols[0];
				var container = consol1.Containers.AddNew();
				container.JC_ContainerNum = "CONT0001";
				shipment.OuterPackLines[0].SetContainer(container.Consol, container);

				var consol2 = shipment.Consols.AddNew();
				var containerFromConsol2 = consol2.Containers.AddNew();
				containerFromConsol2.JC_ContainerNum = "CONT0002";
				shipment.OuterPackLines[0].SetContainer(consol2, containerFromConsol2);

				var shipmentDataObject = ShipmentDataObjectReaderForDispatchTest.BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = consol1.JK_UniqueConsignRef } });
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0002" } };
				shipmentDataObject.SetContainerCollection(() => containers);

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(
					Enumerable.Range(1, shipment.OuterPackLines[0].JL_PackageCount).Select(i => ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PKG" + i, shipment.OuterPackLines[0].JL_PackLineId, 1, 1))));

				var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				CombineAssertions("When Consol1 Container CONT0001 and Consol2 Container CONT0002 are assigned to PackLine packline1 and CW1 receives all packages with container CONT0002, all packages should be assigned to packline1 and container of packline1 should be Consol1 Container CONT0002", () =>
				{
					AssertEquals(shipment.PK, newestShipment.PK);
					AssertEquals(2, consol1.Containers.Count);
					AssertEquals("CONT0002", consol1.Containers[1].JC_ContainerNum);
					AssertEquals(1, newestShipment.OuterPackLines.Count);
					AssertEquals(consol1, newestShipment.OuterPackLines.CurrentConsol);
					AssertEquals("CONT0002", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
					AssertEquals("CNF", newestShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
					AssertArrayEqualsByElements(new ZString[] { "PKG1", "PKG2", "PKG3" }, newestShipment.OuterPackLines[0].PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
				});
			}
		}

		public void TestTWReceipt_AtPickupCFSDoesNotAutoSplit()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				var outerPackline1 = shipment.OuterPackLines.AddNew();
				outerPackline1.JL_PackageCount = 2;
				outerPackline1.JL_F3_NKPackType = "BOX";

				var outerPackline2 = shipment.OuterPackLines.AddNew();
				outerPackline2.JL_PackageCount = 3;
				outerPackline2.JL_F3_NKPackType = "BAG";

				var outerPackline3 = shipment.OuterPackLines.AddNew();
				outerPackline3.JL_PackageCount = 1;
				outerPackline3.JL_F3_NKPackType = "TUB";

				Factory.SaveForTesting();

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				var packingLineDOs = new List<PackingLine>()
				{
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PLT1", outerPackline1.JL_PackLineId, 1, packType: "BOX"),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PLT2", outerPackline1.JL_PackLineId, 1, packType: "CTN"),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PLT3", outerPackline2.JL_PackLineId, 1, packType: "CAS"),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PLT4", outerPackline2.JL_PackLineId, 1, packType: "BAG"),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PLT5", outerPackline2.JL_PackLineId, 1, packType: "CAS"),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PLT6", outerPackline3.JL_PackLineId, 1, packType: "TUB"),
				};

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(packingLineDOs));
				var shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				CombineAssertions("still 3 packline, no autosplit, discrepancies are recorded", () =>
				{
					AssertEquals(3, shipmentRead.OuterPackLines.Count);
					var packLine1 = shipmentRead.OuterPackLines[0];
					AssertContainsExactElementsInAnyOrder("has correct packages", new[] { "PLT1", "PLT2" }, packLine1.PkgPackageCollection.Select(p => p.KP_PackageID));
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packLine1.JL_OriginTransitWarehouseStatus);
					AssertEquals("BOX", packLine1.JL_F3_NKPackType);

					var packLine2 = shipmentRead.OuterPackLines[1];
					AssertContainsExactElementsInAnyOrder("has correct packages", new[] { "PLT3", "PLT4", "PLT5" }, packLine2.PkgPackageCollection.Select(p => p.KP_PackageID));
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packLine2.JL_OriginTransitWarehouseStatus);
					AssertEquals("BAG", packLine2.JL_F3_NKPackType);

					var packLine3 = shipmentRead.OuterPackLines[2];
					AssertContainsExactElementsInAnyOrder("has correct packages", new[] { "PLT6" }, packLine3.PkgPackageCollection.Select(p => p.KP_PackageID));
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packLine3.JL_OriginTransitWarehouseStatus);
				});
			}
		}

		public void TestTWReceipt_AtPickupCFSAutoSplitIfPackageHasDifferentContainer()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				var outerPackline = shipment.OuterPackLines.AddNew();
				outerPackline.JL_PackageCount = 3;
				outerPackline.JL_F3_NKPackType = "BOX";

				Factory.SaveForTesting();

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				var containers = new DataObjectList<Container>
				{
					new Container { Link = 1, ContainerNumber = "CONT0001" },
					new Container { Link = 2, ContainerNumber = "CONT0002" }
				};
				shipmentDataObject.SetContainerCollection(() => containers);
				var packingLineDOs = new List<PackingLine>()
				{
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PLT1", outerPackline.JL_PackLineId, 1, 1, packType: "BOX"),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PLT2", outerPackline.JL_PackLineId, 1, 1, packType: "CTN"),
					ShipmentDataObjectReaderForDispatchTest.CreatePacklineForReceive("PLT3", outerPackline.JL_PackLineId, 1, 2, packType: "BOX"),
				};

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(packingLineDOs));
				var shipmentRead = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				CombineAssertions("Auto-split when package has different container", () =>
				{
					AssertEquals(2, shipmentRead.OuterPackLines.Count);
					var packLine1 = shipmentRead.OuterPackLines[0];
					AssertContainsExactElementsInAnyOrder("has correct packages", new[] { "PLT1", "PLT2" }, packLine1.PkgPackageCollection.Select(p => p.KP_PackageID));
					AssertEquals("BOX", packLine1.JL_F3_NKPackType);
					AssertEquals("CONT0001", packLine1.JL_Calc_ContainerNumber);
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packLine1.JL_OriginTransitWarehouseStatus);

					var packLine2 = shipmentRead.OuterPackLines[1];
					AssertContainsExactElementsInAnyOrder("has correct packages", new[] { "PLT3" }, packLine2.PkgPackageCollection.Select(p => p.KP_PackageID));
					AssertEquals("BOX", packLine2.JL_F3_NKPackType);
					AssertEquals("CONT0002", packLine2.JL_Calc_ContainerNumber);
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, packLine2.JL_OriginTransitWarehouseStatus);
				});
			}
		}
	}
}
