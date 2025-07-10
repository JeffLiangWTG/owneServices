using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Edifact.D96BNZ.Elements;
using Enterprise.Edifact.D96BNZ.Messages.CUSDEC;
using Enterprise.Edifact.D96BNZ.Segments;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry
{
	public class MessageBuilder : EdifactMessageBuilderFromEntryHeader
	{
		public enum MessageTypes { None = 0, CancelEntry = 1, AddLine = 2, CancelLine = 3, Replacement = 5, Original = 9, ReplaceHeader = 20, ReplaceLines = 21, CompletionEntry = 22 }

		public MessageBuilder(Declaration.CusEntryHeader entryHeader, MessageTypes messageType)
			: base(entryHeader)
		{
			this.entryHeader = (CusEntryHeader)entryHeader;
			declaration = entryHeader.Declaration;
			this.messageType = messageType;
		}
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly MessageTypes messageType;

		protected new CUSDECMessage EDIFACTMessage
		{
			get { return (CUSDECMessage)base.EDIFACTMessage; }
		}

		protected override Edifact.Auto.SegmentGroup GetNewEDIFACTMessage()
		{
			return new CUSDECMessage();
		}

		protected override void SetMessageType()
		{
			message.EM_MessageType = NZCMessage.MessageTypes.FormalEntry.MessageType;
		}

		protected override void SetMessageSubType()
		{
			switch (messageType)
			{
				case MessageTypes.CancelEntry:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Cancellation;
					break;
				case MessageTypes.Replacement:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Replacement;
					break;
				case MessageTypes.Original:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Original;
					break;
				case MessageTypes.CompletionEntry:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Completion;
					break;

				// The following message types are deprecated, but have been left in place in case Customs need them back for anything. They are sill in the spec and able to be used.
				case MessageTypes.ReplaceHeader:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.ReplaceHeader;
					break;
				case MessageTypes.ReplaceLines:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.ReplaceLines;
					break;
				case MessageTypes.AddLine:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.AddLine;
					break;
				case MessageTypes.CancelLine:
					message.EM_MessageSubType = NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.CancelLine;
					break;
			}
		}

		protected override ZDateTime GetHeldUntilDate() => CalculateMessageHeldDate(declaration);

		protected override void SetParentMessagingStatusAfterMessagePosting()
		{
			using (new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(entryHeader.Declaration))
			{
				ZString newEntryStatus = message.EM_HeldUntilDate.IsEmpty ? FormalEntryStatusList.Codes.SentToCustoms : FormalEntryStatusList.Codes.QueuedForSending;
				entryHeader.CH_EntryStatus = newEntryStatus;
				entryHeader.CH_LastEntryStyle = declaration.JE_MessageSubType;
				entryHeader.CH_LastNumberOfLinesSentToCustoms = lastNumberOfLinesSentToCustoms;
				declaration.JE_EntryStatus = newEntryStatus;
				declaration.JE_EntrySubmittedDate = message.EM_HeldUntilDate.IsEmpty ? declaration.CachedTodaysDate : ZDateTime.Empty;
				declaration.LogCustomsCommencedIfNeeded();
			}
		}

		#region Group Generators

		protected override void GenerateGroup0()
		{
			totalPackages = 0;
			lastNumberOfLinesSentToCustoms = entryHeader.CH_LastNumberOfLinesSentToCustoms;
			GenerateGroup0UNH(EDIFACTMessage);
			GenerateGroup0BGM(EDIFACTMessage);

			if (MessageTypeIncludesHeaders)
			{
				GenerateGroup0Header(EDIFACTMessage);
			}

			GenerateGroup0FTX(EDIFACTMessage);

			if (MessageTypeIncludesHeaders)
			{
				if (!declaration.IsPeriodic)
				{
					GenerateGroup1(EDIFACTMessage); // House Bills/Containers/Packaging
				}
				GenerateGroup4(EDIFACTMessage); // Transport Details
				GenerateGroup5(EDIFACTMessage); // Permit Authority Codes
				GenerateGroup6(EDIFACTMessage); // Client/Broker/Delivery Authority
				if (declaration.IsImport && !declaration.IsPeriodic)
				{
					GenerateGroup10(EDIFACTMessage); // Invoice Details
				}
			}

			EDIFACTMessage.UNS1.InstantiateAChildAndAddItToChildrenCollection().SectionIdentification = SectionIdentificationList.HeaderDetailSectionSeparation;

			if (MessageTypeIncludesLines)
			{
				GenerateGroup30(EDIFACTMessage);
			}

			EDIFACTMessage.UNS2.InstantiateAChildAndAddItToChildrenCollection().SectionIdentification = SectionIdentificationList.DetailSummarySectionSeparation;

			if (MessageTypeIncludesHeaders)
			{
				if (declaration.IsImport && !declaration.IsPeriodic)
				{
					GenerateGroup0CNT(ControlQualifierList.NumberOfInvoiceLines, declaration.Invoices.Count.ToString());
				}
				if (messageType != MessageTypes.ReplaceHeader)
				{
					GenerateGroup0CNT(ControlQualifierList.NumberOfCustomsItemDetailLines, entryHeader.MergedLines.Count.ToString());
				}
				if (!declaration.IsPeriodic)
				{
					GenerateGroup0CNT(ControlQualifierList.TotalNumberOfPackages, totalPackages.ToString());
				}
			}

			if (MessageTypeIncludesLines)
			{
				DutyTaxFeeFunctionQualifierList totals = DutyTaxFeeFunctionQualifierList.TotalOfEachDutyTaxOrFeeTypeCustomsDeclaration;
				DutyTaxFeeFunctionQualifierList grandTotal = DutyTaxFeeFunctionQualifierList.TotalOfAllDutiesTaxesAndFeeTypesCustomsDeclaration;

				if (declaration.IsImport)
				{
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.AlacAlcoholLevy, entryHeader.ALACLevyAmount);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.HeraSteelLevy, entryHeader.HERALevyAmount);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.AccFuelLevy, entryHeader.ACCFuelLevyAmount);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.PfmlFuelLevy, entryHeader.PFMLFuelLevyAmount);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.SggSyntheticGreenhouseGasesLevy, entryHeader.SyntheticGreenhouseGasesLevyAmount);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.AntiDumpingDuty, null, entryHeader.AntiDumpingDutyAmount);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CountervailingDuty, null, entryHeader.CountervailingDutyAmount);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CustomsDuty, null, entryHeader.DutyAmount, entryHeader.VFDWholeNZD, 0m, 0m, ZString.Empty);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.GoodsAndServicesTax, null, entryHeader.GSTAmount);
				}
				else if (declaration.IsExport && (declaration.IsDrawback || declaration.IsCompletion))
				{
					if (declaration.IsDrawback || !entryHeader.DutyCreditAmount.IsEmpty)
					{
						GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CustomsDuty, null, entryHeader.DutyCreditAmount, entryHeader.VFDWholeNZD, 0m, 0m, ZString.Empty);
					}
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.AlacAlcoholLevy, entryHeader.ALACLevyCreditAmount);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.GoodsAndServicesTax, null, entryHeader.GSTCreditAmount);
				}
				else if (declaration.IsExcise)
				{
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CustomsDuty, null, entryHeader.DutyAmount, 0m, 0m, entryHeader.ExciseDutyCreditAmount, ZString.Empty);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.AlacAlcoholLevy, entryHeader.ALACLevyAmount);
					GenerateGroup49(EDIFACTMessage, totals, DutyTaxFeeTypeCodedList.GoodsAndServicesTax, null, entryHeader.GSTAmount);
				}

				var paymentMethod = ZString.Empty;

				if (entryHeader.ShouldSendPaymentMethodInMessage)
				{
					paymentMethod = declaration.JE_PaymentMethod;
				}

				if ((declaration.IsImport || declaration.IsExport) && declaration.IsCompletion)
				{
					GenerateGroup49(EDIFACTMessage, grandTotal, DutyTaxFeeTypeCodedList.Total, null, entryHeader.TotalAmountPayable, 0m, entryHeader.DepositRefundAmount, 0m, paymentMethod);
				}
				else
				{
					GenerateGroup49(EDIFACTMessage, grandTotal, DutyTaxFeeTypeCodedList.Total, null, entryHeader.TotalAmountPayable, 0m, 0m, 0m, paymentMethod);
				}
			}

			GenerateGroup50(EDIFACTMessage);

			UNTSegment uNT = EDIFACTMessage.UNT.InstantiateAChildAndAddItToChildrenCollection(); // Message Trailer.
			uNT.MessageReferenceNumber = NZCMessage.MessageNumberPlaceHolder;
			uNT.NumberOfSegmentsInTheMessage = EDIFACTMessage.CountIncludingUNT.ToString();
		}

		protected void GenerateGroup0Header(CUSDECMessage baseSegment)
		{
			CSTSegment cST = EDIFACTMessage.CST.InstantiateAChildAndAddItToChildrenCollection();
			cST.CustomsIdentityCodes1.CustomsCodeIdentification = GetCustomsCodeIdentification();
			cST.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.CustomsDeclarationType;
			cST.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;

			GenerateSegmentLOC(EDIFACTMessage.LOC, PlaceLocationQualifierList.PlacePortOfLoading, declaration.JE_RL_NKPortOfLoading);   // Port of Loading
			GenerateSegmentLOC(EDIFACTMessage.LOC, PlaceLocationQualifierList.PlacePortOfDischarge, declaration.JE_RL_NKPortOfArrival); // Port of Discharge
			if (declaration.WarehouseAddress != null)
			{
				GenerateSegmentLOC(EDIFACTMessage.LOC, PlaceLocationQualifierList.Warehouse, declaration.WarehouseAddress.LocalControlledPremisesID);            // Customs Controlled Area - Spec says n9 but gives no supporting business case.
			}
			GenerateSegmentLOC(EDIFACTMessage.LOC, PlaceLocationQualifierList.CustomsOfficeOfEntry, declaration.JE_RL_NKProcessingPort); // Processing Port
																																		 // Processing Port can be NZAKL, NZCHC, NZDUD, NZIVC, NZNSN, NZNPL, NZTRGm NZNPE, NZWLG
			if (declaration.IsExport)
			{
				GenerateSegmentLOC(EDIFACTMessage.LOC, PlaceLocationQualifierList.CountryOfDestinationOfGoods, declaration.JE_RL_NKFinalDestination.Left(2));   // Country of Destination
			}

			if (declaration.IsPeriodic)
			{
				GenerateGroup0DTM(DateTimePeriodQualifierList.ProcessingDatePeriod, declaration.JE_EntryAuthorisationDate.ToString("yyyyMM"), DateTimePeriodFormatQualifierList.Ccyymm);
			}
			else if (declaration.IsImport)
			{
				GenerateGroup0DTM(DateTimePeriodQualifierList.ImportationDate, declaration.JE_DateOfArrival.ToString("yyyyMMdd"), DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
			else if (declaration.IsExport)
			{
				GenerateGroup0DTM(DateTimePeriodQualifierList.ExportationDate, declaration.JE_ExportDate.ToString("yyyyMMdd"), DateTimePeriodFormatQualifierList.Ccyymmdd);
			}

			if (entryHeader.CH_OverrideIndicator)
			{
				GenerateSegmentGIS(EDIFACTMessage.GIS, "Y", CodeListQualifierList.CustomsProcedure, ""); // Override Entry Errors
			}
			if (declaration.IsExport)
			{
				ZString soldOrConsignedFlag = "";
				switch (declaration.JE_SoldOrConsigned)
				{
					case TermsOfSaleList.Codes.Sold:
						soldOrConsignedFlag = "S";
						break;
					case TermsOfSaleList.Codes.Consigned:
						soldOrConsignedFlag = "C";
						break;
				}
				if (!soldOrConsignedFlag.IsEmpty)
				{
					GenerateSegmentGIS(EDIFACTMessage.GIS, soldOrConsignedFlag, CodeListQualifierList.StatisticalNatureOfTransaction, ""); // Sold/Consigned
				}
			}
			foreach (OtherInfo otherInfo in declaration.OtherInfos)
			{
				if (!otherInfo.ZO_Code.IsEmpty)
				{
					GenerateSegmentGIS(EDIFACTMessage.GIS, otherInfo.ZO_Code, CodeListQualifierList.CustomsSpecialCodes, otherInfo.ZO_Data); // Other Info Code with Associated Data
				}
			}

			GenerateSegmentMEA(EDIFACTMessage.MEA, MeasurementApplicationQualifierList.Weights, MeasurementDimensionCodedList.TotalGrossWeight, StatisticalUQList.Codes.Kilograms, declaration.JE_DeclaredWeight.ToString(0));

			if (!declaration.IsPeriodic)
			{
				foreach (CusContainer container in declaration.CusContainers)
				{
					SegmentGroup99 group99 = EDIFACTMessage.Group99.InstantiateAChildAndAddItToChildrenCollection();
					EQDSegment eQD = group99.EQD.InstantiateAChildAndAddItToChildrenCollection();
					if (container.ContainerNumberIsValidPalletNumber())
					{
						eQD.EquipmentQualifier = EquipmentQualifierList.Pallet;
						eQD.EquipmentIdentification.EquipmentIdentificationNumber = container.CO_ContainerNumber.SubstringSafe(1);
					}
					else
					{
						eQD.EquipmentQualifier = EquipmentQualifierList.Container;
						eQD.EquipmentIdentification.EquipmentIdentificationNumber = container.CO_ContainerNumber;
						eQD.FullEmptyIndicatorCoded = GetFullEmptyIndicator(container);
						if (declaration.IsExport && declaration.IsExportedUnderSecureExportPartnershipScheme && container.IsFullContainer && !container.CO_Seal.IsEmpty)
						{
							SELSegment sEL = group99.SEL.InstantiateAChildAndAddItToChildrenCollection();
							sEL.SealNumber = container.CO_Seal;
						}
					}
				}
			}
		}

		protected FullEmptyIndicatorCodedList GetFullEmptyIndicator(CusContainer container)
		{
			FullEmptyIndicatorCodedList result = null;
			switch (container.CO_FCL_LCL_AIR)
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
				case ContainerModeList.Codes.Empty:
					result = FullEmptyIndicatorCodedList.Empty;
					break;
			}
			return result;
		}

		protected void GenerateGroup1(CUSDECMessage baseSegment)
		{
			if (declaration.IsAir)
			{
				if (declaration.JE_MasterBill != "")
				{
					SegmentGroup1 group1 = EDIFACTMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
					GenerateGroup1RFF(group1, ReferenceQualifierList.MasterBillOfLadingNumber, declaration.JE_MasterBill);
				}
			}
			else
			{
				if (declaration.JE_MasterBill != "") //TODO: Refactor to use the Shipping Line Booking Reference if no MAWB is available. Maybe.
				{
					SegmentGroup1 group1 = EDIFACTMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
					GenerateGroup1RFF(group1, ReferenceQualifierList.BillOfLadingNumber, declaration.JE_MasterBill);
					SegmentGroup2 group2 = group1.Group2.InstantiateAChildAndAddItToChildrenCollection();
					GenerateGroup2PAC(group2, 0, "PK"); // Put in to pass NZ Customs Dodgy Validation. They want the Master Bill, but don't have a field for it for SeaFreight. We send it as a House Bill with no Packages for Seafreight only.
				}
			}

			foreach (Bill houseBill in declaration.LowestBills)
			{
				foreach (PackingGroup packGroup in houseBill.PackingGroups)
				{
					if (packGroup.Packages.Count > 0)
					{
						SegmentGroup1 group1 = EDIFACTMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
						GenerateGroup1RFF(group1, GetRFFTypeFromDeclarationMode(), houseBill.CU_HouseBill);

						if (packGroup.Container != null)
						{
							group1 = EDIFACTMessage.Group1.InstantiateAChildAndAddItToChildrenCollection();
							if (packGroup.Container.CO_ContainerNumber.Left(1) == "P" && packGroup.Container.CO_ContainerNumber.Length < 4)
							{
								GenerateGroup1RFF(group1, ReferenceQualifierList.ShippingUnitIdentification, packGroup.Container.CO_ContainerNumber.SubstringSafe(1));
							}
							else
							{
								GenerateGroup1RFF(group1, ReferenceQualifierList.UnitLoadDeviceEGContainerIdentificationNumber, packGroup.Container.CO_ContainerNumber);
							}
						}

						SegmentGroup2 group2 = group1.Group2.InstantiateAChildAndAddItToChildrenCollection();
						foreach (Package package in packGroup.Packages)
						{
							GenerateGroup2PAC(group2, package.CW_PackQty, package.CW_PackType);
						}
					}
				}
			}
		}

		protected void GenerateGroup4(CUSDECMessage baseSegment)
		{
			SegmentGroup4 group4 = EDIFACTMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
			TDTSegment tDT = group4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tDT.TransportStageQualifier = TransportStageQualifierList.MainCarriageTransport;
			if (declaration.IsAir)
			{
				tDT.ModeOfTransport.ModeOfTransportCoded = "4";
				tDT.TransportIdentification.IdOfTheMeansOfTransport = declaration.JE_VoyageFlightNo.Left(7);
			}
			else if (declaration.IsSea)
			{
				tDT.ConveyanceReferenceNumber = declaration.JE_VoyageFlightNo.Left(8);
				tDT.ModeOfTransport.ModeOfTransportCoded = "1";
				if (declaration.Vessel != null)
				{
					tDT.TransportIdentification.IdOfTheMeansOfTransport = declaration.Vessel.RV_Code.Left(30);
				}
				else if (!declaration.JE_VesselName.IsEmpty)
				{
					tDT.TransportIdentification.IdOfTheMeansOfTransport = declaration.JE_VesselName;
				}
			}
			else
			{
				tDT.ModeOfTransport.ModeOfTransportCoded = "5";
			}
		}

		protected void GenerateGroup5(CUSDECMessage baseSegment)
		{
			SegmentGroup5 group5 = baseSegment.Group5.InstantiateAChildAndAddItToChildrenCollection();
			foreach (PermitCode permit in declaration.PermitCodes)
			{
				if (!permit.ZO_Code.IsEmpty)
				{
					GenerateSegmentDOC(group5.DOC, permit.ZO_Code, permit.ZO_Data);
				}
			}
		}

		protected void GenerateGroup6(CUSDECMessage baseSegment)
		{
			SegmentGroup6 group6 = baseSegment.Group6.InstantiateAChildAndAddItToChildrenCollection();

			OrgHeader client = declaration.IsExport ? declaration.Supplier : declaration.Importer;

			ZString clientCode = "";
			if (client != null)
			{
				ZString clientName = "";
				if (client.IsMiscellaneous)
				{
					clientName = declaration.IsExport ? declaration.MiscSupplierName : declaration.MiscImporterName;
				}
				else
				{
					clientCode = client.LocalCustomsClientCode;
					clientName = client.OH_FullNameTruncated;
				}
				GenerateSegmentNAD(group6.NAD, PartyQualifierList.Principal, clientCode, clientName);
			}

			ZString brokerCode = NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant();
			if (brokerCode != clientCode)
			{
				GenerateSegmentNAD(group6.NAD, PartyQualifierList.CustomsBroker, brokerCode, null);
			}

			var notifyParty = declaration.NotifyParty;
			if (notifyParty != null)
			{
				ZString deliveryAuthorityCode = notifyParty.LocalCustomsClientCode;
				if (!deliveryAuthorityCode.IsEmpty)
				{
					GenerateSegmentNAD(group6.NAD, PartyQualifierList.DeliveryParty, deliveryAuthorityCode, null);
				}
			}
		}

		protected void GenerateGroup10(CUSDECMessage baseSegment)
		{
			foreach (JobComInvoiceHeader invoiceHeader in declaration.JobComInvoiceGroupHeaders[0].AllJobComInvoiceHeaders)
			{
				SegmentGroup10 group10 = baseSegment.Group10.InstantiateAChildAndAddItToChildrenCollection();
				DMSSegment dMS = group10.DMS.InstantiateAChildAndAddItToChildrenCollection();
				dMS.DocumentMessageNumber = invoiceHeader.JZ_InvoiceNumber.TrimStart().Left(17);
				dMS.DocumentMessageNameCoded = DocumentMessageNameCodedList.CustomsInvoice;
				GenerateGroup13(group10, invoiceHeader);
			}
		}

		protected void GenerateGroup13(SegmentGroup10 baseSegment, JobComInvoiceHeader invoiceHeader)
		{
			SegmentGroup13 group13 = baseSegment.Group13.InstantiateAChildAndAddItToChildrenCollection();
			TODSegment tOD = group13.TOD.InstantiateAChildAndAddItToChildrenCollection();
			tOD.TermsOfDeliveryOrTransport.TermsOfDeliveryOrTransportCoded = invoiceHeader.JZ_IncoTerm.Left(3);
			tOD.TermsOfDeliveryOrTransport.CodeListQualifier = CodeListQualifierList.Incoterms1980;
			tOD.TermsOfDeliveryOrTransport.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;
		}

		protected int lastNumberOfLinesSentToCustoms;
		protected void GenerateGroup30(CUSDECMessage baseSegment)
		{
			if (messageType == MessageTypes.CancelLine)
			{
				for (int counter = entryHeader.MergedLines.Count + 1; counter <= lastNumberOfLinesSentToCustoms; counter++)
				{
					SegmentGroup30 group30 = baseSegment.Group30.InstantiateAChildAndAddItToChildrenCollection();
					GenerateSegmentCST(group30.CST, Convert.ToInt16(counter), "", "");
				}
			}
			else
			{
				if (lastNumberOfLinesSentToCustoms == 0)
				{
					lastNumberOfLinesSentToCustoms = entryHeader.MergedLines.Count;
				}

				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					if (ShouldIncludeThisLine(entryLine.CL_LineNumber))
					{
						SegmentGroup30 group30 = baseSegment.Group30.InstantiateAChildAndAddItToChildrenCollection();
						GenerateSegmentCST(group30.CST, entryLine.CL_LineNumber, entryLine.CL_AdValoremTariff.Replace(".", ""), entryLine.ConcessionCode);

						GenerateSegmentFTX(group30.FTX, TextSubjectQualifierList.GoodsDescription, entryLine.Description);

						GenerateSegmentLOC(group30.LOC, PlaceLocationQualifierList.CountryOfOrigin, entryLine.CountryOfOrigin);
						if (declaration.IsImport)
						{
							GenerateSegmentLOC(group30.LOC, PlaceLocationQualifierList.CountryOfExportationDespatch, entryLine.CountryOfExport);
						}

						if (!entryLine.StatisticalUnit.IsEmpty)
						{
							GenerateSegmentMEA(group30.MEA, MeasurementApplicationQualifierList._1stSpecifiedTariffQuantity, null, entryLine.StatisticalUnit, entryLine.StatisticalQty.ToString(JobComInvoiceLine.CustomsQtyMaxDecimalPlaces));
						}
						if (!entryLine.SupplementaryUQ.IsEmpty)
						{
							GenerateSegmentMEA(group30.MEA, MeasurementApplicationQualifierList._2ndSpecifiedTariffQuantity, null, entryLine.SupplementaryUQ, entryLine.SupplementaryQty.ToString(JobComInvoiceLine.CustomsQtyMaxDecimalPlaces));
						}

						if (declaration.IsImport)
						{
							OrgHeader supplier = entryLine.Supplier;
							ZString supplierCode = supplier != null && !supplier.IsMiscellaneous ? supplier.LocalCustomsSupplierCode : ZString.Empty;
							GenerateSegmentNAD(group30.NAD, PartyQualifierList.Supplier, supplierCode, entryLine.SupplierName);
						}

						GenerateGroup33(group30, MonetaryAmountTypeQualifierList.AmountTargetCurrency, entryLine.OSCustomsValue.ToString(2), entryLine.OSCurrencyCode, entryLine.ExchangeRate.ToString(2), GetExchangeRateIndicator(entryLine.ExchangeRateIndicator));
						if (declaration.IsImport)
						{
							GenerateGroup33(group30, MonetaryAmountTypeQualifierList.CustomsValue, entryLine.VFDWholeNZD.ToString(0), null, null, null);
							GenerateGroup33(group30, MonetaryAmountTypeQualifierList.FreightCharge, entryLine.FreightWholeNZD.ToString(0), null, null, null);
							GenerateGroup33(group30, MonetaryAmountTypeQualifierList.InsuranceChargesCustoms, entryLine.InsuranceWholeNZD.ToString(0), null, null, null);
						}

						foreach (PermitCode permit in entryLine.PermitCodes)
						{
							if (!permit.ZO_Code.IsEmpty)
							{
								GenerateGroup37(group30, permit.ZO_Code, permit.ZO_Data);
							}
						}

						if (declaration.IsImport)
						{
							GenerateGroup40(group30, entryLine.RelationshipIndicator, CodeListQualifierList.CustomsIndicator, null);
						}

						foreach (ProhibitedCode prohibitedCode in entryLine.ProhibitedCodes)
						{
							if (!prohibitedCode.ZO_Code.IsEmpty)
							{
								GenerateGroup40(group30, prohibitedCode.ZO_Code, CodeListQualifierList.GovernmentAgencyProcedure, null);
							}
						}

						foreach (OtherInfo otherInfo in entryLine.OtherInfos)
						{
							if (!otherInfo.ZO_Code.IsEmpty)
							{
								GenerateGroup40(group30, otherInfo.ZO_Code, CodeListQualifierList.CustomsSpecialCodes, otherInfo.ZO_Data);
							}
						}

						if (declaration.IsImport)
						{
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.AlacAlcoholLevy, entryLine.ALACLevyAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.HeraSteelLevy, entryLine.HERALevyAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.AccFuelLevy, entryLine.ACCFuelLevyAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.PfmlFuelLevy, entryLine.PFMLFuelLevyAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.SggSyntheticGreenhouseGasesLevy, entryLine.SyntheticGreenhouseGasesLevyAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.AntiDumpingDuty, null, entryLine.AntiDumpingDutyAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CountervailingDuty, null, entryLine.CountervailingDutyAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CustomsDuty, null, entryLine.DutyAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.GoodsAndServicesTax, null, entryLine.GSTAmount, 0m, entryLine.PreferentialDutyIndicator);
						}
						else if (declaration.IsExport && (declaration.IsDrawback || declaration.IsCompletion))
						{
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.AlacAlcoholLevy, entryLine.ALACLevyCreditAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CustomsDuty, null, entryLine.DutyCreditAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.GoodsAndServicesTax, null, entryLine.GSTCreditAmount, 0m, entryLine.PreferentialDutyIndicator);
						}
						else if (declaration.IsExcise)
						{
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CommoditySpecificTax, CustomsLevyTypeList.Codes.AlacAlcoholLevy, entryLine.ALACLevyAmount, 0m, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.CustomsDuty, null, entryLine.DutyAmount, entryLine.ExciseDutyCreditAmount, null);
							GenerateGroup41(group30, DutyTaxFeeTypeCodedList.GoodsAndServicesTax, null, entryLine.GSTAmount, 0m, entryLine.PreferentialDutyIndicator);
						}
					}
				}
			}
			if (messageType != MessageTypes.ReplaceLines)
			{
				lastNumberOfLinesSentToCustoms = entryHeader.MergedLines.Count;
			}
		}

		protected void GenerateGroup33(SegmentGroup30 baseSegment, MonetaryAmountTypeQualifierList monetaryAmountTypeQualifier, string valueInCurrency, string currencyCode, string exchangeRate, string exchangeRateIndicator)
		{
			SegmentGroup33 group33 = baseSegment.Group33.InstantiateAChildAndAddItToChildrenCollection();
			GenerateSegmentMOA(group33.MOA, monetaryAmountTypeQualifier, valueInCurrency, currencyCode);

			if (exchangeRate != null || exchangeRateIndicator != null)
			{
				GenerateGroup34(group33, exchangeRate, exchangeRateIndicator);
			}
		}

		protected void GenerateGroup34(SegmentGroup33 baseSegment, string exchangeRate, string exchangeRateIndicator)
		{
			SegmentGroup34 group34 = baseSegment.Group34.InstantiateAChildAndAddItToChildrenCollection();

			CUXSegment cUX = group34.CUX.InstantiateAChildAndAddItToChildrenCollection();
			cUX.CurrencyDetails1.CurrencyDetailsQualifier = CurrencyDetailsQualifierList.ReferenceCurrency;
			cUX.RateOfExchange = exchangeRate;
			if (declaration.IsExport)
			{
				cUX.CurrencyMarketExchangeCoded = CurrencyMarketExchangeCodedList.GetFromString(exchangeRateIndicator);
			}
		}

		protected void GenerateGroup37(SegmentGroup30 baseSegment, string permitCode, string permitNumber)
		{
			SegmentGroup37 group37 = baseSegment.Group37.InstantiateAChildAndAddItToChildrenCollection();
			GenerateSegmentDOC(group37.DOC, permitCode, permitNumber);
		}

		protected void GenerateGroup40(SegmentGroup30 baseSegment, string code, CodeListQualifierList codeListQualifier, string data)
		{
			SegmentGroup40 group40 = baseSegment.Group40.InstantiateAChildAndAddItToChildrenCollection();
			GenerateSegmentGIS(group40.GIS, code, codeListQualifier, data);
		}

		protected void GenerateGroup41(SegmentGroup30 baseSegment, DutyTaxFeeTypeCodedList dutyTaxFeeTypeCoded, string commoditySpecificTaxType, ZDecimal amount, ZDecimal exciseCreditAmount, ZString preferentialDutyIndicator)
		{
			if (!amount.IsEmpty || preferentialDutyIndicator == "Q")
			{
				SegmentGroup41 group41 = baseSegment.Group41.InstantiateAChildAndAddItToChildrenCollection();
				GenerateSegmentTAX(group41.TAX, DutyTaxFeeFunctionQualifierList.IndividualDutyTaxOrFeeCustomsItem, dutyTaxFeeTypeCoded, commoditySpecificTaxType, null);
				GenerateSegmentMOA(group41.MOA, MonetaryAmountTypeQualifierList.DutyTaxOrFeeAmount, amount.ToString(2), null);
				if (!exciseCreditAmount.IsEmpty)
				{
					GenerateSegmentMOA(group41.MOA, MonetaryAmountTypeQualifierList.NonTaxableAmount, exciseCreditAmount.ToString(2), null);
				}
				if (preferentialDutyIndicator == "Q")
				{
					GenerateSegmentGIS(group41.GIS, preferentialDutyIndicator, CodeListQualifierList.CustomsPreference, null);
				}
			}
		}

		protected void GenerateGroup49(CUSDECMessage baseSegment, DutyTaxFeeFunctionQualifierList dutyTaxFeeFunctionQualifier, DutyTaxFeeTypeCodedList dutyTaxFeeTypeCoded, string commoditySpecificTaxType, ZDecimal amount)
		{
			GenerateGroup49(baseSegment, dutyTaxFeeFunctionQualifier, dutyTaxFeeTypeCoded, commoditySpecificTaxType, amount, 0m, 0m, 0m, ZString.Empty);
		}

		protected void GenerateGroup49(CUSDECMessage baseSegment, DutyTaxFeeFunctionQualifierList dutyTaxFeeFunctionQualifier, DutyTaxFeeTypeCodedList dutyTaxFeeTypeCoded, string commoditySpecificTaxType, ZDecimal amount, ZDecimal totalGoodsValue, ZDecimal depositRefundAmount, ZDecimal exciseDutyCreditAmount, ZString methodOfPayment)
		{
			if (!amount.IsEmpty || !totalGoodsValue.IsEmpty || !depositRefundAmount.IsEmpty || !exciseDutyCreditAmount.IsEmpty || !methodOfPayment.IsEmpty)
			{
				SegmentGroup49 group49 = baseSegment.Group49.InstantiateAChildAndAddItToChildrenCollection();
				GenerateSegmentTAX(group49.TAX, dutyTaxFeeFunctionQualifier, dutyTaxFeeTypeCoded, commoditySpecificTaxType, totalGoodsValue.IsEmpty ? "" : totalGoodsValue.ToString(0));
				GenerateSegmentMOA(group49.MOA, MonetaryAmountTypeQualifierList.DutyTaxOrFeeAmount, amount.ToString(2), null);
				if (!depositRefundAmount.IsEmpty)
				{
					GenerateSegmentMOA(group49.MOA, MonetaryAmountTypeQualifierList.DepositRefund, depositRefundAmount.ToString(2), null);
				}
				if (!exciseDutyCreditAmount.IsEmpty)
				{
					GenerateSegmentMOA(group49.MOA, MonetaryAmountTypeQualifierList.NonTaxableAmount, exciseDutyCreditAmount.ToString(2), null);
				}
				if (!methodOfPayment.IsEmpty)
				{
					GenerateSegmentGIS(group49.GIS, PaymentMethodList.GetCustomsCode(methodOfPayment), CodeListQualifierList.DutyTaxOrFeePaymentMethod, null);
				}
			}
		}

		protected void GenerateGroup50(CUSDECMessage baseSegment)
		{
			SegmentGroup50 group50 = baseSegment.Group50.InstantiateAChildAndAddItToChildrenCollection();
			AUTSegment aUT = group50.AUT.InstantiateAChildAndAddItToChildrenCollection(); // Authentication (PIN).

			aUT.ValidationKeyIdentification = UsersPin.BrokerID;

			aUT.ValidationResult = NZCMessage.ContainedChecksumPlaceHolder;
		}

		#endregion

		#region Group Specific Segment Generators

		protected void GenerateGroup0UNH(CUSDECMessage baseSegment)
		{
			UNHSegment uNH = baseSegment.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNH.MessageReferenceNumber = NZCMessage.MessageNumberPlaceHolder;
			uNH.MessageIdentifier.MessageType = Edifact.D96BNZ.Elements.MessageTypeList.CustomsDeclarationMessage;
			uNH.MessageIdentifier.MessageVersionNumber = "D";
			uNH.MessageIdentifier.MessageReleaseNumber = "96B";
			uNH.MessageIdentifier.ControllingAgency = ControllingAgencyList.UnEceTradeWp4;
			uNH.CommonAccessReference = messageType == MessageTypes.CompletionEntry ? declaration.JE_OriginalEntryNumber : declaration.DeclarationNumber;
		}

		protected void GenerateGroup0BGM(CUSDECMessage baseSegment)
		{
			BGMSegment bGM = baseSegment.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageName.DocumentMessageNameCoded = GetDocumentMessageName();
			bGM.DocumentMessageIdentification.DocumentMessageNumber = NZCMessage.SendersReferencePlaceHolder;
			bGM.MessageFunctionCoded = GetMessageFunction();
		}

		protected void GenerateGroup0DTM(DateTimePeriodQualifierList dateTimePeriodQualifier, string dateTime, DateTimePeriodFormatQualifierList dateTimePeriodFormatQualifier)
		{
			DTMSegment dTM = EDIFACTMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodQualifier = dateTimePeriodQualifier;
			dTM.DateTimePeriod.DateTimePeriod = dateTime;
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = dateTimePeriodFormatQualifier;
		}

		protected void GenerateGroup0CNT(ControlQualifierList controlQualifier, string controlValue)
		{
			CNTSegment cNT = EDIFACTMessage.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlQualifier = controlQualifier;
			cNT.Control.ControlValue = controlValue;
		}

		protected void GenerateGroup0FTX(CUSDECMessage baseSegment)
		{
			GenerateSegmentFTX(baseSegment.FTX, TextSubjectQualifierList.GeneralInformation, declaration.CustomsMessageRemarks);
		}

		protected void GenerateGroup1RFF(SegmentGroup1 group1, ReferenceQualifierList referenceQualifier, string referenceNumber)
		{
			RFFSegment rFFBillNumber = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFFBillNumber.Reference.ReferenceQualifier = referenceQualifier;
			rFFBillNumber.Reference.ReferenceNumber = referenceNumber;
		}

		protected void GenerateGroup2PAC(SegmentGroup2 group2, ZInt packQty, ZString packType)
		{
			PACSegment pAC = group2.PAC.InstantiateAChildAndAddItToChildrenCollection();
			pAC.NumberOfPackages = packQty.ToString();
			pAC.PackageType.TypeOfPackagesIdentification = packType;

			totalPackages += packQty;
		}

		#endregion

		#region Segment Generators

		protected void GenerateSegmentCST(CSTSegmentMessageSection cSTSection, ZShort lineNumber, ZString tariffCode, ZString concessionCode)
		{
			CSTSegment cST = cSTSection.InstantiateAChildAndAddItToChildrenCollection(); // Invoice Line Header
			cST.GoodsItemNumber = lineNumber.ToString();

			cST.CustomsIdentityCodes1.CustomsCodeIdentification = tariffCode.ExcludeChars(".").Left(11);
			cST.CustomsIdentityCodes1.CodeListQualifier = CodeListQualifierList.HarmonizedSystem;
			cST.CustomsIdentityCodes1.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;

			if (!concessionCode.IsEmpty)
			{
				cST.CustomsIdentityCodes2.CustomsCodeIdentification = concessionCode.Left(7);
				cST.CustomsIdentityCodes2.CodeListQualifier = CodeListQualifierList.TaxAssessmentMethod;
				cST.CustomsIdentityCodes2.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;
			}
		}

		protected void GenerateSegmentFTX(FTXSegmentMessageSection fTXSection, TextSubjectQualifierList textSubjectQualifier, ZString notes)
		{
			StringLineBreaker customsMessageRemarks = new StringLineBreaker(notes.ToUpper(), false);

			if (!customsMessageRemarks.IsEmpty())
			{
				FTXSegment fTX = fTXSection.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectQualifier = textSubjectQualifier;
				fTX.TextLiteral.FreeText1 = customsMessageRemarks.GetNextLine(70);
				fTX.TextLiteral.FreeText2 = customsMessageRemarks.GetNextLine(70);
				fTX.TextLiteral.FreeText3 = customsMessageRemarks.GetNextLine(70);
				fTX.TextLiteral.FreeText4 = customsMessageRemarks.GetNextLine(40);
			}
		}

		protected void GenerateSegmentMEA(MEASegmentMessageSection mEASection, MeasurementApplicationQualifierList measurementApplicationQualifier, MeasurementDimensionCodedList measurementDimensionCoded, string unit, string value)
		{
			MEASegment mEA = mEASection.InstantiateAChildAndAddItToChildrenCollection();
			mEA.MeasurementApplicationQualifier = measurementApplicationQualifier;
			mEA.MeasurementDetails.MeasurementDimensionCoded = measurementDimensionCoded;
			mEA.ValueRange.MeasureUnitQualifier = unit;
			mEA.ValueRange.MeasurementValue = value;
		}

		protected void GenerateSegmentLOC(LOCSegmentMessageSection lOCSection, PlaceLocationQualifierList placeLocationQualifier, string placeLocationIdentification)
		{
			if (placeLocationIdentification != null && !string.IsNullOrEmpty(placeLocationIdentification))
			{
				LOCSegment lOC = lOCSection.InstantiateAChildAndAddItToChildrenCollection();
				lOC.PlaceLocationQualifier = placeLocationQualifier;
				lOC.LocationIdentification.PlaceLocationIdentification = placeLocationIdentification;
				// BG - Should not be put back in as was never sent in ediBroker and no problems resulted.
				//				if (PlaceLocationQualifier == PlaceLocationQualifierList.Warehouse)
				//				{
				//					LOC.LocationIdentification.CodeListQualifier = CodeListQualifierList.LocationOfGoods;
				//					LOC.LocationIdentification.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;
				//				}
				// BG - Should not be put back in as was never sent in ediBroker and no problems resulted.
			}
		}

		protected void GenerateSegmentNAD(NADSegmentMessageSection nADSection, PartyQualifierList partyQualifier, ZString customsCode, ZString companyName)
		{
			NADSegment nAD = nADSection.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyQualifier = partyQualifier;

			if (customsCode.IsEmpty && declaration.IsSimplified && !companyName.IsEmpty)
			{
				nAD.NameAndAddress.NameAndAddressLine1 = companyName.Left(35);
				if (companyName.Length > 35)
				{
					nAD.NameAndAddress.NameAndAddressLine2 = companyName.Substring(35).Left(35);
				}
			}
			else
			{
				nAD.PartyIdentificationDetails.CodeListQualifier = CodeListQualifierList.MutuallyDefined;
				nAD.PartyIdentificationDetails.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;
				nAD.PartyIdentificationDetails.PartyIdIdentification = customsCode.Trim().Left(9).PadLeft(9, '0');
			}
		}

		protected void GenerateSegmentDOC(DOCSegmentMessageSection dOCSection, string permitCode, string permitNumber)
		{
			DOCSegment dOC = dOCSection.InstantiateAChildAndAddItToChildrenCollection();
			dOC.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.GetFromString(permitCode);
			dOC.DocumentMessageName.CodeListQualifier = CodeListQualifierList.DocumentRequestedByCustoms;
			dOC.DocumentMessageName.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;
			dOC.DocumentMessageDetails.DocumentMessageNumber = permitNumber;
		}

		protected void GenerateSegmentGIS(GISSegmentMessageSection gISSection, string processingIndicator, CodeListQualifierList codeListQualifier, ZString additionalInfo)
		{
			GISSegment gIS = gISSection.InstantiateAChildAndAddItToChildrenCollection();
			gIS.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.GetFromString(processingIndicator);
			gIS.ProcessingIndicator.CodeListQualifier = codeListQualifier;
			gIS.ProcessingIndicator.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;
			if (!additionalInfo.IsEmpty)
			{
				gIS.ProcessingIndicator.ProcessTypeIdentification = ProcessTypeIdentificationList.GetFromString(additionalInfo);
			}
		}

		protected void GenerateSegmentTAX(TAXSegmentMessageSection tAXSection, DutyTaxFeeFunctionQualifierList dutyTaxFeeFunctionQualifier, DutyTaxFeeTypeCodedList dutyTaxFeeTypeCoded, ZString commoditySpecificTaxType, ZString optionalValue)
		{
			TAXSegment tAX = tAXSection.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyTaxFeeFunctionQualifier = dutyTaxFeeFunctionQualifier;
			tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded = dutyTaxFeeTypeCoded;
			tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification = commoditySpecificTaxType;
			if (!commoditySpecificTaxType.IsEmpty)
			{
				tAX.DutyTaxFeeAccountDetail.CodeListQualifier = CodeListQualifierList.TaxPartyIdentification;
				tAX.DutyTaxFeeAccountDetail.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.NzNewZealandCustoms;
			}
			tAX.DutyTaxFeeAssessmentBasis = optionalValue;
		}

		protected void GenerateSegmentMOA(MOASegmentMessageSection mOASection, MonetaryAmountTypeQualifierList monetaryAmountTypeQualifier, string amount, string currency)
		{
			MOASegment mOA = mOASection.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeQualifier = monetaryAmountTypeQualifier;
			mOA.MonetaryAmount.MonetaryAmount = amount;
			mOA.MonetaryAmount.CurrencyCoded = currency;
		}

		#endregion

		protected DocumentMessageNameCodedList GetDocumentMessageName()
		{
			DocumentMessageNameCodedList result = null;
			switch (declaration.JE_MessageType)
			{
				case JobMessageTypeList.Codes.Export:
					result = DocumentMessageNameCodedList.GoodsDeclarationForExportation;
					break;
				case JobMessageTypeList.Codes.Excise:
					result = DocumentMessageNameCodedList.CustomsDeclarationWithCommercialAndItemDetail;
					break;
				case JobMessageTypeList.Codes.Import:
					result = DocumentMessageNameCodedList.GoodsDeclarationForImportation;
					break;
			}
			return result;
		}

		protected string GetExchangeRateIndicator(ZString exchangeRateIndicator)
		{
			string result = "";
			switch (exchangeRateIndicator)
			{
				case ExchangeRateIndicatorList.Codes.Floating:
					result = "F";
					break;
				case ExchangeRateIndicatorList.Codes.ForwardCover:
					result = "C";
					break;
				case ExchangeRateIndicatorList.Codes.NZD:
					result = "N";
					break;
			}
			return result;
		}

		protected string GetCustomsCodeIdentification()
		{
			string result = "";
			switch (declaration.JE_MessageSubType)
			{
				case JobMessageSubTypeList.Codes.Normal:
				case JobMessageSubTypeList.Codes.Completion:
					switch (declaration.JE_MessageType)
					{
						case JobMessageTypeList.Codes.Export:
							result = "40";
							break;
						case JobMessageTypeList.Codes.Excise:
							result = "30";
							break;
						case JobMessageTypeList.Codes.Import:
							result = "10";
							break;
					}
					break;
				case JobMessageSubTypeList.Codes.Simplified:
					result = "11";
					break;
				case JobMessageSubTypeList.Codes.Drawback:
					result = "41";
					break;
				case JobMessageSubTypeList.Codes.Temporary:
					result = "51";
					break;
				case JobMessageSubTypeList.Codes.Sight:
					result = "52";
					break;
				case JobMessageSubTypeList.Codes.Periodic:
					result = "53";
					break;
			}
			return result;
		}

		protected MessageFunctionCodedList GetMessageFunction()
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
				case MessageTypes.ReplaceLines:
					result = MessageFunctionCodedList.ReplaceItemDetailAndSummaryOnly;
					break;
				case MessageTypes.AddLine:
					result = MessageFunctionCodedList.Addition;
					break;
				case MessageTypes.CancelLine:
					result = MessageFunctionCodedList.Deletion;
					break;
				case MessageTypes.CancelEntry:
					result = MessageFunctionCodedList.Cancellation;
					break;
				case MessageTypes.CompletionEntry:
					result = MessageFunctionCodedList.FinalTransmission;
					break;
				case MessageTypes.Replacement:
					result = MessageFunctionCodedList.Replace;
					break;
			}
			return result;
		}

		protected ReferenceQualifierList GetRFFTypeFromDeclarationMode()
		{
			ReferenceQualifierList result;
			switch (declaration.JE_TransportMode)
			{
				case JobTransportModeList.Codes.Post:
					result = ReferenceQualifierList.ArticleNumber;
					break;
				case JobTransportModeList.Codes.Sea:
					result = ReferenceQualifierList.BillOfLadingNumber;
					break;
				default: // case JobTransportModeList.Codes.Air:
					result = ReferenceQualifierList.HouseWaybillNumber;
					break;
			}
			return result;
		}

		protected bool MessageTypeIncludesHeaders
		{
			get
			{
				return messageType == MessageTypes.Replacement ||
					messageType == MessageTypes.Original ||
					messageType == MessageTypes.ReplaceHeader ||
					messageType == MessageTypes.CompletionEntry;
			}
		}

		protected bool MessageTypeIncludesLines
		{
			get
			{
				return messageType == MessageTypes.Replacement ||
					messageType == MessageTypes.Original ||
					messageType == MessageTypes.ReplaceLines ||
					messageType == MessageTypes.AddLine ||
					messageType == MessageTypes.CancelLine ||
					messageType == MessageTypes.CompletionEntry;
			}
		}

		protected bool ShouldIncludeThisLine(int lineNumber)
		{
			bool result = false;
			switch (messageType)
			{
				case MessageTypes.Original:
				case MessageTypes.Replacement:
				case MessageTypes.CompletionEntry:
					result = true;
					break;
				case MessageTypes.ReplaceLines:
					result = (lineNumber <= lastNumberOfLinesSentToCustoms);
					break;
				case MessageTypes.AddLine:
					result = (lineNumber > lastNumberOfLinesSentToCustoms);
					break;
			}
			return result;
		}

		CurrentUsersPin UsersPin
		{
			get
			{
				if (fUsersPin == null)
				{
					fUsersPin = new CurrentUsersPin(entryHeader.Factory, declaration.IsInTestMode);
				}
				return fUsersPin;
			}
		}
		CurrentUsersPin fUsersPin;

		public bool IsPasswordOk(ZString password)
		{
			return (password == UsersPin.DecryptedPinCode);
		}

		protected ZInt totalPackages;
	}
}
