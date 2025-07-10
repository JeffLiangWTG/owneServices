using System;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business
{
	public class RequestTariffUpdatesMessageSender : MessageSender
	{
		public RequestTariffUpdatesMessageSender(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool Prepare()
		{
			var filerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(Job.RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode;

			if (filerCode.IsEmpty)
			{
				Job.MessageInitiator.WarnUserAboutSomething("No ABI filer code found. The ABI tariff query can only be used when an active ABI profile is found. No Query was sent.", "Request Tariff Update for tariffs used");
				return false;
			}

			return true;
		}

		protected override bool GenerateMessage()
		{
			new ReferenceFileRequester(Job.Factory, true).RequestTariffs(Job, true);
			return true;
		}

		protected override string SuccessfulSendNotification
		{
			get { return SuccessfulSendMessage; }
		}
		const string SuccessfulSendMessage = "Tariff information has been requested from ABI, once the request has been processed by Customs, select 'Refresh Tariff Details' from the Brokerage menu to update the tariff details in this job.";
	}
}
