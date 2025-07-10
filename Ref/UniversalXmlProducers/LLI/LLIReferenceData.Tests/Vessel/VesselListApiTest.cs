using System;
using System.Net.Http;
using CargoWise.RefDbRepo.LLIReferenceData.Services;
using CargoWise.RefDbRepo.LLIReferenceData.Services.Vessel;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.LLIReferenceData.Tests.Vessel;

[TestFixture]
class VesselListApiTest
{
	[SetUp]
	public void Setup()
	{
		ApplicationConfig.SetJsonConfigFile("CargoWise.RefDbRepo.LLIReferenceData.Tests.config.json");
	}

	[TearDown]
	public void TearDown()
	{
		ApplicationConfig.SetJsonConfigFile("CargoWise.RefDbRepo.LLIReferenceData.CmdLine.config.json");
	}

	[Test]
	public void InvalidURL()
	{
		var exception = Assert.ThrowsAsync<UriFormatException>(async () => await VesselListRetriever.GetVesselsDataAsync(string.Empty, string.Empty, 1));
		Assert.That(exception.Message, Is.EqualTo("Invalid URI: The format of the URI could not be determined."));
	}
}
