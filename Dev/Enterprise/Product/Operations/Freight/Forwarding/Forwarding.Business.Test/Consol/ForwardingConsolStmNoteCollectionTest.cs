using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolStmNoteCollection))]
	sealed class ForwardingConsolStmNoteCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadElements()
		{
			var note = Consol.Notes.AddNew();

			var collection = new ForwardingConsolStmNoteCollection(Consol);
			collection.Load();

			Assert(collection.Contains(note));
		}

		public void TestShowOriginalBillNotesType()
		{
			var note = Consol.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			note.ST_NoteText = "test";

			var boleroEBLConfiguration = new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = "096889EA-79B6-470C-AC9E-C18455EFC965", GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = "FCF7C512-2B3D-4D8D-B51A-A01F94382D24" };

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var collection = new ForwardingConsolStmNoteCollection(Consol);
				collection.Load();

				Assert(collection.Contains(note));
			}
		}

		public void TestHideOriginalBillNotesType()
		{
			var note = Consol.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			note.ST_NoteText = "test";

			var boleroEBLConfiguration = new BoleroEBLConfiguration() { EnableEBLIntegration = false };

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				var collection = new ForwardingConsolStmNoteCollection(Consol);
				collection.Load();

				Assert(!collection.Contains(note));
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest() => new ForwardingConsolStmNoteCollection(Consol);

		ForwardingConsol Consol => consol ?? (consol = Factory.New<ForwardingConsol>());
		ForwardingConsol consol;

		#endregion
	}
}
