using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentAddressDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestGetExistingBusinessObject()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var pickUpAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			Factory.SaveForTesting();
			var multiConsignmentAddress = UniversalTestHelper.CreateInstructionDataObject(ConsignmentAddressTypes.Codes.Multi);
			var reader1 = GetNewReader(multiConsignmentAddress, Logger, Factory, consignment);
			var newConsignmentAddress = reader1.ReadIntoBusinessObject();
			AssertEquals("Should not match Multi ConsignmentAddress.", false, newConsignmentAddress.IsInDatabase);
			var pickUpConsignmentAddress = UniversalTestHelper.CreateInstructionDataObject(ConsignmentAddressTypes.Codes.PickUp);
			var reader2 = GetNewReader(pickUpConsignmentAddress, Logger, Factory, consignment);
			var matchingConsignmentAddress = reader2.ReadIntoBusinessObject();
			AssertEquals("Should match non-Multi Instructions by Type.", pickUpAddress, matchingConsignmentAddress);
		}

		public void TestBasicFieldMappings()
		{
			new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageCFS), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();
			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_ShortCode = "TRUCK";
			var addressDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);
			addressDataObject.Address = GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageCFS);
			addressDataObject.DropMode = new DropMode { Code = "ASK" };
			addressDataObject.Equipment = "TRUCK";
			addressDataObject.Sequence = 10;
			addressDataObject.ServiceInstruction = "Service Instruction";
			addressDataObject.Type = new CodeDescriptionPair { Code = ConsignmentAddressTypes.Codes.Multi };
			var consignment = Helper.CreateConsignment("CS001");
			var reader = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var address = reader.ReadIntoBusinessObject();
			address.DocAddresses.Load(); // Have to reload into memory to reflect the delete + Add in Reader
			CombineAssertions(() =>
			{
				AssertJobDocAddressContentMatches_CRAHOLSYDWithoutGovRegNumAndType(address.Address);
				AssertEquals("address.LTS_DropMode", "PSL", address.LTS_DropMode);
				AssertEquals("address.LTS_InstructionType", ConsignmentAddressTypes.Codes.Multi, address.LTS_InstructionType);
				AssertEquals("address.LTS_RQ_RequiredEquipment", equipment.PK, address.LTS_RQ_RequiredEquipment);
				AssertEquals("address.LTS_Sequence", 10, address.LTS_Sequence);
				AssertEquals("address.LTS_ServiceInstruction", "Service Instruction", address.LTS_Notes);
			});
		}

		public void TestAddressOfConsignmentAddress()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var addressDataObject = new Instruction { Address = new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageExporter) }, Type = new CodeDescriptionPair { Code = ConsignmentAddressTypes.Codes.PickUp } };
			var reader1 = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var consignmentAddress1 = reader1.ReadIntoBusinessObject();
			AssertEquals(OrganisationTypesList.Codes.CNR, consignmentAddress1.OrganisationType);
			consignmentAddress1.LTS_LTC_Consignment = consignment.GetValue(DtbConsignmentSchema.PK);
			consignmentAddress1.LTS_Sequence = 1; // this would have been done by the DtbConsignment Reader
			consignmentAddress1.LTS_Status = ConsignmentAddressStatus.Codes.Allocated; // this would have been done by the DtbConsignment Reader
			Factory.SaveForTesting();
			var consignmentAddressInOtherFactory = new BusinessObjectFactory().Load<DtbConsignmentAddress>(consignmentAddress1.PK);
			AssertEquals("Should save JobDocAddress to Database.", consignmentAddress1.Address.PK, consignmentAddressInOtherFactory.Address.PK);
			AssertEquals(true, consignmentAddress1.Address.IsEmpty);
			addressDataObject.Address.Address1 = "Some Street";
			addressDataObject.Address.CompanyName = "Some Co";
			addressDataObject.Address.State = "Some State";
			addressDataObject.Address.City = "Some City";
			addressDataObject.Type = new CodeDescriptionPair { Code = ConsignmentAddressTypes.Codes.Delivery };
			var reader2 = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var consignmentAddress2 = reader2.ReadIntoBusinessObject();
			consignmentAddress2.LTS_LTC_Consignment = consignment.GetValue(DtbConsignmentSchema.PK);
			consignmentAddress2.LTS_Sequence = 2; // this would have been done by the DtbConsignment Reader
			consignmentAddress2.LTS_Status = ConsignmentAddressStatus.Codes.Allocated; // this would have been done by the DtbConsignment Reader
			AssertEquals(OrganisationTypesList.Codes.CNE, consignmentAddress2.OrganisationType); // as the dataObject type is changed to Delivery
			AssertEquals(true, consignmentAddress2.Address.E2_AddressOverride);
			AssertEquals("Some Street", consignmentAddress2.Address.E2_Address1);
			AssertEquals("Some City", consignmentAddress2.Address.E2_City);
			AssertEquals("Some Co", consignmentAddress2.Address.E2_CompanyName);
			AssertEquals("Some State", consignmentAddress2.Address.E2_State);
			Factory.SaveForTesting();
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, consignmentAddress2.PK);
			AssertEquals("Should only have one DocAddress.", 1, Factory.BOFactory.GetDatabaseCount(typeof(JobDocAddress), query));
		}

		public void TestAddress_IsProperlyDeleted()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var pickUpAddress = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.LocalCartageExporter));
			new OrganisationDataObjectReader(pickUpAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();
			var addressDataObject = new Instruction { Address = pickUpAddress, Type = new CodeDescriptionPair { Code = ConsignmentAddressTypes.Codes.PickUp } };
			var reader1 = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var consignmentAddress1 = reader1.ReadIntoBusinessObject();
			AssertEquals(OrganisationTypesList.Codes.CNR, consignmentAddress1.OrganisationType);
			AssertAddressContentMatches_CRAHOLSYD(consignmentAddress1.Address.Address);
			consignmentAddress1.LTS_LTC_Consignment = consignment.GetValue(DtbConsignmentSchema.PK);
			consignmentAddress1.LTS_Sequence = 1; // this would have been done by the DtbConsignment Reader
			consignmentAddress1.LTS_Status = ConsignmentAddressStatus.Codes.Allocated; // this would have been done by the DtbConsignment Reader
			Factory.SaveForTesting();
			var newDeliveryAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.LocalCartageExporter));
			new OrganisationDataObjectReader(newDeliveryAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();
			var newfactory1 = new UniversalObjectFactory();
			var consignmentInNewFactory = newfactory1.Load<DtbConsignment>(consignment.PK);
			var query1 = new ZQuery(JobDocAddressSchema.E2_ParentID, consignmentAddress1.PK);
			var poke = newfactory1.Load<JobDocAddress>(query1); // load the bizO into the factory to mix the rows and bizOs
			addressDataObject.Type = new CodeDescriptionPair { Code = ConsignmentAddressTypes.Codes.Delivery };
			addressDataObject.Address = newDeliveryAddress;
			var reader2 = GetNewReader(addressDataObject, Logger, newfactory1, consignmentInNewFactory);
			var consignmentAddress2 = reader2.ReadIntoBusinessObject();
			AssertEquals(OrganisationTypesList.Codes.CNE, consignmentAddress2.OrganisationType); // as the dataObject type is changed to Delivery
			AssertAddressContentMatches_INTHEMSYD(consignmentAddress2.Address.Address);
			consignmentAddress2.LTS_LTC_Consignment = consignment.GetValue(DtbConsignmentSchema.PK);
			consignmentAddress2.LTS_Sequence = 2; // this would have been done by the DtbConsignment Reader
			consignmentAddress2.LTS_Status = ConsignmentAddressStatus.Codes.Allocated; // this would have been done by the DtbConsignment Reader
			newfactory1.SaveForTesting();
			var newfactory2 = new UniversalObjectFactory();
			var query2 = new ZQuery(JobDocAddressSchema.E2_ParentID, consignmentAddress2.PK);
			var docAddresses = newfactory2.Load<JobDocAddress>(query2);
			AssertEquals("Should only have one DocAddress.", 1, docAddresses.Length);
		}

		public void TestAddress_DoesNotUseUnmatchedOrgWhenAddressIsEmpty()
		{
			SetUseUnmatchedOrganisationForMatchingRegistry(true);
			var consignment = Helper.CreateConsignment("CS001");
			var deliveryAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageImporter) };
			var addressDataObject = new Instruction { Address = deliveryAddress };
			var reader = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var consignmentAddress = reader.ReadIntoBusinessObject();
			AssertEquals("ConsignmentAddress: Address should be empty.", ZGuid.Empty, consignmentAddress.Address.E2_OA_Address);
			AssertEquals("ConsignmentAddress: Address should be empty.", false, consignmentAddress.Address.E2_AddressOverride);
			AssertEquals("ConsignmentAddress: should have correct Address Type.", OrganisationTypesList.Codes.CNE, consignmentAddress.OrganisationType);
		}

		public void TestUnmatchedOrgNotesAreStoredForUnmatchedAddressesOfConsignmentAddress()
		{
			SetUseUnmatchedOrganisationForMatchingRegistry(true);
			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageExporter, "Consignor");
			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageImporter, "Consignee");
			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageCFS, "CFS");
			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageCTO, "CTO");
			AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType.LocalCartageYard, "CYD");
		}

		void AssertUnmatchedOrgNotesAreStoredForAddressType(DocAddressType orgType, string orgTypeString)
		{
			var consignment = Helper.CreateConsignment("CS001");
			var initialUnmatchedOrganisationNotes = consignment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Precondition", 0, initialUnmatchedOrganisationNotes.Length);
			var addressDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);
			addressDataObject.Address = GetNewAddressData_CRAHOLSYD(orgType);
			var reader = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var consignmentAddress = reader.ReadIntoBusinessObject();
			consignmentAddress.DocAddresses.Load(); // Have to reload into memory to reflect the delete + Add in Reader
			AssertEquals(true, consignmentAddress.Address.Organisation.IsSystemDefinedOrganisation);
			var unmatchedOrganisationNotes = consignment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Unmatched organisation note should have been created", 1, unmatchedOrganisationNotes.Length);
			AssertMultilineASCIIEquals("Note should be added to the consignment", string.Format(@"Organisation Type: {0}
Owner Code: 
EDI Code: CRAHOLSYD
Organisation Name: CRACKERJACK HOLDINGS
Address Line 1: 1804 Fudrucker Way
Address Line 2: 
City: BOTANY
Post Code: 2035
State or Province: NSW
Country: AU
Doc Address Type:", orgTypeString), unmatchedOrganisationNotes[0].ST_NoteText.TrimEnd());
		}

		public void TestPopulatingAddress_AddressTypes()
		{
			var warehouseAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.LocalCartageWarehouse), Address1 = "123 Warehouse Street", City = "Sydney", CompanyName = "WHS", Country = new Country { Code = "AU" }, OrganizationCode = "WHS123", Port = new UNLOCO { Code = "AUSYD" }, Postcode = "2000" };
			var consignorAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.LocalCartageExporter), Address1 = "123 Consignor Street", City = "Sydney", CompanyName = "CNR", Country = new Country { Code = "AU" }, OrganizationCode = "CNR123", Port = new UNLOCO { Code = "AUSYD" }, Postcode = "2000" };
			var consigneeAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.LocalCartageImporter), Address1 = "123 Consignee Street", City = "Sydney", CompanyName = "CNE", Country = new Country { Code = "AU" }, OrganizationCode = "CNE123", Port = new UNLOCO { Code = "AUSYD" }, Postcode = "2000" };
			var depotAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{ AddressType = nameof(DocAddressType.LocalCartageCFS), Address1 = "123 Depot Street", City = "Sydney", CompanyName = "DEP", Country = new Country { Code = "AU" }, OrganizationCode = "DEP123", Port = new UNLOCO { Code = "AUSYD" }, Postcode = "2000" };
			UniversalTestHelper.CreateAddressInDB(warehouseAddress, Factory);
			UniversalTestHelper.CreateAddressInDB(consignorAddress, Factory);
			UniversalTestHelper.CreateAddressInDB(consigneeAddress, Factory);
			var depotAddressInDB = UniversalTestHelper.CreateAddressInDB(depotAddress, Factory);
			depotAddressInDB.Header.OH_IsMiscFreightServices = true;
			depotAddressInDB.Header.OH_IsUnpackDepot = true;
			var consignment1 = Helper.CreateConsignment("CS001");
			var reader1 = GetNewReader(new Instruction { Address = warehouseAddress, Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.PickUp } }, Logger, Factory, consignment1);
			var reader2 = GetNewReader(new Instruction { Address = warehouseAddress, Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.Delivery } }, Logger, Factory, consignment1);
			var pickupConsignmentAddress1 = reader1.ReadIntoBusinessObject();
			var deliveryConsignmentAddress1 = reader2.ReadIntoBusinessObject();
			AssertEquals(DocAddressType.LocalCartageExporter, pickupConsignmentAddress1.Address.DocAddressType);
			AssertEquals(DocAddressType.LocalCartageImporter, deliveryConsignmentAddress1.Address.DocAddressType);
			var consignment2 = Helper.CreateConsignment("CS001");
			var reader3 = GetNewReader(new Instruction { Address = consignorAddress, Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.PickUp } }, Logger, Factory, consignment2);
			var reader4 = GetNewReader(new Instruction { Address = consigneeAddress, Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.Delivery } }, Logger, Factory, consignment2);
			var pickupConsignmentAddress2 = reader3.ReadIntoBusinessObject();
			var deliveryConsignmentAddress2 = reader4.ReadIntoBusinessObject();
			AssertEquals(DocAddressType.LocalCartageExporter, pickupConsignmentAddress2.Address.DocAddressType);
			AssertEquals(DocAddressType.LocalCartageImporter, deliveryConsignmentAddress2.Address.DocAddressType);
			var consignment3 = Helper.CreateConsignment("CS001");
			var reader5 = GetNewReader(new Instruction { Address = depotAddress, Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.PickUp } }, Logger, Factory, consignment3);
			var reader6 = GetNewReader(new Instruction { Address = depotAddress, Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.Delivery } }, Logger, Factory, consignment3);
			var pickupConsignmentAddress3 = reader5.ReadIntoBusinessObject();
			var deliveryConsignmentAddress3 = reader6.ReadIntoBusinessObject();
			AssertEquals(DocAddressType.LocalCartageExporter, pickupConsignmentAddress3.Address.DocAddressType);
			AssertEquals(DocAddressType.LocalCartageImporter, deliveryConsignmentAddress3.Address.DocAddressType);
		}

		public void TestAction_ShouldBeAdded_WhenContainerLinkExists()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var addressDataObject = UniversalTestHelper.CreateInstructionDataObject(ConsignmentAddressTypes.Codes.Delivery);
			var consignmentActionDataObject = UniversalTestHelper.AddContainerDivotConfirmation(addressDataObject, InstructionTypes.Codes.PickUp);
			consignmentActionDataObject.Reference = "123";

			var reader1 = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var consignmentAddress1 = reader1.ReadIntoBusinessObject();

			AssertEquals(1, consignmentAddress1.Actions.Count);
			var action1 = consignmentAddress1.Actions[0];
			AssertEquals("123", action1.LTA_ReferenceNumber);
			AssertEquals(ActionTypes.Codes.PickUp, action1.LTA_ActionType);
		}

		[TestDate(2022, 1, 2)]
		public void TestActionIsAddedIfAddressHasNone()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var addressDataObject = UniversalTestHelper.CreateInstructionDataObject(ConsignmentAddressTypes.Codes.Delivery);
			var consignmentActionDataObject = UniversalTestHelper.AddDivotConfirmation(addressDataObject, InstructionTypes.Codes.PickUp);
			consignmentActionDataObject.Reference = "123";
			var reader1 = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var consignmentAddress1 = reader1.ReadIntoBusinessObject();
			AssertEquals(1, consignmentAddress1.Actions.Count);
			var action1 = consignmentAddress1.Actions[0];
			AssertEquals("123", action1.LTA_ReferenceNumber);
			AssertEquals(ActionTypes.Codes.PickUp, action1.LTA_ActionType);
			addressDataObject.SetInstructionPackingLineLinkCollection(() => null);
			addressDataObject.Type = new CodeDescriptionPair { Code = ConsignmentAddressTypes.Codes.PickUp };
			var reader2 = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var consignmentAddress2 = reader2.ReadIntoBusinessObject();
			AssertEquals(1, consignmentAddress2.Actions.Count);
			var action2 = consignmentAddress2.Actions[0];
			AssertEquals("There is no Confirmation Data Object, so reference number is empty.", "", action2.LTA_ReferenceNumber);
			AssertEquals("There is no Confirmation Data Object, so Action Type comes from Instruction Type.", ActionTypes.Codes.PickUp, action2.LTA_ActionType);

			consignmentAddress1.LTS_Sequence = 1;
			consignmentAddress2.LTS_Sequence = 2;
			consignmentAddress1.LTS_LTC_Consignment = consignment.PK;
			consignmentAddress2.LTS_LTC_Consignment = consignment.PK;
			Factory.SaveForTesting();
			AssertNotEquals(string.Empty, action2.LTA_ActionID);

			CombineAssertions("Audit columns are populated correctly", () =>
			{
				AssertEquals("LTA_SystemCreateUser", GlbStaff.CurrentUser.GS_Code, action2.LTA_SystemCreateUser);
				AssertEquals("LTA_SystemCreateTimeUtc", ZDateTime.UtcNow, action2.LTA_SystemCreateTimeUtc);
				AssertEquals("LTA_SystemLastEditUser", GlbStaff.CurrentUser.GS_Code, action2.LTA_SystemLastEditUser);
				AssertEquals("LTA_SystemLastEditTimeUtc", ZDateTime.UtcNow, action2.LTA_SystemLastEditTimeUtc);
			});
		}

		public void TestAction_ShouldNotAdded_WhenAddressHasNone_AndAddressTypeIsMLT()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var addressDataObject = UniversalTestHelper.CreateInstructionDataObject(ConsignmentAddressTypes.Codes.Delivery);
			addressDataObject.SetInstructionPackingLineLinkCollection(() => null);
			addressDataObject.Type = new CodeDescriptionPair { Code = ConsignmentAddressTypes.Codes.Multi };

			var reader1 = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var consignmentAddress1 = reader1.ReadIntoBusinessObject();

			AssertEquals(0, consignmentAddress1.Actions.Count);
		}

		public void TestActionsForExistingAddress()
		{
			var consignment = Helper.CreateConsignment("CS001");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var deliveryAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageImporter) };
			var deliveryConsignmentAddressDataObject = UniversalTestHelper.CreateInstructionDataObject(ConsignmentAddressTypes.Codes.Delivery, deliveryAddress);
			var action = UniversalTestHelper.AddDivotConfirmation(deliveryConsignmentAddressDataObject, ActionTypes.Codes.Delivery);
			action.Reference = "SDELREF";
			var reader = new DtbConsignmentAddressDataObjectReader(deliveryConsignmentAddressDataObject, Logger, Factory, consignment);
			var consignmentAddress = reader.ReadIntoBusinessObject();
			AssertEquals("Should have found existing Delivery ConsignmentAddress.", consignment.DeliveryAddress, consignmentAddress);
			AssertEquals("Should not have read in new Actions.", 1, consignmentAddress.Actions.Count);
			AssertEquals("Should not have read in new Actions.", ConsignmentAddressTypes.Codes.Delivery, consignmentAddress.Actions.Single().LTA_ActionType);
		}

		public void TestActionsIgnoresConNotes()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var consignmentAddressDataObject = UniversalTestHelper.CreateInstructionDataObject(ConsignmentAddressTypes.Codes.Delivery);
			var consignmentActionDataObject = UniversalTestHelper.AddDivotConfirmation(consignmentAddressDataObject, ActionTypes.Codes.ConNoteNo);
			consignmentActionDataObject.Reference = "123";
			var reader = new DtbConsignmentAddressDataObjectReader(consignmentAddressDataObject, Logger, Factory, consignment);
			var consignmentAddress = reader.ReadIntoBusinessObject();
			AssertEquals("ConsignmentAddress should not read in Connote actions.", 0, consignmentAddress.Actions.Count(c => c.LTA_ActionType == ActionTypes.Codes.ConNoteNo));
		}

		public void TestDomesticZoneIsPopulatedCorrectly()
		{
			var zoneProvider = Helper.CreateZoneRateProvider("AU");
			var zone = Helper.CreateZoneWithPostCodes("Zone1", zoneProvider, "2030", "2040");
			var consignment = Helper.CreateConsignment("CS001");
			var pickUpAddress = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.LocalCartageExporter));
			new OrganisationDataObjectReader(pickUpAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();
			var addressDataObject = new Instruction { Address = pickUpAddress, Type = new CodeDescriptionPair { Code = ConsignmentAddressTypes.Codes.PickUp } };
			var reader1 = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var address = reader1.ReadIntoBusinessObject();
			AssertNotNull(address.LTS_TZ_DomesticZone);
			AssertEquals(address.LTS_TZ_DomesticZone, zone.PK);
		}

		public void TestDomesticZoneIsEmpty()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var pickUpAddress = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.LocalCartageExporter));
			new OrganisationDataObjectReader(pickUpAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();
			var addressDataObject = new Instruction { Address = pickUpAddress, Type = new CodeDescriptionPair { Code = ConsignmentAddressTypes.Codes.PickUp } };
			var reader1 = GetNewReader(addressDataObject, Logger, Factory, consignment);
			var address = reader1.ReadIntoBusinessObject();
			AssertEquals(address.LTS_TZ_DomesticZone, Guid.Empty);
		}

		DtbConsignmentAddressDataObjectReader GetNewReader(Instruction addressDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbConsignment consignment)
		{
			return new DtbConsignmentAddressDataObjectReader(addressDataObject, logger, factory, consignment);
		}

		TransportConsignmentTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new TransportConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportConsignmentTestHelper helper;
	}
}
