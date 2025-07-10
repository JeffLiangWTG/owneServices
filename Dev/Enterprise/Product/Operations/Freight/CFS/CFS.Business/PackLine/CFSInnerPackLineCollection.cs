
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSInnerPackLineCollection : InnerPackLineCollection
	{
		public CFSInnerPackLineCollection(CFSShipment master) : base(master, master.Factory)
		{
		}

		public new CFSPackLine AddNew()
		{
			return (CFSPackLine)base.AddNew();
		}

		public new CFSPackLine this[int index]
		{
			get { return (CFSPackLine)Elements[index]; }
		}
	}
}
