using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using IDocumentScanningEDocsService = Enterprise.DocumentScanning.Services.IEDocsService;

namespace Enterprise.Services.ServiceHost.Tests
{
	class EDocsControllerTest : TestCaseWithFactory
	{
		public void TestUpload_NotMultipartContent()
		{
			controller.Request.Content = new StringContent("aaa");
			var recordPrefix = "A";
			var recordPK = Guid.NewGuid();

			var actionResult = controller.Upload(recordPrefix, recordPK).GetAwaiter().GetResult();

			actionResult.AssertResultContains(HttpStatusCode.UnsupportedMediaType, null);
		}

		public void TestUpload_NoContent()
		{
			TestUploadCore(
				_ => { },
				actionResult => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "No content specified."));
		}

		public void TestUpload_NoFileName()
		{
			TestUploadCore(
				multipartContent => multipartContent.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "content"),
				actionResult => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "File name must be specified."));
		}

		public void TestUpload_NotAcceptable()
		{
			TestUploadCore(
				multipartContent => multipartContent.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "content", "danger.exe"),
				actionResult => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "File cannot be added because it has a potentially dangerous file name (danger.exe)."));
		}

		public void TestUpload_NotAcceptable_SneakyExe()
		{
			TestUploadCore(
				multipartContent => multipartContent.Add(new ByteArrayContent(new byte[] { 0x4D, 0x5A, 0x00, 0xFF }), "content", "nicefile.tif"),
				actionResult => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "File cannot be added because it has a potentially dangerous file type (exe)."));
		}

		public void TestUpload_NoDetail()
		{
			TestUploadCore(
				multipartContent => multipartContent.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "content", "file.bmp"),
				actionResult => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "No detail specified."));
		}

		public void TestUpload_MalformedDetail()
		{
			TestUploadCore(
				multipartContent =>
				{
					multipartContent.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "content", "file.bmp");
					multipartContent.Add(new StringContent("You shall not parse!"), "detail");
				},
				actionResult => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "EDocs detail is not in the correct format."));
		}

		public void TestUpload_MalformedDetail_InvalidPK()
		{
			TestUploadCore(
				multipartContent =>
				{
					multipartContent.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "content", "file.bmp");
					multipartContent.Add(
						new StringContent("{\"description\":\"abc\",\"isPublished\":true,\"docType\":\"MSC\""),
						"detail");
				},
				actionResult => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "EDocs detail is not in the correct format."));
		}

		void TestUploadCore(Action<MultipartFormDataContent> setupContent, Action<IHttpActionResult> assertActionResult)
		{
			using (var multipartContent = new MultipartFormDataContent())
			{
				setupContent(multipartContent);
				controller.Request.Content = multipartContent;
				var recordPrefix = "A";
				var recordPK = Guid.NewGuid();

				Task.Run(async () =>
				{
					var actionResult = await controller.Upload(recordPrefix, recordPK);
					assertActionResult(actionResult);
				}).GetAwaiter().GetResult();
			}
		}

		public void TestUpload_Succeeded()
		{
			TestUploadCore(
				expectedEDocDetail => new EDocUpdateResult
				{
					Status = EDocUpdateStatus.Succeeded,
					EDocDetail = expectedEDocDetail
				},
				(expectedEDocDetail, actionResult) => actionResult.AssertJsonResultEquals(expectedEDocDetail));
		}

		public void TestUpload_SucceededForStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			SetupPrincipal(staff);

			TestUploadCore(
				expectedEDocDetail => new EDocUpdateResult
				{
					Status = EDocUpdateStatus.Succeeded,
					EDocDetail = expectedEDocDetail
				},
				(expectedEDocDetail, actionResult) => actionResult.AssertJsonResultEquals(expectedEDocDetail),
				true);
		}

		public void TestUpload_SucceededForContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			Factory.Save();

			SetupPrincipal(contact);

			TestUploadCore(
				expectedEDocDetail => new EDocUpdateResult
				{
					Status = EDocUpdateStatus.Succeeded,
					EDocDetail = expectedEDocDetail
				},
				(expectedEDocDetail, actionResult) => actionResult.AssertJsonResultEquals(expectedEDocDetail),
				false,
				contact.PK);
		}

		public void TestUpload_EDocNotFound()
		{
			TestUploadCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.EDocNotFound },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.NotFound, null));
		}

		public void TestUpload_InvalidDocumentType()
		{
			TestUploadCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.InvalidDocumentType },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "Invalid document type."));
		}

		public void TestUpload_ContactUploadUnpublishedFile()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupPrincipal(contact);

			TestUploadCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.UnpublishedForContact },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "File that was uploaded by contact should be published."),
				false,
				contact.PK);
		}

		public void TestUpload_InvalidDocument()
		{
			TestUploadCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.InvalidDocument },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "Invalid document."));
		}

		public void TestUpload_BusinessObjectNotSupported()
		{
			TestUploadCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.BusinessObjectNotSupported },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "eDoc unsupported for this record type."));
		}

		public void TestUpload_Undefined()
		{
			TestUploadCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.Undefined },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.InternalServerError, null));
		}

		public void TestUpload_FilePathFormatNotSupported()
		{
			controller.Request.Content = new StringContent("aaa");
			var recordPrefix = "A";
			var recordPK = Guid.NewGuid();

			var eDocDetail = new EDocDetail
			{
				FileName = "content.txt",
				DocumentTypePK = Guid.NewGuid(),
				Description = "desc",
				IsPublished = false
			};

			var expectedContent = new byte[] { 1, 2, 3 };
			var detailArgs = new EDocsDetailArgs
			{
				RefDocTypePK = eDocDetail.DocumentTypePK,
				Description = eDocDetail.Description,
				IsPublished = eDocDetail.IsPublished
			};

			using (var multipartContent = new MultipartFormDataContent())
			{
				multipartContent.Add(new ByteArrayContent(expectedContent), "content", eDocDetail.FileName);
				var detailJson = JsonConvert.SerializeObject(detailArgs);
				multipartContent.Add(new StringContent(detailJson), "detail");

				controller.Request.Content = multipartContent;
				mockService.Setup(x => x.IsFileAcceptable(It.IsAny<string>())).Throws(new NotSupportedException());

				AssertNoExceptionThrown(() =>
				{
					Task.Run(async () =>
					{
						var actionResult = await controller.Upload(recordPrefix, recordPK);
						actionResult.AssertResultContains(HttpStatusCode.BadRequest, null);
					}).GetAwaiter().GetResult();
				});
			}
		}

		void TestUploadCore(Func<EDocDetail, EDocUpdateResult> getExpectedUpdateResult, Action<EDocDetail, IHttpActionResult> assertActionResult, bool expectedIncludeUnpublished = false, ZGuid? contactPK = null)
		{
			var expectedEDocDetail = new EDocDetail
			{
				FileName = "content.txt",
				DocumentTypePK = Guid.NewGuid(),
				Description = "desc",
				IsPublished = false
			};
			var expectedResult = getExpectedUpdateResult(expectedEDocDetail);
			var recordPrefix = "A";
			var recordPK = Guid.NewGuid();
			var detailArgs = new EDocsDetailArgs
			{
				RefDocTypePK = expectedEDocDetail.DocumentTypePK,
				Description = expectedEDocDetail.Description,
				IsPublished = expectedEDocDetail.IsPublished
			};
			Expression<Func<EDocDetail, bool>> matchDetail = y => y.FileName == expectedEDocDetail.FileName && y.DocumentTypePK == expectedEDocDetail.DocumentTypePK && y.Description == expectedEDocDetail.Description && y.IsPublished == expectedEDocDetail.IsPublished;
			var expectedContent = new byte[] { 1, 2, 3 };
			Expression<Func<byte[], bool>> matchContent = y => y.Length == expectedContent.Length && Enumerable.Zip(y, expectedContent, (ye, ece) => ye == ece).All(b => b);

			mockService.Setup(x => x.AddOrUpdateEDoc(recordPrefix, recordPK, It.Is(matchDetail), It.Is(matchContent), expectedIncludeUnpublished, contactPK)).Returns(expectedResult).Verifiable();

			using (var multipartContent = new MultipartFormDataContent())
			{
				multipartContent.Add(new ByteArrayContent(expectedContent), "content", expectedEDocDetail.FileName);
				var detailJson = JsonConvert.SerializeObject(detailArgs);
				multipartContent.Add(new StringContent(detailJson), "detail");

				controller.Request.Content = multipartContent;

				Task.Run(async () =>
				{
					var actionResult = await controller.Upload(recordPrefix, recordPK);
					assertActionResult(expectedEDocDetail, actionResult);
				}).GetAwaiter().GetResult();
			}
		}

		public void TestUpdate_Succeeded()
		{
			TestUpdateCore(
				expectedEDocDetail => new EDocUpdateResult
				{
					Status = EDocUpdateStatus.Succeeded,
					EDocDetail = expectedEDocDetail
				},
				(expectedEDocDetail, actionResult) => actionResult.AssertJsonResultEquals(expectedEDocDetail));
		}

		public void TestUpdate_SucceededForStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			SetupPrincipal(staff);

			TestUpdateCore(
				expectedEDocDetail => new EDocUpdateResult
				{
					Status = EDocUpdateStatus.Succeeded,
					EDocDetail = expectedEDocDetail
				},
				(expectedEDocDetail, actionResult) => actionResult.AssertJsonResultEquals(expectedEDocDetail),
				true);
		}

		public void TestUpdate_SucceededForContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			Factory.Save();

			SetupPrincipal(contact);

			TestUpdateCore(
				expectedEDocDetail => new EDocUpdateResult
				{
					Status = EDocUpdateStatus.Succeeded,
					EDocDetail = expectedEDocDetail
				},
				(expectedEDocDetail, actionResult) => actionResult.AssertJsonResultEquals(expectedEDocDetail),
				false,
				contact.PK);
		}

		public void TestUpdate_BusinessObjectNotFound()
		{
			TestUpdateCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.BusinessObjectNotFound },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.NotFound, null));
		}

		public void TestUpdate_InvalidDocumentType()
		{
			TestUpdateCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.InvalidDocumentType },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "Invalid document type."));
		}

		public void TestUpdate_ContactUploadUnpublishedFile()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupPrincipal(contact);

			TestUpdateCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.UnpublishedForContact },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "File that was uploaded by contact should be published."),
				false,
				contact.PK);
		}

		public void TestUpdate_InvalidDocument()
		{
			TestUpdateCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.InvalidDocument },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "Invalid document."));
		}

		public void TestUpdate_BusinessObjectNotSupported()
		{
			TestUpdateCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.BusinessObjectNotSupported },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.BadRequest, "eDoc unsupported for this record type."));
		}

		public void TestUpdate_Undefined()
		{
			TestUpdateCore(
				_ => new EDocUpdateResult { Status = EDocUpdateStatus.Undefined },
				(_, actionResult) => actionResult.AssertResultContains(HttpStatusCode.InternalServerError, null));
		}

		void TestUpdateCore(Func<EDocDetail, EDocUpdateResult> getExpectedUpdateResult, Action<EDocDetail, IHttpActionResult> assertActionResult, bool expectedIncludeUnpublished = false, ZGuid? contactPK = null)
		{
			var expectedEDocDetail = new EDocDetail
			{
				DocumentTypePK = Guid.NewGuid(),
				Description = "desc",
				IsPublished = false
			};
			var expectedResult = getExpectedUpdateResult(expectedEDocDetail);
			var recordPrefix = "A";
			var recordPK = Guid.NewGuid();
			var eDocPK = Guid.NewGuid();
			var detailArgs = new EDocsDetailArgs
			{
				RefDocTypePK = expectedEDocDetail.DocumentTypePK,
				Description = expectedEDocDetail.Description,
				IsPublished = expectedEDocDetail.IsPublished
			};
			Expression<Func<EDocDetail, bool>> matchDetail = y => y.Id == eDocPK && y.DocumentTypePK == expectedEDocDetail.DocumentTypePK && y.Description == expectedEDocDetail.Description && y.IsPublished == expectedEDocDetail.IsPublished;
			mockService.Setup(x => x.AddOrUpdateEDoc(recordPrefix, recordPK, It.Is(matchDetail), null, expectedIncludeUnpublished, contactPK)).Returns(expectedResult).Verifiable();

			var actionResult = controller.Update(recordPrefix, recordPK, eDocPK, detailArgs);

			assertActionResult(expectedEDocDetail, actionResult);
			mockService.Verify();
		}

		public void TestGetEDocDetails()
		{
			TestGetEDocDetailsCore(forContact: false, true);
			TestGetEDocDetailsCore(forContact: true, false);
		}

		void TestGetEDocDetailsCore(bool forContact, bool expectedIncludeUnpublished)
		{
			var principal = Factory.NewWithValidTestData(forContact ? typeof(OrgContact) : typeof(GlbStaff));
			Factory.Save();

			SetupPrincipal(principal);
			var expectedContactPK = forContact ? (ZGuid?)principal.PK : null;
			var publishedResult = new[]
			{
				new EDocDetail { Id = Guid.NewGuid(), IsPublished = true, },
			};
			var allResult = new[]
			{
				publishedResult[0],
				new EDocDetail { Id = Guid.NewGuid(), IsPublished = false, },
			};

			var recordPK = Guid.NewGuid();
			var includeDeleted = false;
			mockService.Setup(x => x.GetEDocDetails(recordPK, includeDeleted, expectedIncludeUnpublished, expectedContactPK))
				.Returns(forContact ? publishedResult : allResult)
				.Verifiable();

			var actionResult = controller.GetEDocDetails(recordPK, includeDeleted);

			actionResult.AssertJsonResultEquals(forContact ? publishedResult : allResult);
			mockService.Verify();
		}

		void SetupPrincipal(BusinessObject principal)
		{
			if (principal is GlbStaff staff)
			{
				GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			}
			else if (principal is OrgContact contact)
			{
				GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);
			}
			else
			{
				throw new ArgumentException("invalid principal type");
			}
		}

		public void TestGetEDocCount()
		{
			var recordPK = Guid.NewGuid();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			mockService.Setup(x => x.GetEDocCount(recordPK, null)).Returns(5);
			mockService.Setup(x => x.GetEDocCount(recordPK, contact.PK)).Returns(10);
			mockService.Setup(x => x.GetEDocCount(recordPK, staff.PK)).Returns(20);

			var actionResult = controller.GetEDocCount(recordPK);
			actionResult.AssertJsonResultEquals(5);

			SetupPrincipal(contact);
			actionResult = controller.GetEDocCount(recordPK);
			actionResult.AssertJsonResultEquals(10);

			SetupPrincipal(staff);
			actionResult = controller.GetEDocCount(recordPK);
			actionResult.AssertJsonResultEquals(5);
		}

		public void TestGetEDocImageDoc_NotFoundResponse()
		{
			var response = controller.GetEDocImageData(Guid.NewGuid(), 1, true);
			AssertEquals(HttpStatusCode.NotFound, response.StatusCode);
		}

		public void TestGetEDocImageDoc_VirusDetected()
		{
			var fileName = "TestVirus.txt";
			var eDocData = new EDocImageData
			{
				FullFileName = fileName,
				Data = new byte[] { 1, 2, 3 }
			};
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 1;

			mockService.Setup(x => x.GetEDocImageData(eDocPK, databaseNumber, false, null)).Throws(new VirusDetectedException(fileName));
			var responseMessage = controller.GetEDocImageData(eDocPK, databaseNumber, asAttachment: false);
			var actualData = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			AssertEquals(actualData, "The file has been detected with virus and therefore cannot be saved or opened.");
			AssertEquals(HttpStatusCode.BadRequest, responseMessage.StatusCode);
			mockService.Verify();
		}

		public void TestGetEDocImageDoc_GivenNonOpenableTypes_ShouldAlwaysTreatAsAttachement()
		{
			TestGetEDocImageDocCore("testfile.tif", forStaff: true, true, asAttachment: true, "application/octet-stream", "attachment");
			TestGetEDocImageDocCore("testfile.tif", forStaff: true, true, asAttachment: false, "application/octet-stream", "attachment");

			TestGetEDocImageDocCore("testfile.TiF", forStaff: true, true, asAttachment: true, "application/octet-stream", "attachment");
			TestGetEDocImageDocCore("testfile.TiF", forStaff: true, true, asAttachment: false, "application/octet-stream", "attachment");

			TestGetEDocImageDocCore("testfile.xml", forStaff: true, true, asAttachment: true, "application/octet-stream", "attachment");
			TestGetEDocImageDocCore("testfile.xml", forStaff: true, true, asAttachment: false, "application/octet-stream", "attachment");

			TestGetEDocImageDocCore("testfile.dat", forStaff: true, true, asAttachment: true, "application/octet-stream", "attachment");
			TestGetEDocImageDocCore("testfile.dat", forStaff: true, true, asAttachment: false, "application/octet-stream", "attachment");
		}

		public void TestGetEDocImageDoc_GivenOpenableType_ShouldReturnCorrectMimeType()
		{
			TestGetEDocImageDocCore("testfile.txt", forStaff: true, expectedIncludeUnpublished: true, asAttachment: true, "application/octet-stream", "attachment");
			TestGetEDocImageDocCore("testfile.txt", forStaff: true, expectedIncludeUnpublished: true, asAttachment: false, "text/plain", "inline");

			TestGetEDocImageDocCore("testfile.pdf", forStaff: true, expectedIncludeUnpublished: true, asAttachment: true, "application/octet-stream", "attachment");
			TestGetEDocImageDocCore("testfile.pdf", forStaff: true, expectedIncludeUnpublished: true, asAttachment: false, "application/pdf", "inline");

			TestGetEDocImageDocCore("testfile.PdF", forStaff: true, expectedIncludeUnpublished: true, asAttachment: true, "application/octet-stream", "attachment");
			TestGetEDocImageDocCore("testfile.PdF", forStaff: true, expectedIncludeUnpublished: true, asAttachment: false, "application/pdf", "inline");
		}

		public void TestGetEDocImageData_StaffReturnsUnpublished()
		{
			TestGetEDocImageDocCore("testfile.tif", forStaff: true, expectedIncludeUnpublished: true, asAttachment: true, "application/octet-stream", "attachment");
			TestGetEDocImageDocCore("testfile.tif", forStaff: true, expectedIncludeUnpublished: true, asAttachment: false, "application/octet-stream", "attachment");
		}

		public void TestGetEDocImageDoc_ContactExcludesUnpublished()
		{
			TestGetEDocImageDocCore("testfile.tif", forStaff: false, expectedIncludeUnpublished: false, asAttachment: true, "application/octet-stream", "attachment");
			TestGetEDocImageDocCore("testfile.tif", forStaff: false, expectedIncludeUnpublished: false, asAttachment: false, "application/octet-stream", "attachment");
		}

		void TestGetEDocImageDocCore(
			string fileName,
			bool forStaff,
			bool expectedIncludeUnpublished,
			bool asAttachment,
			string expectedMimeType,
			string expectedDispositionTypeType)
		{
			var principal = Factory.NewWithValidTestData(forStaff ? typeof(GlbStaff) : typeof(OrgContact));
			Factory.Save();

			SetupPrincipal(principal);

			var expectedData = new byte[] { 1, 2, 3 };
			var eDocData = new EDocImageData
			{
				FullFileName = fileName,
				Data = expectedData
			};
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 86;
			var expectedContactPK = forStaff ? null : (ZGuid?)principal.PK;
			mockService.Setup(x => x.GetEDocImageData(eDocPK, databaseNumber, expectedIncludeUnpublished, expectedContactPK)).Returns(eDocData).Verifiable();

			var responseMessage = controller.GetEDocImageData(eDocPK, databaseNumber, asAttachment);
			var actualData = responseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
			AssertEquals(actualData, expectedData);

			AssertEquals(responseMessage.Content.Headers.ContentType.MediaType, expectedMimeType);
			AssertEquals(responseMessage.Content.Headers.ContentDisposition.FileName, fileName);
			AssertEquals(responseMessage.Content.Headers.ContentDisposition.FileNameStar, fileName);
			AssertEquals(responseMessage.Content.Headers.ContentDisposition.DispositionType, expectedDispositionTypeType);

			mockService.Verify();

			// GetEDocImageData throw ExternalStorageException
			eDocData = null;
			mockService.Setup(x => x.GetEDocImageData(eDocPK, databaseNumber, expectedIncludeUnpublished, expectedContactPK)).Throws(new ExternalStorageException("S3 Fails", "S3", null));
			responseMessage = controller.GetEDocImageData(eDocPK, databaseNumber, asAttachment);
			AssertEquals(HttpStatusCode.InternalServerError, responseMessage.StatusCode);
			mockService.Verify();
		}

		[TestDate(2023, 4, 5)]
		public void TestGetAllEDocsData_Contacts()
		{
			var principal = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupPrincipal(principal);

			AssertGetAllEDocsData(false, principal.PK);
		}

		[TestDate(2023, 4, 5)]
		public void TestGetAllEDocsData_Staff()
		{
			var principal = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			SetupPrincipal(principal);

			AssertGetAllEDocsData(true, null);
		}

		[TestDate(2023, 4, 5)]
		public void TestGetAllEDocsDataWithTimeExpiredToken()
		{
			var token = MockDownloadToken();
			TestDateAttribute.AddSeconds(301);

			var result = controller.GetAllEDocsData(token) as OkNegotiatedContentResult<string>;
			AssertNotNull(result);
			AssertEquals(result.Content, "Your file download has expired. Please try again.");
		}

		[TestDate(2023, 4, 5)]
		public void TestGetAllEDocsDataWithTimeNotExpiredToken()
		{
			var token = MockDownloadToken();
			TestDateAttribute.AddSeconds(300);

			var result = controller.GetAllEDocsData(token);
			var messageResult = result as ResponseMessageResult;
			Assert(result is not OkNegotiatedContentResult<string>);
			AssertNotNull(messageResult);
			AssertEquals(messageResult.Response.StatusCode, HttpStatusCode.OK);
			AssertNotNull(messageResult.Response.Content.Headers.ContentDisposition);
		}

		Guid MockDownloadToken()
		{
			var principal = Factory.NewWithValidTestData<OrgContact>();
			SetupPrincipal(principal);
			var token = Guid.NewGuid();
			var stmData = Factory.NewWithValidTestData<StmData>();
			var identity = controller.User.Identity as IGlowAuthenticationTicketIdentity;
			stmData.SD_BinaryValue = ZBlob.FromUTF8(JsonConvert.SerializeObject(new[] { token }));
			stmData.SD_Name = $"{nameof(EDocsController)}-GetAll{token}";
			stmData.SD_Owner = identity.ProviderKey;
			Factory.Save();

			return token;
		}

		void AssertGetAllEDocsData(bool expectedIncludeUnpublished, ZGuid? expectedContactPK)
		{
			var publishedEDocs = new[]
			{
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = true,
					FileName = "SomeInvoice",
					DocumentTypeCode = "INV",
					DatabaseNumber = 1,
					OwnerReadableName = "JobShipment 1",
				},
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = true,
					FileName = "SomeQuote",
					DocumentTypeCode = "QTE",
					DatabaseNumber = 2,
					OwnerReadableName = "JobShipment 2",
				},
			};
			var allEDocs = new[]
			{
				publishedEDocs[0],
				publishedEDocs[1],
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = false,
					FileName = "SomePacklingList",
					DocumentTypeCode = "PKL",
					DatabaseNumber = 2,
					OwnerReadableName = "Invoice 1",
				},
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = false,
					FileName = $"SomeReport",
					DocumentTypeCode = "REP",
					DatabaseNumber = 1,
					OwnerReadableName = "Invoice 2",
				},
			};

			var recordPKs = new[] { Guid.NewGuid(), Guid.NewGuid() };
			var eDocIndex = 0;
			foreach (var recordPK in recordPKs)
			{
				mockService.Setup(x => x.GetEDocDetails(recordPK, false, false, expectedContactPK))
					.Returns(new[] { publishedEDocs[eDocIndex] })
					.Verifiable();

				mockService.Setup(x => x.GetEDocDetails(recordPK, false, true, expectedContactPK))
				.Returns(new[] { allEDocs[eDocIndex], allEDocs[eDocIndex + 2] })
				.Verifiable();

				eDocIndex++;
			}

			var eDocsData = new Dictionary<Guid, EDocImageData>();
			foreach (var eDoc in allEDocs)
			{
				var fileExtension = eDoc.DatabaseNumber == 1 ? "pdf" : "txt";
				var eDocData = new EDocImageData
				{
					FullFileName = $"eDoc.FileName.{fileExtension}",
					Data = new byte[] { 1, 2, 3, (byte)eDoc.DatabaseNumber },
				};
				eDocsData.Add(eDoc.Id, eDocData);

				mockService.Setup(x => x.GetEDocImageData(eDoc.Id, eDoc.DatabaseNumber, expectedIncludeUnpublished, expectedContactPK))
					.Returns(eDocData)
					.Verifiable();
			}

			var expectedFileName = $"eDocs_{ZDateTime.Now.ToString("yyyyMMdd-HHmmss-fff")}.zip";

			var tokenResult = controller.GetAllEDocsToken(recordPKs) as OkNegotiatedContentResult<string>;
			var result = controller.GetAllEDocsData(new Guid(tokenResult.Content)) as ResponseMessageResult;

			AssertNotNull(result);
			var responseMessage = result.Response;
			AssertEquals(responseMessage.StatusCode, HttpStatusCode.OK);
			AssertEquals(responseMessage.Content.Headers.ContentType.MediaType, DataContentTypes.Zip);
			AssertEquals(responseMessage.Content.Headers.ContentDisposition.FileName, expectedFileName);
			AssertEquals(responseMessage.Content.Headers.ContentDisposition.FileNameStar, expectedFileName);
			AssertEquals(responseMessage.Content.Headers.ContentDisposition.DispositionType, "attachment");

			var actualData = responseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
			var zip = new ZipExtractor();
			var expectedEDocs = expectedIncludeUnpublished ? allEDocs : publishedEDocs;

			foreach (var eDoc in expectedEDocs)
			{
				var expectedEDocData = eDocsData[eDoc.Id];
				var expectedEDocFileName = $"[{eDoc.OwnerReadableName}]-[{eDoc.DocumentTypeCode}]-{eDoc.FileName}{Path.GetExtension(expectedEDocData.FullFileName)}";
				AssertEquals($"{expectedEDocFileName} is missing", true, zip.ContainsFile(new MemoryStream(actualData), expectedEDocFileName));
			}
		}

		public void TestGetAllEDocsDataWithLongFileNames()
		{
			var principal = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			SetupPrincipal(principal);

			var eDocs = new[]
			{
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = true,
					FileName = $"SomeReport{new string('1', StorageDocsSchema.SC_FileName.MaxLength - 4)}",
					DocumentTypeCode = "REP",
					DatabaseNumber = 1,
				},
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = true,
					FileName = $"SomeReport{new string('1', StorageDocsSchema.SC_FileName.MaxLength - 4)}",
					DocumentTypeCode = "REP",
					DatabaseNumber = 1,
				},
			};

			var recordPK = Guid.NewGuid();
			mockService.Setup(x => x.GetEDocDetails(recordPK, false, true, null))
				.Returns(eDocs)
				.Verifiable();

			var eDocsData = new Dictionary<Guid, EDocImageData>();
			foreach (var eDoc in eDocs)
			{
				var eDocData = new EDocImageData
				{
					FullFileName = $"{eDoc.FileName}.pdf",
					Data = new byte[] { 1, 2, 3, (byte)eDoc.DatabaseNumber },
				};
				eDocsData.Add(eDoc.Id, eDocData);

				mockService.Setup(x => x.GetEDocImageData(eDoc.Id, eDoc.DatabaseNumber, true, default(ZGuid?)))
					.Returns(eDocData)
					.Verifiable();
			}

			var expectedFileName = $"eDocs_{ZDateTime.Now.ToString("yyyyMMdd-HHmmss-fff")}.zip";

			var tokenResult = controller.GetAllEDocsToken(new[] { recordPK }) as OkNegotiatedContentResult<string>;
			var result = controller.GetAllEDocsData(new Guid(tokenResult.Content)) as ResponseMessageResult;

			AssertNotNull(result);
			var responseMessage = result.Response;
			var actualData = responseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
			var zip = new ZipExtractor();
			var zippedFileNames = zip.GetFileNames(new MemoryStream(actualData));
			Assert(zippedFileNames.Any(x => x.Contains("1.pdf")));
			Assert(zippedFileNames.Any(x => x.Contains("1[2].pdf")));
		}

		public void TestGetAllEDocsDataWithExpiredToken()
		{
			var principal = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupPrincipal(principal);

			var response = controller.GetAllEDocsData(Guid.NewGuid()) as OkNegotiatedContentResult<string>;
			AssertNotNull(response);
			AssertEquals(response.Content, "Your file download has expired. Please try again.");
		}

		public void TestGetAllEDocsTokenWithoutParameters()
		{
			var result = controller.GetAllEDocsToken(null);
			AssertType<BadRequestResult>(result);

			result = controller.GetAllEDocsToken(Array.Empty<Guid>());
			AssertType<BadRequestResult>(result);
		}

		public void TestGetAllEDocsToken_Staff_PersistedToDatabase()
		{
			var principal = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			SetupPrincipal(principal);

			var pks = new Guid[] { Guid.NewGuid() };
			var response = controller.GetAllEDocsToken(pks) as OkNegotiatedContentResult<string>;
			AssertNotNull(response);

			var data = new StmData.Loader(Factory).LoadTop1($"{nameof(EDocsController)}-GetAll" + response.Content, principal.PK, ZGuid.Empty);
			var cachedPks = JsonConvert.DeserializeObject<Guid[]>(data.SD_BinaryValue.ToUTF8());

			AssertEquals(pks[0], cachedPks[0]);
		}

		public void TestGetAllEDocsToken_Contact_PersistedToDatabase()
		{
			var principal = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupPrincipal(principal);

			var pks = new Guid[] { Guid.NewGuid() };
			var response = controller.GetAllEDocsToken(pks) as OkNegotiatedContentResult<string>;
			AssertNotNull(response);

			var data = new StmData.Loader(Factory).LoadTop1($"{nameof(EDocsController)}-GetAll" + response.Content, principal.PK, ZGuid.Empty);
			var cachedPks = JsonConvert.DeserializeObject<Guid[]>(data.SD_BinaryValue.ToUTF8());

			AssertEquals(pks[0], cachedPks[0]);
		}

		public void TestGetAllEDocsToken_MultipleCalls()
		{
			var principal = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupPrincipal(principal);

			var pks = new Guid[] { Guid.NewGuid() };

			controller.GetAllEDocsToken(pks);
			controller.GetAllEDocsToken(pks);
			controller.GetAllEDocsToken(pks);

			var query = new ZQuery();
			query.AddToFilter(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, $"{nameof(EDocsController)}-GetAll");
			var cachedTokens = Factory.Load<StmData>(query);

			AssertEquals(1, cachedTokens.Length);
		}

		[ExpectNoExceptions]
		public void TestDeliverEDoc()
		{
			var recordPK = Guid.NewGuid();
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 86;
			var eDocDetail = new EDocDetail() { Id = eDocPK, DatabaseNumber = databaseNumber };
			var deliveryInstructions = new DeliveryInstructionsBase();
			mockService.Setup(x => x.GetEDocDetails(recordPK, false, false, null)).Returns(new[] { eDocDetail }).Verifiable();
			mockService.Setup(x => x.DeliverEDoc(eDocPK, databaseNumber, deliveryInstructions, false, null)).Verifiable();

			var response = controller.DeliverEDoc(recordPK, eDocPK, deliveryInstructions);

			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			mockService.Verify();
		}

		[ExpectNoExceptions]
		public void TestDeliverEDocForStaff()
		{
			var principal = Factory.NewWithValidTestData(typeof(GlbStaff));
			Factory.Save();

			SetupPrincipal(principal);

			var recordPK = Guid.NewGuid();
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 86;
			var eDocDetail = new EDocDetail() { Id = eDocPK, DatabaseNumber = databaseNumber };
			var deliveryInstructions = new DeliveryInstructionsBase();
			mockService.Setup(x => x.GetEDocDetails(recordPK, false, true, null)).Returns(new[] { eDocDetail }).Verifiable();
			mockService.Setup(x => x.DeliverEDoc(eDocPK, databaseNumber, deliveryInstructions, true, null)).Verifiable();

			var response = controller.DeliverEDoc(recordPK, eDocPK, deliveryInstructions);

			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			mockService.Verify();
		}

		[ExpectNoExceptions]
		public void TestDeliverEDocForContact()
		{
			var principal = Factory.NewWithValidTestData(typeof(OrgContact));
			Factory.Save();

			SetupPrincipal(principal);

			var recordPK = Guid.NewGuid();
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 86;
			var eDocDetail = new EDocDetail() { Id = eDocPK, DatabaseNumber = databaseNumber };
			var deliveryInstructions = new DeliveryInstructionsBase();
			mockService.Setup(x => x.GetEDocDetails(recordPK, false, false, principal.PK)).Returns(new[] { eDocDetail }).Verifiable();
			mockService.Setup(x => x.DeliverEDoc(eDocPK, databaseNumber, deliveryInstructions, false, principal.PK)).Verifiable();

			var response = controller.DeliverEDoc(recordPK, eDocPK, deliveryInstructions);

			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			mockService.Verify();
		}

		[ExpectNoExceptions]
		public void TestDeliverEDoc_WhenEDocNotExisted()
		{
			var recordPK = Guid.NewGuid();
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 86;
			var eDocDetail = new EDocDetail() { Id = eDocPK, DatabaseNumber = databaseNumber };
			var deliveryInstructions = new DeliveryInstructionsBase();
			mockService.Setup(x => x.GetEDocDetails(recordPK, false, false, null)).Verifiable();
			mockService.Setup(x => x.DeliverEDoc(eDocPK, databaseNumber, deliveryInstructions, false, null));

			var response = controller.DeliverEDoc(recordPK, eDocPK, deliveryInstructions);

			AssertEquals(response.StatusCode, HttpStatusCode.NotFound);
			mockService.Verify();
		}

		[ExpectNoExceptions]
		public void TestDeleteEDoc()
		{
			var recordPK = Guid.NewGuid();
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 86;
			var eDocDetail = new EDocDetail() { Id = eDocPK, DatabaseNumber = databaseNumber };
			mockService.Setup(x => x.GetEDocDetails(recordPK, false, false, null)).Returns(new[] { eDocDetail }).Verifiable();
			mockService.Setup(x => x.DeleteEDoc(eDocPK, databaseNumber, false, null)).Verifiable();

			var response = controller.DeleteEDoc(recordPK, eDocPK);

			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			mockService.Verify();
		}

		[ExpectNoExceptions]
		public void TestDeleteEDocForStaff()
		{
			var principal = Factory.NewWithValidTestData(typeof(GlbStaff));
			Factory.Save();

			SetupPrincipal(principal);

			var recordPK = Guid.NewGuid();
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 86;
			var eDocDetail = new EDocDetail() { Id = eDocPK, DatabaseNumber = databaseNumber };
			mockService.Setup(x => x.GetEDocDetails(recordPK, false, true, null)).Returns(new[] { eDocDetail }).Verifiable();
			mockService.Setup(x => x.DeleteEDoc(eDocPK, databaseNumber, true, null)).Verifiable();

			var response = controller.DeleteEDoc(recordPK, eDocPK);

			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			mockService.Verify();
		}

		[ExpectNoExceptions]
		public void TestDeleteEDocForContact()
		{
			var principal = Factory.NewWithValidTestData(typeof(OrgContact));
			Factory.Save();

			SetupPrincipal(principal);

			var recordPK = Guid.NewGuid();
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 86;
			var eDocDetail = new EDocDetail() { Id = eDocPK, DatabaseNumber = databaseNumber };
			mockService.Setup(x => x.GetEDocDetails(recordPK, false, false, principal.PK)).Returns(new[] { eDocDetail }).Verifiable();
			mockService.Setup(x => x.DeleteEDoc(eDocPK, databaseNumber, false, principal.PK)).Verifiable();

			var response = controller.DeleteEDoc(recordPK, eDocPK);

			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			mockService.Verify();
		}

		[ExpectNoExceptions]
		public void TestDeleteEDoc_WhenEDocNotExisted()
		{
			var recordPK = Guid.NewGuid();
			var eDocPK = Guid.NewGuid();
			var databaseNumber = 86;
			var eDocDetail = new EDocDetail() { Id = eDocPK, DatabaseNumber = databaseNumber };
			mockService.Setup(x => x.GetEDocDetails(recordPK, false, false, null)).Verifiable();
			mockService.Setup(x => x.DeleteEDoc(eDocPK, databaseNumber, false, null));

			var response = controller.DeleteEDoc(recordPK, eDocPK);

			AssertEquals(response.StatusCode, HttpStatusCode.NotFound);
			mockService.Verify();
		}

		public void TestGetRefDocTypes_InvalidDocManagerCode()
		{
			var actionResult = controller.GetRefDocTypes("INVALID");

			actionResult.AssertResultContains(HttpStatusCode.NotFound);
			mockService.Verify(x => x.GetReferenceType("INVALID"), Times.Once);
		}

		public void TestGetRefDocTypes_ValidResponseForStaff()
		{
			var docType1 = Factory.NewWithValidTestData<RefDocType>();
			docType1.RT_DocType = "DT1";
			docType1.RT_Desc = "DocType 1 Desc";
			docType1.RT_IsPublished = true;
			docType1.RT_ReferenceType = "TST";

			var docType2 = Factory.NewWithValidTestData<RefDocType>();
			docType2.RT_DocType = "DT2";
			docType2.RT_Desc = "DocType 2 Desc";
			docType2.RT_IsPublished = false;
			docType2.RT_ReferenceType = "TST";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			SetupPrincipal(staff);

			mockService.Setup(x => x.GetReferenceType("a")).Returns("TST");
			mockService.Setup(x => x.GetRefDocTypes("TST", true, null)).Returns(new[] { docType1, docType2 });

			var result = controller.GetRefDocTypes("a");
			Type contentType = result.GetType().GetProperty("Content").GetValue(result).GetType();

			Assert("JsonResult should not contain lazy enumerable", contentType.IsGenericType && contentType.GetGenericTypeDefinition() == typeof(List<>));

			result.AssertJsonResultEquals(new[]
			{
				new
				{
					key = docType1.PK,
					code = "DT1",
					description = "DocType 1 Desc",
					isPublished = true,
					canEditDescription = false,
					referenceType = "TST",
				},
				new
				{
					key = docType2.PK,
					code = "DT2",
					description = "DocType 2 Desc",
					isPublished = false,
					canEditDescription = false,
					referenceType = "TST",
				},
			});
		}

		public void TestGetRefDocTypes_ValidResponseForContact()
		{
			var docType1 = Factory.NewWithValidTestData<RefDocType>();
			docType1.RT_DocType = "DT1";
			docType1.RT_Desc = "DocType 1 Desc";
			docType1.RT_IsPublished = true;
			docType1.RT_ReferenceType = "TST";

			var docType2 = Factory.NewWithValidTestData<RefDocType>();
			docType2.RT_DocType = "DT2";
			docType2.RT_Desc = "DocType 2 Desc";
			docType2.RT_IsPublished = false;
			docType2.RT_ReferenceType = "TST";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			SetupPrincipal(contact);

			mockService.Setup(x => x.GetReferenceType("a")).Returns("TST");
			mockService.Setup(x => x.GetRefDocTypes("TST", false, contact.PK)).Returns(new[] { docType1 });

			controller.GetRefDocTypes("a").AssertJsonResultEquals(new[]
			{
				new
				{
					key = docType1.PK,
					code = "DT1",
					description = "DocType 1 Desc",
					isPublished = true,
					canEditDescription = false,
					referenceType = "TST",
				}
			});
		}

		public void TestGetRefDocTypes_TranslatedDescriptionsForSupportedLanguage()
		{
			var controller = BuildController();

			using (var fr = Res.GetLanguageInstance(Core.SharedConstants.Languages.French).UseMockData())
			using (var ch = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				fr.SetResourceGetter(key => new ResourceStringData(key, $"{Core.SharedConstants.Languages.French} translation"));
				ch.SetResourceGetter(key => new ResourceStringData(key, $"{Core.SharedConstants.Languages.ChineseSimplified} translation"));

				AssertTranslatedDescriptions(Core.SharedConstants.Languages.French);
				AssertTranslatedDescriptions(Core.SharedConstants.Languages.ChineseSimplified);
			}

			void AssertTranslatedDescriptions(string languageCode)
			{
				var docTypes = controller.GetRefDocTypes(Core.Constants.DocManagerCodes.Airline, languageCode).GetJsonResult();
				AssertGreaterThan("Should return doc types", docTypes.Count(), 0);
				foreach (var docType in docTypes)
				{
					AssertEquals($"{languageCode} translation", docType.Value<string>("description"));
				}
			}
		}

		public void TestGetRefDocTypes_EnglishDescriptionsForUnsupportedLanguage()
		{
			var controller = BuildController();
			var docTypes = controller.GetRefDocTypes(Core.Constants.DocManagerCodes.Airline, "UNSUPPORTED").GetStringResult();
			var enDocTypes = controller.GetRefDocTypes(Core.Constants.DocManagerCodes.Airline, Core.SharedConstants.Languages.English).GetStringResult();

			AssertEquals(enDocTypes, docTypes);
		}

		public void TestGetRefDocTypes_ShouldAllowEditingDescriptionForSupportedDocTypesForStaff()
		{
			var docType1 = Factory.NewWithValidTestData<RefDocType>();
			docType1.RT_DocType = "MSC";
			docType1.RT_IsPublished = true;
			docType1.RT_ReferenceType = "TST";

			var docType2 = Factory.NewWithValidTestData<RefDocType>();
			docType2.RT_DocType = "INV";
			docType2.RT_IsPublished = true;
			docType2.RT_ReferenceType = "TST";

			var docType3 = Factory.NewWithValidTestData<RefDocType>();
			docType3.RT_DocType = "DT1";
			docType3.RT_IsPublished = true;
			docType3.RT_ReferenceType = "TST";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			SetupPrincipal(staff);

			mockService.Setup(x => x.GetReferenceType("a")).Returns("TST");
			mockService.Setup(x => x.GetRefDocTypes("TST", true, null)).Returns(new[] { docType1, docType2, docType3 });

			var docTypes = controller.GetRefDocTypes("a").GetJsonResult();

			var codes = docTypes.Select(d => d.Value<string>("code"));
			foreach (var docType in docTypes)
			{
				var code = docType.Value<string>("code");
				var actualCanEditDescription = docType.Value<bool>("canEditDescription");
				var expectedCanEditDescription = code == "MSC" || code == "INV";

				AssertEquals($"Can edit description should be {expectedCanEditDescription} for {code}", expectedCanEditDescription, actualCanEditDescription);
			}
		}

		public void TestGetRefDocTypes_ShouldAllowEditingDescriptionForSupportedDocTypesForContact()
		{
			var docType1 = Factory.NewWithValidTestData<RefDocType>();
			docType1.RT_DocType = "MSC";
			docType1.RT_IsPublished = true;
			docType1.RT_ReferenceType = "TST";

			var docType2 = Factory.NewWithValidTestData<RefDocType>();
			docType2.RT_DocType = "INV";
			docType2.RT_IsPublished = true;
			docType2.RT_ReferenceType = "TST";

			var docType3 = Factory.NewWithValidTestData<RefDocType>();
			docType3.RT_DocType = "DT1";
			docType3.RT_IsPublished = true;
			docType3.RT_ReferenceType = "TST";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			SetupPrincipal(contact);

			mockService.Setup(x => x.GetReferenceType("a")).Returns("TST");
			mockService.Setup(x => x.GetRefDocTypes("TST", false, contact.PK)).Returns(new[] { docType1, docType2, docType3 });

			var docTypes = controller.GetRefDocTypes("a").GetJsonResult();

			var codes = docTypes.Select(d => d.Value<string>("code"));
			foreach (var docType in docTypes)
			{
				var code = docType.Value<string>("code");
				var actualCanEditDescription = docType.Value<bool>("canEditDescription");
				var expectedCanEditDescription = code == "MSC" || code == "INV";

				AssertEquals($"Can edit description should be {expectedCanEditDescription} for {code}", expectedCanEditDescription, actualCanEditDescription);
			}
		}

		[ExpectNoExceptions]
		public void TestGetRefDocTypes_IncludeUnPublishedDocTypesForStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			SetupPrincipal(staff);

			mockService.Setup(x => x.GetReferenceType("a")).Returns("TST");
			mockService.Setup(x => x.GetRefDocTypes("TST", true, null)).Returns(Enumerable.Empty<RefDocType>);

			controller.GetRefDocTypes("a");
			mockService.Verify(x => x.GetRefDocTypes("TST", true, null), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestGetRefDocTypes_RestrictOnlyPublishedDocTypesForContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupPrincipal(contact);

			mockService.Setup(x => x.GetReferenceType("a")).Returns("TST");
			mockService.Setup(x => x.GetRefDocTypes("TST", false, contact.PK)).Returns(Enumerable.Empty<RefDocType>);

			controller.GetRefDocTypes("a");
			mockService.Verify(x => x.GetRefDocTypes("TST", false, contact.PK), Times.Once);
		}

		public void TestGetEDocImageData_WithRecordPKAndEDocsPK_Staff()
		{
			var principal = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			SetupPrincipal(principal);

			AssertGetEDocImageData_WithRecordPKAndEDocsPK(true, null);
		}

		public void TestGetEDocImageData_WithRecordPKAndEDocsPK_Contact()
		{
			var principal = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupPrincipal(principal);

			AssertGetEDocImageData_WithRecordPKAndEDocsPK(false, principal.PK);
		}

		void AssertGetEDocImageData_WithRecordPKAndEDocsPK(bool expectedIncludeUnpublished, ZGuid? expectedContactPK)
		{
			var allEDocs = new[]
			{
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = true,
					FileName = "SomeInvoice",
					DocumentTypeCode = "INV",
					DatabaseNumber = 1,
				},
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = true,
					FileName = "SomeQuote",
					DocumentTypeCode = "QTE",
					DatabaseNumber = 2,
				},
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = false,
					FileName = "SomePacklingList",
					DocumentTypeCode = "PKL",
					DatabaseNumber = 2,
				},
				new EDocDetail
				{
					Id = Guid.NewGuid(),
					IsPublished = false,
					FileName = $"SomeReport",
					DocumentTypeCode = "REP",
					DatabaseNumber = 1,
				},
			};

			var recordPKs = new[] { Guid.NewGuid(), Guid.NewGuid() };
			for (var index = 0; index < recordPKs.Length; index++)
			{
				mockService.Setup(x => x.GetEDocDetails(recordPKs[index], false, false, expectedContactPK))
					.Returns(new[] { allEDocs[index] })
					.Verifiable();

				mockService.Setup(x => x.GetEDocDetails(recordPKs[index], false, true, expectedContactPK))
					.Returns(new[] { allEDocs[index], allEDocs[index + 2] })
					.Verifiable();
			}

			var eDocsData = new Dictionary<Guid, EDocImageData>();
			var expectedIncludeUnpublishedEDocs = expectedIncludeUnpublished ? allEDocs : allEDocs.Where(x => x.IsPublished);
			foreach (var eDoc in expectedIncludeUnpublishedEDocs)
			{
				var fileExtension = eDoc.DatabaseNumber == 1 ? "pdf" : "txt";
				var eDocData = new EDocImageData
				{
					FullFileName = $"eDoc.FileName.{fileExtension}",
					Data = new byte[] { 1, 2, 3, (byte)eDoc.DatabaseNumber },
				};
				eDocsData.Add(eDoc.Id, eDocData);
				mockService.Setup(x => x.GetEDocImageData(eDoc.Id, eDoc.DatabaseNumber, expectedIncludeUnpublished, expectedContactPK))
					.Returns(eDocData)
					.Verifiable();
			}

			var response00Attachment = controller.GetEDocImageData(recordPKs[0], allEDocs[0].Id, true);
			AssertEquals("response00Attachment", HttpStatusCode.OK, response00Attachment.StatusCode);
			AssertEquals("response00Attachment", "attachment", response00Attachment.Content.Headers.ContentDisposition.DispositionType);
			AssertEquals("response00Attachment", eDocsData[allEDocs[0].Id].Data, response00Attachment.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult());

			var response00Inline = controller.GetEDocImageData(recordPKs[0], allEDocs[0].Id, false);
			AssertEquals("response00Inline", HttpStatusCode.OK, response00Inline.StatusCode);
			AssertEquals("response00Inline", "inline", response00Inline.Content.Headers.ContentDisposition.DispositionType);
			AssertEquals("response00Inline", eDocsData[allEDocs[0].Id].Data, response00Inline.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult());

			var response01 = controller.GetEDocImageData(recordPKs[0], allEDocs[1].Id, true);
			AssertEquals("response01", HttpStatusCode.NotFound, response01.StatusCode);

			var response02 = controller.GetEDocImageData(recordPKs[0], allEDocs[2].Id, true);
			if (expectedIncludeUnpublished)
			{
				AssertEquals("response02", HttpStatusCode.OK, response02.StatusCode);
				AssertEquals("response02", eDocsData[allEDocs[2].Id].Data, response02.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult());
			}
			else
			{
				AssertEquals("response02", HttpStatusCode.NotFound, response02.StatusCode);
			}

			var response03 = controller.GetEDocImageData(recordPKs[0], allEDocs[3].Id, true);
			AssertEquals("response03", HttpStatusCode.NotFound, response03.StatusCode);

			var response10 = controller.GetEDocImageData(recordPKs[1], allEDocs[0].Id, true);
			AssertEquals("response10", HttpStatusCode.NotFound, response10.StatusCode);

			var response11 = controller.GetEDocImageData(recordPKs[1], allEDocs[1].Id, true);
			AssertEquals("response11", HttpStatusCode.OK, response11.StatusCode);
			AssertEquals("response11", eDocsData[allEDocs[1].Id].Data, response11.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult());

			var response12 = controller.GetEDocImageData(recordPKs[1], allEDocs[2].Id, true);
			AssertEquals("response12", HttpStatusCode.NotFound, response12.StatusCode);

			var response13 = controller.GetEDocImageData(recordPKs[1], allEDocs[3].Id, true);
			if (expectedIncludeUnpublished)
			{
				AssertEquals("response13", HttpStatusCode.OK, response13.StatusCode);
				AssertEquals("response13", eDocsData[allEDocs[3].Id].Data, response13.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult());
			}
			else
			{
				AssertEquals("response13", HttpStatusCode.NotFound, response13.StatusCode);
			}
		}

		static EDocsController BuildController(IDocumentScanningEDocsService service = null)
		{
			var controller = service == null ? new EDocsController() : new EDocsController(service);
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage())
			{
				Controller = controller
			};
			return controller;
		}

		Mock<IDocumentScanningEDocsService> mockService;
		EDocsController controller;

		delegate bool IsFileAcceptableReturns(Stream stream, out string apparentFileType);

		protected override void SetUp()
		{
			base.SetUp();

			mockService = new Mock<IDocumentScanningEDocsService>();
			mockService.Setup(x => x.IsFileAcceptable(It.IsAny<string>())).Returns<string>(fileName => !fileName.EndsWith(".exe"));
			mockService.Setup(x => x.IsFileAcceptable(It.IsAny<Stream>(), out It.Ref<string>.IsAny))
				.Returns(new IsFileAcceptableReturns((Stream stream, out string apparentFileType) =>
				{
					var buffer = new byte[2];
					var bytesRead = stream.Read(buffer, 0, buffer.Length); // it's fine if the file is short
					if (buffer[0] == 0x4D && buffer[1] == 0x5A)
					{
						apparentFileType = "exe";
					}
					else
					{
						apparentFileType = null;
					}
					return apparentFileType != "exe";
				}
				));
			controller = BuildController(mockService.Object);
		}
	}
}
