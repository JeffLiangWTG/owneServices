using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Testing
{
	[TestedType(typeof(HrlBalanceAffectingLog))]
	class HrlBalanceAffectingLogTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;
	}
}
