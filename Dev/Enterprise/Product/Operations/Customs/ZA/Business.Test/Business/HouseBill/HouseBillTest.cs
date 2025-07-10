using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(Bill))]
	sealed class HouseBillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
	{
	}
}
