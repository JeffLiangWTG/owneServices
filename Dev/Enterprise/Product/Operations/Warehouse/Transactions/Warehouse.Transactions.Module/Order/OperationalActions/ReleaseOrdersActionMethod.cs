using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReleaseOrdersActionMethod : OperationalActionMethod
	{
		public ReleaseOrdersActionMethod()
			: base(new ZGuid("6f5497e9-e8ac-417f-bf71-568e22e2a275"))
		{
		}

		public override string Name => Res.GetString("b732395c-2c66-4dc8-8852-bc557a835d26", "Release Orders");

		public override string Description => Res.GetString("b732395c-2c66-4dc8-8852-bc557a835d26", "Release Orders");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ReleaseOrdersActionMethodApplicator();
		}
	}
}
