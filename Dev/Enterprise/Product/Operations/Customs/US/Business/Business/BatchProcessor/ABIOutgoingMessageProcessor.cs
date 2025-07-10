using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ABIOutgoingMessageProcessor : CBPOutgoingMessageProcessor
	{
		public ABIOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZQuery MessageFilter
		{
			get
			{
				if (messageFilter == null)
				{
					messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
					messageFilter.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, GetMessageTypesToExclude());
				}
				return messageFilter;
			}
		}
		ZQuery messageFilter;

		public static ZString[] GetMessageTypesToExclude() => new ZString[] { ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling };
	}
}
