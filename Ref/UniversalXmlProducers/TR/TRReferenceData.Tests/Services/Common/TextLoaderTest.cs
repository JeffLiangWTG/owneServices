using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Internal;
using CargoWise.RefDbRepo.TRReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Tests
{
    public class TextLoaderTest
    {
		[TestCase("http://abc.com")]
		[TestCase("https://abc.com")]
		public void TestNew_WithHttpOrHttpsScheme_ReturnsHttpLoader(string url)
		{
			var uri = new Uri(url);
			var loader = TextLoader.New(uri);

			Assert.IsInstanceOf<HttpLoader>(loader);
		}

		[Test]
		public void TestNew_WithFileScheme_ReturnsLocalFileLoader()
		{
			var filePath = Path.GetTempFileName();
			var fileUri = new Uri(filePath);

			var loader = TextLoader.New(fileUri);

			Assert.IsInstanceOf<LocalFileLoader>(loader);
		}

		[TestCase("ftp://example.com")]
		[TestCase("mailto:user@example.com")]
		public void TestNew_WithUnsupportedScheme_ThrowsException(string url)
		{
			var uri = new Uri(url);

			var ex = Assert.Throws<InvalidOperationException>(() => TextLoader.New(uri));
			Assert.That(ex.Message, Does.Contain("is not supported yet"));
		}
	}
}
