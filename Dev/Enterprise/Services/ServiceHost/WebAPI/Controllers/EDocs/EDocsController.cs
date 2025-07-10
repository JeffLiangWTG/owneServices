using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.DocumentScanning.Services;
using Enterprise.Services.ServiceHost.WebAPI.Authentication;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Utilities;
using Newtonsoft.Json;
using IDocumentScanningEDocsService = Enterprise.DocumentScanning.Services.IEDocsService;

namespace Enterprise.Services.ServiceHost
{
	#region SuppressResourceStringsCheckRegion

	[GlowTicketAuthentication(StrictEndpoint = true)]
	public class EDocsController : ApiController
	{
		readonly IDocumentScanningEDocsService service;

		public EDocsController()
			: this(new EDocsService())
		{
		}

		public EDocsController(IDocumentScanningEDocsService service)
		{
			this.service = service;
		}

		//We need this to make sure that ReadAsMultipartAsync is executed on a separate thread.
		MultipartMemoryStreamProvider ReadAsMultipart()
		{
			var provider = Task.Factory.StartNew(() => Request.Content.ReadAsMultipartAsync().GetAwaiter().GetResult(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).GetAwaiter().GetResult();
			return provider;
		}

		[Route("api/edocs/{recordPrefix}/{recordPK}")]
		[HttpPost]
		public async Task<IHttpActionResult> Upload(string recordPrefix, Guid recordPK)
		{
			if (!Request.Content.IsMimeMultipartContent())
			{
				return StatusCode(HttpStatusCode.UnsupportedMediaType);
			}

			var provider = ReadAsMultipart();

			// get content
			if (!TryGetContent(provider, "content", out var contentContent))
			{
				return BadRequest("No content specified.");
			}
			if (!TryGetFileName(contentContent, out var fileName))
			{
				return BadRequest("File name must be specified.");
			}

			byte[] contentBytes = null;
			try
			{
				if (!service.IsFileAcceptable(fileName))
				{
					return BadRequest(string.Format("File cannot be added because it has a potentially dangerous file name ({0}).", fileName));
				}

				contentBytes = await contentContent.ReadAsByteArrayAsync().ConfigureAwait(false);
				using (var stream = new MemoryStream(contentBytes))
				{
					if (!service.IsFileAcceptable(stream, out string apparentFileType))
					{
						return BadRequest(string.Format("File cannot be added because it has a potentially dangerous file type ({0}).", apparentFileType));
					}
				}
			}
			catch (NotSupportedException ex)
			{
				return BadRequest(ex.Message);
			}

			// get detail
			if (!TryGetContent(provider, "detail", out var detailContent))
			{
				return BadRequest("No detail specified.");
			}

			if (!(await GetDetailAsync(detailContent).ConfigureAwait(false) is EDocsDetailArgs detailArgs))
			{
				return BadRequest("EDocs detail is not in the correct format.");
			}

			// get result
			var detail = new EDocDetail
			{
				FileName = fileName,
				DocumentTypePK = detailArgs.RefDocTypePK,
				Description = detailArgs.Description,
				IsPublished = detailArgs.IsPublished
			};
			return AddOrUpdateEDoc(recordPrefix, recordPK, detail, contentBytes);
		}

		[Route("api/edocs/{recordPrefix}/{recordPK}/{eDocPK}/details")]
		[HttpPut]
		public IHttpActionResult Update(string recordPrefix, Guid recordPK, Guid eDocPK, [FromBody] EDocsDetailArgs detailArgs)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var detail = new EDocDetail
				{
					Id = eDocPK,
					DocumentTypePK = detailArgs.RefDocTypePK,
					Description = detailArgs.Description,
					IsPublished = detailArgs.IsPublished
				};
				return AddOrUpdateEDoc(recordPrefix, recordPK, detail, null);
			}
		}

		[Route("api/edocs/{recordPK}/details")]
		[HttpGet]
		public IHttpActionResult GetEDocDetails(Guid recordPK, bool includeDeleted)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var includeUnpublished = GlowPrincipalHelper.IsStaff(User);
				return Json(service.GetEDocDetails(recordPK, includeDeleted, includeUnpublished, ContactPK));
			}
		}

		ZGuid? ContactPK
		{
			get
			{
				if (User?.Identity is IGlowAuthenticationTicketIdentity identity && identity.ProviderType == OrgContactSchema.Constants.Prefix)
				{
					return (ZGuid)identity.ProviderKey;
				}

				return null;
			}
		}

		[Route("api/edocs/{recordPK}/count")]
		[HttpGet]
		public IHttpActionResult GetEDocCount(Guid recordPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(service.GetEDocCount(recordPK, ContactPK));
			}
		}

		[Route("api/edocs/getAllToken")]
		[HttpPost]
		public IHttpActionResult GetAllEDocsToken([FromBody] Guid[] pks)
		{
			if (pks == null || pks.Length == 0)
			{
				return BadRequest();
			}

			var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
			if (identity == null)
			{
				return Unauthorized();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var token = GetDownloadToken(pks, identity.ProviderKey);

				return Ok(token.ToString());
			}
		}

		static ZGuid GetDownloadToken(Guid[] pks, Guid providerKey)
		{
			var factory = GetNewFactory();

			var query = new ZQuery();
			query.AddToFilter(StmDataSchema.SD_Owner, providerKey);
			query.AddToFilter(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, TokenPrefix);
			var stmData = factory.LoadTop1<StmData>(query) ?? factory.New<StmData>();

			var token = Guid.NewGuid();
			stmData.SD_Name = GetStmDataName(token);
			stmData.SD_Owner = providerKey;
			var data = JsonConvert.SerializeObject(pks);
			stmData.SD_BinaryValue = ZBlob.FromUTF8(data);

			factory.Save();

			return token;
		}

		static BusinessObjectFactory GetNewFactory() => new BusinessObjectFactory { NameForDebugging = nameof(EDocsController) };

		static string GetStmDataName(Guid token) => TokenPrefix + token;

		const string TokenPrefix = $"{nameof(EDocsController)}-GetAll";

		[Route("api/edocs/getAll")]
		[HttpGet]
		public IHttpActionResult GetAllEDocsData([FromUri] Guid token)
		{
			var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
			if (identity == null)
			{
				return Unauthorized();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = GetNewFactory();
				var data = new StmData.Loader(factory).LoadTop1(GetStmDataName(token), identity.ProviderKey, ZGuid.Empty);
				var pks = data != null ? JsonConvert.DeserializeObject<Guid[]>(data.SD_BinaryValue.ToUTF8()) : null;

				if (pks == null || data.SD_SystemLastEditTimeUtc.AddMinutes(5) < ZDateTime.UtcNow)
				{
					return Ok("Your file download has expired. Please try again.");
				}

				var outputFileName = $"eDocs_{ZDateTime.Now.ToString("yyyyMMdd-HHmmss-fff")}.zip";
				var response = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new PushStreamContent((outputStream, httpContext, transportContext) =>
					{
						using (Db.DisposableActionForDbConnection())
						using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
						using (var zip = new ZipEDocsCreator())
						{
							var includeUnpublished = GlowPrincipalHelper.IsStaff(User);
							var eDocs = pks
								.SelectMany(pk => service.GetEDocDetails(pk, includeDeleted: false, includeUnpublished, ContactPK));

							foreach (var eDoc in eDocs)
							{
								var imageData = service.GetEDocImageData(eDoc.Id, eDoc.DatabaseNumber, includeUnpublished, ContactPK);
								var fileName = $"[{eDoc.OwnerReadableName}]-[{eDoc.DocumentTypeCode}]-{eDoc.FileName}{Path.GetExtension(imageData.FullFileName)}";
								zip.AddEDoc(imageData.Data, fileName);
							}

							zip.WriteTo(outputStream);
						}
						outputStream.Close();
					},
					new MediaTypeHeaderValue(DataContentTypes.Zip))
				};

				response.Content.Headers.ContentDisposition =
					new ContentDispositionHeaderValue("attachment")
					{
						FileName = outputFileName,
						FileNameStar = outputFileName,
					};

				return ResponseMessage(response);
			}
		}

		[Route("api/edocs/{eDocPK}/{databaseNumber}/contents")]
		[HttpGet]
		public HttpResponseMessage GetEDocImageData(Guid eDocPK, int databaseNumber, bool asAttachment)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				try
				{
					var includeUnpublished = GlowPrincipalHelper.IsStaff(User);
					var eDocImageData = service.GetEDocImageData(eDocPK, databaseNumber, includeUnpublished, ContactPK);
					if (eDocImageData == null)
					{
						return new HttpResponseMessage(HttpStatusCode.NotFound);
					}

					var response = new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StreamContent(new MemoryStream(eDocImageData.Data))
					};

					string mimeType;
					string dispositionType;

					if (!asAttachment
						&& MimeMapping.GetMimeMapping(eDocImageData.FullFileName) is string mappedType
						&& CanOpenTypes.Contains(mappedType))
					{
						mimeType = mappedType;
						dispositionType = "inline";
					}
					else
					{
						mimeType = "application/octet-stream";
						dispositionType = "attachment";
					}

					response.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
					response.Content.Headers.ContentDisposition =
						new ContentDispositionHeaderValue(dispositionType)
						{
							FileName = eDocImageData.FullFileName,
							FileNameStar = eDocImageData.FullFileName,
						};

					return response;
				}
				catch (ExternalStorageException ex)
				{
					if (ex is ExternalStorageObjectNotFoundException)
					{
						return new HttpResponseMessage(HttpStatusCode.NotFound);
					}
					else
					{
						return new HttpResponseMessage(HttpStatusCode.InternalServerError);
					}
				}
				catch (VirusDetectedException)
				{
					return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent("The file has been detected with virus and therefore cannot be saved or opened.") };
				}
			}
		}

		[Route("api/edocs/{recordPK}/{eDocPK:Guid}/contents")]
		[HttpGet]
		public HttpResponseMessage GetEDocImageData(Guid recordPK, Guid eDocPK, bool asAttachment)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				try
				{
					var includeUnpublished = GlowPrincipalHelper.IsStaff(User);
					var eDocsDetails = service.GetEDocDetails(recordPK, includeDeleted: false, includeUnpublished, ContactPK);
					var targetEDocs = eDocsDetails.FirstOrDefault(x => x.Id == eDocPK);
					if (targetEDocs == null)
					{
						return new HttpResponseMessage(HttpStatusCode.NotFound);
					}

					return GetEDocImageData(targetEDocs.Id, targetEDocs.DatabaseNumber, asAttachment);
				}
				catch (ExternalStorageException ex)
				{
					if (ex is ExternalStorageObjectNotFoundException)
					{
						return new HttpResponseMessage(HttpStatusCode.NotFound);
					}
					else
					{
						return new HttpResponseMessage(HttpStatusCode.InternalServerError);
					}
				}
			}
		}

		[ActionName("api/edocs/{recordPK}/{eDocPK}/deliver")]
		[HttpPut]
		public HttpResponseMessage DeliverEDoc(Guid recordPK, Guid eDocPK, DeliveryInstructionsBase deliveryInstructions)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var includeUnpublished = GlowPrincipalHelper.IsStaff(User);
				var eDocsDetails = service.GetEDocDetails(recordPK, includeDeleted: false, includeUnpublished, ContactPK);
				var targetEDocs = eDocsDetails.FirstOrDefault(x => x.Id == eDocPK);

				if (targetEDocs == null)
				{
					return new HttpResponseMessage(HttpStatusCode.NotFound);
				}

				service.DeliverEDoc(eDocPK, targetEDocs.DatabaseNumber, deliveryInstructions, includeUnpublished, ContactPK);

				return new HttpResponseMessage(HttpStatusCode.OK);
			}
		}

		[Route("api/edocs/{recordPK}/{eDocPK}")]
		[HttpDelete]
		public HttpResponseMessage DeleteEDoc(Guid recordPK, Guid eDocPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var includeUnpublished = GlowPrincipalHelper.IsStaff(User);
				var eDocsDetails = service.GetEDocDetails(recordPK, includeDeleted: false, includeUnpublished, ContactPK);
				var targetEDocs = eDocsDetails.FirstOrDefault(x => x.Id == eDocPK);

				if (targetEDocs == null)
				{
					return new HttpResponseMessage(HttpStatusCode.NotFound);
				}

				service.DeleteEDoc(eDocPK, targetEDocs.DatabaseNumber, includeUnpublished, ContactPK);

				return new HttpResponseMessage(HttpStatusCode.OK);
			}
		}

		[Route("api/edocs/docTypes")]
		[HttpGet]
		public IHttpActionResult GetRefDocTypes(string docManagerCode, string languageCode = Core.SharedConstants.Languages.English)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var referenceType = service.GetReferenceType(docManagerCode);
				if (string.IsNullOrEmpty(referenceType))
				{
					return NotFound();
				}

				var includeUnpublished = GlowPrincipalHelper.IsStaff(User);
				var result = service.GetRefDocTypes(referenceType, includeUnpublished, ContactPK).Select(x =>
				{
					return new
					{
						key = x.PK,
						code = x.RT_DocType,
						description = x.RT_DescMultilingual.ToString(languageCode),
						isPublished = x.RT_IsPublished == ZBool.True,
						canEditDescription = DocTypesHelper.IsCustomisableDocType(x.RT_DocType),
						referenceType = x.RT_ReferenceType,
					};
				}).ToList();

				return Json(result);
			}
		}

		IHttpActionResult AddOrUpdateEDoc(string recordPrefix, Guid recordPK, EDocDetail detail, byte[] contents)
		{
			EDocUpdateResult updateResult;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var includeUnpublished = GlowPrincipalHelper.IsStaff(User);
				updateResult = service.AddOrUpdateEDoc(recordPrefix, recordPK, detail, contents, includeUnpublished, ContactPK);
			}

			switch (updateResult.Status)
			{
				case EDocUpdateStatus.BusinessObjectNotFound:
				case EDocUpdateStatus.EDocNotFound:
					return NotFound();

				case EDocUpdateStatus.UnpublishedForContact:
					return BadRequest("File that was uploaded by contact should be published.");

				case EDocUpdateStatus.InvalidDocumentType:
					return BadRequest("Invalid document type.");

				case EDocUpdateStatus.InvalidDocument:
					return BadRequest("Invalid document.");

				case EDocUpdateStatus.BusinessObjectNotSupported:
					return BadRequest("eDoc unsupported for this record type.");

				case EDocUpdateStatus.VirusDetected:
					return BadRequest("Virus detected.");

				case EDocUpdateStatus.Succeeded:
					return Json(updateResult.EDocDetail);

				default:
					return InternalServerError();
			}
		}

		static bool TryGetContent(MultipartMemoryStreamProvider provider, string name, out HttpContent content)
		{
			content = provider.Contents.FirstOrDefault(x => x.Headers.ContentDisposition.Name?.Trim('"') == name);
			return content != null;
		}

		static async Task<EDocsDetailArgs> GetDetailAsync(HttpContent detailContent)
		{
			var detailString = await detailContent.ReadAsStringAsync().ConfigureAwait(false);

			var success = true;
			var detail = JsonConvert.DeserializeObject<EDocsDetailArgs>(
				detailString,
				new JsonSerializerSettings
				{
					Error = (s, e) =>
					{
						success = false;
						e.ErrorContext.Handled = true;
					}
				});

			return success ? detail : null;
		}

		static bool TryGetFileName(HttpContent content, out string filename)
		{
			var contentDisposition = content.Headers.ContentDisposition;
			filename = contentDisposition.FileNameStar?.Trim('"');
			if (string.IsNullOrEmpty(filename))
			{
				filename = contentDisposition.FileName?.Trim('"');
			}

			return !string.IsNullOrEmpty(filename);
		}

		static readonly ImmutableArray<string> CanOpenTypes = ImmutableArray.Create(new[] { "text/plain", "application/pdf" });
	}

	#endregion
}
