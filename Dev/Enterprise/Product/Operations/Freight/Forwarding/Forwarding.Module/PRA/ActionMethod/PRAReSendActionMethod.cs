using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using MessageType = Enterprise.Customs.Common.AU.PRAMessageTypeConstants.MessageType;

namespace Enterprise.Freight.Forwarding.Module
{
	public class PRAReSendActionMethod : PRABaseActionMethod
	{
		public PRAReSendActionMethod()
			: base(new ZGuid("9a0f7603-dddd-4a7c-8b4e-28b2e62874d2")) { }

		public override string Name
		{
			get { return Res.GetString("9a0f7603-dddd-4a7c-8b4e-28b2e62874d2", "Re-Send PRA for All Containers"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description
		{
			get
			{
				return Res.GetString(
					"1ff14349-ff90-4e5d-a32f-b8ba93cf0e5d",
					@"Re-send a PRA message for all containers in the forwarding consol which the action is run.

Error Behavior: defines what should happen if an error is encountered while attempting to re-send a PRA message.
Abort - if an error is encountered then the entire action is aborted. This is the recommended behavior if the action does more then simply sending PRA messages.
Skip - if an error is encountered then the affected records are skipped allowing the action to continue with the remaining records. This is the recommended behavior if sending PRA messages is the only thing the action does.");
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new PRAMessageApplicator(MessageType.ReSubmit, (PRASettings)settings);
		}
	}
}
