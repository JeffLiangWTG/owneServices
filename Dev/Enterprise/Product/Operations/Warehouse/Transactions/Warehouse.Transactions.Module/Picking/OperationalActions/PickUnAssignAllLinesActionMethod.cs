using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickUnAssignAllLinesActionMethod : LinesUserAssignerActionMethod<WhsPick>
	{
		public PickUnAssignAllLinesActionMethod(bool isRelease)
			: base(new ZGuid("d6cee324-dbe3-4f3c-8723-afb23fe08d56"))
		{
			this.isRelease = isRelease;
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
			=> isRelease ? new ReleaseUnAssignAllLinesActionMethodApplicator(factory) : new PickUnAssignAllLinesActionMethodApplicator(factory);

		protected override string GetOperationalActionName()
			=> Res.GetString("36fb870f-20b1-47e1-b28c-f907a92f77e2", "Un-assign Pick Lines");

		readonly bool isRelease;
	}
}
