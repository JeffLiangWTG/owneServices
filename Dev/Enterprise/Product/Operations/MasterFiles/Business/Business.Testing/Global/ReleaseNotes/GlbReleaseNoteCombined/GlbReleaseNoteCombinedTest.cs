using System;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.CustomerService;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(GlbReleaseNoteCombined))]
public class GlbReleaseNoteCombinedTest : EnterpriseBusinessObjectTestCase
{
	[StressTest]
	public override void TestBizObjectFields()
	{
		var note = CreateNoteWithDefaultValues(Factory);

		TestBizObjectFieldsCore(note);
	}

	public void TestDelete()
	{
		var note = Factory.New<GlbReleaseNoteCombined>();
		var read = Factory.New<GlbReleaseNoteRead>();
		read.GR_ReleaseNoteID = note.PK;
		read.GR_GS_Staff = GlbStaff.CurrentUser.PK;

		note.Delete();
		AssertEquals("GlbReleaseNoteCombined BizO should be marked as deleted", true, note.IsDeleted);
		AssertEquals("GlbReleaseNoteRead BizO should be marked as Deleted", true, read.IsDeleted);
	}

	public void TestDelete_CascadeDelete()
	{
		var note1 = CreateNoteWithDefaultValues(Factory);
		var read1 = Factory.New<GlbReleaseNoteRead>();
		read1.GR_ReleaseNoteID = note1.PK;
		read1.GR_GS_Staff = GlbStaff.CurrentUser.PK;
		Factory.Save();

		note1.Delete();
		AssertEquals("GlbReleaseNoteCombined BizO should be marked as deleted", true, note1.IsDeleted);
		AssertEquals("GlbReleaseNoteRead BizO should be marked as Deleted", true, read1.IsDeleted);

		var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
		var read2 = factory2.New<GlbReleaseNoteRead>();
		var staff2 = factory2.New<GlbStaff>();
		staff2.GS_Code = "G01";
		read2.GR_ReleaseNoteID = note1.PK;
		read2.GR_GS_Staff = staff2.PK;

		factory2.Save();
		Factory.Save();

		var factory3 = new BusinessObjectFactory();
		AssertNull(factory3.Load<GlbReleaseNoteCombined>(note1.PK));
		AssertNull(factory3.Load<GlbReleaseNoteRead>(read1.PK));
		AssertNull(factory3.Load<GlbReleaseNoteRead>(read2.PK));
	}

	public void TestTitle()
	{
		var note = Factory.New<GlbReleaseNoteCombined>();
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
		var note = Factory.New<GlbReleaseNoteCombined>();
		note.GF_Summary = "New Feature in ediEnterprise: Hi there\r\nSomething happened\thello";
		AssertEquals("New Feature in ediEnterprise: Hi there<br />Something happened&nbsp;&nbsp;&nbsp;&nbsp;hello", note.SummaryForWeb);

		note.GF_Summary = "New Feature in ediEnterprise: Hi there\nSomething happened\thello";
		AssertEquals("New Feature in ediEnterprise: Hi there<br />Something happened&nbsp;&nbsp;&nbsp;&nbsp;hello", note.SummaryForWeb);

		note.GF_Summary = "";
		AssertEquals("", note.SummaryForWeb);
	}

	#region GetDownloadURL

	public void TestGetDownloadURL()
	{
		WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/myaccount/");

		var wiseTechNoteUrl = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";

		var noteProductUpdate = Factory.New<GlbReleaseNoteCombined>();
		noteProductUpdate.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
		noteProductUpdate.GF_URL = wiseTechNoteUrl;

		var noteWiseLearningUpdate = Factory.New<GlbReleaseNoteCombined>();
		noteWiseLearningUpdate.GF_Section = NewsSectionTypeList.Codes.WiseLearningUpdates;
		noteWiseLearningUpdate.GF_URL = wiseTechNoteUrl;

		var noteWiseNews = Factory.New<GlbReleaseNoteCombined>();
		noteWiseNews.GF_Section = NewsSectionTypeList.Codes.WiseNews;
		noteWiseNews.GF_URL = wiseTechNoteUrl;

		var noteClientAnnouncement = Factory.New<GlbReleaseNoteCombined>();
		noteClientAnnouncement.GF_Section = NewsSectionTypeList.Codes.ClientAnnouncements;
		noteClientAnnouncement.GF_URL = "http://www.google.com";

		var noteNewsClient = Factory.New<GlbReleaseNoteCombined>();
		noteNewsClient.GF_Section = NewsSectionTypeList.Codes.ClientNews;
		noteNewsClient.GF_URL = "http://www.yahoo.com";

		var noteClientStaff = Factory.New<GlbReleaseNoteCombined>();
		noteClientStaff.GF_Section = NewsSectionTypeList.Codes.ClientStaffNews;
		noteClientStaff.GF_URL = "http://www.live.com";

		// Custom types are possible via NewsSectionTypes registry
		var noteOther = Factory.New<GlbReleaseNoteCombined>();
		noteOther.GF_Section = "OTH";
		noteOther.GF_URL = "http://www.duckduckgo.com";

		var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
		registrationKey.EnterpriseCodeForTest = "DDD";
		registrationKey.ServerCodeForTest = "TST";
		registrationKey.DatabaseNumberForTest = 1200;
		GlbCompany.CurrentCompany.GC_Code = "AAA";

		var converter = ObjectFactory.Get<IStaffContactConverter>();
		var queryString = converter.CurrentStaffAndRegistrationToSecuredQueryString();
		queryString["file"] = noteProductUpdate.GF_URL;
		queryString["language"] = EnvProxy.Instance.CurrentUser.Language;

		// TODO This unit test should be updated. The expectedUrl would be const string
		var expectedUrl = "http://www.cargowise.com/myaccount/Download.aspx?qdata=" + WebUtility.UrlEncode(queryString.ToString());

		AssertEquals("noteProductUpdate.GetDownloadURL()", expectedUrl, noteProductUpdate.GetDownloadURL());
		AssertEquals("noteWiseLearningUpdate.GetDownloadURL()", expectedUrl, noteWiseLearningUpdate.GetDownloadURL());
		AssertEquals("noteWiseNews.GetDownloadURL()", expectedUrl, noteWiseNews.GetDownloadURL());
		AssertEquals("noteClientAnnouncement.GetDownloadURL()", noteClientAnnouncement.GF_URL, noteClientAnnouncement.GetDownloadURL());
		AssertEquals("noteNewsClient.GetDownloadURL()", noteNewsClient.GF_URL, noteNewsClient.GetDownloadURL());
		AssertEquals("noteClientStaff.GetDownloadURL()", noteClientStaff.GF_URL, noteClientStaff.GetDownloadURL());
		AssertEquals("noteOther.GetDownloadURL()", noteOther.GF_URL, noteOther.GetDownloadURL());
	}

	public void TestGetDownloadURL_NoUser()
	{
		var noteWiseTechItem = Factory.New<GlbReleaseNoteCombined>();
		noteWiseTechItem.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
		noteWiseTechItem.GF_URL = "http://www.yahoo.com";

		using (Env.SetTemporaryUserContext(null))
		{
			AssertEquals("Value of GF_URL is returned", "http://www.yahoo.com", noteWiseTechItem.GetDownloadURL());
		}
	}

	public void TestGetDownloadURL_IsWiseTechGlobalItemViaTrustedMessaging()
	{
		WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.cargowise.com/myaccount/");
		WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

		var noteWiseTechItem = Factory.New<GlbReleaseNoteCombined>();
		noteWiseTechItem.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
		noteWiseTechItem.GF_URL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";

		AssertEquals(true, noteWiseTechItem.IsWiseTechGlobalItemViaTrustedMessaging);

		using (ObjectFactory.Substitute<IUserPortalClient>(new TrustedMessaging.Intergration.Testing.UserPortalClientForTest()))
		{
			AssertEquals("http://www.cw1.com/autologin.aspx?token=123&return=http://www.cargowise.com/myaccount/Download.aspx?file=http%253A%252F%252Fwww.cargowise.com%252FDocuments%252FUpdateNotes%252FediEnterpriseupdatenote20100204a.pdf&language=EN", noteWiseTechItem.GetDownloadURL());
		}

		var mock = new Mock<IUserPortalClient>();
		mock.Setup(m => m.GetMyAccountAutoLoginUrl(It.IsAny<Uri>())).Returns((Uri)null);

		using (ObjectFactory.Substitute(mock.Object))
		{
			AssertNull(mock.Object.GetMyAccountAutoLoginUrl(null));
			AssertEquals("http://www.cargowise.com/myaccount/Download.aspx?file=http%253A%252F%252Fwww.cargowise.com%252FDocuments%252FUpdateNotes%252FediEnterpriseupdatenote20100204a.pdf&language=EN", noteWiseTechItem.GetDownloadURL());
		}
	}

	#endregion

	public void TestIsWiseTechGlobalItem()
	{
		CombineAssertions(() =>
		{
			AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.ProductUpdates, true);
			AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.WiseLearningUpdates, true);
			AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.WiseNews, true);
			AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.ClientNews, false);
			AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.ClientAnnouncements, false);
			AssertIsWiseTechGlobalItem(NewsSectionTypeList.Codes.ClientStaffNews, false);
			AssertIsWiseTechGlobalItem("XXX", false);
		});
	}

	#region IsCurrentlyRead

	public void TestIsCurrentlyRead()
	{
		var note1 = Factory.NewWithValidTestData<GlbReleaseNoteCombined>();

		AssertEquals("Precondition: IsCurrentlyRead should be false.", false, note1.IsCurrentlyRead);

		var staff = Factory.New<GlbStaff>();
		var anotherNote1 = Factory.NewWithValidTestData<GlbReleaseNoteCombined>();
		var noteRead1 = Factory.New<GlbReleaseNoteRead>();
		var noteRead2 = Factory.New<GlbReleaseNoteRead>();

		noteRead1.GR_ReleaseNoteID = note1.PK;
		noteRead1.GR_GS_Staff = staff.PK;
		noteRead2.GR_ReleaseNoteID = anotherNote1.PK;
		noteRead2.GR_GS_Staff = GlbStaff.CurrentUser.PK;

		note1.ClearIsNoteReadLoaded();
		AssertEquals("IsCurrentlyRead", false, note1.IsCurrentlyRead);
		AssertEquals("anotherNote1.IsCurrentlyRead", true, anotherNote1.IsCurrentlyRead);

		var anotherNote2 = Factory.New<GlbReleaseNote>();
		var noteRead3 = Factory.New<GlbReleaseNoteRead>();
		noteRead3.GR_ReleaseNoteID = anotherNote2.PK;
		noteRead3.GR_GS_Staff = GlbStaff.CurrentUser.PK;

		using (Env.SetTemporaryUserContext(null))
		{
			note1.ClearIsNoteReadLoaded();
			AssertEquals("anotherNote2.IsCurrentlyRead", false, anotherNote2.IsCurrentlyRead);
		}
	}

	public void TestSettingIsCurrentlyRead_WhenStaffExists()
	{
		var note = Factory.NewWithValidTestData<GlbReleaseNoteCombined>();
		note.GF_Section = NewsSectionTypeList.Codes.ClientNews;

		var filter = new ZQuery(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
		filter.AddToFilter(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, note.PK);

		var read = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
		AssertNull(read);

		note.IsCurrentlyRead = true;

		read = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
		AssertNotNull(read);
	}

	public void TestSettingIsCurrentlyRead_WhenNoCurrentStaff()
	{
		var note = Factory.NewWithValidTestData<GlbReleaseNoteCombined>();
		note.GF_Section = NewsSectionTypeList.Codes.ClientNews;

		var filter = new ZQuery(GlbReleaseNoteReadSchema.GR_GS_Staff, GlbStaff.CurrentUser.PK);
		filter.AddToFilter(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, note.PK);

		var read = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
		AssertNull(read);

		using (Env.SetTemporaryUserContext(null))
		{
			note.IsCurrentlyRead = true;
		}

		read = Factory.LoadTop1<GlbReleaseNoteRead>(filter);
		AssertNull(read);
	}

	#endregion

	public void TestCategoryDescription()
	{
		var note = Factory.NewWithValidTestData<GlbReleaseNoteCombined>();

		foreach (var licence in Env.Licence.GetAllCheckpoints())
		{
			note.GF_Category = licence.Name;
			AssertEquals("CategoryDisplayName", licence.DisplayName, note.CategoryDisplayName);
		}

		note.GF_Category = "";
		AssertEquals("CategoryDisplayName", "", note.CategoryDisplayName);

		note.GF_Category = "!@#";
		AssertEquals("CategoryDisplayName", "", note.CategoryDisplayName);
	}

	public void TestGF_SummaryMultilingual()
	{
		var note = Factory.NewWithValidTestData<GlbReleaseNoteCombined>();
		using var mockData = Res.UseMockData();

		var key = note.GF_SummaryInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Freight Services");
		mockData.Put(key, new ResourceStringData(key, "货运服务"));
		mockData.Put(note.GF_SummaryInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Air Freight Services"), new ResourceStringData(key, "空运服务"));

		note.GF_Summary = "Freight Services";
		AssertEquals("货运服务", note.GF_SummaryMultilingual);

		note.GF_Summary = "Air Freight Services";
		AssertEquals("空运服务", note.GF_SummaryMultilingual);
	}

	public void TestCodePropertyAttribute()
	{
		var attribute = typeof(GlbReleaseNoteCombined)
			.GetCustomAttributes(typeof(CodePropertyAttribute), false)
			.Cast<CodePropertyAttribute>()
			.Single();

		AssertEquals("GF_Category used in CodePropertyAttribute", "GF_Category", attribute.PropertyName);
	}

	public void TestDescriptionPropertyAttribute()
	{
		var attribute = typeof(GlbReleaseNoteCombined)
			.GetCustomAttributes(typeof(DescriptionPropertyAttribute), false)
			.Cast<DescriptionPropertyAttribute>()
			.Single();

		AssertEquals("GF_Summary used in DescriptionPropertyAttribute", "GF_Summary", attribute.PropertyName);
	}

	public override void TestSaveAndDeleteBusinessObject()
	{
		Assert("It's already tested by TestDelete_CascadeDelete", true);
	}

	#region Implementations

	void AssertIsWiseTechGlobalItem(string section, bool isWiseTechGlobalItem)
	{
		var note = Factory.New<GlbReleaseNoteCombined>();
		note.GF_Section = section;

		AssertEquals(isWiseTechGlobalItem, note.IsWiseTechGlobalItem);
	}

	protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory) => CreateNoteWithDefaultValues(factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateNoteWithDefaultValues(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateNoteWithDefaultValues(factory);

	protected override BusinessObject GetNewBusinessObject() => CreateNoteWithDefaultValues(Factory);

	GlbReleaseNoteCombined CreateNoteWithDefaultValues(BusinessObjectFactory factory, string section = NewsSectionTypeList.Codes.ClientNews)
	{
		var note = factory.NewWithValidTestData<GlbReleaseNoteCombined>();
		note.GF_Section = section;
		note.GF_Summary = "New Feature in ediEnterprise";
		note.GF_URL = "http://www.CWTest.com";

		return note;
	}

	#endregion
}
