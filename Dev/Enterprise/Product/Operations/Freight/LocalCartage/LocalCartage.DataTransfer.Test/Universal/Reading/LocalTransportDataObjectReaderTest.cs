using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	sealed class LocalTransportDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestClientAndOrderNumberMatch()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 1);
			cartage.JJ_OrderReferenceNumber = "000";
			var job = new JobHeader.Loader(cartage).TryLoadOrCreate();
			var client = Helper.CreateOrgHeader("AAABBB", "1 Blue Street");
			job.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			Factory.Save();
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage));
			var writer = new LocalTransportDataObjectWriter(writeManager);
			var cartageDataObject = writer.GetDataObject(cartage);
			var addressDataObject = cartageDataObject.AddOrgAddress(writeManager, client.MainAddress, DocAddressType.BuyingParty);
			addressDataObject.AddressType = nameof(DocAddressType.LocalClient); // workaround, does not allow export of Local Client
																				   // reset references so we don't get a match on Consignment ID
			cartage.JJ_ConsignmentID = "111";
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertEquals(cartage.PK, cartageRead.PK);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'LocalClient':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Information - Successfully loaded matching CommonCartage.
Information - Populating CommonCartage...
Information - Matching 'LocalClient':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Matching 'SendersLocalClient':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Warning - Unknown Address Type [SendersLocalClient] found. Job Document Address not imported.
Information - Matching 'LocalClient':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Warning - Unknown Address Type [LocalClient] found. Job Document Address not imported.
Warning - JobCosting element was ignored. To import JobCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Information - Updated Port Transport T00001000 from UniversalShipment.
				".Trim(), logger.Logs);
		}

		public void TestClientAndOrderNumberMatchFromTransportBookingUsingSendingParty()
		{
			var blueCo = Helper.CreateOrgHeader("AAABBB", "1 Blue Street");
			Factory.Save();
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Loose.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var readerFactory = new UniversalObjectFactory();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, readerFactory);
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			readerFactory.SaveForTesting();
			AssertEquals(blueCo.MainAddress.PK, cartageBO.LocalClientAddress.PK);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Information - No matching CommonCartage found, creating new CommonCartage.
Information - Populating CommonCartage...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Information - Added Port Transport  from UniversalShipment.
".Trim(), logger.Logs);
			logger.ClearLogs();
			var readerAgain = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageAgainBO = readerAgain.ReadIntoBusinessObject();
			AssertNotNull(cartageAgainBO);
			AssertEquals(cartageBO.PK, cartageAgainBO.PK);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Information - Successfully loaded matching CommonCartage.
Information - Populating CommonCartage...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Information - Updated Port Transport T00001000 from UniversalShipment.
".Trim(), logger.Logs);
		}

		public void TestClientAndOrderNumberMatchFromTransportBookingWhichHasNoJobShouldPopulateClientRequestedBillingParty()
		{
			using (AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var orangeCo = Helper.CreateOrgHeader("AAABBB", "1 Orange Street");
				Factory.Save();
				var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Loose - ClientRequestedBillingParty.xml");
				using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
				}

				var readerFactory = new UniversalObjectFactory();
				var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, readerFactory);
				var cartageBO = reader.ReadIntoBusinessObject();
				AssertNotNull(cartageBO);
				readerFactory.SaveForTesting();
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching CommonCartage found, creating new CommonCartage.
Information - Populating CommonCartage...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Information - Matching 'ClientRequestedBillingParty':- Matched to 'AAABBB' by code, address '1 Orange Street' (only address).
Information - Added Port Transport  from UniversalShipment.
".Trim(), logger.Logs);
				logger.ClearLogs();
				Assert("ClientRequestedBillingParty should be present", cartageBO.DocAddresses.Find(da => da.DocAddressType == DocAddressType.ClientRequestedBillingParty).Count() == 1);
			}
		}

		public void TestReferencesWithETB()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingReferencesBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var uFactory = new UniversalObjectFactory();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, uFactory);
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Test XML file contains 3 Additional Refs: MSB, HSB and ETB. A new 4th TRF reference is expected to be added", 4, cartageBO.AdditionalReferenceNumbers.Count);
			AssertEquals("Testing ExternalTransportBookingNumber", "ETB00001", cartageBO.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber).FirstOrDefault().ToString());
			AssertEquals("Testing TransportReference", "TB00000034", cartageBO.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportAdditionalReferenceTypes.Codes.TransportReference).FirstOrDefault().ToString());
			AssertEquals("Testing Ref #", "ETB00001", cartageBO.JJ_OrderReferenceNumber);
			uFactory.SaveForTesting();
			var cartageReadInAgainBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageReadInAgainBO);
			AssertEquals(cartageBO.PK, cartageReadInAgainBO.PK);
		}

		public void TestReferencesWithoutETB()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - ReferencesNoETB.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var uFactory = new UniversalObjectFactory();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, uFactory);
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Testing Ref #", "TB00000034", cartageBO.JJ_OrderReferenceNumber);
			uFactory.SaveForTesting();
			var cartageReadInAgainBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageReadInAgainBO);
			AssertEquals(cartageBO.PK, cartageReadInAgainBO.PK);
		}

		public void TestDoNotMatchUXMLSourceTBNumberWithExternalTBOrderRef()
		{
			// create Cartage created from internal booking TB00000001, with external booking TB00000034 (TB00000034 happens to match the internal booking # in the UXML we will import)
			var existingCartage = Factory.New<CommonCartage>();
			existingCartage.JJ_ConsignmentID = "T12345678";
			existingCartage.JJ_OrderReferenceNumber = "TB00000034"; // Reader reads ETB into JJ_OrderReferenceNumber
			existingCartage.JJ_WaybillNumber = "W12345678";
			existingCartage.JJ_QuoteNumber = "Q12345678";
			var job = new JobHeader.Loader(existingCartage).TryLoadOrCreate();
			var client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BACCRI"));
			job.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			LocalTransportDataObjectWriterTest.SetupAdditionalReference(existingCartage.AdditionalReferenceNumbers.AddNew(), AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber, "TB00000034");
			LocalTransportDataObjectWriterTest.SetupAdditionalReference(existingCartage.AdditionalReferenceNumbers.AddNew(), AdditionalReferenceTypes.Codes.TransportReference, "TB00000001");
			Factory.Save();
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingReferencesBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			cartageRead.Job.Dispose();
			AssertNotEquals(existingCartage.PK, cartageRead.PK);
			AssertEquals("Test XML file contains 3 Additional Refs: MSB, HSB and ETB. A new 4th TRF reference is expected to be added", 4, cartageRead.AdditionalReferenceNumbers.Count);
			AssertEquals("Testing ExternalTransportBookingNumber", "ETB00001", cartageRead.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber).FirstOrDefault().ToString());
			AssertEquals("Testing TransportReference", "TB00000034", cartageRead.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(TransportAdditionalReferenceTypes.Codes.TransportReference).FirstOrDefault().ToString());
			AssertEquals("Testing Ref #", "ETB00001", cartageRead.JJ_OrderReferenceNumber);
		}

		public void TestMatchCartageUxmlETBWithCartageETB()
		{
			// create Cartage created from internal booking TB00000034, with external booking ETB00001
			var existingCartage = Factory.New<CommonCartage>();
			existingCartage.JJ_ConsignmentID = "T12345678";
			existingCartage.JJ_OrderReferenceNumber = "ETB00001"; // Reader reads ETB into JJ_OrderReferenceNumber
			existingCartage.JJ_WaybillNumber = "W12345678";
			existingCartage.JJ_QuoteNumber = "Q12345678";
			var job = new JobHeader.Loader(existingCartage).TryLoadOrCreate();
			var client = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BACCRI"));
			job.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			LocalTransportDataObjectWriterTest.SetupAdditionalReference(existingCartage.AdditionalReferenceNumbers.AddNew(), AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber, "ETB00001");
			LocalTransportDataObjectWriterTest.SetupAdditionalReference(existingCartage.AdditionalReferenceNumbers.AddNew(), AdditionalReferenceTypes.Codes.TransportReference, "TB00000034");
			Factory.Save();
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingReferencesBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertEquals(existingCartage.PK, cartageRead.PK);
		}

		public void TestUnmatchedOrgNotes()
		{
			var year = ZDateTime.Now.Year;
			var unmatchedOrgRegistryItem = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			unmatchedOrgRegistryItem.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrgRegistryItem);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 2);
			cartage.JJ_OrderReferenceNumber = "000";
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "Consignor st", "2000", "Sydney", "AUSYD", false);
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			cartage.Containers.DeleteAll();
			var move = Helper.SetupBookedMove(cartage.ContainerBookedMoves.AddNew(), cnr, cto, new ZDateTime(year, 1, 1));
			move.Container.JC_ContainerNum = "CONT1";
			move.EW_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			move.CartageLegs.DeleteAll();
			var container1leg1 = Helper.SetupLeg(move.CartageLegs.AddNew(), cnr, null, cto, new ZDateTime(year, 1, 1));
			container1leg1.JU_LegNotes = "Leg1Note";
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertNotEquals(cartage, cartageRead);
			AssertEquals(1, cartageRead.Containers.Count());
			var containerRead1 = AssertContainer(cartageRead, "CONT1");
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs.Count);
			var leg = cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs[0];
			AssertEquals(unmatchedOrgRegistryItem.Organisation, leg.PickupFromDocAddress.OrganisationPK);
			AssertEquals(unmatchedOrgRegistryItem.Organisation, leg.DeliverToDocAddress.OrganisationPK);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching CommonCartage found, creating new CommonCartage.
Information - Populating CommonCartage...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Matching 'LocalCartageExporter':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - Matching 'LocalCartageCTO':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Added Port Transport  from UniversalShipment.
				".Trim(), logger.Logs);
			var unmatchedOrgNotes = cartageRead.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("unmatchedOrgNotes.Length", 1, unmatchedOrgNotes.Length);
			var unmatchedOrgNote = unmatchedOrgNotes[0];
			AssertMultilineASCIIEquals("Unmatched Orgs Note Content", @"Organisation Type: Consignor
Owner Code: 
EDI Code: CNRSYD
Organisation Name: CNR
Address Line 1: Consignor st
Address Line 2: 
City: Sydney
Post Code: 2000
State or Province: NSW
Country: AU
Doc Address Type: 
 
Organisation Type: CTO
Owner Code: 
EDI Code: CTOSYD
Organisation Name: CTO
Address Line 1: Wharf st
Address Line 2: 
City: Sydney
Post Code: 2000
State or Province: NSW
Country: AU
Doc Address Type: 
 ", unmatchedOrgNote.ST_NoteText);
		}

		public void TestWithNotes()
		{
			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = false;
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			cartageDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			cartageDataObject.NoteCollection.Add(noteDataObject);
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			StmNote[] note = cartageBO.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);
			CombineAssertions(delegate
			{
				AssertNoteContents(note[0]);
				AssertEquals("noteBO.ST_IsCustomDescription", true, note[0].ST_IsCustomDescription);
			});
		}

		public void TestNotesNotOnTopLevelDO_SchemaVersion2012()
		{
			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = false;
			var cartageDataObject_Parent = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			cartageDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			cartageDataObject.NoteCollection.Add(noteDataObject);
			cartageDataObject.SetParentShipmentCollection(() => new List<UniversalShipment>()
			{ cartageDataObject_Parent });
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
				var cartageBO = reader.ReadIntoBusinessObject();
				AssertNotNull(cartageBO);
				var note = cartageBO.Notes.FindByDescription("DOG FLOGGER!!");
				AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);
			}
		}

		public void TestLocalTransportFieldMappings()
		{
			TestLocalTransportFieldMappingsBase();
		}

		public void TestLocalTransportFieldMappings_GetVesselNameFallingBackToLloydsNumberLookup()
		{
			Action<UniversalShipment> setVesselNameNull = (UniversalShipment cartageDataObject) =>
			{
				var transportLeg = cartageDataObject.TransportLegCollection[0];
				transportLeg.VesselName = null;
			};
			TestLocalTransportFieldMappingsBase(setVesselNameNull);
		}

		void TestLocalTransportFieldMappingsBase(Action<UniversalShipment> changeCartageDataObject = null)
		{
			var now = ZDateTime.Now;
			var dayFromNow = now.AddDays(1);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, 3);
			cartage.JJ_OrderReferenceNumber = "order1";
			cartage.JJ_QuoteNumber = "quote1";
			cartage.JJ_WaybillNumber = "waybill1";
			cartage.JJ_F3_NKPackType = "PLT";
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			cartage.JJ_EstimatedPickup = now.AddHours(1);
			cartage.JJ_EstimatedDelivery = now.AddHours(2);
			cartage.JJ_A_JCL = now.AddHours(3);
			LocalTransportDataObjectWriterTest.SetupAdditionalReference(cartage.AdditionalReferenceNumbers.AddNew(), "OTH", "hello");
			LocalTransportDataObjectWriterTest.SetupAdditionalReference(cartage.AdditionalReferenceNumbers.AddNew(), "TRF", "transportIDDD");
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.SydLaxSailing);
			cartage.JJ_JX_Sailing = sailingHelper.SydLaxSailing.PK;
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			var cfs = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "CFS", "Depot st", "2000", "Sydney", "AUSYD", false);
			var cne = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE", "Consignee st", "2000", "Sydney", "AUSYD", false);
			var cne2 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE2", "Consignee2 st", "2000", "Sydney", "AUSYD", false);
			var cyd = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageYard, "CYD", "Yard st", "2000", "Sydney", "AUSYD", false);
			Factory.Save();
			var container1 = Helper.SetupBookedMove(cartage.ContainerBookedMoves[0], cto, cne, now);
			var container2 = Helper.SetupBookedMove(cartage.ContainerBookedMoves[1], cto, cne, now.AddMinutes(10));
			var container3 = Helper.SetupBookedMove(cartage.ContainerBookedMoves[2], cto, cne, now.AddMinutes(20));
			var package1 = Helper.SetupBookedMove(cartage.LooseBookedMoves[0], cfs, cne, dayFromNow);
			var package2 = Helper.SetupBookedMove(cartage.LooseBookedMoves[1], cfs, cne, dayFromNow.AddMinutes(10));
			var package3 = Helper.SetupBookedMove(cartage.LooseBookedMoves[2], cfs, cne2, dayFromNow.AddMinutes(20));
			container1.Container.JC_ContainerNum = "CONT1";
			container2.Container.JC_ContainerNum = "CONT2";
			container3.Container.JC_ContainerNum = "CONT3";
			Helper.SetupBookedMove(package1, 10, Constants.PkgUnit.Bag, 11, Constants.Weight.Kilograms, 12, Constants.Volume.CubicMetres);
			Helper.SetupBookedMove(package2, 20, Constants.PkgUnit.Box, 21, Constants.Weight.Pounds, 22, Constants.Volume.CubicFeet);
			Helper.SetupBookedMove(package3, 30, Constants.PkgUnit.Pallet, 31, Constants.Weight.Ounces, 32, Constants.Volume.CubicInches);
			container1.EW_DropMode = "";
			container2.EW_DropMode = Constants.FCLEquipmentNeeded.WaitForUnpack;
			container3.EW_DropMode = "";
			package1.EW_DropMode = "";
			package2.EW_DropMode = "";
			package3.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			container1.CartageLegs.DeleteAll();
			container2.CartageLegs.DeleteAll();
			container3.CartageLegs.DeleteAll();
			package1.CartageLegs.DeleteAll();
			package2.CartageLegs.DeleteAll();
			package3.CartageLegs.DeleteAll();
			var container1leg1 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cto, null, cfs, now);
			var container1leg2 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cfs, null, cne, now.AddMinutes(10));
			var container1leg3 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cne, null, cfs, now.AddMinutes(20));
			var container1leg4 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cfs, null, cyd, now.AddMinutes(30));
			var container2leg1 = Helper.SetupLeg(container2.CartageLegs.AddNew(), cto, cne, cyd, now.AddMinutes(40));
			var container3leg1 = Helper.SetupLeg(container3.CartageLegs.AddNew(), cto, null, cne, now.AddMinutes(50));
			var container3leg2 = Helper.SetupLeg(container3.CartageLegs.AddNew(), cne, null, cyd, now.AddMinutes(60));
			var package1leg1 = Helper.SetupLeg(package1.CartageLegs.AddNew(), cfs, null, cne, dayFromNow);
			var package2leg1 = Helper.SetupLeg(package2.CartageLegs.AddNew(), cfs, null, cne, dayFromNow.AddMinutes(10));
			var package3leg1 = Helper.SetupLeg(package3.CartageLegs.AddNew(), cfs, null, cne2, dayFromNow.AddMinutes(20));
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			if (changeCartageDataObject != null)
			{
				changeCartageDataObject.Invoke(cartageDataObject);
			}

			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			cartage.JJ_QuoteNumber = "111";
			cartage.JJ_WaybillNumber = "111";
			cartage.AdditionalReferenceNumbers.RemoveAndDeleteAll();
			Factory.Save();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertNotEquals(cartage, cartageRead);
			AssertEquals("order1", cartageRead.JJ_OrderReferenceNumber);
			AssertEquals("quote1", cartageRead.JJ_QuoteNumber);
			AssertEquals("waybill1", cartageRead.JJ_WaybillNumber);
			AssertEquals("Pack Type - total packs weight volume were zero, so defaulted from booked moves", "PCE", cartageRead.JJ_F3_NKPackType);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, cartageRead.JJ_DropMode);
			AssertEquals(now.AddHours(1), cartageRead.JJ_EstimatedPickup);
			AssertEquals(now.AddHours(2), cartageRead.JJ_EstimatedDelivery);
			AssertEquals(now.AddHours(3), cartageRead.JJ_A_JCL);
			AssertEquals(3, cartageRead.AdditionalReferenceNumbers.Count);
			AssertAdditionalReference(cartageRead.AdditionalReferenceNumbers[0], "OTH", "hello");
			AssertAdditionalReference(cartageRead.AdditionalReferenceNumbers[1], "TRF", "transportIDDD");
			AssertAdditionalReference(cartageRead.AdditionalReferenceNumbers[2], "TRF", "order1");
			Assert(!cartageRead.LCLReceivalCommences.IsEmpty);
			Assert(!cartageRead.LCLCutOff.IsEmpty);
			Assert(!cartageRead.FCLReceivalCommences.IsEmpty);
			Assert(!cartageRead.FCLCutOff.IsEmpty);
			Assert(!cartageRead.FCLAvailabilityDate.IsEmpty);
			Assert(!cartageRead.FCLStorageDate.IsEmpty);
			Assert(!cartageRead.LCLAvailabilityDate.IsEmpty);
			Assert(!cartageRead.LCLStorageDate.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JV_NKVessel, cartageRead.Vessel);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JV_VoyageFlight, cartageRead.VoyageFlight);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_RL_NKPortOfLoading, cartageRead.PortOfLoading);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JB_RL_NKPortOfDischarge, cartageRead.PortOfDischarge);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_E_DEP, cartageRead.E_DEP);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JB_E_ARV, cartageRead.E_ARV);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_CTOReceivalCommences, cartageRead.FCLReceivalCommences);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_CTOCutOff, cartageRead.FCLCutOff);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotReceivalCommences, cartageRead.LCLReceivalCommences);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotCutOff, cartageRead.LCLCutOff);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JB_CTOAvailabilityDate, cartageRead.FCLAvailabilityDate);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JB_CTOStorageDate, cartageRead.FCLStorageDate);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotAvailabilityDate, cartageRead.LCLAvailabilityDate);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotStorageDate, cartageRead.LCLStorageDate);
			AssertEquals(3, cartageRead.Containers.Count());
			var containerRead1 = AssertContainer(cartageRead, "CONT1");
			var containerRead2 = AssertContainer(cartageRead, "CONT2");
			var containerRead3 = AssertContainer(cartageRead, "CONT3");
			AssertEquals(4, cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs.Count);
			AssertLeg(cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs[0], container1leg1);
			AssertLeg(cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs[1], container1leg2);
			AssertLeg(cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs[2], container1leg3);
			AssertLeg(cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs[3], container1leg4);
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead2)[0].CartageLegs.Count);
			AssertLeg(cartageRead.GetBookedMoves(containerRead2)[0].CartageLegs[0], container2leg1);
			AssertEquals(2, cartageRead.GetBookedMoves(containerRead3)[0].CartageLegs.Count);
			AssertLeg(cartageRead.GetBookedMoves(containerRead3)[0].CartageLegs[0], container3leg1);
			AssertLeg(cartageRead.GetBookedMoves(containerRead3)[0].CartageLegs[1], container3leg2);
			AssertEquals(3, cartageRead.LooseBookedMoves.Count);
			var packageRead1 = AssertPackage(cartageRead, package1);
			var packageRead2 = AssertPackage(cartageRead, package2);
			var packageRead3 = AssertPackage(cartageRead, package3);
			AssertEquals(1, packageRead1.CartageLegs.Count);
			AssertLeg(packageRead1.CartageLegs[0], package1leg1);
			AssertEquals(1, packageRead2.CartageLegs.Count);
			AssertLeg(packageRead2.CartageLegs[0], package2leg1);
			AssertEquals(1, packageRead3.CartageLegs.Count);
			AssertLeg(packageRead3.CartageLegs[0], package3leg1);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching CommonCartage found, creating new CommonCartage.
Information - Populating CommonCartage...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Matching 'LocalCartageCFS':- Matched to 'CFSSYD' by code, address 'Depot st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageCFS':- Matched to 'CFSSYD' by code, address 'Depot st' (only address).
Information - Matching 'LocalCartageImporter':- Matched to 'CNESYD' by code, address 'Consignee st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageImporter':- Matched to 'CNESYD' by code, address 'Consignee st' (only address).
Information - Matching 'LocalCartageCFS':- Matched to 'CFSSYD' by code, address 'Depot st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageCFS':- Matched to 'CFSSYD' by code, address 'Depot st' (only address).
Information - Matching 'LocalCartageYard':- Matched to 'CYDSYD' by code, address 'Yard st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Matching 'LocalCartageImporter':- Matched to 'CNESYD' by code, address 'Consignee st' (only address).
Information - Matching 'LocalCartageYard':- Matched to 'CYDSYD' by code, address 'Yard st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Matching 'LocalCartageImporter':- Matched to 'CNESYD' by code, address 'Consignee st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageImporter':- Matched to 'CNESYD' by code, address 'Consignee st' (only address).
Information - Matching 'LocalCartageYard':- Matched to 'CYDSYD' by code, address 'Yard st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageCFS':- Matched to 'CFSSYD' by code, address 'Depot st' (only address).
Information - Matching 'LocalCartageImporter':- Matched to 'CNESYD' by code, address 'Consignee st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageCFS':- Matched to 'CFSSYD' by code, address 'Depot st' (only address).
Information - Matching 'LocalCartageImporter':- Matched to 'CNESYD' by code, address 'Consignee st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageCFS':- Matched to 'CFSSYD' by code, address 'Depot st' (only address).
Information - Matching 'LocalCartageImporter':- Matched to 'CNE2SYD' by code, address 'Consignee2 st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Added Port Transport  from UniversalShipment.
				".Trim(), logger.Logs);
		}

		public void TestLocalTransportLegs_FCL()
		{
			var now = ZDateTime.Now;
			var dayFromNow = now.AddDays(1);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 2);
			cartage.JJ_OrderReferenceNumber = "000";
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "Consignor st", "2000", "Sydney", "AUSYD", false);
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			Factory.Save();
			var container1 = Helper.SetupBookedMove(cartage.ContainerBookedMoves[0], cnr, cto, now);
			var container2 = Helper.SetupBookedMove(cartage.ContainerBookedMoves[1], cnr, cto, now.AddMinutes(10));
			container1.Container.JC_ContainerNum = "CONT1";
			container2.Container.JC_ContainerNum = "CONT2";
			container1.EW_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			container2.EW_DropMode = Constants.FCLEquipmentNeeded.WaitForUnpack;
			container1.CartageLegs.DeleteAll();
			container2.CartageLegs.DeleteAll();
			var container1leg1 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cnr, null, cto, now);
			var container2leg1 = Helper.SetupLeg(container2.CartageLegs.AddNew(), cnr, null, cto, now.AddMinutes(10));
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			// These should NOT be read in and populate loose booked moves and legs
			cartageDataObject.SetPackingLineCollection(() =>
			{
				var result = new DataObjectList<PackingLine>()
				{ Content = CollectionContent.Complete };
				result.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerNumber = "CONT1",
					PackQty = 10,
					PackType = new PackageType()
					{ Code = "PLT" }
				});
				result.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerNumber = "CONT2",
					PackQty = 5,
					PackType = new PackageType()
					{ Code = "PLT" }
				});
				return result;
			});
			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			Factory.Save();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertNotEquals(cartage, cartageRead);
			AssertEquals(2, cartageRead.Containers.Count());
			var containerRead1 = AssertContainer(cartageRead, "CONT1");
			var containerRead2 = AssertContainer(cartageRead, "CONT2");
			AssertEquals(CartageContainerMode.Containerized, cartageRead.JJ_ContainerMode);
			AssertEquals(0, cartageRead.LooseBookedMoves.Count);
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs.Count);
			AssertLeg(cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs[0], container1leg1);
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead2)[0].CartageLegs.Count);
			AssertLeg(cartageRead.GetBookedMoves(containerRead2)[0].CartageLegs[0], container2leg1);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching CommonCartage found, creating new CommonCartage.
Information - Populating CommonCartage...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - Matching 'LocalCartageExporter':- Matched to 'CNRSYD' by code, address 'Consignor st' (only address).
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageExporter':- Matched to 'CNRSYD' by code, address 'Consignor st' (only address).
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Added Port Transport  from UniversalShipment.
				".Trim(), logger.Logs);
		}

		public void TestEstimatedDatesPopulatedFromLegs()
		{
			var now = ZDateTime.Now;
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 2);
			cartage.JJ_OrderReferenceNumber = "000";
			var cfs = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "CFS", "CFS st", "2000", "Sydney", "AUSYD", false);
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "Consignor st", "2000", "Sydney", "AUSYD", false);
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			Factory.Save();
			var container1 = Helper.SetupBookedMove(cartage.ContainerBookedMoves[0], cfs, cto, now);
			var container2 = Helper.SetupBookedMove(cartage.ContainerBookedMoves[1], cnr, cto, now.AddMinutes(40));
			container1.Container.JC_ContainerNum = "CONT1";
			container2.Container.JC_ContainerNum = "CONT2";
			container1.CartageLegs.DeleteAll();
			container2.CartageLegs.DeleteAll();
			var container1leg1 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cnr, null, cfs, now);
			var container1leg2 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cfs, null, cto, now.AddMinutes(20));
			var container2leg1 = Helper.SetupLeg(container2.CartageLegs.AddNew(), cnr, null, cto, now.AddMinutes(40));
			var container1leg1PlannedPickupTime = container1leg1.JU_PlannedPickupTime;
			var container1leg1EstimatedDeliveryTime = container1leg1.JU_EstimatedDeliveryTime;
			AssertEquals("Precondition: JU_PlannedPickupTime was set in helper correctly", now.AddMinutes(-10), container1leg1PlannedPickupTime);
			AssertEquals("Precondition: JU_EstimatedDeliveryTime was set in helper correctly", now.AddMinutes(-8), container1leg1EstimatedDeliveryTime);
			// Making sure that other legs has different planned pickup / delivery times
			AssertNotEquals(container1leg1.JU_PlannedPickupTime, container1leg2.JU_PlannedPickupTime);
			AssertNotEquals(container1leg2.JU_PlannedPickupTime, container2leg1.JU_PlannedPickupTime);
			AssertNotEquals(container1leg1.JU_EstimatedDeliveryTime, container1leg2.JU_EstimatedDeliveryTime);
			AssertNotEquals(container1leg2.JU_EstimatedDeliveryTime, container2leg1.JU_EstimatedDeliveryTime);
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			Factory.Save();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertEquals("JJ_EstimatedPickup must be populated from JU_PlannedPickupTime of the first leg", container1leg1PlannedPickupTime, cartageRead.JJ_EstimatedPickup);
			AssertEquals("JJ_EstimatedDelivery must be populated from JU_EstimatedDeliveryTime of the first leg", container1leg1EstimatedDeliveryTime, cartageRead.JJ_EstimatedDelivery);
		}

		public void TestLocalTransportLegs_LCL()
		{
			var now = ZDateTime.Now;
			var dayFromNow = now.AddDays(1);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_LCLImport, 2);
			cartage.JJ_OrderReferenceNumber = "order1";
			var cfs = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "CFS", "Depot st", "2000", "Sydney", "AUSYD", false);
			var cne = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE", "Consignee st", "2000", "Sydney", "AUSYD", false);
			Factory.Save();
			var package1 = Helper.SetupBookedMove(cartage.LooseBookedMoves[0], cfs, cne, dayFromNow);
			var package2 = Helper.SetupBookedMove(cartage.LooseBookedMoves[1], cfs, cne, dayFromNow.AddMinutes(10));
			Helper.SetupBookedMove(package1, 10, Constants.PkgUnit.Bag, 11, Constants.Weight.Kilograms, 12, Constants.Volume.CubicMetres);
			Helper.SetupBookedMove(package2, 20, Constants.PkgUnit.Box, 21, Constants.Weight.Pounds, 22, Constants.Volume.CubicFeet);
			package1.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			package2.EW_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			package1.CartageLegs.DeleteAll();
			package2.CartageLegs.DeleteAll();
			var package1leg1 = Helper.SetupLeg(package1.CartageLegs.AddNew(), cfs, null, cne, dayFromNow);
			var package2leg1 = Helper.SetupLeg(package2.CartageLegs.AddNew(), cfs, null, cne, dayFromNow.AddMinutes(10));
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			Factory.Save();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertNotEquals(cartage, cartageRead);
			AssertEquals(2, cartageRead.LooseBookedMoves.Count);
			var packageRead1 = AssertPackage(cartageRead, package1);
			var packageRead2 = AssertPackage(cartageRead, package2);
			AssertEquals(1, packageRead1.CartageLegs.Count);
			AssertLeg(packageRead1.CartageLegs[0], package1leg1);
			AssertEquals(1, packageRead2.CartageLegs.Count);
			AssertLeg(packageRead2.CartageLegs[0], package2leg1);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching CommonCartage found, creating new CommonCartage.
Information - Populating CommonCartage...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Information - Populating CommonBookedCtgMove...
Information - Matching 'LocalCartageCFS':- Matched to 'CFSSYD' by code, address 'Depot st' (only address).
Information - Matching 'LocalCartageImporter':- Matched to 'CNESYD' by code, address 'Consignee st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageCFS':- Matched to 'CFSSYD' by code, address 'Depot st' (only address).
Information - Matching 'LocalCartageImporter':- Matched to 'CNESYD' by code, address 'Consignee st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Added Port Transport  from UniversalShipment.
				".Trim(), logger.Logs);
		}

		public void TestLocalTransportLegs_CustomFields()
		{
			var year = ZDateTime.Now.Year;
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "LTL";
			processTaskTemplate.P0_IsActive = true;
			AddCustomColumn(processTaskTemplate, "Are you Happy?", "BOO");
			AddCustomColumn(processTaskTemplate, "What Makes You Happy?", "STR");
			AddCustomColumn(processTaskTemplate, "The Happy Number", "INT");
			AddCustomColumn(processTaskTemplate, "The Happy Decimal", "DEC");
			AddCustomColumn(processTaskTemplate, "The Date You Are Happy", "DAT");
			Factory.Save();
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLCTOtoCNE, 1);
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "Consignor st", "2000", "Sydney", "AUSYD", false);
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			var container = Helper.SetupBookedMove(cartage.ContainerBookedMoves[0], cnr, cto, new ZDateTime(year, 5, 5));
			var leg = Helper.SetupLeg(container.CartageLegs.AddNew(), cnr, null, cto, new ZDateTime(year, 5, 5));
			leg.SetUserDefinedValue("Are you Happy?", ZBool.True);
			leg.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			leg.SetUserDefinedValue("The Happy Number", new ZInt(42));
			leg.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			leg.SetUserDefinedValue("The Date You Are Happy", ZDateTime.BrettsBirthday);
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			AssertNotNull("Precondition: legData", cartageDataObject);
			AssertNotNull("Precondition: instructions for leg data for data reader to popluate", cartageDataObject.InstructionCollection);
			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			Factory.Save();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertNotEquals(cartage, cartageRead);
			AssertEquals(1, cartageRead.Containers.Count());
			AssertEquals(1, cartageRead.GetBookedMoves(cartageRead.Containers.First()).Length);
			AssertEquals(1, cartageRead.GetBookedMoves(cartageRead.Containers.First())[0].CartageLegs.Count);
			var legBO = cartageRead.GetBookedMoves(cartageRead.Containers.First())[0].CartageLegs[0];
			CombineAssertions(delegate
			{
				var boCustomFields = legBO.GetUserDefinedValues();
				var result = "";
				foreach (var customField in boCustomFields)
				{
					result += customField.PropertyName + " - " + customField.Value + "\r\n";
				}

				AssertMultilineASCIIEquals("All Custom Fields should have been imported with none extra", @"
Are you Happy? - Y
The Date You Are Happy - 18-Sep-71 00:00:00
The Happy Decimal - 7.7
The Happy Number - 42
What Makes You Happy? - Lots Of Ice
			".Trim(), result);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CommonCartage found, creating new CommonCartage.
Information - Populating CommonCartage...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Matching 'LocalCartageExporter':- Matched to 'CNRSYD' by code, address 'Consignor st' (only address).
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Added Port Transport  from UniversalShipment.
			".Trim(), logger.Logs);
			});
		}

		void AddCustomColumn(ProcessTaskTemplate processTaskTemplate, string name, string type)
		{
			var customField = Factory.New<GenCustomColumnDefinition>();
			customField.XC_Name = name;
			customField.XC_Type = type;
			processTaskTemplate.GenCustomColumnDefinitions.Add(customField);
		}

		public void TestLocalTransport_LocalProcessingDropMode_Pic()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 1);
			cartage.JJ_OrderReferenceNumber = "000";
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			Factory.Save();
			cartageDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			cartageDataObject.LocalProcessing.FCLPickupEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(Constants.FCLEquipmentNeeded.SideLoader, cartage.BindToLists.DropModes());
			cartageDataObject.LocalProcessing.FCLDeliveryEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(Constants.FCLEquipmentNeeded.LiftOffOn, cartage.BindToLists.DropModes());
			var picReader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var picCartageRead = picReader.ReadIntoBusinessObject();
			AssertNotNull(picCartageRead);
			AssertNotEquals(cartage, picCartageRead);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, picCartageRead.JJ_DropMode);
		}

		public void TestLocalTransport_LocalProcessingDropMode_Dlv()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLCTOtoCNE, 1);
			cartage.JJ_OrderReferenceNumber = "000";
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			Factory.Save();
			cartageDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			cartageDataObject.LocalProcessing.FCLPickupEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(Constants.FCLEquipmentNeeded.SideLoader, cartage.BindToLists.DropModes());
			cartageDataObject.LocalProcessing.FCLDeliveryEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(Constants.FCLEquipmentNeeded.LiftOffOn, cartage.BindToLists.DropModes());
			var dlvReader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var dlvCartageRead = dlvReader.ReadIntoBusinessObject();
			AssertNotNull(dlvCartageRead);
			AssertNotEquals(cartage, dlvCartageRead);
			AssertEquals(Constants.FCLEquipmentNeeded.LiftOffOn, dlvCartageRead.JJ_DropMode);
		}

		public void TestLocalTransport_ImportPartDeliveries()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Import Part Deliveries.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Pre-condition: ParkType", "PLT", cartageBO.JJ_F3_NKPackType);
			AssertEquals("Pre-condition: Direction", "LOC", cartageBO.JJ_Direction);
			AssertEquals("Pre-condition: ContaionerMode", "LSE", cartageBO.JJ_ContainerMode);
			AssertEquals(1, cartageBO.LooseBookedMoves.Count);
			var looseBookedMove = cartageBO.LooseBookedMoves[0];
			AssertEquals(6, looseBookedMove.EW_BookedPackCount);
			AssertEquals(6m, looseBookedMove.EW_BookedVolume);
			AssertEquals(6m, looseBookedMove.EW_BookedWeight);
			// cartasge totals
			AssertEquals(6, cartageBO.JJ_OuterPacks);
			AssertEquals(6m, cartageBO.JJ_Volume);
			AssertEquals(6m, cartageBO.JJ_Weight);
		}

		public void TestLocalTransport_ImportMultiPartsDeliveries()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Import Part Deliveries Complex.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Pre-condition: ParkType", "PLT", cartageBO.JJ_F3_NKPackType);
			AssertEquals("Pre-condition: Direction", "LOC", cartageBO.JJ_Direction);
			AssertEquals("Pre-condition: ContaionerMode", "LSE", cartageBO.JJ_ContainerMode);
			AssertEquals(50, cartageBO.JJ_OuterPacks);
			AssertEquals(40m, cartageBO.JJ_Weight);
			AssertEquals(30m, cartageBO.JJ_Volume);
			AssertEquals(5, cartageBO.LooseBookedMoves.Count);
			var looseBookedMove = cartageBO.LooseBookedMoves[0];
			AssertEquals(15, looseBookedMove.EW_BookedPackCount);
			AssertEquals(12m, looseBookedMove.EW_BookedWeight);
			AssertEquals(9m, looseBookedMove.EW_BookedVolume);
			looseBookedMove = cartageBO.LooseBookedMoves[1];
			AssertEquals(5, looseBookedMove.EW_BookedPackCount);
			AssertEquals(4m, looseBookedMove.EW_BookedWeight);
			AssertEquals(3m, looseBookedMove.EW_BookedVolume);
			looseBookedMove = cartageBO.LooseBookedMoves[2];
			AssertEquals(10, looseBookedMove.EW_BookedPackCount);
			AssertEquals(8m, looseBookedMove.EW_BookedWeight);
			AssertEquals(6m, looseBookedMove.EW_BookedVolume);
			looseBookedMove = cartageBO.LooseBookedMoves[3];
			AssertEquals(15, looseBookedMove.EW_BookedPackCount);
			AssertEquals(12m, looseBookedMove.EW_BookedWeight);
			AssertEquals(9m, looseBookedMove.EW_BookedVolume);
			looseBookedMove = cartageBO.LooseBookedMoves[4];
			AssertEquals(5, looseBookedMove.EW_BookedPackCount);
			AssertEquals(4m, looseBookedMove.EW_BookedWeight);
			AssertEquals(3m, looseBookedMove.EW_BookedVolume);
		}

		public void TestLocalTransportLegs_LegNotes()
		{
			var now = ZDateTime.Now;
			var dayFromNow = now.AddDays(1);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 2);
			cartage.JJ_OrderReferenceNumber = "000";
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "Consignor st", "2000", "Sydney", "AUSYD", false);
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			Factory.Save();
			var container1 = Helper.SetupBookedMove(cartage.ContainerBookedMoves[0], cnr, cto, now);
			var container2 = Helper.SetupBookedMove(cartage.ContainerBookedMoves[1], cnr, cto, now.AddMinutes(10));
			container1.Container.JC_ContainerNum = "CONT1";
			container2.Container.JC_ContainerNum = "CONT2";
			container1.EW_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			container2.EW_DropMode = Constants.FCLEquipmentNeeded.WaitForUnpack;
			container1.CartageLegs.DeleteAll();
			container2.CartageLegs.DeleteAll();
			var container1leg1 = Helper.SetupLeg(container1.CartageLegs.AddNew(), cnr, null, cto, now);
			container1leg1.JU_LegNotes = "Leg1Note";
			var container2leg1 = Helper.SetupLeg(container2.CartageLegs.AddNew(), cnr, null, cto, now.AddMinutes(10));
			container2leg1.JU_LegNotes = "Leg2Note";
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			Factory.Save();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertNotEquals(cartage, cartageRead);
			AssertEquals(2, cartageRead.Containers.Count());
			var containerRead1 = AssertContainer(cartageRead, "CONT1");
			var containerRead2 = AssertContainer(cartageRead, "CONT2");
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs.Count);
			AssertLeg(cartageRead.GetBookedMoves(containerRead1)[0].CartageLegs[0], container1leg1);
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead2)[0].CartageLegs.Count);
			AssertLeg(cartageRead.GetBookedMoves(containerRead2)[0].CartageLegs[0], container2leg1);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching CommonCartage found, creating new CommonCartage.
Information - Populating CommonCartage...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Matching 'LocalCartageExporter':- Matched to 'CNRSYD' by code, address 'Consignor st' (only address).
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageExporter':- Matched to 'CNRSYD' by code, address 'Consignor st' (only address).
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Added Port Transport  from UniversalShipment.
				".Trim(), logger.Logs);
		}

		public void TestWorkflowCustomFields()
		{
			WorkflowCustomFieldsReaderTestHelper.TestWorkflowCustomFields<UniversalShipment, CommonCartage>((s, l) => new LocalTransportDataObjectReader(s, l, new UniversalObjectFactory()));
		}

		public void TestLocalTransport_ContainerModeInference_Containerised()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Containerised - Freight Mode.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Pre-condition: ContainerMode", Constants.CartageContainerMode.Containerized, cartageBO.JJ_ContainerMode);
		}

		public void TestLocalTransport_ContainerModeInference_Loose()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Loose - Freight Mode.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			cartageBO.Job.Dispose();
			AssertEquals("Pre-condition: ContainerMode", Constants.CartageContainerMode.Loose, cartageBO.JJ_ContainerMode);
		}

		public void TestLocalTransport_ContainerModeInference_Mixed()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Mixed - Freight Mode.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Pre-condition: ContainerMode", Constants.CartageContainerMode.Mixed, cartageBO.JJ_ContainerMode);
		}

		public void TestLocalTransport_ContainerModeInference_Empty()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Empty - Freight Mode.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Pre-condition: ContainerMode", Constants.CartageContainerMode.Mixed, cartageBO.JJ_ContainerMode);
		}

		public void TestAdditionalReferenceForTransportBookingstoLocalTransport()
		{
			var readerFactory = new UniversalObjectFactory();
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - AdditionalReference.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var cartageBusinessObject = RunTestOnceAndAssert(cartageDataObject);
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartageBusinessObject)));
			var cartageDataObject2 = writer.GetDataObject(cartageBusinessObject);
			RunTestOnceAndAssert(cartageDataObject2, "There should still only be one TRF/TB00000001 additional reference for this shipment");
			CommonCartage RunTestOnceAndAssert(UniversalShipment cartageDO, string assertMessageIfAny = "")
			{
				var reader = new LocalTransportDataObjectReader(cartageDO, logger, readerFactory);
				var cartageBO = reader.ReadIntoBusinessObject();
				AssertNotNull(cartageBO);
				readerFactory.SaveForTesting();
				AssertEquals(assertMessageIfAny, 5, cartageBO.AdditionalReferenceNumbers.Count);
				AssertOnlyOneReference(cartageBO.AdditionalReferenceNumbers, "TRF", "TB00000001", assertMessageIfAny);
				AssertOnlyOneReference(cartageBO.AdditionalReferenceNumbers, "HSB", "S00001000"); // Transport booking and consolidation
				AssertOnlyOneReference(cartageBO.AdditionalReferenceNumbers, "BPR", "S00001000"); // Transport booking
				AssertOnlyOneReference(cartageBO.AdditionalReferenceNumbers, "TRF", "TB-TRF"); // Transport booking
				AssertOnlyOneReference(cartageBO.AdditionalReferenceNumbers, "TRF", "CON-TRF"); // Consolidation
				return cartageBO;
			}
		}

		public void TestUpdateAdditionalEmptyReferenceForTransportBookingstoLocalTransportMultipleTimes()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 2);
			cartage.JJ_ConsignmentID = "T00014765";
			Factory.Save();
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			var readerFactory = new UniversalObjectFactory();
			var cartageBusinessObject = RunTestOnceAndAssert(cartageDataObject);
			RunTestOnceAndAssert(cartageDataObject, "There should still only be one TRF/(empty ref no) additional reference for this shipment");
			CommonCartage RunTestOnceAndAssert(UniversalShipment cartageDO, string assertMessageIfAny = "")
			{
				var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - UpdateWithAdditionalEmptyReference.xml");
				using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
				}

				var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, readerFactory);
				var cartageBO = reader.ReadIntoBusinessObject();
				AssertNotNull(cartageBO);
				readerFactory.SaveForTesting();
				AssertOnlyOneReference(cartageBO.AdditionalReferenceNumbers, "TRF", string.Empty, assertMessageIfAny);
				return cartageBO;
			}
		}

		public void TestLocalTransport_LinkJobHeaderToParentJob()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 2);
			cartage.JJ_OrderReferenceNumber = "000";
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<IDtbBooking>();
			booking.KM_JobID = "TB123";
			booking.KM_KB_Booking = consolidation.PK;
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "AAA";
			client.OH_FullName = "ClientAAA";
			var bookingJob = new JobHeader.Loader((IJobHeaderParent)booking).TryCreate();
			bookingJob.JH_ParentID = booking.PK;
			bookingJob.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			bookingJob.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			Factory.Save();
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			// reset references so we don't get a match
			cartage.JJ_ConsignmentID = "111";
			cartage.JJ_OrderReferenceNumber = "111";
			Factory.Save();
			var dataWritingManager = new DataWritingManager(new DummyActionInfo());
			var addressDataObject = cartageDataObject.AddOrgAddress(dataWritingManager, client.MainAddress, AddressTypes.SendersLocalClient);
			logger.OutboundSessionTracker = dataWritingManager;
			logger.TopLevelDataObject = cartageDataObject;
			logger.TopLevelDataContext.AddDataSource(DataContextType.TransportBooking, "TB123");
			logger.TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			cartageRead.Job.Dispose();
			AssertNotEquals(cartage, cartageRead);
			AssertEquals(bookingJob.PK, cartageRead.Job.JH_JH_ParentJob);
		}

		public void TestLocalTransport_AddressImportFromBooking()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingTestAddressImportBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals(DocAddressType.LocalCartageCTO, cartageBO.FirstDocAddress.DocAddressType);
			AssertEquals(false, cartageBO.FirstDocAddress.E2_AddressOverride);
			AssertEquals(DocAddressType.LocalCartageImporter, cartageBO.SecondDocAddress.DocAddressType);
			AssertEquals(true, cartageBO.SecondDocAddress.E2_AddressOverride);
			AssertEquals(DocAddressType.LocalCartageYard, cartageBO.ThirdDocAddress.DocAddressType);
			AssertEquals(false, cartageBO.ThirdDocAddress.E2_AddressOverride);
		}

		public void TestLocalTransport_TransportLegCollectionImportFromBooking()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - TransportLegCollection.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("A P MOLLER", cartageBO.Vessel);
			AssertEquals("123", cartageBO.VoyageFlight);
			AssertEquals("NZAKL", cartageBO.PortOfLoading);
			AssertEquals("AUSYD", cartageBO.PortOfDischarge);
			AssertEquals(new ZDateTime(2016, 10, 24, 1, 0, 0), cartageBO.E_DEP);
			AssertEquals(new ZDateTime(2016, 10, 25, 1, 0, 0), cartageBO.E_ARV);
			AssertEquals(ZDateTime.Empty, cartageBO.FCLReceivalCommences);
			AssertEquals(ZDateTime.Empty, cartageBO.FCLCutOff);
			AssertEquals(ZDateTime.Empty, cartageBO.FCLAvailabilityDate);
			AssertEquals(ZDateTime.Empty, cartageBO.FCLStorageDate);
			AssertEquals(ZDateTime.Empty, cartageBO.LCLReceivalCommences);
			AssertEquals(ZDateTime.Empty, cartageBO.LCLCutOff);
			AssertEquals(ZDateTime.Empty, cartageBO.LCLReceivalCommences);
			AssertEquals(ZDateTime.Empty, cartageBO.LCLAvailabilityDate);
			AssertEquals(ZDateTime.Empty, cartageBO.LCLStorageDate);
		}

		public void TestLocalTransportLegs_ValidateEstimatedDepartureEstimatedArrival_Integration()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Invalid EstimatedDeparture.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			reader.ReadIntoBusinessObject();
			CombineAssertions("Should indicate that estimated departure is later than estimated arrival", () =>
			{
				Assert("Should have message", logger.Logs.Contains("ETA cannot be before ETD. Transport Leg information not updated."));
			});
		}

		public void TestLocalTransportLegs_Air_EstimatedDepartureLessThanOneDayLaterThanEstimatedArrival_HasNoWarning()
		{
			var assertionMessage = "Should allow ETD less than one day later than estimated arrival, when the transport mode is air";
			var estimatedDeparture = new ZDateTime(2022, 9, 8, 1, 0, 0);
			var estimatedArrival = new ZDateTime(2022, 9, 7, 23, 0, 0);
			var transportModeCode = TransportModes.Air;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_Air_EstimatedDepartureMoreThanOneDayLaterThanEstimatedArrival_HasWarning()
		{
			var assertionMessage = "Should warn that ETD is more than one day later than estimated arrival, when the transport mode is air";
			var estimatedDeparture = new ZDateTime(2022, 9, 8, 23, 0, 0);
			var estimatedArrival = new ZDateTime(2022, 9, 7, 1, 0, 0);
			var transportModeCode = TransportModes.Air;
			var expectedMessage = "ETA cannot be more than a day before ETD. Transport Leg information not updated.";
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_NonAir_EstimatedDepartureEarlierThanEstimatedArrival_HasNoWarning()
		{
			var assertionMessage = "Should allow ETD earlier than estimated arrival, when the transport mode is not air";
			var estimatedDeparture = new ZDateTime(2022, 9, 8, 1, 0, 0);
			var estimatedArrival = new ZDateTime(2022, 9, 8, 2, 0, 0);
			var transportModeCode = TransportModes.Road;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_NonAir_EstimatedDepartureLaterThanEstimatedArrival_HasWarning()
		{
			var assertionMessage = "Should warn that ETD is later than estimated arrival, when the transport mode is not air";
			var estimatedDeparture = new ZDateTime(2022, 9, 8, 2, 0, 0);
			var estimatedArrival = new ZDateTime(2022, 9, 8, 1, 0, 0);
			var transportModeCode = TransportModes.Road;
			var expectedMessage = "ETA cannot be before ETD. Transport Leg information not updated.";
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_Air_EstimatedDepartureNull_HasNoWarning()
		{
			var assertionMessage = "Should allow null ETD, when the transport mode is air";
			var estimatedDeparture = (ZDateTime?)null;
			var estimatedArrival = new ZDateTime(2022, 9, 7, 23, 0, 0);
			var transportModeCode = TransportModes.Air;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_Air_EstimatedArrivalNull_HasNoWarning()
		{
			var assertionMessage = "Should allow null ETA, when the transport mode is air";
			var estimatedDeparture = new ZDateTime(2022, 9, 8, 1, 0, 0);
			var estimatedArrival = (ZDateTime?)null;
			var transportModeCode = TransportModes.Air;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_Air_EstimatedArrivalEstimatedDepartureBothNull_HasNoWarning()
		{
			var assertionMessage = "Should allow null ETA and null ETD, when the transport mode is air";
			var estimatedDeparture = (ZDateTime?)null;
			var estimatedArrival = (ZDateTime?)null;
			var transportModeCode = TransportModes.Air;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_NonAir_EstimatedDepartureNull_HasNoWarning()
		{
			var assertionMessage = "Should allow null ETD, when the transport mode is not air";
			var estimatedDeparture = (ZDateTime?)null;
			var estimatedArrival = new ZDateTime(2022, 9, 7, 23, 0, 0);
			var transportModeCode = TransportModes.Road;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_NonAir_EstimatedArrivalNull_HasNoWarning()
		{
			var assertionMessage = "Should allow null ETA, when the transport mode is not air";
			var estimatedDeparture = new ZDateTime(2022, 9, 8, 1, 0, 0);
			var estimatedArrival = (ZDateTime?)null;
			var transportModeCode = TransportModes.Road;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_NonAir_EstimatedArrivalEstimatedDepartureBothNull_HasNoWarning()
		{
			var assertionMessage = "Should allow null ETA and ETD, when the transport mode is not air";
			var estimatedDeparture = (ZDateTime?)null;
			var estimatedArrival = (ZDateTime?)null;
			var transportModeCode = TransportModes.Road;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_Air_EstimatedArrivalInvalid_HasNoWarning()
		{
			var assertionMessage = "Should allow invalid ETA, when the transport mode is air";
			var estimatedDeparture = new ZDateTime(2022, 9, 8, 1, 0, 0);
			var estimatedArrival = ZDateTime.Invalid;
			var transportModeCode = TransportModes.Air;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_NonAir_EstimatedArrivalInvalid_HasNoWarning()
		{
			var assertionMessage = "Should allow invalid ETA, when the transport mode is not air";
			var estimatedDeparture = new ZDateTime(2022, 9, 8, 1, 0, 0);
			var estimatedArrival = ZDateTime.Invalid;
			var transportModeCode = TransportModes.Road;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void TestLocalTransportLegs_EstimatedDepartureInvalid_HasNoWarning()
		{
			var assertionMessage = "Should allow invalid ETD for any transport mode";
			var estimatedDeparture = ZDateTime.Invalid;
			var estimatedArrival = new ZDateTime(2022, 9, 7, 23, 0, 0);
			var transportModeCode = TransportModes.Road;
			var expectedMessage = (string)null;
			AssertEstimatedDepartureEstimatedArrivalValidation(assertionMessage, estimatedDeparture, estimatedArrival, transportModeCode, expectedMessage);
		}

		public void AssertEstimatedDepartureEstimatedArrivalValidation(string assertionMessage, ZDateTime? estimatedDeparture, ZDateTime? estimatedArrival, string transportModeCode, string expectedMessage)
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Invalid EstimatedDeparture.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			cartageDataObject.TransportMode = new CodeDescriptionPair { Code = transportModeCode, Description = "Uninformative description" };
			foreach (var transportLeg in cartageDataObject.TransportLegCollection)
			{
				transportLeg.EstimatedArrival = estimatedArrival;
				transportLeg.EstimatedDeparture = estimatedDeparture;
			}
			var cartageBO = reader.ReadIntoBusinessObject();
			if (expectedMessage != null)
			{
				CombineAssertions(assertionMessage, () =>
				{
					Assert($"Should have message {expectedMessage}", logger.Logs.Contains(expectedMessage));
					AssertEquals("Should not populate E_DEP", ZDateTime.Empty, cartageBO.E_DEP);
					AssertEquals("Should not populate E_ARV", ZDateTime.Empty, cartageBO.E_ARV);
					AssertEquals("Should not populate other transport leg information such as vessel name", ZString.Empty, cartageBO.Vessel);
				});
			}
			else
			{
				var expectedEstimatedDeparture = estimatedDeparture ?? new ZDateTime(string.Empty);
				var expectedEstimatedArrival = estimatedArrival ?? new ZDateTime(string.Empty);
				var unrelatedWarnings = @"Matching 'TransportCompanyDocumentaryAddress':- No match found for '[Org. Code: CAREDIAKL; Company Name: CARGOWISE EDI PTY LTD; Address Code: LEVEL 1 / 19A RONWOOD AVE; Address 1: LEVEL 1 / 19A RONWOOD AVENUE; City: Manukau City]'.
Matching 'LocalCartageCTO':- No match found for '[Org. Code: AACFOR_CN; Company Name: AAC FOR COM; Address Code: 100 QUEEN STREET; Address 1: 100 QUEEN STREET; Address 2: AAAA; City: EAGLE FAARM]'.
Matching 'LocalCartageYard':- No match found for '[Org. Code: AACFOR_CN; Company Name: AAC FOR COM; Address Code: 100 QUEEN STREET; Address 1: 100 QUEEN STREET; Address 2: AAAA; City: EAGLE FAARM]'.";
				CombineAssertions(assertionMessage, () =>
				{
					AssertEquals("Should not have warning message related to estimated departure or estimated arrival", string.Empty, logger.GetWarnings().Replace(unrelatedWarnings, ""));
					AssertEquals("Should populate E_DEP", expectedEstimatedDeparture, cartageBO.E_DEP);
					AssertEquals("Should populate E_ARV", expectedEstimatedArrival, cartageBO.E_ARV);
					AssertEquals("Should populate other transport leg information such as vessel name", "A P MOLLER", cartageBO.Vessel);
				});
			}
		}

		public void TestLocalTransport_TransportCompanyControllingBranch_SetAsDefaultBranch()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingTestAddressImportBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Cannot find 'ELITELON' so set Branch to Current Logged in Branch.", GlbBranch.CurrentBranch.PK, cartageBO.JJ_GB);
			var transportCo = Helper.CreateOrgHeader("ELITELON", "1 Blue Street"); // ELITELON is in 'TransportBooking - TestAddressImport.xml'
			Factory.Save();
			reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("'ELITELON' exists, but it doesn't have a branch set, so fallback to logged in branch.", GlbBranch.CurrentBranch.PK, cartageBO.JJ_GB);
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_Code = "XXX";
			transportCo.CompanyData.OB_GB_ControllingBranch = newBranch.PK;
			Factory.Save();
			reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("'ELITELON' with a Controlling Branch now exists, so use it to set onto the Cartage Job.", newBranch.PK, cartageBO.JJ_GB);
		}

		public void TestContainersWithContainerCount_EmptyContainerNumber()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingContainerisedWithContainerCountBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var containerWithContainerNumber = cartageDataObject.ContainerCollection.Single(c => !c.ContainerNumber.Value.IsEmpty);
			cartageDataObject.ContainerCollection.Remove(containerWithContainerNumber);
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Should have three Containers.", 3, cartageBO.Containers.Count());
			AssertEquals("Should not have copied Container Job IDs from anywhere.", 3, cartageBO.Containers.Count(c => c.JC_ContainerJobID.IsEmpty));
			AssertEquals("Should have three Container Moves.", 3, cartageBO.ContainerBookedMoves.Count);
			foreach (var container in cartageBO.Containers)
			{
				AssertEquals("Container Count should be 1, since the Container was split.", (short)1, container.JC_ContainerCount);
				AssertEquals("Container Tare Weight should have been split, original XML file had 11490.", 3830m, container.JC_TareWeight);
				var goodsWeight = container.JC_Calc_NetWeight;
				var containerMove = cartageBO.GetBookedMoves(container).Single();
				if (containerMove.EW_DisplayOrder == 1)
				{
					AssertEquals("Container Gross Weight should include the Goods Weight, only on the First Container from the Split.", 3870m, container.JC_GrossWeight);
					AssertEquals("Goods Weight should only be stored on the First Container from the Split.", 40m, container.JC_Calc_NetWeight);
				}
				else
				{
					AssertEquals("Container Gross Weight should not include the Goods Weight on the Split Containers.", 3830m, container.JC_GrossWeight);
					AssertEquals("Goods Weight should not be stored on the Split Containers.", 0m, container.JC_Calc_NetWeight);
				}

				AssertEquals("4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", containerMove.PickupFromDocAddress.AddressAsASingleLine);
				AssertEquals("CTO SYD ALEX ZANDER NSW 2015", containerMove.WaitPointDocAddress.AddressAsASingleLine);
				AssertNull(containerMove.DeliverToDocAddress);
				AssertEquals(2, containerMove.CartageLegs.Count);
				var legs = containerMove.CartageLegs.OrderBy(l => l.JU_DisplayOrder).ToArray();
				var leg1 = legs[0];
				AssertEquals("CYD 1 STUDIO STREET BOTANY NSW 2020", leg1.PickupFromDocAddress.AddressAsASingleLine);
				AssertEquals("4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", leg1.DeliverToDocAddress.AddressAsASingleLine);
				var leg2 = legs[1];
				AssertEquals("4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", leg2.PickupFromDocAddress.AddressAsASingleLine);
				AssertEquals("CTO SYD ALEX ZANDER NSW 2015", leg2.DeliverToDocAddress.AddressAsASingleLine);
			}
		}

		public void TestContainersWithContainerCount_NonEmptyContainerNumber()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingContainerisedWithContainerCountBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var containerWithoutContainerNumber = cartageDataObject.ContainerCollection.Single(c => c.ContainerNumber.Value.IsEmpty);
			cartageDataObject.ContainerCollection.Remove(containerWithoutContainerNumber);
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Should have only one Container.", 1, cartageBO.Containers.Count());
			AssertEquals("Should have only one Container Move.", 1, cartageBO.ContainerBookedMoves.Count);
			var container = cartageBO.Containers.Single();
			AssertEquals("CONT1234567", container.JC_ContainerNum);
			var containerMove = cartageBO.GetBookedMoves(container).Single();
			AssertEquals("4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", containerMove.PickupFromDocAddress.AddressAsASingleLine);
			AssertEquals("CTO SYD ALEX ZANDER NSW 2015", containerMove.WaitPointDocAddress.AddressAsASingleLine);
			AssertNull(containerMove.DeliverToDocAddress);
			AssertEquals(2, containerMove.CartageLegs.Count);
			var legs = containerMove.CartageLegs.OrderBy(l => l.JU_DisplayOrder).ToArray();
			var leg1 = legs[0];
			AssertEquals("CYD 1 STUDIO STREET BOTANY NSW 2020", leg1.PickupFromDocAddress.AddressAsASingleLine);
			AssertEquals("4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", leg1.DeliverToDocAddress.AddressAsASingleLine);
			var leg2 = legs[1];
			AssertEquals("4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", leg2.PickupFromDocAddress.AddressAsASingleLine);
			AssertEquals("CTO SYD ALEX ZANDER NSW 2015", leg2.DeliverToDocAddress.AddressAsASingleLine);
		}

		public void TestContainersWithContainerCount_EmptyContainerNumber_MixedFreight()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Mixed - With Container Count.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
			AssertEquals("Should have only one Container.", 1, cartageBO.Containers.Count());
			AssertEquals("Should have only one Container Move.", 1, cartageBO.ContainerBookedMoves.Count);
		}

		public void TestContainersWithSameNumber()
		{
			var now = ZDateTime.Now;
			var dayFromNow = now.AddDays(1);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 2);
			cartage.JJ_OrderReferenceNumber = "000";
			var container1 = cartage.Containers.ElementAt(0);
			container1.JC_ContainerNum = "CONT1";
			container1.JC_SealNum = "Seal1";
			container1.JC_ReleaseNum = "r1";
			var container2 = cartage.Containers.ElementAt(1);
			container2.JC_ContainerNum = "CONT1";
			container2.JC_SealNum = "Seal2";
			container2.JC_ReleaseNum = "r2";
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "Consignor st", "2000", "Sydney", "AUSYD", false);
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			Helper.SetupBookedMove(cartage.GetBookedMoves(container1).Single(), cnr, cto, now);
			Helper.SetupBookedMove(cartage.GetBookedMoves(container2).Single(), cnr, cto, now.AddMinutes(10));
			AssertEquals("Precondition: Container1 should have 1 leg.", 1, cartage.GetBookedMoves(container1).Single().CartageLegs.Count);
			AssertEquals("Precondition: Container2 should have 1 leg.", 1, cartage.GetBookedMoves(container2).Single().CartageLegs.Count);
			Helper.SetupLeg(cartage.GetBookedMoves(container1).Single().CartageLegs.Single(), cnr, null, cto, now);
			Helper.SetupLeg(cartage.GetBookedMoves(container2).Single().CartageLegs.Single(), cnr, null, cto, now.AddMinutes(10));
			Factory.Save();
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertNotEquals("Should import a NEW cartage.", cartage, cartageRead);
			AssertEquals("Even though both containers have the same number, both containers should still be imported.", 2, cartageRead.Containers.Count());
			var containerRead1 = cartageRead.Containers.Single(c => c.ContainerCode == "CONT1" && c.JC_SealNum == "Seal1" && c.JC_ReleaseNum == "r1");
			var containerRead2 = cartageRead.Containers.Single(c => c.ContainerCode == "CONT1" && c.JC_SealNum == "Seal2" && c.JC_ReleaseNum == "r2");
			AssertEquals(2, cartageRead.CartageLegs.Count);
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead1).Single().CartageLegs.Count);
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead2).Single().CartageLegs.Count);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching CommonCartage found, creating new CommonCartage.
Information - Populating CommonCartage...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - No matching CommonContainer found, creating new CommonContainer.
Information - Populating CommonContainer...
Information - Matching 'LocalCartageExporter':- Matched to 'CNRSYD' by code, address 'Consignor st' (only address).
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageExporter':- Matched to 'CNRSYD' by code, address 'Consignor st' (only address).
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Added Port Transport  from UniversalShipment.
			".Trim(), logger.Logs);
		}

		public void TestContainersWithSameNumberUpdate()
		{
			var now = ZDateTime.Now;
			var dayFromNow = now.AddDays(1);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLSHPtoCTO, 2);
			cartage.JJ_OrderReferenceNumber = "000";
			var container1 = cartage.Containers.ElementAt(0);
			container1.JC_ContainerNum = "CONT1";
			container1.JC_SealNum = "Seal1";
			container1.JC_ReleaseNum = "r1";
			var container2 = cartage.Containers.ElementAt(1);
			container2.JC_ContainerNum = "CONT1";
			container2.JC_SealNum = "Seal2";
			container2.JC_ReleaseNum = "r2";
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "Consignor st", "2000", "Sydney", "AUSYD", false);
			var cto = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			var move1 = cartage.GetBookedMoves(container1).Single();
			var move2 = cartage.GetBookedMoves(container2).Single();
			Helper.SetupBookedMove(move1, cnr, cto, now);
			Helper.SetupBookedMove(move2, cnr, cto, now.AddMinutes(10));
			AssertEquals("Precondition: Container1 should have 1 leg.", 1, move1.CartageLegs.Count);
			AssertEquals("Precondition: Container2 should have 1 leg.", 1, move2.CartageLegs.Count);
			Helper.SetupLeg(move1.CartageLegs.Single(), cnr, null, cto, now);
			Helper.SetupLeg(move2.CartageLegs.Single(), cnr, null, cto, now.AddMinutes(10));
			Factory.Save();
			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			cartageDataObject.DataContext.AddDataTarget(DataContextType.LocalTransport, "T00001000");
			cartageDataObject.ContainerCollection[0].Seal = "Seal3";
			cartageDataObject.ContainerCollection[1].Seal = "Seal4";
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageRead = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageRead);
			AssertEquals("Ensure the data object that was read in updates the existing Job. (it should override data)", cartage.PK, cartageRead.PK);
			var containerRead1 = cartageRead.Containers.Single(c => c.ContainerCode == "CONT1" && c.JC_SealNum == "Seal3" && c.JC_ReleaseNum == "r1");
			var containerRead2 = cartageRead.Containers.Single(c => c.ContainerCode == "CONT1" && c.JC_SealNum == "Seal4" && c.JC_ReleaseNum == "r2");
			AssertEquals(2, cartageRead.CartageLegs.Count);
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead1).Single().CartageLegs.Count);
			AssertEquals(1, cartageRead.GetBookedMoves(containerRead2).Single().CartageLegs.Count);
			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching CommonCartage.
Information - Populating CommonCartage...
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Successfully loaded matching CommonContainer.
Information - Populating CommonContainer...
Information - Matching 'LocalCartageExporter':- Matched to 'CNRSYD' by code, address 'Consignor st' (only address).
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Matching 'LocalCartageExporter':- Matched to 'CNRSYD' by code, address 'Consignor st' (only address).
Information - Matching 'LocalCartageCTO':- Matched to 'CTOSYD' by code, address 'Wharf st' (only address).
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Information - Updated Port Transport T00001000 from UniversalShipment.
			".Trim(), logger.Logs);
		}

		public void TestPopulateContainerLegsUsingInstructionsOnly_WhenDataObjectInstructionCollectionIsNull()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var sourceDO = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			sourceDO.SetInstructionCollection(() =>
			{
				return null;
			});
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingContainerisedWithContainerCountBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory(), sourceDO);
			var cartageBO = reader.ReadIntoBusinessObject();
			AssertNotNull(cartageBO);
		}

		public void TestScheduleDeliveryUXML2011_WithLegs()
		{
			AssertScheduleDeliveryUXML2011("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - With Legs Delivery UXML2011.xml");
		}

		public void TestScheduleDeliveryUXML2011_WithoutLegs()
		{
			AssertScheduleDeliveryUXML2011("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Without Legs Delivery UXML2011.xml");
		}

		public void TestSchedulePickupUXML2011()
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - With Legs Pickup UXML2011.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			CombineAssertions("For pickup, should get schedule details from leg sequence 2 (first non-local transport leg by sequence)", () =>
			{
				AssertNotNull(cartageBO);
				AssertEquals("AUSYD", cartageBO.PortOfLoading);
				AssertEquals("AUMEL", cartageBO.PortOfDischarge);
				AssertEquals(new ZDateTime(2016, 9, 1, 8, 43, 0, 0), cartageBO.E_DEP);
				AssertEquals(new ZDateTime(2016, 9, 2, 8, 43, 0, 0), cartageBO.E_ARV);
				AssertEquals("AAL BRISBANE", cartageBO.Vessel);
				AssertEquals("456", cartageBO.VoyageFlight);
			});
		}

		public void TestScheduleDeliveryUXML2012_LegsOnPreCarriageShipmentParent()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - With Legs Delivery UXML2012 TB from Shipment.xml");
				using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
				}

				var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
				var cartageBO = reader.ReadIntoBusinessObject();
				CombineAssertions("For delivery, should get schedule details from leg sequence 2 (last non-local transport leg by sequence)", () =>
				{
					AssertNotNull(cartageBO);
					AssertEquals("NZAKL", cartageBO.PortOfLoading);
					AssertEquals("AUSYD", cartageBO.PortOfDischarge);
					AssertEquals(new ZDateTime(2016, 9, 3, 8, 43, 0, 0), cartageBO.E_DEP);
					AssertEquals(new ZDateTime(2016, 9, 8, 8, 43, 0, 0), cartageBO.E_ARV);
					AssertEquals("AAL BRISBANE 2", cartageBO.Vessel);
					AssertEquals("789", cartageBO.VoyageFlight);
				});
			}
		}

		public void TestScheduleDeliveryUXML2012_LegsOnParent()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - With Legs Delivery UXML2012 TB from CustomsDec.xml");
				using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
				}

				var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
				var cartageBO = reader.ReadIntoBusinessObject();
				CombineAssertions("For delivery, should get schedule details from leg sequence 2 (last non-local transport leg by sequence)", () =>
				{
					AssertNotNull(cartageBO);
					AssertEquals("NZAKL", cartageBO.PortOfLoading);
					AssertEquals("AUSYD", cartageBO.PortOfDischarge);
					AssertEquals(new ZDateTime(2016, 9, 3, 8, 43, 0, 0), cartageBO.E_DEP);
					AssertEquals(new ZDateTime(2016, 9, 8, 8, 43, 0, 0), cartageBO.E_ARV);
					AssertEquals("AAL BRISBANE 2", cartageBO.Vessel);
					AssertEquals("789", cartageBO.VoyageFlight);
				});
			}
		}

		public void TestSchedulePickupUXML2012_LegsOnPreCarriageShipmentParent()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - With Legs Pickup UXML2012 TB from Shipment.xml");
				using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
				}

				var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
				var cartageBO = reader.ReadIntoBusinessObject();
				CombineAssertions("For delivery, should get schedule details from leg sequence 2 (last non-local transport leg by sequence)", () =>
				{
					AssertNotNull(cartageBO);
					AssertEquals("AUSYD", cartageBO.PortOfLoading);
					AssertEquals("AUMEL", cartageBO.PortOfDischarge);
					AssertEquals(new ZDateTime(2016, 9, 1, 8, 43, 0, 0), cartageBO.E_DEP);
					AssertEquals(new ZDateTime(2016, 9, 2, 8, 43, 0, 0), cartageBO.E_ARV);
					AssertEquals("AAL BRISBANE", cartageBO.Vessel);
					AssertEquals("456", cartageBO.VoyageFlight);
				});
			}
		}

		public void TestSchedulePickupUXML2012_LegsOnParent()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - With Legs Pickup UXML2012 TB from CustomsDec.xml");
				using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
				{
					ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
				}

				var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
				var cartageBO = reader.ReadIntoBusinessObject();
				CombineAssertions("For delivery, should get schedule details from leg sequence 2 (last non-local transport leg by sequence)", () =>
				{
					AssertNotNull(cartageBO);
					AssertEquals("AUSYD", cartageBO.PortOfLoading);
					AssertEquals("AUMEL", cartageBO.PortOfDischarge);
					AssertEquals(new ZDateTime(2016, 9, 1, 8, 43, 0, 0), cartageBO.E_DEP);
					AssertEquals(new ZDateTime(2016, 9, 2, 8, 43, 0, 0), cartageBO.E_ARV);
					AssertEquals("AAL BRISBANE", cartageBO.Vessel);
					AssertEquals("456", cartageBO.VoyageFlight);
				});
			}
		}

		public void TestPopulateAddress_NonOverrideJobDocAddressExcludingContactMatches()
		{
			const string ContactName = "John Smith";
			const string AdditionalInfo = "some info";
			var now = ZDateTime.Now;
			var dayFromNow = now.AddDays(1);

			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, 3);
			var job = new JobHeader.Loader(cartage).TryLoadOrCreate();
			var client = Helper.CreateOrgHeader("AAABBB", "1 Blue Street");
			job.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			cartage.JJ_OrderReferenceNumber = "order1";
			cartage.JJ_QuoteNumber = "quote1";
			cartage.JJ_WaybillNumber = "waybill1";
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			cartage.JJ_EstimatedPickup = now.AddHours(1);
			cartage.JJ_EstimatedDelivery = now.AddHours(2);
			cartage.JJ_A_JCL = now.AddHours(3);

			var cto = Helper.CreateOrUpdateJobDocAddressOnCartage(cartage, DocAddressType.LocalCartageCTO, "LCT", "CTO", "Wharf st", "2000", "Sydney", "AUSYD", false);
			var cne = Helper.CreateOrUpdateJobDocAddressOnCartage(cartage, DocAddressType.LocalCartageImporter, "LCI", "CNE", "Consignee st", "2000", "Sydney", "AUSYD", false);

			var cneContact = cne.Organisation.Contacts.AddNew();
			cneContact.OC_ContactName = ContactName;
			cneContact.OC_OA_OrgAddress = cne.Address.PK;
			cneContact.FillWithValidTestData();
			cne.Address.OA_AdditionalAddressInformation = AdditionalInfo;

			var container1 = Helper.SetupBookedMove(cartage.GetBookedMoves(cartage.Containers.ElementAt(0))[0], cto, cne, now);
			container1.Container.JC_ContainerNum = "CONT1";
			container1.EW_DropMode = "";
			container1.CartageLegs.DeleteAll();

			var containerLeg = Helper.SetupLeg(container1.CartageLegs.AddNew(), cto, null, cne, now.AddMinutes(40));

			Factory.Save();
			AssertEquals("Precondition: Original cartage should only have one LCI (Consignee) DocAddress", 1, cartage.DocAddresses.Where(a => a.E2_AddressType == "LCI").Count());
			var cartageLCIAddress = cartage.DocAddresses.Where(a => a.E2_AddressType == "LCI").Single();
			AssertEquals("Precondition: Original cartage LCI (Consignee) DocAddress E2_Contact should be blank", string.Empty, cartageLCIAddress.E2_Contact);

			var writer = new LocalTransportDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage)));
			var cartageDataObject = writer.GetDataObject(cartage);
			cartageDataObject.DataContext.AddDataTarget(DataContextType.LocalTransport, cartage.JJ_ConsignmentID);
			var cneInstructionAddressDataObject = cartageDataObject.InstructionCollection.FirstOrDefault(i => i.Address.AddressType == (ZString?)"LocalCartageImporter").Address;
			cneInstructionAddressDataObject.Contact = ContactName;
			cneInstructionAddressDataObject.AdditionalAddressInformation = AdditionalInfo;
			AssertEquals("Precondition: original cartage LCI address's E2_AdditionalAddressInformation should match the cneInstructionAddressDataObject's AdditionalAddressInformation", cneInstructionAddressDataObject.AdditionalAddressInformation, cartageLCIAddress.E2_AdditionalAddressInformation);
			AssertNotEquals("Precondition: original cartage LCI address's E2_Contact should not match the cneInstructionAddressDataObject's Contact", cneInstructionAddressDataObject.Contact, cartageLCIAddress.E2_Contact);

			var cartageReader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var updatedCartage = cartageReader.ReadIntoBusinessObject();
			AssertEquals("Updated cartage (from reader after reading data object) should still only have one LCI (Consignee) DocAddress as it should have been matched in OrganisationDataObjectReader.IsJobDocAddressMatchingOrgAddress() despite not matching on E2_Contact and E2_AdditionalAddressInformation", 1, updatedCartage.DocAddresses.Where(a => a.E2_AddressType == "LCI").Count());
		}

		public void TestJobIsCreatedWithMutexWhenCallPopulateJobDetails()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_ConsignmentID = "T00001133";
			Factory.Save();

			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingJobHasMutexWhenImportCommonCartageBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var readerFactory = new UniversalObjectFactory();
			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, readerFactory);
			var cartageBO = reader.ReadIntoBusinessObject();
			using (var job = new JobHeader.Loader(cartageBO).Load())
			{
				AssertNotNull(job);
				Assert(JobHeader.GetMutexForTest(job).HasLock);
			}
		}

		public void TestPopulateJobDetailsWhenJobMutexIsLocked_NoExceptionIsThrown()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_ConsignmentID = "T00001133";
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(TransportBookingJobHasMutexWhenImportCommonCartageWithAddressAsNeededBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}
			string xmlCartageKey = cartageDataObject.DataContext.DataTargetCollection.FirstOrDefault().Key;
			AssertEquals("Precondition: cartageDataObjectID should match the ID of cartage in xml", cartage.JJ_ConsignmentID, xmlCartageKey);
			foreach (var address in cartageDataObject.OrganizationAddressCollection.ToArray())
			{
				if (address.AddressType.ToString() == "SendersLocalClient")
				{
					cartageDataObject.OrganizationAddressCollection.Remove(address);
				}
			}
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, cartage));
			var addressDataObject = cartageDataObject.AddOrgAddress(writeManager, orgAddress, AddressTypes.SendersLocalClient);

			Factory.Save();

			using (var job = new JobHeader.Loader(cartage).TryCreateWithMutex())
			{
				var readerFactory = new UniversalObjectFactory();
				var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, readerFactory);
				Assert("Precondition: Mutex should be locked", JobHeader.GetMutexForTest(job).HasLock);
				AssertNoExceptionThrown("Object reference null error is thrown if lock is on", () =>
				{
					var cartageBO = reader.ReadIntoBusinessObject();
				});
			}
		}

		public void TestTryPopulateSchedule_PopulatesLCLAvailabilityDateFromLocalProcessingOnSubShipmentIfPresent()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var overridenLCLAvailabilityDate = new ZDateTime(2024, 8, 15, 9, 30, 0, 0);

			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Shipment With Enough Information To Set LCLAvailability.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(dataObject, stream, logger);
			}
			dataObject.SubShipmentCollection.First().LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance) { LCLAvailable = overridenLCLAvailabilityDate };
			dataObject.SubShipmentCollection.Last().LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance) { LCLAvailable = new ZDateTime(2025, 8, 15, 9, 30, 0, 0) };

			var reader = new LocalTransportDataObjectReader(dataObject, logger, new UniversalObjectFactory());

			var cartageRead = reader.ReadIntoBusinessObject();

			AssertNotEquals("The overriden LCLAvaibilityDate should not be the same as dataObject.TransportLegCollection.Single().LCLAvailability", overridenLCLAvailabilityDate, dataObject.TransportLegCollection.Single().LCLAvailability);
			AssertEquals("LCLAvailabilityDate in cartage business object should be the same as the overriden LCLAvaibilityDate", overridenLCLAvailabilityDate, cartageRead.LCLAvailabilityDate);
		}

		public void TestTryPopulateSchedule_PopulatesLCLStorageDateFromLocalProcessingOnSubShipmentIfPresent()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var overridenLCLStorageDate = new ZDateTime(2024, 8, 22, 9, 30, 0, 0);

			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Shipment With Enough Information To Set LCLStorageDate.xml");
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(dataObject, stream, logger);
			}
			dataObject.SubShipmentCollection.First().LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance) { LCLStorageCommences = overridenLCLStorageDate };
			dataObject.SubShipmentCollection.Last().LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance) { LCLStorageCommences = new ZDateTime(2025, 8, 15, 9, 30, 0, 0) };

			var reader = new LocalTransportDataObjectReader(dataObject, logger, new UniversalObjectFactory());

			var cartageRead = reader.ReadIntoBusinessObject();

			AssertNotEquals("The overriden LCLStorageCommences should not be the same as dataObject.TransportLegCollection.Single().LCLStorageDate", overridenLCLStorageDate, dataObject.TransportLegCollection.Single().LCLStorageDate);
			AssertEquals("LCLStorageDate in cartage business object should be the same as the overriden LCLStorageDate", overridenLCLStorageDate, cartageRead.LCLStorageDate);
		}
		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
			BehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
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

		TestErrorLogger logger;

		static Note SetupNote()
		{
			var noteDataObject = new Note();
			noteDataObject.Description = "DOG FLOGGER!!";
			noteDataObject.Visibility = new CodeDescriptionPair()
			{ Code = nameof(StmNoteVisibility.PUB), Description = "Public" };
			noteDataObject.NoteContext = new NoteContext()
			{ Code = "BEB", Description = "Baby Eats Banana" };
			return noteDataObject;
		}

		static void AssertNoteContents(StmNote noteBO)
		{
			AssertEquals("noteBO.ST_Description", "DOG FLOGGER!!", noteBO.ST_Description);
			AssertEquals("noteBO.ST_NoteContext", "BEB", noteBO.ST_NoteContext);
			AssertEquals("noteBO.ST_NoteType", "PUB", noteBO.ST_NoteType);
		}

		CommonContainer AssertContainer(CommonCartage cartage, ZString containerNumber)
		{
			var container = cartage.Containers.FirstOrDefault(c => c.JC_ContainerNum.Equals(containerNumber));
			AssertNotNull(container);
			AssertEquals(1, cartage.GetBookedMoves(container).Length);
			return container;
		}

		CommonBookedCtgMove AssertPackage(CommonCartage cartage, CommonBookedCtgMove compareTo)
		{
			var package = cartage.LooseBookedMoves.FirstOrDefault(m => m.EW_BookedPackCount.Equals(compareTo.EW_BookedPackCount));
			AssertNotNull(package);
			AssertEquals(compareTo.EW_BookedPackCount, package.EW_BookedPackCount);
			AssertEquals(compareTo.EW_F3_NKPackType, package.EW_F3_NKPackType);
			AssertEquals(compareTo.EW_BookedWeight, package.EW_BookedWeight);
			AssertEquals(compareTo.EW_WeightUQ, package.EW_WeightUQ);
			AssertEquals(compareTo.EW_BookedVolume, package.EW_BookedVolume);
			AssertEquals(compareTo.EW_VolumeUQ, package.EW_VolumeUQ);
			return package;
		}

		void AssertLeg(CommonCartageLeg legRead, CommonCartageLeg compareTo)
		{
			AssertAddress(legRead.PickupFromDocAddress, compareTo.PickupFromDocAddress);
			AssertAddress(legRead.WaitPointDocAddress, compareTo.WaitPointDocAddress);
			AssertAddress(legRead.DeliverToDocAddress, compareTo.DeliverToDocAddress);
			AssertEquals(legRead.JU_PlannedPickupTime, compareTo.JU_PlannedPickupTime);
			AssertEquals(legRead.JU_PlannedPickupTimeEnd, compareTo.JU_PlannedPickupTimeEnd);
			AssertEquals(legRead.JU_EstimatedDeliveryTime, compareTo.JU_EstimatedDeliveryTime);
			AssertEquals(legRead.JU_EstimatedDeliveryTimeEnd, compareTo.JU_EstimatedDeliveryTimeEnd);
			AssertEquals(legRead.JU_PickupTimeIn, compareTo.JU_PickupTimeIn);
			AssertEquals(legRead.JU_PickupTimeOut, compareTo.JU_PickupTimeOut);
			AssertEquals(legRead.JU_DeliverTimeIn, compareTo.JU_DeliverTimeIn);
			AssertEquals(legRead.JU_DeliverTimeOut, compareTo.JU_DeliverTimeOut);
			AssertEquals(legRead.JU_LegNotes, compareTo.JU_LegNotes);
		}

		void AssertAddress(JobDocAddress addressRead, JobDocAddress compareTo)
		{
			if (addressRead == null && compareTo == null || (addressRead != null && compareTo != null && addressRead.IsTheSameAddressAs(compareTo)))
			{
				Assert(true);
			}
			else
			{
				Assert("Addresses don't match", false);
			}
		}

		void AssertAdditionalReference(CusEntryNumber additionalReference, ZString type, ZString number)
		{
			AssertEquals(CusEntryNumber.Categories.AdditionalReferenceNumber, additionalReference.CE_Category);
			AssertEquals(type, additionalReference.CE_EntryType);
			AssertEquals(number, additionalReference.CE_EntryNum);
		}

		void AssertOnlyOneReference(CusEntryNumAdditionalReferenceCollection refs, ZString type, ZString number, string overrideAssertMessage = null)
		{
			var relevantRefs = refs.Cast<CusEntryNumber>().Where(ce => ce.CE_EntryType == type && ce.CE_EntryNum == number).ToArray();
			if (string.IsNullOrEmpty(overrideAssertMessage))
			{
				AssertEquals(relevantRefs.Length, 1);
			}
			else
			{
				AssertEquals(overrideAssertMessage, relevantRefs.Length, 1);
			}
		}

		void AssertScheduleDeliveryUXML2011(string testFile)
		{
			var cartageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var resourceBytes = resourceRetriever.Value.GetBytes(testFile);
			using (var stream = (SubStreamableStream)new MemoryStream(resourceBytes))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(cartageDataObject, stream, logger);
			}

			var reader = new LocalTransportDataObjectReader(cartageDataObject, logger, new UniversalObjectFactory());
			var cartageBO = reader.ReadIntoBusinessObject();
			CombineAssertions("For delivery, should get schedule details from leg sequence 2 (last non-local transport leg by sequence)", () =>
			{
				AssertNotNull(cartageBO);
				AssertEquals("NZAKL", cartageBO.PortOfLoading);
				AssertEquals("AUSYD", cartageBO.PortOfDischarge);
				AssertEquals(new ZDateTime(2016, 9, 3, 8, 43, 0, 0), cartageBO.E_DEP);
				AssertEquals(new ZDateTime(2016, 9, 8, 8, 43, 0, 0), cartageBO.E_ARV);
				AssertEquals("AAL BRISBANE 2", cartageBO.Vessel);
				AssertEquals("789", cartageBO.VoyageFlight);
			});
		}

		byte[] TransportBookingReferencesBytes => resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - References.xml");

		byte[] TransportBookingTestAddressImportBytes => resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - TestAddressImport.xml");

		byte[] TransportBookingContainerisedWithContainerCountBytes => resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Containerised - With Container Count.xml");

		byte[] TransportBookingJobHasMutexWhenImportCommonCartageBytes => resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - JobHasMutexWhenImportCommonCartage.xml");

		byte[] TransportBookingJobHasMutexWhenImportCommonCartageWithAddressAsNeededBytes => resourceRetriever.Value.GetBytes("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - JobHasMutexWhenImportCommonCartageWithAddressAsNeeded.xml");
	}
}
