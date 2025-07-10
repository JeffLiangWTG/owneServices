using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendAESTIROperationalActionMethod : USOperationalActionMethod
	{
		public SendAESTIROperationalActionMethod()
			: base(new ZGuid("476E2DB4-F7BE-4F36-8332-7D6352B71815"))
		{
		}

		public override string Name => "Send AESTIR operational action";

		public override string Description => "Send AES (US)";

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendAESTIRActionMethodApplicator(factory);

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new SendAESTIROperationActionControl();

		public override bool HasSettings => false;
	}
}
