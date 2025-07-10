using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business
{
	public static class EDocsHelper
	{
		public static IEnumerable<IStorageDocsBaseCollection> GetEDocCollections(params IDocManagerSupport[] docManagers)
		{
			return docManagers
				.Select(docManagerSupport => docManagerSupport?.DocManagerInfo?.AllEDocs)
				.Where(eDocs => eDocs != null);
		}
	}
}
