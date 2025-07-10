using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Service;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Authentication;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class DocumentsController : ApiController
	{
		readonly IDocumentDeliveryService deliveryService;
		readonly IDocumentListService listService;
		readonly IDocumentPreviewService documentPreviewService;

		public DocumentsController()
			: this(new DocumentDeliveryService(), new DocumentListService(), new DocumentPreviewService())
		{
		}

		public DocumentsController(IDocumentDeliveryService deliveryService, IDocumentListService listService, IDocumentPreviewService documentPreviewService)
		{
			this.deliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
			this.listService = listService ?? throw new ArgumentNullException(nameof(listService));
			this.documentPreviewService = documentPreviewService ?? throw new ArgumentNullException(nameof(documentPreviewService));
		}

		[Route("api/documents")]
		[HttpGet]
		public IHttpActionResult GetDocuments(string businessContext, Guid? entityPK = null, string entityTableCode = null)
		{
			if (string.IsNullOrEmpty(businessContext))
			{
				return BadRequest((NoResString)"Business context must be specified."); // Exception message
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new CargoWise.EntityFramework.BusinessObjectFactory { NameForDebugging = "Documents WebService Get Documents" };
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contact = identity.GetContact(factory);

				var documents = listService.GetDocumentList(businessContext, contact, entityPK ?? Guid.Empty, entityTableCode ?? string.Empty);
				return Json(documents);
			}
		}

		[Route("api/documents/deliver")]
		[HttpPost]
		public IHttpActionResult DeliverDocument([FromBody] DeliveryRequest deliveryRequest)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (deliveryRequest?.DeliveryInstructions == null)
				{
					return BadRequest((NoResString)"DeliveryInstructions context must be specified."); // Exception message
				}
				else if (deliveryRequest.DeliveryInstructions.Recipients == null || deliveryRequest.DeliveryInstructions.Recipients.Length == 0 || deliveryRequest.DeliveryInstructions.Recipients.Any(recipient => recipient == null))
				{
					return BadRequest((NoResString)"Recipients cannot be null or empty");
				}

				var deliverResult = deliveryService.DeliverDocument(deliveryRequest.DocumentCommandPk, deliveryRequest.TablePrefix, deliveryRequest.BusinessObjectPk, deliveryRequest.DeliveryInstructions, deliveryRequest.Documents);
				if (deliverResult.Success)
				{
					return Ok();
				}
				else
				{
					return Content(
						HttpStatusCode.BadRequest,
						new { IsKnownError = true, Message = deliverResult.Message },
						Configuration.Formatters.JsonFormatter);
				}
			}
		}

		[Route("api/documents/instructions")]
		[HttpGet]
		public IHttpActionResult GetDeliveryInstructions(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var recipientDetails = deliveryService.GetDeliveryRecipients(documentCommandPk, tablePrefix, businessObjectPk);
				var documentDetails = deliveryService.GetDocuments(documentCommandPk);

				var printerDetails = deliveryService.GetPrinters();
				var defaultPrinterPK = deliveryService.GetDefaultPrinterKey(documentCommandPk);

				var canPreview = deliveryService.CanPreview(documentCommandPk);

				var instructionDetail = new DeliveryInstructionDetail()
				{
					Recipients = recipientDetails,
					Documents = documentDetails,
					Printers = printerDetails,
					PrinterPK = defaultPrinterPK,
					ShowOnlyPrintersUserCanPrintTo = deliveryService.ShowOnlyPrintersUserCanPrintTo,
					CanPreview = canPreview,
				};

				return Json(instructionDetail);
			}
		}

		[Route("api/documents/deliverycontact")]
		[HttpGet]
		public IHttpActionResult GetDeliveryContact(Guid organizationPK, string name, string deliveryMethod, string attachmentType, Guid documentCommandPK, string tablePrefix, Guid businessObjectPK)
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory() { NameForDebugging = "DocumentDelivery WebService Email Delivery Recipient" };
			var documentCommand = factory.Load<DocumentEngine.DocumentCommand>(documentCommandPK);
			var documentSupportable = factory.Load(tablePrefix, businessObjectPK) as DocumentEngineCore.DocumentSupport.IDocumentSupportable;

			var deliveryContact = new DocDeliveryContact(factory, new GlowDocAutoDelivery(documentCommand, documentSupportable.DocumentSupporter));
			deliveryContact.OrgHeaderPK = new CargoWise.Types.ZGuid(organizationPK);
			deliveryContact.Name = name;
			deliveryContact.DeliveryMethod = deliveryMethod;
			deliveryContact.AttachmentType = attachmentType;

			return Json(new RecipientDetail
			{
				Name = deliveryContact.Name,
				AttachmentType = deliveryContact.AttachmentType,
				Address = deliveryContact.DeliveryAddress,
				DeliveryMethod = deliveryContact.DeliveryMethod,
				CC = deliveryContact.EmailCarbonCopyRecipientsAsString,
				BCC = deliveryContact.EmailBlindCarbonCopyRecipientsAsString,
			});
		}

		[Route("api/documents/getAllAvailablePrinters")]
		[HttpGet]
		public IHttpActionResult GetAllAvailablePrinters()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(deliveryService.GetPrinters());
			}
		}

		[Route("api/documents/defaultPrinter")]
		[HttpGet]
		public IHttpActionResult GetDefaultPrinter(Guid documentId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var defaultPrinterPK = deliveryService.GetDefaultPrinterKey(documentId);
				return Json(defaultPrinterPK);
			}
		}

		[Route("api/documents/canDeliverDocument")]
		[HttpGet]
		public IHttpActionResult CanDeliverDocument(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var documentSupportState = deliveryService.CheckDataState(documentCommandPk, tablePrefix, businessObjectPk);
				var canDeliverResult = new CanDeliverResult()
				{
					CanDeliver = documentSupportState.IsValid,
					ErrorMessage = documentSupportState.ErrorMessage,
				};

				return Json(canDeliverResult);
			}
		}

		[Route("api/documents/preview")]
		[HttpGet]
		public IHttpActionResult GetPreview(Guid documentCommandPk, string tablePrefix, Guid businessObjectPk, ContentDispositionType disposition = ContentDispositionType.Inline)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var documentCommand = documentPreviewService.GetDocumentCommand(documentCommandPk, tablePrefix, businessObjectPk);
				if (documentCommand == null)
				{
					return NotFound();
				}

				if (!deliveryService.CanPreview(documentCommandPk))
				{
					return StatusCode(HttpStatusCode.Forbidden);
				}

				var previewStream = new MemoryStream();
				try
				{
					documentPreviewService.WriteDocumentPreview(documentCommand, previewStream);
				}
				catch (DocumentPreviewException exception)
				{
					var line1 = Res.GetString("16599174-7a2b-46ba-911b-878a3799cc55", "Errors were found in the template!");
					var line2 = Res.GetString("97da1ffc-3de4-4465-8d66-6c958eb78300", "Errors were found during the generation of this template.");
					var line3 = Res.GetString("d9b1ff54-b411-408a-a66d-8d8183b39d27", "Technical message: ");
					var line4 = Res.GetString("f0102f97-9dd9-476b-aa09-664cd7f41b6e", "Please fix the template and try again.");
					#region SuppressResourceStringsCheckRegion Reason = html template
					var message = $@"<center><p><strong><span class=""g-header1"">{HttpUtility.HtmlEncode(line1)}</span></strong></p>
<p>{HttpUtility.HtmlEncode(line2)}</p>
<p>{HttpUtility.HtmlEncode(line3)}'{HttpUtility.HtmlEncode(exception.Message)}'</p>
<p>{HttpUtility.HtmlEncode(line4)}</p></center>";
					#endregion
					var errorContent = new StringContent(message);
					errorContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");
					var errorResponse = Request.CreateResponse(HttpStatusCode.InternalServerError);
					errorResponse.Content = errorContent;

					return ResponseMessage(errorResponse);
				}

				previewStream.Seek(0, SeekOrigin.Begin);
				// We are using stream content with memory stream input here instead of directly writing to the HttpResponseStream
				// since the FlexCel code used to convert the document to pdf requires a seekable stream.
				var content = new StreamContent(previewStream);
				content.Headers.ContentLength = previewStream.Length;
				content.Headers.ContentType = new MediaTypeHeaderValue(DataContentTypes.Pdf);
				var dispositionType = disposition == ContentDispositionType.Attachment ? DispositionTypeNames.Attachment : DispositionTypeNames.Inline;
				content.Headers.ContentDisposition = new ContentDispositionHeaderValue(dispositionType) { FileName = documentCommand.SU_MenuName + ".pdf" };
				var response = new HttpResponseMessage { Content = content };

				return ResponseMessage(response);
			}
		}
	}

	[DataContract]
	public class CanDeliverResult
	{
		[DataMember]
		public bool CanDeliver { get; set; }
		[DataMember]
		public string ErrorMessage { get; set; }
	}
}
