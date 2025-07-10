using OpenQA.Selenium;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public static class WebElementExtension
	{
		public static bool IsVisible(this IWebElement webElement)
		{
			return webElement != null && webElement.Displayed && webElement.Enabled;
		}
	}
}
