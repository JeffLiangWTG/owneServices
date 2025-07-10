using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(RateFormulaComparer))]
	sealed class RateFormulaComparerTest : TestCaseWithFactory
	{
		public void TestComparison()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			entryLine.CL_CustomsValue = 22m;
			var universalRateCalcData = new EntryLineUniversalRate(entryLine);
			IComparer<ZString> comparer = new RateFormulaComparer(universalRateCalcData);
			CombineAssertions(() =>
			{
				AssertEquals("both are equal", 0, comparer.Compare("0.15 * VFD", "0.15 * VFD"));

				entryLine.CL_CustomsValue = 0m;
				AssertEquals("formula one is zero", -1, comparer.Compare("0", "0.15 * VFD"));
				AssertEquals("formula two is zero", 1, comparer.Compare("0.15 * VFD", "0"));

				entryLine.CL_CustomsValue = 22m;
				AssertEquals("formula one is cheaper", -1, comparer.Compare("0.05 * VFD", "0.15 * VFD"));
				AssertEquals("formula two is cheaper", 1, comparer.Compare("0.15 * VFD", "0.05 * VFD"));
				AssertEquals("formulas evaluate to same", 0, comparer.Compare("0.15 * VFD", "(0.05 + 0.10) * VFD"));
			});
		}
	}
}
