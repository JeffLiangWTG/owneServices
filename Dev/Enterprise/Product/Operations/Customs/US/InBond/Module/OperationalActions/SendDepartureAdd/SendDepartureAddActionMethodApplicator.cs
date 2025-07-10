using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.InBond.Business.OperationalActions;
using Enterprise.Customs.US.Module.OperationalActions;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.InBond.Module.OperationalActions
{
	public class SendDepartureAddActionMethodApplicator : USOperationalActionMethodApplicator
	{
		public SendDepartureAddActionMethodApplicator(BusinessObjectFactory factory)
			: base("Send Departure Add operational action", factory)
		{ }

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (IsMessagingAllowed())
			{
				var sendWithMessageErrors = SendWithMessageErrors && IsSendWithMessageErrorsAllowed;
				jobsPK = new List<ZGuid>();
				IEnumerable<ZGuid> jobsRunAction;
				if (targets.Length > 0 && targets[0] is USInBondMoveHeader)
				{
					var runner = new SendDepartureAddPerMovementOperationalActionRunner(log);
					jobsRunAction = runner.PerformFunctionOperationalAction(sendWithMessageErrors, targets);
				}
				else
				{
					var runner = new SendDepartureAddOperationalActionRunner(log);
					jobsRunAction = runner.PerformFunctionOperationalAction(sendWithMessageErrors, targets);
				}

				jobsPK.AddRange(jobsRunAction);
			}
		}

		bool IsMessagingAllowed()
		{
			bool result = true;
			if (!Env.Security.USInBondMessaging.IsAllowed)
			{
				Env.Security.ShowError(Env.Security.USInBondMessaging);
				result = false;
			}

			return result;
		}

		protected override ZString MessageDescriptionCore
		{
			get { return "Departure Add"; }
		}

		public override US.Business.ValidationModes ValidationMode
		{
			get { return US.Business.ValidationModes.None; }
		}
	}
}
