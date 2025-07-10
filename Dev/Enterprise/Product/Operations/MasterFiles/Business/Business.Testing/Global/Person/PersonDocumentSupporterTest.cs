using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PersonDocumentSupporter))]
	sealed class PersonDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<GlbPerson>();
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var person = Factory.New<GlbPerson>();
			AssertEquals(Env.Security.PersonIntelligenceCustomiseDocuments, person.DocumentSupporter.CustomisationSecurityCheckpoint);
		}
	}
}
