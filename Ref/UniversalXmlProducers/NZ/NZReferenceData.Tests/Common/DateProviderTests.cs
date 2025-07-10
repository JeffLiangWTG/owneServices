using System;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests.Common
{
	[TestFixture]
	class DateProviderTests
	{
		[Test]
		public void TestTodayShouldReturnCurrentDate()
		{
			var provider = new DateProvider();
			Assert.AreEqual(DateTime.Today, provider.Today);
		}

		[Test]
		public void TestActiveDateShouldUseConfiguredOffset()
		{
			ApplicationConfigTestHelper.SetApplicationConfigValue("ActiveDateMonthOffset", "12");
			var provider = new DateProvider();

			Assert.AreEqual(DateTime.Today.AddMonths(-12), provider.ActiveDate);

			ApplicationConfigTestHelper.ResetApplicationConfig();
		}
	}
}
