using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class TranshipmentMovementPermitProcessor : BasePermitSectionProcessor<TranshipmentMovementPermit>
	{
		public TranshipmentMovementPermitProcessor(LoggingInformation logger, TranshipmentMovementPermit section)
			: base(logger, section, MessageName)
		{
		}

		public const string MessageName = "Transhipment Movement Permit Message";

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.TNPPMT;
	}

	class TranshipmentMovementUpdatePermitProcessor : BasePermitSectionProcessor<TranshipmentMovementUpdatePermit>
	{
		public TranshipmentMovementUpdatePermitProcessor(LoggingInformation logger, TranshipmentMovementUpdatePermit section)
			: base(logger, section, MessageName)
		{
		}

		public const string MessageName = "Transhipment Movement Update Permit Message";

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.TNPUPT;
	}
}
