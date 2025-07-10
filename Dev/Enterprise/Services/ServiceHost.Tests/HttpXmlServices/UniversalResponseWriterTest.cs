using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using static NUnit.Framework.XmlAssertions;

namespace Enterprise.Services.ServiceHost.Test
{
	class UniversalResponseWriterTest : TransactionedTestCase
	{
		public void TestWriteUniversalResponse()
		{
			TestWriteUniversalResponse("肥胖的狗");
		}

		public void TestWriteUniversalResponse_OverBuffer()
		{
			var value = string.Empty;
			for (var i = 1; i < 2048; i++)
			{
				value += (char)i;
			}
			TestWriteUniversalResponse(RemoveInvalidXMLChars(value));
		}

		readonly Regex invalidXMLChars = new Regex(
			@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
			RegexOptions.Compiled);

		/// <summary>
		/// removes any unusual unicode characters that can't be encoded into XML
		/// </summary>
		public string RemoveInvalidXMLChars(string text)
		{
			return invalidXMLChars.Replace(text, "?");
		}

		public void TestWriteUniversalResponse(string value)
		{
			using (var logStream = new MemoryStream())
			using (var dataStream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				value = value
					.Replace("<", string.Empty)
					.Replace(">", string.Empty)
					.Replace("\r", string.Empty)
					.Replace("&", string.Empty)
					.Replace("'", string.Empty)
					.Replace("\"", string.Empty)
					.Replace("\n", string.Empty);
				var streamWriter = new StreamWriter(logStream);
				streamWriter.Write($"<Log{value}>");
				streamWriter.Flush();

				var dataWriter = new StreamWriter(dataStream);
				dataWriter.Write($"<ANode>node {value}</ANode>");
				dataWriter.Flush();
				dataStream.Position = 0;

				var messageNumbers = new TestMessageResult()
				{
					InterchangeNumber = "00001",
					TrackingID = ZGuid.NewZGuid(),
					MessageNumbers = new[] { (ZString)"00002", (ZString)"00003" },
					ExternalReferenceNumbers = new[] { (ZString)"00004", (ZString)"00005" }
				};

				using (var result = UniversalResponseWriter.CreateResponse("STS", dataStream, new StreamReader(logStream), messageNumbers))
				{
					result.Position = 0;

					var universalResponse = new StreamReader(result).ReadToEnd();
					var expectedResponse = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>STS</Status>
  <Data><ANode>node {value}</ANode></Data>
  <MessageNumberCollection>
    <MessageNumber Type=""TrackingID"">{messageNumbers.TrackingID}</MessageNumber>
    <MessageNumber Type=""InterchangeNumber"">00001</MessageNumber>
    <MessageNumber Type=""MessageNumber"">00002</MessageNumber>
    <MessageNumber Type=""MessageNumber"">00003</MessageNumber>
    <MessageNumber Type=""External"">00004</MessageNumber>
    <MessageNumber Type=""External"">00005</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>&lt;Log{value}&gt;</ProcessingLog>
</UniversalResponse>";

					AssertEquals(expectedResponse, universalResponse.Replace("&amp;", "&"));
					AssertResponseMatchesSchema(universalResponse);
				}
			}
		}

		public void TestBasicResponse()
		{
			using (var result = UniversalResponseWriter.CreateResponse("STS", null, null))
			{
				result.Position = 0;
				var universalResponse = new StreamReader(result).ReadToEnd();
				var expectedResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>STS</Status>
  <Data />
  <ProcessingLog />
</UniversalResponse>";

				AssertXMLEquals(expectedResponse, universalResponse);
				AssertResponseMatchesSchema(universalResponse);
			}
		}

		void AssertResponseMatchesSchema(string universalResponse)
		{
			var schema = new XmlSchemaResourceLoader().ReadSchema(GetType().Assembly, "Enterprise.Services.ServiceHost.Tests.TestSchemas", "UniversalResponse.xsd");
			var notify = new NotificationBuffer();
			var builder = new XsdSchemaBuilder();
			builder.CompileSchema(schema);
			new XmlValidator(schema).Validate(universalResponse, notify, false);
			Assert(notify.AsString, !notify.HasErrors);
		}

		public void TestMultipleMessageNumberCollectionsInResponse()
		{
			var handlerResult = new TestMessageResult
			{
				InterchangeNumber = "00001",
				TrackingID = ZGuid.NewZGuid(),
				MessageNumbers = new[] { (ZString)"00002", (ZString)"00003" },
				ExternalReferenceNumbers = new[] { (ZString)"00004", (ZString)"00005" }
			};

			using (var result = UniversalResponseWriter.CreateResponse("STS", null, null, result: handlerResult, emitMessageNumberCollections: true))
			{
				result.Seek(0, SeekOrigin.Begin);
				var universalResponse = new StreamReader(result).ReadToEnd();

				AssertResponseMatchesSchema(universalResponse);
				AssertIsXml(universalResponse)
					.HavingExactlyOneChildNode("MessageNumberCollection", collection => collection
						.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "TrackingID").WithValue(handlerResult.TrackingID.ToString()))
						.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "InterchangeNumber").WithValue("00001"))
						.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "MessageNumber").WithValue("00002"))
						.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "External").WithValue("00004"))
						.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "MessageNumber").WithValue("00003"))
						.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "External").WithValue("00005"))
					)
					.HavingExactlyOneChildNode("MessageNumberCollections", collections =>
						collections
							.HavingAllChildNodes(collection => collection.WithName("MessageNumberCollection"))
							.HavingExactlyOneChildNode("MessageNumberCollection", collection =>
								collection
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "MessageNumber"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "External"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "MessageNumber").WithValue("00002"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "External").WithValue("00004"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "InterchangeNumber").WithValue("00001"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "TrackingID").WithValue(handlerResult.TrackingID.ToString()))
								)
							.HavingExactlyOneChildNode("MessageNumberCollection", collection =>
								collection
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "MessageNumber"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "External"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "MessageNumber").WithValue("00003"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "External").WithValue("00005"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "InterchangeNumber").WithValue("00001"))
									.HavingExactlyOneChildNode("MessageNumber", mn => mn.WithAttribute("Type", "TrackingID").WithValue(handlerResult.TrackingID.ToString()))
								)
							);
			}
		}
	}

	class TestMessageResult : IMessageHandlerResult
	{
		public NotificationBuffer Notifications => throw new System.NotImplementedException();

		public ZString InterchangeNumber { get; set; }

		public ZGuid TrackingID { get; set; }

		IEnumerable<ZString> _messageNumber;
		public IEnumerable<ZString> MessageNumbers
		{
			get
			{
				return _messageNumber ?? Enumerable.Empty<ZString>();
			}
			set
			{
				_messageNumber = value;
			}
		}

		IEnumerable<ZString> _externalReferenceNumbers;
		public IEnumerable<ZString> ExternalReferenceNumbers
		{
			get
			{
				return _externalReferenceNumbers ?? Enumerable.Empty<ZString>();
			}
			set
			{
				_externalReferenceNumbers = value;
			}
		}

		public ZString FailureReason { get; set; }

		IEnumerable<ZString> _warnings;
		public IEnumerable<ZString> Warnings
		{
			get
			{
				return _warnings ?? Enumerable.Empty<ZString>();
			}
			set
			{
				_warnings = value;
			}
		}
	}
}
