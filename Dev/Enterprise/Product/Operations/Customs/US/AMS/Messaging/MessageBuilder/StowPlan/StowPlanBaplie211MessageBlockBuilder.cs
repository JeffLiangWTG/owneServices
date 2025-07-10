using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact;
using Enterprise.Edifact.D95B.Elements;
using Enterprise.Edifact.D95B.Messages.BAPLIE;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business.StowPlan
{
	public class StowPlanBaplie211MessageBuilder : EDIFACTMessageBuilder<IStowPlanSailingData, BAPLIEMessage, StowPlanMessage>
	{
		public StowPlanBaplie211MessageBuilder(IStowPlanSailingData stowPlanData)
			: base(stowPlanData, MessageSubTypes.Create, new UNOACharacterSet())
		{
		}

		protected override void PopulateEdifactMessage()
		{
			#region UNH
			var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unh.MessageIdentifier.MessageType = MessageTypeList.BAPLIEMessage;
			unh.MessageIdentifier.MessageVersionNumber = "D";
			unh.MessageIdentifier.MessageReleaseNumber = "95B";
			unh.MessageIdentifier.ControllingAgency = ControllingAgencyList.UnEceTradeWp4UnitedNationsStandardMessagesUnsm;
			unh.MessageIdentifier.AssociationAssignedCode = "SMDG20";
			#endregion

			#region BGM
			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bgm.DocumentMessageNumber = EDIMessage.MessageNumberPlaceHolder;
			bgm.MessageFunctionCoded = MessageFunctionCodedList.Original;
			#endregion

			#region DTM
			var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
			D95BMessageUtilities.PopulateDTM(dtm, DateTimePeriodQualifierList.DocumentMessageDateTime, ZDateTime.Now);
			#endregion

			PopulateGroup1(edifactMessage.Group1.InstantiateAChildAndAddItToChildrenCollection());

			foreach (var shipment in data.Shipments)
			{
				foreach (var container in shipment.Containers)
				{
					PopulateGroup2(edifactMessage.Group2.InstantiateAChildAndAddItToChildrenCollection(), shipment, container);
				}
			}

			#region UNT

			var unt = edifactMessage.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unt.NumberOfSegmentsInTheMessage = edifactMessage.CountIncludingUNT.ToString();

			#endregion
		}

		#region Group1

		void PopulateGroup1(SegmentGroup1 group1)
		{
			#region TDT
			var tdt = group1.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageQualifier = TransportStageQualifierList.MainCarriageTransport;
			tdt.ConveyanceReferenceNumber = data.VoyageNumber;
			var vesselData = data.Vessel;
			if (vesselData != null)
			{
				tdt.Carrier.CarrierIdentification = vesselData.VesselOperator;
				tdt.Carrier.CodeListQualifier = CodeListQualifierList.CarrierCode;
				tdt.Carrier.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.UsNationalMotorFreightClassificationAssociation;
				tdt.TransportIdentification.IdOfMeansOfTransportIdentification = vesselData.IMONumber;
				tdt.TransportIdentification.CodeListQualifier = CodeListQualifierList.MeansOfTransportIdentification;
				tdt.TransportIdentification.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.LloydsRegisterOfShipping;
				tdt.TransportIdentification.IdOfTheMeansOfTransport = vesselData.VesselName;
			}
			#endregion

			#region LOC - DTM (Departure)
			var departureLOC = group1.LOC.InstantiateAChildAndAddItToChildrenCollection();
			D95BMessageUtilities.PopulateLOC(departureLOC, PlaceLocationQualifierList.PlaceOfDeparture, data.Departure,
				CodeListQualifierList.Port, CodeListResponsibleAgencyCodedList.UnEceUnitedNationsEconomicCommissionForEurope);

			var departureDTM = group1.DTM.InstantiateAChildAndAddItToChildrenCollection();
			D95BMessageUtilities.PopulateDTM(departureDTM, data.IsDepartureTimeEstimated ?
				DateTimePeriodQualifierList.DepartureDateTimeEstimated : DateTimePeriodQualifierList.DepartureDateTimeActual,
				data.DepartureTime);

			#endregion

			#region LOC - DTM (Arrival)

			var arrival = data.Arrival;

			var arrivalLOC = group1.LOC.InstantiateAChildAndAddItToChildrenCollection();
			D95BMessageUtilities.PopulateLOC(arrivalLOC, PlaceLocationQualifierList.NextPortOfCall, data.Arrival,
				CodeListQualifierList.Port, CodeListResponsibleAgencyCodedList.UnEceUnitedNationsEconomicCommissionForEurope);

			var arrivalDTM = group1.DTM.InstantiateAChildAndAddItToChildrenCollection();
			D95BMessageUtilities.PopulateDTM(arrivalDTM, data.IsArrivalTimeEstimated ?
				DateTimePeriodQualifierList.ArrivalDateTimeEstimated : DateTimePeriodQualifierList.ArrivalDateTimeActual,
				data.ArrivalTime);

			#endregion
		}

		#endregion

		#region Group2

		void PopulateGroup2(SegmentGroup2 group2, IStowPlanShipmentData shipment, IStowPlanContainerData container)
		{
			#region StowageLOC

			var stowageLoc = group2.StowageLOC.InstantiateAChildAndAddItToChildrenCollection();
			D95BMessageUtilities.PopulateLOC(stowageLoc, PlaceLocationQualifierList.StowageCell, container.StowPosition, null, CodeListResponsibleAgencyCodedList.IsoInternationalOrganizationForStandardization);

			#endregion

			#region OtherLOC

			var shipmentLadingLoc = group2.OtherLOC.InstantiateAChildAndAddItToChildrenCollection();
			D95BMessageUtilities.PopulateLOC(shipmentLadingLoc, PlaceLocationQualifierList.PlacePortOfLoading, shipment.PortOfLading, null, null);

			var shipmentDischargeLoc = group2.OtherLOC.InstantiateAChildAndAddItToChildrenCollection();
			D95BMessageUtilities.PopulateLOC(shipmentDischargeLoc, PlaceLocationQualifierList.PlacePortOfDischarge, shipment.PortOfDischarge, null, null);

			#endregion

			#region MEA

			var mea = group2.MEA.InstantiateAChildAndAddItToChildrenCollection();
			mea.MeasurementApplicationQualifier = MeasurementApplicationQualifierList.Weights;
			mea.ValueRange.MeasureUnitQualifier = "KG";
			mea.ValueRange.MeasurementValue = container.GrossWeightInKG.ToStringTrimZeros(0);

			#endregion

			#region RFF

			var rff = group2.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceQualifier = ReferenceQualifierList.BillOfLadingNumber;
			rff.Reference.ReferenceNumber = !shipment.JS_HouseBill.IsEmpty ? shipment.JS_HouseBill.ToString() : "1";

			#endregion

			PopulateGroup3(group2.Group3.InstantiateAChildAndAddItToChildrenCollection(), container);

			foreach (var hazardCode in container.HazardCodes)
			{
				PopulateGroup4(group2.Group4.InstantiateAChildAndAddItToChildrenCollection(), hazardCode);
			}
		}

		#endregion

		#region Group 3

		void PopulateGroup3(SegmentGroup3 group3, IStowPlanContainerData container)
		{
			var stockData = container.Stock;

			#region EQD

			var eqd = group3.EQD.InstantiateAChildAndAddItToChildrenCollection();
			eqd.EquipmentQualifier = EquipmentQualifierList.Container;
			eqd.EquipmentIdentification.EquipmentIdentificationNumber = container.EquipmentNumber;
			eqd.EquipmentSizeAndType.EquipmentSizeAndTypeIdentification = EquipmentSizeAndTypeIdentificationList.GetFromString(container.ISOType);
			//EquipmentSizeAndTypeIdentificationList.GetFromString(stockData != null ? stockData.EquipmentSizeType : ZString.Empty);
			eqd.FullEmptyIndicatorCoded = FullEmptyIndicatorCodedList.Full; // dummy

			#endregion

			if (stockData != null)
			{
				var nad = group3.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nad.PartyQualifier = PartyQualifierList.Carrier;
				nad.PartyIdentificationDetails.PartyIdIdentification = stockData.ContainerOperator;
				nad.PartyIdentificationDetails.CodeListQualifier = CodeListQualifierList.CarrierCode;
				nad.PartyIdentificationDetails.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.UsNationalMotorFreightClassificationAssociation;
			}
		}

		#endregion

		void PopulateGroup4(SegmentGroup4 group4, ZString hazardCode)
		{
			var dgs = group4.DGS.InstantiateAChildAndAddItToChildrenCollection();
			dgs.DangerousGoodsRegulationsCoded = DangerousGoodsRegulationsCodedList.ImoImdgCode;
			dgs.HazardCode.HazardCodeIdentification = hazardCode;
		}
	}
}
