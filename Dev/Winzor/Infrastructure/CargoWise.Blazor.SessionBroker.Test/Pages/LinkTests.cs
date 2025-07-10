using System.Net;
using System.Threading.Tasks;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWise.Blazor.SessionBroker.Pages;
using CargoWiseNext.Infrastructure.Installations;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test.Pages;

[TestFixture]
public class LinkTests
{
	// Positive test cases (should return edient link)
	[TestCase("/link/SomeUserAction/SomeController/SomePK/", "edient:Command=SomeUserAction&ControllerID=SomeController&BusinessEntityPK=SomePK", HttpStatusCode.OK, true)]
	// Negative test cases (should return cargowiseclient link
	[TestCase("/link/SomeUserAction/SomeController", "cargowiseclient:http://localhost", HttpStatusCode.OK, false)]
	[TestCase("/link/SomeUserAction", "cargowiseclient:http://localhost", HttpStatusCode.OK, false)]
	[TestCase("/link", "cargowiseclient:http://localhost", HttpStatusCode.OK, false)]
	public async Task PageLink_TestView_200(string testPath, string expectedEdiEntScheme, HttpStatusCode expectedStatusCode, bool expectRedirect)
	{
		// Setup
		await using var factory = new CustomWebApplicationFactory<Startup>();

		// Act
		using var client = factory.CreateClient();
		var response = await client.GetAsync(testPath);

		// Assert
		string htmlContent = await response.Content.ReadAsStringAsync();
		var htmlDocument = new HtmlDocument();
		htmlDocument.LoadHtml(htmlContent);

		///   Assert - Response Code
		Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));

		//   Assert - Launch Button
		var launchButtonElement = htmlDocument.DocumentNode.SelectSingleNode("//a[@class='button']");
		string launchButtonHrefValue = launchButtonElement?.GetAttributeValue("href", "");
		Assert.That(launchButtonHrefValue, Is.EqualTo(expectedEdiEntScheme));

		//   Assert - Metatag
		var metaElement = htmlDocument.DocumentNode.SelectSingleNode("//meta[@http-equiv='refresh']");
		if (expectRedirect)
		{
			string contentValue = metaElement?.GetAttributeValue("content", "");
			Assert.That(contentValue, Is.EqualTo($"1;URL='{expectedEdiEntScheme}'"));
		} else {
			Assert.That(metaElement, Is.Null);
		}

		//   Assert - MSIX installer link
		var downloadButtonElement = htmlDocument.DocumentNode.SelectSingleNode("//a[@id='downloadButton']");
		string downloadButtonHrefValue = downloadButtonElement?.GetAttributeValue("href", "");
		Assert.That(downloadButtonHrefValue, Is.EqualTo($"{response.RequestMessage.RequestUri.Scheme}://{response.RequestMessage.RequestUri.Host}/_clientapi/installer/client.appinstaller"));
	}

	// Test the main parameters and combos
	[TestCase("SomeUserAction", "SomeController", "SomePK", "https://localhost:5001", null, "edient:Command=SomeUserAction&ControllerID=SomeController&BusinessEntityPK=SomePK&WebVersionLaunchURL=https%3a%2f%2flocalhost%3a5001")]
	[TestCase("SomeUserAction", "SomeController", "SomePK", null, "", "edient:Command=SomeUserAction&ControllerID=SomeController&BusinessEntityPK=SomePK")]
	[TestCase("SomeUserAction", "SomeController", "SomePK", null, null, "edient:Command=SomeUserAction&ControllerID=SomeController&BusinessEntityPK=SomePK")]
	[TestCase("SomeUserAction", "SomeController", "", null, null, "cargowiseclient:https://mySomething")]
	[TestCase("SomeUserAction", "SomeController", null, null, null, "cargowiseclient:https://mySomething")]
	[TestCase("SomeUserAction", "", null, null, null, "cargowiseclient:https://mySomething")]
	[TestCase("SomeUserAction", null, null, null, null, "cargowiseclient:https://mySomething")]
	[TestCase("", null, null, null, null, "cargowiseclient:https://mySomething")]
	[TestCase(null, null, null, null, null, "cargowiseclient:https://mySomething")]
	// Test the query string
	[TestCase("test", "test", "test", null, "?test1=1&test2=2", "edient:Command=test&ControllerID=test&BusinessEntityPK=test&test1=1&test2=2")]
	[TestCase("test", "test", "test", null, "?test1=1&", "edient:Command=test&ControllerID=test&BusinessEntityPK=test&test1=1")]
	[TestCase("test", "test", "test", null, "?test1=1", "edient:Command=test&ControllerID=test&BusinessEntityPK=test&test1=1")]
	[TestCase("test", "test", "test", null, "?test1=", "edient:Command=test&ControllerID=test&BusinessEntityPK=test&test1=")]
	[TestCase("test", "test", "test", null, "?test1", "edient:Command=test&ControllerID=test&BusinessEntityPK=test&=test1")]	// This is a bit of an odd one, but it is how HttpUtility.ParseQueryString considered keys with no values (as a value)
	[TestCase("test", "test", "test", null, "?", "edient:Command=test&ControllerID=test&BusinessEntityPK=test")]
	[TestCase("test", "test", "test", null, "", "edient:Command=test&ControllerID=test&BusinessEntityPK=test")]
	// Try some injection (XSS) attacks (values an keys), note can only set keys for the extras so some built in security for the defined keys
	[TestCase("\" onclick=\"alert('fail!')\"", "test", "test", null, null, "edient:Command=%22+onclick%3d%22alert(%27fail!%27)%22&ControllerID=test&BusinessEntityPK=test")]
	[TestCase("test", "test", "test", null, "?test1=\" onclick=\"alert('fail!')\"", "edient:Command=test&ControllerID=test&BusinessEntityPK=test&test1=%22+onclick%3d%22alert(%27fail!%27)%22")]
	[TestCase("test", "test", "test", null, "?\" onclick=\"alert('fail!')\"", "edient:Command=test&ControllerID=test&BusinessEntityPK=test&%22+onclick=%22alert(%27fail!%27)%22")]
	// Test of a real-world example https://svc-ediprod.wtg.zone/Services/link/ShowEditForm/WorkItem/939c1a24-89fa-498d-876d-6204a6e54c8e?lang=en-gb
	[TestCase("ShowEditForm", "WorkItem", "939c1a24-89fa-498d-876d-6204a6e54c8e", null, "?lang=en-gb", "edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=939c1a24-89fa-498d-876d-6204a6e54c8e&lang=en-gb")]
	public void PageLink_TestOnGet_Multi(string ua, string ctr, string pk, string launchUrl, string qs, string expectedEdient)
	{
		// Setup
		const string expectedMsixAppInstallerUrl = "https://mySomething/_clientapi/installer/client.appinstaller";

		var mockUrlHandlerProvider = new Mock<IUrlHandlerProvider>();
		mockUrlHandlerProvider.Setup(m => m.GetUrlHandler()).Returns("cargowiseclient");
		var registryAccessor = new Mock<IRegistryAccessor>();
		if (launchUrl != null)
		{
			registryAccessor.Setup(r => r.GetStringValue(Link.LaunchUrlRegistryName)).Returns(launchUrl);
		}

		var testPageModel = new Link(mockUrlHandlerProvider.Object, registryAccessor.Object);
		testPageModel.PageContext = new PageContext() { HttpContext = new DefaultHttpContext() };
		testPageModel.HttpContext.Request.Scheme = "https";
		testPageModel.HttpContext.Request.Host = new HostString("mySomething");
		testPageModel.HttpContext.Request.QueryString = new QueryString(qs);
		testPageModel.Command = ua;
		testPageModel.ControllerID = ctr;
		testPageModel.BusinessEntityPK = pk;

		// NOTE: Testing ModelState is not trivial as it depends on internal classes. The ASP.Net Core team consider including it in a test to be integration (and that you should just test the full stack).
		// So the following is just to trigger the ModelState.IsValid check so the remainder of the code works as expected.
		var expectedIsValid = !string.IsNullOrWhiteSpace(ua) && !string.IsNullOrWhiteSpace(ctr) && !string.IsNullOrWhiteSpace(pk);
		if (!expectedIsValid)
		{
			testPageModel.ModelState.AddModelError("Command", "Command, ControllerID and BusinessEntityPK are required");
		}

		// Act
		testPageModel.OnGet();

		// Assert
		Assert.That(testPageModel.ModelState.IsValid, Is.EqualTo(expectedIsValid));
		Assert.That(testPageModel.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
		Assert.That(testPageModel.ClientAppLaunchUri, Is.EqualTo(expectedEdient));
		Assert.That(testPageModel.MsixAppInstallerUrl, Is.EqualTo(expectedMsixAppInstallerUrl));
	}
}
