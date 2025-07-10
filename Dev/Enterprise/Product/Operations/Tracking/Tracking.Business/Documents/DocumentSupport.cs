using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Helper class to support documents for any business object
	/// </summary>
	public class DocumentSupport
	{
		#region Constructor

		public DocumentSupport(IWebDocumentsSupport parent)
		{
			this.Parent = parent;
		}

		readonly internal IWebDocumentsSupport Parent;

		#endregion

		#region DocFactory

		public void RetrieveDocFactory()
		{
			if (Parent is IDocManagerSupport && ((IDocManagerSupport)Parent).DocManagerInfo != null)
			{
				fDocumentFactory = (DocumentFactory)(((IDocManagerSupport)Parent).DocManagerInfo.MasterFactory);
			}
		}

		public DocumentFactory DocFactory
		{
			get
			{
				if (fDocumentFactory == null)
				{
					fDocumentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				}

				return fDocumentFactory;
			}
		}
		DocumentFactory fDocumentFactory;

		#endregion

		#region StorageMain

		public StorageMain StorageMain
		{
			get
			{
				if (fStorageMain == null && fParentHasStorageMain)
				{
					fStorageMain = DocFactory.GetStorageMainForPK(Parent.DocParentPK);

					if (fStorageMain == null)
					{
						fParentHasStorageMain = false;
					}
				}

				return fStorageMain;
			}
		}

		StorageMain fStorageMain;
		bool fParentHasStorageMain = true;

		#endregion

		#region NewPublishedDocuments

		public void ResetNewPublishedDocuments()
		{
			if (Parent != null)
			{
				if (Parent is BusinessObject)
				{
					((BusinessObject)Parent).UnRegisterEditableChildObject(fNewPublishedDocuments);
				}
			}
			fNewPublishedDocuments = null;
		}

		public StorageDocsCollection NewPublishedDocuments
		{
			get
			{
				if (fNewPublishedDocuments == null)
				{
					fNewPublishedDocuments = new StorageDocsCollection(DocFactory);
					if (Parent != null)
					{
						if (Parent is BusinessObject)
						{
							((BusinessObject)Parent).RegisterEditableChildObject(fNewPublishedDocuments);
						}
					}
				}
				return fNewPublishedDocuments;
			}
		}
		StorageDocsCollection fNewPublishedDocuments;

		#endregion

		#region PublishedEDocsAndFiles

		public BusinessObjectCollection PublishedEDocsAndFiles
		{
			get
			{
				if (fDocumentStore == null)
				{
					var siteUser = WebEnv.AppInstance?.SiteUser as TrackingSiteUser;
					var edocs = StorageMain?.PublishedEDocsAndFiles ?? new StorageDocsCollectionView(new StorageDocsCollection(DocFactory));

					fDocumentStore = siteUser != null ?
						new PermittedStorageDocsCollection(edocs, siteUser) :
						edocs;
				}

				return fDocumentStore;
			}
		}
		BusinessObjectCollection fDocumentStore;

		#endregion

		#region PublishedDocumentsFilter

		protected ZQuery PublishedDocumentsFilter
		{
			get
			{
				if (fPublishedDocumentsFilter == null)
				{
					fPublishedDocumentsFilter = new ZQuery(StorageDocsSchema.SC_IsPublished, ZBool.True);
				}
				return fPublishedDocumentsFilter;
			}
		}
		ZQuery fPublishedDocumentsFilter;

		#endregion

		#region DeleteFileOrDocument

		public bool DeleteFileOrDocument(string docType)
		{
			bool result = false;

			if (StorageMain != null)
			{
				result = StorageMain.DeleteFileOrDocument(docType);
				fAllDocuments = null;

				if (JobRequiredDocuments != null)
				{
					foreach (JobRequiredDocument requiredDocument in JobRequiredDocuments)
					{
						if (requiredDocument.EQ_DocType == docType)
						{
							requiredDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region Save

		public void Save()
		{
			if (StorageMain != null)
			{
				StorageMain.MasterFactory.Save();
				JobRequiredDocuments.Factory.Save();
				fJobRequiredDocuments = null;
			}
		}

		#endregion

		#region AllDocuments

		/// <summary>
		/// Currently only used in ZClientSEV
		/// </summary>
		public DocumentViewCollection AllDocuments
		{
			get
			{
				if (fAllDocuments == null)
				{
					fAllDocuments = new DocumentViewCollection(DocFactory);

					if (StorageMain != null)
					{
						var siteUser = WebEnv.AppInstance?.SiteUser as TrackingSiteUser;

						if (siteUser != null)
						{
							foreach (StorageDocsBase storageDoc in StorageMain.eDocs)
							{
								if (!storageDoc.SC_IsDeleted && storageDoc.SC_IsPublished && siteUser.CanViewDocument(DocFactory, storageDoc.DocType))
								{
									fAllDocuments.Add(new DocumentView(DocFactory) { StorageDoc = storageDoc, ParentPK = Parent.DocParentPK });
								}
							}
						}

						if (JobRequiredDocuments != null)
						{
							foreach (JobRequiredDocument requiredDocument in JobRequiredDocuments)
							{
								bool matchingStorageDocExists = false;
								foreach (DocumentView documentView in fAllDocuments)
								{
									if (documentView.DocType == requiredDocument.EQ_DocType)
									{
										if (requiredDocument.EQ_DocType == Core.Constants.RefDocTypes.MiscellaneousDocument)
										{
											string description = documentView.Description.GetUnresolvedString();
											if (description.Length > AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength)
											{
												description = description.Substring(0, AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength);
											}

											matchingStorageDocExists = (description == requiredDocument.EQ_DocDescriptionMultilingual.GetUnresolvedString());
										}
										else
										{
											matchingStorageDocExists = true;
										}
									}
									if (matchingStorageDocExists)
									{
										documentView.ReqDoc = requiredDocument;
										break;
									}
								}
								if (!matchingStorageDocExists)
								{
									fAllDocuments.Add(new DocumentView(DocFactory) { ReqDoc = requiredDocument });
								}
							}
						}
					}
				}

				return fAllDocuments;
			}
		}
		DocumentViewCollection fAllDocuments;

		JobRequiredDocumentDependentCollection JobRequiredDocuments
		{
			get
			{
				if (fJobRequiredDocuments == null)
				{
					if (StorageMain != null)
					{
						IHaveRequiredDocuments requiredDocumentsParent = StorageMain.DocumentOwner as IHaveRequiredDocuments;

						if (requiredDocumentsParent == null)
						{
							IDocsAndCartageParent docsAndCartageParent = StorageMain.DocumentOwner as IDocsAndCartageParent;

							if (docsAndCartageParent != null)
							{
								requiredDocumentsParent = docsAndCartageParent.RequiredDocumentsProvider;
							}
						}

						if (requiredDocumentsParent != null)
						{
							fJobRequiredDocuments = requiredDocumentsParent.RequiredDocuments;
						}
					}
				}

				return fJobRequiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection fJobRequiredDocuments;

		#endregion

		#region Documents

		public void AddDocumentsForEmailReporting(DataState state, PropertyChangeInfoCollection propertiesForEmailReporting)
		{
			int i = 1;
			foreach (StorageDocsBase document in NewPublishedDocuments)
			{
				var value = GenerateDocumentDetailsForEmailReporting(document);
				var propertyName = MultilingualString.Join("", ResString.GetMultilingualString("3969c07b-20c6-430f-9fab-6e8cb201fd72", "New Attached File"), (NoResString)(NewPublishedDocuments.Count > 1 ? " " + i.ToString() : ""));
				i++;
				propertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		protected MultilingualString GenerateDocumentDetailsForEmailReporting(StorageDocsBase document)
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("51b69c89-82ae-49b2-9f60-928eff5e9d54", "Date: {0}", document.SC_Date),
				ResString.GetMultilingualString("8a86cb23-8d64-43dd-b70b-50e70cdd1d9c", "Description: {0}", document.SC_DescriptionForWeb),
				ResString.GetMultilingualString("03c370f6-6e81-4020-b223-8bb08139cb53", "Type: {0}", document.SC_DescMultilingual));
		}

		/// <summary>
		/// This includes:
		///  - Published StorageDocs
		///  - Related BusinessObject Docs
		///  - RequiredDocuments
		/// </summary>
		public DocumentViewCollection Documents
		{
			get
			{
				if (fDocuments == null)
				{
					fDocuments = new DocumentViewCollection(DocFactory);

					if (StorageMain != null)
					{
						AddStorageDocsToDocuments();
					}

					AddRelatedBODocumentsToDocuments();
					fDocuments.Sort(new SortInfo(DocumentView.Schema.DateReceived, ListSortDirection.Ascending));

					if (StorageMain != null)
					{
						AddRequiredDocuments();
					}
				}

				return fDocuments;
			}
		}

		DocumentViewCollection fDocuments;

		void AddStorageDocsToDocuments()
		{
			// Add storage docs
			foreach (StorageDocsBase storageDoc in PublishedEDocsAndFiles)
			{
				fDocuments.Add(new DocumentView(fDocuments.Factory, ((IBusinessObjectInternals)storageDoc).Row)
				{
					StorageDoc = storageDoc,
					Description = storageDoc.SC_DescriptionForWeb,
					ParentPK = Parent.DocParentPK
				});
			}
		}

		void AddRelatedBODocumentsToDocuments()
		{
			foreach (DocumentView documentView in RelatedBODocuments)
			{
				fDocuments.Add(documentView);
			}
		}

		void AddRequiredDocuments()
		{
			var siteUser = WebEnv.AppInstance?.SiteUser as TrackingSiteUser;
			if (siteUser != null)
			{
				IHaveRequiredDocuments requiredDocumentsParent = Parent as IHaveRequiredDocuments;

				if (requiredDocumentsParent == null)
				{
					IDocsAndCartageParent docsAndCartageParent = Parent as IDocsAndCartageParent;

					if (docsAndCartageParent != null)
					{
						requiredDocumentsParent = docsAndCartageParent.RequiredDocumentsProvider;
					}
				}

				if (requiredDocumentsParent != null)
				{
					foreach (JobRequiredDocument requiredDocument in requiredDocumentsParent.RequiredDocuments)
					{
						if (requiredDocument.DocType != null &&
							requiredDocument.DocType.RT_IsPublished &&
							siteUser.CanViewDocument(DocFactory, requiredDocument.DocType) &&
							!StorageDocsContainsMatchingRequiredDocument(fDocuments, requiredDocument) &&
							!RequiredDocumentHasStorageDoc(requiredDocument))
						{
							DocumentView documentView = new DocumentView(fDocuments.Factory, ((IBusinessObjectInternals)requiredDocument).Row) { DateReceived = requiredDocument.EQ_DateReceived, DocType = requiredDocument.EQ_DocType };

							if (requiredDocument.EQ_DocDescriptionMultilingual.IsEmpty)
							{
								documentView.Description = requiredDocument.Lookups.DocType_List.GetMultilingualDescriptionFromCode(requiredDocument.EQ_DocType);
							}
							else
							{
								documentView.Description = requiredDocument.EQ_DocDescriptionMultilingual;
							}

							fDocuments.Add(documentView);
						}
					}
				}
			}
		}

		bool StorageDocsContainsMatchingRequiredDocument(DocumentViewCollection documentsView, JobRequiredDocument requiredDocument)
		{
			bool result = false;

			foreach (DocumentView documentView in documentsView)
			{
				if (documentView.DocType == requiredDocument.EQ_DocType ||
					(requiredDocument.EQ_DocType == Core.Constants.RefDocTypes.MiscellaneousDocument && !requiredDocument.Lookups.DocType_List.ContainsCode(documentView.DocType)))
				{
					if (requiredDocument.EQ_DocType == Core.Constants.RefDocTypes.MiscellaneousDocument)
					{
						string description = documentView.Description.GetUnresolvedString();
						if (description.Length > AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength)
						{
							description = description.Substring(0, AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength);
						}

						result = (description == requiredDocument.EQ_DocDescriptionMultilingual.GetUnresolvedString());
						break;
					}
					else
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		bool RequiredDocumentHasStorageDoc(JobRequiredDocument requiredDocument)
		{
			return FindMatchingStorageDoc(requiredDocument) != null;
		}

		StorageDocsBase FindMatchingStorageDoc(JobRequiredDocument requiredDocument)
		{
			if (StorageMain != null)
			{
				foreach (StorageDocsBase document in StorageMain.eDocs)
				{
					if (IsMatchingStorageDoc(document, requiredDocument))
					{
						return document;
					}
				}
			}

			if (Parent != null)
			{
				if (Parent.DocRelatedPKs.Count > 0)
				{
					var docStorages = new StorageMainCollection(DocFactory);
					docStorages.LoadWithMoreFiltering(RelatedDocumentBaseFilter);
					foreach (StorageMain storage in docStorages)
					{
						if (storage != null)
						{
							foreach (StorageDocsBase document in storage.eDocs)
							{
								if (IsMatchingStorageDoc(document, requiredDocument))
								{
									return document;
								}
							}
						}
					}
				}
			}
			return null;
		}

		bool IsMatchingStorageDoc(StorageDocsBase document, JobRequiredDocument requiredDocument)
		{
			if (document.SC_DocType == requiredDocument.EQ_DocType ||
				(requiredDocument.EQ_DocType == Core.Constants.RefDocTypes.MiscellaneousDocument && !requiredDocument.Lookups.DocType_List.ContainsCode(document.SC_DocType)))
			{
				if (requiredDocument.EQ_DocType == Core.Constants.RefDocTypes.MiscellaneousDocument)
				{
					var description = document.SC_DescMultilingual.GetUnresolvedString();
					if (description.Length > AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength)
					{
						description = description.Substring(0, AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength);
					}
					return description == requiredDocument.EQ_DocDescriptionMultilingual.GetUnresolvedString();
				}
				return true;
			}
			return false;
		}

		#endregion

		#region RelatedBODocuments

		internal DocumentViewCollection RelatedBODocuments
		{
			get
			{
				if (fRelatedBODocuments == null)
				{
					fRelatedBODocuments = new DocumentViewCollection(DocFactory);

					if (Parent.DocRelatedPKs.Count > 0)
					{
						var docStorage = new StorageMainCollection(DocFactory);
						docStorage.LoadWithMoreFiltering(RelatedDocumentBaseFilter);

						var documentsOfRelatedBizos = docStorage
									.WhereNotNull()
									.SelectMany(storage => ((StorageMain)storage).PublishedEDocsAndFiles)
									.Cast<StorageDocsBase>();

						var siteUser = WebEnv.AppInstance?.SiteUser as TrackingSiteUser;
						if (siteUser != null)
						{
							documentsOfRelatedBizos = documentsOfRelatedBizos.Where(doc => siteUser.CanViewDocument(DocFactory, doc.DocType));
						}

						foreach (var storageDoc in documentsOfRelatedBizos)
						{
							if (DocumentTypeShouldBeShown(storageDoc))
							{
								fRelatedBODocuments.Add(new DocumentView(DocFactory, ((IBusinessObjectInternals)storageDoc).Row)
								{
									StorageDoc = storageDoc,
									Description = storageDoc.SC_DescriptionForWeb,
									ParentPK = storageDoc.ParentMain.SM_ParentFK
								});
							}
						}
					}

					fRelatedBODocuments.Sort(new SortInfo(DocumentView.Schema.DateReceived, ListSortDirection.Ascending));
				}

				return fRelatedBODocuments;
			}
		}

		bool DocumentTypeShouldBeShown(StorageDocsBase storageDoc)
		{
			return storageDoc.ParentMain.SM_Type != Core.Constants.DocManagerCodes.DomesticTransportBooking || storageDoc.SC_DocType == "POD";
		}

		DocumentViewCollection fRelatedBODocuments;

		protected ZQuery RelatedDocumentBaseFilter
		{
			get
			{
				if (fRelatedDocumentBaseFilter == null)
				{
					fRelatedDocumentBaseFilter = new ZQuery(StorageMainSchema.SM_ParentFK, Parent.DocRelatedPKs);
				}
				return fRelatedDocumentBaseFilter;
			}
		}

		ZQuery fRelatedDocumentBaseFilter;

		#endregion
	}
}
