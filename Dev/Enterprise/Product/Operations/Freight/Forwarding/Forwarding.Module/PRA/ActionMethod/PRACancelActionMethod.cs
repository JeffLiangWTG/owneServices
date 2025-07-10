using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using MessageType = Enterprise.Customs.Common.AU.PRAMessageTypeConstants.MessageType;

namespace Enterprise.Freight.Forwarding.Module
{
	public sealed class PRACancelActionMethod : PRABaseActionMethod
	{
		public PRACancelActionMethod()
			: base(new ZGuid("2e171b35-7e86-4d64-a24e-4a4c6d332b23")) { }

		public override string Name
		{
			get { return Res.GetString("2e171b35-7e86-4d64-a24e-4a4c6d332b23", "Cancel PRA for All Containers"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description
		{
			get
			{
				return Res.GetString(
					"7b66a7e4-0d6f-4c83-949b-e4a87812c24c",
					@"Cancel a PRA message for all containers in the forwarding consol which the action is run.

Error Behavior: defines what should happen if an error is encountered while attempting to cancel a PRA message.
Abort - if an error is encountered then the entire action is aborted. This is the recommended behavior if the action does more then simply sending PRA messages.
Skip - if an error is encountered then the affected records are skipped allowing the action to continue with the remaining records. This is the recommended behavior if sending PRA messages is the only thing the action does.");
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new PRAMessageApplicator(MessageType.Cancel, (PRASettings)settings);
		}
	}
}
