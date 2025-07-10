using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFOutgoingMessageProcessor : CBPOutgoingMessageProcessor
	{
		public ISFOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		public static ZString GetMessageTypeToInclude() => ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;

		protected override ZQuery MessageFilter
		{
			get
			{
				if (messageFilter == null)
				{
					messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, MQEDIMessage.ApplicationCodes.USCustomsImport);
					messageFilter.AddToFilter(EDIMessageSchema.EM_MessageType, GetMessageTypeToInclude());
				}
				return messageFilter;
			}
		}
		ZQuery messageFilter;

		protected override Enterprise.Messaging.InterchangeProviders.InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new ISFInterchangeProvider(readyMessages);
		}

		class ISFInterchangeProvider : CBPInterchangeProvider
		{
			public ISFInterchangeProvider(NonDependentEDIMessageCollection messages)
				: base(messages)
			{
			}

			protected override ZString GetFromKey(Enterprise.Messaging.Business.EDIMessage message, ZString filerID)
			{
				var result = base.GetFromKey(message, filerID);
				if (result.IsEmpty)
				{
					var company = message.Company ?? GlbCompany.CurrentCompany;
					result = FromKey + company.GC_Code;
				}
				return result;
			}

			const string FromKey = "ISF";
		}
	}
}
