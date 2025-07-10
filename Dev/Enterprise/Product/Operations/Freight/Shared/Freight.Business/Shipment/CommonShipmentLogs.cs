using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonShipmentLogs : Logs
	{
		public CommonShipmentLogs(CommonShipment shipment)
			: base(shipment)
		{
		}

		public new CommonShipment Parent
		{
			get { return (CommonShipment)base.Parent; }
		}

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			var additionalQuery = new ZQuery(StmALogSchema.SL_Table, JobShipmentSchema.Constants.TableName);
			return new StmALogDependentCollection(Parent, additionalQuery);
		}
	}
}
