using System;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.ServiceClient.Tests
{
	[TestFixture]
	public class DateTimeWrapperTests
	{
		[Test]
		public void TestDateTimeWrapper()
		{
			var wrapper = new DateTimeWrapper();
			var expected = DateTime.UtcNow;
			var tolerance = TimeSpan.FromMilliseconds(5);

			var actual = wrapper.UtcNow;

			Assert.GreaterOrEqual(actual, expected);
			Assert.LessOrEqual(actual, expected.Add(tolerance));
		}
	}
}
