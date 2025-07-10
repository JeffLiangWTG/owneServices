using System;
using CargoWise.IO;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IDocManagerInfoCore : IDocManagerInfo
	{
		bool UseBusinessEntityFactoryAsInternal { get; set; }
		IDocumentFactory MasterFactory { get; }

		IeDocBase AddFileOrDocumentCore(
			SubStreamableStream contents,
			string filenameOnly,
			string documentType,
			bool overwriteExistingFileIfNotImageFile = false,
			Guid visibleCompanyPK = default,
			Guid visibleBranchPK = default,
			Guid visibleDepartmentPK = default,
			string documentSource = "");

		void Save();
	}
}
