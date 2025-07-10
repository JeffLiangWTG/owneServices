using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunityStageProgress))]
	sealed class OrgOpportunityStageProgressTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2020, 5, 1, 12, 0, 0)]
		public void TestDaysInStage()
		{
			var newItem = Factory.New<OrgOpportunityStageProgress>();
			AssertEquals("Date Started not set", 0, newItem.DaysInStage);

			newItem.OSP_DateStarted = new ZDateTimeOffset(new DateTime(2020, 4, 1, 12, 0, 0));
			AssertEquals("Date Started set but no date completed", 30, newItem.DaysInStage);

			newItem.OSP_DateCompleted = new ZDateTimeOffset(new DateTime(2020, 4, 5, 12, 0, 0));
			AssertEquals("Date Completed set", 4, newItem.DaysInStage);
		}
	}
}
