using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GeneratePickActionMethod : OperationalActionMethod
	{
		public GeneratePickActionMethod()
			: base(new ZGuid("6CFF4247-661E-45df-A517-9E9564506358"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new GeneratePickActionMethodApplicator();
		}

		public override string Name => Res.GetString("b937e5ae-fdaa-49a2-80c1-9770bb4f1010", "Generate Pick");

		public override string Description => Res.GetString("b937e5ae-fdaa-49a2-80c1-9770bb4f1010", "Generate Pick");
	}
}
