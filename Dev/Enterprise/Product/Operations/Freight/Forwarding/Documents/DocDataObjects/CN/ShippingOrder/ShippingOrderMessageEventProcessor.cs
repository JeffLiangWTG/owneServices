using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Data.Mutex;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ShippingOrderMessageEventProcessor : IMessageEventsProcessor, IPrintEventsProcessor
	{
		public ShippingOrderMessageEventProcessor(ForwardingConsol consol, IDocument document, bool isSendMessageAsPDF = false)
		{
			this.isSendMessageAsPDF = isSendMessageAsPDF;
			this.consol = consol;
			this.document = document;
		}

		readonly ForwardingConsol consol;
		readonly IDocument document;
		readonly bool isSendMessageAsPDF;

		#region IPrintEventsProcessor members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Action")]
		public void OnPrintJobsCreated(IStmPrintJob[] printJobs)
		{
			if (IsShippingOrderAvailable(consol) || isSendMessageAsPDF)
			{
				OCBEventProcessorHelper.AddOCBEvent(consol, ConsolDocumentNames.ShippingOrder, ConsolDocumentDataStoreNames.ShippingOrder, false, "Document Delivery");
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Event")]
		public void OnMessageSent()
		{
			if (!IsShippingOrderAvailable(consol) && !consol.IsCoLoad && IsEmailAction())
			{
				OCBEventProcessorHelper.AddOCBEvent(consol, ConsolDocumentNames.ShippingOrder, ConsolDocumentDataStoreNames.ShippingOrder, false, "Email");
				MSNEventProcessHelper.AddMSNEvent(consol, ConsolDocumentNames.ShippingOrder);
			}
			else if ((IsShippingOrderAvailable(consol) || isSendMessageAsPDF) && consol.JK_RL_NKLoadPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.China)
			{
				OCBEventProcessorHelper.AddOCBEvent(consol, ConsolDocumentNames.ShippingOrder, ConsolDocumentDataStoreNames.ShippingOrder, false);
			}
		}

		public void OnMessageWithdrawalSent()
		{
			if (IsShippingOrderAvailable(consol) && consol.JK_RL_NKLoadPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.China)
			{
				OCBEventProcessorHelper.AddOCBEvent(consol, ConsolDocumentNames.ShippingOrder, ConsolDocumentDataStoreNames.ShippingOrder, true);
			}
		}

		public void OnResetToOriginal()
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
						&& data.Value is ShippingOrder shippingOrder)
					{
						shippingOrder.SourceID = consolInAnotherFactory.CarrierShipperReferenceWithFallback;
						shippingOrder.FreightForwarderReference = consolInAnotherFactory.CarrierShipperReferenceWithFallback;
						data.Properties.GetOrCreate(nameof(shippingOrder.SourceID))?.AcceptChanges();
						data.Properties.GetOrCreate(nameof(shippingOrder.FreightForwarderReference))?.AcceptChanges();
					}

					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
					consol.Numbers.Reload(true);
				}
			}
		}

		bool IsEmailAction()
		{
			if (document != null && document.Data is IDynamicData data && data.Value is ShippingOrder shippingOrder)
			{
				var carrier = consol.ShippingLine;
				var shippingOrderCarrier = shippingOrder.Carrier;
				return ((!carrier?.ShippingLine?.RSL_ShippingOrderAvailable ?? false) &&
					!shippingOrderCarrier.Email.IsEmpty && !shippingOrderCarrier.Contact.IsEmpty);
			}
			else
			{
				return false;
			}
		}

		bool IsShippingOrderAvailable(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return false;
			}
			if (consol.CarrierBookingAgent != null && consol.CarrierBookingAgent.ShippingLine.RSL_ShippingOrderAvailable)
			{
				return false;
			}

			return (consol.IsCoLoad ? consol.Creditor?.ShippingLine?.RSL_ShippingOrderAvailable : consol.ShippingLine?.ShippingLine?.RSL_ShippingOrderAvailable) ?? (consol.GetRefShippingLineFromCarrierWithFallback()?.RSL_ShippingOrderAvailable ?? false);
		}
	}
}
