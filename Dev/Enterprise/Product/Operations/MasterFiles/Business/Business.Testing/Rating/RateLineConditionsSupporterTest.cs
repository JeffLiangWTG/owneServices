namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class RateLineConditionsSupporterTest<TAdapter, TSupporter> : SupporterTest<TAdapter, IAutoRatingFreightConditionsSupportable, TSupporter>
			where TAdapter : IAutoRatingFreightConditionsSupportable
			where TSupporter : RateLineConditionsSupporter
	{
		protected override void RunAssertions()
		{
			RunSetterGetterAssertion("ExportBroker", SetExportBroker, x => x.ExportBroker);
			RunSetterGetterAssertion("ImportBroker", SetImportBroker, x => x.ImportBroker);
			RunSetterGetterAssertion("SendingAgent", SetSendingAgent, x => x.SendingAgent);
			RunSetterGetterAssertion("ReceivingAgent", SetReceivingAgent, x => x.ReceivingAgent);
			RunSetterGetterAssertion("ControllingAgent", SetControllingAgent, x => x.ControllingAgent);
			RunSetterGetterAssertion("DepartureCFS", SetDepartureCFS, x => x.DepartureCFS);
			RunSetterGetterAssertion("ArrivalCFS", SetArrivalCFS, x => x.ArrivalCFS);
		}

		protected override TSupporter GetSupporter(IAutoRatingFreightConditionsSupportable supportable)
		{
			return (TSupporter)supportable.ConditionsSupporter;
		}

		protected abstract OrgHeader SetExportBroker(TAdapter adapter);
		protected abstract OrgHeader SetImportBroker(TAdapter adapter);
		protected abstract OrgHeader SetSendingAgent(TAdapter adapter);
		protected abstract OrgHeader SetReceivingAgent(TAdapter adapter);
		protected abstract OrgHeader SetControllingAgent(TAdapter adapter);
		protected abstract OrgHeader SetDepartureCFS(TAdapter adapter);
		protected abstract OrgHeader SetArrivalCFS(TAdapter adapter);
	}
}
