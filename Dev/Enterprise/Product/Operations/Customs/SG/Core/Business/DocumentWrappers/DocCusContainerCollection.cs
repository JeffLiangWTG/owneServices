using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocCusContainerCollection : DocBaseCusContainerCollection
	{
		public DocCusContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusContainerCollection(CusContainerCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusContainer this[int index]
		{
			get { return (DocCusContainer)base[index]; }
		}
	}
}
