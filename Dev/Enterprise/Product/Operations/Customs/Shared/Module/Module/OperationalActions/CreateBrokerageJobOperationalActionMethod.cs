using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Module.OperationalActions
{
	public class CreateBrokerageJobOperationalActionMethod : OperationalActionMethod
	{
		public CreateBrokerageJobOperationalActionMethod()
			: base(new ZGuid("f6cdbea4-dbc5-471e-87b0-2840fe655db8"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("ead53984-633f-4324-8ea4-3e53d29e2929", "Create Brokerage Job operational action for shipments"); }
		}

		public override string Name
		{
			get { return Res.GetString("b65b6334-a1c2-430e-a364-71eebe9c9742", "Create Brokerage Job operational action"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CreateBrokerageJobOperationalActionMethodApplicator();
		}

		public override bool IsRunAgainDisabled
		{
			get { return true; }
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override bool HasSettings
		{
			get { return false; }
		}

		public override System.ComponentModel.IComponent NewGuiControl()
		{
			return new CreateBrokerageJobOperationalActionControl();
		}
	}
}
