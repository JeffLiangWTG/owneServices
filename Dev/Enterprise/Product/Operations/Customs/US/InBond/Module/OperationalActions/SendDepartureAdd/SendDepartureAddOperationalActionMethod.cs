using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions
{
	public class SendDepartureAddOperationalActionMethod : USOperationalActionMethod
	{
		public SendDepartureAddOperationalActionMethod()
			: base(new ZGuid("2948E70E-4A79-4024-BC59-AEDE7C55C478"))
		{
		}

		public override string Name => "Send Departure Add Operational Action";

		public override string Description => "Send Departure Add";

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendDepartureAddActionMethodApplicator(factory);

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new SendDepartureAddOperationActionControl();
		}

		public override bool HasSettings
		{
			get { return false; }
		}
	}
}
