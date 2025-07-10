using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;
using ParameterCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Business.EventProcessors
{
	public static class SupportingDocumentStatusHelper
	{
		internal static void UpdateStatus(CusEntryHeader entryHeader, ZString caseNo)
		{
			if (entryHeader != null && !caseNo.IsEmpty)
			{
				var caseNumber = entryHeader.EntryInstruction?.CaseNumbers.OfType<CaseNumber>().FirstOrDefault(x => x.CY_Data == caseNo);
				if (caseNumber != null)
				{
					caseNumber.Document_Status = DocumentStatusCodes.Codes.PND;
				}
			}
		}

		static IEnumerable<StmALog> GetEventsRelatedToCaseNumber(Logs logs, ZString lrn, ZString caseNo)
		{
			return logs?.GetAllLogs().OfType<StmALog>().Where(x =>
			{
				var result = x.SL_SE_NKEvent == Events.DocumentSentCode
							|| x.SL_SE_NKEvent == Events.DocumentDeliveredCode
							|| x.SL_SE_NKEvent == Events.DocumentNotDeliveredCode;
				if (result)
				{
					result = IsEventRelatedToCase(x, lrn, caseNo);
				}
				return result;
			});
		}

		public static bool IsEventRelatedToCase(StmALog evnt, ZString lrn, ZString caseNo)
		{
			var parameters = StmALog.GetParametersFromReference(evnt.SL_Reference);
			return parameters.GetValueSafe(ParameterCodes.RequestNumber) == caseNo
				&& parameters.GetValueSafe(ParameterCodes.ReferenceNumber) == lrn;
		}

		public static ZString GetFileName(UniversalEvent evnt)
		{
			return evnt?.ContextCollection?.FirstOrDefault(x => x.Type == nameof(UniversalEvent.ContextTypes.FileName))?.Value ?? ZString.Empty;
		}

		static void AddEventsToMap(BusinessObjectFactory factory, SupportingDocumentDictionary files, IEnumerable<StmALog> events, UniversalEvent currentEvent)
		{
			foreach (var documentEvent in events)
			{
				if (currentEvent == null ||
					documentEvent.SL_SE_NKEvent != currentEvent.EventType.Value ||
					documentEvent.SL_EventTimeUtc != currentEvent.EventTime.GetValueOrDefault().ToUtcZDateTime() ||
					documentEvent.SL_Reference != currentEvent.EventReference.Value)
				{
					UniversalEvent universalEvent = null;
					try
					{
						var relatedMessagePivot = factory.LoadTop1<IGenPivot>(MessagePivotQuery(documentEvent.PK));
						if (relatedMessagePivot != null)
						{
							var ediMessage = factory.Load<EDIMessage>(relatedMessagePivot.XX_Relation2ID);
							if (ediMessage != null && ediMessage.IsInDatabase)
							{
								using (var reader = ediMessage?.GetEM_MessageTextReader())
								{
									universalEvent = reader?.Parse<UniversalEvent>();
								}
							}
						}

						var fileName = GetFileName(universalEvent);
						AddToMap(files, fileName, documentEvent.SL_SE_NKEvent, documentEvent.SL_EventTime);
					}
					finally
					{
						universalEvent?.Dispose();
					}
				}
			}
			AddCurrentEventToMap(files, currentEvent);
		}

		static ZQuery MessagePivotQuery(ZGuid logPK)
		{
			var query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_RelationType, Constants.GenPivotTypes.XmlEdiMessage);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, logPK);
			return query;
		}

		static void AddCurrentEventToMap(SupportingDocumentDictionary files, UniversalEvent currentEvent)
		{
			if (currentEvent != null)
			{
				var eventType = currentEvent?.EventType ?? ZString.Empty;
				var fileName = GetFileName(currentEvent);
				AddToMap(files, fileName, eventType, ZDateTime.Now);
			}
		}

		static void AddToMap(SupportingDocumentDictionary files, ZString fileName, ZString eventType, ZDateTime eventTime)
		{
			if (!fileName.IsEmpty)
			{
				if (files.ContainsKey(fileName))
				{
					var map = files.GetValueSafe(fileName);
					if (eventType == Events.DocumentSentCode)
					{
						if (map.LastSentTime < eventTime)
						{
							map.LastSentTime = eventTime;
						}
						if (files.LastOverallSentTime < eventTime)
						{
							files.LastOverallSentTime = eventTime;
						}
					}
					else if (map.LastReceivedTime < eventTime)
					{
						map.LastReceivedTime = eventTime;
						map.LastEvent = eventType;
					}
				}
				else
				{
					var map = new SupportingDocumentMap(files)
					{
						LastSentTime = eventType == Events.DocumentSentCode ? eventTime : ZDateTime.MinSmallDateTimeValue,
						LastReceivedTime = eventType != Events.DocumentSentCode ? eventTime : ZDateTime.MinSmallDateTimeValue,
						LastEvent = eventType
					};
					files.Add(fileName, map);
					if (files.LastOverallSentTime < eventTime)
					{
						files.LastOverallSentTime = eventTime;
					}
				}
			}
		}

		internal static void UpdateStatus(CusEntryHeader entryHeader, CaseNumber caseNumber, UniversalEvent currentEvent = null)
		{
			if (entryHeader != null && caseNumber != null)
			{
				var files = new SupportingDocumentDictionary();
				var lrn = entryHeader.CH_BGMReference;
				var caseNo = caseNumber.CY_Data;

				var documentEventsRelatedToCaseNumber = GetEventsRelatedToCaseNumber(entryHeader?.Declaration?.Logs, lrn, caseNo);

				AddEventsToMap(entryHeader.Factory, files, documentEventsRelatedToCaseNumber, currentEvent);

				if (files.Count > 0)
				{
					if (files.All(x => x.Value.Status == DocumentStatusCodes.Codes.SNT))
					{
						caseNumber.Document_Status = DocumentStatusCodes.Codes.SNT;
					}
					else if (files.Any(x => x.Value.Status == DocumentStatusCodes.Codes.FAL))
					{
						caseNumber.Document_Status = DocumentStatusCodes.Codes.FAL;
					}
					else
					{
						caseNumber.Document_Status = DocumentStatusCodes.Codes.PND;
					}
				}
				else
				{
					caseNumber.Document_Status = ZString.Empty;
				}
			}
		}
	}

	class SupportingDocumentDictionary : Dictionary<ZString, SupportingDocumentMap>
	{
		public ZDateTime LastOverallSentTime { get; set; } = ZDateTime.MinSmallDateTimeValue;
	}

	class SupportingDocumentMap
	{
		public SupportingDocumentMap(SupportingDocumentDictionary parentDictionary)
		{
			this.parentDictionary = parentDictionary;
		}

		readonly SupportingDocumentDictionary parentDictionary;

		public ZDateTime LastSentTime { get; set; }
		public ZDateTime LastReceivedTime { get; set; }
		public ZString LastEvent { get; set; }
		public ZString Status
		{
			get
			{
				var result = ZString.Empty;

				if (LastSentTime > LastReceivedTime)
				{
					result = DocumentStatusCodes.Codes.PND;
				}
				else if (LastEvent == Events.DocumentNotDeliveredCode)
				{
					if (LastReceivedTime >= parentDictionary.LastOverallSentTime)
					{
						result = DocumentStatusCodes.Codes.FAL;
					}
					else
					{
						result = DocumentStatusCodes.Codes.SNT;
					}
				}
				else if (LastEvent == Events.DocumentDeliveredCode)
				{
					result = DocumentStatusCodes.Codes.SNT;
				}

				return result;
			}
		}
	}
}
