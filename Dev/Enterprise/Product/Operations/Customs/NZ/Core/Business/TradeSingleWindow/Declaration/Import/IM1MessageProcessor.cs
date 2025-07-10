using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.DocumentScanning.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	internal class IM1MessageProcessor : DeclarationMessageProcessor<IM1Response>
	{
		public IM1MessageProcessor(LoggingInformation logger)
			: base(logger, "Import")
		{
		}

		#region Overrides

		protected override bool IsWarningAboutTotalAmountInconsistencyRequired
		{
			get { return true; }
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendImpedimentsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendImpediments.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return NZCustomsDataRegistry.Instance.ImportDeclarationsSendErrors.Value; }
		}

		protected override DeclarationResponse PreviousResponsesWithSameMessageType
		{
			get
			{
				if (fPreviousResponsesWithSameMessageType == null)
				{
					var responseMessages = EntryHeader.Messages.OfType<TSWMessage>()
											.Where(message => message.EM_ReceiveTransmit == Enterprise.Messaging.Business.EDIInterchange.Direction.Receive && message != Response.IncomingTSWMessage)
											.OrderByDescending(message => message.EM_SystemCreateTimeUtc);

					var lastestMessageWithSameSubType = responseMessages.FirstOrDefault(message => message.EM_MessageSubType == Response.IncomingTSWMessage.EM_MessageSubType);
					if (lastestMessageWithSameSubType != null)
					{
						BaseTSWResponse baseTSWResponse = null;
						if (BaseTSWResponse.TryParse(lastestMessageWithSameSubType, out baseTSWResponse))
						{
							fPreviousResponsesWithSameMessageType = new IM1Response(baseTSWResponse);
						}
					}
				}

				return fPreviousResponsesWithSameMessageType;
			}
		}
		DeclarationResponse fPreviousResponsesWithSameMessageType;

		protected override void ProcessAttachedDocuments(EDIMessage message, StorageDocsBase[] interchangeDocs)
		{
			PopulateConsignmentNumberFromBACCDoc(interchangeDocs);
			base.ProcessAttachedDocuments(message, interchangeDocs);
		}

		void PopulateConsignmentNumberFromBACCDoc(StorageDocsBase[] interchangeDocs)
		{
			if (EntryHeader?.Declaration is IHaveNZAddInfo declaration)
			{
				var baccDoc = interchangeDocs?.FirstOrDefault(x => Regex.IsMatch(x.SC_FileName, "^BACC.*_xml$"));
				if (baccDoc != null)
				{
					var baccData = baccDoc.SC_ImageData;

					var (errorMessage, consignmentNumber) = GetConsignmentNumberFromBACCDoc(Encoding.Unicode.GetString(baccData));
					if (!errorMessage.IsEmpty)
					{
						var baccUTF16Error = errorMessage;

						var baccUTF8 = Encoding.UTF8.GetString(baccData);
						(errorMessage, consignmentNumber) = GetConsignmentNumberFromBACCDoc(baccUTF8);
						if (!errorMessage.IsEmpty)
						{
							var fileContent = baccUTF8.StartsWith("ERROR", ignoreCase: true, CultureInfo.InvariantCulture) ? baccUTF8 : Convert.ToBase64String(baccData);
							string loggerMessage = $"Failed to read ConsignmentNumber from the BACC XML file '{baccDoc.SC_FileName}' with encoding UTF16 => {baccUTF16Error}  UTF8 => {errorMessage}\r\nFile Contents: {fileContent}";

							Logger.LogError(loggerMessage);
						}
					}

					if (!consignmentNumber.IsEmpty)
					{
						declaration.AddInfo.ZN_MAF_ConsignmentNumber = consignmentNumber;
					}
				}
			}
		}

		(ZString ErrorMessage, ZString ConsignmentNumber) GetConsignmentNumberFromBACCDoc(string xml)
		{
			const string ConsignNumXPath = "/table/d_bacc_header_rept/d_bacc_header_rept_row/consign_num";
			var errorMessage = ZString.Empty;
			ZString consignmentNumber;

			try
			{
				var document = new XmlDocument();
				document.LoadXml(xml);
				consignmentNumber = document.SelectSingleNode(ConsignNumXPath)?.InnerText;
			}
			catch (Exception e) when (e is XmlException || e is XPathException)
			{
				consignmentNumber = FindConsignmentNumber(xml);
				if (consignmentNumber.IsEmpty)
				{
					errorMessage = e.Message;
				}
			}

			return (errorMessage, consignmentNumber);
		}

		ZString FindConsignmentNumber(ZString xml)
		{
			const int tagSize = 13;     // <consign_num>
			const int tagCloseSize = 2; // </
			var consignmentNumber = ZString.Empty;

			var match = Regex.Match(xml, @"<consign_num>[\w/]+</");
			if (match.Success)
			{
				var taggedValue = (ZString)match.Value;
				consignmentNumber = taggedValue.SubstringSafe(tagSize, taggedValue.Length - tagSize - tagCloseSize);
			}

			return consignmentNumber;
		}

		#endregion // Overrides

		protected ZGuid UnsolicitedDOGroup
		{
			get { return NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.Value; }
		}

		protected ZString UnsolicitedDOMode
		{
			get { return NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.Value; }
		}
	}
}
