using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class TransportDocumentProviderTest : DataProviderTestCase<TransportDocumentProvider>
{
	public void TestSequenceNumber() => AssertEquals(25, Provider.SequenceNumber);

	public void TestType() => AssertEquals("TT", Provider.Type);

	public void TestReferenceNumber() => AssertEquals("TestReferenceNumber", Provider.ReferenceNumber);

	protected override TransportDocumentProvider GetProvider() => new(DocumentTestIndex, DocumentTestType, DocumentTestReferenceNumber);

	const int DocumentTestIndex = 25;
	const string DocumentTestType = "TT";
	const string DocumentTestReferenceNumber = "TestReferenceNumber";
}

