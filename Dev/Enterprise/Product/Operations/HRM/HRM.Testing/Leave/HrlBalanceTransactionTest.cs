using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Testing
{
	[TestedType(typeof(HrlBalanceTransaction))]
	class HrlBalanceTransactionTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported() => false;
	}
}
