using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionHeaderSubAccount))]
	public class AccTransactionHeaderSubAccountTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccTransactionHeaderSubAccount>();
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
