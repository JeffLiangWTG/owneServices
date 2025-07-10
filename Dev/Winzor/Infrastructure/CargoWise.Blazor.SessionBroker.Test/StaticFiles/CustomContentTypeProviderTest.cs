using CargoWise.Blazor.SessionBroker.StaticFiles;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test.StaticFiles
{
	public class CustomContentTypeProviderTest
	{
		[TestCase(@"client.msix", "application/msix")]
		[TestCase(@"client.MSIX", "application/msix")]
		[TestCase(@"client.appinstaller", "application/appinstaller")]
		[TestCase(@"client.AppInstaller", "application/appinstaller")]
		[TestCase(@"CargoWise.msix", "application/msix")]
		[TestCase(@"CargowiseAppx.appinstaller", "application/appinstaller")]
		public void TestCustomContentTypeProviderShouldReturnCorrectType(string filePath, string expectedContentType)
		{
			var provider = new CustomContentTypeProvider();
			Assert.That(provider.TryGetContentType(filePath, out var contentType), Is.True);
			Assert.That(contentType, Is.EqualTo(expectedContentType));
		}
	}
}
