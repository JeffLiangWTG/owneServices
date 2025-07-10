using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(CYDTransportationUnitDataObjectReader))]
	public class CYDTransportationUnitForGateBookingDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestReadFromDataObject()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment();
			var logger = GetLogger(shipment);

			new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			var estimatedGateInTime = shipment.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));
			Assert("Estimated gate in time should be populated", transportationUnit.YTU_EstimatedGateInTime.Equals(estimatedGateInTime));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);
		}

		public void TestReadFromDataObject_BookingSlotIsZDateTime()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment();
			var logger = GetLogger(shipment);
			var now = ZDateTime.Now;
			shipment.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value = new UXmlDateTime(now);

			new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));

			var yardRow = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var yard = Factory.BOFactory.Load<WhsWarehouse>(yardRow.GetValue(WhsWarehouseSchema.PK));
			var estimatedGateInTime = yard.GetWarehouseBranchLocalDateTimeOffset(now);
			Assert("Estimated gate in time should be populated", transportationUnit.YTU_EstimatedGateInTime.Equals(estimatedGateInTime));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);
		}

		public void TestReadFromDataObject_BookingSlotIsZDateTimeOffset()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment();
			var logger = GetLogger(shipment);
			var now = ZDateTimeOffset.Now;
			shipment.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value = new UXmlDateTime(now);

			new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));

			var yardRow = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var yard = Factory.BOFactory.Load<WhsWarehouse>(yardRow.GetValue(WhsWarehouseSchema.PK));
			var estimatedGateInTime = yard.GetWarehouseBranchDateTimeOffset(now.ToUtcDateTime());
			Assert("Estimated gate in time should be populated", transportationUnit.YTU_EstimatedGateInTime.Equals(estimatedGateInTime));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);
		}

		public void TestReadFromDataObject_DeliverContainer_WithAcceptanceNumber()
		{
			TestCaseForDeliverContainer("BKR01");
		}

		public void TestReadFromDataObject_DeliverContainer_WithoutAcceptanceNumber()
		{
			TestCaseForDeliverContainer(null);
		}

		void TestCaseForDeliverContainer(string acceptanceNumber)
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = false, BookingConfirmationReference = acceptanceNumber });
			var logger = GetLogger(shipment);
			var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);

			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			var estimatedGateInTime = shipment.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("Transportation reference should be populated.", "REG1", transportationUnit.YTU_TransportationReference);
			AssertEquals("Transportation Unit ID should be populated", "TPU00000001", transportationUnit.YTU_TransportationUnitID);
			Assert("Estimated gate in time should be populated", transportationUnit.YTU_EstimatedGateInTime.Equals(estimatedGateInTime));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

			var deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", "DI00000001", deliveryHeader.YDH_JobNumber);

			var delivery = Factory.Load<CYDDelivery>(new ZQuery()).Single();
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", delivery.YDL_TransportReference);

			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			AssertEquals("Transportation reference should be populated.", "REG1", transportationUnit.YTU_TransportationReference);
			AssertEquals("Transportation Unit ID should be populated", "TPU00000001", transportationUnit.YTU_TransportationUnitID);

			jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

			deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", "DI00000001", deliveryHeader.YDH_JobNumber);

			delivery = Factory.Load<CYDDelivery>(new ZQuery()).Single();
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", delivery.YDL_TransportReference);
		}

		public void TestReadFromDataObject_PickupContainer()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true });
			var logger = GetLogger(shipment);
			var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);

			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			var transportationUnitPK = transportationUnit.PK;
			var estimatedGateInTime = shipment.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("Transportation reference should be populated.", "REG1", transportationUnit.YTU_TransportationReference);
			AssertEquals("Transportation Unit ID should be populated", "TPU00000001", transportationUnit.YTU_TransportationUnitID);
			Assert("Estimated gate in time should be populated", transportationUnit.YTU_EstimatedGateInTime.Equals(estimatedGateInTime));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			var jobDocAddressPK = jobDocAddress.PK;
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			var universalJobLinkPK = universalJobLink.PK;
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

			var pickupHeader = Factory.Load<CYDPickupHeader>(new ZQuery()).Single();
			var pickupHeaderPK = pickupHeader.PK;
			AssertEquals("Pickup Header Job Number should be populated.", "PI00000001", pickupHeader.YPH_JobNumber);

			var pickup = Factory.Load<CYDPickup>(new ZQuery()).Single();
			var pickupPK = pickup.PK;
			AssertEquals("Pickup Transport Reference should be populated", "TREF240419151008", pickup.YPL_TransportReference);
			AssertEquals("Pickup quantity should be 1", (short)1, pickup.UnitLineItem.YLI_Quantity);
			AssertEquals("Pickup type should be populated", "CNT", pickup.UnitLineItem.YLI_Type);

			var yardUnit = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_YPL_Pickup, pickupPK)).Single();
			AssertEquals("Pickup unit number should be matched", "GENL", yardUnit.YUS_UnitID);

			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit2 = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			AssertEquals("TransportationUnit primary key should be unchanged", transportationUnitPK, transportationUnit2.PK);
			AssertEquals("Transportation reference should be unchanged.", "REG1", transportationUnit2.YTU_TransportationReference);
			AssertEquals("Transportation Unit ID should be unchanged", "TPU00000001", transportationUnit2.YTU_TransportationUnitID);

			var jobDocAddress2 = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("JobDocAddress primary key should be unchanged", jobDocAddressPK, jobDocAddress2.PK);
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress2.CompanyName);

			var universalJobLink2 = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("UniversalJobLink primary key should be unchanged", universalJobLinkPK, universalJobLink2.PK);
			AssertEquals("GateBooking", universalJobLink2.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink2.UCL_SourceKey);

			var pickupHeader2 = Factory.Load<CYDPickupHeader>(new ZQuery()).Single();
			AssertEquals("PickupHeader primary key should be unchanged", pickupHeaderPK, pickupHeader2.PK);
			AssertEquals("Pickup Header Job Number should be unchanged.", "PI00000001", pickupHeader2.YPH_JobNumber);

			var pickup2 = Factory.Load<CYDPickup>(new ZQuery()).Single();
			AssertEquals("Pickup primary key should be unchanged", pickupPK, pickup2.PK);
			AssertEquals("Pickup Transport Reference should be unchanged", "TREF240419151008", pickup2.YPL_TransportReference);
			AssertEquals("Pickup quantity should be unchanged", (short)1, pickup2.UnitLineItem.YLI_Quantity);
			AssertEquals("Pickup type should be unchanged", "CNT", pickup2.UnitLineItem.YLI_Type);

			var yardUnit2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_YPL_Pickup, pickupPK)).Single();
			AssertEquals("Pickup unit number should be matched", "GENL", yardUnit2.YUS_UnitID);
		}

		public void TestReadFromDataObject_PickupContainer_WithoutContainerNumber()
		{
			CYDTransportationUnit transportationUnit = null;
			CYDPickup pickup = null;
			{
				var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true, ContainerNumbers = [null], DataSourceKeys = ["GBM000001"] });
				var logger = GetLogger(shipment);
				var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);

				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
				var estimatedGateInTime = shipment.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
				AssertEquals("Transportation reference should be populated.", "REG1", transportationUnit.YTU_TransportationReference);
				AssertEquals("Transportation Unit ID should be populated", "TPU00000001", transportationUnit.YTU_TransportationUnitID);
				Assert("Estimated gate in time should be populated", transportationUnit.YTU_EstimatedGateInTime.Equals(estimatedGateInTime));

				var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
				AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

				var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
				AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
				AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

				var pickupHeader = Factory.Load<CYDPickupHeader>(new ZQuery()).Single();
				AssertEquals("Pickup Header Job Number should be populated.", "PI00000001", pickupHeader.YPH_JobNumber);

				var pickups = Factory.Load<CYDPickup>(new ZQuery());
				AssertEquals("1 Pickup should be populated.", 1, pickups.Length);

				pickup = pickups[0];
				AssertEquals("Pickup Transport Reference should be populated", "TREF240419151008", pickup.YPL_TransportReference);
				AssertEquals("Pickup quantity should be 1", (short)1, pickup.UnitLineItem.YLI_Quantity);
				AssertEquals("Pickup type should be populated", "CNT", pickup.UnitLineItem.YLI_Type);
				AssertEquals("20GP", Factory.Load<RefContainer>(pickup.UnitLineItem.YLI_RC_ContainerType).RC_Code);

				var yardUnit = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_YEL_ReleaseLine, pickup.YPL_YEL_ReleaseAdviceLine)).Single();
				AssertEquals("YUS_YTU_DispatchTransportationUnit should be populated when picking up an itemize line", transportationUnit.PK, yardUnit.YUS_YTU_DispatchTransportationUnit);
				AssertEquals("YUS_YPL_Pickup should be populated when picking up an itemize line", pickup.PK, yardUnit.YUS_YPL_Pickup);
			}

			{
				var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true, ContainerNumbers = [null], DataSourceKeys = ["GBM000001"] });
				var logger = GetLogger(shipment);
				var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);

				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var transportationUnitAfterReImport = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
				AssertEquals("Transportation should be unchanged.", transportationUnit.PK, transportationUnitAfterReImport.PK);
				AssertEquals("Transportation reference should be unchanged.", "REG1", transportationUnitAfterReImport.YTU_TransportationReference);
				AssertEquals("Transportation Unit ID should be unchanged", "TPU00000001", transportationUnitAfterReImport.YTU_TransportationUnitID);

				var jobDocAddress2 = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
				AssertEquals("WUFU SHIPPING LINE", jobDocAddress2.CompanyName);

				var universalJobLinkAfterReImport = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
				AssertEquals("GateBooking", universalJobLinkAfterReImport.UCL_SourceType);
				AssertEquals("GB00001234", universalJobLinkAfterReImport.UCL_SourceKey);

				var pickupsAfterReImport = Factory.Load<CYDPickup>(new ZQuery());
				AssertEquals("There should be only 1 Pickup", 1, pickupsAfterReImport.Length);
				AssertEquals("The Pickup should be unchanged", pickup.PK, pickupsAfterReImport[0].PK);
				AssertEquals("Pickup Transport Reference should be unchanged", "TREF240419151008", pickupsAfterReImport[0].YPL_TransportReference);
				AssertEquals("Pickup quantity should be unchanged", (short)1, pickupsAfterReImport[0].UnitLineItem.YLI_Quantity);
				AssertEquals("Pickup type should be unchanged", "CNT", pickupsAfterReImport[0].UnitLineItem.YLI_Type);
				AssertEquals("20GP", Factory.Load<RefContainer>(pickupsAfterReImport[0].UnitLineItem.YLI_RC_ContainerType).RC_Code);

				var yardUnitAfterReImport = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_YEL_ReleaseLine, pickupsAfterReImport[0].YPL_YEL_ReleaseAdviceLine)).Single();
				AssertEquals("YUS_YTU_DispatchTransportationUnit should be unchanged", transportationUnit.PK, yardUnitAfterReImport.YUS_YTU_DispatchTransportationUnit);
				AssertEquals("YUS_YPL_Pickup should be unchanged", pickup.PK, yardUnitAfterReImport.YUS_YPL_Pickup);
			}
		}

		public void TestReadFromDataObject_YardIsNotFound()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { YardIsNotFound = true });
			var logger = GetLogger(shipment);

			AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure", "Yard is not found.", () =>
			{
				var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestReadFromDataObject_VehicleIsNotFound()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsNeedToBuildPreCarriage = false });
			var logger = GetLogger(shipment);

			AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure", "Vehicle Registration Number is not found.", () =>
			{
				var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestReadFromDataObject_BookingSlotNotFound()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { PopulateBookingSlotTime = false });
			var logger = GetLogger(shipment);

			AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure", "Booking slot date time are missing from UXML.", () =>
			{
				var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestReadFromDataObject_GetVehicleFromVehicleRun()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsNeedToBuildPreCarriage = false, IsNeedToBuildVehicleRun = true });
			var logger = GetLogger(shipment);

			AssertNull(shipment.PreCarriageShipmentCollection);
			AssertNotNull(shipment.VehicleRun);

			AssertNoExceptionThrown(() =>
			{
				var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestReadFromDataObject_GetVehicleFromPreCarriage()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsNeedToBuildVehicleRun = false });
			var logger = GetLogger(shipment);

			AssertNull(shipment.VehicleRun);
			AssertNotNull(shipment.PreCarriageShipmentCollection);

			AssertNoExceptionThrown(() =>
			{
				var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestReadFromDataObject_BookingPartyIsNotFound()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { BookingPartyIsNotFound = true });
			var logger = GetLogger(shipment);

			AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure", "Transport Company details are missing from UXML.", () =>
			{
				var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules_LoadFromJobLinks()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment();
			var logger = GetLogger(shipment);

			new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var factoryForLoad = NewUniversalObjectFactory();
			var transportationUnit = factoryForLoad.Load<CYDTransportationUnit>(new ZQuery()).Single();
			Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));

			var universalJobLink = factoryForLoad.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals(transportationUnit.PK, universalJobLink.UCL_ParentID);

			var factoryForSave = NewUniversalObjectFactory();
			new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, factoryForSave).ReadIntoBusinessObject();
			factoryForSave.SaveForTesting();

			var factoryForVerify = NewUniversalObjectFactory();
			var matchedTPU = factoryForVerify.Load<CYDTransportationUnit>(new ZQuery()).Single();
			AssertEquals(transportationUnit.PK, matchedTPU.PK);
			Assert("Transportation reference should be load the data from job links.", matchedTPU.YTU_TransportationReference.Equals("REG1"));

			var universalLinkInNewFactory = factoryForVerify.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals(universalJobLink.PK, universalLinkInNewFactory.PK);
		}

		public void TestReadFromDataObject_TransportReferenceIsNotFound()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { TransportReferenceIsNotFound = true });
			var logger = GetLogger(shipment);
			var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);

			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var delivery = Factory.Load<CYDDelivery>(new ZQuery()).Single();
			AssertEquals("Delivery Transport Reference should be empty", "", delivery.YDL_TransportReference);
		}

		public void TestReadFromDataObject_ReleaseHasExpired()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true });
			var logger = GetLogger(shipment);
			var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);

			var releaseAdvice = Factory.Load<CYDReleaseAdvice>(new ZQuery(CYDReleaseAdviceSchema.YRE_ReleaseNumber, "BKR01")).Single();
			releaseAdvice.YRE_FromDate = new ZDate(2024, 1, 1);
			releaseAdvice.YRE_ToDate = new ZDate(2024, 1, 31);
			Factory.SaveForTesting();

			AssertExceptionThrown<YardUnitNotFoundException>("Data Object Read Failure", "Yard Unit (GENL) is not found or Instruction Advice (BKR01) has expired.", () =>
			{
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment();
			var logger = GetLogger(shipment);

			var yard = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);
			AssertNull(reader.TryGetExistingBusinessObject());

			var estimatedGateInTime = shipment.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			var tpu = CreateCYDTransportationUnit("REG1", estimatedGateInTime, ZDateTimeOffset.Empty, new DateTime(2024, 3, 1), ZGuid.Empty, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(tpu, reader.TryGetExistingBusinessObject());

			var olderTPUWithMatchingVehicleRego = CreateCYDTransportationUnit("REG1", estimatedGateInTime, ZDateTimeOffset.Empty, new DateTime(2024, 2, 1), ZGuid.Empty, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(olderTPUWithMatchingVehicleRego, reader.TryGetExistingBusinessObject());

			var newerTPUWithMatchingVehicleRego = CreateCYDTransportationUnit("REG1", estimatedGateInTime, ZDateTimeOffset.Empty, new DateTime(2024, 4, 1), ZGuid.Empty, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(olderTPUWithMatchingVehicleRego, reader.TryGetExistingBusinessObject());

			var tpuWithDifferentRego = CreateCYDTransportationUnit("REG2", estimatedGateInTime, ZDateTimeOffset.Empty, new DateTime(2024, 1, 1), ZGuid.Empty, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(olderTPUWithMatchingVehicleRego, reader.TryGetExistingBusinessObject());

			var area = Factory.NewWithValidTestData<WhsArea>();
			var location = Factory.NewWithValidTestData<WhsLocation>();
			location.WLV_WA_PickingArea = area.PK;
			location.WLV_WA_PutawayArea = area.PK;

			var gateInTPUWithMatchingRego = CreateCYDTransportationUnit("REG1", estimatedGateInTime, new DateTime(2024, 1, 1), new DateTime(2024, 1, 1), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(olderTPUWithMatchingVehicleRego, reader.TryGetExistingBusinessObject());
		}

		public void TestReadFromDataObject_GivenNoMatchingExistingTransportationUnit_ThenCreateTransportationUnit()
		{	
			var shipment = UniversalTestShipmentBuilder.GetShipment();
			var logger = GetLogger(shipment);

			new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery());

			AssertEquals("Expected a new transportation unit row to be created", 1, transportationUnit.Length);
		}

		public void TestReadFromDataObject_GivenMatchingExistingTransportationUnit_ThenUpdateTransportationUnit_Pickup()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true });
			var logger = GetLogger(shipment);

			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-1" }, IsEmptyContainer = true }
			});
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-2" }, IsEmptyContainer = true }
			});
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON5", ContainerType = new ContainerType() { Code = "LD-2" }, IsEmptyContainer = true }
			});

			var yard = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var estimatedGateInTime = shipment.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();

			var tpu = CreateCYDTransportationUnit("REG1", estimatedGateInTime, ZDateTimeOffset.Empty, new DateTime(2024, 7, 1), ZGuid.Empty, yard.GetValue(WhsWarehouseSchema.PK), shipment);

			var existingTransportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery());
			var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Precondition: Expected there to be one transportation unit row", 1, existingTransportationUnit.Length);

			Factory.SaveForTesting();

			AssertEquals("Transportation unit exist", tpu.YTU_GateInTime, reader.YTU_GateInTime);

			var pickups = Factory.Load<CYDPickup>(new ZQuery());
			AssertEquals("1 Pickup should be populated.", 1, pickups.Length);
			AssertEquals("Expect only one Pickup row is created", 1, reader.Pickups.Count);
		}

		public void TestReadFromDataObject_GivenMatchingExistingTransportationUnit_ThenUpdateTransportationUnit_Delivery()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = false });
			var logger = GetLogger(shipment);

			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-1" }, IsEmptyContainer = true }
			});
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-2" }, IsEmptyContainer = true }
			});

			var yard = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var estimatedGateInTime = shipment.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();

			var tpu = CreateCYDTransportationUnit("REG1", estimatedGateInTime, ZDateTimeOffset.Empty, new DateTime(2024, 7, 1), ZGuid.Empty, yard.GetValue(WhsWarehouseSchema.PK), shipment);

			var existingTransportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery());
			var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Precondition: Expected there to be one transportation unit row", 1, existingTransportationUnit.Length);

			Factory.SaveForTesting();

			AssertEquals("Transportation unit exist", tpu.YTU_GateInTime, reader.YTU_GateInTime);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals("1 Delivery should be populated.", 1, deliveries.Length);
			AssertEquals("Expect only one delivery row is created", 1, reader.Deliveries.Count);
		}

		public void TestReadFromDataObject_GivenMatchingMultipleExistingTransportationUnit_ThenUpdateEarliestTransportationUnit()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = new string[] { "GENL", "GENL2" } });
			var logger = GetLogger(shipment);

			var yard = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var estimatedGateInTime = shipment.SubShipmentCollection[0].DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();

			var tpuLatest = CreateCYDTransportationUnit("REG1", estimatedGateInTime, ZDateTimeOffset.Empty, new DateTime(2024, 7, 2), ZGuid.Empty, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			var tpuEarliest = CreateCYDTransportationUnit("REG1", estimatedGateInTime, ZDateTimeOffset.Empty, new DateTime(2024, 7, 1), ZGuid.Empty, yard.GetValue(WhsWarehouseSchema.PK), shipment);

			var existingTransportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery());
			var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Precondition: Expected there to be two transportation unit rows", 2, existingTransportationUnit.Length);

			Factory.SaveForTesting();

			AssertEquals("Earliest transportation unit row should be updated", tpuEarliest.YTU_SystemCreateTimeUtc, reader.YTU_SystemCreateTimeUtc);
			AssertNotEquals("Latest transportation unit row should not be updated", tpuLatest.YTU_SystemCreateTimeUtc, reader.YTU_SystemCreateTimeUtc);
		}

		public void TestReadFromDataObject_GivenUXMLWithEmptyRelatedShipmentCollection_ThenGateBookingIsIgnoredAndNoExceptionThrown()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment();
			var logger = GetLogger(shipment);

			AssertNull(shipment.RelatedShipmentCollection);

			AssertNoExceptionThrown(() =>
			{
				var reader = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
			});
		}

		public void TestReadFromDataObject_GivenDuplicateUXML_ShouldUpdateExistingBusinessObjectsWithoutCreatingNewBusinessObjects()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { DataSourceIsNotFound = true });
			var logger = GetLogger(shipment);

			var tpu1 = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var delivery1 = Factory.Load<CYDDelivery>(new ZQuery()).Single();

			var tpu2 = new CYDTransportationUnitForGateBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var delivery2 = Factory.Load<CYDDelivery>(new ZQuery()).Single();

			AssertEquals(tpu1.PK, tpu2.PK);
			AssertEquals(tpu1.ReceiveYardUnits.Count, tpu2.ReceiveYardUnits.Count);
			AssertEquals(tpu1.ReceiveYardUnits[0].PK, tpu2.ReceiveYardUnits[0].PK);
			AssertEquals(delivery1.PK, delivery2.PK);
		}

		CYDTransportationUnit CreateCYDTransportationUnit(string transportationReference, ZDateTimeOffset estimatedGateInTime, ZDateTimeOffset gateInTime, DateTime systemCreateTimeUtc, ZGuid waitingBayLocation, ZGuid yardPK, Shipment shipment)
		{
			var tpu = Factory.NewWithValidTestData<CYDTransportationUnit>();
			tpu.YTU_TransportationReference = transportationReference;
			tpu.YTU_GateInTime = gateInTime;
			tpu.YTU_SystemCreateTimeUtc = systemCreateTimeUtc;
			tpu.YTU_WL_WaitingBayLocation = waitingBayLocation;
			tpu.YTU_WW_Yard = yardPK;
			tpu.YTU_EstimatedGateInTime = estimatedGateInTime;

			var organizationAddress = shipment.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.TransportCompanyDocumentaryAddress));

			var header = Factory.LoadFromUniqueKey<IOrgHeader>(OrgHeaderSchema.OH_Code, (ZString)organizationAddress.OrganizationCode);
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, header.PK));

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_ParentID = tpu.PK;
			jobDocAddress.E2_OA_Address = address.PK;
			jobDocAddress.E2_AddressType = "TRA";

			Factory.SaveForTesting();
			return tpu;
		}

		UniversalTestData Data;

		protected override void SetUp()
		{
			base.SetUp();
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			Data.SetupDataForPickup("BKR01", "BKR01", today, tomorrow, "20GP", new string[] { "GENL", "GENL2", "GENL3" });
		}

		DummyLogger GetLogger(Shipment shipment)
		{
			var logger = new DummyLogger();
			logger.TopLevelDataObject = shipment;
			return logger;
		}
	}
}
