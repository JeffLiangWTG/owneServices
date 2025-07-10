using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class WebCFSGlobalTest : ZGlobalTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateWebRegistry_HandlesUrlsWithProtocol()
		{
			var tester = GetNewZGlobalForTesting();

			var factory = new BusinessObjectFactory();
			var branch = factory.LoadTop1<GlbBranch>(new ZQuery());
			tester.GlobalConfig.ConfigurationItemsForTesting.Add("Branch", branch.GB_Code);
			var webCFSUrls = new[] { "HTTPS://www.TEST.com/", "Http://www.Test2.com/" };
			var webCFSUrlsCount = webCFSUrls.Length;

			using (WebDataRegistry.Instance.OldTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ThemeCodeDescriptionPairList.Codes.CUS))
			using (WebDataRegistry.Instance.WebCFSUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, webCFSUrls))
			using (WebDataRegistry.Instance.WebCFSTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerTheme>()))
			using (WebDataRegistry.Instance.WebCFSCustomImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomImage>()))
			using (WebDataRegistry.Instance.WebCFSCustomCss.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomCss>()))
			{
				var url = "www.test.com";
				tester.PopulateWebRegistry(url);

				AssertEquals("Should not add new WebCFSUrl if has https version", webCFSUrlsCount, WebDataRegistry.Instance.WebCFSUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);

				url = "www.test2.com";
				tester.PopulateWebRegistry(url);

				AssertEquals("Should not add new WebCFSUrl if has http version", webCFSUrlsCount, WebDataRegistry.Instance.WebCFSUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateWebRegistry()
		{
			ZGlobal tester = GetNewZGlobalForTesting();

			var factory = new BusinessObjectFactory();
			var branch = factory.LoadTop1<GlbBranch>(new ZQuery());
			tester.GlobalConfig.ConfigurationItemsForTesting.Add("Branch", branch.GB_Code);

			using (WebDataRegistry.Instance.OldTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, String.Empty))
			using (WebDataRegistry.Instance.WebCFSUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>()))
			using (WebDataRegistry.Instance.WebCFSTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerTheme>()))
			using (WebDataRegistry.Instance.WebCFSCustomImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomImage>()))
			using (WebDataRegistry.Instance.WebCFSCustomCss.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomCss>()))
			{
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebCFSUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebCFSTheme.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebCFSCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebCFSCustomCss.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);

				var url = "www.test.com";
				tester.PopulateWebRegistry(url);

				var urls = WebDataRegistry.Instance.WebCFSUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertCollectionContains(url, urls);

				var themeObjects = WebDataRegistry.Instance.WebCFSTheme.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertEquals(true, themeObjects.Exists(x => x.Code == "CUS" && x.Url == url));

				var imageObjects = WebDataRegistry.Instance.WebCFSCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertEquals(true, imageObjects.All(x => x.Url == url));

				var imagesFolder = Path.Combine(tester.ApplicationRoot, "Images");
				var imagePaths = new DirectoryInfo(tester.MapPath(imagesFolder)).GetFiles()
					.Where(x => WebTrackerCustomImage.IsSupportedFileType(x.Extension)).Select(x => x.FullName);
				AssertEquals(imageObjects.Count, imagePaths.Count());

				var webCfsCustomCss = new WebTrackerCustomCss(url, File.ReadAllText(tester.MapPath(tester.BaseStyleSheet)));
				var cssObjects = WebDataRegistry.Instance.WebCFSCustomCss.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertEquals(true, cssObjects.Exists(x => x.Url == url && x.Data == webCfsCustomCss.Data));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPopulateWebRegistry_DoesNotOverrideAllUrlImages()
		{
			ZGlobal tester = GetNewZGlobalForTesting();

			var factory = new BusinessObjectFactory();
			GlbBranch branch = factory.LoadTop1<GlbBranch>(new ZQuery());
			tester.GlobalConfig.ConfigurationItemsForTesting.Add("Branch", branch.GB_Code);

			var customImage = new WebTrackerCustomImage("Logo.gif", string.Empty, Array.Empty<byte>());
			using (WebDataRegistry.Instance.OldTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ThemeCodeDescriptionPairList.Codes.CUS))
			using (WebDataRegistry.Instance.WebCFSUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>()))
			using (WebDataRegistry.Instance.WebCFSTheme.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerTheme>()))
			using (WebDataRegistry.Instance.WebCFSCustomImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { customImage }))
			using (WebDataRegistry.Instance.WebCFSCustomCss.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<WebTrackerCustomCss>()))
			{
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebCFSUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebCFSTheme.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should have one custom image", 1, WebDataRegistry.Instance.WebCFSCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);
				AssertEquals("Registry item should be empty", 0, WebDataRegistry.Instance.WebCFSCustomCss.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList().Count);

				var url = "www.test.com";
				tester.PopulateWebRegistry(url);

				var imageObjects = WebDataRegistry.Instance.WebCFSCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				AssertEquals(true, imageObjects.All(x => x.Url == url && x.Name != "Logo.gif" || (string.IsNullOrEmpty(x.Url) && x.Name == "Logo.gif")));
			}
		}

		public override void AssertLocations(System.Xml.XmlNodeList locations)
		{
			Assert("No items in Locations section in Web.Config", true);
		}

		protected override string WebConfigPath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "WebCFS", "WebCFS.Web", "Web.config");

		protected override int NumberOfLocations => 0;

		protected override ZGlobal GetNewZGlobalForTesting() => new TestGlobal();
	}
}
