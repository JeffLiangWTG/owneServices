using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class eDocAttachPageTest : ZFileUploadDialogTest
	{
		public void TestProcessUploadedFile()
		{
			var page = new eDocAttachDummyPage();
			page.BeforeDataSourceFactorySaved += (_, e) => AssertUploadedFile(page, "IMP", "Important Document");
			SetupAndProcessUploadedFile(page, "IMP");

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			AssertUploadedFileLog(page);
		}

		public void TestProcessUploadedFileWithUnknownDocType()
		{
			var page = new eDocAttachDummyPage();
			page.BeforeDataSourceFactorySaved += (_, e) => AssertUploadedFile(page, "UNK", ZString.Empty);
			SetupAndProcessUploadedFile(page, "UNK");

			AssertEquals("Document type UNK is not in the list of available document types. (Available types: IMP,OTH)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertUploadedFileLog(page);
		}

		public void TestAuthorisedDivVisibleWhenHasDocTypes()
		{
			var page = new eDocAttachDummyPage();
			page.OnLoadForTest();

			Assert(page.AuthorisedContentInternal.Visible);
			Assert(!page.UnauthorisedDivInternal.Visible);
			AssertEquals(string.Empty, page.UnauthorisedLabelInternal.Text);
			Assert(!page.DisableOKButtonInternal);
			Assert(page.OKButtonInternal.Enabled);
		}

		public void TestUnauthorisedDivVisibleWhenNoDocTypes()
		{
			var page = new eDocAttachDummyPage();
			page.DocUploadManager.DocumentUploadHelper.DocTypes.DeleteAll();
			page.OnLoadForTest();

			Assert(!page.AuthorisedContentInternal.Visible);
			Assert(page.UnauthorisedDivInternal.Visible);
			AssertEquals("You do not have access to any Document Types. Please contact your system administrator to request access rights.", page.UnauthorisedLabelInternal.Text);
			Assert(page.DisableOKButtonInternal);
			Assert(!page.OKButtonInternal.Enabled);
		}

		public void TestMaximumFileSize()
		{
			using (SystemDataRegistry.Instance.eDocsMaximumFilesize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var mb = 1024 * 1024;
				var page = new eDocAttachDummyPage();
				var contents = new byte[mb + 1];
				SetupAndProcessUploadedFile(page, "IMP", contents);
				AssertEquals(0, page.DocUploadManager.DocManagerInfo.Files.Count);

				contents = new byte[mb];
				SetupAndProcessUploadedFile(page, "IMP", contents);
				AssertEquals(1, page.DocUploadManager.DocManagerInfo.Files.Count);
			}
		}

		public void TestMaximumFileSizeClientValues()
		{
			using (SystemDataRegistry.Instance.eDocsMaximumFilesize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var page = new eDocAttachDummyPage();
				page.maximumFileSizeHiddenInternal = new HtmlInputHidden();
				page.maximumFileSizeMessageHiddenInternal = new HtmlInputHidden();

				page.OnLoadForTest();

				AssertEquals((1024 * 1024).ToString(), page.maximumFileSizeHiddenInternal.Value);
				AssertEquals("The selected file exceeds the 1MB maximum size allowed for Documents.", page.maximumFileSizeMessageHiddenInternal.Value);
			}
		}

		void AssertUploadedFile(eDocAttachPage page, string expectedDocType, string expectedDocDescription)
		{
			AssertEquals(1, page.DocUploadManager.DocManagerInfo.Files.Count);
			var file = page.DocUploadManager.DocManagerInfo.Files[0] as StorageFile;
			AssertNotNull(file);
			AssertEquals(expectedDocType, file.SC_DocType);
			AssertEquals(expectedDocDescription, file.SC_Desc);
			AssertEquals("someDoc.doc", file.SC_FileNameWithExtension);
			AssertEquals(true, file.SC_IsPublished);
			Assert(file.IsInDatabase);
		}

		void AssertUploadedFileLog(eDocAttachPage page)
		{
			var logs = page.DocUploadManager.DocManagerInfo.BusinessEntity.GetLogs();
			AssertEquals(logs.MostRecentLog.SL_SE_NKEvent, AutoEvents.DocumentImportedCode);
		}

		void SetupAndProcessUploadedFile(eDocAttachPage page, string uploadType, byte[] contents = null)
		{
			page.DocUploadManager.DocumentUploadHelper.DocType = uploadType;
			page.DocUploadManager.DocumentUploadHelper.DocTypes.DeleteAll();
			var docType = page.DocUploadManager.DocumentUploadHelper.DocTypes.AddNew();
			docType.RT_DocType = "IMP";
			docType.RT_Desc = "Important Document";
			docType = page.DocUploadManager.DocumentUploadHelper.DocTypes.AddNew();
			docType.RT_DocType = "OTH";
			docType.RT_Desc = "Other Document";

			page.ProcessUploadedFileForTest(contents ?? new byte[] { 1, 2, 3 }, "someDoc.doc");
		}

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.eDocAttach;
		}

		protected override Control GetNewControl()
		{
			eDocAttachPage result = new eDocAttachPage();
			result.CreateChildControlsForTest();
			result.EnableViewState = true;
			return result;
		}

		protected override string GetExpOKButtonText()
		{
			return "Upload & Attach";
		}

		protected override bool OKButtonIsEnabled => false;

		public void TestProcessRequestAccessDeny()
		{
			var page = new eDocAttachPageAccessDenyTest();

			ZArchitecture.Web.GUI.Testing.TestGlobal.AssertReloadPage(() => page.ProcessRequest(System.Web.HttpContext.Current));
		}

		public void TestCannotSaveException_ReloadsPage()
		{
			var page = new eDocAttachDummyPage();
			void onSaving(BusinessObjectFactory factory)
			{
				page.DocUploadManager.DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs.Saving -= onSaving;
				throw new ZCannotSaveException("Test - Cannot Save", "Test Exception");
			}
			page.DocUploadManager.DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs.Saving += onSaving;

			ZArchitecture.Web.GUI.Testing.TestGlobal.AssertReloadPage(() => SetupAndProcessUploadedFile(page, "IMP"));
		}

		public class eDocAttachPageAccessDenyTest : eDocAttachPage
		{
			protected override void OnInit(EventArgs e)
			{
			}

			public override void Dispose()
			{
				throw new IOException(@"The process cannot access the file 'C:\ediWebTrackerWG2\Germany\HSLDUS\Forwarding\app_data\NeatUpload_Temp\bb7029b3443e4681ab2b849459f1803d.config' because it is being used by another process."); // Exception for Test
			}
		}
	}
}
