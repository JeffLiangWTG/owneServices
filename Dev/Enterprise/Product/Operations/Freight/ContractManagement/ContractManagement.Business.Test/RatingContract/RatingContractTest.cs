using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(RatingContract))]
	internal class RatingContractTest : EnterpriseBusinessObjectTestCase
	{
		public RatingContractTest()
		{
		}

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
