using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ISupportCriticalValidation))]
	sealed class ISupportCriticalValidationTest : IConflictWithCriticalFieldsTester
	{
		protected override IConflictWithCriticalFields GetNewBusinessObject()
		{
			return null;
		}
	}
}
