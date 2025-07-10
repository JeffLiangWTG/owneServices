using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateOrderFromReceiveActionMethod : OperationalActionMethod
	{
		public GenerateOrderFromReceiveActionMethod()
			: base(new ZGuid("A833E4CC-6220-4554-B344-746EB7EB9F4A"))
		{
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl()
		{
			return new GenerateOrderFromInventoryOrReceiveControl();
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new GenerateOrderFromReceiveActionMethodApplicator(factory);
		}

		public override string Name => Res.GetString("54992738-8b80-417f-92bf-15143b110e90", "Generate Order from Receive");

		public override string Description => Res.GetString("54992738-8b80-417f-92bf-15143b110e90", "Generate Order from Receive");
	}
}
