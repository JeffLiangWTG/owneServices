using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitReceiveConsignmentDataObjectReaderTest : WhsTransitConsignmentDataObjectReaderTest<WhsItemReceiveConsignment, WhsTransitReceiveConsignmentDataObjectReader>
	{
		#region TestImportIsRejectedIfNoWarehouseProvided

		public void TestImportIsRejectedIfNoWarehouseProvided()
		{
			var poke1 = Data.Orgs.INTHEMSYD;
			var poke2 = Data.Orgs.WUFSHIJNB;

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } } });

			// no warehouse with the supplied address
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot import without a valid Warehouse supplied. Please ensure your Arrival CFS Address 'INTHEMSYD - Unit 12, Level 3' has an active Transit Warehouse.",
				() => GetDataObjectReader(new UniversalObjectFactory(), Data.ShipmentDataObject).ReadIntoBusinessObject());

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			// no warehouse with the supplied address
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot import without a valid Warehouse supplied. Please ensure your Departure CFS Address 'WUFSHIJNB - Level 2, Building G' has an active Transit Warehouse.",
				() => GetDataObjectReader(new UniversalObjectFactory(), Data.ShipmentDataObject).ReadIntoBusinessObject());

			// no departure cfs address in the UXML
			var poke3 = Data.Warehouse;
			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD });
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot import without a valid Warehouse supplied. Please ensure there is a valid departure/arrival CFS address and that you targeted the correct recipient type.",
				() => GetDataObjectReader(new UniversalObjectFactory(), Data.ShipmentDataObject).ReadIntoBusinessObject());

			// no transit warehouse with the supplied address
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.Warehouse_WUFSHIJNB);
			Data.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Data.Warehouse[WhsWarehouseSchema.Constants.WW_IsVirtualWarehouse] = true; // to prevent check constraint violation
			Factory.SaveForTesting();
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot import without a valid Warehouse supplied. Please ensure your Departure CFS Address 'WUFSHIJNB - Level 2, Building G' has an active Transit Warehouse.",
				() => GetDataObjectReader(new UniversalObjectFactory(), Data.ShipmentDataObject).ReadIntoBusinessObject());

			Data.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Data.Warehouse.WW_IsVirtualWarehouse = false;
			Factory.SaveForTesting();
			AssertNoExceptionThrown(() => GetDataObjectReader(new UniversalObjectFactory(), Data.ShipmentDataObject).ReadIntoBusinessObject());
		}

		#endregion

		#region TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_MixedPackagesWithIDsAndNoIDs

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_MixedPackagesWithIDsAndNoIDs()
		{
			Data.SetupForForwardingImport();
			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 1 package on the package Job.", 1, packageJob.Packages.Count);
			AssertEquals("Precondition: Should have no errors.", false, Logger.HasErrors);

			Data.ShipmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "PKG-1", PackQty = 1, PackType = new PackageType { Code = Constants.PkgUnit.Package } });
			GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			AssertEquals("Should not have updated receive consignment.", 1, packageJob.Packages.Count);
			AssertEquals(@"Cannot populate Receive Consignment RC00000001 because:
A mix of Packages with and without Package IDs was provided for Receive Consignment 'RC00000001 - Waybill123'. If any Package ID is provided then all Packages must have IDs.
Either add or remove all Package IDs and send again.", Logger.GetErrors());
		}

		#endregion

		#region TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_MixedPackagesWithIDsAndNoIDs_HasLoosePackageIDs

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_MixedPackagesWithIDsAndNoIDs_HasLoosePackageIDs()
		{
			Data.SetupForForwardingImport();
			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 1 package on the package Job.", 1, packageJob.Packages.Count);
			AssertEquals("Precondition: Should have no errors.", false, Logger.HasErrors);

			Data.ShipmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "PLT-1", PackQty = 1, PackType = new PackageType { Code = Constants.PkgUnit.Pallet } });
			Data.ShipmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "LP-1", PackQty = 1 });
			GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();

			AssertEquals("Should have updated receive consignment, previous booked packages have been deleted and 2 new packages have been imported.", 2, packageJob.Packages.Count);
			AssertPackageInfo(packageJob.Packages.Single(p => p.KP_PackageID == "PLT-1"), Constants.PkgUnit.Pallet, 1, 0m, 0m);
			AssertPackageInfo(packageJob.Packages.Single(p => p.KP_PackageID == "LP-1"), Constants.PkgUnit.Package, 1, 0m, 0m);
			AssertEquals("Should have updated receive consignment without error.", ZString.Empty, Logger.GetErrors());
		}

		#endregion

		#region TestPopulateBusinessObject_WithConsol

		public void TestPopulateBusinessObject_WithConsol()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			Data.HeaderDataObject.SubShipmentCollection.First().PackingLineCollection.First().IsHighRisk = true;
			AssertPopulateSucceeded(Data.HeaderDataObject);
		}

		#endregion

		#region TestPopulateBusinessObject_WithoutConsol

		public void TestPopulateBusinessObject_WithoutConsol()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } }, TriggerDate = new ZDateTimeOffset(2000, 1, 1) });
			Data.ShipmentDataObject.PackingLineCollection.First().IsHighRisk = true;
			AssertPopulateSucceeded(Data.ShipmentDataObject);
		}

		#endregion

		#region TestExpectedArrivalAndDepartures

		public void TestExpectedArrivalAndDispatch() => ExpectedArrivalAndDispatchCore(false);
		public void TestExpectedArrivalAndDispatch_UseExtraPort() => ExpectedArrivalAndDispatchCore(true);

		void ExpectedArrivalAndDispatchCore(bool useExtraPort)
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var port2 = "AUSYD";

			Data.SetupForForwardingImport();

			if (useExtraPort)
			{
				UniversalHelper.CreateExtraPort(Data.Warehouse, homePort);
			}

			var consol = Data.HeaderDataObject;
			var outboundConsolRoutingLeg = new TransportLeg
			{
				EstimatedArrival = new ZDateTime(2000, 1, 1),
				EstimatedDeparture = new ZDateTime(2001, 2, 2)
			};
			var inboundConsolRoutingLeg = new TransportLeg
			{
				EstimatedArrival = new ZDateTime(2002, 3, 3),
				EstimatedDeparture = new ZDateTime(2003, 4, 4)
			};
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { outboundConsolRoutingLeg, inboundConsolRoutingLeg });

			var shipment = consol.SubShipmentCollection[0];
			var outboundShipmentRoutingLeg = new TransportLeg
			{
				EstimatedArrival = new ZDateTime(2004, 1, 1),
				EstimatedDeparture = new ZDateTime(2005, 2, 2)
			};
			var inboundShipmentRoutingLeg = new TransportLeg
			{
				EstimatedArrival = new ZDateTime(2006, 7, 7),
				EstimatedDeparture = new ZDateTime(2007, 8, 8)
			};
			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { outboundShipmentRoutingLeg, inboundShipmentRoutingLeg });

			var consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertExpectedArrivalAndDispatch(consignment, ZDateTime.Empty, ZDateTime.Empty);

			outboundConsolRoutingLeg.PortOfLoading = new UNLOCO { Code = homePort };
			outboundShipmentRoutingLeg.PortOfLoading = new UNLOCO { Code = port2 };
			inboundConsolRoutingLeg.PortOfDischarge = new UNLOCO { Code = homePort };
			inboundShipmentRoutingLeg.PortOfDischarge = new UNLOCO { Code = port2 };
			consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertExpectedArrivalAndDispatch(consignment, new ZDateTime(2002, 3, 3), new ZDateTime(2001, 2, 2));

			outboundConsolRoutingLeg.PortOfLoading = new UNLOCO { Code = port2 };
			outboundShipmentRoutingLeg.PortOfLoading = new UNLOCO { Code = homePort };
			inboundConsolRoutingLeg.PortOfDischarge = new UNLOCO { Code = port2 };
			inboundShipmentRoutingLeg.PortOfDischarge = new UNLOCO { Code = homePort };
			consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertExpectedArrivalAndDispatch(consignment, ZDateTime.Empty, ZDateTime.Empty);

			if (!useExtraPort)
			{
				Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "AUMEL";
				consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
				AssertExpectedArrivalAndDispatch(consignment, ZDateTime.Empty, ZDateTime.Empty);
			}
		}

		void AssertExpectedArrivalAndDispatch(WhsItemReceiveConsignment consignment, ZDateTime expectedArrival, ZDateTime expectedDispatch)
		{
			var assertionMessage = !expectedArrival.IsEmpty ? "Transit Warehouse Receive Consignment Expected Arrival and Dispatch Times should NOT be empty." : "Transit Warehouse Receive Consignment Expected Arrival and Dispatch Times should be empty.";

			CombineAssertions(assertionMessage, () =>
			{
				AssertEquals("Consignment.WRC_ExpectedArrivalTime should be " + expectedArrival, expectedArrival, consignment.WRC_ExpectedArrivalTime);
				AssertEquals("Consignment.WRC_ExpectedDispatchTime should be " + expectedDispatch, expectedDispatch, consignment.WRC_ExpectedDispatchTime);
			});
		}

		public void TestExpectedArrival_ShipmentWithConsol_ShouldTakeShipmentPickupDetailsAsFallback()
		{
			Data.SetupForForwardingImport();
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var consol = Data.HeaderDataObject;
			var inboundRoutingLeg = Helper.CreateTransportLeg(portOfDischarge: homePort);
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { inboundRoutingLeg });

			var estimatedPickupDate = new ZDateTime(2006, 7, 7);
			var shipment = consol.SubShipmentCollection[0];
			Helper.AddTestDataToShipment(shipment, inboundLeg: inboundRoutingLeg, portOfOrigin: homePort, estimatedPickup: estimatedPickupDate);

			var consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertEquals(
				"RCN ETA takes Estimated Pickup date (Port Of Origin has to be a related port too) as there is no Inbound Leg ETA",
				estimatedPickupDate,
				consignment.WRC_ExpectedArrivalTime
			);

			var pickupRequiredFromDate = new ZDateTime(2006, 8, 8);
			shipment.LocalProcessing = new LocalProcessing();
			shipment.LocalProcessing.PickupRequiredFrom = pickupRequiredFromDate;

			consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertEquals(
				"RCN ETA takes Pickup Required From (Port Of Origin has to be a related port too) as there is no Inbound Leg ETA or Estimated Pickup",
				pickupRequiredFromDate,
				consignment.WRC_ExpectedArrivalTime
			);
		}

		public void TestExpectedArrival_NonOriginWarehouse_ShouldTakeCFSReceivalAsFallback() =>
			TestExpectedArrival_ShouldTakeCFSReceivalAsFallback(originPortCode: "ABC");

		public void TestExpectedArrival_OriginWarehouse_ShouldTakeCFSReceivalAsFallback() =>
			TestExpectedArrival_ShouldTakeCFSReceivalAsFallback(originPortCode: null);

		// If the originPortCode is not provided or the the same as the homePort, we will test scenario when we dont have a 'Origin Warehouse'
		// Theoretically, this function should pass using any argument
		void TestExpectedArrival_ShouldTakeCFSReceivalAsFallback(string originPortCode)
		{
			Data.SetupForForwardingImport();
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var consol = Data.HeaderDataObject;
			var cfsReceivalDate = new ZDateTime(2006, 9, 9);
			var inboundRoutingLeg = Helper.CreateTransportLeg(portOfDischarge: homePort);
			var outboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, cfsReceivalDate: cfsReceivalDate);
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { inboundRoutingLeg, outboundRoutingLeg });

			var shipment = consol.SubShipmentCollection[0];
			Helper.AddTestDataToShipment(shipment, portOfOrigin: originPortCode ?? homePort, inboundLeg: inboundRoutingLeg, outboundLeg: outboundRoutingLeg,
				estimatedPickup: ZDateTime.Empty, pickupRequiredFrom: null); // Test that Empty DateTime and nulls are ignored

			var consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertEquals(
				"RCN ETA fallsback to CFS Receival Date if no Inbound ETA or Shipment Estimated Pickup or Pickup Required From dates",
				cfsReceivalDate,
				consignment.WRC_ExpectedArrivalTime
			);
		}

		public void TestExpectedDispatch_ShouldTakeDesinationWarehouseDetails()
		{
			Data.SetupForForwardingImport();
			var homePort = Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort;
			var consol = Data.HeaderDataObject;
			var shipment = consol.SubShipmentCollection[0];
			var estimatedDelivery = new ZDateTime(2006, 7, 7);
			Helper.AddTestDataToShipment(shipment, portOfDestination: homePort, estimatedDelivery: estimatedDelivery);

			var consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertEquals("RCN ETD should take the Estimated Delivery date when Shipment Port of Destination is a related port", estimatedDelivery, consignment.WRC_ExpectedDispatchTime);

			var deliveryRequiredByDate = new ZDateTime(2006, 8, 8);
			shipment.LocalProcessing = new LocalProcessing();
			shipment.LocalProcessing.DeliveryRequiredBy = deliveryRequiredByDate;

			consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertEquals(
				"RCN ETD should take the Delivery Required By date if Shipment Port of Destination is a related port and there was no Estimated Delivery date",
				deliveryRequiredByDate,
				consignment.WRC_ExpectedDispatchTime
			);
		}

		public void TestExpectedDispatch_NonDestinationWarehouse_ShouldTakeOutboundRoutingDetailsAsFallback() =>
			TestExpectedDispatch_ShouldTakeOutboundRoutingDetailsAsFallback(destinationPortCode: "ABC");

		public void TestExpectedDispatch_DestinationWarehouse_ShouldTakeOutboundRoutingDetailsAsFallback() =>
			TestExpectedDispatch_ShouldTakeOutboundRoutingDetailsAsFallback(destinationPortCode: null);

		// If the destinationPortCode is null or not the same as homePort, we will test scenario when we dont have a 'Destination Warehouse'
		// Theoretically, this function should pass using any argument
		void TestExpectedDispatch_ShouldTakeOutboundRoutingDetailsAsFallback(string destinationPortCode)
		{
			Data.SetupForForwardingImport();
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var consol = Data.HeaderDataObject;
			var ctoCutOffDate = new ZDateTime(2005, 5, 5);
			var outboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, ctoCutOffDate: ctoCutOffDate);
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { outboundRoutingLeg });

			var shipment = consol.SubShipmentCollection[0];
			Helper.AddTestDataToShipment(shipment, outboundLeg: outboundRoutingLeg, portOfDestination: destinationPortCode ?? homePort,
				estimatedDelivery: ZDateTime.Empty, deliveryRequiredBy: null); // Test that Empty DateTime and nulls are ignored

			var consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertEquals("RCN ETD will use CTO Cut Off date if there is no Shipment with Port of Destination as a related port", ctoCutOffDate, consignment.WRC_ExpectedDispatchTime);

			outboundRoutingLeg.FCLCutOff = null;
			var etd = new ZDateTime(2006, 8, 8);
			outboundRoutingLeg.EstimatedDeparture = etd;

			consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertEquals("RCN ETD will use ETD date if there is no Shipment with Port of Destination as a related port and CTO Cut Off is empty", etd, consignment.WRC_ExpectedDispatchTime);
		}

		#endregion

		#region TestNextDischargePort

		public void TestNextDischargePort() => NextDischargePortCore(false);
		public void TestNextDischargePort_UseExtraPort() => NextDischargePortCore(true);

		void NextDischargePortCore(bool useExtraPort)
		{
			var portOfDischarge = "NZAKL";
			var relatedPortCode = "AUSYD";
			var portOfDestination = "AUPER";

			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.PortOfDestination = new UNLOCO { Code = portOfDestination };
			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = relatedPortCode;

			if (useExtraPort)
			{
				UniversalHelper.CreateExtraPort(Data.Warehouse, relatedPortCode);
			}

			var header = Data.HeaderDataObject;
			header.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } }, TriggerDate = ZDateTimeOffset.Now });
			var transportLeg1 = header.TransportLegCollection[0];
			transportLeg1.PortOfLoading = new UNLOCO { Code = relatedPortCode };
			transportLeg1.PortOfDischarge = new UNLOCO { Code = portOfDischarge };

			header.TransportLegCollection.Add(new TransportLeg
			{
				PortOfLoading = new UNLOCO { Code = "AUBNE" },
				PortOfDischarge = new UNLOCO { Code = "US2CW" }
			});

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			AssertEquals("Consignment.NextDischargePort should be " + portOfDischarge, portOfDischarge, consignment.WRC_RL_NKNextDischargePort);

			transportLeg1.PortOfLoading = new UNLOCO { Code = "AUBNE" };
			reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
			var updatedConsignment = reader.ReadIntoBusinessObject();
			AssertEquals("Consignment.NextDischargePort should be Shipment Destination Port, as PortOfLoading doesn't match with Warehouse.RelatedPort.", portOfDestination, updatedConsignment.WRC_RL_NKNextDischargePort);

			transportLeg1.PortOfLoading = new UNLOCO { Code = "AUSYD" };
			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "TEST1";
			reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
			updatedConsignment = reader.ReadIntoBusinessObject();
			if (useExtraPort)
			{
				AssertEquals("Consignment.NextDischargePort should be " + portOfDischarge, portOfDischarge, updatedConsignment.WRC_RL_NKNextDischargePort);
			}
			else
			{
				AssertEquals("Consignment.NextDischargePort should be Shipment Destination Port, as PortOfLoading doesn't match with Warehouse.RelatedPort.", portOfDestination, updatedConsignment.WRC_RL_NKNextDischargePort);
			}

			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = relatedPortCode;
			reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
			updatedConsignment = reader.ReadIntoBusinessObject();
			AssertEquals("Consignment.NextDischargePort should be " + portOfDischarge, portOfDischarge, updatedConsignment.WRC_RL_NKNextDischargePort);
		}

		public void TestNextDischargePortForNewNamespace() => NextDischargePortForNewNamespaceCore(false);
		public void TestNextDischargePortForNewNamespace_UseExtraPort() => NextDischargePortForNewNamespaceCore(true);

		void NextDischargePortForNewNamespaceCore(bool useExtraPort)
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var portOfDischarge = "NZAKL";
				var relatedPortCode = "AUSYD";
				var portOfDestination = "AUPER";

				Data.SetupForForwardingImport();
				Data.ShipmentDataObject.PortOfDestination = new UNLOCO { Code = portOfDestination };
				Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = relatedPortCode;
				if (useExtraPort)
				{
					UniversalHelper.CreateExtraPort(Data.Warehouse, relatedPortCode);
				}

				var header = Data.HeaderDataObject;
				header.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } }, TriggerDate = ZDateTimeOffset.Now });
				header.TransportLegCollection.Add(new TransportLeg
				{
					PortOfLoading = new UNLOCO { Code = "AUBNE" },
					PortOfDischarge = new UNLOCO { Code = "US2CW" }
				});

				var transportLeg1 = header.TransportLegCollection[0];
				transportLeg1.PortOfLoading = new UNLOCO { Code = relatedPortCode };
				transportLeg1.PortOfDischarge = new UNLOCO { Code = portOfDischarge };

				Data.ShipmentDataObject.SetTransportLegCollection(() => header.TransportLegCollection);

				var reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
				var consignment = reader.ReadIntoBusinessObject();
				AssertEquals("Consignment.NextDischargePort should be :" + portOfDischarge, portOfDischarge, consignment.WRC_RL_NKNextDischargePort);

				transportLeg1.PortOfLoading = new UNLOCO { Code = "AUBNE" };
				reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
				var updatedConsignment = reader.ReadIntoBusinessObject();
				AssertEquals("Consignment.NextDischargePort should be Shipment Destination Port, as PortOfLoading doesn't match with Warehouse.RelatedPort.", portOfDestination, updatedConsignment.WRC_RL_NKNextDischargePort);

				transportLeg1.PortOfLoading = new UNLOCO { Code = "AUSYD" };
				Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "TEST1";
				reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
				updatedConsignment = reader.ReadIntoBusinessObject();
				if (useExtraPort)
				{
					AssertEquals("Consignment.NextDischargePort should be " + portOfDischarge, portOfDischarge, updatedConsignment.WRC_RL_NKNextDischargePort);
				}
				else
				{
					AssertEquals("Consignment.NextDischargePort should be Shipment Destination Port, as PortOfLoading doesn't match with Warehouse.RelatedPort.", portOfDestination, updatedConsignment.WRC_RL_NKNextDischargePort);
				}

				Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = relatedPortCode;
				reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
				updatedConsignment = reader.ReadIntoBusinessObject();
				AssertEquals("Consignment.NextDischargePort should be " + portOfDischarge, portOfDischarge, updatedConsignment.WRC_RL_NKNextDischargePort);
			}
		}

		#endregion

		#region TestAdditionalReferences

		protected override int AdditionalReferencesRows { get => 8; }

		protected override int InitialReferencesRowCount { get => 7; }

		public void TestAdditionalReferences_Arrival() => TestAdditionalReferences(true);
		public void TestAdditionalReferences_Departure() => TestAdditionalReferences(false);

		void TestAdditionalReferences(bool isArrival)
		{
			Data.SetupForForwardingImport();
			var header = Data.HeaderDataObject;
			header.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = isArrival ? RecipientRoleType.ATW : RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "ZAJNB";

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			var additionalReferencesRows = isArrival ? 3 : 6;

			var cusEntryNumbers1 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
			AssertEquals(string.Format("We should populate {0} rows in additional references.", additionalReferencesRows), additionalReferencesRows, cusEntryNumbers1.Length);
			AssertEquals("AUSYD", consignment.WRC_RL_NKDestination);

			Helper.AssertAdditionalReferences(cusEntryNumbers1, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, "C1000000", "ForwardingConsolNumber");
			Helper.AssertAdditionalReferences(cusEntryNumbers1, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "S1000000", "ForwardingShipmentNumber");
			if (isArrival)
			{
				Helper.AssertAdditionalReferences(cusEntryNumbers1, AdditionalReferenceTypes.Codes.MasterBill, "WaybillParent", "MasterBill");
			}
			else
			{
				Helper.AssertAdditionalReferences(cusEntryNumbers1, WarehouseAdditionalReferenceTypes.Codes.CutOffDate, new ZDateTime(2015, 4, 14).FormatDateTime(), "CutOffDate");
				Helper.AssertAdditionalReferences(cusEntryNumbers1, WarehouseAdditionalReferenceTypes.Codes.ETDDate, new ZDateTime(2015, 4, 15).FormatDateTime(), "ETDDate");
				Helper.AssertAdditionalReferences(cusEntryNumbers1, WarehouseAdditionalReferenceTypes.Codes.Vessel, "VES1", "Vessel");
				Helper.AssertAdditionalReferences(cusEntryNumbers1, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, "VF1", "VoyageFlightNumber");
			}

			if (!isArrival)
			{
				var newCutOffDate = new ZDateTime(2015, 4, 16);
				header.TransportLegCollection[0].LCLCutOff = newCutOffDate;
				reader = GetDataObjectReader(Factory, header);
				var updatedConsignment = reader.ReadIntoBusinessObject();
				AssertEquals("Should have updated the same consignment.", consignment, updatedConsignment);

				var cusEntryNumbers2 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
				AssertEquals(string.Format("We should delete original references and populate {0} new additional references.", additionalReferencesRows),
					additionalReferencesRows, cusEntryNumbers2.Length);
				Helper.AssertAdditionalReferences(cusEntryNumbers2, WarehouseAdditionalReferenceTypes.Codes.CutOffDate, newCutOffDate.FormatDateTime(), "CutOffDate");
			}
		}

		public void TestPopulate_TransportMode()
		{
			Data.SetupForForwardingImport();
			var header = Data.HeaderDataObject;
			header.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			// remove existing transport legs to avoid getting the wrong outbound leg
			header.TransportLegCollection.RemoveAll(l => l != null);
			header.TransportLegCollection.Add(new TransportLeg
			{
				LCLCutOff = new ZDateTime(2015, 5, 14),
				EstimatedDeparture = new ZDateTime(2015, 5, 15),
				PortOfLoading = Data.Orgs.Warehouse_WUFSHIJNB.Port,
				VesselName = "VS1",
				VoyageFlightNo = "VF1",
				TransportMode = TransportMode.Air,
			});
			Data.ShipmentDataObject.TransportMode = new CodeDescriptionPair { Code = TransportModes.SeaAir };

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();

			AssertEquals(TransportModes.SeaAir, consignment.WRC_TransportMode);
		}

		public void TestSeaCargoShipmentAdditionalReferences_CustomsNumber_And_CustomsReleaseNumber()
		{
			Data.SetupForForwardingImport();
			var header = SetupSeaCargoShipmentForImporting("RC00000001", DataContextType.Outturn, "Consignee");
			header.ConsolidatedCargoStatus = new CodeDescriptionPair()
			{
				Code = "CLR",
				Description = "Customs Cleared"
			};

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			int additionalReferencesRows = 0;
			int customsReferencesRows = 2;
			Factory.SaveForTesting();

			AssertEquals($"We should populate {additionalReferencesRows} rows in additional references.", additionalReferencesRows, consignment.AdditionalReferenceNumbers.Count);
			AssertEquals($"We should populate {customsReferencesRows} rows in customs references.", customsReferencesRows, consignment.CustomsReferenceNumbers.Count);

			var cusEntryNumbers1 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
			var cusEntryNum = cusEntryNumbers1.FirstOrDefault(r => r.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var cusReleaseNum = cusEntryNumbers1.FirstOrDefault(r => r.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			Helper.AssertAdditionalReference(cusEntryNum, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "Customs Held", category: TransitWarehouseReferenceCategories.Codes.CustomsReference);
			Helper.AssertAdditionalReference(cusReleaseNum, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "Customs Cleared", category: TransitWarehouseReferenceCategories.Codes.CustomsReference);
		}

		public void TestAirCargoShipmentAdditionalReferences_CustomsNumber_And_CustomsReleaseNumber()
		{
			Data.SetupForForwardingImport();
			var header = SetupSeaCargoShipmentForImporting("RC00000002", DataContextType.AirManifestLine, "Consignee");
			header.ConsolidatedCargoStatus = new CodeDescriptionPair()
			{
				Code = "CLR",
				Description = "Customs Cleared"
			};

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var cusEntryNumbers1 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
			var cusEntryNum = cusEntryNumbers1.FirstOrDefault(r => r.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var cusReleaseNum = cusEntryNumbers1.FirstOrDefault(r => r.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			Helper.AssertAdditionalReference(cusEntryNum, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "Customs Held", category: TransitWarehouseReferenceCategories.Codes.CustomsReference);
			Helper.AssertAdditionalReference(cusReleaseNum, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "Customs Cleared", category: TransitWarehouseReferenceCategories.Codes.CustomsReference);
		}

		public void TestUnderBondShipmentAdditionalReferences_CustomsNumber_And_CustomsReleaseNumber()
		{
			Data.SetupForForwardingImport();
			var header = SetupSeaCargoShipmentForImporting("RC00000003", DataContextType.UnderBond, "Consignee");
			header.ConsolidatedCargoStatus = new CodeDescriptionPair()
			{
				Code = "CLR",
				Description = "Customs Cleared"
			};

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var cusEntryNumbers1 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
			var cusEntryNum = cusEntryNumbers1.FirstOrDefault(r => r.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
			var cusReleaseNum = cusEntryNumbers1.FirstOrDefault(r => r.CE_EntryType == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			Helper.AssertAdditionalReference(cusEntryNum, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "Customs Held", category: TransitWarehouseReferenceCategories.Codes.CustomsReference);
			Helper.AssertAdditionalReference(cusReleaseNum, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "Customs Cleared", category: TransitWarehouseReferenceCategories.Codes.CustomsReference);
		}

		public void TestSeaCargoShipmentAdditionalReferences_MappingCustomsNumberWithDirection()
		{
			var rcnSetDirection = GenerateShipmentRCNWithDirection(inboundPOL: ForeignPort);
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			var transitMapping = new TransitReferenceMappingConfiguration();
			var cenReferenceMapping = new TransitReferenceMapping()
			{
				SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				SourceType = WarehouseAdditionalReferenceTypes.Codes.T1,
				TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber,
				Direction = "IMP"
			};

			var crnReferenceMapping = new TransitReferenceMapping()
			{
				SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				SourceType = WarehouseAdditionalReferenceTypes.Codes.T1,
				TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber,
				Direction = "EXP"
			};

			transitMapping.TransitReferenceMappingCollection.Add(cenReferenceMapping);
			transitMapping.TransitReferenceMappingCollection.Add(crnReferenceMapping);

			using (WarehouseDataRegistry.Instance.TransitReferenceMapping.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, transitMapping))
			{
				var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
				Data.SetupForForwardingImport();
				var legs = new DataObjectList<TransportLeg>();
				TransportLeg inboundRoutingLeg = null;
				TransportLeg outboundRoutingLeg = null;

				inboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: ForeignPort, portOfDischarge: homePort);
				legs.Add(inboundRoutingLeg);

				var estimatedPickupDate = new ZDateTime(2006, 7, 7);
				var shipment = Data.CreateShipmentWithPackages("SHIP123");
				shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
				shipment.AdditionalReferenceCollection.Add(new AdditionalReference()
				{
					Type = new EntryType
					{
						Code = WarehouseAdditionalReferenceTypes.Codes.T1
					},
					ReferenceNumber = "CEN123",
					CountryOfIssue = new Country
					{
						Code = "AU"
					}
				});
				Helper.AddTestDataToShipment(shipment, inboundLeg: inboundRoutingLeg, outboundLeg: outboundRoutingLeg, estimatedPickup: estimatedPickupDate);

				var rcn = GetDataObjectReader(Factory, shipment).ReadIntoBusinessObject();

				CombineAssertions(() =>
				{
					AssertEquals(rcnSetDirection.WRC_Direction, "IMP");
					AssertEquals(1, rcn.CustomsReferenceNumbers.Cast<CusEntryNumber>().Distinct().Count());
					var rcnCustomsReference = rcn.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber) as CusEntryNumber;
					AssertNotNull(rcnCustomsReference);
					AssertEquals(TransitWarehouseReferenceCategories.Codes.CustomsReference, rcnCustomsReference.CE_Category);
					AssertEquals("CEN123", rcnCustomsReference.CE_EntryNum);
					var sourceTypeAddOnValue = rcnCustomsReference.GetAddOnValues(a => a.XV_Name == "SourceType").SingleOrDefault();
					AssertNotNull(sourceTypeAddOnValue);
					AssertEquals("T1", sourceTypeAddOnValue.XV_Data);
				});
			}
		}

		public void TestUXMLShipmentAdditionalReferencesPANNumber()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.ShipmentDataObject;

			var reference = Helper.CreatePortReference("PAN", "Port reference Desc", "AU", "PANRef", "CLR");
			shipment.SetPortReferenceCollection(() => new List<PortReference>());
			shipment.PortReferenceCollection.Add(reference);
			var reader = GetDataObjectReader(Factory, Data.HeaderDataObject);
			var consignment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK);
			query.AddToFilter(CusEntryNumSchema.CE_Category, "PRT");
			var cusEntryNumber = Factory.Load<CusEntryNumber>(query).Single();
			Helper.AssertAdditionalReference(cusEntryNumber, "PAN", "PANRef", "AU", "CLR", TransitWarehouseReferenceCategories.Codes.PortReference);
		}

		public void TestSeaCargoShipmentJobDocAddress_ConsigneeInfo()
		{
			Data.SetupForForwardingImport();
			var header = SetupSeaCargoShipmentForImporting("RC00000001", DataContextType.Outturn, "ConsigneeCompany");

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();

			var jobDocAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK)).Where(a => a.E2_AddressType == DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
			AssertEquals("Should have created 1 ConsigneeDocAddress.", 1, jobDocAddresses.Count());

			var consigneeDocAddress = jobDocAddresses.Single();
			AssertEquals("Company name is ConsigneeCompany.", "ConsigneeCompany", consigneeDocAddress.E2_CompanyName);
			AssertEquals("Address1 has been defaulted.", "not specified", consigneeDocAddress.E2_Address1);
			AssertEquals("City has been defaulted.", "not specified", consigneeDocAddress.E2_City);
		}

		UniversalShipment SetupSeaCargoShipmentForImporting(string consignmentID, DataContextType type, string consigneeCompanyName = "Consignee", string portOfDestination = "AUSYD")
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PortOfOrigin = new UNLOCO { Code = "ZAJNB" },
				PortOfDestination = new UNLOCO { Code = portOfDestination },
				WayBillNumber = consignmentID
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(type, "S1000000");
			dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } } });
			shipment.DataContext = dataContext;

			shipment.SetAddInfoCollection(() =>
			{
				var addInfoCollection = new List<AddInfo>()
				{
					AddInfo.New("Consignee", consigneeCompanyName)
				};

				return addInfoCollection;
			});

			return shipment;
		}

		public void TestAdditionalReferencesForNewNamespace()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				Data.SetupForForwardingImport();
				Data.HeaderDataObjectWithoutChildShipment.DataContext = DataContextFactory.New();
				Data.HeaderDataObjectWithoutChildShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "CONSOL1");
				Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } }, TriggerDate = new ZDateTimeOffset(2000, 1, 1) });
				Data.ShipmentDataObject.SetParentShipmentCollection(() => new List<UniversalShipment> { Data.HeaderDataObjectWithoutChildShipment });
				Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "ZAJNB";
				var reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
				var consignment = reader.ReadIntoBusinessObject();

				var additionalReferences1 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
				Helper.AssertAdditionalReferences(additionalReferences1, WarehouseAdditionalReferenceTypes.Codes.CutOffDate, new ZDateTime(2015, 4, 14).FormatDateTime(), "CutOffDate");
				Helper.AssertAdditionalReferences(additionalReferences1, WarehouseAdditionalReferenceTypes.Codes.ETDDate, new ZDateTime(2015, 4, 15).FormatDateTime(), "ETDDate");
				AssertEquals("Since there is no data context type of run sheet, there should not be a run sheet reference.",
					0, additionalReferences1.Count(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber));

				AssertEquals("Since there is no data context type of run sheet, there should not be a consignment reference.",
					0, additionalReferences1.Count(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.ConsignmentNumber));

				Data.ShipmentDataObject.SetParentShipmentCollection(() => null);
				Data.ShipmentDataObject.SetTransportLegCollection(() => Data.HeaderDataObjectWithoutChildShipment.TransportLegCollection);
				Data.ShipmentDataObject.TransportLegCollection[0].LCLCutOff = new ZDateTime(2015, 4, 16);
				reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
				var updatedConsignment = reader.ReadIntoBusinessObject();
				AssertEquals("Should have updated existing Consignment.", consignment, updatedConsignment);

				var additionalReferences2 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
				Helper.AssertAdditionalReferences(additionalReferences2, WarehouseAdditionalReferenceTypes.Codes.CutOffDate, new ZDateTime(2015, 4, 16).FormatDateTime(), "CutOffDate");
				Helper.AssertAdditionalReferences(additionalReferences2, WarehouseAdditionalReferenceTypes.Codes.ETDDate, new ZDateTime(2015, 4, 15).FormatDateTime(), "ETDDate");
			}
		}

		public void TestAdditionalReferences_RunSheetDataContext()
		{
			Data.SetupForForwardingImport();
			var header = Data.HeaderDataObjectWithoutChildShipment;
			var headerDataContext = DataContextFactory.New();
			headerDataContext.AddDataSource(DataContextType.ForwardingConsol, "CONSOL1");
			headerDataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "R1");
			headerDataContext.AddDataSource(DataContextType.LandTransportConsignment, "C1");
			headerDataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			header.DataContext = headerDataContext;

			var shipment = Data.CreateShipmentWithPackages("HSB1", "Pack1");
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			var childDataContext = DataContextFactory.New();
			childDataContext.AddDataSource(DataContextType.LandTransportConsignment, "C1");
			shipment.DataContext = childDataContext;
			header.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			header.SubShipmentCollection.Add(shipment);
			header.VoyageFlightNo = "V1"; // Run Sheet Vehicle Rego
			header.WayBillNumber = ""; // RunSheet doesn't export a waybill atm.

			Logger.TopLevelDataObject = header;

			var warehouseFromConsol = WarehouseMatchingHelper.GetWarehouse(header, Factory, Logger);
			var detachedASNPKs = new HashSet<ZGuid>();
			var packagesByContainer = new Dictionary<ZInt, List<IColumnIndexer>>();
			var reader = new WhsTransitReceiveConsignmentDataObjectReader(header, Logger, Factory, warehouseFromConsol, packagesByContainer, detachedASNPKs);
			var consignment = reader.ReadIntoBusinessObject();

			var additionalReferencesForConsignment = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
			Helper.AssertAdditionalReferences(additionalReferencesForConsignment, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "R1", "Transport Ref");
			Helper.AssertAdditionalReferences(additionalReferencesForConsignment, WarehouseAdditionalReferenceTypes.Codes.ConsignmentNumber, "C1", "Consignment Reference Number");
		}

		public void TestAdditionalReferences_RunSheetInstructionDataContext()
		{
			Data.SetupForForwardingImport();
			var header = Data.HeaderDataObjectWithoutChildShipment;
			var headerDataContext = DataContextFactory.New();
			headerDataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			headerDataContext.AddDataSource(DataContextType.ForwardingConsol, "CONSOL1");
			headerDataContext.AddDataSource(DataContextType.TransportConsignmentRunSheetInstruction, "RI1");
			headerDataContext.AddDataSource(DataContextType.LandTransportConsignment, "C1");
			header.DataContext = headerDataContext;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			var childDataContext = DataContextFactory.New();
			childDataContext.AddDataSource(DataContextType.LandTransportConsignment, "C1");
			shipment.DataContext = childDataContext;
			header.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			header.SubShipmentCollection.Add(shipment);

			Logger.TopLevelDataObject = header;

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			var additionalReferences = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
			Helper.AssertAdditionalReferences(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ConsignmentNumber, "C1", "Consignment Reference Number");
			Helper.AssertAdditionalReferences(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.RunSheetInstructionReference, "RI1", "Transport RunSheet Instruction Reference");
		}

		#endregion

		#region AssertPopulateSucceeded

		void AssertPopulateSucceeded(UniversalShipment shipmentDO)
		{
			var reader = GetDataObjectReader(Factory, shipmentDO);
			var consignment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Should have set the Intended Warehouse.", Data.Warehouse.PK, consignment.WRC_WW_IntendedWarehouse);

			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			var packages = packageJob.Packages;
			AssertEquals("There should be one package.", 1, packages.Count);

			var package = packages.First();

			AssertEquals("The weight should be 75", 150m, package.KP_Weight);
			AssertEquals("The volume should be 2", 2m, package.KP_Volume);
			AssertEquals("The qty should be 2", 2, package.KP_PackageQty);
			AssertEquals("The booked weight should be 150", 150m, package.BookedDimensions.KPB_Weight);
			AssertEquals("The booked volume should be 2", 2m, package.BookedDimensions.KPB_Volume);

			var packageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, package.PK)).Single();
			AssertEquals("Package State Is High Risk must be set to true", true, packageState.WPS_IsHighRisk);

			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK));
			AssertEquals("Should have Booking party, Pickup, Consignor & Consignee Doc Addresses attached.", 4, addresses.Length);
			var cneAddr = addresses.Single((a) => a.DocAddressType == DocAddressType.ConsigneeDocumentaryAddress);
			var cnrAddr = addresses.Single((a) => a.DocAddressType == DocAddressType.LocalCartageExporter);
			addresses.Single((a) => a.DocAddressType == DocAddressType.ConsignorPickupDeliveryAddress);
			addresses.Single((a) => a.DocAddressType == DocAddressType.BookingPartyDocumentaryAddress);
			AssertAddressContentMatches_CRAHOLSYD(cnrAddr.Address);
			AssertAddressContentMatches_INTHEMSYD(cneAddr.Address);
			AssertNoExceptionThrown("Assert Data can save properly", () => Factory.SaveForTesting());
		}

		#endregion

		#region TestPopulateBusinessObject_NewNameSpace

		public void TestPopulateBusinessObject_NewNameSpace_WithoutConsol()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				TestPopulateBusinessObject_WithoutConsol();
			}
		}

		#endregion

		#region TestPopulateOverpacks

		public void TestPopulateOverpack()
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var handlingUnitPackingLine = Helper.CreatePackingLine("HandlingUnit-01", "", Constants.PkgUnit.Pallet, 300m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var inner1 = Helper.CreatePackingLine("Inner-01", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var inner2 = Helper.CreatePackingLine("Inner-02", "", Constants.PkgUnit.Carton, 150m, 2m, 2m, 2m, marksAndNumbers: "Furnitures");
			handlingUnitPackingLine.SetPackingLineCollection(() => new List<PackingLine> { inner1, inner2 });
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(handlingUnitPackingLine);
			// Shipment PackingLine Tree To Be Imported
			//	HandlingUnit-01
			//		Inner-01
			//		Inner-02

			AssertEquals("Precondition - No Divot have been created yet.", 0, Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package have been created yet.", 0, Factory.BOFactory.Load<PkgPackage>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package State have been created yet.", 0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var consignment = GetDataObjectReader(Factory, shipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 3 packages on the package Job", 3, packageJob.GetAllPackagesOnJob().Length);

			var packages = packageJob.GetAllPackagesOnJob().AsEnumerable();
			var handlingUnitPackage = Helper.AssertAndReturnPackage(packages, consignment.PK, 3, "HandlingUnit-01", "", "Furnitures", 300m, "KG", 10m, 10m, 10m, "CM", 1, Constants.PkgUnit.Pallet);
			var inner1Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 3, "Inner-01", "", "Furnitures", 150m, "KG", 5m, 5m, 5m, "CM", 1, Constants.PkgUnit.Box);
			var inner2Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 3, "Inner-02", "", "Furnitures", 150m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Carton);
			AssertEquals(handlingUnitPackage.PK, inner1Package.KP_KP_ParentPackage);
			AssertEquals(handlingUnitPackage.PK, inner2Package.KP_KP_ParentPackage);

			var divots = Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());
			AssertEquals("2 Divots should have been created.", 2, divots.Length);
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage.PK && d.KPD_KP_Package == inner1Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage.PK && d.KPD_KP_Package == inner2Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));

			var packageStates = Factory.BOFactory.Load<WhsItemReceiveConsignment>(consignment.PK).PackageStates;
			AssertEquals("3 Package States should have been created on the Receive Consignment.", 3, packageStates.Count);
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == handlingUnitPackage.PK && ps.WPS_UnitType == PackageStateUnitType.Codes.Overpack && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == inner1Package.PK && !ps.WPS_IsHandlingUnit && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == inner2Package.PK && !ps.WPS_IsHandlingUnit && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
		}

		public void TestPopulateOverpack_WithMoreThan100PackingLines()
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;
			var outerList = new DataObjectList<PackingLine>();

			for (var index = 0; index < 100; index++)
			{
				var handlingUnitPackingLine = Helper.CreatePackingLine($"HandlingUnit-{index}", "", Constants.PkgUnit.Pallet, 300m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
				var inner1 = Helper.CreatePackingLine($"Inner-{index}-1", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
				var inner2 = Helper.CreatePackingLine($"Inner-{index}-2", "", Constants.PkgUnit.Carton, 150m, 2m, 2m, 2m, marksAndNumbers: "Furnitures");
				handlingUnitPackingLine.SetPackingLineCollection(() => new List<PackingLine> { inner1, inner2 });
				shipmentDataObject.PackingLineCollection.Clear();
				outerList.Add(handlingUnitPackingLine);
			}
			shipmentDataObject.PackingLineCollection.AddRange(outerList);

			AssertEquals("Precondition - No Divot have been created yet.", 0, Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package have been created yet.", 0, Factory.BOFactory.Load<PkgPackage>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package State have been created yet.", 0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var consignment = GetDataObjectReader(Factory, shipmentDataObject).ReadIntoBusinessObject();

			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 300 packages on the package Job", 300, packageJob.GetAllPackagesOnJob().Length);

			var packageStates = Factory.BOFactory.Load<WhsItemReceiveConsignment>(consignment.PK).PackageStates;
			AssertEquals("300 Package States should have been created on the Receive Consignment.", 300, packageStates.Count);
		}

		public void TestPopulateOverpack_IgnoreThreeLevelPackages()
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var handlingUnitPackingLine1 = Helper.CreatePackingLine("HandlingUnit-01", "", Constants.PkgUnit.Pallet, 150m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var handlingUnitPackingLine2 = Helper.CreatePackingLine("HandlingUnit-02", "", Constants.PkgUnit.Pallet, 150m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var handlingUnitPackingLine3 = Helper.CreatePackingLine("HandlingUnit-03", "", Constants.PkgUnit.Pallet, 150m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var inner1 = Helper.CreatePackingLine("Inner-01", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var inner2 = Helper.CreatePackingLine("Inner-02", "", Constants.PkgUnit.Carton, 150m, 2m, 2m, 2m, marksAndNumbers: "Furnitures");
			var inner3 = Helper.CreatePackingLine("Inner-03", "", Constants.PkgUnit.Bag, 150m, 1m, 1m, 1m, marksAndNumbers: "Furnitures");
			handlingUnitPackingLine1.SetPackingLineCollection(() => new List<PackingLine> { handlingUnitPackingLine2, inner3 });
			handlingUnitPackingLine2.SetPackingLineCollection(() => new List<PackingLine> { handlingUnitPackingLine3, inner2 });
			handlingUnitPackingLine3.SetPackingLineCollection(() => new List<PackingLine> { inner1 });
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(handlingUnitPackingLine1);
			// Shipment PackingLine Tree To Be Imported
			//	HandlingUnit-01
			//		HandlingUnit-02
			//			HandlingUnit-03
			//				Inner-01
			//			Inner-02
			//		Inner-03

			AssertEquals("Precondition - No Divot have been created yet.", 0, Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package have been created yet.", 0, Factory.BOFactory.Load<PkgPackage>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package State have been created yet.", 0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var consignment = GetDataObjectReader(Factory, shipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 6 packages on the package Job", 6, packageJob.GetAllPackagesOnJob().Length);

			var packages = packageJob.GetAllPackagesOnJob().AsEnumerable();
			var handlingUnitPackage1 = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "HandlingUnit-01", "", "Furnitures", 150m, "KG", 10m, 10m, 10m, "CM", 1, Constants.PkgUnit.Pallet);
			var handlingUnitPackage2 = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "HandlingUnit-02", "", "Furnitures", 150m, "KG", 10m, 10m, 10m, "CM", 1, Constants.PkgUnit.Pallet);
			var handlingUnitPackage3 = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "HandlingUnit-03", "", "Furnitures", 150m, "KG", 10m, 10m, 10m, "CM", 1, Constants.PkgUnit.Pallet);
			var inner1Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "Inner-01", "", "Furnitures", 150m, "KG", 5m, 5m, 5m, "CM", 1, Constants.PkgUnit.Box);
			var inner2Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "Inner-02", "", "Furnitures", 150m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Carton);
			var inner3Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "Inner-03", "", "Furnitures", 150m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Bag);

			var divots = Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());
			AssertEquals("5 Divots should have been created.", 5, divots.Length);
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage1.PK && d.KPD_KP_Package == handlingUnitPackage2.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage1.PK && d.KPD_KP_Package == inner3Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage2.PK && d.KPD_KP_Package == handlingUnitPackage3.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage2.PK && d.KPD_KP_Package == inner2Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage3.PK && d.KPD_KP_Package == inner1Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));

			var packageStates = Factory.BOFactory.Load<WhsItemReceiveConsignment>(consignment.PK).PackageStates;
			AssertEquals("3 Package States should have been created on the Receive Consignment.", 3, packageStates.Count);
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == handlingUnitPackage1.PK && ps.WPS_UnitType == PackageStateUnitType.Codes.Overpack && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == handlingUnitPackage2.PK && ps.WPS_UnitType == PackageStateUnitType.Codes.Package && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == inner3Package.PK && !ps.WPS_IsHandlingUnit && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked && ps.WPS_UnitType == PackageStateUnitType.Codes.Package));
		}

		public void TestPopulateOverpack_NoReference()
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var handlingUnitPackingLine = Helper.CreatePackingLine("", "", Constants.PkgUnit.Pallet, 300m, 10m, 10m, 10m, marksAndNumbers: "NoRef");
			var inner1 = Helper.CreatePackingLine("Inner-01", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var inner2 = Helper.CreatePackingLine("", "", Constants.PkgUnit.Carton, 150m, 2m, 2m, 2m, marksAndNumbers: "NoRef2");
			handlingUnitPackingLine.SetPackingLineCollection(() => new List<PackingLine> { inner1, inner2 });
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(handlingUnitPackingLine);
			// Shipment PackingLine Tree To Be Imported
			//	(No Ref)
			//		Inner-01
			//		(No Ref)

			AssertEquals("Precondition - No Divot have been created yet.", 0, Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package have been created yet.", 0, Factory.BOFactory.Load<PkgPackage>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package State have been created yet.", 0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var consignment = GetDataObjectReader(Factory, shipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 3 packages on the package Job", 3, packageJob.GetAllPackagesOnJob().Length);

			var packages = packageJob.GetAllPackagesOnJob().AsEnumerable();
			var handlingUnitPackage = Helper.AssertAndReturnPackage(packages, consignment.PK, 3, "", "", "NoRef", 300m, "KG", 10m, 10m, 10m, "CM", 1, Constants.PkgUnit.Pallet);
			var inner1Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 3, "Inner-01", "", "Furnitures", 150m, "KG", 5m, 5m, 5m, "CM", 1, Constants.PkgUnit.Box);
			var inner2Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 3, "", "", "NoRef2", 150m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Carton);

			var divots = Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());
			AssertEquals("No Divots should have been created.", 0, divots.Length);

			var packageStates = Factory.BOFactory.Load<WhsItemReceiveConsignment>(consignment.PK).PackageStates;
			AssertEquals("1 Package State should have been created on the Receive Consignment.", 1, packageStates.Count);
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == handlingUnitPackage.PK && ps.WPS_UnitType == PackageStateUnitType.Codes.Package && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
		}

		public void TestPopulateOverpack_QtyNotEqualTo1()
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var handlingUnitPackingLine = Helper.CreatePackingLine("HandlingUnit-01", "", Constants.PkgUnit.Pallet, 300m, 10m, 10m, 10m, marksAndNumbers: "HU", packQty: 5);
			var inner1 = Helper.CreatePackingLine("Inner-01", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var inner2 = Helper.CreatePackingLine("Inner-02", "", Constants.PkgUnit.Carton, 150m, 2m, 2m, 2m, marksAndNumbers: "Furnitures");
			handlingUnitPackingLine.SetPackingLineCollection(() => new List<PackingLine> { inner1, inner2 });
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(handlingUnitPackingLine);
			// Shipment PackingLine Tree To Be Imported
			//	HandlingUnit-01
			//		Inner-01
			//		Inner-02

			AssertEquals("Precondition - No Divot have been created yet.", 0, Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package have been created yet.", 0, Factory.BOFactory.Load<PkgPackage>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package State have been created yet.", 0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var consignment = GetDataObjectReader(Factory, shipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 3 packages on the package Job", 3, packageJob.GetAllPackagesOnJob().Length);

			var packages = packageJob.GetAllPackagesOnJob().AsEnumerable();
			var handlingUnitPackage = Helper.AssertAndReturnPackage(packages, consignment.PK, 3, "", "", "HU", 300m, "KG", 10m, 10m, 10m, "CM", 5, Constants.PkgUnit.Pallet);
			var inner1Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 3, "Inner-01", "", "Furnitures", 150m, "KG", 5m, 5m, 5m, "CM", 1, Constants.PkgUnit.Box, handlingUnitPackage.PK);
			var inner2Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 3, "Inner-02", "", "Furnitures", 150m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Carton, handlingUnitPackage.PK);

			var divots = Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());
			AssertEquals("No Divot should have been created.", 0, divots.Length);

			var packageStates = Factory.BOFactory.Load<WhsItemReceiveConsignment>(consignment.PK).PackageStates;
			AssertEquals("1 Package State should have been created on the Receive Consignment.", 1, packageStates.Count);
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == handlingUnitPackage.PK && ps.WPS_UnitType == PackageStateUnitType.Codes.PackLine && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
		}

		public void TestPopulateOverpack_InnerPackline()
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var handlingUnitPackingLine = Helper.CreatePackingLine("HandlingUnit-01", "", Constants.PkgUnit.Pallet, 300m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var inner1 = Helper.CreatePackingLine("Inner-01", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var inner2 = Helper.CreatePackingLine("Inner-02", "", Constants.PkgUnit.Carton, 150m, 2m, 2m, 2m, marksAndNumbers: "Packline1", packQty: 5);
			var inner3 = Helper.CreatePackingLine("", "", Constants.PkgUnit.Cylinder, 150m, 1m, 1m, 1m, marksAndNumbers: "Packline2", packQty: 3);
			handlingUnitPackingLine.SetPackingLineCollection(() => new List<PackingLine> { inner1, inner2, inner3 });
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(handlingUnitPackingLine);
			// Shipment PackingLine Tree To Be Imported
			//	HandlingUnit-01
			//		Inner-01
			//		Inner-02
			//		(No Ref)

			AssertEquals("Precondition - No Divot have been created yet.", 0, Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package have been created yet.", 0, Factory.BOFactory.Load<PkgPackage>(new ZQuery()).Length);
			AssertEquals("Precondition - No Package States have been created yet.", 0, Factory.BOFactory.Load<WhsItemPackageState>(new ZQuery()).Length);

			var consignment = GetDataObjectReader(Factory, shipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 4 packages on the package Job", 4, packageJob.GetAllPackagesOnJob().Length);

			var packages = packageJob.GetAllPackagesOnJob().AsEnumerable();
			var handlingUnitPackage = Helper.AssertAndReturnPackage(packages, consignment.PK, 4, "HandlingUnit-01", "", "Furnitures", 300m, "KG", 10m, 10m, 10m, "CM", 1, Constants.PkgUnit.Pallet);
			var inner1Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 4, "Inner-01", "", "Furnitures", 150m, "KG", 5m, 5m, 5m, "CM", 1, Constants.PkgUnit.Box);
			var packline1Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 4, "", "", "Packline1", 150m, "KG", 2m, 2m, 2m, "CM", 5, Constants.PkgUnit.Carton);
			var packline2Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 4, "", "", "Packline2", 150m, "KG", 1m, 1m, 1m, "CM", 3, Constants.PkgUnit.Cylinder);
			AssertEquals(handlingUnitPackage.PK, packline1Package.KP_KP_ParentPackage);
			AssertEquals(handlingUnitPackage.PK, packline2Package.KP_KP_ParentPackage);

			var divots = Factory.BOFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());
			AssertEquals("3 Divots should have been created.", 3, divots.Length);
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage.PK && d.KPD_KP_Package == inner1Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage.PK && d.KPD_KP_Package == packline1Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));
			AssertNotNull(divots.Single(d => d.KPD_KP_HandlingUnit == handlingUnitPackage.PK && d.KPD_KP_Package == packline2Package.PK && !d.KPD_PackedTime.IsEmpty && d.KPD_GS_NKPackedUser == Env.CurrentUser.Initials));

			var packageStates = Factory.BOFactory.Load<WhsItemReceiveConsignment>(consignment.PK).PackageStates;
			AssertEquals("4 Package States should have been created on the Receive Consignment.", 4, packageStates.Count);
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == handlingUnitPackage.PK && ps.WPS_UnitType == PackageStateUnitType.Codes.Overpack && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == inner1Package.PK && !ps.WPS_IsHandlingUnit && ps.WPS_UnitType == PackageStateUnitType.Codes.Package && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == packline1Package.PK && !ps.WPS_IsHandlingUnit && ps.WPS_UnitType == PackageStateUnitType.Codes.PackLine && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
			AssertNotNull(packageStates.Single(ps => ps.WPS_KP_Package == packline2Package.PK && !ps.WPS_IsHandlingUnit && ps.WPS_UnitType == PackageStateUnitType.Codes.PackLine && ps.WPS_Status == TransitWarehouseStatuses.Codes.Booked));
		}

		#endregion

		#region TestPopulatePackageJob

		public void TestPopulatePackageJob()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { Weight = 150m, Volume = 2m, PackQty = 1 });

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 2 packages on the package Job", 2, packageJob.Packages.Count);

			var countOfNonSplitPacks = packageJob.Packages.Count(p => p.KP_Weight == 150m && p.KP_Volume == 2m);
			AssertEquals("There should be 2 package with weight of 150 & volume of 2", 2, countOfNonSplitPacks);
			AssertEquals("Original packing collection should not have changed.", 2, Data.ShipmentDataObject.PackingLineCollection.Count);
			AssertEquals("Original packing collection should not have changed.", 1, Data.ShipmentDataObject.PackingLineCollection.Count(p => p.PackQty == 2 && p.Weight == 150 && p.Volume == 2));
			AssertEquals("Original packing collection should not have changed.", 1, Data.ShipmentDataObject.PackingLineCollection.Count(p => p.PackQty == 1 && p.Weight == 150 && p.Volume == 2));
		}

		public void TestPopulatePackageJob_NoPackingLine_FromForwarding()
		{
			Data.SetupForForwardingImport();

			var shipment = SetupSeaCargoShipmentForImporting("RC00001", DataContextType.ForwardingShipment);

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { });
			shipment.OuterPacks = 10;
			shipment.OuterPacksPackageType = new PackageType { Code = "PLT" };
			shipment.TotalVolume = 5m;
			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			shipment.TotalWeight = 7m;
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "KG" };

			var consignment = GetDataObjectReader(Factory, shipment).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();

			AssertEquals("There should be 0 package in the package Job", 0, packageJob.Packages.Count);
		}

		public void TestPopulatePackageJob_NoPackingLine_FromCustoms()
		{
			Data.SetupForForwardingImport();

			var shipment = SetupSeaCargoShipmentForImporting("RC00001", DataContextType.CustomerServiceTicket);

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { });
			shipment.OuterPacks = 2;
			shipment.OuterPacksPackageType = new PackageType { Code = "PLT" };
			shipment.TotalVolume = 5m;
			shipment.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			shipment.TotalWeight = 7m;
			shipment.TotalWeightUnit = new UnitOfWeight { Code = "KG" };

			var consignment = GetDataObjectReader(Factory, shipment).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();

			AssertEquals("There should be 1 package in the package Job", 1, packageJob.Packages.Count);
			AssertEquals("That package.KP_PackageQty should be 2", 2, packageJob.Packages.First().KP_PackageQty);
		}

		#endregion

		#region TestPopulatePackageJob_DoesNotCreateTotalsPackage

		public void TestPopulatePackageJob_DoesNotCreateTotalsPackage()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1 }, new PackingLine { PackQty = 1 } });

			// setup totals information
			Data.ShipmentDataObject.OuterPacks = 10;
			Data.ShipmentDataObject.OuterPacksPackageType = new PackageType { Code = "PLT" };
			Data.ShipmentDataObject.TotalVolume = 5m;
			Data.ShipmentDataObject.TotalVolumeUnit = new UnitOfVolume { Code = "M3" };
			Data.ShipmentDataObject.TotalWeight = 7m;
			Data.ShipmentDataObject.TotalWeightUnit = new UnitOfWeight { Code = "KG" };

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			packageJob.AssertPackageTree("Make a job totals outer with the 2 packs inside.", @"
- 1 PKG
- 1 PKG
");
			var package1 = packageJob.Packages.ToArray()[0];
			var package2 = packageJob.Packages.ToArray()[1];

			AssertEquals("Package weight should be set to 0.", 0m, package1.KP_Weight);
			AssertEquals("Package volume should be set to 0.", 0m, package1.KP_Volume);
			AssertEquals("Package weight should be set to 0.", 0m, package2.KP_Weight);
			AssertEquals("Package volume should be set to 0.", 0m, package2.KP_Volume);
		}

		#endregion

		#region TestPopulatePackageJob_RemovesExistingPackages

		public void TestPopulatePackageJob_RemovesExistingPackages()
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { Weight = 150m, Volume = 2m, PackQty = 1 };
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine);

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 2 packages on the package Job", 2, packageJob.Packages.Count);

			Data.ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine }));
			GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			AssertEquals("There should be 1 package on the package Job as the previous ones should have been deleted.", 1, packageJob.Packages.Count);
		}

		#endregion

		#region TestPopulatePackageJob_RemovesExistingPackages_ClearExistingPackageStates

		public void TestPopulatePackageJob_RemovesExistingPackages_ClearExistingPackageStates_WithActivePackline()
			=> PopulatePackageJob_RemovesExistingPackages_ClearExistingPackageStates(withActivePackline: true);
		public void TestPopulatePackageJob_RemovesExistingPackages_ClearExistingPackageStates_WithLockedPackage()
			=> PopulatePackageJob_RemovesExistingPackages_ClearExistingPackageStates(withActivePackline: false);
		public void TestPopulatePackageJob_RemovesExistingPackages_ClearExistingPackageStates_WithLastScannedPackage()
			=> PopulatePackageJob_RemovesExistingPackages_ClearExistingPackageStates(withActivePackline: false, isLastScannedPackage: true);

		void PopulatePackageJob_RemovesExistingPackages_ClearExistingPackageStates(bool withActivePackline, bool isLastScannedPackage = false)
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { Weight = 150m, Volume = 2m, PackQty = 1 };
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine);

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 2 packages on the package Job", 2, packageJob.Packages.Count);

			var packageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, consignment.PK)).FirstOrDefault();
			var warehouse = Data.Warehouse;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var unloadTask = Factory.New<WhsItemUnloadTask>();
			if (withActivePackline)
			{
				unloadTask.WUT_WPS_ActivePackline = packageState.PK;
			}
			else
			{
				var arrivedPackage = helper.CreatePackageState(rtu, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);
				if (isLastScannedPackage)
				{
					unloadTask.WUT_WPS_LastScannedPackage = arrivedPackage.PK;
				}
				else
				{
					unloadTask.WUT_WPS_LockedPackage = arrivedPackage.PK;
				}
			}
			unloadTask.WUT_WRC_ActiveReceiveConsignment = consignment.PK;
			unloadTask.WUT_WRH_ActiveReceiveHeader = rtu.PK;
			unloadTask.WUT_WW_Warehouse = warehouse.PK;
			Factory.SaveForTesting();

			Data.ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine }) { Content = CollectionContent.Complete });
			GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			AssertEquals("There should be 1 package on the package Job as the previous ones should have been deleted.", 1, packageJob.Packages.Count);
			Factory.SaveForTesting();

			var unloadTaskAfterSave = Factory.Load<WhsItemUnloadTask>(new ZQuery()).FirstOrDefault();
			AssertEquals("Active Packline for Unload Task should be null", ZGuid.Empty, unloadTask.WUT_WPS_ActivePackline);
			AssertEquals("Locked Package for Unload Task should be null", ZGuid.Empty, unloadTask.WUT_WPS_LockedPackage);
			AssertEquals("Last Scanned Package for Unload Task should be null", ZGuid.Empty, unloadTask.WUT_WPS_LastScannedPackage);
		}

		#endregion

		#region TestPopulatePackageJob_HasLoosePackageIds

		public void TestPopulatePackageJob_HasLoosePackageIds()
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { Weight = 150m, Volume = 2m, PackQty = 1, ReferenceNumber = "P1", PackType = new PackageType { Code = Constants.PkgUnit.Box } };
			var loosePackageID = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "LP1", PackQty = 1 };
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine);
			Data.ShipmentDataObject.PackingLineCollection.Add(loosePackageID);

			AssertContainsExactElementsInAnyOrder("Precondition: PackingLineCollection has 2 PackingLines", new[] { "", "P1", "LP1" }, Data.ShipmentDataObject.PackingLineCollection.Select(p => p.ReferenceNumber.ToString()));

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 2 packages on the package Job, the Packages with PackageID must be created", 2, packageJob.Packages.Count);

			AssertPackageInfo(packageJob.Packages.Single(p => p.KP_PackageID == "P1"), Constants.PkgUnit.Box, 1, 150m, 2m);
			AssertPackageInfo(packageJob.Packages.Single(p => p.KP_PackageID == "LP1"), Constants.PkgUnit.Package, 1, 0m, 0m);
		}

		void AssertPackageInfo(PkgPackage package, ZString expectedPackType, ZInt expectedPackQty, ZDecimal expectedWeight, ZDecimal expectedVolume)
		{
			CombineAssertions("Package Info", () =>
			{
				AssertEquals("Pack Type", expectedPackType, package.KP_F3_NKPackType);
				AssertEquals("Pack Quantity", expectedPackQty, package.KP_PackageQty);
				AssertEquals("Weight", expectedWeight, package.KP_Weight);
				AssertEquals("Volume", expectedVolume, package.KP_Volume);

				AssertEquals("Booked Dimensions FK", package.PK, package.BookedDimensions.KPB_KP_Package);
				AssertEquals("Booked Weight", expectedWeight, package.BookedDimensions.KPB_Weight);
				AssertEquals("Booked Volume", expectedVolume, package.BookedDimensions.KPB_Volume);
			});
		}

		#endregion

		#region TestPopulatePackageJob_HasLoosePackageIds_ShouldNotRemoveExistingPackages

		public void TestPopulatePackageJob_HasLoosePackageIds_ShouldNotRemoveExistingPackages()
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { Weight = 150m, Volume = 2m, PackQty = 1, ReferenceNumber = "P1", PackType = new PackageType { Code = Constants.PkgUnit.Box } };
			var loosePackageID = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "LP1", PackQty = 1 };
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine);
			Data.ShipmentDataObject.PackingLineCollection.Add(loosePackageID);

			AssertContainsExactElementsInAnyOrder("Precondition: PackingLineCollection has 2 PackingLines ", new[] { "", "P1", "LP1" }, Data.ShipmentDataObject.PackingLineCollection.Select(p => p.ReferenceNumber.ToString()));

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			var package = packageJob.Packages.Single(p => p.KP_PackageID == "LP1");
			AssertEquals("There should be 2 packages on the package Job, the Packages with PackageID must be created", 2, packageJob.Packages.Count);

			var packLines = new DataObjectList<PackingLine>(new[] { loosePackageID });
			packLines.Content = CollectionContent.Partial;
			Data.ShipmentDataObject.SetPackingLineCollection(() => packLines);
			GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			AssertEquals("There should be 2 package on the package job as loose package has package Id, it will keep unmatched packages.", 2, packageJob.Packages.Count);

			var newPackage = packageJob.Packages.Single(p => p.KP_PackageID == "LP1");
			AssertPackageInfo(packageJob.Packages.Single(p => p.KP_PackageID == "P1"), Constants.PkgUnit.Box, 1, 150m, 2m);
			AssertPackageInfo(newPackage, Constants.PkgUnit.Package, 1, 0m, 0m);
			AssertEquals("Must not create a new concrete package", package.PK, newPackage.PK);
		}

		#endregion

		#region TestPopulatePackageJob_DuplicateLink

		public void TestPopulatePackageJob_DuplicateLink()
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = Constants.PkgUnit.Box }, Link = 1 };
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.PackingLineCollection.Clear();
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine);

			AssertEquals("Precondition: Only have 1 PackingLine", 1, Data.ShipmentDataObject.PackingLineCollection.Count);

			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
				var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
				var packages = packageJob.Packages.Where(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box);

				AssertEquals("Should create 1 package", 1, packages.Count());
			});
		}

		#endregion

		#region TestPopulatePackageJob_PackageSequence

		public void TestPopulatePackageJob_PackageSequence()
		{
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "PKG-1", PackQty = 1, PackType = new PackageType { Code = Constants.PkgUnit.Box } };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "PKG-2", PackQty = 1, PackType = new PackageType { Code = Constants.PkgUnit.Box } };
			var packingLine3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, PackType = new PackageType { Code = Constants.PkgUnit.Box } };
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.PackingLineCollection.Clear();
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine1);
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine2);
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine3);

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageStates = consignment.PackageStates;
			AssertEquals("Should create 3 packages", 3, packageStates.Count);

			AssertEquals((ZShort)1, packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "PKG-1")?.Package.KP_Sequence);
			AssertEquals((ZShort)2, packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "PKG-2")?.Package.KP_Sequence);
			AssertEquals((ZShort)0, packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "")?.Package.KP_Sequence);

			var packageHeader = Factory.New<PkgPackageHeader>();
			packageHeader.KPH_PackageID = "LP1";
			var loosePackgeID = Helper.PackingHelper.CreatePackageHeaderPivot(consignment.PackageJob, packageHeader);
			Factory.SaveForTesting();

			AssertEquals("The sequence of Loose Package ID should be populated", (ZShort)3, loosePackgeID.KPJ_Sequence);

			var packingLine4 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "PKG-4", PackQty = 1, PackType = new PackageType { Code = Constants.PkgUnit.Box } };
			Data.ShipmentDataObject.PackingLineCollection.Clear();
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine1);
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine2);
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLine4);

			consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			packageStates = consignment.PackageStates;
			AssertEquals("Packages should be updated", 3, packageStates.Count);
			AssertEquals((ZShort)2, packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "PKG-1")?.Package.KP_Sequence);
			AssertEquals((ZShort)3, packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "PKG-2")?.Package.KP_Sequence);
			AssertEquals((ZShort)4, packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "PKG-4")?.Package.KP_Sequence);
			Factory.SaveForTesting();

			loosePackgeID.Reload();
			AssertEquals("The sequence of Loose Package ID should be updated", (ZShort)1, loosePackgeID.KPJ_Sequence);
		}

		#endregion

		#region TestNoCFSProvided_RejectsImport

		public void TestNoCFSProvided_RejectsImport()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.SetContainerCollection(() => new DataObjectList<Container> { new Container { Link = 1, ContainerNumber = "CNT-1" }, new Container { Link = 2, ContainerNumber = "CNT-2" } });
			shipment.PackingLineCollection[0].Link = 1;
			shipment.PackingLineCollection[0].ContainerLink = 1;
			shipment.PackingLineCollection[1].Link = 2;
			shipment.PackingLineCollection[1].ContainerLink = 2;

			var dataContext = DataContextFactory.New();
			dataContext.SetWorkflowInfo(new WorkflowInfo { });
			shipment.DataContext = dataContext;

			var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipment, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot import without a valid Warehouse supplied. Please ensure there is a valid departure/arrival CFS address and that you targeted the correct recipient type.",
				() => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestReceiveExpectedPackingsNotCreatedFromShipment

		public void TestReceiveExpectedPackingsNotCreatedFromShipment()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } } });

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			shipment.SetContainerCollection(() => new DataObjectList<Container> { new Container { Link = 1, ContainerNumber = "CNT-1" } });
			shipment.PackingLineCollection[0].Link = 1;
			shipment.PackingLineCollection[0].ContainerLink = 1;

			var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipment, Logger, Factory);
			reader.ReadIntoBusinessObject();

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("No ASNs should be created when a shipment is imported.", 0, receiveASNs.Length);
			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("No RTUs should be created when a shipment is imported.", 0, rtus.Length);
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("No RTUs should be created when a shipment is imported.", 0, pivots.Length);
		}

		public void TestReceiveExpectedPackingsNotCreatedFromShipmentWithConsol()
		{
			Data.SetupForForwardingImport();
			var header = Data.HeaderDataObjectWithoutChildShipment;
			var headerDataContext = DataContextFactory.New();
			headerDataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			headerDataContext.AddDataSource(DataContextType.ForwardingConsol, "CONSOL1");
			headerDataContext.AddDataSource(DataContextType.TransportConsignmentRunSheetInstruction, "RI1");
			headerDataContext.AddDataSource(DataContextType.LandTransportConsignment, "C1");
			header.DataContext = headerDataContext;

			var shipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			shipment.PackingLineCollection[0].Link = 1;
			shipment.PackingLineCollection[0].ContainerLink = 1;

			var childDataContext = DataContextFactory.New();
			childDataContext.AddDataSource(DataContextType.LandTransportConsignment, "C1");
			shipment.DataContext = childDataContext;
			header.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			header.SubShipmentCollection.Add(shipment);

			Logger.TopLevelDataObject = header;

			var consignment = GetDataObjectReader(Factory, header).ReadIntoBusinessObject();

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("No ASNs should be created when a shipment is imported.", 0, receiveASNs.Length);
			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("No RTUs should be created when a shipment is imported.", 0, rtus.Length);
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("No RTUs should be created when a shipment is imported.", 0, pivots.Length);
		}

		#endregion

		#region TestReceiveASNCreation_WhenReImporting_AfterSettingUpPlannedRTUs

		// Although shipment imports don't create ASNs, we should still clean up old ones if they are detached during an import.
		public void TestReceiveASNCreation_WhenReImporting_AfterSettingUpPlannedRTUs()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.CreateShipmentWithPackages("A123", "ABCD");

			shipment.SetContainerCollection(() => new DataObjectList<Container> {
				new Container { Link = 1, ContainerNumber = "CNT-1" },
			});

			shipment.PackingLineCollection[0].Link = 1;
			shipment.PackingLineCollection[0].ContainerLink = 1;

			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			Logger.TopLevelDataObject = shipment;
			var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipment, Logger, Factory);
			reader.ReadIntoBusinessObject();

			// An ASN may be created through a Consol import or Transit Warehouse in Glow
			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Precondition.", 0, receiveASNs.Length);
			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Precondition. An RCN should be created", 1, receiveConsignments.Length);
			AssertEquals("Precondition. A package should be created", 1, receiveConsignments[0].PackageStates.Count);
			var packageState = receiveConsignments[0].PackageStates.Single();
			var receiveASN = Helper.CreateReceiveASN("ASN1", receiveConsignments[0].WRC_WW_IntendedWarehouse);
			packageState.WPS_WRP_ReceiveExpectedPacking = receiveASN.PK;
			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			var pivot = Factory.New<WhsItemReceiveASNRTUPivot>();
			pivot.WAR_WRP_TransitReceiveASN = receiveASN.PK;
			pivot.WAR_WRH_TransitReceiveTransportationUnit = rtu.PK;
			Factory.SaveForTesting();

			// The package is deleted from the shipment and the shipment is exported
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			var newFactory = new UniversalObjectFactory();
			reader = new WhsTransitReceiveConsignmentDataObjectReader(shipment, Logger, newFactory);
			reader.ReadIntoBusinessObject();

			// The ASN and planned RTU should be removed
			AssertEquals("Logger should create a table for each ASN which packages are going to detach from",
				@"Information - Recipient Role Service is 'ATW - Arrival Transit Warehouse'.
Information - Searching for Transit Warehouse matching Arrival CFS Address 'INTHEMSYD - Unit 12, Level 3'.
Information - Matching 'ArrivalCFSAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Found Warehouse TW2 from Arrival CFS Address.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'ReceivingForwarderAddress' for this Arrival Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Source is 'ForwardingShipment - A123'.
Information - Searching for Receive Consignment for 'ForwardingShipment - A123'.
Information - No matching Receive Consignment found, creating new Receive Consignment.
Information - Populating Receive Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Receive Consignment...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - No matching WhsItemPackageState found, creating new WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Receive Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Receive Consignment RC00000001 from UniversalShipment.
Information - Recipient Role Service is 'ATW - Arrival Transit Warehouse'.
Information - Searching for Transit Warehouse matching Arrival CFS Address 'INTHEMSYD - Unit 12, Level 3'.
Information - Matching 'ArrivalCFSAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Found Warehouse TW2 from Arrival CFS Address.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'ReceivingForwarderAddress' for this Arrival Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Source is 'ForwardingShipment - A123'.
Information - Searching for Receive Consignment for 'ForwardingShipment - A123'.
Information - Successfully loaded matching Receive Consignment RC00000001.
Information - Populating Receive Consignment RC00000001...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Detaching the following Packages from Advanced Shipping Notice ASN1:
Package        RCN
ABCD           A123 (RC00000001)
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Packages for Receive Consignment...
Information - Deleted ASNs 'ASN1' as they have no remaining packages.
Information - Deleted RTUs 'PQCNZ64I15X3NX3' as their planned ASNs were deleted.
Information - Populating Addresses for Receive Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Updated Receive Consignment RC00000001 from UniversalShipment.", Logger.Logs);
			var asnsInNewFactory = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("The empty ASN should be deleted", 0, asnsInNewFactory.Length);
			var rtusInNewFactory = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("The empty RTU should be deleted", 0, rtusInNewFactory.Length);
			var pivotsInNewFactory = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("The pivot should be deleted", 0, pivotsInNewFactory.Length);
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());
		}

		#endregion

		#region TestReceiveASNCreation_WhenNoPacklineInShipment

		public void TestReceiveASNCreation_WhenNoPacklineInShipment()
		{
			Data.SetupForForwardingImport();

			var shipment = Data.CreateShipmentWithPackages("A123");
			shipment.TotalNoOfPieces = 1;
			shipment.OrganizationAddressCollection[2] = Data.Orgs.Warehouse_CRAHOLSYD;
			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipment });
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { Link = 1, ContainerNumber = "CNT-1", ContainerType = new ContainerType { Code = "20GP" } } });
			Data.HeaderDataObject.SetOrganizationAddressCollection(() => Data.ShipmentDataObject.OrganizationAddressCollection);
			var orgAddressForConsol = Data.HeaderDataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
			AssertEquals("Precondition: consol level arrival CFS is pointing to INTHEMSYD", Data.Orgs.Warehouse_INTHEMSYD.OrganizationCode, orgAddressForConsol.OrganizationCode);
			var orgAddressForShipment = shipment.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
			AssertEquals("Precondition: shipment level arrival CFS is pointing to CRAHOLSYD", Data.Orgs.Warehouse_CRAHOLSYD.OrganizationCode, orgAddressForShipment.OrganizationCode);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var consol = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown("Assert Data can save properly", () => Factory.SaveForTesting());

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("A Consol ASN and a Container ASN should be created", 2, receiveASNs.Length);

			var consolASN = receiveASNs.Where(asn => asn.WRP_VehicleReference == "WaybillParent").FirstOrDefault();
			AssertNotNull("ASN should be created for the consol", consolASN);

			var containerASN = receiveASNs.Where(asn => asn.WRP_VehicleReference == "CNT-1").FirstOrDefault();
			AssertNotNull("ASN should be created for the empty container", containerASN);

			AssertEquals("Should use Master Bill.", Data.HeaderDataObject.WayBillNumber, consolASN.WRP_VehicleReference);
			AssertEquals("Should have set the Booking Party.", GlbCompany.CurrentCompany.OrgProxy.PK, consolASN.BookingPartyDocAddress?.Organisation.PK);
			AssertEquals("ASN's intended warehouse should be taken from consol (INTHEMSYD).", Data.WarehouseINTHEMSYD.PK, consolASN.WRP_WW_IntendedWarehouse);

			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("A single RTU should be created.", 1, rtus.Length);

			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("A RTU-ASN pivot should be created.", 1, pivots.Length);
			AssertEquals("A RTU-ASN pivot should be created.", containerASN.PK, pivots.Single().WAR_WRP_TransitReceiveASN);
			AssertEquals("A RTU-ASN pivot should be created.", rtus.Single().PK, pivots.Single().WAR_WRH_TransitReceiveTransportationUnit);
		}

		#endregion

		#region TestUpdateRCN_DifferentWarehouse

		public void TestUpdateRCN_DifferentWarehouse()
		{
			var warehouse = Data.WarehouseINTHEMSYD;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			Factory.SaveForTesting();

			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContextType.TransitReceive, "EXTREF1");

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Consignment cannot be updated as the Warehouse specified 'TWH' differs from the Consignment Warehouse 'TW2'.",
				() => GetDataObjectReader(new UniversalObjectFactory(), Data.ShipmentDataObject).ReadIntoBusinessObject());
		}

		#endregion

		#region TestUpdateRCN_NoMatchingWarehouse_DoesNotThrow

		public void TestUpdateRCN_NoMatchingWarehouse_DoesNotThrow()
		{
			var warehouse = Data.WarehouseINTHEMSYD;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK, "EXTREF1");
			Factory.SaveForTesting();

			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.AddDataTarget(DataContextType.TransitReceive, "EXTREF1");
			Data.ShipmentDataObject.OrganizationAddressCollection.RemoveAll(a => true);

			AssertNoExceptionThrown("Should not require a warehouse when updating targetting an existing consignment.",
				() => GetDataObjectReader(new UniversalObjectFactory(), Data.ShipmentDataObject).ReadIntoBusinessObject());
		}

		#endregion

		#region TestLogForRCNLinkedToShipment

		public void TestLogForRCNLinkedToShipment()
		{
			Data.SetupForForwardingImport();
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("SHIP123", "PKG-1", "PKG-2", "PKG-3");
			receiveConsignmentDataObject.WayBillNumber = "HSB123";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIP123";
			Factory.SaveForTesting();

			var outboundSessionTracker = new Mock<IDataWritingManager>();
			Logger.OutboundSessionTracker = outboundSessionTracker.Object;

			var reader = GetDataObjectReader(Factory, receiveConsignmentDataObject);
			reader.ReadIntoBusinessObject();
			AssertContains("Receive Consignment HSB123 linked to Shipment SHIP123.", Logger.Logs);
		}

		#endregion

		#region TestLogMessage_CombinedInstructions

		class WhsTransitReceiveConsignmentDataContextManagerForTesting : WhsTransitReceiveConsignmentDataContextManager
		{
			public WhsTransitReceiveConsignmentDataObjectReader GetShipmentDataObjectReaderForTesting(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
				=> (WhsTransitReceiveConsignmentDataObjectReader)GetShipmentDataObjectReader(universalShipment, logger, factory);
		}

		public void TestLogMessage_CombinedInstructions_GivenForwardingShipment()
		{
			string rcnExpectedCusEntryNumberLogs = @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - Deleting existing booked Packages from previous Universal Import for Receive Consignment 'Waybill123'.";

			string dcnExpectedCusEntryNumberLogs = @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...";

			TestLogMessage_CombinedInstructions(DataContextType.ForwardingShipment, rcnExpectedCusEntryNumberLogs, dcnExpectedCusEntryNumberLogs, 2);
		}

		public void TestLogMessage_CombinedInstructions_GivenLandTransportConsignment()
		{
			string rcnExpectedCusEntryNumberLogs = @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - Deleting existing booked Packages from previous Universal Import for Receive Consignment 'Waybill123'.";

			string dcnExpectedCusEntryNumberLogs = "";

			TestLogMessage_CombinedInstructions(DataContextType.LandTransportConsignment, rcnExpectedCusEntryNumberLogs, dcnExpectedCusEntryNumberLogs, 2);
		}

		public void TestLogMessage_CombinedInstructions_GivenSeaCargoOutturn()
		{
			string rcnExpectedCusEntryNumberLogs = @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - Deleting existing booked Packages from previous Universal Import for Receive Consignment 'Waybill123'.";

			string dcnExpectedCusEntryNumberLogs = "";

			TestLogMessage_CombinedInstructions(DataContextType.SeaCargoOutturn, rcnExpectedCusEntryNumberLogs, dcnExpectedCusEntryNumberLogs, 1);
		}

		void TestLogMessage_CombinedInstructions(DataContextType dataContextType, string rcnExpectedCusEntryNumberLogs, string dcnExpectedCusEntryNumberLogs, int cusEntryNumber)
		{
			Data.SetupForForwardingImport();
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(Data.ShipmentDataObject, shipmentNumber: null);
			Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject, consolNumber: "C1000000", shipmentNumber: null);
			Data.ShipmentDataObject.DataContext.AddDataSource(dataContextType, "S1000000");
			Data.HeaderDataObject.DataContext.AddDataSource(dataContextType, "S1000000");
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWX } } });

			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWX } } });
			var manager = new WhsTransitReceiveConsignmentDataContextManagerForTesting();
			Data.HeaderDataObject.DataContext.AddDataTarget(DataContextType.TransitReceive, "Waybill123");
			var reader = manager.GetShipmentDataObjectReaderForTesting(Data.HeaderDataObject, Logger, Factory);
			var consignment = reader.ReadIntoBusinessObject();

			AssertEquals("Log message must not have dispatch consol loading text.",
string.Format(@"Information - ================================Processing Receive Instruction================================
Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Source is '{0} - S1000000'.
Information - Searching for Receive Consignment for '{0} - S1000000'.
Information - No matching Receive Consignment found, creating new Receive Consignment.
Information - Populating Receive Consignment...{1}
Information - Populating Packages for Receive Consignment...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - No matching WhsItemPackageState found, creating new WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Receive Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Successfully saved Package with {2} x CusEntryNumber, 1 x PkgPackageJob, 1 x PkgPackage.
Information - ================================Processing Dispatch Instruction================================
Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Source is '{0} - S1000000'.
Information - Searching for Dispatch Consignment for '{0} - S1000000'.
Information - No matching Dispatch Consignment found, creating new Dispatch Consignment.
Information - Populating Dispatch Consignment...{3}
Information - Populating Packages for Dispatch Consignment...
Information - Some Imported Packlines do not have Package IDs. Registry setting set to TYP. Attempting to match by Receive Consignment + Packline Quantity + Pack Type.
Information - Searching for Receive Consignment for '{0} - S1000000'.
Information - Found matching Receive Consignment 'RC00000001 - Waybill123'.
Information - Some Imported Packlines do not have Package IDs. Registry setting set to TYP. Attempting to match by Receive Consignment + Packline Quantity + Pack Type.
Information - Searching for Receive Consignment for '{0} - S1000000'.
Information - Found matching Receive Consignment 'RC00000001 - Waybill123'.
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - The following packages have been attached to Dispatch Consignment Waybill123:
UXML Package    Package        RCN
2 PKG           2 PKG          Waybill123 (RC00000001)
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Dispatch Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Dispatch Consignment DC00000001 from UniversalShipment.
Information - Added Receive Consignment RC00000001 from UniversalShipment.", dataContextType.ToString(), rcnExpectedCusEntryNumberLogs, cusEntryNumber, dcnExpectedCusEntryNumberLogs), Logger.Logs);
		}

		public void TestLogMessage_CombinedInstructions_GivenBuyersConsol()
		{
			Data.SetupForForwardingImport();
			var masterShipment = Data.CreateShipmentWithPackages("S1000000", "P1");
			masterShipment.IsBuyersConsol = true;
			var subShipment = Data.CreateShipmentWithPackages("S1000001", "P2");

			masterShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });
			Logger.TopLevelDataObject = masterShipment;
			masterShipment.ShipmentType = new CodeDescriptionPair() { Code = Constants.ShipmentTypes.BuyersConsolLead };

			masterShipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWX } } });
			var manager = new WhsTransitReceiveConsignmentDataContextManagerForTesting();
			var reader = manager.GetShipmentDataObjectReaderForTesting(masterShipment, Logger, Factory);
			var consignment = reader.ReadIntoBusinessObject();

			AssertEquals("Log message must not have dispatch consol loading text.",
string.Format(@"Information - ================================Processing Receive Instruction================================
Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Source is 'ForwardingShipment - S1000000'.
Information - Searching for Receive Consignment for 'ForwardingShipment - S1000000'.
Information - No matching Receive Consignment found, creating new Receive Consignment.
Information - Populating Receive Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Receive Consignment...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - No matching WhsItemPackageState found, creating new WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Receive Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Source is 'ForwardingShipment - S1000001'.
Information - Searching for Receive Consignment for 'ForwardingShipment - S1000001'.
Information - No matching Receive Consignment found, creating new Receive Consignment.
Information - Populating Receive Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Receive Consignment...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - No matching WhsItemPackageState found, creating new WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Receive Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Receive Consignment RC00000002 from UniversalShipment.
Information - Successfully saved Receive Consignment RC00000002 with 4 x CusEntryNumber, 2 x PkgPackage, 2 x PkgPackageJob, 2 x WhsItemPackageState.
Information - ================================Processing Dispatch Instruction================================
Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Source is 'ForwardingShipment - S1000000'.
Information - Searching for Dispatch Consignment for 'ForwardingShipment - S1000000'.
Information - No matching Dispatch Consignment found, creating new Dispatch Consignment.
Information - Populating Dispatch Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Dispatch Consignment...
Information - Some Imported Packlines have Package IDs. Attempting to match by Package IDs.
Information - Some Imported Packlines have Package IDs. Attempting to match by Package IDs.
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - The following packages have been attached to Dispatch Consignment S1000000:
Package        RCN
P1             S1000000 (RC00000001)
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Dispatch Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Matching 'ConsigneePickupDeliveryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'DeliveryLocalCartage':- Matched to 'INTHEMSYD' by code, address '' (only address).
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Source is 'ForwardingShipment - S1000001'.
Information - Searching for Dispatch Consignment for 'ForwardingShipment - S1000001'.
Information - No matching Dispatch Consignment found, creating new Dispatch Consignment.
Information - Populating Dispatch Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Dispatch Consignment...
Information - Some Imported Packlines have Package IDs. Attempting to match by Package IDs.
Information - Some Imported Packlines have Package IDs. Attempting to match by Package IDs.
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - The following packages have been attached to Dispatch Consignment S1000001:
Package        RCN
P2             S1000001 (RC00000002)
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Dispatch Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Matching 'ConsigneePickupDeliveryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'DeliveryLocalCartage':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Dispatch Consignment DC00000002 from UniversalShipment.
Information - Added Dispatch Consignment DC00000001 from UniversalShipment.
Information - Added Receive Consignment RC00000001 from UniversalShipment."), Logger.Logs);
		}

		public void TestLogMessage_GivenDataTarget()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("Waybill123", new string[] { "PKG1" });
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);
			rcn.WRC_JobID = "RC0000001";

			Factory.SaveForTesting();

			Logger.TopLevelDataObject = receiveConsignmentDataObject;
			Data.SetupNewDataContextWithDataSource(receiveConsignmentDataObject, shipmentNumber: null);
			receiveConsignmentDataObject.DataContext.AddDataTarget(DataContextType.TransitReceive, rcn.WRC_JobID);

			var manager = new WhsTransitReceiveConsignmentDataContextManagerForTesting();
			var reader = manager.GetShipmentDataObjectReaderForTesting(receiveConsignmentDataObject, Logger, Factory);
			var consignment = reader.ReadIntoBusinessObject();

			AssertEquals("Log message must not have dispatch consol loading text.",
@"Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Warning - Could not find Sending Party Organisation with eHub Client ID 'EDIDATEDI'.
Information - Matching 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to 'EDICUS' by code. Successfully loaded Booking Party.
Information - Data Target is 'TransitReceive - RC0000001'.
Information - Successfully loaded matching Receive Consignment RC0000001.
Information - Populating Receive Consignment RC0000001...
Information - Populating Packages for Receive Consignment...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Information - Package with ID PKG1 did not update weight, volume or dimension.
Information - Booked Package Detail with ID PKG1 did not update weight, volume or dimension.
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Receive Consignment...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Updated Receive Consignment RC0000001 from UniversalShipment.", Logger.Logs);
		}

		public void TestLogsForConsignmentMatching_GivenEmptyDataSource()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("Waybill123", new string[] { "PKG1" });
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);

			Factory.SaveForTesting();

			Logger.TopLevelDataObject = receiveConsignmentDataObject;
			Data.SetupNewDataContextWithDataSource(receiveConsignmentDataObject, shipmentNumber: null);

			new WhsTransitReceiveConsignmentDataObjectReader(receiveConsignmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Information - Searching for Receive Consignment.", Logger.Logs);
		}

		#endregion

		#region TestReadIntoBusinessObject_WithUnrecognisedAdditionalReferenceType

		public void TestReadIntoBusinessObject_WithUnrecognisedAdditionalReferenceType_AddNote()
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123");
			var warehouse = Data.Warehouse;

			Helper.SetShipmentAdditionalReference(shipmentDataObject,
				(WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, "Invoice No", "AU"),
				("UNA", "Unknown Type A", "AU"));

			var receiveConsignmentAfterReading = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created receive consignment.", receiveConsignmentAfterReading);
			Factory.SaveForTesting();

			var notes = receiveConsignmentAfterReading.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Note should only contain unrecognised additional reference type notes.", 1, notes.Count());
			var expectedMessage =
$@"Type: UNA
Number: Unknown Type A
Country: AU";
			Helper.AssertNoteContents(notes.Single(), PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Code, expectedMessage, StmNoteDescription.Int, false);
		}

		public void TestReadIntoBusinessObject_WithUnrecognisedAdditionalReferenceType_UpdateNote()
		{
			var shipmentDataObject = Data.CreateShipmentWithPackages("SHIPMENT123");
			var warehouse = Data.Warehouse;

			Helper.SetShipmentAdditionalReference(shipmentDataObject,
				(WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, "Invoice No", "AU"),
				("UNA", "Unknown Type A", "AU"),
				("UNB", "Unknown Type B", "AU"));

			new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			Helper.SetShipmentAdditionalReference(shipmentDataObject,
				(WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, "Invoice No", "AU"),
				("UNB", "Unknown Type B", "AU"),
				("UNC", "Unknown Type C", "AU"));

			var updatedReceiveConsignment = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var updatedNote = updatedReceiveConsignment.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Note should add new unrecognised additional reference type notes.", 1, updatedNote.Count());
			var updatedExpectedMessage =
$@"Type: UNA
Number: Unknown Type A
Country: AU

Type: UNB
Number: Unknown Type B
Country: AU

Type: UNC
Number: Unknown Type C
Country: AU";
			Helper.AssertNoteContents(updatedNote.Single(), PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Code, updatedExpectedMessage, StmNoteDescription.Int, false);

			Helper.SetShipmentAdditionalReference(shipmentDataObject,
				(WarehouseAdditionalReferenceTypes.Codes.InvoiceNumber, "Invoice No", "AU"),
				("UNB", "Unknown Type B Updated", "AU"),
				("UNC", "Unknown Type C", "AU"),
				(WarehouseAdditionalReferenceTypes.Codes.HouseBill, "HouseBill", "AU"),
				("UND", "Unknown Type D", "AU"));

			var doubleUpdatedReceiveConsignment = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var doubleUpdatedNotes = doubleUpdatedReceiveConsignment.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Note should contain updated unrecognised additional reference type notes.", 1, doubleUpdatedNotes.Count());
			var doubleUpdatedExpectedMessage =
$@"Type: UNA
Number: Unknown Type A
Country: AU

Type: UNB
Number: Unknown Type B
Country: AU

Type: UNC
Number: Unknown Type C
Country: AU

Type: UNB
Number: Unknown Type B Updated
Country: AU

Type: UND
Number: Unknown Type D
Country: AU";
			var doubleUpdatedNote = doubleUpdatedReceiveConsignment.Notes.GetAllNotes().Cast<StmNote>();
			Helper.AssertNoteContents(doubleUpdatedNote.Single(), PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Code, doubleUpdatedExpectedMessage, StmNoteDescription.Int, false);
		}

		#endregion

		#region TestImportNotes

		public void TestPopulateBizO_ImportNoteInXML()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var consolDO = Data.HeaderDataObject;
			var shipmentDO = Data.ShipmentDataObject;

			consolDO.WayBillNumber = "MB1";
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C00000001", runSheet: "R1");
			consolDO.SetNoteCollection(() => Helper.CreateNotes("console", "test console"));
			shipmentDO.SetNoteCollection(() => new DataObjectList<Note>()
			{
				Helper.CreateNote("Client-Visible Note", "Client-Visible Note", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote("Private Note", "Private Note", StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive),
				Helper.CreateNote("Agent-Visible Note", "Agent-Visible Note", StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive),
				Helper.CreateNote("Internal Note", "Internal Note", StmNoteDescription.Int, StmNoteDescription.IntDescriptive)
			});
			Logger.TopLevelDataObject = shipmentDO;

			var createdConsignment = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			var notes = createdConsignment.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Should import all note visibility types.", 4, notes.Count());
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Pub), "Client-Visible Note", "Client-Visible Note", StmNoteDescription.Pub, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Prv), "Private Note", "Private Note", StmNoteDescription.Prv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Agv), "Agent-Visible Note", "Agent-Visible Note", StmNoteDescription.Agv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Int), "Internal Note", "Internal Note", StmNoteDescription.Int, true);

			shipmentDO.SetNoteCollection(() => new DataObjectList<Note>()
			{
				Helper.CreateNote("Client-Visible Note", "Client-Visible Note UPDATED", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote("Private Note", "Private Note UPDATED", StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive),
				Helper.CreateNote("Agent-Visible Note", "Agent-Visible Note UPDATED", StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive),
				Helper.CreateNote("Internal Note", "Internal Note UPDATED", StmNoteDescription.Int, StmNoteDescription.IntDescriptive)
			});
			createdConsignment = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDO, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			notes = createdConsignment.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Should have updated notes.", 4, notes.Count());
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Pub), "Client-Visible Note", "Client-Visible Note UPDATED", StmNoteDescription.Pub, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Prv), "Private Note", "Private Note UPDATED", StmNoteDescription.Prv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Agv), "Agent-Visible Note", "Agent-Visible Note UPDATED", StmNoteDescription.Agv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Int), "Internal Note", "Internal Note UPDATED", StmNoteDescription.Int, true);
		}

		#endregion

		#region TestPopulateBizO_ImportAdditionalServices

		[TestDate(2021, 12, 15)]
		public void TestReadIntoBusinessObject_AdditionalServices_AssertFields()
		{
			Data.SetupForForwardingImport();
			var bookedTime = ZDateTime.Now.ToSmallDateTime();
			var completedTime = ZDateTime.Now.ToSmallDateTime();
			var duration = ZDateTime.Now.ToTimeSpan();
			var reference = "Test Reference";
			var serviceNote = "Test Service Note";
			var additionalService = Helper.CreateAdditionalService(
				"CLN",
				bookedTime,
				completedTime,
				Data.Orgs.Warehouse_WUFSHIJNB,
				Data.Orgs.Warehouse_WUFSHIJNB,
				duration,
				reference,
				serviceNote,
				10);

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(additionalService);
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(
				result.jobServices.First(),
				"CLN",
				bookedTime,
				completedTime,
				Data.Orgs.WUFSHIJNB.PK,
				Data.Orgs.WUFSHIJNB.MainAddress.PK,
				duration,
				reference,
				serviceNote,
				10);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedTime.ToDateTime(), "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completedTime.ToDateTime(), "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_OneService_ImportTwice()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_TwoDifferentServices_ImportTwice()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("WSH", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			AssertNoExceptionThrown(() => GetDataObjectReader(Factory, shipmentDO).ReadIntoBusinessObject());
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var receiveConsignment = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).SingleOrDefault();
			var jobServices = receiveConsignment.Services.Cast<JobService>();

			AssertContainsExactElementsInAnyOrder(new[] { "CLN", "WSH" }, jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, receiveConsignment, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=WSH");

			AssertNoExceptionThrown(() => GetDataObjectReader(newFactory, shipmentDO).ReadIntoBusinessObject());
			Factory.SaveForTesting();

			var newFactory2 = new UniversalObjectFactory();
			var receiveConsignment2 = newFactory2.Load<WhsItemReceiveConsignment>(new ZQuery()).SingleOrDefault();
			var jobServices2 = receiveConsignment.Services.Cast<JobService>();

			AssertContainsExactElementsInAnyOrder(new[] { "CLN", "WSH" }, jobServices2.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, receiveConsignment2, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=WSH");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_TwoSameServices_ImportTwice_WithServiceID()
		{
			TestReadIntoBusinessObject_AdditionalServices_TwoSameServices_ImportTwiceCore(true);
		}
		public void TestReadIntoBusinessObject_AdditionalServices_TwoSameServices_ImportTwice_WithoutServiceID()
		{
			TestReadIntoBusinessObject_AdditionalServices_TwoSameServices_ImportTwiceCore(false);
		}

		public void TestReadIntoBusinessObject_AdditionalServices_TwoSameServices_ImportTwiceCore(bool hasServiceID)
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB, serviceID: hasServiceID ? "SRV001" : ""));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB, serviceID: hasServiceID ? "SRV002" : ""));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			if (hasServiceID)
			{
				var result = SendUxmlThenGetJobServices(shipmentDO, 2);
				AssertContainsExactElementsInAnyOrder(new[] { "CLN", "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
				AssertNotNullOrEmpty($"Expected ServiceId not empty:", result.jobServices.ToList()[0].ES_ServiceId);
				AssertNotNullOrEmpty($"Expected ServiceId not empty:", result.jobServices.ToList()[1].ES_ServiceId);
				AssertContainsExactElementsInAnyOrder(new[] { "SRV001", "SRV002" }, result.jobServices.Select(t => t.ES_ExternalServiceId));
				AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

				result = SendUxmlThenGetJobServices(shipmentDO, 2);
				AssertContainsExactElementsInAnyOrder(new[] { "CLN", "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
				AssertNotNullOrEmpty($"Expected ServiceId not empty:", result.jobServices.ToList()[0].ES_ServiceId);
				AssertNotNullOrEmpty($"Expected ServiceId not empty:", result.jobServices.ToList()[1].ES_ServiceId);
				AssertContainsExactElementsInAnyOrder(new[] { "SRV001", "SRV002" }, result.jobServices.Select(t => t.ES_ExternalServiceId));
				AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
			}
			else
			{
				var result = SendUxmlThenGetJobServices(shipmentDO, 1);
				AssertContainsExactElementsInAnyOrder(new[] { "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
				AssertContainsExactElementsInAnyOrder(new[] { "" }, result.jobServices.Select(t => t.ES_ServiceId));
				AssertContainsExactElementsInAnyOrder(new[] { "" }, result.jobServices.Select(t => t.ES_ExternalServiceId));
				AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

				result = SendUxmlThenGetJobServices(shipmentDO, 1);
				AssertContainsExactElementsInAnyOrder(new[] { "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
				AssertContainsExactElementsInAnyOrder(new[] { "" }, result.jobServices.Select(t => t.ES_ServiceId));
				AssertContainsExactElementsInAnyOrder(new[] { "" }, result.jobServices.Select(t => t.ES_ExternalServiceId));
				AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
			}
		}

		public void TestReadIntoBusinessObject_AdditionalServices_OneServices_ImportTwice_AddADifferentServiceBeforeReimport()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("WSH", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));

			result = SendUxmlThenGetJobServices(shipmentDO, 2);
			AssertContainsExactElementsInAnyOrder(new[] { "CLN", "WSH" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=WSH");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_OneServices_ImportTwice_AddASameServiceBeforeReimport()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			result = SendUxmlThenGetJobServices(shipmentDO, 1);
			AssertContainsExactElementsInAnyOrder(new[] { "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_OneService_ImportTwice_ClearServiceCollectionBeforeReimport()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			result = SendUxmlThenGetJobServices(shipmentDO, 0);
		}

		public void TestReadIntoBusinessObject_AdditionalServices_ImportTwice_RemoveExistingServiceAndAddNewOnesBeforeReimport()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("WSH", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("STE", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			result = SendUxmlThenGetJobServices(shipmentDO, 2);
			AssertContainsExactElementsInAnyOrder(new[] { "WSH", "STE" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=WSH", "|FAC=CFS|LOC=Johannesburg|TYP=STE");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_ServiceIsCompleted_ImportTwice()
		{
			Data.SetupForForwardingImport();
			var completedDate = new ZDateTime(2021, 12, 16, 0, 0, 0);

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: completedDate));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, completedTime: completedDate);
			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));

			result = SendUxmlThenGetJobServices(shipmentDO, 1);
			AssertContainsExactElementsInAnyOrder(new[] { "CLN" }, result.jobServices.Select(t => t.ES_ServiceCode));
			AssertContainsExactElementsInAnyOrder(new[] { completedDate }, result.jobServices.Select(t => t.ES_Completed));
			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_ServiceIsCompleted_ImportTwice_ClearServiceCollectionBeforeReimport()
		{
			Data.SetupForForwardingImport();
			var completedDate = new ZDateTime(2021, 12, 16, 0, 0, 0);

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: completedDate));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, completedTime: completedDate);
			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();

			result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, completedTime: completedDate);
			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_ServiceLocationDoesNotMatchCurrentWarehouse()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, bookedTime: bookedDate);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Consignor_CRAHOLSYD));

			result = SendUxmlThenGetJobServices(shipmentDO, 0);
		}

		public void TestReadIntoBusinessObject_AdditionalServices_MissingWarehouseLocation()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			Helper.AssertAdditionalService(result.jobServices.First(), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN"));

			result = SendUxmlThenGetJobServices(shipmentDO, 0);
		}

		public void TestReadIntoBusinessObject_AdditionalServices_DeleteExistingBookedServices()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 2);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "CLN"), "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, bookedTime: bookedDate);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "FUM"), "FUM", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, bookedTime: bookedDate);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=FUM");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();

			result = SendUxmlThenGetJobServices(shipmentDO, 0);
			var oldSrvLogs = result.parent.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode);
			AssertEquals("Should still have 2 Service Requested events after re-importing.", 2, oldSrvLogs.Count());
			Assert("Old Service Requested Events should have been cancelled.", oldSrvLogs.All(l => l.SL_IsCancelled));
		}

		public void TestReadIntoBusinessObject_AdditionalServices_ServiceEventsAddedToParentRCN()
		{
			Data.SetupForForwardingImport();
			var newBookedDate = new ZDateTime(2021, 12, 16, 0, 0, 0);
			var completedDate = new ZDateTime(2021, 12, 16, 0, 0, 0);

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("TAI", bookedTime: bookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: completedDate));

			// Events not raised for services with location not matching to the warehouse
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("QIN"));

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 3);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "CLN"), "CLN", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "FUM"), "FUM", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			Helper.AssertAdditionalService(result.jobServices.Single(s => s.ES_ServiceCode == "TAI"), "TAI", bookedTime: bookedDate, locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK, completedTime: completedDate);
			AssertLogParentLogs(Events.ServiceRequestedCode, result.parent, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN", "|FAC=CFS|LOC=Johannesburg|TYP=FUM", "|FAC=CFS|LOC=Johannesburg|TYP=TAI");
			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completedDate, "|FAC=CFS|LOC=Johannesburg|TYP=TAI");

			// Update services
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", bookedTime: newBookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", bookedTime: newBookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: completedDate));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("TAI", bookedTime: newBookedDate, location: Data.Orgs.Warehouse_WUFSHIJNB, completedTime: completedDate));

			result = SendUxmlThenGetJobServices(shipmentDO, 3);
			var svrLogs = result.parent.Logs.Find(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == Events.ServiceRequestedCode);
			AssertEquals("Parent should have 3 Service Requested Events.", 3, svrLogs.Count());
			AssertLog(svrLogs.Single(l => l.SL_Reference.Contains("TYP=CLN")), Events.ServiceRequestedCode, newBookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=CLN");
			AssertLog(svrLogs.Single(l => l.SL_Reference.Contains("TYP=FUM")), Events.ServiceRequestedCode, newBookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=FUM");
			AssertLog(svrLogs.Single(l => l.SL_Reference.Contains("TYP=TAI")), Events.ServiceRequestedCode, bookedDate, "|FAC=CFS|LOC=Johannesburg|TYP=TAI");

			AssertLogParentLogs(Events.ServiceCompletedCode, result.parent, completedDate, "|FAC=CFS|LOC=Johannesburg|TYP=FUM", "|FAC=CFS|LOC=Johannesburg|TYP=TAI");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_NoBookedTimeAndCompletedTime_NoEventsAdded()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("TAI", location: Data.Orgs.Warehouse_WUFSHIJNB));

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 3);
			AssertEquals("No service events added to parent.", 0, result.parent.Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceRequestedCode || l.SL_SE_NKEvent == Events.ServiceCompletedCode).Count());
		}

		public void TestReadIntoBusinessObject_AdditionalServices_PopulateAddOnValueWithShipmentNumber()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB));

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			var clnService = result.jobServices.Single();
			Helper.AssertAdditionalService(clnService, "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			var addOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, clnService.PK)).Single();
			Helper.AssertAddOnValue(addOnValue, clnService.PK, clnService.TablePrefix, "STR", shipmentDO.FirstDataSource().Key.GetValueOrDefault(), "JobNumber");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB));
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", location: Data.Orgs.Warehouse_WUFSHIJNB));

			result = SendUxmlThenGetJobServices(shipmentDO, 2);
			var clnServiceAfterReading = result.jobServices.Single(s => s.ES_ServiceCode == "CLN");
			var fumServiceAfterReading = result.jobServices.Single(s => s.ES_ServiceCode == "FUM");
			var clnAddOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, clnServiceAfterReading.PK)).Single();
			var fumAddOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, fumServiceAfterReading.PK)).Single();
			Helper.AssertAddOnValue(clnAddOnValue, clnServiceAfterReading.PK, clnService.TablePrefix, "STR", shipmentDO.FirstDataSource().Key.GetValueOrDefault(), "JobNumber");
			Helper.AssertAddOnValue(fumAddOnValue, fumServiceAfterReading.PK, clnService.TablePrefix, "STR", shipmentDO.FirstDataSource().Key.GetValueOrDefault(), "JobNumber");
		}

		public void TestReadIntoBusinessObject_AdditionalServices_PopulateAddOnValueWithShipmentNumber_DifferentSenderIDWillNotUpdateExistingService()
		{
			Data.SetupForForwardingImport();

			var shipmentDO = Data.ShipmentDataObject;
			Logger.TopLevelDataObject = shipmentDO;
			shipmentDO.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDO.DataContext.DataSourceCollection.First().Key = "JobNumber123";
			shipmentDO.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>());
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("CLN", location: Data.Orgs.Warehouse_WUFSHIJNB));

			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });

			var result = SendUxmlThenGetJobServices(shipmentDO, 1);
			var clnService = result.jobServices.Single();
			Helper.AssertAdditionalService(clnService, "CLN", locationPK: Data.Orgs.WUFSHIJNB.MainAddress.PK);
			var addOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, clnService.PK)).Single();
			Helper.AssertAddOnValue(addOnValue, clnService.PK, clnService.TablePrefix, "STR", shipmentDO.FirstDataSource().Key.GetValueOrDefault(), "JobNumber");

			shipmentDO.LocalProcessing.AdditionalServiceCollection.Clear();
			// Change the sender key so it doesn't modify the existing CLN service
			shipmentDO.DataContext.DataSourceCollection.First().Key = "New_JobNumber";
			shipmentDO.LocalProcessing.AdditionalServiceCollection.Add(Helper.CreateAdditionalService("FUM", location: Data.Orgs.Warehouse_WUFSHIJNB));

			result = SendUxmlThenGetJobServices(shipmentDO, 2);
			var clnServiceAfterReading = result.jobServices.Single(s => s.ES_ServiceCode == "CLN");
			var fumServiceAfterReading = result.jobServices.Single(s => s.ES_ServiceCode == "FUM");
			var clnAddOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, clnServiceAfterReading.PK)).Single();
			var fumAddOnValue = Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, fumServiceAfterReading.PK)).Single();
			Helper.AssertAddOnValue(clnAddOnValue, clnServiceAfterReading.PK, clnService.TablePrefix, "STR", "JobNumber123", "JobNumber");
			Helper.AssertAddOnValue(fumAddOnValue, fumServiceAfterReading.PK, clnService.TablePrefix, "STR", "New_JobNumber", "JobNumber");
		}

		(IEnumerable<WhsJobService> jobServices, IStmALogParent parent) SendUxmlThenGetJobServices(UniversalShipment shipmentDO, int expectedCount)
		{
			var factory = NewUniversalObjectFactory();
			AssertNoExceptionThrown(() => GetDataObjectReader(factory, shipmentDO).ReadIntoBusinessObject());
			factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignment = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery()).SingleOrDefault();
			var jobServices = receiveConsignment.Services.Cast<WhsJobService>();
			AssertEquals($"Service Collection Count should equal {expectedCount}.", expectedCount, jobServices.Count());

			return (jobServices, receiveConsignment);
		}

		#endregion

		#region TestReadIntoBusinessObject_DoesNotRejectReceive_RCNIsCompleted

		public void TestReadIntoBusinessObject_DoesNotRejectReceive_RCNIsCompleted_MatchedByJobLink()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", new string[] { "PKG1" });
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);
			rcn.WRC_CompleteTime = DateTimeOffset.Now;
			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => GetDataObjectReader(new UniversalObjectFactory(), receiveConsignmentDataObject).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_DoesNotRejectReceive_RCNIsCompleted_MatchedByHouseBill()
		{
			var receiveConsignmentDataObject = Data.CreateShipmentWithPackages("S1000000", new string[] { "PKG1" });
			var rcn = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject);
			rcn.WRC_CompleteTime = DateTimeOffset.Now;
			Factory.SaveForTesting();

			DeleteJobLinks(Factory);
			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => GetDataObjectReader(new UniversalObjectFactory(), receiveConsignmentDataObject).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_DoesNotRejectReceive_RCNIsCompleted_MatchedByConsignmentID()
		{
			var receiveConsignmentDataObject1 = Data.CreateShipmentWithPackages("Bill123", new string[] { "PKG1" });
			var rcn1 = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject1);
			rcn1.WRC_CompleteTime = DateTimeOffset.Now;

			var receiveConsignmentDataObject2 = Data.CreateShipmentWithPackages("S1000000", new string[] { "PKG2" });
			var rcn2 = Data.CreateReceiveConsignmentInDB(receiveConsignmentDataObject2);
			rcn2.WRC_CompleteTime = DateTimeOffset.Now;
			Factory.SaveForTesting();

			receiveConsignmentDataObject1.DataContext.GetMatchingDataSource(DataContextType.ForwardingShipment).Key = "S1000000";

			DeleteJobLinks(Factory);
			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => GetDataObjectReader(new UniversalObjectFactory(), receiveConsignmentDataObject1).ReadIntoBusinessObject());
		}

		public void TestReadIntoBusinessObject_DoesNotRejectDispatch_SubShipmentIsComplete()
		{
			var subShipment = Data.CreateShipmentWithPackages("CONID1", packageIDs: new string[] { "PKG-1" });
			var masterShipment = Data.CreateShipmentWithPackages("CONID2", packageIDs: new string[] { "PKG-2" });
			masterShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });
			masterShipment.ShipmentType = new CodeDescriptionPair() { Code = Constants.ShipmentTypes.BuyersConsolLead };

			var rcn1 = Data.CreateReceiveConsignmentInDB(subShipment);
			var rcn2 = Data.CreateReceiveConsignmentInDB(masterShipment);
			rcn1.WRC_CompleteTime = DateTimeOffset.Now;

			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => new WhsTransitReceiveConsignmentDataObjectReader(masterShipment, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestSetRCNDirection

		public void TestRCNDirectionRule_InboundPortOfLoadingIsForeignPort()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var rcnSetDirection = GenerateConsolRCNWithDirection(inboundPOL: ForeignPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "IMP");
			rcnSetDirection = GenerateShipmentRCNWithDirection(inboundPOL: ForeignPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "IMP");
		}

		public void TestRCNDirectionRule_InboundPortOfLoadingIsForeignPortPrecedence()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var rcnSetDirection = GenerateConsolRCNWithDirection(ForeignPort, ForeignPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "IMP");
			rcnSetDirection = GenerateShipmentRCNWithDirection(ForeignPort, ForeignPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "IMP");
		}

		public void TestRCNDirectionRule_OutboundPortOfDischargeIsForeignPort()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var rcnSetDirection = GenerateConsolRCNWithDirection(outboundPOD: ForeignPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "EXP");
			rcnSetDirection = GenerateShipmentRCNWithDirection(outboundPOD: ForeignPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "EXP");
		}

		public void TestRCNDirectionRule_OutboundPortOfDischargeIsForeignPortPrecedence()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var rcnSetDirection = GenerateConsolRCNWithDirection(DomesticPort, ForeignPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "EXP");
			rcnSetDirection = GenerateShipmentRCNWithDirection(DomesticPort, ForeignPort, DomesticPort, DomesticPort, DomesticPort, DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "EXP");
		}

		public void TestRCNDirectionRule_InboundPortOfLoadingIsDomesticPortAndDestinationIsDomesticPort()
		{
			var rcnSetDirection = GenerateConsolRCNWithDirection(inboundPOL: DomesticPort, destinationPort: DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "DOM");
			rcnSetDirection = GenerateShipmentRCNWithDirection(inboundPOL: DomesticPort, destinationPort: DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "DOM");
		}

		public void TestRCNDirectionRule_OutboundPortOfDischargeIsDomesticPortAndOriginIsDomesticPort()
		{
			var rcnSetDirection = GenerateConsolRCNWithDirection(outboundPOD: DomesticPort, originPort: DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "DOM");
			rcnSetDirection = GenerateShipmentRCNWithDirection(outboundPOD: DomesticPort, originPort: DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "DOM");
		}

		public void TestRCNDirectionRule_NoOutboundLegAndShipmentPlannedLoadIsDomesticPortAndShipmentPlannedDischargeIsDomesticPort()
		{
			var rcnSetDirection = GenerateConsolRCNWithDirection(shippingPL: DomesticPort, shippingPD: DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "DOM");
			rcnSetDirection = GenerateShipmentRCNWithDirection(shippingPL: DomesticPort, shippingPD: DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "DOM");
		}

		public void TestRCNDirectionRule_NoOutboundLegAndShipmentPlannedLoadIsDomesticPortAndShipmentPlannedDischargeIsForeignPort()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var rcnSetDirection = GenerateConsolRCNWithDirection(shippingPL: DomesticPort, shippingPD: ForeignPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "EXP");
			rcnSetDirection = GenerateShipmentRCNWithDirection(shippingPL: DomesticPort, shippingPD: ForeignPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "EXP");
		}

		public void TestRCNDirectionRule_NoInboundLegAndShipmentPlannedLoadIsForeignPortAndShipmentPlannedDischargeIsDomesticPort()
		{
			AssertNotEquals("US", HomePort.Substring(0, 2));
			var rcnSetDirection = GenerateConsolRCNWithDirection(shippingPL: ForeignPort, shippingPD: DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "IMP");
			rcnSetDirection = GenerateShipmentRCNWithDirection(shippingPL: ForeignPort, shippingPD: DomesticPort);
			AssertEquals(rcnSetDirection.WRC_Direction, "IMP");
		}

		public void TestRCNDirectionRule_NoMatchingConditionsReturnsNull()
		{
			var rcnSetDirection = GenerateConsolRCNWithDirection();
			AssertEquals(rcnSetDirection.WRC_Direction, ZString.Empty);
			rcnSetDirection = GenerateShipmentRCNWithDirection();
			AssertEquals(rcnSetDirection.WRC_Direction, ZString.Empty);
		}

		public WhsItemReceiveConsignment GenerateConsolRCNWithDirection(ZString? inboundPOL = null, ZString? outboundPOD = null, ZString? destinationPort = null, ZString? originPort = null, ZString? shippingPL = null, ZString? shippingPD = null)
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			Data.SetupForForwardingImport();
			var consol = Data.HeaderDataObject;
			TransportLeg inboundRoutingLeg = null;
			TransportLeg outboundRoutingLeg = null;

			if (inboundPOL is not null)
			{
				inboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: inboundPOL, portOfDischarge: homePort);
			}

			if (outboundPOD is not null)
			{
				outboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, portOfDischarge: outboundPOD);
			}

			var estimatedPickupDate = new ZDateTime(2006, 7, 7);
			Helper.AddTestDataToShipment(consol, inboundLeg: inboundRoutingLeg, outboundLeg: outboundRoutingLeg, portOfLoading: shippingPL, portOfDischarge: shippingPD, portOfOrigin: originPort, portOfDestination: destinationPort, estimatedPickup: estimatedPickupDate);

			var shipment = consol.SubShipmentCollection[0];
			Helper.AddTestDataToShipment(shipment, portOfLoading: shippingPL, portOfDischarge: shippingPD, portOfOrigin: originPort, portOfDestination: destinationPort, estimatedPickup: estimatedPickupDate);

			var consignment = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			return consignment;
		}

		public WhsItemReceiveConsignment GenerateShipmentRCNWithDirection(ZString? inboundPOL = null, ZString? outboundPOD = null, ZString? destinationPort = null, ZString? originPort = null, ZString? shippingPL = null, ZString? shippingPD = null)
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			Data.SetupForForwardingImport();
			var legs = new DataObjectList<TransportLeg>();
			TransportLeg inboundRoutingLeg = null;
			TransportLeg outboundRoutingLeg = null;

			if (inboundPOL is not null)
			{
				inboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: inboundPOL, portOfDischarge: homePort);
				legs.Add(inboundRoutingLeg);
			}

			if (outboundPOD is not null)
			{
				outboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, portOfDischarge: outboundPOD);
				legs.Add(outboundRoutingLeg);
			}

			var estimatedPickupDate = new ZDateTime(2006, 7, 7);
			var shipment = Data.CreateShipmentWithPackages("SHIP123");
			Helper.AddTestDataToShipment(shipment, inboundLeg: inboundRoutingLeg, outboundLeg: outboundRoutingLeg, portOfLoading: shippingPL, portOfDischarge: shippingPD, portOfOrigin: originPort, portOfDestination: destinationPort, estimatedPickup: estimatedPickupDate);

			var consignment = GetDataObjectReader(Factory, shipment).ReadIntoBusinessObject();
			return consignment;
		}

		public void TestRCNDirectionRule_ConsolIsImport_ShipmentIsImport() => GenerateConsolAndShipmentRCNWithDirection(ForeignPort, null, ForeignPort, DomesticPort, ForeignPort, null, ForeignPort, DomesticPort, "IMP");
		public void TestRCNDirectionRule_ConsolIsImport_ShipmentIsExport() => GenerateConsolAndShipmentRCNWithDirection(ForeignPort, null, ForeignPort, DomesticPort, null, ForeignPort, DomesticPort, ForeignPort, "IMP");
		public void TestRCNDirectionRule_ConsolIsImport_ShipmentIsDomestic() => GenerateConsolAndShipmentRCNWithDirection(ForeignPort, null, ForeignPort, DomesticPort, null, null, DomesticPort, DomesticPort, "IMP");
		public void TestRCNDirectionRule_ConsolIsImport_ShipmentIsBlank() => GenerateConsolAndShipmentRCNWithDirection(ForeignPort, null, ForeignPort, DomesticPort, null, null, null, null, "IMP");

		public void TestRCNDirectionRule_ConsolIsExport_ShipmentIsImport() => GenerateConsolAndShipmentRCNWithDirection(null, ForeignPort, DomesticPort, ForeignPort, ForeignPort, null, ForeignPort, DomesticPort, "EXP");
		public void TestRCNDirectionRule_ConsolIsExport_ShipmentIsExport() => GenerateConsolAndShipmentRCNWithDirection(null, ForeignPort, DomesticPort, ForeignPort, null, ForeignPort, DomesticPort, ForeignPort, "EXP");
		public void TestRCNDirectionRule_ConsolIsExport_ShipmentIsDomestic() => GenerateConsolAndShipmentRCNWithDirection(null, ForeignPort, DomesticPort, ForeignPort, null, null, DomesticPort, DomesticPort, "EXP");
		public void TestRCNDirectionRule_ConsolIsExport_ShipmentIsBlank() => GenerateConsolAndShipmentRCNWithDirection(null, ForeignPort, DomesticPort, ForeignPort, null, null, null, null, "EXP");

		public void TestRCNDirectionRule_ConsolIsDomestic_ShipmentIsImport() => GenerateConsolAndShipmentRCNWithDirection(null, null, DomesticPort, DomesticPort, ForeignPort, null, ForeignPort, DomesticPort, "DOM");
		public void TestRCNDirectionRule_ConsolIsDomestic_ShipmentIsExport() => GenerateConsolAndShipmentRCNWithDirection(null, null, DomesticPort, DomesticPort, null, ForeignPort, DomesticPort, ForeignPort, "DOM");
		public void TestRCNDirectionRule_ConsolIsDomestic_ShipmentIsDomestic() => GenerateConsolAndShipmentRCNWithDirection(null, null, DomesticPort, DomesticPort, null, null, DomesticPort, DomesticPort, "DOM");
		public void TestRCNDirectionRule_ConsolIsDomestic_ShipmentIsBlank() => GenerateConsolAndShipmentRCNWithDirection(null, null, DomesticPort, DomesticPort, null, null, null, null, "DOM");

		public void TestRCNDirectionRule_ConsolIsBlank_ShipmentIsImport() => GenerateConsolAndShipmentRCNWithDirection(null, null, null, null, ForeignPort, null, ForeignPort, DomesticPort, "");
		public void TestRCNDirectionRule_ConsolIsBlank_ShipmentIsExport() => GenerateConsolAndShipmentRCNWithDirection(null, null, null, null, null, ForeignPort, DomesticPort, ForeignPort, "");
		public void TestRCNDirectionRule_ConsolIsBlank_ShipmentIsDomestic() => GenerateConsolAndShipmentRCNWithDirection(null, null, null, null, null, null, DomesticPort, DomesticPort, "");
		public void TestRCNDirectionRule_ConsolIsBlank_ShipmentIsBlank() => GenerateConsolAndShipmentRCNWithDirection(null, null, null, null, null, null, null, null,"");

		public void GenerateConsolAndShipmentRCNWithDirection(ZString? consolInboundLeg = null, ZString? consolOutboundLeg = null, ZString? consolLoadingPort = null, ZString? consolDischargePort = null, ZString? shipmentInboundLeg = null, ZString? shipmentOutboundLeg = null, ZString? shipmentLoadingPort = null, ZString? shipmentDischargePort = null, ZString? rcnDirection = null)
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			Data.SetupForForwardingImport();
			var consol = Data.HeaderDataObject;
			TransportLeg consolInboundRoutingLeg = null;
			TransportLeg consolOutboundRoutingLeg = null;

			if (consolInboundLeg is not null)
			{
				consolInboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: consolInboundLeg, portOfDischarge: homePort);
			}

			if (consolOutboundLeg is not null)
			{
				consolOutboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, portOfDischarge: consolOutboundLeg);
			}

			var estimatedPickupDate = new ZDateTime(2006, 7, 7);
			Helper.AddTestDataToShipment(consol, inboundLeg: consolInboundRoutingLeg, outboundLeg: consolOutboundRoutingLeg, portOfLoading: consolLoadingPort, portOfDischarge: consolDischargePort, estimatedPickup: estimatedPickupDate);

			TransportLeg shipmentInboundRoutingLeg = null;
			TransportLeg shipmentOutboundRoutingLeg = null;

			if (shipmentInboundLeg is not null)
			{
				shipmentInboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: shipmentInboundLeg, portOfDischarge: homePort);
			}

			if (shipmentOutboundLeg is not null)
			{
				shipmentOutboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: homePort, portOfDischarge: shipmentOutboundLeg);
			}

			var shipment = consol.SubShipmentCollection[0];
			Helper.AddTestDataToShipment(shipment, inboundLeg: shipmentInboundRoutingLeg, outboundLeg: shipmentOutboundRoutingLeg, portOfLoading: shipmentLoadingPort, portOfDischarge: shipmentDischargePort, estimatedPickup: estimatedPickupDate);
			Factory.SaveForTesting();

			var rcn = GetDataObjectReader(Factory, consol).ReadIntoBusinessObject();
			AssertEquals(rcnDirection, rcn.WRC_Direction);
		}

		#endregion

		#region TestLogMessage_MatchedRCN

		public void TestLogMessage_MatchedRCNHasHigherPriorityUniversalLink()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.CreateShipmentWithPackages("HB1", "P1");
			Data.CreateReceiveConsignmentInDB(shipment);

			var header = SetupSeaCargoShipmentForImporting("HB1", DataContextType.Outturn, "ConsigneeCompany");

			var reader = GetDataObjectReader(Factory, header);
			reader.ReadIntoBusinessObject();

			var expectedErrorMessage =
@"Error - Cannot populate Receive Consignment RC00000001 because:
Job information is not overridden since it has been previously received from ForwardingShipment source.";

			AssertContains("Log message should contain error of import for higher priority import found", expectedErrorMessage, Logger.Logs);
		}

		public void TestLogMessage_MatchedRCNHasSamePriorityUniversalLinkFromDifferentSender()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.CreateShipmentWithPackages("HB1", "P1");
			Data.CreateReceiveConsignmentInDB(shipment);

			var anotherSender = CreateAnotherSender();

			using (Env.SetTemporaryUserContext(anotherSender.staff.PK.ToGuid(), anotherSender.branch.PK.ToGuid(), anotherSender.department.PK.ToGuid()))
			{
				var shipmentAfter = Data.CreateShipmentWithPackages("HB1", "P1", "P2");

				var header = SetupSeaCargoShipmentForImporting("HB1", DataContextType.Outturn, "ConsigneeCompany");

				var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipmentAfter, Logger, Factory);
				reader.ReadIntoBusinessObject();
			}

			var expectedErrorMessage =
@"Error - Cannot populate Receive Consignment RC00000001 because:
Job information is not overridden since it has been previously received from ForwardingShipment source by another sender.";

			AssertContains("Log message should contain error of import for the same priority import from another sender found", expectedErrorMessage, Logger.Logs);
		}

		#endregion

		#region TestAdditionalReferences_MatchedRCN

		public void TestAdditionalReferences_MatchedRCNHasHigherPriorityUniversalLink()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			var consignmentFromShipment = Data.CreateReceiveConsignmentInDB(shipment, false);
			var additionalReferencesFromShipment = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignmentFromShipment.PK));

			AssertEquals(2, additionalReferencesFromShipment.Length);
			Helper.AssertAdditionalReferences(additionalReferencesFromShipment, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, "C1000000", "ForwardingConsolNumber");
			Helper.AssertAdditionalReferences(additionalReferencesFromShipment, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "S1000000", "ForwardingShipmentNumber");

			var header = SetupSeaCargoShipmentForImporting("Waybill123", DataContextType.Outturn, "ConsigneeCompany", "AUBNE");
			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "ZAJNB";

			var reader = GetDataObjectReader(Factory, header);
			var consignmentFromShipmentAndCustoms = reader.ReadIntoBusinessObject();
			var additionalReferencesFromShipmentAndCustoms = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignmentFromShipmentAndCustoms.PK));

			AssertEquals(7, additionalReferencesFromShipmentAndCustoms.Length);
			Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAndCustoms, WarehouseAdditionalReferenceTypes.Codes.CutOffDate, new ZDateTime(2015, 4, 14).FormatDateTime(), "CutOffDate");
			Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAndCustoms, WarehouseAdditionalReferenceTypes.Codes.ETDDate, new ZDateTime(2015, 4, 15).FormatDateTime(), "ETDDate");
			Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAndCustoms, WarehouseAdditionalReferenceTypes.Codes.Vessel, "VES1", "Vessel");
			Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAndCustoms, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, "VF1", "VoyageFlightNumber");
			Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAndCustoms, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, "C1000000", "ForwardingConsolNumber");
			Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAndCustoms, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "S1000000", "ForwardingShipmentNumber");
		}

		public void TestAddtionalReferences_MatchedRCNHasSamePriorityUniversalLinkFromDifferentSender()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			var consignmentFromShipment = Data.CreateReceiveConsignmentInDB(shipment, false);
			var additionalReferencesFromShipment = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignmentFromShipment.PK));

			AssertEquals(2, additionalReferencesFromShipment.Length);
			Helper.AssertAdditionalReferences(additionalReferencesFromShipment, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "S1000000", "ForwardingShipmentNumebr");
			Helper.AssertAdditionalReferences(additionalReferencesFromShipment, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, "C1000000", "ForwardingConsolNumber");

			var anotherSender = CreateAnotherSender();

			using (Env.SetTemporaryUserContext(anotherSender.staff.PK.ToGuid(), anotherSender.branch.PK.ToGuid(), anotherSender.department.PK.ToGuid()))
			{
				var shipmentAfter = Data.CreateShipmentWithDestinationPort("Waybill123", "AUBNE", "P1");
				Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "ZAJNB";
				var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipmentAfter, Logger, Factory);
				var consignmentFromShipmentAfter = reader.ReadIntoBusinessObject();
				var additionalReferencesFromShipmentAfter = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignmentFromShipmentAfter.PK));

				AssertEquals(7, additionalReferencesFromShipmentAfter.Length);
				Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAfter, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "S1000000", "ForwardingShipmentNumebr");
				Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAfter, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, "C1000000", "ForwardingConsolNumber");
				Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAfter, WarehouseAdditionalReferenceTypes.Codes.Vessel, "VES1", "Vessel");
				Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAfter, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, "VF1", "VoyageFlightNumber");
				Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAfter, WarehouseAdditionalReferenceTypes.Codes.CutOffDate, new ZDateTime(2015, 4, 14).FormatDateTime(), "CutOffDate");
				Helper.AssertAdditionalReferences(additionalReferencesFromShipmentAfter, WarehouseAdditionalReferenceTypes.Codes.ETDDate, new ZDateTime(2015, 4, 15).FormatDateTime(), "ETDDate");
			}
		}

		#endregion

		#region JobServiceLinks

		public void TestImportWithNonCompletedJobServiceLink()
		{
			Data.SetupForForwardingImport();
			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 1 package on the package Job.", 1, packageJob.Packages.Count);

			var box = packageJob.Packages.First();
			var jobservice = Helper.CreateJobService(consignment.PK, consignment.TablePrefix);
			consignment.Services.Add(jobservice);
			Helper.CreateJobServiceLink(jobservice.PK, box, 10);
			Factory.SaveForTesting();

			AssertNoExceptionThrown(() => GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject());
			Factory.SaveForTesting();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newJobServiceLinkSingle = factory.LoadTop1<JobServiceLink>(new ZQuery());
			AssertNull(newJobServiceLinkSingle);
		}

		#endregion

		#region TestPopulatePackageStateSecurityStatus

		public void TestPopulatePackageStateSecurityStatus()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.PackingLineCollection.First().IsHighRisk = true;
			Data.Warehouse.WW_TransitSecurityProcessingRequired = true;
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });

			var reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
			var consignment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("Should have set the Intended Warehouse.", Data.Warehouse.PK, consignment.WRC_WW_IntendedWarehouse);

			var packageJob = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			var packages = packageJob.Packages;
			AssertEquals("There should be one package.", 1, packages.Count);

			var package = packages.First();
			var packageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, package.PK)).Single();
			packageState.Reload();
			AssertEquals("Package State Is High Risk must be set to true", true, packageState.WPS_IsHighRisk);
			AssertEquals("SecurityStatus should be HRS", TransitWarehouseSecurityStatuses.Codes.RequiresHighRiskScreening, packageState.WPS_SecurityStatus);
		}

		#endregion

		#region TestOrderReferences

		public void TestOrderReferencesReceiveConsignment()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.CreateShipmentWithPackages("CONID123");
			shipment.SetPackingLineCollection(() => null);
			var orderNumberCollection = new DataObjectList<OrderNumber>() { new OrderNumber() { OrderReference = "OrderRef1", Sequence = 1 }, new OrderNumber() { OrderReference = "OrderRef2", Sequence = 2 } };
			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing.SetOrderNumberCollection(() => orderNumberCollection);

			var receiveConsignment = new WhsTransitReceiveConsignmentDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created receive consignment.", receiveConsignment);
			Factory.SaveForTesting();

			receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).SingleOrDefault();
			AssertEquals("receive Consignment should have 2 Order References", 2, receiveConsignment.OrderReferences.Count);
			AssertContainsExactElementsInAnyOrder("order references should be applied to receive consignment",
				receiveConsignment.OrderReferences.Select(ord => ord.WOR_OrderReference), orderNumberCollection.Select(ord => ord.OrderReference));
		}

		#endregion

		#region TestShipperReferences

		public void TestShippersReferenceReceiveConsignment()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.CreateShipmentWithPackages("CONID123");
			shipment.BookingConfirmationReference = "ShippersReference";
			shipment.SetPackingLineCollection(() => null);

			var receiveConsignment = new WhsTransitReceiveConsignmentDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull("Precondition - created receive consignment.", receiveConsignment);
			AssertEquals("Precondition - created receive consignment.", receiveConsignment.WRC_ShippersReference, shipment.BookingConfirmationReference);
			Factory.SaveForTesting();
		}

		#endregion

		#region TestCannotPopulateAddresses

		public void TestCannotUpdateConsignorDocumentaryAddress() => TestCannotPopulateAddressesCore(docAddressType: nameof(DocAddressType.ConsignorDocumentaryAddress),
			errorMessage: "Cannot update the RCN Consignor Documentary Address. Some packages have already been unloaded.");

		public void TestCannotUpdateConsignorPickupDeliveryAddress() => TestCannotPopulateAddressesCore(docAddressType: nameof(DocAddressType.ConsignorPickupDeliveryAddress),
			errorMessage: "Cannot update the RCN Consignor Pickup Delivery Address. Some packages have already been unloaded.");

		void TestCannotPopulateAddressesCore(string docAddressType, string errorMessage)
		{
			Data.SetupForForwardingImport();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
			var consignment = reader.ReadIntoBusinessObject();
			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK));

			Data.ShipmentDataObject.OrganizationAddressCollection
				.Where(address => address.AddressType.GetValueOrDefault().ToString() == docAddressType).FirstOrDefault()
				.Address1 = "New Address";
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, Data.Warehouse.DefaultInboundDockDoorLocation.PK);
			var arrivedPackage = helper.CreatePackageState(consignment, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Data.ShipmentDataObject.PackingLineCollection.Content = CollectionContent.Partial;
			Factory.SaveForTesting();

			var newReader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
			AssertExceptionThrown<DataObjectReadFailureException>(errorMessage, () => newReader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestPopulateReferences_AssemblyMasterShipment

		public void TestPopulateReferences_AssemblyMasterShipment()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			var transitMapping = new TransitReferenceMappingConfiguration();
			using (WarehouseDataRegistry.Instance.TransitReferenceMapping.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, transitMapping))
			{
				Data.SetupForForwardingImport();
				var masterShipment = Data.CreateShipmentWithPackages("S1000000", "P1");
				masterShipment.IsBuyersConsol = true;
				var subShipment = Data.CreateShipmentWithPackages("S1000001", "P2");
				masterShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });
				Logger.TopLevelDataObject = masterShipment;
				masterShipment.ShipmentType = new CodeDescriptionPair() { Code = Constants.ShipmentTypes.BuyersConsolLead };
				masterShipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWX } } });
				var manager = new WhsTransitReceiveConsignmentDataContextManagerForTesting();
				var reader = manager.GetShipmentDataObjectReaderForTesting(masterShipment, Logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();
				var asmRefNumbers = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, reader.PK));
				var asmRefNumber = asmRefNumbers.FirstOrDefault(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment);
				Helper.AssertAdditionalReference(asmRefNumber, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment, "S1000000", countryCode: "", category: TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			}
		}

		#endregion

		(GlbStaff staff, GlbBranch branch, GlbDepartment department) CreateAnotherSender()
		{
			CreateOrgHeader($"EDIDATGC2", $"TestOrg2");

			var companyNew = Factory.NewWithValidTestData<GlbCompany>();
			companyNew.CompanyName = $"Company2";
			companyNew.GC_Code = $"GC2";

			var branchNew = Factory.NewWithValidTestData<GlbBranch>();
			branchNew.GB_BranchName = $"homeBranch2";
			branchNew.GB_Code = $"GB2";
			branchNew.GB_GC = companyNew.PK;

			var staffNew = Factory.NewWithValidTestData<GlbStaff>();
			staffNew.GS_FullName = $"Staff2";
			staffNew.GS_Code = $"GS2";
			staffNew.GS_GB_HomeBranch = branchNew.PK;
			staffNew.GS_IsOperational = false;

			var departmentNew = Factory.NewWithValidTestData<GlbDepartment>();
			departmentNew.GE_Code = $"DP2";

			Factory.SaveForTesting();

			return (staffNew, branchNew, departmentNew);
		}

		OrgHeader CreateOrgHeader(string code, string fullName)
		{
			var client = Helper.CreateClient(code, fullName);
			var address = client.Addresses.AddNew();
			address.OA_Address1 = "Address fullName";
			address.OA_City = "SYD";

			return client;
		}

		#region TestReadIntoBusinessObject_ContainsRadioactivePackages

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceCore(true);

		public void TestReadIntoBusinessObject_NotContainsNotAllowedUNDGSubstanceDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceCore(true, false);

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceDGManagementDisabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceCore(false);

		void TestReadIntoBusinessObject_ContainsNotAllowedUNDGSubstanceCore(bool isDGManagementEnabled, bool hasLimitedDG = true)
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var warehouse = Data.Warehouse;
			warehouse.WW_IsDangerousGoodsManagementEnabled = isDGManagementEnabled;

			var dgCode1 = hasLimitedDG ? "0004a" : "1009b";
			var dgCode2 = hasLimitedDG ? "0014a" : "1010b";

			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, dgCode1, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, dgCode2, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit2);

			Factory.SaveForTesting();

			var packLine = Helper.CreatePackingLine("", "", Constants.PkgUnit.Box);
			Func<List<UNDG>> funcListCollection = () =>
			{
				var result = new List<UNDG>();
				var packLineUndg1 = new UNDG();
				packLineUndg1.UNDGCode = "0004a";
				packLineUndg1.Standard = "IMO";
				result.Add(packLineUndg1);

				var packLineUndg2 = new UNDG();
				packLineUndg2.UNDGCode = "0014a";
				packLineUndg2.Standard = "IMO";
				result.Add(packLineUndg2);

				return result;
			};

			packLine.SetUNDGCollection(funcListCollection);
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(packLine);

			var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory);

			if (isDGManagementEnabled && hasLimitedDG)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException),
					"The packages on this Receive/Dispatch Instruction could not be created as the UNDG Substance threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Substance 0004a, 0014a goods.", () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("NoExceptionThrown", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassCore(true);

		public void TestReadIntoBusinessObject_NotContainsNotAllowedUNDGClassDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassCore(true, false);

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassDGManagementDisabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassCore(false);

		void TestReadIntoBusinessObject_ContainsNotAllowedUNDGClassCore(bool isDGManagementEnabled, bool hasLimitedDG = true)
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var warehouse = Data.Warehouse;
			warehouse.WW_IsDangerousGoodsManagementEnabled = isDGManagementEnabled;

			var dgClass1 = hasLimitedDG ? "1" : "2";
			var dgClass2 = hasLimitedDG ? "Comb" : "3";

			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, "", dgClass1, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, "", dgClass2, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit2);

			Factory.SaveForTesting();

			var packLine = Helper.CreatePackingLine("", "", Constants.PkgUnit.Box);
			Func<List<UNDG>> funcListCollection = () =>
			{
				var result = new List<UNDG>();
				var packLineUndg1 = new UNDG();
				packLineUndg1.UNDGCode = "0004a";
				packLineUndg1.Standard = "IMO";
				packLineUndg1.IMOClass = "1";
				result.Add(packLineUndg1);

				var packLineUndg2 = new UNDG();
				packLineUndg2.UNDGCode = "1993d";
				packLineUndg2.Standard = "CFR";
				packLineUndg2.IMOClass = "Comb";
				result.Add(packLineUndg2);

				return result;
			};

			packLine.SetUNDGCollection(funcListCollection);
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(packLine);

			var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory);

			if (isDGManagementEnabled && hasLimitedDG)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException),
					"The packages on this Receive/Dispatch Instruction could not be created as the UNDG Class threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Class 1, Comb goods.", () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("NoExceptionThrown", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceCore(true);

		public void TestReadIntoBusinessObject_NotContainsNotAllowedUNDGCountryReferenceDGManagementEnabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceCore(true, false);

		public void TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceDGManagementDisabled() => TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceCore(false);

		void TestReadIntoBusinessObject_ContainsNotAllowedUNDGCountryReferenceCore(bool isDGManagementEnabled, bool hasLimitedDG = true)
		{
			Data.SetupForForwardingImport();
			var shipmentDataObject = Data.ShipmentDataObject;

			var warehouse = Data.Warehouse;
			warehouse.WW_IsDangerousGoodsManagementEnabled = isDGManagementEnabled;

			var dgCode1 = hasLimitedDG ? "0004a" : "1009b";
			var dgCode2 = hasLimitedDG ? "0014a" : "1010b";

			var dcr1 = Helper.CreateCountryReference("1234");
			var dcr2 = Helper.CreateCountryReference("5678");

			var dcp1 = Helper.CreateCountryReferencePivot(dcr1, dgCode1);
			var dcp2 = Helper.CreateCountryReferencePivot(dcr2, dgCode2);

			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, "", "", dcr1, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, "", "", dcr2, totalWeightLimit: 0, totalVolumeLimit: 0);
			warehouse.UNDGLimits.Add(undgLimit2);

			Factory.SaveForTesting();

			var packLine = Helper.CreatePackingLine("", "", Constants.PkgUnit.Box);
			Func<List<UNDG>> funcListCollection = () =>
			{
				var result = new List<UNDG>();
				var packLineUndg1 = new UNDG();
				packLineUndg1.UNDGCode = "0004a";
				packLineUndg1.Standard = "IMO";
				result.Add(packLineUndg1);

				var packLineUndg2 = new UNDG();
				packLineUndg2.UNDGCode = "0014a";
				packLineUndg2.Standard = "IMO";
				result.Add(packLineUndg2);

				return result;
			};

			packLine.SetUNDGCollection(funcListCollection);
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(packLine);

			var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, Factory);

			if (isDGManagementEnabled && hasLimitedDG)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException),
					"The packages on this Receive/Dispatch Instruction could not be created as the UNDG Country Reference threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Country Reference 1234, 5678 goods.", () => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("NoExceptionThrown", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestUnderBondShipment_SendTWAndCreateJobLinkSuccessfully()
		{
			Data.SetupForForwardingImport();
			var header = SetupSeaCargoShipmentForImporting("RC00000003", DataContextType.UnderBond, "Consignee");
			header.ConsolidatedCargoStatus = new CodeDescriptionPair()
			{
				Code = "CLR",
				Description = "Customs Cleared"
			};

			var childShipment = SetupSeaCargoShipmentForImporting("", DataContextType.Outturn);
			header.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			header.SubShipmentCollection.Add(childShipment);

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var jobLink = Factory.LoadTop1<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, consignment.PK));
			AssertNotNull(jobLink);
			AssertEquals(nameof(DataContextType.UnderBond), jobLink.UCL_SourceType);
		}

		#endregion

		#region TestPopulateReferencesUsingMapper_ReferenceHelper_CollectTransitAdditionalReferenceInfoParametersMatching

		public void TestPopulateReferencesUsingMapper_ReferenceHelper_CollectTransitAdditionalReferenceInfo_ParametersMatching()
		{
			var branchPK = Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			var transitMapping = new TransitReferenceMappingConfiguration();

			var crnReferenceMapping = new TransitReferenceMapping()
			{
				SourceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference,
				SourceType = WarehouseAdditionalReferenceTypes.Codes.T1,
				TargetCategory = TransitWarehouseReferenceCategories.Codes.CustomsReference,
				TargetType = TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber,
				Direction = "EXP"
			};

			transitMapping.TransitReferenceMappingCollection.Add(crnReferenceMapping);

			using (WarehouseDataRegistry.Instance.TransitReferenceMapping.SetTemporaryValue(Guid.Empty, branchPK, Guid.Empty, transitMapping))
			{
				var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
				Data.SetupForForwardingImport();
				var legs = new DataObjectList<TransportLeg>();
				TransportLeg inboundRoutingLeg = null;
				TransportLeg outboundRoutingLeg = null;

				inboundRoutingLeg = Helper.CreateTransportLeg(portOfLoading: ForeignPort, portOfDischarge: homePort);
				legs.Add(inboundRoutingLeg);

				var estimatedPickupDate = new ZDateTime(2006, 7, 7);
				var shipment = Data.CreateShipmentWithPackages("SHIP123");
				shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
				shipment.AdditionalReferenceCollection.Add(new AdditionalReference()
				{
					Type = new EntryType
					{
						Code = WarehouseAdditionalReferenceTypes.Codes.T1
					},
					ReferenceNumber = "CRN123",
					CountryOfIssue = new Country
					{
						Code = "AU"
					}
				});
				Helper.AddTestDataToShipment(shipment, inboundLeg: inboundRoutingLeg, outboundLeg: outboundRoutingLeg, estimatedPickup: estimatedPickupDate);

				var rcn = GetDataObjectReader(Factory, shipment).ReadIntoBusinessObject();
				Factory.SaveForTesting();
				var cusEntryNumbers1 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, rcn.PK));
				var cusEntryNum = cusEntryNumbers1.FirstOrDefault(r => r.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.T1);
				Helper.AssertAdditionalReference(cusEntryNum, WarehouseAdditionalReferenceTypes.Codes.T1, "CRN123", countryCode: "AU", category: TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			}
		}

		#endregion

		#region TestPopulateBlindPackage

		public void TestPopulateBlindPackage()
		{
			Data.SetupForForwardingImport();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, Data.Warehouse.DefaultInboundDockDoorLocation.PK);
			var blindRCN = Helper.CreateReceiveConsignment("RCN1", Data.Warehouse.PK);
			var blindPacakgeState1 = Helper.CreatePackageState(blindRCN, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);
			var blindPacakgeState2 = Helper.CreatePackageState(blindRCN, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var shipmentDataObject = Data.ShipmentDataObject;

			var packline = Helper.CreatePackingLine("PKG-2", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(packline);

			Factory.SaveForTesting();

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction.",
				() => GetDataObjectReader(Factory, shipmentDataObject).ReadIntoBusinessObject());
		}

		public void TestPopulateBlindPackageWithOVP_InnerHasPackageID()
		{
			Data.SetupForForwardingImport();

			var blindRCN = Helper.CreateReceiveConsignment("RCN1", Data.Warehouse.PK);

			var row = Helper.CreateRowAndGenerateLocations(Data.Warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var ovp = Helper.CreateOverpackPackage("OVP1", blindRCN, rtu, status: TransitWarehouseStatuses.Codes.Arrived, rcn: blindRCN);
			var innerPackage = Helper.CreatePackageState(blindRCN, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackage, ZDateTimeOffset.Now, "ABC", ovp);

			var shipmentDataObject = Data.ShipmentDataObject;

			var ovpPackline = Helper.CreatePackingLine("OVP1", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var innerPackline = Helper.CreatePackingLine("Inner1", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			ovpPackline.SetPackingLineCollection(() => new List<PackingLine> { innerPackline });
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(ovpPackline);

			Factory.SaveForTesting();

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction.",
				() => GetDataObjectReader(Factory, shipmentDataObject).ReadIntoBusinessObject());
		}

		public void TestPopulateBlindPackageWithOVP_InnerIsPackline()
		{
			Data.SetupForForwardingImport();

			var blindRCN = Helper.CreateReceiveConsignment("RCN1", Data.Warehouse.PK);

			var row = Helper.CreateRowAndGenerateLocations(Data.Warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var ovp = Helper.CreateOverpackPackage("OVP1", blindRCN, rtu, status: TransitWarehouseStatuses.Codes.Arrived, rcn: blindRCN);
			var innerPackline = Helper.CreatePackageState(blindRCN, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackline, ZDateTimeOffset.Now, "ABC", ovp);

			var shipmentDataObject = Data.ShipmentDataObject;

			var ovpPackline = Helper.CreatePackingLine("OVP1", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var inner = Helper.CreatePackingLine("", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			ovpPackline.SetPackingLineCollection(() => new List<PackingLine> { inner });
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(ovpPackline);

			Factory.SaveForTesting();

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Unable to send a receive instruction for Shipment with blind packages. Users need to send either a prepare to dispatch or dispatch instruction.",
				() => GetDataObjectReader(Factory, shipmentDataObject).ReadIntoBusinessObject());
		}

		#endregion

		#region Implementation

		protected override void AdditionalSetupForImport(UniversalShipment consignmentDataObject)
		{
			consignmentDataObject.GoodsDescription = "shipment desc";
		}

		protected override WhsTransitReceiveConsignmentDataObjectReader GetDataObjectReader(UniversalObjectFactory factory, UniversalShipment shipmentDataObject)
		{
			return new WhsTransitReceiveConsignmentDataObjectReader(shipmentDataObject, Logger, factory);
		}

		protected override SchemaStringColumn ConsignmentIDColumn => WhsItemReceiveConsignmentSchema.WRC_ConsignmentID;
		protected override SchemaStringColumn HouseBillNumberColumn => WhsItemReceiveConsignmentSchema.WRC_HouseBillNumber;
		protected override SchemaDateTimeColumn CreateTimeColumn => WhsItemReceiveConsignmentSchema.WRC_SystemCreateTimeUtc;
		protected override SchemaStringColumn ServiceLevelColumn => WhsItemReceiveConsignmentSchema.WRC_RS_NKServiceLevel;
		protected override SchemaGuidColumn WarehouseColumn => WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse;
		protected override SchemaStringColumn DestinationColumn => WhsItemReceiveConsignmentSchema.WRC_RL_NKDestination;
		protected override SchemaGuidColumn PKColumn => WhsItemReceiveConsignmentSchema.PK;
		protected override SchemaStringColumn JobIDColumn => WhsItemReceiveConsignmentSchema.WRC_JobID;
		protected override SchemaDateTimeOffsetColumn CompleteTimeColumn => WhsItemReceiveConsignmentSchema.WRC_CompleteTime;
		protected override ZString JobIDPrefix => "RC";

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		TestHelperForUniversal UniversalHelper => universalHelper ?? (universalHelper = new TestHelperForUniversal(Factory.BOFactory));
		TestHelperForUniversal universalHelper;

		readonly ZDateTime bookedDate = new ZDateTime(2021, 12, 15, 0, 0, 0);

		string HomePort => Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
		string ForeignPort => "USLAX";
		string DomesticPort
		{
			get
			{
				return $"{HomePort.Substring(0, 2)}TST";
			}
		}

		#endregion
	}
}
