using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Edifact;
using Enterprise.Edifact.D03A.Elements;
using Enterprise.Edifact.D03A.Messages.CUSCAR;
using Enterprise.Edifact.D03A.Segments;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport
{
	public sealed class MessageBuilder : EdifactMessageBuilder, IManifestMessageBuilder
	{
		public enum MessageTypes { None = 0, Cancellation = 1, Replacement = 5, Original = 9 }

		public MessageBuilder(ForwardingConsol consol, MessageTypes messageType, OutwardReportManifestStatus manifestStatus)
		{
			if (consol != null)
			{
				this.consol = consol;
			}
			this.messageType = messageType;
			this.manifestStatus = manifestStatus;

			consol.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
		}
		readonly ForwardingConsol consol;
		readonly MessageTypes messageType;
		readonly OutwardReportManifestStatus manifestStatus;

		protected override Edifact.Auto.SegmentGroup GetNewEDIFACTMessage()
		{
			return new CUSCARMessage();
		}

		protected override EDIMessage GetNewMessage()
		{
			CheckErrorsBeforeGeneratingMessage();
			var message = (OutwardReportMessage)consol.Messages.AddNew(typeof(OutwardReportMessage));
			message.MsgTransMode = MsgTransportList.Codes.TSW;
			message.EM_IsTestMessage = NZCustomsDataRegistry.Instance.ExportOrnTestMode.Value;
			message.EM_LinkedObject = consol;
			return message;
		}

		protected override void SetMessageType()
		{
			//set by using a specific type of EDIMessage, OutwardReportMessage
		}

		void CheckErrorsBeforeGeneratingMessage()
		{
			if (!errorChecked)
			{
				errorList = OutwardReportValidation.CheckErrorsBeforeGeneratingMessage(messageType);
				errorChecked = true;
			}
		}

		OutwardReportValidation OutwardReportValidation
		{
			get
			{
				if (fOutwardReportValidation == null)
				{
					fOutwardReportValidation = new OutwardReportValidation(manifestStatus);
				}
				return fOutwardReportValidation;
			}
		}
		OutwardReportValidation fOutwardReportValidation;

		protected override void SetMessageSubType()
		{
			message.SetMessageSubType(messageType);
		}

		new OutwardReportMessage message
		{
			get { return (OutwardReportMessage)base.message; }
		}

		protected override void SetParentMessagingStatusAfterMessagePosting()
		{
			manifestStatus.E2_MessageStatus = OutwardReportStatusList.Codes.AwaitingResponse;
		}

		new CUSCARMessage EDIFACTMessage
		{
			get { return (CUSCARMessage)base.EDIFACTMessage; }
		}

		#region Segment Generators
		protected override void GenerateGroup0()
		{
			GenerateGroup0UNH(EDIFACTMessage);
			GenerateGroup0BGM(EDIFACTMessage);
			GenerateGroup1RFF(EDIFACTMessage.Group1.InstantiateAChildAndAddItToChildrenCollection());
			GenerateGroup2NAD(EDIFACTMessage.Group2.InstantiateAChildAndAddItToChildrenCollection());
			GenerateGroup4(EDIFACTMessage.Group4.InstantiateAChildAndAddItToChildrenCollection());
			GenerateGroup5(EDIFACTMessage.Group5.InstantiateAChildAndAddItToChildrenCollection());
			GenerateGroup0CNT(EDIFACTMessage);
			GenerateGroup7CNI_RFF(EDIFACTMessage);
			GenerateUNT(EDIFACTMessage);
		}

		void GenerateGroup0UNH(CUSCARMessage message)
		{
			UNHSegment uNH = message.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNH.MessageReferenceNumber = OutwardReportMessage.MessageNumberPlaceHolder;
			uNH.MessageIdentifier.MessageType = "CUSCAR";
			uNH.MessageIdentifier.MessageVersionNumber = "D";
			uNH.MessageIdentifier.MessageReleaseNumber = "03A";
			uNH.MessageIdentifier.ControllingAgency = "UN";
		}

		void GenerateGroup0BGM(CUSCARMessage message)
		{
			BGMSegment bGM = message.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.CargoDeclarationDeparture;
			bGM.DocumentMessageName.DocumentName = "DEPART";
			bGM.DocumentMessageIdentification.DocumentIdentifier = OutwardReportMessage.SendersReferencePlaceHolder;

			MessageFunctionCodeList messageTypeCode = null;

			switch (messageType)
			{
				case MessageTypes.Cancellation:
					messageTypeCode = MessageFunctionCodeList.Cancellation;
					break;
				case MessageTypes.Original:
					messageTypeCode = MessageFunctionCodeList.Original;
					break;
				case MessageTypes.Replacement:
					messageTypeCode = MessageFunctionCodeList.Replace;
					break;
			}

			bGM.MessageFunctionCode = messageTypeCode;
		}

		void GenerateGroup1RFF(SegmentGroup1 group1)
		{
			if (messageType == MessageTypes.Cancellation || messageType == MessageTypes.Replacement)
			{
				RFFSegment rFF = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.ExportReferenceNumber;
				rFF.Reference.ReferenceIdentifier = manifestStatus.E2_CustomsEntryNumber;
			}
		}

		void GenerateGroup2NAD(SegmentGroup2 group2)
		{
			NADSegment nADForConsolidator = group2.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nADForConsolidator.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.Consolidator;
			nADForConsolidator.PartyIdentificationDetails.PartyIdentifier = NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant();
			nADForConsolidator.PartyIdentificationDetails.CodeListIdentificationCode = CodeListIdentificationCodeList.X_MutuallyDefined;
			nADForConsolidator.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.NzNewZealandCustoms;

			if ((messageType == MessageTypes.Original || messageType == MessageTypes.Replacement))
			{
				NADSegment nADForOperator = group2.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nADForOperator.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.ConnectingCarrier;
				nADForOperator.NameAndAddress.NameAndAddressDescription1 = ConsolTransportDetails.ShippingLine == null ? ZString.Empty : ConsolTransportDetails.ShippingLine.OH_FullName.SubstringSafe(0, 35);
			}
		}

		void GenerateGroup4(SegmentGroup4 group4)
		{
			GenerateTDTForGroup4(group4);
			GenerateLOCForGroup4(group4);
			GenerateDTMForGroup4(group4);
		}

		void GenerateTDTForGroup4(SegmentGroup4 group4)
		{
			if (messageType == MessageTypes.Original || messageType == MessageTypes.Replacement)
			{
				TDTSegment tDT = group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tDT.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;

				if (consol.IsAir)
				{
					tDT.ModeOfTransport.TransportModeNameCode = "4";
					tDT.TransportIdentification.TransportMeansIdentificationName = ConsolTransportDetails.VoyageFlight.SubstringSafe(0, 7);
				}
				else if (consol.IsSea)
				{
					tDT.ModeOfTransport.TransportModeNameCode = "1";
					tDT.TransportIdentification.TransportMeansIdentificationName = ConsolTransportDetails.VesselName.SubstringSafe(0, 30);
					tDT.MeansOfTransportJourneyIdentifier = ConsolTransportDetails.VoyageFlight.SubstringSafe(0, 8);
				}
			}
		}

		void GenerateLOCForGroup4(SegmentGroup4 group4)
		{
			if (messageType == MessageTypes.Replacement || messageType == MessageTypes.Original)
			{
				LOCSegment lOCForLoadPort = group4.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOCForLoadPort.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlaceOfDeparture;
				lOCForLoadPort.LocationIdentification.LocationNameCode = ConsolTransportDetails.LoadPort;

				LOCSegment lOCForDisPort = group4.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOCForDisPort.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.PlaceOfDestination;
				lOCForDisPort.LocationIdentification.LocationNameCode = ConsolTransportDetails.PortOfDischarge.SubstringSafe(0, 2);
			}
		}

		void GenerateDTMForGroup4(SegmentGroup4 group4)
		{
			if (messageType == MessageTypes.Replacement || messageType == MessageTypes.Original)
			{
				DTMSegment dTM = group4.DTM.InstantiateAChildAndAddItToChildrenCollection();
				dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTime;
				dTM.DateTimePeriod.DateOrTimeOrPeriodText = ConsolTransportDetails.DepartureDate.ToString("yyyyMMdd");
				dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			}
		}

		void GenerateGroup0CNT(CUSCARMessage message)
		{
			if (messageType == MessageTypes.Replacement || messageType == MessageTypes.Original)
			{
				CNTSegment cNT = message.CNT.InstantiateAChildAndAddItToChildrenCollection();
				cNT.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.NumberOfLineItemsInMessage;
				//CNT.Control.ControlTotalQuantity will be populated later after Group7CNI_RFF
			}
		}

		void GenerateGroup5(SegmentGroup5 group5)
		{
			if (consol.IsSea && (messageType == MessageTypes.Replacement || messageType == MessageTypes.Original))
			{
				foreach (CommonContainer container in consol.Containers)
				{
					EQDSegment eQD = group5.EQD.InstantiateAChildAndAddItToChildrenCollection();
					eQD.EquipmentTypeCodeQualifier = EquipmentTypeCodeQualifierList.Container;
					eQD.EquipmentIdentification.EquipmentIdentifier = container.JC_ContainerNum;

					FullOrEmptyIndicatorCodeList mode = null;
					if (container.JC_IsEmptyContainer)
					{
						mode = FullOrEmptyIndicatorCodeList.Empty;
					}
					else if (container.JC_ContainerMode == Core.Constants.ContainerModes.LCL)
					{
						mode = FullOrEmptyIndicatorCodeList.FullMixedConsignment;
					}
					else
					{
						mode = FullOrEmptyIndicatorCodeList.Full;
					}

					eQD.FullOrEmptyIndicatorCode = mode;
				}
			}
		}

		void GenerateGroup7CNI_RFF(CUSCARMessage message)
		{
			if (messageType == MessageTypes.Replacement || messageType == MessageTypes.Original)
			{
				int itemNumber = 0;
				OrderedShipments shipments = new OrderedShipments(consol.Shipments);
				SegmentGroup7MessageSection group7s = message.Group7;
				foreach (ForwardingShipment shipment in shipments)
				{
					if (shipment.JS_JS_ColoadMasterShipment.IsEmpty && !ProcessedFromExpressChildren(shipment, group7s, ref itemNumber))
					{
						if (!shipment.CustomsEntryNumber.IsEmpty)
						{
							GenerateGroup7(group7s, shipment, ++itemNumber);
						}
						else
						{
							foreach (ForwardingShipment subShipment in shipment.CoLoadShipments)
							{
								GenerateGroup7(group7s, subShipment, ++itemNumber);
							}
						}
					}
				}

				CNTSegment cNT = message.CNT[0];
				cNT.Control.ControlTotalQuantity = itemNumber.ToString();
			}
		}

		void GenerateGroup7(SegmentGroup7MessageSection group7s, ForwardingShipment shipment, int itemNumber)
		{
			GenerateGroup7(group7s.InstantiateAChildAndAddItToChildrenCollection(), shipment.CustomsEntryNumber.Replace(", ", ""), shipment.JS_HouseBill, itemNumber);
		}

		bool ProcessedFromExpressChildren(ForwardingShipment shipment, SegmentGroup7MessageSection group7s, ref int itemNumber)
		{
			if (!shipment.IsAir)
			{
				return false;
			}

			var hawbs = CusHAWB.LoadHAWBsLinkedTo(shipment);
			if (hawbs.Count == 0)
			{
				return false;
			}

			if (hawbs.Count == 1)
			{
				CusHAWB hawb = hawbs[0];
				if (hawb.IsWrittenOff)
				{
					GenerateGroup7(group7s, hawb, ++itemNumber);
					return true;
				}
				else
				{
					return false;
				}
			}

			foreach (CusHAWB hawb in hawbs)
			{
				GenerateGroup7(group7s, hawb, ++itemNumber);
			}
			return true;
		}

		void GenerateGroup7(SegmentGroup7MessageSection group7s, CusHAWB hawb, int itemNumber)
		{
			GenerateGroup7(group7s.InstantiateAChildAndAddItToChildrenCollection(), hawb.MAWB.ECINumber, hawb.CS_HAWB, itemNumber);
		}

		void GenerateGroup7(SegmentGroup7 group7, string entryNumber, string houseBill, int itemNumber)
		{
			CNISegment cNI = group7.CNI.InstantiateAChildAndAddItToChildrenCollection();
			cNI.ConsolidationItemNumber = itemNumber.ToString();
			cNI.DocumentMessageDetails.DocumentIdentifier = entryNumber;

			SegmentGroup8 group8 = group7.Group8.InstantiateAChildAndAddItToChildrenCollection();
			RFFSegment rFF = group8.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.HouseWaybillNumber;
			rFF.Reference.ReferenceIdentifier = houseBill;
		}

		void GenerateUNT(CUSCARMessage message)
		{
			UNTSegment uNT = message.UNT.InstantiateAChildAndAddItToChildrenCollection();
			uNT.NumberOfSegmentsInTheMessage = message.CountIncludingUNT.ToString();
			uNT.MessageReferenceNumber = OutwardReportMessage.MessageNumberPlaceHolder;
		}
		#endregion

		#region IManifestMessageBuilder
		public string Errors
		{
			get
			{
				CheckErrorsBeforeGeneratingMessage();
				if (errorReport == null)
				{
					errorReport = new StringBuilder(ErrorCount);
					foreach (string error in errorList)
					{
						errorReport.Append(error);
					}
				}
				return errorReport.ToString();
			}
		}

		public int ErrorCount
		{
			get { return OutwardReportValidation.GetErrorCount(messageType); }
		}

		public ZString[] GeneratedMessageStrings
		{
			get { return new ZString[] { EDIFACTMessage.ToString(new UNOACharacterSet()) }; }
		}

		public IMessageBuilderResult PopulateMessages()
		{
			GenerateMessage();
			try
			{
				message.Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}
			return null;
		}

		public ZString ManifestMessageTypeCode
		{
			get
			{
				ZString result = ZString.Empty;
				switch (messageType)
				{
					case MessageTypes.Cancellation:
						result = "Cancel";
						break;
					case MessageTypes.None:
						result = "Unknown";
						break;
					case MessageTypes.Original:
						result = "Original";
						break;
					case MessageTypes.Replacement:
						result = "Replacement";
						break;
				}
				return result;
			}
		}
		#endregion

		ConsolTransportDetailsType ConsolTransportDetails
		{
			get { return consolTransportDetails ?? (consolTransportDetails = new ConsolTransportDetailsType(consol)); }
		}
		ConsolTransportDetailsType consolTransportDetails;

		class ConsolTransportDetailsType
		{
			public ConsolTransportDetailsType(ForwardingConsol consol)
			{
				Transport transport = consol.Transports.ExportTransport ?? consol.Transports.MostInterestingTransport;
				if (transport != null)
				{
					ShippingLine = transport.Carrier ?? consol.ShippingLine;
					VesselName = transport.JW_Vessel;
					VoyageFlight = transport.JW_VoyageFlight;
					DepartureDate = transport.JW_ATD.IsEmpty ? transport.JW_ETD : transport.JW_ATD;
					LoadPort = transport.JW_RL_NKLoadPort;
					PortOfDischarge = consol.JK_RL_NKDischargePort;
				}
			}

			public readonly OrgHeader ShippingLine;
			public readonly ZString VesselName;
			public readonly ZString VoyageFlight;
			public readonly ZString LoadPort;
			public readonly ZString PortOfDischarge;
			public readonly ZDateTime DepartureDate;
		}

		StringBuilder errorReport;
		List<string> errorList;
		bool errorChecked;

#if DEBUG
		internal
#endif
 void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				errorChecked = false;//Need to check again
			}
		}
	}
}
