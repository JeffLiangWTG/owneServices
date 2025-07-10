using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(PhaseStatusFilterTypeList))]
sealed class PhaseStatusFilterTypeListTest : TestCaseWithFactory
{
	public void TestPhaseStatusFilterTypeList() => CombineAssertions(() =>
		AssertCodeDescriptionPairList(new PhaseStatusFilterTypeList(),
			("All", "All Entries"),
			("Any", "Any Entry")
		));
}
