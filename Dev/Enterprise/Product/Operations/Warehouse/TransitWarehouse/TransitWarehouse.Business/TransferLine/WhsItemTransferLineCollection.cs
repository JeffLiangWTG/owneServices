using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemTransferLineCollection : ActiveBusinessObjectCollection<WhsItemTransferLine>
	{
		public WhsItemTransferLineCollection(WhsItemTransferHeader transferHeader)
			: base(transferHeader.Factory, transferHeader, null, WhsItemTransferLineSchema.WTF_WTH_TransitTransferHeader)
		{
		}

		public WhsItemTransferLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
