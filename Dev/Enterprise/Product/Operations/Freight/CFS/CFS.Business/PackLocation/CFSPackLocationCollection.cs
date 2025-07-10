
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSPackLocationCollection : PackLocationCollection
	{
		public CFSPackLocationCollection(CFSPackLine parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public new CFSPackLocation this[int index]
		{
			get { return (CFSPackLocation)(Elements[index]); }
		}

		public new CFSPackLocation AddNew()
		{
			return (CFSPackLocation)base.AddNew();
		}

		protected new CFSPackLine Parent
		{
			get { return (CFSPackLine)base.Parent; }
		}
	}
}
