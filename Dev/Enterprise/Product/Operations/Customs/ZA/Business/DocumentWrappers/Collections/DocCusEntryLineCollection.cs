using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocCusEntryLineCollection : DocumentWrapperCollection
	{
		public DocCusEntryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryLineCollection(Enterprise.Customs.Business.ICusEntryLineCollection<CusEntryLine> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryLine this[int index]
		{
			get { return (DocCusEntryLine)base[index]; }
		}
	}
}
