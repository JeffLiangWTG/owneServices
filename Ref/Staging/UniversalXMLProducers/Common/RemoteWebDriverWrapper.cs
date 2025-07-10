using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public class RemoteWebDriverWrapper : IRemoteWebDriverWrapper
	{
		readonly RemoteWebDriver remoteWebDriver;

		public RemoteWebDriverWrapper(RemoteWebDriver remoteWebDriver)
		{
			this.remoteWebDriver = remoteWebDriver ?? throw new InvalidOperationException("web driver should not be null");
		}

		public IWebElement FindElementByLinkText(string linkText)
		{
			return remoteWebDriver.FindElementByLinkText(linkText);
		}

		public IWebElement FindElementByXPath(string xpath)
		{
			return remoteWebDriver.FindElementByXPath(xpath);
		}

		public IWebElement FindElementById(string id)
		{
			return remoteWebDriver.FindElementById(id);
		}

		public ReadOnlyCollection<IWebElement> FindElementsByClassName(string className)
		{
			return remoteWebDriver.FindElementsByClassName(className);
		}

		public ReadOnlyCollection<IWebElement> FindElementsByPartialLinkText(string partialLinkText)
		{
			return remoteWebDriver.FindElementsByPartialLinkText(partialLinkText);
		}

		public INavigation Navigate()
		{
			return remoteWebDriver.Navigate();
		}

		public void Dispose()
		{
			remoteWebDriver.Dispose();
		}

		public string PageSource => remoteWebDriver.PageSource;

		public IWebDriver WebDriver => remoteWebDriver;
	}
}
