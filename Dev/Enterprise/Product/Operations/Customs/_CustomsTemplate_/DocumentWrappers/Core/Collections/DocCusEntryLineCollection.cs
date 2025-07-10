#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocCusEntryLineCollection : DocumentWrapperCollection
	{
		public DocCusEntryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryLineCollection(Customs.Business.ICusEntryLineCollection<CusEntryLine> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryLine this[int index] => (DocCusEntryLine)base[index];
	}
}
#endif
