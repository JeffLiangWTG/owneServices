using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class OutwardPermitProcessor : BasePermitSectionProcessor<OutwardPermit>
	{
		public OutwardPermitProcessor(LoggingInformation logger, OutwardPermit section)
			: base(logger, section, MessageName)
		{
		}

		public const string MessageName = "Outward Permit Message";

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.OUTPMT;
	}

	class OutwardUpdatePermitProcessor : BasePermitSectionProcessor<OutwardUpdatePermit>
	{
		public OutwardUpdatePermitProcessor(LoggingInformation logger, OutwardUpdatePermit section)
			: base(logger, section, MessageName)
		{
		}

		public const string MessageName = "Outward Update Permit Message";

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.OUTUPT;
	}
}
