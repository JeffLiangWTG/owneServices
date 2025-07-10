using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Module.OperationalActions
{
	public class BrokerageSubmitOperationalActionMethod : OperationalActionMethod
	{
		public BrokerageSubmitOperationalActionMethod()
			: base(new ZGuid("264c801a-6f9d-4f5a-ad90-67df63ea90d2"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("be992087-c117-4934-9cc3-031fc14a7f5e", "Submit operational action for brokerage jobs"); }
		}

		public override string Name
		{
			get { return Res.GetString("8b01d1fe-509f-4836-ac06-3de3931e2b74", "Submit operational action"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new BrokerageSubmitOperationalActionMethodApplicator();
		}

		public override bool IsRunAgainDisabled
		{
			get { return true; }
		}

		public override bool HasControl
		{
			get { return false; }
		}

		public override bool HasSettings
		{
			get { return false; }
		}
	}
}
