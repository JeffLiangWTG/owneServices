using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	class BatchSGInterchangeHelperForTesting : BatchSGInterchangeHelper
	{
		public BatchSGInterchangeHelperForTesting(LoggingInformation logger) : base(logger)
		{
		}

		public override bool ShowVerboseLogging
		{
			get
			{
				return true;
			}
		}

		public override GlbExternalPassword_SGv4 GetGlbExternalPassword(SGGlbStaffWrapper brokerWrapper)
		{
			return brokerWrapper.Tradenetv4Password;
		}

		public override ZString PasswordType => PasswordTypesList.Codes.SG4;
		public override ZString ApplicationDescription => "TradeNet";
		public override ZString[] ApplicationCodes => new ZString[] { ApplicationCodeList.Codes.SGCustomsTradenet4, ApplicationCodeList.Codes.SGCustomsTradenetXML };
	}
}
