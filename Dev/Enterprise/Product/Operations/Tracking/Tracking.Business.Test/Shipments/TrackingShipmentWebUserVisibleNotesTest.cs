using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingShipmentWebUserVisibleNotesTest : IWebUserVisibleNotesSupportTest
	{
		public void TestShowAgentNotes()
		{
			TrackingShipment shipment = BizObj as TrackingShipment;
			AssertNotNull("Should be TrackingShipment", shipment);
			AssertNotNull("Should be logged in", shipment.LoggedInOrganisation);

			AssertNotEquals("Should not be a Delivery Agent", shipment.LoggedInOrganisation, shipment.DeliveryAgent);
			AssertNotEquals("Should not be a Tranship Agent", shipment.LoggedInOrganisation, shipment.TranshipAgent);
			AssertEquals("Should not show Agent Notes", false, shipment.ShowAgentNotes);

			shipment.JS_OH_DeliveryAgent = shipment.LoggedInOrganisation.PK;
			AssertEquals("Should show Agent Notes", true, shipment.ShowAgentNotes);

			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			shipment.JS_OH_TranshipAgent = shipment.LoggedInOrganisation.PK;
			AssertEquals("Should show Agent Notes", true, shipment.ShowAgentNotes);

			shipment.JS_OH_TranshipAgent = ZGuid.Empty;

			TrackingConsol consol = shipment.Consols.AddNew();
			AssertNotEquals("Should not be a Sending Forwarder", shipment.LoggedInOrganisation, consol.SendingForwarder);
			AssertNotEquals("Should not be a Receiving Forwarder", shipment.LoggedInOrganisation, consol.ReceivingForwarder);
			AssertEquals("Should not show Agent Notes", false, shipment.ShowAgentNotes);

			consol.SetDefaultSendingForwarderAddress(shipment.LoggedInOrganisation);
			AssertEquals("Should show Agent Notes", true, shipment.ShowAgentNotes);

			consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			consol.SetDefaultReceivingForwarderAddress(shipment.LoggedInOrganisation);
			AssertEquals("Should show Agent Notes", true, shipment.ShowAgentNotes);

			OrgContact user = shipment.LoggedInContact;
			shipment.SiteUser = null;
			AssertEquals("Should not show Agent Notes", false, shipment.ShowAgentNotes);

			shipment.AttachedOrders.AddNew();
			TrackingOrder secondOrder = shipment.AttachedOrders.AddNew();
			secondOrder.LoggedInContact = user;
			AssertEquals("Should show Agent Notes", true, shipment.ShowAgentNotes);
		}

		protected override IWebUserVisibleNotesSupport GetNewBusinessObject()
		{
			TestHelper helper = new TestHelper(Factory);
			TrackingShipment testShipment = helper.CreateShipment();
			testShipment.SiteUser = helper.TestSiteUser;
			return testShipment;
		}

		protected override void SetAgentNotesVisibility(IWebUserVisibleNotesSupport notesSupport, bool visibility)
		{
			TrackingShipment shipment = notesSupport as TrackingShipment;
			AssertNotNull("Should be TrackingShipment", shipment);
			Assert("SiteUser should be logged in", shipment.SiteUser.IsLoggedIn);
			if (visibility)
			{
				shipment.JS_OH_DeliveryAgent = shipment.LoggedInOrganisation.PK;
			}
			else
			{
				if (shipment.JS_OH_DeliveryAgent == shipment.LoggedInOrganisation.PK)
				{
					shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
				}

				if (shipment.JS_OH_TranshipAgent == shipment.LoggedInOrganisation.PK)
				{
					shipment.JS_OH_TranshipAgent = ZGuid.Empty;
				}
			}
		}

		public void TestNoteContextVisibility()
		{
			var shipment = BizObj as TrackingShipment;
			shipment.JS_TransportMode = "AIR";

			var airImportPublicNote = "Air Import Public Note";
			var note1 = shipment.Consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description, airImportPublicNote);
			note1.ST_NoteType = nameof(StmNoteVisibility.PUB);
			note1.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			note1.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			note1.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.I);

			var airExportPublicNote = "Air Export Public Note";
			var note2 = shipment.Consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description, airExportPublicNote);
			note2.ST_NoteType = nameof(StmNoteVisibility.PUB);
			note2.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			note2.ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			note2.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.I);

			var airExportInternalNote = "Air Export Internal Note";
			var note3 = shipment.Consignor.Notes.AddNew(false, PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description, airExportInternalNote);
			note3.ST_NoteType = nameof(StmNoteVisibility.INT);
			note3.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			note3.ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
			note3.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.I);

			AssertEquals("Three Notes", 3, shipment.Consignor.Notes.GetAllNotes().Count);
			AssertEquals("One VisibleNote", 1, shipment.NotesHelper.VisibleNotes.Count);
			AssertEquals("VisibleNote Description", PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description, shipment.NotesHelper.VisibleNotes[0].ST_Description);
			AssertEquals("VisibleNote Text", airExportPublicNote, shipment.NotesHelper.VisibleNotes[0].ST_NoteDataAsText);
		}

		public void TestNoteAreSortedChronologically()
		{
			var shipment = BizObj as TrackingShipment;
			shipment.JS_TransportMode = "AIR";

			CreateTestNotes(shipment, 10);
			var visibleNotes = shipment.NotesHelper.VisibleNotes;
			var visibleNotesSortedChronologically = shipment.NotesHelper.VisibleNotes.OrderBy(note => note.ST_CreatedDateUtc).ToArray();

			AssertEquals("10 VisibleNote", 10, visibleNotes.Count);
			AssertArrayEqualsByElements("Visible notes should be sorted chronologically", visibleNotesSortedChronologically, visibleNotes.ToArray());
		}

		void CreateTestNotes(TrackingShipment shipment, int numberOfNotesToCreate)
		{
			for (int i = 0; i < numberOfNotesToCreate; i++)
			{
				var note = shipment.Consignor.Notes.AddNew(true, "blah", "Air Export Public Note " + i);
				note.ST_NoteType = nameof(StmNoteVisibility.PUB);
				note.ST_NoteContextModule = nameof(StmNoteContextModule.A);
				note.ST_NoteContextDirection = nameof(StmNoteContextDirection.E);
				note.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.I);
				Factory.Save();
			}
		}

		protected override BusinessObject GetRelatedBusinessObject(IWebUserVisibleNotesSupport parent)
		{
			return null;//return ((TrackingShipment)Parent.NotesParentBO).DocsAndCartage.Cartages.AddNew();
		}
	}
}
