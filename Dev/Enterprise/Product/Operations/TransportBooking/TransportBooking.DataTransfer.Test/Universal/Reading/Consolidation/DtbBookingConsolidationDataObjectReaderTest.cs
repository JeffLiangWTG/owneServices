using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using ICusEntryNumber = Enterprise.Integration.Customs.ICusEntryNumber;
using UniversalDataBusEntryType = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryType;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	sealed class DtbBookingConsolidationDataObjectReaderTest : DtbBookingTestCaseWithFactory
	{
		public void TestDataContextType()
		{
			var reader = new DtbBookingConsolidationDataObjectReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, new UniversalObjectFactory());
			AssertEquals(DataContextType.TransportBookingConsolidation, reader.DataContextType);
		}

		public void TestGetNewBusinessObject()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var reader = new DtbBookingConsolidationDataObjectReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, new UniversalObjectFactory());
			var bookingRead = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingRead);
		}

		public void TestPopulateBusinessObject()
		{
			// consol
			// -shipment + booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			container.ContainerNumber = "CONT102938";
			container.ContainerType = new ContainerType() { Code = "20GP" };
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			shipment.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPU" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR
			shipment.GoodsDescription = "GOODS";

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals("GOODS", consolidationRead.KB_GoodsDescription);
			AssertEquals(1, consolidationRead.Bookings.Count);

			var booking = consolidationRead.Bookings[0];
			AssertEquals("EFPU", booking.KM_KT_NKBookingTemplate);
			AssertEquals(2, booking.Instructions.Count);
			AssertEquals(1, booking.AssignedPackages.Count);
			AssertEquals(20, booking.AssignedPackages[0].KP_PackageQty);
		}

		public void TestPopulateBusinessObject_JobDirection_PIC()
		{
			CoreTestPopulateBusinessObject_JobDirection(new TransportBookingDirection() { Code = "PIC" }, "PIC");
		}

		public void TestPopulateBusinessObject_JobDirection_ORG()
		{
			CoreTestPopulateBusinessObject_JobDirection(new TransportBookingDirection() { Code = "ORG" }, "PIC");
		}

		public void TestPopulateBusinessObject_JobDirection_EXP()
		{
			CoreTestPopulateBusinessObject_JobDirection(new TransportBookingDirection() { Code = "EXP" }, "PIC");
		}

		public void TestPopulateBusinessObject_JobDirection_DLV()
		{
			CoreTestPopulateBusinessObject_JobDirection(new TransportBookingDirection() { Code = "DLV" }, "DLV");
		}

		public void TestPopulateBusinessObject_JobDirection_DST()
		{
			CoreTestPopulateBusinessObject_JobDirection(new TransportBookingDirection() { Code = "DST" }, "DLV");
		}

		public void TestPopulateBusinessObject_JobDirection_IMP()
		{
			CoreTestPopulateBusinessObject_JobDirection(new TransportBookingDirection() { Code = "IMP" }, "DLV");
		}

		public void TestPopulateBusinessObject_JobDirection_LOC()
		{
			CoreTestPopulateBusinessObject_JobDirection(new TransportBookingDirection() { Code = "LOC" }, "LOC");
		}

		public void TestPopulateBusinessObject_JobDirection_EmptyCode()
		{
			CoreTestPopulateBusinessObject_JobDirection(new TransportBookingDirection() { Code = ZString.Empty }, ZString.Empty);
		}

		public void TestPopulateBusinessObject_JobDirection_NullCode()
		{
			CoreTestPopulateBusinessObject_JobDirection(new TransportBookingDirection() { Code = null }, ZString.Empty);
		}

		public void TestPopulateBusinessObject_JobDirection_Null()
		{
			CoreTestPopulateBusinessObject_JobDirection(null, ZString.Empty);
		}

		void CoreTestPopulateBusinessObject_JobDirection(TransportBookingDirection transportBookingDirection, ZString expectedKB_JobDirection)
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			container.ContainerNumber = "CONT102938";
			container.ContainerType = new ContainerType() { Code = "20GP" };
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			shipment.TransportBookingDirection = transportBookingDirection;
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPU" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR
			shipment.GoodsDescription = "GOODS";

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(expectedKB_JobDirection, consolidationRead.KB_JobDirection);
		}

		public void TestPopulateBusinessObject_NullTransportBookingDirectionOnMatchedDataObjectDoesNotUpdateJobDirection()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = "PIC";
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T123";
			Factory.Save();
			AssertEquals("Precondition: consolidation.KB_JobDirection should be PIC", "PIC", consolidation.KB_JobDirection);

			var consolidatedTransportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidatedTransportBooking.DataContext = DataContextFactory.New();
			consolidatedTransportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consolidatedTransportBooking.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			consolidatedTransportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "T123");

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "T123");
			transportBooking.ServiceLevel = new ServiceLevel { Code = "SVC" };
			transportBooking.TransportBookingDirection = null;
			transportBooking.GoodsDescription = "Updated Goods Description";
			consolidatedTransportBooking.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolidatedTransportBooking.SubShipmentCollection.Add(transportBooking);

			Logger.TopLevelDataObject = consolidatedTransportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(consolidatedTransportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Updated consolidation should still have PIC as JobDirection as null TransportBookingDirection in matched data object (at TB level) should not update DtbBookingConsolidation.KB_JobDirection", "PIC", consolidationRead.KB_JobDirection);
			AssertEquals("Also checking for updated goods description on consolidation (from GoodsDescription in matched data object - at TB level) to make sure updated successfully", "Updated Goods Description", consolidationRead.KB_GoodsDescription);
		}

		public void TestPopulateBusinessObject_MasterShipment()
		{
			// consol
			// -shipment + booking
			//  -subShipment1
			//  -subShipment2

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			container.ContainerNumber = "CONT102938";
			container.ContainerType = new ContainerType() { Code = "20GP" };
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			shipment.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPU" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			// ignore these sub shipments and assert results exactly as in TestPopulateBusinessObject()
			var subShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment1.ShipmentType = new CodeDescriptionPair() { Code = "XX1" };

			var subShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment2.ShipmentType = new CodeDescriptionPair() { Code = "XX2" };

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(subShipment1);
			shipment.SubShipmentCollection.Add(subShipment2);

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(1, consolidationRead.Bookings.Count);

			var booking = consolidationRead.Bookings[0];
			AssertEquals("EFPU", booking.KM_KT_NKBookingTemplate);
			AssertEquals(2, booking.Instructions.Count);
			AssertEquals(1, booking.AssignedPackages.Count);
			AssertEquals(20, booking.AssignedPackages[0].KP_PackageQty);
		}

		public void TestPopulateBusinessObject_Containerised()
		{
			// consol
			// -shipment + booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container);

			var containerButForShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(containerButForShipment);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			container.ContainerNumber = "CONT102938";
			container.ContainerType = new ContainerType() { Code = "20GP" };
			containerButForShipment.ContainerNumber = "CONT102938";
			containerButForShipment.ContainerType = new ContainerType() { Code = "20GP" };
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			shipment.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(1, consolidationRead.Bookings.Count);

			var booking = consolidationRead.Bookings[0];
			AssertEquals("EECR", booking.KM_KT_NKBookingTemplate);
			AssertEquals(2, booking.Instructions.Count);
			AssertEquals(1, booking.AssignedPackages.Count);
			AssertEquals("CONT102938", booking.AssignedPackages[0].KP_PackageID);
		}

		public void TestPopulateBusinessObject_ContainerDetailsOnTopLevelDOWithContainerLinks()
		{
			// consol
			// -shipment + booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipmentWithoutContainers = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithoutContainers.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipmentWithoutContainers);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container);

			var packlineForContainer = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentWithoutContainers.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipmentWithoutContainers.PackingLineCollection.Add(packlineForContainer);

			container.Link = 1;
			container.ContainerNumber = "CONT102938";
			container.ContainerType = new ContainerType() { Code = "20GP" };
			packlineForContainer.ContainerLink = container.Link;
			packlineForContainer.PackQty = 20;
			packlineForContainer.PackType = new PackageType() { Code = "PLT" };

			shipmentWithoutContainers.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithoutContainers.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(1, consolidationRead.Bookings.Count);

			var booking = consolidationRead.Bookings[0];
			AssertEquals("EECR", booking.KM_KT_NKBookingTemplate);
			AssertEquals("There should be two instructions.", 2, booking.Instructions.Count);
			AssertEquals("There should be one outer package.", 1, booking.AssignedPackages.Count);
			AssertEquals("CONT102938", booking.AssignedPackages[0].KP_PackageID);
			AssertEquals("Package with 20PLT should be inside the container.", 1, booking.AssignedPackages[0].Packages.Count);
			AssertEquals("Package quantity should be 20.", 20, booking.AssignedPackages[0].Packages[0].KP_PackageQty);
			AssertEquals("Package type should be PLT.", "PLT", booking.AssignedPackages[0].Packages[0].PackType.F3_Code);
		}

		public void TestPopulateBusinessObject_ContainerDetailsOnSourceDOWithContainerLinks()
		{
			// consol
			// -shipment + booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipmentWithContainers = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithContainers.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipmentWithContainers);

			var containerForConsol = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONS102938", ContainerType = new ContainerType() { Code = "40GP" } };
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(containerForConsol);

			var packlineForContainer = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentWithContainers.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipmentWithContainers.PackingLineCollection.Add(packlineForContainer);
			var containerForShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "SHP102938", ContainerType = new ContainerType() { Code = "20GP" } };
			shipmentWithContainers.SetContainerCollection(() => new DataObjectList<Container>());
			shipmentWithContainers.ContainerCollection.Add(containerForShipment);

			containerForShipment.Link = 1;
			packlineForContainer.ContainerLink = containerForShipment.Link;
			packlineForContainer.PackQty = 20;
			packlineForContainer.PackType = new PackageType() { Code = "PLT" };

			shipmentWithContainers.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithContainers.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(1, consolidationRead.Bookings.Count);

			var booking = consolidationRead.Bookings[0];
			AssertEquals("EECR", booking.KM_KT_NKBookingTemplate);
			AssertEquals("There should be two instructions.", 2, booking.Instructions.Count);
			AssertEquals("There should be one outer package.", 1, booking.AssignedPackages.Count);
			AssertEquals("SHP102938", booking.AssignedPackages[0].KP_PackageID);
			AssertEquals("Package with 20PLT should be inside the container.", 1, booking.AssignedPackages[0].Packages.Count);
			AssertEquals("Package quantity should be 20.", 20, booking.AssignedPackages[0].Packages[0].KP_PackageQty);
			AssertEquals("Package type should be PLT.", "PLT", booking.AssignedPackages[0].Packages[0].PackType.F3_Code);
		}

		public void TestPopulateBusinessObject_ContainerDetailsWithoutContainerLinks()
		{
			// consol
			// -shipment + booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipmentWithContainers = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithContainers.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipmentWithContainers);

			var containerForConsol = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONS102938" };
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(containerForConsol);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentWithContainers.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipmentWithContainers.PackingLineCollection.Add(packline);
			shipmentWithContainers.SetContainerCollection(() => new DataObjectList<Container>());
			var containerForShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "SHP102938" };
			containerForShipment.ContainerType = new ContainerType() { Code = "40GP" };
			shipmentWithContainers.ContainerCollection.Add(containerForShipment);

			containerForConsol.ContainerType = new ContainerType() { Code = "20GP" };
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			shipmentWithContainers.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithContainers.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(1, consolidationRead.Bookings.Count);

			var booking = consolidationRead.Bookings[0];
			AssertEquals("EECR", booking.KM_KT_NKBookingTemplate);
			AssertEquals("There should be two instructions.", 2, booking.Instructions.Count);
			AssertEquals("SHP102938", booking.AssignedPackages.Single().KP_PackageID);
		}

		public void TestPopulateBusinessObject_Containerised_PackQuantityOnContainerNotMatchingOuterPacksAndNoContainerNumber()
		{
			// consol
			// -shipment + booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container);

			var containerButForShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(containerButForShipment);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			container.ContainerNumber = ""; // Intentionally blank
			container.ContainerType = new ContainerType { Code = "20GP" };
			containerButForShipment.ContainerNumber = "";
			containerButForShipment.ContainerType = new ContainerType { Code = "20GP" };

			shipment.OuterPacks = 100; // Intentionally different to pack qty on the single pack line
			shipment.OuterPacksPackageType = new PackageType { Code = "PLT" };
			packline.PackQty = 20;
			packline.PackType = new PackageType { Code = "PLT" };

			shipment.TransportBookingDirection = new TransportBookingDirection { Code = "PIC" };
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "IFCD" }; // Import FCL/ULD, Unpack at CNE

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(1, consolidationRead.Bookings.Count);
			AssertEquals("Should have container and loose pallet", 2, consolidationRead.PackageJob.Packages.Count);

			var containerOnConsolidation = consolidationRead.PackageJob.Packages.Single(p => p.IsContainer);
			var loosePalletOnConsolidation = consolidationRead.PackageJob.Packages.Single(p => !p.IsContainer);
			var booking = consolidationRead.Bookings[0];
			AssertEquals("IFCD", booking.KM_KT_NKBookingTemplate);
			AssertEquals(3, booking.Instructions.Count);
			AssertContainsExactElementsInAnyOrder(new[] { containerOnConsolidation }, booking.AssignedPackages);
		}

		public void TestPopulateBusinessObject_UpdateTransportBooking_Shipment_NoKey()
		{
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("New booking must be created.", 1, consolidationRead.Bookings.Count);
		}

		public void TestPopulateBusinessObject_UpdateTransportBooking_SubShipment_NoKey()
		{
			var consolidation = Helper.CreateConsolidation();
			Factory.Save();

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, consolidation.KB_JobID);
			consol.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("New booking must be created.", 1, consolidationRead.Bookings.Count);
		}

		public void TestPopulateBusinessObject_UpdateTransportBooking_Shipment_WithKey_NotFound()
		{
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "T123");
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, new UniversalObjectFactory());
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Match couldn't be found for TransportBooking with Key T123", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBusinessObject_UpdateTransportBooking_SubShipment_WithKey_NotFound()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			shipment.DataContext.AddDataTarget(DataContextType.TransportBooking, "T123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			shipment.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "IFCD" };

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Match couldn't be found for TransportBooking with Key T123", () => reader.ReadIntoBusinessObject());
		}
		public void TestPopulateBusinessObject_UpdateTransportBooking_Shipment_WithKey_Found()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T123";
			Factory.Save();
			AssertEquals("Precondition", ZString.Empty, booking.KM_RS_NKServiceLevel);

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "T123");
			transportBooking.ServiceLevel = new ServiceLevel { Code = "SVC" };
			Logger.TopLevelDataObject = transportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Booking T123 must be found.", booking.PK, consolidationRead.Bookings.Single().PK);
			universalFactory.SaveForTesting();

			var dtbBooking = new BusinessObjectFactory().Load<DtbBooking>(new ZQuery()).Single();
			AssertEquals("Just to make sure it found the TB and update(it can be any field update by reader).", "SVC", dtbBooking.KM_RS_NKServiceLevel);
		}

		public void TestPopulateBusinessObject_From2ndDispatchConsignment()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var consolidation = Helper.CreateConsolidation();
			consolidation.PackageJob.Packages.Add(Helper.CreatePackage("P1", 1));

			var booking = Helper.CreateBooking(consolidation, "DLTW", null, null, null);
			booking.KM_JobID = "TB001";
			var originalInstruction = booking.Instructions.First(i => i.KN_Sequence == 1);
			var joblink = Factory.New<StmUniversalJobLink>();
			joblink.UCL_ParentID = booking.PK;
			joblink.UCL_ParentTableCode = booking.TablePrefix;
			joblink.UCL_SourceKey = "WDC01";
			joblink.UCL_SourceType = "TransitDispatch";
			joblink.UCL_EnterpriseCode = FormattableString.Invariant($"{registrationKey.EnterpriseCode}");
			joblink.UCL_ServerCode = FormattableString.Invariant($"{registrationKey.ServerCode}");
			joblink.UCL_CompanyCode = "EDI";

			Factory.Save();

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataSource(DataContextType.TransitDispatch, "WDC02");
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			transportBooking.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "DLTW" };
			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "PLT" },
				ReferenceNumber = "P2",
				Link = 1
			};
			transportBooking.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline });
			Logger.TopLevelDataObject = transportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Should create a new booking.", 2, consolidationRead.Bookings.Count);
			AssertEquals("Consolidation should have 2 packages", 2, consolidationRead.PackageJob.Packages.Count);

			var existingBooking = consolidationRead.Bookings.SingleOrDefault(b => b.JobLinks.Any(l => l.UCL_SourceKey == "WDC01"));
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, existingBooking.KM_Status);
			AssertEquals("Booking should have P1", "P1", existingBooking.AssignedPackages.SingleOrDefault().KP_PackageID);
			AssertEquals("Sibling Booking Instructions should not be deleted and repopulated", originalInstruction.PK, existingBooking.Instructions.First(i => i.KN_Sequence == 1).PK);

			var newBooking = consolidationRead.Bookings.SingleOrDefault(b => b.KM_JobID != "TB001");
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, newBooking.KM_Status);
			AssertEquals("Booking should have P2", "P2", newBooking.AssignedPackages.SingleOrDefault().KP_PackageID);
		}

		public void TestPopulateBusinessObject_FromSameDispatchConsignment()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var consolidation = Helper.CreateConsolidation();
			consolidation.PackageJob.Packages.Add(Helper.CreatePackage("P1", 1));

			var booking = Helper.CreateBooking(consolidation, "DLTW", null, null, null);
			var originalInstruction = booking.Instructions.First(i => i.KN_Sequence == 1);
			booking.KM_JobID = "TB001";
			var joblink = Factory.New<StmUniversalJobLink>();
			joblink.UCL_ParentID = booking.PK;
			joblink.UCL_ParentTableCode = booking.TablePrefix;
			joblink.UCL_SourceKey = "WDC01";
			joblink.UCL_SourceType = "TransitDispatch";
			joblink.UCL_EnterpriseCode = FormattableString.Invariant($"{registrationKey.EnterpriseCode}");
			joblink.UCL_ServerCode = FormattableString.Invariant($"{registrationKey.ServerCode}");
			joblink.UCL_CompanyCode = "EDI";

			Factory.Save();

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataSource(DataContextType.TransitDispatch, "WDC01");
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			transportBooking.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "DLTW" };
			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "PLT" },
				ReferenceNumber = "P1",
				Link = 1
			};
			transportBooking.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline });
			Logger.TopLevelDataObject = transportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Should create a new booking.", 1, consolidationRead.Bookings.Count);
			AssertEquals("Consolidation should have 1 packages", 1, consolidationRead.PackageJob.Packages.Count);

			var existingBooking = consolidationRead.Bookings.SingleOrDefault(b => b.JobLinks.Any(l => l.UCL_SourceKey == "WDC01"));
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, existingBooking.KM_Status);
			AssertEquals("Booking should have P1", "P1", existingBooking.AssignedPackages.SingleOrDefault().KP_PackageID);
			AssertNotEquals("Same Booking, so Instructions should be deleted and repopulated", originalInstruction.PK, existingBooking.Instructions.First(i => i.KN_Sequence == 1).PK);
		}

		public void TestPopulateBusinessObject_From2ndDispatchConsignment_ManyPackagesWithIDs()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var consolidation = Helper.CreateConsolidation();
			consolidation.PackageJob.Packages.Add(Helper.CreatePackage("P1", 1));
			consolidation.PackageJob.Packages.Add(Helper.CreatePackage("P2", 1));

			var booking = Helper.CreateBooking(consolidation, "DLTW", null, null, null);
			booking.KM_JobID = "TB001";
			var joblink = Factory.New<StmUniversalJobLink>();
			joblink.UCL_ParentID = booking.PK;
			joblink.UCL_ParentTableCode = booking.TablePrefix;
			joblink.UCL_SourceKey = "WDC01";
			joblink.UCL_SourceType = "TransitDispatch";
			joblink.UCL_EnterpriseCode = FormattableString.Invariant($"{registrationKey.EnterpriseCode}");
			joblink.UCL_ServerCode = FormattableString.Invariant($"{registrationKey.ServerCode}");
			joblink.UCL_CompanyCode = "EDI";

			Factory.Save();

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataSource(DataContextType.TransitDispatch, "WDC02");
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			transportBooking.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "DLTW" };
			var p3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "PLT" },
				ReferenceNumber = "P3",
				Link = 3
			};
			var p4 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "PLT" },
				ReferenceNumber = "P4",
				Link = 4
			};
			transportBooking.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { p3, p4 });
			Logger.TopLevelDataObject = transportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Should create a new booking.", 2, consolidationRead.Bookings.Count);
			AssertEquals("Consolidation should have 4 packages", 4, consolidationRead.PackageJob.Packages.Count);

			var existingBooking = consolidationRead.Bookings.SingleOrDefault(b => b.JobLinks.Any(l => l.UCL_SourceKey == "WDC01"));
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, existingBooking.KM_Status);
			AssertEquals("Booking should have 2 Packages", 2, existingBooking.AssignedPackages.Count);
			AssertNotNull("Booking should have P1", existingBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "P1"));
			AssertNotNull("Booking should have P2", existingBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "P2"));

			var newBooking = consolidationRead.Bookings.SingleOrDefault(b => b.KM_JobID != "TB001");
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, newBooking.KM_Status);
			AssertEquals("Booking should have 2 Packages", 2, newBooking.AssignedPackages.Count);
			AssertNotNull("Booking should have P3", newBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "P3"));
			AssertNotNull("Booking should have P4", newBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "P4"));
		}

		public void TestPopulateBusinessObject_From2ndDispatchConsignment_Packlines()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var consolidation = Helper.CreateConsolidation();
			var packline1 = Helper.CreatePackage("", 3);
			var packline2 = Helper.CreatePackage("", 4);
			packline1.KP_ExternalReference = "Packline1";
			packline2.KP_ExternalReference = "Packline2";
			consolidation.PackageJob.Packages.Add(packline1);
			consolidation.PackageJob.Packages.Add(packline2);

			var booking = Helper.CreateBooking(consolidation, "DLTW", null, null, null);
			booking.KM_JobID = "TB001";
			var joblink = Factory.New<StmUniversalJobLink>();
			joblink.UCL_ParentID = booking.PK;
			joblink.UCL_ParentTableCode = booking.TablePrefix;
			joblink.UCL_SourceKey = "WDC01";
			joblink.UCL_SourceType = "TransitDispatch";
			joblink.UCL_EnterpriseCode = FormattableString.Invariant($"{registrationKey.EnterpriseCode}");
			joblink.UCL_ServerCode = FormattableString.Invariant($"{registrationKey.ServerCode}");
			joblink.UCL_CompanyCode = "EDI";

			Factory.Save();

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataSource(DataContextType.TransitDispatch, "WDC02");
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			transportBooking.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "DLTW" };
			var packline3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 5,
				PackType = new PackageType() { Code = "CTN" },
				ReferenceNumber = "",
				PackingLineID = "Packline3",
				Link = 3
			};
			var packline4 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 6,
				PackType = new PackageType() { Code = "CTN" },
				ReferenceNumber = "",
				PackingLineID = "Packline4",
				Link = 4
			};
			transportBooking.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline3, packline4 });
			Logger.TopLevelDataObject = transportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Should create a new booking.", 2, consolidationRead.Bookings.Count);
			AssertEquals("Consolidation should have 4 packlines", 4, consolidationRead.PackageJob.Packages.Count);

			var existingBooking = consolidationRead.Bookings.SingleOrDefault(b => b.JobLinks.Any(l => l.UCL_SourceKey == "WDC01"));
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, existingBooking.KM_Status);
			AssertEquals("Booking should have 2 Packlines", 2, existingBooking.AssignedPackages.Count);
			AssertNotNull("Booking should have P1", existingBooking.AssignedPackages.SingleOrDefault(p => p.KP_ExternalReference == "Packline1"));
			AssertNotNull("Booking should have P2", existingBooking.AssignedPackages.SingleOrDefault(p => p.KP_ExternalReference == "Packline2"));

			var newBooking = consolidationRead.Bookings.SingleOrDefault(b => b.KM_JobID != "TB001");
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, newBooking.KM_Status);
			AssertEquals("Booking should have 2 Packlines", 2, newBooking.AssignedPackages.Count);
			var packline3OnNewBooking = newBooking.AssignedPackages.SingleOrDefault(p => p.KP_ExternalReference == "Packline3");
			AssertNotNull("Booking should have Packline3", packline3OnNewBooking);
			AssertEquals("Packline3's PackQty should be 5", 5, packline3OnNewBooking.KP_PackageQty);
			var packline4OnNewBooking = newBooking.AssignedPackages.SingleOrDefault(p => p.KP_ExternalReference == "Packline4");
			AssertNotNull("Booking should have Packline4", packline4OnNewBooking);
			AssertEquals("Packline4's PackQty should be 6", 6, packline4OnNewBooking.KP_PackageQty);
		}

		public void TestPopulateBusinessObject_From2ndDispatchConsignment_HUsWithPackages()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var consolidation = Helper.CreateConsolidation();
			var packline1 = Helper.CreatePackage("", 3);
			var packline2 = Helper.CreatePackage("", 4);
			packline1.KP_ExternalReference = "Packline1";
			packline2.KP_ExternalReference = "Packline2";
			consolidation.PackageJob.Packages.Add(packline1);
			consolidation.PackageJob.Packages.Add(packline2);

			var booking = Helper.CreateBooking(consolidation, "DLTW", null, null, null);
			booking.KM_JobID = "TB001";
			var joblink = Factory.New<StmUniversalJobLink>();
			joblink.UCL_ParentID = booking.PK;
			joblink.UCL_ParentTableCode = booking.TablePrefix;
			joblink.UCL_SourceKey = "WDC01";
			joblink.UCL_SourceType = "TransitDispatch";
			joblink.UCL_EnterpriseCode = FormattableString.Invariant($"{registrationKey.EnterpriseCode}");
			joblink.UCL_ServerCode = FormattableString.Invariant($"{registrationKey.ServerCode}");
			joblink.UCL_CompanyCode = "EDI";

			Factory.Save();

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataSource(DataContextType.TransitDispatch, "WDC02");
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			transportBooking.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "DLTW" };
			var p3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "CTN" },
				ReferenceNumber = "P3",
				PackingLineID = "",
				Link = 3,
				ParentPackingLineLink = 3
			};
			var p4 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "CTN" },
				ReferenceNumber = "P4",
				PackingLineID = "",
				Link = 4,
				ParentPackingLineLink = 44
			};
			transportBooking.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { p3, p4 });

			var hu3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "PLT" },
				ReferenceNumber = "HU3",
				PackingLineID = "Packline3",
				Link = 3
			};
			var hu44 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "PLT" },
				ReferenceNumber = "HU44",
				PackingLineID = "",
				Link = 44
			};
			transportBooking.SetParentPackingLineCollection(() => new DataObjectList<PackingLine>() { hu3, hu44 });

			Logger.TopLevelDataObject = transportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Should create a new booking.", 2, consolidationRead.Bookings.Count);
			AssertEquals("Consolidation should have 4 packlines", 4, consolidationRead.PackageJob.Packages.Count);

			var existingBooking = consolidationRead.Bookings.SingleOrDefault(b => b.JobLinks.Any(l => l.UCL_SourceKey == "WDC01"));
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, existingBooking.KM_Status);
			AssertEquals("Booking should have 2 Packlines", 2, existingBooking.AssignedPackages.Count);
			AssertNotNull("Booking should have P1", existingBooking.AssignedPackages.SingleOrDefault(p => p.KP_ExternalReference == "Packline1"));
			AssertNotNull("Booking should have P2", existingBooking.AssignedPackages.SingleOrDefault(p => p.KP_ExternalReference == "Packline2"));

			var newBooking = consolidationRead.Bookings.SingleOrDefault(b => b.KM_JobID != "TB001");
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, newBooking.KM_Status);
			AssertEquals("Booking should have 2 HUs", 2, newBooking.AssignedPackages.Count);
			AssertNotNull("Booking should have HU3", newBooking.AssignedPackages.Single(p => p.KP_PackageID == "HU3"));
			AssertNotNull("Booking should have HU44", newBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "HU44"));
			AssertNotNull("Booking should have HU3", newBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "HU3").Packages.SingleOrDefault(p => p.KP_PackageID == "P3"));
			AssertNotNull("Booking should have HU44", newBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "HU44").Packages.SingleOrDefault(p => p.KP_PackageID == "P4"));
		}

		public void TestPopulateBusinessObject_From2ndDispatchConsignment_HUsWithPacklines()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var consolidation = Helper.CreateConsolidation();
			var packline1 = Helper.CreatePackage("", 3);
			var packline2 = Helper.CreatePackage("", 4);
			packline1.KP_ExternalReference = "Packline1";
			packline2.KP_ExternalReference = "Packline2";
			consolidation.PackageJob.Packages.Add(packline1);
			consolidation.PackageJob.Packages.Add(packline2);

			var booking = Helper.CreateBooking(consolidation, "DLTW", null, null, null);
			booking.KM_JobID = "TB001";
			var joblink = Factory.New<StmUniversalJobLink>();
			joblink.UCL_ParentID = booking.PK;
			joblink.UCL_ParentTableCode = booking.TablePrefix;
			joblink.UCL_SourceKey = "WDC01";
			joblink.UCL_SourceType = "TransitDispatch";
			joblink.UCL_EnterpriseCode = FormattableString.Invariant($"{registrationKey.EnterpriseCode}");
			joblink.UCL_ServerCode = FormattableString.Invariant($"{registrationKey.ServerCode}");
			joblink.UCL_CompanyCode = "EDI";

			Factory.Save();

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataSource(DataContextType.TransitDispatch, "WDC02");
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			transportBooking.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "DLTW" };
			var packline3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 5,
				PackType = new PackageType() { Code = "CTN" },
				ReferenceNumber = "",
				PackingLineID = "Packline3",
				Link = 3,
				ParentPackingLineLink = 3
			};
			var packline4 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 6,
				PackType = new PackageType() { Code = "CTN" },
				ReferenceNumber = "",
				PackingLineID = "Packline4",
				Link = 4,
				ParentPackingLineLink = 44
			};
			transportBooking.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline3, packline4 });

			var hu3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "PLT" },
				ReferenceNumber = "HU3",
				PackingLineID = "Packline3",
				Link = 3
			};
			var hu44 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType() { Code = "PLT" },
				ReferenceNumber = "HU44",
				PackingLineID = "",
				Link = 44
			};
			transportBooking.SetParentPackingLineCollection(() => new DataObjectList<PackingLine>() { hu3, hu44 });

			Logger.TopLevelDataObject = transportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Should create a new booking.", 2, consolidationRead.Bookings.Count);
			AssertEquals("Consolidation should have 4 packlines", 4, consolidationRead.PackageJob.Packages.Count);

			var existingBooking = consolidationRead.Bookings.SingleOrDefault(b => b.JobLinks.Any(l => l.UCL_SourceKey == "WDC01"));
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, existingBooking.KM_Status);
			AssertEquals("Booking should have 2 Packlines", 2, existingBooking.AssignedPackages.Count);
			AssertNotNull("Booking should have P1", existingBooking.AssignedPackages.SingleOrDefault(p => p.KP_ExternalReference == "Packline1"));
			AssertNotNull("Booking should have P2", existingBooking.AssignedPackages.SingleOrDefault(p => p.KP_ExternalReference == "Packline2"));

			var newBooking = consolidationRead.Bookings.SingleOrDefault(b => b.KM_JobID != "TB001");
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, newBooking.KM_Status);
			AssertEquals("Booking should have 2 HUs", 2, newBooking.AssignedPackages.Count);
			AssertNotNull("Booking should have HU3", newBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "HU3"));
			AssertNotNull("Booking should have HU44", newBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "HU44"));
			AssertNotNull("Booking should have HU3", newBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "HU3").Packages.SingleOrDefault(p => p.KP_ExternalReference == "Packline3"));
			AssertNotNull("Booking should have HU44", newBooking.AssignedPackages.SingleOrDefault(p => p.KP_PackageID == "HU44").Packages.SingleOrDefault(p => p.KP_ExternalReference == "Packline4"));
		}

		public void TestPopulateBusinessObject_UpdatePacklinesInHandleUnit_MatchByPackingLineID()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var consolidation = Helper.CreateConsolidation();
			var package = Helper.CreatePackage("", 10);
			package.KP_ExternalReference = "Packline01";
			consolidation.PackageJob.Packages.Add(package);

			var booking = Helper.CreateBooking(consolidation, "DLTW", null, null, null);
			var originalInstruction = booking.Instructions.First(i => i.KN_Sequence == 1);
			booking.KM_JobID = "TB001";
			var joblink = Factory.New<StmUniversalJobLink>();
			joblink.UCL_ParentID = booking.PK;
			joblink.UCL_ParentTableCode = booking.TablePrefix;
			joblink.UCL_SourceKey = "WDC01";
			joblink.UCL_SourceType = "TransitDispatch";
			joblink.UCL_EnterpriseCode = FormattableString.Invariant($"{registrationKey.EnterpriseCode}");
			joblink.UCL_ServerCode = FormattableString.Invariant($"{registrationKey.ServerCode}");
			joblink.UCL_CompanyCode = "EDI";

			Factory.Save();

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			transportBooking.DataContext.AddDataSource(DataContextType.TransitDispatch, "WDC01");
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			transportBooking.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "DLTW" };
			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 5,
				PackingLineID = "Packline01"
			};
			transportBooking.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline });
			Logger.TopLevelDataObject = transportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(transportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Should create a new booking.", 1, consolidationRead.Bookings.Count);
			AssertEquals("Consolidation should only have one package", 1, consolidationRead.PackageJob.Packages.Count);

			var existingBooking = consolidationRead.Bookings.SingleOrDefault(b => b.JobLinks.Any(l => l.UCL_SourceKey == "WDC01"));
			AssertEquals("Booking should be available", TransportStatuses.Codes.Available, existingBooking.KM_Status);
			AssertEquals("Booking should only have one package", 1, existingBooking.AssignedPackages.Count);
			AssertEquals("The only one package's qty should be 5", 5, existingBooking.AssignedPackages.First().KP_PackageQty);
		}

		public void TestPopulateBusinessObject_UpdateTransportBooking_SubShipment_WithKey_NotMatch()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "BLA";
			Factory.Save();

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, consolidation.KB_JobID);
			consol.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			shipment.DataContext.AddDataTarget(DataContextType.TransportBooking, "T123");
			shipment.ServiceLevel = new ServiceLevel { Code = "SVC" };
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			shipment.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "IFCD" };

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Match couldn't be found for TransportBooking with Key T123", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBusinessObject_UpdateTransportBooking_ConsolidationAndTB_WithKey_Found()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "T123";
			Factory.Save();
			AssertEquals("Precondition", ZString.Empty, booking.KM_RS_NKServiceLevel);

			var consolidatedTransportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidatedTransportBooking.DataContext = DataContextFactory.New();
			consolidatedTransportBooking.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consolidatedTransportBooking.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			consolidatedTransportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "T123");

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBooking.DataContext = DataContextFactory.New();
			transportBooking.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "T123");
			transportBooking.ServiceLevel = new ServiceLevel { Code = "SVC" };
			consolidatedTransportBooking.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolidatedTransportBooking.SubShipmentCollection.Add(transportBooking);

			Logger.TopLevelDataObject = consolidatedTransportBooking;

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(consolidatedTransportBooking, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals("Booking T123 must be found.", booking.PK, consolidationRead.Bookings.Single().PK);
			universalFactory.SaveForTesting();

			var dtbBooking = new BusinessObjectFactory().Load<DtbBooking>(new ZQuery()).Single();
			AssertEquals("Just to make sure it found the TB and update(it can be any field update by reader).", "SVC", dtbBooking.KM_RS_NKServiceLevel);
		}

		public void TestPopulateBusinessObject_SplitContainerised()
		{
			// consol
			// -shipment + booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);
			shipment.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			var container1forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ContainerCollection.Add(container1forShipment);
			shipment.ContainerCollection.Add(container2forShipment);

			container1.ContainerNumber = "CONT102938";
			container1.ContainerType = new ContainerType() { Code = "20GP" };
			container2.ContainerNumber = "CONT203744";
			container2.ContainerType = new ContainerType() { Code = "20GP" };
			container1forShipment.ContainerNumber = "CONT102938";
			container1forShipment.ContainerType = new ContainerType() { Code = "20GP" };
			container2forShipment.ContainerNumber = "CONT203744";
			container2forShipment.ContainerType = new ContainerType() { Code = "20GP" };
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			var booking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			booking.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			booking.SetContainerCollection(() => new DataObjectList<Container>());
			booking.ContainerCollection.Add(container2);
			booking.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(booking);

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(1, consolidationRead.Bookings.Count);

			var bookingRead = consolidationRead.Bookings[0];
			AssertEquals("EECR", bookingRead.KM_KT_NKBookingTemplate);
			AssertEquals(2, bookingRead.Instructions.Count);
			AssertEquals(1, bookingRead.AssignedPackages.Count);
			AssertEquals("CONT203744", bookingRead.AssignedPackages[0].KP_PackageID);
		}

		public void TestPopulateBusinessObject_MixedOverriteWithMulti()
		{
			// consol
			// -shipment + booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);

			var packline10PLT = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			var packline20PLT = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline10PLT);
			shipment.PackingLineCollection.Add(packline20PLT);
			shipment.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			var container1forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ContainerCollection.Add(container1forShipment);
			shipment.ContainerCollection.Add(container2forShipment);

			var ref20GP = Helper.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			container1.ContainerType = ContainerType.New(ref20GP);
			container2.ContainerType = ContainerType.New(ref20GP);
			container1.ContainerNumber = "CONT102938";
			container2.ContainerNumber = "CONT203744";
			container1forShipment.ContainerType = ContainerType.New(ref20GP);
			container2forShipment.ContainerType = ContainerType.New(ref20GP);
			container1forShipment.ContainerNumber = "CONT102938";
			container2forShipment.ContainerNumber = "CONT203744";
			container1forShipment.Link = 1;
			container2forShipment.Link = 2;
			packline10PLT.PackQty = 10;
			packline10PLT.PackType = new PackageType() { Code = "PLT" };
			packline20PLT.PackQty = 20;
			packline20PLT.PackType = new PackageType() { Code = "PLT" };
			packline10PLT.ContainerLink = container1forShipment.Link; // shipment link not consol
			packline20PLT.ContainerLink = container2forShipment.Link;

			// 2 bookings w/ 1 container each

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var bookingContainer1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer1.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer1.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer1.ContainerCollection.Add(container1);
			bookingContainer1.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPL" }; // c:cyd-cfs-cto, l:cnr-cfs
			shipment.SubShipmentCollection.Add(bookingContainer1);

			var bookingContainer2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer2.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer2.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer2.ContainerCollection.Add(container2);
			bookingContainer2.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPL" }; // c:cyd-cfs-cto, l:cnr-cfs
			shipment.SubShipmentCollection.Add(bookingContainer2);

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(2, consolidationRead.Bookings.Count);

			var booking1Read = consolidationRead.Bookings[0];
			AssertEquals("EFPL", booking1Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking1Read.Instructions.Count);
			AssertEquals("1 cnt, 1 lse", 2, booking1Read.AssignedPackages.Count);

			var booking2Read = consolidationRead.Bookings[1];
			AssertEquals("EFPL", booking2Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking2Read.Instructions.Count);
			AssertEquals("1 cnt, 1 lse", 2, booking2Read.AssignedPackages.Count);

			// import multi booking 

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var bookingWithBothContainers = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingWithBothContainers.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingWithBothContainers.SetContainerCollection(() => new DataObjectList<Container>());
			bookingWithBothContainers.ContainerCollection.Add(container1);
			bookingWithBothContainers.ContainerCollection.Add(container2);
			bookingWithBothContainers.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPL" }; // c:cyd-cfs-cto, l:cnr-cfs
			shipment.SubShipmentCollection.Add(bookingWithBothContainers);

			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidationRead;
			topLevelReader.ReadIntoBusinessObject(ref con);

			universalFactory.SaveForTesting();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals("Should have reused tb 1 - ie added the 2nd container", 2, consolidationRead.Bookings.Count);

			booking1Read = consolidationRead.Bookings[0];
			AssertEquals("EFPL", booking1Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking1Read.Instructions.Count);
			AssertEquals("2 cnt, 2 lse", 4, booking1Read.AssignedPackages.Count);

			booking2Read = consolidationRead.Bookings[1];
			AssertEquals("EFPL", booking2Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking2Read.Instructions.Count);
			AssertEquals("1 cnt, 1 lse", 2, booking2Read.AssignedPackages.Count);

			// re-import 1 & 2 singles

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(bookingContainer1);
			shipment.SubShipmentCollection.Add(bookingContainer2);

			topLevelReader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			topLevelReader.ReadIntoBusinessObject(ref con);

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(3, consolidationRead.Bookings.Count);

			booking1Read = consolidationRead.Bookings[0];
			AssertEquals("EFPL", booking1Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking1Read.Instructions.Count);
			AssertEquals("2 cnt, 2 lse", 4, booking1Read.AssignedPackages.Count);

			booking2Read = consolidationRead.Bookings[1];
			AssertEquals("EFPL", booking2Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking2Read.Instructions.Count);
			AssertEquals("1 cnt, 1 lse", 2, booking2Read.AssignedPackages.Count);

			var booking3Read = consolidationRead.Bookings[2];
			AssertEquals("EFPL", booking3Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking3Read.Instructions.Count);
			AssertEquals("1 cnt, 1 lse", 2, booking3Read.AssignedPackages.Count);
		}

		public void TestPopulateBusinessObject_MixedOverrite()
		{
			// consol
			// -shipment + booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);

			var packline10PLT = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			var packline20PLT = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline10PLT);
			shipment.PackingLineCollection.Add(packline20PLT);
			shipment.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			var container1forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ContainerCollection.Add(container1forShipment);
			shipment.ContainerCollection.Add(container2forShipment);

			var ref20GP = Helper.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			container1.ContainerType = ContainerType.New(ref20GP);
			container2.ContainerType = ContainerType.New(ref20GP);
			container1.ContainerNumber = "CONT102938";
			container2.ContainerNumber = "CONT203744";
			container1forShipment.ContainerType = ContainerType.New(ref20GP);
			container2forShipment.ContainerType = ContainerType.New(ref20GP);
			container1forShipment.ContainerNumber = "CONT102938";
			container2forShipment.ContainerNumber = "CONT203744";
			container1forShipment.Link = 1;
			container2forShipment.Link = 2;
			packline10PLT.PackQty = 10;
			packline10PLT.PackType = new PackageType() { Code = "PLT" };
			packline20PLT.PackQty = 20;
			packline20PLT.PackType = new PackageType() { Code = "PLT" };
			packline10PLT.ContainerLink = container1forShipment.Link; // shipment link not consol
			packline20PLT.ContainerLink = container2forShipment.Link;

			// 2 bookings w/ 1 container each

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var bookingContainer1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer1.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer1.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer1.ContainerCollection.Add(container1);
			bookingContainer1.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPL" }; // c:cyd-cfs-cto, l:cnr-cfs
			shipment.SubShipmentCollection.Add(bookingContainer1);

			var bookingContainer2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer2.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer2.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer2.ContainerCollection.Add(container2);
			bookingContainer2.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPL" }; // c:cyd-cfs-cto, l:cnr-cfs
			shipment.SubShipmentCollection.Add(bookingContainer2);

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(2, consolidationRead.Bookings.Count);

			var booking1Read = consolidationRead.Bookings[0];
			AssertEquals("EFPL", booking1Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking1Read.Instructions.Count);
			AssertEquals("1 cnt, 1 lse", 2, booking1Read.AssignedPackages.Count);

			var booking2Read = consolidationRead.Bookings[1];
			AssertEquals("EFPL", booking2Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking2Read.Instructions.Count);
			AssertEquals("1 cnt, 1 lse", 2, booking2Read.AssignedPackages.Count);

			// re-import
			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidationRead;
			topLevelReader.ReadIntoBusinessObject(ref con);

			AssertEquals(false, consolidationRead.IsMultiBooking);
			AssertEquals(false, consolidationRead.KB_IsOverridden);
			AssertEquals("PIC", consolidationRead.KB_JobDirection);
			AssertEquals(2, consolidationRead.Bookings.Count);

			booking1Read = consolidationRead.Bookings[0];
			AssertEquals("EFPL", booking1Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking1Read.Instructions.Count);
			AssertEquals("1 cnt, 1 lse", 2, booking1Read.AssignedPackages.Count);

			booking2Read = consolidationRead.Bookings[1];
			AssertEquals("EFPL", booking2Read.KM_KT_NKBookingTemplate);
			AssertEquals(4, booking2Read.Instructions.Count);
			AssertEquals("1 cnt, 1 lse", 2, booking2Read.AssignedPackages.Count);

			// set one inactive
			booking1Read.KM_IsActive = false;
			Factory.Save();

			topLevelReader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			con = consolidationRead;
			topLevelReader.ReadIntoBusinessObject(ref con);
			AssertEquals(3, consolidationRead.Bookings.Count);
			AssertEquals(2, consolidationRead.ActiveBookings.Count);
		}

		public void TestPopulateBusinessObject_ContainersWithNoContainerIDs()
		{
			// consol
			//   container 20GP (blank)
			//   container 20GP (blank)
			//   container 40GP (blank)
			//   container 40GP x2 (blank)
			//   container 20GP CNT123
			//   + shipment & packlines all packed into each container

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var ref20GP = Helper.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var ref40GP = Helper.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));

			var cont1With20GP = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP) };
			var cont2With20GP = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP) };
			var contWith40GP = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref40GP) };
			var contCount2With40Gp = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref40GP), ContainerCount = 2 };
			var contWithContainerNumberAnd20GP = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP), ContainerNumber = "CNT123" };
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(cont1With20GP);
			consol.ContainerCollection.Add(cont2With20GP);
			consol.ContainerCollection.Add(contWith40GP);
			consol.ContainerCollection.Add(contCount2With40Gp);
			consol.ContainerCollection.Add(contWithContainerNumberAnd20GP);

			shipment.TransportBookingDirection = new TransportBookingDirection() { Code = "DST" };
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			var container1forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP) };
			var container2forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP) };
			var container3forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref40GP) };
			var container4forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref40GP), ContainerCount = 2 };
			var container5forShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP), ContainerNumber = "CNT123" };
			shipment.ContainerCollection.Add(container1forShipment);
			shipment.ContainerCollection.Add(container2forShipment);
			shipment.ContainerCollection.Add(container3forShipment);
			shipment.ContainerCollection.Add(container4forShipment);
			shipment.ContainerCollection.Add(container5forShipment);

			// 5 bookings w/ 1 container record each

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var bookingContainer1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer1.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer1.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer1.ContainerCollection.Add(cont1With20GP);
			bookingContainer1.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" };
			shipment.SubShipmentCollection.Add(bookingContainer1);

			var bookingContainer2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer2.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer2.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer2.ContainerCollection.Add(cont2With20GP);
			bookingContainer2.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" };
			shipment.SubShipmentCollection.Add(bookingContainer2);

			var bookingContainer3 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer3.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer3.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer3.ContainerCollection.Add(contWith40GP);
			bookingContainer3.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" };
			shipment.SubShipmentCollection.Add(bookingContainer3);

			var bookingContainer4 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer4.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer4.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer4.ContainerCollection.Add(contCount2With40Gp);
			bookingContainer4.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" };
			shipment.SubShipmentCollection.Add(bookingContainer4);

			var bookingContainer5 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer5.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer5.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer5.ContainerCollection.Add(contWithContainerNumberAnd20GP);
			bookingContainer5.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" };
			shipment.SubShipmentCollection.Add(bookingContainer5);

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var consolidation = reader.ReadIntoBusinessObject();

			AssertEquals(false, consolidation.IsMultiBooking);
			AssertEquals(false, consolidation.KB_IsOverridden);
			AssertEquals("DLV", consolidation.KB_JobDirection);
			AssertEquals("It should create 5 transport bookings.", 5, consolidation.Bookings.Count);

			var orderedBookings = consolidation.Bookings.OrderBy(b => b.KM_JobID).ToArray();
			AssertEquals("1 cnt", 1, orderedBookings[0].AssignedPackages.Single().KP_PackageQty);
			AssertEquals("1 cnt", 1, orderedBookings[1].AssignedPackages.Single().KP_PackageQty);
			AssertEquals("1 cnt", 1, orderedBookings[2].AssignedPackages.Single().KP_PackageQty);
			AssertEquals("2 cnt", 2, orderedBookings[3].AssignedPackages.Single().KP_PackageQty);
			AssertEquals("1 cnt", 1, orderedBookings[4].AssignedPackages.Single().KP_PackageQty);

			// re-import containerised booking information
			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidation;
			topLevelReader.ReadIntoBusinessObject(ref con);
			orderedBookings = consolidation.Bookings.OrderBy(b => b.KM_JobID).ToArray();
			AssertEquals(false, consolidation.IsMultiBooking);
			AssertEquals(false, consolidation.KB_IsOverridden);
			AssertEquals("DLV", consolidation.KB_JobDirection);
			AssertEquals(5, consolidation.Bookings.Count);
			AssertEquals("1 cnt", 1, orderedBookings[0].AssignedPackages.Single().KP_PackageQty);
			AssertEquals("1 cnt", 1, orderedBookings[1].AssignedPackages.Single().KP_PackageQty);
			AssertEquals("1 cnt", 1, orderedBookings[2].AssignedPackages.Single().KP_PackageQty);
			AssertEquals("2 cnt", 2, orderedBookings[3].AssignedPackages.Single().KP_PackageQty);
			AssertEquals("1 cnt", 1, orderedBookings[4].AssignedPackages.Single().KP_PackageQty);
		}

		public void TestPopulateBusinessObject_ContainersWithNoContainerIDs_SameInfo_WithPacking_WithLink()
		{
			TestPopulateBusinessObject_ContainersWithNoContainerIDs_SameInfo_WithPacking_WithLinkCore(hasLink: true);
		}

		public void TestPopulateBusinessObject_ContainersWithNoContainerIDs_SameInfo_WithPacking_WithoutLink()
		{
			TestPopulateBusinessObject_ContainersWithNoContainerIDs_SameInfo_WithPacking_WithLinkCore(hasLink: false);
		}

		void TestPopulateBusinessObject_ContainersWithNoContainerIDs_SameInfo_WithPacking_WithLinkCore(bool hasLink)
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var ref20GP = Helper.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP) };
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP) };
			var container3 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP) };
			var container4 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP) };
			var container5 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP) };
			if (hasLink)
			{
				container1.Link = 1;
				container2.Link = 2;
				container3.Link = 3;
				container4.Link = 4;
				container5.Link = 6; // no in sequence 
			}

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);
			consol.ContainerCollection.Add(container3);
			consol.ContainerCollection.Add(container4);
			consol.ContainerCollection.Add(container5);

			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(container1);
			shipment.ContainerCollection.Add(container2);
			shipment.ContainerCollection.Add(container3);
			shipment.ContainerCollection.Add(container4);
			shipment.ContainerCollection.Add(container5);

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 30, PackType = new PackageType() { Code = "PLT" }, ContainerLink = 1 });
			shipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 40, PackType = new PackageType() { Code = "PLT" }, ContainerLink = 2 });
			shipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 50, PackType = new PackageType() { Code = "PLT" }, ContainerLink = 3 });
			shipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 60, PackType = new PackageType() { Code = "PLT" }, ContainerLink = 4 });
			shipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 70, PackType = new PackageType() { Code = "PLT" }, ContainerLink = 6 });

			// 2 bookings w/ 2 and 3 container record each

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var bookingContainer1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer1.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer1.TransportBookingDirection = new TransportBookingDirection() { Code = "DST" };
			bookingContainer1.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" };
			bookingContainer1.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer1.ContainerCollection.Add(new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 1 });
			bookingContainer1.ContainerCollection.Add(new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 2 });
			shipment.SubShipmentCollection.Add(bookingContainer1);

			var bookingContainer2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer2.TransportBookingDirection = new TransportBookingDirection() { Code = "DST" };
			bookingContainer2.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer2.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" };
			bookingContainer2.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer2.ContainerCollection.Add(new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 3 });
			bookingContainer2.ContainerCollection.Add(new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 4 });
			bookingContainer2.ContainerCollection.Add(new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 6 });
			shipment.SubShipmentCollection.Add(bookingContainer2);

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var consolidation = reader.ReadIntoBusinessObject();

			var orderedBookings = consolidation.Bookings;
			AssertEquals(false, consolidation.IsMultiBooking);
			AssertEquals(false, consolidation.KB_IsOverridden);
			AssertEquals("It should create 2 transport bookings.", 2, orderedBookings.Count);
			if (hasLink)
			{
				AssertEquals("With link should assign all containers", 2, orderedBookings[0].AssignedPackages.Count);
				AssertEquals("With link should assign all containers", 3, orderedBookings[1].AssignedPackages.Count);
			}
			else
			{
				AssertEquals("With no link it will assigned first containar which matches 'Package Quantity' with 'Container Count' ... Existing functionality", 1, orderedBookings[0].AssignedPackages.Count);
				AssertEquals("With no link it will assigned first containar which matches 'Package Quantity' with 'Container Count' ... Existing functionality", 1, orderedBookings[1].AssignedPackages.Count);
			}

			// re-import same containerised booking information
			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidation;
			topLevelReader.ReadIntoBusinessObject(ref con);
			orderedBookings = consolidation.Bookings;
			AssertEquals(false, consolidation.IsMultiBooking);
			AssertEquals(false, consolidation.KB_IsOverridden);
			AssertEquals(2, orderedBookings.Count);
			if (hasLink)
			{
				AssertEquals("With link should assign all containers", 2, orderedBookings[0].AssignedPackages.Count);
				AssertEquals("With link should assign all containers", 3, orderedBookings[1].AssignedPackages.Count);
			}
			else
			{
				AssertEquals("With no link it will assigned first containar which is match 'Package Quantity' with 'Container Count' ... Existing functionality", 1, orderedBookings[0].AssignedPackages.Count);
				AssertEquals("With no link it will assigned first containar which is match 'Package Quantity' with 'Container Count' ... Existing functionality", 1, orderedBookings[1].AssignedPackages.Count);
			}
		}

		public void TestPopulateBusinessObject_ContainersWithNoContainerIDs_EnsureExistingBookingsPopulatedCorrectlyWhenNewTemplateGiven()
		{
			var ref20GP = Helper.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = GetForwardingConsolWithShipmentDataObject();
			var shipment = consol.SubShipmentCollection.Single();
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(GetTransportBookingInfoForParentDataObject("EECR", new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 1 } }));
			shipment.SubShipmentCollection.Add(GetTransportBookingInfoForParentDataObject("EECR", new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 3 } }));
			shipment.SubShipmentCollection.Add(GetTransportBookingInfoForParentDataObject("EECR", new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 5 } }));

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var consolidation = reader.ReadIntoBusinessObject();
			AssertEquals(false, consolidation.KB_IsOverridden);
			var orderedBookings = consolidation.Bookings;
			AssertEquals("It should create 3 transport bookings.", 3, orderedBookings.Count);
			var resultingContainer1 = orderedBookings[0].AssignedPackages.Single();
			var resultingContainer2 = orderedBookings[1].AssignedPackages.Single();
			var resultingContainer3 = orderedBookings[2].AssignedPackages.Single();
			AssertEquals("Bookings should each have a unique container.", 3, new[] { resultingContainer1, resultingContainer2, resultingContainer3 }.Distinct().Count());

			universalFactory.SaveForTesting();

			// re-import same containers with different template
			consol.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(GetTransportBookingInfoForParentDataObject("EECS", new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 1 } }));
			shipment.SubShipmentCollection.Add(GetTransportBookingInfoForParentDataObject("EECS", new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 3 } }));
			shipment.SubShipmentCollection.Add(GetTransportBookingInfoForParentDataObject("EECS", new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerCount = 1, ContainerType = ContainerType.New(ref20GP), Link = 5 } }));

			universalFactory = new UniversalObjectFactory();
			reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			consolidation = reader.ReadIntoBusinessObject();
			var orderedBookings_Existing = consolidation.Bookings.Where(b => b.KM_KT_NKBookingTemplate == "EECR").ToArray();
			var orderedBookings_New = consolidation.Bookings.Where(b => b.KM_KT_NKBookingTemplate == "EECS").ToArray();
			AssertEquals("It should still have 3 existing transport bookings.", 3, orderedBookings_Existing.Length);
			var resultingContainer1_existing = orderedBookings_Existing[0].AssignedPackages.Single();
			var resultingContainer2_existing = orderedBookings_Existing[1].AssignedPackages.Single();
			var resultingContainer3_existing = orderedBookings_Existing[2].AssignedPackages.Single();
			AssertEquals("Existing Bookings should each have a unique container.", 3, new[] { resultingContainer1_existing, resultingContainer2_existing, resultingContainer3_existing }.Distinct().Count());

			AssertEquals("It should create 3 new transport bookings.", 3, orderedBookings_New.Length);
			var resultingContainer1_new = orderedBookings_New[0].AssignedPackages.Single();
			var resultingContainer2_new = orderedBookings_New[1].AssignedPackages.Single();
			var resultingContainer3_new = orderedBookings_New[2].AssignedPackages.Single();
			AssertEquals("New Bookings should each have a unique container.", 3, new[] { resultingContainer1_new, resultingContainer2_new, resultingContainer3_new }.Distinct().Count());
		}

		UniversalShipment GetForwardingConsolWithShipmentDataObject()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var ref20GP = Helper.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP), Link = 1 };
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP), Link = 3 };
			var container3 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerType = ContainerType.New(ref20GP), Link = 5 };

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);
			consol.ContainerCollection.Add(container3);

			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(container1);
			shipment.ContainerCollection.Add(container2);
			shipment.ContainerCollection.Add(container3);

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 30, PackType = new PackageType() { Code = "PLT" }, ContainerLink = 1 });
			shipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 40, PackType = new PackageType() { Code = "PLT" }, ContainerLink = 3 });
			shipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 50, PackType = new PackageType() { Code = "PLT" }, ContainerLink = 5 });

			return consol;
		}

		UniversalShipment GetTransportBookingInfoForParentDataObject(ZString template, IEnumerable<Container> containerInfos)
		{
			var transportBookingInfo = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			transportBookingInfo.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			transportBookingInfo.TransportBookingDirection = new TransportBookingDirection() { Code = "ORG" };
			transportBookingInfo.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = template };
			transportBookingInfo.SetContainerCollection(() => new DataObjectList<Container>());
			containerInfos.ForEach(i => transportBookingInfo.ContainerCollection.Add(i));

			return transportBookingInfo;
		}

		public void TestPopulateBusinessObject_WayBillOnTopLevelDO_MasterBill()
		{
			// consol
			// - 1 shipment, 1 booking

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
			shipment.SubShipmentCollection.Add(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));

			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);
			consol.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());
			consol.WayBillNumber = "ABC123";

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead1 = reader.ReadIntoBusinessObject();
			AssertEquals("Precondition.", 1, consolidationRead1.Bookings.Count);

			AssertEquals("Precondition.", 1, consolidationRead1.AdditionalReferenceNumbers.Count);
			AssertEquals("MasterBill should be populated in AdditionalReferences.", TransportCommonAdditionalReferenceTypes.Codes.MasterBill, consolidationRead1.AdditionalReferenceNumbers[0].CE_EntryType);
			AssertEquals("MasterBill should be populated in AdditionalReferences.", "ABC123", consolidationRead1.AdditionalReferenceNumbers[0].CE_EntryNum);
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_WhenSourceDOIsEqualToToplevelDO()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.TransportMode = new CodeDescriptionPair();
			consol.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			consol.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "CONSOLIDATION-TRF" });
			consol.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.OrderNumber }, ReferenceNumber = "CONSOLIDATION-ORD" });

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead1 = reader.ReadIntoBusinessObject();

			AssertEquals("BookingConsolidation AdditionalReferenceNumbers Count", 2, consolidationRead1.AdditionalReferenceNumbers.Count);
			var bookingConsolAdditionalReference1 = consolidationRead1.AdditionalReferenceNumbers[0];
			AssertEquals("bookingConsolAdditionalReference1.CE_EntryNum", "CONSOLIDATION-TRF", bookingConsolAdditionalReference1.CE_EntryNum);
			AssertEquals("bookingConsolAdditionalReference1.CE_EntryType", AdditionalReferenceTypes.Codes.TransportReference, bookingConsolAdditionalReference1.CE_EntryType);

			var bookingConsolAdditionalReference2 = consolidationRead1.AdditionalReferenceNumbers[1];
			AssertEquals("bookingConsolAdditionalReference2.CE_EntryNum", "CONSOLIDATION-ORD", bookingConsolAdditionalReference2.CE_EntryNum);
			AssertEquals("bookingConsolAdditionalReference2.CE_EntryType", AdditionalReferenceTypes.Codes.OrderNumber, bookingConsolAdditionalReference2.CE_EntryType);
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_WhenSourceDOIsNotEqualToTopLevelDO()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.TransportMode = new CodeDescriptionPair();
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "CONSOLIDATION-TRF" });
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.OrderNumber }, ReferenceNumber = "CONSOLIDATION-ORD" });
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead1 = reader.ReadIntoBusinessObject();

			AssertEquals("BookingConsolidation AdditionalReferenceNumbers Count", 2, consolidationRead1.AdditionalReferenceNumbers.Count);
			var bookingConsolAdditionalReference1 = consolidationRead1.AdditionalReferenceNumbers[0];
			AssertEquals("bookingConsolAdditionalReference1.CE_EntryNum", "CONSOLIDATION-TRF", bookingConsolAdditionalReference1.CE_EntryNum);
			AssertEquals("bookingConsolAdditionalReference1.CE_EntryType", AdditionalReferenceTypes.Codes.TransportReference, bookingConsolAdditionalReference1.CE_EntryType);

			var bookingConsolAdditionalReference2 = consolidationRead1.AdditionalReferenceNumbers[1];
			AssertEquals("bookingConsolAdditionalReference2.CE_EntryNum", "CONSOLIDATION-ORD", bookingConsolAdditionalReference2.CE_EntryNum);
			AssertEquals("bookingConsolAdditionalReference2.CE_EntryType", AdditionalReferenceTypes.Codes.OrderNumber, bookingConsolAdditionalReference2.CE_EntryType);
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_OnSourceDO_WithTargetBooking()
		{
			var mappedOrganisation = Helper.MapOrganisation(EdiOrgCode);
			var consolidation = SimulateExistingConsolidation(mappedOrganisation, "ETB", "TB042");

			Factory.Save();

			var targetBooking = consolidation.Bookings[0];

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.DataContext = DataContextFactory.New();
			bookingDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = bookingDataObject;

			bookingDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			bookingDataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "BOOKING-TRF" });
			bookingDataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.OrderNumber }, ReferenceNumber = "BOOKING-ORD" });

			var reader = new DtbBookingConsolidationDataObjectReader(bookingDataObject, Logger, new UniversalObjectFactory(), targetBooking);
			var consolidationRead1 = reader.ReadIntoBusinessObject();

			AssertEquals("BookingConsolidation AdditionalReferenceNumbers Count should be 0 we are only importing a booking.", 0, consolidationRead1.AdditionalReferenceNumbers.Count);

			var booking = consolidationRead1.Bookings.Single(t => t.PK == targetBooking.PK);
			AssertEquals("First booking of BookingConsolidation's AdditionalReferenceNumbers Count should be 2", 2, booking.AdditionalReferenceNumbers.Count);

			var bookingAdditionalReference1 = booking.AdditionalReferenceNumbers[0];
			AssertEquals("bookingAdditionalReference1.CE_EntryNum", "BOOKING-TRF", bookingAdditionalReference1.CE_EntryNum);
			AssertEquals("bookingAdditionalReference1.CE_EntryType", AdditionalReferenceTypes.Codes.TransportReference, bookingAdditionalReference1.CE_EntryType);

			var bookingAdditionalReference2 = booking.AdditionalReferenceNumbers[1];
			AssertEquals("bookingAdditionalReference2.CE_EntryNum", "BOOKING-ORD", bookingAdditionalReference2.CE_EntryNum);
			AssertEquals("bookingAdditionalReference2.CE_EntryType", AdditionalReferenceTypes.Codes.OrderNumber, bookingAdditionalReference2.CE_EntryType);
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_EmptyBookingConfirmationReferenceToCollection()
		{
			var bookingConsolidationDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingConsolidationDO.TransportMode = new CodeDescriptionPair();
			SetDataObjectBookingConfirmationReferenceAndNoOtherAdditionalReferenceNumbers(bookingConsolidationDO, null);

			var reader = new DtbBookingConsolidationDataObjectReader(bookingConsolidationDO, Logger, new UniversalObjectFactory());
			var bookingConsolidationBORead = reader.ReadIntoBusinessObject();

			AssertEquals("Should not contain Additional Reference Number when BookingConfirmationReference empty", 0, bookingConsolidationBORead.AdditionalReferenceNumbers.Count);
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_NonEmptyBookingConfirmationReferenceToCollection()
		{
			var bookingConsolidationDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingConsolidationDO.TransportMode = new CodeDescriptionPair();
			SetDataObjectBookingConfirmationReferenceAndNoOtherAdditionalReferenceNumbers(bookingConsolidationDO, "AAAAAAAAAA");

			var reader = new DtbBookingConsolidationDataObjectReader(bookingConsolidationDO, Logger, new UniversalObjectFactory());
			var bookingConsolidationBORead = reader.ReadIntoBusinessObject();

			CombineAssertions("Check that CBK additional reference was correctly read in from BookingConfirmationReference", () =>
			{
				AssertEquals("Should contain 1 Additional Reference Number", 1, bookingConsolidationBORead.AdditionalReferenceNumbers.Count);
				var reference = bookingConsolidationBORead.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().FirstOrDefault();
				AssertEquals("Additional Reference Number should be of type 'CBK'", TransportAdditionalReferenceTypes.Codes.CarrierBookingReference, reference?.CE_EntryType);
				AssertEquals("Additional Reference Number should have CE_EntryNum set to the dataObject's BookingConfirmationReference", bookingConsolidationDO.BookingConfirmationReference, reference?.CE_EntryNum);
			});
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_NonEmptyBookingConfirmationReference_IntoExistingConsolidationWithBookingReferenceNumber()
		{
			const string oldCarrierReference = "OLDREF";
			const string newCarrierReference = "NEWREF";
			const string bookingJobID = "T123";

			var bookingConsolidationBO = Helper.CreateConsolidation();
			var bookingBO = Helper.CreateBooking(bookingConsolidationBO);
			bookingBO.KM_JobID = bookingJobID;
			bookingConsolidationBO.KB_IsOverridden = false;
			var carrierBookingReference = bookingConsolidationBO.AdditionalReferenceNumbers.AddNew();
			carrierBookingReference.CE_EntryType = TransportAdditionalReferenceTypes.Codes.CarrierBookingReference;
			carrierBookingReference.CE_EntryNum = oldCarrierReference;
			Factory.Save();

			var bookingConsolidationDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingConsolidationDO.DataContext = DataContextFactory.New();
			bookingConsolidationDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			bookingConsolidationDO.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, bookingConsolidationBO.KB_JobID);
			bookingConsolidationDO.DataContext.AddDataTarget(DataContextType.TransportBooking, bookingJobID);
			bookingConsolidationDO.TransportMode = new CodeDescriptionPair();

			SetDataObjectBookingConfirmationReferenceAndNoOtherAdditionalReferenceNumbers(bookingConsolidationDO, newCarrierReference);

			var reader = new DtbBookingConsolidationDataObjectReader(bookingConsolidationDO, Logger, new UniversalObjectFactory());
			var bookingConsolidationBORead = reader.ReadIntoBusinessObject();

			CombineAssertions("Check that new CBK additional reference was correctly read in from BookingConfirmationReference, replacing old CBK additional reference", () =>
			{
				AssertEquals("Should have matched correct consolidation from DataTarget", bookingConsolidationBO.KB_JobID, bookingConsolidationBORead.KB_JobID);
				AssertEquals("Should contain exactly 1 Additional Reference Number", 1, bookingConsolidationBORead.AdditionalReferenceNumbers.Count);
				var reference = bookingConsolidationBORead.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().FirstOrDefault();
				AssertEquals("Additional Reference Number should be of type 'CBK'", TransportAdditionalReferenceTypes.Codes.CarrierBookingReference, reference?.CE_EntryType);
				AssertEquals("Additional Reference Number should have CE_EntryNum updated to the dataObject's BookingConfirmationReference", newCarrierReference, reference?.CE_EntryNum);
			});
		}

		void SetDataObjectBookingConfirmationReferenceAndNoOtherAdditionalReferenceNumbers(UniversalShipment dataObject, ZString? bookingConfirmationReference)
		{
			dataObject.SetAdditionalAddressInfoCollection(() => null);
			dataObject.BookingConfirmationReference = bookingConfirmationReference;
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_OnShipmentSourceDO_WithTargetBooking()
		{
			var consolXml = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.ShipmentWithAdditionalReferencesOnParentConsolidation.xml");
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var xmlReader = ObjectFactory.Get<IXmlReader>();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(consolXml)))
			{
				xmlReader.ReadXML(dataObject, stream, Logger);
			}

			var cbkReferenceNumber = dataObject.ParentShipmentCollection[0].BookingConfirmationReference = "CBKREF123";
			var masterBillReferenceNumber = dataObject.ParentShipmentCollection[0].WayBillNumber = "MAB123";
			var houseBillReferenceNumber = dataObject.WayBillNumber = "HSB123";
			var bprReferenceNumber = "BPR123";
			dataObject.ParentShipmentCollection[0].SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ParentShipmentCollection[0].SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() {
				new AdditionalReference()
				{
					Type = new UniversalDataBusEntryType()
					{
						Code = AdditionalReferenceTypes.Codes.BookingPartyReference,
						Description = AdditionalReferenceTypes.Descriptions.BookingPartyReference
					},
					ReferenceNumber = bprReferenceNumber
				},
			});

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(dataObject, Logger, universalFactory);
			DtbBookingConsolidation consolidationRead = null;
			AssertNoExceptionThrown(() => { consolidationRead = reader.ReadIntoBusinessObject(); });
			AssertNotNull(consolidationRead);

			CombineAssertions("Additional References should read correctly from the dataObjects parent shipment", () =>
			{
				AssertEquals("Should contain 4 Additional Reference Numbers", 4, consolidationRead.AdditionalReferenceNumbers.Count);
				var cbkReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.CarrierBookingReference);
				AssertNotNull("There should be an Additional Reference Number of type 'CBK'", cbkReference);
				AssertEquals("Should contain Additional Reference Number when BookingConfirmationReference is empty on shipment, but present on consolidation", cbkReferenceNumber, cbkReference?.CE_EntryNum);

				var masterBillReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.MasterBill);
				AssertNotNull("There should be an Additional Reference Number of type 'MAB'", masterBillReference);
				AssertEquals("Should contain Additional Reference Number when master WayBillNumber is present on consolidation", masterBillReferenceNumber, masterBillReference?.CE_EntryNum);

				var houseBillReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.HouseBill);
				AssertNotNull("There should be an Additional Reference Number of type 'HSB'", houseBillReference);
				AssertEquals("Should contain Additional Reference Number when house WayBillNumber is present on shipment", houseBillReferenceNumber, houseBillReference?.CE_EntryNum);

				var bprReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.BookingPartyReference);
				AssertNotNull("There should be an Additional Reference Number of type 'BPR'", bprReference);
				AssertEquals("Should contain Additional Reference Number when additional reference is empty on shipment, but present on consolidation", bprReferenceNumber, bprReference?.CE_EntryNum);
			});

			var bprReferenceNumber2 = "BPR1232";
			dataObject.ParentShipmentCollection[0].SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>() {
				new AdditionalReference()
				{
					Type = new UniversalDataBusEntryType()
					{
						Code = AdditionalReferenceTypes.Codes.BookingPartyReference,
						Description = AdditionalReferenceTypes.Descriptions.BookingPartyReference
					},
					ReferenceNumber = bprReferenceNumber2
				},
			});
			consolidationRead = null;
			AssertNoExceptionThrown(() => { consolidationRead = reader.ReadIntoBusinessObject(); });
			AssertNotNull(consolidationRead);

			CombineAssertions("Additional References should read correctly from the dataObjects parent shipment, apart from reference that also shows on dataObject.", () =>
			{
				AssertEquals("Should contain 4 Additional Reference Numbers", 4, consolidationRead.AdditionalReferenceNumbers.Count);
				var cbkReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.CarrierBookingReference);
				AssertNotNull("There should be an Additional Reference Number of type 'CBK'", cbkReference);
				AssertEquals("Should contain Additional Reference Number when BookingConfirmationReference is empty on shipment, but present on consolidation", cbkReferenceNumber, cbkReference?.CE_EntryNum);

				var masterBillReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.MasterBill);
				AssertNotNull("There should be an Additional Reference Number of type 'MAB'", masterBillReference);
				AssertEquals("Should contain Additional Reference Number when master WayBillNumber is present on consolidation", masterBillReferenceNumber, masterBillReference?.CE_EntryNum);

				var houseBillReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.HouseBill);
				AssertNotNull("There should be an Additional Reference Number of type 'HSB'", houseBillReference);
				AssertEquals("Should contain Additional Reference Number when house WayBillNumber is present on shipment", houseBillReferenceNumber, houseBillReference?.CE_EntryNum);

				var bprReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.BookingPartyReference);
				AssertNotNull("There should be an Additional Reference Number of type 'BPR'", bprReference);
				AssertEquals("Should contain Additional Reference Number from shipment when additional reference is populated on shipment, and present on consolidation", bprReferenceNumber2, bprReference?.CE_EntryNum);
			});

			dataObject.DataContext.DataSourceCollection.First().Type = nameof(DataContextType.LocalTransport);

			consolidationRead = null;
			AssertNoExceptionThrown(() => { consolidationRead = reader.ReadIntoBusinessObject(); });
			AssertNotNull(consolidationRead);

			CombineAssertions("Additional References should not read from dataObjects parent shipment as dataObject is not a shipment", () =>
			{
				AssertEquals("Should only contain 1 Additional Reference Number (HSB) as dataObject is not a shipment", 1, consolidationRead.AdditionalReferenceNumbers.Count);
				var houseBillReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.HouseBill);
				AssertNotNull("There should be an Additional Reference Number of type 'HSB'", houseBillReference);
				AssertEquals("Should contain Additional Reference Number when house WayBillNumber is present on shipment", houseBillReferenceNumber, houseBillReference?.CE_EntryNum);
			});

			dataObject.DataContext.DataSourceCollection.First().Type = nameof(DataContextType.ForwardingShipment);
			dataObject.ParentShipmentCollection[0].DataContext.DataSourceCollection.First().Type = nameof(DataContextType.LocalTransport);

			consolidationRead = null;
			AssertNoExceptionThrown(() => { consolidationRead = reader.ReadIntoBusinessObject(); });
			AssertNotNull(consolidationRead);

			CombineAssertions("Additional References should not read from dataObjects parent shipment as the parent shipment is not a consolidation", () =>
			{
				AssertEquals("Should contain 1 Additional Reference Number (HSB) as the parent is not a consol", 1, consolidationRead.AdditionalReferenceNumbers.Count);
				var houseBillReference = consolidationRead.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.HouseBill);
				AssertNotNull("There should be an Additional Reference Number of type 'HSB'", houseBillReference);
				AssertEquals("Should contain Additional Reference Number when house WayBillNumber is present on shipment", houseBillReferenceNumber, houseBillReference?.CE_EntryNum);
			});
		}

		public void TestPopulateBusinessObject_WhenBookingNotNull()
		{
			var booking = Helper.CreateBooking();
			var reader = new DtbBookingConsolidationDataObjectReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, new UniversalObjectFactory(), booking);
			var consolidationRead = reader.ReadIntoBusinessObject();
			AssertEquals(consolidationRead.Bookings.Single(), booking);
		}

		public void TestPopulateBusinessObject_WhenRemovingExistingInstructions_WithTargetBooking()
		{
			// Arrange

			var universalConsolidation = new UniversalShipment { DataContext = DataContextFactory.New() };
			universalConsolidation.DataContext.AddDataSource(DataContextType.TransportBooking, "TB042");
			universalConsolidation.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = universalConsolidation;

			var mappedOrganisation = Helper.MapOrganisation(EdiOrgCode);
			var consolidation = SimulateExistingConsolidation(mappedOrganisation, "ETB", "TB042");

			Factory.Save();

			var booking1 = consolidation.Bookings[0];
			var package1 = booking1.Instructions[0].DivotsWithPackages[0].Package;
			var booking2 = consolidation.Bookings[1];

			// Act

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetInstructionCollection(() => new DataObjectList<Instruction>());
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());

			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, new UniversalObjectFactory(), booking2);
			var resultConsolidation = reader.ReadIntoBusinessObject();

			// Assert

			AssertEquals("Expecting 2 bookings", 2, resultConsolidation.Bookings.Count);

			var resultBooking1 = resultConsolidation.Bookings.Single(booking => booking.PK == booking1.PK);
			AssertEquals("Expecting 1 original instruction 1st booking", 1, resultBooking1.Instructions.Count);
			AssertEquals("Expecting 1 original package for 1st booking", 1, resultBooking1.Instructions[0].DivotsWithPackages.Count);
			AssertEquals("Expecting package with original PK for 1st booking", package1.PK, resultBooking1.Instructions[0].DivotsWithPackages[0].Package.PK);

			var resultBooking2 = resultConsolidation.Bookings.Single(booking => booking.PK == booking2.PK);
			AssertEquals("Expecting 0 instruction for 2nd booking", 0, resultBooking2.Instructions.Count);
		}

		public void TestPopulateBusinessObject_WhenRemovingExistingInstructions_WithoutTargetBooking()
		{
			// Arrange

			var universalConsolidation = new UniversalShipment { DataContext = DataContextFactory.New() };
			universalConsolidation.DataContext.AddDataSource(DataContextType.TransportBooking, "TB042");
			universalConsolidation.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = universalConsolidation;

			var mappedOrganisation = Helper.MapOrganisation(EdiOrgCode);
			var consolidation = SimulateExistingConsolidation(mappedOrganisation, "ETB", "TB042");

			Factory.Save();

			// Act

			var reader = new DtbBookingConsolidationDataObjectReader(universalConsolidation, Logger, new UniversalObjectFactory());
			var resultConsolidation = reader.ReadIntoBusinessObject();

			// Assert

			AssertEquals("Expecting 2 bookings", 2, resultConsolidation.Bookings.Count);

			foreach (var booking in resultConsolidation.Bookings)
			{
				AssertEquals("Expecting 0 instruction", 0, booking.Instructions.Count);
			}
		}

		public void TestPopulateBusinessObject_WhenRemovingExistingInstructions_WithoutTargetBookingAndHasCartageAgentRecipientRole()
		{
			// Arrange

			var universalConsolidation = new UniversalShipment { DataContext = DataContextFactory.New() };
			universalConsolidation.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			universalConsolidation.DataContext.AddDataSource(DataContextType.TransportBooking, "TB042");
			universalConsolidation.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = universalConsolidation;

			var mappedOrganisation = Helper.MapOrganisation(EdiOrgCode);
			var consolidation = SimulateExistingConsolidation(mappedOrganisation, "ETB", "TB042");

			Factory.Save();

			// Act

			var reader = new DtbBookingConsolidationDataObjectReader(universalConsolidation, Logger, new UniversalObjectFactory());
			var resultConsolidation = reader.ReadIntoBusinessObject();

			// Assert

			AssertEquals("Expecting 2 bookings", 2, resultConsolidation.Bookings.Count);

			foreach (var booking in resultConsolidation.Bookings)
			{
				AssertEquals("Expecting 0 instruction", 0, booking.Instructions.Count);
			}
		}

		DtbBookingConsolidation SimulateExistingConsolidation(OrgHeader mappedOrganisation, string entryType, string entryNum)
		{
			var consolidation = Helper.CreateConsolidation();
			var packageJob = Helper.CreatePackageJob(consolidation);

			var booking1 = Helper.CreateBooking(consolidation);
			booking1.AdditionalReferenceNumbers.AddNewIfNotExist(entryType, entryNum);
			var instruction1 = Helper.CreateInstruction(booking1);
			var package1 = Helper.CreatePackage("PKG1", 1);
			package1.KP_Sequence = 1;
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			instruction1.DivotsWithPackages.AddPackage(package1);

			var booking2 = Helper.CreateBooking(consolidation);
			booking2.AdditionalReferenceNumbers.AddNewIfNotExist(entryType, entryNum);
			var instruction2 = Helper.CreateInstruction(booking2);
			var package2 = Helper.CreatePackage("PKG2", 1);
			package2.KP_Sequence = 1;
			package2.KP_KJ_ParentPackageJob = packageJob.PK;
			instruction2.DivotsWithPackages.AddPackage(package2);

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = false;
			jobDocAddress.E2_ParentID = consolidation.PK;
			jobDocAddress.E2_ParentTableCode = DtbBookingConsolidationSchema.Constants.Prefix;
			jobDocAddress.E2_OA_Address = mappedOrganisation.MainAddress.PK;

			return consolidation;
		}

		DtbBookingConsolidation SimulateExistingConsolidationForTestReturnMatching(string bookingJobId)
		{
			var consolidation = Helper.CreateConsolidation();
			var packageJob = Helper.CreatePackageJob(consolidation);
			consolidation.KB_JobType = "BKG";

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobType = "BKG";
			booking.KM_Direction = "PIC";
			booking.KM_JobID = bookingJobId;
			booking.KM_RS_NKServiceLevel = "STD";

			var instruction1 = Helper.CreateInstruction(booking);
			var package1 = Helper.CreatePackage("PKG1", 1);
			package1.KP_Sequence = 1;
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			instruction1.DivotsWithPackages.AddPackage(package1);

			// no booked by organisation necessary as we aren't expecting any constraint on Booked By Organisation - unlike other Simulate() method

			return consolidation;
		}

		public void TestPopulateBusinessObject_IgnoreZeroPacklineWhenContainerEmpty()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipmentWithContainer = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithContainer.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { shipmentWithContainer });

			var emptyContainer = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "SHP102938",
				ContainerType = new ContainerType() { Code = "20GP" },
				IsEmptyContainer = true,
				Link = 1
			};
			shipmentWithContainer.SetContainerCollection(() => new DataObjectList<Container>() { emptyContainer });

			var emptyPackline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerLink = emptyContainer.Link,
				PackQty = 0,
				PackType = new PackageType() { Code = "PLT" }
			};
			shipmentWithContainer.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { emptyPackline });
			shipmentWithContainer.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithContainer.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();
			var booking = consolidationRead.Bookings.Single();
			var container = booking.AssignedPackages.Single();
			AssertEquals("Should be our container.", "SHP102938", container.KP_PackageID);
			AssertEquals("Should ignore packlines with 0 qty in empty containers.", 0, container.Packages.Count);
		}

		public void TestPopulateBusinessObject_PacklinesHaveParentContainers_WhenCreatingTBFromConsol()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			Logger.TopLevelDataObject = consol;

			var shipmentWithContainer = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithContainer.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { shipmentWithContainer });

			var container = new Container()
			{
				ContainerNumber = "SHP102938",
				ContainerType = new ContainerType() { Code = "20GP" },
				IsEmptyContainer = false,
				Link = 1
			};
			shipmentWithContainer.SetContainerCollection(() => new DataObjectList<Container>() { container });
			consol.SetContainerCollection(() => new DataObjectList<Container>() { container });

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerLink = container.Link,
				PackQty = 5,
				PackType = new PackageType() { Code = "PLT" }
			};
			shipmentWithContainer.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline });
			shipmentWithContainer.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithContainer.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var resultConsolidation = reader.ReadIntoBusinessObject();

			AssertEquals("Packages should have container as parent", 1, resultConsolidation.PackageJob.Packages[0].Packages.Count);
		}

		public void TestPopulateBusinessObject_AllPacklinesAreCreated_WhenCreatingTBFromConsol()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			Logger.TopLevelDataObject = consol;

			var shipmentWithContainer1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithContainer1.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			var shipmentWithContainer2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithContainer2.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S456");

			var shipmentWithContainer3 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithContainer3.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S789");

			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { shipmentWithContainer1, shipmentWithContainer2 });
			shipmentWithContainer2.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { shipmentWithContainer3 });

			var container = new Container()
			{
				ContainerNumber = "SHP102938",
				ContainerType = new ContainerType() { Code = "20GP" },
				IsEmptyContainer = false,
				Link = 1
			};
			shipmentWithContainer1.SetContainerCollection(() => new DataObjectList<Container>() { container });
			shipmentWithContainer2.SetContainerCollection(() => new DataObjectList<Container>() { container });
			shipmentWithContainer3.SetContainerCollection(() => new DataObjectList<Container>() { container });
			consol.SetContainerCollection(() => new DataObjectList<Container>() { container });

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerLink = container.Link,
				PackQty = 5,
				PackType = new PackageType() { Code = "PLT" }
			};
			shipmentWithContainer1.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline });
			shipmentWithContainer1.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithContainer1.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			shipmentWithContainer2.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline });
			shipmentWithContainer2.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithContainer2.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			shipmentWithContainer3.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline });
			shipmentWithContainer3.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithContainer3.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var resultConsolidation = reader.ReadIntoBusinessObject();

			AssertEquals("All packlines should be populated", 3, resultConsolidation.PackageJob.Packages[0].Packages.Count);
		}

		public void TestPopulateBusinessObject_PopulatesPackagesFromSubShipments_WhenSubShipmentsHavePackagesAndTopLevelShipmentHasNoContainers()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			Logger.TopLevelDataObject = consol;

			var shipmentWithPackages1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithPackages1.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			var shipmentWithPackages2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithPackages2.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S456");

			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { shipmentWithPackages1, shipmentWithPackages2 });

			var packlineForShipmentWithPackages1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 6,
				PackType = new PackageType() { Code = "PLT" }
			};

			var packlineForShipmentWithPackages2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 11,
				PackType = new PackageType() { Code = "CTN" }
			};

			shipmentWithPackages1.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packlineForShipmentWithPackages1 });
			shipmentWithPackages1.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithPackages1.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			shipmentWithPackages2.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packlineForShipmentWithPackages2 });
			shipmentWithPackages2.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithPackages2.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var resultConsolidation = reader.ReadIntoBusinessObject();

			AssertEquals("There should be packlines on the package job equal to the number of packlines on sub shipments.", 2, resultConsolidation.PackageJob.Packages.Count);
			AssertContainsExactElementsInAnyOrder("Package quantities should match quantities on sub shipments", new ZInt[] { 6, 11 }, resultConsolidation.PackageJob.Packages.Select(p => p.KP_PackageQty));
			AssertContainsExactElementsInAnyOrder("Package types should match types on sub shipments", new string[] { "PLT", "CTN" }, resultConsolidation.PackageJob.Packages.Select(p => p.KP_F3_NKPackType));
		}

		public void TestPopulateBusinessObject_EmptyContainersAreIncludedOnTB_WhenCreatingTBFromConsol()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			Logger.TopLevelDataObject = consol;

			var shipmentWithContainer = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithContainer.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			var container = new Container()
			{
				ContainerNumber = "SHP102938",
				ContainerType = new ContainerType() { Code = "20GP" },
				IsEmptyContainer = true,
				Link = 1
			};
			shipmentWithContainer.SetContainerCollection(() => new DataObjectList<Container>() { container });
			consol.SetContainerCollection(() => new DataObjectList<Container>() { container });

			shipmentWithContainer.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithContainer.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var resultConsolidation = reader.ReadIntoBusinessObject();

			AssertEquals("Empty container should be present on the consol", 1, resultConsolidation.PackageJob.Packages.Count);
		}

		public void TestPopulateBusinessObject_ImportFailOnZeroPacklineWhenContainerNotEmpty()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipmentWithContainer = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			shipmentWithContainer.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { shipmentWithContainer });

			var emptyContainer = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "SHP102938",
				ContainerType = new ContainerType() { Code = "20GP" },
				IsEmptyContainer = false,
				Link = 1
			};
			shipmentWithContainer.SetContainerCollection(() => new DataObjectList<Container>() { emptyContainer });

			var emptyPackline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerLink = emptyContainer.Link,
				PackQty = 0,
				PackType = new PackageType() { Code = "PLT" }
			};
			shipmentWithContainer.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { emptyPackline });
			shipmentWithContainer.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC" };
			shipmentWithContainer.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EECR" }; // EFPU - Loose CNR - CFS | EECR - Empty To CNR

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "A container must be empty if its packline has quantity 0.", () => reader.ReadIntoBusinessObject());
		}

		public void TestReasonsForNotUpdateSourceAndTargetBO()
		{
			using (Factory.AddDisposableService())
			{
				var consolidation = Helper.CreateConsolidation();
				var booking = Helper.CreateBooking(consolidation);
				Factory.Save();

				var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				consolidationDataObject.DataContext = DataContextFactory.New();
				consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
				consolidationDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

				var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
				consolidationDataObject.SubShipmentCollection.Add(bookingDataObject);

				booking.KM_IsActive = false;
				Factory.Save();
				var importResultForNoActiveBookings = GetMessageResult(consolidationDataObject);
				AssertNotContains("The Transport Booking Consolidation cannot be updated as it has been overridden.", importResultForNoActiveBookings.ToString());
				booking.KM_IsActive = true;
				Factory.Save();

				var importResultForNotOverriddenConsolidation = GetMessageResult(consolidationDataObject);
				AssertNotContains("The Transport Booking Consolidation cannot be updated as it has been overridden.", importResultForNotOverriddenConsolidation.ToString());

				consolidation.KB_IsOverridden = true;
				Factory.Save();
				var importResultForOverriddenConsolidation = GetMessageResult(consolidationDataObject);
				AssertContains("The Transport Booking Consolidation cannot be updated as it has been overridden.", importResultForOverriddenConsolidation.ToString());

				consolidationDataObject.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
				var importResultWithJobCostingElement = GetMessageResult(consolidationDataObject);
				AssertContains("The Transport Booking Consolidation cannot be updated as it has been overridden.", importResultWithJobCostingElement.ToString());

				consolidationDataObject.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());
				var importResultWithEmptyChargeLineCollection = GetMessageResult(consolidationDataObject);
				AssertContains("The Transport Booking Consolidation cannot be updated as it has been overridden.", importResultWithEmptyChargeLineCollection.ToString());

				consolidationDataObject.JobCosting.ChargeLineCollection.Add(new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance) { ChargeCode = new ChargeCode { Code = "Code" } });
				var importResultWithCharges = GetMessageResult(consolidationDataObject);
				AssertNotContains("The Transport Booking Consolidation cannot be updated as it has been overridden.", importResultWithCharges.ToString());
			}
		}

		public void TestReasonsForNotUpdateSourceAndTargetBO_Commenced()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			consolidationDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolidationDataObject.SubShipmentCollection.Add(bookingDataObject);

			consolidation.KB_IsOverridden = false;
			booking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
			Factory.Save();
			var importResultForBookingWithCommencedLog = GetMessageResult(consolidationDataObject);
			AssertContains("Bookings on this Consolidation have already been commenced, cannot update the Consolidation.", importResultForBookingWithCommencedLog.ToString());

			booking.KM_IsActive = false;
			Factory.Save();
			var importResultForNoActiveBookings = GetMessageResult(consolidationDataObject);
			AssertNotContains("Bookings on this Consolidation have already been commenced, cannot update the Consolidation.", importResultForNoActiveBookings.ToString());
		}

		public void TestReasonsForNotUpdateSourceAndTargetBO_SenderIsTransportCompany_OverrideTrue()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			var transportCompany = Helper.CreateOrganisation("ABC");
			booking.Address.OrganisationPK = transportCompany.PK;
			Factory.Save();

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, booking.KM_JobID);
			consolidationDataObject.DataContext.DataProviderForCodeMapping = "ABC";

			consolidation.KB_IsOverridden = true;
			Factory.Save();

			var importResultForBookingWithCommencedLog = GetMessageResult(consolidationDataObject);
			AssertNotContains("The Transport Booking Consolidation cannot be updated as it has been overridden.", importResultForBookingWithCommencedLog.ToString());
		}

		public void TestReasonsForNotUpdateSourceAndTargetBO_SenderIsCarrierBookingAgent_OverrideTrue()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			var carrierBookingAgent = Helper.CreateOrganisation("ABC");
			booking.CarrierBookingAgentDocAddress.OrganisationPK = carrierBookingAgent.PK;
			Factory.Save();

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, booking.KM_JobID);
			consolidationDataObject.DataContext.DataProviderForCodeMapping = "ABC";

			consolidation.KB_IsOverridden = true;
			Factory.Save();

			var importResultForBookingWithCommencedLog = GetMessageResult(consolidationDataObject);
			AssertNotContains("The Transport Booking Consolidation cannot be updated as it has been overridden.", importResultForBookingWithCommencedLog.ToString());
		}

		IImportResult GetMessageResult(UniversalShipment consolidationDataObject)
		{
			var message = TestCaseHelper.GetQueuedUniversalShipmentMessageFromDataObject(consolidationDataObject);
			var manager = new UniversalMessageProcessingManager(new UniversalObjectFactory(Factory), new ServiceTaskLogForTesting());
			return manager.Process(message).ImportResults.Single(o => o.DataContextType == DataContextType.TransportBookingConsolidation);
		}

		public void TestRepopulateOrCancel()
		{
			var ufactory = new UniversalObjectFactory();

			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation); // no packages, will be deactivated
			Factory.Save();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "ILDV" };
			Logger.TopLevelDataObject = shipment;

			booking = ufactory.Load<DtbBooking>(booking.PK);
			consolidation = ufactory.Load<DtbBookingConsolidation>(consolidation.PK);
			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory, booking);
			reader.ReadIntoBusinessObject();
			AssertEquals("Should have created a new booking.", 2, consolidation.Bookings.Count);
			AssertEquals("Should not have added an error.", false, Logger.HasErrors);
			AssertEquals(false, booking.KM_IsActive);
		}

		public void TestBookingsAreReadInNormallyWhenRecipientRoleIsNotCartageAdvice()
		{
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var bookingDataObject1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject1.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "TRANS1" };
			bookingDataObject1.SetInstructionCollection(() => new DataObjectList<Instruction> { new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Sequence = 2 } });
			bookingDataObject1.DataContext = DataContextFactory.New();
			bookingDataObject1.DataContext.AddDataSource(DataContextType.TransportBooking, "TB123");

			var bookingDataObject2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject2.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "TRANS2" };
			bookingDataObject2.SetInstructionCollection(() => new DataObjectList<Instruction> { new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Sequence = 3 } });
			bookingDataObject2.DataContext = DataContextFactory.New();
			bookingDataObject2.DataContext.AddDataSource(DataContextType.TransportBooking, "TB234");

			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolidationDataObject.SubShipmentCollection.Add(bookingDataObject1);
			consolidationDataObject.SubShipmentCollection.Add(bookingDataObject2);

			var reader = new DtbBookingConsolidationDataObjectReader(consolidationDataObject, Logger, new UniversalObjectFactory());
			var consolidation = reader.ReadIntoBusinessObject();
			AssertEquals(2, consolidation.Bookings.Count);

			var booking1 = consolidation.Bookings.Single(b => b.KM_TransportReference == "TRANS1");
			AssertEquals(1, booking1.Instructions.Count);
			AssertEquals(2, booking1.Instructions[0].KN_Sequence);

			var booking2 = consolidation.Bookings.Single(b => b.KM_TransportReference == "TRANS2");
			AssertEquals(1, booking2.Instructions.Count);
			AssertEquals(3, booking2.Instructions[0].KN_Sequence);
		}

		public void TestTopLevelShipmentIsReadInAsBookingIfNoSubShipments()
		{
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "TRANSREF" };

			var reader = new DtbBookingConsolidationDataObjectReader(consolidationDataObject, Logger, new UniversalObjectFactory());
			var consolidation = reader.ReadIntoBusinessObject();
			AssertEquals(1, consolidation.Bookings.Count);
			AssertEquals("TRANSREF", consolidation.Bookings[0].KM_TransportReference);
		}

		public void TestBookingsAreNotDeletedWhenMerging()
		{
			var matchingConsolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(matchingConsolidation);
			booking.KM_TransportReference = "EXISTINGREF";
			Factory.Save();

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, matchingConsolidation.KB_JobID);
			consolidationDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "TRANSREF" };
			bookingDataObject.DataContext = DataContextFactory.New();
			bookingDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, "TB123");
			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consolidationDataObject.SubShipmentCollection.Add(bookingDataObject);

			var message = TestCaseHelper.GetQueuedUniversalShipmentMessageFromDataObject(consolidationDataObject);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var importResult = manager.Process(message).ImportResults.Single(o => o.DataContextType == DataContextType.TransportBookingConsolidation);
			var consolidation = (DtbBookingConsolidation)importResult.GetBizOForTesting(consolidationDataObject, new BusinessObjectFactory());
			AssertEquals(matchingConsolidation.PK, consolidation.PK);
			AssertEquals(2, consolidation.Bookings.Count);
			var existingBooking = consolidation.Bookings.Single(b => b.KM_TransportReference == "EXISTINGREF");
			AssertEquals(booking.PK, existingBooking.PK);
			AssertEquals("TRANSREF", consolidation.Bookings.Except(new[] { existingBooking }).Single().KM_TransportReference);
		}

		public void TestCommencedBookingsAreNotOverridenWhenCreatingCartageAdvice()
		{
			var ufactory = new UniversalObjectFactory();

			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_KT_NKBookingTemplate = "LC2C";
			booking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
			Factory.Save();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "LC2C" };
			Logger.TopLevelDataObject = shipment;

			booking = ufactory.Load<DtbBooking>(booking.PK);
			consolidation = ufactory.Load<DtbBookingConsolidation>(consolidation.PK);
			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory, booking);
			reader.ReadIntoBusinessObject();
			AssertEquals("Should not have created a new booking.", 1, consolidation.Bookings.Count);
			AssertEquals("Should have added an error.", true, Logger.HasErrors);
			AssertEquals(true, Logger.Logs.Contains("Bookings on this Consolidation have already been commenced, cannot update the Consolidation."));

			Logger.ClearLogs();
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "ILDV" };
			reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory, booking);
			reader.ReadIntoBusinessObject();
			AssertEquals("Should not have created a new booking.", 1, consolidation.Bookings.Count);
			AssertEquals("Should have added an error.", true, Logger.HasErrors);
			AssertEquals(true, Logger.Logs.Contains("Bookings on this Consolidation have already been commenced, cannot update the Consolidation."));
		}

		public void TestCommencedContainerisedMultiBookingsAreNotOverridenWhenCreatingCartageAdvice()
		{
			var uFactory = new UniversalObjectFactory();

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);

			var container1ForShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var container2ForShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(container1ForShipment);
			shipment.ContainerCollection.Add(container2ForShipment);

			var ref20GP = Helper.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			container1.ContainerType = ContainerType.New(ref20GP);
			container2.ContainerType = ContainerType.New(ref20GP);
			container1ForShipment.ContainerType = ContainerType.New(ref20GP);
			container2ForShipment.ContainerType = ContainerType.New(ref20GP);
			container1.ContainerNumber = "CONT102938";
			container2.ContainerNumber = "CONT203744";
			container1ForShipment.ContainerNumber = "CONT102938";
			container2ForShipment.ContainerNumber = "CONT203744";

			var bookingContainer1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer1.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer1.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer1.ContainerCollection.Add(container1);
			bookingContainer1.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPL" }; // c:cyd-cfs-cto, l:cnr-cfs
			shipment.SubShipmentCollection.Add(bookingContainer1);

			var bookingContainer2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingContainer2.ShipmentType = new CodeDescriptionPair() { Code = "TBK" };
			bookingContainer2.SetContainerCollection(() => new DataObjectList<Container>());
			bookingContainer2.ContainerCollection.Add(container2);
			bookingContainer2.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "EFPL" }; // c:cyd-cfs-cto, l:cnr-cfs
			shipment.SubShipmentCollection.Add(bookingContainer2);

			var reader1 = new DtbBookingConsolidationDataObjectReader(shipment, Logger, uFactory);
			var bookingConsol = reader1.ReadIntoBusinessObject();
			uFactory.SaveForTesting();
			AssertEquals(2, bookingConsol.Bookings.Count);

			var booking1 = bookingConsol.Bookings[0];
			var booking2 = bookingConsol.Bookings[1];
			booking2.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
			uFactory.SaveForTesting();

			var newFactory = new UniversalObjectFactory(); // for testing fetch hints
			var reader2 = new DtbBookingConsolidationDataObjectReader(shipment, Logger, newFactory, newFactory.Load<DtbBooking>(booking1.PK));
			bookingConsol = reader2.ReadIntoBusinessObject();
			AssertEquals("Should not have created new bookings.", 2, bookingConsol.Bookings.Count);
			AssertEquals("Should have an error for not being able to update the bookings.", true, Logger.HasErrors);
			AssertEquals(true, Logger.Logs.Contains("Bookings on this Consolidation have already been commenced, cannot update the Consolidation."));
			AssertMaxTableHits("", 2, StmALogSchema.Constants.TableName, newFactory.BOFactory);
		}

		public void TestBookingsAreReadIn_NewNamespace()
		{
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var bookingDataObject1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { LocalProcessing = new LocalProcessing { ArrivalCartageRef = "TRANS1" } };
			bookingDataObject1.DataContext = DataContextFactory.New();
			bookingDataObject1.DataContext.AddDataSource(DataContextType.TransportBooking, "TB123");

			var bookingDataObject2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance) { LocalProcessing = new LocalProcessing { ArrivalCartageRef = "TRANS2" } };
			bookingDataObject2.DataContext = DataContextFactory.New();
			bookingDataObject2.DataContext.AddDataSource(DataContextType.TransportBooking, "TB234");

			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingDataObject1, bookingDataObject2 });
			bookingDataObject1.SetParentShipmentCollection(() => new List<UniversalShipment> { consolidationDataObject });

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var reader1 = new DtbBookingConsolidationDataObjectReader(consolidationDataObject, Logger, new UniversalObjectFactory());
				var consolidationWithTwoBookings = reader1.ReadIntoBusinessObject();
				AssertEquals("Should have read in both Bookings as Consolidation was top Level.", 2, consolidationWithTwoBookings.Bookings.Count);

				var reader2 = new DtbBookingConsolidationDataObjectReader(bookingDataObject1, Logger, new UniversalObjectFactory());
				var consolidationWithSingleBooking = reader2.ReadIntoBusinessObject();
				AssertEquals("Should have read in the Single Booking passed in.", "TRANS1", consolidationWithSingleBooking.Bookings.Single().KM_TransportReference);
			}
		}

		public void TestPopulatePackages()
		{
			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject.PackType = new PackageType() { Code = "" }; // customs sets this to blank
			packageDataObject.GoodsDescription = "My Pack";

			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "CONT1234567";
			containerDataObject.ContainerType = new ContainerType() { Code = "20GP" };

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			shipment.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "C123");
			Logger.TopLevelDataObject = shipment;
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject });
			shipment.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "ILDV" };

			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(1, consolidationRead.Bookings.Count);
			var booking = consolidationRead.Bookings[0];
			AssertEquals("ILDV", booking.KM_KT_NKBookingTemplate);
			AssertEquals(2, booking.Instructions.Count);
			AssertEquals(1, booking.AssignedPackages.Count);
			AssertEquals("My Pack", booking.AssignedPackages[0].KP_GoodsDescription);
		}

		public void TestPopulatePackages_WhenDataSurceTypeIsHVLVBookingHeader_PapulatePackageFromSubShipmentPackingLineCollection()
		{
			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject.GoodsDescription = "My Pack";

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.HVLVBookingHeader, "TestHVLVBookingHeader");
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });

			var subshipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subshipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject });
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(subshipment);

			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "ILDV" };

			Logger.TopLevelDataObject = shipment;

			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(1, consolidationRead.Bookings.Count);
			var booking = consolidationRead.Bookings[0];
			AssertEquals(1, booking.AssignedPackages.Count);
			AssertEquals("My Pack", booking.AssignedPackages[0].KP_GoodsDescription);
		}

		public void TestPopulatePackages_IgnoreCaseUniquePackageId()
		{
			var packageDataObject1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject1.GoodsDescription = "My Pack1";
			packageDataObject1.ReferenceNumber = "ref 777";

			var packageDataObject2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packageDataObject2.GoodsDescription = "My Pack2";
			packageDataObject2.ReferenceNumber = "REF 777";

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.HVLVBookingHeader, "TestHVLVBookingHeader");
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });

			var subshipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subshipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject1, packageDataObject2 });
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(subshipment);

			shipment.LocalTransportJobType = new CodeDescriptionPair4Char() { Code = "ILDV" };

			Logger.TopLevelDataObject = shipment;

			var factory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, factory);
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertNoExceptionThrown(() => factory.SaveAtEndOfImport(Logger));
		}

		public void TestPopulatePackages_HasTargetBooking()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Link = 0,
				PackQty = 1,
				PackType = new PackageType() { Code = "PLT" },
				GoodsDescription = "My Pack"
			};

			var instruction = new Instruction(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ServiceInstruction = "INSTRUCTION1",
				Sequence = 0,
			};
			instruction.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>
			{
				new InstructionPackingLineLink
				{
					PackingLineLink = 0,
					Quantity = 1,
					ConfirmationCollection = new List<Confirmation>
					{
						new Confirmation { Reference = "IREF1", Quantity = 1 }
					}
				}
			});

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.DataContext = DataContextFactory.New();
			bookingDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, booking.KM_TransportReference);
			bookingDataObject.DataContext.AddDataTarget(DataContextType.TransportBooking, booking.KM_TransportReference);
			bookingDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { instruction });

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, booking.KM_TransportReference);
			shipment.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, consolidation.KB_JobID);
			shipment.DataContext.AddDataTarget(DataContextType.TransportBooking, booking.KM_TransportReference);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packageDataObject });
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingDataObject });
			Logger.TopLevelDataObject = shipment;

			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, new UniversalObjectFactory(), bookingTarget: booking);
			var consolidationRead = reader.ReadIntoBusinessObject();

			var bookingAfterImport = consolidationRead.Bookings.Single();
			AssertEquals("Should not create a new Booking", booking.PK, bookingAfterImport.PK);
			AssertEquals("Instruction Count:", 1, bookingAfterImport.Instructions.Count);
			AssertEquals("Packages Count", 1, bookingAfterImport.AssignedPackages.Count);
			AssertEquals("My Pack", bookingAfterImport.AssignedPackages.Single().KP_GoodsDescription);
		}

		public void TestPopulatePackagesWithNoPackingLines()
		{
			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject.ContainerNumber = "CONT1234567";
			containerDataObject.ContainerType = new ContainerType() { Code = "20GP" };

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			Logger.TopLevelDataObject = shipment;
			shipment.SetPackingLineCollection(() => null);
			shipment.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });

			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			AssertEquals(1, consolidationRead.Bookings.Count);
			var booking = consolidationRead.Bookings[0];
			AssertEquals(1, booking.PackageJob.Containers.Count);
			AssertEquals(1, booking.PackageJob.Packages.Count);
		}

		public void TestPopulateBookingParty()
		{
			var ufactory = new UniversalObjectFactory();

			var tempCompany = ufactory.New<GlbCompany>();
			tempCompany.GC_Code = "FWD";
			var dc = DataContextFactory.New();
			dc.SetCompanyAndDataProviderDetails(tempCompany);
			var licenceCode = dc.DataProviderForCodeMapping;

			var invalidCompany = ufactory.New<GlbCompany>();
			invalidCompany.GC_Code = "GHI";

			// map to sender
			var sender = ufactory.New<OrgHeader>();
			sender.OH_Code = "SENDR";
			var orgMatch = ufactory.New<OrgPatternMatchOverride>();
			orgMatch.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMatch.OO_OH = Env.CurrentCompany.OrganisationPK;
			orgMatch.OO_LocalGuid = sender.PK;
			orgMatch.OO_ForeignCode = licenceCode;

			// empty uShipment
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			var consolidationRead = reader.ReadIntoBusinessObject();
			var bookingPartyRead = consolidationRead.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).FirstOrDefault();
			AssertNull(bookingPartyRead);

			// uShipment with empty DataContext
			shipment.DataContext = DataContextFactory.New();
			reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			consolidationRead = reader.ReadIntoBusinessObject();
			bookingPartyRead = consolidationRead.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).FirstOrDefault();
			AssertNull(bookingPartyRead);

			// uShipment with Booking Party
			var otherBookingParty = ufactory.New<OrgHeader>();
			otherBookingParty.OH_FullName = "DontImportThisBookingParty";
			var writeManager = new DataWritingManager(new ActionInfo(null, otherBookingParty));
			shipment.AddOrgAddress(writeManager, otherBookingParty.MainAddress, DocAddressType.BookingPartyDocumentaryAddress);
			reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			consolidationRead = reader.ReadIntoBusinessObject();
			bookingPartyRead = consolidationRead.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).FirstOrDefault();
			AssertNull("Do not read from Organisation Collection.", bookingPartyRead);

			// uShipment with DataContext and invalid Sender
			shipment.DataContext.SetCompanyAndDataProviderDetails(invalidCompany);
			Logger.TopLevelDataObject = shipment;
			reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			consolidationRead = reader.ReadIntoBusinessObject();
			bookingPartyRead = consolidationRead.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).FirstOrDefault();
			AssertNull("Invalid Sender, cannot match.", bookingPartyRead);

			// uShipment with DataContext mapped Sender
			shipment.DataContext.SetCompanyAndDataProviderDetails(tempCompany);
			Logger.TopLevelDataObject = shipment;
			reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			consolidationRead = reader.ReadIntoBusinessObject();
			bookingPartyRead = consolidationRead.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).FirstOrDefault();
			AssertNotNull("Should find the Sender in Data Context.", bookingPartyRead);
			AssertEquals("Should find the Org of the Sender in Data Context.", sender, bookingPartyRead.Organisation);
		}

		public void TestMarkEmptyContainerLegAsIsEmptyContainer()
		{
			var ctoAddress = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageCTO);
			var cydAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.LocalCartageYard);
			var consigneeAddress = OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB(DocAddressType.ConsigneeAddress);

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			Logger.TopLevelDataObject = consol;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { ctoAddress, cydAddress, consigneeAddress });
			shipment.TransportBookingDirection = new TransportBookingDirection { Code = "PIC" };
			shipment.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "IFCD" }; // Import FCL/ULD, Unpack at CNE and empty container leg to CYD

			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			consol.SubShipmentCollection.Add(shipment);

			var containerForShipment = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(containerForShipment);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);
			containerForShipment.ContainerNumber = "ABCD";
			containerForShipment.ContainerType = new ContainerType { Code = "20GP" };
			packline.PackQty = 20;
			packline.PackType = new PackageType { Code = "PLT" };

			var reader = new DtbBookingConsolidationDataObjectReader(consol, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();
			AssertEquals(1, consolidationRead.Bookings.Count);
			var booking = consolidationRead.Bookings[0];
			AssertEquals("IFCD", booking.KM_KT_NKBookingTemplate);
			AssertEquals("Precondition", 3, booking.Instructions.Count);
			AssertEquals(true, booking.Instructions.Single(i => i.IsEmptyYard).Confirmations.Single().KK_IsEmptyContainer);
		}

		public void TestUpdateTransportBookingConsolidation()
		{
			var ufactory = new UniversalObjectFactory();

			// map to sender
			var senderA = ufactory.New<OrgHeader>();
			senderA.OH_Code = "SENDA";
			var senderB = ufactory.New<OrgHeader>();
			senderB.OH_Code = "SENDB";

			var tempCompanyA = ufactory.New<GlbCompany>();
			tempCompanyA.GC_Code = "GHI";
			var dc = DataContextFactory.New();
			dc.SetCompanyAndDataProviderDetails(tempCompanyA);
			var licenceCodeA = dc.DataProviderForCodeMapping;

			var tempCompanyB = ufactory.New<GlbCompany>();
			tempCompanyB.GC_Code = "PQR";
			dc = DataContextFactory.New();
			dc.SetCompanyAndDataProviderDetails(tempCompanyB);
			var licenceCodeB = dc.DataProviderForCodeMapping;

			var orgMatchA = ufactory.New<OrgPatternMatchOverride>();
			orgMatchA.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMatchA.OO_OH = Env.CurrentCompany.OrganisationPK;
			orgMatchA.OO_LocalGuid = senderA.PK;
			orgMatchA.OO_ForeignCode = licenceCodeA;

			var orgMatch = ufactory.New<OrgPatternMatchOverride>();
			orgMatch.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMatch.OO_OH = Env.CurrentCompany.OrganisationPK;
			orgMatch.OO_LocalGuid = senderB.PK;
			orgMatch.OO_ForeignCode = licenceCodeB;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, "123");
			shipment.DataContext.SetCompanyAndDataProviderDetails(tempCompanyA);

			var readerA = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			var consolidationReadA = readerA.ReadIntoBusinessObject();
			AssertNotNull("Should create the consolidation.", consolidationReadA);
			AssertEquals("Should create a booking.", 1, consolidationReadA.Bookings.Count);
			AssertContains("Should have logged search for External Booking Reference numbers", "Information - Searching for Booking Consolidation, attempting to match External Transport Booking numbers to Transport Booking number(s) 123 from sending system and match Address to Sending Party SENDA", Logger.Logs);
			AssertEquals(senderA, consolidationReadA.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).First().Organisation);
			AssertEquals("123", consolidationReadA.Bookings.First().AdditionalReferenceNumbers.Cast<Integration.Customs.ICusEntryNumber>().First(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber).CE_EntryNum);
			ufactory.SaveForTesting();

			var readerA2 = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			var consolidationReadA2 = readerA2.ReadIntoBusinessObject();
			AssertNotNull("Should update the consolidation.", consolidationReadA2);
			AssertEquals("Should update the consolidation.", consolidationReadA, consolidationReadA2);
			AssertEquals("Should update the booking.", 1, consolidationReadA.Bookings.Count);
			AssertContains("Should have logged search for External Booking Reference numbers", "Information - Searching for Booking Consolidation, attempting to match External Transport Booking numbers to Transport Booking number(s) 123 from sending system and match Address to Sending Party SENDA", Logger.Logs);
			AssertEquals(senderA, consolidationReadA2.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).FirstOrDefault().Organisation);
			AssertEquals("123", consolidationReadA2.Bookings.First().AdditionalReferenceNumbers.Cast<Integration.Customs.ICusEntryNumber>().First(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber).CE_EntryNum);

			var shipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment2.DataContext = DataContextFactory.New();
			shipment2.DataContext.AddDataSource(DataContextType.TransportBooking, "456");
			shipment2.DataContext.SetCompanyAndDataProviderDetails(tempCompanyA);
			var readerA3 = new DtbBookingConsolidationDataObjectReader(shipment2, Logger, ufactory);
			var consolidationReadA3 = readerA3.ReadIntoBusinessObject();
			AssertNotNull("Should create a consolidation.", consolidationReadA3);
			AssertNotEquals("Should create the consolidation.", consolidationReadA, consolidationReadA3);
			AssertEquals("Should create the booking.", 1, consolidationReadA3.Bookings.Count);
			AssertContains("Should have logged search for External Booking Reference numbers", "Information - Searching for Booking Consolidation, attempting to match External Transport Booking numbers to Transport Booking number(s) 456 from sending system and match Address to Sending Party SENDA", Logger.Logs);
			AssertEquals(senderA, consolidationReadA3.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).First().Organisation);
			AssertEquals("456", consolidationReadA3.Bookings.First().AdditionalReferenceNumbers.Cast<Integration.Customs.ICusEntryNumber>().First(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber).CE_EntryNum);

			shipment2.DataContext.SetCompanyAndDataProviderDetails(tempCompanyB);
			var readerB = new DtbBookingConsolidationDataObjectReader(shipment2, Logger, ufactory);
			var consolidationReadB = readerB.ReadIntoBusinessObject();
			AssertNotNull("Should create a consolidation.", consolidationReadB);
			AssertNotEquals("Should create the consolidation.", consolidationReadA3, consolidationReadB);
			AssertEquals("Should create the booking.", 1, consolidationReadB.Bookings.Count);
			AssertContains("Should have logged search for External Booking Reference numbers", "Information - Searching for Booking Consolidation, attempting to match External Transport Booking numbers to Transport Booking number(s) 456 from sending system and match Address to Sending Party SENDB", Logger.Logs);
			AssertEquals(senderB, consolidationReadB.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).First().Organisation);
			AssertEquals("456", consolidationReadB.Bookings.First().AdditionalReferenceNumbers.Cast<Integration.Customs.ICusEntryNumber>().First(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber).CE_EntryNum);
		}

		public void TestReadScheduleIntoBooking()
		{
			var ufactory = new UniversalObjectFactory();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, "123");
			var tempCompanyA = ufactory.New<GlbCompany>();
			shipment.DataContext.SetCompanyAndDataProviderDetails(tempCompanyA);

			var legs = new DataObjectList<TransportLeg>()
						{
								new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
								{
										PortOfLoading = new UNLOCO { Code = "AUSYD" },
										PortOfDischarge = new UNLOCO { Code = "NZAKL" },
										VoyageFlightNo = "999"
								},
								new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
								{
										PortOfLoading = new UNLOCO { Code = "NZAKL" },
										PortOfDischarge = new UNLOCO { Code = "USSFO" },
										VoyageFlightNo = "002"
								}
						};

			shipment.SetTransportLegCollection(() => legs);
			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			var bookingRead = reader.ReadIntoBusinessObject();
			var collection = ufactory.BOFactory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, bookingRead.PK));

			AssertContainsExactElementsInAnyOrder(new[]
			{
								"AUSYD|NZAKL|999", "NZAKL|USSFO|002"
						},
			FormatTransports(collection));

			Logger.OutboundSessionTracker = new DataWritingManager(new ActionInfo(null, bookingRead));  // to have IsInternalImport() return true (and !IsInternalImport() return false in DtbBookingDataObjectReader) 
			Logger.TopLevelDataObject = shipment;
			reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			bookingRead = reader.ReadIntoBusinessObject();
			collection = ufactory.BOFactory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, bookingRead.PK));

			AssertEquals("Should not copy legs", 0, collection.Length);
		}

		public void TestReadScheduleIntoBooking_TB2TB_FromConsolidatedBookingWithBookings()
		{
			var ufactory = new UniversalObjectFactory();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBookingConsolidation, "123");
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, "456");
			var tempCompanyA = ufactory.New<GlbCompany>();
			shipment.DataContext.SetCompanyAndDataProviderDetails(tempCompanyA);

			// add TransportBooking as SubShipment
			var subBookingSource = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subBookingSource.DataContext = DataContextFactory.New();
			subBookingSource.DataContext.AddDataSource(DataContextType.TransportBooking, "456");
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subBookingSource });

			var legs = new DataObjectList<TransportLeg>()
						{
								new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
								{
										PortOfLoading = new UNLOCO { Code = "AUSYD" },
										PortOfDischarge = new UNLOCO { Code = "NZAKL" },
										VoyageFlightNo = "999"
								},
								new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
								{
										PortOfLoading = new UNLOCO { Code = "NZAKL" },
										PortOfDischarge = new UNLOCO { Code = "USSFO" },
										VoyageFlightNo = "002"
								}
						};

			shipment.SetTransportLegCollection(() => legs);
			var reader = new DtbBookingConsolidationDataObjectReader(shipment, Logger, ufactory);
			var bookingRead = reader.ReadIntoBusinessObject();
			var collection = ufactory.BOFactory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, bookingRead.PK));

			AssertContainsExactElementsInAnyOrder("Should read from Consol DO not Booking Source.", new[]
			{
								"AUSYD|NZAKL|999", "NZAKL|USSFO|002"
			},
			FormatTransports(collection));
		}

		string[] FormatTransports(IEnumerable<ITransport> collection)
		{
			return collection
					.Cast<ITransport>()
					.Select(t => string.Format("{0}|{1}|{2}", t.JW_RL_NKLoadPort, t.JW_RL_NKDiscPort, t.JW_VoyageFlight))
					.ToArray();
		}

		public void TestCheckValidInstructionAndPackageData()
		{
			var validInstructions = new DataObjectList<Instruction> { new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Sequence = 1 } };
			var containerDataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT1234567", ContainerType = new ContainerType { Code = "20GP" } };
			var containers = new DataObjectList<Container> { containerDataObject };
			var packageDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackType = new PackageType { Code = "BOX" } };
			var packingLines = new DataObjectList<PackingLine> { packageDataObject };

			TestCheckValidInstructionAndPackageDataCore(validInstructions, null, null, expectedError: true);
			TestCheckValidInstructionAndPackageDataCore(null, containers, null, expectedError: true);
			TestCheckValidInstructionAndPackageDataCore(null, null, packingLines, expectedError: true);
			TestCheckValidInstructionAndPackageDataCore(null, null, null, expectedError: false);
			TestCheckValidInstructionAndPackageDataCore(validInstructions, containers, packingLines, expectedError: false);

			TestCheckValidInstructionAndPackageData_BookingOnlyCore(validInstructions, null, null, expectedError: true);
			TestCheckValidInstructionAndPackageData_BookingOnlyCore(null, containers, null, expectedError: true);
			TestCheckValidInstructionAndPackageData_BookingOnlyCore(null, null, packingLines, expectedError: true);
			TestCheckValidInstructionAndPackageData_BookingOnlyCore(null, null, null, expectedError: false);
			TestCheckValidInstructionAndPackageData_BookingOnlyCore(validInstructions, containers, packingLines, expectedError: false);
		}

		void TestCheckValidInstructionAndPackageDataCore(DataObjectList<Instruction> instructions, DataObjectList<Container> containers, DataObjectList<PackingLine> packingLines, bool expectedError)
		{
			var bookingDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDO.DataContext = DataContextFactory.New();
			bookingDO.DataContext.AddDataSource(DataContextType.TransportBooking, "TB001");
			bookingDO.SetInstructionCollection(() => instructions);

			var consolidationDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDO.DataContext = DataContextFactory.New();
			consolidationDO.DataContext.AddDataSource(DataContextType.TransportBookingConsolidation, "CM001");
			consolidationDO.DataContext.AddDataSource(DataContextType.TransportBooking, "TB001");
			consolidationDO.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingDO });
			consolidationDO.SetContainerCollection(() => containers);
			consolidationDO.SetPackingLineCollection(() => packingLines);
			Logger.TopLevelDataObject = consolidationDO;

			var booking = Helper.CreateBooking();
			var reader = new DtbBookingConsolidationDataObjectReader(consolidationDO, Logger, new UniversalObjectFactory(), booking);
			if (expectedError)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Should blow up",
				"Cannot import Transport Booking, ensure <ContainerCollection>/<PackingLineCollection> and <InstructionCollection> are specified.",
				() => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("Should not blow up", () => reader.ReadIntoBusinessObject());
			}
		}

		void TestCheckValidInstructionAndPackageData_BookingOnlyCore(DataObjectList<Instruction> instructions, DataObjectList<Container> containers, DataObjectList<PackingLine> packingLines, bool expectedError)
		{
			var bookingDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDO.DataContext = DataContextFactory.New();
			bookingDO.DataContext.AddDataSource(DataContextType.TransportBooking, "TB001");
			bookingDO.SetInstructionCollection(() => instructions);
			bookingDO.SetContainerCollection(() => containers);
			bookingDO.SetPackingLineCollection(() => packingLines);

			var booking = Helper.CreateBooking();
			var reader = new DtbBookingConsolidationDataObjectReader(bookingDO, Logger, new UniversalObjectFactory(), booking);
			if (expectedError)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Should blow up",
				"Cannot import Transport Booking, ensure <ContainerCollection>/<PackingLineCollection> and <InstructionCollection> are specified.",
				() => reader.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("Should not blow up", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestForwardingShipmentIncludesAddresses_Namespace2012()
		{
			var cnr = Helper.CreateOrganisation("CNRORG");
			var cyd = Helper.CreateOrganisation("CYDORG");
			var cfs = Helper.CreateOrganisation("CFSORG");
			var cto = Helper.CreateOrganisation("CTOORG");

			var shipment = Helper.CreateForwardingShipment("S00001234", "HB31278903", "SEA", "FCL");
			Helper.SetupForwardingShipmentAddresses(shipment, cnr, null, cfs, null);
			var consol = Helper.CreateForwardingConsol(shipment, "C00001432", "SEA", "MB234890232");
			Helper.SetupForwardingConsolAddresses(consol, cto, null, cyd, null, null, null);
			var container = Helper.CreateForwardingContainer(consol, "CONT1", "20GP");
			Helper.CreateForwardingPackline(shipment, container, 10);

			Factory.Save();

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var parentBO = (BusinessObject)shipment;
				var parentManager = parentBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var parentWriter = parentManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO)));
				var topLevelDO = (UniversalShipment)parentWriter.GetDataObject(parentBO);

				var options = new TransportBookingDocumentOptions((IDtbBookingParent)parentBO, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", true);
				var writer = new DtbBookingParentDataObjectWriter(options, topLevelDO, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO)));
				var dataObject = writer.GetDataObject(parentBO);

				var dataContext = DataContextFactory.New();
				dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
				Logger.TopLevelDataObject = new UniversalShipment { DataContext = dataContext };

				OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
				var reader = new DtbBookingConsolidationDataObjectReader(dataObject, Logger, new UniversalObjectFactory());
				var consolidationRead = reader.ReadIntoBusinessObject();
				var booking = consolidationRead.Bookings[0];
				AssertEquals("EFPL", booking.KM_KT_NKBookingTemplate);
				AssertEquals(4, booking.Instructions.Count);

				var instructions = booking.Instructions.OrderBy(i => i.KN_Sequence).ToArray();
				AssertEquals(cnr.PK, instructions[0].Address.Organisation.PK);
				AssertEquals(cyd.PK, instructions[1].Address.Organisation.PK);
				AssertEquals(cfs.PK, instructions[2].Address.Organisation.PK);
				AssertEquals(cto.PK, instructions[3].Address.Organisation.PK);
			}
		}

		public void TestForwardingShipmentParentJobShouldFlowThroughToCreatedTransportBooking()
		{
			const string testShipmentNumber = "S00001234";
			var shipment = Helper.CreateForwardingShipment(testShipmentNumber, "HB31278903", "SEA", "FCL");
			var shipmentAsJobHeaderParent = (IJobHeaderParent)shipment;
			var shipmentJob = new JobHeader.Loader(shipmentAsJobHeaderParent).TryLoadOrCreate();
			shipmentJob.Parent = shipmentAsJobHeaderParent;

			Factory.Save();

			using (eAdaptorRegistry.Instance.UniversalXMLAlwaysIncludeJobCostingInUniversalShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var parentBO = (BusinessObject)shipment;
				var parentManager = parentBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var parentWriter = parentManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO)));
				var topLevelDO = (UniversalShipment)parentWriter.GetDataObject(parentBO);

				var options = new TransportBookingDocumentOptions((IDtbBookingParent)parentBO, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", true);
				var writer = new DtbBookingParentDataObjectWriter(options, topLevelDO, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, parentBO)));
				var dataObject = writer.GetDataObject(parentBO);

				var dataContext = DataContextFactory.New();
				dataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.CodesMappedToTarget = true;
				Logger.TopLevelDataObject = new UniversalShipment { DataContext = dataContext };
				Logger.OutboundSessionTracker = new DataWritingManager(new ActionInfo(null, parentBO));

				OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
				var reader = new DtbBookingConsolidationDataObjectReader(dataObject, Logger, new UniversalObjectFactory());
				var consolidationRead = reader.ReadIntoBusinessObject();
				var booking = consolidationRead.Bookings[0];
				AssertEquals("Job on new booking should be linked to the shipment", testShipmentNumber, booking.Job.JH_JobNum);
			}
		}

		public void TestConsolWithContainerisedCoLoadMasterShipmentShouldSuccessfullyCreateTransportBookingWithoutFailingOnDuplicatePackLine()
		{
			var consolXml = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.ConsolWithCoLoadMasterShipmentContainerisedExport.xml");
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var xmlReader = ObjectFactory.Get<IXmlReader>();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(consolXml)))
			{
				xmlReader.ReadXML(dataObject, stream, Logger);
			}

			var reader = new DtbBookingConsolidationDataObjectReader(dataObject, Logger, new UniversalObjectFactory());
			DtbBookingConsolidation consolidationRead = null;
			AssertNoExceptionThrown(() => { consolidationRead = reader.ReadIntoBusinessObject(); });
			AssertNotNull(consolidationRead);
		}

		public void TestConsolWithContainerisedAssemblyMasterShipmentShouldSuccessfullyCreateTransportBookingWithoutFailingOnDuplicatePackLine()
		{
			var consolXml = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.ConsolWithAssemblyMasterShipmentContainerisedExport.xml");
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var xmlReader = ObjectFactory.Get<IXmlReader>();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(consolXml)))
			{
				xmlReader.ReadXML(dataObject, stream, Logger);
			}

			var reader = new DtbBookingConsolidationDataObjectReader(dataObject, Logger, new UniversalObjectFactory());
			DtbBookingConsolidation consolidationRead = null;
			AssertNoExceptionThrown(() => { consolidationRead = reader.ReadIntoBusinessObject(); });
			AssertNotNull(consolidationRead);
		}

		public void TestConsolWithContainerisedBlindCoLoadMasterShipmentShouldSuccessfullyCreateTransportBookingWithoutFailingOnDuplicatePackLine()
		{
			var consolXml = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.ConsolWithBlindCoLoadMasterShipmentContainerisedExport.xml");
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var xmlReader = ObjectFactory.Get<IXmlReader>();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(consolXml)))
			{
				xmlReader.ReadXML(dataObject, stream, Logger);
			}

			var reader = new DtbBookingConsolidationDataObjectReader(dataObject, Logger, new UniversalObjectFactory());
			DtbBookingConsolidation consolidationRead = null;
			AssertNoExceptionThrown(() => { consolidationRead = reader.ReadIntoBusinessObject(); });
			AssertNotNull(consolidationRead);
		}

		public void TestPopulateBusinessObject_JobTypeChanged_HasBookings_ThrowsException()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			Helper.CreateBooking(consolidation);

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var universalFactory = new UniversalObjectFactory();
			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidation;
			AssertExceptionThrown(typeof(DataObjectReadFailureException), DtbBookingConsolidationSchema.Constants.KB_JobType + " cannot be changed from BTC if the Consolidation already has Bookings.", () => topLevelReader.ReadIntoBusinessObject(ref con));
		}

		public void TestPopulateBusinessObject_JobTypeNotChanged_DoesNotThrowException()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			Helper.CreateBooking(consolidation);

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var universalFactory = new UniversalObjectFactory();
			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidation;
			AssertNoExceptionThrown(() => topLevelReader.ReadIntoBusinessObject(ref con));
		}

		public void TestPopulateBusinessObject_JobTypeChanged_NoBookings_DoesNotThrowException()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var universalFactory = new UniversalObjectFactory();
			var topLevelReader = (ITopLevelDataObjectReader)new DtbBookingConsolidationDataObjectReader(consol, Logger, universalFactory);
			var con = (BusinessObject)consolidation;
			AssertNoExceptionThrown(() => topLevelReader.ReadIntoBusinessObject(ref con));
		}

		public void TestFindExistingBusinessObjectMatchExternalTransportBookingNumbersOnJobIDForTestCba()
		{
			var externalConsolidationJobId = "CM87654321";
			var externalBookingJobId = "TB87654321";
			var bookingJobId = "TB12345678";
			Helper.MapOrganisation(EdiOrgCode);
			var consolidation = SimulateExistingConsolidationForTestReturnMatching(bookingJobId);

			Factory.Save();
			var consolidationJobId = consolidation.KB_JobID;

			var bookingConsolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingConsolidationDataObject.DataContext = DataContextFactory.New();
			bookingConsolidationDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			bookingConsolidationDataObject.DataContext.AddDataSource(DataContextType.TransportBookingConsolidation, externalConsolidationJobId);
			bookingConsolidationDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, externalBookingJobId);
			bookingConsolidationDataObject.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole() { Code = RecipientRoleType.BKP } };
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.DataContext = DataContextFactory.New();
			bookingDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, externalBookingJobId);
			bookingDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			bookingDataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber }, ReferenceNumber = bookingJobId });

			bookingConsolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			bookingConsolidationDataObject.SubShipmentCollection.Add(bookingDataObject);

			var message = Factory.New<XmlEDIMessage>();
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = EdiOrgCode;
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = bookingConsolidationDataObject;

			var reader = new DtbBookingConsolidationDataObjectReader(bookingConsolidationDataObject, Logger, new UniversalObjectFactory());
			DtbBookingConsolidation consolidationRead = null;
			using (TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EdiOrgCode))
			{
				consolidationRead = reader.ReadIntoBusinessObject();
			}
			AssertEquals("Consolidation Reader should have found existing consolidation", consolidationJobId, consolidationRead.KB_JobID);
			AssertContains("Expect informational log on search for external booking number from sending system", FormattableString.Invariant($"Information - Searching for Booking Consolidation, attempting to match Transport Booking Job ID to External Transport Booking number(s) {bookingJobId} from sending system"), Logger.Logs);
		}

		public void TestPopulateNotes_AllowsSupportedNote()
		{
			var consolidationObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var autoRatingAuditLog = PredefinedNoteTypes.Instance.AutoRatingAuditLog;
			AssertEquals("Precondition: Predefined note type has to be read only after add for this test", true, autoRatingAuditLog.IsReadOnlyAfterAdd);

			consolidationObject.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note
				{
					Description = autoRatingAuditLog.Description,
					NoteText = "234",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.PUB), Description = "Public" },
					NoteContext = new NoteContext() { Code = "123", Description = "123" },
					IsCustomDescription = false
				}
			});

			var reader = new DtbBookingConsolidationDataObjectReader(consolidationObject, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			var notes = consolidationRead.Notes.GetAllNotes();
			AssertEquals("Should allow note which matches a supported type", 1, notes.Count);
			var noteBO = (StmNote)notes.First();

			CombineAssertions(delegate
			{
				AssertEquals("noteBO.ST_Description", "AutoRating Log", noteBO.ST_Description);
				AssertEquals("noteBO.ST_NoteDataAsText", "234", noteBO.ST_NoteDataAsText);
				AssertEquals("noteBO.ST_NoteContext", "123", noteBO.ST_NoteContext);
				AssertEquals("noteBO.ST_NoteType", "PUB", noteBO.ST_NoteType);
				AssertEquals("noteBO.ST_IsCustomDescription", false, noteBO.ST_IsCustomDescription);
			});
		}

		public void TestPopulateNotes_AllowsNonReadOnlyNote()
		{
			var consolidationObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var awbRatelineOvertypedNotes = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes;
			AssertEquals("Precondition: Predefined note type has to be read only after add for this test", false, awbRatelineOvertypedNotes.IsReadOnlyAfterAdd);

			consolidationObject.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note
				{
					Description = awbRatelineOvertypedNotes.Description,
					NoteText = "234",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.PUB), Description = "Public" },
					NoteContext = new NoteContext() { Code = "123", Description = "123" },
					IsCustomDescription = false
				}
			});

			var reader = new DtbBookingConsolidationDataObjectReader(consolidationObject, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			var notes = consolidationRead.Notes.GetAllNotes();
			AssertEquals("Should allow note which is not read only", 1, notes.Count);
			var noteBO = (StmNote)notes.First();

			CombineAssertions(delegate
			{
				AssertEquals("noteBO.ST_Description", "AWB Rateline Overtyped Notes", noteBO.ST_Description);
				AssertEquals("noteBO.ST_NoteDataAsText", "234", noteBO.ST_NoteDataAsText);
				AssertEquals("noteBO.ST_NoteContext", "123", noteBO.ST_NoteContext);
				AssertEquals("noteBO.ST_NoteType", "PUB", noteBO.ST_NoteType);
				AssertEquals("noteBO.ST_IsCustomDescription", true, noteBO.ST_IsCustomDescription);
			});
		}

		public void TestPopulateNotes_ExcludesReadOnlyUnsupportedNote()
		{
			var consolidationObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var airsValidationResults = PredefinedNoteTypes.Instance.AIRSValidationResults;
			AssertEquals("Precondition: Predefined note type has to be read only after add for this test", true, airsValidationResults.IsReadOnlyAfterAdd);

			consolidationObject.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note
				{
					Description = airsValidationResults.Description,
					NoteText = "234",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.PUB), Description = "Public" },
					NoteContext = new NoteContext() { Code = "123", Description = "123" },
					IsCustomDescription = false
				}
			});

			var reader = new DtbBookingConsolidationDataObjectReader(consolidationObject, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();

			var notes = consolidationRead.Notes.GetAllNotes();
			AssertEquals("Should exclude read only unsupported notes", 0, notes.Count);
		}

		public void TestPopulateNotes_ExcludesCountryRulesValidationNote()
		{
			var consolidationObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			consolidationObject.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note
				{
					Description = PredefinedNoteTypes.Instance.CountryRules.Description,
					NoteText = "234",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.PUB), Description = "Public" },
					NoteContext = new NoteContext() { Code = "123", Description = "123" },
					IsCustomDescription = false
				},
				new Note
				{
					Description = PredefinedNoteTypes.Instance.CountryRulesInternal.Description,
					NoteText = "235",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.INT), Description = "Internal" },
					NoteContext = new NoteContext() { Code = "124", Description = "124" },
					IsCustomDescription = false
				},
				new Note
				{
					Description = PredefinedNoteTypes.Instance.CountryRulesValidation.Description,
					NoteText = "236",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.INT), Description = "Internal" },
					NoteContext = new NoteContext() { Code = "125", Description = "125" },
					IsCustomDescription = false
				}
			});

			var reader = new DtbBookingConsolidationDataObjectReader(consolidationObject, Logger, new UniversalObjectFactory());
			var consolidationRead = reader.ReadIntoBusinessObject();
			var notes = consolidationRead.Notes.GetAllNotes().Cast<StmNote>();

			AssertEquals("Precondition: Confirm that Country Rules is not read only after add, and therefore ok to populate the TB", false, PredefinedNoteTypes.Instance.CountryRules.IsReadOnlyAfterAdd);
			var countryRulesNote = notes.Single(n => n.ST_Description == PredefinedNoteTypes.Instance.CountryRules.Description);
			CombineAssertions("Country Rules note is populated", delegate
			{
				AssertEquals("countryRulesNote.ST_Description", "Country Rules", countryRulesNote.ST_Description);
				AssertEquals("countryRulesNote.ST_NoteDataAsText", "234", countryRulesNote.ST_NoteDataAsText);
				AssertEquals("countryRulesNote.ST_NoteContext", "123", countryRulesNote.ST_NoteContext);
				AssertEquals("countryRulesNote.ST_NoteType", "PUB", countryRulesNote.ST_NoteType);
				AssertEquals("countryRulesNote.ST_IsCustomDescription", true, countryRulesNote.ST_IsCustomDescription);
			});

			AssertEquals("Precondition: Confirm that Country Rules Internal is not read only after add, and therefore ok to populate the TB", false, PredefinedNoteTypes.Instance.CountryRulesInternal.IsReadOnlyAfterAdd);
			var countryRulesInternalNote = notes.Single(n => n.ST_Description == PredefinedNoteTypes.Instance.CountryRulesInternal.Description);
			CombineAssertions("Country Rules Internal note is populated", delegate
			{
				AssertEquals("countryRulesInternalNote.ST_Description", "Country Rules Internal", countryRulesInternalNote.ST_Description);
				AssertEquals("countryRulesInternalNote.ST_NoteDataAsText", "235", countryRulesInternalNote.ST_NoteDataAsText);
				AssertEquals("countryRulesInternalNote.ST_NoteContext", "124", countryRulesInternalNote.ST_NoteContext);
				AssertEquals("countryRulesInternalNote.ST_NoteType", "INT", countryRulesInternalNote.ST_NoteType);
				AssertEquals("countryRulesInternalNote.ST_IsCustomDescription", true, countryRulesInternalNote.ST_IsCustomDescription);
			});

			AssertEquals("Precondition: Confirm that Country Rules Validation is read only after add, and therefore not ok to populate the TB", true, PredefinedNoteTypes.Instance.CountryRulesValidation.IsReadOnlyAfterAdd);
			AssertEquals("Country Rules Validation note is not populated", 0, notes.Count(n => n.ST_Description == PredefinedNoteTypes.Instance.CountryRulesValidation.Description));
			AssertEquals("Should not have any notes other than Country Rules and Country Rules Internal", 2, notes.Count());
		}

		public void TestParentPackingLineCollection()
		{
			var consolXml = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.DispatchConsignmentWithParentPackingLineCollection.xml");
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var xmlReader = ObjectFactory.Get<IXmlReader>();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(consolXml)))
			{
				xmlReader.ReadXML(dataObject, stream, Logger);
			}

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(dataObject, Logger, universalFactory);
			DtbBookingConsolidation consolidationRead = null;
			AssertNoExceptionThrown(() => { consolidationRead = reader.ReadIntoBusinessObject(); });
			AssertNotNull(consolidationRead);

			var booking = consolidationRead.Bookings[0];
			AssertEquals("Should have 4 outer packages: parent packing lines 0, 1, 2, plus a child packing line that doesn't have a parent", 4, booking.PackageJob.Packages.Count);
			var expectedDictionaryOfReferenceNumbers = new Dictionary<ZString, ZString[]>()
			{
				["MWN-22"] = new ZString[] { "27014" },
				["HU80"] = new ZString[] { "27006", "27005", "27016", "27010", "27017", "27013", "27012", "27004", "27018", "27007", "27019", "27009", "27015", "27008" },
				["27000HU1"] = new ZString[] { "27002", "27001", "27003", "27000" },
				["27011"] = Array.Empty<ZString>(), // Member of PackingLineCollection that didn't have a ParentPackingLineLink
			};
			var actualDictionaryOfReferenceNumbers = new Dictionary<ZString, ZString[]>();
			CombineAssertions(() =>
			{
				booking.PackageJob.Packages.ForEach(p =>
				{
					actualDictionaryOfReferenceNumbers[p.KP_PackageID] = p.Packages.Select(pp => pp.KP_PackageID).ToArray();
					AssertEquals("All children should lack grandchildren", true, p.Packages.All(pp => pp.Packages.Count == 0));
					AssertContainsExactElementsInAnyOrder("Should group packages correctly", expectedDictionaryOfReferenceNumbers[p.KP_PackageID], actualDictionaryOfReferenceNumbers[p.KP_PackageID]);
				});
			});
		}

		public void TestPackagesPopulateUNDGsFromOrderLines()
		{
			var consolXml = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.WarehouseOrderWithPackageUNDGsOnOrderLines.xml");
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var xmlReader = ObjectFactory.Get<IXmlReader>();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(consolXml)))
			{
				xmlReader.ReadXML(dataObject, stream, Logger);
			}

			var universalFactory = new UniversalObjectFactory();
			var reader = new DtbBookingConsolidationDataObjectReader(dataObject, Logger, universalFactory);
			DtbBookingConsolidation consolidationRead = null;
			AssertNoExceptionThrown(() => { consolidationRead = reader.ReadIntoBusinessObject(); });
			AssertNotNull(consolidationRead);

			var booking = consolidationRead.Bookings[0];
			AssertEquals("Should have 3 outer packages: parent packing lines 0, 1 and 2", 3, booking.PackageJob.Packages.Count);
			var packageUNDG1 = booking.PackageJob.Packages.Single(p => p.KP_PackageQty == 1).UNDGDataItems.Single();
			CombineAssertions("First package should have expected UNDG values", () =>
			{
				AssertEquals(true, packageUNDG1.DI_IsLimitedQuantity);
				AssertEquals(1, packageUNDG1.DI_PackageCount);
				AssertEquals((ZDecimal)1.000, packageUNDG1.DI_RadioactiveTransportIndex);
				AssertEquals("2.1", packageUNDG1.DI_IMOClass);
			});
			var packageUNDG2 = booking.PackageJob.Packages.Single(p => p.KP_PackageQty == 2).UNDGDataItems.Single();
			CombineAssertions("Second package should have expected UNDG values", () =>
			{
				AssertEquals(false, packageUNDG2.DI_IsLimitedQuantity);
				AssertEquals(0, packageUNDG2.DI_PackageCount);
				AssertEquals((ZDecimal)0.000, packageUNDG2.DI_RadioactiveTransportIndex);
				AssertEquals("2.1", packageUNDG2.DI_IMOClass);
			});
			AssertContainsExactElementsInExactOrder("Third package should not contain any UNDGs", Array.Empty<Integration.IUNDGDataItem>(), booking.PackageJob.Packages.Single(p => p.KP_PackageQty == 3).UNDGDataItems.ToArray());
		}

		class TestCaseHelper : TestCaseWithFactoryAndMessagingHelpers
		{
			public static IEDIMessage GetQueuedUniversalShipmentMessageFromDataObject(UniversalShipment dataObject)
			{
				return new TestCaseHelper().GetQueuedUniversalShipmentMessage(dataObject);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new TestErrorLogger();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		TestErrorLogger Logger;
		const string EdiOrgCode = "EDICRY";
	}
}
