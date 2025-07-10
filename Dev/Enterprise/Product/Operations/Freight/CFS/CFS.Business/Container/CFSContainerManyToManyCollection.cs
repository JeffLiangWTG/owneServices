
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSContainerManyToManyCollection : CommonContainerManyToManyCollection
	{
		public CFSContainerManyToManyCollection(CFSPackLine parent) : base(parent)
		{
		}

		public new CFSContainer this[int index]
		{
			get { return (CFSContainer)(Elements[index]); }
		}

		public new CFSContainer AddNew()
		{
			return (CFSContainer)base.AddNew();
		}
	}
}
