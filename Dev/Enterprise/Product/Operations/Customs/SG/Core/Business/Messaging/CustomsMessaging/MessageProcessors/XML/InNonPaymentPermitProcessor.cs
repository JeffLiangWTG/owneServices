using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class InNonPaymentPermitProcessor : BasePermitSectionProcessor<InNonPaymentPermit>
	{
		public InNonPaymentPermitProcessor(LoggingInformation logger, InNonPaymentPermit section)
			: base(logger, section, MessageName)
		{
		}

		public const string MessageName = "In Non Payment Permit Message";

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.INPPMT;
	}

	class InNonPaymentUpdatePermitProcessor : BasePermitSectionProcessor<InNonPaymentUpdatePermit>
	{
		public InNonPaymentUpdatePermitProcessor(LoggingInformation logger, InNonPaymentUpdatePermit section)
			: base(logger, section, MessageName)
		{
		}

		public const string MessageName = "In Non Payment Update Permit Message";

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.INPUPT;
	}
}
