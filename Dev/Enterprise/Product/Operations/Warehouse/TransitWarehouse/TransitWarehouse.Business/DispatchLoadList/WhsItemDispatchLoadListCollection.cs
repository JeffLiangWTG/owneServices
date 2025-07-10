using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchLoadListCollection : ActiveBusinessObjectCollection<WhsItemDispatchLoadList>
	{
		public WhsItemDispatchLoadListCollection(BusinessObjectFactory factory) : base(factory) { }

		public WhsItemDispatchLoadListCollection(BusinessObjectFactory factory, WhsItemDispatchTransportationUnit dtu)
			: base(factory, new AdhocCollectionRelationship(typeof(WhsItemDispatchLoadList)))
		{
			AddRange(Factory.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, dtu.PK)).Select(p => p.DispatchLoadList).Distinct());
		}
	}
}
