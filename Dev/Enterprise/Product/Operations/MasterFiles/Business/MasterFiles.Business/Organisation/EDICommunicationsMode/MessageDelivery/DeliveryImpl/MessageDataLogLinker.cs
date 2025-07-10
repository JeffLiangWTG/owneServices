using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	public class MessageDataLogLinker
	{
		public MessageDataLogLinker(Event eventType, BusinessObjectFactory factory, ZString eventReference)
		{
			EventType = eventType;
			EventReference = eventReference;
			Factory = factory;
		}

		public void LinkMessageToParentBOLogs(IEDIMessage message, IStmALogParent parentBOWithLogs)
		{
			if (parentBOWithLogs != null)
			{
				var logsNotInDB = parentBOWithLogs.Logs.LogsNotInDB;
				var logsToLink = GetLogsToLink(Factory, logsNotInDB);

				if (logsToLink.Length == 0)
				{
					var newLog = parentBOWithLogs.Logs.AddNew(EventType, EventReference);
					message.AddUniversalDataLink(newLog);
				}
				else
				{
					foreach (var log in logsToLink)
					{
						message.AddUniversalDataLink(log);
					}
				}
			}
		}

		StmALog[] GetLogsToLink(BusinessObjectFactory factory, StmALog[] logsNotInDB)
		{
			return logsNotInDB.Where(log => ShouldLinkToMessage(factory, log)).ToArray();
		}

		bool ShouldLinkToMessage(BusinessObjectFactory factory, StmALog log)
		{
			return
				log != null
				&& log.SL_SE_NKEvent == EventType.Code
				&& log.SL_Reference == EventReference
				&& !IsLogLinkedToMessage(factory, log);
		}

		static bool IsLogLinkedToMessage(BusinessObjectFactory factory, StmALog log)
		{
			var query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, StmALogSchema.Constants.Prefix);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, log.PK);
			query.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);

			return factory.LoadTop1<GenPivot>(query) != null;
		}

		protected Event EventType { get; set; }
		protected BusinessObjectFactory Factory { get; set; }
		protected ZString EventReference { get; set; }
	}
}
