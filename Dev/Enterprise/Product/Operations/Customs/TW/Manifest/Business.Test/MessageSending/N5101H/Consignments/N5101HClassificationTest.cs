using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HClassificationTest : TestCaseWithFactory
	{
		public void TestClassification()
		{
			IClassification classification = new N5101HClassification("12345678", "A");
			AssertEquals("12345678", classification.ID);
			AssertEquals("A", classification.IdentificationTypeCode);
		}
	}
}
