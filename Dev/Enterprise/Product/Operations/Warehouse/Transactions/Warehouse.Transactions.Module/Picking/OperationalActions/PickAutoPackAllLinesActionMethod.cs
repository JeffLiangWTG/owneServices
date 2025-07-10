using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickAutoPackAllLinesActionMethod : OperationalActionMethod
	{
		public PickAutoPackAllLinesActionMethod()
			: base(new ZGuid("EE94623D-4ED7-4849-9FB9-B6A49E1434B5"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new PickAutoPackAllLinesApplicator(factory);
		}

		public override string Name => Res.GetString("ab8fff47-604a-47fe-a809-14791726a487", "Auto Pack Picks");

		public override string Description => Res.GetString("ab8fff47-604a-47fe-a809-14791726a487", "Auto Pack Picks");
	}
}
