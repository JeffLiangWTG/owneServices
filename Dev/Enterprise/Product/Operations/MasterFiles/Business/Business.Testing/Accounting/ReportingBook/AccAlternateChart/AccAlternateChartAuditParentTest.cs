using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateChart))]
	public class AccAlternateChartAuditParentTest : AuditParentTest<AccAlternateChart>
	{
		protected override AccAlternateChart NewTestAuditParent()
		{
			return Factory.New<AccAlternateChart>();
		}
	}
}
