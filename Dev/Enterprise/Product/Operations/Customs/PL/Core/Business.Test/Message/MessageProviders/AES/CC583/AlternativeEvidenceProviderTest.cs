using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;
namespace Enterprise.Customs.PL.Business.Testing;

sealed class AlternativeEvidenceProviderTest : DataProviderTestCase<AlternativeEvidenceProvider>
{
	public void TestSequenceNumber() => AssertEquals(1, Provider.SequenceNumber);

	public void TestType() => AssertEquals(TestEvidenceTypeFromRuleC0684, Provider.Type);

	public void TestTransportDocuments() => AssertType<TransportDocumentProvider>(Provider.TransportDocuments.Single());

	public void TestTransportDocuments_RuleC0684() => CombineAssertions(() =>
	{
		var evidenceTypesFromRuleC0684 = new[] { "11", "14", "15", "17" };
		foreach (var evidenceType in evidenceTypesFromRuleC0684)
		{
			testEvidence.EvidenceType = evidenceType;
			AssertEquals("Transport document added for evidence type from the list in rule C0684.", 1, GetProvider().TransportDocuments.Count);
		}

		testEvidence.EvidenceType = "12";
		AssertEquals("Transport document is not added for other evidence types.", 0, Provider.TransportDocuments.Count);
	});

	protected override AlternativeEvidenceProvider GetProvider() => new(testEvidence, TestEvidenceIndex);

	protected override void SetUp()
	{
		base.SetUp();

		var jobDeclaration = Factory.New<JobDeclaration>();
		var baseMessageSendingParent = new BaseMessageSendingObjectParent(jobDeclaration);
		testEvidence = new AlternativeEvidence(baseMessageSendingParent)
		{
			EvidenceType = TestEvidenceTypeFromRuleC0684,
			DocType = "DocType",
			Reference = "Reference"
		};
	}

	const int TestEvidenceIndex = 1;

	const string TestEvidenceTypeFromRuleC0684 = "11";

	AlternativeEvidence testEvidence;
}
