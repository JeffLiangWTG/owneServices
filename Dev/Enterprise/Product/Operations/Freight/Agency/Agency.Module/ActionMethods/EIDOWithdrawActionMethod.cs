using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	internal sealed class EIDOWithdrawActionMethod : EIDOBaseActionMethod
	{
		public EIDOWithdrawActionMethod()
			: base(new ZGuid("e35c29e8-a4d6-40e4-8adb-8d7efb90752e")) { }

		public override string Name
		{
			get { return Res.GetString("e35c29e8-a4d6-40e4-8adb-8d7efb90752e", "Withdraw E-IDO Message"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description
		{
			get
			{
				return Res.GetString(
					"82a6b194-f39e-4690-99b1-73e4ee50df67",
					@"Sends an E-IDO withdraw message for all containers attached to the bills on which the action is run.

Error Behavior: defines what should happen if an error is encountered while attempting to send an E-IDO withdraw message.
Abort - if an error is encountered then the entire action is aborted. This is the recommended behavior if the action does more then simply sending E-IDO withdraw messages.
Skip - if an error is encountered that only affects a limited number of bills, then the affected bills are skipped allowing the action to continue with the remaining bills. This is the recommended behavior if sending E-IDO withdraw messages is the only thing the action does."
					);
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new EIDOSendCancellationApplicator((ReleaseImportOrderSettings)settings);
		}
	}
}


