using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ReportRequestHandler))]
	[HttpContextEnabledTest]
	sealed class ReportRequestHandlerTest : DataRequestHandlerTestCase<ReportRequestHelper>
	{
		public void TestContentType()
		{
			var report = RequestHandler.BusinessObjects[0] as Report;
			AssertEquals("ContentType should be PDF by default", DataContentTypes.Pdf, ((ReportRequestHandler)RequestHandler).ContentType);

			report.SelectedFormatType = OrgConstants.AttachmentType.PDF;
			AssertEquals("ContentType should be PDF", DataContentTypes.Pdf, ((ReportRequestHandler)RequestHandler).ContentType);

			report.SelectedFormatType = OrgConstants.AttachmentType.PDFA;
			AssertEquals("ContentType should be PDF", DataContentTypes.Pdf, ((ReportRequestHandler)RequestHandler).ContentType);

			report.SelectedFormatType = OrgConstants.AttachmentType.XLS;
			AssertEquals("ContentType should be Excel", DataContentTypes.Excel, ((ReportRequestHandler)RequestHandler).ContentType);

			report.SelectedFormatType = OrgConstants.AttachmentType.XLSX;
			AssertEquals("ContentType should be Excel", DataContentTypes.Excel, ((ReportRequestHandler)RequestHandler).ContentType);

			report.SelectedFormatType = OrgConstants.AttachmentType.TIF;
			AssertEquals("ContentType should be TIFF", DataContentTypes.Tiff, ((ReportRequestHandler)RequestHandler).ContentType);
		}

		public void TestFormatType()
		{
			AssertEquals(DataContentTypes.Pdf, ((ReportRequestHandler)RequestHandler).ContentType);
		}

		public void TestFileName()
		{
			var report = RequestHandler.BusinessObjects[0] as Report;
			AssertNotNull("The request handler must be able to return BusinessObjects", RequestHandler.BusinessObjects);
			AssertNotNullOrEmpty(((IDocument)report).DocumentName);

			var expectedFileNamePattern = ((IDocument)report).DocumentName + ".{0}";
			AssertEquals(string.Format(expectedFileNamePattern, OrgConstants.AttachmentType.PDF), RequestHandler.FileName);

			report.SelectedFormatType = OrgConstants.AttachmentType.PDF;
			AssertEquals(string.Format(expectedFileNamePattern, OrgConstants.AttachmentType.PDF), RequestHandler.FileName);

			report.SelectedFormatType = OrgConstants.AttachmentType.PDFA;
			AssertEquals(string.Format(expectedFileNamePattern, OrgConstants.AttachmentType.PDF), RequestHandler.FileName);

			report.SelectedFormatType = OrgConstants.AttachmentType.XLS;
			AssertEquals(string.Format(expectedFileNamePattern, OrgConstants.AttachmentType.XLS), RequestHandler.FileName);

			report.SelectedFormatType = OrgConstants.AttachmentType.XLSX;
			AssertEquals(string.Format(expectedFileNamePattern, OrgConstants.AttachmentType.XLSX), RequestHandler.FileName);

			report.SelectedFormatType = OrgConstants.AttachmentType.TIF;
			AssertEquals(string.Format(expectedFileNamePattern, OrgConstants.AttachmentType.TIF), RequestHandler.FileName);
		}

		public void TestGetBinaryData()
		{
			DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var report = RequestHandler.BusinessObjects[0] as Report;

			Assert("Should return PDF data by default", ImageToPDFConverter.IsPDF(RequestHandler.GetBinaryData()));

			report.SelectedFormatType = OrgConstants.AttachmentType.PDF;
			Assert("Should return PDF data", ImageToPDFConverter.IsPDF(RequestHandler.GetBinaryData()));
			Assert("Should not return PDF/A data", !ImageToPDFConverter.IsPDFA(RequestHandler.GetBinaryData()));

			report.SelectedFormatType = OrgConstants.AttachmentType.PDFA;
			Assert("Should return PDF/A data", ImageToPDFConverter.IsPDFA(RequestHandler.GetBinaryData()));

			report.SelectedFormatType = OrgConstants.AttachmentType.TIF;
			Assert("Should return TIFF data", ImageToPDFConverter.IsTiff(RequestHandler.GetBinaryData()));

			report.SelectedFormatType = OrgConstants.AttachmentType.XLS;
			AssertExcelFormat("Should return XLS data", OrgConstants.AttachmentType.XLS, RequestHandler.GetBinaryData());

			report.SelectedFormatType = OrgConstants.AttachmentType.XLSX;
			AssertExcelFormat("Should return XLSX data", OrgConstants.AttachmentType.XLSX, RequestHandler.GetBinaryData());

			RequestHandler.BusinessObjects[0] = null;
			var res = RequestHandler.GetBinaryData();
			AssertEquals("Length should be zero", 0, res.Length);
		}

		void AssertExcelFormat(string message, string expectedExcelFormat, byte[] excelData)
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(excelData);
				AssertEquals(message, expectedExcelFormat, excelInterface.GetExtensionForExcelFromFile());
			}
		}

		public override void TestWithBinaryData()
		{
			using (var memoryStream = new MemoryStream())
			{
				var testResponseFilter = new TestResponseFilter(HttpContext.Current.Response.Filter, memoryStream);
				HttpContext.Current.Response.Filter = testResponseFilter;
				RequestHandler.ProcessRequest(HttpContext.Current);

				HttpContext.Current.Response.Flush();
				HttpContext.Current.Response.End();

				AssertEquals("The content type should be the specified content type.", RequestHandler.ContentType, HttpContext.Current.Response.ContentType);
				AssertNotNull("The Content-Disposition header should exist.", ApplicationInstance.WorkerRequest.Headers["Content-Disposition"]);

				AssertEquals("The Content-Disposition header should specify and attachment and contain the file name.",
					ExpectedContentDispositionForTestWithBinaryData, ApplicationInstance.WorkerRequest.Headers["Content-Disposition"]);

				byte[] binaryData = RequestHandler.GetBinaryData();
				AssertEquals("The binary data length should be the same as the length of the returned data.", memoryStream.Length, binaryData.Length);
			}
		}

		#region Implementation

		protected override DataRequestHandler<ReportRequestHelper> GetNewRequestHandler()
		{
			var testRequestHandler = new ReportRequestHandlerForTesting();
			testRequestHandler.QueryStringForTesting.Add(DataRequestHelper.DataKey, reportIndex.ToString());
			return testRequestHandler;
		}

		class ReportRequestHandlerForTesting : ReportRequestHandler
		{
			public NameValueCollection QueryStringForTesting
			{
				get { return base.QueryString; }
			}
		}

		DocumentPack pack;
		ZGuid reportIndex;
		Report reportForTest;
		OrgContactWebUser siteUser;
		OrgContact testContact;
		EmbeddedResourceRetriever resourceRetriever;

		protected override void SetUp()
		{
			base.SetUp();
			pack = new DocumentPack(Factory.New<StmMenuItem>());
			reportIndex = ZGuid.NewZGuid();

			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();

			resourceRetriever = new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly);
			var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate", Path.GetFullPath(tempFileName));
			reportForTest = new Report(pack, excelTemplate, TestData.GetFirstGuidInTestTable(), Core.Constants.DataContext.UnitTest, "TestReport");

			testContact = Factory.NewWithValidTestData<OrgContact>();
			testContact.OC_Email = "test@cargowise.com";
			testContact.OC_WebAccessEnabled = true;
			testContact.SetHashedPassword("test");
			Factory.Save();

			siteUser = new OrgContactWebUser();
			siteUser.Login(testContact.Header.OH_Code, "test@cargowise.com", "test");
			Assert("Should be Logged In", siteUser.IsLoggedIn);

			HttpContext.Current.Session[reportIndex.ToString()] = reportForTest;
			HttpContext.Current.Session["SiteUser"] = siteUser;
		}

		protected override void TearDown()
		{
			base.TearDown();
			reportForTest.Dispose();
			resourceRetriever.Dispose();
			temporarilyUseMainConnection?.Dispose();
		}

		IDisposable temporarilyUseMainConnection;

		#endregion
	}
}
