using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.eTail.Integration;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocZADA306LineCollection : DocumentWrapperCollection<DocZADA306Line>
	{
		public DocZADA306LineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocZADA306LineCollection(IHVLVItemCollectionForDocument objectsToWrap)
			: base(objectsToWrap, objectsToWrap?.Factory ?? new BusinessObjectFactory())
		{
		}
	}
}
