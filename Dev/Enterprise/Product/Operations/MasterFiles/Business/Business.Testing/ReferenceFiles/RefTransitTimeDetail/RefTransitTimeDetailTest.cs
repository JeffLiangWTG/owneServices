using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefTransitTimeDetail))]
	public class RefTransitTimeDetailTest : EnterpriseBusinessObjectTestCase
	{
		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();
			var refTransitTimeDetail = factory.NewWithValidTestData<RefTransitTimeDetail>();
			factory.Save();

			return refTransitTimeDetail;
		}

		public void TestTransitDaysAndHours()
		{
			var transitTimeDetail = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail.TransitDays = 2;
			transitTimeDetail.TransitHours = 18;

			AssertEquals(66, transitTimeDetail.RTD_TransitHours);

			transitTimeDetail.RTD_TransitHours = 83;
			Factory.Save();

			transitTimeDetail = new BusinessObjectFactory().Load<RefTransitTimeDetail>(transitTimeDetail.PK);
			AssertEquals(3, transitTimeDetail.TransitDays);
			AssertEquals(11, transitTimeDetail.TransitHours);
		}

		public void TestArrivalTime()
		{
			var transitTimeDetail = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail.RTD_ArrivalTime = new ZDateTime(2022, 1, 10, 10, 2, 1);
			AssertEquals(new ZDateTime(1900, 1, 1, 10, 2, 0), transitTimeDetail.RTD_ArrivalTime);

			Factory.Save();

			transitTimeDetail = new BusinessObjectFactory().Load<RefTransitTimeDetail>(transitTimeDetail.PK);
			AssertEquals(new ZDateTime(1900, 1, 1, 10, 2, 0), transitTimeDetail.RTD_ArrivalTime);
		}

		public void TestDayOfWeek()
		{
			var transitTimeDetail = Factory.NewWithValidTestData<RefTransitTimeDetail>();
			transitTimeDetail.RTD_DayOfWeek = 1;
			AssertEquals(AutoDayOfWeekCodeList.Codes.Monday, transitTimeDetail.DayOfWeek);
			transitTimeDetail.RTD_DayOfWeek = 2;
			AssertEquals(AutoDayOfWeekCodeList.Codes.Tuesday, transitTimeDetail.DayOfWeek);
			transitTimeDetail.RTD_DayOfWeek = 3;
			AssertEquals(AutoDayOfWeekCodeList.Codes.Wednesday, transitTimeDetail.DayOfWeek);
			transitTimeDetail.RTD_DayOfWeek = 4;
			AssertEquals(AutoDayOfWeekCodeList.Codes.Thursday, transitTimeDetail.DayOfWeek);
			transitTimeDetail.RTD_DayOfWeek = 5;
			AssertEquals(AutoDayOfWeekCodeList.Codes.Friday, transitTimeDetail.DayOfWeek);
			transitTimeDetail.RTD_DayOfWeek = 6;
			AssertEquals(AutoDayOfWeekCodeList.Codes.Saturday, transitTimeDetail.DayOfWeek);
			transitTimeDetail.RTD_DayOfWeek = 7;
			AssertEquals(AutoDayOfWeekCodeList.Codes.Sunday, transitTimeDetail.DayOfWeek);
			transitTimeDetail.RTD_DayOfWeek = 0;
			AssertEquals(string.Empty, transitTimeDetail.DayOfWeek);

			transitTimeDetail.DayOfWeek = AutoDayOfWeekCodeList.Codes.Monday;
			AssertEquals(transitTimeDetail.RTD_DayOfWeek, (ZByte)1);
			transitTimeDetail.DayOfWeek = AutoDayOfWeekCodeList.Codes.Tuesday;
			AssertEquals(transitTimeDetail.RTD_DayOfWeek, (ZByte)2);
			transitTimeDetail.DayOfWeek = AutoDayOfWeekCodeList.Codes.Wednesday;
			AssertEquals(transitTimeDetail.RTD_DayOfWeek, (ZByte)3);
			transitTimeDetail.DayOfWeek = AutoDayOfWeekCodeList.Codes.Thursday;
			AssertEquals(transitTimeDetail.RTD_DayOfWeek, (ZByte)4);
			transitTimeDetail.DayOfWeek = AutoDayOfWeekCodeList.Codes.Friday;
			AssertEquals(transitTimeDetail.RTD_DayOfWeek, (ZByte)5);
			transitTimeDetail.DayOfWeek = AutoDayOfWeekCodeList.Codes.Saturday;
			AssertEquals(transitTimeDetail.RTD_DayOfWeek, (ZByte)6);
			transitTimeDetail.DayOfWeek = AutoDayOfWeekCodeList.Codes.Sunday;
			AssertEquals(transitTimeDetail.RTD_DayOfWeek, (ZByte)7);
			transitTimeDetail.DayOfWeek = "AnyText";
			AssertEquals(transitTimeDetail.RTD_DayOfWeek, (ZByte)0);
		}

		public override (ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimes(string propertyName)
		{
			if (propertyName == "RTD_ArrivalTime")
			{
				return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
				{
					(new ZDateTime(1900, 1, 1, 8, 30, 0), new ZDateTime(1900, 1, 1, 8, 30, 0)),
					(new ZDateTime(1900, 1, 1, 23, 30, 0), new ZDateTime(1900, 1, 1, 23, 30, 0)),
					(new ZDateTime(1900, 1, 1, 0, 0, 0), new ZDateTime(1900, 1, 1, 0, 0, 0)),

					(ZDateTime.Invalid, ZDateTime.Invalid),
					(new ZDateTime(DateTime.MaxValue), new ZDateTime(1900, 1, 1, 23, 59, 0)),
					(new ZDateTime(DateTime.MinValue), new ZDateTime(DateTime.MinValue)),
					(ZDateTime.Empty, ZDateTime.Empty),
				};
			}
			else
			{
				return base.GetValidZDateTimes(propertyName);
			}
		}
	}
}
