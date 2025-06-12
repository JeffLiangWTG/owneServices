using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ITCustoms.Configuration;
using Common.Logging;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.ITCustoms.Helpers
{
	public class OrchestrationHelper
	{
		public static TimeSpan GetSendMessageBatchTimeSpan(string receipient)
		{
			using (var context = GetContext())
			{
				var codeMapValue = context.eHubCodeMapValues
					.Single(x => x.eHubCodeMapKey.eHubCodeSet.eHubTransformationSet.TS_Name == "ITCustoms Outbound Message Batch Time Limit" &&
					x.eHubCodeMapKey.eHubCodeSet.eHubClient_Recipient.CC_ID == receipient &&
											x.eHubCodeSetResult.CR_Name == "Batch Time(Seconds)");

				if (string.IsNullOrWhiteSpace(codeMapValue.CV_OutputCode))
					throw new InvalidOperationException("Could not find ITCustoms outbound message batch time limit.");
				else
				{
					int seconds;
					if (!int.TryParse(codeMapValue.CV_OutputCode, out seconds))
						throw new InvalidOperationException("ITCustoms outbound message batch time limit is not a valid number: " +
																								codeMapValue);
					return new TimeSpan(0, 0, seconds);
				}
			}
		}

		public static XmlDocument GeneratePutRequest(string message, string senderID, string userId, string node, string fileName, string fileExtension, ILog logger)
		{
			var base64Content = Convert.ToBase64String(CertificateHelper.CreateSignedEncryptedMessage(message, senderID, userId, node, logger));

			if (logger.IsDebugEnabled)
				logger.Debug("Encrypted Content:\r\n" + base64Content + "\r\n");

			var xml = new System.Xml.XmlDocument();
			fileName = fileName + "." + fileExtension;
			var xmlString = string.Format(
@"<ns0:put xmlns:ns0='http://webservices.ftp.telematico.dogana.dogane.ag_dogane.finanze.it'>
	<fileMsg_Content>{0}</fileMsg_Content>
	<fileMsg_Nome>{1}</fileMsg_Nome>
</ns0:put>", base64Content, fileName);
			xml.LoadXml(xmlString);
			return xml;
		}

		public static bool ShouldProceedOnWebException(SoapException ex, Account account, ILog logger)
		{
			if (ex.Message.Contains("The operation has timed out"))
			{
				return true;
			}
			return false;
		}

		public static string GetXpathValue(string xmlString, string xpath)
		{
			var collector = new XPathValueCollector(xpath);
			var collections = collector.Collect(WebUtility.HtmlDecode(xmlString));

			return collections.Count == 0 ? null : collections.ToArray()[0].Item3;
		}


		public static void InsertOrUpdateXmlElement(XmlDocument xmlDoc, string elementName, string value)
		{
			var xpath = $"//*[local-name()='{elementName}']";
			if (xmlDoc.SelectSingleNode(xpath) == null)
			{
				var tempXElement = xmlDoc.CreateElement(elementName);
				tempXElement.InnerText = value;
				xmlDoc.DocumentElement.PrependChild(tempXElement);
			}
			else
			{
				var tempXElement = xmlDoc.SelectSingleNode(xpath);
				tempXElement.InnerText = value;
			}
		}

		public static void CheckForLAndQIndicators(string RFileConent, out bool LIsExpected, out bool QIsExpected)
		{
			LIsExpected = QIsExpected = false;

			var lines = RFileConent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			for (var i = 1; i < lines.Length; i++)
			{
				if (lines[i].StartsWith("T"))
				{
					var elements = lines[i].Split('\t');
					if (elements.Length >= 4)
					{
						LIsExpected = CheckIRILDES(elements);
						QIsExpected = CheckIVISTO(elements);
					}

					if (LIsExpected && QIsExpected)
						break;
				}
			}
		}

		public static int GetCountOfTETRowsByRules(string ITDeclarationContentDecoded, CheckRules checkRules)
		{
			var resultCnt = 0;
			var lines = ITDeclarationContentDecoded.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			for (var i = 1; i < lines.Length; i++)
			{
				if (lines[i].StartsWith("T"))
				{
					var elements = lines[i].Split('\t');
					if (elements.Length >= 4)
					{
						//L is expected if the 4th element is IN (“T1”, “T2”, “T2F”, “T2SM”.).
						if (checkRules(elements))
						{
							resultCnt += 1;
						}
					}
				}
			}
			return resultCnt;
		}

		public static int GetCountOfTETRowsFromIRILDES(string ITDeclarationContentDecoded)
		{
			return GetCountOfTETRowsByRules(ITDeclarationContentDecoded, CheckIRILDES);
		}

		public static int GetCountOfTETRowsFromIVISTO(string ITDeclarationContentDecoded)
		{
			return GetCountOfTETRowsByRules(ITDeclarationContentDecoded, CheckIVISTO);
		}

		public delegate bool CheckRules(string[] elements);

		public static bool CheckIRILDES(string[] elements)
		{
			//L is expected if the 4th element is IN (“T1”, “T2”, “T2F”, “T2SM”.).
			return LFileIndicators.Contains(elements[3]);
		}

		public static bool CheckIVISTO(string[] elements)
		{
			// Q is expected if the 2nd element is not empty.
			return !string.IsNullOrWhiteSpace(elements[1]);
		}

		public static int GetCountOfLines(string content)
		{
			return content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Count();
		}

		public static int GetCountOfFinalStatusForIRILDES(string IRILDESContent)
		{
			var lines = IRILDESContent.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			var linesSegments = new ArraySegment<String>(lines);
			if (linesSegments.Count() <= 1)
			{
				throw new FatalMessageProcessingException("Invalid IRILDES FILE CONTENT");
			}
			var contentLines = lines.Skip(1).ToArray();
			return (from line in contentLines where CheckIfFinalStatusOfLineForL(line) select line).Count();
		}

		public static int GetCountOfFinalStatusForIVISTO(string IVISTOContent)
		{
			var lines = IVISTOContent.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			var linesSegments = new ArraySegment<String>(lines);
			if (linesSegments.Count() <= 1)
			{
				throw new FatalMessageProcessingException("Invalid IRILDES FILE CONTENT");
			}
			var contentLines = lines.Skip(1).ToArray();
			return (from line in contentLines where CheckIfFinalStatusOfLineForQ(line) select line).Count();
		}

		public static bool CheckIfFinalStatusOfLineForL(string line)
		{
			return line.Contains("TIE45");
		}

		public static bool CheckIfFinalStatusOfLineForQ(string line)
		{
			return line.Contains("TIVISTO");
		}

		internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();
		public const string ApproachingLimiteWarning = "WarningApproachingMaxFile";
		public const string FileLimitReachedError = "ErrorMaxFileReached";
		public const string InvalidAccountError = "ErrorInvalidAccount";
		public const string InvalidSubscriberError = "ErrorInvalidSubscriber";
		private static string[] LFileIndicators = { "T1", "T2", "T2F", "T2SM" };
	}
}
