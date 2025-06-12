using CargoWise.eHub.Shared.Mime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using System.Xml.XPath;
using System.Xml;
using Microsoft.XLANGs.BaseTypes;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.Products.GBCustoms.CDS.BT.Helpers
{
    public class OrchestrationHelpers
    {
        public static string ConvertDestinationParty(string originalParty)
        {
            Dictionary<string, string> destinationParties = new Dictionary<string, string>{
                {"GBCustomsTest-DirectDocument", "GBCustomsTest-Direct" },
                {"GBCustoms-DirectDocument", "GBCustoms-Direct" } };

            var destinationParty = string.Empty;

            if (string.IsNullOrEmpty(originalParty) || destinationParties.TryGetValue(originalParty, out destinationParty))
            {
                return destinationParty;
            }

            return originalParty;
        }

        public static string GetHttpHeaderValue(string httpHeaders, string name)
        {
            var headers = MimeUtils.ParseHeaders(httpHeaders);
            string value;
            return headers.TryGetValue(name, out value) ? value : string.Empty;
        }

        public static Stream CreateAWSendMessage(string keys, XmlDocument xmlMessage, string contentType, string boundary,
                string attachmentInBase64, string fileName, string fileContentType, string senderId, string recipientId, ILog logger)
        {
            try
            {
                var attachmentContent = Convert.FromBase64String(attachmentInBase64);

                List<string> listAWSContentHeaders = new List<string>(keys.Split(','));

                var mimeMessage = new MimePart.Multipart(contentType, boundary)
                {
                    Headers =
                    {
                        { "Message-ID", string.Format("<{0}>", boundary) },
                        { "MIME-Version", "1.0" },
                        { "Content-Type", contentType }
                    }
                };

                string value;
                foreach (string key in listAWSContentHeaders)
                {
                    value = ExtractDataValueXPath(ref xmlMessage, "//*[local-name()='Fields']/*[local-name()='" + key + "']");

                    mimeMessage.Parts.Add(new MimePart.Content("text/xml")
                    {
                        Headers =
                                {
                                    { "Content-Disposition", "form-data; name=\"" + key + "\"" }
                                },
                        Contents = new MemoryStream(Encoding.UTF8.GetBytes(value))

                    });
                }

                using (var streamForLog = mimeMessage.Format())
                using (var reader = new StreamReader(streamForLog, Encoding.UTF8))
                {
                    streamForLog.Position = 0;
                    logger.Trace($"CreateAWSendMessage - Request message without attachment: {reader.ReadToEnd()}");
                }

                var attachmentStream = new MemoryStream(attachmentContent);
                fileContentType = "application/octet-stream";
                mimeMessage.Parts.Add(
                    new MimePart.Content(fileContentType)
                    {
                        Headers =
                        {
                            { "Content-Disposition", String.Format("form-data; name=\"file\"; filename=\"{0}\"", fileName) },
                            { "Content-Type", fileContentType },
                            { "Content-ID", string.Format("<{0}>", boundary) },
                            { "Content-Transfer-Encoding", "binary" },
                            { "Content-Length", attachmentContent.Length.ToString() }
                        },
                        Contents = attachmentStream
                    });
                return mimeMessage.Format();
            }
            catch (Exception ex)
            {
                LogMessageDetails(ex.Message, senderId, recipientId, attachmentInBase64, ref logger);
                throw;
            }
        }

        static void LogMessageDetails(string error, string senderId, string recipientId, string attachmentInBase64, ref ILog logger)
        {
            logger.ErrorFormat(@"Error while proccessing the following message: 
                Sender: {0}
                Recipient: {1}
                Error: {2}
                Attachment:{3}", senderId, recipientId, error, attachmentInBase64);
        }

        static string ExtractDataValueXPath(ref XmlDocument xmlMessage, string msgXPath)
        {
            var xmlNode = xmlMessage.SelectSingleNode(msgXPath);

            if (xmlNode == null || string.IsNullOrEmpty(xmlNode.InnerText))
            {
                return "";
            }

            return xmlNode.InnerText;
        }

        static public string RemoveImageDataNode(XLANGMessage message, ILog logger)
        {
            StringBuilder sb = new StringBuilder();
            bool isImageData = false;

            try
            {
                using (VirtualStream virtualStream = new VirtualStream(0x280, 0x100000))
                {
                    using (Stream partStream = (Stream)message[0].RetrieveAs(typeof(Stream)))
                    {
                        using (XmlReader reader = XmlReader.Create(partStream))
                        {
                            while (reader.Read())
                            {
                                if (reader.LocalName != "ImageData" && !isImageData)
                                {
                                    switch (reader.NodeType)
                                    {
                                        case XmlNodeType.Element:
                                            sb.AppendFormat("<{0}", reader.Name);
                                            if (reader.HasAttributes)
                                            {
                                                for (int i = 0; i < reader.AttributeCount; i++)
                                                {
                                                    reader.MoveToAttribute(i);
                                                    sb.AppendFormat(" {0}=\"{1}\"", reader.Name, reader.Value);
                                                }
                                                reader.MoveToElement();
                                            }
                                            sb.Append(">");
                                            break;
                                        case XmlNodeType.Text:
                                            sb.AppendFormat(System.Security.SecurityElement.Escape(reader.Value));
                                            break;
                                        case XmlNodeType.CDATA:
                                            sb.AppendFormat("<![CDATA[{0}]]>", reader.Value);
                                            break;
                                        case XmlNodeType.Comment:
                                            sb.AppendFormat("<!--{0}-->", reader.Value);
                                            break;
                                        case XmlNodeType.XmlDeclaration:
                                            sb.AppendFormat("<?xml version='1.0'?>");
                                            break;
                                        case XmlNodeType.EndElement:
                                            sb.AppendFormat("</{0}>", reader.Name);
                                            break;
                                        default:
                                            sb.AppendFormat(reader.Value);
                                            break;
                                    }
                                }
                                else
                                {
                                    if (reader.NodeType != XmlNodeType.EndElement) isImageData = !isImageData;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("RemoveImageDataNode error {0}", ex.Message);
            }
            finally
            {
                message.Dispose();
            }
            return sb.ToString();
        }

        static public string SubscribeUploadFiles(XmlDocument xmlDoc, string subscriptionType, string sender, string recipient, string eHubTrackingID)
        {
            var sqlSB = new StringBuilder("");
            var a = xmlDoc.SelectNodes("//*[local-name()='FileUploadResponse']/*[local-name()='Files']/*[local-name()='File']/*[local-name()='Reference']");
            foreach (XmlNode node in xmlDoc.SelectNodes("//*[local-name()='FileUploadResponse']/*[local-name()='Files']/*[local-name()='File']/*[local-name()='Reference']"))
            {
                sqlSB.Append($@"<ns0:InsertSubscriptionValue xmlns:ns0='http://schemas.microsoft.com/Sql/2008/05/Procedures/dbo'>
    <ns0:ST_PK>{subscriptionType}</ns0:ST_PK>
    <ns0:senderId>{sender}</ns0:senderId>
    <ns0:recipientId>{recipient}</ns0:recipientId>
    <ns0:value>{node.InnerText}</ns0:value>
    <ns0:reference>{eHubTrackingID}</ns0:reference>
</ns0:InsertSubscriptionValue>
");
            }

            return sqlSB.ToString();
        }

        static public string EscapeXML(string xml, ILog logger)
        {
            try
            {
                return eHub.Core.Orchestrations.Helper.OrchestrationHelper.RemoveXmlDeclaration(
                           eHub.Core.Orchestrations.Helper.OrchestrationHelper.RemoveInvalidCharactersFromXmlContent(xml)
                       );
            }
            catch
            {
                logger.Warn("[EscapeXML] - Could not escape XML");
                return "";
            }
        }

		static public string GetQueryUrl(QueryType queryType, string baseURL, string entryNumberType, string entryNumber, string queryString)
		{
			switch (queryType)
			{
				case QueryType.Status:
					return string.Format(baseURL, entryNumberType.ToLower(), Uri.EscapeDataString(entryNumber), queryType.ToStringValue());
				case QueryType.List:
					return $"{baseURL}/search?{queryString}";
				case QueryType.VAT:
					return $"{baseURL}/{entryNumber}";
				default:
					return baseURL;
			}
		}

		static public string GetQueryMethod(QueryType queryType)
		{
			switch (queryType)
			{
				case QueryType.Status:
				case QueryType.List:
				case QueryType.VAT:
					return "GET";
				default:
					return "POST";
			}
		}

		static public string GetQueryContentType(QueryType queryType)
		{
			switch (queryType)
			{
				case QueryType.Status:
				case QueryType.List:
				case QueryType.VAT:
					return "application/xml; charset=UTF-8";
				default:
					return "application/json";
			}
		}

		static public string GetQueryAcceptHeader(QueryType queryType, string version)
		{
			string content;
			
			switch (queryType)
			{
				case QueryType.Status:
				case QueryType.List:
				case QueryType.VAT:
					content = "xml";
					break;
				default:
					content = "json";
					break;
			}

			return $"application/vnd.hmrc.{version}+{content}";
		}

		static public string GetQueryBody(QueryType queryType, string payload)
		{
			switch (queryType)
			{
				case QueryType.Status:
				case QueryType.List:
				case QueryType.VAT:
					return "<body />";
				default:
					return payload;
			}
		}

		static public string SetResponseMessage(QueryType queryType, string responseString, string eHubTrackingID, string jobNumber, string organization, string entryNumberType, string entryNumber)
		{
			return $@"<UniversalInterchange xmlns='http://www.cargowise.com/Schemas/Universal/2011/11'>
  <Header>
   	<SenderID/>
   	<RecipientID/>
  </Header>
  <Body>
    <UniversalEvent xmlns='http://www.cargowise.com/Schemas/Universal/2012/11'>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>{(queryType.ExistsInList(new List<QueryType>() { QueryType.List, QueryType.Status }) ? jobNumber : organization)}</Key>
              <Type>{(queryType.ExistsInList(new List<QueryType>() {QueryType.List, QueryType.Status}) ? "CustomsDeclaration" : "Organization")}</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      {(queryType.ExistsInList(new List<QueryType>() { QueryType.VAT, QueryType.NOP, QueryType.EORI })
      ? $@"
        <DataSource>
           <DataProvider>UkHmrc{queryType.ToStringValue()}</DataProvider>
        </DataSource>"
      : string.Empty)
      }
        <EventTime>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}</EventTime>
        <EventType>SVR</EventType>
        <ContextCollection>
        {(queryType.ExistsInList(new List<QueryType>() { QueryType.VAT, QueryType.NOP, QueryType.EORI })
	    ? $@"
          <Context>
            <Type>ResponseType</Type>
            <Value>UkHmrc{queryType.ToStringValue()}</Value>
          </Context>"
	    : string.Empty)
	    }
        {(queryType.ExistsInList(new List<QueryType>() { QueryType.Status })
		? $@"
          <Context>
            <Type>EntryNumberType</Type>
            <Value>{entryNumberType}</Value>
          </Context>
          <Context>
            <Type>EntryNumber</Type>
            <Value>{entryNumber}</Value>
          </Context>"
		: string.Empty)
		}
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{eHubTrackingID}</Value>
          </Context>
          <Context>
            <Type>ResponseText</Type>
            <Value>{responseString}</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";
		}
	}
}



