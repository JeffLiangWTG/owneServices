using System;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Tests
{
	[TestFixture]
	class CachedCusCodeAttributeProviderTest
	{
		[Test]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>(
				() => new CachedCusCodeAttributeProvider(
					errorBuilder: null,
					new Mock<IHttpClientHelper>().Object,
					validPublicationForTestFile));

			Assert.Throws<ArgumentNullException>(
				() => new CachedCusCodeAttributeProvider(
					new StringBuilder(),
					httpClientHelper: null,
					validPublicationForTestFile));
		}

		[Test]
		public void GetCusCodeAttribute_ReturnsYAttribute_WhenCodeAndDateMatch()
		{
			using (var zipStream = TestHelper.ReadManifestResourceContentAsStream(ZipResourceName))
			{
				var httpClientHelperMock = new Mock<IHttpClientHelper>();
				httpClientHelperMock
					.Setup(x => x.GetAsync(It.IsAny<string>()))
					.Returns(Task.FromResult(zipStream));

				ICusCodeAttributeProvider attributeProvider = new CachedCusCodeAttributeProvider(
					new StringBuilder(),
					httpClientHelperMock.Object,
					validPublicationForTestFile);

				var cusCodeAttribute = attributeProvider.GetCusCodeAttribute("0010001-6");
				Assert.That(cusCodeAttribute, Is.Not.Null);
				Assert.Multiple(() =>
				{
					Assert.That(cusCodeAttribute.ZZE_Value, Is.EqualTo("Y"));
					Assert.That(cusCodeAttribute.ZZE_ZXE_NKName, Is.EqualTo("CL016"));
				});
			}
		}

		[Test]
		public void GetCusCodeAttribute_ReturnsNAttribute_WhenCodeMatchesButDateDoesNot()
		{
			var invalidPublicationForTestFile = new DateTime(2020, 01, 01);

			using (var zipStream = TestHelper.ReadManifestResourceContentAsStream(ZipResourceName))
			{
				var httpClientHelperMock = new Mock<IHttpClientHelper>();
				httpClientHelperMock
					.Setup(x => x.GetAsync(It.IsAny<string>()))
					.Returns(Task.FromResult(zipStream));

				ICusCodeAttributeProvider attributeProvider = new CachedCusCodeAttributeProvider(
					new StringBuilder(),
					httpClientHelperMock.Object,
					invalidPublicationForTestFile);

				var cusCodeAttribute = attributeProvider.GetCusCodeAttribute("0010001-6");
				Assert.That(cusCodeAttribute, Is.Not.Null);
				Assert.Multiple(() =>
				{
					Assert.That(cusCodeAttribute.ZZE_Value, Is.EqualTo("N"));
					Assert.That(cusCodeAttribute.ZZE_ZXE_NKName, Is.EqualTo("CL016"));
				});
			}
		}

		[Test]
		public void GetCusCodeAttribute_ReturnsNAttribute_WhenCodeDoesNotMatch()
		{
			using (var zipStream = TestHelper.ReadManifestResourceContentAsStream(ZipResourceName))
			{
				var httpClientHelperMock = new Mock<IHttpClientHelper>();
				httpClientHelperMock
					.Setup(x => x.GetAsync(It.IsAny<string>()))
					.Returns(Task.FromResult(zipStream));

				ICusCodeAttributeProvider attributeProvider = new CachedCusCodeAttributeProvider(
					new StringBuilder(),
					httpClientHelperMock.Object,
					validPublicationForTestFile);

				var cusCodeAttribute = attributeProvider.GetCusCodeAttribute("XXXXXXX-X");
				Assert.That(cusCodeAttribute, Is.Not.Null);
				Assert.Multiple(() =>
				{
					Assert.That(cusCodeAttribute.ZZE_Value, Is.EqualTo("N"));
					Assert.That(cusCodeAttribute.ZZE_ZXE_NKName, Is.EqualTo("CL016"));
				});
			}
		}

		[Test]
		public void DataLoadingIsCached()
		{
			using (var zipStream = TestHelper.ReadManifestResourceContentAsStream(ZipResourceName))
			{
				var httpClientHelperMock = new Mock<IHttpClientHelper>();
				httpClientHelperMock
					.Setup(x => x.GetAsync(It.IsAny<string>()))
					.Returns(Task.FromResult(zipStream));

				ICusCodeAttributeProvider attributeProvider = new CachedCusCodeAttributeProvider(
					new StringBuilder(),
					httpClientHelperMock.Object,
					validPublicationForTestFile);

				_ = attributeProvider.GetCusCodeAttribute("XXXXXXX-X");
				_ = attributeProvider.GetCusCodeAttribute("YYYYYYY-Y");
				httpClientHelperMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once);
			}
		}

		const string ZipResourceName = "CargoWise.RefDbRepo.EUReferenceData.Tests.CUSNumbers.TestFiles.Input.RD_NCTS-P5_CUSCode.zip";
		readonly DateTime validPublicationForTestFile = new DateTime(2024, 12, 01);
	}
}
