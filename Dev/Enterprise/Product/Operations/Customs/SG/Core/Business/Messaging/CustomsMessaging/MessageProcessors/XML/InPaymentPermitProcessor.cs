using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class InPaymentPermitProcessor : BasePermitSectionProcessor<InPaymentPermit>
	{
		public InPaymentPermitProcessor(LoggingInformation logger, InPaymentPermit section)
			: base(logger, section, MessageName)
		{
		}

		public const string MessageName = "In Payment Permit Message";

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.IPTPMT;
	}

	class InPaymentUpdatePermitProcessor : BasePermitSectionProcessor<InPaymentUpdatePermit>
	{
		public InPaymentUpdatePermitProcessor(LoggingInformation logger, InPaymentUpdatePermit section)
			: base(logger, section, MessageName)
		{
		}

		public const string MessageName = "In Payment Update Permit Message";

		protected override string URN => GetReferenceNumber(Section.RefundOnly?.RefundHeader?.UniqueReferenceNumber ?? Section.Declaration?.Header?.UniqueReferenceNumber);

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.IPTUPT;

		protected override bool NeedToUpdateRefund
		{
			get
			{
				var updateIndicatorCode = Section.Update?.UpdateIndicatorCode?.Trim()?.ToUpperInvariant() ?? string.Empty;
				return updateIndicatorCode == SGConstants.UpdateIndicators.AMR || updateIndicatorCode == SGConstants.UpdateIndicators.FRF || updateIndicatorCode == SGConstants.UpdateIndicators.PRG || updateIndicatorCode == SGConstants.UpdateIndicators.PRS;
			}
		}
	}
}
