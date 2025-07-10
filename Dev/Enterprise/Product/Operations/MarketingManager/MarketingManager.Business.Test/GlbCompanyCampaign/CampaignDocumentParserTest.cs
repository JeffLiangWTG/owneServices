using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class CampaignDocumentParserTest : TestCaseWithFactory
	{
		public void TestCampaignDocumentParser()
		{
			GlbCompanyCampaignItem item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Factory.New<GlbCompanyCampaign>().PK;
			item.CompanyCampaign.G0_CampaignName = "AAA Campaign";

			CampaignDocumentParser parser = new CampaignDocumentParser(Factory);
			AssertEquals("Type of wrapper supported", ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocCompanyCampaignItem>(), parser.InternalTypeofWrapperForTest());

			AssertEquals("Parsing should not throw exception", "Parse this", parser.Parse(CampaignDocumentParser.ParseType.PlainText, item, "Parse this"));
			AssertEquals("Parsing should support doc builder syntax", "YES", parser.Parse(CampaignDocumentParser.ParseType.PlainText, item, "(*IF(\"(*CompanyCampaign.G0_CampaignName*)\"==\"AAA Campaign\", \"YES\", \"NO\")*)"));
		}

		public void TestParseForPreviewingHtmlPage()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "N1";
			staff.GS_FriendlyName = "N1";
			staff.GS_FullName = "N1 N2";
			staff.GS_WorkPhone = "(02) 1111 1111";
			staff.GS_MobilePhone = "1111 111 111";
			staff.GS_EmailAddress = "n1@noemail.com";
			staff.GS_Title = "Product Manager";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Hello Campaign with sender option set";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;

			var item1 = campaign.CampaignsItemsSent.AddNew();
			var html = @"<html><body><img src='img1.png' macro='(*EmailSenderStaff.ProfilePhotoEncoded*)' width='100' height='200' style='HEIGHT: 200px; WIDTH: 100px'/></body></html>";

			var parser = new CampaignDocumentParser(Factory);
			var outputHtml = parser.Parse(CampaignDocumentParser.ParseType.HtmlPreview, item1, html);
			AssertEquals("staff does not have profile image yet", "<html><body></body></html>", outputHtml);

			staff.ProfileImage = new Bitmap(1, 1);
			outputHtml = parser.Parse(CampaignDocumentParser.ParseType.HtmlPreview, item1, html);
			var base64String = Convert.ToBase64String(staff.GS_ProfilePhoto);
			AssertEquals(@"<html><body><img src='data:image/png;base64," + base64String + @"' width='100' style=' WIDTH: 100px'></body></html>", outputHtml);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			outputHtml = parser.Parse(CampaignDocumentParser.ParseType.HtmlPreview, item1, html);
			AssertEquals("<html><body></body></html>", outputHtml);
		}

		public void TestParseForSendingHtmlEmail()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "N1";
			staff.GS_FriendlyName = "N1";
			staff.GS_FullName = "N1 N2";
			staff.GS_WorkPhone = "(02) 1111 1111";
			staff.GS_MobilePhone = "1111 111 111";
			staff.GS_EmailAddress = "n1@noemail.com";
			staff.GS_Title = "Product Manager";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Hello Campaign with sender option set";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;

			var item1 = campaign.CampaignsItemsSent.AddNew();
			var html = @"<html><body><img src='data:image/png;base64' macro='(*EmailSenderStaff.ProfilePhotoEncoded*)' width='100' height='200' style='HEIGHT: 200px; WIDTH: 100px'/></body></html>";
			var embedInHtmlImages = new Dictionary<string, byte[]>();

			var parser = new CampaignDocumentParser(Factory);
			var outputHtml = parser.Parse(CampaignDocumentParser.ParseType.HtmlEmail, item1, html, embedInHtmlImages);
			AssertEquals("staff does not have profile image yet", "<html><body></body></html>", outputHtml);
			AssertEquals(0, embedInHtmlImages.Count);

			embedInHtmlImages.Clear();
			staff.ProfileImage = new Bitmap(1, 1);
			outputHtml = parser.Parse(CampaignDocumentParser.ParseType.HtmlEmail, item1, html, embedInHtmlImages);
			AssertEquals(1, embedInHtmlImages.Count);
			AssertEquals($"<html><body><img src='{embedInHtmlImages.Keys.First()}' width='100' style=' WIDTH: 100px'></body></html>", outputHtml);

			var bytes = embedInHtmlImages.Values.First();
			AssertEquals(58, bytes.Length);
			AssertEquals('B', bytes[0]);
			AssertEquals('M', bytes[1]);

			embedInHtmlImages.Clear();
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			outputHtml = parser.Parse(CampaignDocumentParser.ParseType.HtmlEmail, item1, html, embedInHtmlImages);
			AssertEquals("<html><body></body></html>", outputHtml);
			AssertEquals(0, embedInHtmlImages.Count);
		}

		public void TestParseForEmbeddedImages()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Hello Campaign with sender option set";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;

			var item1 = campaign.CampaignsItemsSent.AddNew();
			var html = @"<html><body>
<img src='data:image/jpg;base64,LS0t' />
<img src='data:image/aaa;base64,LS0t' />
<img src='dat:image/jpg;base64,LS0t' />
<img src='data:image/jpg;bas64,LS0t' />
<img src='data:image/jpg;base64LS0t' />
<img src='data:image/jpg;base64LS0ta' />
<img src='data:image/png;base64,LS0t' />
</body></html>";
			var embedInHtmlImages = new Dictionary<string, byte[]>();

			var parser = new CampaignDocumentParser(Factory);
			var outputHtml = parser.Parse(CampaignDocumentParser.ParseType.HtmlEmail, item1, html, embedInHtmlImages);

			AssertEquals("Number of valid embedded images", 2, embedInHtmlImages.Count);
			AssertContains(embedInHtmlImages.Keys.First(), outputHtml);
			AssertContains(embedInHtmlImages.Keys.Last(), outputHtml);
		}
	}
}
