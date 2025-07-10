using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	public class DtbParentInfoTest : DtbBookingTestCaseWithFactory
	{
		public void TestParentWithWorkflow()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertNull(parentInfo.ParentWithWorkflow);

			var parent = Factory.New<DummyWithDtbBooking>();
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals(parent, parentInfo.ParentWithWorkflow);
		}

		public void TestParentBoNotCastableToIDtbBookingParent()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyBusinessObject);
			var consolidation = Helper.CreateConsolidation();
			var parent = Factory.New<DummyBusinessObject>();
			consolidation.KB_ParentID = parent.PK;
			consolidation.KB_ParentTableCode = parent.TablePrefix;
			Assert("Precondition - if the parent is not castable , it should not throw exception.", !typeof(IDtbBookingParent).IsAssignableFrom(parent.GetType()));

			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertNoExceptionThrown("If ParentBO is null it should not throw exception.", () => { var poke = parentInfo.ParentWithWorkflow; });
			AssertNoExceptionThrown("If SourceDO is null it should not throw exception.", () => { var poke = parentInfo.ClientServiceLevel; });
		}

		public void TestLoad()
		{
			var shipment = Helper.CreateForwardingShipment("YES", "", "", "");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);
			IDtbParentInfoLoader parentInfoLoader = new DtbParentInfoLoader();
			AssertEquals(typeof(DtbParentInfo), parentInfoLoader.Load(consolidation).GetType());
		}

		public void TestLoad_HiddenParent()
		{
			var booking = Helper.CreateBooking();
			var agentBooking = DtbAgentBooking.NewFromBooking(booking);
			IDtbParentInfoLoader parentInfoLoader = new DtbParentInfoLoader();
			AssertNull("Agent Booking has 'HiddenBookingParentAttribute' so the Loader treats it as if the Booking was Standalone.", parentInfoLoader.Load(booking.ConsolidationSingleJob));
		}

		public void TestSupportsDirectSailing()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals(false, parentInfo.SupportsDirectSailing);

			var builder = ObjectFactory.New<Freight.Integration.QuotedBooking.IQuotedBookingBuilder>();
			var quotedBooking = (BusinessObject)builder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			consolidation.KB_ParentID = quotedBooking.PK;
			consolidation.KB_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals(true, parentInfo.SupportsDirectSailing);
		}

		public void TestPK()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals(ZGuid.Empty, parentInfo.PK);

			var parent = Factory.New<DummyWithDtbBooking>();
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals(parent.PK, parentInfo.PK);
		}

		public void TestTablePrefix()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals("", parentInfo.TablePrefix);

			var parent = Factory.New<DummyWithDtbBooking>();
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals(parent.TablePrefix, parentInfo.TablePrefix);
		}

		public void TestFactory()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals(consolidation.Factory, parentInfo.Factory);
		}

		public void TestHumanReadableName()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals("", parentInfo.HumanReadableName);

			var parent = Factory.New<DummyWithDtbBooking>();
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals(parent.HumanReadableName, parentInfo.HumanReadableName);
		}

		public void TestIsInDatabase()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals(false, parentInfo.IsInDatabase);

			var parent = Factory.New<DummyWithDtbBooking>();
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals(false, parentInfo.IsInDatabase);

			Factory.Save();
			AssertEquals(true, parentInfo.IsInDatabase);
		}

		public void TestControllerID()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals(null, parentInfo.ControllerID);

			var parent = Factory.New<DummyWithDtbBooking>();
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals(DummyControllerIDs.Dummy, parentInfo.ControllerID);
		}

		// direct

		public void TestInvoicingJob()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals(null, parentInfo.InvoicingJob);

			var parent = Factory.New<DummyWithDtbBooking>();
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals(parent, parentInfo.InvoicingJob);
		}

		public void TestJobNumber()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals("", parentInfo.JobNumber);

			var parent = Factory.New<DummyWithDtbBooking>();
			parent.Z0_Description = "D00000123";
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals("D00000123", parentInfo.JobNumber);
		}

		public void TestJobDescription()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals("", parentInfo.JobDescription);

			var parent = Factory.New<DummyWithDtbBooking>();
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals("Dummy", parentInfo.JobDescription);
		}

		public void TestJobType()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals("", parentInfo.JobType);

			var parent = Factory.New<DummyWithDtbBooking>();
			consolidation = Helper.CreateConsolidation(parent);
			parentInfo = new DtbParentInfo(consolidation);
			AssertEquals("DUM", parentInfo.JobType);
		}

		// bus

		public void TestClientServiceLevel()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertEquals("", parentInfo.ClientServiceLevel);

			var shipment = GetNewShipment();
			shipment.ServiceLevel = new ServiceLevel() { Code = "123" };
			parentInfo = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment));
			AssertEquals("123", parentInfo.ClientServiceLevel);
		}

		public void TestTransportMode()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfoWithoutShipment = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertNull(parentInfoWithoutShipment.TransportMode);

			var emptyShipment = GetNewShipment();
			var parentInfoForEmptyShipment = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(emptyShipment));
			AssertNull(parentInfoForEmptyShipment.TransportMode);

			var shipmentWithoutTransportMode = GetNewShipment();
			shipmentWithoutTransportMode.TransportMode = new CodeDescriptionPair() { };
			var parentInfoForShipmentWithoutTransportMode = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipmentWithoutTransportMode));
			AssertNull(parentInfoForShipmentWithoutTransportMode.TransportMode);

			var shipmentWithTransportMode = GetNewShipment();
			shipmentWithTransportMode.TransportMode = new CodeDescriptionPair() { Code = "123" };
			var parentInfoForShipmentWithTransportMode = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipmentWithTransportMode));
			AssertEquals("123", parentInfoForShipmentWithTransportMode.TransportMode);
		}

		public void TestContainerMode()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfoWithoutShipment = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertNull(parentInfoWithoutShipment.ContainerMode);

			var emptyShipment = GetNewShipment();
			var parentInfoForEmptyShipment = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(emptyShipment));
			AssertNull(parentInfoForEmptyShipment.ContainerMode);

			var shipmentWithoutContainerMode = GetNewShipment();
			shipmentWithoutContainerMode.ContainerMode = new ContainerMode() { };
			var parentInfoForShipmentWithoutContainerMode = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipmentWithoutContainerMode));
			AssertNull(parentInfoForShipmentWithoutContainerMode.ContainerMode);

			var shipmentWithContainerMode = GetNewShipment();
			shipmentWithContainerMode.ContainerMode = new ContainerMode() { Code = "123" };
			var parentInfoForShipmentWithContainerMode = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipmentWithContainerMode));
			AssertEquals("123", parentInfoForShipmentWithContainerMode.ContainerMode);
		}

		public void TestCarrierServiceLevel()
		{
			var consolidation = Helper.CreateConsolidation();
			var parentInfoWithoutShipment = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertNull(parentInfoWithoutShipment.CarrierServiceLevel);

			var emptyShipment = GetNewShipment();
			var parentInfoForEmptyShipment = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(emptyShipment));
			AssertNull(parentInfoForEmptyShipment.CarrierServiceLevel);

			var shipmentWithoutSerivceLevel = GetNewShipment();
			shipmentWithoutSerivceLevel.CarrierServiceLevel = new ServiceLevel() { };
			var parentInfoForShipmentWithoutSerivceLevel = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipmentWithoutSerivceLevel));
			AssertNull(parentInfoForShipmentWithoutSerivceLevel.CarrierServiceLevel);

			var shipmentWithServiceLevel = GetNewShipment();
			shipmentWithServiceLevel.CarrierServiceLevel = new ServiceLevel() { Code = "123" };
			var parentInfoForShipmentWithServiceLevel = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipmentWithServiceLevel));
			AssertEquals("123", parentInfoForShipmentWithServiceLevel.CarrierServiceLevel);
		}

		public void TestTransportReference()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var parentInfoWithoutShipment = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertNull(parentInfoWithoutShipment.TransportReference);

			var emptyShipment = GetNewShipment();
			var parentInfoForEmptyShipment = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(emptyShipment));
			AssertNull(parentInfoForEmptyShipment.TransportReference);

			var shipmentWithoutTransportReference = GetNewShipment();
			shipmentWithoutTransportReference.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			var parentInfoForShipmentWithoutTransportReference = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipmentWithoutTransportReference));
			AssertNull(parentInfoForShipmentWithoutTransportReference.TransportReference);

			var shipmentWithTransportReference = GetNewShipment();
			shipmentWithTransportReference.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentWithTransportReference.LocalProcessing.ArrivalCartageRef = "123";
			var parentInfoForShipmentWithTransportReference = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipmentWithTransportReference));
			AssertEquals("123", parentInfoForShipmentWithTransportReference.TransportReference);
		}

		public void TestDropMode()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation);
			AssertNull(parentInfo.DropMode);

			var shipment = GetNewShipment();
			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			parentInfo = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment));
			AssertNull(parentInfo.DropMode);

			shipment.Order = new Order { DropMode = new DropMode { Code = "999" } };
			AssertEquals("999", parentInfo.DropMode);

			shipment.LocalProcessing.FCLPickupEquipmentNeeded = new CodeDescriptionPair() { Code = "123" };
			parentInfo = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment));
			AssertEquals("123", parentInfo.DropMode);

			shipment.LocalProcessing.FCLDeliveryEquipmentNeeded = new CodeDescriptionPair() { Code = "456" };
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			AssertEquals("456", parentInfo.DropMode);

			shipment.LocalTransportEquipmentNeeded = new CodeDescriptionPair() { Code = "789" };
			AssertEquals("789", parentInfo.DropMode);

			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			AssertEquals("789", parentInfo.DropMode);
		}

		public void TestUpdateAddress()
		{
			var consolidation = Helper.CreateConsolidation();
			var shipment = GetNewShipment();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "Hello";

			var writeManager = new DataWritingManager(new ActionInfo(null, consolidation));

			shipment.AddOrgAddress(writeManager, orgAddress, DocAddressType.LocalCartageCFS);

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DocAddressType = DocAddressType.LocalCartageCFS;
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment));
			parentInfo.UpdateAddress(docAddress, true, null, null);
			AssertEquals("Hello", docAddress.E2_CompanyName);
		}

		public void TestUpdateAddress_WhsReceiveCnrPopulatesFromSupplier()
		{
			var consolidation = Helper.CreateConsolidation();
			var shipment = GetNewShipment();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "Hello";

			var writeManager = new DataWritingManager(new ActionInfo(null, consolidation));

			shipment.AddOrgAddress(writeManager, orgAddress, DocAddressType.SupplierDocumentaryAddress);

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DocAddressType = DocAddressType.LocalCartageExporter;
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment));
			parentInfo.UpdateAddress(docAddress, true, null, null);
			AssertEquals("Hello", docAddress.E2_CompanyName);
		}

		public void TestUpdateAddress_LocalCartageYard_BookingHasContainerLinksAndNumbers()
		{
			var consolidation = Helper.CreateConsolidation();
			var shipment = GetNewShipment();
			var pickupEmpty1 = CreateOrganizationAddress(DocAddressType.ContainerYardEmptyPickupAddress, "Pickup 1");
			var deliveryEmpty1 = CreateOrganizationAddress(DocAddressType.ContainerYardEmptyReturnAddress, "Delivery 1");
			var pickupEmpty2 = CreateOrganizationAddress(DocAddressType.ContainerYardEmptyPickupAddress, "Pickup 2");
			var deliveryEmpty2 = CreateOrganizationAddress(DocAddressType.ContainerYardEmptyReturnAddress, "Delivery 2");
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(CreateContainer(pickupEmpty1, deliveryEmpty1, 1, "CN1"));
			shipment.ContainerCollection.Add(CreateContainer(pickupEmpty2, deliveryEmpty2, 2, "CN2"));

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DocAddressType = DocAddressType.LocalCartageYard;
			var parentInfo = (IDtbParentInfo)new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment));
			parentInfo.UpdateAddress(docAddress, true, new List<ZInt>() { 2 }, new List<ZString>() { "CN2" });
			AssertEquals("Should select correct address according to list of container links/numbers passed to UpdateAddress()", "Delivery 2", docAddress.E2_CompanyName);
		}

		Tuple<IDtbParentInfo, PkgPackage, Dictionary<PkgPackage, ZString>> TestGetDatesAndReferences_HasConsolidation_Arrange(ZString containerNumber, ZString containerReleaseNumber, ZString pkgContainerReleaseNumber)
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_JobDirection = ConfirmationTypes.Codes.PickUp;

			var container = new Container
			{
				ContainerNumber = containerNumber,
				ReleaseNum = containerReleaseNumber
			};

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>() { container });

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>() { container });

			IDtbParentInfo parentInfo = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment, consol));

			var pkgContainer = Helper.CreatePackageContainer(containerNumber);

			var releaseNumbersByPackage = new Dictionary<PkgPackage, ZString>
			{
				{ pkgContainer, pkgContainerReleaseNumber }
			};

			return Tuple.Create(parentInfo, pkgContainer, releaseNumbersByPackage);
		}

		Tuple<IDtbParentInfo, PkgPackage, Dictionary<PkgPackage, ZString>> TestGetDatesAndReferences_GetConsolidation_Arrange(ZString containerNumber, ZString pkgContainerReleaseNumber, string jobDirection)
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_JobDirection = jobDirection;

			IDtbParentInfo parentInfo = new DtbParentInfo(consolidation, new GetConsolidationParentDataObjectStrategy(consolidation));

			var pkgContainer = Helper.CreatePackageContainer(containerNumber);

			var releaseNumbersByPackage = new Dictionary<PkgPackage, ZString>
			{
				{ pkgContainer, pkgContainerReleaseNumber }
			};

			return Tuple.Create(parentInfo, pkgContainer, releaseNumbersByPackage);
		}

		DatesAndReference TestGetDatesAndReferences_Act(IDtbParentInfo parentInfo, PkgPackage pkgContainer, Dictionary<PkgPackage, ZString> releaseNumbersByPackage)
		{
			return parentInfo.GetDatesAndReferences(ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer, releaseNumbersByPackage);
		}

		DatesAndReference TestGetDatesAndReferences_HasConsolidation(ZString containerNumber, ZString containerReleaseNumber, ZString pkgContainerReleaseNumber)
		{
			var (parentInfo, pkgContainer, releaseNumbersByPackage) = TestGetDatesAndReferences_HasConsolidation_Arrange(containerNumber, containerReleaseNumber, pkgContainerReleaseNumber);

			return TestGetDatesAndReferences_Act(parentInfo, pkgContainer, releaseNumbersByPackage);
		}

		DatesAndReference TestGetDatesAndReferences_GetConsolidation(ZString containerNumber, ZString pkgContainerReleaseNumber, string jobDirection = ConfirmationTypes.Codes.PickUp)
		{
			var (parentInfo, pkgContainer, releaseNumbersByPackage) = TestGetDatesAndReferences_GetConsolidation_Arrange(containerNumber, pkgContainerReleaseNumber, jobDirection);

			return TestGetDatesAndReferences_Act(parentInfo, pkgContainer, releaseNumbersByPackage);
		}

		public void TestGetDatesAndReferences_NoParent_GetConsolidationStrategy_Origin()
		{
			AssertNoExceptionThrown("Should not throw null reference exception anymore.", () => TestGetDatesAndReferences_GetConsolidation("A", "B"));
		}
		public void TestGetDatesAndReferences_NoParent_GetConsolidationStrategy_Destination()
		{
			AssertNoExceptionThrown("Should not throw null reference exception anymore.", () => TestGetDatesAndReferences_GetConsolidation("A", "B", ConfirmationTypes.Codes.Delivery));
		}

		public void TestGetDatesAndReferencesReturnsTheCorrectContainerInfo_ContainerNumberEmpty()
		{
			var releaseNumber1 = "RELEASENUM1";
			var releaseNumber2 = "RELEASENUM2";

			// ReleaseNumber matches with value from dictionary
			var result1 = TestGetDatesAndReferences_HasConsolidation(null, releaseNumber1, releaseNumber1);
			AssertEquals("Tests GetDatesAndReferences returns correct container info when container number empty, and container release number equals pkg release number",
						releaseNumber1, result1.Reference);

			// ReleaseNumber does not match with value from dictionary
			var result2 = TestGetDatesAndReferences_HasConsolidation(null, releaseNumber1, releaseNumber2);
			AssertEquals("Tests GetDatesAndReferences returns correct container info when container number empty, and container release number does not equal pkg release number",
						ZString.Empty, result2.Reference);
		}

		public void TestGetDatesAndReferencesReturnsTheCorrectContainerInfo_ContainerNumberNotEmpty()
		{
			var containerNumber = "CONTAINERNUM1";
			var releaseNumber1 = "RELEASENUM1";
			var releaseNumber2 = "RELEASENUM2";

			// ReleaseNumber matches with value from dictionary
			var result1 = TestGetDatesAndReferences_HasConsolidation(containerNumber, releaseNumber1, releaseNumber1);
			AssertNotEquals("Test GetDatesAndReferences gets correct match when container is not empty", null, result1);
			AssertEquals(releaseNumber1, result1.Reference);

			// ReleaseNumber does not match with value from dictionary
			var result2 = TestGetDatesAndReferences_HasConsolidation(containerNumber, releaseNumber1, releaseNumber2);
			AssertNotEquals("Test GetDatesAndReferences gets correct match when container is not empty", null, result2);
			AssertEquals(releaseNumber1, result2.Reference);
		}

		public void TestGetDatesAndReferences_PIC()
		{
			var day = ZDateTime.Today;
			var oneDay = new TimeSpan(1, 0, 0, 0);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM456");

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM456");

			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container); // do not add to Shipment anymore

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			var firstLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			var secondLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.TransportLegCollection.Add(firstLeg);
			consol.TransportLegCollection.Add(secondLeg);

			firstLeg.LegOrder = 0;
			secondLeg.LegOrder = 1;
			container.ContainerNumber = "CONT102938";
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			// booking packages

			var pkgContainer = Factory.New<PkgPackage>();
			var pkgLoose = Factory.New<PkgPackage>();
			pkgContainer.KP_PackageID = container.ContainerNumber.Value;
			pkgContainer.KP_F3_NKPackType = "CNT";
			pkgLoose.KP_PackageQty = packline.PackQty.Value.ToZInt();
			pkgLoose.KP_F3_NKPackType = packline.PackType.GetCodeAsUpperCase();

			// pickup

			firstLeg.DocumentCutOff = day += oneDay;
			firstLeg.LCLReceivalCommences = day += oneDay; // CFS -> Dlv -> Req. From
			firstLeg.LCLCutOff = day += oneDay; // CFS -> Dlv -> Req. To
			firstLeg.FCLReceivalCommences = day += oneDay; // CTO -> Dlv -> Req. From
			firstLeg.FCLCutOff = day += oneDay; // CTO -> Dlv -> Req. To
			firstLeg.EstimatedDeparture = day += oneDay;
			firstLeg.EstimatedArrival = day += oneDay;
			firstLeg.FCLAvailability = day += oneDay;
			firstLeg.FCLStorage = day += oneDay;
			firstLeg.LCLAvailability = day += oneDay;
			firstLeg.LCLStorageDate = day += oneDay;

			// delivery

			secondLeg.DocumentCutOff = day += oneDay;
			secondLeg.LCLReceivalCommences = day += oneDay;
			secondLeg.LCLCutOff = day += oneDay;
			secondLeg.FCLReceivalCommences = day += oneDay;
			secondLeg.FCLCutOff = day += oneDay;
			secondLeg.EstimatedDeparture = day += oneDay;
			secondLeg.EstimatedArrival = day += oneDay;
			secondLeg.FCLAvailability = day += oneDay;
			secondLeg.FCLStorage = day += oneDay;
			secondLeg.LCLAvailability = day += oneDay;
			secondLeg.LCLStorageDate = day += oneDay;

			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			IDtbParentInfo info = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment, consol));

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgLoose);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, firstLeg.LCLReceivalCommences.Value, firstLeg.LCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, firstLeg.FCLReceivalCommences.Value, firstLeg.FCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer);

			// pickup

			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing.EstimatedPickup = day += oneDay; // CNR -> Est
			shipment.LocalProcessing.PickupRequiredFrom = day += oneDay; // CNR -> Req. From
			shipment.LocalProcessing.PickupRequiredBy = day += oneDay; // CNR -> Req. To
			shipment.LocalProcessing.PickupCartageCompleted = day += oneDay; // CNR -> Act

			// delivery

			shipment.LocalProcessing.EstimatedDelivery = day += oneDay; // CNE -> Est
			shipment.LocalProcessing.DeliveryRequiredFrom = day += oneDay; // CNE -> Req. From
			shipment.LocalProcessing.DeliveryRequiredBy = day += oneDay; // CNE -> Req. To
			shipment.LocalProcessing.DeliveryCartageCompleted = day += oneDay; // CNE -> Act
			shipment.LocalProcessing.FCLAvailable = day += oneDay; // CTO -> Req. From
			shipment.LocalProcessing.FCLStorageCommences = day += oneDay; // CTO -> Req. To
			shipment.LocalProcessing.LCLAvailable = day += oneDay; // CFS -> Req. To
			shipment.LocalProcessing.LCLStorageCommences = day += oneDay; // CFS -> Req. To

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgLoose, shipment.LocalProcessing.EstimatedPickup.Value, shipment.LocalProcessing.PickupCartageCompleted.Value, shipment.LocalProcessing.PickupRequiredFrom.Value, shipment.LocalProcessing.PickupRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, firstLeg.LCLReceivalCommences.Value, firstLeg.LCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgContainer, shipment.LocalProcessing.EstimatedPickup.Value, shipment.LocalProcessing.PickupCartageCompleted.Value, shipment.LocalProcessing.PickupRequiredFrom.Value, shipment.LocalProcessing.PickupRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, firstLeg.FCLReceivalCommences.Value, firstLeg.FCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer);

			// pickup

			container.ReleaseNum = "EmptyRelease123"; //CYD -> Est
			container.ContainerParkEmptyPickupGateOut = day += oneDay; // CYD -> Pic -> Act

			container.EmptyRequired = day += oneDay; // CNR or CFS -> DLV -> Req. To
			container.DepartureEstimatedPickup = day += oneDay; //CNR or CFS -> Est pic

			container.DepartureSlotReference = "DepSlot123";
			container.DepartureSlotDateTime = day += oneDay;
			container.FCLWharfGateIn = day += oneDay; // CTO -> Del -> Act

			// delivery

			container.FCLAvailable = day += oneDay;
			container.FCLStorageCommences = day += oneDay;
			container.LCLAvailable = day += oneDay;
			container.LCLStorageCommences = day += oneDay;

			container.ArrivalSlotReference = "ArvSlot123";
			container.ArrivalSlotDateTime = day += oneDay;

			container.ContainerImportDORelease = "ContImpDORel123";
			container.FCLWharfGateOut = day += oneDay; // CTO -> pic -> Act
			container.ArrivalEstimatedDelivery = day += oneDay; // CFS or CNE -> pic -> Act
			container.EmptyReadyForReturn = day += oneDay; // CFS or CNE -> Pic -> Req From
			container.EmptyReturnedBy = day += oneDay; // CYD -> Del -> Req. To
			container.ContainerParkEmptyReturnGateIn = day += oneDay; // CYD -> Del -> Act

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgLoose, shipment.LocalProcessing.EstimatedPickup.Value, shipment.LocalProcessing.PickupCartageCompleted.Value, shipment.LocalProcessing.PickupRequiredFrom.Value, shipment.LocalProcessing.PickupRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, firstLeg.LCLReceivalCommences.Value, firstLeg.LCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer, ZDateTime.Empty, container.ContainerParkEmptyPickupGateOut.Value, ZDateTime.Empty, container.EmptyRequired.Value, container.ReleaseNum.Value, ZDateTime.Empty, ZString.Empty);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, container.EmptyRequired.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgContainer, container.DepartureEstimatedPickup.Value, shipment.LocalProcessing.PickupCartageCompleted.Value, shipment.LocalProcessing.PickupRequiredFrom.Value, shipment.LocalProcessing.PickupRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer, ZDateTime.Empty, container.FCLWharfGateIn.Value, firstLeg.FCLReceivalCommences.Value, firstLeg.FCLCutOff.Value, container.ReleaseNum.Value, container.DepartureSlotDateTime.Value, container.DepartureSlotReference.Value);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer);
		}

		public void TestGetDatesAndReferences_DLV()
		{
			var day = ZDateTime.Today;
			var oneDay = new TimeSpan(1, 0, 0, 0);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM456");

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM456");

			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container); // do not add to Shipment anymore

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			var firstLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			var secondLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.TransportLegCollection.Add(firstLeg);
			consol.TransportLegCollection.Add(secondLeg);

			firstLeg.LegOrder = 0;
			secondLeg.LegOrder = 1;
			container.ContainerNumber = "CONT102938";
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			// booking packages

			var pkgContainer = Factory.New<PkgPackage>();
			var pkgLoose = Factory.New<PkgPackage>();
			pkgContainer.KP_PackageID = container.ContainerNumber.Value;
			pkgContainer.KP_F3_NKPackType = "CNT";
			pkgLoose.KP_PackageQty = packline.PackQty.Value.ToZInt();
			pkgLoose.KP_F3_NKPackType = packline.PackType.GetCodeAsUpperCase();

			// pickup

			firstLeg.DocumentCutOff = day += oneDay;
			firstLeg.LCLReceivalCommences = day += oneDay; // CFS -> Dlv -> Req. From
			firstLeg.LCLCutOff = day += oneDay; // CFS -> Dlv -> Req. To
			firstLeg.FCLReceivalCommences = day += oneDay; // CTO -> Dlv -> Req. From
			firstLeg.FCLCutOff = day += oneDay; // CTO -> Dlv -> Req. To
			firstLeg.EstimatedDeparture = day += oneDay;
			firstLeg.EstimatedArrival = day += oneDay;
			firstLeg.FCLAvailability = day += oneDay;
			firstLeg.FCLStorage = day += oneDay;
			firstLeg.LCLAvailability = day += oneDay;
			firstLeg.LCLStorageDate = day += oneDay;

			// delivery

			secondLeg.DocumentCutOff = day += oneDay;
			secondLeg.LCLReceivalCommences = day += oneDay;
			secondLeg.LCLCutOff = day += oneDay;
			secondLeg.FCLReceivalCommences = day += oneDay;
			secondLeg.FCLCutOff = day += oneDay;
			secondLeg.EstimatedDeparture = day += oneDay;
			secondLeg.EstimatedArrival = day += oneDay;
			secondLeg.FCLAvailability = day += oneDay;
			secondLeg.FCLStorage = day += oneDay;
			secondLeg.LCLAvailability = day += oneDay;
			secondLeg.LCLStorageDate = day += oneDay;

			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			IDtbParentInfo info = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment, consol));

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, secondLeg.LCLAvailability.Value, secondLeg.LCLStorageDate.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, secondLeg.FCLAvailability.Value, secondLeg.FCLStorage.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);

			// pickup

			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing.EstimatedPickup = day += oneDay; // CNR -> Est
			shipment.LocalProcessing.PickupRequiredFrom = day += oneDay; // CNR -> Req. From
			shipment.LocalProcessing.PickupRequiredBy = day += oneDay; // CNR -> Req. To
			shipment.LocalProcessing.PickupCartageCompleted = day += oneDay; // CNR -> Act

			// delivery

			shipment.LocalProcessing.EstimatedDelivery = day += oneDay; // CNE -> Est
			shipment.LocalProcessing.DeliveryRequiredFrom = day += oneDay; // CNE -> Req. From
			shipment.LocalProcessing.DeliveryRequiredBy = day += oneDay; // CNE -> Req. To
			shipment.LocalProcessing.DeliveryCartageCompleted = day += oneDay; // CNE -> Act
			shipment.LocalProcessing.FCLAvailable = day += oneDay; // CTO -> Req. From
			shipment.LocalProcessing.FCLStorageCommences = day += oneDay; // CTO -> Req. To
			shipment.LocalProcessing.LCLAvailable = day += oneDay; // CFS -> Req. To
			shipment.LocalProcessing.LCLStorageCommences = day += oneDay; // CFS -> Req. To

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, shipment.LocalProcessing.LCLAvailable.Value, shipment.LocalProcessing.LCLStorageCommences.Value, "", ZDateTime.Empty, ZString.Empty); // shipment overrides transport dates

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgLoose, shipment.LocalProcessing.EstimatedDelivery.Value, shipment.LocalProcessing.DeliveryCartageCompleted.Value, shipment.LocalProcessing.DeliveryRequiredFrom.Value, shipment.LocalProcessing.DeliveryRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, shipment.LocalProcessing.FCLAvailable.Value, shipment.LocalProcessing.FCLStorageCommences.Value, "", ZDateTime.Empty, ZString.Empty); // shipment overrides transport dates

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgContainer, shipment.LocalProcessing.EstimatedDelivery.Value, shipment.LocalProcessing.DeliveryCartageCompleted.Value, shipment.LocalProcessing.DeliveryRequiredFrom.Value, shipment.LocalProcessing.DeliveryRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);

			// pickup

			container.ReleaseNum = "EmptyRelease123"; //CYD -> Est
			container.ContainerParkEmptyPickupGateOut = day += oneDay; // CYD -> Pic -> Act

			container.EmptyRequired = day += oneDay; // CNR or CFS -> DLV -> Req. To
			container.DepartureEstimatedPickup = day += oneDay; //CNR or CFS -> Est pic

			container.DepartureSlotReference = "DepSlot123";
			container.DepartureSlotDateTime = day += oneDay;
			container.FCLWharfGateIn = day += oneDay; // CTO -> Del -> Act

			// delivery

			container.FCLAvailable = day += oneDay; // cto req from
			container.FCLStorageCommences = day += oneDay; // cto req to
			container.LCLAvailable = day += oneDay;
			container.LCLStorageCommences = day += oneDay;

			container.ReleaseNum = "ReleaseNum890"; // cto slot act
			container.ArrivalSlotReference = "ArvSlot123"; // cto slot act
			container.ArrivalSlotDateTime = day += oneDay; // cto slot ref

			container.ContainerImportDORelease = "ContImpDORel123"; // cto ref
			container.FCLWharfGateOut = day += oneDay; // CTO -> pic -> Act
			container.ArrivalEstimatedDelivery = day += oneDay; // CFS or CNE -> pic -> Act
			container.EmptyReadyForReturn = day += oneDay; // CFS or CNE -> Pic -> Req From
			container.EmptyReturnedBy = container.EmptyReadyForReturn += oneDay; // CYD -> Del -> Req. To
			container.EmptyReturnRef = "EmptyReturnRef123";
			container.ContainerParkEmptyReturnGateIn = day += oneDay; // CYD -> Del -> Act

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, shipment.LocalProcessing.LCLAvailable.Value, shipment.LocalProcessing.LCLStorageCommences.Value, "", ZDateTime.Empty, ZString.Empty); // shipment overrides transport dates

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgLoose, shipment.LocalProcessing.EstimatedDelivery.Value, shipment.LocalProcessing.DeliveryCartageCompleted.Value, shipment.LocalProcessing.DeliveryRequiredFrom.Value, shipment.LocalProcessing.DeliveryRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer, ZDateTime.Empty, container.FCLWharfGateOut.Value, container.FCLAvailable.Value, container.FCLStorageCommences.Value, container.ContainerImportDORelease.Value, container.ArrivalSlotDateTime.Value, container.ArrivalSlotReference.Value); // container overrides shipment/transport dates

			container.ArrivalSlotReference = null; // Fall back to ContainerImportDORelease
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer, ZDateTime.Empty, container.FCLWharfGateOut.Value, container.FCLAvailable.Value, container.FCLStorageCommences.Value, container.ContainerImportDORelease.Value, container.ArrivalSlotDateTime.Value, ZString.Empty); // container overrides shipment/transport dates

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgContainer, container.ArrivalEstimatedDelivery.Value, shipment.LocalProcessing.DeliveryCartageCompleted.Value, shipment.LocalProcessing.DeliveryRequiredFrom.Value, shipment.LocalProcessing.DeliveryRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgContainer, ZDate.Empty, ZDateTime.Empty, container.EmptyReadyForReturn.Value, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer, ZDate.Empty, container.ContainerParkEmptyReturnGateIn.Value, ZDate.Empty, container.EmptyReturnedBy.Value, container.EmptyReturnRef.Value, ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);
		}

		public void TestGetDatesAndReferences_PIC_ContainersOnShipmentOnly()
		{
			var day = ZDateTime.Today;
			var oneDay = new TimeSpan(1, 0, 0, 0);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM456");

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM456");

			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(container); // only add to the shipment for this test (agency)

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			var firstLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			var secondLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.TransportLegCollection.Add(firstLeg);
			consol.TransportLegCollection.Add(secondLeg);

			firstLeg.LegOrder = 0;
			secondLeg.LegOrder = 1;
			container.ContainerNumber = "CONT102938";
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			// booking packages

			var pkgContainer = Factory.New<PkgPackage>();
			var pkgLoose = Factory.New<PkgPackage>();
			pkgContainer.KP_PackageID = container.ContainerNumber.Value;
			pkgContainer.KP_F3_NKPackType = "CNT";
			pkgLoose.KP_PackageQty = packline.PackQty.Value.ToZInt();
			pkgLoose.KP_F3_NKPackType = packline.PackType.GetCodeAsUpperCase();

			// pickup

			firstLeg.DocumentCutOff = day += oneDay;
			firstLeg.LCLReceivalCommences = day += oneDay; // CFS -> Dlv -> Req. From
			firstLeg.LCLCutOff = day += oneDay; // CFS -> Dlv -> Req. To
			firstLeg.FCLReceivalCommences = day += oneDay; // CTO -> Dlv -> Req. From
			firstLeg.FCLCutOff = day += oneDay; // CTO -> Dlv -> Req. To
			firstLeg.EstimatedDeparture = day += oneDay;
			firstLeg.EstimatedArrival = day += oneDay;
			firstLeg.FCLAvailability = day += oneDay;
			firstLeg.FCLStorage = day += oneDay;
			firstLeg.LCLAvailability = day += oneDay;
			firstLeg.LCLStorageDate = day += oneDay;

			// delivery

			secondLeg.DocumentCutOff = day += oneDay;
			secondLeg.LCLReceivalCommences = day += oneDay;
			secondLeg.LCLCutOff = day += oneDay;
			secondLeg.FCLReceivalCommences = day += oneDay;
			secondLeg.FCLCutOff = day += oneDay;
			secondLeg.EstimatedDeparture = day += oneDay;
			secondLeg.EstimatedArrival = day += oneDay;
			secondLeg.FCLAvailability = day += oneDay;
			secondLeg.FCLStorage = day += oneDay;
			secondLeg.LCLAvailability = day += oneDay;
			secondLeg.LCLStorageDate = day += oneDay;

			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			IDtbParentInfo info = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment, consol));

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgLoose);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, firstLeg.LCLReceivalCommences.Value, firstLeg.LCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, firstLeg.FCLReceivalCommences.Value, firstLeg.FCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer);

			// pickup

			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing.EstimatedPickup = day += oneDay; // CNR -> Est
			shipment.LocalProcessing.PickupRequiredFrom = day += oneDay; // CNR -> Req. From
			shipment.LocalProcessing.PickupRequiredBy = day += oneDay; // CNR -> Req. To
			shipment.LocalProcessing.PickupCartageCompleted = day += oneDay; // CNR -> Act

			// delivery

			shipment.LocalProcessing.EstimatedDelivery = day += oneDay; // CNE -> Est
			shipment.LocalProcessing.DeliveryRequiredFrom = day += oneDay; // CNE -> Req. From
			shipment.LocalProcessing.DeliveryRequiredBy = day += oneDay; // CNE -> Req. To
			shipment.LocalProcessing.DeliveryCartageCompleted = day += oneDay; // CNE -> Act
			shipment.LocalProcessing.FCLAvailable = day += oneDay; // CTO -> Req. From
			shipment.LocalProcessing.FCLStorageCommences = day += oneDay; // CTO -> Req. To
			shipment.LocalProcessing.LCLAvailable = day += oneDay; // CFS -> Req. To
			shipment.LocalProcessing.LCLStorageCommences = day += oneDay; // CFS -> Req. To

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgLoose, shipment.LocalProcessing.EstimatedPickup.Value, shipment.LocalProcessing.PickupCartageCompleted.Value, shipment.LocalProcessing.PickupRequiredFrom.Value, shipment.LocalProcessing.PickupRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, firstLeg.LCLReceivalCommences.Value, firstLeg.LCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgContainer, shipment.LocalProcessing.EstimatedPickup.Value, shipment.LocalProcessing.PickupCartageCompleted.Value, shipment.LocalProcessing.PickupRequiredFrom.Value, shipment.LocalProcessing.PickupRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, firstLeg.FCLReceivalCommences.Value, firstLeg.FCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer);

			// pickup

			container.ReleaseNum = "EmptyRelease123"; //CYD -> Est
			container.ContainerParkEmptyPickupGateOut = day += oneDay; // CYD -> Pic -> Act

			container.EmptyRequired = day += oneDay; // CNR or CFS -> DLV -> Req. To
			container.DepartureEstimatedPickup = day += oneDay; //CNR or CFS -> Est pic

			container.DepartureSlotReference = "DepSlot123";
			container.DepartureSlotDateTime = day += oneDay;
			container.FCLWharfGateIn = day += oneDay; // CTO -> Del -> Act

			// delivery

			container.FCLAvailable = day += oneDay;
			container.FCLStorageCommences = day += oneDay;
			container.LCLAvailable = day += oneDay;
			container.LCLStorageCommences = day += oneDay;

			container.ArrivalSlotReference = "ArvSlot123";
			container.ArrivalSlotDateTime = day += oneDay;

			container.ContainerImportDORelease = "ContImpDORel123";
			container.FCLWharfGateOut = day += oneDay; // CTO -> pic -> Act
			container.ArrivalEstimatedDelivery = day += oneDay; // CFS or CNE -> pic -> Act
			container.EmptyReadyForReturn = day += oneDay; // CFS or CNE -> Pic -> Req From
			container.EmptyReturnedBy = day += oneDay; // CYD -> Del -> Req. To
			container.ContainerParkEmptyReturnGateIn = day += oneDay; // CYD -> Del -> Act

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgLoose, shipment.LocalProcessing.EstimatedPickup.Value, shipment.LocalProcessing.PickupCartageCompleted.Value, shipment.LocalProcessing.PickupRequiredFrom.Value, shipment.LocalProcessing.PickupRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, firstLeg.LCLReceivalCommences.Value, firstLeg.LCLCutOff.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer, ZDateTime.Empty, container.ContainerParkEmptyPickupGateOut.Value, ZDateTime.Empty, container.EmptyRequired.Value, container.ReleaseNum.Value, ZDateTime.Empty, ZString.Empty);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNR", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, container.EmptyRequired.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNR", pkgContainer, container.DepartureEstimatedPickup.Value, shipment.LocalProcessing.PickupCartageCompleted.Value, shipment.LocalProcessing.PickupRequiredFrom.Value, shipment.LocalProcessing.PickupRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer, ZDateTime.Empty, container.FCLWharfGateIn.Value, firstLeg.FCLReceivalCommences.Value, firstLeg.FCLCutOff.Value, container.ReleaseNum.Value, container.DepartureSlotDateTime.Value, container.DepartureSlotReference.Value);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer);
		}

		public void TestGetDatesAndReferences_DLV_ContainersOnShipmentOnly()
		{
			var day = ZDateTime.Today;
			var oneDay = new TimeSpan(1, 0, 0, 0);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");
			consol.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM456");

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM456");

			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.SubShipmentCollection.Add(shipment);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(container); // only add to the shipment for this test (agency)

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipment.PackingLineCollection.Add(packline);

			var firstLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			var secondLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			consol.TransportLegCollection.Add(firstLeg);
			consol.TransportLegCollection.Add(secondLeg);

			firstLeg.LegOrder = 0;
			secondLeg.LegOrder = 1;
			container.ContainerNumber = "CONT102938";
			packline.PackQty = 20;
			packline.PackType = new PackageType() { Code = "PLT" };

			// booking packages

			var pkgContainer = Factory.New<PkgPackage>();
			var pkgLoose = Factory.New<PkgPackage>();
			pkgContainer.KP_PackageID = container.ContainerNumber.Value;
			pkgContainer.KP_F3_NKPackType = "CNT";
			pkgLoose.KP_PackageQty = packline.PackQty.Value.ToZInt();
			pkgLoose.KP_F3_NKPackType = packline.PackType.GetCodeAsUpperCase();

			// pickup

			firstLeg.DocumentCutOff = day += oneDay;
			firstLeg.LCLReceivalCommences = day += oneDay; // CFS -> Dlv -> Req. From
			firstLeg.LCLCutOff = day += oneDay; // CFS -> Dlv -> Req. To
			firstLeg.FCLReceivalCommences = day += oneDay; // CTO -> Dlv -> Req. From
			firstLeg.FCLCutOff = day += oneDay; // CTO -> Dlv -> Req. To
			firstLeg.EstimatedDeparture = day += oneDay;
			firstLeg.EstimatedArrival = day += oneDay;
			firstLeg.FCLAvailability = day += oneDay;
			firstLeg.FCLStorage = day += oneDay;
			firstLeg.LCLAvailability = day += oneDay;
			firstLeg.LCLStorageDate = day += oneDay;

			// delivery

			secondLeg.DocumentCutOff = day += oneDay;
			secondLeg.LCLReceivalCommences = day += oneDay;
			secondLeg.LCLCutOff = day += oneDay;
			secondLeg.FCLReceivalCommences = day += oneDay;
			secondLeg.FCLCutOff = day += oneDay;
			secondLeg.EstimatedDeparture = day += oneDay;
			secondLeg.EstimatedArrival = day += oneDay;
			secondLeg.FCLAvailability = day += oneDay;
			secondLeg.FCLStorage = day += oneDay;
			secondLeg.LCLAvailability = day += oneDay;
			secondLeg.LCLStorageDate = day += oneDay;

			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			IDtbParentInfo info = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment, consol));

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, secondLeg.LCLAvailability.Value, secondLeg.LCLStorageDate.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, secondLeg.FCLAvailability.Value, secondLeg.FCLStorage.Value, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);

			// pickup

			shipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing.EstimatedPickup = day += oneDay; // CNR -> Est
			shipment.LocalProcessing.PickupRequiredFrom = day += oneDay; // CNR -> Req. From
			shipment.LocalProcessing.PickupRequiredBy = day += oneDay; // CNR -> Req. To
			shipment.LocalProcessing.PickupCartageCompleted = day += oneDay; // CNR -> Act

			// delivery

			shipment.LocalProcessing.EstimatedDelivery = day += oneDay; // CNE -> Est
			shipment.LocalProcessing.DeliveryRequiredFrom = day += oneDay; // CNE -> Req. From
			shipment.LocalProcessing.DeliveryRequiredBy = day += oneDay; // CNE -> Req. To
			shipment.LocalProcessing.DeliveryCartageCompleted = day += oneDay; // CNE -> Act
			shipment.LocalProcessing.FCLAvailable = day += oneDay; // CTO -> Req. From
			shipment.LocalProcessing.FCLStorageCommences = day += oneDay; // CTO -> Req. To
			shipment.LocalProcessing.LCLAvailable = day += oneDay; // CFS -> Req. To
			shipment.LocalProcessing.LCLStorageCommences = day += oneDay; // CFS -> Req. To

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, shipment.LocalProcessing.LCLAvailable.Value, shipment.LocalProcessing.LCLStorageCommences.Value, "", ZDateTime.Empty, ZString.Empty); // shipment overrides transport dates

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgLoose, shipment.LocalProcessing.EstimatedDelivery.Value, shipment.LocalProcessing.DeliveryCartageCompleted.Value, shipment.LocalProcessing.DeliveryRequiredFrom.Value, shipment.LocalProcessing.DeliveryRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer, ZDateTime.Empty, ZDateTime.Empty, shipment.LocalProcessing.FCLAvailable.Value, shipment.LocalProcessing.FCLStorageCommences.Value, "", ZDateTime.Empty, ZString.Empty); // shipment overrides transport dates

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgContainer, shipment.LocalProcessing.EstimatedDelivery.Value, shipment.LocalProcessing.DeliveryCartageCompleted.Value, shipment.LocalProcessing.DeliveryRequiredFrom.Value, shipment.LocalProcessing.DeliveryRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgContainer);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);

			// pickup

			container.ReleaseNum = "EmptyRelease123"; //CYD -> Est
			container.ContainerParkEmptyPickupGateOut = day += oneDay; // CYD -> Pic -> Act

			container.EmptyRequired = day += oneDay; // CNR or CFS -> DLV -> Req. To
			container.DepartureEstimatedPickup = day += oneDay; //CNR or CFS -> Est pic

			container.DepartureSlotReference = "DepSlot123";
			container.DepartureSlotDateTime = day += oneDay;
			container.FCLWharfGateIn = day += oneDay; // CTO -> Del -> Act

			// delivery

			container.FCLAvailable = day += oneDay; // cto req from
			container.FCLStorageCommences = day += oneDay; // cto req to
			container.LCLAvailable = day += oneDay;
			container.LCLStorageCommences = day += oneDay;

			container.ArrivalSlotReference = "ArvSlot123"; // cto slot act
			container.ArrivalSlotDateTime = day += oneDay; // cto slot ref

			container.ContainerImportDORelease = "ContImpDORel123"; // cto ref
			container.FCLWharfGateOut = day += oneDay; // CTO -> pic -> Act
			container.ArrivalEstimatedDelivery = day += oneDay; // CFS or CNE -> pic -> Act
			container.EmptyReadyForReturn = day += oneDay; // CFS or CNE -> Pic -> Req From
			container.EmptyReturnedBy = day += oneDay; // CYD -> Del -> Req. To
			container.EmptyReturnRef = "EmptyReturnRef123";
			container.ContainerParkEmptyReturnGateIn = day += oneDay; // CYD -> Del -> Act

			// Delivery / Pickup for each Org Type with Loose Package

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgLoose);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgLoose, ZDateTime.Empty, ZDateTime.Empty, shipment.LocalProcessing.LCLAvailable.Value, shipment.LocalProcessing.LCLStorageCommences.Value, "", ZDateTime.Empty, ZString.Empty); // shipment overrides transport dates

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgLoose, shipment.LocalProcessing.EstimatedDelivery.Value, shipment.LocalProcessing.DeliveryCartageCompleted.Value, shipment.LocalProcessing.DeliveryRequiredFrom.Value, shipment.LocalProcessing.DeliveryRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgLoose);

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgLoose);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgLoose);

			// Delivery / Pickup for each Org Type with the Container

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CTO", pkgContainer);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer, ZDateTime.Empty, container.FCLWharfGateOut.Value, container.FCLAvailable.Value, container.FCLStorageCommences.Value, container.ContainerImportDORelease.Value, container.ArrivalSlotDateTime.Value, container.ArrivalSlotReference.Value); // container overrides shipment/transport dates

			container.ArrivalSlotReference = null;
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CTO", pkgContainer, ZDateTime.Empty, container.FCLWharfGateOut.Value, container.FCLAvailable.Value, container.FCLStorageCommences.Value, container.ContainerImportDORelease.Value, container.ArrivalSlotDateTime.Value, ZString.Empty); // container overrides shipment/transport dates

			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.Delivery, "CFS", pkgContainer);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CFS", pkgContainer);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CNE", pkgContainer, container.ArrivalEstimatedDelivery.Value, shipment.LocalProcessing.DeliveryCartageCompleted.Value, shipment.LocalProcessing.DeliveryRequiredFrom.Value, shipment.LocalProcessing.DeliveryRequiredBy.Value, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmation(info, ConfirmationTypes.Codes.PickUp, "CNE", pkgContainer, ZDate.Empty, ZDateTime.Empty, container.EmptyReadyForReturn.Value, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);

			AssertConfirmation(info, ConfirmationTypes.Codes.Delivery, "CYD", pkgContainer, ZDate.Empty, container.ContainerParkEmptyReturnGateIn.Value, ZDate.Empty, container.EmptyReturnedBy.Value, container.EmptyReturnRef.Value, ZDateTime.Empty, ZString.Empty);
			AssertConfirmationEmpty(info, ConfirmationTypes.Codes.PickUp, "CYD", pkgContainer);
		}

		public void TestGetDatesAndReferencesFromOrderDateCollection()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");

			var eTD = ZDateTime.Today;
			var eTA = ZDateTime.Today.AddDays(1);
			var actualArrival = ZDateTime.Today.AddDays(2);

			var order = shipment.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance);
			order.SetDateCollection(() => new List<Date>());
			order.DateCollection.Add(new Date { IsEstimate = true, Type = DateType.Departure, Value = eTD });
			order.DateCollection.Add(new Date { IsEstimate = true, Type = DateType.Arrival, Value = eTA });
			order.DateCollection.Add(new Date { IsEstimate = false, Type = DateType.Arrival, Value = actualArrival });

			var consolidation1 = Helper.CreateConsolidation();
			consolidation1.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var info1 = (IDtbParentInfo)new DtbParentInfo(consolidation1, new HasConsolidationParentDataObjectStrategy(shipment));

			AssertConfirmation(info1, ConfirmationTypes.Codes.PickUp, "CNR", null, eTD, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmation(info1, ConfirmationTypes.Codes.Delivery, "WHS", null, eTA, actualArrival, ZDateTime.Empty, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);

			var consolidation2 = Helper.CreateConsolidation();
			consolidation2.KB_JobDirection = nameof(DtbBookingDirection.PIC);

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(container);
			shipment.ContainerMode = new ContainerMode { Code = "CNT" };
			container.ContainerNumber = "CONT102938";
			var pkgContainer = Factory.New<PkgPackage>();
			pkgContainer.KP_PackageID = container.ContainerNumber.Value;
			pkgContainer.KP_F3_NKPackType = "CNT";

			var info2 = (IDtbParentInfo)new DtbParentInfo(consolidation2, new HasConsolidationParentDataObjectStrategy(shipment));
			AssertConfirmation(info2, ConfirmationTypes.Codes.PickUp, "CNR", pkgContainer, eTD, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);
			AssertConfirmation(info2, ConfirmationTypes.Codes.Delivery, "WHS", pkgContainer, eTA, actualArrival, ZDateTime.Empty, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);
		}

		public void TestGetDatesAndReferences_NullCollections()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");

			var consolidation = Helper.CreateConsolidation();
			var container = Helper.CreatePackageContainer("CONT123");
			IDtbParentInfo info = new DtbParentInfo(consolidation, new HasConsolidationParentDataObjectStrategy(shipment));

			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			AssertNoExceptionThrown(() => info.GetDatesAndReferences("", "", container, null));

			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			AssertNoExceptionThrown(() => info.GetDatesAndReferences("", "", container, null));
		}

		void AssertConfirmationEmpty(IDtbParentInfo info, ZString confirmationTypeCode, ZString orgType, PkgPackage package)
		{
			AssertConfirmation(info, confirmationTypeCode, orgType, package, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", ZDateTime.Empty, ZString.Empty);
		}

		void AssertConfirmation(IDtbParentInfo info, ZString confirmationTypeCode, ZString orgType, PkgPackage package, ZDateTime expectEstimated, ZDateTime expectActual, ZDateTime expectReqFrom, ZDateTime expectReqTo, ZString expectReference, ZDateTime expectedSlotTime, ZString expectedSlotReference)
		{
			var dateAndReference = info.GetDatesAndReferences(confirmationTypeCode, orgType, package, null);
			CombineAssertions(() =>
			{
				AssertEquals("Estimated", expectEstimated, dateAndReference.Estimated);
				AssertEquals("Actual", expectActual, dateAndReference.Actual);
				AssertEquals("ReqFrom", expectReqFrom, dateAndReference.ReqFrom);
				AssertEquals("ReqTo", expectReqTo, dateAndReference.ReqTo);
				AssertEquals("Reference", expectReference, dateAndReference.Reference);
				AssertEquals("SlotTime", expectedSlotTime, dateAndReference.SlotTime);
				AssertEquals("SlotReference", expectedSlotReference, dateAndReference.SlotReference);
			});
		}

		// TestOnBookingConsolidationComplete

		//public void TestOnBookingConsolidationComplete_Loose()
		//{
		//    var shipment = Factory.New<ForwardingShipment>();
		//    shipment.JS_TransportMode = Constants.TransportModes.Air;
		//    shipment.JS_OuterPacks = 10;
		//    IDtbBookingDirectionInfo deliveryTransportBookingDirectionInfo = new ForwardingShipmentDeliveryTransportBooking(shipment);

		//    var now = ZDateTime.Now;
		//    AssertEquals("Precondition", ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);

		//    deliveryTransportBookingDirectionInfo.OnBookingConsolidationDeliveryComplete(now, "");
		//    AssertEquals(now, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
		//    AssertEquals(now, shipment.DeliveryConfirms[0].EU_PickupDeliveryTime);
		//    AssertEquals("", shipment.DeliveryConfirms[0].EU_GoodsSignForBy);

		//    deliveryTransportBookingDirectionInfo.OnBookingConsolidationDeliveryComplete(now, "Bob");
		//    AssertEquals(now, shipment.DeliveryConfirms[0].EU_PickupDeliveryTime);
		//    AssertEquals("Bob", shipment.DeliveryConfirms[0].EU_GoodsSignForBy);
		//}

		//public void TestOnBookingConsolidationComplete_Containerised()
		//{
		//    var shipment = Factory.New<ForwardingShipment>();
		//    shipment.JS_TransportMode = Constants.TransportModes.Sea;
		//    var consol = shipment.Consols.AddNew();
		//    var container = consol.Containers.AddNew();
		//    shipment.OuterPackLines.AddNew().SetContainer(consol, container);
		//    IDtbBookingDirectionInfo deliveryTransportBookingDirectionInfo = new ForwardingShipmentDeliveryTransportBooking(shipment);

		//    var now = ZDateTime.Now;
		//    AssertEquals("Precondition", ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);

		//    deliveryTransportBookingDirectionInfo.OnBookingConsolidationDeliveryComplete(now, "");
		//    AssertEquals(now, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
		//    AssertEquals(now, container.DestinationConfirm.EU_PickupDeliveryTime);
		//    AssertEquals("", container.DestinationConfirm.EU_GoodsSignForBy);

		//    deliveryTransportBookingDirectionInfo.OnBookingConsolidationDeliveryComplete(now, "Bob");
		//    AssertEquals(now, container.DestinationConfirm.EU_PickupDeliveryTime);
		//    AssertEquals("Bob", container.DestinationConfirm.EU_GoodsSignForBy);
		//}

		Shipment GetNewShipment()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");
			return shipment;
		}

		OrganizationAddress CreateOrganizationAddress(DocAddressType addressType, ZString companyNameOverride)
		{
			var addressTypeToUse = addressType.ToString();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = companyNameOverride;
			return new OrganizationDataObjectWriter(GetNewDataObjectWriter(), addressTypeToUse).GetDataObject(orgAddress);
		}

		Container CreateContainer(OrganizationAddress pickupEmpty, OrganizationAddress deliveryEmpty, ZInt? containerLink, ZString? containerNumber)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			container.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { pickupEmpty, deliveryEmpty });
			container.Link = containerLink;
			container.ContainerNumber = containerNumber;
			return container;
		}

		public DataWritingManager GetNewDataObjectWriter()
		{
			return new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
		}
	}
}
