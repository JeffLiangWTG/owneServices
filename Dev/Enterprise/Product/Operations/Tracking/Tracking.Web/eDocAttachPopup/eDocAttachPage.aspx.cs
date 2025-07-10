using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI.HtmlControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class eDocAttachPage : ZFileUploadDialog
	{
		#region Overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (dangerousFileMessageHidden != null)
			{
				dangerousFileMessageHidden.Value = Res.GetString("38C77942-63E6-43AB-BB84-822B966BD5EE", "The following file was not added because it is potentially dangerous file type.");
			}
			if (zeroSizeFileMessageHidden != null)
			{
				zeroSizeFileMessageHidden.Value = Res.GetString("0DF2C73C-E96A-4EEF-97F9-D540906166ED", "The selected file was not added because its size is 0 byte.");
			}
			if (maximumFileSizeHidden != null)
			{
				maximumFileSizeHidden.Value = MaximumFileSize.ToString();
			}
			if (maximumFileSizeMessageHidden != null)
			{
				maximumFileSizeMessageHidden.Value = Res.GetString("28772391-D336-4D29-B56E-70F06652CCBA", "The selected file exceeds the {0}MB maximum size allowed for Documents.", MaximumFileSizeMb);
			}

			var hasDocTypes = HasDocTypes;
			UnauthorisedDiv.Visible = !hasDocTypes;
			AuthorisedContent.Visible = hasDocTypes;
			UnauthorisedLabel.Text = !hasDocTypes ? Res.GetString("7f5b1adf-4d1a-4a00-906b-7925005cfbf4", "You do not have access to any Document Types. Please contact your system administrator to request access rights.") : string.Empty;
		}

		RefDocTypeCollection docTypes;
		RefDocTypeCollection DocTypes => docTypes ?? (docTypes = DocUploadManager?.DocumentUploadHelper.DocTypes);

		bool HasDocTypes => DocTypes?.Count > 0;

		protected override bool DisableOKButton => !HasDocTypes;

		public override void ProcessRequest(System.Web.HttpContext context)
		{
			try
			{
				base.ProcessRequest(context);
			}
			catch (IOException)
			{
				AppInstance.TryToReloadPage();
			}
		}

		protected override string GetOKButtonCaption()
		{
			return Res.GetString("243839df-c4ca-4598-8150-6ae16e1cc9c5", "Upload & Attach");
		}

		protected override string GetInputFileLabelText()
		{
			return Res.GetString("90f7ad31-9155-4041-a6c9-b9e172def253", "Document File:");
		}

		protected override bool ProceedWithUpload()
		{
			return DocUploadManager != null;
		}

		readonly int MaximumFileSizeMb = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value;

		const int BytesInMb = 1048576;

		int MaximumFileSize => MaximumFileSizeMb * BytesInMb;

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		[SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		protected override void ProcessUploadedFile(byte[] contents, string fileName)
		{
			if (DocUploadManager != null && contents?.Length <= MaximumFileSize)
			{
				var uploadHelper = DocUploadManager.DocumentUploadHelper;
				var managerInfo = DocUploadManager.DocManagerInfo;
				if (uploadHelper != null && managerInfo != null)
				{
					var tempFileName = ZGuid.NewZGuid().ToString();
					var docType = uploadHelper.DocType;
					var addedFile = (StorageFile)managerInfo.AddFileOrDocument(contents, tempFileName, docType);
					addedFile.SC_DocType_List.Clear();
					addedFile.SC_DocType_List.AddRange(DocTypes);
					addedFile.SetNewFileName(fileName);
					addedFile.SC_IsPublished = true;
					addedFile.SC_Desc = DocTypes.FirstOrDefault(r => r.RT_DocType == docType)?.RT_Desc ?? ZString.Empty;
					if (addedFile.SC_Desc.IsEmpty)
					{
						ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Document type {0} is not in the list of available document types. (Available types: {1})", docType, string.Join(",", DocTypes.Select(r => r.RT_DocType))));
					}
					DocUploadManager.DocumentHelper.NewPublishedDocuments.Add(addedFile);

					var parent = managerInfo.BusinessEntity as EnterpriseBusinessObject ?? DataSource as EnterpriseBusinessObject;

					var documentFactory = managerInfo.MasterFactory as DocumentFactory;
					if (documentFactory != null)
					{
						var x = documentFactory.FactoryForEverythingExceptEDocs; // Pre-initialize child factory on document factory before saving
					}

					try
					{
						managerInfo.Save();

						managerInfo.AddLogsForNewDocument(parent, addedFile);
						SaveDataSourceFactory();
					}
					catch (ZCannotSaveException)
					{
						AppInstance.TryToReloadPage();
						return;
					}

					DocUploadManager.ResetDocumentHelper();
					DocUploadManager.DocumentHelper.ResetNewPublishedDocuments();
					var bizObj = DocUploadManager as IBizOChangesEmailNotification;
					if (bizObj != null)
					{
						new BusinessObjectChangesEmailNotifier(bizObj);
					}
					SaveDataSource(DataSourceIndexer, DataSource);
				}
			}
		}

		protected override HtmlGenericControl UploadFileElementsPanel
		{
			get { return FileUploadDetails; }
		}

		#endregion

		#region Implementation

		protected internal IWebDocumentsWithUploadSupport DocUploadManager
		{
			get
			{
				if (DataSource != null)
				{
					return DataSource as IWebDocumentsWithUploadSupport;
				}
				return null;
			}
		}

		#endregion

		protected override string GetPageName()
		{
			return WebTracker.Pages.eDocAttach;
		}

		#region test
#if DEBUG

		protected internal void ProcessUploadedFileForTest(byte[] bytes, string v)
		{
			ProcessUploadedFile(bytes, v);
		}

		protected internal void CreateChildControlsForTest()
		{
			CreateChildControls();
		}

		protected internal bool DisableOKButtonInternal => DisableOKButton;

		protected internal System.Web.UI.WebControls.Button OKButtonInternal => OKButton;

#endif
		#endregion
	}
}
