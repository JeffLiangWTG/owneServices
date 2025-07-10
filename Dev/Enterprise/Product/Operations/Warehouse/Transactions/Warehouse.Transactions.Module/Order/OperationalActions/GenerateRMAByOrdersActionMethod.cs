using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateRMAByOrdersActionMethod : OperationalActionMethod
	{
		public GenerateRMAByOrdersActionMethod()
			: base(new ZGuid("61f413b0-b1df-4906-b20b-6d976b4fc067"))
		{
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl()
		{
			return new GenerateRMAReferenceReceivesByOrdersControl();
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new GenerateRMAByOrdersActionMethodApplicator(factory);
		}

		public override string Name => Res.GetString("e01d40ae-c925-4921-a099-b0eb1fde89ee", "Generate RMA for Selected Orders");

		public override string Description => Name;
	}
}
