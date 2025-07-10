using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HREmailsCollection))]
	sealed class HREmailsCollectionTest : ActiveBusinessObjectCollectionTestCase<HREmailsCollection>
	{
		public override void TestAddNew()
		{
			Assert("Not Supported", true);
		}
	}
}
