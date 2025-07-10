using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitStmNoteHelperTest : TransitUniversalTestCase
	{
		#region TestConstructor_FactoryNotNull

		public void TestConstructor_FactoryNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitStmNoteHelper(Logger, null));
		}

		#endregion

		#region TestPopulateStmNote

		public void TestPopulateStmNote()
		{
			var referenceHelper = new WhsTransitStmNoteHelper(Logger, Factory);
			var parentPK = ZGuid.NewZGuid();
			referenceHelper.PopulateStmNote(parentPK, "TestTable", "TestText", "TestDescription", true);
			var stmNoteCreated = Factory.RowFactory.Load(StmNoteSchema.Constants.TableName, new ZQuery(StmNoteSchema.ST_ParentID, parentPK)).Single();
			var referenceRow = DataObjectReader.GetColumnIndexerFromRow(stmNoteCreated);
			AssertEquals("Reference must point to the parent.", parentPK, referenceRow.GetValue(StmNoteSchema.ST_ParentID));
			AssertEquals("Table must point be TestTable.", "TestTable", referenceRow.GetValue(StmNoteSchema.ST_Table));
			AssertEquals("Note Text must be set.", "TestText", referenceRow.GetValue(StmNoteSchema.ST_NoteText));
			AssertEquals("Note Description must be set.", "TestDescription", referenceRow.GetValue(StmNoteSchema.ST_Description));
			AssertEquals("Note Context must be set to default (AAA)", "AAA", referenceRow.GetValue(StmNoteSchema.ST_NoteContext));
			AssertEquals("Note Type must be set to default (INT)", nameof(StmNoteVisibility.INT), referenceRow.GetValue(StmNoteSchema.ST_NoteType));
		}

		#endregion

		#region TestGetSupportedNotesByVisibilityTypes

		public void TestGetSupportedNotesByVisibilityTypes()
		{
			var allNotes = new DataObjectList<Note>()
			{
				Helper.CreateNote("Client-Visible Note", "Client-Visible Note", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote("Private Note", "Private Note", StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive),
				Helper.CreateNote("Agent-Visible Note", "Agent-Visible Note", StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive),
				Helper.CreateNote("Internal Note", "Internal Note", StmNoteDescription.Int, StmNoteDescription.IntDescriptive)
			};
			var consignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			AssertEquals(0, WhsTransitStmNoteHelper.GetSupportedNotesByVisibilityTypes(consignment, allNotes).Count);

			AssertArrayEqualsByElements("Only include specifided visibility types.", new ZString[] {
				"Client-Visible Note" },
				WhsTransitStmNoteHelper.GetSupportedNotesByVisibilityTypes(consignment, allNotes, StmNoteDescription.Pub).Select(n => n.Description.Value).ToArray());

			AssertArrayEqualsByElements("Only include specifided visibility types.", new ZString[] {
				"Private Note" },
				WhsTransitStmNoteHelper.GetSupportedNotesByVisibilityTypes(consignment, allNotes, StmNoteDescription.Prv).Select(n => n.Description.Value).ToArray());

			AssertArrayEqualsByElements("Only include specifided visibility types.", new ZString[] {
				"Agent-Visible Note" },
				WhsTransitStmNoteHelper.GetSupportedNotesByVisibilityTypes(consignment, allNotes, StmNoteDescription.Agv).Select(n => n.Description.Value).ToArray());

			AssertArrayEqualsByElements("Only include specifided visibility types.", new ZString[] {
				"Internal Note" },
				WhsTransitStmNoteHelper.GetSupportedNotesByVisibilityTypes(consignment, allNotes, StmNoteDescription.Int).Select(n => n.Description.Value).ToArray());

			PredefinedNoteTypes.Instance.All.Append(new PredefinedNoteType(PredefinedNoteTypes.Instance.WebUserNote.MultilingualDescription, StmNoteVisibility.PUB, false, true, false, false));
			var readOnlyNotes = new DataObjectList<Note>()
			{
				Helper.CreateNote(PredefinedNoteTypes.Instance.WebUserNote.Description, "Non-supported ReadOnly Note", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, "Supported ReadOnly Note", StmNoteDescription.Int, StmNoteDescription.IntDescriptive),
				Helper.CreateNote(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description, "Supported ReadOnly Note", StmNoteDescription.Int, StmNoteDescription.IntDescriptive),
			};

			AssertArrayEqualsByElements("Non-supported readonly notes are filtered out.", new ZString[] {
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description },
				WhsTransitStmNoteHelper.GetSupportedNotesByVisibilityTypes(consignment, readOnlyNotes, StmNoteDescription.Pub, StmNoteDescription.Int).Select(n => n.Description.Value).ToArray());
		}

		#endregion

		#region Implementation

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
