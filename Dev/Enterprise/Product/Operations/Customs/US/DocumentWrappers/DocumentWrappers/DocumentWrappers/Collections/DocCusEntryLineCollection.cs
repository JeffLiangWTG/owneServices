using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class DocCusEntryLineCollection : DocBaseCusEntryLineCollection
	{
		public DocCusEntryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryLineCollection(Customs.Business.ICusEntryLineCollection<CusEntryLine> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryLine this[int index]
		{
			get { return (DocCusEntryLine)base[index]; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
