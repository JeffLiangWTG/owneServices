using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsol))]
	sealed class ForwardingConsolRelatableActivityTest : RelatableActivityTestCase<ForwardingConsol>
	{
		protected override ForwardingConsol GetNewActivity()
		{
			return Factory.NewWithValidTestData<ForwardingConsol>();
		}
	}
}
