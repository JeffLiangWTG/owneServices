using CargoWise.EntityFramework;
using Enterprise.MailManager;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HREmails))]
	sealed class HREmailsTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return NewBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return NewBusinessObject(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = base.GetBusinessObjectForFetchForLoad() as HREmails;
			result.MI_Direction = MailDirection.Receive;
			return result;
		}

		BusinessObject NewBusinessObject(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<HREmails>();
			result.MI_Direction = MailDirection.Receive;
			return result;
		}

		[DeveloperOnlyTest]
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("Cannot save the factory for a view.", true);
		}
	}
}
