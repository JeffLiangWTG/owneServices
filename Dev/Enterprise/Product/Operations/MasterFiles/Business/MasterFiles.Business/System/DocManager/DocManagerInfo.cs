using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DocManagerInfo : IDocManagerInfoCore
	{
		public static DocManagerInfo New(BusinessObject parent, string docManagerCode)
		{
			return new DocManagerInfo(parent, docManagerCode);
		}

		public DocManagerInfo(BusinessObject parent, string docManagerCode)
		{
			if (parent?.Factory == null)
			{
				ErrorReporter.ReportOnce($"A null {(parent == null ? (NoResString)"parent" : (NoResString)"factory")} was passed into {GetType().Name}");
			}

			fDocManagerCode = docManagerCode;
			BusinessEntity = parent;
		}

		#region eDocs Providers

		public IEDocsProvider[] GetEDocsProviders()
		{
			return GetEDocsProvidersCore();
		}

		protected virtual IEDocsProvider[] GetEDocsProvidersCore()
		{
			return Array.Empty<IEDocsProvider>();
		}

		#endregion

		#region StorageMain

#if DEBUG
		internal
#endif
 protected IStorageMain StorageMain
		{
			get
			{
				if (fStorageMain == null)
				{
					fStorageMain = MasterFactory.RetrieveExistingOrCreateStorageMain(BusinessEntity, DocManagerCode);
				}
				return fStorageMain;
			}
		}

		IStorageMain fStorageMain;

		public void ResetStorageMainIfNeeded()
		{
			if (fStorageMain is BusinessObject storageMain && storageMain.IsDeleted)
			{
				fStorageMain = null;
			}
		}

		#endregion

		#region All eDocs

		/// <summary>
		/// This method creates new StorageMain if there isn't one for adding eDocs, but it will cause extra DB hits.
		/// Use this method for adding new eDocs, otherwise consider using EDocsView.
		/// </summary>
		public IStorageDocsBaseCollection AllEDocs => GetAllEDocs();

		protected virtual IStorageDocsBaseCollection GetAllEDocs()
		{
			return StorageMain.AllEDocs;
		}

		#endregion

		#region EDocs View

		/// <summary>
		/// This method doesn't create new StorageMain, but only return all existing eDocs.
		/// This method can be used for accessing existing eDocs, but not for adding new eDocs.
		/// </summary>
		public IStorageDocsBaseCollection EDocsView => GetEDocsView() ?? new EmptyStorageDocsBaseCollection();

		protected virtual IStorageDocsBaseCollection GetEDocsView()
		{
			return ExistingStorageMain?.EDocsView;
		}

		internal class EmptyStorageDocsBaseCollection : List<IeDoc>, IStorageDocsBaseCollection
		{
			int IStorageDocsBaseCollection.Count => 0;
			void IStorageDocsBaseCollection.Add(IeDoc edoc)
			{
				throw new NotSupportedException("EmptyStorageDocsBaseCollection does not allow to add new item.");
			}
			bool IStorageDocsBaseCollection.Contains(IeDoc element) => false;
			IeDoc IStorageDocsBaseCollection.GetFromUniqueKey(Guid uniqueKey) => null;
			IeDoc IStorageDocsBaseCollection.GetMostRecentEDoc(string docType) => null;
			void IStorageDocsBaseCollection.Remove(IeDoc elementToRemove) { }
			bool IStorageDocsBaseCollection.ContainsDocType(ZString docType) => false;
		}

		#endregion

		#region Existing StorageMain

		IStorageMain ExistingStorageMain
		{
			get
			{
				if (existingStorageMain == null || (existingStorageMain is BusinessObject storageMain && storageMain.IsDeleted))
				{
					existingStorageMain = MasterFactory.GetStorageMainForPK(BusinessEntity.PK);
				}
				return existingStorageMain;
			}
		}
		IStorageMain existingStorageMain;

		#endregion

		#region Documents

		/// <summary>
		/// This method doesn't create new StorageMain, but only get documents from existing StorageMain.
		/// Use this method for accessing existing documents, but not for adding new documents.
		/// </summary>
		public IStorageDocsBaseCollection ExistingDocuments => ExistingStorageMain?.Documents ?? new EmptyStorageDocsBaseCollection();

		/// <summary>
		/// This method creates new StorageMain if there isn't one for adding documents, but it will cause extra DB hits.
		/// Use this method for adding new documents, otherwise consider using ExistingDocuments.
		/// </summary>
		public IStorageDocsBaseCollection Documents => GetDocuments();

		protected virtual IStorageDocsBaseCollection GetDocuments()
		{
			return StorageMain.Documents;
		}

		#endregion

		#region Files

		/// <summary>
		/// This method doesn't create new StorageMain, but only get files from existing StorageMain.
		/// Use this method for accessing existing files, but not for adding new files.
		/// </summary>
		public IStorageDocsBaseCollection ExistingFiles => ExistingStorageMain?.Files ?? new EmptyStorageDocsBaseCollection();

		/// <summary>
		/// This method creates new StorageMain if there isn't one for adding files, but it will cause extra DB hits.
		/// Use this method for adding new files, otherwise consider using ExistingFiles.
		/// </summary>
		public IStorageDocsBaseCollection Files => GetFiles();

		protected virtual IStorageDocsBaseCollection GetFiles()
		{
			return StorageMain.Files;
		}

		#endregion

		#region DocManagerCode

		/// <summary>
		/// This DocManagerCode matches the 3-letter Reference Type code used within Document Scanning
		/// </summary>

		public ZString DocManagerCode
		{
			get { return fDocManagerCode; }
		}

		public bool ShouldRecordDocument(IStmMenuItem menuItem)
		{
			return ShouldRecordDocumentCore(menuItem);
		}

		protected virtual bool ShouldRecordDocumentCore(IStmMenuItem menuItem)
		{
			return true;
		}

		readonly ZString fDocManagerCode;

		#endregion

		#region BusinessEntity

		public BusinessObject BusinessEntity { get; }

		#endregion

		#region MasterFactory / Save()

		public bool UseBusinessEntityFactoryAsInternal { get; set; }

		public IDocumentFactory MasterFactory
		{
			get
			{
				if (factory == null)
				{
					var useBizoFactory = BusinessEntity != null && (UseBusinessEntityFactoryAsInternal || ServiceContainerSuspenderHelper.IsDocManagerInfoNewFactorySuspended(BusinessEntity.Factory));
					factory = ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(useBizoFactory ? BusinessEntity.Factory : null);
				}

				return factory;
			}
			set
			{
				factory = value;
			}
		}

		IDocumentFactory factory;

		bool isSaving;
		public void Save()
		{
			if (!isSaving)
			{
				try
				{
					isSaving = true;

					ZExceptionReporting.ProcessWithSaveExceptionHandling(
						MasterFactory.Save,
						() =>
						{
							var bizoStorageMain = fStorageMain as BusinessObject;
							if (bizoStorageMain != null && bizoStorageMain.IsDeleted)
							{
								fStorageMain = null;
							}
						},
						true, attempts: 2);
				}
				finally
				{
					isSaving = false;
				}
			}
		}

		public void SetupEDocsFactoryToBeSavedWithMainFactory(bool alwaysRemoveMasterFactoryFromChildrenOnFactorySaved)
		{
			var docFactory = (BusinessObjectFactory)this.MasterFactory;
			if (docFactory.ChildFactories.Contains(BusinessEntity.Factory))
			{
				docFactory.ChildFactories.Remove(BusinessEntity.Factory);
			}
			BusinessEntity.Factory.ChildFactories.Add(docFactory);
			BusinessEntity.Factory.Saved += RemoveMasterFactoryFromChildrenOnFactorySaved;

			void RemoveMasterFactoryFromChildrenOnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully || alwaysRemoveMasterFactoryFromChildrenOnFactorySaved)
				{
					factory.Saved -= RemoveMasterFactoryFromChildrenOnFactorySaved;
					factory.ChildFactories.Remove(docFactory);
				}
			}
		}

		#endregion

		#region RelatedObjects

		/// <summary>
		/// A collection of any child objects which also implement IDocManagerSupport. You should ALWAYS cache
		/// this property before using it in a foreach loop or other such scenario.
		/// </summary>
		public BusinessObject[] RelatedObjects
		{
			get
			{
				var result = new List<BusinessObject>();
				var relatedBusinessObjects = GetRelatedObjects();
				if (relatedBusinessObjects != null)
				{
					result.AddRange(relatedBusinessObjects);
				}
				var invoicingJob = GetInvoicingJobHeader();
				if (invoicingJob != null)
				{
					result.Add(invoicingJob);
				}

				if (result.Any(x => x == null))
				{
					ErrorReporter.ReportOnce($"GetRelatedObjects returns an Array with null Elements in {GetType()}. Reassign this to the specific team if needed. The responsible team should check if the TestClass inherited DocManagerInfoTestCase and why the existing unit test didn't test it out.");
				}

#if NET
				return System.Linq.Enumerable.DistinctBy(result.Where(x => x != null), x => x.PK).ToArray();
#else
				return result.Where(x => x != null).DistinctBy(x => x.PK).ToArray();
#endif
			}
		}

		protected virtual BusinessObject[] GetRelatedObjects()
		{
			return Array.Empty<BusinessObject>();
		}

		public bool RelatedObjectTypesAlwaysShow(BusinessObject relatedObject) => RelatedObjectTypesAlwaysShowCore(relatedObject);

		protected virtual bool RelatedObjectTypesAlwaysShowCore(BusinessObject relatedObject) => false;

		protected virtual JobHeader GetInvoicingJobHeader()
		{
			if (BusinessEntity is IJobInvoicingPlugIn)
			{
				return new JobHeader.Loader((IJobHeaderParent)BusinessEntity).Load();
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region HasUnreadRelatedDocuments

		public bool HasUnreadRelatedDocuments
		{
			get
			{
				#region SuppressResourceStringsCheckRegion

				bool result = false;
				try
				{
					StringBuilder sql = new StringBuilder();
					if (ExistsForceToReadDocumentTypes && DocumentDBNumber != -1)
					{
						string databaseName = MasterFactory.GetDatabaseName(DocumentDBNumber);

						sql.AppendFormat("IF EXISTS (SELECT TOP 1 NULL FROM sys.databases WHERE name = @databaseName)");
						sql.AppendFormat("  SELECT TOP 1 NULL");
						sql.AppendFormat("  FROM dbo.StorageMain");
						sql.AppendFormat("  INNER JOIN {0}..StorageDocs ON SC_SM=SM_PK", databaseName);
						sql.AppendFormat("  LEFT OUTER JOIN dbo.RefDocType DocType ON DocType.RT_DocType=SC_DocType AND DocType.RT_ReferenceType=@referenceType");
						sql.AppendFormat("  LEFT OUTER JOIN dbo.RefDocType AllDocType ON AllDocType.RT_DocType=SC_DocType AND AllDocType.RT_ReferenceType='ALL'");
						sql.AppendFormat($"  LEFT OUTER JOIN dbo.StmALog ReadRelatedDocumentsLog ON SM_ParentFK=ReadRelatedDocumentsLog.SL_Parent AND ReadRelatedDocumentsLog.SL_SE_NKEvent='{Events.RelatedEDocsRead.Code}' AND ReadRelatedDocumentsLog.SL_GS_NKUser = @user AND ReadRelatedDocumentsLog.SL_IsCancelled = 'N'");
						sql.AppendFormat("  WHERE SM_ParentFK=@parent AND (DocType.RT_ForceUserToRead = 1 OR (DocType.RT_PK IS NULL AND AllDocType.RT_ForceUserToRead = 1)) AND SC_SystemCreateUser != @user AND (ReadRelatedDocumentsLog.SL_PostedTimeUtc IS NULL OR SC_Date > ReadRelatedDocumentsLog.SL_PostedTimeUtc)");
						if (!Env.Security.ViewAllCompanySpecificDocuments.IsAllowed)
						{
							sql.AppendFormat("  AND (SC_GC_Company is null or SC_GC_Company = @company)");
						}
						if (!Env.Security.ViewAllBranchSpecificDocuments.IsAllowed)
						{
							sql.AppendFormat("  AND (SC_GB_Branch is null or SC_GB_Branch = @branch)");
						}
						if (!Env.Security.ViewAllDepartmentSpecificDocuments.IsAllowed)
						{
							sql.AppendFormat("  AND (SC_GE_Department is null or SC_GE_Department = @department)");
						}

						using (var cmd = Db.Connection.Command(sql.ToString()))
						{
							cmd.AddParameter("databaseName", SqlDbType.NVarChar, 128, databaseName);
							cmd.AddParameterBasedOnDbColumn("referenceType", DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(DocManagerCode), RefDocTypeSchema.RT_ReferenceType);
							cmd.AddParameterBasedOnDbColumn("user", GlbStaff.CurrentUser.GS_Code.ToString(), StmALogSchema.SL_GS_NKUser);
							cmd.AddParameter("parent", SqlDbType.UniqueIdentifier, BusinessEntity.PK.ToGuid());
							cmd.AddParameter("company", SqlDbType.UniqueIdentifier, Env.CurrentCompany.PK);
							cmd.AddParameter("branch", SqlDbType.UniqueIdentifier, Env.CurrentBranch.PK);
							cmd.AddParameter("department", SqlDbType.UniqueIdentifier, Env.CurrentDepartment.PK);
							using (var reader = cmd.ExecuteReader())
							{
								return reader.Read();
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("DocManagerInfo.HasUnreadRelatedDocuments", ex.Message, ex);
				}
				return result;

				#endregion
			}
		}

		bool ExistsForceToReadDocumentTypes
		{
			get
			{
				IList<ZString> forceToReadReferenceTypes = RefDocType.ForceToReadReferenceTypes;
				string referenceType = DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(DocManagerCode);

				return
					forceToReadReferenceTypes.Contains(Core.Constants.ReferenceTypes.All) ||
					forceToReadReferenceTypes.Contains(referenceType);
			}
		}

		int DocumentDBNumber
		{
			get
			{
				int result = -1;
				var bizo = fStorageMain as BusinessObject;
				if (bizo != null && !bizo.IsDeleted)
				{
					result = (ZInt)bizo[StorageMainSchema.SM_DB];
				}
				else
				{
					// every form needs to do this when they save, hit the db directly for performance
					using (var cmd = Db.Connection.Command("SELECT SM_DB FROM dbo.StorageMain WHERE SM_ParentFK=@PK"))
					{
						cmd.AddParameterBasedOnDbColumn("@PK", BusinessEntity.PK.ToGuid(), StorageMainSchema.SM_ParentFK);
						object dBNumber = cmd.ExecuteScalar();
						result = dBNumber == null ? -1 : (int)dBNumber;
					}
				}
				return result;
			}
		}

		#endregion

		#region AddFileOrDocument

		/// <summary>
		/// This method returns a business object IN A DIFFERENT FACTORY. You must call DocManagerInfo.Save() to save your new Document in the database.
		/// </summary>
		public IeDoc AddFileOrDocument(
			string filename,
			string documentType,
			bool overwriteExistingFileIfNotImageFile = false,
			string description = "",
			string filenameOnly = "")
		{
			var contents = File.ReadAllBytes(filename);
			if (string.IsNullOrEmpty(filenameOnly))
			{
				filenameOnly = Path.GetFileName(filename);
			}
			return AddFileOrDocument(
				contents,
				filenameOnly,
				documentType,
				overwriteExistingFileIfNotImageFile: overwriteExistingFileIfNotImageFile,
				description: description);
		}

		/// <summary>
		/// This method returns a business object IN A DIFFERENT FACTORY. You must call DocManagerInfo.Save() to save your new Document in the database.
		/// </summary>
		public IeDoc AddFileOrDocument(
			SubStreamableStream contents,
			string filenameOnly,
			string documentType,
			bool overwriteExistingFileIfNotImageFile = false,
			Guid visibleCompanyPK = default,
			Guid visibleBranchPK = default,
			Guid visibleDepartmentPK = default,
			string description = "",
			string documentSource = "")
		{
			return StorageMain.AddFileOrDocument(
				contents,
				filenameOnly,
				documentType,
				overwriteExistingFileIfNotImageFile,
				visibleCompanyPK,
				visibleBranchPK,
				visibleDepartmentPK,
				description: description,
				source: documentSource);
		}

		/// <summary>
		/// This method returns a business object IN A DIFFERENT FACTORY. You must call DocManagerInfo.Save() to save your new Document in the database.
		/// </summary>
		public IeDoc AddFileOrDocument(
			byte[] contents,
			string filenameOnly,
			string documentType,
			bool overwriteExistingFileIfNotImageFile = false,
			Guid visibleCompanyPK = default,
			Guid visibleBranchPK = default,
			Guid visibleDepartmentPK = default,
			string description = "",
			string documentSource = "")
		{
			return StorageMain.AddFileOrDocument(
				contents,
				filenameOnly,
				documentType,
				overwriteExistingFileIfNotImageFile,
				visibleCompanyPK,
				visibleBranchPK,
				visibleDepartmentPK,
				description: description,
				source: documentSource);
		}

		public IRefDocTypeCollection GetAvailableDocumentTypes()
		{
			return ObjectFactory.Get<IDocumentScanningHelper>().GetAvailableDocumentTypes(DocManagerCode, BusinessEntity.Factory);
		}

		public void AddLogsForNewDocument(BusinessObject logOwner, IeDoc newDoc)
		{
			StorageMain.AddLogsForNewDocument(logOwner, newDoc);
		}

		#endregion

		#region ReadOnly

		/// <summary>
		/// ReadOnly status for the eDocs tab.
		/// </summary>
		public virtual bool ReadOnly
		{
			get { return BusinessEntity.ReadOnly; }
		}

		#endregion

		#region StorageNumber

		public virtual ZBool SupportsStorageNumberGeneration
		{
			get { return false; }
		}

		public virtual ZString GenerateStorageNumber()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region EDocsHasChanges

		public bool EDocsHasChanges => fStorageMain?.EDocsHasChanges ?? false;

		#endregion

		#region IDocManagerInfo Implementation

		IeDocBase IDocManagerInfo.AddFileOrDocument(SubStreamableStream contents, string filenameOnly, string documentType)
		{
			return AddFileOrDocument(contents, filenameOnly, documentType);
		}

		IeDocBase IDocManagerInfo.AddFileOrDocument(byte[] contents, string filenameOnly, string documentType)
		{
			return AddFileOrDocument(contents, filenameOnly, documentType);
		}

		IeDocBase IDocManagerInfoCore.AddFileOrDocumentCore(
			SubStreamableStream contents,
			string filenameOnly,
			string documentType,
			bool overwriteExistingFileIfNotImageFile,
			Guid visibleCompanyPK,
			Guid visibleBranchPK,
			Guid visibleDepartmentPK,
			string documentSource)
		{
			return AddFileOrDocument(
				contents,
				filenameOnly,
				documentType,
				overwriteExistingFileIfNotImageFile,
				visibleCompanyPK,
				visibleBranchPK,
				visibleDepartmentPK,
				documentSource: documentSource);
		}

		#endregion
	}
}
