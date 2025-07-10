using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingShipmentNoteDataObjectReaderTest : NoteDataObjectReaderTest
	{
		public void TestGetNewBusinessObjectIsForwardingShipmentStmNote()
		{
			var noteDataObject = SetupNote();
			noteDataObject.NoteText = "Feee-lix the cat, what a wonderful-wonderful cat.";

			var reader = new ForwardingShipmentNoteDataObjectReader(noteDataObject, Logger, Factory, Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>(), data => null);
			Assert(reader.ReadIntoBusinessObject() is ForwardingShipmentStmNote);
		}
	}
}
