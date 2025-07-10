using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonShipmentNotes))]
	sealed class CommonShipmentNotesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConsigorAndConsigneeNotes()
		{
			CommonShipmentTest.TestShipmentExposer shipment = Factory.New<CommonShipmentTest.TestShipmentExposer>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			OrgHeader consignor = Factory.New<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			OrgHeader consignee = Factory.New<OrgHeader>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignor Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignor Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignee Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignee Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			AssertEquals("Shipment is an Export, so should show Consignor Export Notes, and Consignee Import Notes ... therefore should only show 2 notes", 2, shipment.Notes.VisibleNotes.Count);
			string[] noteTexts = Array.ConvertAll(shipment.Notes.VisibleNotes.ToArray<StmNote>(), (n) => (string)n.ST_NoteDataAsText);
			AssertContainsExactElementsInAnyOrder(new string[] { "Consignor Pickup Instructions for Export", "Consignee Delivery Instructions for Import" }, noteTexts);

			shipment = Factory.New<CommonShipmentTest.TestShipmentExposer>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			consignor = Factory.New<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			consignee = Factory.New<OrgHeader>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignor Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignor Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Consignee Pickup Instructions for Export").ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Consignee Delivery Instructions for Import").ST_NoteContextDirection = nameof(StmNoteContextDirection.I);

			AssertEquals("Shipment is an Import, so should show Consignor Export Notes, and Consignee Import Notes ... therefore should only show 2 notes", 2, shipment.Notes.VisibleNotes.Count);
			noteTexts = Array.ConvertAll(shipment.Notes.VisibleNotes.ToArray<StmNote>(), (n) => (string)n.ST_NoteDataAsText);
			AssertContainsExactElementsInAnyOrder(new string[] { "Consignor Pickup Instructions for Export", "Consignee Delivery Instructions for Import" }, noteTexts);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CommonShipment>().Notes;
		}
	}
}
