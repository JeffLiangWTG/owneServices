using System;
using CargoWise.RefDbRepo.AsycudaReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Tests.Business;

[TestFixture]
sealed class NCExchangeRateProcessorTest
{
	[Test]
	public void TestGetExchangeRatesDownloadUrl()
	{
		var result = NCExchangeRateProcessor.GetExchangeRatesDownloadUrl(new FakeTimeProvider());

		Assert.That(result.ToString(), Does.Contain("fevrier-2025"));
	}

	class FakeTimeProvider : TimeProvider
	{
		private readonly DateTimeOffset _specificDateTime = new(2025, 4, 20, 0, 0, 0, TimeZoneInfo.Utc.BaseUtcOffset);

		public override DateTimeOffset GetUtcNow() => _specificDateTime;
	}
}
