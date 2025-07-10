using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if NETFRAMEWORK
using System.Web;
#endif
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Registry;
using Moq;

namespace Enterprise.Recruitment.Testing.Common
{
	sealed class ConvertApiResumeConverterTest : TestCaseWithFactory
	{
		public void TestConvertToPdfAsync()
		{
			// arrange
			var input = new MemoryStream(Encoding.ASCII.GetBytes("hello world"));
			var convertApi = new TestConvertApiImpl();
			var converter = new ConvertApiResumeConverter(null, convertApi);

			// act
			converter.ConvertToPdfAsync("DOCX", input).Wait();

			// assert
			AssertContainsExactElementsInAnyOrder(new List<string>() { "hello world" }, convertApi.Converted);
		}

		public void TestUnseekableStreams()
		{
			var convertApi = new TestConvertApiImpl(new NoSeekMemoryStream());
			var converter = new ConvertApiResumeConverter(null, convertApi);
			var applicant = CreateApplicantWithDocuments((CVDocType.RT_DocType, "ConvertMe.docx"));

			converter.ConvertAvailableResumes(applicant.Applications[0]);
			AssertContainsExactElementsInAnyOrder(new[] { "ConvertMe.docx" }, convertApi.Converted);
		}

		public void TestExceptionThrownFromConversionService()
		{
			var logs = new List<DummyLogEventArgs>();
			var logger = new DummyLogger();
			logger.OnLog += (a, b) => logs.Add(b);

			var convertApi = new Mock<IConvertApi>();
#if NETFRAMEWORK
			convertApi.Setup(m => m.ConvertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>())).Throws(new HttpException("Boo hoo web server down"));
#else
			convertApi.Setup(m => m.ConvertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>())).Throws(new ArgumentException("Boo hoo web server down"));
#endif

			var converter = new ConvertApiResumeConverter(logger, convertApi.Object);
			var applicant = CreateApplicantWithDocuments((CVDocType.RT_DocType, "ConvertMe.docx"));

			AssertNoExceptionThrown("We should handle errors when converting and simply log", () => converter.ConvertAvailableResumes(applicant.Applications[0]));
			var relevantLog = logs.FirstOrDefault(l => l.Message.Contains("Boo hoo web server down"));
			AssertNotNull("When failing to convert a resume, we should log what occured", relevantLog);
			AssertEquals("Log should be a warning", LogType.Warning, relevantLog.Type);
		}

		public void TestUnseekableStreams_WhenAttaching()
		{
			var convertApi = new TestConvertApiImpl(new NoSeekMemoryStream());
			var converter = new ConvertApiResumeConverter(null, convertApi);
			var applicant = CreateApplicantWithDocuments((CVDocType.RT_DocType, "ConvertMe.docx"));

			var contents = new NoSeekMemoryStream(Encoding.UTF8.GetBytes("Here is some dummy data"));
			AssertNoExceptionThrown("Network streams are often not seekable, so we need to support them", () => converter.AttachEdocCore(applicant.Applications[0], "test.pdf", contents, "RES"));
		}

		public void TestConvertsEdocWithResumeDoctype()
		{
			var applicant = CreateApplicantWithDocuments((CVDocType.RT_DocType, "ConvertMe.docx"), ("MSC", "Email.msg"));
			var convertApi = new TestConvertApiImpl();
			var converter = new ConvertApiResumeConverter(null, convertApi);
			converter.ConvertAvailableResumes(applicant.Applications[0]);

			AssertContainsExactElementsInAnyOrder(new[] { "ConvertMe.docx" }, convertApi.Converted);
		}

		public void TestNoConvertIfAlreadySupported_PDF()
		{
			var applicant = CreateApplicantWithDocuments((CVDocType.RT_DocType, "Resume.PDF"), ("MSC", "Email.msg"));

			var convertApi = new TestConvertApiImpl();
			var converter = new ConvertApiResumeConverter(null, convertApi);
			converter.ConvertAvailableResumes(applicant.Applications[0]);

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), convertApi.Converted);
		}

		public void TestShouldConvertInPreference()
		{
			var applicant = CreateApplicantWithDocuments((CVDocType.RT_DocType, "Email.eml"), (CVDocType.RT_DocType, "Email.msg"), (CVDocType.RT_DocType, "ConvertMe.docx"), (CVDocType.RT_DocType, "ConvertMe.doc"));
			AssertConvertedEdoc(applicant, "ConvertMe.doc");

			var applicant1 = CreateApplicantWithDocuments((CVDocType.RT_DocType, "Email.eml"), (CVDocType.RT_DocType, "Email.msg"), (CVDocType.RT_DocType, "ConvertMe.docx"));
			AssertConvertedEdoc(applicant1, "ConvertMe.docx");

			var applicant2 = CreateApplicantWithDocuments((CVDocType.RT_DocType, "Email.eml"), (CVDocType.RT_DocType, "Email.msg"));
			AssertConvertedEdoc(applicant2, "Email.msg");

			var applicant3 = CreateApplicantWithDocuments((CVDocType.RT_DocType, "Email.eml"));
			AssertConvertedEdoc(applicant3, "Email.eml");

			var applicant4 = CreateApplicantWithDocuments(("EML", "Email.eml"), ("MSC", "Email.msg"), ("DOX", "ConvertMe.docx"), ("DOC", "ConvertMe.doc"));
			AssertConvertedEdoc(applicant4);

			var applicant5 = CreateApplicantWithDocuments(("EML", "Email.eml"), (CVDocType.RT_DocType, "Email.msg"), ("DOX", "ConvertMe.docx"), ("DOC", "ConvertMe.doc"));
			AssertConvertedEdoc(applicant5, "Email.msg");

			var applicant6 = CreateApplicantWithDocuments();
			AssertConvertedEdoc(applicant6);
		}

		static void AssertConvertedEdoc(HRJobApplicant applicant, string convertedDocExpected = null)
		{
			List<string> expectedInOrder = convertedDocExpected != null ? new List<string> { convertedDocExpected } : new List<string>();

			var convertApi = new TestConvertApiImpl();
			var converter = new ConvertApiResumeConverter(null, convertApi);
			converter.ConvertAvailableResumes(applicant.Applications[0]);
			AssertContainsExactElementsInAnyOrder(expectedInOrder, convertApi.Converted);
		}

		public void TestUsesSecretKey()
		{
			using (RecruitmentDataRegistry.Instance.ConvertApiSecretKey.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "BlahBlah"))
			{
				var converter = new ConvertApiResumeConverter(null);
				AssertEquals("BlahBlah", ((ConvertApiImpl)converter.ConvertApi).Key);
			}
		}

		public void TestThrowsOnDodgyFromFormat()
		{
			var converter = new ConvertApiImpl("notnull");
			var ex = AssertExceptionThrown<AggregateException>(() => converter.ConvertAsync("foo.doc", "PDF", new MemoryStream()).Wait());
			AssertContains("Extension expected", ex.InnerException.Message);
			AssertContains("foo.doc", ex.InnerException.Message);
		}

		public void TestThrowsOnDodgyToFormat()
		{
			var converter = new ConvertApiImpl("notnull");
			var ex = AssertExceptionThrown<AggregateException>(() => converter.ConvertAsync("doc", "idunno.pdf", null).Wait());
			AssertContains("Extension expected", ex.InnerException.Message);
			AssertContains("idunno.pdf", ex.InnerException.Message);
		}

		public void TestThrowsOnDodgyStream()
		{
			var converter = new ConvertApiImpl("notnull");
			var ex = AssertExceptionThrown<AggregateException>(() => converter.ConvertAsync("doc", "PDF", null).Wait());
			AssertType<ArgumentNullException>(ex.InnerException);
		}

		public void TestUsesSecretKey_WarnInvalidKey()
		{
			RecruitmentDataRegistry.Instance.ConvertApiSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			AssertNoExceptionThrown(() =>
			{
				var converter = new ConvertApiResumeConverter(new DummyLogger());
				var applicant = CreateApplicantWithDocuments((CVDocType.RT_DocType, "Resume.docx"));

				converter.ConvertAvailableResumes(applicant.Applications[0]);
			});
		}

		public void TestCanConvert()
		{
			var thingsToConvert = new[] { ".doc", ".docx", ".DoCx", "MSg", ".msg", "eml" };
			var thingsToNotConvert = new[] { "pdf", ".pdf", "img", "jpg", "jpeg", "xyz" };

			var convertApi = new TestConvertApiImpl(new NoSeekMemoryStream());
			var converter = new ConvertApiResumeConverter(null, convertApi);
			CombineAssertions(() =>
			{
				foreach (var type in thingsToConvert)
				{
					Assert(FormattableString.Invariant($"We should support '{type}' for conversion"), converter.CanConvert(type));
				}

				foreach (var type in thingsToNotConvert)
				{
					Assert(FormattableString.Invariant($"We should NOT convert '{type}'"), !converter.CanConvert(type));
				}
			});
		}

		public void TestConvertedResumeIsEventuallySaved()
		{
			var emptyDocxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.docx", "empty.docx");

			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Test Name";
			applicant.HA_EmailAddress = "test1@wisetechglobal.com";

			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			var doc = application.DocManagerInfo.AddFileOrDocument(emptyDocxPath, "CVV");

			var queue = Factory.New<HRJobApplicationParsingQueue>();
			queue.HPQ_HP = application.PK;
			queue.HPQ_StorageDocReference = doc.UniqueKey;

			Factory.Save();
			application.DocManagerInfo.Save();

			RecruitmentDataRegistry.Instance.ConvertApiSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "notempty");

			var convertApi = new TestConvertApiImpl(new MemoryStream(Encoding.UTF8.GetBytes("I guess anything will do")));
			var converter = new ConvertApiResumeConverter(null, convertApi);

			converter.ConvertAvailableResumes(application);

			var otherFactory = new BusinessObjectFactory();
			var applicationOnOther = otherFactory.Load<HRJobApplication>(application.PK);

			AssertNotNull("The converter should have been called and the result should be saved in the eDocs", applicationOnOther.DocManagerInfo.AllEDocs.Cast<IeDoc>().SingleOrDefault(d => "empty.pdf".Equals(d.FileName, StringComparison.OrdinalIgnoreCase)));
		}

		HRJobApplicant CreateApplicantWithDocuments(params (string doctype, string filename)[] files)
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = applicant.Applications.AddNew();
			applicant.FillWithValidTestData();

			foreach (var (type, name) in files)
			{
				application.DocManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes(name), name, type);
			}

			return applicant;
		}

		protected override void SetUp()
		{
			base.SetUp();

			CVDocType = Factory.NewWithValidTestData<RefDocType>();
			CVDocType.RT_DocType = "CVV";

			ReferringSourceDocType = Factory.NewWithValidTestData<RefDocType>();
			ReferringSourceDocType.RT_DocType = "RSD";

			Factory.Save();

			RecruiterDataRegistry.Instance.DocTypeReferringSource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReferringSourceDocType.PK.ToGuid());
			RecruiterDataRegistry.Instance.DocTypeCV.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CVDocType.PK.ToGuid());
			RecruitmentDataRegistry.Instance.ConvertApiSecretKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NotBlank");
		}

		RefDocType CVDocType, ReferringSourceDocType;

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(Recruiter.Business.Testing.EmailParsingRuleTest).Assembly));

		class NoSeekMemoryStream : MemoryStream
		{
			public override bool CanSeek => false;

			public NoSeekMemoryStream(byte[] contents)
				: base(contents)
			{
			}
			public NoSeekMemoryStream()
				: base()
			{
			}
		}
	}
}
