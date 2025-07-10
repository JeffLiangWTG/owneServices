
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackLoadListConsolCollection : CFSLoadListConsolCollection
	{
		public PackUnpackLoadListConsolCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new PackUnpackLoadListConsol this[int index]
		{
			get { return (PackUnpackLoadListConsol)Elements[index]; }
		}

		public new PackUnpackLoadListConsol AddNew()
		{
			return (PackUnpackLoadListConsol)base.AddNew();
		}
	}
}
