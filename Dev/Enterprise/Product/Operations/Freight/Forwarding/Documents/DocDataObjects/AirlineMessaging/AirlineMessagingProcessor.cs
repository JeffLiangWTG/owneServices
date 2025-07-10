using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging;

public class AirlineMessagingProcessor
{
	readonly AirlineMessagingProcessorLogger logger = new AirlineMessagingProcessorLogger();

	public bool TryProcessResponse(string apiResponseXml, out IDictionary<int, string> originalMessagesById, out IDictionary<int, UniversalEvent> parsedEventsById, out string exception)
	{
		exception = null;
		originalMessagesById = new Dictionary<int, string>();
		parsedEventsById = new Dictionary<int, UniversalEvent>();
		try
		{
			var bodyDocument = GetValidXmlFromInterchangeBody(apiResponseXml);
			foreach (var universalEvent in bodyDocument.Root.Elements())
			{
				using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(universalEvent.ToString())))
				{
					var eventDataObject = new UniversalEvent();
					ObjectFactory.Get<IXmlReader>().ReadXML(eventDataObject, stream, logger);
					var eventId = universalEvent.GetHashCode();
					parsedEventsById.Add(eventId, eventDataObject);
					originalMessagesById.Add(eventId, universalEvent.ToString());
					if (logger.HasErrors())
					{
						throw new XmlProcessingException(logger.GetAllErrors());
					}
					logger.Clear();
				}
			}
			if (!parsedEventsById.Any())
			{
				throw new XmlProcessingException("'UniversalEvent' does not exist or it is empty.");
			}
			if (parsedEventsById.Count == 1 && parsedEventsById.First().Value.EventType.HasValue && parsedEventsById.First().Value.EventType.Value == AutoEvents.InterchangeRejectedCode)
			{
				throw new ArgumentException(parsedEventsById.First().Value.EventParameters.Reason);
			}
			if (parsedEventsById.Count > 1 && parsedEventsById.Values.Any(pe => pe.EventType.HasValue && pe.EventType.Value == AutoEvents.InterchangeRejectedCode))
			{
				throw new XmlProcessingException("Unexpected format of XML response from Pelican API.");
			}
		}
		catch (XmlProcessingException ex)
		{
			parsedEventsById = null;
			exception = Res.GetString("E86DAC42-D010-4949-9833-DAF61934E565",
				$"Failed to parse message received from Airline Messaging Gateway due to XML processing error: {ex.Message}");
			return false;
		}
		catch (ArgumentException ex)
		{
			exception = Res.GetString("93346B29-5178-42E4-A4C4-805CE530A071",
				$"Interchange message rejected by API: {ex.Message}");
			return false;
		}
		catch (InvalidOperationException ex)
		{
			parsedEventsById = null;
			exception = Res.GetString("B6A31CC9-EFBF-4967-B557-DEB7BCB786F1",
				$"Failed to parse message received from Airline Messaging Gateway: {ex.Message}");
			return false;
		}
		return true;
	}

	XDocument GetValidXmlFromInterchangeBody(string responseXml)
	{
		var interchangeSerializer = new XmlSerializer(typeof(UniversalInterchange));
		var bodySerializer = new XmlSerializer(typeof(UniversalInterchangeBody));
		var interchange = (UniversalInterchange)interchangeSerializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(responseXml)));
		var xmlWriter = new StringWriter();
		bodySerializer.Serialize(xmlWriter, interchange.Body);
		var bodyXml = xmlWriter.ToString();
		return XDocument.Parse(bodyXml);
	}

	sealed class AirlineMessagingProcessorLogger : IXmlImportLogger
	{
		public IEnumerable<ISimpleLog> Logs => logs;

		readonly List<ISimpleLog> logs = new List<ISimpleLog>();

		public void Clear() => logs.Clear();

		public bool HasErrors() => logs.Any(l => l.Type == LogType.Error);

		public void Log(LogType type, string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
			{
				logs.Add(new SimpleLog(type, message));
			}
		}

		public string GetAllErrors()
		{
			var stringBuilder = new StringBuilder();
			foreach (var log in logs.Where(l => l.Type == LogType.Error))
			{
				stringBuilder.Append(log.Message);
			}
			return stringBuilder.ToString();
		}

		public void LogBoth(LogType type, string message)
		{
			throw new NotImplementedException();
		}

		public void LogErrorToServiceTaskOnly(string message)
		{
			throw new NotImplementedException();
		}

		public void FireDataImportedToBusinessObject(BusinessObject targetBO)
		{
			throw new NotImplementedException();
		}

		public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
		{
			throw new NotImplementedException();
		}

		public bool IsUpdatingConsol { get; set; }
		public bool HasIgnoredModule { get; set; }
		public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }
		public ITopLevelDataObject TopLevelDataObject { get; }
		public IDataContextDataObject TopLevelDataContext { get; }
		public bool OrgMatchingDisabled { get; }
	}
}
