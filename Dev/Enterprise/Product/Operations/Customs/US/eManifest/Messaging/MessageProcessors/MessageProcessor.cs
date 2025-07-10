using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	public class MessageProcessor : BaseMessageProcessor<Business.EDIMessage>
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new EDIFACTMessageProcessor(Logger));
			return result;
		}

		class EDIFACTMessageProcessor : Customs.Business.MessageProcessors.EDIFACTMessageProcessor
		{
			public EDIFACTMessageProcessor(LoggingInformation logger)
				: base(logger)
			{
			}

			#region Overrides of ApplicationTypeMessageProcessor

			protected override string MessageFriendlyNameCore
			{
				get { return "e-Manifest Response"; }
			}

			protected override string ApplicationCodeCore
			{
				get { return EDIMessage.ApplicationCodes.USeManifest; }
			}

			#endregion

			#region Overrides of EDIFACTMessageProcessor

			protected override CustomsMessageProcessor GetMessageProcessor(EDIMessage ediMessage)
			{
				return ediMessage.EM_MessageText.Contains("CONTRL") ? new SyntaxAndServiceReportMessageProcessor(Logger)
						: ediMessage.EM_MessageText.Contains("MEDPID") ? new CrewOrEquipmentRegistrationMessageProcessor(Logger)
										: new eManifestResponseMessageProcessor(Logger);
			}

			#endregion
		}
	}
}
