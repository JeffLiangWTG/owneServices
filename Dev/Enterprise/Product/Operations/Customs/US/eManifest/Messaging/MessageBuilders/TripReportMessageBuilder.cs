using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Elements;
using Enterprise.Edifact.D08A.Messages.CUSREP;
using Enterprise.Edifact.D08A.Segments;
using Enterprise.ZArchitecture.Core;
using Converter = Enterprise.Customs.US.eManifest.Messaging.CompleteManifestDataConverter;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class TripReportMessageBuilder : EDIFACTMessageBuilder<ICompleteManifest, CUSREPMessage, EDIMessage>, ICompleteManifestMessageBuilder
	{
		public TripReportMessageBuilder(ICompleteManifest data, ZString messageType, MessageSubTypes messageSubType, bool lazyGenerateMessageContent = false)
			: base(data, messageSubType, new UNOACharacterSet())
		{
			isCompleteTrip = messageType == MessageTypes.Codes.CompleteTrip;
			this.lazyGenerateMessageContent = lazyGenerateMessageContent;
			this.messageType = messageType;
		}

		#region Overrides of EDIFACTMessageBuilder

		protected override EDIMessage PopulateMessagesReturningResult()
		{
			var result = base.PopulateMessagesReturningResult();
			result.EM_MessageType = messageType;
			if (lazyGenerateMessageContent)
			{
				result.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			}
			return result;
		}

		protected override void PopulateEdifactMessage()
		{
			if (!lazyGenerateMessageContent)
			{
				#region PopulateUNH

				var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, "CUSREP", "D", "03B", "UN");
				interpretation.AddUNHInterpretation(unh, unh.MessageReferenceNumber);

				#endregion

				#region Trip Reference / Action Code

				var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateBGM(bgm, DocumentNameCodeList.CustomsCrewAndConveyance, "STANDARD", data.TripReference, MessageActionCode);

				var bgmInterpretation = interpretation.AddNewSegmentInterpretation(bgm);
				bgmInterpretation.AddElementInterpretation("Message Type", messageType.ToCodeDescription<MessageTypes>(DocumentNameCodeList.CustomsCrewAndConveyance));
				bgmInterpretation.AddElementInterpretation(() => data.TripReference);
				bgmInterpretation.AddElementInterpretation(() => GetMessageSubType().ToCodeDescription<MessageActionCodes>(MessageActionCode));

				#endregion

				#region Transmission Reference Number

				if (!data.TransmissionReferenceNumber.IsEmpty)
				{
					var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
					var rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.OriginatorsReference, data.TransmissionReferenceNumber);
					interpretation.AddNewSegmentInterpretation(rff, () => data.TransmissionReferenceNumber);
				}

				#endregion

				#region Estimated Date Of Arrival

				var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated, data.EstimatedDateOfArrival);
				interpretation.AddNewSegmentInterpretation(dtm, () => data.EstimatedDateOfArrival);

				#endregion

				PopulateForNonWithdrawAndNonConfirmation();

				#region PopulateUNT

				var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateUNT(unt, edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture), unh.MessageReferenceNumber);
				interpretation.AddUNTInterpretation(unt, unt.MessageReferenceNumber);

				#endregion
			}
		}

		void PopulateForNonWithdrawAndNonConfirmation()
		{
			if (messageSubType != MessageSubTypes.Withdraw && messageSubType != MessageSubTypes.Confirmation)
			{
				#region First Expected Port Of Arrival

				var group2 = edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection();
				var loc = group2.LOC.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateLOC(
					loc,
					LocationFunctionCodeQualifierList.PlaceOfArrival,
					data.FirstExpectedPortOfArrival,
					Converter.IdentificationCodes.ScheduleD);
				interpretation.AddNewSegmentInterpretation(loc, () => data.FirstExpectedPortOfArrival);

				#endregion

				#region Amendment Reason

				if (messageSubType == MessageSubTypes.Change && data.IsFinalized)
				{
					var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
					var rff = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
					D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.GetFromString("RFA"), data.AmendmentReasonCode);
					interpretation.AddNewSegmentInterpretation(rff, () => data.AmendmentReasonCode);
				}

				#endregion

				#region Carrier Code

				var group5 = edifactMessage.Group5.InstantiateAChildAndAddItToChildrenCollection();
				var nad = group5.NAD.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateNAD(nad, PartyFunctionCodeQualifierList.Carrier, data.CarrierCode, Converter.IdentificationCodes.SCAC);
				interpretation.AddNewSegmentInterpretation(nad, () => data.CarrierCode);

				#endregion

				#region Pre Registered Crew

				if (isCompleteTrip)
				{
					foreach (var crew in data.CrewMembers)
					{
						if (!crew.CrewId.IsEmpty)
						{
							var group6 = edifactMessage.Group5.InstantiateAChildAndAddItToChildrenCollection();
							var nad1 = group6.NAD.InstantiateAChildAndAddItToChildrenCollection();
							CompleteManifestMessageBuilder.PopulateCrew(data.Factory, nad1, interpretation, crew, populateName: false);
						}
					}
				}

				#endregion

				#region Conveyance / Equipment

				PopulateConveyance();

				PopulateEquipment();

				#endregion

				#region Shipments

				foreach (var shipment in data.Shipments.Where(s => !s.ShipmentActionCode.IsEmpty))
				{
					PopulateShipment(shipment);
				}

				#endregion
			}
		}

		#region Populate Conveyance / Equipment

		void PopulateConveyance()
		{
			#region Populate Conveyance Identifiers

			Func<RFFSegment> getRffSegment = () =>
			{
				var group1 = edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
				return group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			};

			Func<TDTSegment> getTdtSegment = () =>
			{
				var group8 = edifactMessage.Group8.InstantiateAChildAndAddItToChildrenCollection();
				return group8.TDT.InstantiateAChildAndAddItToChildrenCollection();
			};

			Func<LOCSegment> getLocSegment = () =>
			{
				var group8 = edifactMessage.Group8.Cast<SegmentGroup8>().Last();
				var group9 = group8.Group9.InstantiateAChildAndAddItToChildrenCollection();
				return group9.LOC.InstantiateAChildAndAddItToChildrenCollection();
			};

			CompleteManifestMessageBuilder.PopulateConveyanceIdentifiers(getRffSegment, getTdtSegment, getLocSegment, interpretation, data);

			if (!isCompleteTrip && (edifactMessage.Group8.Count == 0 || edifactMessage.Group8[0].TDT.Count == 0))
			{
				CompleteManifestMessageBuilder.PopulateConveyanceIdentifier(getTdtSegment, interpretation, data, CompleteManifestDataConverter.IdentificationCodes.VIN, "DUMMY", "Dummy VIN");
			}

			#endregion

			#region Populate Conveyance Insurance Information

			var insurance = data.Conveyance.Insurance;
			if (insurance != null
				&& (!insurance.InsuranceName.IsEmpty
					|| !insurance.InsurancePolicyNumber.IsEmpty
					|| !insurance.InsuranceAmount.IsEmpty
					|| !insurance.InsuranceYearPolicyIssue.IsEmpty))
			{
				#region Mandatory Trigger Segment

				var group4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
				var tax = group4.TAX.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateTAX(tax, DutyOrTaxOrFeeFunctionCodeQualifierList.GetFromString("10"));
				interpretation.AddMandatoryTriggerSegmentInterpretation(tax);

				#endregion

				#region Insurance Amount

				var moa = group4.MOA.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateMOA(moa, MonetaryAmountTypeCodeQualifierList.InsuranceAmount, insurance.InsuranceAmount, 0);
				interpretation.AddNewSegmentInterpretation(moa, () => insurance.InsuranceAmount);

				#endregion

				#region Insurance Name

				var fii = group4.FII.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateFII(fii, PartyFunctionCodeQualifierList.Surety, insurance.InsuranceName);
				interpretation.AddNewSegmentInterpretation(fii, () => insurance.InsuranceName);

				#endregion

				#region Insurance Policy Number

				var rff = group4.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.InsuranceContractReferenceNumber, insurance.InsurancePolicyNumber);
				interpretation.AddNewSegmentInterpretation(rff, () => insurance.InsurancePolicyNumber);

				#endregion

				#region Insurance Year Policy Issue

				var dtm = group4.DTM.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateDTM(dtm, DateOrTimeOrPeriodFunctionCodeQualifierList.CoverageDuration, insurance.InsuranceYearPolicyIssue);
				interpretation.AddNewSegmentInterpretation(dtm, () => insurance.InsuranceYearPolicyIssue);

				#endregion
			}

			#endregion
		}

		void PopulateEquipment()
		{
			foreach (var equipment in data.Equipment)
			{
				CompleteManifestMessageBuilder.PopulateEquipment(
					equipment,
					() => edifactMessage.Group10.InstantiateAChildAndAddItToChildrenCollection(),
					sg => sg.EQD.InstantiateAChildAndAddItToChildrenCollection(),
					sg => sg.SEL.InstantiateAChildAndAddItToChildrenCollection(),
					sg => sg.Group11.InstantiateAChildAndAddItToChildrenCollection(),
					sg => sg.RFF.InstantiateAChildAndAddItToChildrenCollection(),
					sg => sg.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					interpretation);
			}
		}

		#endregion

		#region Populate Shipment

		void PopulateShipment(IShipment shipment)
		{
			var group3 = edifactMessage.Group3.InstantiateAChildAndAddItToChildrenCollection();

			#region Action Code

			var shipmentActionCode = GetShipmentActionCode(shipment.ShipmentActionCode);
			var doc = group3.DOC.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.Waybill, string.Empty, string.Empty, string.Empty, shipmentActionCode);
			interpretation.AddNewSegmentInterpretation(doc, () => shipment.ShipmentActionCode.ToCodeDescription<MessageActionCodes>(shipmentActionCode));

			#endregion

			#region Shipment Control Number

			var rff = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D08AMessageUtilities.PopulateRFF(rff, ReferenceCodeQualifierList.WaybillNumber, shipment.ShipmentControlNumber);
			interpretation.AddNewSegmentInterpretation(rff, () => shipment.ShipmentControlNumber);

			#endregion

			#region Boarded Quantity

			if (!shipment.BoardedQuantity.IsEmpty)
			{
				var qty = group3.QTY.InstantiateAChildAndAddItToChildrenCollection();
				D08AMessageUtilities.PopulateQTY(qty, QuantityTypeCodeQualifierList.SplitQuantity, shipment.BoardedQuantity);
				interpretation.AddNewSegmentInterpretation(qty, () => shipment.BoardedQuantity);
			}

			#endregion
		}

		#endregion

		#region Action Code

		MessageFunctionCodeList MessageActionCode
		{
			get
			{
				MessageFunctionCodeList result = null;
				switch (messageSubType)
				{
					case MessageSubTypes.Create:
						result = isCompleteTrip ? MessageFunctionCodeList.FinalTransmission : MessageFunctionCodeList.Addition;
						break;
					case MessageSubTypes.Withdraw:
						result = MessageFunctionCodeList.Deletion;
						break;
					case MessageSubTypes.Change:
						result = MessageFunctionCodeList.Change;
						break;
					case MessageSubTypes.Confirmation:
						result = MessageFunctionCodeList.Confirmation;
						break;
				}
				return result;
			}
		}

		static DocumentStatusCodeList GetShipmentActionCode(string shipmentActionCode)
		{
			DocumentStatusCodeList result = null;
			switch (shipmentActionCode)
			{
				case MessageActionCodes.Codes.Link:
					result = DocumentStatusCodeList.Status1;
					break;
				case MessageActionCodes.Codes.DeLink:
					result = DocumentStatusCodeList.Status0;
					break;
			}
			return result;
		}

		protected override ZString GetMessageSubType()
		{
			return messageSubType == MessageSubTypes.Confirmation ? (ZString)MessageActionCodes.Codes.Confirmation : base.GetMessageSubType();
		}

		#endregion

		void ICompleteManifestMessageBuilder.GenerateMessageContent(Enterprise.Messaging.Business.EDIMessage message)
		{
			this.GenerateMessageContent(PopulateEdifactMessage, () => edifactMessage.ToString(characterSet), () => interpretation.ToHtml(), message);
		}

		#endregion

		readonly ZString messageType;
		readonly bool isCompleteTrip;
		readonly ZBool lazyGenerateMessageContent;
	}
}
