using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Testing
{
	public abstract class DtbTransportInstructionDataObjectReaderTest<TInstruction> : OrganizationAddressTestHelper
			where TInstruction : DtbTransportInstruction
	{
		#region TestBasicFieldMappings

		public void TestBasicFieldMappings()
		{
			new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageCFS), new TestErrorLogger(), Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_ShortCode = "TRUCK";

			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);
			instructionDataObject.Address = GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageCFS);
			instructionDataObject.DropMode = new DropMode { Code = "ASK" };
			instructionDataObject.Equipment = "TRUCK";
			instructionDataObject.IsContainerRateable = true;
			instructionDataObject.IsAuthorisedToLeave = true;
			instructionDataObject.IsLooseRateable = true;
			instructionDataObject.Sequence = 10;
			instructionDataObject.ServiceInstruction = "Service Instruction";
			instructionDataObject.Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.Multi };

			var transport = GetNewTransport();
			var reader = GetNewReader(instructionDataObject, Logger, Factory, transport);
			var instruction = reader.ReadIntoBusinessObject();
			instruction.DocAddresses.Load(); // Have to reload into memory to reflect the delete + Add in Reader

			CombineAssertions(() =>
			{
				AssertJobDocAddressContentMatches_CRAHOLSYDWithoutGovRegNumAndType(instruction.Address);
				AssertEquals("instruction.KN_DropMode", "ASK", instruction.KN_DropMode);
				AssertEquals("instruction.KN_InstructionType", InstructionTypes.Codes.Multi, instruction.KN_InstructionType);
				AssertEquals("instruction.KN_IsContainerRateable", true, instruction.KN_IsContainerRateable);
				AssertEquals("instruction.KN_IsLooseRateable", true, instruction.KN_IsLooseRateable);
				AssertEquals("instruction.KN_IsAuthorisedToLeave", true, instruction.KN_IsAuthorisedToLeave);
				AssertEquals("instruction.KN_RQ_Equipment", equipment.PK, instruction.KN_RQ_Equipment);
				AssertEquals("instruction.KN_Sequence", ShouldPopulateSequence ? 10 : 0, instruction.KN_Sequence);
				AssertEquals("instruction.KN_ServiceInstruction", "Service Instruction", instruction.KN_ServiceInstruction);
			});
		}

		protected virtual bool ShouldPopulateSequence
		{
			get { return true; }
		}

		#endregion

		#region TestAddress

		public void TestAddress()
		{
			var transport = GetNewTransport();
			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageExporter) } };
			var reader1 = GetNewReader(instructionDataObject, Logger, Factory, transport);
			var instruction1 = reader1.ReadIntoBusinessObject();
			AssertEquals(OrganisationTypesList.Codes.CNR, instruction1.OrganisationType);
			instruction1.KN_KM_BookingMovement = transport.GetValue(DtbBookingSchema.PK);
			instruction1.KN_Sequence = 1; // this would have been done by the Transport Reader
			Factory.SaveForTesting();

			var instructionInOtherFactory = new BusinessObjectFactory().Load<TInstruction>(instruction1.PK);
			AssertEquals("Should save JobDocAddress to Database.", instruction1.Address.PK, instructionInOtherFactory.Address.PK);
			AssertEquals(true, instruction1.Address.IsEmpty);

			instructionDataObject.Address.Address1 = "Some Street";
			instructionDataObject.Address.CompanyName = "Some Co";
			instructionDataObject.Address.State = "Some State";
			instructionDataObject.Address.City = "Some City";
			var reader2 = GetNewReader(instructionDataObject, Logger, Factory, transport);
			var instruction2 = reader2.ReadIntoBusinessObject();
			instruction2.KN_KM_BookingMovement = transport.GetValue(DtbBookingSchema.PK);
			instruction2.KN_Sequence = 1; // this would have been done by the Transport Reader
			AssertEquals(OrganisationTypesList.Codes.CNR, instruction2.OrganisationType);
			AssertEquals(true, instruction2.Address.E2_AddressOverride);
			AssertEquals("Some Street", instruction2.Address.E2_Address1);
			AssertEquals("Some City", instruction2.Address.E2_City);
			AssertEquals("Some Co", instruction2.Address.E2_CompanyName);
			AssertEquals("Some State", instruction2.Address.E2_State);
			Factory.SaveForTesting();

			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, instruction2.PK);
			AssertEquals("Should only have one DocAddress.", 1, Factory.BOFactory.GetDatabaseCount(typeof(JobDocAddress), query));
		}

		#endregion

		#region TestAddress_IsProperlyDeleted

		public void TestAddress_IsProperlyDeleted()
		{
			var transport = GetNewTransport();
			var pickUpAddress = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.LocalCartageExporter));
			new OrganisationDataObjectReader(pickUpAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = pickUpAddress, Type = new CodeDescriptionPair { Code = InstructionTypes.Codes.PickUp } };
			var reader1 = GetNewReader(instructionDataObject, Logger, Factory, transport);
			var instruction1 = reader1.ReadIntoBusinessObject();
			AssertEquals(OrganisationTypesList.Codes.CNR, instruction1.OrganisationType);
			AssertAddressContentMatches_CRAHOLSYD(instruction1.Address.Address);

			instruction1.KN_KM_BookingMovement = transport.GetValue(DtbBookingSchema.PK);
			instruction1.KN_Sequence = 1; // this would have been done by the Transport Reader
			Factory.SaveForTesting();

			var newPickUpAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.LocalCartageExporter));
			new OrganisationDataObjectReader(newPickUpAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var newfactory1 = new UniversalObjectFactory();
			var transportInNewFactory = (DtbTransport)newfactory1.Load(transport.GetType(), transport.PK);
			var query1 = new ZQuery(JobDocAddressSchema.E2_ParentID, instruction1.PK);
			var poke = newfactory1.Load<JobDocAddress>(query1); // load the bizO into the factory to mix the rows and bizOs

			instructionDataObject.Address = newPickUpAddress;
			var reader2 = GetNewReader(instructionDataObject, Logger, newfactory1, transportInNewFactory);
			var instruction2 = reader2.ReadIntoBusinessObject();
			AssertEquals(OrganisationTypesList.Codes.CNR, instruction2.OrganisationType);
			AssertAddressContentMatches_INTHEMSYD(instruction2.Address.Address);
			instruction2.KN_KM_BookingMovement = transport.GetValue(DtbBookingSchema.PK);
			instruction2.KN_Sequence = 1; // this would have been done by the Transport Reader
			newfactory1.SaveForTesting();

			var newfactory2 = new UniversalObjectFactory();
			var query2 = new ZQuery(JobDocAddressSchema.E2_ParentID, instruction2.PK);
			var docAddresses = newfactory2.Load<JobDocAddress>(query2);
			AssertEquals("Should only have one DocAddress.", 1, docAddresses.Length);
		}

		#endregion

		#region TestAddress_DoesNotUseUnmatchedOrgWhenAddressIsEmpty

		public void TestAddress_DoesNotUseUnmatchedOrgWhenAddressIsEmpty()
		{
			SetUseUnmatchedOrganisationForMatchingRegistry(true);

			var transport = GetNewTransport();
			var deliveryAddress = new OrganizationAddress { AddressType = nameof(DocAddressType.LocalCartageImporter) };
			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = deliveryAddress };
			var reader = GetNewReader(instructionDataObject, Logger, Factory, transport);
			var instruction = reader.ReadIntoBusinessObject();

			AssertEquals("Instruction Address should be empty.", CargoWise.Types.ZGuid.Empty, instruction.Address.E2_OA_Address);
			AssertEquals("Instruction Address should be empty.", false, instruction.Address.E2_AddressOverride);
			AssertEquals("Instruction should have correct Address Type.", OrganisationTypesList.Codes.CNE, instruction.OrganisationType);
		}

		#endregion

		#region TestUnmatchedOrgNotesAreStoredForUnmatchedInstructionAddresses

		public void TestUnmatchedOrgNotesAreStoredForUnmatchedInstructionAddresses()
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
			var transport = GetNewTransport();
			var initialUnmatchedOrganisationNotes = transport.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Precondition", 0, initialUnmatchedOrganisationNotes.Length);

			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);
			instructionDataObject.Address = GetNewAddressData_CRAHOLSYD(orgType);

			var reader = GetNewReader(instructionDataObject, Logger, Factory, transport);
			var instruction = reader.ReadIntoBusinessObject();
			instruction.DocAddresses.Load(); // Have to reload into memory to reflect the delete + Add in Reader
			AssertEquals(true, instruction.Address.Organisation.IsSystemDefinedOrganisation);

			var unmatchedOrganisationNotes = transport.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("Unmatched organisation note should have been created", 1, unmatchedOrganisationNotes.Length);

			AssertMultilineASCIIEquals("Note should be added to the booking", string.Format(@"Organisation Type: {0}
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

		#endregion

		#region Implementation

		protected abstract DtbTransport GetNewTransport();
		protected abstract DtbTransportInstructionDataObjectReader<TInstruction> GetNewReader(Instruction instructionDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbTransport transport);

		#endregion
	}
}
