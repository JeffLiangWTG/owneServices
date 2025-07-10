using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocCusEntryHeaderCollection : DocBaseCusEntryHeaderCollection
	{
		public DocCusEntryHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryHeaderCollection(ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryHeader this[int index]
		{
			get { return (DocCusEntryHeader)Elements[index]; }
		}
	}
}
