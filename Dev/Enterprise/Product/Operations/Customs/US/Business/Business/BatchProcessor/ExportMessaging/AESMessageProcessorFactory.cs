using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class AESMessageProcessorFactory : MessageProcessorFactory
	{
		public AESMessageProcessorFactory(LoggingInformation logger)
			: base(logger, EDIMessage.ApplicationCodes.USCustomsExport, "US Customs AES Message Processor")
		{
		}

		protected override ZQuery MessageFilterCore
		{
			get
			{
				ZQuery result = base.MessageFilterCore;
				result.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.AES.GetAESCodes());
				return result;
			}
		}
	}
}
