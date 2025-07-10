using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.OperationalAction;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendEntrySummaryQueryMethodApplicator : USDeclarationOperationalActionMethodApplicator
	{
		public SendEntrySummaryQueryMethodApplicator(BusinessObjectFactory factory)
			: base("Send Entry Summary Query operational action", factory)
		{ }

		public override ValidationModes ValidationMode => ValidationModes.None;

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new SendEntrySummaryQueryOperationalActionRunner(log);
			jobsPK = new List<ZGuid>();
			jobsPK.AddRange(runner.PerformFunctionOperationalAction(true, targets));
		}

		protected override ZString MessageDescriptionCore => "Entry Summary Query";
	}
}
