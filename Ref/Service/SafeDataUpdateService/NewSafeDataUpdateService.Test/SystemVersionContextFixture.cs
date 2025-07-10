using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class SystemVersionContextFixture
	{
		ISystemVersionContext systemVersionContext;

		[SetUp]
		public void SetUp()
		{
			systemVersionContext = new SystemVersionContext();
		}

		[Test]
		public void SetSystemVersion_ShouldStoreVersion()
		{
			const string expectedVersion = "2025-02-10T00:00:00Z";
			systemVersionContext.SystemVersionUTC = expectedVersion;
			Assert.That(systemVersionContext.SystemVersionUTC, Is.EqualTo(expectedVersion));
		}

		[Test]
		public void GetSystemVersion_WhenNotSet_ShouldReturnNull()
		{
			var result = systemVersionContext.SystemVersionUTC;
			Assert.That(result, Is.Null);
		}

		[Test]
		public void SetSystemVersion_ShouldOverridePreviousValue()
		{
			const string initialVersion = "2025-01-01T00:00:00Z";
			systemVersionContext.SystemVersionUTC = initialVersion;
			const string newVersion = "2025-02-10T00:00:00Z";
			systemVersionContext.SystemVersionUTC = newVersion;
			Assert.That(systemVersionContext.SystemVersionUTC, Is.EqualTo(newVersion));
		}
	}
}
