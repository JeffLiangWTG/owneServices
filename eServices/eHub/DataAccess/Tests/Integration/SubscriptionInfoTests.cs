using System;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.Integration
{
	public class SubscriptionInfoTests
	{
		[Test]
		public void SubscriptionInfo_ConvertsFromSharedTypes()
		{
			Assert.Multiple(() =>
			{
				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.SubscriptionInfo)
						new eServices.eHubDataAccess.Integration.SubscriptionInfo
						{
							ID = Guid.NewGuid(),
							Type = "type",
							Provider = "provider",
							Subscriber = "subscriber",
							Value = "value",
							Reference = "reference",
							ReferenceType = "referenceType",
							Subscribed = DateTime.Now,
							Expiry = DateTime.Now.AddDays(1)
						});

				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.SubscriptionInfo)
						new eServices.eHubDataAccess.Integration.SubscriptionInfo
						{
							ID = Guid.Empty,
							Type = null,
							Provider = null,
							Subscriber = null,
							Value = null,
							Reference = null,
							ReferenceType = null,
							Subscribed = null,
							Expiry = null
						});
			});
		}
	}
}
