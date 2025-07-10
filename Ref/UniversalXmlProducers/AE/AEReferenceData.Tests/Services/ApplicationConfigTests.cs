using CargoWise.RefDbRepo.AEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AEReferenceData.Tests;

[TestFixture]
sealed class ApplicationConfigTests
{
	[Test]
	public void TestOutputFolder()
	{
		Assert.That(ApplicationConfig.OutputFolder, Is.EqualTo("..\\..\\UXmlFiles\\"));
	}

	[Test]
	public void TestInputFolder()
	{
		Assert.That(ApplicationConfig.InputFolder, Is.EqualTo("..\\..\\UXmlFiles\\AE\\"));
	}

	[Test]
	public void TestDubaiInputFile()
	{
		Assert.That(ApplicationConfig.DubaiRefDataInputFile, Is.EqualTo("..\\..\\UXmlFiles\\AE\\Dubai\\Declaration_List_of_Values.xlsx"));
	}
}
