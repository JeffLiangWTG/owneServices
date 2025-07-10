using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	sealed class DtbBookingDataObjectReaderFromCartageAdviceTest : DtbBookingDataObjectReaderTest
	{
		public void TestAdditionalReferences_WithHouseBillOnSourceAndTopLevelDataObjects()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelDataObject.WayBillNumber = "TOP";
			topLevelDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var reader1 = new DtbBookingDataObjectReaderFromCartageAdvice(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, new UniversalObjectFactory(), consolidation, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), topLevelDataObject, booking);
			var transport1 = reader1.ReadIntoBusinessObject();
			AssertEquals(0, transport1.AdditionalReferenceNumbers.Count);
			AssertEquals(1, transport1.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
			var additionalReference1 = transport1.ConsolidationSingleJob.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference.CE_EntryNum", "TOP", additionalReference1.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "HSB", additionalReference1.CE_EntryType);

			additionalReference1.Delete();

			var sourceDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceDataObject.WayBillNumber = "SOURCE";
			sourceDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var reader2 = new DtbBookingDataObjectReaderFromCartageAdvice(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, new UniversalObjectFactory(), consolidation, sourceDataObject, topLevelDataObject, booking);
			var transport2 = reader2.ReadIntoBusinessObject();
			AssertEquals(0, transport2.AdditionalReferenceNumbers.Count);
			AssertEquals(1, transport2.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
			var additionalReference2 = transport2.ConsolidationSingleJob.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference.CE_EntryNum", "SOURCE", additionalReference2.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "HSB", additionalReference2.CE_EntryType);

			additionalReference2.Delete();

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.WayBillNumber = "DATAOBJECT";
			bookingDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());

			var reader3 = new DtbBookingDataObjectReaderFromCartageAdvice(bookingDataObject, Logger, new UniversalObjectFactory(), consolidation, sourceDataObject, topLevelDataObject, booking);
			var transport3 = reader3.ReadIntoBusinessObject();
			AssertEquals(0, transport3.AdditionalReferenceNumbers.Count);
			AssertEquals(1, transport3.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			var additionalReference3 = transport3.ConsolidationSingleJob.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference.CE_EntryNum", "DATAOBJECT", additionalReference3.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "HSB", additionalReference3.CE_EntryType);
		}

		public void TestAdditionalReferences_WithMasterBillOnSourceAndTopLevelDataObjects()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelDataObject.WayBillNumber = "TOP";
			topLevelDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			var sourceDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceDataObject.WayBillNumber = "SOURCE";
			sourceDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, new UniversalObjectFactory(), consolidation, sourceDataObject, topLevelDataObject, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();
			AssertEquals(2, bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			var additionalReference1 = bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference1.CE_EntryNum", "SOURCE", additionalReference1.CE_EntryNum);
			AssertEquals("additionalReference1.CE_EntryType", "MAB", additionalReference1.CE_EntryType);

			var additionalReference2 = bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers[1];
			AssertEquals("additionalReference1.CE_EntryNum", "TOP", additionalReference2.CE_EntryNum);
			AssertEquals("additionalReference1.CE_EntryType", "MAB", additionalReference2.CE_EntryType);
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_ImportsSourceThenDataObjectReferences()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.TransportMode = new CodeDescriptionPair();
			consol.TransportMode.Code = TransportModes.Air;
			consol.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			consol.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "CONSOLIDATION-TRF" });
			consol.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.OrderNumber }, ReferenceNumber = "CONSOLIDATION-ORD" });

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TransportMode = new CodeDescriptionPair();
			shipment.TransportMode.Code = TransportModes.Air;
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber }, ReferenceNumber = "Shipment-ETBN" });
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.BookingPartyReference }, ReferenceNumber = "Shipment-BPR" });

			var sourceDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			sourceDataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "SOURCE-TRF" });

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, new UniversalObjectFactory(), consolidation, sourceDataObject, consol, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();
			AssertEquals(1, bookingReadIn.AdditionalReferenceNumbers.Count);

			var additionalReference1 = bookingReadIn.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference1.CE_EntryNum", "SOURCE-TRF", additionalReference1.CE_EntryNum);
			AssertEquals("additionalReference1.CE_EntryType", AdditionalReferenceTypes.Codes.TransportReference, additionalReference1.CE_EntryType);
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_ImportsSourceThenDataObjectReferences_SourceHasNoRefs()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.TransportMode = new CodeDescriptionPair();
			consol.TransportMode.Code = TransportModes.Air;
			consol.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			consol.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "CONSOLIDATION-TRF" });
			consol.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.OrderNumber }, ReferenceNumber = "CONSOLIDATION-ORD" });

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TransportMode = new CodeDescriptionPair();
			shipment.TransportMode.Code = TransportModes.Air;
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber }, ReferenceNumber = "Shipment-ETBN" });
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.BookingPartyReference }, ReferenceNumber = "Shipment-BPR" });

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, new UniversalObjectFactory(), consolidation, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), consol, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();
			AssertEquals("Booking AdditionalReferenceNumbers Count", 0, bookingReadIn.AdditionalReferenceNumbers.Count);
		}

		public void TestAdditionalReferences_WayBillPrecedence_InOrderOfDataObjectSourceDOTopLevelDO()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.TransportMode = new CodeDescriptionPair();
			consol.TransportMode.Code = TransportModes.Air;
			consol.WayBillNumber = "Consol";
			consol.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TransportMode = new CodeDescriptionPair();
			shipment.TransportMode.Code = TransportModes.Air;
			shipment.WayBillNumber = "Shipment";
			shipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var sourceDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceDataObject.WayBillNumber = "Source";
			sourceDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, new UniversalObjectFactory(), consolidation, sourceDataObject, consol, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();
			AssertEquals(1, bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			var additionalReference1 = bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference1.CE_EntryNum", "Shipment", additionalReference1.CE_EntryNum);
			AssertEquals("additionalReference1.CE_EntryType", "HSB", additionalReference1.CE_EntryType);
		}

		public void TestAdditionalReferences_WayBillPrecedence_InOrderOfDataObjectSourceDOTopLevelDO_Source()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.TransportMode = new CodeDescriptionPair();
			consol.TransportMode.Code = TransportModes.Air;
			consol.WayBillNumber = "Consol";
			consol.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TransportMode = new CodeDescriptionPair();
			shipment.TransportMode.Code = TransportModes.Air;
			shipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var sourceDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceDataObject.WayBillNumber = "Source";
			sourceDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, new UniversalObjectFactory(), consolidation, sourceDataObject, consol, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();
			AssertEquals(1, bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			var additionalReference1 = bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference1.CE_EntryNum", "Source", additionalReference1.CE_EntryNum);
			AssertEquals("additionalReference1.CE_EntryType", "HSB", additionalReference1.CE_EntryType);
		}

		public void TestAdditionalReferences_WayBillPrecedence_InOrderOfDataObjectSourceDOTopLevelDO_TopLevel()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.TransportMode = new CodeDescriptionPair();
			consol.TransportMode.Code = TransportModes.Air;
			consol.WayBillNumber = "Consol";
			consol.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TransportMode = new CodeDescriptionPair();
			shipment.TransportMode.Code = TransportModes.Air;
			shipment.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());

			var sourceDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, new UniversalObjectFactory(), consolidation, sourceDataObject, consol, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();
			AssertEquals(1, bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			var additionalReference1 = bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference1.CE_EntryNum", "Consol", additionalReference1.CE_EntryNum);
			AssertEquals("additionalReference1.CE_EntryType", "HSB", additionalReference1.CE_EntryType);
		}

		ZString TestHelperForTestingPopulatePackageReleaseNumberDictionary(ZInt packageLink, ZInt containerLink, ZString releaseNumber)
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var transportBookingParent = factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(transportBookingParent);
			var pkgContainer = consolidation.PackageJob.Packages.AddNew("CNT", "");

			factory.Save();

			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.Confirmations.AddNew();
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;

			factory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>
			{
				{ packageLink, pkgContainer }
			};

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalTransportJobType = new CodeDescriptionPair4Char { Code = "EFPL" }
			};
			bookingDataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { Link = containerLink, ReleaseNum = releaseNumber } });

			// Act
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingDataObject, Logger, new UniversalObjectFactory(), consolidation, bookingDataObject, bookingDataObject, booking, packageContainerLinks);
			var bookingReader = reader.ReadIntoBusinessObject();
			var result = bookingReader.PickupConfirmations.Single(x => x.OrganisationType == "CYD").KK_ReferenceNum;

			return result;
		}

		public void TestPopulatePackageReleaseNumberDictionary_PopulatesDictionaryWithCorrectValues()
		{
			var releaseNumber = "EmptyRelease123";
			var link = 1;

			var result = TestHelperForTestingPopulatePackageReleaseNumberDictionary(link, link, releaseNumber);

			AssertEquals(releaseNumber, result);
		}

		public void TestPopulatePackageReleaseNumberDictionary_PopulatesDictionaryWithCorrectValues_ContainerHasNoReleaseNumber()
		{
			var link = 1;
			var result = TestHelperForTestingPopulatePackageReleaseNumberDictionary(link, link, null);

			AssertEquals(ZString.Empty, result);
		}

		public void TestPopulatePackageReleaseNumberDictionary_PopulatesDictionaryWithCorrectValues_LinkDoesNotMatch()
		{
			var releaseNumber = "EmptyRelease123";
			var packageLink = 1;
			var containerLink = 2;

			var result = TestHelperForTestingPopulatePackageReleaseNumberDictionary(packageLink, containerLink, releaseNumber);

			AssertEquals(ZString.Empty, result);
		}

		public void TestPopulatePackages_SetBookingIsHazardous()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = factory.New<DtbBookingConsolidation>();
			var container = consolidation.PackageJob.Packages.AddNew("CNT", "CONT123");
			var containerPackage = container.Packages.AddNew("BOT", "XYZ");
			var loosePackage = consolidation.PackageJob.Packages.AddNew("BOT", "ABC");
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "CONT123", ContainerType = new ContainerType { Code = "20GP" }, Link = 1 } });

			// Job type is mixed
			bookingDataObject.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "EFPL" }; // Export FCL/ULD, Pack at CFS, Pickup Loose from CNR
			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>();
			packageContainerLinks.Add(1, container);
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingDataObject, Logger, factory, consolidation, bookingDataObject, bookingDataObject, null, packageContainerLinks);
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals("Booking KM_IsHazardous should be false.", false, booking.KM_IsHazardous);

			containerPackage.UNDGs.AddNew();
			var dgReader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingDataObject, Logger, factory, consolidation, bookingDataObject, bookingDataObject);
			var dgBooking = reader.ReadIntoBusinessObject();
			AssertEquals("Booking KM_IsHazardous should be true.", true, dgBooking.KM_IsHazardous);
		}

		public void TestPopulatePackages_SetBookingRequiresRefrigeration()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = factory.New<DtbBookingConsolidation>();
			var container = consolidation.PackageJob.Packages.AddNew("CNT", "CONT123");
			var containerPackage = container.Packages.AddNew("BOT", "XYZ");
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "CONT123", ContainerType = new ContainerType { Code = "20GP" }, Link = 1 } });

			bookingDataObject.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "EFPL" }; // Export FCL/ULD, Pack at CFS, Pickup Loose from CNR
			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>();
			packageContainerLinks.Add(1, container);
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingDataObject, Logger, factory, consolidation, bookingDataObject, bookingDataObject, null, packageContainerLinks);
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals("Booking KM_RequiresRefrigeration should be false.", false, booking.KM_RequiresRefrigeration);

			containerPackage.KP_RequiresTemperatureControl = true;
			var rfReader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingDataObject, Logger, factory, consolidation, bookingDataObject, bookingDataObject);
			var rfBooking = rfReader.ReadIntoBusinessObject();
			AssertEquals("Booking KM_RequiresRefrigeration should be true.", true, rfBooking.KM_RequiresRefrigeration);
		}

		public void TestPopulatePackages_WithContainerLinksWithValueOfZero()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = factory.New<DtbBookingConsolidation>();
			var container1 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT111");
			var container2 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT222");
			var container3 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT333");
			var container4 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT444");
			var container5 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT555");

			var containerPackage = container1.Packages.AddNew("BOT", "AAA");
			var containerPackage2 = container2.Packages.AddNew("BOT", "BBB");
			var containerPackage3 = container2.Packages.AddNew("BOT", "CCC");

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.SetContainerCollection(() => new DataObjectList<Container> {
				new Container { ContainerNumber = "CONT111", ContainerType = new ContainerType { Code = "20GP" }, Link = 1 },
				new Container { ContainerNumber = "CONT222", ContainerType = new ContainerType { Code = "20GP" }, Link = 2 },
				new Container { ContainerNumber = "CONT333", ContainerType = new ContainerType { Code = "20GP" }, Link = 3 },
				new Container { ContainerNumber = "CONT444", ContainerType = new ContainerType { Code = "20GP" }, Link = 0 },
				new Container { ContainerNumber = "CONT555", ContainerType = new ContainerType { Code = "20GP" }, Link = 0 },
			});

			bookingDataObject.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "EFPL" }; // Export FCL/ULD, Pack at CFS, Pickup Loose from CNR
			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>();
			packageContainerLinks.Add(1, container1);
			packageContainerLinks.Add(2, container2);
			packageContainerLinks.Add(3, container3);
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingDataObject, Logger, factory, consolidation, bookingDataObject, bookingDataObject, null, packageContainerLinks);

			var booking = reader.ReadIntoBusinessObject();
			AssertEquals(3, booking.Containers.Count());
		}

		public void TestPackageContainerLinks_IsPopulatedCorrectlyForBooking()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = factory.New<DtbBookingConsolidation>();
			var container1 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT111");
			var container2 = consolidation.PackageJob.Packages.AddNew("CNT", "CONT222");

			var containerPackage1 = container1.Packages.AddNew("BOT", "AAA");
			var containerPackage2 = container2.Packages.AddNew("BOT", "BBB");

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.SetContainerCollection(() => new DataObjectList<Container> {
				new Container { ContainerNumber = "CONT111", ContainerType = new ContainerType { Code = "20GP" }, Link = 1 },
				new Container { ContainerNumber = "CONT222", ContainerType = new ContainerType { Code = "20GP" }, Link = 2 },
			});

			bookingDataObject.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "EFPL" }; // Export FCL/ULD, Pack at CFS, Pickup Loose from CNR
			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>
			{
				{ 1, container1 },
				{ 2, container2 }
			};
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingDataObject, Logger, factory, consolidation, bookingDataObject, bookingDataObject, null, packageContainerLinks);

			var booking = reader.ReadIntoBusinessObject();
			AssertEquals(packageContainerLinks, booking.PackageContainerLinks);
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			var reader = GetNewReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, consolidation, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), booking);
			var bookingRead = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingRead);
			AssertEquals(booking.PK, bookingRead.PK);
		}

		public void TestOrderNumbers_OrderNo()
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var order = topLevelShipment.Order = new Order();
			Logger.TopLevelDataObject = topLevelShipment;
			order.OrderNumber = "ReceiveReference";

			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingShipment });

			var consolidation = Factory.New<DtbBookingConsolidation>();
			var bookingBO = consolidation.Bookings.AddNew();
			AddOrderNumbersToBookingAndConsolidation(bookingBO, oldOrderNumbers);

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingShipment, Logger, new UniversalObjectFactory(), consolidation, topLevelShipment, topLevelShipment, bookingBO);
			var booking = reader.ReadIntoBusinessObject();
			var consolidationReferenceNumbers = booking.ConsolidationSingleJob.AdditionalReferenceNumbers;
			CombineAssertions(
				"Should have overwritten order numbers on both booking and booking consolidation with new values correctly from Order.OrderNumber",
				() =>
				{
					AssertEquals("Should populate OrderNumber into Consolidation AdditionalReferences, and it should be the only one - old order numbers should be deleted.", "ReceiveReference", consolidationReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Single());
					AssertEquals("Should have deleted all OrderNumbers from booking", 0, booking.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Length);
				});
		}

		public void TestOrderNumbers_OwnerRefs()
		{
			TestOrderNumbers_OwnerRefs("123", "123");
		}

		public void TestOrderNumbers_OwnerRefs_CSV()
		{
			TestOrderNumbers_OwnerRefs("123, ABC, , DEF", "123", "ABC", "DEF");
		}

		public void TestOrderNumbers_OwnerRefs_OnSourceDO()
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			Logger.TopLevelDataObject = topLevelShipment;

			topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingShipment });
			topLevelShipment.OwnerRef = "A";

			var consolidation = Factory.New<DtbBookingConsolidation>();
			var bookingBO = consolidation.Bookings.AddNew();
			AddOrderNumbersToBookingAndConsolidation(bookingBO, oldOrderNumbers);

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingShipment, Logger, new UniversalObjectFactory(), consolidation, topLevelShipment, topLevelShipment, bookingBO);
			var booking = reader.ReadIntoBusinessObject();
			var consolidationReferenceNumbers = booking.ConsolidationSingleJob.AdditionalReferenceNumbers;
			CombineAssertions(
				"Should have overwritten order numbers on both booking and booking consolidation with new values correctly",
				() =>
				{
					AssertEquals("Should populate OrderNumber into Consolidation AdditionalReferences, and it should be the only one - old order numbers should be deleted", "A", consolidationReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Single());
					AssertEquals("Should have deleted all OrderNumbers from booking", 0, booking.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Length);
				});
		}

		void TestOrderNumbers_OwnerRefs(string ownerRef, params string[] expectedAdditionalReferences)
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			Logger.TopLevelDataObject = topLevelShipment;
			topLevelShipment.OwnerRef = "TOPLEVEL";
			var order = topLevelShipment.Order = new Order();
			order.OrderNumber = "123";

			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingShipment });
			bookingShipment.OwnerRef = ownerRef;

			var consolidation = Factory.New<DtbBookingConsolidation>();
			var bookingBO = consolidation.Bookings.AddNew();
			AddOrderNumbersToBookingAndConsolidation(bookingBO, oldOrderNumbers);
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingShipment, Logger, new UniversalObjectFactory(), consolidation, topLevelShipment, topLevelShipment, bookingBO);
			var booking = reader.ReadIntoBusinessObject();
			var consolidationOrderReferenceNumbers = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber);

			CombineAssertions(
				"Should have overwritten order numbers on both booking and booking consolidation with new values correctly from OwnerRefs",
				() =>
				{
					AssertContainsExactElementsInAnyOrder("Should have populated order numbers as per OwnerRefs", expectedAdditionalReferences, consolidationOrderReferenceNumbers);
					AssertEquals("Should have deleted all OrderNumbers from booking", 0, booking.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Length);
				});
		}

		public void TestOrderNumbers_OrderNumberCollection()
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			Logger.TopLevelDataObject = topLevelShipment;
			var topLevelLocalProcessing = topLevelShipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelLocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>(new[] { new OrderNumber { OrderReference = "ONTOPLEVEL" } }));
			topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingShipment });

			var localProcessing = bookingShipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			var newOrderNumbers = new[] { "123", "ABC" };
			var newOrderNumberCollection = newOrderNumbers.Select(o => new OrderNumber { OrderReference = o });
			localProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>(newOrderNumberCollection));

			var consolidation = Factory.New<DtbBookingConsolidation>();
			var bookingBO = consolidation.Bookings.AddNew();
			AddOrderNumbersToBookingAndConsolidation(bookingBO, oldOrderNumbers);
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingShipment, Logger, new UniversalObjectFactory(), consolidation, topLevelShipment, topLevelShipment, bookingBO);
			var booking = reader.ReadIntoBusinessObject();
			var consolidationOrderReferenceNumbers = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber);

			CombineAssertions("Should have overwritten order numbers on both booking and booking consolidation with new values correctly from OrderNumberCollection",
				() =>
				{
					AssertContainsExactElementsInAnyOrder("Should populate OrderNumber into AdditionalReferences.", newOrderNumbers, consolidationOrderReferenceNumbers);
					AssertEquals("Should have deleted all OrderNumbers from booking", 0, booking.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Length);
				});

			var newOwnerRefsWithDuplicatesFromOrderNumbers = new[] { "123", "ABC", "DEF" };
			bookingShipment.OwnerRef = string.Join(", ", newOwnerRefsWithDuplicatesFromOrderNumbers); // Customs Declaration can have duplicated data in UXML

			booking = reader.ReadIntoBusinessObject();
			consolidationOrderReferenceNumbers = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber);
			CombineAssertions("Should have overwritten order numbers on both booking and booking consolidation with new values correctly from both OrderNumberCollection and OwnerRefs",
				() =>
				{
					AssertContainsExactElementsInAnyOrder("Should populate OrderNumber into AdditionalReferences.", newOwnerRefsWithDuplicatesFromOrderNumbers, consolidationOrderReferenceNumbers);
					AssertEquals("Should still have no OrderNumbers on booking", 0, booking.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Length);
				});
		}

		public void TestOrderNumbers_OrderNumberCollection_OnSourceDO()
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			Logger.TopLevelDataObject = topLevelShipment;
			var topLevelLocalProcessing = topLevelShipment.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelLocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>(new[] { new OrderNumber { OrderReference = "ONTOPLEVEL" } }));
			topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingShipment });

			var consolidation = Factory.New<DtbBookingConsolidation>();
			var bookingBO = consolidation.Bookings.AddNew();
			AddOrderNumbersToBookingAndConsolidation(bookingBO, oldOrderNumbers);
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingShipment, Logger, new UniversalObjectFactory(), consolidation, topLevelShipment, topLevelShipment, bookingBO);
			var booking = reader.ReadIntoBusinessObject();
			var consolidationOrderReferenceNumbers = booking.ConsolidationSingleJob.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber);
			CombineAssertions(
				"Should have overwritten order numbers on both booking and booking consolidation with new values correctly from OrderNumberCollection",
				() =>
				{
					AssertEquals("Should populate single OrderNumber into Consolidation AdditionalReferences from OrderNumberCollection.", "ONTOPLEVEL", consolidationOrderReferenceNumbers.Single());
					AssertEquals("Should have deleted all OrderNumbers on booking", 0, booking.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Length);
				});
		}

		public void TestOldOrderNumbersAreRemovedFromBookingAndConsolidation()
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var consolidation = Factory.New<DtbBookingConsolidation>();
			var bookingBO = consolidation.Bookings.AddNew();
			AddOrderNumbersToBookingAndConsolidation(bookingBO, oldOrderNumbers);
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingShipment, Logger, new UniversalObjectFactory(), consolidation, topLevelShipment, topLevelShipment, bookingBO);
			var booking = reader.ReadIntoBusinessObject();
			CombineAssertions(
				"Should have removed all OrderNumbers from consolidation and booking",
				() =>
				{
					AssertEquals("Should have deleted all OrderNumbers on booking consolidation", 0, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Length);
					AssertEquals("Should have deleted all OrderNumbers on booking", 0, booking.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Length);
				});
		}

		public void TestSetReference_ForWhsReceive()
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var order = topLevelShipment.Order = new Order();
			var client = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			client.AddressType = AddressTypes.SendersLocalClient;
			client.CompanyName = "TEST CLIENT";
			topLevelShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { client });

			order.ClientReference = "ClientReference";
			order.OrderNumber = "ReceiveReference";

			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelShipment.DataContext = DataContextFactory.New();
			topLevelShipment.DataContext.AddDataSource(DataContextType.WarehouseReceive, "WHSRCV");
			Logger.TopLevelDataObject = topLevelShipment;
			topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingShipment });

			var consolidation = Factory.New<DtbBookingConsolidation>();
			var bookingBO = consolidation.Bookings.AddNew();
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingShipment, Logger, new UniversalObjectFactory(), consolidation, topLevelShipment, bookingShipment, bookingBO);
			var booking = reader.ReadIntoBusinessObject();
			var bookingReferenceNumbers = booking.AdditionalReferenceNumbers;
			var consolidationReferenceNumbers = booking.ConsolidationSingleJob.AdditionalReferenceNumbers;
			AssertEquals(2, bookingReferenceNumbers.Count);
			AssertEquals(1, consolidationReferenceNumbers.Count);
			AssertEquals("TEST CLIENT", bookingReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.Client).Single());
			AssertEquals("ClientReference", bookingReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber).Single());
			AssertEquals("ReceiveReference", consolidationReferenceNumbers.GetAllReferenceNumbersByType(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber).Single());
		}

		public void TestSetReference_ForWhsReceive_BlankValuesForReferenceNumbers()
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var order = topLevelShipment.Order = new Order();
			var client = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			client.AddressType = AddressTypes.SendersLocalClient;
			client.CompanyName = "TEST CLIENT";
			topLevelShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { client });

			order.ClientReference = "";
			order.OrderNumber = "";

			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelShipment.DataContext = DataContextFactory.New();
			topLevelShipment.DataContext.AddDataSource(DataContextType.WarehouseReceive, "WHSRCV");
			Logger.TopLevelDataObject = topLevelShipment;
			topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingShipment });

			var consolidation = Factory.New<DtbBookingConsolidation>();
			var bookingBO = consolidation.Bookings.AddNew();
			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingShipment, Logger, new UniversalObjectFactory(), consolidation, topLevelShipment, bookingShipment, bookingBO);
			var booking = reader.ReadIntoBusinessObject();
			var bookingReferenceNumbers = booking.AdditionalReferenceNumbers;
			var consolidationReferenceNumbers = booking.ConsolidationSingleJob.AdditionalReferenceNumbers;
			AssertEquals(1, bookingReferenceNumbers.Count);
			var bookingReferenceNumber = bookingReferenceNumbers.Cast<ICusEntryNumber>().Single();
			AssertEquals("TEST CLIENT", bookingReferenceNumber.CE_EntryNum);
			AssertEquals(TransportCommonAdditionalReferenceTypes.Codes.Client, bookingReferenceNumber.CE_EntryType);
			AssertEquals(0, consolidationReferenceNumbers.Count);
		}

		public void TestSetReference_ForWhsReceiveUpdatesExistingReferences()
		{
			var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var order = topLevelShipment.Order = new Order();
			order.ClientReference = "ClientReference";
			order.OrderNumber = "OrderNumber";

			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelShipment.DataContext = DataContextFactory.New();
			topLevelShipment.DataContext.AddDataSource(DataContextType.WarehouseReceive, "WHSRCV");
			Logger.TopLevelDataObject = topLevelShipment;
			topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { bookingShipment });

			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber, "1");
			consolidation.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber, "OLD");

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(bookingShipment, Logger, new UniversalObjectFactory(), consolidation, topLevelShipment, bookingShipment, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();
			var bookingReferenceNumbers = bookingReadIn.AdditionalReferenceNumbers;
			var consolidationReferenceNumbers = bookingReadIn.ConsolidationSingleJob.AdditionalReferenceNumbers;
			AssertEquals(1, bookingReferenceNumbers.Count);
			var bookingReferenceNumber = bookingReferenceNumbers.Cast<ICusEntryNumber>().Single();
			AssertEquals("ClientReference", bookingReferenceNumber.CE_EntryNum);
			AssertEquals(TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber, bookingReferenceNumber.CE_EntryType);
			AssertEquals(1, consolidationReferenceNumbers.Count);
			var consolidationReferenceNumber = consolidationReferenceNumbers.Cast<ICusEntryNumber>().Single();
			AssertEquals("OrderNumber", consolidationReferenceNumber.CE_EntryNum);
			AssertEquals(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber, consolidationReferenceNumber.CE_EntryType);
		}

		public void TestGetNewBusinessObject()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			Factory.SaveForTesting();

			var reader = GetNewReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, consolidation, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
			var bookingRead = reader.ReadIntoBusinessObject();

			AssertNotNull(bookingRead);
			AssertEquals(consolidation.PK, bookingRead.ConsolidationSingleJob.PK);
		}

		public void TestSetContainerLinksAndNumbersOnBookingFromShipment()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var containerList = new DataObjectList<Container>() {
				new Container() { Link = 1, ContainerNumber = "CN1" },
				new Container() { Link = 2, ContainerNumber = "CN2" }
			};
			shipment.SetContainerCollection(() => containerList);
			var reader = GetNewReader(shipment, Logger, consolidation, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), booking);
			var bookingRead = reader.ReadIntoBusinessObject();
			var expectedContainerLinks = new List<ZInt>() { 1, 2 };
			var expectedContainerNumbers = new List<ZString>() { "CN1", "CN2" };
			AssertEquals("Booking should have container links set", true, bookingRead.ContainerLinks.SequenceEqual(expectedContainerLinks));
			AssertEquals("Booking should have container numbers set", true, bookingRead.ContainerNumbers.SequenceEqual(expectedContainerNumbers));
		}

		protected override DtbBookingDataObjectReader GetNewReader(UniversalShipment bookingDataObject, IXmlImportLogger logger, DtbBookingConsolidation consolidation, UniversalShipment topLevelDO, DtbBooking booking = null, UniversalObjectFactory uFactory = null)
		{
			return new DtbBookingDataObjectReaderFromCartageAdvice(bookingDataObject, logger, uFactory ?? new UniversalObjectFactory(), consolidation, bookingDataObject, topLevelDO, booking);
		}

		protected override DtbBookingDataObjectReader GetNewReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBooking booking, UniversalShipment topLevelDO)
		{
			return GetNewReader(shipment, logger, factory.New<DtbBookingConsolidation>(), topLevelDO, booking, factory);
		}

		void AddOrderNumbersToBookingAndConsolidation(DtbBooking bookingBO, string[] orderNumbers)
		{
			foreach (var orderNumber in orderNumbers)
			{
				var bookingOrderNumber = bookingBO.AdditionalReferenceNumbers.AddNew();
				bookingOrderNumber.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.OrderNumber;
				bookingOrderNumber.CE_EntryNum = orderNumber;

				var bookingConsolidationOrderNumber = bookingBO.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNew();
				bookingConsolidationOrderNumber.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.OrderNumber;
				bookingConsolidationOrderNumber.CE_EntryNum = orderNumber;
			}
		}

		readonly string[] oldOrderNumbers = new string[]
		{
			"OLDORD1",
			"OLDORD2"
		};
	}
}
