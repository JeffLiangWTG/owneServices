using Moq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	[TestFixture]
	class WebElementExtensionTest
	{
		[Test]
		public void TestWebElementNotVisible()
		{
			var webElement = new Mock<IWebElement>();
			webElement.Setup(element => element.Displayed).Returns(false);
			webElement.Setup(element => element.Enabled).Returns(true);
			Assert.IsFalse(webElement.Object.IsVisible());

			webElement.Setup(element => element.Displayed).Returns(true);
			webElement.Setup(element => element.Enabled).Returns(false);
			Assert.IsFalse(webElement.Object.IsVisible());

			webElement.Setup(element => element.Displayed).Returns(false);
			webElement.Setup(element => element.Enabled).Returns(false);
			Assert.IsFalse(webElement.Object.IsVisible());
		}

		[Test]
		public void TestWebElementVisible()
		{
			var webElement = new Mock<IWebElement>();
			webElement.Setup(element => element.Displayed).Returns(true);
			webElement.Setup(element => element.Enabled).Returns(true);
			Assert.IsTrue(webElement.Object.IsVisible());
		}
	}
}
