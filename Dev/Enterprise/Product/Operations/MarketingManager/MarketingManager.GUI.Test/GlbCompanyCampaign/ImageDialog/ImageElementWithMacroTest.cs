using System.Drawing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class ImageElementWithMacroTest : TestCase
	{
		public void TestImageElementWithEmpty()
		{
			var element = new ImageElementWithMacro();
			AssertEquals("<img />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithAlternativeText()
		{
			var element = new ImageElementWithMacro() { AlternativeText = "Test Alternative Text" };
			AssertEquals("<img alt=\"Test Alternative Text\" />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithSrcUrl()
		{
			var element = new ImageElementWithMacro() { SrcUrl = TestUrl };
			AssertEquals($"<img src=\"{TestUrl}\" />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithAlign()
		{
			var element = new ImageElementWithMacro() { Align = "Top" };
			AssertEquals("<img align=\"Top\" />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithBorderStyle()
		{
			var element = new ImageElementWithMacro() { BorderStyle = "Solid" };
			AssertEquals("<img style=\"border-style:Solid;\" />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithBorderWidth()
		{
			var element = new ImageElementWithMacro() { BorderWidth = "8px" };
			AssertEquals("<img style=\"border-width:8px;\" />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithBorderColor()
		{
			var element = new ImageElementWithMacro() { BorderColor = Color.FromArgb(30, 144, 255) };
			AssertEquals("<img style=\"border-color:#1E90FF;\" />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithHeight()
		{
			var element = new ImageElementWithMacro() { Height = "100px" };
			AssertEquals("<img style=\"height:100px;\" />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithWidth()
		{
			var element = new ImageElementWithMacro() { Width = "100px" };
			AssertEquals("<img style=\"width:100px;\" />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithMacro()
		{
			var element = new ImageElementWithMacro() { Macro = "test macro" };
			AssertEquals("<img macro=\"test macro\" />", element.ToHtmlStringWithMacro());
		}

		public void TestImageElementWithMarcoMultiplePropertiesTesting()
		{
			var element = new ImageElementWithMacro()
			{
				Macro = "test macro",
				Align = "Center",
				AlternativeText = "test alt content",
				BorderColor = Color.Red,
				BorderStyle = "Solid",
				BorderWidth = "2px",
				Height = "100px",
				Width = "200px",
				SrcUrl = TestUrl,
				Title = "test title",
			};
			AssertEquals("<img src=\"www.test.com/A.png\" title=\"test title\" align=\"Center\" alt=\"test alt content\" style=\"border-width:2px;height:100px;width:200px;border-color:Red;border-style:Solid;\" macro=\"test macro\" />", element.ToHtmlStringWithMacro());
		}

		const string TestUrl = "www.test.com/A.png";
	}
}
