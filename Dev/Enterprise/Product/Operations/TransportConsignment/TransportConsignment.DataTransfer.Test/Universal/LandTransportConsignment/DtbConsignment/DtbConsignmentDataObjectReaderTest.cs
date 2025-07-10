using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using DataContext2011 = Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
using DataSource2011 = Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataSource;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentDataObjectReaderTest : OrganizationAddressTestHelper
	{
		#region TestAdditionalReferences
		public void TestAdditionalReferences()
		{
			var year = ZDateTime.Now.Year;
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var additionalReferenceDataObject = new AdditionalReference();
			additionalReferenceDataObject.IssueDate = new ZDateTime(year, 1, 1);
			additionalReferenceDataObject.ReferenceNumber = "R1234";
			additionalReferenceDataObject.Type = new EntryType { Code = "TRF", Description = "Transport Reference Number" };
			consignmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			consignmentDataObject.AdditionalReferenceCollection.Add(additionalReferenceDataObject);
			var consignment = Helper.CreateConsignment("LT001");
			var reader = GetNewReader(consignmentDataObject, Logger, Factory, consignment, topLevelDataObject);
			var newConsignment = reader.ReadIntoBusinessObject();
			AssertEquals(1, newConsignment.AdditionalReferenceNumbers.Count);
			var additionalReference = newConsignment.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference.CE_EntryNum", "R1234", additionalReference.CE_EntryNum);
			AssertEquals("additionalReference.CE_EntryType", "TRF", additionalReference.CE_EntryType);
			AssertEquals("additionalReference.CE_IssueDate", new ZDateTime(year, 1, 1), additionalReference.CE_IssueDate);
		}

		#endregion
		#region TestPopulateNotes
		public void TestPopulateNotes()
		{
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingShipment.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note
				{
					Description = "Dangerous Goods Additional Handling Information",
					NoteText = "234",
					Visibility = new CodeDescriptionPair()
					{
						Code = nameof(CargoWise.Definitions.StmNoteVisibility.PUB),
						Description = "Public"
					},
					NoteContext = new NoteContext()
					{
						Code = "123",
						Description = "123"
					},
					IsCustomDescription = false
				}
			});
			var reader = GetNewReader(bookingShipment, Logger, Factory, null, bookingShipment);
			var consignmentBO = reader.ReadIntoBusinessObject();
			var notes = consignmentBO.Notes.GetAllNotes();
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

		#endregion
		#region TestPopulateNotes_UnmatchedOrgs
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
					Visibility = new CodeDescriptionPair()
					{
						Code = nameof(CargoWise.Definitions.StmNoteVisibility.INT),
						Description = "INTERNAL"
					},
					NoteContext = new NoteContext()
					{
						Code = "AAA",
						Description = "AAA"
					},
					IsCustomDescription = false
				}
			});
			var reader = GetNewReader(bookingShipment, Logger, Factory, null, bookingShipment);
			var consignmenttBO = reader.ReadIntoBusinessObject();
			var notes = consignmenttBO.Notes.GetAllNotes();
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

		#endregion
		#region TestDataContextType
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.LandTransportConsignment, new DtbConsignmentDataObjectReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory, null, new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)).DataContextType);
		}

		#endregion
		#region TestConstructor
		public void TestConstructor()
		{
			var consignment = Helper.CreateConsignment("LT001");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var depotAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi);
			depotAddress.Address.DocAddressType = DocAddressType.LocalCartageCFS;
			var depotDeliveryAction = Helper.CreateConsignmentAction(depotAddress, ActionTypes.Codes.Delivery);
			Factory.SaveForTesting();
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertExceptionThrown(typeof(ArgumentException), "We only want to copy the Pick Up Info to new Consignments.", () => new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consolidationDataObject, null, consignment, consignment.PickupAddress.Row()));
			var pickupDepotAddress = consignment.Addresses.OrderBy(i => i.LTS_Sequence).FirstOrDefault(i => i.IsDepot && i.LTS_InstructionType.EqualsIgnoringCase(ConsignmentAddressTypes.Codes.Multi));
			var deliverToDepotAction = pickupDepotAddress.DeliveryAction;
			AssertExceptionThrown(typeof(ArgumentException), "We only want to copy the Pick Up Info to new Consignments.", () => new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consolidationDataObject, null, consignment, null, deliverToDepotAction.Row()));
		}

		#endregion
		#region TestBasicLevelFieldMappings
		public void TestBasicLevelFieldMappings()
		{
			var consignment = Helper.CreateConsignment("LT001");
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentDataObject.ServiceLevel = new ServiceLevel { Code = "D2D" };
			consignmentDataObject.ConsignmentNote = "CN0001";
			consignmentDataObject.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "LTL" };
			consignmentDataObject.ShipmentIncoTerm = new IncoTerm() { Code = "INC" };
			consignmentDataObject.AdditionalTerms = "Additional terms";
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consolidationDataObject, null, consignment);
			var consignmentBO = reader.ReadIntoBusinessObject();
			AssertEquals("consignment.LTC_RS_NKServiceLevel", "D2D", consignmentBO.LTC_RS_NKServiceLevel);
			AssertEquals("consignment.LTC_Status", "BKD", consignmentBO.LTC_Status);
			AssertEquals("consignment.LTC_JobID", "LT001", consignmentBO.LTC_JobID);
			AssertEquals("consignment.LTC_ConnoteNumber", "CN0001", consignmentBO.LTC_ConnoteNumber);
			AssertEquals("consignment.LTC_JobType", "LTL", consignmentBO.LTC_JobType);
			AssertEquals("consignment.LTC_Incoterm", "INC", consignmentBO.LTC_Incoterm);
			AssertEquals("consignment.LTC_AdditionalTerms", "Additional terms", consignmentBO.LTC_AdditionalTerms);
		}

		#endregion
		#region TestConNoteNoMapping
		public void TestConNoteNoMapping()
		{
			// create UXML for the consignment
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var addressPIC = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var addressDLV = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			CreateAndAssignPackage(addressPIC, consignmentDataObject, addressDLV);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { addressPIC, addressDLV });
			var conNoteAction = UniversalTestHelper.AddInstructionConfirmation(addressDLV, ActionTypes.Codes.ConNoteNo);
			conNoteAction.Reference = "123";
			consignmentDataObject.LocalProcessing = new LocalProcessing { ArrivalCartageRef = "abc" };
			AssertTransportReference(consignmentDataObject, addressDLV, "123");
			conNoteAction.Reference = null;
			AssertTransportReference(consignmentDataObject, addressDLV, "abc");
			conNoteAction.Reference = "123";
			UniversalTestHelper.AddInstructionConfirmation(addressDLV, ConfirmationTypes.Codes.ConNoteNo);
			AssertTransportReference(consignmentDataObject, addressDLV, "abc");
			UniversalTestHelper.AddDivotConfirmation(addressDLV, ConfirmationTypes.Codes.ConNoteNo).Reference = "123";
			UniversalTestHelper.AddDivotConfirmation(addressDLV, ConfirmationTypes.Codes.ConNoteNo);
			AssertTransportReference(consignmentDataObject, addressDLV, "abc");
			consignmentDataObject.LocalProcessing = null;
			AssertTransportReference(consignmentDataObject, addressDLV, "", saveFactory: true);
		}

		void AssertTransportReference(UniversalShipment consignmentDataObject, Instruction deliveryInstruction, ZString expectedConNote, bool saveFactory = false)
		{
			var reader = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consignmentDataObject, deliveryInstruction, null);
			var importedConsignment = reader.ReadIntoBusinessObject();
			if (saveFactory)
			{
				Factory.SaveForTesting();
			}

			AssertEquals("consignment.LTC_ConnoteNumber", expectedConNote, importedConsignment.LTC_ConnoteNumber);
			AssertEquals(0, importedConsignment.Addresses.SelectMany(a => a.Actions).Count(c => c.LTA_ActionType == ActionTypes.Codes.ConNoteNo));
		}

		#endregion
		#region TestIsHazardousAndRequiresRefrigeration
		public void TestIsHazardousAndRequiresRefrigeration()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentDataObject.IsHazardous = true;
			consignmentDataObject.RequiresRefrigeration = true;
			int packageLink = 1;
			var package = UniversalTestHelper.CreatePackageDataObject("XYZ", "PLT", packageLink);
			topLevelDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment1 = reader1.ReadIntoBusinessObject();
			AssertEquals("No package with hazardous material.", false, consignment1.LTC_IsHazardous);
			AssertEquals("No package with refrigeration required.", false, consignment1.LTC_RequiresRefrigeration);
			package.SetUNDGCollection(() => new List<UNDG> { new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { UNDGCode = "1004A" } });
			var reader2 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Has 1 package with hazadous material.", true, consignment2.LTC_IsHazardous);
			AssertEquals("No package with refrigeration required.", false, consignment2.LTC_RequiresRefrigeration);
			package.RequiresTemperatureControl = true;
			var reader3 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment3 = reader3.ReadIntoBusinessObject();
			AssertEquals("Has 1 package with hazadous material.", true, consignment3.LTC_IsHazardous);
			AssertEquals("Has 1 package with refrigeration required.", true, consignment3.LTC_RequiresRefrigeration);
			topLevelDataObject.SetPackingLineCollection(() => null);
			pickupInstruction.SetInstructionPackingLineLinkCollection(() => null);
			int containerLink = 2;
			var container = UniversalTestHelper.CreateContainerDataObject("CONT123", containerLink);
			container.IsControlledAtmosphere = true;
			topLevelDataObject.SetContainerCollection(() => new DataObjectList<Container> { container });
			UniversalTestHelper.AddContainerLink(pickupInstruction, containerLink);
			var reader4 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment4 = reader4.ReadIntoBusinessObject();
			AssertEquals("Has 1 container with refrigeration required.", true, consignment4.LTC_RequiresRefrigeration);
		}

		#endregion
		public void TestPopulateGoodsAndInsuranceValueAndCurrencyFromParentIfValueIsNullOnSource_Delivery_2012()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var bookingConsolidationShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				bookingConsolidationShipment.TransportBookingDirection = new TransportBookingDirection() { Code = "DLV", Description = "Delivery" };
				bookingShipment.SetParentShipmentCollection(() => new List<UniversalShipment>() { bookingConsolidationShipment });
				var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				SetGoodsAndInsuranceValues(parentShipment, TestGoodsValue, TestGoodsCurrency, TestGoodsCurrencyDescription, TestInsuranceValue, TestInsuranceCurrency, TestInsuranceCurrencyDescription);
				bookingConsolidationShipment.SetPreCarriageShipmentCollection(() => new List<UniversalShipment>() { parentShipment });
				AssertGoodsAndInsuranceValueAndCurrencyOnCreatedConsignment(bookingShipment, bookingShipment, TestGoodsValue, TestGoodsCurrency, TestInsuranceValue, TestInsuranceCurrency, "parent");
			}
		}

		public void TestPopulateGoodsAndInsuranceValueAndCurrencyFromParentIfValueIsNullOnSource_Pickup_2012()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var bookingConsolidationShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				bookingConsolidationShipment.TransportBookingDirection = new TransportBookingDirection() { Code = "PIC", Description = "Pickup" };
				bookingShipment.SetParentShipmentCollection(() => new List<UniversalShipment>() { bookingConsolidationShipment });
				var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				SetGoodsAndInsuranceValues(parentShipment, TestGoodsValue, TestGoodsCurrency, TestGoodsCurrencyDescription, TestInsuranceValue, TestInsuranceCurrency, TestInsuranceCurrencyDescription);
				bookingConsolidationShipment.SetPostCarriageShipmentCollection(() => new List<UniversalShipment>() { parentShipment });
				AssertGoodsAndInsuranceValueAndCurrencyOnCreatedConsignment(bookingShipment, bookingShipment, TestGoodsValue, TestGoodsCurrency, TestInsuranceValue, TestInsuranceCurrency, "parent");
			}
		}

		public void TestPopulateGoodsAndInsuranceValueAndCurrencyFromParentIfValueIsNullOnSource_2011()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var bookingShipmentDataContext = new DataContext2011();
				bookingShipmentDataContext.DataSourceCollection = new List<DataSource2011>() { new DataSource2011() { Key = "TB1", Type = "TransportBooking" } };
				bookingShipment.DataContext = bookingShipmentDataContext;
				var topLevelShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var topLevelShipmentDataContext = new DataContext2011();
				topLevelShipmentDataContext.DataSourceCollection = new List<DataSource2011>() { new DataSource2011() { Key = "CM1", Type = "TransportBookingConsolidation" }, new DataSource2011() { Key = "TB1", Type = "TransportBooking" } };
				topLevelShipment.DataContext = topLevelShipmentDataContext;
				var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				SetGoodsAndInsuranceValues(parentShipment, TestGoodsValue, TestGoodsCurrency, TestGoodsCurrencyDescription, TestInsuranceValue, TestInsuranceCurrency, TestInsuranceCurrencyDescription);
				var parentShipmentDataContext = new DataContext2011();
				parentShipmentDataContext.DataSourceCollection = new List<DataSource2011>() { new DataSource2011() { Key = "S01", Type = "ForwardingShipment" } };
				parentShipment.DataContext = parentShipmentDataContext;
				topLevelShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { parentShipment, bookingShipment });
				AssertGoodsAndInsuranceValueAndCurrencyOnCreatedConsignment(bookingShipment, topLevelShipment, TestGoodsValue, TestGoodsCurrency, TestInsuranceValue, TestInsuranceCurrency, "parent");
			}
		}

		public void TestPopulateGoodsAndInsuranceValueAndCurrencyIfNotNullOnSource()
		{
			var bookingShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetGoodsAndInsuranceValues(bookingShipment, TestGoodsValue, TestGoodsCurrency, TestGoodsCurrencyDescription, TestInsuranceValue, TestInsuranceCurrency, TestInsuranceCurrencyDescription);
			AssertGoodsAndInsuranceValueAndCurrencyOnCreatedConsignment(bookingShipment, bookingShipment, TestGoodsValue, TestGoodsCurrency, TestInsuranceValue, TestInsuranceCurrency, "booking");
		}

		#region TestGetExistingBusinessObject
		public void TestGetExistingBusinessObject()
		{
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignment = Helper.CreateConsignment("LT001");
			var readerWithConsignment = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, null, consignment);
			var readerWithoutConsignment = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			AssertEquals(consignment, ((ITopLevelDataObjectReader)readerWithConsignment).GetExistingBusinessObject());
			AssertNull(((ITopLevelDataObjectReader)readerWithoutConsignment).GetExistingBusinessObject());
		}

		#endregion
		#region Related Entities
		#region TestPopulateAddresses
		public void TestPopulateAddresses()
		{
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataSource(DataContextType.TransportBookingConsolidation, "C");
			consolidationDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, "123");
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "New Local Client Co";
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			consolidationDataObject.AddOrgAddress(writeManager, org.MainAddress, DocAddressType.BookingPartyDocumentaryAddress);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentDataObject.DataContext = DataContextFactory.New();
			consignmentDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, "123");
			var pickUpInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			CreateAndAssignPackage(pickUpInstruction, consolidationDataObject);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstruction });
			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { consignmentDataObject });
			var reader = new LTConsignmentConsolidationDataObjectReader(consolidationDataObject, Logger, Factory);
			var consolidation = reader.ReadIntoBusinessObject();
			AssertEquals(1, consolidation.ConsignmentsCreatedDuringImport_ForTesting.Count);
			var consignment = consolidation.ConsignmentsCreatedDuringImport_ForTesting[0];
			AssertEquals("New Local Client Co", consignment.DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress).E2_CompanyName);
		}

		#endregion
		#region TestAddresses
		public void TestAddresses()
		{
			var clientRequestedBillingPartData = GetNewAddressData_CRAHOLSYD(DocAddressType.ClientRequestedBillingParty);
			new OrganisationDataObjectReader(clientRequestedBillingPartData, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { clientRequestedBillingPartData });
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consolidationDataObject, instruction: null, consignment: null);
			var consignment = reader.ReadIntoBusinessObject();
			AssertJobDocAddressContentMatches_CRAHOLSYDWithoutGovRegNumAndType(consignment.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty));
		}

		#endregion
		#region TestCharges

		public void TestJobCosting_ShouldNotImportJobCosting()
		{
			var mockedAdapter = new Mock<IJobCostingAdapter>();

			using (ObjectFactory.Substitute(mockedAdapter.Object))
			{
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				consignmentDataObject.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance)
				{ Branch = new Branch { Code = "BNE" }, Department = new Department { Code = "BRN" } };

				var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				topLevelDataObject.DataContext = DataContextFactory.New();
				topLevelDataObject.DataContext.CodesMappedToTarget = true;
				Logger.TopLevelDataObject = topLevelDataObject;

				var reader = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
				mockedAdapter.Setup(f => f.ImportCharges(It.IsAny<BusinessObjectFactory>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IJobCostingData>(), It.IsAny<ZGuid>(), It.IsAny<ZString>()));

				AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());

				mockedAdapter.Verify(
					x => x.ImportCharges(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<IXmlImportLogger>(),
						It.IsAny<IJobCostingData>(),
						It.IsAny<ZGuid>(),
						It.IsAny<ZString>()),
					Times.Never());
			}
		}

		#endregion
		#region TestPackageJob
		public void TestPackageJob()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment1 = reader1.ReadIntoBusinessObject();
			AssertNull(PkgPackageJob.LoadPackageJob(consignment1));
			// Containers
			int containerLink1 = 1;
			int containerLink2 = 2;
			var container1 = UniversalTestHelper.CreateContainerDataObject("123", containerLink1);
			var container2 = UniversalTestHelper.CreateContainerDataObject("ABC", containerLink2);
			topLevelDataObject.SetContainerCollection(() => new DataObjectList<Container> { container1, container2 });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddContainerLink(pickupInstruction, containerLink2);
			// Packages
			int packageLink1 = 3;
			int packageLink2 = 4;
			int childPackageLink1 = 5;
			int childPackageLink2 = 6;
			var package1 = UniversalTestHelper.CreatePackageDataObject("XYZ", "PLT", packageLink1);
			var package2 = UniversalTestHelper.CreatePackageDataObject("DEF", "BOX", packageLink2);
			var childPackage1 = UniversalTestHelper.CreatePackageDataObject("Child1", "KEG", childPackageLink1);
			var childPackage2 = UniversalTestHelper.CreatePackageDataObject("Child2", "BOT", childPackageLink2);
			package1.SetPackingLineCollection(() => new List<PackingLine> { childPackage1 });
			package2.SetPackingLineCollection(() => new List<PackingLine> { childPackage2 });
			topLevelDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1, package2 });
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink1);
			UniversalTestHelper.AddPackageLink(pickupInstruction, childPackageLink1);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction });
			var reader2 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment2 = reader2.ReadIntoBusinessObject();
			var packageJob1 = PkgPackageJob.LoadPackageJob(consignment2);
			AssertNotNull(packageJob1);
			AssertEquals(2, packageJob1.Packages.Count);
			AssertEquals("ABC", packageJob1.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Container).KP_PackageID);
			var pallet = packageJob1.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals("XYZ", pallet.KP_PackageID);
			AssertEquals(1, pallet.Packages.Count);
			AssertEquals("Child1", pallet.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Keg).KP_PackageID);
			var deliveryInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddContainerLink(deliveryInstruction, containerLink1);
			UniversalTestHelper.AddPackageLink(deliveryInstruction, childPackageLink2);
			UniversalTestHelper.AddContainerLink(pickupInstruction, containerLink1);
			UniversalTestHelper.AddPackageLink(pickupInstruction, childPackageLink2);
			var reader3 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction, null);
			var consignment3 = reader3.ReadIntoBusinessObject();
			//consolidation.Bookings.Add(consignment3); // Consolidation needed to type decide PackageJob Parent
			var packageJob2 = PkgPackageJob.LoadPackageJob(consignment3);
			AssertNotNull(packageJob2);
			AssertEquals(2, packageJob2.Packages.Count);
			AssertEquals("Delivery Instruction should take precedence.", "123", packageJob2.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Container).KP_PackageID);
			AssertEquals("Delivery Instruction should take precedence.", "Child2", packageJob2.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Bottle).KP_PackageID);
		}

		public void TestPackageJob_CorrectPackageQty()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			int packageLink1 = 1;
			int packageLink2 = 2;
			int packageLink3 = 3;
			var package1 = UniversalTestHelper.CreatePackageDataObject(100, "XYZ", "PLT", volume: 100m, weight: 200m, packageLink: packageLink1);
			var package2 = UniversalTestHelper.CreatePackageDataObject(20, "DEF", "BOX", volume: 20m, weight: 10m, packageLink: packageLink2);
			var package3 = UniversalTestHelper.CreatePackageDataObject(10, "ABC", "CTN", volume: 10m, weight: 10m, packageLink: packageLink3);
			topLevelDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1, package2, package3 });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink1, 100);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink2, 20);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink3, 10);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddPackageLink(deliveryInstruction1, packageLink1, 60);
			UniversalTestHelper.AddPackageLink(deliveryInstruction1, packageLink2, 5);
			UniversalTestHelper.AddPackageLink(deliveryInstruction1, packageLink3);
			var deliveryInstruction2 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddPackageLink(deliveryInstruction2, packageLink1, 40);
			UniversalTestHelper.AddPackageLink(deliveryInstruction2, packageLink2, 15);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction, deliveryInstruction1, deliveryInstruction2 });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction1, null);
			var consigment1 = reader1.ReadIntoBusinessObject();
			var packageJob1 = PkgPackageJob.LoadPackageJob(consigment1);
			AssertNotNull(packageJob1);
			AssertEquals(3, packageJob1.Packages.Count);
			var palletPackage1 = packageJob1.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals("", palletPackage1.KP_PackageID);
			AssertEquals(60, palletPackage1.KP_PackageQty);
			AssertEquals(60m, palletPackage1.KP_Volume);
			AssertEquals(120m, palletPackage1.KP_Weight);
			var boxPackage1 = packageJob1.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals("", boxPackage1.KP_PackageID);
			AssertEquals(5, boxPackage1.KP_PackageQty);
			AssertEquals(5m, boxPackage1.KP_Volume);
			AssertEquals(2.5m, boxPackage1.KP_Weight);
			var cartonPackage1 = packageJob1.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Carton);
			AssertEquals("", cartonPackage1.KP_PackageID);
			AssertEquals(10, cartonPackage1.KP_PackageQty);
			AssertEquals(10m, cartonPackage1.KP_Volume);
			AssertEquals(10m, cartonPackage1.KP_Weight);
			var reader2 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction2, null);
			var consigment2 = reader2.ReadIntoBusinessObject();
			var packageJob2 = PkgPackageJob.LoadPackageJob(consigment2);
			AssertNotNull(packageJob2);
			AssertEquals(2, packageJob2.Packages.Count);
			var palletPackage2 = packageJob2.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals("", palletPackage2.KP_PackageID);
			AssertEquals(40, palletPackage2.KP_PackageQty);
			AssertEquals(40m, palletPackage2.KP_Volume);
			AssertEquals(80m, palletPackage2.KP_Weight);
			var boxPackage2 = packageJob2.Packages.Single(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals("", boxPackage2.KP_PackageID);
			AssertEquals(15, boxPackage2.KP_PackageQty);
			AssertEquals(15m, boxPackage2.KP_Volume);
			AssertEquals(7.5m, boxPackage2.KP_Weight);
		}

		public void TestPackageJobMultipleInstructions_ShouldThrowException_WhenPackageLinkedToPickupInstructionOnly()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			int packageLink1 = 1;
			var package1 = UniversalTestHelper.CreatePackageDataObject(100, "XYZ", "PLT", volume: 100m, weight: 200m, packageLink: packageLink1);
			topLevelDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1 });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink1, 100);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction, deliveryInstruction1 });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction1, null);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "The packages are not linked correctly.", () => reader1.ReadIntoBusinessObject());
		}

		public void TestPackageJobMultipleInstructions_ShouldThrowException_WhenNoCommonPackagesLinkedToInstructions()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			int packageLink1 = 1;
			int packageLink2 = 2;
			var package1 = UniversalTestHelper.CreatePackageDataObject(100, "XYZ", "PLT", volume: 100m, weight: 200m, packageLink: packageLink1);
			var package2 = UniversalTestHelper.CreatePackageDataObject(100, "XYZ", "CTN", volume: 100m, weight: 200m, packageLink: packageLink2);
			topLevelDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1, package2 });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink1, 100);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddPackageLink(deliveryInstruction1, packageLink2, 100);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction, deliveryInstruction1 });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction1, null);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "The packages are not linked correctly.", () => reader1.ReadIntoBusinessObject());
		}

		public void TestPackageJobMultipleInstructions_ShouldAssignPackagesCorrectly_WhenContainerIsLinkedtoDLV1_And_PackageIsLinkedToDLV2()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			int containerLink1 = 1;
			var container1 = UniversalTestHelper.CreateContainerDataObject("123", containerLink1);
			topLevelDataObject.SetContainerCollection(() => new DataObjectList<Container> { container1 });
			int packageLink1 = 1;
			var package1 = UniversalTestHelper.CreatePackageDataObject(100, "XYZ", "PLT", volume: 100m, weight: 200m, packageLink: packageLink1);
			topLevelDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1 });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddContainerLink(pickupInstruction, containerLink1);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink1, 100);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddPackageLink(deliveryInstruction1, packageLink1, 100);
			var deliveryInstruction2 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddContainerLink(deliveryInstruction2, containerLink1);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction, deliveryInstruction1, deliveryInstruction2 });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction1, null);
			var consignment1 = reader1.ReadIntoBusinessObject();
			var packageJob1 = PkgPackageJob.LoadPackageJob(consignment1);
			AssertNotNull(packageJob1);
			AssertEquals(1, packageJob1.Packages.Count);
			AssertEquals(Constants.PkgUnit.Pallet, packageJob1.Packages.Single().KP_F3_NKPackType);
			AssertEquals(100, packageJob1.Packages.Single().KP_PackageQty);
			var reader2 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction2, null);
			var consigment2 = reader2.ReadIntoBusinessObject();
			var packageJob2 = PkgPackageJob.LoadPackageJob(consigment2);
			AssertNotNull(packageJob2);
			AssertEquals(1, packageJob2.Packages.Count);
			AssertEquals(Constants.PkgUnit.Container, packageJob2.Packages.Single().KP_F3_NKPackType);
			AssertEquals("123", packageJob2.Packages.Single().KP_PackageID);
		}

		public void TestPackageJobMultipleInstructions_ShouldThrowException_WhenContainersLinkedToPickupInstructionOnly()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			int containerLink1 = 1;
			var container1 = UniversalTestHelper.CreateContainerDataObject("123", containerLink1);
			topLevelDataObject.SetContainerCollection(() => new DataObjectList<Container> { container1 });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddContainerLink(pickupInstruction, containerLink1);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction, deliveryInstruction1 });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction1, null);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "The packages are not linked correctly.", () => reader1.ReadIntoBusinessObject());
		}

		public void TestPackageJobMultipleInstructions_ShouldThrowException_WhenNoCommonContainersLinkedToInstructions()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			int containerLink1 = 1;
			int containerLink2 = 2;
			var container1 = UniversalTestHelper.CreateContainerDataObject("123", containerLink1);
			var container2 = UniversalTestHelper.CreateContainerDataObject("1234", containerLink2);
			topLevelDataObject.SetContainerCollection(() => new DataObjectList<Container> { container1, container2 });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddContainerLink(pickupInstruction, containerLink1);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddContainerLink(deliveryInstruction1, containerLink2);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction, deliveryInstruction1 });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction1, null);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "The packages are not linked correctly.", () => reader1.ReadIntoBusinessObject());
		}

		public void TestPackageJobMultipleInstructions_ShouldCreateOneConsignmentWithCommonPackage_WhenThereIsOneUnCommonPackage()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			int packageLink1 = 1;
			int packageLink2 = 2;
			var package1 = UniversalTestHelper.CreatePackageDataObject(100, "XYZ", "PLT", volume: 100m, weight: 200m, packageLink: packageLink1);
			var package2 = UniversalTestHelper.CreatePackageDataObject(20, "DEF", "PLT", volume: 20m, weight: 10m, packageLink: packageLink2);
			topLevelDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1, package2 });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink1, 100);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink2, 50);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddPackageLink(deliveryInstruction1, packageLink1, 100);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction, deliveryInstruction1 });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction1, null);
			var consignment = reader1.ReadIntoBusinessObject();
			var packageJob1 = PkgPackageJob.LoadPackageJob(consignment);
			AssertNotNull(packageJob1);
			AssertEquals(1, packageJob1.Packages.Count);
			AssertEquals(Constants.PkgUnit.Pallet, packageJob1.Packages.Single().KP_F3_NKPackType);
			AssertEquals(100, packageJob1.Packages.Single().KP_PackageQty);
		}

		#endregion
		#region TestConNoteSetToTransportReference
		public void TestConNoteSetToTransportReference()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var instructionPIC = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var instructionDLV = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { instructionPIC, instructionDLV });
			CreateAndAssignPackage(instructionPIC, topLevelDataObject, instructionDLV);
			var conNoteConfirmation = UniversalTestHelper.AddInstructionConfirmation(instructionDLV, ConfirmationTypes.Codes.ConNoteNo);
			conNoteConfirmation.Reference = "123";
			var reader = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instructionDLV, null);
			var importedConsignment = reader.ReadIntoBusinessObject();
			AssertEquals("Connote and TransportReference should be same", "123", importedConsignment.LTC_ConnoteNumber);
		}

		#endregion
		#region TestConsignmentAddresses
		public void TestConsignmentAddresses()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var pickUpInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstructionDataObject });
			var package1 = CreateAndAssignPackage(pickUpInstructionDataObject, topLevelDataObject);
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment1 = reader1.ReadIntoBusinessObject();
			AssertEquals(2, consignment1.Addresses.Count);
			AssertEquals(1, consignment1.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp).LTS_Sequence);
			var deliveryAddress1 = consignment1.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery);
			AssertEquals(2, deliveryAddress1.LTS_Sequence);
			AssertEquals(OrganisationTypesList.Codes.CNE, deliveryAddress1.OrganisationType);
			var deliveryInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			AddPackageToInstruction(deliveryInstructionDataObject);
			deliveryInstructionDataObject.ServiceInstruction = "SERVICEINSTRUCTION";
			var reader2 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstructionDataObject, null);
			var consignment2 = reader2.ReadIntoBusinessObject();
			AssertEquals(2, consignment2.Addresses.Count);
			AssertEquals(1, consignment2.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp).LTS_Sequence);
			var deliveryAddress2 = consignment2.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery);
			AssertEquals(2, deliveryAddress2.LTS_Sequence);
			AssertEquals("SERVICEINSTRUCTION", deliveryAddress2.LTS_Notes);
		}

		public void TestDtbConsignmentAddressesWithDepotAddress()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var pickUpInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			pickUpInstructionDataObject.Sequence = 1;
			var depotInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Multi);
			depotInstructionDataObject.Sequence = 2;
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstructionDataObject, depotInstructionDataObject });
			var package1 = CreateAndAssignPackage(pickUpInstructionDataObject, topLevelDataObject);
			AddPackageToInstruction(depotInstructionDataObject);

			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment1 = reader1.ReadIntoBusinessObject();

			AssertEquals(3, consignment1.Addresses.Count);
			AssertEquals(1, consignment1.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp).LTS_Sequence);
			var deliveryAddress1 = consignment1.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery);
			AssertEquals(2, deliveryAddress1.LTS_Sequence);
			AssertEquals(OrganisationTypesList.Codes.CNE, deliveryAddress1.OrganisationType);

			var deliveryInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			AddPackageToInstruction(deliveryInstructionDataObject);
			deliveryInstructionDataObject.ServiceInstruction = "SERVICEINSTRUCTION";
			deliveryInstructionDataObject.Sequence = 3;

			var reader2 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstructionDataObject, null);
			var consignment2 = reader2.ReadIntoBusinessObject();

			AssertEquals(3, consignment2.Addresses.Count);
			AssertEquals(1, consignment2.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp).LTS_Sequence);
			AssertEquals(2, consignment2.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Multi).LTS_Sequence);
			var deliveryAddress2 = consignment2.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery);
			AssertEquals(3, deliveryAddress2.LTS_Sequence);
			AssertEquals("SERVICEINSTRUCTION", deliveryAddress2.LTS_Notes);
		}

		public void TestEmptyPickAddress()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			var shipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment1.DataContext = DataContextFactory.New();
			shipment1.DataContext.AddDataSource(DataContextType.ForwardingShipment, "DUM456");
			var consignmentDataObject = shipment;
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction>());
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipment.SubShipmentCollection.Add(shipment1);
			shipment.DataContext.AddDataSource(DataContextType.TransportBookingConsolidation, "DUM789");
			shipment.DataContext.AddDataSource(DataContextType.ForwardingConsol, "DUM123");
			var reader = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "The UXML can not be imported as there is no Pickup address. Possible reason: No matching sub-shipment ID is found.", () =>
			{
				reader.ReadIntoBusinessObject();
			});
		}

		#endregion

		#region TestLocalClient
		public void TestLocalClient()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			topLevelDataObject.DataContext = DataContextFactory.New();
			Logger.TopLevelDataObject = topLevelDataObject;
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var localClientDataObject = GetNewAddressData_CRAHOLSYD(AddressTypes.SendersLocalClient);
			var orgAddress = UniversalTestHelper.CreateAddressInDB(localClientDataObject, Factory);
			consignmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { localClientDataObject });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment1 = reader1.ReadIntoBusinessObject();
			AssertNull(consignment1.Job);
			Logger.TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var reader2 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment2 = reader2.ReadIntoBusinessObject();
			AssertAddressContentMatches_CRAHOLSYD(consignment2.Job.LocalChargesAddr);
			consignment2.Job.Dispose(); // dispose mutex
			Logger.TopLevelDataObject.DataContext = DataContextFactory.New();
			var reader3 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment3 = reader3.ReadIntoBusinessObject();
			AssertNull(consignment3.Job);
			localClientDataObject.AddressType = nameof(DocAddressType.LocalClient);
			var reader4 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);
			var consignment4 = reader4.ReadIntoBusinessObject();
			AssertAddressContentMatches_CRAHOLSYD(consignment4.Job.LocalChargesAddr);
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, consignment4.PK);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.ClientRequestedBillingParty);
			AssertNotNull(Factory.LoadTop1<JobDocAddress>(query));
			consignment4.Job.Dispose(); // dispose mutex
		}

		#endregion

		#region TestConsignmentActionPackageDivot
		public void TestConsignmentActionPackageDivot_TwoActionsWithOnePackage()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var deliveryInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);

			CreateAndAssignPackage(pickupInstruction, topLevelDataObject, deliveryInstruction);

			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction, deliveryInstruction });
			UniversalTestHelper.AddInstructionConfirmation(pickupInstruction, ConfirmationTypes.Codes.PickUp);
			UniversalTestHelper.AddInstructionConfirmation(deliveryInstruction, ConfirmationTypes.Codes.Delivery);

			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction, consignment: null, additionalInstruction: pickupInstruction);
			var consignment = reader1.ReadIntoBusinessObject();

			var pickupAction = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp).PickupAction;
			var deliveryAction = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery).DeliveryAction;

			var pickupActionPackageDivots = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp).PickupAction.PackageDivots;
			var deliveryActionPackageDivots = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery).DeliveryAction.PackageDivots;

			AssertEquals(pickupActionPackageDivots.Count, 1);
			AssertEquals(deliveryActionPackageDivots.Count, 1);

			AssertEquals(pickupActionPackageDivots[0].LTP_LTA_ConsignmentAction, pickupAction.PK);
			AssertEquals(deliveryActionPackageDivots[0].LTP_LTA_ConsignmentAction, deliveryAction.PK);
			AssertEquals(pickupActionPackageDivots[0].LTP_KP_Package, consignment.PackageJob.Packages[0].PK);
			AssertEquals(deliveryActionPackageDivots[0].LTP_KP_Package, consignment.PackageJob.Packages[0].PK);

			AssertEquals(pickupActionPackageDivots[0].LTP_PackageQuantity, consignment.PackageJob.Packages[0].KP_PackageQty);
			AssertEquals(deliveryActionPackageDivots[0].LTP_PackageQuantity, consignment.PackageJob.Packages[0].KP_PackageQty);
		}

		public void TestConsignmentActionPackageDivot_TwoActionsWithMultiPackages()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var packageLink1 = 1;
			var package1 = UniversalTestHelper.CreatePackageDataObject(1, "AAA", "PLT", volume: 100m, weight: 100m, packageLink: packageLink1);
			var packageLink2 = 2;
			var package2 = UniversalTestHelper.CreatePackageDataObject(2, "BBB", "CTN", volume: 100m, weight: 100m, packageLink: packageLink2);
			var packageLink3 = 3;
			var package3 = UniversalTestHelper.CreatePackageDataObject(3, "CCC", "BOX", volume: 100m, weight: 100m, packageLink: packageLink3);
			var containerLink = 1;
			var container = UniversalTestHelper.CreateContainerDataObject("DDD", containerLink);

			topLevelDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1, package2, package3 });
			topLevelDataObject.SetContainerCollection(() => new DataObjectList<Container> { container });
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink1, 1);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink2, 2);
			UniversalTestHelper.AddPackageLink(pickupInstruction, packageLink3, 3);
			UniversalTestHelper.AddContainerLink(pickupInstruction, containerLink, 1);
			var deliveryInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddPackageLink(deliveryInstruction, packageLink1, 1);
			UniversalTestHelper.AddPackageLink(deliveryInstruction, packageLink2, 2);
			UniversalTestHelper.AddPackageLink(deliveryInstruction, packageLink3, 3);
			UniversalTestHelper.AddContainerLink(deliveryInstruction, containerLink, 1);

			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstruction, deliveryInstruction });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, deliveryInstruction, consignment: null, additionalInstruction: pickupInstruction);
			var consignment = reader1.ReadIntoBusinessObject();

			var pickupAction = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp).PickupAction;
			var deliveryAction = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery).DeliveryAction;

			var pickupActionPackageDivots = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp).PickupAction.PackageDivots;
			var deliveryActionPackageDivots = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery).DeliveryAction.PackageDivots;

			AssertEquals(pickupActionPackageDivots.Count, 4);
			AssertEquals(deliveryActionPackageDivots.Count, 4);

			Assert(pickupActionPackageDivots.All(d => d.LTP_LTA_ConsignmentAction == pickupAction.PK));
			Assert(deliveryActionPackageDivots.All(d => d.LTP_LTA_ConsignmentAction == deliveryAction.PK));

			AssertEquals(pickupActionPackageDivots.FirstOrDefault(d => d.LTP_KP_Package == consignment.PackageJob.Packages[0].PK)?.LTP_PackageQuantity, consignment.PackageJob.Packages[0].KP_PackageQty);
			AssertEquals(pickupActionPackageDivots.FirstOrDefault(d => d.LTP_KP_Package == consignment.PackageJob.Packages[1].PK)?.LTP_PackageQuantity, consignment.PackageJob.Packages[1].KP_PackageQty);
			AssertEquals(pickupActionPackageDivots.FirstOrDefault(d => d.LTP_KP_Package == consignment.PackageJob.Packages[2].PK)?.LTP_PackageQuantity, consignment.PackageJob.Packages[2].KP_PackageQty);
			AssertEquals(pickupActionPackageDivots.FirstOrDefault(d => d.LTP_KP_Package == consignment.PackageJob.Packages[3].PK)?.LTP_PackageQuantity, consignment.PackageJob.Packages[3].KP_PackageQty);
			AssertEquals(deliveryActionPackageDivots.FirstOrDefault(d => d.LTP_KP_Package == consignment.PackageJob.Packages[0].PK)?.LTP_PackageQuantity, consignment.PackageJob.Packages[0].KP_PackageQty);
			AssertEquals(deliveryActionPackageDivots.FirstOrDefault(d => d.LTP_KP_Package == consignment.PackageJob.Packages[1].PK)?.LTP_PackageQuantity, consignment.PackageJob.Packages[1].KP_PackageQty);
			AssertEquals(deliveryActionPackageDivots.FirstOrDefault(d => d.LTP_KP_Package == consignment.PackageJob.Packages[2].PK)?.LTP_PackageQuantity, consignment.PackageJob.Packages[2].KP_PackageQty);
			AssertEquals(deliveryActionPackageDivots.FirstOrDefault(d => d.LTP_KP_Package == consignment.PackageJob.Packages[3].PK)?.LTP_PackageQuantity, consignment.PackageJob.Packages[3].KP_PackageQty);
		}

		#endregion
		#endregion
		#region TestLogChildTopLevelObjectsOnImport
		public void TestLogChildTopLevelObjectsOnImport()
		{
			var pickupOrg = Helper.CreateOrganisation("PCK1", address1: "123 Test st");
			var deliveryOrg = Helper.CreateOrganisation("Dlb1", address1: "456 Test st");
			var consignment = Helper.CreateConsignment("LT001");
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupOrg.MainAddress);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			Helper.CreatePackage(consignment, "XYZ", "PLT");
			Factory.SaveForTesting();
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consolidationDataObject, null, consignment);
			var matchingConsignment1 = reader1.ReadIntoBusinessObject();
			AssertEquals("Should match existing Consignment.", consignment, matchingConsignment1);
			AssertEquals("No Top Level Child Objects should be logged because there is no Delivery Instruction.", 0, Logger.ChildTopLevelObjects.Count());
			var pickupInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var deliveryInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			CreateAndAssignPackage(pickupInstruction, consolidationDataObject, deliveryInstruction);
			var reader2 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consolidationDataObject, deliveryInstruction, consignment);
			var matchingConsignment2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Should match existing Consignment.", consignment, matchingConsignment2);
			AssertContainsExactElementsInAnyOrder(new[] { "LandTransportConsignment|LT001" }, Logger.ChildTopLevelObjects.Select(c => c.DataContextType + "|" + c.DataContextKey));
		}

		#endregion
		#region TestRejectImport_WhenPackageIDsProvidedAreNotCorrect
		public void TestRejectImport_WhenPackageIDsProvidedAreNotCorrect()
		{
			var pickupOrg = Helper.CreateOrganisation("PCK1", address1: "123 Test st");
			var deliveryOrg = Helper.CreateOrganisation("Dlb1", address1: "456 Test st");
			var consignment = Helper.CreateConsignment("LT001");
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupOrg.MainAddress);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			var outerPallet = helper.CreatePackage(consignment.PackageJob, "PLT", "PLT-1");
			var innerCarton = outerPallet.Packages.AddNew("CTN", "CTN-1");
			Factory.SaveForTesting();
			int palletLink = 1;
			int cartonLink = 2;
			// make delivery Package have a different ID
			var palletDataObject = UniversalTestHelper.CreatePackageDataObject(1, "PLT-1", "PLT", palletLink);
			palletDataObject.SetPackingLineCollection(() => new List<PackingLine> { UniversalTestHelper.CreatePackageDataObject(1, "CTN-X", "CTN", cartonLink) });
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var pickUpInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var deliveryInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddPackageLink(pickUpInstruction, palletLink);
			UniversalTestHelper.AddPackageLink(deliveryInstruction, cartonLink);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstruction, deliveryInstruction });
			consolidationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { palletDataObject });
			var reader1 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consolidationDataObject, deliveryInstruction, consignment);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Package IDs were specified for the Packages to Deliver but they cannot be found on the Existing Consignment.", () => reader1.ReadIntoBusinessObject());
			var package = Factory.New<PkgPackage>(); // purposefully create a detached package to cause below exception when creating new Consignment.
			var reader2 = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consolidationDataObject, deliveryInstruction, null);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Package IDs were specified for the Packages to Deliver but they cannot be found on the Existing Consignment.", () => reader2.ReadIntoBusinessObject());
		}

		#endregion
		#region TestRejectImport_WhenPackageStructureDoesNotMatch
		public void TestRejectImport_WhenPackageStructureDoesNotMatch()
		{
			var pickupOrg = Helper.CreateOrganisation("PCK1", address1: "123 Test st");
			var deliveryOrg = Helper.CreateOrganisation("Dlb1", address1: "456 Test st");
			var consignment = Helper.CreateConsignment("LT001");
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupOrg.MainAddress);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryOrg.MainAddress);
			var outerPallet = helper.CreatePackage(consignment.PackageJob, "PLT", "PLT-1");
			var innerCarton = outerPallet.Packages.AddNew("CTN", "CTN-1");
			Factory.SaveForTesting();
			int palletLink = 1;
			int cartonLink = 2;
			var boxDataObject = UniversalTestHelper.CreatePackageDataObject(1, "BOX-1", "BOX");
			var cartonDataObject = UniversalTestHelper.CreatePackageDataObject(1, "CTN-1", "CTN", cartonLink);
			var palletDataObject = UniversalTestHelper.CreatePackageDataObject(1, "PLT-1", "PLT", palletLink);
			palletDataObject.SetPackingLineCollection(() => new List<PackingLine> { boxDataObject, cartonDataObject });
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var pickUpInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var deliveryInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			UniversalTestHelper.AddPackageLink(pickUpInstruction, palletLink);
			UniversalTestHelper.AddPackageLink(deliveryInstruction, cartonLink);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstruction, deliveryInstruction });
			consolidationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { palletDataObject });
			var reader = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, consolidationDataObject, deliveryInstruction, consignment);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import as Existing Pick Up Consignment Packages do not match the Booking Pick Up Packages.", () => reader.ReadIntoBusinessObject());
			palletDataObject.SetPackingLineCollection(() => new List<PackingLine> { cartonDataObject });
			consignment.PackageJob.Delete();
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import as Existing Pick Up Consignment Packages do not match the Booking Pick Up Packages.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		public void TestConsignmentLegs_ShouldCNAndActionsLinkedCorrectly()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var pickUpInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstructionDataObject });
			var package = CreateAndAssignPackage(pickUpInstructionDataObject, topLevelDataObject);
			var reader = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, topLevelDataObject, instruction: null, consignment: null);

			var consignment = reader.ReadIntoBusinessObject();

			var pickupAction = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp).PickupAction;
			var pickupLegs = pickupAction.PickupLegs;
			var deliveryAction = consignment.Addresses.Single(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery).DeliveryAction;
			var deliveryLegs = deliveryAction.DeliveryLegs;

			AssertEquals(1, pickupLegs.Count);
			AssertEquals(1, deliveryLegs.Count);
			AssertEquals(pickupLegs.First(), deliveryLegs.First());
			AssertEquals(1, pickupLegs.First().LTG_Sequence);
			AssertEquals(consignment.PK, pickupLegs.First().LTG_LTC_Consignment);
			AssertEquals(pickupAction.PK, pickupLegs.First().LTG_LTA_Pickup);
			AssertEquals(deliveryAction.PK, pickupLegs.First().LTG_LTA_Delivery);
		}

		public void TestConsignmentLegs_WithDepotAddress_ShouldCNAndActionsLinkedCorrectly()
		{
			var topLevelDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var pickUpInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			pickUpInstructionDataObject.Sequence = 1;
			var depotInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Multi);
			depotInstructionDataObject.Sequence = 2;
			var deliveryInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			deliveryInstructionDataObject.Sequence = 3;
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstructionDataObject, depotInstructionDataObject, deliveryInstructionDataObject });
			var package1 = CreateAndAssignPackage(pickUpInstructionDataObject, topLevelDataObject, packageLink: 1);
			var package2 = CreateAndAssignPackage(pickUpInstructionDataObject, topLevelDataObject, packageLink: 2);
			var package3 = CreateAndAssignPackage(pickUpInstructionDataObject, topLevelDataObject, packageLink: 3);
			consignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1, package2, package3 });
			AddPackageToInstruction(depotInstructionDataObject, 1);
			AddPackageToInstruction(deliveryInstructionDataObject, 1);
			AddPackageToInstruction(deliveryInstructionDataObject, 2);
			AddPackageToInstruction(deliveryInstructionDataObject, 3);

			pickUpInstructionDataObject.InstructionPackingLineLinkCollection[0].ConfirmationCollection = new List<Confirmation>()
			{
				NewConfirmation(ConfirmationTypes.Codes.PickUp, new List<PackingLink>()
					{
						new PackingLink() { PackingLineLink = 1, PackedQuantity = 1 },
						new PackingLink() { PackingLineLink = 3, PackedQuantity = 5 },
					}),
				NewConfirmation(ConfirmationTypes.Codes.PickUp, new List<PackingLink>()
					{
						new PackingLink() { PackingLineLink = 2, PackedQuantity = 3 },
					}),
			};

			depotInstructionDataObject.InstructionPackingLineLinkCollection[0].ConfirmationCollection = new List<Confirmation>()
			{
				NewConfirmation(ConfirmationTypes.Codes.PickUp, new List<PackingLink>()
					{
						new PackingLink() { PackingLineLink = 2, PackedQuantity = 3 },
					}),
				NewConfirmation(ConfirmationTypes.Codes.Delivery, new List<PackingLink>()
					{
						new PackingLink() { PackingLineLink = 2, PackedQuantity = 3 },
					}),
			};

			deliveryInstructionDataObject.InstructionPackingLineLinkCollection[0].ConfirmationCollection = new List<Confirmation>()
			{
				NewConfirmation(ConfirmationTypes.Codes.Delivery, new List<PackingLink>()
					{
						new PackingLink() { PackingLineLink = 1, PackedQuantity = 1 },
						new PackingLink() { PackingLineLink = 3, PackedQuantity = 5 },
					}),
				NewConfirmation(ConfirmationTypes.Codes.Delivery, new List<PackingLink>()
					{
						new PackingLink() { PackingLineLink = 2, PackedQuantity = 3 },
					}),
			};

			var reader = new DtbConsignmentDataObjectReader(consignmentDataObject, Logger, Factory, null, topLevelDataObject);
			var consignment = reader.ReadIntoBusinessObject();

			var pickupAddress = consignment.Addresses.FirstOrDefault(i => i.LTS_InstructionType == InstructionTypes.Codes.PickUp);
			var depotAddress = consignment.Addresses.FirstOrDefault(i => i.LTS_InstructionType == InstructionTypes.Codes.Multi);
			var deliveryAddress = consignment.Addresses.FirstOrDefault(i => i.LTS_InstructionType == InstructionTypes.Codes.Delivery);

			AssertEquals("Pickup Address should have 2 Actions", 2, pickupAddress.Actions.Count);
			AssertEquals("Pickup Address should have 2 Pickup Actions", 2, pickupAddress.Actions.Count(action => action.ActionType == ActionTypes.Codes.PickUp));
			AssertEquals("Depot Address should have 2 Actions", 2, depotAddress.Actions.Count);
			AssertEquals("Depot Address should have 1 Pickup Action", 1, depotAddress.Actions.Count(action => action.ActionType == ActionTypes.Codes.PickUp));
			AssertEquals("Depot Address should have 1 Delivery Action", 1, depotAddress.Actions.Count(action => action.ActionType == ActionTypes.Codes.Delivery));
			AssertEquals("Delivery Address should have 2 Actions", 2, deliveryAddress.Actions.Count);
			AssertEquals("Delivery Address should have 2 Delivery Actions", 2, deliveryAddress.Actions.Count(action => action.ActionType == ActionTypes.Codes.Delivery));

			var depotPickupAction = depotAddress.Actions.Single(i => i.ActionType == ActionTypes.Codes.PickUp);
			var depotDeliveryAction = depotAddress.Actions.Single(i => i.ActionType == ActionTypes.Codes.Delivery);

			AssertEquals("Depot Pickup Action should have 1 Leg.", 1, depotPickupAction.PickupLegs.Count);
			AssertEquals("Depot Delivery Action should have 1 Leg.", 1, depotDeliveryAction.DeliveryLegs.Count);

			AssertEquals("Depot Pickup Action should have 1 Leg linked to deliveryAddress' action", 1, deliveryAddress.Actions.Count(a => a.PK == depotPickupAction.PickupLegs[0].LTG_LTA_Delivery));
			AssertEquals("Depot Delivery Action should have 1 Leg linked to pickupAddress' action", 1, pickupAddress.Actions.Count(a => a.PK == depotDeliveryAction.DeliveryLegs[0].LTG_LTA_Pickup));
			Assert("The other leg should be from pickup address to delivery address directly", deliveryAddress.Actions.Single(a => a.PK != depotPickupAction.PickupLegs[0].LTG_LTA_Delivery).DeliveryLegs.Single().PK == pickupAddress.Actions.Single(a => a.PK != depotDeliveryAction.DeliveryLegs[0].LTG_LTA_Pickup).PickupLegs.Single().PK);
		}

		Confirmation NewConfirmation(ZString type, List<PackingLink> links)
		{
			var confirmation = new Confirmation { DateDescription = type };

			confirmation.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			confirmation.SetPackingLinkCollection(() => links);

			return confirmation;
		}

		#region Helper
		TransportConsignmentTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new TransportConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportConsignmentTestHelper helper;
		#endregion
		#region Implementation
		#region GetNewReader
		DtbConsignmentDataObjectReader GetNewReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, DtbConsignment consignment, UniversalShipment topLevelDO)
		{
			return new DtbConsignmentDataObjectReader(shipment, logger, factory, consignment, topLevelDO);
		}

		void AssertGoodsAndInsuranceValueAndCurrencyOnCreatedConsignment(UniversalShipment bookingShipment, UniversalShipment topLevelDataObject, ZDecimal expectedGoodsValue, ZString expectedGoodsCurrency, ZDecimal expectedInsuranceValue, ZString expectedInsuranceCurrency, string sourceShipmentDescription)
		{
			var reader = GetNewReader(bookingShipment, Logger, Factory, null, topLevelDataObject);
			var consignmentBO = reader.ReadIntoBusinessObject();
			AssertEquals("Goods Value of new consignment object should equal Goods Value of " + sourceShipmentDescription + " shipment", expectedGoodsValue, consignmentBO.LTC_GoodsValue);
			AssertEquals("Goods Currency of new consignment object should equal Goods Currency of " + sourceShipmentDescription + " shipment", expectedGoodsCurrency, consignmentBO.LTC_RX_NKGoodsValueCurrency);
			AssertEquals("Insurance Value of new consignment object should equal Insurance Value of " + sourceShipmentDescription + " shipment", expectedInsuranceValue, consignmentBO.LTC_InsuranceValue);
			AssertEquals("Insurance Currency of new consignment object should equal Insurance Currency of " + sourceShipmentDescription + " shipment", expectedInsuranceCurrency, consignmentBO.LTC_RX_NKInsuranceValueCurrency);
		}

		void SetGoodsAndInsuranceValues(UniversalShipment shipment, ZDecimal goodsValue, ZString goodsCurrency, ZString goodsCurrencyDescription, ZDecimal insuranceValue, ZString insuranceCurrency, ZString insuranceCurrencyDescription)
		{
			shipment.GoodsValue = goodsValue;
			shipment.GoodsValueCurrency = new Currency()
			{ Code = goodsCurrency, Description = goodsCurrencyDescription };
			shipment.InsuranceValue = insuranceValue;
			shipment.InsuranceValueCurrency = new Currency()
			{ Code = insuranceCurrency, Description = insuranceCurrencyDescription };
		}

		static readonly ZDecimal TestGoodsValue = 100;
		static readonly ZString TestGoodsCurrency = "AUD";
		static readonly ZString TestGoodsCurrencyDescription = "Australian Dollar";
		// different to the GoodsValue/Currency only so that we can see clearly that they are not derived from the same place
		static readonly ZDecimal TestInsuranceValue = 80;
		static readonly ZString TestInsuranceCurrency = "USD";
		static readonly ZString TestInsuranceCurrencyDescription = "US Dollar";
		#endregion
		#region CreateAndAssignPackageToInstruction
		PackingLine CreateAndAssignPackage(Instruction addressPIC, UniversalShipment consignmentDataObject, Instruction addressDLV = null, int packageLink = 1)
		{
			var package1 = UniversalTestHelper.CreatePackageDataObject("XYZ", "PLT", packageLink);
			AddPackageToInstruction(addressPIC, packageLink);
			if (addressDLV != null)
			{
				AddPackageToInstruction(addressDLV, packageLink);
			}

			consignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1 });
			return package1;
		}

		void AddPackageToInstruction(Instruction instruction, int packageLink = 1)
		{
			UniversalTestHelper.AddPackageLink(instruction, packageLink);
			instruction.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink> { new InstructionPackingLineLink { PackingLineLink = PACKAGELINK, Quantity = 1 } });
		}

		const int PACKAGELINK = 1;
		#endregion
		#endregion
	}
}
