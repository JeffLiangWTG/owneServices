using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class TaskDurationCalculatorTest : TransactionedTestCase
	{
		public void TestGetDurationFromTimeSpan()
		{
			AssertEquals((ZDateTime)TimeSpan.FromHours(1), TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(1, 0, 0)));
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(30), TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(0, 30, 0)));
			AssertEquals((ZDateTime)TimeSpan.FromHours(24), TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(24, 0, 0)));
			AssertEquals((ZDateTime)TimeSpan.FromDays(40), TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(40, 0, 0, 0)));
		}
	}
}
