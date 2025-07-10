using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocCusContainerCollection : DocBaseCusContainerCollection
	{
		public DocCusContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusContainer this[int index]
		{
			get
			{
				return (DocCusContainer)Elements[index];
			}
		}
	}
}
