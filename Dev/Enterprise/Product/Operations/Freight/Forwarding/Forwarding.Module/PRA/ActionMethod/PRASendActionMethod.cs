using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using MessageType = Enterprise.Customs.Common.AU.PRAMessageTypeConstants.MessageType;

namespace Enterprise.Freight.Forwarding.Module
{
	public sealed class PRASendActionMethod : PRABaseActionMethod
	{
		public PRASendActionMethod()
			: base(new ZGuid("4a431ec3-7ed4-410c-9baf-b1145fb85fa8")) { }

		public override string Name
		{
			get { return Res.GetString("4a431ec3-7ed4-410c-9baf-b1145fb85fa8", "Submit PRA for All Containers"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description
		{
			get
			{
				return Res.GetString(
					"d1276d11-e41d-4ecb-b866-25e3a1e1ae44",
					@"Submit a PRA message for all containers in the forwarding consol which the action is run.

Error Behavior: defines what should happen if an error is encountered while attempting to submit a PRA message.
Abort - if an error is encountered then the entire action is aborted. This is the recommended behavior if the action does more then simply sending PRA messages.
Skip - if an error is encountered then the affected records are skipped allowing the action to continue with the remaining records. This is the recommended behavior if sending PRA messages is the only thing the action does.");
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new PRAMessageApplicator(MessageType.Submit, (PRASettings)settings);
		}
	}
}
