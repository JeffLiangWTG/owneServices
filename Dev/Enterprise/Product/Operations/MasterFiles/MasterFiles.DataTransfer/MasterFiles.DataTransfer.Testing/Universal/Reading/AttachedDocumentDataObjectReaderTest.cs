using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class AttachedDocumentDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReadAttachedDocuments_IsPublishedDefaultedToDocTypeIfNotSpecified()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "JTD";
			docType.RT_ReferenceType = "ALL";
			docType.RT_IsPublished = true;
			AssertEDocIsPublished(true);

			docType.RT_IsPublished = false;
			AssertEDocIsPublished(false);
		}

		public void TestAddAttachedDocument_WithEmptyFileName()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var document = new AttachedDocument())
			{
				document.FileName = "   ";
				document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
				document.Type = new DocumentType { Code = "JTD", Description = "DummyDescription" };

				AssertExceptionThrown<DataObjectReadFailureException>("AttachedDocumentDataObjectReader should validate file name and path", "Invalid file name and path: \"   \".", () => new AttachedDocumentDataObjectReader().TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _));
			}
		}

		public void TestAddAttachedDocument_WithNullImageData()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var document = new AttachedDocument())
			{
				document.FileName = "1.doc";
				document.ImageData = null;
				document.Type = new DocumentType { Code = "JTD", Description = "DummyDescription" };

				AssertExceptionThrown<DataObjectReadFailureException>("AttachedDocumentDataObjectReader should prevent empty file", "File cannot be empty.", () => new AttachedDocumentDataObjectReader().TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _));
			}
		}

		public void TestAddAttachedDocument_WithEmptyImageData()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var document = new AttachedDocument())
			{
				document.FileName = "1.doc";
				document.ImageData = (SubStreamableStream)new MemoryStream(System.Array.Empty<byte>());
				document.Type = new DocumentType { Code = "JTD", Description = "DummyDescription" };

				AssertExceptionThrown<DataObjectReadFailureException>("AttachedDocumentDataObjectReader should prevent empty file", "File cannot be empty.", () => new AttachedDocumentDataObjectReader().TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _));
			}
		}

		public void TestAddAttachedDocument_WithInvalidFileName()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var document = new AttachedDocument())
			{
				document.FileName = ":123.doc";
				document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
				document.Type = new DocumentType { Code = "JTD", Description = "DummyDescription" };

				AssertExceptionThrown<DataObjectReadFailureException>("AttachedDocumentDataObjectReader should validate file name and path", "Invalid file name and path: \":123.doc\".", () => new AttachedDocumentDataObjectReader().TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _));
			}
		}

		public void TestAddAttachedDocument_WithDangerousFileType()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var document = new AttachedDocument())
			{
				document.FileName = "123.cmd";
				document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
				document.Type = new DocumentType { Code = "JTD", Description = "DummyDescription" };

				AssertExceptionThrown<DataObjectReadFailureException>("AttachedDocumentDataObjectReader should validate dangerous file type", "File cannot be added because it has a potentially dangerous file type.", () => new AttachedDocumentDataObjectReader().TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _));
			}
		}

		void AssertEDocIsPublished(bool isPublished)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(0, ((IDocManagerSupport)orgHeader).DocManagerInfo.AllEDocs.Count);
			Factory.SaveForTesting();

			using (var document = new AttachedDocument())
			{
				document.FileName = "Hahaha";
				document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
				document.Type = new DocumentType { Code = "JTD", Description = "Hahaha" };

				var reader = new AttachedDocumentDataObjectReader();

				using (Factory.BOFactory.AddDisposableService())
				{
					reader.TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _);
					Factory.SaveForTesting();
					var reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);
					AssertEquals("eDocs are saved with Main Factory", 1, ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs.Count);
					var eDoc = ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs[0];
					AssertEquals("eDoc.IsPublished should be defaulted to docType's if not specified", isPublished, eDoc.IsPublished);
				}
			}
		}

		public void TestReadAttachedDocuments()
		{
			var docSource = Factory.NewWithValidTestData<RefDocSource>();
			docSource.RDS_Code = "AZA";
			docSource.RDS_Desc = "Aaaaa!";

			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(0, ((IDocManagerSupport)orgHeader).DocManagerInfo.AllEDocs.Count);
			Factory.SaveForTesting();

			using (var document = new AttachedDocument())
			{
				document.FileName = "Hahaha";
				document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
				document.Type = new DocumentType { Code = "HHH", Description = "Hahaha" };
				document.Source = new CodeDescriptionPair { Code = "AZA", Description = "Aaaaa!" };
				document.IsPublished = true;
				document.VisibleBranchCode = GlbBranch.CurrentBranch.GB_Code;
				document.VisibleCompanyCode = GlbCompany.CurrentCompany.GC_Code;
				document.VisibleDepartmentCode = GlbDepartment.CurrentDepartment.GE_Code;

				var reader = new AttachedDocumentDataObjectReader();

				using (Factory.BOFactory.AddDisposableService())
				using (var docImageData = document.ImageData.Copy())
				{
					reader.TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _);
					Factory.SaveForTesting();
					var reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);
					AssertEquals("eDocs are saved with Main Factory", 1, ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs.Count);
					var eDoc = ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs[0];

					AssertEquals(document.FileName, eDoc.FileName);
					AssertArrayEqualsByElements(docImageData.ToByteArray(), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
					AssertEquals(document.Type.Code, eDoc.DocType);
					AssertEquals(document.Source.Code, eDoc.DocSource);
					AssertEquals(document.IsPublished, eDoc.IsPublished);
					AssertEquals(document.VisibleBranchCode, eDoc.VisibleBranchCode);
					AssertEquals(document.VisibleCompanyCode, eDoc.VisibleCompanyCode);
					AssertEquals(document.VisibleDepartmentCode, eDoc.VisibleDepartmentCode);
				}
			}
		}

		public void TestReadAttachedDocumentsWithoutSaving()
		{
			var docSource = Factory.NewWithValidTestData<RefDocSource>();
			docSource.RDS_Code = "AZA";
			docSource.RDS_Desc = "Aaaaa!";

			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(0, ((IDocManagerSupport)orgHeader).DocManagerInfo.AllEDocs.Count);
			Factory.SaveForTesting();

			using (var document = new AttachedDocument())
			{
				document.FileName = "Hahaha";
				document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
				document.Type = new DocumentType { Code = "HHH", Description = "Hahaha" };
				document.Source = new CodeDescriptionPair { Code = "AZA", Description = "Aaaaa!" };

				var reader = new AttachedDocumentDataObjectReader();
				using (Factory.BOFactory.AddDisposableService())
				using (var docImageData = document.ImageData.Copy())
				{
					reader.TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _);

					var reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);
					AssertEquals(0, ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs.Count);

					((IDocManagerSupport)orgHeader).DocManagerInfo.MasterFactory.Save();
					reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);
					AssertEquals(1, ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs.Count);
					var eDoc = ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs[0];

					AssertEquals(document.FileName, eDoc.FileName);
					AssertArrayEqualsByElements(docImageData.ToByteArray(), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
					AssertEquals(document.Type.Code, eDoc.DocType);
					AssertEquals(document.Source.Code, eDoc.DocSource);
				}
			}
		}

		public void TestReadAttachedDocumentsWithInvalidCode()
		{
			var docSource = Factory.NewWithValidTestData<RefDocSource>();

			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(0, ((IDocManagerSupport)orgHeader).DocManagerInfo.AllEDocs.Count);
			Factory.SaveForTesting();

			using (var document = new AttachedDocument())
			{
				document.FileName = "Hahaha";
				document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
				document.Type = new DocumentType { Code = "HHH", Description = "Hahaha" };
				document.Source = new CodeDescriptionPair { Code = "AZA", Description = "Aaaaa!" };
				document.IsPublished = true;
				document.VisibleBranchCode = "!!!";
				document.VisibleCompanyCode = "@@@";
				document.VisibleDepartmentCode = "###";

				var reader = new AttachedDocumentDataObjectReader();
				using (Factory.BOFactory.AddDisposableService())
				using (var docImageData = document.ImageData.Copy())
				{
					reader.TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _);
					Factory.SaveForTesting();
					var reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);
					AssertEquals(1, ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs.Count);
					var eDoc = ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs[0];
					AssertEquals(document.FileName, eDoc.FileName);
					AssertArrayEqualsByElements(docImageData.ToByteArray(), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
					AssertEquals(document.Type.Code, eDoc.DocType);
					AssertEquals(document.IsPublished, eDoc.IsPublished);
					AssertEquals(document.Source.Code, eDoc.DocSource);
					Assert(string.IsNullOrEmpty(eDoc.VisibleBranchCode));
					Assert(string.IsNullOrEmpty(eDoc.VisibleCompanyCode));
					Assert(string.IsNullOrEmpty(eDoc.VisibleDepartmentCode));

					AssertEquals(@"Warning - The visible company code '@@@' specified in XML is invalid.
Warning - The visible branch code '!!!' specified in XML is invalid.
Warning - The visible department code '###' specified in XML is invalid.
Information - Successfully Added eDoc: Hahaha.", Logger.Logs);
				}
			}
		}

		public void TestAddAttachedDocument_JPG_NoConvertToTiff()
		{
			TestAddAttachedDocument_ImageData_NoConvertToTiff("jpg");
		}

		public void TestAddAttachedDocument_PNG_NoConvertToTiff()
		{
			TestAddAttachedDocument_ImageData_NoConvertToTiff("png");
		}

		public void TestAddAttachedDocument_TIF_NoConvertToTiff()
		{
			TestAddAttachedDocument_ImageData_NoConvertToTiff("tif");
		}

		public void TestAddAttachedDocument_PDF_NoConvertToTiff()
		{
			TestAddAttachedDocument_ImageData_NoConvertToTiff("pdf");
		}

		void TestAddAttachedDocument_ImageData_NoConvertToTiff(string extension)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(0, ((IDocManagerSupport)orgHeader).DocManagerInfo.AllEDocs.Count);
			Factory.SaveForTesting();

			using (var document = new AttachedDocument())
			{
				var image = new Bitmap(1, 1);
				image.SetPixel(0, 0, Color.White);

				document.FileName = $"test.{extension}";
				using var stream = new MemoryStream();
				image.Save(stream, ImageFormat.Png);
				document.ImageData = (SubStreamableStream)stream;
				document.Type = new DocumentType { Code = "HHH", Description = "Hahaha" };
				document.Source = new CodeDescriptionPair { Code = "AZA", Description = "Aaaaa!" };

				var reader = new AttachedDocumentDataObjectReader();
				using (Factory.BOFactory.AddDisposableService())
				using (var docImageData = document.ImageData.Copy())
				{
					reader.TryAddAttachedDocument(document, Logger, orgHeader, out IeDoc _);

					var reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);
					AssertEquals(0, ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs.Count);

					((IDocManagerSupport)orgHeader).DocManagerInfo.MasterFactory.Save();
					reloadedHeader = new BusinessObjectFactory().Load<OrgHeader>(orgHeader.PK);
					AssertEquals(1, ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs.Count);
					var eDoc = ((IDocManagerSupport)reloadedHeader).DocManagerInfo.AllEDocs[0];

					AssertEquals(document.FileName, eDoc.FileName);
					AssertArrayEqualsByElements(docImageData.ToByteArray(), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
					AssertEquals(document.Type.Code, eDoc.DocType);
					AssertEquals(document.Source.Code, eDoc.DocSource);
				}
			}
		}

		TestErrorLogger Logger
		{
			get
			{
				if (logger == null)
				{
					logger = new TestErrorLogger();
					var dataContext = new DataContext();
					var dataSource = new DataSource();
					dataSource.DataProvider = new DataProvider();
					dataSource.DataProvider.Code = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID + "XXX";
					dataSource.DataProvider.Type = DataProviderType.EnterpriseID;
					dataContext.DataSource = dataSource;
					logger.TopLevelDataObject = new Shipment { DataContext = dataContext };
				}
				return logger;
			}
		}
		TestErrorLogger logger;
	}
}
