using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.PdfiumWrapper;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.Web;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Web
{
	public class DocumentRequestHandler : DocumentRequestHandler<DocumentRequestHelper>
	{
	}

	public abstract class DocumentRequestHandler<T> : DataRequestHandler<T>
		where T : DocumentRequestHelper, new()
	{
		protected override BusinessObject[] GetNewBusinessObjects()
		{
			var result = new List<BusinessObject>();
			if (PKs.Length > 1 && PKs[0].IsValid && PKs[1].IsValid)
			{
				var document = Factory.Load<DocumentCommand>(PKs[1]);
				if (document != null)
				{
					document.Parent = Helper != null ? Helper.GetDocumentSupportable() : HttpContext.Current.Session[PKs[0].ToString()] as IDocumentSupportable;
					result.Add(document);
				}
			}

			return result.ToArray();
		}

		DocumentsMenuHelper Helper
		{
			get { return helper ?? (helper = GetHelperForThisThread(PKs[0])); }
		}
		DocumentsMenuHelper helper;

		protected virtual DocumentsMenuHelper GetHelperForThisThread(ZGuid pk)
		{
			if (HttpContext.Current.Request[TrackingConstants.QueryStringKeys.Helper] != null)
			{
				var typeName = new QueryParamsEncoder().Decrypt(HttpContext.Current.Request[TrackingConstants.QueryStringKeys.Helper]);
				return (DocumentsMenuHelper)Activator.CreateInstance(Type.GetType(typeName), new object[] { pk });
			}

			return null;
		}

		DocumentCommand Document
		{
			get { return BusinessObjects.Length > 0 ? BusinessObjects[0] as DocumentCommand : null; }
		}

		public override string ContentType
		{
			get
			{
				var contentType = HttpContext.Current.Request[TrackingConstants.QueryStringKeys.ContentType];

				if (string.IsNullOrEmpty(contentType))
				{
					return DataContentTypes.Pdf;
				}

				if (SupportedContentTypes.Contains(contentType))
				{
					return contentType;
				}

				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "\"{0}\" content type is not supported", contentType));
			}
		}

		readonly internal List<string> SupportedContentTypes = new List<string> { DataContentTypes.Pdf, DataContentTypes.Excel };

		public override string FileName
		{
			get
			{
				var fileName = string.Empty;

				if (BusinessObjects.Length > 0)
				{
					switch (ContentType)
					{
						case DataContentTypes.Pdf:
							fileName = Document.SU_MenuName + ".pdf";
							break;

						case DataContentTypes.Excel:
							fileName = Document.SU_MenuName + ".xls";
							break;
					}
				}

				return fileName;
			}
		}

		GlbBranch LoginBranchForWeb
		{
			get
			{
				var branch = CurrentLoggedInOrg != null ? GlbBranch.Find(CurrentLoggedInOrg.CompanyDataCollection) : null;
				return branch ?? GlbBranch.CurrentBranch;
			}
		}

		public override ZBlob GetBinaryData()
		{
			byte[] data = null;
			if (Document != null && Document.Parent != null)
			{
				lock (Document)
				{
					try
					{
#if DEBUG
						StopStopwatchForTesting();
#endif
						using (var set = GetDocumentPrintSet(Document))
						{
							if (!set.UseStreamMode)
							{
								data = GetBinaryDataForNonStreamingMode(set);
							}
							else
							{
								data = GetBinaryDataForStreamingMode(set);
							}
						}
					}
					catch (DocumentEngineException ex)
					{
						throw new InvalidQueryStringException(ex.Message);
					}
				}
			}

			return data;
		}

#if DEBUG
		internal
#endif
		DocumentPrintSet GetDocumentPrintSet(DocumentCommand command)
		{
			var iSupportCustomizedDocumentPrintSet = command.ParentDocumentSupporter as ISupportCustomizedDocumentPrintSet;
			if (iSupportCustomizedDocumentPrintSet != null && iSupportCustomizedDocumentPrintSet.ShouldCustomizedDocumentPrintSet(command))
			{
				return (DocumentPrintSet)iSupportCustomizedDocumentPrintSet.GetCustomizedDocumentPrintSet(command);
			}
			else
			{
				return new DocumentPrintSet(Document, new DocumentEngine.RuntimeOptions.UserControlProviderList(), null);
			}
		}

		byte[] GetBinaryDataForNonStreamingMode(DocumentPrintSet set)
		{
			byte[] data = null;
			if (set.Count > 0)
			{
				DocumentPack pack = set[0];

				if (!RequiresWebUser || CurrentLoggedInOrg != null)
				{
					pack.Organisation = CurrentLoggedInOrg;
					if (Helper != null)
					{
						Helper.SetupDocumentPack(pack);
					}

					using (new WebLoginBranch(LoginBranchForWeb))
					{
						data = GetDocument(pack, CurrentLoggedInUser);
					}

					if (CreateLog)
					{
						new EventLogHelper().CreateLogForDocumentPrinted(WebEnv.AppInstance.SiteUser, Document.SU_MenuName);
					}
				}
			}
			return data;
		}

		#region Streaming mode

		byte[] GetBinaryDataForStreamingMode(DocumentPrintSet set)
		{
			try
			{
				byte[] data = null;
				switch (ContentType)
				{
					case DataContentTypes.Pdf:
						data = GetBinaryDataForStreamingModePDF(set);
						break;
					case DataContentTypes.Excel:
						data = GetBinaryDataForStreamingModeExcel(set);
						break;
				}

				if (CreateLog)
				{
					new EventLogHelper().CreateLogForDocumentPrinted(WebEnv.AppInstance.SiteUser, Document.SU_MenuName);
				}

				return data;
			}
			catch (Exception ex)
			{
				string message = (NoResString)"Error occured when generating HVLV document in streaming mode.";
				ErrorReporter.ReportOnce("HVLV_Web_Exception", message, ex);
				throw;
			}
		}

		byte[] GetBinaryDataForStreamingModePDF(DocumentPrintSet set)
		{
			if (set.Count <= 0)
			{
				return null;
			}

			using (new WebLoginBranch(LoginBranchForWeb))
			{
				var createPdfsFunc = GetThreadSafeDocumentPackCreator(CurrentLoggedInOrg, CurrentLoggedInUser, Document, RequiresWebUser, () => GetHelperForThisThread(PKs[0]));

				var pdfsToBeMerged = set.GetDocumentPacks()
					.AsParallel()
					.Select((pack, index) => Tuple.Create(index, createPdfsFunc(pack)))
					.OrderBy(tpl => tpl.Item1)
					.Select(tpl => tpl.Item2);

				return MergeDocuments(pdfsToBeMerged);
			}
		}

		static Func<DocumentPack, ICollection<byte[]>> GetThreadSafeDocumentPackCreator(OrgHeader packOrg, OrgContact packContact, DocumentCommand document, bool requiresWebUser, Func<DocumentsMenuHelper> getHelper)
		{
			var packOrgPk = packOrg.PK;
			var packContactPk = packContact.PK;
			var isLocalDocument = document.IsLocalDocument;
			var lineSpacing = document.SU_FlexCelLineSpacing;
			var userContextUnsafe = Env.CurrentUserContext;
			var httpContext = HttpContext.Current;

			return pack =>
			{
				HttpContext.Current = httpContext; //This will make it set up the environment in the new thread

				using (Db.DisposableActionForDbConnection())
				using (Env.SetTemporaryUserContext(userContextUnsafe.ThreadSafeClone()))
				{
					return CreatePackToBeMerged(pack, packOrgPk, packContactPk, requiresWebUser, getHelper, isLocalDocument, lineSpacing);
				}
			};
		}

		static ICollection<byte[]> CreatePackToBeMerged(DocumentPack pack, ZGuid packOrgPk, ZGuid packContactPk, bool requiresWebUser, Func<DocumentsMenuHelper> getHelper, bool isLocalDocument, decimal lineSpacing)
		{
			using (var method = new WebDeliveryMethod())
			{
				if (!packOrgPk.IsEmpty || requiresWebUser)
				{
					var factory = new BusinessObjectFactory();
					pack.Organisation = factory.Load<OrgHeader>(packOrgPk);
					getHelper()?.SetupDocumentPack(pack);

					var utility = new DocumentUtility(factory, requiresWebUser);
					var contact = factory.Load<OrgContact>(packContactPk);
					utility.RenderAndAddDocumentPackToDeliveryMethod(pack, contact, method);
				}

				pack.Dispose();

				var pdfDocumentsInThisPack = new List<byte[]>();
				foreach (var fileInfo in method.Infos)
				{
					if (fileInfo?.FileContents?.Length > 0)
					{
						var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
						var pdfData = DocumentConverter.ConvertFromExcel(fileInfo.FileContents.CopyToByteArray(), string.Empty, OutputFormatType.PDFAcrobat5, deliveryInfo.Watermark, ColourDepth.BlackAndWhite, isLocalDocument, lineSpacing);

						pdfDocumentsInThisPack.Add(pdfData);
						fileInfo.ReleaseFileContentsWhenSafe();
					}
				}

				return pdfDocumentsInThisPack;
			}
		}

		byte[] MergeDocuments(IEnumerable<ICollection<byte[]>> packsToBeMerged)
		{
			using (var outputDocument = new PdfDocument())
			{
				foreach (var packofDocuments in packsToBeMerged)
				{
					foreach (var pdfData in packofDocuments)
					{
						using (var memoryStream = new MemoryStream(pdfData))
						using (var pdfDocument = new PdfDocument(memoryStream))
						{
							outputDocument.InsertPages(pdfDocument, outputDocument.PageCount);
						}
					}
				}

				using (var resultStream = new MemoryStream())
				{
					outputDocument.Save(resultStream);
					return resultStream.ToArray();
				}
			}
		}

		byte[] GetBinaryDataForStreamingModeExcel(DocumentPrintSet set)
		{
			byte[] result = null;
			if (set.Count > 0)
			{
				var utility = new DocumentUtility(Factory, RequiresWebUser);
				using (new WebLoginBranch(LoginBranchForWeb))
				{
					using (WebDeliveryMethod method = new WebDeliveryMethod())
					{
						foreach (var pack in set.GetDocumentPacks())
						{
							if (!RequiresWebUser || CurrentLoggedInOrg != null)
							{
								pack.Organisation = CurrentLoggedInOrg;
								if (Helper != null)
								{
									Helper.SetupDocumentPack(pack);
								}

								utility.RenderAndAddDocumentPackToDeliveryMethod(pack, CurrentLoggedInUser, method);
							}
							pack.Dispose();
						}
						using (MemoryStream stream = method.MergeFilesIntoSingleExcelFile())
						{
							result = stream.ToArray();
						}
					}
				}
			}
			return result;
		}

		#endregion

		protected virtual bool CreateLog
		{
			get { return true; }
		}

		protected virtual bool RequiresWebUser
		{
			get { return true; }
		}

		protected virtual OrgContact CurrentLoggedInUser
		{
			get
			{
				if (AppInstance != null && AppInstance.SiteUser != null)
				{
					return (OrgContact)AppInstance.SiteUser.LoggedInUser;
				}

				return null;
			}
		}

		protected virtual OrgHeader CurrentLoggedInOrg
		{
			get { return CurrentLoggedInUser != null ? CurrentLoggedInUser.Header : null; }
		}

		protected virtual byte[] GetDocument(DocumentPack pack, OrgContact contact)
		{
			return new DocumentUtility(Factory, RequiresWebUser).GetDocument(pack, contact, ContentType);
		}

		public override string NoDataErrorMessage
		{
			get { return Res.GetString("3fa18de3-0af4-44cf-8a9c-be9b5ce2db3b", "The Document was not found in the current web session."); }
		}
	}
}
