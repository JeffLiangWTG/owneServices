using System;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class HttpClientUtilsTest
	{
		[Test]
		public void TestNew()
		{
			Assert.AreEqual(TimeSpan.FromMinutes(5), HttpClientUtils.New().Timeout);
			using (var http = new HttpClientHandler())
			{
				Assert.AreEqual(TimeSpan.FromMinutes(5), HttpClientUtils.New(http).Timeout);
			}
		}
	}
}
