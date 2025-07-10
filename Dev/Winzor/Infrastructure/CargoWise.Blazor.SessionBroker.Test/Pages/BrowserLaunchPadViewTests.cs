using System.Threading.Tasks;
using CargoWise.Blazor.Testing.Common;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test.Pages;

[TestFixture]
public class BrowserLaunchPadViewTests
{
	[Test]
	public async Task TestBrowserLaunchPage()
	{
		using var factory = new CustomWebApplicationFactory<Startup>();

		using var client = factory.CreateClient();
		var response = await client.GetAsync("/BrowserLaunchPad");
		string htmlContent = await response.Content.ReadAsStringAsync();

		var htmlDocument = new HtmlDocument();
		htmlDocument.LoadHtml(htmlContent);

		//verify the launch button able to launch client app
		var launchButtonElement = htmlDocument.DocumentNode.SelectSingleNode("//a[@class='button']");
		string launchButtonhrefValue = launchButtonElement?.GetAttributeValue("href", "");
		Assert.That(launchButtonhrefValue, Is.EqualTo($"cargowiseclient:{response.RequestMessage.RequestUri.Scheme}://{response.RequestMessage.RequestUri.Host}"));

		//metatag
		var metaElement = htmlDocument.DocumentNode.SelectSingleNode("//meta[@http-equiv='refresh']");
		string contentValue = metaElement?.GetAttributeValue("content", "");

		Assert.That(contentValue, Is.EqualTo($"1;URL='cargowiseclient:{response.RequestMessage.RequestUri.Scheme}://{response.RequestMessage.RequestUri.Host}'"));

		//verify download button able to Download MSIX
		var downloadButtonElement = htmlDocument.DocumentNode.SelectSingleNode("//a[@id='downloadButton']");
		string downloadButtonhrefValue = downloadButtonElement?.GetAttributeValue("href", "");

		// Assert that the href matches the expected MsixAppInstallerUrl
		Assert.That(downloadButtonhrefValue, Is.EqualTo($"{response.RequestMessage.RequestUri.Scheme}://{response.RequestMessage.RequestUri.Host}/_clientapi/installer/client.appinstaller"));
	}
}
