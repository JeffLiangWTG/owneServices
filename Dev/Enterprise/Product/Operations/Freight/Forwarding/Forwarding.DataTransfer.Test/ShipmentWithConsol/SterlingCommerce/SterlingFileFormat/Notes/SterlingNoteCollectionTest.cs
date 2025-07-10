using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingNoteCollection))]
	class SterlingNoteCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SterlingNoteCollection>
	{
		#region Test Overrides

		protected override SterlingNoteCollection GetCollectionToTest()
		{
			SterlingCommerceConsolAndShipmentExporter master = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo());
			return new SterlingNoteCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SterlingNote();
		}

		#endregion

		public void TestSterlingNoteCollection()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.NotesNote note = shipment.Notes.AddNew();
			note = shipment.Notes.AddNew();
			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals(2, sterling.NoteInfo.Count);
		}
	}
}
