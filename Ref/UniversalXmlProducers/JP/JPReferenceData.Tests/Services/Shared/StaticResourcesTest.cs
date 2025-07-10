using System;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class StaticResourcesTest
	{
		[Test]
		public void TestDefaultZZD_EndDate()
		{
			Assert.That(StaticResources.DefaultZZD_EndDate, Is.EqualTo(new DateTime(2079, 06, 06, 23, 59, 00)));
		}

		[Test]
		public void TestDefaultZZD_StartDate()
		{
			Assert.That(StaticResources.DefaultZZD_StartDate, Is.EqualTo(new DateTime(1900, 01, 01, 00, 00, 00)));
		}
	}
}
