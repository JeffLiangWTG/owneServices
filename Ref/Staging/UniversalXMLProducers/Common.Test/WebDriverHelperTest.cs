using System.IO;
using System.Web;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	[TestFixture]
	class WebDriverHelperTest
	{
		[Test]
		public void NavigateWebPageByXpath()
		{
			var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			var mainPageFile = Path.Combine(tempFolder, "WebDriverHelperMain.html");
			var detailPageFile = Path.Combine(tempFolder, "WebDriverHelperDetail.html");
			TestHelper.SimulateDownload(Path.Combine(tempFolder, "WebDriverHelperMain.html"), "CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test.TestFiles.WebDriverHelperMain.html");
			TestHelper.SimulateDownload(Path.Combine(tempFolder, "WebDriverHelperDetail.html"), "CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test.TestFiles.WebDriverHelperDetail.html");

			var detailPage = HttpUtility.HtmlDecode(File.ReadAllText(detailPageFile));
			var mainPage = HttpUtility.HtmlDecode(File.ReadAllText(mainPageFile));

			var webDriverHelperMock = new Mock<IWebDriverHelper>();
			webDriverHelperMock.Setup(x => x.GetWebPage(It.IsAny<string>(), It.IsAny<int>())).Returns(mainPage);

			webDriverHelperMock.Setup(x => x.NavigateWebPageByXpath("//*[contains(text(), 'Detail')]", 1)).Returns(detailPage);
			var resultPage = TestHelper.SimulateNavigateWebPageByXpath(webDriverHelperMock.Object, mainPageFile, "//*[contains(text(), 'DetailX')]");
			Assert.AreNotEqual(detailPage, resultPage);
			resultPage = TestHelper.SimulateNavigateWebPageByXpath(webDriverHelperMock.Object, mainPageFile, "//*[contains(text(), 'Detail')]");
			Assert.AreEqual(detailPage, resultPage);
		}

		[Test]
		public void TestAcceptAllCookiesForTaricWebSite()
		{
			var remoteWebDriverWrapper = new Mock<IRemoteWebDriverWrapper>();
			Mock<IWebElement> acceptAllCookiesButton = new Mock<IWebElement>();
			acceptAllCookiesButton.Setup(element => element.Displayed).Returns(true);
			acceptAllCookiesButton.Setup(element => element.Enabled).Returns(true);
			Mock<IWebElement> closeTextButton = new Mock<IWebElement>();
			closeTextButton.Setup(element => element.Displayed).Returns(false);
			closeTextButton.Setup(element => element.Enabled).Returns(false);
			remoteWebDriverWrapper.Setup(webDriver => webDriver.FindElementByLinkText("Accept all cookies")).Returns(acceptAllCookiesButton.Object);
			remoteWebDriverWrapper.Setup(webDriver => webDriver.FindElementByXPath("//*[contains(text(), 'Close')]")).Returns(closeTextButton.Object);
			using (var webDriverHelper = new WebDriverHelper(remoteWebDriverWrapper.Object))
			{
				webDriverHelper.AcceptCookiesForTaricWebSite("Accept all cookies");
				acceptAllCookiesButton.Verify(element => element.Click(), Times.Once, "Accept all cookies button should be clicked");
				closeTextButton.Verify(element => element.Click(), Times.Never, "close text button should not be clicked");
			}
		}
	}
}
