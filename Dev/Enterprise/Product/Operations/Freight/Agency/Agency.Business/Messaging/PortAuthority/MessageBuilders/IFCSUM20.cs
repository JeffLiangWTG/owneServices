using System;
using System.Globalization;
using CargoWise.Common;
using Enterprise.Edifact;
using Enterprise.Edifact.D98B.Elements;
using Enterprise.Edifact.D98B.Messages.IFCSUM;
using Enterprise.Edifact.D98B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.Business
{
	/// <summary>
	/// IFCSUM v2.0 (D98B)
	/// </summary>
	public class IFCSUM20 : IPortAuthorityMessageBuilder
	{
		public string GenerateMessageText(IPortAuthorityMessagingData data)
		{
			UNCharacterSet characterSet = new UNOACharacterSet();
			return GenerateMessage(data).ToString(characterSet);
		}

		#region Generate Segment Group

		protected virtual IFCSUMMessage GenerateMessage(IPortAuthorityMessagingData data)
		{
			IFCSUMMessage message = new IFCSUMMessage();

			UNHSegment uNH = message.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNH.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			uNH.MessageIdentifier.MessageType = MessageTypeList.ForwardingAndConsolidationSummaryMessage;
			uNH.MessageIdentifier.MessageVersionNumber = MessageVersionNumberList.DraftVersionUnEdifactDirectory;
			uNH.MessageIdentifier.MessageReleaseNumber = MessageReleaseNumberList.Release1998B;
			uNH.MessageIdentifier.ControllingAgency = ControllingAgencyList.UnEceTradeWp4;
			uNH.MessageIdentifier.AssociationAssignedCode = "ANZ20";

			BGMSegment bGM = message.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.CargoManifest;
			bGM.DocumentMessageIdentification.DocumentMessageNumber = EDIMessage.MessageNumberPlaceHolder;
			bGM.MessageFunctionCoded = Lookup(data.MessageFunction);
			bGM.ResponseTypeCoded = ResponseTypeCodedList.MessageAcknowledgement;

			DTMSegment dTM = message.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.DocumentMessageDateTime;
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmddhhmm;
			dTM.DateTimePeriod.DateTimePeriod = data.MessagePrepared.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);

			GenerateGroup8(message.Group8, data);

			foreach (IPortAuthorityEquipmentData equipment in data.Equipment)
			{
				GenerateGroup17(message.Group17, equipment);
			}

			foreach (IPortAuthorityConsignmentData consignment in data.Consignments)
			{
				GenerateGroup19(message.Group19, data, consignment);
			}

			GenerateCNT(message.CNT, ControlQualifierList.TotalNumberOfConsignments, message.Group19.Count);
			GenerateCNT(message.CNT, ControlQualifierList.TotalNumberOfEquipment, message.Group17.Count);

			UNTSegment uNT = message.UNT.InstantiateAChildAndAddItToChildrenCollection();
			uNT.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			uNT.NumberOfSegmentsInTheMessage = message.CountIncludingUNT.ToString(CultureInfo.InvariantCulture);
			return message;
		}

		/// <summary>
		/// Segment Group 8 - Details Of Transport
		/// </summary>
		static void GenerateGroup8(SegmentGroup8MessageSection section, IPortAuthorityMessagingData data)
		{
			SegmentGroup8 group = section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateTDT(group.TDT, data.VesselName, data.VesselLloyds, data.Voyage);
			GenerateLOC(group.LOC, PlaceLocationQualifierList.PlacePortOfLoading, data.Load);
			GenerateLOC(group.LOC, PlaceLocationQualifierList.PlacePortOfDischarge, data.Discharge);
		}

		/// <summary>
		/// Segment Group 17 - Equipment Details
		/// </summary>
		static void GenerateGroup17(SegmentGroup17MessageSection section, IPortAuthorityEquipmentData equipment)
		{
			SegmentGroup17 group = section.InstantiateAChildAndAddItToChildrenCollection();

			EQDSegment eQD = group.EQD.InstantiateAChildAndAddItToChildrenCollection();
			eQD.EquipmentQualifier = EquipmentQualifierList.Container;
			eQD.EquipmentIdentification.EquipmentIdentificationNumber = equipment.ContainerNumber;
			eQD.EquipmentSizeAndType.EquipmentSizeAndTypeIdentification = EquipmentSizeAndTypeIdentificationList.GetFromString(equipment.ContainerISOCode);
			eQD.EquipmentStatusCoded = Lookup(equipment.ContainerStatus);
			eQD.FullEmptyIndicatorCoded = equipment.IsEmpty ? FullEmptyIndicatorCodedList.Empty : FullEmptyIndicatorCodedList.Full;
		}

		/// <summary>
		/// Segment Group 19 - Consignment Information
		/// </summary>
		static void GenerateGroup19(SegmentGroup19MessageSection section, IPortAuthorityMessagingData data, IPortAuthorityConsignmentData consignment)
		{
			SegmentGroup19 group = section.InstantiateAChildAndAddItToChildrenCollection();

			GenerateCNI(group.CNI, consignment.ConsignmentNumber.ToString(CultureInfo.InvariantCulture));
			GenerateGroup24(group.Group24, consignment);
			GenerateGroup26(group.Group26, consignment);
			GenerateGroup34(group.Group34, consignment);

			foreach (IPortAuthorityGoodsData details in consignment.Goods)
			{
				GenerateGroup41(group.Group41, data, consignment, details);
			}
		}

		/// <summary>
		/// Segment Group 24 - Place/Location Identification
		/// </summary>
		static void GenerateGroup24(SegmentGroup24MessageSection section, IPortAuthorityConsignmentData consignment)
		{
			SegmentGroup24 group = section.InstantiateAChildAndAddItToChildrenCollection();
			GenerateLOC(group.LOC, PlaceLocationQualifierList.PlaceOfDelivery, consignment.PortOfDestination);
			GenerateLOC(group.LOC, PlaceLocationQualifierList.PlacePortOfLoading, consignment.PortOfLoading);
			GenerateLOC(group.LOC, PlaceLocationQualifierList.PlacePortOfDischarge, consignment.PortOfDischarge);
			GenerateLOC(group.LOC, PlaceLocationQualifierList.CountryOfOrigin, consignment.CountryOfOrigin);
			GenerateLOC(group.LOC, PlaceLocationQualifierList.CountryOfDestinationOfGoods, consignment.CountryOfDestination);
			GenerateLOC(group.LOC, PlaceLocationQualifierList.PlaceOfReceipt, consignment.PortOfOrigin);
		}

		/// <summary>
		/// Segment Group 26 - Reference
		/// </summary>
		static void GenerateGroup26(SegmentGroup26MessageSection section, IPortAuthorityConsignmentData consignment)
		{
			SegmentGroup26 group = section.InstantiateAChildAndAddItToChildrenCollection();

			RFFSegment rFF = group.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceQualifier = consignment.IsWaybill ? ReferenceQualifierList.WaybillNumber : ReferenceQualifierList.BillOfLadingNumber;
			rFF.Reference.ReferenceNumber = consignment.BillOfLading;
		}

		/// <summary>
		/// Segment Group 34 - Name and Address
		/// </summary>
		static void GenerateGroup34(SegmentGroup34MessageSection section, IPortAuthorityConsignmentData consignment)
		{
			SegmentGroup34 group = section.InstantiateAChildAndAddItToChildrenCollection();

			GenerateNAD(group.NAD, PartyQualifierList.Consignor, consignment.ConsignorNameAndAddress);
			GenerateNAD(group.NAD, PartyQualifierList.Consignee, consignment.ConsigneeNameAndAddress);
		}

		/// <summary>
		/// Segment Group 41 - Goods Item Details
		/// </summary>
		static void GenerateGroup41(SegmentGroup41MessageSection section, IPortAuthorityMessagingData data, IPortAuthorityConsignmentData consignment, IPortAuthorityGoodsData details)
		{
			SegmentGroup41 group = section.InstantiateAChildAndAddItToChildrenCollection();

			GIDSegment gID = group.GID.InstantiateAChildAndAddItToChildrenCollection();
			gID.GoodsItemNumber = details.ItemNumber.ToString(CultureInfo.InvariantCulture);
			gID.NumberAndTypeOfPackages1.NumberOfPackages = details.PackageCount.ToString(CultureInfo.InvariantCulture);
			gID.NumberAndTypeOfPackages1.TypeOfPackagesIdentification = details.PackageCode;

			GeneratePIA(group.PIA, details.HarmonisedCode);
			GenerateFTX(group.FTX, TextSubjectQualifierList.GoodsDescription, details.GoodsDescription);

			GenerateGroup42(group.Group42, details);
			GenerateGroup43(group.Group43, details);
			GenerateGroup46(group.Group46, data, consignment, details);
			GenerateGroup52(group.Group52, data, consignment, details);
		}

		/// <summary>
		/// Segment Group 42 - EmptyEquipmentReturnParty
		/// </summary>
		static void GenerateGroup42(SegmentGroup42MessageSection section, IPortAuthorityGoodsData details)
		{
			if (!details.ContainerYardAddress.IsNullOrEmpty())
			{
				SegmentGroup42 group = section.InstantiateAChildAndAddItToChildrenCollection();
				GenerateNAD(group.NAD, PartyQualifierList.EmptyEquipmentReturnParty, details.ContainerYardAddress);
			}
		}

		/// <summary>
		/// Segment Group 43 - Measurements
		/// </summary>
		static void GenerateGroup43(SegmentGroup43MessageSection section, IPortAuthorityGoodsData details)
		{
			SegmentGroup43 group = section.InstantiateAChildAndAddItToChildrenCollection();

			GenerateMEA(group.MEA, PropertyMeasuredCodedList.UnitGrossWeight, "KGM", details.Kilograms);
			GenerateMEA(group.MEA, PropertyMeasuredCodedList.GrossVolume, "MTQ", details.CubicMetres);
		}

		/// <summary>
		/// Segment Group 46 - Package Identification
		/// </summary>
		static void GenerateGroup46(SegmentGroup46MessageSection section, IPortAuthorityMessagingData data, IPortAuthorityConsignmentData consignment, IPortAuthorityGoodsData details)
		{
			var marksAndNumbers = details.MarksAndNumbers;
			if (data.Load == "AUPKL" || data.Discharge == "AUPKL")
			{
				marksAndNumbers = $"{consignment.BillOfLading}{details.ItemNumber.ToString("000")}".Trim();
				if (consignment.PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
				{
					marksAndNumbers = details.ContainerNumber;
				}
			}

			if (!string.IsNullOrEmpty(marksAndNumbers))
			{
				SegmentGroup46 group = section.InstantiateAChildAndAddItToChildrenCollection();
				PCISegment pCI = group.PCI.InstantiateAChildAndAddItToChildrenCollection();

				TextSplitter splitter = new TextSplitter(35);
				splitter.Text = marksAndNumbers;

				pCI.MarksLabels.ShippingMarks1 = splitter[0];
				pCI.MarksLabels.ShippingMarks2 = splitter[1];
				pCI.MarksLabels.ShippingMarks3 = splitter[2];
				pCI.MarksLabels.ShippingMarks4 = splitter[3];
				pCI.MarksLabels.ShippingMarks5 = splitter[4];
				pCI.MarksLabels.ShippingMarks6 = splitter[5];
				pCI.MarksLabels.ShippingMarks7 = splitter[6];
				pCI.MarksLabels.ShippingMarks8 = splitter[7];
				pCI.MarksLabels.ShippingMarks9 = splitter[8];
				pCI.MarksLabels.ShippingMarks10 = splitter[9];
			}
	}

		/// <summary>
		/// Segment Group 52 - Split Goods Placement
		/// </summary>
		static void GenerateGroup52(SegmentGroup52MessageSection section, IPortAuthorityMessagingData data, IPortAuthorityConsignmentData consignment, IPortAuthorityGoodsData details)
		{
			if (consignment.PackingMode == Core.Constants.ContainerModes.FCL)
			{
				foreach (string containerNumber in details.ContainerNumbers)
				{
					SegmentGroup52 group = section.InstantiateAChildAndAddItToChildrenCollection();

					SGPSegment sGP = group.SGP.InstantiateAChildAndAddItToChildrenCollection();
					sGP.EquipmentIdentification.EquipmentIdentificationNumber = containerNumber;
				}
			}
		}

		#endregion

		#region Generate Segment

		/// <summary>
		/// CNT - Control Total
		/// </summary>
		static void GenerateCNT(CNTSegmentMessageSection section, ControlQualifierList qualifier, int count)
		{
			CNTSegment cNT = section.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlQualifier = qualifier;
			cNT.Control.ControlValue = count.ToString(CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// FTX - Free Text
		/// </summary>
		static void GenerateFTX(FTXSegmentMessageSection section, TextSubjectQualifierList qualifier, string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				TextSplitter splitter = new TextSplitter(70);
				splitter.Text = text;

				FTXSegment fTX = section.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectQualifier = qualifier;
				fTX.TextLiteral.FreeText1 = splitter[0];
				fTX.TextLiteral.FreeText2 = splitter[1];
				fTX.TextLiteral.FreeText3 = splitter[2];
				fTX.TextLiteral.FreeText4 = splitter[3];
				fTX.TextLiteral.FreeText5 = splitter[4];
			}
		}

		/// <summary>
		/// TDT - Details of Transport
		/// </summary>
		static void GenerateTDT(TDTSegmentMessageSection section, string vessel, string lloyds, string voyage)
		{
			TDTSegment tDT = section.InstantiateAChildAndAddItToChildrenCollection();
			tDT.TransportStageQualifier = TransportStageQualifierList.MainCarriageTransport;
			tDT.ConveyanceReferenceNumber = voyage;
			tDT.TransportIdentification.IdOfMeansOfTransportIdentification = lloyds;
			tDT.TransportIdentification.IdOfTheMeansOfTransport = vessel;
		}

		/// <summary>
		/// LOC - Place/Location Identification
		/// </summary>
		static void GenerateLOC(LOCSegmentMessageSection section, PlaceLocationQualifierList qualifier, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				LOCSegment lOC = section.InstantiateAChildAndAddItToChildrenCollection();
				lOC.PlaceLocationQualifier = qualifier;
				lOC.LocationIdentification.PlaceLocationIdentification = value;
			}
		}

		/// <summary>
		/// CNI - Consignment Information
		/// </summary>
		static void GenerateCNI(CNISegmentMessageSection section, string consignmentNumber)
		{
			CNISegment cNI = section.InstantiateAChildAndAddItToChildrenCollection();
			cNI.ConsolidationItemNumber = consignmentNumber;
		}

		/// <summary>
		/// NAD - Name and Address
		/// </summary>
		static void GenerateNAD(NADSegmentMessageSection section, PartyQualifierList qualifier, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				TextSplitter splitter = new TextSplitter(35);
				splitter.Text = value;

				NADSegment nAD = section.InstantiateAChildAndAddItToChildrenCollection();
				nAD.PartyQualifier = qualifier;
				nAD.NameAndAddress.NameAndAddressLine1 = splitter[0];
				nAD.NameAndAddress.NameAndAddressLine2 = splitter[1];
				nAD.NameAndAddress.NameAndAddressLine3 = splitter[2];
				nAD.NameAndAddress.NameAndAddressLine4 = splitter[3];
				nAD.NameAndAddress.NameAndAddressLine5 = splitter[4];
			}
		}

		/// <summary>
		/// MEA - Measurements
		/// </summary>
		static void GenerateMEA(MEASegmentMessageSection section, PropertyMeasuredCodedList property, string unit, decimal value)
		{
			MEASegment mEA = section.InstantiateAChildAndAddItToChildrenCollection();
			mEA.MeasurementPurposeQualifier = MeasurementPurposeQualifierList.Measurement;
			mEA.MeasurementDetails.PropertyMeasuredCoded = property;
			mEA.ValueRange.MeasureUnitQualifier = unit;
			mEA.ValueRange.MeasurementValue = value.ToString("0.###", CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// PIA - Additional Product ID
		/// </summary>
		static void GeneratePIA(PIASegmentMessageSection section, string harmonisedCode)
		{
			if (!string.IsNullOrEmpty(harmonisedCode))
			{
				PIASegment pIA = section.InstantiateAChildAndAddItToChildrenCollection();
				pIA.ProductIdFunctionQualifier = ProductIdFunctionQualifierList.AdditionalIdentification;
				pIA.ItemNumberIdentification1.ItemNumber = harmonisedCode;
				pIA.ItemNumberIdentification1.ItemNumberTypeCoded = ItemNumberTypeCodedList.HarmonisedSystem;
			}
		}

		#endregion

		#region Lookup

		static MessageFunctionCodedList Lookup(PortAuthorityMessageFunction function)
		{
			switch (function)
			{
				case PortAuthorityMessageFunction.Cancelation:
					return MessageFunctionCodedList.Cancellation;
				case PortAuthorityMessageFunction.Replace:
					return MessageFunctionCodedList.Replace;
				case PortAuthorityMessageFunction.Original:
					return MessageFunctionCodedList.Original;
				default:
					throw new InvalidOperationException("unrecognised function " + function);
			}
		}

		static EquipmentStatusCodedList Lookup(PortAuthorityContainerStatus status)
		{
			switch (status)
			{
				case PortAuthorityContainerStatus.Continental:
					return EquipmentStatusCodedList.Continental;
				case PortAuthorityContainerStatus.Domestic:
					return EquipmentStatusCodedList.Domestic;
				case PortAuthorityContainerStatus.Export:
					return EquipmentStatusCodedList.Export;
				case PortAuthorityContainerStatus.Import:
					return EquipmentStatusCodedList.Import;
				case PortAuthorityContainerStatus.Shifter:
					return EquipmentStatusCodedList.Shifter;
				case PortAuthorityContainerStatus.Transhipment:
					return EquipmentStatusCodedList.Transhipment;
				default:
					throw new InvalidOperationException("unrecognised status " + status);
			}
		}

		#endregion
	}
}


