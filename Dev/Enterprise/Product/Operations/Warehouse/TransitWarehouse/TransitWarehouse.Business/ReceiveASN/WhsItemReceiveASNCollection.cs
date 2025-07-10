using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveASNCollection : ActiveBusinessObjectCollection<WhsItemReceiveASN>
	{
		public WhsItemReceiveASNCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsItemReceiveASNCollection(BusinessObjectFactory factory, WhsItemReceiveTransportationUnit rtu)
			: base(factory, new AdhocCollectionRelationship(typeof(WhsItemReceiveASN)))
		{
			AddRange(Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, rtu.PK)).Select(p => p.ReceiveASN).Distinct());
		}
	}
}
