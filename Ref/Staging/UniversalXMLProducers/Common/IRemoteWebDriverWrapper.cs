using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public interface IRemoteWebDriverWrapper
	{
		IWebElement FindElementByLinkText(string linkText);
		IWebElement FindElementByXPath(string xpath);
		IWebElement FindElementById(string id);
		ReadOnlyCollection<IWebElement> FindElementsByClassName(string className);
		ReadOnlyCollection<IWebElement> FindElementsByPartialLinkText(string partialLinkText);
		INavigation Navigate();
		void Dispose();
		string PageSource { get; }
		IWebDriver WebDriver { get; }
	}
}
