using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Data.Mutex;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ShippingInstructionEventProcessor : IMessageEventsProcessor, IPrintEventsProcessor
	{
		public ShippingInstructionEventProcessor(ForwardingConsol consol, IDocument document, bool isSendMessageAsPDF = false)
		{
			this.isSendMessageAsPDF = isSendMessageAsPDF;
			this.consol = consol;
			this.document = document;
		}

		readonly ForwardingConsol consol;
		readonly IDocument document;
		readonly bool isSendMessageAsPDF;

		#region IPrintEventsProcessor members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Event")]
		public void OnPrintJobsCreated(IStmPrintJob[] printJobs)
		{
			if (ShippingInstructionAvailable(consol) || isSendMessageAsPDF)
			{
				OCBEventProcessorHelper.AddOCBEvent(consol, ConsolDocumentNames.ShippingInstruction, ConsolDocumentDataStoreNames.SeaBookingRequest2, false, "Document Delivery");
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Event")]
		public void OnMessageSent()
		{
			if (IsEmailAction())
			{
				OCBEventProcessorHelper.AddOCBEvent(consol, ConsolDocumentNames.ShippingInstruction, ConsolDocumentDataStoreNames.SeaBookingRequest2, false, "Email");
				MSNEventProcessHelper.AddMSNEvent(consol, ConsolDocumentNames.ShippingInstruction);
			}
			else if (ShippingInstructionAvailable(consol) || isSendMessageAsPDF)
			{
				OCBEventProcessorHelper.AddOCBEvent(consol, ConsolDocumentNames.ShippingInstruction, ConsolDocumentDataStoreNames.SeaBookingRequest2, false);
			}
		}

		public void OnMessageWithdrawalSent()
		{
			if (ShippingInstructionAvailable(consol))
			{
				OCBEventProcessorHelper.AddOCBEvent(consol, ConsolDocumentNames.ShippingInstruction, ConsolDocumentDataStoreNames.SeaBookingRequest2, true);
			}
		}

		public void OnResetToOriginal()
		{
			if (!consol.HasSentDocument(ConsolDocumentNames.BookingRequest) && !consol.HasSentDocument(ConsolDocumentNames.ShippingOrder))
			{
				using (var mutex = new ZGlobalMutex(ZArchitecture.Modules.MutexIDs.CSRNumberAllocation, consol.PK.ToString()))
				{
					if (!mutex.IsLocked && mutex.Lock())
					{
						var factory = new BusinessObjectFactory();
						var consolInAnotherFactory = factory.Load<ForwardingConsol>(consol.PK);
						ConsolCarrierShipperReferenceNumberCalculator.PopulateShipperReferenceNumber(consolInAnotherFactory);

						if (document != null
							&& document.Data is IDynamicData data
							&& data.Value is CarrierMessageData carrierMessageData)
						{
							carrierMessageData.SourceID = consolInAnotherFactory.CarrierShipperReferenceWithFallback;
							carrierMessageData.FreightForwarderReference = carrierMessageData.SourceID;
							data.Properties.GetOrCreate(nameof(carrierMessageData.SourceID))?.AcceptChanges();
							data.Properties.GetOrCreate(nameof(carrierMessageData.FreightForwarderReference))?.AcceptChanges();
						}

						ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
						consol.Numbers.Reload(true);
					}
				}
			}
		}

		bool IsEmailAction()
		{
			if (document != null && document.Data is IDynamicData data && data.Value is CarrierMessageData messageData)
			{
				var carrier = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;
				var messageDataCarrier = messageData.Recipient;
				return ((!carrier?.ShippingLine?.RSL_ShippingInstructionAvailable ?? false) &&
					!messageDataCarrier.Email.IsEmpty && !messageDataCarrier.Contact.IsEmpty);
			}
			else
			{
				return false;
			}
		}
		bool ShippingInstructionAvailable(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return false;
			}

			return (consol.IsCoLoad ? consol.Creditor?.ShippingLine?.RSL_ShippingInstructionAvailable : consol.ShippingLine?.ShippingLine?.RSL_ShippingInstructionAvailable)
				?? (consol.GetRefShippingLineFromCarrierWithFallback()?.RSL_ShippingInstructionAvailable ?? false);
		}
	}
}
