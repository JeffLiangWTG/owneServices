using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class RemoveHoldAllPackagesActionMethod : OperationalActionMethod
	{
		public RemoveHoldAllPackagesActionMethod()
			: base(new ZGuid("787FFAB5-B69B-45D2-B0CA-0935C6FBD232"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new RemoveHoldAllPackagesApplicator(factory);
		}

		public override string Name => Res.GetString("4276C883-3804-4A45-BB6E-D214734D1849", "Remove Hold All Packages");

		public override string Description => Res.GetString("C4ACCB0A-FD75-4893-AF91-559B8E73CB95", "Remove the hold status of all the packages of the order");
	}
}
