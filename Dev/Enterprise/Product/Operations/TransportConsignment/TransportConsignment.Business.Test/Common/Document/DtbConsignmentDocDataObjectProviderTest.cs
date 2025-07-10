using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbConsignmentDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestGetDocDataObject_FromDtbConsignment()
		{
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			var provider = new DtbConsignmentDocDataObjectProvider();
			var dataObject = provider.GetDocDataObject(consignment, DataContext.CMRConsignmentNote);

			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a CMRConsignmentNoteDocDataObjectCollection", typeof(CMRConsignmentNoteDocDataObjectCollection), dataObject.GetType());
		}

		public void TestGetDocDataObject_FromDtbConsignmentRunSheet()
		{
			var runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			var provider = new DtbConsignmentDocDataObjectProvider();
			var dataObject = provider.GetDocDataObject(runSheet, DataContext.CMRConsignmentNote);

			AssertNotNull("should return a data object", dataObject);
			AssertEquals("should return a CMRConsignmentNoteDocDataObjectCollection", typeof(CMRConsignmentNoteDocDataObjectCollection), dataObject.GetType());
		}
	}
}
