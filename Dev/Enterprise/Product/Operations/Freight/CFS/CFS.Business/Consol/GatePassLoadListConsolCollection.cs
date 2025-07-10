
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassLoadListConsolCollection : CFSLoadListConsolCollection
	{
		public GatePassLoadListConsolCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new GatePassLoadListConsol this[int index]
		{
			get { return (GatePassLoadListConsol)Elements[index]; }
		}

		public new GatePassLoadListConsol AddNew()
		{
			return (GatePassLoadListConsol)base.AddNew();
		}
	}
}
