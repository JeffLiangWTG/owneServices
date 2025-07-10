using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ProfitShareForwardingConsolWrapperCollection))]
	sealed class ProfitShareForwardingConsolWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProfitShareForwardingConsolWrapperCollection>
	{
		protected override ProfitShareForwardingConsolWrapperCollection GetCollectionToTest()
			=> new ProfitShareForwardingConsolWrapperCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new ProfitShareForwardingConsolWrapper(Factory.New<ForwardingConsol>());
	}
}
