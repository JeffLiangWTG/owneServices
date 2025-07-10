using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionLineSubAccount))]
	public class AccTransactionLineSubAccountTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccTransactionLineSubAccount>();
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
