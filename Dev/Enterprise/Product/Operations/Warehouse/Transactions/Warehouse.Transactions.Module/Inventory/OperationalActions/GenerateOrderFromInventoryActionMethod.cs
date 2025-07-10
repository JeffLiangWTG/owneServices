using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateOrderFromInventoryActionMethod : OperationalActionMethod
	{
		public GenerateOrderFromInventoryActionMethod()
			: base(new ZGuid("7B3BE18B-61D1-4877-8FE5-A850934403C2"))
		{
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl()
		{
			return new GenerateOrderFromInventoryOrReceiveControl();
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new GenerateOrderFromInventoryActionMethodApplicator(factory);
		}

		public override string Name => Res.GetString("9a38ba12-ce05-4738-ba7b-ad91e34c279d", "Generate Order from Inventory");

		public override string Description => Res.GetString("9a38ba12-ce05-4738-ba7b-ad91e34c279d", "Generate Order from Inventory");
	}
}
