using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbWorkTime))]
	sealed class GlbWorkTimeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNoValidationErrorOnStartTime()
		{
			var workTime = Factory.NewWithValidTestData<GlbWorkTime>();
			workTime.GW_StartTime = GlbWorkTime.CreateTime(0, 0);
			workTime.Validation.ValidateAll();

			AssertNoErrors("Date 10 years in past doesn't give validation error.", workTime.GW_StartTimeInfo);
		}

		public void TestNoValidationErrorOnEndTime()
		{
			var workTime = Factory.NewWithValidTestData<GlbWorkTime>();
			workTime.GW_EndTime = GlbWorkTime.CreateTime(12, 0);
			workTime.Validation.ValidateAll();

			AssertNoErrors("Date 10 years in past doesn't give validation error.", workTime.GW_EndTimeInfo);
		}

		#region Implementation

		public override (ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimes(string propertyName)
		{
			if (propertyName == "GW_EndTime")
			{
				return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
				{
					(GlbWorkTime.CreateTime(8,30), new ZDateTime(1900, 1, 1, 8, 30, 0)),
					(GlbWorkTime.CreateTime(23,59, true), new ZDateTime(1900, 1, 2, 23, 59, 0)),
					(GlbWorkTime.CreateTime(0,0, true), new ZDateTime(1900, 1, 2, 0, 0, 0)),
					(GlbWorkTime.CreateTime(0,0), new ZDateTime(1900, 1, 1, 0, 0, 0)),
				};
			}
			else if (propertyName == "GW_StartTime")
			{
				return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
				{
					(GlbWorkTime.CreateTime(8,30), new ZDateTime(1900, 1, 1, 8, 30, 0)),
					(GlbWorkTime.CreateTime(23,59), new ZDateTime(1900, 1, 1, 23, 59, 0)),
					(GlbWorkTime.CreateTime(0,0), new ZDateTime(1900, 1, 1, 0, 0, 0)),
				};
			}
			else
			{
				return base.GetValidZDateTimes(propertyName);
			}
		}

		#endregion
	}
}
