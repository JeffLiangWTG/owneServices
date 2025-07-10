using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemConsignmentOrderReferenceCollection : ActiveBusinessObjectCollection<WhsItemConsignmentOrderReference>
	{
		public WhsItemConsignmentOrderReferenceCollection(WhsItemReceiveConsignment rcn) : base(rcn.Factory, rcn, new ZQuery(), WhsItemConsignmentOrderReferenceSchema.WOR_ParentID) { }
		public WhsItemConsignmentOrderReferenceCollection(WhsItemDispatchConsignment dcn) : base(dcn.Factory, dcn, new ZQuery(), WhsItemConsignmentOrderReferenceSchema.WOR_ParentID) { }
	}
}
