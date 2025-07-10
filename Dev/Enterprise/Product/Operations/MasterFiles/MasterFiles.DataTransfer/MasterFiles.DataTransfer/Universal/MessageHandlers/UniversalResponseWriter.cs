using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.IO;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer
{
	public static class UniversalResponseWriter
	{
		public static SubStreamableStream CreateResponse(string status, SubStreamableStream dataStream, TextReader loggingStream, IMessageHandlerResult result = null, IEnumerable<IValidationRule> validationRuleCollection = null, bool emitMessageNumberCollections = false)
		{
			var universalResponse = new CargoWise.IO.Shim.SubStreamableStream();
			var xmlWriter = XmlWriter.Create(universalResponse, new XmlWriterSettings() { Indent = true, Encoding = new UTF8Encoding(false) });
			var schema = SchemaVersionManager.Current;

			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("UniversalResponse", schema.Namespace);
			xmlWriter.WriteAttributeString("version", schema.Version);

			xmlWriter.WriteElementString("Status", status);

			WriteValidationRules(xmlWriter, validationRuleCollection);

			if (dataStream != null && dataStream.Length > 0)
			{
				// Data stream could be large so we avoid the copy
				xmlWriter.Flush();
				var startPos = universalResponse.Position;

				xmlWriter.WriteElementString("Data", "♣");
				xmlWriter.Flush();

				// We want to insert the dataStream where the ♣ is.
				var endPos = universalResponse.Position;

				universalResponse.Position = startPos;
				var reader = new PositionRecordingStreamReader(universalResponse);

				while (reader.Read() != '♣')
				{
				}

				using (var tail = universalResponse.GetSubStream(reader.StreamPosition, endPos - reader.StreamPosition))
				{
					universalResponse.SetLength(reader.StreamPosition - Encoding.UTF8.GetByteCount(new char[] { '♣' }));
					universalResponse.Seek(0, SeekOrigin.End);

					var buffer = new byte[3];
					var bom = new byte[] { 0xEF, 0xBB, 0xBF };
					dataStream.Seek(0, SeekOrigin.Begin);
					var readCount = dataStream.Read(buffer, 0, 3);

					if (readCount == 3 &&
						buffer[0] == bom[0] &&
						buffer[1] == bom[1] &&
						buffer[2] == bom[2])
					{
						using (var subStream = dataStream.GetSubStream(3, dataStream.Length - 3))
						{
							universalResponse.AppendSubStream(subStream);
						}
					}
					else
					{
						universalResponse.AppendSubStream(dataStream);
					}

					universalResponse.Seek(0, SeekOrigin.End);
					universalResponse.AppendSubStream(tail);
					universalResponse.Seek(0, SeekOrigin.End);
				}
			}
			else
			{
				xmlWriter.WriteStartElement("Data");
				xmlWriter.WriteEndElement();
			}

			WriteMessageNumbers(xmlWriter, result, emitMessageNumberCollections);

			xmlWriter.WriteStartElement("ProcessingLog");

			if (loggingStream != null)
			{
				CopyTo(loggingStream, xmlWriter);
			}

			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();

			return universalResponse;
		}

		static void CopyTo(TextReader reader, XmlWriter writer)
		{
			var streamReader = reader as StreamReader;
			if (streamReader?.BaseStream != null)
			{
				streamReader.BaseStream.Position = 0;
			}
			var buffer = new char[2048];
			int length;
			do
			{
				length = reader.Read(buffer, 0, buffer.Length);
				if (length > 0 && buffer.Length >= length)
				{
					writer.WriteValue(new string(buffer, 0, length));
				}
			} while (length > 0);
		}

		static void WriteValidationRules(XmlWriter xmlWriter, IEnumerable<IValidationRule> validationRules)
		{
			if (validationRules != null)
			{
				xmlWriter.WriteStartElement("ValidationRuleCollection");

				foreach (var rule in validationRules)
				{
					WriteValidationRule(xmlWriter, rule);
				}

				xmlWriter.WriteEndElement();
			}
		}

		static void WriteValidationRule(XmlWriter xmlWriter, IValidationRule validationRule)
		{
			xmlWriter.WriteStartElement("ValidationRule");
			xmlWriter.WriteElementString("Code", validationRule.Code);
			xmlWriter.WriteElementString("Sequence", validationRule.Sequence.ToString());
			xmlWriter.WriteElementString("MessageLog", validationRule.MessageLog);
			xmlWriter.WriteElementString("Result", validationRule.Result);
			xmlWriter.WriteEndElement();
		}

		static void WriteMessageNumbers(XmlWriter xmlWriter, IMessageHandlerResult result, bool emitMessageNumberCollections)
		{
			if (result == null)
			{
				return;
			}

			if (!result.MessageNumbers.Any())
			{
				return;
			}

			if (emitMessageNumberCollections)
			{
				xmlWriter.WriteStartElement("MessageNumberCollections");

				foreach (var pair in result.MessageNumbers.Zip(result.ExternalReferenceNumbers, (messageNum, externalRef) => new { MessageNumber = messageNum, ExternalReference = externalRef }))
				{
					xmlWriter.WriteStartElement("MessageNumberCollection");

					if (result.TrackingID.IsValid)
					{
						WriteMessageNumber(xmlWriter, result.TrackingID.ToString(), MessageNumberType.TrackingID);
					}

					WriteMessageNumber(xmlWriter, result.InterchangeNumber, MessageNumberType.InterchangeNumber);
					WriteMessageNumber(xmlWriter, pair.MessageNumber, MessageNumberType.MessageNumber);
					WriteMessageNumber(xmlWriter, pair.ExternalReference, MessageNumberType.External);

					xmlWriter.WriteEndElement();
				}

				xmlWriter.WriteEndElement();
			}

			xmlWriter.WriteStartElement("MessageNumberCollection");

			if (result.TrackingID.IsValid)
			{
				WriteMessageNumber(xmlWriter, result.TrackingID.ToString(), MessageNumberType.TrackingID);
			}

			if (result.InterchangeNumber.IsValid && !result.InterchangeNumber.IsEmpty)
			{
				WriteMessageNumber(xmlWriter, result.InterchangeNumber, MessageNumberType.InterchangeNumber);
			}

			foreach (var message in result.MessageNumbers)
			{
				WriteMessageNumber(xmlWriter, message, MessageNumberType.MessageNumber);
			}

			foreach (var externalReferenceNumber in result.ExternalReferenceNumbers)
			{
				WriteMessageNumber(xmlWriter, externalReferenceNumber, MessageNumberType.External);
			}

			xmlWriter.WriteEndElement();
		}

		static void WriteMessageNumber(XmlWriter xmlWriter, string number, MessageNumberType type)
		{
			if (!string.IsNullOrEmpty(number))
			{
				xmlWriter.WriteStartElement("MessageNumber");
				xmlWriter.WriteAttributeString("Type", type.ToString());
				xmlWriter.WriteValue(number);
				xmlWriter.WriteEndElement();
			}
		}
	}
}
