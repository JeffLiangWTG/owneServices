using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ChangePriorityActionMethod : OperationalActionMethod
	{
		public ChangePriorityActionMethod()
			: base(new ZGuid("1A3CFFD6-AA4A-4E0F-87D6-EDFBBD29954A"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ChangePriorityActionMethodApplicator();
		}

		public override string Name => Res.GetString("ChangePriorityActionMethod|Name", "Change Priority");

		public override string Description => Res.GetString("ChangePriorityActionMethod|Description", "Change Priority");
	}
}
