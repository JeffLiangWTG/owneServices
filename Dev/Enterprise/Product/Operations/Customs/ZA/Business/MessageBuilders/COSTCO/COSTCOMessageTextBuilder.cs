using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Edifact.D16A.Messages.COSTCO;
using Enterprise.Edifact.D16A.Segments;
using Enterprise.Messaging.Business;
using COSTCOMessage = Enterprise.Edifact.D16A.Messages.COSTCO.COSTCOMessage;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	internal class COSTCOMessageTextBuilder
	{
		public COSTCOMessageTextBuilder(COSTCOMessage edifactMessage, ICOSTCOMessageDataProvider source, MessageSubTypes subType)
		{
			this.edifactMessage = edifactMessage;
			this.source = source;
			this.subType = subType;
		}

		internal void Create()
		{
			PopulateUNH(edifactMessage.UNH[0]);
			PopulateBGM(edifactMessage.BGM[0], source, subType);
			PopulateDTMs(edifactMessage.DTM, source);
			PopulateFTXs(edifactMessage.FTX, source);
			if (subType.IsAmendmentOrCancellation())
			{
				AddNewRFF(edifactMessage.Group1[0].RFF, ReferenceCodeQualifierList.ReferenceNumberToPreviousMessage, source.DocumentToBeAmended);
			}
			PopulateTDT(edifactMessage.Group2[0].TDT[0], source);
			PopulatedRFFs(edifactMessage.Group2[0].RFF, source);
			PopulateLOCs(edifactMessage.Group2[0].Group3[0].LOC, source);
			PopulateNADs(edifactMessage.Group4, source);
			PopulateContainers(edifactMessage.Group6, source);
			PopulateBills(edifactMessage.Group10, source);
			PopulateCNT(edifactMessage.CNT[0], source);
			PopulateUNT(edifactMessage.UNT[0]);
		}

		static void PopulateUNH(UNHSegment unh)
		{
			unh.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unh.MessageIdentifier.MessageType = "COSTCO";
			unh.MessageIdentifier.MessageVersionNumber = "D";
			unh.MessageIdentifier.MessageReleaseNumber = "16A";
			unh.MessageIdentifier.ControllingAgency = "UN";
			unh.MessageIdentifier.AssociationAssignedCode = "RCG001";
		}

		static void PopulateBGM(BGMSegment bgm, ICOSTCOMessageDataProvider source, MessageSubTypes subType)
		{
			bgm.DocumentMessageName.DocumentNameCode = Enterprise.Edifact.D16A.Elements.DocumentNameCodeList.ContainerManifestUnitPackingList;
			bgm.DocumentMessageName.DocumentName = source.OutturnManifestType;
			bgm.DocumentMessageIdentification.DocumentIdentifier = EDIMessage.SystemCommonAccessReferencePkPlaceholder; // "A unique reference number assigned to the electronic document." - treat this as a common access reference - SYS-CAR
			bgm.MessageFunctionCode = MessageFunctionCodeList.GetFromString(subType.GetMessageFunction_D16A());
		}

		void PopulateDTMs(DTMSegmentMessageSection dtm, ICOSTCOMessageDataProvider headerInformation)
		{
			AddNewDTM(dtm, headerInformation.DocumentIssueDateTime, DateOrTimeOrPeriodFunctionCodeQualifierList.DocumentIssueDateTime, DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm);

			if (source.IsImport)
			{
				AddNewDTM(dtm, headerInformation.ActualArrivalDateTime, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeEstimated, DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm);
			}
			else if (source.IsExport)
			{
				AddNewDTM(dtm, headerInformation.EstimatedDateOfDeparture, DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeEstimated, DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm);
			}

			if (!source.IsDOR)
			{
				AddNewDTM(dtm, headerInformation.DateTimeFullyUnloadedLoaded, DateOrTimeOrPeriodFunctionCodeQualifierList.ConveyancePortActivityDateTime, DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm);
			}
		}

		static void AddNewDTM(DTMSegmentMessageSection dtmSection, ZDateTime dateTime, DateOrTimeOrPeriodFunctionCodeQualifierList dateTimeType, DateOrTimeOrPeriodFormatCodeList dateTimeFormat, string format = "yyyyMMddHHmm")
		{
			if (dateTime.IsValid)
			{
				var dtm = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
				dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = dateTimeType;
				dtm.DateTimePeriod.DateOrTimeOrPeriodText = dateTime.ToString(format, CultureInfo.InvariantCulture);
				dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = dateTimeFormat;
			}
		}

		void PopulateFTXs(FTXSegmentMessageSection ftxSection, ICOSTCOMessageDataProvider headerInformation)
		{
			if (source.IsVOR || source.IsEOR)
			{
				AddNewFTX(ftxSection, TextSubjectCodeQualifierList.DiscrepancyInformation, headerInformation.ExcessIndicator);
			}
			AddNewFTX(ftxSection, TextSubjectCodeQualifierList.FieldOfApplication, headerInformation.ImportExportTranshipmentIndicator);
		}

		static void AddNewFTX(FTXSegmentMessageSection ftxSection, TextSubjectCodeQualifierList textSubjectCodeQualifier, string freeTextDescriptionCode = null, string freeText = null)
		{
			var ftx = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
			ftx.TextSubjectCodeQualifier = textSubjectCodeQualifier;
			if (freeTextDescriptionCode != null)
			{
				ftx.TextReference.FreeTextDescriptionCode = freeTextDescriptionCode;
			}

			if (freeText != null)
			{
				ftx.TextLiteral.FreeText1 = freeText;
			}
		}

		void PopulateTDT(TDTSegment tdt, ICOSTCOMessageDataProvider headerInformation)
		{
			tdt.TransportStageCodeQualifier = TransportStageCodeQualifierList.MainCarriageTransport;
			tdt.MeansOfTransportJourneyIdentifier = headerInformation.VoyageFlightNumber;
			tdt.ModeOfTransport.TransportModeNameCode = headerInformation.TransportCode;
			tdt.Carrier.CarrierIdentifier = headerInformation.CarrierCode;
			tdt.Carrier.CodeListIdentificationCode = "172";
			tdt.Carrier.CodeListResponsibleAgencyCode = source.IsAir ? CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation : CodeListResponsibleAgencyCodeList.BicBureauInternationalDesContaineurs;
			if (!source.IsALD)
			{
				var callSign = headerInformation.CallSign;
				if (!source.IsAOR && !source.IsEOR || !callSign.IsEmpty)
				{
					tdt.TransportIdentification.TransportMeansIdentificationNameIdentifier = callSign;
				}
				tdt.TransportIdentification.CodeListIdentificationCode = "103";
			}
		}

		void PopulatedRFFs(RFFSegmentMessageSection rffSection, ICOSTCOMessageDataProvider headerInformation)
		{
			if (source.IsBBB || source.IsAOR || source.IsALD)
			{
				AddNewRFF(rffSection, ReferenceCodeQualifierList.CargoManifestNumber, headerInformation.ManifestType);
			}
			AddNewRFF(rffSection, ReferenceCodeQualifierList.PrincipalReferenceNumber, headerInformation.PrincipalCarrierConveyageNumber);
		}

		static void AddNewRFF(RFFSegmentMessageSection rffSection, ReferenceCodeQualifierList referenceCodeQualifier, string referenceIdentifier, string versionIdentifier = null)
		{
			var rff = rffSection.InstantiateAChildAndAddItToChildrenCollection();
			rff.Reference.ReferenceCodeQualifier = referenceCodeQualifier;
			rff.Reference.ReferenceIdentifier = referenceIdentifier;
			if (versionIdentifier != null)
			{
				rff.Reference.VersionIdentifier = versionIdentifier;
			}
		}

		void PopulateLOCs(LOCSegmentMessageSection locSection, ICOSTCOMessageDataProvider headerInformation)
		{
			if (source.IsImport)
			{
				AddNewLOC(locSection, LocationFunctionCodeQualifierList.PlaceOfDischarge, headerInformation.PlaceOfDicharge, headerInformation.TerminalDepotCode);
			}
			else if (source.IsExport)
			{
				AddNewLOC(locSection, LocationFunctionCodeQualifierList.PlaceOfLoading, headerInformation.PlaceOfLoading, headerInformation.TerminalBerth);
			}
		}

		void AddNewLOC(LOCSegmentMessageSection locSection, LocationFunctionCodeQualifierList locationFunctionCodeQualifier, ZString unloco, ZString firstRelatedLocationIdentifier)
		{
			var loc = locSection.InstantiateAChildAndAddItToChildrenCollection();
			loc.LocationFunctionCodeQualifier = locationFunctionCodeQualifier;
			loc.LocationIdentification.LocationIdentifier = UnlocoToIata(unloco);
			loc.LocationIdentification.CodeListIdentificationCode = "139";
			loc.LocationIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope;
			if (source.IsBBB || source.IsALD)
			{
				loc.RelatedLocationOneIdentification.FirstRelatedLocationIdentifier = firstRelatedLocationIdentifier;
			}

			loc.RelatedLocationOneIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.MutuallyDefined;
		}

		static void PopulateNADs(SegmentGroup4MessageSection sg4Section, ICOSTCOMessageDataProvider source)
		{
			AddNewNAD(sg4Section, PartyFunctionCodeQualifierList.DocumentMessageIssuerSender, source.MessageSender);
			AddNewNAD(sg4Section, PartyFunctionCodeQualifierList.ReportingCarrierCustoms, source.OutturnProviderCode);
		}

		static void AddNewNAD(SegmentGroup4MessageSection sg4Section, PartyFunctionCodeQualifierList partyFunctionCodeQualifier, string partyIdentifier)
		{
			var sg4 = sg4Section.InstantiateAChildAndAddItToChildrenCollection();
			var nad = sg4.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nad.PartyFunctionCodeQualifier = partyFunctionCodeQualifier;
			nad.PartyIdentificationDetails.PartyIdentifier = partyIdentifier;
			nad.PartyIdentificationDetails.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.MutuallyDefined;
		}

		void PopulateContainers(SegmentGroup6MessageSection sg6Section, ICOSTCOMessageDataProvider headerInformation)
		{
			if (headerInformation.Containers != null)
			{
				foreach (var containerInformation in headerInformation.Containers)
				{
					PopulateContainer(sg6Section, containerInformation);
				}
			}
		}

		void PopulateContainer(SegmentGroup6MessageSection sg6Section, ICOSTCOContainerInformation containerInformation)
		{
			var sg6 = sg6Section.InstantiateAChildAndAddItToChildrenCollection();
			PopulateEQD(sg6.EQD[0], containerInformation);
			PopulateDTMs(sg6.DTM, containerInformation);
			PopulateSEL(sg6.SEL[0], containerInformation);
		}

		void PopulateEQD(EQDSegment eqd, ICOSTCOContainerInformation containerInformation)
		{
			eqd.EquipmentTypeCodeQualifier = EquipmentTypeCodeQualifierList.GetFromString(containerInformation.EquipmentType);
			if (!source.IsBBB)
			{
				eqd.EquipmentIdentification.EquipmentIdentifier = containerInformation.ContainerNumber;
			}

			if (source.IsDOR)
			{
				eqd.EquipmentSizeAndType.EquipmentSizeAndTypeDescriptionCode = EquipmentSizeAndTypeDescriptionCodeList.GetFromString(containerInformation.ContainerSize);
				eqd.EquipmentSizeAndType.CodeListIdentificationCode = "102";
				eqd.EquipmentSizeAndType.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization;
				eqd.EquipmentStatusCode = EquipmentStatusCodeList.GetFromString(containerInformation.ContainerStatusLandedPurpose);
				eqd.FullOrEmptyIndicatorCode = FullOrEmptyIndicatorCodeList.GetFromString(containerInformation.ServiceType);
			}
		}

		void PopulateDTMs(DTMSegmentMessageSection dtmSection, ICOSTCOContainerInformation containerInformation)
		{
			if (!source.IsBBB && !source.IsALD)
			{
				AddNewDTM(dtmSection, containerInformation.DateUnpacked, DateOrTimeOrPeriodFunctionCodeQualifierList.ProcessingEndDateTime, DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm);
			}

			AddNewDTM(dtmSection, containerInformation.DateTimeFullyUnloaded, DateOrTimeOrPeriodFunctionCodeQualifierList.UnloadedDateAndTime, DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm);
		}

		static void PopulateSEL(SELSegment sel, ICOSTCOContainerInformation containerInformation)
		{
			sel.TransportUnitSealIdentifier = containerInformation.SealNumber;
			sel.SealIssuer.SealingPartyNameCode = SealingPartyNameCodeList.GetFromString(containerInformation.SealingParty);
			sel.SealConditionCode = SealConditionCodeList.GetFromString(containerInformation.SealStatus);
		}

		void PopulateBills(SegmentGroup10MessageSection sg10Section, ICOSTCOMessageDataProvider headerInformation)
		{
			if (headerInformation.Bills != null)
			{
				foreach (var billInformation in headerInformation.Bills)
				{
					PopulateBill(sg10Section, billInformation);
				}
			}
		}

		void PopulateBill(SegmentGroup10MessageSection sg10Section, ICOSTCOLineLevelInformation billInformation)
		{
			var sg10 = sg10Section.InstantiateAChildAndAddItToChildrenCollection();
			PopulateCNI(sg10.CNI[0], billInformation);
			PopulatedRFFs(sg10.RFF, billInformation);
			PopulatePacks(sg10.Group11, billInformation);
		}

		void PopulateCNI(CNISegment cni, ICOSTCOLineLevelInformation billInformation)
		{
			if (!source.IsVOR && !source.IsEOR)
			{
				cni.ConsolidationItemNumber = billInformation.LineNumber;
				cni.DocumentMessageDetails.DocumentIdentifier = billInformation.TransportDocumentNumber;
			}
		}

		void PopulatedRFFs(RFFSegmentMessageSection rffSection, ICOSTCOLineLevelInformation billInformation)
		{
			if (!source.IsBBB)
			{
				AddNewRFF(rffSection, ReferenceCodeQualifierList.FreightForwarderNumber, billInformation.CargoCarrierCode);
			}

			AddNewRFF(rffSection, ReferenceCodeQualifierList.StandardCarrierAlphaCodeScacNumber, billInformation.MasterCargoCarrierCode);

			var externalReference = billInformation.ExternalReference;
			if (!externalReference.IsEmpty)
			{
				AddNewRFF(rffSection, ReferenceCodeQualifierList.CommonTransactionReferenceNumber, externalReference);
			}

			if (!source.IsVOR && !source.IsEOR)
			{
				AddNewRFF(rffSection, ReferenceCodeQualifierList.MasterBillOfLadingNumber, billInformation.MasterBillOfLadingNumber, billInformation.ConsolidationIndicator);
			}

			if (source.IsExport && (source.IsDOR || source.IsBBB || source.IsAOR))
			{
				AddNewRFF(rffSection, ReferenceCodeQualifierList.GoodsDeclarationDocumentIdentifierCustoms, billInformation.LRNExit, billInformation.ExportProcedure);
			}
		}

		void PopulatePacks(SegmentGroup11MessageSection sg11Section, ICOSTCOLineLevelInformation billInformation)
		{
			if (billInformation.Packs != null)
			{
				foreach (var packInformation in billInformation.Packs)
				{
					PopulatePack(sg11Section, packInformation);
				}
			}
		}

		void PopulatePack(SegmentGroup11MessageSection sg11Section, ICOSTCOPackLineInformation packInformation)
		{
			var sg11 = sg11Section.InstantiateAChildAndAddItToChildrenCollection();
			PopulateGID(sg11.GID[0], packInformation);
			PopulateFTXs(sg11.FTX, packInformation);
			PopulateMEAs(sg11.MEA, packInformation);
			PopulatePCI(sg11.PCI[0], packInformation);
			PopulateSGP(sg11.Group13[0].SGP[0], packInformation);
		}

		void PopulateGID(GIDSegment gid, ICOSTCOPackLineInformation packInformation)
		{
			gid.GoodsItemNumber = packInformation.GoodsLineNumber;
			if (!source.IsVOR && !source.IsEOR)
			{
				gid.NumberAndTypeOfPackages1.PackageQuantity = packInformation.NumberOfPackages.ToString();
			}
			gid.NumberAndTypeOfPackages1.PackageTypeDescriptionCode = packInformation.TypeOfPackages;
			gid.NumberAndTypeOfPackages1.TypeOfPackages = packInformation.CargoTypeIndicator;
			gid.NumberAndTypeOfPackages2.PackageQuantity = packInformation.NumberOfPackagesPackedUnpacked.ToString();
		}

		void PopulateFTXs(FTXSegmentMessageSection ftxSection, ICOSTCOPackLineInformation packInformation)
		{
			if (!packInformation.PackageCondition.IsEmpty)
			{
				AddNewFTX(ftxSection, TextSubjectCodeQualifierList.DamageRemarks, packInformation.PackageCondition, packInformation.ConditionDescription);
			}

			if ((source.IsDOR || source.IsBBB || source.IsAOR))
			{
				var contentsFoundToBe = packInformation.ContentsFoundToBe;
				if (!contentsFoundToBe.IsEmpty)
				{
					AddNewFTX(ftxSection, TextSubjectCodeQualifierList.PackageContentsDescription, null, contentsFoundToBe);
				}
			}

			if (!source.IsVOR && !source.IsEOR)
			{
				AddNewFTX(ftxSection, TextSubjectCodeQualifierList.TextRefersToExpectedData, packInformation.ExcessShortIndicator, packInformation.ContentsShouldToBe);
			}

			AddNewFTX(ftxSection, TextSubjectCodeQualifierList.GoodsItemDescription, null, packInformation.DescriptionOfGoods);
		}

		void PopulateMEAs(MEASegmentMessageSection meaSection, ICOSTCOPackLineInformation packInformation)
		{
			if (!source.IsVOR && !source.IsEOR)
			{
				AddNewMEA(meaSection, MeasurementPurposeCodeQualifierList.Measurement, MeasuredAttributeCodeList.GoodsItemGrossWeight, "KGM", packInformation.GrossWeightInKilograms);
			}

			AddNewMEA(meaSection, MeasurementPurposeCodeQualifierList.CargoLoaded, MeasuredAttributeCodeList.AscertainedWeight, "KGM", packInformation.GrossWeightFoundInKilograms);
			if ((source.IsDOR || source.IsBBB || source.IsAOR) && !packInformation.VolumeInLitres.IsEmpty)
			{
				AddNewMEA(meaSection, MeasurementPurposeCodeQualifierList.Measurement, MeasuredAttributeCodeList.Volume, "LTR", packInformation.VolumeInLitres);
			}

			if (!source.IsEOR && !source.IsALD && !packInformation.VolumeOutturnedInLitres.IsEmpty)
			{
				AddNewMEA(meaSection, MeasurementPurposeCodeQualifierList.CargoLoaded, MeasuredAttributeCodeList.AscertainedVolume, "LTR", packInformation.VolumeOutturnedInLitres);
			}
		}

		static void AddNewMEA(MEASegmentMessageSection meaSection, MeasurementPurposeCodeQualifierList measurementPurposeCodeQualifier, MeasuredAttributeCodeList measuredAttributeCode, string measurementUnitCode, ZDecimal value)
		{
			var mea = meaSection.InstantiateAChildAndAddItToChildrenCollection();
			mea.MeasurementPurposeCodeQualifier = measurementPurposeCodeQualifier;
			mea.MeasurementDetails.MeasuredAttributeCode = measuredAttributeCode;
			mea.ValueRange.MeasurementUnitCode = measurementUnitCode;
			mea.ValueRange.Measure = value.ToString();
		}

		static void PopulatePCI(PCISegment pci, ICOSTCOPackLineInformation packInformation)
		{
			pci.MarkingInstructionsCode = MarkingInstructionsCodeList.ShipperAssigned;
			var marksAndNumbers = packInformation.MarksAndNumbers;
			pci.MarksLabels.ShippingMarksDescription1 = marksAndNumbers.SubstringSafe(0, 35);
			const int chunkSize = 35;
			for (var i = 1; i < 10; i++)
			{
				if (marksAndNumbers.Length > i * chunkSize)
				{
					var description = marksAndNumbers.SubstringSafe(i * chunkSize, chunkSize);
					if (!description.IsEmpty)
					{
						CargoWise.Common.ReflectionUtil.SetFieldValue(pci.MarksLabels, System.FormattableString.Invariant($"ShippingMarksDescription{i + 1}"), description.ToString());
					}
				}
				else
				{
					break;
				}
			}
		}

		void PopulateSGP(SGPSegment sGP, ICOSTCOPackLineInformation packInformation)
		{
			sGP.EquipmentIdentification.EquipmentIdentifier = packInformation.ContainerNumber;
			var packageContent = packInformation.ContainerPackageContent;
			if (source.IsDOR || !source.IsBBB && !source.IsAOR && !packageContent.IsEmpty)
			{
				sGP.PackageQuantity = packageContent.ToString();
			}
		}

		static void PopulateCNT(CNTSegment cnt, ICOSTCOMessageDataProvider source)
		{
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.TotalPieces;
			cnt.Control.ControlTotalQuantity = source.TotalNumberOfPackages.ToString();
		}

		void PopulateUNT(UNTSegment unt)
		{
			unt.NumberOfSegmentsInTheMessage = edifactMessage.CountIncludingUNT.ToString(CultureInfo.InvariantCulture);
			unt.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		ZString UnlocoToIata(ZString unloco)
		{
			return MessageBuilderHelper.UnlocoToIata(Factory, source.IsAir, unloco);
		}

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		readonly MessageSubTypes subType;
		readonly COSTCOMessage edifactMessage;
		readonly ICOSTCOMessageDataProvider source;
	}
}
