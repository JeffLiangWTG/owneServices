using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.CustomerService;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.GlbReleaseNote;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbReleaseNote))]
	sealed class GlbReleaseNoteTest : EnterpriseBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[StressTest]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		public void TestDefaultValues()
		{
			var note = Factory.New<GlbReleaseNote>();
			AssertZDatesWithin5Minutes("Release Date", ZDateTime.Today, note.GF_ReleaseNoteDate);
		}

		public void TestFetchHint()
		{
			GlbReleaseNote note = Factory.New<GlbReleaseNote>();
			GlbReleaseNoteRead read1 = Factory.New<GlbReleaseNoteRead>();
			read1.GR_ReleaseNoteID = note.PK;
			read1.GR_GS_Staff = GlbStaff.CurrentUser.PK;

			GlbReleaseNote note2 = Factory.New<GlbReleaseNote>();
			GlbReleaseNoteRead read2 = Factory.New<GlbReleaseNoteRead>();
			read2.GR_ReleaseNoteID = note2.PK;
			read2.GR_GS_Staff = GlbStaff.CurrentUser.PK;

			IFetchHint hint = new ReleaseNoteReadFetchHint(note);
			ZQuery query = hint.GetQuery();
			AssertEquals("GR_GS_Staff = CONVERT('" + GlbStaff.CurrentUser.PK + "', 'System.Guid') and GR_ReleaseNoteID = CONVERT('" + note.PK + "', 'System.Guid')", query.LiteralTextADO);

			var builder = new QueryBuilder();
			hint.GenerateQuery(builder);
			AssertEquals("GR_GS_Staff = '" + GlbStaff.CurrentUser.PK + "' and GR_ReleaseNoteID = '" + note.PK + "'", builder.ToString());
		}

		public void TestDelete()
		{
			GlbReleaseNote note = Factory.New<GlbReleaseNote>();
			GlbReleaseNoteRead read = Factory.New<GlbReleaseNoteRead>();
			read.GR_ReleaseNoteID = note.PK;
			read.GR_GS_Staff = GlbStaff.CurrentUser.PK;

			note.Delete();
			Assert(note.IsDeleted);
			Assert(read.IsDeleted);
		}

		public void TestDelete_CascadeDelete()
		{
			var note1 = Factory.NewWithValidTestData<GlbReleaseNote>();
			var read1 = Factory.New<GlbReleaseNoteRead>();
			read1.GR_ReleaseNoteID = note1.PK;
			read1.GR_GS_Staff = GlbStaff.CurrentUser.PK;
			Factory.Save();

			note1.Delete();
			Assert(note1.IsDeleted);
			Assert(read1.IsDeleted);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var read2 = factory2.New<GlbReleaseNoteRead>();
			var staff2 = factory2.New<GlbStaff>();
			staff2.GS_Code = "G01";
			read2.GR_ReleaseNoteID = note1.PK;
			read2.GR_GS_Staff = staff2.PK;

			factory2.Save();
			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			AssertNull(factory3.Load<GlbReleaseNote>(note1.PK));
			AssertNull(factory3.Load<GlbReleaseNoteRead>(read1.PK));
			AssertNull(factory3.Load<GlbReleaseNoteRead>(read2.PK));
		}

		public void TestTitle()
		{
			GlbReleaseNote note = Factory.New<GlbReleaseNote>();
			note.GF_Section = "";

			note.GF_Summary = "New Feature in ediEnterprise: Hi there\r\nSomething happened\r\nnewline";
			AssertEquals("New Feature in ediEnterprise: Hi there\r", note.Title);

			note.GF_Summary = "New Feature in ediEnterprise Hi there\nSomething happened\nnewline";
			AssertEquals("New Feature in ediEnterprise Hi there", note.Title);

			note.GF_Section = "C1U";

			note.GF_Summary = "New Feature in ediEnterprise: Hi there\r\nSomething happened\r\nnewline";
			AssertEquals("New Feature in ediEnterprise", note.Title);

			note.GF_Summary = "New Feature in ediEnterprise Hi there\nSomething happened\nnewline";
			AssertEquals("New Feature in ediEnterprise Hi there", note.Title);

			note.GF_Summary = "";
			AssertEquals("", note.Title);

			note.GF_Summary = "Short title";
			AssertEquals("Short title", note.Title);
		}

		public void TestSummaryForWeb()
		{
			GlbReleaseNote note = Factory.New<GlbReleaseNote>();
			note.GF_Summary = "New Feature in ediEnterprise: Hi there\r\nSomething happened\thello";
			AssertEquals("New Feature in ediEnterprise: Hi there<br />Something happened&nbsp;&nbsp;&nbsp;&nbsp;hello", note.SummaryForWeb);

			note.GF_Summary = "New Feature in ediEnterprise: Hi there\nSomething happened\thello";
			AssertEquals("New Feature in ediEnterprise: Hi there<br />Something happened&nbsp;&nbsp;&nbsp;&nbsp;hello", note.SummaryForWeb);

			note.GF_Summary = "";
			AssertEquals("", note.SummaryForWeb);
		}

		public void TestGetDownloadProtectedURL()
		{
			Registry.Business.WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Registry.Business.WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/myaccount/");

			string wiseTechNoteURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";

			var noteProductUpdate = Factory.New<GlbReleaseNote>();
			noteProductUpdate.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			noteProductUpdate.GF_URL = wiseTechNoteURL;

			var noteWiseLearningUpdate = Factory.New<GlbReleaseNote>();
			noteWiseLearningUpdate.GF_Section = NewsSectionTypeList.Codes.WiseLearningUpdates;
			noteWiseLearningUpdate.GF_URL = wiseTechNoteURL;

			var noteWiseNews = Factory.New<GlbReleaseNote>();
			noteWiseNews.GF_Section = NewsSectionTypeList.Codes.WiseNews;
			noteWiseNews.GF_URL = wiseTechNoteURL;

			var newsClientAnnouncement = Factory.New<GlbReleaseNote>();
			newsClientAnnouncement.GF_Section = NewsSectionTypeList.Codes.ClientAnnouncements;
			newsClientAnnouncement.GF_URL = "http://www.google.com";

			var newsClient = Factory.New<GlbReleaseNote>();
			newsClient.GF_Section = NewsSectionTypeList.Codes.ClientNews;
			newsClient.GF_URL = "http://www.yahoo.com";

			var newsClientStaff = Factory.New<GlbReleaseNote>();
			newsClientStaff.GF_Section = NewsSectionTypeList.Codes.ClientStaffNews;
			newsClientStaff.GF_URL = "http://www.live.com";

			// Custom types are possible via NewsSectionTypes registry
			var newsOther = Factory.New<GlbReleaseNote>();
			newsOther.GF_Section = "OTH";
			newsOther.GF_URL = "http://www.duckduckgo.com";

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "DDD";
			registrationKey.ServerCodeForTest = "TST";
			registrationKey.DatabaseNumberForTest = 1200;
			GlbCompany.CurrentCompany.GC_Code = "AAA";

			IStaffContactConverter converter = ObjectFactory.Get<IStaffContactConverter>();
			SecureQueryString queryString = converter.CurrentStaffAndRegistrationToSecuredQueryString();
			queryString["file"] = noteProductUpdate.GF_URL;
			queryString["language"] = EnvProxy.Instance.CurrentUser.Language;
			string expectedUrl = Registry.Business.WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/') + "/Download.aspx?qdata=" + WebUtility.UrlEncode(queryString.ToString());

			AssertEquals("noteProductUpdate.GetDownloadProtectedURL()", expectedUrl, noteProductUpdate.GetDownloadURL());
			AssertEquals("noteWiseLearningUpdate.GetDownloadProtectedURL()", expectedUrl, noteWiseLearningUpdate.GetDownloadURL());
			AssertEquals("noteWiseNews.GetDownloadProtectedURL()", expectedUrl, noteWiseNews.GetDownloadURL());
			AssertEquals("newsClientAnnouncement.GetDownloadProtectedURL()", newsClientAnnouncement.GF_URL, newsClientAnnouncement.GetDownloadURL());
			AssertEquals("newsClient.GetDownloadProtectedURL()", newsClient.GF_URL, newsClient.GetDownloadURL());
			AssertEquals("newsClientStaff.GetDownloadProtectedURL()", newsOther.GF_URL, newsOther.GetDownloadURL());
			AssertEquals("newsOther.GetDownloadProtectedURL()", newsOther.GF_URL, newsOther.GetDownloadURL());
		}

		public void TestGetDownloadProtectedURL_NoUser()
		{
			Registry.Business.WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Registry.Business.WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/myaccount/");

			string wiseTechNoteURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";

			var noteProductUpdate = Factory.New<GlbReleaseNote>();
			noteProductUpdate.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			noteProductUpdate.GF_URL = wiseTechNoteURL;

			var noteWiseLearningUpdate = Factory.New<GlbReleaseNote>();
			noteWiseLearningUpdate.GF_Section = NewsSectionTypeList.Codes.WiseLearningUpdates;
			noteWiseLearningUpdate.GF_URL = wiseTechNoteURL;

			var noteWiseNews = Factory.New<GlbReleaseNote>();
			noteWiseNews.GF_Section = NewsSectionTypeList.Codes.WiseNews;
			noteWiseNews.GF_URL = wiseTechNoteURL;

			var newsClientAnnouncement = Factory.New<GlbReleaseNote>();
			newsClientAnnouncement.GF_Section = NewsSectionTypeList.Codes.ClientAnnouncements;
			newsClientAnnouncement.GF_URL = "http://www.google.com";

			var newsClient = Factory.New<GlbReleaseNote>();
			newsClient.GF_Section = NewsSectionTypeList.Codes.ClientNews;
			newsClient.GF_URL = "http://www.yahoo.com";

			var newsClientStaff = Factory.New<GlbReleaseNote>();
			newsClientStaff.GF_Section = NewsSectionTypeList.Codes.ClientStaffNews;
			newsClientStaff.GF_URL = "http://www.live.com";

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "DDD";
			registrationKey.ServerCodeForTest = "TST";
			registrationKey.DatabaseNumberForTest = 1200;
			GlbCompany.CurrentCompany.GC_Code = "AAA";

			using (Env.SetTemporaryUserContext(null))
			{
				AssertNoExceptionThrown(delegate
				{ var x = noteProductUpdate.GetDownloadURL(); });
			}
		}

		public void TestGetDownloadURL_IsWiseTechGlobalItemViaTrustedMessaging()
		{
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/myaccount/");
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			string wiseTechNoteURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";

			var noteProductUpdate = Factory.New<GlbReleaseNote>();
			noteProductUpdate.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			noteProductUpdate.GF_URL = wiseTechNoteURL;
			AssertEquals(true, noteProductUpdate.IsWiseTechGlobalItemViaTrustedMessaging);

			using (ObjectFactory.Substitute<IUserPortalClient>(new TrustedMessaging.Intergration.Testing.UserPortalClientForTest()))
			{
				AssertEquals("http://www.cw1.com/autologin.aspx?token=123&return=http://www.cargowise.com/myaccount/Download.aspx?file=http%253A%252F%252Fwww.cargowise.com%252FDocuments%252FUpdateNotes%252FediEnterpriseupdatenote20100204a.pdf&language=EN", noteProductUpdate.GetDownloadURL());
			}

			var mock = new Mock<IUserPortalClient>();
			mock.Setup(m => m.GetMyAccountAutoLoginUrl(It.IsAny<Uri>())).Returns((Uri)null);
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertNull(mock.Object.GetMyAccountAutoLoginUrl(null));
				AssertEquals("http://www.cargowise.com/myaccount/Download.aspx?file=http%253A%252F%252Fwww.cargowise.com%252FDocuments%252FUpdateNotes%252FediEnterpriseupdatenote20100204a.pdf&language=EN", noteProductUpdate.GetDownloadURL());
			}
		}

		public void TestGetDownloadURL_IsWiseTechGlobalItemViaTrustedMessaging_NoUser()
		{
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/myaccount/");
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			string wiseTechNoteURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";

			var noteProductUpdate = Factory.New<GlbReleaseNote>();
			noteProductUpdate.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			noteProductUpdate.GF_URL = wiseTechNoteURL;
			AssertEquals(true, noteProductUpdate.IsWiseTechGlobalItemViaTrustedMessaging);

			using (Env.SetTemporaryUserContext(null))
			using (ObjectFactory.Substitute<IUserPortalClient>(new TrustedMessaging.Intergration.Testing.UserPortalClientForTest()))
			{
				AssertNoExceptionThrown(delegate
				{ var x = noteProductUpdate.GetDownloadURL(); });
			}
		}

		public void TestIsWiseTechGlobalItem()
		{
			CombineAssertions(() =>
			{
				AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.ProductUpdates, true);
				AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.WiseLearningUpdates, true);
				AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.WiseNews, true);
				AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes, true);
				AssertIsWiseTechGlobalItem("XXX", false);
			});
		}

		void AssertIsWiseTechGlobalItem(string section, bool isWiseTechGlobalItem)
		{
			var noteProductUpdate = Factory.New<GlbReleaseNote>();
			noteProductUpdate.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			AssertEquals(true, noteProductUpdate.IsWiseTechGlobalItem);
		}

		public void TestIsCurrentlyRead()
		{
			AssertEquals("Precondition: IsCurrentlyRead should be false.", false, Note.IsCurrentlyRead);

			GlbStaff staff = Factory.New<GlbStaff>();
			GlbReleaseNote anotherNote1 = Factory.New<GlbReleaseNote>();
			GlbReleaseNoteRead readNoteLink1 = Factory.New<GlbReleaseNoteRead>();
			GlbReleaseNoteRead readNoteLink2 = Factory.New<GlbReleaseNoteRead>();

			readNoteLink1.GR_ReleaseNoteID = Note.PK;
			readNoteLink1.GR_GS_Staff = staff.PK;
			readNoteLink2.GR_ReleaseNoteID = anotherNote1.PK;
			readNoteLink2.GR_GS_Staff = GlbStaff.CurrentUser.PK;

			Note.ClearNoteReadIsLoaded();
			AssertEquals("IsCurrentlyRead", false, Note.IsCurrentlyRead);
			AssertEquals("anotherNote1.IsCurrentlyRead", true, anotherNote1.IsCurrentlyRead);

			GlbReleaseNoteRead readNoteLink3 = Factory.New<GlbReleaseNoteRead>();
			readNoteLink3.GR_ReleaseNoteID = Note.PK;
			readNoteLink3.GR_GS_Staff = GlbStaff.CurrentUser.PK;

			Note.ClearNoteReadIsLoaded();
			AssertEquals("IsCurrentlyRead", true, Note.IsCurrentlyRead);
			AssertEquals("anotherNote1.IsCurrentlyRead", true, anotherNote1.IsCurrentlyRead);

			GlbReleaseNote anotherNote2 = Factory.New<GlbReleaseNote>();
			GlbReleaseNoteRead readNoteLink4 = Factory.New<GlbReleaseNoteRead>();
			readNoteLink4.GR_ReleaseNoteID = anotherNote2.PK;
			readNoteLink4.GR_GS_Staff = GlbStaff.CurrentUser.PK;
			using (Env.SetTemporaryUserContext(null))
			{
				Note.ClearNoteReadIsLoaded();
				AssertEquals("anotherNote2.IsCurrentlyRead", false, anotherNote2.IsCurrentlyRead);
			}
		}

		public void TestSettingIsReadWhenStaffExist()
		{
			ZQuery filter = new ZQuery(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
			filter.AddToFilter(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, Note.PK);

			GlbReleaseNoteRead read = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
			AssertNull(read);

			Note.FillWithValidTestData();
			Note.IsCurrentlyRead = true;
			Factory.Save();

			read = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
			AssertNotNull(read);
		}

		public void TestSettingIsReadWhenStaffNotExist()
		{
			ZQuery filter = new ZQuery(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
			filter.AddToFilter(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, Note.PK);

			GlbReleaseNoteRead read = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
			AssertNull(read);

			using (Env.SetTemporaryUserContext(null))
			{
				Note.FillWithValidTestData();
				Note.IsCurrentlyRead = true;
				Factory.Save();
			}

			read = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
			AssertNull(read);
		}

		public void TestCategoryDescription()
		{
			foreach (LicenceCheckpoint licence in Env.Licence.GetAllCheckpoints())
			{
				Note.GF_Category = licence.Name;
				AssertEquals("CategoryDisplayName", licence.DisplayName, Note.CategoryDisplayName);
			}

			Note.GF_Category = "";
			AssertEquals("CategoryDisplayName", "", Note.CategoryDisplayName);

			Note.GF_Category = "!@#";
			AssertEquals("CategoryDisplayName", "", Note.CategoryDisplayName);
		}

		public void TestReadNoteLinksAreDeletedWhenNoteIsDeleted()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			GlbReleaseNote anotherNote = Factory.New<GlbReleaseNote>();
			GlbReleaseNoteRead readNoteLink1 = Factory.New<GlbReleaseNoteRead>();
			GlbReleaseNoteRead readNoteLink2 = Factory.New<GlbReleaseNoteRead>();
			GlbReleaseNoteRead readNoteLink3 = Factory.New<GlbReleaseNoteRead>();

			readNoteLink1.GR_ReleaseNoteID = Note.PK;
			readNoteLink1.GR_GS_Staff = staff.PK;
			readNoteLink2.GR_ReleaseNoteID = Note.PK;
			readNoteLink2.GR_GS_Staff = GlbStaff.CurrentUser.PK;
			readNoteLink3.GR_ReleaseNoteID = anotherNote.PK;
			readNoteLink3.GR_GS_Staff = GlbStaff.CurrentUser.PK;

			Note.Delete();
			AssertEquals("ReadNoteLink1.IsDeleted", true, readNoteLink1.IsDeleted);
			AssertEquals("ReadNoteLink2.IsDeleted", true, readNoteLink2.IsDeleted);
			AssertEquals("ReadNoteLink3.IsDeleted", false, readNoteLink3.IsDeleted);

			anotherNote.Delete();
			AssertEquals("ReadNoteLink1.IsDeleted", true, readNoteLink1.IsDeleted);
			AssertEquals("ReadNoteLink2.IsDeleted", true, readNoteLink2.IsDeleted);
			AssertEquals("ReadNoteLink3.IsDeleted", true, readNoteLink3.IsDeleted);
		}

		public void TestGF_SummaryMultilingual()
		{
			var releaseNote = Factory.NewWithValidTestData<GlbReleaseNote>();

			using (var mockData = Res.UseMockData())
			{
				var key = releaseNote.GF_SummaryInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Freight Services");
				mockData.Put(key, new ResourceStringData(key, "货运服务"));
				mockData.Put(releaseNote.GF_SummaryInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Air Freight Services"), new ResourceStringData(key, "空运服务"));

				releaseNote.GF_Summary = "Freight Services";
				AssertEquals("货运服务", releaseNote.GF_SummaryMultilingual);

				releaseNote.GF_Summary = "Air Freight Services";
				AssertEquals("空运服务", releaseNote.GF_SummaryMultilingual);
			}
		}

		public void TestCodePropertyAttribute()
		{
			var attribute = typeof(GlbReleaseNote)
				.GetCustomAttributes(typeof(CodePropertyAttribute), false)
				.Cast<CodePropertyAttribute>()
				.Single();

			Assert("GF_Category used in CodePropertyAttribute", attribute.PropertyName == "GF_Category");
		}

		public void TestDescriptionPropertyAttribute()
		{
			var attribute = typeof(GlbReleaseNote)
				.GetCustomAttributes(typeof(DescriptionPropertyAttribute), false)
				.Cast<DescriptionPropertyAttribute>()
				.Single();

			Assert("GF_Summary used in DescriptionPropertyAttribute", attribute.PropertyName == "GF_Summary");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void Test_ThumbnailImage()
		{
			var releaseNote = Factory.NewWithValidTestData<GlbReleaseNote>();
			var image = new Bitmap(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\Testing\40kb.bmp"));

			releaseNote.ThumbnailImage = image;
			AssertEquals(BitmapToByteArray(image), releaseNote.GF_Thumbnail);
		}

		byte[] BitmapToByteArray(Bitmap image)
		{
			using var memoryStream = new MemoryStream();
			image.Save(memoryStream, ImageFormat.Bmp);
			return memoryStream.ToArray();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void Test_ThumbnailValidation()
		{
			var normalImage = new Bitmap(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\Testing\40kb.bmp"));
			var largeImage = new Bitmap(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\Testing\160kb.bmp"));
			var releaseNote = Factory.NewWithValidTestData<GlbReleaseNote>();

			releaseNote.ThumbnailImage = normalImage;
			releaseNote.Validation.ValidateGF_Thumbnail();
			AssertNoErrors(releaseNote.GF_ThumbnailInfo);

			releaseNote.ThumbnailImage = largeImage;
			releaseNote.Validation.ValidateGF_Thumbnail();
			AssertHasError(releaseNote.GF_ThumbnailInfo, "This image is too large. It should be an image with a size no greater than 64KB.");

			releaseNote.GF_Thumbnail = new ZBlob(new byte[] { 1, 2, 3 });
			releaseNote.Validation.ValidateGF_Thumbnail();
			AssertHasError(releaseNote.GF_ThumbnailInfo, "The image supplied is invalid or corrupt.");
		}

		GlbReleaseNote note;
		GlbReleaseNote Note => note ?? (note = Factory.New<GlbReleaseNote>());
	}
}
