using System;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	sealed class EBondInboundInterchangeProcessor : BaseInboundInterchangeProcessor
	{
		public EBondInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes
		{
			get { return new string[] { CBPEDIInterchange.ApplicationCodes.USeBond }; }
		}

		protected sealed override bool ProcessInterchange(EDIInterchange interchange)
		{
			var tracker = new XmlSessionTracker(Logger);

			IInboundMessageCreator messageCreator = new EBondInboundMessageCreator(tracker);
			messageCreator.CreateMessagesForInterchange(interchange);

			return true;
		}

		#region EBondInboundMessageCreator

		sealed class EBondInboundMessageCreator : IInboundMessageCreator
		{
			public EBondInboundMessageCreator(XmlSessionTracker logger)
			{
				this.logger = Argument.NotNull(logger, nameof(logger));
			}

			readonly XmlSessionTracker logger;

			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				try
				{
					using (var stream = GetMessageStream(interchange))
					using (var reader = new XPathReader(new XmlTextReader(stream), GetPayloadTypeNodes()))
					{
						ReadMessages(reader, interchange);
					}
				}
				catch (Exception exception) when (!(exception.IsCriticalException()) && !exception.IsOutOfDiskSpaceException() && !exception.IsUnableToCreateTempFileException())
				{
					var message = Res.GetString("EFE5FC8B-2AEF-48BE-BAC7-40C0EF43EAEE", "There is an error when the system is process {0}. Message: {1}", interchange.EI_InterchangeNum, exception.Message);
					logger.LogErrorToServiceTaskOnly(message);

					interchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;

					foreach (var ediMessage in interchange.ContainedMessages.Cast<EBondEDIMessage>())
					{
						ediMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
					}
				}
			}

			void ReadMessages(XPathReader reader, EDIInterchange interchange)
			{
				var factory = interchange.Factory;

				while (reader.ReadUntilMatch())
				{
					var message = factory.New<EBondEDIMessage>();
					message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
					message.EM_Status = EDIInterchangeStatusList.Codes.Queued;
					message.EM_EI = interchange.PK;

					message.SetEM_MessageTextOrDataSource(LargeMessageHelper.GetStreamFromNode(reader));
				}
			}

			#region Implement

			XPathCollection GetPayloadTypeNodes()
			{
				if (payloadTypeNodes == null)
				{
					payloadTypeNodes = new XPathCollection();
					payloadTypeNodes.Add("/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']");
				}
				return payloadTypeNodes;
			}
			[ThreadStatic]
			static XPathCollection payloadTypeNodes;

			Stream GetMessageStream(EDIInterchange interchange)
			{
				const int bufferSize = 32000;

				var stream = new VirtualMemoryStream();
				var writer = new StreamWriter(stream);

				using (var reader = interchange.GetEI_BodyTextReader())
				{
					CheckIfItIsXmlDocumentAndDeclarationDoesNotExistsThanAdd(reader, writer);

					var buffer = new char[bufferSize];
					int position;
					while ((position = reader.Read(buffer, 0, buffer.Length)) > 0)
					{
						writer.Write(buffer, 0, position);
					}
				}

				writer.Flush();
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}

			static void CheckIfItIsXmlDocumentAndDeclarationDoesNotExistsThanAdd(TextReader reader, StreamWriter writer)
			{
				const int declarationCheckSize = 5;

				var buffer = new char[declarationCheckSize];
				var position = reader.Read(buffer, 0, declarationCheckSize);
				var first5Chars = new string(buffer, 0, position);

				if (first5Chars.Length > 0 && first5Chars.TrimStart()[0] == '<' && !string.Equals(first5Chars, XmlDeclaration.Substring(0, 5), StringComparison.CurrentCultureIgnoreCase))
				{
					writer.Write(XmlDeclaration);
				}

				writer.Write(buffer, 0, position);
			}

			const string XmlDeclaration = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

			#endregion
		}

		#endregion
	}
}
