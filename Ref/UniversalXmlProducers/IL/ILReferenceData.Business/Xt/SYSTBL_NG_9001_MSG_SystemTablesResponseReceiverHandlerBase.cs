using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerBase : BaseReceiveHandlerProcessor
	{
		public SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerBase(Common.Utils.IDateTimeProvider dateTimeProvider, ILogger logger) : base(logger)
		{
			this.dateTimeProvider = dateTimeProvider;
			this.logger = logger;
		}

		protected override string PreProcessResponseCore(Stream response)
		{
			response.Seek(0, SeekOrigin.Begin);
			var xdoc = Helpers.GetRawBodyFromSoapEnvelop(response);
			if (xdoc != null)
			{
				XNamespace ns = "http://malam.com/customs/SystemTables/SYSTBL_NG_9001_MSG_SystemTablesResponse";

				var tableDataElement = xdoc?.Element(ns + "TableAsDataSetTableData");
				if (tableDataElement != null)
				{
					foreach (var node in tableDataElement.Descendants())
					{
						node.Name = node.Name.LocalName;
					}

					string innerXml = string.Concat(tableDataElement.Nodes());
					tableDataElement.RemoveNodes();
					tableDataElement.Value = innerXml;
				}

				return xdoc.ToString();
			}

			response.Seek(0, SeekOrigin.Begin);
			using (var reader = new StreamReader(response))
			{
				return reader.ReadToEnd();
			}
		}

		protected override (bool Success, string ErrorMessage) ProcessResponseCore(string response)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(SYSTBL_NG_9001_MSG_SystemTablesResponse));
			using (var stringReader = new StringReader(response))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				var responseObject = (SYSTBL_NG_9001_MSG_SystemTablesResponse)xmlSerializer.Deserialize(xmlReader);

				var processor = CustomsResponseProcessorProvider.GetCustomsResponseProcessor(responseObject.tableName, logger);

				if (string.IsNullOrEmpty(responseObject.TableAsDataSetTableData))
				{
					return (false, Constants.ReceiveHandlerProcessor.UnexpectedDataSetResponse);
				}

				processor.GenerateFiles(
					responseObject.lastmodifiedDateSpecified ? responseObject.lastmodifiedDate.Value : dateTimeProvider.GetUTCNow(),
					responseObject.tableName, responseObject.TableAsDataSetTableData);
			}

			return (true, string.Empty);
		}

		readonly Common.Utils.IDateTimeProvider dateTimeProvider;
		readonly ILogger logger;
	}
}
