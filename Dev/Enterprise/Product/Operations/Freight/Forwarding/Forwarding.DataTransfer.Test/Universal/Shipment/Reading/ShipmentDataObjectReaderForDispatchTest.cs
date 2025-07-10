using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.DataTransfer.Testing.ShipmentDataObjectReaderTest;
using static Enterprise.Integration.Customs;
using DataContext = Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ShipmentDataObjectReaderForDispatchTest : ShipmentDataObjectReadingHelperTest
	{
		readonly EntryType TruckType = new EntryType { Code = "TRU" };

		internal static PackingLine CreatePacklineForDispatch(ZString? referenceNumber, ZString? packLineId, ZInt? outturnQty, ZInt? containerLink = null, Reference reference = null, bool forPrepareDispatch = false, ZDateTime? loadDate = null)
		{
			var packline = ShipmentDataObjectReaderTest.CreatePacklineForDispatch(new PacklineDOOverridesForTest(referenceNumber, packLineId, containerLink, loadDate) { OutturnQty = outturnQty });
			packline.UNDGCollection.Clear();

			if (!forPrepareDispatch)
			{
				packline.ContainerLink = containerLink;

				var referenceList = new List<Reference>();
				referenceList.Add(new Reference { Type = new EntryType { Code = "TDU", Description = "Transit Warehouse Dispatch Transport" }, ReferenceNumber = "TD00000004" });
				if (reference != null)
				{
					referenceList.Add(reference);
				}
				packline.SetReferenceNumberCollection(() => referenceList);
				packline.UNDGCollection.Clear();
			}

			return packline;
		}

		internal static PackingLine CreatePacklineForReceive(ZString? referenceNumber, ZString? packLineId, ZInt? outturnQty, ZInt? containerLink = null, Reference reference = null, ZDateTime? unloadDate = null, ZString? packType = null)
		{
			var packline = ShipmentDataObjectReaderTest.CreatePacklineForReceive(new PacklineDOOverridesForTest(referenceNumber, packLineId, containerLink, unloadDate) { OutturnQty = outturnQty });
			packline.UNDGCollection.Clear();

			if (!packType.GetValueOrDefault().IsEmpty)
			{
				packline.PackType = new PackageType { Code = packType };
			}

			var referenceList = new List<Reference>();
			referenceList.Add(new Reference { Type = new EntryType { Code = "TRU", Description = "Transit Warehouse Receive Transport" }, ReferenceNumber = "TR00000004" });
			if (reference != null)
			{
				referenceList.Add(reference);
			}
			packline.SetReferenceNumberCollection(() => referenceList);

			packline.UNDGCollection.Clear();

			return packline;
		}

		internal static UniversalShipment BuildUniversalShipmentWithAddress(ForwardingConsol consol, DataContextType contextType, OrgAddress warehouseAddressBO)
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(contextType, "TR00001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

			var addresses = new List<OrganizationAddress>();
			addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressShortCode = warehouseAddressBO.AddressCode,
				AddressType = nameof(DocAddressType.LocalCartageCFS),
				OrganizationCode = warehouseAddressBO.Header.OH_Code
			});
			shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

			if (consol == null)
			{
				return shipmentDataObject;
			}

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());

			if (contextType == DataContextType.TransitDispatch)
			{
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
			}

			var header = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			header.DataContext = new DataContext();
			header.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000054");
			header.VesselName = "CONT0001";
			header.SetContainerCollection(() => new DataObjectList<Container>() { });

			var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			loadList.DataContext = new DataContext();
			loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
			loadList.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() { new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = consol.JK_UniqueConsignRef } });
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { header, loadList });

			return shipmentDataObject;
		}

		void AssertPackLineLastDispatchInformations(ForwardingPackLine packLine, ZGuid warehouseAddressPK, ZString lastKnownTransitWarehouseStatus, ZDateTime lastKnownTransitWarehouseStatusDateTime, ZString originTransitWarehouseStatus, ZString[] referenceNumbers)
		{
			AssertEquals(warehouseAddressPK, packLine.JL_OA_LastKnownTransitWarehouseAddress);
			AssertEquals(lastKnownTransitWarehouseStatus, packLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals(lastKnownTransitWarehouseStatusDateTime, packLine.JL_LastKnownTransitWarehouseStatusDateTime);
			AssertEquals(originTransitWarehouseStatus, packLine.JL_OriginTransitWarehouseStatus);
			AssertEquals(referenceNumbers.Length, packLine.PkgPackageCollection.Count);
			AssertArrayEqualsByElements(referenceNumbers, packLine.PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
		}

		ForwardingShipment PrepareShipment(OrgAddress warehouseAddressBO)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "BACON PANCAKES";
			var consol = shipment.Consols.AddNew();
			consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
			return shipment;
		}

		ForwardingPackLine PreparePackLineWithPackages(OrgAddress warehouseAddressBO, ZInt packageCount, bool isForReceive = false)
		{
			var shipment = PrepareShipment(warehouseAddressBO);
			var consol = shipment.Consols[0];
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_RefNumber = "REF001";
			packLine.JL_PackageCount = packageCount;

			Factory.SaveForTesting();

			var shipmentDataObject = BuildUniversalShipmentWithAddress(consol, DataContextType.TransitReceive, warehouseAddressBO);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
			if (isForReceive)
			{
				shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, ZDateTime.Now));
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, packLine.JL_PackageCount).Select(i => CreatePacklineForReceive("PKG" + i, packLine.JL_PackLineId, 1))));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", shipmentDataObject.PackingLineCollection);
			}
			else
			{
				shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Unpack, ZBool.True, ZDateTime.Now));
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, packLine.JL_PackageCount).Select(i => CreatePacklineForDispatch("PKG" + i, packLine.JL_PackLineId, 1))));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", shipmentDataObject.PackingLineCollection);
			}

			Factory.SaveForTesting();

			var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

			AssertEquals(shipment.PK, newestShipment.PK);
			AssertEquals(1, newestShipment.OuterPackLines.Count);
			var newestPackLine = newestShipment.OuterPackLines.FirstOrDefault() as ForwardingPackLine;
			AssertEquals(packLine.PK, newestPackLine.PK);
			newestPackLine.JL_OA_LastKnownTransitWarehouseAddress = warehouseAddressBO.PK;
			newestPackLine.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			newestPackLine.JL_LastKnownTransitWarehouseStatusDateTime = new ZDateTime(TestDateAttribute.Date);
			newestPackLine.CopyValuesFromPackage(newestPackLine.PkgPackageCollection.First());
			newestPackLine.SetQuantityWeightAndVolumeFromPackageTotals();
			newestPackLine.SetUNDGsFromPackageCollection();
			newestPackLine.JL_OriginTransitWarehouseStatus = "CNF";
			Factory.SaveForTesting();

			return packLine;
		}

		[TestDate(2021, 1, 1, 1, 8, 9)]
		public void TestUnknownLocalCartageCFS()
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
				var shipmentDataObject = BuildUniversalShipmentWithAddress(consol, DataContextType.TransitDispatch, warehouseAddressBO);
				shipmentDataObject.OrganizationAddressCollection.RemoveAt(0);
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, packLine.JL_PackageCount).Select(i => CreatePacklineForDispatch("PKG" + i, packLine.JL_PackLineId, 1))));
				Factory.SaveForTesting();

				AssertNoExceptionThrown(() => new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject());
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_RefNumber = "REF001";
				packLine.JL_PackageCount = 2;
				var shipmentDataObject = BuildUniversalShipmentWithAddress(consol, DataContextType.TransitDispatch, warehouseAddressBO);
				shipmentDataObject.OrganizationAddressCollection[0].OrganizationCode = "XXXX";
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, packLine.JL_PackageCount).Select(i => CreatePacklineForDispatch("PKG" + i, packLine.JL_PackLineId, 1))));
				Factory.SaveForTesting();

				AssertExceptionThrown<DataObjectReadFailureException>(() => new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject());
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestLastKnownTransitWarehouseStatusDateTimeShouldBeLatestDatePacked()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();

				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 2, true);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, 2).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO1.JL_PackLineId, 1, null, null, forPrepareDispatch: false, new ZDateTime(2021, 1, 3)))));
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2010, 1, 1)));
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2010, 1, 1)));
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2019, 1, 2)));
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2019, 1, 2)));
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2021, 1, 2)));
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 2)));
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2021, 1, 3)));
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 3)));
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, new ZDateTime(2021, 1, 4)));
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("LastKnownTransitWarehouseStatusDateTime should be the most recent pack date ", new ZDateTime(2021, 1, 3), shipment.OuterPackLines[0].JL_LastKnownTransitWarehouseStatusDateTime);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDepartureTransitWarehouseExcluded_DispatchedAtConsolOrigin()
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
				var packageJob = Factory.New<PkgPackageJob>();
				packageJob.KJ_ParentID = shipment.PK;
				packageJob.KJ_ParentTableCode = shipment.TablePrefix;
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
				packLine2.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
				packLine2.JL_DepartureTransitWarehouseExcluded = false;
				var package2 = packLine2.PkgPackageCollection.AddNew();
				package2.KP_KJ_ParentPackageJob = packageJob.PK;
				package2.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				package2.KP_PackageID = "PKG" + packLine2.JL_PackLineId;

				Factory.SaveForTesting();

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(consol, DataContextType.TransitDispatch, packDepotAddress);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>
				{
					CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 2))
				});
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packLine2 }.Select(i => CreatePacklineForDispatch("PKG" + i.JL_PackLineId, i.JL_PackLineId, 1, null, null, forPrepareDispatch: false, new ZDateTime(2021, 1, 2)))));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);

				var newestShipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("newestShipment.OuterPackLines.Count", 2, newestShipment.OuterPackLines.Count);
				var newestPackLine1 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId1");
				AssertNotNull("newestPackLine1", newestPackLine1);
				AssertEquals("newestPackLine1.JL_OriginTransitWarehouseStatus", ZString.Empty, newestPackLine1.JL_LastKnownTransitWarehouseStatus);
				var newestPackLine2 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId2");
				AssertNotNull("newestPackLine2", newestPackLine2);
				AssertEquals("newestPackLine2.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, newestPackLine2.JL_LastKnownTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDepartureTransitWarehouseExcluded_DispatchedAtConsolDestination()
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
				var packageJob = Factory.New<PkgPackageJob>();
				packageJob.KJ_ParentID = shipment.PK;
				packageJob.KJ_ParentTableCode = shipment.TablePrefix;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackLineId = "PackLineId1";
				packLine1.JL_RefNumber = "REF001";
				packLine1.JL_PackageCount = 1;
				packLine1.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
				packLine1.JL_DepartureTransitWarehouseExcluded = true;
				var package1 = packLine1.PkgPackageCollection.AddNew();
				package1.KP_KJ_ParentPackageJob = packageJob.PK;
				package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				package1.KP_PackageID = "PKG" + packLine1.JL_PackLineId;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackLineId = "PackLineId2";
				packLine2.JL_RefNumber = "REF002";
				packLine2.JL_PackageCount = 1;
				packLine2.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
				packLine2.JL_DepartureTransitWarehouseExcluded = false;
				var package2 = packLine2.PkgPackageCollection.AddNew();
				package2.KP_KJ_ParentPackageJob = packageJob.PK;
				package2.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				package2.KP_PackageID = "PKG" + packLine2.JL_PackLineId;

				Factory.SaveForTesting();

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(consol, DataContextType.TransitDispatch, unpackDepotAddress);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>
				{
					CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 2))
				});
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packLine1, packLine2 }.Select(i => CreatePacklineForDispatch("PKG" + i.JL_PackLineId, i.JL_PackLineId, 1, null, null, forPrepareDispatch: false, new ZDateTime(2021, 1, 2)))));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);

				var newestShipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("newestShipment.OuterPackLines.Count", 2, newestShipment.OuterPackLines.Count);
				var newestPackLine1 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId1");
				AssertNotNull("newestPackLine1", newestPackLine1);
				AssertEquals("newestPackLine1.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, newestPackLine1.JL_LastKnownTransitWarehouseStatus);
				var newestPackLine2 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId2");
				AssertNotNull("newestPackLine2", newestPackLine2);
				AssertEquals("newestPackLine2.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, newestPackLine2.JL_LastKnownTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDepartureTransitWarehouseExcluded_DispatchedAtShipmentOrigin()
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
				var packageJob = Factory.New<PkgPackageJob>();
				packageJob.KJ_ParentID = shipment.PK;
				packageJob.KJ_ParentTableCode = shipment.TablePrefix;
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
				packLine2.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
				packLine2.JL_DepartureTransitWarehouseExcluded = false;
				var package2 = packLine2.PkgPackageCollection.AddNew();
				package2.KP_KJ_ParentPackageJob = packageJob.PK;
				package2.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				package2.KP_PackageID = "PKG" + packLine2.JL_PackLineId;

				Factory.SaveForTesting();

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(consol, DataContextType.TransitDispatch, exportReceivingDepot);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>
				{
					CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 2))
				});
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packLine2 }.Select(i => CreatePacklineForDispatch("PKG" + i.JL_PackLineId, i.JL_PackLineId, 1, null, null, forPrepareDispatch: false, new ZDateTime(2021, 1, 2)))));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);

				var newestShipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("newestShipment.OuterPackLines.Count", 2, newestShipment.OuterPackLines.Count);
				var newestPackLine1 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId1");
				AssertNotNull("newestPackLine1", newestPackLine1);
				AssertEquals("newestPackLine1.JL_OriginTransitWarehouseStatus", ZString.Empty, newestPackLine1.JL_LastKnownTransitWarehouseStatus);
				var newestPackLine2 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId2");
				AssertNotNull("newestPackLine2", newestPackLine2);
				AssertEquals("newestPackLine2.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, newestPackLine2.JL_LastKnownTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDepartureTransitWarehouseExcluded_DispatchedAtShipmentDestination()
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
				var packageJob = Factory.New<PkgPackageJob>();
				packageJob.KJ_ParentID = shipment.PK;
				packageJob.KJ_ParentTableCode = shipment.TablePrefix;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackLineId = "PackLineId1";
				packLine1.JL_RefNumber = "REF001";
				packLine1.JL_PackageCount = 1;
				packLine1.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
				packLine1.JL_DepartureTransitWarehouseExcluded = true;
				var package1 = packLine1.PkgPackageCollection.AddNew();
				package1.KP_KJ_ParentPackageJob = packageJob.PK;
				package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				package1.KP_PackageID = "PKG" + packLine1.JL_PackLineId;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackLineId = "PackLineId2";
				packLine2.JL_RefNumber = "REF002";
				packLine2.JL_PackageCount = 1;
				packLine2.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
				packLine2.JL_DepartureTransitWarehouseExcluded = false;
				var package2 = packLine2.PkgPackageCollection.AddNew();
				package2.KP_KJ_ParentPackageJob = packageJob.PK;
				package2.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				package2.KP_PackageID = "PKG" + packLine2.JL_PackLineId;

				Factory.SaveForTesting();

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(consol, DataContextType.TransitDispatch, importReleaseDepot);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>
				{
					CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 2))
				});
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packLine1, packLine2 }.Select(i => CreatePacklineForDispatch("PKG" + i.JL_PackLineId, i.JL_PackLineId, 1, null, null, forPrepareDispatch: false, new ZDateTime(2021, 1, 2)))));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);

				var newestShipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("newestShipment.OuterPackLines.Count", 2, newestShipment.OuterPackLines.Count);
				var newestPackLine1 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId1");
				AssertNotNull("newestPackLine1", newestPackLine1);
				AssertEquals("newestPackLine1.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, newestPackLine1.JL_LastKnownTransitWarehouseStatus);
				var newestPackLine2 = newestShipment.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(p => p.JL_PackLineId == "PackLineId2");
				AssertNotNull("newestPackLine2", newestPackLine2);
				AssertEquals("newestPackLine2.JL_OriginTransitWarehouseStatus", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched, newestPackLine2.JL_LastKnownTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestRemoveContainerWhileSplitPackLine()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);
				packlineBO1.CopyValuesFromPackage(packlineBO1.PkgPackageCollection[0]);
				var container = packlineBO1.Shipment.Consols[0].Containers.AddNew();
				container.JC_ContainerNum = "CONT0001";
				packlineBO1.SetContainer(packlineBO1.Shipment.Consols[0], container);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } });
				var packlines = new DataObjectList<PackingLine>();
				packlines.Add(CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1, 1));
				dispatchShipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(2, shipment.OuterPackLines.Count);
				Assert("container of existed packline should be removed", shipment.OuterPackLines[0].JL_Calc_ContainerNumber.IsEmpty);
				AssertEquals("the status of existed packline should be CNF", "CNF", shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
				AssertEquals("splitted packline should have container", "CONT0001", shipment.OuterPackLines[1].JL_Calc_ContainerNumber);
				AssertEquals("the status of splitted packline should be CNF", "CNF", shipment.OuterPackLines[1].JL_OriginTransitWarehouseStatus);
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);
				packlineBO1.CopyValuesFromPackage(packlineBO1.PkgPackageCollection[0]);
				var container = packlineBO1.Shipment.Consols[0].Containers.AddNew();
				container.JC_ContainerNum = "CONT0001";
				packlineBO1.SetContainer(packlineBO1.Shipment.Consols[0], container);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0002" } });
				var packlines = new DataObjectList<PackingLine>();
				packlines.Add(CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1, 1));
				dispatchShipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertEquals("container of existed packline should be CONT0001", "CONT0001", shipment.OuterPackLines[0].JL_Calc_ContainerNumber);
				AssertEquals("the status of existed packline should be CNF", "CNF", shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
				AssertEquals("splitted packline should be CONT0002", "CONT0002", shipment.OuterPackLines[1].JL_Calc_ContainerNumber);
				AssertEquals("the status of splitted packline should be CNF", "CNF", shipment.OuterPackLines[1].JL_OriginTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestSplitPackLineDiscrepancy()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 2)));
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, 3).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO1.JL_PackLineId, 1, null, null, forPrepareDispatch: false, new ZDateTime(2021, 1, 2)))));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				dispatchShipmentDataObject.PackingLineCollection[1].HarmonisedCode = "ZZZ";
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 2),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
					new ZString[] { "PKG1", "PKG2", "PKG3" });
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 3)));
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, 3).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO1.JL_PackLineId, 1, null, null, forPrepareDispatch: false, new ZDateTime(2021, 1, 3)))));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 3),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG1", "PKG2", "PKG3" });
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 4)));
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, 2).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO1.JL_PackLineId, 1, null, null, forPrepareDispatch: false, new ZDateTime(2021, 1, 4)))));
				dispatchShipmentDataObject.PackingLineCollection[1].HarmonisedCode = "ZZZ";
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					new ZDateTime(2021, 1, 3),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG3" });
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 4),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
					new ZString[] { "PKG1", "PKG2" });
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 5)));
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, 2).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO1.JL_PackLineId, 1, null, null, forPrepareDispatch: false, new ZDateTime(2021, 1, 5)))));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					new ZDateTime(2021, 1, 4),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG3" });
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 5),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG1", "PKG2" });
			}
		}

		internal static void FillTransitTransportationUnit(ZString typeCode, ZString number, IEnumerable<PackingLine> packingLines)
		{
			if (packingLines == null)
			{
				return;
			}

			foreach (var packingLine in packingLines)
			{
				if (packingLine.ReferenceNumberCollection == null)
				{
					packingLine.SetReferenceNumberCollection(() => new List<Reference>());
				}

				packingLine.ReferenceNumberCollection.Clear();
				packingLine.ReferenceNumberCollection.Add(new Reference { ReferenceNumber = number, Type = new EntryType { Code = typeCode } });
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestSplitPackLine()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, 2).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO1.JL_PackLineId, 1))));
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 2)));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					new ZDateTime(2021, 1, 1),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG3" });
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 2),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG1", "PKG2" });
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" } });
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 3)));
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				var packlines = new DataObjectList<PackingLine>();
				packlines.Add(CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1, 1));
				packlines.Add(CreatePacklineForDispatch("PKG2", packlineBO1.JL_PackLineId, 1, 2));
				dispatchShipmentDataObject.SetPackingLineCollection(() => packlines);
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(3, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					new ZDateTime(2021, 1, 2),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG3" });
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 3),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG1" });
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[2],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 3),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG2" });
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" } });
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 4)));
				var packlines = new DataObjectList<PackingLine>();
				packlines.Add(CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1, 1));
				packlines.Add(CreatePacklineForDispatch("PKG2", packlineBO1.JL_PackLineId, 1, 1));
				packlines.Add(CreatePacklineForDispatch("PKG3", packlineBO1.JL_PackLineId, 1, 2));
				dispatchShipmentDataObject.SetPackingLineCollection(() => packlines);
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 4),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG1", "PKG2" });
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 4),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG3" });
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 5)));
				var packlines = new DataObjectList<PackingLine>();
				packlines.Add(CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR001" }));
				packlines.Add(CreatePacklineForDispatch("PKG2", packlineBO1.JL_PackLineId, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR001" }));
				packlines.Add(CreatePacklineForDispatch("PKG3", packlineBO1.JL_PackLineId, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR002" }));
				dispatchShipmentDataObject.SetPackingLineCollection(() => packlines);
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 5),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG1", "PKG2", "PKG3" });
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 3, true);
				var newWarehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				packlineBO1.Shipment.Consols.AddNew().JK_OA_PackDepotAddress = newWarehouseAddressBO.PK;

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 6)));
				var packlines = new DataObjectList<PackingLine>();
				packlines.Add(CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR001" }));
				packlines.Add(CreatePacklineForDispatch("PKG2", packlineBO1.JL_PackLineId, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR001" }));
				dispatchShipmentDataObject.SetPackingLineCollection(() => packlines);
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDataObject.PackingLineCollection);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					new ZDateTime(2021, 1, 5),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG3" });
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 6),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG1", "PKG2" });
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO = PreparePackLineWithPackages(warehouseAddressBO, 30, true);

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDO = BuildUniversalShipmentWithAddress(packlineBO.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDO.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDO.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 7)));
				var packingLines = new DataObjectList<PackingLine>(Enumerable.Range(1, 20).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO.JL_PackLineId, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR00" + i })));
				dispatchShipmentDO.SetPackingLineCollection(() => packingLines);
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDO.PackingLineCollection);
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(dispatchShipmentDO, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(2, shipment.OuterPackLines.Count);

				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					new ZDateTime(2021, 1, 6),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					Enumerable.Range(21, 10).Select(i => (ZString)("PKG" + i)).ToArray());
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 7),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					Enumerable.Range(1, 20).Select(i => (ZString)("PKG" + i)).ToArray());

				TestDateAttribute.AddDays(1);
				dispatchShipmentDO = BuildUniversalShipmentWithAddress(packlineBO.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDO.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDO.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 8)));
				packingLines = new DataObjectList<PackingLine>(Enumerable.Range(1, 26).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO.JL_PackLineId, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR00" + i })));
				packingLines.AddRange(Enumerable.Range(31, 2).Select(i => CreatePacklineForDispatch("PKG" + i, null, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR00" + i })));
				dispatchShipmentDO.SetPackingLineCollection(() => packingLines);
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDO.PackingLineCollection);
				Factory.SaveForTesting();

				shipment = new ShipmentDataObjectReader(dispatchShipmentDO, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(4, shipment.OuterPackLines.Count);
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					new ZDateTime(2021, 1, 6),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					Enumerable.Range(27, 4).Select(i => (ZString)("PKG" + i)).ToArray());
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 7),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					Enumerable.Range(1, 20).Select(i => (ZString)("PKG" + i)).ToArray());
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[2],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 8),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					Enumerable.Range(21, 6).Select(i => (ZString)("PKG" + i)).ToArray());
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[3],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 8),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus,
					Enumerable.Range(31, 2).Select(i => (ZString)("PKG" + i)).ToArray());
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO = PreparePackLineWithPackages(warehouseAddressBO, 30, true);

				TestDateAttribute.AddDays(1);
				var newWarehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				packlineBO.Shipment.Consols.AddNew().JK_OA_PackDepotAddress = newWarehouseAddressBO.PK;
				Factory.SaveForTesting();

				var dispatchShipmentDO = BuildUniversalShipmentWithAddress(packlineBO.Shipment.Consols[1], DataContextType.TransitDispatch, newWarehouseAddressBO);
				dispatchShipmentDO.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDO.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 9)));
				var packingLines = new DataObjectList<PackingLine>(Enumerable.Range(1, 20).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO.JL_PackLineId, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR00" + i })));
				dispatchShipmentDO.SetPackingLineCollection(() => packingLines);
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDO.PackingLineCollection);
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(dispatchShipmentDO, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(2, shipment.OuterPackLines.Count);

				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					warehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					new ZDateTime(2021, 1, 8),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					Enumerable.Range(21, 10).Select(i => (ZString)("PKG" + i)).ToArray());
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					newWarehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 9),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					Enumerable.Range(1, 20).Select(i => (ZString)("PKG" + i)).ToArray());

				TestDateAttribute.AddDays(1);
				var newWarehouseAddressBO2 = Factory.NewWithValidTestData<OrgAddress>();
				packlineBO.Shipment.Consols.AddNew().JK_OA_PackDepotAddress = newWarehouseAddressBO2.PK;
				Factory.SaveForTesting();

				dispatchShipmentDO = BuildUniversalShipmentWithAddress(packlineBO.Shipment.Consols[1], DataContextType.TransitDispatch, newWarehouseAddressBO2);
				dispatchShipmentDO.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				dispatchShipmentDO.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitDispatchHeader, "TDU001", DateType.Pack, ZBool.True, new ZDateTime(2021, 1, 10)));
				packingLines = new DataObjectList<PackingLine>(Enumerable.Range(1, 30).Select(i => CreatePacklineForDispatch("PKG" + i, packlineBO.JL_PackLineId, 1, null, new Reference { Type = TruckType, ReferenceNumber = "TR00" + i })));
				dispatchShipmentDO.SetPackingLineCollection(() => packingLines);
				FillTransitTransportationUnit(WorkflowDescriptors.TransitDispatchTransportationUnit, "TDU001", dispatchShipmentDO.PackingLineCollection);
				Factory.SaveForTesting();

				shipment = new ShipmentDataObjectReader(dispatchShipmentDO, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(2, shipment.OuterPackLines.Count);

				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[0],
					newWarehouseAddressBO2.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 10),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					Enumerable.Range(21, 10).Select(i => (ZString)("PKG" + i)).ToArray());
				AssertPackLineLastDispatchInformations(shipment.OuterPackLines[1],
					newWarehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
					new ZDateTime(2021, 1, 9),
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					Enumerable.Range(1, 20).Select(i => (ZString)("PKG" + i)).ToArray());
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestAutomaticalShipmentAllocationWhileReceiveDSP()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES 1";
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_RefNumber = "REF001";
				packLine1.JL_PackageCount = 3;
				packLine1.JL_PackLineId = "PLID0001";
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_RefNumber = "REF002";
				packLine2.JL_PackageCount = 3;
				packLine2.JL_PackLineId = "PLID0002";

				Factory.SaveForTesting();
				AssertEquals(ZString.Empty, shipment.OuterPackLines[0].JL_Calc_ContainerNumber);
				AssertEquals(ZString.Empty, shipment.OuterPackLines[1].JL_Calc_ContainerNumber);

				var shipmentDataObject = BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
					CreatePacklineForReceive("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1),
					CreatePacklineForReceive("PKG2", shipment.OuterPackLines[1].JL_PackLineId, 2) }));

				var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertEquals(shipment.PK, newestShipment.PK);
				AssertEquals(ZString.Empty, newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
				AssertEquals(ZString.Empty, newestShipment.OuterPackLines[1].JL_Calc_ContainerNumber);

				shipmentDataObject = BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } });

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { CreatePacklineForDispatch("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1, 1) }));

				newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(2, newestShipment.OuterPackLines.Count);
				AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
				AssertEquals(ZString.Empty, newestShipment.OuterPackLines[1].JL_Calc_ContainerNumber);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestAutomaticalShipmentAllocationWhileReceiveRCV()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES 1";
				var consol = shipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_RefNumber = "REF001";
				packLine1.JL_PackageCount = 3;
				packLine1.JL_PackLineId = "PLID0001";
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_RefNumber = "REF002";
				packLine2.JL_PackageCount = 3;
				packLine2.JL_PackLineId = "PLID0002";

				Factory.SaveForTesting();
				AssertEquals(ZString.Empty, shipment.OuterPackLines[0].JL_Calc_ContainerNumber);
				AssertEquals(ZString.Empty, shipment.OuterPackLines[1].JL_Calc_ContainerNumber);

				var shipmentDataObject = BuildUniversalShipmentWithAddress(shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } });

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { CreatePacklineForReceive("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1, 1) }));

				var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(2, newestShipment.OuterPackLines.Count);
				AssertEquals("CONT0001", newestShipment.OuterPackLines[0].JL_Calc_ContainerNumber);
				AssertEquals(ZString.Empty, newestShipment.OuterPackLines[1].JL_Calc_ContainerNumber);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestNoSplitReceipt_UpdatesJL_Damaged()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				packLine.JL_PackageCount = 5;
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, 1).Select(i =>
				{
					var packingLine = CreatePacklineForReceive("PKG" + i, packLine.JL_PackLineId, 1);
					packingLine.OutturnDamagedQty = 1;
					return packingLine;
				})));
				var newestShipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				CombineAssertions(() =>
				{
					AssertEquals("JL_PackageCount is not updated", 5, newestShipment.OuterPackLines[0].JL_PackageCount);
					AssertEquals("JL_Damaged is updated", 1, newestShipment.OuterPackLines[0].JL_Damaged);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestNoSplitDispatch_UpdatesJL_Damaged()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				packlineBO1.CopyValuesFromPackage(packlineBO1.PkgPackageCollection[0]);
				packlineBO1.JL_Damaged = 0;
				packlineBO1.JL_PackageCount = 5;

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);

				var packlines = new DataObjectList<PackingLine>();
				var packingLine = CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1, 1);
				packingLine.OutturnDamagedQty = 1;
				packlines.Add(packingLine);
				dispatchShipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(1, shipment.OuterPackLines.Count);
				CombineAssertions(() =>
				{
					AssertEquals("JL_PackageCount is not updated", 5, shipment.OuterPackLines[0].JL_PackageCount);
					AssertEquals("JL_Damaged is updated", 1, shipment.OuterPackLines[0].JL_Damaged);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsOnPackType()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 2, true);
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.PkgPackageCollection[1].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, 2).Select(i =>
				{
					var packingLine = CreatePacklineForReceive("PKG" + i, packLine.JL_PackLineId, 1);
					packingLine.PackType = new PackageType() { Code = i == 1 ? "PLT" : "ZZZ" };
					return packingLine;
				})));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("packline was split", 2, shipment.OuterPackLines.Count);
			}
		}

		#region Split by Additional Screening Method

		[TestDate(2021, 1, 1)]
		public void TestReceipt_UpdateSurplusPacklineAdditionalInspection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, isForReceive: true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "FRPAR";
				packLine.Shipment.JS_RL_NKDestination = "GBNRW";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.JL_IsHighRisk = true;
				packLine.JL_AdditionalInspectionTypeCode = "PHS";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "FRPAR" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "GBNRW" };

				var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.IsHighRisk = true;
				universalPackingLine1.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				var universalPackingLine2 = CreatePacklineForReceive("PKG2", null, 1);
				universalPackingLine2.PackType = new PackageType() { Code = "BOX" };
				universalPackingLine2.IsHighRisk = true;
				universalPackingLine2.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "XRY", Description = "XRY" };

				var universalPackingLine3 = CreatePacklineForReceive("PKG3", null, 1);
				universalPackingLine3.PackType = new PackageType() { Code = "CTN" };
				universalPackingLine3.IsHighRisk = false;
				universalPackingLine3.AviationSecurityAdditionalInspectionType = null;

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { universalPackingLine1, universalPackingLine2, universalPackingLine3 }) { Content = CollectionContent.Complete });

				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(3, shipment.OuterPackLines.Count);
				var outerPackLine = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_AdditionalInspectionTypeCode == "EDS");
				CombineAssertions("PackLine with additional inspection type EDS", () =>
				{
					AssertNotNull(outerPackLine);
					Assert("ReadOnly", !outerPackLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
				});

				outerPackLine = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_AdditionalInspectionTypeCode == "XRY");
				CombineAssertions("PackLine with additional inspection type XRY", () =>
				{
					AssertNotNull(outerPackLine);
					Assert("ReadOnly", !outerPackLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
				});

				outerPackLine = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_AdditionalInspectionTypeCode == "UNK");
				CombineAssertions("PackLine with additional inspection type UNK", () =>
				{
					AssertNotNull(outerPackLine);
					Assert("ReadOnly", outerPackLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_UpdatePacklineAdditionalInspectionAndCreateSecurityEventLog()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "FRPAR";
				packLine.Shipment.JS_RL_NKDestination = "AUSYD";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.JL_IsHighRisk = true;
				packLine.JL_AdditionalInspectionTypeCode = "PHS";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "FRPAR" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

				var universalPackingLine = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine.ScreeningMethod = "VCK";
				universalPackingLine.IsHighRisk = true;
				universalPackingLine.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { universalPackingLine }));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertEquals("JL_InspectionTypeCode has been updated", "VCK", shipment.OuterPackLines[0].JL_InspectionTypeCode);
				AssertEquals("JL_AdditionalInspectionTypeCode has been updated", "EDS", shipment.OuterPackLines[0].JL_AdditionalInspectionTypeCode);
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);

				var secEvents = packLine.Shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SecurityModifiedCode));
				AssertEquals("|NEW=EDS|OLD=PHS|RES=Additional Inspection at Package level Changed by Transit Warehouse package update|TYP=High Risk", secEvents[2].SL_Reference);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsBasedOnPackageAdditionalScreeningMethod()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 4, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "FRPAR";
				packLine.Shipment.JS_RL_NKDestination = "AUSYD";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "FRPAR" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

				var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.ScreeningMethod = "VCK";
				universalPackingLine1.IsHighRisk = true;
				universalPackingLine1.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				var universalPackingLine2 = CreatePacklineForReceive("PKG2", packLine.JL_PackLineId, 1);
				universalPackingLine2.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine2.ScreeningMethod = "VCK";
				universalPackingLine2.IsHighRisk = true;
				universalPackingLine2.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "CMD", Description = "CMD" };

				var universalPackingLine3 = CreatePacklineForReceive("PKG3", packLine.JL_PackLineId, 1);
				universalPackingLine3.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine3.ScreeningMethod = "VCK";
				universalPackingLine3.IsHighRisk = true;
				universalPackingLine3.AviationSecurityAdditionalInspectionType = null;

				var universalPackingLine4 = CreatePacklineForReceive("PKG4", packLine.JL_PackLineId, 1);
				universalPackingLine4.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine4.ScreeningMethod = "VCK";
				universalPackingLine4.IsHighRisk = true;
				universalPackingLine4.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					universalPackingLine1,
					universalPackingLine2,
					universalPackingLine3,
					universalPackingLine4
				}));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(3, shipment.OuterPackLines.Count);

				AssertPackLineAfterSplit("EDS", 2);
				AssertPackLineAfterSplit("CMD", 1);
				AssertPackLineAfterSplit("UNK", 1);

				void AssertPackLineAfterSplit(ZString inspection, ZInt packageCount)
				{
					var packLineAfterSplit = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_AdditionalInspectionTypeCode == inspection);
					AssertNotNull(packLineAfterSplit);

					AssertEquals(packageCount, packLineAfterSplit.JL_PackageCount);
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, packLineAfterSplit.JL_OriginTransitWarehouseStatus);
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_DoNotSplitsBasedOnPackageAdditionalScreeningMethod_DoNotUpdateReadOnlyAdditionalInspection_SupplyChainSecurityDisabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 4, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "AUSYD";
				packLine.Shipment.JS_RL_NKDestination = "DEBER";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "FRPAR" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "DEBER" };

				var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.ScreeningMethod = "VCK";
				universalPackingLine1.IsHighRisk = true;
				universalPackingLine1.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				var universalPackingLine2 = CreatePacklineForReceive("PKG2", packLine.JL_PackLineId, 1);
				universalPackingLine2.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine2.ScreeningMethod = "CMD";
				universalPackingLine2.IsHighRisk = true;
				universalPackingLine2.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "CMD", Description = "CMD" };

				var universalPackingLine3 = CreatePacklineForReceive("PKG3", packLine.JL_PackLineId, 1);
				universalPackingLine3.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine3.ScreeningMethod = ZString.Empty;
				universalPackingLine3.IsHighRisk = true;
				universalPackingLine3.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = ZString.Empty, Description = ZString.Empty };

				var universalPackingLine4 = CreatePacklineForReceive("PKG4", packLine.JL_PackLineId, 1);
				universalPackingLine4.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine4.ScreeningMethod = "CMD";
				universalPackingLine4.IsHighRisk = true;
				universalPackingLine4.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					universalPackingLine1,
					universalPackingLine2,
					universalPackingLine3,
					universalPackingLine4
				}));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("Do not split based on read-only Additional Inspection", 1, shipment.OuterPackLines.Count);
				Assert(shipment.OuterPackLines[0].JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
				Assert(shipment.OuterPackLines[0].JL_AdditionalInspectionTypeCode.IsEmpty);
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsBasedOnPackageAdditionalScreeningMethod_KeepAdditionalInspectionTypeUnchanged()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "FRPAR";
				packLine.Shipment.JS_RL_NKDestination = "GBNRW";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.JL_IsHighRisk = true;
				packLine.JL_AdditionalInspectionTypeCode = "EDS";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "FRPAR" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "GBNRW" };

				var universalPackingLine = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine.ScreeningMethod = "VCK";
				universalPackingLine.IsHighRisk = true;
				universalPackingLine.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					universalPackingLine
				}));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(1, shipment.OuterPackLines.Count);

				foreach (PackLine packLineAfterSplit in shipment.OuterPackLines)
				{
					Assert(!packLineAfterSplit.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
					AssertEquals("When no screening method is specified, non-empty packline additional inspection type code should be unchanged", "EDS", packLineAfterSplit.JL_AdditionalInspectionTypeCode);
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_NoScreenings_AdditionalScreeningMethodRemainsUnchanged()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 2, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "FRPAR";
				packLine.Shipment.JS_RL_NKDestination = "GBNRW";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.JL_IsHighRisk = true;
				packLine.JL_AdditionalInspectionTypeCode = "EDS";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "FRPAR" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "GBNRW" };

				var universalPackingLine = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine.ScreeningMethod = "VCK";
				universalPackingLine.IsHighRisk = true;
				universalPackingLine.AviationSecurityAdditionalInspectionType = null;
				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { universalPackingLine }) { Content = CollectionContent.Complete });
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(1, shipment.OuterPackLines.Count);
				var packLineAfterSplit = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_AdditionalInspectionTypeCode == "EDS");
				AssertNotNull(packLineAfterSplit);
				Assert(!packLineAfterSplit.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDispatch_UpdatePacklineAdditionalInspection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "FRPAR";
				packLine.Shipment.JS_RL_NKDestination = "AUSYD";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.CopyValuesFromPackage(packLine.PkgPackageCollection[0]);

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				dispatchShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "FRPAR" };
				dispatchShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

				var universalPackingLine = CreatePacklineForDispatch("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine.ScreeningMethod = "VCK";
				universalPackingLine.IsHighRisk = true;
				universalPackingLine.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { universalPackingLine }));
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertEquals("JL_AdditionalInspectionTypeCode has been updated", "EDS", shipment.OuterPackLines[0].JL_AdditionalInspectionTypeCode);
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDispatch_SplitsBasedOnPackageAdditionalScreeningMethod()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 4, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "FRPAR";
				packLine.Shipment.JS_RL_NKDestination = "AUSYD";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.CopyValuesFromPackage(packLine.PkgPackageCollection[0]);

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				dispatchShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "FRPAR" };
				dispatchShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

				var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.ScreeningMethod = "VCK";
				universalPackingLine1.IsHighRisk = true;
				universalPackingLine1.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				var universalPackingLine2 = CreatePacklineForReceive("PKG2", packLine.JL_PackLineId, 1);
				universalPackingLine2.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine2.ScreeningMethod = "VCK";
				universalPackingLine2.IsHighRisk = true;
				universalPackingLine2.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "CMD", Description = "CMD" };

				var universalPackingLine3 = CreatePacklineForReceive("PKG3", packLine.JL_PackLineId, 1);
				universalPackingLine3.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine3.ScreeningMethod = "VCK";
				universalPackingLine3.IsHighRisk = true;
				universalPackingLine3.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = ZString.Empty, Description = ZString.Empty };

				var universalPackingLine4 = CreatePacklineForReceive("PKG4", packLine.JL_PackLineId, 1);
				universalPackingLine4.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine4.ScreeningMethod = "VCK";
				universalPackingLine4.IsHighRisk = true;
				universalPackingLine4.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair() { Code = "EDS", Description = "EDS" };

				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					universalPackingLine1,
					universalPackingLine2,
					universalPackingLine3,
					universalPackingLine4
				}));
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(3, shipment.OuterPackLines.Count);
				AssertPackLineAfterSplit("EDS", 2);
				AssertPackLineAfterSplit("CMD", 1);
				AssertPackLineAfterSplit("UNK", 1);

				void AssertPackLineAfterSplit(ZString inspection, ZInt packageCount)
				{
					var packLineAfterSplit = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_AdditionalInspectionTypeCode == inspection);
					AssertNotNull(packLineAfterSplit);

					AssertEquals(packageCount, packLineAfterSplit.JL_PackageCount);
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, packLineAfterSplit.JL_OriginTransitWarehouseStatus);
				}
			}
		}

		public void TestCopyValuesFromPackage_ForJL_AdditionalInspectionTypeCode()
		{
			AssertCopyValuesFromPackage_ForJL_InspectionTypeCode(Core.Constants.TransportModes.Air, "UNK", "EDS");
			AssertCopyValuesFromPackage_ForJL_InspectionTypeCode(Core.Constants.TransportModes.Sea, "UNK", "UNK");

			void AssertCopyValuesFromPackage_ForJL_InspectionTypeCode(ZString transportMode, ZString expectedInspectionTypeCode1, ZString expectedInspectionTypeCode2)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_TransportMode = transportMode;
					shipment.JS_RL_NKOrigin = "FRPAR";
					shipment.JS_RL_NKDestination = "USCHI";

					var packLine = shipment.OuterPackLines.AddNew();
					AssertEquals(transportMode != Core.Constants.TransportModes.Air, packLine.JL_IsHighRiskInfo.ReadOnly);

					var packageParent = Factory.New<DummyBusinessObject>();

					var packageJob = Factory.New<ForwardingPackageJob>();
					packageJob.KJ_ParentID = packageParent.PK;
					packageJob.KJ_ParentTableCode = packageParent.TablePrefix;

					var package = packLine.PkgPackageCollection.AddNew();
					package.KP_KJ_ParentPackageJob = packageJob.PK;
					package.KP_F3_NKPackType = Core.Constants.PkgUnit.Container;
					package.SetScreeningMethod("XRY");
					package.SetIsHighRisk(true);
					package.SetAdditionalScreeningMethod("EDS");
					AssertEquals(expectedInspectionTypeCode1, packLine.JL_AdditionalInspectionTypeCode);

					packLine.CopyValuesFromPackage(package);
					AssertEquals(expectedInspectionTypeCode2, packLine.JL_AdditionalInspectionTypeCode);
				}
			}
		}

		#endregion

		#region Split by Screening Method

		[TestDate(2021, 1, 1)]
		public void TestReceipt_UpdateSurplusPacklineInspection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, isForReceive: true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "FRPAR";
				packLine.Shipment.JS_RL_NKDestination = "GBNRW";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.JL_InspectionTypeCode = "PHS";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "FRPAR" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "GBNRW" };

				var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.ScreeningMethod = "VCK";

				var universalPackingLine2 = CreatePacklineForReceive("PKG2", null, 1);
				universalPackingLine2.PackType = new PackageType() { Code = "BOX" };
				universalPackingLine2.ScreeningMethod = "XRY";

				var universalPackingLine3 = CreatePacklineForReceive("PKG3", null, 1);
				universalPackingLine3.PackType = new PackageType() { Code = "CTN" };
				universalPackingLine3.ScreeningMethod = null;

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { universalPackingLine1, universalPackingLine2, universalPackingLine3 }) { Content = CollectionContent.Complete });

				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(3, shipment.OuterPackLines.Count);
				var outerPackLine = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_InspectionTypeCode == "VCK");
				CombineAssertions("PackLine with inspection type VCK", () =>
				{
					AssertNotNull(outerPackLine);
					Assert("ReadOnly", !outerPackLine.JL_InspectionTypeCodeInfo.ReadOnly);
				});

				outerPackLine = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_InspectionTypeCode == "XRY");
				CombineAssertions("PackLine with inspection type XRY", () =>
				{
					AssertNotNull(outerPackLine);
					Assert("ReadOnly", !outerPackLine.JL_InspectionTypeCodeInfo.ReadOnly);
				});

				outerPackLine = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_InspectionTypeCode == "UNK");
				CombineAssertions("PackLine with inspection type UNK", () =>
				{
					AssertNotNull(outerPackLine);
					Assert("ReadOnly", !outerPackLine.JL_InspectionTypeCodeInfo.ReadOnly);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_UpdatePacklineInspection()
		{
			TestCase(Core.Constants.TransportModes.Air);
			TestCase(Core.Constants.TransportModes.AirSea);

			void TestCase(string transportMode)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
					Factory.SaveForTesting();

					var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
					packLine.Shipment.JS_TransportMode = transportMode;
					packLine.Shipment.JS_RL_NKOrigin = "GBFXT";
					packLine.Shipment.JS_RL_NKDestination = "AUSYD";
					packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
					packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
					packLine.JL_F3_NKPackType = "PLT";
					Factory.SaveForTesting();

					var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = transportMode };
					receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "GBFXT" };
					receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

					var universalPackingLine = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
					universalPackingLine.PackType = new PackageType() { Code = "PLT" };
					universalPackingLine.ScreeningMethod = "VCK";

					receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { universalPackingLine }));
					var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertEquals(1, shipment.OuterPackLines.Count);
					AssertEquals("JL_InspectionTypeCode has been updated", "VCK", shipment.OuterPackLines[0].JL_InspectionTypeCode);
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsBasedOnPackageScreeningMethod()
		{
			TestCase(Core.Constants.TransportModes.Air);
			TestCase(Core.Constants.TransportModes.AirSea);

			void TestCase(string transportMode)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
					Factory.SaveForTesting();

					var packLine = PreparePackLineWithPackages(warehouseAddressBO, 4, true);
					packLine.Shipment.JS_TransportMode = transportMode;
					packLine.Shipment.JS_RL_NKOrigin = "GBFXT";
					packLine.Shipment.JS_RL_NKDestination = "AUSYD";
					packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
					packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
					packLine.JL_F3_NKPackType = "PLT";
					Factory.SaveForTesting();

					var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = transportMode };
					receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "GBFXT" };
					receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

					var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
					universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
					universalPackingLine1.ScreeningMethod = "VCK";

					var universalPackingLine2 = CreatePacklineForReceive("PKG2", packLine.JL_PackLineId, 1);
					universalPackingLine2.PackType = new PackageType() { Code = "PLT" };
					universalPackingLine2.ScreeningMethod = "CMD";

					var universalPackingLine3 = CreatePacklineForReceive("PKG3", packLine.JL_PackLineId, 1);
					universalPackingLine3.PackType = new PackageType() { Code = "PLT" };
					universalPackingLine3.ScreeningMethod = ZString.Empty;

					var universalPackingLine4 = CreatePacklineForReceive("PKG4", packLine.JL_PackLineId, 1);
					universalPackingLine4.PackType = new PackageType() { Code = "PLT" };
					universalPackingLine4.ScreeningMethod = "VCK";

					receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
					{
						universalPackingLine1,
						universalPackingLine2,
						universalPackingLine3,
						universalPackingLine4
					}));
					var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertEquals(3, shipment.OuterPackLines.Count);

					AssertPackLineAfterSplit("VCK", 2);
					AssertPackLineAfterSplit("CMD", 1);
					AssertPackLineAfterSplit("UNK", 1);

					void AssertPackLineAfterSplit(ZString inspection, ZInt packageCount)
					{
						var packLineAfterSplit = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_InspectionTypeCode == inspection);
						AssertNotNull(packLineAfterSplit);

						AssertEquals(packageCount, packLineAfterSplit.JL_PackageCount);
						AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, packLineAfterSplit.JL_OriginTransitWarehouseStatus);
					}
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_UpdateShipmentScreeningMethod()
		{
			TestCase(Core.Constants.TransportModes.Air, "XRY", new[] { "XRY", "XRY", "XRY" });
			TestCase(Core.Constants.TransportModes.AirSea, "XRY", new[] { "XRY", "XRY", "XRY" });

			TestCase(Core.Constants.TransportModes.Air, "SCR", new[] { "XRY", "PHS", "VCK" });
			TestCase(Core.Constants.TransportModes.AirSea, "SCR", new[] { "XRY", "PHS", "VCK" });

			TestCase(Core.Constants.TransportModes.Air, "UNK", new[] { "XRY", "UNK", "VCK" });
			TestCase(Core.Constants.TransportModes.AirSea, "UNK", new[] { "XRY", "UNK", "VCK" });

			TestCase(Core.Constants.TransportModes.Air, "APP", new[] { "APP", "APP", "APP" });
			TestCase(Core.Constants.TransportModes.AirSea, "APP", new[] { "APP", "APP", "APP" });

			TestCase(Core.Constants.TransportModes.Air, "UNK", new[] { "UNK", "UNK", "UNK" });
			TestCase(Core.Constants.TransportModes.AirSea, "UNK", new[] { "UNK", "UNK", "UNK" });

			TestCase(Core.Constants.TransportModes.Air, "UNK", new[] { "APP", "APP", "UNK" });
			TestCase(Core.Constants.TransportModes.AirSea, "UNK", new[] { "APP", "APP", "UNK" });

			TestCase(Core.Constants.TransportModes.Air, "SCR", new[] { "APP", "XRY", "VCK" });
			TestCase(Core.Constants.TransportModes.AirSea, "SCR", new[] { "APP", "XRY", "VCK" });

			TestCase(Core.Constants.TransportModes.Air, "UNK", new[] { "APP", "XRY", "UNK" });
			TestCase(Core.Constants.TransportModes.AirSea, "UNK", new[] { "APP", "XRY", "UNK" });

			void TestCase(string transportMode, string expectedShipmentInspectionType, string[] packageInspectionTypes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
					Factory.SaveForTesting();

					var packLine = PreparePackLineWithPackages(warehouseAddressBO, packageInspectionTypes.Length, true);
					packLine.Shipment.JS_TransportMode = transportMode;
					packLine.Shipment.JS_RL_NKOrigin = "GBFXT";
					packLine.Shipment.JS_RL_NKDestination = "AUSYD";
					packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
					packLine.Shipment.JS_HouseBill = $"S{transportMode}{string.Join("", packageInspectionTypes)}";
					packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
					packLine.JL_F3_NKPackType = "PLT";
					Factory.SaveForTesting();

					var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
					receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = transportMode };
					receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "GBFXT" };
					receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };
					receiveShipmentDataObject.WayBillNumber = $"S{transportMode}{string.Join("", packageInspectionTypes)}";

					receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(0, packageInspectionTypes.Length).Select(i =>
					{
						var packingLine = CreatePacklineForReceive($"PKG{i + 1}", packLine.JL_PackLineId, 1, containerLink: 1);
						packingLine.PackType = new PackageType() { Code = "PLT" };
						packingLine.ScreeningMethod = packageInspectionTypes[i];
						return packingLine;
					})));
					var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertEquals(packageInspectionTypes.Distinct().Count(), shipment.OuterPackLines.Count);
					Assert("All packlines are confirmed", shipment.OuterPackLines.Cast<ForwardingPackLine>()
						.All(p => p.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed));
					AssertContainsExactElementsInAnyOrder("Packlines inspection type", packageInspectionTypes.Distinct(), shipment.OuterPackLines.Cast<ForwardingPackLine>().Select(p => p.JL_InspectionTypeCode));
					AssertEquals("Shipment inspection type is updated correctly", expectedShipmentInspectionType, shipment.JS_InspectionTypeCode);
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_DoNotSplitsBasedOnPackageScreeningMethod_DoNotUpdateReadOnlyInspection_SeaTransportMode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 4, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				packLine.Shipment.JS_RL_NKOrigin = "GBFXT";
				packLine.Shipment.JS_RL_NKDestination = "AUSYD";
				packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Sea };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "GBFXT" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

				var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.ScreeningMethod = "VCK";

				var universalPackingLine2 = CreatePacklineForReceive("PKG2", packLine.JL_PackLineId, 1);
				universalPackingLine2.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine2.ScreeningMethod = "CMD";

				var universalPackingLine3 = CreatePacklineForReceive("PKG3", packLine.JL_PackLineId, 1);
				universalPackingLine3.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine3.ScreeningMethod = ZString.Empty;

				var universalPackingLine4 = CreatePacklineForReceive("PKG4", packLine.JL_PackLineId, 1);
				universalPackingLine4.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine4.ScreeningMethod = "VCK";

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					universalPackingLine1,
					universalPackingLine2,
					universalPackingLine3,
					universalPackingLine4
				}));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("Do not split based on ScreeningMethod", 1, shipment.OuterPackLines.Count);

				Assert(shipment.OuterPackLines[0].JL_InspectionTypeCodeInfo.ReadOnly);
				Assert(shipment.OuterPackLines[0].JL_InspectionTypeCode.IsEmpty);
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_DoNotSplitsBasedOnPackageScreeningMethod_DoNotUpdateReadOnlyInspection_SupplyChainSecurityDisabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 4, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "NZAKL";
				packLine.Shipment.JS_RL_NKDestination = "USCHI";
				packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "CNCAN" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "ZACPT" };

				var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.ScreeningMethod = "VCK";

				var universalPackingLine2 = CreatePacklineForReceive("PKG2", packLine.JL_PackLineId, 1);
				universalPackingLine2.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine2.ScreeningMethod = "CMD";

				var universalPackingLine3 = CreatePacklineForReceive("PKG3", packLine.JL_PackLineId, 1);
				universalPackingLine3.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine3.ScreeningMethod = ZString.Empty;

				var universalPackingLine4 = CreatePacklineForReceive("PKG4", packLine.JL_PackLineId, 1);
				universalPackingLine4.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine4.ScreeningMethod = "VCK";

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					universalPackingLine1,
					universalPackingLine2,
					universalPackingLine3,
					universalPackingLine4
				}));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("Do not split based on ScreeningMethod", 1, shipment.OuterPackLines.Count);

				Assert(shipment.OuterPackLines[0].JL_InspectionTypeCodeInfo.ReadOnly);
				Assert(shipment.OuterPackLines[0].JL_InspectionTypeCode.IsEmpty);
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsBasedOnPackageScreeningMethod_KeepInspectionTypeUnchanged()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "AUSYD";
				packLine.Shipment.JS_RL_NKDestination = "GBNRW";
				packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.JL_InspectionTypeCode = "VCK";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "AUSYD" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "GBNRW" };

				var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.ScreeningMethod = ZString.Empty;

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					universalPackingLine1
				}));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(1, shipment.OuterPackLines.Count);

				foreach (PackLine packLineAfterSplit in shipment.OuterPackLines)
				{
					Assert(!packLineAfterSplit.JL_InspectionTypeCodeInfo.ReadOnly);
					AssertEquals("When no screening method is specified, non-empty packline inspection type code should be unchanged", "VCK", packLineAfterSplit.JL_InspectionTypeCode);
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsBasedOnPackageScreeningMethod_SetDefaultValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 2, true);
				packLine.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packLine.Shipment.JS_RL_NKOrigin = "AUSYD";
				packLine.Shipment.JS_RL_NKDestination = "GBNRW";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.JL_InspectionTypeCode = "VCK";
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				receiveShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "AUSYD" };
				receiveShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "GBNRW" };

				var universalPackingLine1 = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.ScreeningMethod = ZString.Empty;

				var universalPackingLine2 = CreatePacklineForReceive("PKG2", packLine.JL_PackLineId, 1);
				universalPackingLine2.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine2.ScreeningMethod = "VCK";

				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					universalPackingLine1,
					universalPackingLine2
				}));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(2, shipment.OuterPackLines.Count);

				var packLineAfterSplit = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_InspectionTypeCode == "VCK");
				AssertNotNull(packLineAfterSplit);
				Assert(!packLineAfterSplit.JL_InspectionTypeCodeInfo.ReadOnly);

				packLineAfterSplit = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_InspectionTypeCode == "UNK");
				AssertNotNull("UNK should be set as default value when packline's inspection type is empty", packLineAfterSplit);
				Assert(!packLineAfterSplit.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDispatch_UpdatePacklineInspection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				packlineBO1.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packlineBO1.Shipment.JS_RL_NKOrigin = "GBFXT";
				packlineBO1.Shipment.JS_RL_NKDestination = "AUSYD";
				packlineBO1.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packlineBO1.CopyValuesFromPackage(packlineBO1.PkgPackageCollection[0]);

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				dispatchShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "GBFXT" };
				dispatchShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

				var universalPackingLine = CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1);
				universalPackingLine.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine.ScreeningMethod = "VCK";

				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { universalPackingLine }));
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertEquals("JL_InspectionTypeCode has been updated", "VCK", shipment.OuterPackLines[0].JL_InspectionTypeCode);
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDispatch_SplitsBasedOnPackageScreeningMethod()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 4, true);
				packlineBO1.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				packlineBO1.Shipment.JS_RL_NKOrigin = "GBFXT";
				packlineBO1.Shipment.JS_RL_NKDestination = "AUSYD";
				packlineBO1.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packlineBO1.CopyValuesFromPackage(packlineBO1.PkgPackageCollection[0]);

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
				dispatchShipmentDataObject.PortOfOrigin = new UNLOCO { Code = "GBFXT" };
				dispatchShipmentDataObject.PortOfDischarge = new UNLOCO { Code = "AUSYD" };

				var universalPackingLine1 = CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1);
				universalPackingLine1.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine1.ScreeningMethod = "VCK";

				var universalPackingLine2 = CreatePacklineForDispatch("PKG2", packlineBO1.JL_PackLineId, 1);
				universalPackingLine2.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine2.ScreeningMethod = "CMD";

				var universalPackingLine3 = CreatePacklineForDispatch("PKG3", packlineBO1.JL_PackLineId, 1);
				universalPackingLine3.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine3.ScreeningMethod = ZString.Empty;

				var universalPackingLine4 = CreatePacklineForDispatch("PKG4", packlineBO1.JL_PackLineId, 1);
				universalPackingLine4.PackType = new PackageType() { Code = "PLT" };
				universalPackingLine4.ScreeningMethod = "VCK";

				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					universalPackingLine1,
					universalPackingLine2,
					universalPackingLine3,
					universalPackingLine4
				}));
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals(3, shipment.OuterPackLines.Count);
				AssertPackLineAfterSplit("VCK", 2);
				AssertPackLineAfterSplit("CMD", 1);
				AssertPackLineAfterSplit("UNK", 1);

				void AssertPackLineAfterSplit(ZString inspection, ZInt packageCount)
				{
					var packLineAfterSplit = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_InspectionTypeCode == inspection);
					AssertNotNull(packLineAfterSplit);

					AssertEquals(packageCount, packLineAfterSplit.JL_PackageCount);
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, packLineAfterSplit.JL_OriginTransitWarehouseStatus);
				}
			}
		}

		public void TestCopyValuesFromPackage_ForJL_InspectionTypeCode()
		{
			AssertCopyValuesFromPackage_ForJL_InspectionTypeCode(Core.Constants.TransportModes.Air, "UNK", "XRY");
			AssertCopyValuesFromPackage_ForJL_InspectionTypeCode(Core.Constants.TransportModes.Sea, ZString.Empty, ZString.Empty);

			void AssertCopyValuesFromPackage_ForJL_InspectionTypeCode(ZString transportMode, ZString expectedInspectionTypeCode1, ZString expectedInspectionTypeCode2)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_TransportMode = transportMode;
					shipment.JS_RL_NKOrigin = "GBFXT";
					shipment.JS_RL_NKDestination = "AUSYD";

					var packLine = shipment.OuterPackLines.AddNew();
					AssertEquals(transportMode != Core.Constants.TransportModes.Air, packLine.JL_InspectionTypeCodeInfo.ReadOnly);

					var packageParent = Factory.New<DummyBusinessObject>();

					var packageJob = Factory.New<ForwardingPackageJob>();
					packageJob.KJ_ParentID = packageParent.PK;
					packageJob.KJ_ParentTableCode = packageParent.TablePrefix;

					var package = packLine.PkgPackageCollection.AddNew();
					package.KP_KJ_ParentPackageJob = packageJob.PK;
					package.KP_F3_NKPackType = Core.Constants.PkgUnit.Container;
					package.SetScreeningMethod("XRY");
					AssertEquals(expectedInspectionTypeCode1, packLine.JL_InspectionTypeCode);

					packLine.CopyValuesFromPackage(package);
					AssertEquals(expectedInspectionTypeCode2, packLine.JL_InspectionTypeCode);
				}
			}
		}

		#endregion

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsOnPackType_WithSameContainerNumber()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 2, true);
				var consol = packLine.Shipment.Consols[0];
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONT0001";

				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.PkgPackageCollection[1].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.SetContainer(packLine.Shipment.Consols[0], container);
				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);
				receiveShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } });
				receiveShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(Enumerable.Range(1, 2).Select(i =>
				{
					var packingLine = CreatePacklineForReceive("PKG" + i, packLine.JL_PackLineId, 1, containerLink: 1);
					packingLine.PackType = new PackageType() { Code = i == 1 ? "PLT" : "ZZZ" };
					return packingLine;
				})));
				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("packline was split", 2, shipment.OuterPackLines.Count);
				CombineAssertions(() =>
				{
					AssertEquals("container is on pack1", "CONT0001", shipment.OuterPackLines[0].JL_Calc_ContainerNumber);
					AssertEquals("container is on pack2", "CONT0001", shipment.OuterPackLines[1].JL_Calc_ContainerNumber);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsOnRCNForFrance_WithDifferentRCNsAndWarehouse()
		{
			var warehouseAddressBO1 = Factory.NewWithValidTestData<OrgAddress>();
			warehouseAddressBO1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var warehouseAddressBO2 = Factory.NewWithValidTestData<OrgAddress>();
			warehouseAddressBO2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packLine = PreparePackLineWithPackages(warehouseAddressBO1, 1, true);
				packLine.JL_OA_LastKnownTransitWarehouseAddress = warehouseAddressBO2.PK;
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO1.PK;

				packLine.AdditionalReferenceNumbers.RemoveAndDeleteAll();
				var ercNumber = packLine.AdditionalReferenceNumbers.AddNewIfNotExist(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC, "RC00099");
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO1);

				receiveShipmentDataObject.SetPortReferenceCollection(() =>
				{
					return new List<PortReference>()
					{
						new PortReference
						{
							Type = new PortReferenceType
							{
								Code = "PAN",
								Description = "Port Authority Number"
							},
							Reference = "P00001",
							Country = new Country
							{
								Code = "FR",
								Name = "France"
							}
						}
					};
				});

				var packlines = new DataObjectList<PackingLine>();

				var packingLine = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				packingLine.PackType = new PackageType() { Code = "PLT" };

				packlines.Add(packingLine);

				receiveShipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("packline was combined", 1, shipment.OuterPackLines.Count);

				CombineAssertions(() =>
				{
					AssertEquals("Keep the original packline", 1, shipment.OuterPackLines.Count);
					AssertEquals("PAN is extracted to AdditionalReferenceNumbers", "P00001", packLine.PortReferences.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN).CE_EntryNum);
					AssertEquals("JL_ExportRefNumber is populated from PAN", "P00001", packLine.JL_ExportRefNumber);
					AssertEquals("Keep the previous RCN", "RC00099", packLine.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsOnRCNForFrance_WithDifferentRCNsButSameWarehouse()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			warehouseAddressBO.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				packLine.JL_OA_LastKnownTransitWarehouseAddress = warehouseAddressBO.PK;
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;

				packLine.AdditionalReferenceNumbers.RemoveAndDeleteAll();
				var ercNumber = packLine.AdditionalReferenceNumbers.AddNewIfNotExist(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC, "RC00099");
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);

				receiveShipmentDataObject.SetPortReferenceCollection(() =>
				{
					return new List<PortReference>()
					{
						new PortReference
						{
							Type = new PortReferenceType
							{
								Code = "PAN",
								Description = "Port Authority Number"
							},
							Reference = "P00001",
							Country = new Country
							{
								Code = "FR",
								Name = "France"
							}
						}
					};
				});

				var packlines = new DataObjectList<PackingLine>();

				var packingLine = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				packingLine.PackType = new PackageType() { Code = "PLT" };

				packlines.Add(packingLine);

				receiveShipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("packline was combined", 2, shipment.OuterPackLines.Count);

				var anotherPackLine = (ForwardingPackLine)shipment.OuterPackLines.First(p => p.PK != packLine.PK);

				CombineAssertions(() =>
				{
					Assert("Keep the original one", !packLine.PortReferences.Cast<ICusEntryNumber>().Any(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN));
					AssertEquals("Keep the original one", string.Empty, packLine.JL_ExportRefNumber);
					AssertEquals("Keep the original RCN", "RC00099", packLine.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum);

					AssertEquals("PAN is extracted to AdditionalReferenceNumbers", "P00001", anotherPackLine.PortReferences.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN).CE_EntryNum);
					AssertEquals("JL_ExportRefNumber is populated from PAN", "P00001", anotherPackLine.JL_ExportRefNumber);
					AssertEquals("Keep the previous RCN", "TR00001", anotherPackLine.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsOnRCNForFrance_WithRemnantPackagesAndDifferentWarehouse()
		{
			var warehouseAddressBO1 = Factory.NewWithValidTestData<OrgAddress>();
			warehouseAddressBO1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var warehouseAddressBO2 = Factory.NewWithValidTestData<OrgAddress>();
			warehouseAddressBO2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packLine = PreparePackLineWithPackages(warehouseAddressBO1, 2, true);
				packLine.JL_OA_LastKnownTransitWarehouseAddress = warehouseAddressBO2.PK;
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO1.PK;

				packLine.AdditionalReferenceNumbers.RemoveAndDeleteAll();
				var ercNumber = packLine.AdditionalReferenceNumbers.AddNewIfNotExist(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC, "RC00099");
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO1);

				receiveShipmentDataObject.SetPortReferenceCollection(() =>
				{
					return new List<PortReference>()
					{
						new PortReference
						{
							Type = new PortReferenceType
							{
								Code = "PAN",
								Description = "Port Authority Number"
							},
							Reference = "P00001",
							Country = new Country
							{
								Code = "FR",
								Name = "France"
							}
						}
					};
				});

				var packlines = new DataObjectList<PackingLine>();

				var packingLine = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				packingLine.PackType = new PackageType() { Code = "PLT" };

				packlines.Add(packingLine);

				receiveShipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("packline was split", 2, shipment.OuterPackLines.Count);

				var anotherPackLine = (ForwardingPackLine)shipment.OuterPackLines.First(p => p.PK != packLine.PK);

				CombineAssertions(() =>
				{
					Assert("Keep the original one", !packLine.PortReferences.Cast<ICusEntryNumber>().Any(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN));
					AssertEquals("Keep the original one", string.Empty, packLine.JL_ExportRefNumber);
					AssertEquals("Keep the previous RCN", "RC00099", packLine.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum);

					AssertEquals("PAN is extracted to AdditionalReferenceNumbers", "P00001", anotherPackLine.PortReferences.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN).CE_EntryNum);
					AssertEquals("JL_ExportRefNumber is populated from PAN", "P00001", anotherPackLine.JL_ExportRefNumber);
					AssertEquals("Keep the previous RCN", "RC00099", anotherPackLine.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestReceipt_SplitsOnRCNForFrance_WithoutRCNOnForwardingPackLine()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			warehouseAddressBO.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				packLine.JL_OA_LastKnownTransitWarehouseAddress = warehouseAddressBO.PK;
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.JL_F3_NKPackType = "PLT";
				packLine.Shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				packLine.AdditionalReferenceNumbers.RemoveAndDeleteAll();

				Factory.SaveForTesting();

				var receiveShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitReceive, warehouseAddressBO);

				receiveShipmentDataObject.SetPortReferenceCollection(() =>
				{
					return new List<PortReference>()
					{
						new PortReference
						{
							Type = new PortReferenceType
							{
								Code = "PAN",
								Description = "Port Authority Number"
							},
							Reference = "P00001",
							Country = new Country
							{
								Code = "FR",
								Name = "France"
							}
						}
					};
				});

				var packlines = new DataObjectList<PackingLine>();

				var packingLine = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				packingLine.PackType = new PackageType() { Code = "PLT" };

				packlines.Add(packingLine);

				receiveShipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(receiveShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("packline was split", 2, shipment.OuterPackLines.Count);
				var anotherPackLine = (ForwardingPackLine)shipment.OuterPackLines.First(p => p.PK != packLine.PK);

				CombineAssertions(() =>
				{
					Assert("Keep the original one", !packLine.PortReferences.Cast<ICusEntryNumber>().Any(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN));
					AssertEquals("Keep the original one", string.Empty, packLine.JL_ExportRefNumber);
					Assert("Keep the original one", !packLine.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Any(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC));

					AssertEquals("PAN is extracted to AdditionalReferenceNumbers", "P00001", anotherPackLine.PortReferences.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN).CE_EntryNum);
					AssertEquals("JL_ExportRefNumber is populated from PAN", "P00001", anotherPackLine.JL_ExportRefNumber);
					AssertEquals("Keep the previous RCN", "TR00001", anotherPackLine.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDispatch_SplitsOnPackType()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packlineBO1 = PreparePackLineWithPackages(warehouseAddressBO, 2, true);
				packlineBO1.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packlineBO1.PkgPackageCollection[1].KP_F3_NKPackType = "PLT";
				packlineBO1.CopyValuesFromPackage(packlineBO1.PkgPackageCollection[0]);

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packlineBO1.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);

				var packlines = new DataObjectList<PackingLine>();
				var packingLine1 = CreatePacklineForDispatch("PKG1", packlineBO1.JL_PackLineId, 1);
				packingLine1.PackType = new PackageType() { Code = "PLT" };
				var packingLine2 = CreatePacklineForDispatch("PKG2", packlineBO1.JL_PackLineId, 1);
				packingLine1.PackType = new PackageType() { Code = "ZZZ" };
				packlines.Add(packingLine1);
				packlines.Add(packingLine2);
				dispatchShipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals("packline was split", 2, shipment.OuterPackLines.Count);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestDispatch_SplitsOnPackType_WithSameContainerNumber()
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			Factory.SaveForTesting();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 2, true);
				var consol = packLine.Shipment.Consols[0];
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONT0001";
				packLine.PkgPackageCollection[0].KP_F3_NKPackType = "PLT";
				packLine.PkgPackageCollection[1].KP_F3_NKPackType = "PLT";
				packLine.CopyValuesFromPackage(packLine.PkgPackageCollection[0]);
				packLine.SetContainer(packLine.Shipment.Consols[0], container);

				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(packLine.Shipment.Consols[0], DataContextType.TransitDispatch, warehouseAddressBO);
				dispatchShipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } });

				var packlines = new DataObjectList<PackingLine>();
				var packingLine1 = CreatePacklineForDispatch("PKG1", packLine.JL_PackLineId, 1, containerLink: 1);
				packingLine1.PackType = new PackageType() { Code = "PLT" };
				var packingLine2 = CreatePacklineForDispatch("PKG2", packLine.JL_PackLineId, 1, containerLink: 1);
				packingLine1.PackType = new PackageType() { Code = "ZZZ" };
				packlines.Add(packingLine1);
				packlines.Add(packingLine2);
				dispatchShipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();
				var shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals("packline was split", 2, shipment.OuterPackLines.Count);
				CombineAssertions(() =>
				{
					AssertEquals("container is on pack1", "CONT0001", shipment.OuterPackLines[0].JL_Calc_ContainerNumber);
					AssertEquals("container is on pack2", "CONT0001", shipment.OuterPackLines[1].JL_Calc_ContainerNumber);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestSurplusReceipt_NoRef()
		{
			AssertSurplus_NoRef(true);
		}

		[TestDate(2021, 1, 1)]
		public void TestSurplusDispatch_NoRef()
		{
			AssertSurplus_NoRef(false);
		}

		void AssertSurplus_NoRef(bool isForReceive)
		{
			var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = PrepareShipment(warehouseAddressBO);
				Factory.SaveForTesting();

				var shipmentDataObject = BuildUniversalShipmentWithAddress(shipment.Consols[0], isForReceive ? DataContextType.TransitReceive : DataContextType.TransitDispatch, warehouseAddressBO);

				var packingLine = isForReceive
					? CreatePacklineForReceive("PKG1", "0000000", 1)
					: CreatePacklineForDispatch("PKG1", "0000000", 1);

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine }));
				var newestShipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var newPackLine = newestShipment.OuterPackLines[0];
				CombineAssertions(() =>
				{
					AssertEquals("1 outerpackline", 1, newestShipment.OuterPackLines.Count);
					AssertEquals("1 pack", 1, newPackLine.PkgPackageCollection.Count);
					AssertEquals("JL_OriginTransitWarehouseStatus", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus, newPackLine.JL_OriginTransitWarehouseStatus);
					AssertEquals("JL_RefNumber", string.Empty, newPackLine.JL_RefNumber);
				});
			}
		}

		public void TestDoNotImportTransportLegsFromTransitDispatch()
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
				var shipmentDataObject = BuildUniversalShipmentWithAddress(consol, DataContextType.TransitDispatch, warehouseAddressBO);
				shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
				var transportLegs = shipmentDataObject.TransportLegCollection;
				transportLegs.Add(new TransportLeg { LegOrder = 1, PortOfLoading = new UNLOCO { Code = "Syd" }, PortOfDischarge = new UNLOCO { Code = "sin" } });
				transportLegs.Add(new TransportLeg { LegOrder = 2, PortOfLoading = new UNLOCO { Code = "SIN" }, PortOfDischarge = new UNLOCO { Code = "laX" } });
				AssertNoExceptionThrown(() => new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject());
				AssertNotContains("Populating Transport...", Logger.Logs);
			}
		}

		#region PrepareDispatch

		ForwardingPackLine PreparePackLineWithBlindPackages(ForwardingShipment shipment, OrgAddress warehouseAddressBO, string packageID, PkgPackageJob pkgPackageJob)
		{
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received;
			packLine.JL_OA_LastKnownTransitWarehouseAddress = warehouseAddressBO.PK;
			packLine.JL_LastKnownTransitWarehouseStatusDateTime = ZDateTime.Today;
			packLine.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;

			var pkgPackage = packLine.PkgPackageCollection.AddNew();
			pkgPackage.KP_F3_NKPackType = "PLT";
			pkgPackage.KP_DimensionUQ = "M";
			pkgPackage.KP_Height = 0;
			pkgPackage.KP_Width = 0;
			pkgPackage.KP_Length = 0;
			pkgPackage.KP_PackageQty = 1;
			pkgPackage.KP_Weight = 0;
			pkgPackage.KP_Volume = 0;
			pkgPackage.KP_KJ_ParentPackageJob = pkgPackageJob.PK;
			pkgPackage.KP_PackageID = packageID;

			return packLine;
		}

		[TestDate(2021, 1, 1)]
		public void TestProcessPrepareDispatch()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";
				shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				var packageJob = Factory.New<PkgPackageJob>();
				packageJob.KJ_ParentID = shipment.PK;
				packageJob.KJ_ParentTableCode = shipment.TablePrefix;
				PreparePackLineWithBlindPackages(shipment, warehouseAddressBO, "PKG1", packageJob);

				Factory.SaveForTesting();

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(null, DataContextType.TransitDispatch, warehouseAddressBO);
				var packingLineDO = CreatePacklineForReceive("PKG1", shipment.OuterPackLines[0].JL_PackLineId, 1);
				packingLineDO.Weight = 10;
				packingLineDO.Volume = 10;
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDO }));
				shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				CombineAssertions("PackLine's package is updated", () =>
				{
					var pkgPackage = shipment.OuterPackLines[0].PkgPackageCollection[0];
					AssertEquals(packingLineDO.Weight, pkgPackage.KP_Weight);
					AssertEquals(packingLineDO.WeightUnit.Code, pkgPackage.KP_WeightUQ);
					AssertEquals(packingLineDO.Volume, pkgPackage.KP_Volume);
					AssertEquals(packingLineDO.VolumeUnit.Code, pkgPackage.KP_VolumeUQ);
					AssertEquals(packingLineDO.Length, pkgPackage.KP_Length);
					AssertEquals(packingLineDO.Width, pkgPackage.KP_Width);
					AssertEquals(packingLineDO.Height, pkgPackage.KP_Height);
				});

				CombineAssertions("PackLine is updated", () =>
				{
					var packLine = shipment.OuterPackLines[0];
					AssertEquals("Last Known TW Status is RCV", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, packLine.JL_LastKnownTransitWarehouseStatus);
					AssertEquals("Status is set to DIS", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packLine.JL_OriginTransitWarehouseStatus);
					AssertEquals("Status DateTime is unchanged", TestDateAttribute.Date, packLine.JL_LastKnownTransitWarehouseStatusDateTime);
				});
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestProcessPrepareDispatch_BlindPackagesAndExpectedPackages()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				var packLine = PreparePackLineWithPackages(warehouseAddressBO, 1, true);
				var shipment = packLine.Shipment;
				var blindPackLine = PreparePackLineWithBlindPackages(shipment, warehouseAddressBO, "PKG2", packLine.PkgPackageCollection[0].PackageJob);

				Factory.SaveForTesting();

				TestDateAttribute.AddDays(1);
				var dispatchShipmentDataObject = BuildUniversalShipmentWithAddress(null, DataContextType.TransitDispatch, warehouseAddressBO);
				var packingLineDO = CreatePacklineForReceive("PKG1", packLine.JL_PackLineId, 1);
				packingLineDO.Weight = 20;
				packingLineDO.Volume = 20;
				var blindPackingLineDO = CreatePacklineForReceive("PKG2", blindPackLine.JL_PackLineId, 1);
				blindPackingLineDO.Weight = 10;
				blindPackingLineDO.Volume = 10;
				dispatchShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDO, blindPackingLineDO }));
				shipment = new ShipmentDataObjectReader(dispatchShipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				CombineAssertions("PackLines packages are updated", () =>
				{
					var pkgPackage = shipment.OuterPackLines[0].PkgPackageCollection[0];
					AssertEquals(packingLineDO.Weight, pkgPackage.KP_Weight);
					AssertEquals(packingLineDO.WeightUnit.Code, pkgPackage.KP_WeightUQ);
					AssertEquals(packingLineDO.Volume, pkgPackage.KP_Volume);
					AssertEquals(packingLineDO.VolumeUnit.Code, pkgPackage.KP_VolumeUQ);
					AssertEquals(packingLineDO.Length, pkgPackage.KP_Length);
					AssertEquals(packingLineDO.Width, pkgPackage.KP_Width);
					AssertEquals(packingLineDO.Height, pkgPackage.KP_Height);

					var blindPkgPackage = shipment.OuterPackLines[1].PkgPackageCollection[0];
					AssertEquals(blindPackingLineDO.Weight, blindPkgPackage.KP_Weight);
					AssertEquals(blindPackingLineDO.WeightUnit.Code, blindPkgPackage.KP_WeightUQ);
					AssertEquals(blindPackingLineDO.Volume, blindPkgPackage.KP_Volume);
					AssertEquals(blindPackingLineDO.VolumeUnit.Code, blindPkgPackage.KP_VolumeUQ);
					AssertEquals(blindPackingLineDO.Length, blindPkgPackage.KP_Length);
					AssertEquals(blindPackingLineDO.Width, blindPkgPackage.KP_Width);
					AssertEquals(blindPackingLineDO.Height, blindPkgPackage.KP_Height);
				});

				CombineAssertions("PackLines are updated", () =>
				{
					packLine = shipment.OuterPackLines[0];
					AssertEquals("Last Known TW Status is RCV", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, packLine.JL_LastKnownTransitWarehouseStatus);
					AssertEquals("Status is set to DIS", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packLine.JL_OriginTransitWarehouseStatus);
					AssertEquals("Status DateTime is unchanged", TestDateAttribute.Date, packLine.JL_LastKnownTransitWarehouseStatusDateTime);

					blindPackLine = shipment.OuterPackLines[1];
					AssertEquals("Last Known TW Status is RCV", FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, blindPackLine.JL_LastKnownTransitWarehouseStatus);
					AssertEquals("Status is set to DIS", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, blindPackLine.JL_OriginTransitWarehouseStatus);
					AssertEquals("Status DateTime is unchanged", TestDateAttribute.Date, blindPackLine.JL_LastKnownTransitWarehouseStatusDateTime);
				});
			}
		}

		#endregion
	}
}
