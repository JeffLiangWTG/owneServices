using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionLines))]
	sealed class AccTransactionLinesSupportCriticalValidationTest : SupportCriticalValidationTestBase
	{
		protected override IConflictWithCriticalFields GetNewBusinessObject() => Factory.New<AccTransactionLines>();
	}
}
