using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.BatchProcessor
{
	public class NZCOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public NZCOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new NZTSWInterchangeProvider(Logger, readyMessages);
		}

		protected override ZQuery MessageFilter
		{
			get
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.NewZealandCustoms);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, new MessageTypeList().GetAllCodes());
				return query;
			}
		}

		protected override void HandleBeforeMessageProcessing(NonDependentEDIMessageCollection readyMessages, BusinessObjectFactory factory)
		{
			var entryPKs = (from EDIMessage message in readyMessages where message.EM_LinkTable == CusEntryHeader.Schema.TableName select message.EM_LinkUniqueID).ToList();

			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddToFilter(JobDeclarationSchema.JE_EntrySubmittedDate, ZDateTime.Empty);
			var entrySubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			entrySubQuery.AddToFilter(CusEntryHeaderSchema.PK, entryPKs);
			query.AddSubQuery(JobDeclarationSchema.PK, CusEntryHeaderSchema.CH_JE, entrySubQuery, JoinCondition.And);

			var declarations = factory.Load<JobDeclaration>(query);

			if (declarations.Any())
			{
				var today = ZDateTime.Today;
				declarations.ForEach(x => x.JE_EntrySubmittedDate = today);
			}

			AddEventsForOCRMessages(readyMessages, factory);
		}

		void AddEventsForOCRMessages(NonDependentEDIMessageCollection readyMessages, BusinessObjectFactory factory)
		{
			var consolPks = readyMessages.Cast<EDIMessage>().Where(x => x.EM_LinkTable == AutoJobConsol.Schema.TableName && x.EM_MessageType == MessageTypeList.Codes.OCR).Select(x => x.EM_LinkUniqueID);
			var query = new ZDBOnlyQuery(typeof(ForwardingConsol));
			query.AddToFilter(JobConsolSchema.PK, consolPks);
			var consols = factory.Load<ForwardingConsol>(query);
			consols.ForEach(x => x.Logs.AddNew(Events.MessageSent, "STC", ZDateTimeOffset.Now));
		}
	}
}
