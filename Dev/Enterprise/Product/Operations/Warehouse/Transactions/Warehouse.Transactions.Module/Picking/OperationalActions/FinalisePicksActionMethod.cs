using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class FinalisePicksActionMethod : OperationalActionMethod
	{
		public FinalisePicksActionMethod()
			: base(new ZGuid("EE94623D-4E77-4849-9FB9-B6A49E1434B5"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new FinalisePicksActionMethodApplicator();
		}

		public override string Name => Res.GetString("dbd76738-f3be-4f08-a06e-eaaa969f5fb0", "Finalize Picks");

		public override string Description => Res.GetString("dbd76738-f3be-4f08-a06e-eaaa969f5fb0", "Finalize Picks");
	}
}
