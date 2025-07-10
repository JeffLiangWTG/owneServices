using System.Diagnostics;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionMatchLink))]
	sealed class AccTransactionMatchLinkTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			AccTransactionMatchLink result = (AccTransactionMatchLink)base.GetNewBusinessObjectForDeleteTest(factory);
			result.AP_MatchGroupNum = "0123456789";

			return result;
		}

		public void TestHaveConstructorStackTrace()
		{
			IHaveConstructorStackTrace hasTrace = BusinessObject as IHaveConstructorStackTrace;
			AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);

			AssertNull("Should be no ConstructorStackTrace by default", hasTrace.ConstructorStackTrace);

			StackTrace trace = new StackTrace();
			hasTrace.ConstructorStackTrace = trace;

			AssertEquals("Should be assigned StackTrace", trace, hasTrace.ConstructorStackTrace);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectConstructorCallStackDetails).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				hasTrace = this.GetNewBusinessObject() as IHaveConstructorStackTrace;
				AssertNotNull("Should be IHaveConstructorStackTrace", hasTrace);
				AssertNotNull("Should have ConstructorStackTrace", hasTrace.ConstructorStackTrace);
				AssertContains("Trace should be as expected", trace.ToString(), hasTrace.ConstructorStackTrace.ToString());
			}
		}
	}
}
