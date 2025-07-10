using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

[TestFixture]
class ExchangeRatesHelperTests
{
	public static IEnumerable<TestCaseData> FirstDayOfNextMonthTestData
	{
		get
		{
			yield return new TestCaseData(new DateTime(2019, 8, 18)) { ExpectedResult = new DateTime(2019, 9, 1), TestName = "{m} August->September 2019" };
			yield return new TestCaseData(new DateTime(2019, 9, 21)) { ExpectedResult = new DateTime(2019, 10, 1), TestName = "{m} September->October 2019" };
			yield return new TestCaseData(new DateTime(2019, 10, 12)) { ExpectedResult = new DateTime(2019, 11, 1), TestName = "{m} October->November 2019" };
			yield return new TestCaseData(new DateTime(2019, 11, 23)) { ExpectedResult = new DateTime(2019, 12, 1), TestName = "{m} November->December 2019" };
			yield return new TestCaseData(new DateTime(2019, 12, 9)) { ExpectedResult = new DateTime(2020, 1, 1), TestName = "{m} December->January 2020" };
			yield return new TestCaseData(new DateTime(2020, 1, 14)) { ExpectedResult = new DateTime(2020, 2, 1), TestName = "{m} January->February 2020" };
			yield return new TestCaseData(new DateTime(2020, 2, 3)) { ExpectedResult = new DateTime(2020, 3, 1), TestName = "{m} February->March 2020" };
			yield return new TestCaseData(new DateTime(2020, 3, 25)) { ExpectedResult = new DateTime(2020, 4, 1), TestName = "{m} March->April 2020" };
			yield return new TestCaseData(new DateTime(2020, 4, 27)) { ExpectedResult = new DateTime(2020, 5, 1), TestName = "{m} April->May 2020" };
			yield return new TestCaseData(new DateTime(2020, 5, 11)) { ExpectedResult = new DateTime(2020, 6, 1), TestName = "{m} May->June 2020" };
			yield return new TestCaseData(new DateTime(2020, 6, 15)) { ExpectedResult = new DateTime(2020, 7, 1), TestName = "{m} June->July 2020" };
			yield return new TestCaseData(new DateTime(2020, 7, 7)) { ExpectedResult = new DateTime(2020, 8, 1), TestName = "{m} July->August 2020" };
		}
	}

	[TestCaseSource(nameof(FirstDayOfNextMonthTestData))]
	public DateTime TestGetFirstDayOfNextMonth(DateTime randomDate) => randomDate.GetFirstDayOfNextMonth();

	public static IEnumerable<TestCaseData> LastDayOfNextMonthTestData
	{
		get
		{
			yield return new TestCaseData(new DateTime(2019, 8, 21)) { ExpectedResult = new DateTime(2019, 9, 30), TestName = "{m} August->September 2019" };
			yield return new TestCaseData(new DateTime(2019, 9, 18)) { ExpectedResult = new DateTime(2019, 10, 31), TestName = "{m} September->October 2019" };
			yield return new TestCaseData(new DateTime(2019, 10, 23)) { ExpectedResult = new DateTime(2019, 11, 30), TestName = "{m} October->November 2019" };
			yield return new TestCaseData(new DateTime(2019, 11, 20)) { ExpectedResult = new DateTime(2019, 12, 31), TestName = "{m} November->December 2019" };
			yield return new TestCaseData(new DateTime(2019, 12, 17)) { ExpectedResult = new DateTime(2020, 1, 31), TestName = "{m} December->January 2020" };
			yield return new TestCaseData(new DateTime(2020, 1, 22)) { ExpectedResult = new DateTime(2020, 2, 29), TestName = "{m} January->February 2020" };
			yield return new TestCaseData(new DateTime(2020, 2, 19)) { ExpectedResult = new DateTime(2020, 3, 31), TestName = "{m} February->March 2020" };
			yield return new TestCaseData(new DateTime(2020, 3, 14)) { ExpectedResult = new DateTime(2020, 4, 30), TestName = "{m} March->April 2020" };
			yield return new TestCaseData(new DateTime(2020, 4, 8)) { ExpectedResult = new DateTime(2020, 5, 31), TestName = "{m} April->May 2020" };
			yield return new TestCaseData(new DateTime(2020, 5, 29)) { ExpectedResult = new DateTime(2020, 6, 30), TestName = "{m} May->June 2020" };
			yield return new TestCaseData(new DateTime(2020, 6, 2)) { ExpectedResult = new DateTime(2020, 7, 31), TestName = "{m} June->July 2020" };
			yield return new TestCaseData(new DateTime(2020, 7, 10)) { ExpectedResult = new DateTime(2020, 8, 31), TestName = "{m} July->August 2020" };
		}
	}

	[TestCaseSource(nameof(LastDayOfNextMonthTestData))]
	public DateTime TestGetLastDayOfNextMonth(DateTime randomDate) => randomDate.GetLastDayOfNextMonth();

	public static IEnumerable<TestCaseData> PenultimateWednesdayTestData
	{
		get
		{
			yield return new TestCaseData(new DateTime(2019, 9, 12)) { ExpectedResult = new DateTime(2019, 8, 21), TestName = "{m} August 2019" };
			yield return new TestCaseData(new DateTime(2019, 10, 7)) { ExpectedResult = new DateTime(2019, 9, 18), TestName = "{m} September 2019" };
			yield return new TestCaseData(new DateTime(2019, 10, 28)) { ExpectedResult = new DateTime(2019, 10, 23), TestName = "{m} October 2019" };
			yield return new TestCaseData(new DateTime(2019, 11, 23)) { ExpectedResult = new DateTime(2019, 11, 20), TestName = "{m} November 2019" };
			yield return new TestCaseData(new DateTime(2019, 12, 20)) { ExpectedResult = new DateTime(2019, 12, 18), TestName = "{m} December 2019" };
			yield return new TestCaseData(new DateTime(2020, 1, 25)) { ExpectedResult = new DateTime(2020, 1, 22), TestName = "{m} January 2020" };
			yield return new TestCaseData(new DateTime(2020, 2, 27)) { ExpectedResult = new DateTime(2020, 2, 19), TestName = "{m} February 2020" };
			yield return new TestCaseData(new DateTime(2020, 3, 30)) { ExpectedResult = new DateTime(2020, 3, 18), TestName = "{m} March 2020" };
			yield return new TestCaseData(new DateTime(2020, 5, 8)) { ExpectedResult = new DateTime(2020, 4, 22), TestName = "{m} April 2020" };
			yield return new TestCaseData(new DateTime(2020, 6, 13)) { ExpectedResult = new DateTime(2020, 5, 20), TestName = "{m} May 2020" };
			yield return new TestCaseData(new DateTime(2020, 7, 5)) { ExpectedResult = new DateTime(2020, 6, 17), TestName = "{m} June 2020" };
			yield return new TestCaseData(new DateTime(2020, 8, 5)) { ExpectedResult = new DateTime(2020, 7, 22), TestName = "{m} July 2020" };
		}
	}

	[TestCaseSource(nameof(PenultimateWednesdayTestData))]
	public DateTime TestGetPenultimateWednesday(DateTime randomDate) => randomDate.GetLatestValidPenultimateWednesday();
}
