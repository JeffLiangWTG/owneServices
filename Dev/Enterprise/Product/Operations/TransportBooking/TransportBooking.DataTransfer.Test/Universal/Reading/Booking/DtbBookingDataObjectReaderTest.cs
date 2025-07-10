using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;
using UniversalDataBusEntryType = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryType;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	abstract class DtbBookingDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestDataContextType()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var reader = GetNewReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, consolidation, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
			AssertEquals(DataContextType.TransportBooking, reader.DataContextType);
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_JobID = "1";
			booking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
			booking.UpdateStatus();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();
			Assert(Logger.GetErrors().Contains("Transport Booking 1 has already been commenced and could not be overridden."));
		}

		public void TestAdditionalReferences_WithHouseBill_OnSourceLevelDO()
		{
			var sourceLevelDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceLevelDO.WayBillNumber = "12345";
			sourceLevelDO.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());

			var reader = GetNewReader(sourceLevelDO, Logger, Factory, null, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals(0, booking.AdditionalReferenceNumbers.Count);
			AssertEquals(1, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
			AssertEquals("12345", booking.WayBillNumber);
			AssertEquals("12345", booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Single(a => a.CE_EntryType == AdditionalReferenceTypes.Codes.HouseBill).CE_EntryNum);
		}

		public void TestAdditionalReferences_WithHouseBill_OnSourceLevelDO_WithMasterBillOnTopLevelDO()
		{
			var sourceLevelDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceLevelDO.WayBillNumber = "HouseBill";
			sourceLevelDO.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());

			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelDataObject.WayBillNumber = "MasterBill";
			topLevelDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			var reader = GetNewReader(sourceLevelDO, Logger, Factory, null, topLevelDataObject);
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals(0, booking.AdditionalReferenceNumbers.Count);
			AssertEquals(2, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
			AssertEquals("HouseBill", booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Single(a => a.CE_EntryType == AdditionalReferenceTypes.Codes.HouseBill).CE_EntryNum);
			AssertEquals("MasterBill", booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Single(a => a.CE_EntryType == AdditionalReferenceTypes.Codes.MasterBill).CE_EntryNum);
		}

		public void TestAdditionalReferences_WithHouseBill_OnSourceLevelDO_WithMasterBillOnParentShipment()
		{
			var sourceLevelDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceLevelDO.WayBillNumber = "HouseBill";
			sourceLevelDO.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());
			sourceLevelDO.DataContext = DataContextFactory.New();
			sourceLevelDO.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Shipment");

			var consolDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolDataObject.WayBillNumber = "MasterBill";
			consolDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());
			consolDataObject.DataContext = DataContextFactory.New();
			consolDataObject.DataContext.AddDataSource(DataContextType.ForwardingConsol, "Consol");
			sourceLevelDO.SetParentShipmentCollection(() => new List<UniversalShipment>() {
				consolDataObject
			});

			var reader = GetNewReader(sourceLevelDO, Logger, Factory, null, sourceLevelDO);
			var booking = reader.ReadIntoBusinessObject();
			CombineAssertions("Additional References should be correct on booking and consolidation", () =>
			{
				AssertEquals("There should be no Additional References on booking", 0, booking.AdditionalReferenceNumbers.Count);
				AssertEquals("There should be 2 Additional References on booking consolidation", 2, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);
				AssertEquals("There should be a HouseBill on the consolidation", "HouseBill", booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().SingleOrDefault(a => a.CE_EntryType == AdditionalReferenceTypes.Codes.HouseBill)?.CE_EntryNum);
				AssertEquals("There should be a MasterBill on the consolidation", "MasterBill", booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().SingleOrDefault(a => a.CE_EntryType == AdditionalReferenceTypes.Codes.MasterBill)?.CE_EntryNum);
			});
		}

		public void TestAdditionalReferences_OnTopLevelDOAndSourceDO()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			topLevelDataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "CONSOLIDATION-TRF" });
			topLevelDataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = "OTH" }, ReferenceNumber = "CONSOLIDATION-OTH" });

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			dataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "SOURCE-TRF" });

			var reader = GetNewReader(dataObject, Logger, Factory, null, topLevelDataObject);
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals(0, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			var importedReferencesOnBooking = booking.AdditionalReferenceNumbers.Cast<ICusEntryNumber>();
			AssertEquals(1, importedReferencesOnBooking.Count());
			AssertEquals("SOURCE-TRF", importedReferencesOnBooking.Single(a => a.CE_EntryType == AdditionalReferenceTypes.Codes.TransportReference).CE_EntryNum);
		}

		public void TestRemoveUnnecessaryAdditionalReferences_RemovesDuplicates()
		{
			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.AdditionalReferenceNumbers.AddNew();
			consolidation.AdditionalReferenceNumbers[0].CE_EntryType = AdditionalReferenceTypes.Codes.TransportReference;
			consolidation.AdditionalReferenceNumbers[0].CE_EntryNum = "SOURCE-TRF";

			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			dataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "SOURCE-TRF" });

			var reader = GetNewReader(dataObject, Logger, consolidation, topLevelDataObject, null, Factory);
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals(1, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			var importedReferencesOnBooking = booking.AdditionalReferenceNumbers.Cast<ICusEntryNumber>();
			AssertEquals(0, importedReferencesOnBooking.Count());
		}

		public void TestRemoveUnnecessaryAdditionalReferences_DoesNotRemoveNonDuplicates()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			dataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "SOURCE-TRF" });

			var reader = GetNewReader(dataObject, Logger, Factory, null, topLevelDataObject);
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals(0, booking.ConsolidationSingleJob.AdditionalReferenceNumbers.Count);

			var importedReferencesOnBooking = booking.AdditionalReferenceNumbers.Cast<ICusEntryNumber>();
			AssertEquals(1, importedReferencesOnBooking.Count());
			AssertEquals("SOURCE-TRF", importedReferencesOnBooking.Single(a => a.CE_EntryType == AdditionalReferenceTypes.Codes.TransportReference).CE_EntryNum);
		}

		public void TestReadFromPICTestFile_ContainersWithDifferentAddresses_SplitIntoSeparateCYDInstructions()
		{
			var boFactory = new BusinessObjectFactory();

			var orgHeader1 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "CAREDISYD";
			var orgHeader2 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "KGHOMESYD";
			var orgHeader3 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CMACGMSYD";
			var orgHeader4 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader4.OH_Code = "ROGBROUSC";

			var orgAddress1 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress1.AddressCode = "72 O'RIORDAN STREET";
			orgAddress1.OA_OH = orgHeader1.PK;
			var orgAddress2 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress2.AddressCode = "8 PARKVIEW ROAD";
			orgAddress2.OA_OH = orgHeader2.PK;
			var orgAddress3 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress3.AddressCode = "19-21 Pirrama Road";
			orgAddress3.OA_OH = orgHeader3.PK;
			var orgAddress4 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress4.AddressCode = "101 Trade Zone Dr Ste 7a";
			orgAddress4.OA_OH = orgHeader4.PK;

			boFactory.Save();

			var multipleCYDContainersPIC = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Multiple CYD containers with different addresses (PIC).xml");
			var message = GetQueuedUniversalShipmentMessage(multipleCYDContainersPIC);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var booking = boFactory.LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000001"));

			CombineAssertions(() =>
			{
				AssertEquals("Packages on booking", 2, booking.PackageJob.Packages.Count(p => !p.KP_IsUnknownQty));
				AssertEquals("Total # of instructions should be correct", 4, booking.Instructions.Count);

				var cydInstructionsCount = 0;

				foreach (DtbBookingInstruction instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
				{
					cydInstructionsCount += 1;
					var containers = new List<PkgPackage>();
					foreach (DtbBookingInstructionPkgDivot divot in instruction.PackageDivots)
					{
						containers.Add(divot.Package);
					}

					AssertEquals("Packages on instruction", 1, containers.Count);
					AssertEquals(true, containers.Single().KP_PackageID == "AAAAAAAAAA" || containers.Single().KP_PackageID == "BBBBBBBBBB");

					if (containers.Single().KP_PackageID == "AAAAAAAAAA")
					{
						AssertEquals("8 PARKVIEW ROAD", instruction.Address.RealAddress.AddressCode);
					}
					else if (containers.Single().KP_PackageID == "BBBBBBBBBB")
					{
						AssertEquals("19-21 Pirrama Road", instruction.Address.RealAddress.AddressCode);
					}
				}

				AssertEquals("Total CYD instructions should be correct", 2, cydInstructionsCount);

				var orderedInstructions = booking.Instructions.OrderBy(i => i.KN_Sequence).ToList();

				AssertEquals(OrganisationTypesList.Codes.CYD, orderedInstructions[0].OrganisationType);
				AssertEquals("Instruction 1 should have drop mode enabled", "WUP", orderedInstructions[0].DropMode);

				AssertEquals(OrganisationTypesList.Codes.CYD, orderedInstructions[1].OrganisationType);
				AssertEquals("Instruction 2 should have drop mode enabled", "WUP", orderedInstructions[1].DropMode);

				AssertEquals(1, orderedInstructions[0].KN_Sequence);
				AssertEquals(2, orderedInstructions[1].KN_Sequence);
				AssertEquals(3, orderedInstructions[2].KN_Sequence);
				AssertEquals(4, orderedInstructions[3].KN_Sequence);
			});
		}

		public void TestReadFromDLVTestFile_ContainersWithDifferentAddresses_SplitIntoSeparateCYDInstructions()
		{
			var boFactory = new BusinessObjectFactory();

			var orgHeader1 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "CAREDISYD";
			var orgHeader2 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "KGHOMESYD";
			var orgHeader3 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CMACGMSYD";
			var orgHeader4 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader4.OH_Code = "ROGBROUSC";

			var orgAddress1 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress1.AddressCode = "72 O'RIORDAN STREET";
			orgAddress1.OA_OH = orgHeader1.PK;
			var orgAddress2 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress2.AddressCode = "8 PARKVIEW ROAD";
			orgAddress2.OA_OH = orgHeader2.PK;
			var orgAddress3 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress3.AddressCode = "19-21 Pirrama Road";
			orgAddress3.OA_OH = orgHeader3.PK;
			var orgAddress4 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress4.AddressCode = "101 Trade Zone Dr Ste 7a";
			orgAddress4.OA_OH = orgHeader4.PK;

			boFactory.Save();

			var multipleCYDContainersDLV = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Multiple CYD containers with different addresses (DLV).xml");
			var message = GetQueuedUniversalShipmentMessage(multipleCYDContainersDLV);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var booking = boFactory.LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000001"));

			CombineAssertions(() =>
			{
				AssertEquals("Packages on booking", 2, booking.PackageJob.Packages.Count(p => !p.KP_IsUnknownQty));
				AssertEquals("Total # of instructions should be correct", 4, booking.Instructions.Count);

				var cydInstructionsCount = 0;

				foreach (DtbBookingInstruction instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
				{
					cydInstructionsCount += 1;
					var containers = new List<PkgPackage>();
					foreach (DtbBookingInstructionPkgDivot divot in instruction.PackageDivots)
					{
						containers.Add(divot.Package);
					}

					AssertEquals("Packages on instruction", 1, containers.Count);
					AssertEquals(true, containers.Single().KP_PackageID == "AAAAAAAAAA" || containers.Single().KP_PackageID == "BBBBBBBBBB");

					if (containers.Single().KP_PackageID == "AAAAAAAAAA")
					{
						AssertEquals("72 O'RIORDAN STREET", instruction.Address.RealAddress.AddressCode);
					}
					else if (containers.Single().KP_PackageID == "BBBBBBBBBB")
					{
						AssertEquals("101 Trade Zone Dr Ste 7a", instruction.Address.RealAddress.AddressCode);
					}
				}

				AssertEquals("Total CYD instructions should be correct", 2, cydInstructionsCount);

				var orderedInstructions = booking.Instructions.OrderBy(i => i.KN_Sequence).ToList();

				AssertEquals(OrganisationTypesList.Codes.CYD, orderedInstructions[2].OrganisationType);
				AssertEquals("Instruction 3 should have drop mode enabled", "WUP", orderedInstructions[2].DropMode);

				AssertEquals(OrganisationTypesList.Codes.CYD, orderedInstructions[3].OrganisationType);
				AssertEquals("Instruction 4 should have drop mode enabled", "WUP", orderedInstructions[3].DropMode);

				AssertEquals(1, orderedInstructions[0].KN_Sequence);
				AssertEquals(2, orderedInstructions[1].KN_Sequence);
				AssertEquals(3, orderedInstructions[2].KN_Sequence);
				AssertEquals(4, orderedInstructions[3].KN_Sequence);
			});
		}

		public void TestReadFromPICTestFile_TBHasFewerContainersThanParent_OnlyCreatesCYDInstructionsForTBContainer()
		{
			var boFactory = new BusinessObjectFactory();

			var orgHeader1 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "CAREDISYD";
			var orgHeader2 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "KGHOMESYD";
			var orgHeader3 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CMACGMSYD";
			var orgHeader4 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader4.OH_Code = "ROGBROUSC";

			var orgAddress1 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress1.AddressCode = "72 O'RIORDAN STREET";
			orgAddress1.OA_OH = orgHeader1.PK;
			var orgAddress2 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress2.AddressCode = "8 PARKVIEW ROAD";
			orgAddress2.OA_OH = orgHeader2.PK;
			var orgAddress3 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress3.AddressCode = "19-21 Pirrama Road";
			orgAddress3.OA_OH = orgHeader3.PK;
			var orgAddress4 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress4.AddressCode = "101 Trade Zone Dr Ste 7a";
			orgAddress4.OA_OH = orgHeader4.PK;

			boFactory.Save();

			var tbWithLessContainersThanParentPIC = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Multiple CYD containers on shipment but only one CYD container on TB (PIC).xml");
			var message = GetQueuedUniversalShipmentMessage(tbWithLessContainersThanParentPIC);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var booking = boFactory.LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000001"));

			CombineAssertions(() =>
			{
				AssertEquals("Packages on booking PackageJob is same as packages on shipment - current behaviour", 2, booking.PackageJob.Packages.Count(p => !p.KP_IsUnknownQty));
				AssertEquals("Total # of instructions should be correct", 3, booking.Instructions.Count);

				var cydInstructionsCount = 0;

				foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
				{
					cydInstructionsCount += 1;
					var containers = new List<PkgPackage>();
					foreach (var divot in instruction.PackageDivots)
					{
						containers.Add(divot.Package);
					}

					AssertEquals("Packages on instruction", 1, containers.Count);
					AssertEquals(true, containers.Single().KP_PackageID == "AAAAAAAAAA" || containers.Single().KP_PackageID == "BBBBBBBBBB");
					AssertEquals("8 PARKVIEW ROAD", instruction.Address.RealAddress.AddressCode);
				}

				AssertEquals("Total CYD instructions should be correct", 1, cydInstructionsCount);

				var orderedInstructions = booking.Instructions.OrderBy(i => i.KN_Sequence).ToList();

				AssertEquals(OrganisationTypesList.Codes.CYD, orderedInstructions[0].OrganisationType);
				AssertEquals("Instruction 1 should have drop mode enabled", "WUP", orderedInstructions[0].DropMode);

				AssertEquals(1, orderedInstructions[0].KN_Sequence);
				AssertEquals(2, orderedInstructions[1].KN_Sequence);
				AssertEquals(3, orderedInstructions[2].KN_Sequence);
			});
		}

		public void TestReadFromDLVTestFile_TBHasFewerContainersThanParent_OnlyCreatesCYDInstructionsForTBContainer()
		{
			var boFactory = new BusinessObjectFactory();

			var orgHeader1 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "CAREDISYD";
			var orgHeader2 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "KGHOMESYD";
			var orgHeader3 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CMACGMSYD";
			var orgHeader4 = boFactory.NewWithValidTestData<OrgHeader>();
			orgHeader4.OH_Code = "ROGBROUSC";

			var orgAddress1 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress1.AddressCode = "72 O'RIORDAN STREET";
			orgAddress1.OA_OH = orgHeader1.PK;
			var orgAddress2 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress2.AddressCode = "8 PARKVIEW ROAD";
			orgAddress2.OA_OH = orgHeader2.PK;
			var orgAddress3 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress3.AddressCode = "19-21 Pirrama Road";
			orgAddress3.OA_OH = orgHeader3.PK;
			var orgAddress4 = boFactory.NewWithValidTestData<OrgAddress>();
			orgAddress4.AddressCode = "101 Trade Zone Dr Ste 7a";
			orgAddress4.OA_OH = orgHeader4.PK;

			boFactory.Save();

			var multipleCYDContainersDLV = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.TransportBooking - Multiple CYD containers on shipment but only one CYD container on TB (DLV).xml");
			var message = GetQueuedUniversalShipmentMessage(multipleCYDContainersDLV);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var booking = boFactory.LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, "TB00000001"));

			CombineAssertions(() =>
			{
				AssertEquals("Packages on booking PackageJob is same as packages on shipment - current behaviour", 2, booking.PackageJob.Packages.Count(p => !p.KP_IsUnknownQty));
				AssertEquals("Total # of instructions should be correct", 3, booking.Instructions.Count);

				var cydInstructionsCount = 0;

				foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
				{
					cydInstructionsCount += 1;
					var containers = new List<PkgPackage>();
					foreach (var divot in instruction.PackageDivots)
					{
						containers.Add(divot.Package);
					}

					AssertEquals("Packages on instruction", 1, containers.Count);
					AssertEquals(true, containers.Single().KP_PackageID == "AAAAAAAAAA" || containers.Single().KP_PackageID == "BBBBBBBBBB");
					AssertEquals("72 O'RIORDAN STREET", instruction.Address.RealAddress.AddressCode);
				}

				AssertEquals("Total CYD instructions should be correct", 1, cydInstructionsCount);

				var orderedInstructions = booking.Instructions.OrderBy(i => i.KN_Sequence).ToList();

				AssertEquals(OrganisationTypesList.Codes.CYD, orderedInstructions[2].OrganisationType);
				AssertEquals("Instruction 3 should have drop mode enabled", "WUP", orderedInstructions[2].DropMode);

				AssertEquals(1, orderedInstructions[0].KN_Sequence);
				AssertEquals(2, orderedInstructions[1].KN_Sequence);
				AssertEquals(3, orderedInstructions[2].KN_Sequence);
			});
		}

		public void TestInstructionsAreSplitCorrectly_WhenMoreThanOneContainerYardInstructionExists()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");

			var boFactory = new BusinessObjectFactory();

			var orgHeader = Helper.CreateOrgHeader(boFactory, "OOOO");

			var orgAddress1 = Helper.CreateOrgAddress(boFactory, orgHeader, "XXXX");
			var orgAddress2 = Helper.CreateOrgAddress(boFactory, orgHeader, "YYYY");
			var orgAddress3 = Helper.CreateOrgAddress(boFactory, orgHeader, "ZZZZ");
			var orgAddress4 = Helper.CreateOrgAddress(boFactory, orgHeader, "WWWW");

			var cydInstruction1 = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress1);
			var cydInstruction2 = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress2);
			var shipment = Helper.CreateShipmentWithContainerCollection();

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var package1 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction1, "AAA");
			var package2 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction1, "BBB");
			var package3 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction2, "CCC");
			var package4 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction2, "DDD");

			var container1AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };
			var container2AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress2.AddressCode) };
			var container3AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress3.AddressCode) };
			var container4AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress4.AddressCode) };

			var container1 = Helper.CreateUniversalContainerOnShipment("AAA", container1AddressCollection, shipment, 1);
			var container2 = Helper.CreateUniversalContainerOnShipment("BBB", container2AddressCollection, shipment, 2);
			var container3 = Helper.CreateUniversalContainerOnShipment("CCC", container3AddressCollection, shipment, 3);
			var container4 = Helper.CreateUniversalContainerOnShipment("DDD", container4AddressCollection, shipment, 4);

			AssertEquals(4, booking.Instructions.Count);

			var currentSequence = 1;

			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}
			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType != OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}

			Factory.SaveForTesting();
			boFactory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, package1 },
				{ 2, package2 },
				{ 3, package3 },
				{ 4, package4 }
			};

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, ufactory, consolidation, shipment, shipment, booking, packageContainerLinks);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertContainerYardInstructionsAreSplitCorrectly(bookingFromReader, shipment, 6, 4, packageContainerLinks, 1);
		}

		public void TestInstructionsAreSplitCorrectly_WhenContainerCollectionHasMultipleContainersWithSameID()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");

			var boFactory = new BusinessObjectFactory();

			var orgHeader = Helper.CreateOrgHeader(boFactory, "OOOO");

			var orgAddress1 = Helper.CreateOrgAddress(boFactory, orgHeader, "XXXX");
			var orgAddress2 = Helper.CreateOrgAddress(boFactory, orgHeader, "YYYY");
			var orgAddress3 = Helper.CreateOrgAddress(boFactory, orgHeader, "ZZZZ");

			var cydInstruction = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress1);
			var shipment = Helper.CreateShipmentWithContainerCollection();

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var package1 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "AAA");
			var package2 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "AAA");
			var package3 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, null);
			var package4 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, null);

			var container1AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };
			var container2AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress2.AddressCode) };
			var container3AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress2.AddressCode) };
			var container4AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress3.AddressCode) };

			var container1 = Helper.CreateUniversalContainerOnShipment("AAA", container1AddressCollection, shipment, 1);
			var container2 = Helper.CreateUniversalContainerOnShipment("AAA", container2AddressCollection, shipment, 2);
			var container3 = Helper.CreateUniversalContainerOnShipment("", container3AddressCollection, shipment, 3);
			var container4 = Helper.CreateUniversalContainerOnShipment("", container4AddressCollection, shipment, 4);

			AssertEquals("Total instructions should be 3.", 3, booking.Instructions.Count);

			var currentSequence = 1;

			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}
			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType != OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}

			Factory.SaveForTesting();
			boFactory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, package1 },
				{ 2, package2 },
				{ 3, package3 },
				{ 4, package4 }
			};

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, ufactory, consolidation, shipment, shipment, booking, packageContainerLinks);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertContainerYardInstructionsAreSplitCorrectly(bookingFromReader, shipment, 5, 3, packageContainerLinks, testForContainerAmountsOnInstructions: false);
		}

		public void TestInstructionsAreSplitCorrectly_ContainerCollectionHasNullOrganizationAddressCollection_AddsNewContainerYardInstruction()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");

			var boFactory = new BusinessObjectFactory();

			var orgHeader = Helper.CreateOrgHeader(boFactory, "OOOO");

			var orgAddress1 = Helper.CreateOrgAddress(boFactory, orgHeader, "XXXX");
			var orgAddress2 = Helper.CreateOrgAddress(boFactory, orgHeader, "YYYY");

			var cydInstruction = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress1);
			var shipment = Helper.CreateShipmentWithContainerCollection();

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var package1 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "AAA");
			var package2 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "BBB");

			var container1AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };

			var container1 = Helper.CreateUniversalContainerOnShipment("AAA", container1AddressCollection, shipment, 1);
			var container2 = Helper.CreateUniversalContainerOnShipment("BBB", null, shipment, 2);

			AssertEquals("Total instructions should be 3.", 3, booking.Instructions.Count);

			var currentSequence = 1;

			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}
			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType != OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}

			Factory.SaveForTesting();
			boFactory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, package1 },
				{ 2, package2 }
			};

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, ufactory, consolidation, shipment, shipment, booking, packageContainerLinks);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertContainerYardInstructionsAreSplitCorrectly(bookingFromReader, shipment, 4, 2, packageContainerLinks, 1);
			AssertEquals("There should be one CYD instruction on the booking with a blank address", 1, bookingFromReader.Instructions.Count(i => i.OrganisationType == OrganisationTypesList.Codes.CYD && i.Address.RealAddress == null));
		}

		public void TestInstructionsAreSplitCorrectly_ContainerCollectionOrganizationAddressCollectionIsEmpty_AddsNewContainerYardInstruction()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");

			var boFactory = new BusinessObjectFactory();

			var orgHeader = Helper.CreateOrgHeader(boFactory, "OOOO");

			var orgAddress1 = Helper.CreateOrgAddress(boFactory, orgHeader, "XXXX");
			var orgAddress2 = Helper.CreateOrgAddress(boFactory, orgHeader, "YYYY");

			var cydInstruction = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress1);
			var shipment = Helper.CreateShipmentWithContainerCollection();

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var package1 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "AAA");
			var package2 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "BBB");

			var container1AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };

			var container1 = Helper.CreateUniversalContainerOnShipment("AAA", container1AddressCollection, shipment, 1);
			var container2 = Helper.CreateUniversalContainerOnShipment("BBB", new List<OrganizationAddress>(), shipment, 2);

			AssertEquals("Total instructions should be 3.", 3, booking.Instructions.Count);

			var currentSequence = 1;

			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}
			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType != OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}

			Factory.SaveForTesting();
			boFactory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, package1 },
				{ 2, package2 }
			};

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, ufactory, consolidation, shipment, shipment, booking, packageContainerLinks);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertContainerYardInstructionsAreSplitCorrectly(bookingFromReader, shipment, 4, 2, packageContainerLinks, 1);
			AssertEquals("There should be one CYD instruction on the booking with a blank address", 1, bookingFromReader.Instructions.Count(i => i.OrganisationType == OrganisationTypesList.Codes.CYD && i.Address.RealAddress == null));
		}

		public void TestInstructionsAreSplitCorrectly_ContainerCollectionOrganizationAddressCollectionContainsNullOrganizationAddress_AddsNewContainerYardInstruction()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");

			var boFactory = new BusinessObjectFactory();

			var orgHeader = Helper.CreateOrgHeader(boFactory, "OOOO");

			var orgAddress1 = Helper.CreateOrgAddress(boFactory, orgHeader, "XXXX");
			var orgAddress2 = Helper.CreateOrgAddress(boFactory, orgHeader, "YYYY");

			var cydInstruction = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress1);
			var shipment = Helper.CreateShipmentWithContainerCollection();

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var package1 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "AAA");
			var package2 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "BBB");

			var container1AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };
			var container2AddressCollection = new List<OrganizationAddress> { null };

			var container1 = Helper.CreateUniversalContainerOnShipment("AAA", container1AddressCollection, shipment, 1);
			var container2 = Helper.CreateUniversalContainerOnShipment("BBB", container2AddressCollection, shipment, 2);

			AssertEquals("Total instructions should be 3.", 3, booking.Instructions.Count);

			var currentSequence = 1;

			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}
			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType != OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}

			Factory.SaveForTesting();
			boFactory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, package1 },
				{ 2, package2 }
			};

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, ufactory, consolidation, shipment, shipment, booking, packageContainerLinks);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertContainerYardInstructionsAreSplitCorrectly(bookingFromReader, shipment, 4, 2, packageContainerLinks, 1);
			AssertEquals("There should be one CYD instruction on the booking with a blank address", 1, bookingFromReader.Instructions.Count(i => i.OrganisationType == OrganisationTypesList.Codes.CYD && i.Address.RealAddress == null));
		}

		public void TestInstructionsAreSplitCorrectly_ContainerCollectionOrganizationAddressCollectionContainsNullOrganizationAddressFollowedByNotNull_SecondContainerAddressMatchWithFirst_DoesNotAddNewContainerYardInstruction()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");

			var boFactory = new BusinessObjectFactory();

			var orgHeader = Helper.CreateOrgHeader(boFactory, "OOOO");

			var orgAddress1 = Helper.CreateOrgAddress(boFactory, orgHeader, "XXXX");
			var orgAddress2 = Helper.CreateOrgAddress(boFactory, orgHeader, "YYYY");

			var cydInstruction = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress1);
			var shipment = Helper.CreateShipmentWithContainerCollection();

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var package1 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "AAA");
			var package2 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "BBB");

			var container1AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };
			var container2AddressCollection = new List<OrganizationAddress> { null, Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };

			var container1 = Helper.CreateUniversalContainerOnShipment("AAA", container1AddressCollection, shipment, 1);
			var container2 = Helper.CreateUniversalContainerOnShipment("BBB", container2AddressCollection, shipment, 2);

			AssertEquals("Total instructions should be 3.", 3, booking.Instructions.Count);

			var currentSequence = 1;

			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}
			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType != OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}

			Factory.SaveForTesting();
			boFactory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, package1 },
				{ 2, package2 }
			};

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, ufactory, consolidation, shipment, shipment, booking, packageContainerLinks);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertContainerYardInstructionsAreSplitCorrectly(bookingFromReader, shipment, 3, 1, packageContainerLinks, 2);
		}

		public void TestInstructionsAreSplitCorrectly_ContainerCollectionOrganizationAddressCollectionContainsNullOrganizationAddressFollowedByNotNull_SecondContainerAddressNotMatchWithFirst_AddNewContainerYardInstruction()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");

			var boFactory = new BusinessObjectFactory();

			var orgHeader = Helper.CreateOrgHeader(boFactory, "OOOO");

			var orgAddress1 = Helper.CreateOrgAddress(boFactory, orgHeader, "XXXX");
			var orgAddress2 = Helper.CreateOrgAddress(boFactory, orgHeader, "YYYY");

			var cydInstruction = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress1);
			var shipment = Helper.CreateShipmentWithContainerCollection();

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var package1 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "AAA");
			var package2 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "BBB");

			var container1AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };
			var container2AddressCollection = new List<OrganizationAddress> { null, Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress2.AddressCode) };

			var container1 = Helper.CreateUniversalContainerOnShipment("AAA", container1AddressCollection, shipment, 1);
			var container2 = Helper.CreateUniversalContainerOnShipment("BBB", container2AddressCollection, shipment, 2);

			AssertEquals("Total instructions should be 3.", 3, booking.Instructions.Count);

			var currentSequence = 1;

			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}
			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType != OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}

			Factory.SaveForTesting();
			boFactory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, package1 },
				{ 2, package2 },
			};

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, ufactory, consolidation, shipment, shipment, booking, packageContainerLinks);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertContainerYardInstructionsAreSplitCorrectly(bookingFromReader, shipment, 4, 2, packageContainerLinks, 1);
		}

		public void TestInstructionsAreSplitCorrectly_WhenContainerHasNoApplicableAddressToMatchOnShouldFallBackOnShipmentAddresses()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");

			var boFactory = new BusinessObjectFactory();

			var orgHeader = Helper.CreateOrgHeader(boFactory, "OOOO");

			var orgAddress1 = Helper.CreateOrgAddress(boFactory, orgHeader, "XXXX");
			var orgAddress2 = Helper.CreateOrgAddress(boFactory, orgHeader, "YYYY");

			var cydInstruction = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress1);
			var shipment = Helper.CreateShipmentWithContainerCollection();
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipment.OrganizationAddressCollection.Add(Helper.CreateContainerYardOrganizationAddress((ZString?)nameof(DocAddressType.CustomsContainerYardAddress), orgHeader, orgAddress2.AddressCode));

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var package1 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "AAA");
			var package2 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "BBB");

			var container1AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };
			var container2AddressCollection = new List<OrganizationAddress> { null, Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyReturnAddress, orgHeader, orgAddress2.AddressCode) };

			var container1 = Helper.CreateUniversalContainerOnShipment("AAA", container1AddressCollection, shipment, 1);
			var container2 = Helper.CreateUniversalContainerOnShipment("BBB", container2AddressCollection, shipment, 2);

			AssertEquals("Total instructions should be 3.", 3, booking.Instructions.Count);

			var currentSequence = 1;

			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}
			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType != OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}

			Factory.SaveForTesting();
			boFactory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, package1 },
				{ 2, package2 },
			};

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, ufactory, consolidation, shipment, shipment, booking, packageContainerLinks);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertContainerYardInstructionsAreSplitCorrectly(bookingFromReader, shipment, 4, 2, packageContainerLinks, 1);
		}

		public void TestInstructionsAreSplitCorrectly_WhenContainerHasNoApplicableAddressToMatchOnAndNoFallback_ShouldCreateInstructionWithBlankAddress()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");

			var boFactory = new BusinessObjectFactory();

			var orgHeader = Helper.CreateOrgHeader(boFactory, "OOOO");

			var orgAddress1 = Helper.CreateOrgAddress(boFactory, orgHeader, "XXXX");
			var orgAddress2 = Helper.CreateOrgAddress(boFactory, orgHeader, "YYYY");

			var cydInstruction = Helper.CreateContainerYardInstructionForBooking(booking, InstructionTypes.Codes.PickUp, orgAddress1);
			var shipment = Helper.CreateShipmentWithContainerCollection();

			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var package1 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "AAA");
			var package2 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "BBB");
			var package3 = Helper.CreateContainerPkgPackageOnInstruction(cydInstruction, "CCC");

			var container1AddressCollection = new List<OrganizationAddress> { Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyPickupAddress, orgHeader, orgAddress1.AddressCode) };
			var container2AddressCollection = new List<OrganizationAddress> { null, Helper.CreateContainerYardOrganizationAddress(AddressTypes.ContainerYardEmptyReturnAddress, orgHeader, orgAddress2.AddressCode) };
			var container3AddressCollection = new List<OrganizationAddress> { null };

			var container1 = Helper.CreateUniversalContainerOnShipment("AAA", container1AddressCollection, shipment, 1);
			var container2 = Helper.CreateUniversalContainerOnShipment("BBB", container2AddressCollection, shipment, 2);
			var container3 = Helper.CreateUniversalContainerOnShipment("CCC", container3AddressCollection, shipment, 3);

			AssertEquals("Total instructions should be 3.", 3, booking.Instructions.Count);

			var currentSequence = 1;

			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}
			foreach (var instruction in booking.Instructions.Where(i => i.OrganisationType != OrganisationTypesList.Codes.CYD))
			{
				instruction.KN_Sequence = currentSequence;
				currentSequence++;
			}

			Factory.SaveForTesting();
			boFactory.Save();

			var packageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, package1 },
				{ 2, package2 },
				{ 3, package3 }
			};

			var reader = new DtbBookingDataObjectReaderFromCartageAdvice(shipment, Logger, ufactory, consolidation, shipment, shipment, booking, packageContainerLinks);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertContainerYardInstructionsAreSplitCorrectly(bookingFromReader, shipment, 4, 2, packageContainerLinks, testForContainerAmountsOnInstructions: false);

			var blankAddressInstructions = bookingFromReader.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD && i.Address.RealAddress == null);
			AssertEquals("There should be one CYD instruction on the booking with a blank address", 1, blankAddressInstructions.Count());
			AssertEquals("Blank address instruction should have 2 containers attached", 2, blankAddressInstructions.Single().PackageDivots.Count);
		}

		public void TestPopulateBusinessObject()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "EECR" };
			shipment.ServiceLevel = new ServiceLevel { Code = "SVC" };
			shipment.CarrierServiceLevel = new ServiceLevel { Code = "CSV" };
			shipment.TransportBookingDirection = new TransportBookingDirection { Code = "IMP" };
			shipment.IsHazardous = true;
			shipment.RatingTransportMode = new CodeDescriptionPair { Code = RatingFreightModes.Codes.Loose };
			shipment.RequiresRefrigeration = true;

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			container.ContainerNumber = "BOO";
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(container);

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertEquals("EECR", bookingFromReader.KM_KT_NKBookingTemplate);
			AssertEquals("SVC", bookingFromReader.KM_RS_NKServiceLevel);
			AssertEquals("CSV", bookingFromReader.KM_PL_NKCarrierServiceLevel);
			AssertEquals("ORG", bookingFromReader.KM_Direction);
			AssertEquals(true, bookingFromReader.KM_IsHazardous);
			AssertEquals(RatingFreightModes.Codes.Loose, bookingFromReader.KM_RatingFreightMode);
			AssertEquals(true, bookingFromReader.KM_RequiresRefrigeration);
			AssertEquals(0, bookingFromReader.Containers.Count());
		}

		public void TestPopulateBusinessObject_UpdatesTransportRefAndAddress()
		{
			var bookingParent = Factory.New<DummyWithDtbBooking>();
			bookingParent.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.DLV };
			var consol = Helper.CreateConsolidation(bookingParent);
			var booking = consol.Bookings.AddNew();
			AssertEquals("Precondition", "Trans Ref", booking.KM_TransportReference);
			AssertEquals("Precondition", "Company123", booking.Address.E2_CompanyName);

			var writeManager = new DataWritingManager(new ActionInfo(null, bookingParent));

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "New Transport Co";
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "New Trans Ref" };
			shipment.AddOrgAddress(writeManager, org.MainAddress, DocAddressType.TransportCompanyDocumentaryAddress);

			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			var reader1 = GetNewReader(shipment, Logger, consol, shipment, booking);
			var bookingFromReader1 = reader1.ReadIntoBusinessObject();

			AssertEquals("Reference should be overriden.", "New Trans Ref", bookingFromReader1.KM_TransportReference);
			AssertEquals("Transport Company should be overriden with an unmatched org.", "UNMATCHED ORGANISATION", bookingFromReader1.Address.E2_CompanyName);

			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(false);
			var reader2 = GetNewReader(shipment, Logger, consol, shipment, booking);
			var bookingFromReader2 = reader2.ReadIntoBusinessObject();

			AssertEquals("Reference should be overriden.", "New Trans Ref", bookingFromReader2.KM_TransportReference);
			AssertEquals("Transport Company should be overriden.", "New Transport Co", bookingFromReader2.Address.E2_CompanyName);
		}

		public void TestPopulateBusinessObject_PopulateAddresses()
		{
			var bookingParent = Factory.New<DummyWithDtbBooking>();
			bookingParent.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.DLV };
			var consol = Helper.CreateConsolidation(bookingParent);
			var booking = consol.Bookings.AddNew();

			var writeManager = new DataWritingManager(new ActionInfo(null, bookingParent));

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "New Local Client Co";
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing = new LocalProcessing() { ArrivalCartageRef = "New Trans Ref" };
			shipment.AddOrgAddress(writeManager, org.MainAddress, DocAddressType.ClientRequestedBillingParty);

			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
			var reader1 = GetNewReader(shipment, Logger, consol, shipment, booking);
			var bookingFromReader1 = reader1.ReadIntoBusinessObject();
			AssertEquals("Transport Company should be overriden with an unmatched org.", "UNMATCHED ORGANISATION", bookingFromReader1.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty).E2_CompanyName);

			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(false);
			var reader2 = GetNewReader(shipment, Logger, consol, shipment, booking);
			var bookingFromReader2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Transport Company should be overriden.", "New Local Client Co", bookingFromReader2.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty).E2_CompanyName);
		}

		public void TestPopulateBusinessObject_PopulateAddresses_ForBookingWithForwardingShipmentParent()
		{
			var shippingLineAddressLine1 = "1 Shipping Line Lane";
			var shippingLineOrg = Helper.CreateOrganisation("SHIPLINE", isDebtor: false, address1: shippingLineAddressLine1);
			var shippingLineAddress = shippingLineOrg.MainAddress;

			var forwardingShipment = Helper.CreateForwardingShipment("S1", "HB1", "SEA", "LSE");
			var forwardingConsol = Helper.CreateForwardingConsol(forwardingShipment, "C1", "SEA", "MB1");
			forwardingConsol.JK_OA_ShippingLineAddress = shippingLineAddress.PK;

			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingShipment);
			var booking = Helper.CreateBooking(bookingConsolidation);

			var topLevelUniversalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelUniversalShipment.DataContext = DataContextFactory.New();
			topLevelUniversalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C1");
			topLevelUniversalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1");
			var shippingLineAddressDO = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			shippingLineAddressDO.Address1 = shippingLineAddressLine1;
			shippingLineAddressDO.AddressType = nameof(DocAddressType.ShippingLineAddress);
			topLevelUniversalShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			topLevelUniversalShipment.OrganizationAddressCollection.Add(shippingLineAddressDO);

			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1");

			topLevelUniversalShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			topLevelUniversalShipment.SubShipmentCollection.Add(universalShipment);

			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(false);
			var reader = GetNewReader(universalShipment, Logger, bookingConsolidation, topLevelUniversalShipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			CombineAssertions("Check that Shipping Line Address is correct on booking", () =>
			{
				var shippingLineAddress = bookingFromReader.DocAddresses.FindByDocAddressType(DocAddressType.ShippingLineAddress);
				AssertNotNull("Should have populated Shipping Line Address from the forwardingConsol", shippingLineAddress);
				if (shippingLineAddress != null)
				{
					AssertEquals("Shipping Line address line 1 should match Address line 1 of ShippingLineAddress on forwardingConsol", shippingLineAddressLine1, shippingLineAddress.Address1);
				}
			});
		}

		public void TestPopulateBusinessObject_PopulateAddresses_ForBookingWithForwardingConsolParent()
		{
			var shippingLineAddressLine1 = "1 Shipping Line Lane";
			var shippingLineOrg = Helper.CreateOrganisation("SHIPLINE", isDebtor: false, address1: shippingLineAddressLine1);
			var shippingLineAddress = shippingLineOrg.MainAddress;

			var forwardingShipment = Helper.CreateForwardingShipment("S1", "HB1", "SEA", "LSE");
			var forwardingConsol = Helper.CreateForwardingConsol(forwardingShipment, "C1", "SEA", "MB1");
			forwardingConsol.JK_OA_ShippingLineAddress = shippingLineAddress.PK;

			var bookingConsolidation = Helper.CreateConsolidation((IDtbBookingParent)forwardingConsol);
			var booking = Helper.CreateBooking(bookingConsolidation);

			var topLevelUniversalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelUniversalShipment.DataContext = DataContextFactory.New();
			topLevelUniversalShipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C1");
			var shippingLineAddressDO = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			shippingLineAddressDO.Address1 = shippingLineAddressLine1;
			shippingLineAddressDO.AddressType = nameof(DocAddressType.ShippingLineAddress);
			topLevelUniversalShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			topLevelUniversalShipment.OrganizationAddressCollection.Add(shippingLineAddressDO);

			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1");

			topLevelUniversalShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			topLevelUniversalShipment.SubShipmentCollection.Add(universalShipment);

			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(false);
			var reader = GetNewReader(universalShipment, Logger, bookingConsolidation, topLevelUniversalShipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			CombineAssertions("Check that Shipping Line Address is correct on booking", () =>
			{
				var shippingLineAddress = bookingFromReader.DocAddresses.FindByDocAddressType(DocAddressType.ShippingLineAddress);
				AssertNotNull("Should have populated Shipping Line Address from the forwardingConsol", shippingLineAddress);
				if (shippingLineAddress != null)
				{
					AssertEquals("Shipping Line address line 1 should match Address line 1 of ShippingLineAddress on forwardingConsol", shippingLineAddressLine1, shippingLineAddress.Address1);
				}
			});
		}

		public void TestAddressHandling_HasParent_CarrierBookingAgent_ShouldNotPopulate()
		{
			var bookingParent = Factory.New<DummyWithDtbBooking>();
			var consol = Helper.CreateConsolidation(bookingParent);
			var booking = consol.Bookings.AddNew();

			var writeManager = new DataWritingManager(new ActionInfo(null, bookingParent));

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Defect Co";

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.AddOrgAddress(writeManager, org.MainAddress, DocAddressType.CarrierBookingAgent);

			var reader = GetNewReader(shipment, Logger, consol, shipment, booking);
			reader.ReadIntoBusinessObject();

			var docAddresses = booking.DocAddresses;
			var docAddressType = DocAddressType.CarrierBookingAgent;

			var hasDocAddressType = docAddresses.Cast<JobDocAddress>().Any(a => a.DocAddressType == docAddressType);

			Assert("There should be no CarrierBookingAgent type address when there is a parent.", !hasDocAddressType);
		}

		public void TestAddressHandling_NoParent_CarrierBookingAgent_ShouldPopulate()
		{
			var consol = Helper.CreateConsolidation();
			var booking = consol.Bookings.AddNew();

			var writeManager = new DataWritingManager(new ActionInfo(null, booking));

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Defect Co";

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.AddOrgAddress(writeManager, org.MainAddress, DocAddressType.CarrierBookingAgent);

			var reader = GetNewReader(shipment, Logger, consol, shipment, booking);
			reader.ReadIntoBusinessObject();

			var docAddresses = booking.DocAddresses;
			var docAddressType = DocAddressType.CarrierBookingAgent;

			var hasDocAddressType = docAddresses.Cast<JobDocAddress>().Any(a => a.DocAddressType == docAddressType);

			Assert("There should be CarrierBookingAgent type address when there is no parent.", hasDocAddressType);
		}

		public void TestPopulateBusinessObject_PopulateBranch_NewRecord()
		{
			var mainFreightOrg = Helper.CreateOrganisation("MF");
			var mainFreightOrg_Sydney = Helper.CreateOrganisation("MFS", address1: "Sydney");
			var mainFreightOrg_Melbourne = Helper.CreateOrganisation("MFM", address1: "Melbourne");
			var mainFreightCompany = helper.CreateCompany("MFC", "Main Freight Company", mainFreightOrg);
			var mainFreightBranch_Sydney = Helper.CreateBranch("MBS", "Main Freight Sydney", mainFreightCompany, mainFreightOrg_Sydney);
			var mainFreightBranch_Melbourne = Helper.CreateBranch("MBM", "Main Freight Melbourne", mainFreightCompany, mainFreightOrg_Melbourne);

			var owensOrg = Helper.CreateOrganisation("OW");
			var owensOrg_Sydney = Helper.CreateOrganisation("OWS", address1: "Sydney");
			var owensOrg_Melbourne = Helper.CreateOrganisation("OWM", address1: "Melbourne");
			var owensCompany = helper.CreateCompany("OWC", "Main Freight Company", owensOrg);
			var owensBranch_Sydney = Helper.CreateBranch("OBS", "Main Freight Sydney", owensCompany, owensOrg_Sydney);
			var owensBranch_Melbourne = Helper.CreateBranch("OBM", "Main Freight Melbourne", owensCompany, owensOrg_Melbourne);

			var randomOrg = Helper.CreateOrganisation("RND", address1: "Random");
			var unmatchedOrg = new TransportBookingTestHelper(new BusinessObjectFactory()).CreateOrganisation("UNM", address1: "Unmatched");

			Factory.SaveForTesting();

			// External TB (eg Forwarder) --> Transport TB
			AssertBranch(mainFreightOrg, GlbBranch.CurrentBranch);
			AssertBranch(mainFreightOrg_Sydney, mainFreightBranch_Sydney);
			AssertBranch(mainFreightOrg_Melbourne, mainFreightBranch_Melbourne);
			AssertBranch(owensOrg, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg_Sydney, owensBranch_Sydney);
			AssertBranch(owensOrg_Melbourne, owensBranch_Melbourne);
			AssertBranch(randomOrg, GlbBranch.CurrentBranch);
			AssertBranch(unmatchedOrg, GlbBranch.CurrentBranch);
			AssertBranch(null, GlbBranch.CurrentBranch);

			// Internal Forwarder/Customs/Warehouse --> Transport TB
			var outboundWriter = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			Logger.OutboundSessionTracker = outboundWriter;
			AssertBranch(mainFreightOrg, GlbBranch.CurrentBranch);
			AssertBranch(mainFreightOrg_Sydney, GlbBranch.CurrentBranch);
			AssertBranch(mainFreightOrg_Melbourne, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg_Sydney, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg_Melbourne, GlbBranch.CurrentBranch);
			AssertBranch(randomOrg, GlbBranch.CurrentBranch);
			AssertBranch(unmatchedOrg, GlbBranch.CurrentBranch);
			AssertBranch(null, GlbBranch.CurrentBranch);

			// set controlling party on Orgs don't have a direct BranchProxy (should be used instead of Current Branch)
			// OrgCompanyData record is based on Logged in Company, which on uxml import is chosen based on the receiving Company of the XML (essentially the Transport Co OrgHeader Company Proxy)
			mainFreightOrg.CompanyData.OB_GB_ControllingBranch = mainFreightBranch_Sydney.PK;
			owensOrg.CompanyData.OB_GB_ControllingBranch = mainFreightBranch_Sydney.PK;
			randomOrg.CompanyData.OB_GB_ControllingBranch = mainFreightBranch_Sydney.PK;

			// External TB (eg Forwarder) --> Transport TB
			Logger.OutboundSessionTracker = null;
			AssertBranch(mainFreightOrg, mainFreightBranch_Sydney);
			AssertBranch(owensOrg, mainFreightBranch_Sydney);
			AssertBranch(randomOrg, mainFreightBranch_Sydney);

			// Internal Forwarder/Customs/Warehouse --> Transport TB
			Logger.OutboundSessionTracker = outboundWriter;
			AssertBranch(mainFreightOrg, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg, GlbBranch.CurrentBranch);
			AssertBranch(randomOrg, GlbBranch.CurrentBranch);

			void AssertBranch(OrgHeader targetTransportCompany, GlbBranch expectedBranch)
			{
				var uShipment = SetupUniversalShipmentForPopulateBranchTest(targetTransportCompany);

				var reader = GetNewReader(uShipment, Logger, Factory, null, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
				var bookingRead = reader.ReadIntoBusinessObject();
				AssertEquals(expectedBranch.PK, bookingRead.KM_GB_Branch);
			}
		}

		public void TestPopulateBusinessObject_PopulateBranch_UpdateRecord()
		{
			var mainFreightOrg = Helper.CreateOrganisation("MF");
			var mainFreightOrg_Sydney = Helper.CreateOrganisation("MFS", address1: "Sydney");
			var mainFreightOrg_Melbourne = Helper.CreateOrganisation("MFM", address1: "Melbourne");
			var mainFreightCompany = helper.CreateCompany("MFC", "Main Freight Company", mainFreightOrg);
			var mainFreightBranch_Sydney = Helper.CreateBranch("MBS", "Main Freight Sydney", mainFreightCompany, mainFreightOrg_Sydney);
			var mainFreightBranch_Melbourne = Helper.CreateBranch("MBM", "Main Freight Melbourne", mainFreightCompany, mainFreightOrg_Melbourne);

			var owensOrg = Helper.CreateOrganisation("OW");
			var owensOrg_Sydney = Helper.CreateOrganisation("OWS", address1: "Sydney");
			var owensOrg_Melbourne = Helper.CreateOrganisation("OWM", address1: "Melbourne");
			var owensCompany = helper.CreateCompany("OWC", "Main Freight Company", owensOrg);
			var owensBranch_Sydney = Helper.CreateBranch("OBS", "Main Freight Sydney", owensCompany, owensOrg_Sydney);
			var owensBranch_Melbourne = Helper.CreateBranch("OBM", "Main Freight Melbourne", owensCompany, owensOrg_Melbourne);

			var randomOrg = Helper.CreateOrganisation("RND", address1: "Random");
			var unmatchedOrg = new TransportBookingTestHelper(new BusinessObjectFactory()).CreateOrganisation("UNM", address1: "Unmatched");

			var miscOrg = Helper.CreateOrganisation("ZO");
			var miscCompany = Helper.CreateCompany("ZCO", "Misc Company", miscOrg);
			var miscBranch = Helper.CreateBranch("ZB1", "Misc Branch 1", miscCompany, miscOrg);

			Factory.SaveForTesting();

			// External TB (eg Forwarder) --> Transport TB
			AssertBranch(mainFreightOrg, GlbBranch.CurrentBranch);
			AssertBranch(mainFreightOrg_Sydney, mainFreightBranch_Sydney);
			AssertBranch(mainFreightOrg_Melbourne, mainFreightBranch_Melbourne);
			AssertBranch(owensOrg, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg_Sydney, owensBranch_Sydney);
			AssertBranch(owensOrg_Melbourne, owensBranch_Melbourne);
			AssertBranch(randomOrg, GlbBranch.CurrentBranch);
			AssertBranch(unmatchedOrg, GlbBranch.CurrentBranch);
			AssertBranch(null, GlbBranch.CurrentBranch);

			// Internal Forwarder/Customs/Warehouse --> Transport TB
			var outboundWriter = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			Logger.OutboundSessionTracker = outboundWriter;
			AssertBranch(mainFreightOrg, GlbBranch.CurrentBranch);
			AssertBranch(mainFreightOrg_Sydney, GlbBranch.CurrentBranch);
			AssertBranch(mainFreightOrg_Melbourne, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg_Sydney, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg_Melbourne, GlbBranch.CurrentBranch);
			AssertBranch(randomOrg, GlbBranch.CurrentBranch);
			AssertBranch(unmatchedOrg, GlbBranch.CurrentBranch);
			AssertBranch(null, GlbBranch.CurrentBranch);

			// set controlling party on Orgs don't have a direct BranchProxy (should be used instead of Current Branch)
			// OrgCompanyData record is based on Logged in Company, which on uxml import is chosen based on the receiving Company of the XML (essentially the Transport Co OrgHeader Company Proxy)
			mainFreightOrg.CompanyData.OB_GB_ControllingBranch = mainFreightBranch_Sydney.PK;
			owensOrg.CompanyData.OB_GB_ControllingBranch = mainFreightBranch_Sydney.PK;
			randomOrg.CompanyData.OB_GB_ControllingBranch = mainFreightBranch_Sydney.PK;

			// External TB (eg Forwarder) --> Transport TB
			Logger.OutboundSessionTracker = null;
			AssertBranch(mainFreightOrg, mainFreightBranch_Sydney);
			AssertBranch(owensOrg, mainFreightBranch_Sydney);
			AssertBranch(randomOrg, mainFreightBranch_Sydney);

			// Internal Forwarder/Customs/Warehouse --> Transport TB
			Logger.OutboundSessionTracker = outboundWriter;
			AssertBranch(mainFreightOrg, GlbBranch.CurrentBranch);
			AssertBranch(owensOrg, GlbBranch.CurrentBranch);
			AssertBranch(randomOrg, GlbBranch.CurrentBranch);

			void AssertBranch(OrgHeader targetTransportCompany, GlbBranch expectedBranchWhenUpdated)
			{
				AssertUpdatesEmptyBranch(targetTransportCompany, expectedBranchWhenUpdated);
				AssertDoesNotUpdateNonEmptyBranch(targetTransportCompany);
			}

			void AssertUpdatesEmptyBranch(OrgHeader targetTransportCompany, GlbBranch expectedBranch)
			{
				var bookingRead = SetupBookingForUpdateBranchTest(targetTransportCompany, true);
				AssertEquals("Should have updated blank branch with expected branch", expectedBranch.PK, bookingRead.KM_GB_Branch);
			}

			void AssertDoesNotUpdateNonEmptyBranch(OrgHeader targetTransportCompany)
			{
				var bookingRead = SetupBookingForUpdateBranchTest(targetTransportCompany, false);
				AssertEquals("Should have not updated non-blank branch - value of branch should still be the misc branch", miscBranch.PK, bookingRead.KM_GB_Branch);
			}

			DtbBooking SetupBookingForUpdateBranchTest(OrgHeader targetTransportCompany, bool branchStartsAsBlank)
			{
				var uShipment = SetupUniversalShipmentForPopulateBranchTest(targetTransportCompany);

				var booking = Helper.CreateBooking(targetTransportCompany);
				booking.KM_GB_Branch = branchStartsAsBlank ? ZGuid.Empty : miscBranch.PK;
				Factory.SaveForTesting();

				var reader = GetNewReader(uShipment, Logger, Factory, booking, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
				return reader.ReadIntoBusinessObject();
			}
		}

		UniversalShipment SetupUniversalShipmentForPopulateBranchTest(OrgHeader targetTransportCompany)
		{
			var uShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			if (targetTransportCompany != null)
			{
				var dummyWriteManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				uShipment.AddOrgAddress(dummyWriteManager, targetTransportCompany.MainAddress, DocAddressType.TransportCompanyDocumentaryAddress);
			}
			return uShipment;
		}

		public void TestPopulateBusinessObject_DoesNotCreateAJobHeader()
		{
			var factory = new UniversalObjectFactory();
			var localClientDataObject = OrganizationAddressTestHelper.GetLocalClientAddress();
			new OrganisationDataObjectReader(localClientDataObject, new TestErrorLogger(), factory).GetMatchedOrNewForTesting();
			factory.SaveForTesting();

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { localClientDataObject });
			var consolidation = Helper.CreateConsolidation();
			Factory.SaveForTesting();

			var reader = GetNewReader(bookingDataObject, Logger, consolidation, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();

			AssertNull(new JobHeader.Loader(booking).Load());
		}

		public void TestPopulateBusinessObject_UpdateValuesNotExistInShipment()
		{
			var org = Helper.CreateOrganisation("Org1");
			var booking = Helper.CreateBooking(Helper.CreateConsolidation(), "EECR", "Desc", "ORG", "Ref");
			booking.KM_IsHazardous = false;
			booking.KM_RS_NKServiceLevel = "SVC";
			booking.KM_PL_NKCarrierServiceLevel = "CSV";
			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrgAddressType.Delivery.Code, org.MainAddress);
			var confirmation = pickupInstruction.FirstPickupConfirmation;
			Factory.SaveForTesting();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();

			var reader = GetNewReader(shipment, Logger, booking.ConsolidationSingleJob, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();
			AssertEquals("EECR", bookingFromReader.KM_KT_NKBookingTemplate);
			AssertEquals("Desc", bookingFromReader.KM_Description);
			AssertEquals("ORG", bookingFromReader.KM_Direction);
			AssertEquals("SVC", bookingFromReader.KM_RS_NKServiceLevel);
			AssertEquals("CSV", bookingFromReader.KM_PL_NKCarrierServiceLevel);
			AssertEquals("Ref", bookingFromReader.KM_TransportReference);
			AssertEquals(false, bookingFromReader.KM_IsHazardous);

			var instructionFromReader = bookingFromReader.Instructions.Single(i => i.PK == pickupInstruction.PK);
			AssertEquals(pickupInstruction, instructionFromReader);
			AssertEquals(confirmation, instructionFromReader.Confirmations.Single());
		}

		public void TestPopulateBusinessObject_ShipmentCarrierAccountNumber()
		{
			var bookingParent = Factory.New<DummyWithDtbBooking>();
			bookingParent.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.DLV };
			var consol = Helper.CreateConsolidation(bookingParent);
			var booking = consol.Bookings.AddNew();
			AssertEquals("Precondition", "Trans Ref", booking.KM_TransportReference);
			AssertEquals("Precondition", "Company123", booking.Address.E2_CompanyName);

			var writeManager = new DataWritingManager(new ActionInfo(null, bookingParent));

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "New Transport Co";
			org.OH_Code = "ZZAA";

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "New Trans Ref" };
			shipment.AddOrgAddress(writeManager, org.MainAddress, DocAddressType.TransportCompanyDocumentaryAddress);

			SetUseUnmatchedOrganisationForMatchingRegistry(true);

			var carrierAccount = Factory.New<OrgCarrierAccount>();
			carrierAccount.OAN_AccountNumber = "ACCNO1";
			carrierAccount.OAN_OH_Carrier = org.PK;
			carrierAccount.OAN_OH_BillToParty = org.PK;

			booking.KM_OAN_CarrierAccount = carrierAccount.PK;

			var ca = new CarrierAccount();
			ca.AccountNumber = "ACCNO1";
			shipment.CarrierAccount = ca;

			Factory.SaveForTesting();

			var reader = GetNewReader(shipment, Logger, booking.ConsolidationSingleJob, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertEquals(carrierAccount.PK, bookingFromReader.KM_OAN_CarrierAccount);

			shipment.CarrierAccount = null;

			Factory.SaveForTesting();

			reader = GetNewReader(shipment, Logger, booking.ConsolidationSingleJob, shipment, booking);
			bookingFromReader = reader.ReadIntoBusinessObject();

			AssertEquals(ZGuid.Empty, bookingFromReader.KM_OAN_CarrierAccount);
		}

		public void TestPopulateBusinessObject_ShipmentIsAuthorizedToLeave()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertEquals("Precondition: IsAuthorizedToLeave has no value", false, shipment.IsAuthorizedToLeave.HasValue);

			var reader1 = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader1 = reader1.ReadIntoBusinessObject();
			AssertEquals("PickUp instruction's IsAuthorisedToLeave remains false.", false, bookingFromReader1.Instructions.Single(instruction => instruction.KN_InstructionType == InstructionTypes.Codes.PickUp).KN_IsAuthorisedToLeave);
			AssertEquals("Delivery instruction's IsAuthorisedToLeave is not updated.", false, bookingFromReader1.Instructions.Single(instruction => instruction.KN_InstructionType == InstructionTypes.Codes.Delivery).KN_IsAuthorisedToLeave);

			shipment.IsAuthorizedToLeave = true;
			var reader2 = GetNewReader(shipment, Logger, consolidation, shipment, bookingFromReader1);
			var bookingFromReader2 = reader2.ReadIntoBusinessObject();
			AssertEquals("PickUp instruction's IsAuthorisedToLeave is set to false.", false, bookingFromReader2.Instructions.Single(instruction => instruction.KN_InstructionType == InstructionTypes.Codes.PickUp).KN_IsAuthorisedToLeave);
			AssertEquals("Delivery instruction's IsAuthorisedToLeave is set to true.", true, bookingFromReader2.Instructions.Single(instruction => instruction.KN_InstructionType == InstructionTypes.Codes.Delivery).KN_IsAuthorisedToLeave);
		}

		public void TestPopulateBusinessObject_ShipmentIsAuthorizedToLeave_BookingExistsWithValue()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");
			Factory.SaveForTesting();
			AssertEquals("Precondition: IsAuthorisedToLeave is false.", false, booking.Instructions.Single(instruction => instruction.KN_InstructionType == InstructionTypes.Codes.Delivery).KN_IsAuthorisedToLeave);
			AssertEquals("Precondition: IsAuthorisedToLeave is false.", false, booking.Instructions.Single(instruction => instruction.KN_InstructionType == InstructionTypes.Codes.PickUp).KN_IsAuthorisedToLeave);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.IsAuthorizedToLeave = true;

			var reader1 = GetNewReader(shipment, Logger, booking.ConsolidationSingleJob, shipment, booking);
			var bookingFromReader1 = reader1.ReadIntoBusinessObject();
			AssertEquals("Delivery IsAuthorisedToLeave is updated to true.", true, bookingFromReader1.Instructions.DeliveryInstructions.FirstOrDefault().KN_IsAuthorisedToLeave);
			AssertEquals("Pickup IsAuthorisedToLeave is still false.", false, bookingFromReader1.Instructions.PickUpInstructions.FirstOrDefault().KN_IsAuthorisedToLeave);

			shipment.IsAuthorizedToLeave = false;
			var reader2 = GetNewReader(shipment, Logger, booking.ConsolidationSingleJob, shipment, booking);
			var bookingFromReader2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Delivery IsAuthorisedToLeave is updated to false.", false, bookingFromReader2.Instructions.DeliveryInstructions.FirstOrDefault().KN_IsAuthorisedToLeave);
		}

		public void TestPopulateBusinessObject_BookingOfTransportRequestedDate()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = Helper.CreateBooking(consolidation, "DLCW", "Desc", "ORG", "Ref");
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertEquals("Precondition: Requested date has no value", ZDateTime.Empty, booking.KM_BookingOfTransportRequestedDate);

			var reader1 = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader1 = reader1.ReadIntoBusinessObject();
			AssertEquals("Requested date remains empty.", ZDateTime.Empty, bookingFromReader1.KM_BookingOfTransportRequestedDate);

			var now = ZDateTime.Now;
			var date = Date.New(DateType.TransportBookingRequested, true, now);
			shipment.SetDateCollection(() => new List<Date>());
			shipment.DateCollection.Add(date);

			var reader2 = GetNewReader(shipment, Logger, consolidation, shipment, bookingFromReader1);
			var bookingFromReader2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Requested date remains empty.", ZDateTime.Empty, bookingFromReader2.KM_BookingOfTransportRequestedDate);

			date.IsEstimate = false;
			var reader3 = GetNewReader(shipment, Logger, consolidation, shipment, bookingFromReader2);
			var bookingFromReader3 = reader3.ReadIntoBusinessObject();
			AssertEquals("Requested date has a value now.", now, bookingFromReader3.KM_BookingOfTransportRequestedDate);
		}

		[TestDate(2011, 1, 1)]
		public void TestWorkflowCustomFields()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "TBM";
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			bookingDataObject.CustomizedFieldCollection.Add("Textual context", new ZString("HELLO"));
			bookingDataObject.CustomizedFieldCollection.Add("First Date", new ZDateTime(2011, 1, 1));
			bookingDataObject.CustomizedFieldCollection.Add("Deci Deca", new ZDecimal(0.3));
			bookingDataObject.CustomizedFieldCollection.Add("Flagger", ZBool.True);
			bookingDataObject.CustomizedFieldCollection.Add("Integer Mate", new ZInt(42));
			bookingDataObject.CustomizedFieldCollection.Add(new ZString("TOO LONG").PadRight(GenCustomAddOnValueSchema.XV_Name.MaxLength + 1, '1'), new ZInt(2332));
			bookingDataObject.CustomizedFieldCollection.Add(new ZString("Data"), new ZString("DATA TOO LONG").PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, '1'));

			var consolidation = Helper.CreateConsolidation();
			Factory.SaveForTesting();

			var reader = GetNewReader(bookingDataObject, Logger, consolidation, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var customFields = booking.GetUserDefinedValues();
				var result = "";
				foreach (var customField in customFields)
				{
					result += customField.PropertyName + " - " + customField.Value + "\r\n";
				}

				AssertMultilineASCIIEquals("All Custom Fields should have been imported with none extra", string.Format(
@"Data - {1}
Deci Deca - 0.3
First Date - 01-Jan-11 00:00:00
Flagger - Y
Integer Mate - 42
Textual context - HELLO
{0} - 2332", "TOO LONG".PadRight(GenCustomAddOnValueSchema.XV_Name.MaxLength, '1').Trim(), "DATA TOO LONG".PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength, '1').Trim()), result);

				AssertEquals("There should be no errors", string.Format(
@"Attempted to insert 61 characters into Field [TOO LONG11111111111111111111111111111111111111111111111111111] which has a maximum length of 60 characters. Field was truncated.
Attempted to insert 101 characters into Field [Data] which has a maximum length of 100 characters. Field was truncated.", result), Logger.GetWarnings());
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(true, Logger.HasWarnings);
			});
		}

		public void TestPopulateTransportBookingPartyNumber()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();
			ufactory.SaveForTesting();

			// empty uShipment
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = GetNewReader(shipment, Logger, consolidation, shipment, null, ufactory);
			var bookingFromReader = reader.ReadIntoBusinessObject();
			var bookingNumber = bookingFromReader.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().FirstOrDefault(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
			AssertNull(bookingNumber);

			// uShipment with ExternalTransportBookingNumber
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference() { Type = new UniversalDataBusEntryType() { Code = TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber }, ReferenceNumber = "RANDOM" });
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference() { Type = new UniversalDataBusEntryType() { Code = TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber }, ReferenceNumber = "123" });
			reader = GetNewReader(shipment, Logger, consolidation, shipment);
			bookingFromReader = reader.ReadIntoBusinessObject();
			bookingNumber = bookingFromReader.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().FirstOrDefault(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
			AssertNotNull(bookingNumber);
			AssertEquals("123", bookingNumber.CE_EntryNum);

			// uShipment with TB DataSource and Key
			shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, "456");
			reader = GetNewReader(shipment, Logger, consolidation, shipment);
			bookingFromReader = reader.ReadIntoBusinessObject();
			bookingNumber = bookingFromReader.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().FirstOrDefault(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
			AssertNotNull(bookingNumber);
			AssertEquals("456", bookingNumber.CE_EntryNum);
		}

		public void TestUpdateTransportBooking()
		{
			var ufactory = new UniversalObjectFactory();
			var consolidation = ufactory.New<DtbBookingConsolidation>();

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

			consolidation.BookedByAddress.OrganisationPK = senderA.PK;
			ufactory.SaveForTesting();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, "123");
			shipment.DataContext.SetCompanyAndDataProviderDetails(tempCompanyA);

			var readerA = GetNewReader(shipment, Logger, consolidation, shipment, null, ufactory);
			var bookingReadA = readerA.ReadIntoBusinessObject();
			AssertNotNull("Should create the booking.", bookingReadA);
			AssertContains("Should have logged search for External Booking Reference number", "Information - Searching for Booking, attempting to match External Transport Booking numbers to Transport Booking number 123 from sending system and match Address to Sending Party SENDA", Logger.Logs);
			AssertEquals("Bookings Consolidation should be that passed in.", consolidation, bookingReadA.ConsolidationSingleJob);
			AssertEquals("Bookings Consolidation should have created only 1 booking.", 1, bookingReadA.ConsolidationSingleJob.Bookings.Count);
			AssertEquals("Consolidation should still be senderA", senderA, bookingReadA.ConsolidationSingleJob.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).First().Organisation);
			AssertEquals("123", bookingReadA.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber).CE_EntryNum);
			ufactory.SaveForTesting();

			var readerA2 = GetNewReader(shipment, Logger, consolidation, shipment, null, ufactory);
			var bookingReadA2 = readerA2.ReadIntoBusinessObject();
			AssertNotNull("Should update the booking.", bookingReadA2);
			AssertContains("Should have logged search for External Booking Reference number", "Information - Searching for Booking, attempting to match External Transport Booking numbers to Transport Booking number 123 from sending system and match Address to Sending Party SENDA", Logger.Logs);
			AssertEquals("Bookings Consolidation should be that passed in.", consolidation, bookingReadA2.ConsolidationSingleJob);
			AssertEquals("Bookings Consolidation should have created only 1 booking.", 1, bookingReadA2.ConsolidationSingleJob.Bookings.Count);
			AssertEquals("Consolidation should still be senderA", senderA, bookingReadA2.ConsolidationSingleJob.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).First().Organisation);
			AssertEquals("123", bookingReadA2.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber).CE_EntryNum);

			var shipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment2.DataContext = DataContextFactory.New();
			shipment2.DataContext.AddDataSource(DataContextType.TransportBooking, "456");
			shipment2.DataContext.SetCompanyAndDataProviderDetails(tempCompanyA);

			var readerA3 = GetNewReader(shipment2, Logger, consolidation, shipment, null, ufactory);
			var bookingReadA3 = readerA3.ReadIntoBusinessObject();
			AssertNotNull("Should create the booking.", bookingReadA3);
			AssertContains("Should have logged search for External Booking Reference number", "Information - Searching for Booking, attempting to match External Transport Booking numbers to Transport Booking number 456 from sending system and match Address to Sending Party SENDA", Logger.Logs);
			AssertEquals("Bookings Consolidation should be that passed in.", consolidation, bookingReadA3.ConsolidationSingleJob);
			AssertEquals("Bookings Consolidation should have created a new booking on the consolidation.", 2, bookingReadA3.ConsolidationSingleJob.Bookings.Count);
			AssertEquals("Consolidation should still be senderA", senderA, bookingReadA3.ConsolidationSingleJob.DocAddresses.FindDocAddressesByType(DocAddressType.BookingPartyDocumentaryAddress).First().Organisation);
			AssertEquals("456", bookingReadA3.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(c => c.CE_EntryType == TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber).CE_EntryNum);
		}

		public void TestPopulateNotes_AllowsSupportedNote()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var autoRatingAuditLog = PredefinedNoteTypes.Instance.AutoRatingAuditLog;
			AssertEquals("Precondition: Predefined note type has to be read only after add for this test", true, autoRatingAuditLog.IsReadOnlyAfterAdd);

			shipment.SetNoteCollection(() => new DataObjectList<Note>
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

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			var notes = bookingFromReader.Notes.GetAllNotes();
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
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var awbRatelineOvertypedNotes = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes;
			AssertEquals("Precondition: Predefined note type has to not be read only after add for this test", false, awbRatelineOvertypedNotes.IsReadOnlyAfterAdd);

			shipment.SetNoteCollection(() => new DataObjectList<Note>
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

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			var notes = bookingFromReader.Notes.GetAllNotes();
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
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var airsValidationResults = PredefinedNoteTypes.Instance.AIRSValidationResults;
			AssertEquals("Precondition: Predefined note type has to be read only after add for this test", true, airsValidationResults.IsReadOnlyAfterAdd);

			shipment.SetNoteCollection(() => new DataObjectList<Note>
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

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			var notes = bookingFromReader.Notes.GetAllNotes();
			AssertEquals("Should exclude read only unsupported notes", 0, notes.Count);
		}

		public void TestPopulateNotes_ExcludesCountryRulesValidationNote()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetNoteCollection(() => new DataObjectList<Note>
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

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();
			var notes = bookingFromReader.Notes.GetAllNotes().Cast<StmNote>();

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

		public void TestFindExistingBusinessObjectMatchExternalTransportBookingNumbersOnJobIDForTestCba()
		{
			var externalConsolidationJobId = "CM87654321";
			var externalBookingJobId = "TB8765432A";
			var externalBookingJobId2 = "TB8765432B";
			Helper.MapOrganisation(EdiOrgCode);
			var booking2 = SimulateExistingBookingConsolidationWithTwoBookingsAndReturnSecondBookingForTestReturnMatching();

			Factory.SaveForTesting();
			var consolidationJobId = booking2.ConsolidationSingleJob.KB_JobID;
			var bookingJobId2 = booking2.KM_JobID;
			var bookingJobId = booking2.ConsolidationSingleJob.Bookings.First().KM_JobID;

			var bookingConsolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingConsolidationDataObject.DataContext = DataContextFactory.New();
			bookingConsolidationDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			bookingConsolidationDataObject.DataContext.AddDataSource(DataContextType.TransportBookingConsolidation, externalConsolidationJobId);
			bookingConsolidationDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, externalBookingJobId2);
			bookingConsolidationDataObject.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole() { Code = RecipientRoleType.BKP } };
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.DataContext = DataContextFactory.New();
			bookingDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, externalBookingJobId);
			bookingDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			bookingDataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber }, ReferenceNumber = bookingJobId });
			var booking2DataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			booking2DataObject.DataContext = DataContextFactory.New();
			booking2DataObject.DataContext.AddDataSource(DataContextType.TransportBooking, externalBookingJobId2);
			booking2DataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			booking2DataObject.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new UniversalDataBusEntryType { Code = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber }, ReferenceNumber = bookingJobId2 });

			bookingConsolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			bookingConsolidationDataObject.SubShipmentCollection.Add(booking2DataObject);

			var message = Factory.New<XmlEDIMessage>();
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = EdiOrgCode;
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = bookingConsolidationDataObject;

			var reader = GetNewReader(booking2DataObject, Logger, booking2.ConsolidationSingleJob, bookingConsolidationDataObject);
			DtbBooking booking2Read = null;
			using (TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EdiOrgCode))
			{
				booking2Read = reader.ReadIntoBusinessObject();
			}
			AssertContains("Expect informational log on search for external booking number from sending system", FormattableString.Invariant($"Information - Searching for Booking, attempting to match Transport Booking Job ID to External Transport Booking number {bookingJobId2} from sending system"), Logger.Logs);
			AssertEquals("Booking Reader should have found existing booking", bookingJobId2, booking2Read.KM_JobID);
		}

		DtbBooking SimulateExistingBookingConsolidationWithTwoBookingsAndReturnSecondBookingForTestReturnMatching()
		{
			var consolidation = Helper.CreateConsolidation();
			var packageJob = Helper.CreatePackageJob(consolidation);
			consolidation.KB_JobType = "BKG";

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobType = "BKG";
			booking.KM_Direction = "PIC";
			booking.KM_RS_NKServiceLevel = "STD";

			var instruction1 = Helper.CreateInstruction(booking);
			var package1 = Helper.CreatePackage("PKG1", 1);
			package1.KP_Sequence = 1;
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			instruction1.DivotsWithPackages.AddPackage(package1);

			var booking2 = Helper.CreateBooking(consolidation);
			booking.KM_JobType = "BKG";
			booking.KM_Direction = "PIC";
			booking.KM_RS_NKServiceLevel = "STD";

			var instruction2 = Helper.CreateInstruction(booking2);
			var package2 = Helper.CreatePackage("PKG2", 1);
			package2.KP_Sequence = 1;
			package2.KP_KJ_ParentPackageJob = packageJob.PK;
			instruction2.DivotsWithPackages.AddPackage(package2);

			// no booked by organisation necessary as we aren't expecting any constraint on Booked By Organisation - unlike other Simulate() method

			return booking2;
		}

		public void TestPopulateBusinessObject_ActualChargeableHasNonNullValue_TargetWeightUnitIsSame_SetsChargeableWeightToActualChargeableAndOverrideTrue()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_OverrideChargeable = true;
			booking.KM_Chargeable = 550m;
			AssertEquals("Chargeable Weight should be in Kilogram", Constants.Weight.Kilograms, booking.ChargeableUnits);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ActualChargeable = 850m;
			shipment.TotalWeightUnit = new UnitOfWeight() { Code = Constants.Weight.Kilograms };
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, "TB123");

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertEquals("Chargeable Weight should be 850.00", 850m, bookingFromReader.KM_Chargeable);
			AssertEquals("Chargeable Weight should be 850.00", 850m, booking.KM_Chargeable);
			AssertEquals("Override Chargeable should be true", true, booking.KM_OverrideChargeable);
		}

		public void TestPopulateBusinessObject_ActualChargeableHasNonNullValue_TargetWeightUnitIsDifferent_SetsChargeableWeightToActualChargeableAndOverrideTrue()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_OverrideChargeable = true;
			booking.KM_Chargeable = 550m;
			AssertEquals("Chargeable Weight should be in Kilogram", Constants.Weight.Kilograms, booking.ChargeableUnits);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ActualChargeable = 85m;
			shipment.TotalWeightUnit = new UnitOfWeight() { Code = Constants.Weight.Tonnes };
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, "TB123");

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertEquals("Chargeable Weight should be 85000.00 KG", 85000m, bookingFromReader.KM_Chargeable);
			AssertEquals("Chargeable Weight should be 85000.00 KG", 85000m, booking.KM_Chargeable);
			AssertEquals("Override Chargeable should be true", true, booking.KM_OverrideChargeable);
		}

		public void TestPopulateBusinessObject_ActualChargeableHasNonNullValue_DataSourceIsTransportBooking_SetsChargeableWeightToActualChargeableAndOverrideTrue()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_OverrideChargeable = false;
			booking.KM_Chargeable = 550m;
			AssertEquals("Chargeable Weight should be in Kilogram", Constants.Weight.Kilograms, booking.ChargeableUnits);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ActualChargeable = 850m;
			shipment.TotalWeightUnit = new UnitOfWeight() { Code = Constants.Weight.Kilograms };
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, "TB123");

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertEquals("Chargeable Weight should be 850.00", 850m, bookingFromReader.KM_Chargeable);
			AssertEquals("Chargeable Weight should be 850.00", 850m, booking.KM_Chargeable);
			AssertEquals("Override Chargeable should be true", true, booking.KM_OverrideChargeable);
		}

		public void TestPopulateBusinessObject_ActualChargeableHasNonNullValue_DataSourceIsNotTransportBooking_SetsChargeableWeightToZeroAndOverrideFalse()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_OverrideChargeable = false;
			booking.KM_Chargeable = 550m;
			AssertEquals("Chargeable Weight should be in Kilogram", Constants.Weight.Kilograms, booking.ChargeableUnits);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ActualChargeable = 850m;
			shipment.TotalWeightUnit = new UnitOfWeight() { Code = Constants.Weight.Kilograms };
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "Ship456");

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertEquals("Chargeable Weight should be 0.00", 0m, bookingFromReader.KM_Chargeable);
			AssertEquals("Chargeable Weight should be 0.00", 0m, booking.KM_Chargeable);
			AssertEquals("Override Chargeable should be false", false, booking.KM_OverrideChargeable);
		}

		public void TestPopulateBusinessObject_ActualChargeableHasNullValue_SetsChargeableWeightToZeroAndOverrideFalse()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_OverrideChargeable = true;
			booking.KM_Chargeable = 550m;

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.TransportBooking, "TB123");

			var reader = GetNewReader(shipment, Logger, consolidation, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertEquals("Chargeable Weight should be 0.00", 0m, bookingFromReader.KM_Chargeable);
			AssertEquals("Chargeable Weight should be 0.00", 0m, booking.KM_Chargeable);
			AssertEquals("Override Chargeable should be false", false, booking.KM_OverrideChargeable);
		}

		public void TestAdditionalReferences()
		{
			var year = ZDateTime.Now.Year;

			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var additionalReferenceDataObject = new AdditionalReference();

			additionalReferenceDataObject.IssueDate = new ZDateTime(year, 1, 1);
			additionalReferenceDataObject.ReferenceNumber = "R1234";
			additionalReferenceDataObject.Type = new UniversalDataBusEntryType { Code = "TRF", Description = "Transport Reference Number" };

			bookingDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			bookingDataObject.AdditionalReferenceCollection.Add(additionalReferenceDataObject);

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals(1, booking.AdditionalReferenceNumbers.Count);

			var additionalReference = booking.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference.CE_EntryNum", "R1234", additionalReference.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "TRF", additionalReference.CE_EntryType);
			AssertEquals("additionalReference.CE_IssueDate", new ZDateTime(year, 1, 1), additionalReference.CE_IssueDate);
		}

		public void TestAdditionalReferences_WithHouseBill()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.WayBillNumber = "12345";
			bookingDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferences.Count);

			var additionalReference = additionalReferences[0];
			AssertEquals("additionalReference.CE_EntryNum", "12345", additionalReference.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "HSB", additionalReference.CE_EntryType);
		}

		public void TestAdditionalReferences_WithMasterBill()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.WayBillNumber = "12345";
			bookingDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferences.Count);

			var additionalReference = additionalReferences[0];
			AssertEquals("additionalReference.CE_EntryNum", "12345", additionalReference.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "MAB", additionalReference.CE_EntryType);
		}

		public void TestAdditionalReferences_WithMasterBill_OnTopLevelDO()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelDataObject.WayBillNumber = "12345";
			topLevelDataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());

			var reader = GetNewReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, null, topLevelDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferences.Count);

			var additionalReference = additionalReferences[0];
			AssertEquals("additionalReference.CE_EntryNum", "12345", additionalReference.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "MAB", additionalReference.CE_EntryType);
		}

		public void TestAdditionalReferences_AddDashInMasterBillForAirModeAndRemoveDashInNonAirMode()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Core.Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = "777-88888";
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "123456", BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-789", BillType = masterBillType };
			var mab3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "123-456 ", BillType = masterBillType };

			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2, mab3 });
			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForAirTransportMode = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(3, additionalReferencesForAirTransportMode.Count);

			var houseBill = additionalReferencesForAirTransportMode[0];
			AssertEquals("777-88888", houseBill.CE_EntryNum);
			AssertEquals("HSB", houseBill.CE_EntryType);

			var masterBillReference1 = additionalReferencesForAirTransportMode[1];
			AssertEquals("123-456", masterBillReference1.CE_EntryNum);
			AssertEquals("MAB", masterBillReference1.CE_EntryType);

			var masterBillReference2 = additionalReferencesForAirTransportMode[2];
			AssertEquals("456-789", masterBillReference2.CE_EntryNum);
			AssertEquals("MAB", masterBillReference2.CE_EntryType);

			bookingDataObject.TransportMode.Code = Core.Constants.TransportModes.Sea;
			booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForSeaTransportMode = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(3, additionalReferencesForSeaTransportMode.Count);

			houseBill = additionalReferencesForSeaTransportMode[0];
			AssertEquals("777-88888", houseBill.CE_EntryNum);
			AssertEquals("HSB", houseBill.CE_EntryType);

			masterBillReference1 = additionalReferencesForSeaTransportMode[1];
			AssertEquals("123456", masterBillReference1.CE_EntryNum);
			AssertEquals("MAB", masterBillReference1.CE_EntryType);

			masterBillReference2 = additionalReferencesForSeaTransportMode[2];
			AssertEquals("456789", masterBillReference2.CE_EntryNum);
			AssertEquals("MAB", masterBillReference2.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Core.Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = "456-34232";
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-789", BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "123-456", BillType = masterBillType };
			var mab3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "987-654", BillType = masterBillType };

			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2, mab3 });
			var reader1 = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader1.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(4, additionalReferencesForObject1.Count);

			var houseBill1 = additionalReferencesForObject1[0];
			AssertEquals("456-34232", houseBill1.CE_EntryNum);
			AssertEquals("HSB", houseBill1.CE_EntryType);

			var masterBillReferenceOne1 = additionalReferencesForObject1[1];
			AssertEquals("456-789", masterBillReferenceOne1.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceOne1.CE_EntryType);

			var masterBillReferenceOne2 = additionalReferencesForObject1[2];
			AssertEquals("123-456", masterBillReferenceOne2.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceOne2.CE_EntryType);

			var masterBillReferenceOne3 = additionalReferencesForObject1[3];
			AssertEquals("987-654", masterBillReferenceOne3.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceOne3.CE_EntryType);

			var mab4 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "111-111", BillType = masterBillType };
			var mab5 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "222-222", BillType = masterBillType };
			var mab6 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "333-333", BillType = masterBillType };
			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab4, mab5, mab6 });
			bookingDataObject.WayBillNumber = "111-444888";

			var reader2 = GetNewReader(bookingDataObject, Logger, Factory, booking, bookingDataObject);
			var newBooking = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newBooking);
			AssertEquals(4, additionalReferencesForObject2.Count);

			var houseBill2 = additionalReferencesForObject2[0];
			AssertEquals("111-444888", houseBill2.CE_EntryNum);
			AssertEquals("HSB", houseBill2.CE_EntryType);

			var masterBillReferenceTwo1 = additionalReferencesForObject2[1];
			AssertEquals("111-111", masterBillReferenceTwo1.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceTwo1.CE_EntryType);

			var masterBillReferenceTwo2 = additionalReferencesForObject2[2];
			AssertEquals("222-222", masterBillReferenceTwo2.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceTwo2.CE_EntryType);

			var masterBillReferenceTwo3 = additionalReferencesForObject2[3];
			AssertEquals("333-333", masterBillReferenceTwo3.CE_EntryNum);
			AssertEquals("MAB", masterBillReferenceTwo3.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_NewWayBill()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Core.Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = "456-34232";
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferencesForObject.Count);

			var masterBill1 = additionalReferencesForObject[0];
			AssertEquals("456-34232", masterBill1.CE_EntryNum);
			AssertEquals("MAB", masterBill1.CE_EntryType);

			bookingDataObject.WayBillNumber = "1564-4812";
			var reader2 = GetNewReader(bookingDataObject, Logger, Factory, booking, bookingDataObject);
			var newBooking = reader2.ReadIntoBusinessObject();
			additionalReferencesForObject = GetAdditionalReferenceNumbersForWayBills(newBooking);
			AssertEquals(1, additionalReferencesForObject.Count);

			var masterBill2 = additionalReferencesForObject[0];
			AssertEquals("1564-4812", masterBill2.CE_EntryNum);
			AssertEquals("MAB", masterBill2.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_EmptyMasterBill()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = "456-34232";
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-34232", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			bookingDataObject.WayBillNumber = "";
			var reader2 = GetNewReader(bookingDataObject, Logger, Factory, booking, bookingDataObject);
			var newBooking = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newBooking);
			AssertEquals(0, additionalReferencesForObject2.Count);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_EmptyHouseBill()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = "456-34232";
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-34232", masterBill.CE_EntryNum);
			AssertEquals("HSB", masterBill.CE_EntryType);

			bookingDataObject.WayBillNumber = "";
			reader = GetNewReader(bookingDataObject, Logger, Factory, booking, bookingDataObject);
			var newBooking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newBooking);
			AssertEquals(0, additionalReferencesForObject2.Count);
		}

		public void TestAdditionalReferences_ImportsNoDuplicates_HouseBill()
		{
			var houseBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = "456-342";
			bookingDataObject.WayBillType = houseBillType;

			var hsb1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = houseBillType };
			var hsb2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = houseBillType };
			var hsb3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "987-654", BillType = houseBillType };

			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { hsb1, hsb2, hsb3 });
			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferencesForObject.Count);

			var masterBill = additionalReferencesForObject[0];
			AssertEquals("456-342", masterBill.CE_EntryNum);
			AssertEquals("HSB", masterBill.CE_EntryType);
		}

		public void TestAdditionalReferences_ImportsNoDuplicates_MasterBill()
		{
			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = "456-342";
			bookingDataObject.WayBillType = masterBillType;

			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			var mab3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "987-654", BillType = masterBillType };

			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2, mab3 });
			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(2, additionalReferencesForObject.Count);

			var masterBill1 = additionalReferencesForObject[0];
			AssertEquals("456-342", masterBill1.CE_EntryNum);
			AssertEquals("MAB", masterBill1.CE_EntryType);

			var masterBill2 = additionalReferencesForObject[1];
			AssertEquals("987-654", masterBill2.CE_EntryNum);
			AssertEquals("MAB", masterBill2.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_NoWayBillType()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = "456-34232";
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-34232", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			bookingDataObject.WayBillType = null;
			var reader2 = GetNewReader(bookingDataObject, Logger, Factory, booking, bookingDataObject);
			var newBooking = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newBooking);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-34232", masterBillTwo.CE_EntryNum);
			AssertEquals("MAB", masterBillTwo.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_ReplacesCurrentWithImported_NoWayBillNumber()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = "456-34232";
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-34232", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			bookingDataObject.WayBillNumber = null;
			var reader2 = GetNewReader(bookingDataObject, Logger, Factory, booking, bookingDataObject);
			var newBooking = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newBooking);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-34232", masterBillTwo.CE_EntryNum);
			AssertEquals("MAB", masterBillTwo.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_NoRefNumber()
		{
			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Constants.TransportModes.Air;

			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1 });
			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-342", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = null, BillType = masterBillType };
			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab2 });
			var reader2 = GetNewReader(bookingDataObject, Logger, Factory, booking, bookingDataObject);
			var newBooking = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newBooking);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-342", masterBillTwo.CE_EntryNum);
			AssertEquals("MAB", masterBillTwo.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_EmptyRefNumber()
		{
			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Constants.TransportModes.Air;

			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1 });
			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-342", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "", BillType = masterBillType };
			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab2 });
			var reader2 = GetNewReader(bookingDataObject, Logger, Factory, booking, bookingDataObject);
			var newBooking = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newBooking);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-342", masterBillTwo.CE_EntryNum);
			AssertEquals("MAB", masterBillTwo.CE_EntryType);
		}

		public void TestAdditionalReferences_OverridesMasterBillValues_NoRefBillType()
		{
			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Constants.TransportModes.Air;

			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = masterBillType };
			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1 });
			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferencesForObject1 = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferencesForObject1.Count);

			var masterBill = additionalReferencesForObject1[0];
			AssertEquals("456-342", masterBill.CE_EntryNum);
			AssertEquals("MAB", masterBill.CE_EntryType);

			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456-342", BillType = null };
			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab2 });
			var reader2 = GetNewReader(bookingDataObject, Logger, Factory, booking, bookingDataObject);
			var newBooking = reader2.ReadIntoBusinessObject();
			var additionalReferencesForObject2 = GetAdditionalReferenceNumbersForWayBills(newBooking);
			AssertEquals(1, additionalReferencesForObject2.Count);

			var masterBillTwo = additionalReferencesForObject2[0];
			AssertEquals("456-342", masterBillTwo.CE_EntryNum);
			AssertEquals("HSB", masterBillTwo.CE_EntryType);
		}

		public void TestAdditionalReferences_LongNumber()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.TransportMode = new CodeDescriptionPair();
			bookingDataObject.TransportMode.Code = Core.Constants.TransportModes.Air;
			bookingDataObject.WayBillNumber = new ZString('1', 40);
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = new ZString('2', 40), BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = new ZString('2', 40), BillType = masterBillType };

			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2 });
			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(2, additionalReferences.Count);

			var houseBill = additionalReferences[0];
			AssertEquals(new ZString('1', 35), houseBill.CE_EntryNum);
			AssertEquals("HSB", houseBill.CE_EntryType);

			var masterBillReference1 = additionalReferences[1];
			AssertEquals("222-" + new ZString('2', 31), masterBillReference1.CE_EntryNum);
			AssertEquals("MAB", masterBillReference1.CE_EntryType);
		}

		public void TestAdditionalReferences_WithWayBill()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.WayBillNumber = "12345";
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(1, additionalReferences.Count);

			var additionalReference = additionalReferences[0];
			AssertEquals("additionalReference.CE_EntryNum", "12345", additionalReference.CE_EntryNum);
			AssertEquals("If there is no WayBillNumberType the WayBill should be imported as HouseBill.", "HSB", additionalReference.CE_EntryType);
		}

		public void TestAdditionalReferences_WithWayBillsInAdditionalBillCollection()
		{
			// i.e. Forwarding Shipment
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.WayBillNumber = "12345";
			bookingDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var masterBillType = new WayBillType { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			var mab1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "456", BillType = masterBillType };
			var mab2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "789", BillType = masterBillType };

			bookingDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { mab1, mab2 });
			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			var additionalReferences = GetAdditionalReferenceNumbersForWayBills(booking);
			AssertEquals(3, additionalReferences.Count);

			var houseBill = additionalReferences[0];
			AssertEquals("12345", houseBill.CE_EntryNum);
			AssertEquals("HSB", houseBill.CE_EntryType);

			var masterBillReference1 = additionalReferences[1];
			AssertEquals("456", masterBillReference1.CE_EntryNum);
			AssertEquals("MAB", masterBillReference1.CE_EntryType);

			var masterBillReference2 = additionalReferences[2];
			AssertEquals("789", masterBillReference2.CE_EntryNum);
			AssertEquals("MAB", masterBillReference2.CE_EntryType);
		}

		public void TestPopulateNotes()
		{
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingShipment.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note
				{
					Description = "Dangerous Goods Additional Handling Information",
					NoteText = "234",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.PUB), Description = "Public" },
					NoteContext = new NoteContext() { Code = "123", Description = "123" },
					IsCustomDescription = false
				}
			});
			var reader = GetNewReader(bookingShipment, Logger, Factory, null, bookingShipment);
			var bookingBO = reader.ReadIntoBusinessObject();
			var notes = bookingBO.Notes.GetAllNotes();
			AssertEquals(1, notes.Count);
			var noteBO = (StmNote)notes.First();

			CombineAssertions(delegate
			{
				AssertEquals("noteBO.ST_Description", "Dangerous Goods Additional Handling Information", noteBO.ST_Description);
				AssertEquals("noteBO.ST_NoteDataAsText", "234", noteBO.ST_NoteDataAsText);
				AssertEquals("noteBO.ST_NoteContext", "123", noteBO.ST_NoteContext);
				AssertEquals("noteBO.ST_NoteType", "PUB", noteBO.ST_NoteType);
				AssertEquals("noteBO.ST_IsCustomDescription", false, noteBO.ST_IsCustomDescription);
			});
		}

		public void TestPopulateNotes_UnmatchedOrgs()
		{
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingShipment.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note
				{
					Description = "Unmatched Org Details",
					NoteText = @"Organisation Type: Consignor
Owner Code: 
EDI Code: 
Organisation Name: ASDFGAGDDGAGA
Address Line 1: ADSFAFFSS
Address Line 2: 344
City: FAFAF
Post Code: 24532
State or Province: ACT
Country: AU",
					Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.INT), Description = "INTERNAL" },
					NoteContext = new NoteContext() { Code = "AAA", Description = "AAA" },
					IsCustomDescription = false
				}
			});
			var reader = GetNewReader(bookingShipment, Logger, Factory, null, bookingShipment);
			var bookingBO = reader.ReadIntoBusinessObject();
			var notes = bookingBO.Notes.GetAllNotes();
			AssertEquals(1, notes.Count);
			var noteBO = (StmNote)notes.First();

			CombineAssertions(delegate
			{
				AssertEquals("noteBO.ST_Description", "Unmatched Org Details", noteBO.ST_Description);
				AssertEquals("noteBO.ST_NoteDataAsText", @"Organisation Type: Consignor
Owner Code: 
EDI Code: 
Organisation Name: ASDFGAGDDGAGA
Address Line 1: ADSFAFFSS
Address Line 2: 344
City: FAFAF
Post Code: 24532
State or Province: ACT
Country: AU", noteBO.ST_NoteDataAsText);
				AssertEquals("noteBO.ST_NoteContext", "AAA", noteBO.ST_NoteContext);
				AssertEquals("noteBO.ST_NoteType", "INT", noteBO.ST_NoteType);
				AssertEquals("noteBO.ST_IsCustomDescription", false, noteBO.ST_IsCustomDescription);
			});
		}

		public void TestWhenSenderIsCBAAuthorisedAndTransportComapnyAddressIsBeingSet_BookingConfirmedEventShouldBeCreated()
		{
			using (TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TESTSENDER"))
			{
				var consol = Helper.CreateConsolidation();
				var booking = consol.Bookings.AddNew();

				var dummyWriteManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var org = Factory.New<OrgHeader>();
				org.OH_FullName = "New Transport Co";
				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New()
				};
				shipment.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "New Trans Ref" };
				var newTransportCompanyAddress = shipment.AddOrgAddress(dummyWriteManager, org.MainAddress, DocAddressType.TransportCompanyDocumentaryAddress);
				newTransportCompanyAddress.Address1 = "ADDRESS1_TEST";

				var workflow = new WorkflowInfo();
				var recipientRole = new RecipientRoleDetail();
				recipientRole.Type = RecipientRoleType.BKP;
				workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
				shipment.DataContext.SetWorkflowInfo(workflow);

				var interchange = Factory.New<XmlEDIInterchange>();
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				interchange.EI_From = "TESTSENDER";
				var message = Factory.New<XmlEDIMessage>();
				message.EM_EI = interchange.PK;
				Logger.SourceMessage = message;
				Logger.TopLevelDataObject = shipment;

				var bookingTransportCompanyAddresses = booking.DocAddresses.Where(x => x.DocAddressType == DocAddressType.TransportCompanyDocumentaryAddress);

				if (bookingTransportCompanyAddresses.Any())
				{
					booking.DocAddresses.Remove(bookingTransportCompanyAddresses.FirstOrDefault());
				}

				AssertEquals("Address on Booking should not have changed yet", "", booking.Address.Address1);

				var reader = GetNewReader(shipment, Logger, consol, shipment, booking);
				var bookingFromReader = reader.ReadIntoBusinessObject();

				AssertEquals("Address on Booking should have been changed to the new Transport Company Address.", "ADDRESS1_TEST", bookingFromReader.Address.Address1);

				var eventQuery = new ZQuery(StmALogSchema.SL_Parent, bookingFromReader.PK);
				var bookingConfirmedEvent = Factory.Load<StmALog>(eventQuery).FirstOrDefault();

				AssertNotNull("An Event should have been created.", bookingConfirmedEvent);
				AssertEquals("The created event should be a 'Booking Confirmed' event.", "BKC", bookingConfirmedEvent.SL_SE_NKEvent);
				AssertEquals("The created event should have the correct reference string.", "FAC=Transport Company|NAM=New Transport Co", bookingConfirmedEvent.SL_Reference);
			}
		}

		public void TestWhenShipmentHasNoTransportCompanyAddress_BookingConfirmedEventShouldNotBeCreated()
		{
			using (TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TESTSENDER"))
			{
				var consol = Helper.CreateConsolidation();
				var booking = consol.Bookings.AddNew();

				var dummyWriteManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var org = Factory.New<OrgHeader>();
				org.OH_FullName = "New Transport Co";
				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New()
				};

				shipment.AddOrgAddress(dummyWriteManager, org.MainAddress, DocAddressType.BookingPartyDocumentaryAddress);

				var workflow = new WorkflowInfo();
				var recipientRole = new RecipientRoleDetail();
				recipientRole.Type = RecipientRoleType.BKP;
				workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
				shipment.DataContext.SetWorkflowInfo(workflow);

				var interchange = Factory.New<XmlEDIInterchange>();
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				interchange.EI_From = "TESTSENDER";
				var message = Factory.New<XmlEDIMessage>();
				message.EM_EI = interchange.PK;
				Logger.SourceMessage = message;
				Logger.TopLevelDataObject = shipment;

				var bookingTransportCompanyAddresses = booking.DocAddresses.Where(x => x.DocAddressType == DocAddressType.TransportCompanyDocumentaryAddress);

				if (bookingTransportCompanyAddresses.Any())
				{
					booking.DocAddresses.Remove(bookingTransportCompanyAddresses.FirstOrDefault());
				}

				var reader = GetNewReader(shipment, Logger, consol, shipment, booking);
				var bookingFromReader = reader.ReadIntoBusinessObject();

				var eventQuery = new ZQuery(StmALogSchema.SL_Parent, bookingFromReader.PK);
				var bookingConfirmedEvent = Factory.Load<StmALog>(eventQuery).FirstOrDefault();

				AssertNull("Booking Confirmed Event should not have been created.", bookingConfirmedEvent);
			}
		}

		public void TestWhenShipmentHasNoAddresses_BookingConfirmedEventShouldNotBeCreated()
		{
			using (TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TESTSENDER"))
			{
				var consol = Helper.CreateConsolidation();
				var booking = consol.Bookings.AddNew();

				var dummyWriteManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var org = Factory.New<OrgHeader>();
				org.OH_FullName = "New Transport Co";
				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New()
				};

				var workflow = new WorkflowInfo();
				var recipientRole = new RecipientRoleDetail();
				recipientRole.Type = RecipientRoleType.BKP;
				workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
				shipment.DataContext.SetWorkflowInfo(workflow);

				var interchange = Factory.New<XmlEDIInterchange>();
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				interchange.EI_From = "TESTSENDER";
				var message = Factory.New<XmlEDIMessage>();
				message.EM_EI = interchange.PK;
				Logger.SourceMessage = message;
				Logger.TopLevelDataObject = shipment;

				var bookingTransportCompanyAddresses = booking.DocAddresses.Where(x => x.DocAddressType == DocAddressType.TransportCompanyDocumentaryAddress);

				if (bookingTransportCompanyAddresses.Any())
				{
					booking.DocAddresses.Remove(bookingTransportCompanyAddresses.FirstOrDefault());
				}

				var reader = GetNewReader(shipment, Logger, consol, shipment, booking);
				var bookingFromReader = reader.ReadIntoBusinessObject();

				var eventQuery = new ZQuery(StmALogSchema.SL_Parent, bookingFromReader.PK);
				var bookingConfirmedEvent = Factory.Load<StmALog>(eventQuery).FirstOrDefault();

				AssertNull("Booking Confirmed Event should not have been created.", bookingConfirmedEvent);
			}
		}

		public void TestWhenTransportCompanyAddressIsAlreadySet_BookingConfirmedEventShouldNotBeCreated()
		{
			using (TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TESTSENDER"))
			{
				var consol = Helper.CreateConsolidation();
				var booking = consol.Bookings.AddNew();

				var dummyWriteManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var newTransportCompanyOrg = Factory.New<OrgHeader>();
				newTransportCompanyOrg.OH_FullName = "New Transport Co";
				var existingTransportCompanyOrg = Factory.New<OrgHeader>();
				existingTransportCompanyOrg.OH_FullName = "Existing Transport Co";

				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New()
				};
				shipment.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "New Trans Ref" };
				var newTransportCompanyAddress = shipment.AddOrgAddress(dummyWriteManager, newTransportCompanyOrg.MainAddress, DocAddressType.TransportCompanyDocumentaryAddress);
				newTransportCompanyAddress.Address1 = "ADDRESS1_TEST";

				var workflow = new WorkflowInfo();
				var recipientRole = new RecipientRoleDetail();
				recipientRole.Type = RecipientRoleType.BKP;
				workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
				shipment.DataContext.SetWorkflowInfo(workflow);

				var interchange = Factory.New<XmlEDIInterchange>();
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				interchange.EI_From = "TESTSENDER";
				var message = Factory.New<XmlEDIMessage>();
				message.EM_EI = interchange.PK;
				Logger.SourceMessage = message;
				Logger.TopLevelDataObject = shipment;

				booking.Address.E2_OA_Address = existingTransportCompanyOrg.MainAddress.PK;
				booking.Address.Address1 = "EXISTINGBOOKINGADDRESS1_TEST";

				AssertNotEquals("Address on Booking should have an associated Org Address.", ZGuid.Empty, booking.Address.E2_OA_Address);

				var reader = GetNewReader(shipment, Logger, consol, shipment, booking);
				var bookingFromReader = reader.ReadIntoBusinessObject();

				AssertEquals("Address on Booking should have been changed to the new Transport Company Address.", "ADDRESS1_TEST", bookingFromReader.Address.Address1);

				var eventQuery = new ZQuery(StmALogSchema.SL_Parent, bookingFromReader.PK);
				var bookingConfirmedEvent = Factory.Load<StmALog>(eventQuery).FirstOrDefault();

				AssertNull("Booking Confirmed Event should not have been created.", bookingConfirmedEvent);
			}
		}

		public void TestWhenSenderIsNotCBAAuthorised_BookingConfirmedEventShouldNotBeCreated()
		{
			var consol = Helper.CreateConsolidation();
			var booking = consol.Bookings.AddNew();

			var dummyWriteManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "New Transport Co";
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			shipment.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "New Trans Ref" };
			var newTransportCompanyAddress = shipment.AddOrgAddress(dummyWriteManager, org.MainAddress, DocAddressType.TransportCompanyDocumentaryAddress);
			newTransportCompanyAddress.Address1 = "ADDRESS1_TEST";

			var workflow = new WorkflowInfo();
			var recipientRole = new RecipientRoleDetail();
			recipientRole.Type = RecipientRoleType.BKP;
			workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
			shipment.DataContext.SetWorkflowInfo(workflow);

			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "TESTSENDER";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = shipment;

			var bookingTransportCompanyAddresses = booking.DocAddresses.Where(x => x.DocAddressType == DocAddressType.TransportCompanyDocumentaryAddress);

			if (bookingTransportCompanyAddresses.Any())
			{
				booking.DocAddresses.Remove(bookingTransportCompanyAddresses.FirstOrDefault());
			}

			AssertEquals("Address on Booking should not have changed yet", "", booking.Address.Address1);

			var reader = GetNewReader(shipment, Logger, consol, shipment, booking);
			var bookingFromReader = reader.ReadIntoBusinessObject();

			AssertEquals("Address on Booking should have been changed to the new Transport Company Address.", "ADDRESS1_TEST", bookingFromReader.Address.Address1);

			var eventQuery = new ZQuery(StmALogSchema.SL_Parent, bookingFromReader.PK);
			var bookingConfirmedEvent = Factory.Load<StmALog>(eventQuery).FirstOrDefault();

			AssertNull("Booking Confirmed Event should not have been created.", bookingConfirmedEvent);
		}

		public void TestBookingTransportModeIsPopulatedWhenSetInDataObject()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.BookingTransportMode = new CodeDescriptionPair { Code = "RAI", Description = "Rail Transport" };

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals("Booking Transport Mode should be RAI/Rail Transport", "RAI", booking.KM_TransportMode);
		}

		public void TestBookingTransportModeIsSetToDefaultIfNotSetInDataObject()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertEquals("Precondition", null, bookingDataObject.BookingTransportMode);

			var reader = GetNewReader(bookingDataObject, Logger, Factory, null, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals("Booking Transport Mode should be ROA/Road Transport", "ROA", booking.KM_TransportMode);
		}

		protected void AssertContainerYardInstructionsAreSplitCorrectly(DtbBooking bookingFromReader, UniversalShipment shipment, int expectedInstructionAmount, int expectedCYDInstructionAmount, Dictionary<ZInt, PkgPackage> packageLinks, int expectedContainerAmountOnInstructions = 1, bool testForContainerAmountsOnInstructions = true)
		{
			CombineAssertions(() =>
			{
				var cydInstructionsCount = 0;

				foreach (DtbBookingInstruction instruction in bookingFromReader.Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CYD))
				{
					cydInstructionsCount += 1;
					var packagesOnInstruction = new List<PkgPackage>();
					foreach (DtbBookingInstructionPkgDivot divot in instruction.PackageDivots)
					{
						packagesOnInstruction.Add(divot.Package);
					}

					ZString? addressTypeNeededForMatchingContainers = null;
					if (GetInstructionTypeForBookingDirection(bookingFromReader) == (ZString?)InstructionTypes.Codes.PickUp)
					{
						addressTypeNeededForMatchingContainers = AddressTypes.ContainerYardEmptyPickupAddress;
					}
					else if (GetInstructionTypeForBookingDirection(bookingFromReader) == (ZString?)InstructionTypes.Codes.Delivery)
					{
						addressTypeNeededForMatchingContainers = AddressTypes.ContainerYardEmptyReturnAddress;
					}

					if (testForContainerAmountsOnInstructions)
					{
						AssertEquals("Expected number of containers on instruction", expectedContainerAmountOnInstructions, packagesOnInstruction.Count);
					}

					var containersWithSameAddress = shipment.ContainerCollection.Where(c => c.OrganizationAddressCollection != null && c.OrganizationAddressCollection.Select(o => o?.AddressShortCode).Contains(instruction?.Address?.RealAddress?.AddressCode));
					var containersThatShouldBeAttached = containersWithSameAddress.ToList();

					if (shipment.OrganizationAddressCollection != null && shipment.OrganizationAddressCollection.Count > 0 && shipment.OrganizationAddressCollection.Single(a => a.AddressType == (ZString?)nameof(DocAddressType.CustomsContainerYardAddress)).AddressShortCode == (ZString?)instruction.Address.RealAddress.AddressCode)
					{
						var containersWithNoApplicableAddressToMatch = shipment.ContainerCollection.Where(c => !c.OrganizationAddressCollection.Any(a => a != null && a.AddressType == addressTypeNeededForMatchingContainers));
						containersThatShouldBeAttached.AddRange(containersWithNoApplicableAddressToMatch);
					}

					if (instruction.Address == null || instruction.Address.RealAddress == null)
					{
						var containersThatShouldMatchOnShipmentAddress = shipment.ContainerCollection.Where(c => !containersThatShouldBeAttached.Contains(c));
						containersThatShouldBeAttached.AddRange(containersThatShouldMatchOnShipmentAddress);
					}

					foreach (var package in packagesOnInstruction)
					{
						AssertEquals("Each attached container should match the address of the instruction it's attached to", true, containersThatShouldBeAttached.Select(c => c.Link).Contains(packageLinks.FirstOrDefault(l => l.Value.PK == package.PK).Key));
					}
				}

				AssertEquals("Expected number of instructions on booking", expectedInstructionAmount, bookingFromReader.Instructions.Count);
				AssertEquals("Expected number of CYD instructions on booking", expectedCYDInstructionAmount, cydInstructionsCount);

				var orderedInstructions = bookingFromReader.Instructions.OrderBy(i => i.KN_Sequence).ToList();

				var expectedCYDInstructionsSequences = new List<int>();
				if (bookingFromReader.IsPickupDirection)
				{
					expectedCYDInstructionsSequences = Enumerable.Range(1, expectedCYDInstructionAmount).ToList();
				}
				else if (bookingFromReader.IsDeliveryDirection)
				{
					expectedCYDInstructionsSequences = Enumerable.Range(expectedCYDInstructionAmount, expectedInstructionAmount).ToList();
				}

				foreach (var instruction in orderedInstructions.Where(i => expectedCYDInstructionsSequences.Contains(i.KN_Sequence)))
				{
					AssertEquals("This instructrion is expected to be a CYD instruction", OrganisationTypesList.Codes.CYD, instruction.OrganisationType);
					AssertEquals("This instruction should have the same container rateable status as the original CYD instruction on the booking", true, instruction.KN_IsContainerRateable);
					AssertEquals("This instruction should have the same drop mode status as the original CYD instruction on the booking", "MDE", instruction.DropMode);
				}

				for (var s = 1; s <= expectedInstructionAmount; s++)
				{
					AssertEquals("Instructions should have no duplicate sequence numbers", s, orderedInstructions[s - 1].KN_Sequence);
				}
			});
		}

		ZString? GetInstructionTypeForBookingDirection(DtbBooking booking)
		{
			ZString? instructionTypeForDirection = null;

			if (booking.KM_Direction == "ORG" || booking.KM_Direction == "EXP")
			{
				instructionTypeForDirection = InstructionTypes.Codes.PickUp;
			}
			else if (booking.KM_Direction == "DST" || booking.KM_Direction == "IMP")
			{
				instructionTypeForDirection = InstructionTypes.Codes.Delivery;
			}

			return instructionTypeForDirection;
		}

		protected abstract DtbBookingDataObjectReader GetNewReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBooking booking, UniversalShipment topLevelDO);
		protected abstract DtbBookingDataObjectReader GetNewReader(UniversalShipment bookingDO, IXmlImportLogger logger, DtbBookingConsolidation consolidation, UniversalShipment topLevelDO, DtbBooking booking = null, UniversalObjectFactory uFactory = null);

		ICusEntryNumAdditionalReferenceCollection GetAdditionalReferenceNumbersForWayBills(DtbBooking booking)
		{
			return booking.ConsolidationSingleJob.AdditionalReferenceNumbers;
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory.BOFactory)); }
		}

		TransportBookingTestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;
		const string EdiOrgCode = "EDICRY";

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
	}
}
