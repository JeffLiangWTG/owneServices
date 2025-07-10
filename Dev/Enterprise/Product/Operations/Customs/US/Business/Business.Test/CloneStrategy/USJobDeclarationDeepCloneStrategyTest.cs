using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USJobDeclarationDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestNotesShouldNotBeCloned()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var note = declaration.Notes.AddNew();
			note.ST_IsCustomDescription = true;
			note.ST_Description = "This is a test";
			note.ST_NoteDataAsText = "I am a test";

			Factory.Save();
			var clonedDeclaration = (BaseJobDeclaration)new USJobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			Assert("should not copy Notes", !(clonedDeclaration.Notes.GetAllNotes().Count > 0));
		}

		public void TestCopyInBondData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = "IMP";
			declaration.JE_CarrierCode = "AA";

			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;

			var inBondHeader = (CusInBondHeader)declaration.InBondHeader;
			var movementHeader = (CusInBondMoveHeader)inBondHeader.MovementHeaders.AddNew();
			movementHeader.BM_DestinationPortCode = "A1A1";
			var clonedDeclaration = (JobDeclaration)new USJobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			var clonedInBondHeader = (CusInBondHeader)clonedDeclaration.InBondHeader;
			var clonedMovementHeader = (CusInBondMoveHeader)clonedInBondHeader.MovementHeader;
			AssertEquals(declaration.JE_CarrierCode, clonedDeclaration.JE_CarrierCode);
			AssertEquals(movementHeader.BM_DestinationPortCode, clonedMovementHeader.BM_DestinationPortCode);
		}

		public void TestInBondDataNotCloned()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = "IMP";
			declaration.JE_CarrierCode = "AA";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration2.JE_MessageType = "IMP";
			declaration2.JE_CarrierCode = "AA";
			var header1 = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header1.BH_ParentID = declaration2.PK;
			header1.BH_ParentTableCode = declaration2.TablePrefix;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipmentHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			shipmentHeader.BH_ParentID = shipment.PK;
			shipmentHeader.BH_ParentTableCode = shipment.TablePrefix;
			declaration2.JE_JS = shipment.PK;
			var clonedDeclaration = (JobDeclaration)new USJobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			var clonedInBondHeader = (CusInBondHeader)clonedDeclaration.InBondHeader;
			AssertEquals(declaration.JE_CarrierCode, clonedDeclaration.JE_CarrierCode);
			AssertNull(clonedInBondHeader);

			var clonedDeclaration2 = (JobDeclaration)new USJobDeclarationDeepCloneStrategy(declaration2, CloneType.TemplateCopy).Clone();
			var clonedInBondHeader2 = (CusInBondHeader)clonedDeclaration2.InBondHeader;
			AssertEquals(declaration2.JE_CarrierCode, clonedDeclaration2.JE_CarrierCode);
			AssertNull(clonedInBondHeader2);
		}
	}
}
