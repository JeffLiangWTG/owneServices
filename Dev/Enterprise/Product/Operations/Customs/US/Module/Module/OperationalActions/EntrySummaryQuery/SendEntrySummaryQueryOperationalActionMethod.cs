using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendEntrySummaryQueryOperationalActionMethod : USOperationalActionMethod
	{
		public SendEntrySummaryQueryOperationalActionMethod()
			: base(new ZGuid("476E2DB4-F7BE-4F36-8332-7D6352B71817"))
		{
		}

		public override string Name => "Send Entry Summary Query operational action";

		public override string Description => "Send Entry Summary Query(US)";

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendEntrySummaryQueryMethodApplicator(factory);

		public override bool HasControl => false;

		public override bool HasSettings => false;
	}
}
