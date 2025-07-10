using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickAssignAllLinesToUserActionMethod : LinesUserAssignerActionMethod<WhsPick>
	{
		public PickAssignAllLinesToUserActionMethod(bool isRelease)
			: base(new ZGuid("d401eb96-14e6-4aa5-936b-ebc4105e2221"))
		{
			this.IsRelease = isRelease;
		}

		readonly bool IsRelease;

		#region Overrides

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return IsRelease ? new ReleaseAssignAllLinesToUserApplicator(factory) : new PickAssignAllLinesToUserApplicator(factory);
		}

		protected override string GetOperationalActionName()
		{
			return Res.GetString("fe3b26db-2688-4ac8-a74d-2ccb5938df6c", "Assign Pick Lines to user");
		}

		#endregion
	}
}
