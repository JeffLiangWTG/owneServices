using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal sealed class EIDOSendActionMethod : EIDOBaseActionMethod
	{
		public EIDOSendActionMethod()
			: base(new ZGuid("9a54e235-2865-4d2d-af0e-f5782280184c")) { }

		public override string Name
		{
			get { return Res.GetString("9a54e235-2865-4d2d-af0e-f5782280184c", "Send E-IDO Message"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description
		{
			get
			{
				return Res.GetString(
					"01bb1f64-b90b-439b-9c0b-76a9dc3b273a",
					@"Sends an E-IDO original message for all containers attached to the bills on which the action is run.

Error Behavior: defines what should happen if an error is encountered while attempting to send an E-IDO original message.
Abort - if an error is encountered then the entire action is aborted. This is the recommended behavior if the action does more then simply sending E-IDO original messages.
Skip - if an error is encountered that only affects a limited number of bills, then the affected bills are skipped allowing the action to continue with the remaining bills. This is the recommended behavior if sending E-IDO original messages is the only thing the action does."
					);
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new EIDOSendOriginalApplicator((ReleaseImportOrderSettings)settings);
		}
	}
}


