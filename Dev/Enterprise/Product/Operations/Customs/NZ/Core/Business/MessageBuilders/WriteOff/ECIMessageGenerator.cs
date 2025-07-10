using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Edifact; // This is really the base.
using Enterprise.Edifact.D98A.Elements;
using Enterprise.Edifact.D98A.Messages.CUSCAR;
using Enterprise.Edifact.D98A.Segments;
using Enterprise.MasterFiles.Business;
using NZCMessage = Enterprise.Customs.NZ.Business.Declaration.NZCMessage;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff
{
	public class ECIMessageGenerator
	{
		public enum MessageTypes { None = 0, CancelECI = 1, Original = 9, ReplaceHeader = 20, ReplaceConsignment = 21 }

		internal ECIMessageGenerator(CUSCARMessage message, MessageTypes messageType)
		{
			this.message = message;
			this.messageType = messageType;
		}
		readonly CUSCARMessage message;
		readonly MessageTypes messageType;

		internal void GenerateGroup0UNH(ZString eciEntryNumber)
		{
			UNHSegment uNHSegment = message.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNHSegment.MessageReferenceNumber = NZCMessage.MessageNumberPlaceHolder;
			uNHSegment.MessageIdentifier.MessageType = Edifact.D98A.Elements.MessageTypeList.CustomsCargoReportMessage;
			uNHSegment.MessageIdentifier.MessageVersionNumber = MessageVersionNumberList.DraftVersionUnEdifactDirectory;
			uNHSegment.MessageIdentifier.MessageReleaseNumber = MessageReleaseNumberList.Release1998A;
			uNHSegment.MessageIdentifier.ControllingAgency = ControllingAgencyList.UnEceTradeWp4;
			uNHSegment.CommonAccessReference = eciEntryNumber;
		}

		internal void GenerateGroup0BGM()
		{
			BGMSegment bGMSegment = message.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGMSegment.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.CargoManifest;
			bGMSegment.DocumentMessageIdentification.DocumentMessageNumber = NZCMessage.SendersReferencePlaceHolder;
			bGMSegment.MessageFunctionCoded = GetMessageFunction();
		}

		internal void GenerateGroup2(OrgHeader shippingLine, OrgHeader forwarder)
		{
			ZString shippingLineName = shippingLine != null ? shippingLine.OH_FullName.Substring(0, 35) : ZString.Empty;
			ZString forwarderName = forwarder != null ? (forwarder.OH_FullName).Left(35) : ZString.Empty;

			if (!shippingLineName.IsEmpty)
			{
				SegmentGroup2 group2 = message.Group2.InstantiateAChildAndAddItToChildrenCollection();
				GenerateGroup2NAD(group2, shippingLineName, PartyQualifierList.Carrier);
				if (forwarderName != shippingLineName)
				{
					GenerateGroup2NAD(group2, forwarderName, PartyQualifierList.Consolidator);
				}
			}
		}

		internal void GenerateGroup0FTX(ZString customsMessageRemarks)
		{
			StringLineBreaker remarksLines = new StringLineBreaker(customsMessageRemarks.ToUpper(), true);
			if (!remarksLines.IsEmpty())
			{
				FTXSegment fTXSegment = message.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTXSegment.TextSubjectQualifier = TextSubjectQualifierList.GeneralInformation;
				fTXSegment.TextLiteral.FreeText1 = remarksLines.GetNextLine(70);
				fTXSegment.TextLiteral.FreeText2 = remarksLines.GetNextLine(70);
				fTXSegment.TextLiteral.FreeText3 = remarksLines.GetNextLine(70);
				fTXSegment.TextLiteral.FreeText4 = remarksLines.GetNextLine(40);
			}
		}

		internal void GenerateGroup4(ZString codedModeOfTransport, ZString vessel, ZString voyageOrFlightNo, ZString barrierPort, ZDateTime barrierDate)
		{
			SegmentGroup4 group4 = message.Group4.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup4TDT(group4, codedModeOfTransport, vessel, voyageOrFlightNo);
			GenerateGroup4LOC(group4, barrierPort);
			GenerateGroup4DTM(group4, barrierDate);
		}

		internal void GenerateGroup0GIS(ZString processingIndicatorCoded, CodeListQualifierList codeListQualifier)
		{
			GISSegment gISECIType = message.GIS.InstantiateAChildAndAddItToChildrenCollection();
			gISECIType.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.GetFromString(processingIndicatorCoded);
			gISECIType.ProcessingIndicator.CodeListQualifier = codeListQualifier;
			gISECIType.ProcessingIndicator.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;
		}

		internal void GenerateGroup5(ZString containerNo, ZString containerSize, ZString containerMode, ZString attachedEquipmentIndicator, ZString prohibitedPackagingIndicator, ZString quarrantineCode)
		{
			SegmentGroup5 group5 = message.Group5.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup5EQD(group5, containerNo, containerSize, containerMode);
			GenerateGroup5GIS(group5, attachedEquipmentIndicator, CodeListQualifierList.HandlingAction);
			GenerateGroup5GIS(group5, prohibitedPackagingIndicator, CodeListQualifierList.TypeOfPackage);
			GenerateGroup5GIS(group5, quarrantineCode, CodeListQualifierList.GovernmentAgencyProcedure);
		}

		internal void GenerateGroup0CNT(ZInt controlValue, ControlQualifierList controlQualifier)
		{
			if (controlValue > 0)
			{
				CNTSegment cNTConsignmentCount = message.CNT.InstantiateAChildAndAddItToChildrenCollection();
				cNTConsignmentCount.Control.ControlQualifier = controlQualifier;
				cNTConsignmentCount.Control.ControlValue = controlValue.ToString();
			}
		}

		internal void GenerateGroup7(JobDeclaration declaration, ZString consignmentNumber)
		{
			SegmentGroup7 group7 = message.Group7.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup7CNI(group7, consignmentNumber);
			NADLineFormatter importer = declaration.Importer != null ? new NADLineFormatter(declaration.Importer) : null;
			NADLineFormatter supplier = declaration.Supplier != null ? new NADLineFormatter(declaration.Supplier) : null;
			SegmentGroup8 group8 = GenerateGroup8(group7, declaration.JE_HouseBill, declaration.JE_RL_NKPortOfLoading, declaration.JE_RL_NKPortOfArrival
				, declaration.JE_RL_NKOrigin.Left(2), declaration.JE_RL_NKFinalDestination, importer, supplier);
			GenerateGroup14s(group8, declaration);
		}

		internal void GenerateGroup7(CusHAWB hawb)
		{
			SegmentGroup7 group7 = message.Group7.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup7CNI(group7, hawb.CS_ConsignmentNum.ToString());

			NADLineFormatter supplier = new NADLineFormatter(hawb.Factory, hawb.CS_ConsignorName, hawb.CS_ConsignorStreet, hawb.CS_ConsignorStreet2, hawb.CS_ConsignorCity, hawb.CS_ConsignorState, hawb.CS_ConsignorPostcode, hawb.CS_RN_NKConsignorCountry);
			NADLineFormatter importer = new NADLineFormatter(hawb.Factory, hawb.CS_ConsigneeName, hawb.CS_ConsigneeStreet, hawb.CS_ConsigneeStreet2, hawb.CS_ConsigneeCity, hawb.CS_ConsigneeState, hawb.CS_ConsigneePostcode, hawb.CS_RN_NKConsigneeCountry);

			SegmentGroup8 group8 = GenerateGroup8(group7, hawb.CS_HAWB, hawb.CS_RL_NKLoadPort, hawb.CS_RL_NKDischargePort
				, hawb.CS_RN_NKGoodsOrigin, hawb.CS_RL_NKDestination, importer, supplier);
			GenerateGroup14(group8, 1, hawb.CS_PiecesManifested, hawb.CS_PackType, hawb.CS_GoodsDescription, hawb.WeightInKGs
				, hawb.CS_GoodsValue, hawb.CS_RX_NKGoodsCurrency, ZString.Empty, hawb.CS_RL_NKOrigin);
		}

		internal void GenerateGroup0UNT()
		{
			UNTSegment uNTSegment = message.UNT.InstantiateAChildAndAddItToChildrenCollection();
			uNTSegment.NumberOfSegmentsInTheMessage = message.CountIncludingUNT.ToString();
			uNTSegment.MessageReferenceNumber = NZCMessage.MessageNumberPlaceHolder;
		}

		#region Child Group Generators
		SegmentGroup8 GenerateGroup8(SegmentGroup7 parent, ZString houseBill, ZString portOfLoading, ZString portOfArrival
			, ZString countryOfOrigin, ZString portOfDelivery, NADLineFormatter importer, NADLineFormatter supplier)
		{
			SegmentGroup8 group8 = parent.Group8.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup8RFF(group8, houseBill.ToUpper());

			GenerateGroup8LOC(group8, portOfLoading, PlaceLocationQualifierList.PlacePortOfLoading);
			GenerateGroup8LOC(group8, portOfArrival, PlaceLocationQualifierList.PlacePortOfDischarge);
			GenerateGroup8LOC(group8, countryOfOrigin, PlaceLocationQualifierList.CountryOfOrigin); // Spec says this segment is only required for Airfreight, however VFP sends it for sea QED so does 
			if (portOfDelivery != portOfArrival)
			{
				GenerateGroup8LOC(group8, portOfDelivery, PlaceLocationQualifierList.PlaceOfDelivery);
			}

			// Have never had Port of Transhipment or Final Destination this in the dataset. Never been queried.

			GenerateGroup11(group8, importer, PartyQualifierList.Consignee);
			GenerateGroup11(group8, supplier, PartyQualifierList.Consignor);

			// SegmentGroup11 for "Contact Party" not ever catered for in VFP version. No complaints from Clients. Is only "optional".

			// GenerateGroup14 now called externally.

			return group8;
		}

		void GenerateGroup14s(SegmentGroup8 parent, JobDeclaration declaration)
		{
			ZString invoiceCurrency = declaration.ECI_InvoiceCurrency != null ? declaration.ECI_InvoiceCurrency.RX_Code : ZString.Empty;
			if (declaration.CusContainers.Count == 0 || declaration.JE_TransportMode == JobTransportModeList.Codes.Air)
			{
				GenerateGroup14(parent, 1,
					declaration.JE_TotalNoOfPacks, declaration.JE_TotalNoOfPacksPackType,
					declaration.JE_GoodsDescription,
					declaration.JE_DeclaredWeight,
					declaration.JE_ECI_InvoiceAmount, invoiceCurrency,
					ZString.Empty,
					declaration.JE_RL_NKOrigin);
			}
			else
			{
				var houseBill = declaration.Bills.PrimaryHouseBill;
				int index = 0;
				for (index = 0; index < declaration.CusContainers.Count; index++)
				{
					var container = declaration.CusContainers[index];
					var packageCount = ZInt.Zero;
					var packageType = UniversalReferenceConstants.PackageTypeListCodes.Package;
					if (houseBill != null)
					{
						var packingGroup = houseBill.PackingGroups.GetElementWithContainer(container) as PackingGroup;
						if (packingGroup?.Packages.FirstOrDefault() is Package package)
						{
							packageCount = package.CW_PackQty;
							packageType = package.CW_PackType;
						}
					}

					GenerateGroup14(parent, index + 1,
						packageCount, packageType,
						declaration.JE_GoodsDescription,
						container.CO_Weight,
						container.ECI_ApportionedContainerisedGoodsValue, invoiceCurrency,
						container.CO_ContainerNumber,
						declaration.JE_RL_NKOrigin);
				}

				if (houseBill != null && houseBill.LoosePackageCount > 0)
				{
					var packingGroup = houseBill.PackingGroups.GetElementWithNoContainer() as PackingGroup;
					if (packingGroup?.Packages.FirstOrDefault() is Package package)
					{
						GenerateGroup14(parent, index + 1,
							package.CW_PackQty, package.CW_PackType,
							declaration.JE_GoodsDescription,
							houseBill.ECI_ApportionedLoosePackageWeight,
							houseBill.ECI_ApportionedLoosePackageValue, invoiceCurrency,
							ZString.Empty,
							declaration.JE_RL_NKOrigin);
					}
				}
			}
		}

		void GenerateGroup11(SegmentGroup8 parent, NADLineFormatter partyOrgHeader, PartyQualifierList partyQualifier)
		{
			if (partyOrgHeader != null)
			{
				SegmentGroup11 segmentGroup11 = parent.Group11.InstantiateAChildAndAddItToChildrenCollection();
				GenerateGroup11NAD(segmentGroup11, partyOrgHeader, partyQualifier);
			}
		}

		void GenerateGroup14(SegmentGroup8 group8, ZInt goodsItemNumber, ZInt packageQty, ZString packageUQ, ZString goodsDescription, ZDecimal weightInKG
			, ZDecimal invoiceValue, ZString invoiceCurrency, ZString containerNumber, ZString portOfOrigin)
		{
			SegmentGroup14 group14 = group8.Group14.InstantiateAChildAndAddItToChildrenCollection();
			GenerateGroup14GID(group14, goodsItemNumber, packageQty, packageUQ);
			GenerateGroup14FTX(group14, goodsDescription.ToUpper());
			GenerateGroup14MEA(group14, weightInKG);
			GenerateGroup14MOA(group14, invoiceValue, invoiceCurrency);
			GenerateGroup14SGP(group14, containerNumber);
			GenerateGroup14LOC(group14, portOfOrigin);
		}
		#endregion

		#region Child Segment Generators
		void GenerateGroup2NAD(SegmentGroup2 parent, ZString partyNameOnly, PartyQualifierList partyQualifier)
		{
			if (!partyNameOnly.IsEmpty)
			{
				NADSegment nADCarrier = parent.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nADCarrier.PartyQualifier = partyQualifier;
				nADCarrier.NameAndAddress.NameAndAddressLine1 = partyNameOnly;
			}
		}

		void GenerateGroup4TDT(SegmentGroup4 parent, ZString codedModeOfTransport, ZString vessel, ZString voyageOrFlightNo)
		{
			TDTSegment tDTSegment = parent.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tDTSegment.TransportStageQualifier = TransportStageQualifierList.MainCarriageTransport;
			tDTSegment.ModeOfTransport.ModeOfTransportCoded = codedModeOfTransport; // Not Present as a Code List in the UN Spec.
			if (codedModeOfTransport == "1")
			{
				tDTSegment.ConveyanceReferenceNumber = voyageOrFlightNo.Left(8);
				tDTSegment.TransportIdentification.IdOfTheMeansOfTransport = vessel.Left(30);
			}
			else
			{
				tDTSegment.TransportIdentification.IdOfTheMeansOfTransport = voyageOrFlightNo.Left(7);
			}
		}

		void GenerateGroup4LOC(SegmentGroup4 parent, ZString barrierPort)
		{
			LOCSegment lOCSegment = parent.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOCSegment.PlaceLocationQualifier = PlaceLocationQualifierList.CustomsOfficeOfClearance; // Port of Arrival/Departure in NZ EDIFACT Spec
			lOCSegment.LocationIdentification.PlaceLocationIdentification = barrierPort;
		}

		void GenerateGroup4DTM(SegmentGroup4 parent, ZDateTime barrierDate)
		{
			DTMSegment dTMSegment = parent.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dTMSegment.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.ArrivalDateTimeEstimated;
			dTMSegment.DateTimePeriod.DateTimePeriod = barrierDate.ToString("yyyyMMdd");
			dTMSegment.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmdd;
		}

		void GenerateGroup5EQD(SegmentGroup5 parent, ZString containerNo, ZString containerSize, ZString containerMode)
		{
			EQDSegment eQDSegment = parent.EQD.InstantiateAChildAndAddItToChildrenCollection();
			eQDSegment.EquipmentQualifier = EquipmentQualifierList.Container;
			eQDSegment.EquipmentIdentification.EquipmentIdentificationNumber = containerNo;
			eQDSegment.EquipmentSizeAndType.EquipmentSizeAndTypeIdentification = EquipmentSizeAndTypeIdentificationList.GetFromString(containerSize);
			eQDSegment.FullEmptyIndicatorCoded = GetFullEmptyIndicatorCodedList(containerMode);
		}

		void GenerateGroup5GIS(SegmentGroup5 parent, ZString processingIndicator, CodeListQualifierList codeListQualifier)
		{
			if (!processingIndicator.IsEmpty)
			{
				GISSegment gISProhibitedPackingIndicator = parent.GIS.InstantiateAChildAndAddItToChildrenCollection();
				gISProhibitedPackingIndicator.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.GetFromString(processingIndicator);
				gISProhibitedPackingIndicator.ProcessingIndicator.CodeListQualifier = codeListQualifier;
				gISProhibitedPackingIndicator.ProcessingIndicator.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;
			}
		}

		void GenerateGroup7CNI(SegmentGroup7 parent, ZString consignmentNumber)
		{
			CNISegment cNISegment = parent.CNI.InstantiateAChildAndAddItToChildrenCollection();
			cNISegment.ConsolidationItemNumber = consignmentNumber;
		}

		void GenerateGroup8RFF(SegmentGroup8 parent, ZString houseBill)
		{
			RFFSegment rFFSegment = parent.RFF.InstantiateAChildAndAddItToChildrenCollection(); //M1
			rFFSegment.Reference.ReferenceQualifier = ReferenceQualifierList.HouseWaybillNumber;
			rFFSegment.Reference.ReferenceNumber = houseBill;
		}

		void GenerateGroup8LOC(SegmentGroup8 parent, ZString locationCode, PlaceLocationQualifierList placeLocationQualifier)
		{
			if (!locationCode.IsEmpty)
			{
				LOCSegment lOCPortOfLoading = parent.LOC.InstantiateAChildAndAddItToChildrenCollection(); //C6
				lOCPortOfLoading.PlaceLocationQualifier = placeLocationQualifier;
				lOCPortOfLoading.LocationIdentification.PlaceLocationIdentification = locationCode;
			}
		}

		void GenerateGroup11NAD(SegmentGroup11 parent, NADLineFormatter fullAddress, PartyQualifierList partyQualifier)
		{
			NADSegment nadSegment = parent.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nadSegment.PartyQualifier = partyQualifier;
			nadSegment.NameAndAddress.NameAndAddressLine1 = fullAddress.GetLine(0);
			nadSegment.NameAndAddress.NameAndAddressLine2 = fullAddress.GetLine(1);
			nadSegment.NameAndAddress.NameAndAddressLine3 = fullAddress.GetLine(2);
			nadSegment.NameAndAddress.NameAndAddressLine4 = fullAddress.GetLine(3);
			nadSegment.NameAndAddress.NameAndAddressLine5 = fullAddress.GetLine(4);
		}

		void GenerateGroup14GID(SegmentGroup14 parent, ZInt goodsItemNumber, ZInt packageQty, ZString packageUQ)
		{
			GIDSegment gIDSegment = parent.GID.InstantiateAChildAndAddItToChildrenCollection();
			gIDSegment.GoodsItemNumber = goodsItemNumber.ToString();
			gIDSegment.NumberAndTypeOfPackages1.NumberOfPackages = packageQty.ToString();
			gIDSegment.NumberAndTypeOfPackages1.TypeOfPackagesIdentification = packageUQ.Left(2);
		}

		void GenerateGroup14FTX(SegmentGroup14 parent, ZString goodsDescription)
		{
			FTXSegment fTXSegment = parent.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTXSegment.TextSubjectQualifier = TextSubjectQualifierList.GoodsDescription;
			if (goodsDescription.IsEmpty)
			{
				fTXSegment.TextLiteral.FreeText1 = new UNOACharacterSet().EscapeCharacter + " ";
			}
			else
			{
				StringLineBreaker goodsDescriptionLineBreaker = new StringLineBreaker(goodsDescription);
				fTXSegment.TextLiteral.FreeText1 = goodsDescriptionLineBreaker.GetNextLine(70);
				fTXSegment.TextLiteral.FreeText2 = goodsDescriptionLineBreaker.GetNextLine(70);
				fTXSegment.TextLiteral.FreeText3 = goodsDescriptionLineBreaker.GetNextLine(70);
				fTXSegment.TextLiteral.FreeText4 = goodsDescriptionLineBreaker.GetNextLine(40);
			}
		}

		void GenerateGroup14MEA(SegmentGroup14 parent, ZDecimal weightInKG)
		{
			MEASegment mEASegment = parent.MEA.InstantiateAChildAndAddItToChildrenCollection();
			mEASegment.MeasurementPurposeQualifier = MeasurementPurposeQualifierList.Weights;
			mEASegment.MeasurementDetails.PropertyMeasuredCoded = PropertyMeasuredCodedList.X_GrossWeightItemLevel;
			mEASegment.ValueRange.MeasureUnitQualifier = StatisticalUQList.Codes.Kilograms; // Only Accepts KGM.
			mEASegment.ValueRange.MeasurementValue = weightInKG.ToString(3);
		}

		void GenerateGroup14MOA(SegmentGroup14 parent, ZDecimal invoiceValue, ZString invoiceCurrency)
		{
			MOASegment mOASegment = parent.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOASegment.MonetaryAmount.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.AmountTargetCurrency;
			mOASegment.MonetaryAmount.MonetaryAmount = invoiceValue.ToString(2);
			mOASegment.MonetaryAmount.CurrencyCoded = invoiceCurrency;
		}

		void GenerateGroup14SGP(SegmentGroup14 parent, ZString containerNo)
		{
			if (!containerNo.IsEmpty)
			{
				SGPSegment sGPSegment = parent.SGP.InstantiateAChildAndAddItToChildrenCollection();
				sGPSegment.EquipmentIdentification.EquipmentIdentificationNumber = containerNo;
			}
		}

		void GenerateGroup14LOC(SegmentGroup14 parent, ZString portOfOrigin)
		{
			if (!portOfOrigin.IsEmpty)
			{
				LOCSegment lOCSegment = parent.LOC.InstantiateAChildAndAddItToChildrenCollection();
				lOCSegment.PlaceLocationQualifier = PlaceLocationQualifierList.GoodsReceiptPlace;
				lOCSegment.LocationIdentification.PlaceLocationIdentification = portOfOrigin;
			}
		}
		#endregion

		#region Coded List Converters
		MessageFunctionCodedList GetMessageFunction()
		{
			MessageFunctionCodedList result = null;
			switch (messageType)
			{
				case MessageTypes.Original:
					result = MessageFunctionCodedList.Original;
					break;
				case MessageTypes.ReplaceHeader:
					result = MessageFunctionCodedList.ReplaceHeadingSectionOnly;
					break;
				case MessageTypes.ReplaceConsignment:
					result = MessageFunctionCodedList.ReplaceItemDetailAndSummaryOnly;
					break;
				case MessageTypes.CancelECI:
					result = MessageFunctionCodedList.Cancellation;
					break;
			}
			return result;
		}

		FullEmptyIndicatorCodedList GetFullEmptyIndicatorCodedList(string containerMode)
		{
			FullEmptyIndicatorCodedList result;
			switch (containerMode)
			{
				case ContainerModeList.Codes.FCL:
					result = FullEmptyIndicatorCodedList.Full;
					break;
				case ContainerModeList.Codes.LCL:
					result = FullEmptyIndicatorCodedList.FullMixedConsignment;
					break;
				case ContainerModeList.Codes.Bulk:
					result = FullEmptyIndicatorCodedList.FullSingleConsignment;
					break;
				default:
					result = FullEmptyIndicatorCodedList.Empty;
					break;
			}
			return result;
		}
		#endregion
	}
}
