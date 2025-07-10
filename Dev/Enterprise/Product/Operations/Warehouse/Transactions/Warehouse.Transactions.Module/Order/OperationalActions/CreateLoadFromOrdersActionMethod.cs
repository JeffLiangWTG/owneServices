using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class CreateLoadsFromOrdersActionMethod : OperationalActionMethod
	{
		public CreateLoadsFromOrdersActionMethod()
			: base(new ZGuid("ee0c01c1-764d-41e7-93bc-8f9c39c366e0"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CreateLoadsFromOrdersMethodApplicator(factory);
		}

		public override string Name => Res.GetString("53911967-f7ed-4ae0-95fb-7f63b3a9c1ba", "Create Loads");

		public override string Description => Name;
	}
}
