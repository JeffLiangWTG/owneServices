using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Service;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class DocumentsControllerTest : TestCaseWithFactory
	{
		public void TestGetDocuments_AsWebUser()
		{
			SetUpContactPrincipal();

			TestGetDocumentsCore();
		}

		public void TestGetDocuments_AsStaffUser()
		{
			SetUpStaffPrincipal();

			TestGetDocumentsCore();
		}

		void TestGetDocumentsCore()
		{
			var businessContext = "BC";
			var entityPK = Guid.NewGuid();
			var entityTableCode = "ET";
			var documentListItems = new[]
			{
				new DocumentListItem
				{
					Id = Guid.NewGuid(),
					Name = "Name",
					Summary = "Summary",
					Path = "Path",
					Index = 96,
					DownloadOnly = false,
					IsApplicable = true
				}
			};

			mockListService
				.Setup(x => x.GetDocumentList(businessContext, It.IsAny<OrgContact>(), entityPK, entityTableCode))
				.Returns(documentListItems)
				.Verifiable();

			var actionResult = controller.GetDocuments(businessContext, entityPK, entityTableCode);
			AssertJsonResult(documentListItems, actionResult);

			mockListService.Verify();
		}

		[ExpectNoExceptions]
		public void TestGetDocuments_UsesDbSafelyFromAnotherThread()
		{
			SetUpStaffPrincipal();

			var businessContext = "BC";
			var entityPK = Guid.NewGuid();
			var entityTableCode = "ET";
			var documentListItems = Array.Empty<DocumentListItem>();

			mockListService
				.Setup(x => x.GetDocumentList(businessContext, null, entityPK, entityTableCode))
				.Callback(() => { var dbc = Db.Connection; })
				.Returns(documentListItems)
				.Verifiable();

			RunSafelyFromAnotherThread(() => controller.GetDocuments(businessContext, entityPK, entityTableCode));
		}

		public void TestGetDocuments_EmptyBusinessContextIsNotAllowed()
		{
			SetUpStaffPrincipal();

			var actionResult = controller.GetDocuments(string.Empty);
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertContains("Business context must be specified.", content);
			mockListService.Verify(x => x.GetDocumentList(It.IsAny<string>(), It.IsAny<OrgContact>(), It.IsAny<ZGuid>(), It.IsAny<string>()), Times.Never());
		}

		public void TestGetDocuments_OnlyBusinessContextIsMandatory()
		{
			SetUpStaffPrincipal();

			var mandatoryParameters = controller.GetType()
				.GetMethod(nameof(controller.GetDocuments))
				.GetParameters()
				.Where(p => !p.IsOptional)
				.Select(p => p.Name)
				.ToList();

			AssertSequencesEqual(new[] { "businessContext" }, mandatoryParameters);
		}

		public void TestGetDocuments_OptionalParametersFallBackToEmptyValues()
		{
			SetUpStaffPrincipal();

			var businessContext = "BC";
			var documentListItems = new[] { new DocumentListItem() };

			mockListService
				.Setup(x => x.GetDocumentList(businessContext, null, Guid.Empty, string.Empty))
				.Returns(documentListItems)
				.Verifiable();

			var actionResult = controller.GetDocuments(businessContext);
			AssertJsonResult(documentListItems, actionResult);

			mockListService.Verify();
		}

		[ExpectNoExceptions]
		public void TestDeliverDocument_AsWebUser()
		{
			SetUpContactPrincipal();

			TestDeliverDocumentCore(true);
		}

		[ExpectNoExceptions]
		public void TestDeliverDocument_AsStaffUser()
		{
			SetUpStaffPrincipal();

			TestDeliverDocumentCore(false);
		}

		void TestDeliverDocumentCore(bool isWebUser)
		{
			var recipient = new DeliveryRecipientBase()
			{
				DeliveryMethod = "EML",
				Email = "jd.merino@wisetechgloba.com",
				EmailAttachmentType = "T1",
				FaxNumber = null,
				Name = "Name 1",
				OrganizationId = Guid.NewGuid()
			};

			var document = new DocumentDetail()
			{
				Id = Guid.NewGuid(),
				Mode = "M1",
				Name = "Doc 1",
				ShouldInclude = true
			};

			var deliveryInstructions = new DeliveryInstructionsBase()
			{
				Copies = 3,
				CoverNote = null,
				IsDraft = true,
				Language = null,
				PrinterId = Guid.NewGuid(),
				Recipients = new[]
				{
					recipient
				}
			};

			var request = new DeliveryRequest()
			{
				BusinessObjectPk = Guid.NewGuid(),
				DocumentCommandPk = Guid.NewGuid(),
				TablePrefix = "SomeTablePrefix",
				DeliveryInstructions = deliveryInstructions,
				Documents = new[]
				{
					document
				}
			};

			mockDeliveryService.Setup(mock => mock.DeliverDocument(
				request.DocumentCommandPk,
				request.TablePrefix,
				request.BusinessObjectPk,
				request.DeliveryInstructions,
				request.Documents
			)).Returns(new ActionResult(true, "")).Verifiable();

			var response = controller.DeliverDocument(request).ExecuteAsync(CancellationToken.None).Result;

			if (isWebUser)
			{
				AssertEquals(response.StatusCode, HttpStatusCode.Forbidden);
			}
			else
			{
				AssertEquals(response.StatusCode, HttpStatusCode.OK);
			}

			mockDeliveryService.Verify(
				mock => mock.DeliverDocument(
					request.DocumentCommandPk,
					request.TablePrefix,
					request.BusinessObjectPk,
					request.DeliveryInstructions,
					request.Documents),
				isWebUser ? Times.Never() : Times.Once());
		}

		[ExpectNoExceptions]
		public void TestDeliverDocumentUsesDbSafelyFromAnotherThread()
		{
			SetUpStaffPrincipal();

			mockDeliveryService
				.Setup(x => x.DeliverDocument(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<DeliveryInstructionsBase>(), It.IsAny<DocumentDetail[]>()))
				.Callback(() => { var dbc = Db.Connection; })
				.Returns(new ActionResult(true, ""))
				.Verifiable();

			RunSafelyFromAnotherThread(() =>
			{
				var request = new DeliveryRequest();
				controller.DeliverDocument(request);
			});
		}

		public void TestDeliverDocumentWhenInstructionNull()
		{
			SetUpStaffPrincipal();
			var request = new DeliveryRequest();

			mockDeliveryService.Setup(mock => mock.DeliverDocument(
				request.DocumentCommandPk,
				request.TablePrefix,
				request.BusinessObjectPk,
				request.DeliveryInstructions,
				request.Documents
			)).Returns(new ActionResult(true, "")).Verifiable();

			AssertNull(request.DeliveryInstructions);

			var response = controller.DeliverDocument(request).ExecuteAsync(CancellationToken.None).Result;
			AssertEquals(response.StatusCode, HttpStatusCode.BadRequest);
		}

		public void TestDeliverDocumentWhenInstructionRecipientsNull()
		{
			SetUpStaffPrincipal();

			var request = new DeliveryRequest() { DeliveryInstructions  = new DeliveryInstructionsBase() };
			mockDeliveryService.Setup(mock => mock.DeliverDocument(
				request.DocumentCommandPk,
				request.TablePrefix,
				request.BusinessObjectPk,
				request.DeliveryInstructions,
				request.Documents
			)).Returns(new ActionResult(true, "")).Verifiable();

			AssertNull(request.DeliveryInstructions.Recipients);
			var response = controller.DeliverDocument(request).ExecuteAsync(CancellationToken.None).Result;
			AssertEquals(response.StatusCode, HttpStatusCode.BadRequest);

			request = new DeliveryRequest() { DeliveryInstructions = new DeliveryInstructionsBase() { Recipients = Array.Empty<DeliveryRecipientBase>() } };
			mockDeliveryService.Setup(mock => mock.DeliverDocument(
				request.DocumentCommandPk,
				request.TablePrefix,
				request.BusinessObjectPk,
				request.DeliveryInstructions,
				request.Documents
			)).Returns(new ActionResult(true, "")).Verifiable();

			AssertEquals(0, request.DeliveryInstructions.Recipients.Length);
			response = controller.DeliverDocument(request).ExecuteAsync(CancellationToken.None).Result;
			AssertEquals(response.StatusCode, HttpStatusCode.BadRequest);

			request = new DeliveryRequest() { DeliveryInstructions = new DeliveryInstructionsBase() { Recipients = new DeliveryRecipientBase[] { null } } };
			mockDeliveryService.Setup(mock => mock.DeliverDocument(
				request.DocumentCommandPk,
				request.TablePrefix,
				request.BusinessObjectPk,
				request.DeliveryInstructions,
				request.Documents
			)).Returns(new ActionResult(true, "")).Verifiable();

			Assert(request.DeliveryInstructions.Recipients.Any(recipient => recipient == null));
			response = controller.DeliverDocument(request).ExecuteAsync(CancellationToken.None).Result;
			AssertEquals(response.StatusCode, HttpStatusCode.BadRequest);
		}

		public void TestDeliverDocumentWhenServiceDeliveryFails()
		{
			SetUpStaffPrincipal();

			var request = new DeliveryRequest() { DeliveryInstructions = new DeliveryInstructionsBase() { Recipients = new[] { new DeliveryRecipientBase() } } };
			mockDeliveryService.Setup(mock => mock.DeliverDocument(
				request.DocumentCommandPk,
				request.TablePrefix,
				request.BusinessObjectPk,
				request.DeliveryInstructions,
				request.Documents
			)).Returns(new ActionResult(false, "Service delivery fails")).Verifiable();

			var response = controller.DeliverDocument(request).ExecuteAsync(CancellationToken.None);
			AssertEquals(HttpStatusCode.BadRequest, response.Result.StatusCode);
			var resultContent = ((ObjectContent)response.Result.Content).Value.ToString();
			AssertEquals("{ IsKnownError = True, Message = Service delivery fails }", resultContent);
		}

		public void TestGetDeliveryInstructions()
		{
			var documentCommandPk = Guid.NewGuid();
			var tablePrefix = "ET";
			var businessObjectPk = Guid.NewGuid();

			var recipient1 = new RecipientDetail()
			{
				OrganizationID = Guid.NewGuid(),
				Organization = "TestOrg1",
				Name = "Test1",
				Address = "TestAddress1",
				AttachmentType = "Text",
				DeliveryMethod = "CarrierPigeon"
			};
			var recipients = new[] { recipient1 };

			var document1 = new DocumentDetail()
			{
				Id = Guid.NewGuid(),
				Name = "Document1",
				Mode = "Mode1",
				ShouldInclude = true
			};
			var documents = new[] { document1 };

			var printer1Guid = Guid.NewGuid();
			var printer1 = new PrinterDetail()
			{
				ID = printer1Guid,
				Name = "Printer1",
				Location = "Location1"
			};
			var printers = new[] { printer1 };

			var expectedInstruction = new DeliveryInstructionDetail()
			{
				Recipients = recipients,
				Documents = documents,
				Printers = printers,
				PrinterPK = printer1Guid,
				ShowOnlyPrintersUserCanPrintTo = true,
				CanPreview = true,
			};

			mockDeliveryService
				.Setup(x => x.GetDeliveryRecipients(documentCommandPk, tablePrefix, businessObjectPk))
				.Returns(recipients)
				.Verifiable();

			mockDeliveryService
				.Setup(x => x.GetDocuments(documentCommandPk))
				.Returns(documents)
				.Verifiable();

			mockDeliveryService
				.Setup(x => x.GetPrinters())
				.Returns(printers)
				.Verifiable();

			mockDeliveryService
				.Setup(x => x.GetDefaultPrinterKey(documentCommandPk))
				.Returns(printer1Guid);

			mockDeliveryService
				.Setup(x => x.ShowOnlyPrintersUserCanPrintTo)
				.Returns(true)
				.Verifiable();

			mockDeliveryService
				.Setup(x => x.CanPreview(documentCommandPk))
				.Returns(true)
				.Verifiable();

			var actionResult = controller.GetDeliveryInstructions(documentCommandPk, tablePrefix, businessObjectPk);

			AssertJsonResult(expectedInstruction, actionResult);

			mockDeliveryService.Verify();
		}

		[ExpectNoExceptions]
		public void TestGetDeliveryInstructions_UsesDbSafelyFromAnotherThread()
		{
			var documentCommandPk = Guid.NewGuid();
			var tablePrefix = "ET";
			var businessObjectPk = Guid.NewGuid();
			var recipients = Array.Empty<RecipientDetail>();
			var documents = Array.Empty<DocumentDetail>();
			var printers = Array.Empty<PrinterDetail>();

			mockDeliveryService
				.Setup(x => x.GetDeliveryRecipients(documentCommandPk, tablePrefix, businessObjectPk))
				.Callback(() => { var dbc = Db.Connection; })
				.Returns(recipients);

			mockDeliveryService
				.Setup(x => x.GetDocuments(documentCommandPk))
				.Returns(documents);

			mockDeliveryService
				.Setup(x => x.GetPrinters())
				.Returns(printers);

			mockDeliveryService
				.Setup(x => x.GetDefaultPrinterKey(documentCommandPk))
				.Returns<Guid?>(null);

			mockDeliveryService
				.Setup(x => x.ShowOnlyPrintersUserCanPrintTo)
				.Returns(true)
				.Verifiable();

			mockDeliveryService
				.Setup(x => x.CanPreview(documentCommandPk))
				.Returns(true);

			RunSafelyFromAnotherThread(() => controller.GetDeliveryInstructions(documentCommandPk, tablePrefix, businessObjectPk));
		}

		public void TestGetDeliveryContact()
		{
			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "MyCode";

			var address = organization.MainAddress;
			address.OA_Address1 = "My Address";

			var emailContact = organization.Contacts.AddNew();
			emailContact.OC_IsActive = ZBool.True;
			emailContact.OC_ContactName = "Sir Ken Robinson";
			emailContact.OC_Email = "sir.ken.robinson@cargowise.com";
			emailContact.OC_Fax = "11111111";
			var emailDocument = emailContact.Documents.AddNew();
			emailDocument.OD_DocumentGroup = ContactType.All.Code;
			emailDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			emailDocument.OD_AttachmentType = "PDF";
			emailDocument.OD_FilterShipmentMode = "ALL";
			emailDocument.OD_FilterDirection = "ALL";
			emailDocument.OD_CarbonCopyRecipientsAsString = "cc1@qq.com, cc2@qq.com";
			emailDocument.OD_BlindCarbonCopyRecipientsAsString = "bcc1@qq.com, bcc2@qq.com";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_AddressCategory = OrgAddressCategory.Codes.Office;
			documentCommand.Parent = organization;

			Factory.Save();

			var organizationPK = organization.PK.ToGuid();
			var name = emailContact.OC_ContactName;
			var deliveryMethod = Core.Constants.ContactNotifyModes.Email;
			var attachmentType = "PDF";
			var documentCommandPk = documentCommand.PK.ToGuid();
			var tablePrefix = organization.TablePrefix;
			var businessObjectPk = organization.PK.ToGuid();
			var actionResult = controller.GetDeliveryContact(organizationPK, name, deliveryMethod, attachmentType, documentCommandPk, tablePrefix, businessObjectPk);

			var expectedContact = new RecipientDetail()
			{
				Name = "Sir Ken Robinson",
				AttachmentType = "PDF",
				Address = "sir.ken.robinson@cargowise.com",
				DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
				CC = "cc1@qq.com, cc2@qq.com",
				BCC = "bcc1@qq.com, bcc2@qq.com",
			};
			AssertJsonResult(expectedContact, actionResult);
		}

		public void TestGetAllAvailablePrinters()
		{
			var expectedResult = new PrinterDetail[] { new PrinterDetail { ID = Guid.NewGuid(), Name = "Printer", Location = "Server" } };

			mockDeliveryService.Setup(x => x.GetPrinters()).Returns(expectedResult).Verifiable();

			var actionResult = controller.GetAllAvailablePrinters();

			AssertJsonResult(expectedResult, actionResult);

			mockDeliveryService.Verify();
		}

		[ExpectNoExceptions]
		public void TestGetAllAvailablePrinters_UsesDbSafelyFromAnotherThread()
		{
			mockDeliveryService
				.Setup(x => x.GetPrinters())
				.Returns<PrinterDetail[]>(null);

			RunSafelyFromAnotherThread(() => controller.GetAllAvailablePrinters());
		}

		public void TestGetDefaultPrinter()
		{
			var documentCommandPk = Guid.NewGuid();
			var printerPK = Guid.NewGuid();

			mockDeliveryService
				.Setup(x => x.GetDefaultPrinterKey(documentCommandPk))
				.Returns(printerPK)
				.Verifiable();

			var actionResult = controller.GetDefaultPrinter(documentCommandPk);

			AssertJsonResult(printerPK, actionResult);

			mockDeliveryService.Verify();
		}

		public void TestCanDeliverDocument_CheckDataStateReturnsValid()
		{
			var documentCommandPk = Guid.NewGuid();
			var tablePrefix = "ET";
			var businessObjectPk = Guid.NewGuid();

			mockDeliveryService
				.Setup(x => x.CheckDataState(documentCommandPk, tablePrefix, businessObjectPk))
				.Returns(new DocumentSupporterDataState() { IsValid = true })
				.Verifiable();

			var expectedResult = new CanDeliverResult() { CanDeliver = true, ErrorMessage = "" };

			var actionResult = controller.CanDeliverDocument(documentCommandPk, tablePrefix, businessObjectPk);

			AssertJsonResult(expectedResult, actionResult);

			mockDeliveryService.Verify();
		}

		public void TestCanDeliverDocument_CheckDataStateReturnsInvalid()
		{
			var documentCommandPk = Guid.NewGuid();
			var tablePrefix = "ET";
			var businessObjectPk = Guid.NewGuid();

			mockDeliveryService
				.Setup(x => x.CheckDataState(documentCommandPk, tablePrefix, businessObjectPk))
				.Returns(new DocumentSupporterDataState() { IsValid = false, ErrorMessage = "Resriction condition no met." })
				.Verifiable();

			var expectedResult = new CanDeliverResult() { CanDeliver = false, ErrorMessage = "Resriction condition no met." };

			var actionResult = controller.CanDeliverDocument(documentCommandPk, tablePrefix, businessObjectPk);

			AssertJsonResult(expectedResult, actionResult);

			mockDeliveryService.Verify();
		}

		[ExpectNoExceptions]
		public void TestGetDefaultPrinter_UsesDbSafelyFromAnotherThread()
		{
			var documentCommandPk = Guid.NewGuid();

			mockDeliveryService
				.Setup(x => x.GetDefaultPrinterKey(documentCommandPk))
				.Returns<Guid?>(null);

			RunSafelyFromAnotherThread(() => controller.GetDefaultPrinter(documentCommandPk));
		}

		public void TestGetPreview_WithInvalidInputs()
		{
			mockPreviewService.Setup(x => x.GetDocumentCommand(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>())).Returns<byte[]>(null);
			
			var actionResult = controller.GetPreview(Guid.NewGuid(), "XX", Guid.NewGuid());
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals("Response should be NotFound", HttpStatusCode.NotFound, response.StatusCode);

			mockPreviewService.Setup(x => x.GetDocumentCommand(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>())).Returns(DocumentCommand.New(new BusinessObjectFactory()));
			mockDeliveryService.Setup(x => x.CanPreview(It.IsAny<Guid>())).Returns(false).Verifiable();

			actionResult = controller.GetPreview(Guid.NewGuid(), "XX", Guid.NewGuid());
			response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals("Response should be Forbidden", HttpStatusCode.Forbidden, response.StatusCode);
		}

		public void TestGetPreview_DocumentPreviewException()
		{
			var documentCommand = DocumentCommand.New(new BusinessObjectFactory());
			documentCommand.SU_MenuName = "Pub System Shipment Document";
			var businessObjectPk = Guid.NewGuid();

			mockPreviewService.Setup(x => x.GetDocumentCommand(documentCommand.PK.ToGuid(), "XX", businessObjectPk)).Returns(documentCommand);
			mockPreviewService.Setup(x => x.WriteDocumentPreview(documentCommand, It.IsAny<Stream>()))
				.Throws(new DocumentPreviewException("Error due to preview", new Exception()));
			mockDeliveryService.Setup(x => x.CanPreview(It.IsAny<Guid>())).Returns(true).Verifiable();

			var actionResult = controller.GetPreview(documentCommand.PK.ToGuid(), "XX", businessObjectPk);
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult().Replace("&nbsp;", " ");

			var expectedContent = @"<center><p><strong><span class=""g-header1"">Errors were found in the template!</span></strong></p>
<p>Errors were found during the generation of this template.</p>
<p>Technical message: 'Error due to preview'</p>
<p>Please fix the template and try again.</p></center>";
			AssertEquals("Response should be InternalServerError", response.StatusCode, HttpStatusCode.InternalServerError);
			AssertEquals("Should contain message", expectedContent, content);
		}

		public void TestGetPreview_WithValidInputs()
		{
			var documentCommand = DocumentCommand.New(new BusinessObjectFactory());
			documentCommand.SU_MenuName = "Pub System Shipment Document";
			var businessObjectPk = Guid.NewGuid();

			var documentPreview = new byte[] { 1, 2, 3 };
			mockPreviewService.Setup(x => x.GetDocumentCommand(documentCommand.PK.ToGuid(), "XX", businessObjectPk)).Returns(documentCommand);
			mockPreviewService.Setup(x => x.WriteDocumentPreview(documentCommand, It.IsAny<Stream>()))
				.Callback<DocumentCommand, Stream>((_, s) => s.Write(documentPreview, 0, documentPreview.Length));
			mockDeliveryService.Setup(x => x.CanPreview(documentCommand.PK.ToGuid())).Returns(true).Verifiable();

			var actionResult = controller.GetPreview(documentCommand.PK.ToGuid(), "XX", businessObjectPk, ContentDispositionType.Attachment);
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

			AssertEquals("Response should be OK", response.StatusCode, HttpStatusCode.OK);
			AssertEquals("Result should match", response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult(), documentPreview);
			AssertEquals("MediaType should be PDF", response.Content.Headers.ContentType.MediaType, DataContentTypes.Pdf);
			AssertEquals("DispositionType should be attachment", response.Content.Headers.ContentDisposition.DispositionType, DispositionTypeNames.Attachment);
			AssertEquals("Filename should be {SU_MenuName}.pdf", response.Content.Headers.ContentDisposition.FileName, $"\"{documentCommand.SU_MenuName}.pdf\"");
		}

		Mock<IDocumentDeliveryService> mockDeliveryService;
		Mock<IDocumentListService> mockListService;
		Mock<IDocumentPreviewService> mockPreviewService;
		DocumentsController controller;

		protected override void SetUp()
		{
			base.SetUp();

			mockDeliveryService = new Mock<IDocumentDeliveryService>(MockBehavior.Strict);
			mockListService = new Mock<IDocumentListService>(MockBehavior.Strict);
			mockPreviewService = new Mock<IDocumentPreviewService>(MockBehavior.Strict);

			controller = new DocumentsController(mockDeliveryService.Object, mockListService.Object, mockPreviewService.Object);
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage())
			{
				Controller = controller
			};
		}

		void SetUpStaffPrincipal()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
		}

		void SetUpContactPrincipal()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);
		}

		void RunSafelyFromAnotherThread(Action actionToRun)
		{
			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					actionToRun();
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		static void AssertJsonResult(object expected, IHttpActionResult actionResult)
		{
			var expectedJson = JsonConvert.SerializeObject(expected);

			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var actualJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			AssertEquals(expectedJson, actualJson);
		}
	}
}
