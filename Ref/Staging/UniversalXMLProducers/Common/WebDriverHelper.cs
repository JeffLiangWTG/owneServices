using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using CargoWise.RefDbRepo.Common.Argument;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public class WebDriverHelper : IWebDriverHelper
	{
		readonly IRemoteWebDriverWrapper webDriverWrapper;

		public WebDriverHelper()
		{
			var webDriver = GetChromeDriver();
			webDriverWrapper = new RemoteWebDriverWrapper(webDriver);
		}

#if DEBUG
		public WebDriverHelper(IRemoteWebDriverWrapper webDriverWrapper)
		{
			this.webDriverWrapper = webDriverWrapper;
		}
#endif

		public WebDriverHelper(string defaultFullPathDownloadDirectory)
		{
			var downloadPrefs = new Dictionary<string, object>
			{
				{ "default_directory", defaultFullPathDownloadDirectory },
				{ "directory_upgrade", true }
			};
			var options = new ChromeOptions();
			options.AddUserProfilePreference("download", downloadPrefs);
			var webDriver = GetChromeDriver(options);
			webDriverWrapper = new RemoteWebDriverWrapper(webDriver);
		}

		public string GetWebPageByLinkId(string id)
		{
			Argument.NotNullOrEmpty(id, nameof(id));
			var result = string.Empty;
			try
			{
				var element = webDriverWrapper.FindElementById(id);
				if (element.IsVisible())
				{
					element.Click();
					result = webDriverWrapper.PageSource;
				}
			}
			catch (NoSuchElementException)
			{
				result = "";
			}
			return result;
		}

		public string GetWebPageByLinkText(string linkText, int loadInterval = 0)
		{
			Argument.NotNullOrEmpty(linkText, nameof(linkText));
			var result = string.Empty;
			try
			{
				var element = webDriverWrapper.FindElementByLinkText(linkText);
				if (element.IsVisible())
				{
					var actions = new Actions(webDriverWrapper.WebDriver);
					actions.MoveToElement(element).Click().Build().Perform();
					if (loadInterval > 0)
					{
						Thread.Sleep(loadInterval * 1000);
					}
					result = webDriverWrapper.PageSource;
				}
			}
			catch (NoSuchElementException)
			{
				result = "";
			}
			return result;
		}

		public string GetElementTextByClassName(string className, string elementName)
		{
			Argument.NotNullOrEmpty(className, nameof(className));
			Argument.NotNullOrEmpty(elementName, nameof(elementName));
			var result = string.Empty;
			try
			{
				var elements = webDriverWrapper.FindElementsByClassName(className);
				IWebElement c = elements.Where(element => element.Text.Contains(elementName))?.FirstOrDefault();
				if (c != null)
				{
					result = c.Text;
				}
			}
			catch (NoSuchElementException)
			{
				result = "";
			}
			return result;
		}

		public string NavigateWebPageByXpath(string xpath, int loadInterval = 0)
		{
			Argument.NotNullOrEmpty(xpath, nameof(xpath));
			var result = string.Empty;
			try
			{
				var element = webDriverWrapper.FindElementByXPath(xpath);
				if (element.IsVisible())
				{
					element.Click();
					if (loadInterval > 0)
					{
						Thread.Sleep(loadInterval * 1000);
					}
					result = webDriverWrapper.PageSource;
				}
			}
			catch (NoSuchElementException)
			{
				result = "";
			}
			return result;
		}

		public KeyValuePair<string, string> GetDownloadFileInfoByPartialLinkText(string regex, string partialLinkText)
		{
			Argument.NotNullOrEmpty(regex, nameof(regex));
			Argument.NotNullOrEmpty(partialLinkText, nameof(partialLinkText));
			var result = GetDownloadFilesInfoByPartialLinkText(regex, partialLinkText, takeFirst: true);
			return result.Any() ? result.First() : new KeyValuePair<string, string>(string.Empty, string.Empty);
		}

		public IEnumerable<KeyValuePair<string, string>> GetDownloadFilesInfoByPartialLinkText(string regex, string partialLinkText)
		{
			Argument.NotNullOrEmpty(regex, nameof(regex));
			Argument.NotNullOrEmpty(partialLinkText, nameof(partialLinkText));
			return GetDownloadFilesInfoByPartialLinkText(regex, partialLinkText, takeFirst: false);
		}

		public string GetWebPage(string url, int loadInterval = 0)
		{
			Argument.NotNullOrEmpty(url, nameof(url));
			var navigate = webDriverWrapper.Navigate();
			navigate.GoToUrl(new Uri(url));

			if (loadInterval > 0)
			{
				Thread.Sleep(loadInterval * 1000);
			}

			return HttpUtility.HtmlDecode(webDriverWrapper.PageSource);
		}

		public void AcceptCookiesForTaricWebSite(string cookiesButtonText)
		{
			try
			{
				var cookiesElement = webDriverWrapper.FindElementByLinkText(cookiesButtonText);
				if (cookiesElement.IsVisible())
				{
					cookiesElement.Click();
					var closeElement = webDriverWrapper.FindElementByXPath("//*[contains(text(), 'Close')]");
					if (closeElement.IsVisible())
					{
						closeElement.Click();
					}
				}
			}
			catch (NoSuchElementException)
			{
			}
		}

		public void Back(int loadInterval = 0)
		{
			var navigate = webDriverWrapper.Navigate();
			navigate.Back();
			if (loadInterval > 0)
			{
				Thread.Sleep(loadInterval * 1000);
			}
		}

		static RemoteWebDriver GetChromeDriver(ChromeOptions options = null)
		{
			options = options ?? new ChromeOptions();
			options.AddArgument("--lang=en");
#pragma warning disable CA1416 // Validate platform compatibility, we call Registry.GetValue and this method is only available on windows
			var path = Microsoft.Win32.Registry.GetValue(
@"HKEY_CLASSES_ROOT\ChromeHTML\shell\open\command", null, null) as string;
#pragma warning restore CA1416 // Validate platform compatibility, we call Registry.GetValue and this method is only available on windows
			if (path != null)
			{
				var split = path.Split('\"');
				path = split.Length >= 2 ? split[1] : null;
			}
			options.BinaryLocation = path;
			var remoteAddress = CommonApplicationConfig.SeleniumServerURL;

			RemoteWebDriver driver;
			var timeOutInSeconds = GetChromeDriverTimeOutInSeconds();
			if (timeOutInSeconds.HasValue)
			{
				using (var defaultService = ChromeDriverService.CreateDefaultService())
				{
					var timeSpanValue = TimeSpan.FromSeconds(timeOutInSeconds.Value);
					driver = !string.IsNullOrEmpty(remoteAddress)
						? new RemoteWebDriver(new Uri(remoteAddress), options.ToCapabilities(), timeSpanValue)
						: new ChromeDriver(defaultService, options, timeSpanValue);
				}
			}
			else
			{
				driver = !string.IsNullOrEmpty(remoteAddress)
					? new RemoteWebDriver(new Uri(remoteAddress), options)
					: new ChromeDriver(options);
			}

			return driver;
		}

		static int? GetChromeDriverTimeOutInSeconds()
		{
			var configurationTimeOut = CommonApplicationConfig.SeleniumTimeOutInSeconds;
			if (configurationTimeOut != null)
			{
				return Convert.ToInt32(configurationTimeOut, CultureInfo.InvariantCulture);
			}
			return null;
		}

		public void Dispose()
		{
			webDriverWrapper.Dispose();
		}

		IEnumerable<KeyValuePair<string, string>> GetDownloadFilesInfoByPartialLinkText(string regex, string partialLinkText, bool takeFirst = false)
		{
			Argument.NotNull(regex, nameof(regex));

			var result = new List<KeyValuePair<string, string>>();
			var elements = webDriverWrapper.FindElementsByPartialLinkText(partialLinkText);
			foreach (var item in elements)
			{
				if (Regex.Match(item.Text, regex).Success)
				{
					result.Add(new KeyValuePair<string, string>(item.Text, item.GetAttribute("href")));

					if (takeFirst)
					{
						break;
					}
				}
			}
			return result;
		}
	}
}
