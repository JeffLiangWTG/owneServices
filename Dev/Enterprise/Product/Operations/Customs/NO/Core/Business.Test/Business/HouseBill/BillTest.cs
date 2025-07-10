using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(Bill))]
	sealed class BillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
	{
	}
}
