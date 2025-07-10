using System;
using System.Net;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	class S8ServiceRequestManagerTest : TestCaseWithFactory
	{
		[TestDate(2020, 7, 24)]
		[TestUtcOffset(0, 0, 0)]
		public void TestIsSuppressed()
		{
			var manager = CreateManager();
			AssertEquals(false, manager.IsSuppressed());

			using (FreightDataRegistry.Instance.S8LoginSuppressionTimeoutInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			{
				manager.OnTimedOutRequest();
				AssertEquals(true, manager.IsSuppressed());
			}
		}

		public void TestOnSuccessfulRequest()
		{
			var manager = CreateManager();

			manager.OnSuccessfulRequest();
			AssertEquals(DateTime.MinValue, manager.SuppressUntilUtc);
		}

		[TestDate(2020, 7, 24)]
		[TestUtcOffset(0, 0, 0)]
		public void TestOnTimedOutRequest()
		{
			var manager = CreateManager();

			using (FreightDataRegistry.Instance.S8LoginSuppressionTimeoutInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
			{
				manager.OnTimedOutRequest();

				var suppressionUntilUtc = manager.SuppressUntilUtc;
				AssertEquals(new ZDateTime(2020, 7, 24, 0, 20, 0),
					new ZDateTime(suppressionUntilUtc.Year, suppressionUntilUtc.Month, suppressionUntilUtc.Day, suppressionUntilUtc.Hour, suppressionUntilUtc.Minute, 0));
			}
		}

		[TestDate(2020, 7, 24)]
		[TestUtcOffset(0, 0, 0)]
		public void TestHandleException()
		{
			var manager = CreateManager();

			using (FreightDataRegistry.Instance.S8LoginSuppressionTimeoutInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 20))
			{
				manager.HandleException(new WebException("The remote name could not be resolved"));

				var suppressionUntilUtc = manager.SuppressUntilUtc;
				AssertEquals(new ZDateTime(2020, 7, 24, 0, 20, 0),
					new ZDateTime(suppressionUntilUtc.Year, suppressionUntilUtc.Month, suppressionUntilUtc.Day, suppressionUntilUtc.Hour, suppressionUntilUtc.Minute, 0));
			}
		}

		#region Implementation

		S8ServiceRequestManager CreateManager()
		{
			return new S8ServiceRequestManager();
		}

		#endregion
	}
}
