namespace Enterprise.Freight.Agency.Module
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.Services.OperationalActions.Support;

	internal sealed class ReleaseOrderSendActionMethod : ReleaseOrderActionMethod
	{
		public ReleaseOrderSendActionMethod()
			: base(new ZGuid("42f59178-9617-11e4-b649-902b34dc814a")) { }

		public override string Name
		{
			get { return Res.GetString("42f59178-9617-11e4-b649-902b34dc814a", "Send Import Release Order Message"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description
		{
			get
			{
				return Res.GetString(
					"aaad2440-9617-11e4-aefd-902b34dc814a",
					@"Sends a release order message for all containers attached to the bills on which the action is run.

Error Behavior: defines what should happen if an error is encountered while attempting to send an release order original message.
Abort - if an error is encountered then the entire action is aborted. This is the recommended behavior if the action does more then simply sending release order original messages.
Skip - if an error is encountered that only affects a limited number of bills, then the affected bills are skipped allowing the action to continue with the remaining bills. This is the recommended behavior if sending release order original messages is the only thing the action does."
					);
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new SendNZReleaseOrderActionMethodApplicator((ReleaseImportOrderSettings)settings);
		}
	}
}


