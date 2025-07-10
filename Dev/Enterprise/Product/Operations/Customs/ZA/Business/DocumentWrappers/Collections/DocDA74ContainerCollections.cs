using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocDA74ContainerCollection : DocumentWrapperCollection
	{
		public DocDA74ContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocDA74Container this[int index]
		{
			get { return (DocDA74Container)base[index]; }
		}
	}
}
