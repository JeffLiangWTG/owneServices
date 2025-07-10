using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.OperationalAction;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendAESTIRActionMethodApplicator : USDeclarationOperationalActionMethodApplicator
	{
		public SendAESTIRActionMethodApplicator(BusinessObjectFactory factory)
			: base("Send AESTIR Message operational action", factory)
		{ }

		public override ValidationModes ValidationMode => ValidationModes.None;

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var sendWithMessageErrors = SendWithMessageErrors && IsSendWithMessageErrorsAllowed;
			var runner = new SendAESTIROperationalActionRunner(log);
			jobsPK = new List<ZGuid>();
			jobsPK.AddRange(runner.PerformFunctionOperationalAction(sendWithMessageErrors, targets, ApportionWeight));
		}

		protected override ZString MessageDescriptionCore => "AESTIR";
	}
}
