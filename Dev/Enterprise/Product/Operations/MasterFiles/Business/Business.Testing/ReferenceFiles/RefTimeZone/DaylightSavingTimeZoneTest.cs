using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DaylightSavingTimeZone))]
	sealed class DaylightSavingTimeZoneTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			StartRules[0].R4_DaylightSavingDate = ZDateTime.Now;
			EndRules[0].R4_DaylightSavingDate = ZDateTime.Now;
			return DaylightSavingZone;
		}

		#region StartDateRules

		public void TestStartDateRules()
		{
			RefTimeZoneSet timeZoneSet2 = Factory.New<RefTimeZoneSet>();
			timeZoneSet2.HasDaylightSavings = true;

			AssertEquals("Should have no rules initially", 0, timeZoneSet2.DaylightSavingZone.StartDateRules.Count);
			timeZoneSet2.DaylightSavingZone.StartDateRules.AddNew();
			AssertEquals("Should have 1 rule", 1, timeZoneSet2.DaylightSavingZone.StartDateRules.Count);
			Assert(timeZoneSet2.DaylightSavingZone.IsRegisteredEditableChildObject(timeZoneSet2.DaylightSavingZone.StartDateRules));
		}

		#endregion

		#region EndDateRules

		public void TestEndDateRules()
		{
			RefTimeZoneSet timeZoneSet2 = Factory.New<RefTimeZoneSet>();
			timeZoneSet2.HasDaylightSavings = true;

			AssertEquals("Should have no rules initially", 0, timeZoneSet2.DaylightSavingZone.EndDateRules.Count);
			timeZoneSet2.DaylightSavingZone.EndDateRules.AddNew();
			AssertEquals("Should have 1 rule", 1, timeZoneSet2.DaylightSavingZone.EndDateRules.Count);
			Assert(timeZoneSet2.DaylightSavingZone.IsRegisteredEditableChildObject(timeZoneSet2.DaylightSavingZone.EndDateRules));
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			AssertNotNull(DaylightSavingZone);
			AssertEquals(1, DaylightSavingZone.StartDateRules.Count);
			AssertEquals(1, DaylightSavingZone.EndDateRules.Count);

			DaylightSavingZone.Delete();

			AssertEquals(0, DaylightSavingZone.StartDateRules.Count);
			AssertEquals(0, DaylightSavingZone.EndDateRules.Count);
			Assert(DaylightSavingZone.IsDeleted);
		}

		#endregion

		#region Implementation

		DaylightSavingTimeZone DaylightSavingZone;
		RefTimeZoneStartRuleCollection StartRules;
		RefTimeZoneEndRuleCollection EndRules;

		protected override void SetUp()
		{
			base.SetUp();

			DaylightSavingZone = Factory.New<DaylightSavingTimeZone>();
			StartRules = DaylightSavingZone.StartDateRules;
			EndRules = DaylightSavingZone.EndDateRules;
			StartRules.AddNew();
			EndRules.AddNew();

			StartRules[0].R4_FromYear = 2000;
			StartRules[0].R4_ToYear = 2005;
			EndRules[0].R4_FromYear = 2000;
			EndRules[0].R4_ToYear = 2005;
		}

		#endregion

	}
}
