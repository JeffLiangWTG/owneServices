using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OrderSetCarrierAndCarrierServiceLevelActionMethod : OperationalActionMethod
	{
		public OrderSetCarrierAndCarrierServiceLevelActionMethod()
			: base(new ZGuid("f0678605-d380-4ec6-ae3d-302dd5ff4fc6"))
		{
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl()
		{
			return new SelectCarrierAndServiceLevelControl();
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new OrderSetCarrierAndCarrierServiceLevelActionMethodApplicator(factory);
		}

		public override string Name => Res.GetString("f9e05045-defe-4565-a758-661ee8f2544b", "Set Carrier And Carrier Service Level");

		public override string Description => Name;
	}
}
