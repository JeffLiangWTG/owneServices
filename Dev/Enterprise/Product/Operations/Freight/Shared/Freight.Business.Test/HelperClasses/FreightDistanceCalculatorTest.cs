using System;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightDistanceCalculatorTest : TestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestNullCheckOnCtorForNotifications()
		{
			new FreightDistanceCalculator(new Consumer(), null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNullCheckOnCtorForConsumer()
		{
			new FreightDistanceCalculator(null, new NotificationBuffer());
		}
	}
}
