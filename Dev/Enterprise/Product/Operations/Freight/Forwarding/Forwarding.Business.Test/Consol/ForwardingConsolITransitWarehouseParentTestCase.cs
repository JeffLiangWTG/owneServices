using Enterprise.Warehouse.Transit.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsol))]
	sealed class ForwardingConsolITransitWarehouseParentTestCase : ITransitWarehouseParentForConsolTestCase<ForwardingConsol>
	{
		protected override ForwardingConsol GetNewParent(string jobNumber)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = jobNumber;
			return consol;
		}

		protected override string GetParentTableCode()
		{
			return JobConsolSchema.Constants.Prefix;
		}
	}
}
