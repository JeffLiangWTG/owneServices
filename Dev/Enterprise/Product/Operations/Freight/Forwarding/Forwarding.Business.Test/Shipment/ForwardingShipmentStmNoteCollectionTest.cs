using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentStmNoteCollection))]
	sealed class ForwardingShipmentStmNoteCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadElements()
		{
			var note1 = Shipment.Notes.AddNew();

			var collection = new ForwardingShipmentStmNoteCollection(Shipment);
			collection.Load();

			Assert(collection.Contains(note1));
		}

		public void TestShowOriginalBillNotesType()
		{
			var note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			note.ST_NoteText = "test";

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var collection = new ForwardingShipmentStmNoteCollection(Shipment);
				collection.Load();

				Assert(collection.Contains(note));
			}
		}

		public void TestHideOriginalBillNotesType()
		{
			var note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			note.ST_NoteText = "test";

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = false, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var collection = new ForwardingShipmentStmNoteCollection(Shipment);
				collection.Load();

				Assert(!collection.Contains(note));
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ForwardingShipmentStmNoteCollection(Shipment);
		}

		ForwardingShipment Shipment => shipment ?? (shipment = Factory.New<ForwardingShipment>());
		ForwardingShipment shipment;

		#endregion
	}
}
