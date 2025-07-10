using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendEntrySummaryOperationalActionMethod : USOperationalActionMethod
	{
		public SendEntrySummaryOperationalActionMethod()
			: base(new ZGuid("476E2DB4-F7BE-4F36-8332-7D6352B71814"))
		{
		}

		public override string Name => "Send Entry Summary operational action";

		public override string Description => "Send Entry Summary (US)";

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendEntrySummaryActionMethodApplicator(factory);

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new SendEntrySummaryOperationActionControl();

		public override bool HasSettings => false;
	}
}
