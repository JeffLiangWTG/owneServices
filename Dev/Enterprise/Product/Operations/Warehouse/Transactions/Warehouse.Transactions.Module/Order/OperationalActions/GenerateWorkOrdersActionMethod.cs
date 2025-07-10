using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateWorkOrdersActionMethod : OperationalActionMethod
	{
		public GenerateWorkOrdersActionMethod()
			: base(new ZGuid("702C4B68-3773-4BB0-B823-A01194593AC5"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new GenerateWorkOrdersActionMethodApplicator();
		}

		public override string Name => Res.GetString("27d632cd-5f95-4670-8eeb-09ac29c4412f", "Generate Work Orders");

		public override string Description => Res.GetString("27d632cd-5f95-4670-8eeb-09ac29c4412f", "Generate Work Orders");
	}
}
