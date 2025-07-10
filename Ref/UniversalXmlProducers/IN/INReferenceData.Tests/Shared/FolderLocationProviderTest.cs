using System;
using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class FolderLocationProviderTest
	{
		[Test]
		public void TestGetPdfDownloadFolder()
		{
			Assert.Multiple(() =>
			{
				Assert.That(LocationProvider.GetPdfDownloadFolder(null), Is.Null);
				Assert.That(LocationProvider.GetPdfDownloadFolder(new DateTime(2022, 3, 16)), Is.EqualTo("..\\..\\UXmlFiles\\INTariff\\Pdfs\\16.03.2022").NoClip);
			});
		}

		[Test]
		public void TestGetJsonCreationFolder()
		{
			Assert.Multiple(() =>
			{
				Assert.That(LocationProvider.GetJsonCreationFolder(null), Is.Null);
				Assert.That(LocationProvider.GetJsonCreationFolder(new DateTime(2022, 3, 16)), Is.EqualTo("..\\..\\UXmlFiles\\INTariff\\Jsons\\16.03.2022").NoClip);
			});
		}

		[Test]
		public void TestGetOutputTariffXmlFilePath()
		{
			Assert.That(LocationProvider.GetOutputTariffXmlFilePath("03", new DateTime(2022, 3, 16)), Is.EqualTo("..\\..\\UXmlFiles\\RefCusTariff_IN_CTH_Chapter03_20220316.xml").NoClip);
		}

		FolderLocationProvider LocationProvider => locationProvider ?? (locationProvider = new FolderLocationProvider());
		FolderLocationProvider locationProvider;
	}
}
