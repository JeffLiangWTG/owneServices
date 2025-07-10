using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

[assembly: MailSubscriber(typeof(Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.CMDMessageReceiver))]

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage
{
	public class CMDMessageReceiver
	{
		public CMDMessageReceiver()
		{
		}

		public LoggingInformation Logger => logger ?? (logger = new LoggingInformation());
		LoggingInformation logger;

		public void Process()
		{
			var count = 0;
			var processor = new MailBatchProcessor(mailFilter, (item, p) =>
			{
				ProcessMailItem(item.Factory, item);
				Logger.Log("Processed " + item.PK + " Mail Item", LogType.Debug);

				count++;
				return MailProcessingResult.MarkSuccess;
			});

			processor.Process(null);
			if (count > 0)
			{
				Logger.Log(count + " interchanges have been received and processed.", LogType.Information);
			}
		}

		readonly IMailFilter mailFilter = GetInboundMailFilter();

		[MailFilter(MailFilterCodes.CMDMessage)]
		public static IMailFilter GetInboundMailFilter()
		{
			var filter = new ZQuery();
			filter.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Receive);
			filter.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.Queued);
			filter.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress);
			filter.AddToFilter(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.StartsWith, "CARGOIMP RESPONSE FROM EAGLE"); // CargoIMP Email Subject Name
			filter.AddToFilter(BuildIncludedOrExcludedBodiesQuery());

			return new QueryMailFilter(MailFilterCodes.CMDMessage, filter);
		}

		static ZQuery BuildIncludedOrExcludedBodiesQuery()
		{
			var singaporeCustomsQuery = new ZQuery();
			singaporeCustomsQuery.AddToFilter(JoinCondition.Or, MailDBItemsSchema.MI_Body, SQLComparisonOperator.StartsWith, new[] { "CMDCMA", "CMDFNA", "CMDFMA" });

			return singaporeCustomsQuery;
		}

		void ProcessMailItem(BusinessObjectFactory factory, MailItem item)
		{
			CMDInbound inboundMessage = new CMDInbound(item.MI_Body);
			if (inboundMessage.IsValid && inboundMessage.StandardMessageIdentifier != CMDInbound.Constants.FMA)
			{
				var interchange = CreateInterchange(factory, inboundMessage.MessageText);

				var helper = new CMDInboundMessageProcesserHelper(Logger);
				helper.CreateMessageThrouthInterchangeAndSendEmail(interchange, inboundMessage);
			}
			else
			{
				Logger.Log(string.Format("Inbound '{0}' message is not valid. Ignoring. Message starts: {1}", inboundMessage.StandardMessageIdentifier, inboundMessage.MessageText.SubstringSafe(50)), LogType.Warning);
			}
		}

		EDIInterchange CreateInterchange(BusinessObjectFactory factory, string text)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_BodyText = text;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "EDI CCN"; // CargoIMP Message From

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			interchange.EI_To = (registrationKey.EnterpriseCode.Length == 0 ? (string)GlbCompany.CurrentCompany.GC_Code : registrationKey.EnterpriseCode) + registrationKey.ServerCode;

			return interchange;
		}
	}
}
