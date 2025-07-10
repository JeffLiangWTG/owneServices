using System;
using System.Xml;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader
{
	public class GMDMessageCreator : MessageCreator
	{
		protected override SourceData GetSourceDataFromStreamTextMain(string streamText, SourceData data)
		{
			var xmlDoc = new XmlDocument();
			var isSuccess = true;
			xmlDoc.LoadXml(streamText);
			var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace("ns0", "http://cargowise.com/ehub/core/genericmessagedelivery");
			var interchangeType = xmlDoc.SelectSingleNode("ns0:GenericMessageInterchange/Header/InterchangeType", nsmgr)?.InnerText ?? string.Empty;
			if (interchangeType == Constants.RefDataRepoMessage)
			{
				var xmlRefDataRepoMessage = xmlDoc.SelectSingleNode("ns0:GenericMessageInterchange/Body/RefDbRepoMessage", nsmgr);
				if (xmlRefDataRepoMessage != null)
				{
					var sourceNode = xmlRefDataRepoMessage.SelectSingleNode("Source", nsmgr);
					if (sourceNode != null && !string.IsNullOrWhiteSpace(sourceNode.InnerText))
					{
						data.SDA_Source = sourceNode.InnerText.ToUpperInvariant();
					}
					else
					{
						ReportError("Source");
						isSuccess = false;
					}
					var subSourceNode = xmlRefDataRepoMessage.SelectSingleNode("SubSource", nsmgr);
					if (subSourceNode != null && !string.IsNullOrWhiteSpace(subSourceNode.InnerText))
					{
						data.SDA_SubSource = subSourceNode.InnerText.ToUpperInvariant();
					}
					else
					{
						ReportError("SubSource");
						isSuccess = false;
					}
					var contentTypeNode = xmlRefDataRepoMessage.SelectSingleNode("ContentType", nsmgr);
					if (contentTypeNode != null && !string.IsNullOrWhiteSpace(contentTypeNode.InnerText))
					{
						data.SDA_ContentType = contentTypeNode.InnerText.ToUpperInvariant();
					}
					else
					{
						ReportError("ContentType");
						isSuccess = false;
					}
					var textNode = xmlRefDataRepoMessage.SelectSingleNode("Data", nsmgr);
					if (isSuccess && textNode != null && !string.IsNullOrWhiteSpace(textNode.InnerText))
					{
						data.SDA_ContentText = textNode.InnerText;
					}
					else
					{
						ReportError("Data");
						isSuccess = false;
					}
					if (isSuccess)
					{
						data.SDA_SourceTime = DateTime.UtcNow;
						data.SDA_Status = StatusProvider.GetQUEStatus();
					}
				}
				else
				{
					ReportError("RefDbRepoMessage");
				}
			}
			else
			{
				ReportError("InterchangeType");
			}

			return data;
		}

		static void ReportError(string nodeName)
		{
			Console.Error.WriteLine($"{nodeName} node not found in the eHubMessage");
		}
	}
}
