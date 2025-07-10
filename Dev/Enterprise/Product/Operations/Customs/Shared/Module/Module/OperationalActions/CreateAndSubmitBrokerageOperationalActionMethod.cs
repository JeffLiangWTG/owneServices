using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Module.OperationalActions
{
	public class CreateAndSubmitBrokerageOperationalActionMethod : OperationalActionMethod
	{
		public CreateAndSubmitBrokerageOperationalActionMethod()
			: base(new ZGuid("3bc98024-2e18-4999-9929-f2e990800de8"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("9517800b-5242-4259-824f-8ccd21af7e16", "Create and Submit Brokerage Job operational action for shipments"); }
		}

		public override string Name
		{
			get { return Res.GetString("5e9bbb1d-7869-4309-bf26-3eff5097164c", "Create and Submit Brokerage Job operational action"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CreateAndSubmitBrokerageOperationalActionMethodApplicator();
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
