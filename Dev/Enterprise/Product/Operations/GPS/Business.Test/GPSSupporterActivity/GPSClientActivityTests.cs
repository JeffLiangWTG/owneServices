using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.GPS.Business.Testing
{
	[TestedType(typeof(GPSSupporterActivity))]
	sealed class GPSClientActivityTests : EnterpriseBusinessObjectTestCase
	{
		public void TestActivityTypeInfo()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			AssertEquals(ConcurrencyPolicy.Strict, activity.EN_ActivityTypeInfo.ConcurrencyPolicy);
		}

		public void TestActivityStatus()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			activity.EN_JU = ZGuid.NewZGuid();
			AssertEquals(" - overridden manually", activity.ActivityStatus);
		}

		public void TestReadOnly()
		{
			GPSSupporterActivity activity = Factory.NewWithValidTestData<GPSSupporterActivity>();
			AssertEquals("Activity should be read-only", true, activity.ReadOnly);
		}

		#region Flags

		public void TestIsInOutReplyMessage()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			AssertEquals(false, activity.IsInOutReplyMessage);

			activity.EN_ActivityInformation = "blablablabla";
			AssertEquals(false, activity.IsInOutReplyMessage);

			activity.EN_ActivityInformation = "blablapickuptimeinblabla";
			AssertEquals(true, activity.IsInOutReplyMessage);

			activity.EN_ActivityInformation = "blablapickuptimeoutblabla";
			AssertEquals(true, activity.IsInOutReplyMessage);

			activity.EN_ActivityInformation = "blablawaitpointtimeinblabla";
			AssertEquals(true, activity.IsInOutReplyMessage);

			activity.EN_ActivityInformation = "blablawaitpointtimeoutblabla";
			AssertEquals(true, activity.IsInOutReplyMessage);

			activity.EN_ActivityInformation = "blabladeliverytimeinblabla";
			AssertEquals(true, activity.IsInOutReplyMessage);

			activity.EN_ActivityInformation = "blabladeliverytimeoutblabla";
			AssertEquals(true, activity.IsInOutReplyMessage);
		}

		public void TestIsPickupInReplyMessage()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			AssertEquals(false, activity.IsPickupInReplyMessage);

			activity.EN_ActivityInformation = "blablapickuptimeinblabla";
			AssertEquals(true, activity.IsPickupInReplyMessage);

			activity.EN_ActivityInformation = "blablaPickupTimeInblabla";
			AssertEquals(true, activity.IsPickupInReplyMessage);
		}

		public void TestIsPickupOutReplyMessage()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			AssertEquals(false, activity.IsPickupOutReplyMessage);

			activity.EN_ActivityInformation = "blablapickuptimeoutblabla";
			AssertEquals(true, activity.IsPickupOutReplyMessage);

			activity.EN_ActivityInformation = "blablaPickupTimeOutblabla";
			AssertEquals(true, activity.IsPickupOutReplyMessage);
		}

		public void TestIsWaitPointInReplyMessage()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			AssertEquals(false, activity.IsWaitPointInReplyMessage);

			activity.EN_ActivityInformation = "blablawaitpointtimeinblabla";
			AssertEquals(true, activity.IsWaitPointInReplyMessage);

			activity.EN_ActivityInformation = "blablaWaitPointTimeInblabla";
			AssertEquals(true, activity.IsWaitPointInReplyMessage);
		}

		public void TestIsWaitPointOutReplyMessage()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			AssertEquals(false, activity.IsWaitPointOutReplyMessage);

			activity.EN_ActivityInformation = "blablawaitpointtimeoutblabla";
			AssertEquals(true, activity.IsWaitPointOutReplyMessage);

			activity.EN_ActivityInformation = "blablaWaitPointTimeOutblabla";
			AssertEquals(true, activity.IsWaitPointOutReplyMessage);
		}

		public void TestIsDeliveryInReplyMessage()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			AssertEquals(false, activity.IsDeliveryInReplyMessage);

			activity.EN_ActivityInformation = "blabladeliverytimeinblabla";
			AssertEquals(true, activity.IsDeliveryInReplyMessage);

			activity.EN_ActivityInformation = "blablaDeliveryTimeInblabla";
			AssertEquals(true, activity.IsDeliveryInReplyMessage);
		}

		public void TestIsDeliveryOutReplyMessage()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			AssertEquals(false, activity.IsDeliveryOutReplyMessage);

			activity.EN_ActivityInformation = "blabladeliverytimeoutblabla";
			AssertEquals(true, activity.IsDeliveryOutReplyMessage);

			activity.EN_ActivityInformation = "blablaDeliveryTimeOutblabla";
			AssertEquals(true, activity.IsDeliveryOutReplyMessage);
		}

		public void TestIsDeliveredToReplyMessage()
		{
			var activity = Factory.New<GPSSupporterActivity>();
			AssertEquals(false, activity.IsDeliveredToReplyMessage);

			activity.EN_ActivityInformation = "blabladelivered to:blabla";
			AssertEquals(true, activity.IsDeliveredToReplyMessage);

			activity.EN_ActivityInformation = "blablaDelivered To:blabla";
			AssertEquals(true, activity.IsDeliveredToReplyMessage);
		}

		#endregion
	}
}
