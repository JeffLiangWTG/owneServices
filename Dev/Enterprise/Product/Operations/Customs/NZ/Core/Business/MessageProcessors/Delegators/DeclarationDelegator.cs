using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageProcessors.FormalEntry;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	class DeclarationDelegator : IProcessorDelegator
	{
		#region IProcessorDelegator Members

		public string MessageFriendlyName
		{
			get { return "Declaration response"; }
		}

		public bool CanProcess(NZCMessage message)
		{
			var cusresMessage = message.MessageAsCUSRESD98A;
			ZString sendersReference = cusresMessage.UNH[0].CommonAccessReference;
			var declarationFilter = JobDeclarationFilter.ForDeclarationReference(true, sendersReference);
			declaration = message.Factory.LoadTop1<JobDeclaration>(declarationFilter);
			if (declaration == null)
			{
				var entryHeader = CusEntryHeader.LoadForBGMReference(message.Factory, sendersReference);
				if (entryHeader != null)
				{
					declaration = (JobDeclaration)entryHeader.Declaration;
				}
			}
			return declaration != null;
		}
		JobDeclaration declaration;

		public void Process(LoggingInformation logger, NZCMessage message)
		{
			var cusresMessage = message.MessageAsCUSRESD98A;
			ZString customsReference = cusresMessage.BGM[0].DocumentMessageIdentification.DocumentMessageNumber;
			CusEntryHeader entryHeader = declaration.CusEntryHeader;

			if (!entryHeader.EntryNumber.IsEmpty && (customsReference.IsEmpty || customsReference.IsNumbersOnlyOrEmpty))
			{
				if (entryHeader.EntryNumber != customsReference)
				{
					entryHeader = null;

					foreach (CusEntryHeader possiblyBetterEntryHeader in declaration.CustomsEntryHeaders)
					{
						if (possiblyBetterEntryHeader.EntryNumber == customsReference)
						{
							entryHeader = possiblyBetterEntryHeader;
							if (entryHeader.CH_IsActive)
							{
								break;
							}
						}
					}

					if (entryHeader == null)
					{
						var currentlyActiveEntry = declaration.CusEntryHeader;

						entryHeader = declaration.CustomsEntryHeaders.AddNew();
						entryHeader.CH_BGMReference = cusresMessage.UNH[0].CommonAccessReference;

						if (currentlyActiveEntry != null)
						{
							entryHeader.IsActive = false;
							currentlyActiveEntry.IsActive = true;
						}
					}
				}
			}

			entryHeader.Messages.Add(message);

			MessageProcessor messageProcessor = null;
			if (entryHeader.IsECIWriteOff)
			{
				messageProcessor = new ECIWriteOff.MessageProcessor(logger);
			}
			else
			{
				messageProcessor = new FormalEntry.MessageProcessor(logger);
			}

			logger.Log("Processing " + MessageFriendlyName + "...");
			message.EM_MessageType = messageProcessor.GetMessageTypeDelegate(message);
			message.EM_MessageSubType = messageProcessor.GetMessageTypeDelegate(message);

			messageProcessor.ProcessMessage(message);
		}

		#endregion

		#region Unsolicited Message Processing

		public bool CanProcessUnsolicitedMessage(NZCMessage message)
		{
			var result = false;
			var cusresMessage = message.MessageAsCUSRESD96B;
			if (cusresMessage != null)
			{
				ZString responseTypeCode = cusresMessage.BGM[0].DocumentMessageName.DocumentMessageNameCoded.ToString();
				ZString generalIndicatorCode = ZString.Empty;
				var gisSegment = cusresMessage.GIS[0];
				if (gisSegment != null)
				{
					var processingIndicatorCoded = gisSegment.ProcessingIndicator.ProcessingIndicatorCoded;
					if (processingIndicatorCoded != null)
					{
						generalIndicatorCode = processingIndicatorCoded.ToString();
					}
				}

				result = responseTypeCode == ResponseTypeList.Codes.DeliveryOrder && IsValidIndicatorForDeliveryOrder(generalIndicatorCode);
			}

			return result;
		}

		bool IsValidIndicatorForDeliveryOrder(ZString generalIndicatorCode)
		{
			return generalIndicatorCode == ResponseStatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified ||
				   generalIndicatorCode == ResponseStatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecifiedEntryRoutedToDocumentAudit ||
				   generalIndicatorCode == ResponseStatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecifiedPleaseNoteWarningsCorrectIfNecessary ||
				   generalIndicatorCode == ResponseStatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecified ||
				   generalIndicatorCode == ResponseStatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecifiedPleaseNoteWarningsCorrectIfNecessary ||
				   generalIndicatorCode == ResponseStatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecifiedEntryRoutedToDocumentAudit;
		}

		public void ProcessUnsolicitedDeliveryOrder(LoggingInformation logger, NZCMessage message)
		{
			UnsolicitedMessageProcessor messageProcessor = null;
			messageProcessor = new UnsolicitedMessageProcessor(logger);

			logger.Log("Processing Unsolicited Delivery Order: " + MessageFriendlyName + "...");
			message.EM_MessageType = messageProcessor.GetMessageTypeDelegate(message);
			message.EM_MessageSubType = messageProcessor.GetMessageTypeDelegate(message);
			messageProcessor.ProcessUnsolicitedMessage(message);
		}

		#endregion
	}
}
