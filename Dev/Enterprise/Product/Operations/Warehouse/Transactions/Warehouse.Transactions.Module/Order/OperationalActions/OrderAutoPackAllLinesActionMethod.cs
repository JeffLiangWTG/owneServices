using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OrderAutoPackAllLinesActionMethod : OperationalActionMethod
	{
		public OrderAutoPackAllLinesActionMethod()
			: base(new ZGuid("EE94623D-4EA7-4849-9FB9-B6A49E1434B5"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new OrderAutoPackAllLinesApplicator(factory);
		}

		public override string Name => Res.GetString("dbd76738-f3be-4f08-ASDF-eaaa969f5fb0", "Auto Pack Orders");

		public override string Description => Res.GetString("dbd76738-f3be-4f08-ASDF-eaaa969f5fb0", "Auto Pack Orders");
	}
}
