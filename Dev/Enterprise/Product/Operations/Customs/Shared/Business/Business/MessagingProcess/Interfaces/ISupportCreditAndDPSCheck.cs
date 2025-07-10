using Enterprise.MasterFiles.CreditControl.Business;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ISupportCreditAndDPSCheckOptions
	{
		bool MustRunCreditCheck { get; }
		string CreditRestrictionMessageCaption { get; }
		string DefaultApprovalRequestReason { get; }
	}

	public interface ISupportCreditAndDPSCheck : ISupportCreditAndDPSCheckOptions
	{
		ICreditControlledDocumentDelivery DocumentDeliveryObject { get; }
	}
}
