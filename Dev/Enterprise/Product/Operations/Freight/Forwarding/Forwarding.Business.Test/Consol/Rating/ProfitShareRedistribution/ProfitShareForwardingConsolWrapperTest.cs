using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ProfitShareForwardingConsolWrapper))]
	sealed class ProfitShareForwardingConsolWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ProfitShareForwardingConsolWrapper(Factory.New<ForwardingConsol>());
	}
}
