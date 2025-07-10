using System;
using System.IO;
using System.Web;
using CargoWise.Definitions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(DocumentRequestHandler))]
	[HttpContextEnabledTest]
	public abstract class DocumentRequestHandlerTestCase<T> : DataRequestHandlerTestCase<T>
			where T : DocumentRequestHelper, new()
	{
		#region TestContentType

		public virtual void TestContentType()
		{
			var handler = new DocumentRequestHandlerForTest<T>();

			var expectedContentTypes = new string[] { DataContentTypes.Pdf, DataContentTypes.Excel };
			AssertEquals("Unexpected content types count", expectedContentTypes.Length, handler.SupportedContentTypesForTest.Count);
			for (var i = 0; i < expectedContentTypes.Length; i++)
			{
				var msg = string.Format("Should support {0} content type", expectedContentTypes[i]);
				Assert(msg, handler.SupportedContentTypesForTest.Contains(expectedContentTypes[i]));
			}

			var initialContext = HttpContext.Current;
			try
			{
				AssertContentType(handler, DataContentTypes.Pdf, "PDF");
				AssertContentType(handler, DataContentTypes.Excel, "Excel");
				AssertContentType(handler, string.Empty, "PDF");

				HttpContext.Current = CustomContextHelperForTest.GetCustomContext(TrackingConstants.QueryStringKeys.ContentType, "foo");
				AssertExceptionThrown(typeof(NotSupportedException), () => { string temp = handler.ContentType; });
			}
			finally
			{
				HttpContext.Current = initialContext;
			}
		}

		void AssertContentType(DataRequestHandler<T> handler, string contentType, string contentTypeFriendlyName)
		{
			HttpContext.Current = CustomContextHelperForTest.GetCustomContext(TrackingConstants.QueryStringKeys.ContentType, contentType);

			var message = string.Format("{0} ContentType should be {1}",
					string.IsNullOrEmpty(contentType) ? "Default" : "",
					contentTypeFriendlyName);

			var expectedContentType = string.IsNullOrEmpty(contentType) ? DataContentTypes.Pdf : contentType;
			AssertEquals(message, expectedContentType, handler.ContentType);
		}

		#endregion

		#region TestFileName

		public virtual void TestFileName()
		{
			CreateDocumentCommandIfDoesntAlreadyExist();
			AssertNotNull("The request handler must be able to return BusinessObjects", RequestHandler.BusinessObjects);

			var initialContext = HttpContext.Current;
			try
			{
				AssertFileName(DataContentTypes.Pdf, ".pdf");
				AssertFileName(DataContentTypes.Excel, ".xls");
			}
			finally
			{
				HttpContext.Current = initialContext;
			}
		}

		void AssertFileName(string contentType, string fileExtension)
		{
			HttpContext.Current = CustomContextHelperForTest.GetCustomContext(TrackingConstants.QueryStringKeys.ContentType, contentType);
			AssertEquals("File name should have extension " + fileExtension, TestCommand.SU_MenuName + fileExtension, RequestHandler.FileName);
		}

		#endregion

		#region TestGetBinaryData

		public void TestGetBinaryData()
		{
			AssertGetBinaryData(RequestHandler, "IDocumentSupportable is retrieved from Session");
		}

		public void TestGetBinaryDataUsingDocumentSupportableCreator()
		{
			if (IsDocumentRequestHandler)
			{
				var context = "IDocumentSupportable is created using DocumentSupportableCreator";
				var handler = GetDocumentRequestHandler();
				CreateDocumentCommandIfDoesntAlreadyExist();

				handler.QueryString.Add(DataRequestHelper.DataKey, DummyDocumentSupportableBizOCreator.DocumentSupportablePK.ToString() + "," + TestCommand.PK.ToString());
				handler.QueryString.Add(TrackingConstants.QueryStringKeys.ContentType, DataContentTypes.Pdf);
				handler.QueryString.Add(TrackingConstants.QueryStringKeys.Helper, new QueryParamsEncoder().Encrypt(typeof(DummyDocumentSupportableBizOCreator).AssemblyQualifiedName));
				AssertGetBinaryData(handler, context + ", PDF document");

				handler.QueryString[TrackingConstants.QueryStringKeys.ContentType] = DataContentTypes.Excel;
				AssertGetBinaryData(handler, context + ", Excel document");
			}
			else
			{
				Assert(true);
			}
		}

		void AssertGetBinaryData(DataRequestHandler<T> handler, string context)
		{
			AssertNotNull(string.Format("The request handler must be able to return BusinessObjects. Context: {0}", context), handler.BusinessObjects);
			AssertEquals(string.Format("There should be 1 object. Context: {0}", context), 1, handler.BusinessObjects.Length);

			var docCommand = handler.BusinessObjects[0] as DocumentCommand;
			AssertNotNull(string.Format("The first and only object should be the DocumentCommand. Context: {0}", context), docCommand);

			AssertNotNull(string.Format("The documentCommand should have its parent set to the IDocumentSupportable. Context: {0}", context), docCommand.Parent);

			AssertNotEquals(string.Format("Should return some data. Context: {0}", context), ZBlob.Empty, handler.GetBinaryData());
		}

		public override void TestGetBinaryDataWithLock()
		{
			AssertNotNull("Nothing to lock in this class");
		}

		public void TestGetBinaryData_ChildMenus()
		{
			if (IsDocumentRequestHandler)
			{
				var context = "Child Menus";
				var handler = GetDocumentRequestHandler();
				CreateDocumentCommandWithChildrenMenusIfDoesntAlreadyExist();

				handler.QueryString.Add(DataRequestHelper.DataKey, DummyDocumentSupportableBizOCreator.DocumentSupportablePK.ToString() + "," + TestCommandWithChildren.PK.ToString());
				handler.QueryString.Add(TrackingConstants.QueryStringKeys.ContentType, DataContentTypes.Pdf);
				handler.QueryString.Add(TrackingConstants.QueryStringKeys.Helper, new QueryParamsEncoder().Encrypt(typeof(DummyDocumentSupportableBizOCreator).AssemblyQualifiedName));
				AssertGetBinaryData(handler, context + ", PDF document");

				handler.QueryString[TrackingConstants.QueryStringKeys.ContentType] = DataContentTypes.Excel;
				AssertGetBinaryData(handler, context + ", Excel document");
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool IsDocumentRequestHandler => true;

		public void TestGetDocumentPrintSet()
		{
			var handler = new DocumentRequestHandler();
			var dummy = Factory.New<DummyBODocSupportableWithCustomizedDocumentPrintSet>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;

			var printSet = handler.GetDocumentPrintSet(documentCommand);
			Assert("Should use the document supporter implements interface ISupportCustomizedDocumentPrintSet", printSet is DocumentPrintSetWithStreaming);
		}

		#endregion

		#region TestGetPdfDocumentRunsWithCorrectBranch

		public virtual void TestGetPdfDocumentRunsWithCorrectBranch()
		{
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			OrgHeader orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			newCompany.GC_OH_OrgProxy = orgProxy.PK;

			GlbBranch branchOne = Factory.NewWithValidTestData<GlbBranch>();
			branchOne.GB_GC = newCompany.PK;
			branchOne.GB_Code = "ONE";

			GlbBranch branchTwo = Factory.NewWithValidTestData<GlbBranch>();
			branchTwo.GB_GC = newCompany.PK;
			branchTwo.GB_Code = "TWO";

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CompanyData.OB_GB_ControllingBranch = branchOne.PK;

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = organisation.PK;

			Factory.Save();

			string loginName = Env.CurrentUser.LoginName;
			Guid branchPK = Env.CurrentBranch.PK;
			Guid departmentPK = Env.CurrentDepartment.PK;

			using (Env.SetTemporaryUserContext(loginName, branchTwo.PK.ToGuid(), departmentPK))
			{
				var testRequestHandler = new DocumentRequestHandlerForTest<T>();
				SetupForTest(testRequestHandler);

				testRequestHandler.Organisation = organisation;
				testRequestHandler.Contact = contact;

				AssertNotEquals("Precondition: Should return some data", ZBlob.Empty, testRequestHandler.GetBinaryData());
				AssertNotNull("Precondition: TestRequestHandler.Branch", testRequestHandler.Branch);
				AssertEquals("Current branch while Printing Document", branchOne.GB_Code, testRequestHandler.Branch.Code);
			}
		}

		#endregion

		#region Implementation

		protected override DataRequestHandler<T> GetNewRequestHandler()
		{
			var requestHandler = GetDocumentRequestHandler();
			SetupForTest(requestHandler);
			return requestHandler;
		}

		protected abstract DocumentRequestHandler<T> GetDocumentRequestHandler();

		protected virtual void SetupForTest(DocumentRequestHandler<T> requestHandler)
		{
			CreateDocumentCommandIfDoesntAlreadyExist();

			DocDummy = Factory.New<DummyShipmentBusinessObject>();
			Factory.Save();
			TestCommand.Parent = DocDummy;

			HttpContext.Current.Session[DocDummy.PK.ToString()] = DocDummy;

			requestHandler.QueryString.Add(DataRequestHelper.DataKey, DocDummy.PK.ToString() + "," + TestCommand.PK.ToString());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly));

		void CreateDocumentCommandIfDoesntAlreadyExist(BusinessContext businessContext = BusinessContext.Shipment)
		{
			if (TestCommand == null)
			{
				TestCommand = Factory.New<DocumentCommand>();
				TestCommand.SU_BusinessContext = businessContext.ToString();
				TestCommand.SU_IsPublished = true;
				TestCommand.SU_IsSystemDefined = true;
				TestCommand.SU_MenuName = "Pub System Shipment Document";
				TestCommand.SU_MenuIndex = 1;
				TestCommand.SU_MenuPath = "";
				TestCommand.SU_MenuShortcut = "CtrlF1";

				StmTemplateBase template = Factory.New<StmTemplateBase>();
				template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
				template.SO_IsSystemDefined = true;
				template.SO_Name = "System Shipment Template";
				var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with tabs3.xls", "UDF with tabs3.xls");
				var shipmentTemplate = new ExcelTemplateForUnitTesting("UDF with tabs3.xls", Path.GetFullPath(tempFileName));
				template.SO_Template = shipmentTemplate.GetAsByteArray();

				StmMenuTemplatePivot templatePivot1 = Factory.New<StmMenuTemplatePivot>();
				templatePivot1.SI_SO = template.PK;
				templatePivot1.SI_SU = TestCommand.PK;
				templatePivot1.SI_Index = 1;
				templatePivot1.SI_DocumentTitle = "Pub System Shipment Document 1";

				Factory.Save();
			}
		}

		void CreateDocumentCommandWithChildrenMenusIfDoesntAlreadyExist(BusinessContext businessContext = BusinessContext.Shipment)
		{
			CreateDocumentCommandIfDoesntAlreadyExist(businessContext);

			if (TestCommandWithChildren == null)
			{
				TestCommandWithChildren = Factory.New<DocumentCommand>();
				TestCommandWithChildren.SU_BusinessContext = businessContext.ToString();
				TestCommandWithChildren.SU_IsPublished = true;
				TestCommandWithChildren.SU_IsSystemDefined = true;
				TestCommandWithChildren.SU_MenuName = "Pub System Shipment Document With Children";
				TestCommandWithChildren.SU_MenuIndex = 1;
				TestCommandWithChildren.SU_MenuPath = "";
				TestCommandWithChildren.SU_MenuShortcut = "CtrlF1";

				var pivot = Factory.New<StmMenuMenuPivot>();
				pivot.SF_SU_Inward = TestCommandWithChildren.PK;
				pivot.SF_SU_Outward = TestCommand.PK;

				Factory.Save();
			}
		}

		DummyShipmentBusinessObject DocDummy;
		DocumentCommand TestCommand;
		DocumentCommand TestCommandWithChildren;

		#endregion
	}
}
