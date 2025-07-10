
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport
{
	public class OrderedShipments : BusinessObjectCollection<CommonShipment>
	{
		public OrderedShipments(ConsolShipmentCollection unorderedShipments)
			: base(unorderedShipments.Factory)
		{
			unorderedShipments.CopyToList(this);
			Sort(JobShipmentSchema.JS_UniqueConsignRef.Name, System.ComponentModel.ListSortDirection.Ascending);
		}
	}
}
