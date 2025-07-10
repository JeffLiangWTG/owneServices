using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonConsolNotes))]
	sealed class CommonConsolNotesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingAndReceivingForwarersNotes()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			consol.SetDefaultSendingForwarderAddress(sendingForwarder);

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			consol.SetDefaultReceivingForwarderAddress(receivingForwarder);

			sendingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Sending Forwarder Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			sendingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Sending Forwarder Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			receivingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Receiving Forwarder Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			receivingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Receiving Forwarder Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			AssertEquals("Consol is an Export, so should show Sending Forwarder Export Notes, and Receiving Forwarder Import Notes ... therefore should only show 2 notes", 2, consol.Notes.VisibleNotes.Count);
			string[] noteTexts = Array.ConvertAll(consol.Notes.VisibleNotes.ToArray<StmNote>(), (n) => (string)n.ST_NoteDataAsText);
			AssertContainsExactElementsInAnyOrder(new string[] { "Sending Forwarder Pickup Instructions for Export", "Receiving Forwarder Delivery Instructions for Import" }, noteTexts);

			consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			sendingForwarder = Factory.New<OrgHeader>();
			consol.SetDefaultSendingForwarderAddress(sendingForwarder);

			receivingForwarder = Factory.New<OrgHeader>();
			consol.SetDefaultReceivingForwarderAddress(receivingForwarder);

			sendingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Sending Forwarder Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			sendingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Sending Forwarder Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			receivingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Receiving Forwarder Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			receivingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Receiving Forwarder Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			AssertEquals("Consol is an Import, so should show Sending Forwarder Export Notes, and Receiving Forwarder Import Notes ... therefore should only show 2 notes", 2, consol.Notes.VisibleNotes.Count);
			noteTexts = Array.ConvertAll(consol.Notes.VisibleNotes.ToArray<StmNote>(), (n) => (string)n.ST_NoteDataAsText);
			AssertContainsExactElementsInAnyOrder(new string[] { "Sending Forwarder Pickup Instructions for Export", "Receiving Forwarder Delivery Instructions for Import" }, noteTexts);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CommonConsol>().Notes;
		}
	}
}
