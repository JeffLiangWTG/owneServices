#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocCusEntryHeaderCollection : DocBaseCusEntryHeaderCollection
	{
		public DocCusEntryHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryHeaderCollection(Customs.Business.ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryHeader this[int index] => (DocCusEntryHeader)Elements[index];
	}
}
#endif
