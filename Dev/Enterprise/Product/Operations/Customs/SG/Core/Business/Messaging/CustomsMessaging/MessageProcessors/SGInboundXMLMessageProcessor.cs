using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class SGInboundXMLMessageProcessor : ApplicationTypeMessageProcessor
	{
		public SGInboundXMLMessageProcessor(LoggingInformation logger, ZString applicationCode)
			: base(logger)
		{
			this.applicationCode = applicationCode;
		}

		readonly ZString applicationCode;

		protected override string MessageFriendlyNameCore => "SG Customs XML Message Processor";

		protected override string ApplicationCodeCore => applicationCode;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var tradenetResponse = Deserialize(message);

			var processors = GetMessageProcessors(tradenetResponse)
				.Where(c => c != null);

			if (processors.Any())
			{
				foreach (var processor in processors)
				{
					processor.ProcessMessage(message);
				}
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Failed;
				Logger.Log("Can't find a valid processor for this message.");
			}
		}

		IEnumerable<SGMessageProcessor> GetMessageProcessors(TradenetResponse tradenetResponse)
		{
			var outboundMessage = tradenetResponse?.OutboundMessage;

			if (outboundMessage != null)
			{
				yield return GetMessageProcessorCore<ApprovalMessageProcessor>(outboundMessage.ApprovalMessage);
				yield return GetMessageProcessorCore<ErrorMessageProcessor>(outboundMessage.ErrorMessage);
				yield return GetMessageProcessorCore<RejectionMessageProcessor>(outboundMessage.RejectionMessage);
				yield return GetMessageProcessorCore<FeeMessageProcessor>(outboundMessage.FeeMessage);
				yield return GetMessageProcessorCore<CustomsCommonCodeProcessor>(outboundMessage.CustomsCommonCode);
				yield return GetMessageProcessorCore<CustomsExchangeRateProcessor>(outboundMessage.CustomsExchangeRate);

				yield return GetMessageProcessorCore<InPaymentPermitProcessor>(outboundMessage.InPaymentPermit);
				yield return GetMessageProcessorCore<InNonPaymentPermitProcessor>(outboundMessage.InNonPaymentPermit);
				yield return GetMessageProcessorCore<OutwardPermitProcessor>(outboundMessage.OutwardPermit);
				yield return GetMessageProcessorCore<TranshipmentMovementPermitProcessor>(outboundMessage.TranshipmentMovementPermit);

				yield return GetMessageProcessorCore<InPaymentUpdatePermitProcessor>(outboundMessage.InPaymentUpdatePermit);
				yield return GetMessageProcessorCore<InNonPaymentUpdatePermitProcessor>(outboundMessage.InNonPaymentUpdatePermit);
				yield return GetMessageProcessorCore<OutwardUpdatePermitProcessor>(outboundMessage.OutwardUpdatePermit);
				yield return GetMessageProcessorCore<TranshipmentMovementUpdatePermitProcessor>(outboundMessage.TranshipmentMovementUpdatePermit);

				yield return GetMessageProcessorCore<CertificateOfOriginApprovalProcessor>(outboundMessage.CertificateOfOriginApproval);
			}
		}

		T GetMessageProcessorCore<T>(ITradeNetOutSection section) where T : SGMessageProcessor
		{
			return section != null ? Activator.CreateInstance(typeof(T), Logger, section) as T : null;
		}

		TradenetResponse Deserialize(EDIMessage message)
		{
			try
			{
				using (var stream = message.GetEM_MessageTextReader())
				{
					var serializer = ZXmlSerializer.New(typeof(TradenetResponse));
					return serializer.Deserialize(stream) as TradenetResponse;
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Logger.Log("Invalid XML Format.");
				message.EM_Status = EDIMessage.Status.Failed;

				return null;
			}
		}
	}
}
