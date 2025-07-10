using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationDocument))]
	sealed class HRJobApplicationDocumentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var doc = Factory.New<HRJobApplicationDocument>();
			AssertEquals("RES", doc.HPD_Type);
		}
	}
}
