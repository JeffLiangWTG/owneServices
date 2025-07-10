using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingConsolNoteDataObjectReaderTest : NoteDataObjectReaderTest
	{
		public void TestGetNewBusinessObjectIsForwardingConsolStmNote()
		{
			var noteDataObject = SetupNote();
			noteDataObject.NoteText = "test note";

			var reader = new ForwardingConsolNoteDataObjectReader(noteDataObject, Logger, Factory, Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>(), data => null);
			Assert(reader.ReadIntoBusinessObject() is ForwardingConsolStmNote);
		}
	}
}
