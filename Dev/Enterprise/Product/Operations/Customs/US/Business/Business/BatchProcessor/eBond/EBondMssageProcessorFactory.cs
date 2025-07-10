using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	sealed class EBondMssageProcessorFactory : ApplicationTypeMessageProcessor
	{
		public EBondMssageProcessorFactory(LoggingInformation logger)
			: base(logger)
		{
			innerMessageProcessor = ObjectFactory.New<IEBondMessageProcessor>(new XmlSessionTracker(logger));
		}

		readonly IEBondMessageProcessor innerMessageProcessor;

		protected override string MessageFriendlyNameCore => Res.GetString("D40840BF-8702-4CD2-95D8-1D3739AEA639", "US Customs eBond Message Processor");

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.USeBond;

		protected override ZQuery MessageFilterCore
		{
			get
			{
				var result = base.MessageFilterCore;
				result.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
				return result;
			}
		}

		protected override void ProcessMessageCore(Enterprise.Messaging.Business.EDIMessage message)
		{
			try
			{
				innerMessageProcessor.Process(message.Factory, Logger, message.PK);
			}
			catch (InvalidMessageFormatException ex)
			{
				message.EM_Status = EDIMessage.Status.Failed;
				Logger.Log("Processing message failed : " + ex.Message);
			}
			catch (MessageProcessDiscardedException ex)
			{
				message.EM_Status = EDIMessage.Status.Discarded;
				Logger.Log("Message is discarded : " + ex.Message);
			}
		}
	}
}
