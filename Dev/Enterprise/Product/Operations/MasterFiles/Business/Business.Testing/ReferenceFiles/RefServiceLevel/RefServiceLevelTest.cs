using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefServiceLevel))]
	sealed class RefServiceLevelTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			AssertEquals(ServiceLevelDeliveryTypeList.Codes.DIFOT, serviceLevel.RS_ServiceDeliveryType);
		}

		public void TestServiceDeliveryPercentageReadOnlyAndZeroWhenGuarantee()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_ServiceDeliveryPercentage = 33;
			AssertEquals(33, serviceLevel.RS_ServiceDeliveryPercentage.ToZInt());
			AssertEquals(false, serviceLevel.RS_ServiceDeliveryPercentageInfo.ReadOnly);

			serviceLevel.RS_ServiceDeliveryType = ServiceLevelDeliveryTypeList.Codes.Guaranteed;
			AssertEquals(0, serviceLevel.RS_ServiceDeliveryPercentage.ToZInt());
			AssertEquals(true, serviceLevel.RS_ServiceDeliveryPercentageInfo.ReadOnly);

			serviceLevel.RS_ServiceDeliveryType = ServiceLevelDeliveryTypeList.Codes.DIFOT;
			AssertEquals(0, serviceLevel.RS_ServiceDeliveryPercentage.ToZInt());
			AssertEquals(false, serviceLevel.RS_ServiceDeliveryPercentageInfo.ReadOnly);
		}

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(RefServiceLevel)));
		}
		public void TestTransitTimeFormatted()
		{
			var serviceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
			serviceLevel.DefaultTransitDays = 1;
			serviceLevel.DefaultTransitHours = 15;

			AssertEquals("1 day 15 hours", serviceLevel.DefaultTransitTimeFormatted);

			serviceLevel.DefaultTransitDays = 4;
			serviceLevel.DefaultTransitHours = 1;

			AssertEquals("4 days 1 hour", serviceLevel.DefaultTransitTimeFormatted);
		}

		public void TestTransitDaysAndHours()
		{
			var serviceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
			serviceLevel.DefaultTransitDays = 2;
			serviceLevel.DefaultTransitHours = 18;

			AssertEquals(66, serviceLevel.RS_DefaultTransitHours);

			serviceLevel.RS_DefaultTransitHours = 83;
			Factory.Save();

			serviceLevel = new BusinessObjectFactory().Load<RefServiceLevel>(serviceLevel.PK);
			AssertEquals(3, serviceLevel.DefaultTransitDays);
			AssertEquals(11, serviceLevel.DefaultTransitHours);
		}

		public void TestDefaultArrivalTime()
		{
			var serviceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
			AssertNullOrEmpty(serviceLevel.DefaultArrivalTime);

			serviceLevel.RS_DefaultArrivalTime = new ZDateTime(2022, 1, 10, 10, 2, 1);
			AssertEquals(new ZDateTime(1900, 1, 1, 10, 2, 0), serviceLevel.RS_DefaultArrivalTime);
			AssertEquals("10:02", serviceLevel.DefaultArrivalTime);

			Factory.Save();

			serviceLevel = new BusinessObjectFactory().Load<RefServiceLevel>(serviceLevel.PK);
			AssertEquals(new ZDateTime(1900, 1, 1, 10, 2, 0), serviceLevel.RS_DefaultArrivalTime);
			AssertEquals("10:02", serviceLevel.DefaultArrivalTime);
		}

		public void TestDefaultDeliveryDueTime()
		{
			var serviceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
			AssertNullOrEmpty(serviceLevel.ServiceDeliveryDueTime);

			serviceLevel.RS_DefaultDeliveryDueTime = new ZDateTime(2022, 1, 10, 10, 2, 1);
			AssertEquals(new ZDateTime(1900, 1, 1, 10, 2, 0), serviceLevel.RS_DefaultDeliveryDueTime);
			AssertEquals("10:02", serviceLevel.ServiceDeliveryDueTime);

			Factory.Save();

			serviceLevel = new BusinessObjectFactory().Load<RefServiceLevel>(serviceLevel.PK);
			AssertEquals(new ZDateTime(1900, 1, 1, 10, 2, 0), serviceLevel.RS_DefaultDeliveryDueTime);
			AssertEquals("10:02", serviceLevel.ServiceDeliveryDueTime);
		}

		public void TestDeliverOnWeekend()
		{
			var serviceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
			serviceLevel.RS_DeliverOnSaturday = false;
			serviceLevel.RS_DeliverOnSunday = false;
			AssertEquals(false, serviceLevel.DeliverOnWeekend);

			serviceLevel.RS_DeliverOnSaturday = true;
			serviceLevel.RS_DeliverOnSunday = false;
			Assert(serviceLevel.DeliverOnWeekend);

			serviceLevel.RS_DeliverOnSaturday = false;
			serviceLevel.RS_DeliverOnSunday = true;
			Assert(serviceLevel.DeliverOnWeekend);

			serviceLevel.RS_DeliverOnSaturday = true;
			serviceLevel.RS_DeliverOnSunday = true;
			Assert(serviceLevel.DeliverOnWeekend);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		public override (ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimes(string propertyName)
		{
			if (propertyName == "RS_DefaultArrivalTime" || propertyName == "RS_DefaultDeliveryDueTime")
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
