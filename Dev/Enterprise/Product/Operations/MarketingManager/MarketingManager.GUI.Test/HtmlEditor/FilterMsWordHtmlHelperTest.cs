using System.IO;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class FilterMsWordHtmlHelperTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFilteredOrRawHtmlContentFromUploadWithFilteredContent()
		{
			var filteredWebPage = Path.Combine(BaseSourcePath, TestFolderPath, "FilteredWebPage.htm");
			var filteredWebPageContent = File.ReadAllText(filteredWebPage);
			var result = FilterMsWordHtmlHelper.GetFilteredOrRawHtmlContentFromUpload(filteredWebPageContent);
			AssertEquals(filteredWebPageContent, result);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFilteredOrRawHtmlContentFromUploadWithNotFilteredContent()
		{
			var notFilteredWebPage = Path.Combine(BaseSourcePath, TestFolderPath, "NotFilteredWebPage.htm");
			var resultWebPage = Path.Combine(BaseSourcePath, TestFolderPath, "ResultWebPage.htm");
			var notFilteredWebPageContent = File.ReadAllText(notFilteredWebPage);
			var resultWebPageContent = File.ReadAllText(resultWebPage);

			var result = FilterMsWordHtmlHelper.GetFilteredOrRawHtmlContentFromUpload(notFilteredWebPageContent);

			var expectedDoc = HtmlTestHelper.LoadHtml(resultWebPageContent);
			var actualDoc = HtmlTestHelper.LoadHtml(result);
			Assert("The Html content should be equal.", HtmlTestHelper.HtmlDocumentsAreEqual(expectedDoc, actualDoc));
		}

		const string TestFolderPath = @"Enterprise\Product\Operations\MarketingManager\MarketingManager.GUI.Test\HtmlEditor\Test File";
	}
}
