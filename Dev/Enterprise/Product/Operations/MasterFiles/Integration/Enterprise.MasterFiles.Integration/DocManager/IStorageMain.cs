using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	/// <summary>
	/// Not accessible via ObjectFactory. Use IDocumentFactoryProvider.
	/// </summary>
	public interface IStorageMain
	{
		IStorageDocsBaseCollection Documents { get; }
		IStorageDocsBaseCollection Files { get; }
		IStorageDocsBaseCollection AllEDocs { get; }
		IStorageDocsBaseCollection EDocsView { get; }
		BusinessObject DocumentOwner { get; }
		ZString PhysicalLocation { get; }
		ZString DocumentOwnerDescription { get; }
		IeDoc AddFileOrDocument(byte[] contents, string filenameOnly, string documentType);
		IeDoc AddFileOrDocument(
			SubStreamableStream contents,
			string filenameOnly,
			string documentType,
			bool overwriteExistingFileIfNotImageFile = false,
			Guid visibleCompanyPK = default,
			Guid visibleBranchPK = default,
			Guid visibleDepartmentPK = default,
			string description = "",
			string source = "");
		IeDoc AddFileOrDocument(
			byte[] contents,
			string filenameOnly,
			string documentType,
			bool overwriteExistingFileIfNotImageFile = false,
			Guid visibleCompanyPK = default,
			Guid visibleBranchPK = default,
			Guid visibleDepartmentPK = default,
			string description = "",
			string source = "");
		ZGuid ParentFK { get; }
		Enum DocumentOwnerModuleID { get; }
		void AddLogsForNewDocument(BusinessObject logOwner, IeDoc newDoc);
		bool EDocsHasChanges { get; }
	}
}
