using System.Collections;
using System.Linq;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BusinessObjectCopyManagerForTest : BusinessObjectCopyManager
	{
		protected override IEnumerable GetCollectionFromDb(object sourceEntity, CollectionCopyTemplateNode collectionCopyTemplateNode)
		{
			var collection = base.GetCollectionFromDb(sourceEntity, collectionCopyTemplateNode);
			return collectionCopyTemplateNode.Name == "JobComInvoiceHeaders" ? collection?.Cast<CommonJobComInvoiceHeader>().OrderBy(x => x.JZ_GroupInvoice) : collection;
		}
	}
}
