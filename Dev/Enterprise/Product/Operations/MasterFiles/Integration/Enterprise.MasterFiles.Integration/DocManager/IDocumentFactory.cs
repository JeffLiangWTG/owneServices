using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocumentFactory : ITransactionParticipant, ICacheVersionProvider
	{
		/// <param name="itemFK">PK of the item to allocate to</param>
		/// <param name="relatedBusinessContext">3-letter DocumentScanning code</param>
		/// <param name="pathToFile">Full path to the tif file</param>
		/// <param name="documentType">3-letter Document Type</param>
		/// <param name="documentDescription">Description of the document</param>
		/// <param name="forceAllocate">Allocate document even if its type is not allowed to be autosaved to eDocs</param>
		/// <param name="fileName">File name of the document to be autosaved to eDocs</param>
		/// <param name="language">Language of the document in eDocs</param>
		/// <param name="dataType">Data type of the document in eDocs</param>
		BusinessObject CreateAndAllocateDocument(ZGuid itemFK, ZString relatedBusinessContext, ZString pathToFile, ZString documentType, ZString documentDescription, bool forceAllocate = false, string fileName = "", string language = "", string dataType = "", string printJobPKAsString = "");

		/// <summary>
		/// Adds a file or document to the specified parent. If the file you are adding is NOT an image file and 
		/// you choose to overwrite it, a new filename with a numeric suffix will be created (e.g. Test[2].doc)
		/// </summary>
		/// <param name="parentBizOPK">PK of the Business Object to add this document to</param>
		/// <param name="docManagerCode">The 3-letter DocManagerCode from DocManagerInfo</param>
		/// <param name="contents">Contents to add as stream</param>
		/// <param name="filenameOnly">The filename only (e.g. 'Sample.pdf')</param>
		/// <param name="documentType">three letter document type (e.g. 'CIV')</param>
		/// <param name="overwriteExistingFileIfNotAnImage">Whether or not to overwrite the existing file if one exists. Only applies to non-image files.</param>
		BusinessObject AddFileOrDocument(ZGuid parentBizOPK, string docManagerCode, SubStreamableStream contents, string filenameOnly, string documentType, string documentSource, bool overwriteExistingFileIfNotAnImage);

		/// <summary>
		/// Adds a file or document to the specified parent. If the file you are adding is NOT an image file and 
		/// you choose to overwrite it, a new filename with a numeric suffix will be created (e.g. Test[2].doc)
		/// </summary>
		/// <param name="parentBizOPK">PK of the Business Object to add this document to</param>
		/// <param name="docManagerCode">The 3-letter DocManagerCode from DocManagerInfo</param>
		/// <param name="contents">Contents to add as byte array</param>
		/// <param name="filenameOnly">The filename only (e.g. 'Sample.pdf')</param>
		/// <param name="documentType">three letter document type (e.g. 'CIV')</param>
		/// <param name="overwriteExistingFileIfNotAnImage">Whether or not to overwrite the existing file if one exists. Only applies to non-image files.</param>
		BusinessObject AddFileOrDocument(ZGuid parentBizOPK, string docManagerCode, byte[] contents, string filenameOnly, string documentType, string documentSource, bool overwriteExistingFileIfNotAnImage);

		IStorageMain RetrieveExistingOrCreateStorageMainForPK(ZGuid bizOPK, string docManagerCode);

		IStorageMain RetrieveExistingOrCreateStorageMain(BusinessObject bizO, string docManagerCode);

		IStorageMain GetStorageMainForPK(ZGuid bizObjPK);

		void UpdatePublishedFlagForAllEDocs(ZString referenceType, ZString documentType, ZBool isPublished);

		void UpdateDocTypeForAllEDocs(ZPropertyInfo referenceTypeInfo, ZPropertyInfo docTypeInfo, ZPropertyInfo descInfo);

		void UpdateDocTypeForJobRequiredDocument(ZPropertyInfo referenceTypeInfo, ZPropertyInfo docTypeInfo, ZPropertyInfo descInfo);

		BusinessObjectFactory GetFactory(ZInt databaseNumber);

		string[] NamesOfMissingDbs { get; }

		string GetDatabaseName(int databaseNumber);

		void Save();

		void SaveFactoriesExceptFactoryForEverythingExceptEDocs();

		IeDoc FindEDocsFromAllSDDatabasesByPK(ZGuid uniqueKey);

		BusinessObjectFactory FactoryForEverythingExceptEDocs { get; }
	}
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Integration.Testing
{
	public interface IDocumentFactoryForTest : IDocumentFactory
	{
		BusinessObject CreateDocument(ZGuid storageMainPK);
		bool Import(byte[] contents, ZString filenameOnly, ZString userSuppliedRefType, ZGuid userSuppliedRefPK, ZString userSuppliedDocType, ZString userSuppliedDocSource, ZGuid visibleCompanyPK, ZGuid visibleBranchPK, ZGuid visibleDepartmentPK, string fileFullPath = "");
	}
}

#endif
#endregion
