using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class DocManagerInfoExtensionMethod
	{
		public static IEnumerable<IeDoc> GetRelatedEDocs(this DocManagerInfo docManager)
		{
			foreach (IeDoc eDoc in docManager.AllEDocs)
			{
				yield return eDoc;
			}

			foreach (var relatedBizO in docManager.RelatedObjects)
			{
				var docManagerSupport = relatedBizO as IDocManagerSupport;
				if (docManagerSupport != null)
				{
					foreach (IeDoc eDoc in docManagerSupport.DocManagerInfo.AllEDocs)
					{
						yield return eDoc;
					}
				}
			}
		}

		public static IEnumerable<IeDoc> GetRelatedEDocsView(this DocManagerInfo docManager)
		{
			foreach (IeDoc eDoc in docManager.EDocsView)
			{
				yield return eDoc;
			}

			foreach (var relatedBizO in docManager.RelatedObjects)
			{
				var docManagerSupport = relatedBizO as IDocManagerSupport;
				if (docManagerSupport != null)
				{
					foreach (IeDoc eDoc in docManagerSupport.DocManagerInfo.EDocsView)
					{
						yield return eDoc;
					}
				}
			}
		}
	}
}
