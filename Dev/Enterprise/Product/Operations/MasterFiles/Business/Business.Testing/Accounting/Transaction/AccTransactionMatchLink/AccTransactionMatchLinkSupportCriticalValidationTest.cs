using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionMatchLink))]
	sealed class AccTransactionMatchLinkSupportCriticalValidationTest : SupportCriticalValidationTestBase
	{
		protected override IConflictWithCriticalFields GetNewBusinessObject()
		{
			return Factory.New<AccTransactionMatchLink>();
		}
	}
}
