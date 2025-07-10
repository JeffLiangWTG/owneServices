using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IEDocsWebHelper
	{
		IEnumerable<IeDocBase> GetEDocs(List<ZGuid> parentPKs);
		bool HasEDocs(List<ZGuid> parentPKs);
		IEnumerable<IeDocBase> GetEDocsByDocType(List<ZGuid> parentPKs, string docType);
		bool HasEDocsByDocType(List<ZGuid> parentPKs, string docType);
	}
}
