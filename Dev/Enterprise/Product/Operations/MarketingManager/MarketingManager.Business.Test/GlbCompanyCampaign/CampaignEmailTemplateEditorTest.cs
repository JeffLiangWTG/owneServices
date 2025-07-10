using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using HtmlAgilityPack;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(CampaignEmailTemplateEditor))]
	sealed class CampaignEmailTemplateEditorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTrackedLinks()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_URL = "http://www.abc.com";
			link1.GCL_Context = "abc";
			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.def.com";
			link2.GCL_Context = "def";
			var link3 = campaign.TrackedLinks.AddNew();
			link3.GCL_URL = "http://www.xyz.com";
			link3.GCL_Context = "xyz";
			link1.HasTrackingMacro = true;
			link2.GCL_IsTracked = true;

			var editor = new CampaignEmailTemplateEditor(campaign);
			AssertEquals("editor should not have a copy because TemplateHtmlText is empty", 0, editor.TrackedLinks.Count);
			editor.TemplateHtmlText = "<a href='http://www.abc.com'>abc</a><a href='http://www.def.com'>def</a><a href='http://www.xyz.com'>xyz</a>";

			AssertEquals("editor has a copy of campaign links", campaign.TrackedLinks.Count, editor.TrackedLinks.Count);
			AssertNotEquals("editor links in another factory", Factory._Instance, editor.TrackedLinks.Factory._Instance);
			AssertEquals("HasTrackingMacro copied", true, editor.TrackedLinks[0].HasTrackingMacro);
			AssertEquals("HasTrackingMacro set", true, editor.TrackedLinks[1].HasTrackingMacro);
			AssertEquals("HasTrackingMacro set", true, editor.TrackedLinks[2].HasTrackingMacro);
			var link4 = editor.TrackedLinks.AddNew();

			AssertEquals("link added to editor not added to campaign", 3, campaign.TrackedLinks.Count);
		}

		public void TestTemplateHtmlText_MergeWithIsTrackedLinks()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var linkTracked = campaign.TrackedLinks.AddNew();
			linkTracked.GCL_IsTracked = true;
			linkTracked.GCL_Context = "Tracked";
			linkTracked.GCL_URL = "http://www.tracked.com";
			var linkNotTracked = campaign.TrackedLinks.AddNew();
			linkNotTracked.GCL_Context = "NotTracked";
			linkNotTracked.GCL_URL = "http://www.nottracked.com";

			// Link with exact match to tracked link
			var editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText = @"<a href='(*LinkTracking(Tracked,""http://www.tracked.com"")*)'>Tracked</a>";
			var links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToList();
			AssertEquals("tracked link is preserved", true, links.Any(x => x.PK == linkTracked.PK && x.GCL_Context == "Tracked" && x.GCL_URL == "http://www.tracked.com"));
			AssertEquals(1, editor.TrackedLinks.Count);

			// Link with different context to tracked link should add a new link
			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText = @"<a href='(*LinkTracking(NewContext,""http://www.tracked.com"")*)'>Tracked</a>";
			links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToList();
			AssertEquals("new link added", true, links.Any(x => x.PK != linkTracked.PK && x.GCL_Context == "NewContext" && x.GCL_URL == "http://www.tracked.com"));
			AssertEquals("Original tracked link is lost because TemplateHtmlText has changed", false, links.Any(x => x.PK == linkTracked.PK && x.GCL_Context == "Tracked" && x.GCL_URL == "http://www.tracked.com"));
			AssertEquals(1, links.Count);

			// Link with different URL to tracked link should add a new link
			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText = @"<a href='(*LinkTracking(Tracked,""http://www.newurl.com"")*)'>Tracked</a>";
			links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToList();
			AssertEquals("new link added", true, links.Any(x => x.PK != linkTracked.PK && x.GCL_Context == "Tracked" && x.GCL_URL == "http://www.newurl.com"));
			AssertEquals("Original tracked link is lost because TemplateHtmlText has changed", false, links.Any(x => x.PK == linkTracked.PK && x.GCL_Context == "Tracked" && x.GCL_URL == "http://www.tracked.com"));
			AssertEquals(1, links.Count);

			//	Link with same URL to tracked link should bring back the original link
			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText = @"<a href='(*LinkTracking(Tracked,""http://www.tracked.com"")*)'>Tracked</a><a href='(*LinkTracking(Tracked,""http://www.newlink.com"")*)'>Tracked</a>";
			links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToList();
			AssertEquals("new link added", true, links.Any(x => x.PK != linkTracked.PK && x.GCL_Context == "Tracked" && x.GCL_URL == "http://www.newlink.com"));
			AssertEquals("Original tracked link is added", true, links.Any(x => x.PK == linkTracked.PK && x.GCL_Context == "Tracked" && x.GCL_URL == "http://www.tracked.com"));
			AssertEquals(2, links.Count);
		}

		public void TestEmailBodyEncoding()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			// default decoding
			char nbsp = '\u00A0';
			string text = "Very strange string:;%'?''" + nbsp;
			var windows1252 = Encoding.GetEncoding("windows-1252");
			ZBlob body = new ZBlob(windows1252.GetBytes(text));
			editor.TemplateBlob = body;
			AssertEquals("Email body decoding default to windows1252", text, editor.TemplateHtmlText);

			text = "<head><meta charset='utf-8'></head>" + nbsp;
			body = new ZBlob(Encoding.UTF8.GetBytes(text));
			editor.TemplateBlob = body;
			AssertEquals("HtmlText decoded correctly", text, editor.TemplateHtmlText);

			text = "<head><meta charset='utf-8'></head>" + nbsp;
			body = new ZBlob(windows1252.GetBytes(text));
			editor.TemplateBlob = body;
			AssertEquals("HtmlText decoded as UTF8", Encoding.UTF8.GetString(body), editor.TemplateHtmlText);

			text = "<head><meta charset='windows-1252'></head>" + nbsp;
			body = new ZBlob(Encoding.UTF8.GetBytes(text));
			editor.TemplateBlob = body;
			AssertEquals("HtmlText decoded as windows-1252", windows1252.GetString(body), editor.TemplateHtmlText);

			// add UTF8 preamble
			List<byte> byteList = new List<byte>();
			byteList.AddRange(Encoding.UTF8.GetPreamble());
			byteList.AddRange((byte[])body);
			editor.TemplateBlob = byteList.ToArray();
			AssertEquals("HtmlText - UTF8 preamble overrides charset and is removed", text, editor.TemplateHtmlText);

			text = "<head><meta charset=\"gb2312\" /><title>\u65B0\u6d6a</title></head>";
			body = new ZBlob(Encoding.Unicode.GetBytes(text));
			byteList.Clear();
			byteList.AddRange(Encoding.Unicode.GetPreamble());
			byteList.AddRange((byte[])body);
			editor.TemplateBlob = byteList.ToArray();
			AssertEquals("HtmlText - unicode preamble overrides charset and is removed", text, editor.TemplateHtmlText);

			editor.TemplateBlob = body;
			AssertEquals("HtmlText - unicode without preamble detected", text, editor.TemplateHtmlText);

			body = new ZBlob(Encoding.BigEndianUnicode.GetBytes(text));
			editor.TemplateBlob = body;
			AssertEquals("HtmlText - big endian unicode without preamble detected", text, editor.TemplateHtmlText);

			Encoding gb2312Encoding = null;
			Encoding windows1251Encoding = null;
			try
			{
				gb2312Encoding = Encoding.GetEncoding("gb2312");
				windows1251Encoding = Encoding.GetEncoding("Windows-1251");
			}
			catch
			{
			}

			// skip these tests if gb2312 charset not supported on this computer
			if (gb2312Encoding != null)
			{
				body = new ZBlob(gb2312Encoding.GetBytes(text));
				editor.TemplateBlob = body;
				AssertEquals("HtmlText - gb2312 decoded by metatag", text, editor.TemplateHtmlText);

				text = "<head>" + new string('\u65B0', 9) + "</head>"; // Detector works sufficiently when there is enough non Windows-1252 characters
				body = new ZBlob(gb2312Encoding.GetBytes(text));
				editor.TemplateBlob = body;
				AssertEquals("HtmlText - gb2312 decoded by mozilla's CharsetDetector", text, editor.TemplateHtmlText);
			}

			// skip this test if windows1251 charset not supported on this computer
			if (windows1251Encoding != null)
			{
				text = "<head><title>\u043F\u0430\u043C</title></head>";
				body = new ZBlob(windows1251Encoding.GetBytes(text));
				editor.TemplateBlob = body;
				AssertEquals("HtmlText - windows1251 decoded by mozilla's CharsetDetector", text, editor.TemplateHtmlText);
			}
		}

		public void TestSetTemplateBlobEvent()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			editor.SetTemplateBlob += (object sender, CampaignEmailTemplateEditor.HtmlTextEventArgs e) => e.ConvertedHtml = e.Html.Replace("unconvert", "converted");

			var text = @"<html><head><meta charset='utf-8'></head><body><p>unconvert</p></body></html>";
			var expectedText = @"<html><head><meta charset='utf-8'></head><body><p>converted</p></body></html>";
			var body = new ZBlob(Encoding.UTF8.GetBytes(text));
			editor.TemplateBlob = body;
			AssertEquals("HtmlText decoded correctly", expectedText, editor.TemplateHtmlText);
		}

		public void TestClearTemplateHtml()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			AssertEquals("The default TemplateHtmlText is empty.", string.Empty, editor.TemplateHtmlText);

			editor.ClearTemplateHtml();
			AssertEquals("The TemplateHtmlText is SkeletonHtml after cleared.", CampaignEmailTemplateEditor.SkeletonHtml, editor.TemplateHtmlText);
		}

		public void TestIsHtmlEmptyOrTemplateEmpty()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			Assert("The default HtmlDocumentBlob is empty.", campaign.HtmlDocumentBlob.IsEmpty);
			AssertEquals("IsHtmlEmptyOrTemplateEmpty is true, when HtmlDocumentBlob is empty.", true, campaign.EmailContentIsNotSet);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii(CampaignEmailTemplateEditor.SkeletonHtml);
			AssertEquals("IsHtmlEmptyOrTemplateEmpty is true, when HtmlDocumentBlob is Template empty.", true, campaign.EmailContentIsNotSet);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii(CampaignEmailTemplateEditor.SkeletonHtmlOld);
			AssertEquals("IsHtmlEmptyOrTemplateEmpty is true, when HtmlDocumentBlob is old Template empty.", true, campaign.EmailContentIsNotSet);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("Any other text");
			AssertEquals("IsHtmlEmptyOrTemplateEmpty is false, when HtmlDocumentBlob is any other thing.", false, campaign.EmailContentIsNotSet);
		}

		public void TestRemoveContentEditableAttributes()
		{
			var html = @"<html><body><div contenteditable=""true"">This text can be edited by the user.</div><cite contenteditable=""true"">Write your own name here</cite></body></html>";
			var expectedHtml = @"<html><body><div>This text can be edited by the user.</div><cite>Write your own name here</cite></body></html>";
			var actualResultHtml = CampaignEmailTemplateEditor.RemoveContentEditableAttributes(html);
			AssertEquals("contenteditable removed", expectedHtml, actualResultHtml);
		}

		//
		public void TestEmbedImagesInHtmlFromLocalDirectory()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var directories = new[] {
					Path.Combine(tempDirectory, @"Sub Directory"),
					Path.Combine(tempDirectory, @"http_files"),
					Path.Combine(tempDirectory, @"https_files")
				};
				directories.ForEach(d => { Directory.CreateDirectory(d); });

				var fileNames = new[] {
					"file1.jpg",
					"file2.gif",
					"file3.png",
					"fileX.xyz",
					"fileN"
				};

				var imageContent = new byte[] { 45, 45, 45 };

				foreach (var directory in directories)
				{
					foreach (var fileName in fileNames)
					{
						File.WriteAllBytes(Path.Combine(directory, fileName), imageContent);
					}
				}

				var html = @"<html>
<body><img id=""0"" src=""http://www.asx.com.au/pict.png"" />
<img id=""1"" src=""Sub%20Directory/file1.jpg"" />
<img id=""2"" src=""Sub%20Directory/file2.gif"" />
<img id=""3"" src=""Sub%20Directory/file3.png"" />
<img id=""4"" src=""Sub%20Directory/file4.png"" />
<img id=""5"" src=""SubDirectoryDoesNotExist/file4.png"" />
<img id=""6"" src=""data:image/png;base64,LS0t"" />
<img id=""7"" src=""http_files/file1.jpg"" />
<img id=""8"" src=""https_files/file2.gif"" />
<img id=""9"" src=""Sub%20Directory/fileX.xyz"" />
<img id=""10"" src=""Sub%20Directory/fileN"" />
<img id=""11"" />
</body>
</html>";
				var changedHtml = CampaignEmailTemplateEditor.EmbedImagesInHtmlFromLocalDirectory(tempDirectory, html);

				AssertImageSourceEmbedding(changedHtml,
						"http://www.asx.com.au/pict.png",
						"data:image/jpg;base64,LS0t",
						"data:image/gif;base64,LS0t",
						"data:image/png;base64,LS0t",
						"Sub%20Directory/file4.png",
						"SubDirectoryDoesNotExist/file4.png",
						"data:image/png;base64,LS0t",
						"data:image/jpg;base64,LS0t",
						"data:image/gif;base64,LS0t",
						"Sub%20Directory/fileX.xyz",
						"Sub%20Directory/fileN",
						null);

				var mappedClientPath = new TestMappedClientPath();
				mappedClientPath.FindMappedPath = true;

				html = @"<html>
<body><img id=""0"" src=""Sub%20Directory/file1.jpg"" />
</body>
</html>";

				changedHtml = CampaignEmailTemplateEditor.EmbedImagesInHtmlFromLocalDirectory(tempDirectory, html, mappedClientPath);

				AssertImageSourceEmbedding(changedHtml,
						"data:image/jpg;base64,LS0t");

				mappedClientPath.FindMappedPath = false;
				changedHtml = CampaignEmailTemplateEditor.EmbedImagesInHtmlFromLocalDirectory(tempDirectory, html, mappedClientPath);

				AssertImageSourceEmbedding(changedHtml,
						"Sub%20Directory/file1.jpg");
			}
		}

		public void AssertImageSourceEmbedding(string html, params string[] expectedSources)
		{
			var actualSources = new string[expectedSources.Length];

			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			var nodes = doc.DocumentNode.SelectNodes(@"//img");
			foreach (var node in nodes)
			{
				var sourceAttr = node.Attributes["src"];
				var idAttr = node.Attributes["id"];

				actualSources[int.Parse(idAttr.Value)] = sourceAttr?.Value;
			}

			AssertArrayEqualsByElements(expectedSources, actualSources);
		}

		public void TestRemoveEmbeddedImage_WithEmbeddedFile()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var fileName = new Uri(Path.Combine(tempDirectory.DirectoryName, "logo.svg")).LocalPath;
				var html = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""{fileName}"" embedded=""true"">
</body></html>";
				var hasEmbeddedImage = false;
				var expectedHtml = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src="""">
</body></html>";
				var actualResultHtml = CampaignEmailTemplateEditor.RemoveEmbeddedImage(html, ref hasEmbeddedImage);
				AssertEquals("img src should be removed for embedded images", expectedHtml, actualResultHtml);
				Assert("Has embedded images", hasEmbeddedImage);
			}
		}

		public void TestRemoveEmbeddedImage_WithValidExternalUrl()
		{
			var hasEmbeddedImage = false;
			var html = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""http://somewhere/logo.jpg"" embedded=""true"">
</body></html>";
			var expectedHtml = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""http://somewhere/logo.jpg"" embedded=""true"">
</body></html>";
			var actualResultHtml = CampaignEmailTemplateEditor.RemoveEmbeddedImage(html, ref hasEmbeddedImage);
			AssertEquals("img src should not be removed for http images", expectedHtml, actualResultHtml);
			Assert("Does not have embedded images", !hasEmbeddedImage);

			hasEmbeddedImage = false;
			html = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""https://wallpaperscraft.com/image.jpg"" embedded=""true"">
</body></html>";
			expectedHtml = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""https://wallpaperscraft.com/image.jpg"" embedded=""true"">
</body></html>";
			actualResultHtml = CampaignEmailTemplateEditor.RemoveEmbeddedImage(html, ref hasEmbeddedImage);
			AssertEquals("img src should not be removed for https images", expectedHtml, actualResultHtml);
			Assert("Does not have embedded images", !hasEmbeddedImage);
		}

		public void TestRemoveEmbeddedImage_WithInvalidExternalUrl()
		{
			var hasEmbeddedImage = false;
			var html = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""http://somewhere/logo.exe"">
</body></html>";
			var expectedHtml = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src="""">
</body></html>";
			var actualResultHtml = CampaignEmailTemplateEditor.RemoveEmbeddedImage(html, ref hasEmbeddedImage);
			AssertEquals("img src should be removed for invalid images", expectedHtml, actualResultHtml);
			Assert("Does have invalid images", hasEmbeddedImage);
		}

		public void TestRemoveEmbeddedImage_WithBase64Images()
		{
			var hasEmbeddedImage = false;
			var html = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src='data:image/jpg;base64,LS0t' width='20' height='10'>
</body></html>";
			var expectedHtml = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src='data:image/jpg;base64,LS0t' width='20' height='10'>
</body></html>";
			var actualResultHtml = CampaignEmailTemplateEditor.RemoveEmbeddedImage(html, ref hasEmbeddedImage);
			AssertEquals("img src should not be removed for base64 images", expectedHtml, actualResultHtml);
			Assert("Does not have embedded images", !hasEmbeddedImage);
		}

		public void TestRemoveEmbeddedImage_WithNoSrc()
		{
			var hasChange = false;
			var html = $@"
<html><head><meta charset=""utf-8""></head><body>
<img embedded=""true"">
</body></html>";
			var expectedHtml = $@"
<html><head><meta charset=""utf-8""></head><body>
<img embedded=""true"">
</body></html>";
			var actualResultHtml = CampaignEmailTemplateEditor.RemoveEmbeddedImage(html, ref hasChange);
			AssertEquals("img src don't need change when no src attribute", expectedHtml, actualResultHtml);
			Assert("Does not have any changes", !hasChange);
		}

		public void TestRemoveEmbeddedImage_WithEmptySrc()
		{
			var hasChange = false;
			var html = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src="""" embedded=""true"">
</body></html>";
			var expectedHtml = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src="""" embedded=""true"">
</body></html>";
			var actualResultHtml = CampaignEmailTemplateEditor.RemoveEmbeddedImage(html, ref hasChange);
			AssertEquals("img src don't need change when src is empty", expectedHtml, actualResultHtml);
			Assert("Does not have any changes", !hasChange);
		}

		public void TestValidFileExtensions()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			AssertEquals("Should contain 2 elements", 2, editor.ValidFileExtensions.Length);
			Assert(".html file valid", Array.IndexOf(editor.ValidFileExtensions, ".HTML") != -1);
			Assert(".htm file valid", Array.IndexOf(editor.ValidFileExtensions, ".HTM") != -1);
		}

		[TestDate(2018, 11, 22, 10, 00, 00)]
		public void TestSetCampaignEmailTemplate()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			string text = "(*CampaignID*)\u00A0";

			editor.TemplateBlob = new ZBlob(Encoding.Unicode.GetBytes(text));
			AssertEquals(text, editor.TemplateHtmlText);
			editor.SetCampaignEmailTemplate();
			AssertEquals(new ZBlob(Encoding.UTF8.GetBytes(text)), campaign.HtmlDocumentBlob);

			editor.TemplateBlob = new ZBlob(Encoding.BigEndianUnicode.GetBytes(text));
			AssertEquals(text, editor.TemplateHtmlText);
			editor.SetCampaignEmailTemplate();
			AssertEquals(new ZBlob(Encoding.UTF8.GetBytes(text)), campaign.HtmlDocumentBlob);

			editor.TemplateBlob = new ZBlob(Encoding.UTF8.GetBytes(text));
			AssertEquals(text, editor.TemplateHtmlText);
			editor.SetCampaignEmailTemplate();
			AssertEquals(new ZBlob(Encoding.UTF8.GetBytes(text)), campaign.HtmlDocumentBlob);

			editor.TemplateFileName = "test.html";
			editor.TemplateHtmlText = "<head><meta charset='utf-8'></head>";
			editor.SetCampaignEmailTemplate();
			AssertEquals("Template attached: test.html", campaign.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
			AssertEquals(FormattableString.Invariant($@"<head><meta charset='utf-8'></head><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">"), campaign.HtmlTextToSend);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			editor.TemplateFileName = @"\\abc\" + @"LongTemplateFileName.html" + new string('L', 1000);
			editor.TemplateHtmlText = "<head><meta charset='utf-8'></head>";
			editor.SetCampaignEmailTemplate();
			var longLog = campaign.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference;
			AssertEquals("Template attached: LongTemplateFileName.html" + new string('L', 980), longLog);
			Assert(longLog.Length <= StmALog.Schema.SL_ReferenceMaxLength);
			AssertEquals(FormattableString.Invariant($@"<head><meta charset='utf-8'></head><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">"), campaign.HtmlTextToSend);
		}

		public void TestDeleteLinkOnUpdateCampaignLinksFromLocalLinks()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "AAA";
			link1.GCL_URL = "a@test.com";
			link1.GCL_IsTracked = false;

			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "BBB";
			link2.GCL_URL = "b@tester.org";
			link2.GCL_IsTracked = true;

			editor.TrackedLinks.Add(link1);
			editor.TrackedLinks.Add(link2);

			editor.SetCampaignEmailTemplate();
			AssertEquals(1, campaign.TrackedLinks.Count);
			AssertEquals(link2.PK, campaign.TrackedLinks[0].PK);

			var link3 = campaign.TrackedLinks.AddNew();
			link3.GCL_Context = "AAA";
			link3.GCL_URL = "a@test.com";
			link3.GCL_IsTracked = true;

			campaign.TrackedLinks.Add(link3);
			editor.SetCampaignEmailTemplate();
			AssertEquals(2, campaign.TrackedLinks.Count);
		}

		public void TestDecodeDocumentBlob_WithManyHighByteCharactersThatDoNotDetermineAnEncoding()
		{
			var blobOfUnknownHighByteCharacter = new ZBlob(Enumerable.Range(0, 2000).Select(x => (byte)0xFF).ToArray());
			AssertNoExceptionThrown(() =>
			{
				CampaignEmailTemplateEditor.DecodeDocumentBlob(blobOfUnknownHighByteCharacter);
			});
		}

		public void TestSetCampaignEmailTemplate_NonWestern1252Characters()
		{
			Encoding gb2312Encoding = null;
			Encoding windows1251Encoding = null;
			try
			{
				gb2312Encoding = Encoding.GetEncoding("gb2312");
				windows1251Encoding = Encoding.GetEncoding("Windows-1251");
			}
			catch
			{
				// see AssertExistingBlobIsConverted
			}

			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			string text = "<head>" + new string('\u65B0', 9) + "</head>";
			string expectedText = "<head><meta charset=\"utf-8\">" + new string('\u65B0', 9) + "</head>";
			editor.TemplateHtmlText = text;
			editor.SetCampaignEmailTemplate();
			AssertEquals(new ZBlob(Encoding.UTF8.GetBytes(expectedText)), campaign.HtmlDocumentBlob);
			AssertEquals(expectedText, CampaignEmailTemplateEditor.DecodeDocumentBlob(campaign.HtmlDocumentBlob));

			AssertExistingBlobIsConverted(gb2312Encoding, text, expectedText);

			text = "<head><meta name=\"description\" content=\"Russian characters\"><title>\u043F\u0430\u043C</title></head>";
			expectedText = text.Replace("<head><meta name", "<head><meta charset=\"utf-8\"><meta name");
			editor.TemplateHtmlText = text;
			editor.SetCampaignEmailTemplate();
			AssertEquals(new ZBlob(Encoding.UTF8.GetBytes(expectedText)), campaign.HtmlDocumentBlob);
			AssertEquals(expectedText, CampaignEmailTemplateEditor.DecodeDocumentBlob(campaign.HtmlDocumentBlob));

			AssertExistingBlobIsConverted(windows1251Encoding, text, expectedText);

			text = "<html><head><META charset=\"gb2312\"><meta name=\"description\" content=\"Simplified Chinese characters\"><title>\u65B0\u6d6a</title></head></html>";
			expectedText = text.Replace("<META charset=\"gb2312\">", "<meta charset=\"utf-8\">");
			editor.TemplateHtmlText = text;
			editor.SetCampaignEmailTemplate();
			AssertEquals("Converted to Utf-8 blob", new ZBlob(Encoding.UTF8.GetBytes(expectedText)), campaign.HtmlDocumentBlob);
			AssertEquals("Converted to Utf-8 charset", expectedText, CampaignEmailTemplateEditor.DecodeDocumentBlob(campaign.HtmlDocumentBlob));

			AssertExistingBlobIsConverted(gb2312Encoding, text, expectedText);

			text = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=gb2312\"><meta name=\"description\" content=\"Simplified Chinese characters\"><title>\u65B0\u6d6a</title></head></html>";
			expectedText = text.Replace("gb2312", "utf-8");
			editor.TemplateHtmlText = text;
			editor.SetCampaignEmailTemplate();
			AssertEquals("Converted to Utf-8 blob", new ZBlob(Encoding.UTF8.GetBytes(expectedText)), campaign.HtmlDocumentBlob);
			AssertEquals("Converted to Utf-8 charset", expectedText, CampaignEmailTemplateEditor.DecodeDocumentBlob(campaign.HtmlDocumentBlob));

			AssertExistingBlobIsConverted(gb2312Encoding, text, expectedText);
		}

		void AssertExistingBlobIsConverted(Encoding encoding, string inputText, string expectedOutputText)
		{
			// see TestSetCampaignEmailTemplate_NonWestern1252Characters
			// skip test if gb2312 or windows1251 charset not supported on this computer
			if (encoding != null)
			{
				var campaign = Factory.New<GlbCompanyCampaign>();
				var editor = new CampaignEmailTemplateEditor(campaign);

				ZBlob body = new ZBlob(encoding.GetBytes(inputText));
				editor.TemplateBlob = body;
				AssertEquals(string.Format("Precondition: HtmlText - {0} decoded by metatag/detector", encoding.WebName), inputText, editor.TemplateHtmlText);
				editor.SetCampaignEmailTemplate();
				AssertEquals("Converted to Utf-8 blob", new ZBlob(Encoding.UTF8.GetBytes(expectedOutputText)), campaign.HtmlDocumentBlob);
				AssertEquals("Converted to Utf-8 charset", expectedOutputText, CampaignEmailTemplateEditor.DecodeDocumentBlob(campaign.HtmlDocumentBlob));
			}
		}

		public void TestSetCampaignEmailTemplate_TrackedLinks()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "1";
			link1.GCL_URL = "http://localhost/1";
			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "2";
			link2.GCL_URL = "http://localhost/2";
			campaign.HtmlDocumentBlob = ZBlob.FromAscii(
				@"<a href='(*LinkTracking(1,""http://localhost/1"")*)'>1</a>" +
				@"<a href='(*LinkTracking(2,""http://localhost/2"")*)'>2</a>" +
				@"<a href='http://localhost/ZZ'>ZZ</a>"
				);

			var editor = campaign.TemplateEditor;
			var localLink1 = (GlbCompanyCampaignLink)editor.TrackedLinks.FindByPK(link1.PK);
			var localLink2 = (GlbCompanyCampaignLink)editor.TrackedLinks.FindByPK(link2.PK);
			editor.TemplateHtmlText =
				@"<a href='(*LinkTracking(3,""http://localhost/3"")*)'>3</a>" +
				@"<a href='(*LinkTracking(2,""http://localhost/2"")*)'>2</a>" +
				@"<a href='http://localhost/ZZ'>ZZ</a>";

			var localLink3 = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().First(x => x.GCL_Context == "3");
			AssertEquals("untracked link ZZ added to collection", true, editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().Any(x => x.GCL_Context == "ZZ"));

			localLink2.GCL_Context = "2b";

			AssertEquals("Pre:", "2", link2.GCL_Context);

			editor.SetCampaignEmailTemplate();

			AssertEquals(2, campaign.TrackedLinks.Count);
			AssertNull("link 1 removed", campaign.TrackedLinks.FindByPK(link1.PK));
			AssertEquals("link 2 updated", "2b", link2.GCL_Context);
			var link3 = (GlbCompanyCampaignLink)campaign.TrackedLinks.FindByPK(localLink3.PK);
			AssertNotNull("link3 added", link3);
			AssertEquals("http://localhost/3", link3.GCL_URL);
			AssertEquals("3", link3.GCL_Context);
			AssertEquals(campaign.PK, link3.GCL_G0_Campaign);
		}

		public void TestSetCampaignEmailTemplate_DocumentAttachedMessageInfoHasErrors()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			var campaign = helper.GetCampaignForTestWithoutErrors();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			var editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText = "Hello this is a test";
			editor.TemplateFileName = "temp_filte.html";

			AssertEquals("Pre-condition", 0, campaign.Logs.GetAllLogs().Count);

			editor.SetCampaignEmailTemplate();
			string expectedErrorMessage = string.Format("HTML document should include field (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName);
			AssertHasError(campaign.IsDocumentAttachedInfo, expectedErrorMessage);
			AssertEquals("should not be logged if there is error", 0, campaign.Logs.GetAllLogs().Count);

			editor.TemplateHtmlText = string.Format("HELLO (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName);
			editor.SetCampaignEmailTemplate();
			AssertNoErrors(campaign.IsDocumentAttachedInfo);
			AssertEquals("Template attached: temp_filte.html", campaign.Logs.MostRecentLogByEventTime(Events.EditedARecord).SL_Reference);
		}

		public void TestEnableMacroDataPreview()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			editor.EnableMacroDataPreview = true;
			AssertEquals(true, editor.EnableMacroDataPreview);

			editor.EnableMacroDataPreview = false;
			AssertEquals(false, editor.EnableMacroDataPreview);
		}

		public void TestSimulationContactPK()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "BBCE";

			var contact = org.Contacts.AddNew();

			editor.SimulationContactPK = contact.PK;
			AssertEquals(contact.PK, editor.SimulationContactPK);
		}

		public void TestTemplateHtmlText_hrefAndMacro()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			editor.TemplateHtmlText = "<a href='http://www.wisetechglobal.com'>Hello1</a>";
			AssertEquals("<a href='http://www.wisetechglobal.com'>Hello1</a>", editor.TemplateHtmlText);

			var link = editor.TrackedLinks[0];
			AssertEquals("Hello1", link.GCL_Context);
			AssertEquals(false, link.GCL_IsImage);
			AssertEquals(@"http://www.wisetechglobal.com", link.UnicodeUrl);
			AssertEquals(false, link.HasTrackingMacro);
			AssertEquals(1, editor.TrackedLinks.Count);

			link.HasTrackingMacro = true;
			AssertEquals("attr was added", @"<a href='http://www.wisetechglobal.com' tid=""Hello1"">Hello1</a>", editor.TemplateHtmlText);

			link.HasTrackingMacro = false;
			AssertEquals("attr was removed", @"<a href='http://www.wisetechglobal.com'>Hello1</a>", editor.TemplateHtmlText);

			link.HasTrackingMacro = true;
			AssertEquals("macro is still absent", @"<a href='http://www.wisetechglobal.com' tid=""Hello1"">Hello1</a>", editor.TemplateHtmlText);

			editor.TemplateHtmlText = @"<a href='(*LinkTracking(Hello1,http://www.wisetechglobal.com)*)' tid=""Hello1"">Hello1</a>";
			link = editor.TrackedLinks[0];
			AssertEquals("Hello1", link.GCL_Context);
			AssertEquals(false, link.GCL_IsImage);
			AssertEquals(@"http://www.wisetechglobal.com", link.UnicodeUrl);
			AssertEquals(true, link.HasTrackingMacro);
			AssertEquals("macro is still here", @"<a href='(*LinkTracking(Hello1,http://www.wisetechglobal.com)*)' tid=""Hello1"">Hello1</a>", editor.TemplateHtmlText);

			link.HasTrackingMacro = false;
			AssertEquals("macro and attr was removed", @"<a href='http://www.wisetechglobal.com'>Hello1</a>", editor.TemplateHtmlText);

			link.HasTrackingMacro = true;
			AssertEquals("attr was added", @"<a href='http://www.wisetechglobal.com' tid=""Hello1"">Hello1</a>", editor.TemplateHtmlText);
		}

		public void TestTemplateHtmlText_LinksAddedRemovedChanged()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			editor.TemplateHtmlText =
@"
<a href='(*LinkTracking(Hello1,""http://www.wisetechglobal.com"")*)'>Click Me 1</a>
<a href='http://www.wisetechglobal.com/2'>Click Me 2</a>
<a href='http://www.wisetechglobal.com/3'>Click Me 3</a>
";
			AssertEquals(3, editor.TrackedLinks.Count);
			var oldLinks = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToArray();
			var link1 = oldLinks.First(x => x.GCL_Context == "Hello1");
			var link2 = oldLinks.First(x => x.GCL_Context == "Click Me 2");
			var link3 = oldLinks.First(x => x.GCL_Context == "Click Me 3");

			editor.TemplateHtmlText =
@"

<a href='http://www.wisetechglobal.com/2'>link2 modified</a>
<a href='http://www.wisetechglobal.com/3'>Click Me 3</a>

<a href='(*LinkTracking(Hello1,http://www.wisetechglobal.com/1)*)'>link1 modified</a>
<a href='(*LinkTracking(HelloNew,http://www.wisetechglobal.com/new)*)'>New Link</a>
";

			var newLinks = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToArray();
			var link1Updated = newLinks.First(x => x.PK == link1.PK);
			AssertEquals("link1 PK unchanged - context matched", "Hello1", link1Updated.GCL_Context);
			AssertEquals("link1 URL updated", "http://www.wisetechglobal.com/1", link1Updated.GCL_URL);

			var link3a = newLinks.FirstOrDefault(x => x.GCL_URL == "http://www.wisetechglobal.com/3");
			AssertNotNull(link3a);

			var linkNew = newLinks.FirstOrDefault(x => x.GCL_Context == "HelloNew");
			AssertEquals("http://www.wisetechglobal.com/new", linkNew.GCL_URL);

			AssertEquals(4, editor.TrackedLinks.Count);
		}

		public void TestTemplateHtmlText_hrefContainingImage()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			editor.TemplateHtmlText = @"<a href='http://www.test.com/newsroom?utm_source=july2014'><img src='http://www.test.com//image.jpg' width='20' height='10'></a>'";

			var linkHref = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().First(x => !x.GCL_IsImage);
			var linkImage = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().FirstOrDefault(x => x.GCL_IsImage);
			AssertEquals("image.jpg [Image Link]", linkHref.GCL_Context);
			AssertEquals(@"http://www.test.com/newsroom?utm_source=july2014", linkHref.UnicodeUrl);

			AssertNull("Image is not tracked. Only Image Link is tracked.", linkImage);
			AssertEquals(1, editor.TrackedLinks.Count);

			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText = @"<a href='http://www.test.com/newsroom?utm_source=july2014'><img src='(*LinkTracking(image,http://www.test.com//image.jpg)*)' width='20' height='10'></a>'";
			linkHref = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().First(x => !x.GCL_IsImage);
			AssertEquals("image.jpg [Image Link]", linkHref.GCL_Context);
			AssertEquals(1, editor.TrackedLinks.Count);

			editor.TemplateHtmlText =
@"<a href='http://www.test.com/newsroom?utm_source=july2014'>
		<img src='http://www.test.com/arrow.png' width='10' height='10' />
</a>'";
			var link = editor.TrackedLinks[0];
			link.HasTrackingMacro = true;
			AssertEquals("http://www.test.com/newsroom?utm_source=july2014", link.GCL_URL);
			AssertEquals("arrow.png [Image Link]", link.GCL_Context);

			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText = @"<a href='http://www.test.com/newsroom?utm_source=july2014'><span><img src='data:image/jpg;base64,LS0t' width='20' height='10'/></span></a>'";
			linkHref = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().First(x => !x.GCL_IsImage);
			AssertEquals("Embedded Image", linkHref.GCL_Context);
			AssertEquals(1, editor.TrackedLinks.Count);
		}

		public void TestTemplateHtmlText_InvalidCharacters()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText = @"<a href='http://www.test.com/newsroom?utm_source=july2014'>Comma, quote "", single ', stop. bracket ()</a>'";
			var link = editor.TrackedLinks[0];

			link.HasTrackingMacro = true;
			AssertEquals("http://www.test.com/newsroom?utm_source=july2014", link.GCL_URL);
			AssertEquals("Comma quote  single  stop. bracket ()", link.GCL_Context);
		}

		public void TestTemplateHtmlText_IsUrlTrackable()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			editor.TemplateHtmlText = @"<a href='http://www.abc.net/CampaignUrl'>Link</a>";
			AssertEquals("Valid URI is trackable", 1, editor.TrackedLinks.Count);

			editor.TemplateHtmlText = @"<a href='http://www.abc.net/CampaignUrl'>Link</a>";
			AssertEquals("Valid URI is trackable", 1, editor.TrackedLinks.Count);

			editor.TemplateHtmlText = @"<a href='http://www.abc.net/(*CampaignUrl*)'>Link</a>";
			AssertEquals("Nested macro is now supported so trackable", 1, editor.TrackedLinks.Count);

			editor.TemplateHtmlText = @"<a href='(*LinkTracking(Z,http://www.3.info)*)'>Link</a>";
			AssertEquals("LinkTracking macro is trackable", 1, editor.TrackedLinks.Count);
		}

		public void TestTemplateHtmlText_DuplicateUrls()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText =
				"<a href='http://www.1.info'>1</a>" +
				"<a href='http://www.1.info'>1</a>";
			AssertEquals("context", "1", editor.TrackedLinks[0].GCL_Context);
			AssertEquals("context", "1", editor.TrackedLinks[1].GCL_Context);
			AssertEquals(2, editor.TrackedLinks.Count);

			editor.TrackedLinks[1].HasTrackingMacro = true;
			AssertEquals("<a href='http://www.1.info'>1</a><a href='http://www.1.info' tid=\"1\">1</a>", editor.TemplateHtmlText);
			AssertEquals("1", editor.TrackedLinks[0].GCL_Context);
			AssertEquals("1", editor.TrackedLinks[1].GCL_Context);
			AssertEquals(2, editor.TrackedLinks.Count);
		}

		public void TestTemplateHtmlText_SequentialDuplicateUrls()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText = "<a href='http://www.one.net'>One Net</a>";
			AssertEquals(1, editor.TrackedLinks.Count);
			AssertEquals("context", "One Net", editor.TrackedLinks[0].GCL_Context);

			editor.TemplateHtmlText += "<a href='http://www.one.net'>One Net</a>";
			AssertEquals(2, editor.TrackedLinks.Count);
			AssertEquals("context", "One Net", editor.TrackedLinks[0].GCL_Context);
			AssertEquals("context", "One Net", editor.TrackedLinks[1].GCL_Context);
		}

		public void TestTemplateHtmlText_AddRemoveTrackIDAttribute()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText =
				"<a href='http://www.1.info'>1</a>" +
				"<a href='http://www.1.info'>1</a>";
			AssertEquals("context", "1", editor.TrackedLinks[0].GCL_Context);
			AssertEquals("context", "1", editor.TrackedLinks[1].GCL_Context);
			AssertEquals(2, editor.TrackedLinks.Count);

			editor.TrackedLinks[1].HasTrackingMacro = true;
			AssertEquals("<a href='http://www.1.info'>1</a><a href='http://www.1.info' tid=\"1\">1</a>", editor.TemplateHtmlText);
			AssertEquals("1", editor.TrackedLinks[0].GCL_Context);
			AssertEquals("1", editor.TrackedLinks[1].GCL_Context);
			AssertEquals(2, editor.TrackedLinks.Count);

			editor.TrackedLinks[1].HasTrackingMacro = false;
			AssertEquals("<a href='http://www.1.info'>1</a><a href='http://www.1.info'>1</a>", editor.TemplateHtmlText);
			AssertEquals("1", editor.TrackedLinks[0].GCL_Context);
			AssertEquals("1", editor.TrackedLinks[1].GCL_Context);
			AssertEquals(2, editor.TrackedLinks.Count);

			editor.TrackedLinks[1].HasTrackingMacro = true;
			editor.TrackedLinks[1].GCL_Context = "Goose";
			AssertEquals("<a href='http://www.1.info'>1</a><a href='http://www.1.info' tid=\"Goose\">1</a>", editor.TemplateHtmlText);
			AssertEquals("1", editor.TrackedLinks[0].GCL_Context);
			AssertEquals("Goose", editor.TrackedLinks[1].GCL_Context);
			AssertEquals(2, editor.TrackedLinks.Count);
		}

		public void TestTemplateHtmlText_DuplicateContexts()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText =
				"<a href='http://www.1.info'>W</a>" +
				"<a href='http://www.2.info'>W</a>" +
				"<a href='(*LinkTracking(Z,http://www.3.info)*)'>Z</a>" +
				"<a href='(*LinkTracking(Z,http://www.4.info)*)'>Z</a>";
			var links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToArray();
			var untracked1 = links.First(x => x.GCL_URL == "http://www.1.info");
			var untracked2 = links.First(x => x.GCL_URL == "http://www.2.info");
			var tracked1 = links.First(x => x.GCL_URL == "http://www.3.info");
			var tracked2 = links.First(x => x.GCL_URL == "http://www.4.info");
			AssertEquals(4, links.Length);

			AssertEquals("duplicate untracked context not modified", "W", untracked1.GCL_Context);
			AssertEquals("duplicate untracked context not modified", "W", untracked2.GCL_Context);
			AssertEquals("duplicate tracked context not modified", "Z", tracked1.GCL_Context);
			AssertEquals("duplicate tracked context not modified", "Z", tracked2.GCL_Context);
			AssertNoErrors("duplicate untracked context IS allowed", untracked1.GCL_ContextInfo);
			AssertNoErrors("duplicate untracked context IS allowed", untracked2.GCL_ContextInfo);
			Assert("duplicate tracked context not allowed", tracked1.GCL_ContextInfo.HasErrors() || tracked2.GCL_ContextInfo.HasErrors());

			editor.TrackedLinks[1].HasTrackingMacro = true;
			editor.TrackedLinks[0].HasTrackingMacro = true;
			links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToArray();
			AssertEquals("first link to be tracked has unmodified context", "W", links[1].GCL_Context);
			AssertEquals("second link automatically made unique", "W (2)", links[0].GCL_Context);
			AssertNoErrors("no errors", links[0].GCL_ContextInfo);
			AssertNoErrors("no errors", links[1].GCL_ContextInfo);

			// truncate to max length
			string maxLengthContext = ZString.Replicate('W', GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength);
			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText =
				"<a href='http://www.1.info'>" + maxLengthContext + "</a>" +
				"<a href='http://www.2.info'>" + maxLengthContext + "</a>";
			editor.TrackedLinks[0].HasTrackingMacro = true;
			editor.TrackedLinks[1].HasTrackingMacro = true;
			links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToArray();
			AssertNotEquals(links[0].GCL_Context, links[1].GCL_Context);
			Assert(links[0].GCL_Context.Length <= GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength);
			Assert(links[1].GCL_Context.Length <= GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength);

			// increment existing number
			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText =
				"<a href='http://www.1.info'>" + "AAA (2) " + maxLengthContext + "</a>" +
				"<a href='http://www.2.info'>" + "AAA (2) " + maxLengthContext + "</a>";
			editor.TrackedLinks[0].HasTrackingMacro = true;
			editor.TrackedLinks[1].HasTrackingMacro = true;
			links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToArray();
			AssertNotEquals(links[0].GCL_Context, links[1].GCL_Context);
			Assert(links[0].GCL_Context.Length <= GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength);
			Assert(links[1].GCL_Context.Length <= GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength);
			AssertNotNull(links.FirstOrDefault(x => x.GCL_Context == ("AAA (2) " + maxLengthContext).Substring(0, GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength)));
			AssertNotNull(links.FirstOrDefault(x => x.GCL_Context == ("AAA (3) " + maxLengthContext).Substring(0, GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength)));

			// existing number too big
			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText =
				"<a href='http://www.1.info'>AAA (123456789123456789)</a>" +
				"<a href='http://www.2.info'>AAA (123456789123456789)</a>";
			editor.TrackedLinks[0].HasTrackingMacro = true;
			editor.TrackedLinks[1].HasTrackingMacro = true;
			links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToArray();
			AssertNotEquals(links[0].GCL_Context, links[1].GCL_Context);
			Assert(links.Any(x => x.GCL_Context == "AAA (123456789123456789)"));
			Assert(links.Any(x => x.GCL_Context == "AAA (123456789123456789) (2)"));

			// max int
			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText =
				"<a href='http://www.1.info'>AAA (" + Int32.MaxValue.ToString() + ")</a>" +
				"<a href='http://www.2.info'>AAA (" + Int32.MaxValue.ToString() + ")</a>";
			editor.TrackedLinks[0].HasTrackingMacro = true;
			editor.TrackedLinks[1].HasTrackingMacro = true;
			links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToArray();
			AssertNotEquals(links[0].GCL_Context, links[1].GCL_Context);
			Assert(links[0].GCL_Context.Length <= GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength);
			Assert(links[1].GCL_Context.Length <= GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength);
			Assert(links.Any(x => x.GCL_Context == "AAA (" + Int32.MaxValue.ToString() + ")"));
			Assert(links.Any(x => x.GCL_Context == "AAA (1)"));

			editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText =
				"<a href='http://www.1.info'>AAA</a>" +
				"<a href='http://www.1b.info'>AAA</a>" +
				"<a href='http://www.2.info'>AAA (2)</a>" +
				"<a href='http://www.3.info'>AAA (3)</a>" +
				"<a href='http://www.5.info'>AAA (5)</a>" +
				"<a href='http://www.1c.info'>AAa</a>" +
				"<a href='http://www.1d.info'>AAA</a>";
			for (int i = 0; i < 7; ++i)
			{
				editor.TrackedLinks[i].HasTrackingMacro = true;
			}
			links = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().ToArray();
			AssertEquals("all distinct", 7, links.Select(x => x.GCL_Context.ToUpper()).Distinct().Count());
			Assert(links.Any(x => x.GCL_Context == "AAA (2)" && x.GCL_URL == "http://www.2.info"));
			Assert(links.Any(x => x.GCL_Context == "AAA (3)" && x.GCL_URL == "http://www.3.info"));
			Assert(links.Any(x => x.GCL_Context == "AAA (5)" && x.GCL_URL == "http://www.5.info"));
			Assert(links.Any(x => x.GCL_Context == "AAA (4)"));
			Assert(links.Any(x => x.GCL_Context == "AAa (6)"));
		}

		public void TestTemplateHtmlText_HtmlHead()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			editor.TemplateHtmlText =
@"<html>
<head>
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>News</title>
<style type=""text/css"">
  a{color:#AAA;text-decoration:none}
  h1{color:#BBB;text-decoration:none}
</style><style type=""text/css"">a{color:#AAA;text-decoration:none}</style>
</head>
<body class=""md-r"">
<table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""600"">
<tbody>
<tr><td>AAA</td></tr>
</tbody>
</table>
</body>
</html>";
			var updatedHtml = @"<html><head></head><body class=""md-r""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""600""><tbody><tr><td>AAA</td></tr></tbody></table></body></html>";
			editor.SetTemplateHtmlText(updatedHtml, CampaignEmailTemplateEditor.HtmlTextSource.Designer);

			var expectedHtml =
@"<html><head><meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""><title>News</title><style type=""text/css"">  a{color:#AAA;text-decoration:none}  h1{color:#BBB;text-decoration:none}</style><style type=""text/css"">a{color:#AAA;text-decoration:none}</style></head><body class=""md-r""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""600""><tbody><tr><td>AAA</td></tr></tbody></table></body></html>";
			AssertEquals(expectedHtml, editor.TemplateHtmlText);
		}

		public void TestHasTemplateChanges()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			editor.TemplateHtmlText = "";
			AssertEquals(false, campaign.HasChanges);

			editor.TemplateHtmlText = @"<body><a href=""http://www.wisetechglobal.com"">Wise Tech Global</a>";
			AssertEquals(false, campaign.HasChanges);

			editor.TemplateHtmlText = @"<body><a href=""http://www.wisetechglobal.com"">Cargowise</a>";
			AssertEquals(true, campaign.HasChanges);
		}

		public void TestHighlightLink()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			var htmlText = "<html><body><a href='http://www.1.info'>1</a><a href='http://www.2.info'>2</a></body></html>";

			editor.SetTemplateHtmlText(htmlText, CampaignEmailTemplateEditor.HtmlTextSource.TextEditor);

			int start;
			int length;
			var highlightId = editor.HighlightLink(editor.TrackedLinks[0], out start, out length);
			AssertEquals(editor.TemplateHtmlText.IndexOf("'http://www.1.info'") + 1, start);
			AssertEquals(17, length);
			AssertEquals(1, highlightId);

			highlightId = editor.HighlightLink(editor.TrackedLinks[1], out start, out length);
			AssertEquals(editor.TemplateHtmlText.IndexOf("'http://www.2.info'") + 1, start);
			AssertEquals(17, length);
			AssertEquals(2, highlightId);

			htmlText = "<html><body><a href='http://www.1.info'>1</a></body></html>";
			editor.SetTemplateHtmlText(htmlText, CampaignEmailTemplateEditor.HtmlTextSource.TextEditor);
			AssertEquals("Image is not tracked", 1, editor.TrackedLinks.Count);
			highlightId = editor.HighlightLink(editor.TrackedLinks[0], out start, out length);
			AssertEquals(1, highlightId);
		}

		[ExpectNoExceptions]
		public void TestSyncImageTrackIDAttributeAndTrackingMacro_NullRef()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);

			editor.TemplateHtmlText = $@"
<html><head><meta charset=""utf - 8""></head><body>
	<img src = 'abc.png' embedded = 'false'> &nbsp;
	<img sr0 = 'typo.png' embedded = 'true' > &nbsp;
</body ></html>";

			editor.SyncImageTrackIDAttributeAndTrackingMacro();
		}

		public void TestDecodeDocumentBlob_RestoreTrackingLinks()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			var actual = @"";
			var expected = FormattableString.Invariant($@"<img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">");
			var actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Same text", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<img src=""https://wallpaperscraft.com/image.jpg"">";
			expected = FormattableString.Invariant($@"<img src=""https://wallpaperscraft.com/image.jpg""><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Same text", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<img src=""https://wallpaperscraft.com/image.jpg"" tid=""img"">";
			expected = FormattableString.Invariant($@"<img src=""(*LinkTracking(img,https://wallpaperscraft.com/image.jpg)*)""><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Macro appears, attr disappears", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<a href=""http://google.com"">http://google.com</a>";
			expected = FormattableString.Invariant($@"<a href=""http://google.com"">http://google.com</a><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Same text", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<a href=""http://google.com"" tid=""Gogle"">http://google.com</a>";
			expected = FormattableString.Invariant($@"<a href=""(*LinkTracking(Gogle,http://google.com)*)"">http://google.com</a><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Macro appears, attr disappears", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<a tid=""Gogle"">http://google.com</a>";
			expected = FormattableString.Invariant($@"<a>http://google.com</a><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("No macro appears, attr disappears", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));
		}

		public void TestDecodeDocumentBlob_AddRegistryTrackingImage()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			var actual = @"";
			var expected = FormattableString.Invariant($@"<img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">");
			var actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Pixel image added", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<img src=""https://wallpaperscraft.com/image.jpg"">";
			expected = FormattableString.Invariant($@"<img src=""https://wallpaperscraft.com/image.jpg""><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Pixel image added to the end", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<html><head><meta charset=""utf-8""></head><body style=""BACKGROUND-COLOR: #575a5d"" scroll=""auto""><a href=""http://www.google.com.au"">AAA</a></body></html>";
			expected = FormattableString.Invariant($@"<html><head><meta charset=""utf-8""></head><body style=""BACKGROUND-COLOR: #575a5d"" scroll=""auto""><a href=""http://www.google.com.au"">AAA</a><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)""></body></html>");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Pixel image added before body close tag", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<html><head><meta charset=""utf-8""></head></html>";
			expected = FormattableString.Invariant($@"<html><head><meta charset=""utf-8""></head><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)""></html>");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Same text", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Pixel image added before html close tag", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));
		}

		public void TestAddRegistryTrackingImageLinkIfNotExists()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var originalLinkCount = campaign.TrackedLinks.Count;

			CampaignEmailTemplateEditor.AddTrackingImageLinkIfNotExists(campaign);
			AssertEquals("New link should be added", originalLinkCount + 1, campaign.TrackedLinks.Count);

			var newLinks = campaign.TrackedLinks.Where(x => x.GCL_Context == CampaignEmailTemplateEditor.TrackingImageContext);
			AssertEquals("Should only have 1 link with this ID", 1, newLinks.Count());

			var link = newLinks.FirstOrDefault();
			AssertEquals("URL should match", "http://easy.me/tracking.png", link.GCL_URL);
			AssertEquals(campaign.PK, link.GCL_G0_Campaign);

			CampaignEmailTemplateEditor.AddTrackingImageLinkIfNotExists(campaign);
			AssertEquals("No new links should be added for this campaign", originalLinkCount + 1, campaign.TrackedLinks.Count);
		}

		public void TestDecodeDocumentBlob_RemoveContentEditableAttribute()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			var actual = @"<html><head><meta charset=""utf-8""></head><body contenteditable=""true"" style=""BACKGROUND-COLOR: #575a5d"" scroll=""auto""><a href=""http://www.google.com.au"">AAA</a></body></html>";
			var actualBlob = ZBlob.FromAscii(actual);
			var expected = FormattableString.Invariant($@"<html><head><meta charset=""utf-8""></head><body style=""BACKGROUND-COLOR: #575a5d"" scroll=""auto""><a href=""http://www.google.com.au"">AAA</a><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)""></body></html>");
			AssertEquals("Contenteditable should be removed", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<html><head><meta charset=""utf-8""></head><body contenteditable=""false"" style=""BACKGROUND-COLOR: #575a5d"" scroll =""auto""><a href=""http://www.google.com.au"">AAA</a></body></html>";
			actualBlob = ZBlob.FromAscii(actual);
			expected = FormattableString.Invariant($@"<html><head><meta charset=""utf-8""></head><body style=""BACKGROUND-COLOR: #575a5d"" scroll=""auto""><a href=""http://www.google.com.au"">AAA</a><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)""></body></html>");
			AssertEquals("Contenteditable should be removed", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));
		}

		public void TestDecodeDocumentBlob_RemoveEmbeddedImages()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			using (var tempDirectory = new TempDirectory())
			{
				var fileName = new Uri(Path.Combine(tempDirectory.DirectoryName, "logo.svg")).LocalPath;
				var actual = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""{fileName}"" embedded=""true""> &nbsp;
</body></html>";
				var actualBlob = ZBlob.FromAscii(actual);
				var expected = FormattableString.Invariant($@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""""> &nbsp;
<img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)""></body></html>");
				AssertEquals("img src should be removed", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

				actual = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""{fileName}"" embedded=""false""> &nbsp;
</body></html>";
				actualBlob = ZBlob.FromAscii(actual);
				expected = FormattableString.Invariant($@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""""> &nbsp;
<img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)""></body></html>");
				AssertEquals("img src should be removed", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));
			}
		}

		public void TestDecodeDocumentBlob_SetImageWidthAndHeightFromStyle()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			var expectedLinkTrackingImage = $@"<img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking({CampaignEmailTemplateEditor.TrackingImageContext},http://easy.me/tracking.png)*)"">";

			var actual = @"<img src=""https://wallpaperscraft.com/image.jpg"" style=""width: 480px; height: 192px"">";
			var expected = FormattableString.Invariant($@"<img src=""https://wallpaperscraft.com/image.jpg"" style=""width: 480px; height: 192px"" width=""480"" height=""192"">{expectedLinkTrackingImage}");
			var actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Pixel Units used Decode Only", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Pixel Units used", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<img src=""https://wallpaperscraft.com/image.jpg"" style=""width: 480px; height: 192px"" width=""100"" height=""200"">";
			expected = FormattableString.Invariant($@"<img src=""https://wallpaperscraft.com/image.jpg"" style=""width: 480px; height: 192px"" width=""480"" height=""192"">{expectedLinkTrackingImage}");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Pixel Units used override image width and height, Decode Only", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Pixel Units used override image width and height", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));

			actual = @"<img src=""https://wallpaperscraft.com/image.jpg"" style=""width: 40em; height: 60em"">";
			expected = FormattableString.Invariant($@"<img src=""https://wallpaperscraft.com/image.jpg"" style=""width: 40em; height: 60em"">{expectedLinkTrackingImage}");
			actualBlob = ZBlob.FromAscii(actual);
			AssertEquals("Font Size units used Decode Only", actual, CampaignEmailTemplateEditor.DecodeDocumentBlob(actualBlob));
			AssertEquals("Font Size Units used", expected, CampaignEmailTemplateEditor.DecodeDocumentBlobAndGetHtmForEmail(actualBlob));
		}

		public void TestEmbeddedAndTidAttributesAreRemovedWhenSent()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			using (var tempDirectory = new TempDirectory())
			{
				var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
				var editor = new CampaignEmailTemplateEditor(campaign);
				string initialHtml;

				var fileName = new Uri(Path.Combine(tempDirectory.DirectoryName, "logo.svg")).LocalPath;
				initialHtml = $@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""{fileName}"" embedded=""true""> &nbsp;
<img src=""https://wallpaperscraft.com/image.jpg"" tid=""img"" embedded=""false"">	
<a href=""http://google.com"" tid=""Gogle"">http://google.com</a>
</body></html>";

				editor.TemplateHtmlText = initialHtml;
				editor.SyncImageTrackIDAttributeAndTrackingMacro();
				editor.SetCampaignEmailTemplate();

				var expected = FormattableString.Invariant($@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""""> &nbsp;
<img src=""(*LinkTracking(img,https://wallpaperscraft.com/image.jpg)*)"">	
<a href=""(*LinkTracking(Gogle,http://google.com)*)"">http://google.com</a>
<img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""(*LinkTracking(TrackingImage,http://easy.me/tracking.png)*)""></body></html>");

				AssertMultilineASCIIEquals("Macro appears, all attr disappears", expected, campaign.HtmlTextToSend);

				Factory.Save();

				campaign.Reload();

				expected = FormattableString.Invariant($@"
<html><head><meta charset=""utf-8""></head><body>
<img src=""""> &nbsp;
<img src=""https://wallpaperscraft.com/image.jpg"" tid=""img"" embedded=""false"">	
<a href=""http://google.com"" tid=""Gogle"">http://google.com</a>
</body></html>");
				AssertMultilineASCIIEquals("Embedded images has been removed as well", expected, CampaignEmailTemplateEditor.DecodeDocumentBlob(campaign.HtmlDocumentBlob));
			}
		}

		public void TestResizeTablesForDesignSuspender()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			var html = @"<html><head></head><body><a href='https://www.wisetechglobal.com/' tid='wisetech'>Wisetech Global</a></body></html>";
			editor.TemplateHtmlText = html;
			var link = editor.TrackedLinks.OfType<GlbCompanyCampaignLink>().Single();
			var isSuspended = false;
			editor.ForceHtmlUpdate += (sender, e) => isSuspended = editor.ResizeTablesForDesignSuspender.IsSuspended;

			AssertEquals(false, isSuspended);
			link.GCL_Context = "newContext";
			AssertEquals(true, isSuspended);
		}

		[ExpectNoExceptions]
		public void TestModifyTrackedLinkUrl()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var editor = new CampaignEmailTemplateEditor(campaign);
			var html = @"<html><head></head><body><a href='https://www.wisetechglobal.com/' tid='wisetech'>Wisetech Global</a></body></html>";
			editor.TemplateHtmlText = html;
			var link = editor.TrackedLinks.Cast<GlbCompanyCampaignLink>().Single();
			link.GCL_IsTracked = true;
			editor.SetCampaignEmailTemplate();

			link.UnicodeUrl = "htps://www.wisetechglobal.com/";

			var duplicateLink = campaign.TrackedLinks.AddNew();
			duplicateLink.GCL_Context = link.GCL_Context;
			duplicateLink.GCL_URL = "https://www.wisetechglobal.com/";

			link.UnicodeUrl = "https://www.wisetechglobal.com/";
		}

		public void TestIsGeneratedFormWordButNotFiltered()
		{
			var htmlText = "<html><head></head><body></body></html>";
			byte[] bytes = Encoding.UTF8.GetBytes(htmlText);
			AssertEquals("No generator specified", false, CampaignEmailTemplateEditor.IsGeneratedFromWordButNotFiltered(bytes));

			htmlText = "<html><head><meta name=Generator></head><body></body></html>";
			bytes = Encoding.UTF8.GetBytes(htmlText);
			AssertEquals("generator but no content specified", false, CampaignEmailTemplateEditor.IsGeneratedFromWordButNotFiltered(bytes));

			htmlText = @"<html><head><meta name=Generator content=""Some other Application""></head><body></body></html>";
			bytes = Encoding.UTF8.GetBytes(htmlText);
			AssertEquals("generator is some other application", false, CampaignEmailTemplateEditor.IsGeneratedFromWordButNotFiltered(bytes));

			htmlText = @"<html><head><meta name=Generator content=""Microsoft Word 15""></head><body></body></html>";
			bytes = Encoding.UTF8.GetBytes(htmlText);
			AssertEquals("generator is Word and web page isn't filtered", true, CampaignEmailTemplateEditor.IsGeneratedFromWordButNotFiltered(bytes));

			htmlText = @"<html><HEAD><meta name=GENERATOR content=""Microsoft Word 15""></HEAD><body></body></html>";
			bytes = Encoding.UTF8.GetBytes(htmlText);
			AssertEquals("generator is Word and web page isn't filtered, case insensitive", true, CampaignEmailTemplateEditor.IsGeneratedFromWordButNotFiltered(bytes));

			htmlText = @"<html><head><meta name=Generator content=""Microsoft Word 15 (filtered)""></head><body></body></html>";
			bytes = Encoding.UTF8.GetBytes(htmlText);
			AssertEquals("generator is Word and web page is filtered", false, CampaignEmailTemplateEditor.IsGeneratedFromWordButNotFiltered(bytes));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			return new CampaignEmailTemplateEditor(campaign);
		}
	}
}
