using System;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.RefDbService;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSPortData
{
	[TestFixture]
	class RefDataLoaderTest
	{
		[Test]
		public void GetQuery()
		{
			var loader1 = new RefDataLoader("http://singleslash", false);
			var loader2 = new RefDataLoader("http://singleslash/", false);

			var query = "abc?filter=test etc";

			var expected = new Uri("http://singleslash/abc?filter=test etc");

			Assert.That(loader1.GetQuery(query), Is.EqualTo(expected), "Base url without backslash");
			Assert.That(loader2.GetQuery(query), Is.EqualTo(expected), "Base url with backslash");
		}
	}
}
