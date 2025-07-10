using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class ACEEntryMessage7501Print : EntrySummaryPrintingFromMessage
	{
		public ACEEntryMessage7501Print(CusEntryHeader entryHeader, MQEDIMessage outgoingMessage, MQEDIMessage incomingMessage, EDIMessage bluMessage)
			: base(entryHeader, outgoingMessage, incomingMessage.HasCensusWarnings, bluMessage)
		{
			var aens43RulingList = new List<AENS43>();
			var aens44DescriptionList = new List<AENS44>();
			var aens47PartyLines = new List<AENS47>();
			var aens62ChargesList = new List<IChargeBlock>();

			#region Populate Data from Message Blocks

			foreach (var block in outgoingMessage.MessageBlock.MessageBlocks)
			{
				switch (block.MandatoryCharacters)
				{
					case "10":
						aens10 = (AENS10)block;
						break;
					case "11":
						aens11 = (AENS11)block;
						break;
					case "20":
						aens20 = (AENS20)block;
						blocksForBillPrinting.Add(block);
						break;
					case "21":
						aens21 = (AENS21)block;
						break;
					case "22":
						blocksForBillPrinting.Add(block);
						break;
					case "23":
						blocksForBillPrinting.Add(block);
						break;
					case "30":
						aens30 = (AENS30)block;
						break;
					case "31":
						aens31List.Add((AENS31)block);
						break;
					case "33":
						aens33 = (AENS33)block;
						break;
					case "40":
						aens43RulingList = new List<AENS43>();
						aens44DescriptionList = new List<AENS44>();
						aens47PartyLines = new List<AENS47>();
						aens52Lines = new List<AENS52>();
						aens53_ADDCVDBlocks = new List<AENS53>();
						aens62ChargesList = new List<IChargeBlock>();
						var aens40 = (AENS40)block;
						aens40Lines.Add(aens40);
						aens50Lines = new List<AENS50>();
						aens54Lines = new List<AENS54>();
						printLineObject = new EntryMessageLine(Factory);
						printLineObject.aens40 = aens40;
						LineObjectCollection.Add(printLineObject);
						break;
					case "43":
						aens43RulingList.Add((AENS43)block);
						printLineObject.aens43 = aens43RulingList;
						break;
					case "44":
						aens44DescriptionList.Add((AENS44)block);
						printLineObject.aens44 = aens44DescriptionList;
						break;
					case "47":
						var aens47 = (AENS47)block;
						aens47PartyLines.Add(aens47);
						allAENS47Lines.Add(aens47);
						printLineObject.aens47 = aens47PartyLines;
						break;
					case "50":
						var aens50 = (AENS50)block;
						if (aens50.HTSNumber != TariffViewAsCodeDescription.NotApplicableCode)
						{
							aens50Lines.Add(aens50);
							printLineObject.aens50 = aens50Lines;
							entryHasADDCVDLines |= !aens50.Quantity2.IsEmpty;
							if (aens50Lines.Count > 1)
							{
								entryHasSecondaryTariffLines = true;
								entryHasAdditionalTariffLines = true;
							}
						}
						break;
					case "51":
						printLineObject.aens51 = (AENS51)block;
						break;
					case "52":
						aens52Lines.Add((AENS52)block);
						printLineObject.aens52 = aens52Lines;
						break;
					case "53":
						var aens53 = (AENS53)block;
						aens53_ADDCVDBlocks.Add(aens53);
						printLineObject.aens53 = aens53_ADDCVDBlocks;
						entryHasADDCVDLines |= !aens53.CaseNumber.IsEmpty;
						break;
					case "54":
						var aens54 = (AENS54)block;
						aens54Lines.Add(aens54);
						printLineObject.aens54 = aens54Lines;
						break;
					case "60":
						printLineObject.aens60 = (AENS60)block;
						break;
					case "61":
					case "62":
						aens62ChargesList.Add((IChargeBlock)block);
						printLineObject.aens62 = aens62ChargesList;
						break;
					case "88":
						aens88 = (AENS88)block;
						break;
					case "89":
						aens89Lines.Add((AENS89)block);
						break;
					case "90":
						aens90 = (AENS90)block;
						break;
				}
			}

			#endregion

			DetermineWatchSectionPrintingFlags();
			aENSE0StatusNotificationBlock = incomingMessage.MessageBlock.MessageBlocks.OfType<AENSE0>().FirstOrDefault(x => x.ReferenceDataTypeCode == EntrySummaryReferenceDataList.Codes.SUMMRY);
			FindE1BlockFromStatusNotificationMessage();
			billPrintManager.PopulateElements(blocksForBillPrinting);
		}
		readonly EntryMessageLine printLineObject;

		readonly AENS10 aens10 = new AENS10();
		readonly AENS11 aens11 = new AENS11();
		readonly AENS20 aens20 = new AENS20();
		readonly AENS21 aens21 = new AENS21();
		readonly AENS30 aens30 = new AENS30();
		readonly List<AENS31> aens31List = new List<AENS31>();
		readonly AENS33 aens33 = new AENS33();
		readonly List<AENS40> aens40Lines = new List<AENS40>();
		readonly List<AENS47> allAENS47Lines = new List<AENS47>();
		readonly List<AENS50> aens50Lines = new List<AENS50>();
		readonly List<AENS52> aens52Lines = new List<AENS52>();
		readonly List<AENS53> aens53_ADDCVDBlocks = new List<AENS53>();
		readonly List<AENS54> aens54Lines = new List<AENS54>();
		readonly AENS88 aens88 = new AENS88();
		readonly List<AENS89> aens89Lines = new List<AENS89>();
		readonly AENS90 aens90 = new AENS90();

		readonly AENSE0 aENSE0StatusNotificationBlock;
		AESSE1 aESSE1StatusNotificationBlock;

		void FindE1BlockFromStatusNotificationMessage()
		{
			if (aESSE1StatusNotificationBlock == null)
			{
				var message = (MQEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification, EDIMessage.Direction.Receive);
				if (message != null)
				{
					aESSE1StatusNotificationBlock = message.MessageBlock.MessageBlocks.OfType<AESSE1>().FirstOrDefault();
				}
			}
		}

		#region Overriden Document Properties

		protected override bool IsACE
		{
			get { return true; }
		}

		public override ZString USTeamNo
		{
			get
			{
				return aESSE1StatusNotificationBlock != null ?
					aESSE1StatusNotificationBlock.ImportSpecialistTeam :
						aENSE0StatusNotificationBlock != null ?
						aENSE0StatusNotificationBlock.GetTeamNumber() :
						ZString.Empty;
			}
		}

		protected override ZString PaymentTypeFromEntrySummary
		{
			get { return aens10.PaymentTypeCode; }
		}

		#region Block 31 - Bond

		AENS31 BondBlockDetails
		{
			get { return bondBlockDetails ?? (bondBlockDetails = FindBondByDesignationTypeCode("B")); }
		}
		AENS31 bondBlockDetails;

		AENS31 ADDCVDBondBlockDetails
		{
			get { return addcvdBondBlockDetails ?? (addcvdBondBlockDetails = FindBondByDesignationTypeCode("A")); }
		}
		AENS31 addcvdBondBlockDetails;

		AENS31 FindBondByDesignationTypeCode(ZString code)
		{
			return aens31List.Find(x => x.BondDesignationTypeCode == code);
		}

		public override ZString SuretyCode
		{
			get { return BondBlockDetails != null ? BondBlockDetails.SuretyCompanyCode : ZString.Empty; }
		}

		public override ZString BondType
		{
			get { return BondBlockDetails != null ? BondBlockDetails.BondTypeCode : ZString.Empty; }
		}

		#endregion

		public override ZDateTime DateOfFirstArrival
		{
			get { return aens11.DateOfImportation; }
		}

		public override ZBool DecFinalWithdrawal
		{
			get { return aens30.FinalWarehouseWithdrawalIndicator == "Y"; }
		}

		public override ZBool DeferredTaxToBePaidByEFT
		{
			get { return aens10.DeferredTaxPaymentCode == TaxDeferIndicatorList.Codes.DeferredTaxWithEFT; }
		}

		protected override ZString EffectiveUltimateConsigneeCustomsRegNoCore
		{
			get
			{
				return IsConsumptionFTZ ?
						  aens11.ConsigneeNumber :
						  aens11.ConsigneeNumber != aens11.ImporterOfRecordNumber ?
							  aens11.ConsigneeNumber :
							  new ZString(USConstants.Same);
			}
		}

		public override ZString EntryFilerCode
		{
			get { return aens10.EntryFilerCode; }
		}

		public override ZString EntryType
		{
			get { return aens10.EntryTypeCode; }
		}

		public override ZString EntryTypeCode
		{
			get { return "ABI/" + GetABIStatusIndicator(PaymentType) + (aens10.LiveEntryIndicator == "Y" ? "/L" : ""); }
		}

		public override ZString EntryNo
		{
			get { return aens10.EntryNumber; }
		}

		public override ZString FormattedEntryNumber
		{
			get { return aens10.EntryFilerCode + "-" + aens10.EntryNumber.SubstringSafe(0, 7) + "-" + aens10.EntryNumber.SubstringSafe(7, 1); }
		}

		protected override ZString ImporterOfRecordCustomsRegNoCore
		{
			get { return aens11.ImporterOfRecordNumber; }
		}

		protected override ZDate ExportDateFromFirstLine
		{
			get
			{
				var firstElement = aens40Lines.FirstOrDefault();
				return firstElement != null ? firstElement.DateOfExportation : ZDate.Empty;
			}
		}

		protected override bool HasMultiExportDates
		{
			get
			{
				if (!hasMultiExportDates.HasValue)
				{
					hasMultiExportDates = UniqueValueCalculator.HasMultiValues(aens40Lines, x => x.DateOfExportation);
				}
				return hasMultiExportDates.Value;
			}
		}
		bool? hasMultiExportDates;

		public override ZString UniqueCountryOfOrigin
		{
			get
			{
				if (!uniqueCountryOfOriginCached.HasValue)
				{
					uniqueCountryOfOriginCached = UniqueValueCalculator.GetUniqueValue(aens40Lines, x => x.CountryOfOriginCode, (ZString)USConstants.MultipleValueIndicator);
				}

				return uniqueCountryOfOriginCached.Value;
			}
		}
		ZString? uniqueCountryOfOriginCached;

		public override ZString SchDArrival
		{
			get { return aens20.DistrictPortOfUnlading; }
		}

		public override ZString SchDEntry
		{
			get { return aens10.DistrictPortOfEntry; }
		}

		protected override ZString USTransportModeCore
		{
			get { return aens10.ModeOfTransportationMOTCode; }
		}

		protected override ZString VesselName
		{
			get { return aens20.ConveyanceName; }
		}

		protected override ZString PipelineName
		{
			get { return aens20.ConveyanceName; }
		}

		protected override ZString CarrierCode
		{
			get { return aens20.CarrierCode; }
		}

		protected override ZString FTZNumber
		{
			get { return aens11.ForeignTradeZoneIdentifier.IsEmpty ? aens11.NewForeignTradeZoneIdentifier : aens11.ForeignTradeZoneIdentifier; }
		}

		public override ZString ManufacturerID
		{
			get
			{
				if (!manufacturerIDCached.HasValue)
				{
					manufacturerIDCached = UniqueValueCalculator.GetUniqueValue(allAENS47Lines, x => x.ArticlePartyTypeCode == "M" ? x.ArticlePartyIdentifier : ZString.Empty, (ZString)USConstants.MultipleValueIndicator);
				}

				return manufacturerIDCached.Value;
			}
		}
		ZString? manufacturerIDCached;

		protected override ZString MissingDoc1Core
		{
			get { return aens33 != null ? aens33.MissingDocumentCode1 : ZString.Empty; }
		}

		protected override ZString MissingDoc2Core
		{
			get { return aens33 != null ? aens33.MissingDocumentCode2 : ZString.Empty; }
		}

		protected override ZString UltimateStateCore
		{
			get { return aens11.USStateOfDestinationCode; }
		}

		public override ZString UniqueCountryOfExport
		{
			get
			{
				if (!uniqueCountryOfExportCached.HasValue)
				{
					uniqueCountryOfExportCached = ZString.Empty;

					if (!IsConsumptionFTZ)
					{
						uniqueCountryOfExportCached = UniqueValueCalculator.GetUniqueValue(aens40Lines, x => x.CountryOfExportCode, (ZString)USConstants.MultipleValueIndicator);
					}
					else
					{
						EntryMessageLine lineWithHighestCustomsValue = null;

						foreach (EntryMessageLine line in LineObjectCollection)
						{
							if (lineWithHighestCustomsValue == null || lineWithHighestCustomsValue != line && line.aens50.Sum(x => x.ValueOfGoodsAmount) > lineWithHighestCustomsValue.aens50.Sum(y => y.ValueOfGoodsAmount))
							{
								lineWithHighestCustomsValue = line;
							}
						}
						uniqueCountryOfExportCached = lineWithHighestCustomsValue != null ? lineWithHighestCustomsValue.aens40.CountryOfExportCode : ZString.Empty;
					}
				}

				return uniqueCountryOfExportCached.Value;
			}
		}
		ZString? uniqueCountryOfExportCached;

		protected override ZString GetPortOfLadingFromEntryLines()
		{
			if (!portOfLadingFromEntryLinesCached.HasValue)
			{
				portOfLadingFromEntryLinesCached = UniqueValueCalculator.GetUniqueValue(aens40Lines, x => x.ForeignPortOfLadingCode, (ZString)USConstants.MultipleValueIndicator);
			}
			return portOfLadingFromEntryLinesCached.Value;
		}
		ZString? portOfLadingFromEntryLinesCached;

		protected override ZString LocationOfGoodsAndNameCore
		{
			get
			{
				return CachedValueHelper.GetValue(
					ref cachedLocationOfGoodsAndName,
					() =>
					{
						var result = base.LocationOfGoodsAndNameCore;

						if (result.IsEmpty)
						{
							result = billPrintManager.BLUFirmsCode.IsEmpty ? aens20.LocationOfGoodsCode : billPrintManager.BLUFirmsCode;
							var firms = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, result, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);
							result += firms != null ? ("/" + firms.ZZD_Description) : string.Empty;
						}

						if (result.IsEmpty && Declaration.IsAir)
						{
							result = billPrintManager.BLUVoyageFlight.IsEmpty ? aens21.TripIdentifier : billPrintManager.BLUVoyageFlight;
						}

						return result;
					});
			}
		}

		CachedValue<ZString> cachedLocationOfGoodsAndName;

		protected override ZString Block24ReferenceNumberCore
		{
			get { return aens11.DesignatedNotifyParty4811Number; }
		}

		protected override ZString WarehouseEntryNoForWarehouseEntryWithdrawal
		{
			get
			{
				var result = ZString.Empty;
				if (aens30 != null && !aens30.AssociatedWarehouseEntryNumber.IsEmpty)
				{
					result = aens30.AssociatedWarehouseEntryFilerCode + "-" + aens30.AssociatedWarehouseEntryNumber.SubstringSafe(0, 7) + "-" + aens30.AssociatedWarehouseEntryNumber.SubstringSafe(7, 1);
				}
				return result;
			}
		}

		protected override void AddFeesFrom89Lines()
		{
			foreach (AENS89 feeSummary in aens89Lines)
			{
				if (!feeSummary.AccountingClassCode1.IsEmpty)
				{
					AddFee(feeSummary.AccountingClassCode1, feeSummary.TotalFeeAmount1);
				}
				if (!feeSummary.AccountingClassCode2.IsEmpty)
				{
					AddFee(feeSummary.AccountingClassCode2, feeSummary.TotalFeeAmount2);
				}
				if (!feeSummary.AccountingClassCode3.IsEmpty)
				{
					AddFee(feeSummary.AccountingClassCode3, feeSummary.TotalFeeAmount3);
				}
				if (!feeSummary.AccountingClassCode4.IsEmpty)
				{
					AddFee(feeSummary.AccountingClassCode4, feeSummary.TotalFeeAmount4);
				}
				if (!feeSummary.AccountingClassCode5.IsEmpty)
				{
					AddFee(feeSummary.AccountingClassCode5, feeSummary.TotalFeeAmount5);
				}
			}
		}

		protected override ZInt GetSummaryChargesCountFromEntryLines()
		{
			ZInt result = 0;
			List<ZString> differentChargesList = new List<ZString>();

			foreach (EntryMessageLine lineObject in LineObjectCollection)
			{
				foreach (IChargeBlock chargeDetail in lineObject.aens62)
				{
					string chargeDetailCode = chargeDetail.AccountingClassCode.ToString().PadLeft(3, '0');
					if (!differentChargesList.Contains(chargeDetailCode))
					{
						differentChargesList.Add(chargeDetailCode);
						result++;

						if (result > 4)
						{
							break;
						}
					}
				}
			}
			return result;
		}

		protected override ZString CalculatedPaperlessStatusFromDispositions
		{
			get
			{
				var result = ZString.Empty;
				if (aESSE1StatusNotificationBlock != null)
				{
					result = USConstants.EntrySummaryDisposition.Paperless;

					var notPaperless = aESSE1StatusNotificationBlock.DispositionTypeCode == ENSStatusDispositionCodeList._2 ||
									aESSE1StatusNotificationBlock.DispositionTypeCode == ENSStatusDispositionCodeList._3 ||
									aESSE1StatusNotificationBlock.DispositionTypeCode == ENSStatusDispositionCodeList._7;

					if (notPaperless)
					{
						result = USConstants.EntrySummaryDisposition.DocsRequired;
					}
				}
				return result;
			}
		}

		protected override ZString CalculatedPaperlessStatusFromDeclaration
		{
			get
			{
				var result = ZString.Empty;

				if (entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings ||
					entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings)
				{
					result = USConstants.EntrySummaryDisposition.Paperless;
				}
				return result;
			}
		}

		public override ZBool TaxToBeDeferred
		{
			get { return aens10.DeferredTaxPaymentCode == TaxDeferIndicatorList.Codes.DeferredTax; }
		}

		protected override ZString DeferredTaxIndicator
		{
			get { return aens10.DeferredTaxPaymentCode; }
		}

		// CS00113346 - For Warehouse entry, Duty is not payable on entry. It should be shown on entry lines but not included in the box 37 & 38 totals.
		public override ZDecimal TotalDutyAmt
		{
			get { return EntryTypeList.IsOnlyHMFPayable(EntryType) || EntryTypeList.IsNothingPayable(EntryType) ? ZDecimal.Zero : (ZDecimal)(aens90.GrandTotalDutyAmount + AdditionalTotalPrintingDuty); }
		}

		public override ZDecimal AdditionalTotalPrintingDuty
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (US7501DocPrinting line in DocPrintingData)
				{
					if (line.US_IsAdditionalTotalPrintingDuty)
					{
						result += line.US_DutyAmount;
					}
				}
				return result;
			}
		}

		public override ZDecimal TotalEnteredValue
		{
			get { return entry.TotalEnteredValue; }
		}

		public override ZDecimal TotalEstTax
		{
			get { return aens90.GrandTotalIRTaxAmount; }
		}

		protected override ZDecimal GetTotalLineLevelHMFsFromCharges()
		{
			ZDecimal result = 0;

			foreach (EntryMessageLine lineObject in LineObjectCollection)
			{
				foreach (IChargeBlock chargeDetail in lineObject.aens62)
				{
					if (chargeDetail.AccountingClassCode == Core.Constants.USCustoms.FeeCodes.HMF)
					{
						result += chargeDetail.UserFeeAmount;
					}
				}
			}
			return result;
		}

		protected override ZDecimal EntryHMF
		{
			get { return aens89Lines.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		protected override ZDecimal GetTotalAntidumpingDutyAmountPayableFromCharges()
		{
			return aens88 != null ? aens88.TotalCashDepositADDutyAmount : ZDecimal.Zero;
		}

		protected override ZDecimal GetTotalCountervailingDutyPayableFromCharges()
		{
			return aens88 != null ? aens88.TotalCashDepositCVDutyAmount : ZDecimal.Zero;
		}

		protected override ZDecimal GrandTotalFeeAmount
		{
			get { return aens90.GrandTotalUserFeeAmount + aens90.GrandTotalOtherRevenueAmount; }
		}

		protected override ZDecimal TotalAntidumpingDutyAmount
		{
			get { return aens90.GrandTotalADDutyAmount; }
		}

		protected override ZDecimal TotalCountervailingDutyAmount
		{
			get { return aens90.GrandTotalCVDutyAmount; }
		}

		ZBool MultipleRelationships
		{
			get
			{
				ZBool result = false;
				ZString firstRelationship = aens40Lines.Count > 0 ? aens40Lines[0].RelatedPartyIndicator : ZString.Empty;

				foreach (AENS40 aens40 in aens40Lines)
				{
					if (aens40.RelatedPartyIndicator != firstRelationship)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		protected override EntrySummary7501LineCollection LinesFromMessageAndSnapshot()
		{
			int currentLineNo = 0;
			ZGuid previousInvoicePK = ZGuid.Empty;

			foreach (EntryMessageLine lineBlocks in LineObjectCollection)
			{
				currentLineNo++;

				ZString[] dutyPercentageStrings = GetDutyPercentageStringsForLine(currentLineNo);
				var docData = DocPrintingDataForCurrentLine(currentLineNo);
				US7501DocPrinting[] docDataForChildLines = DocPrintingDataForCurrentChildLines(currentLineNo);

				if (docData != null)
				{
					bool printInvoiceHeading = previousInvoicePK != docData.US_InvoicePK;
					bool printInvoiceDetails = ShouldPrintInvoiceDetails(lineBlocks.aens40, docData.US_InvoiceLinePK);
					lineBlocks.MPFRate = MPFRateAsString;

					var line = new ACEEntryMessage7501Line//TODO
								(
									lineBlocks,
									MultipleOrigins,
									MultipleExport,
									MultipleLadings,
									MultipleManufacturers,
									MultipleRelationships,
									HasMultiExportDates,
									printInvoiceHeading,
									printInvoiceDetails,
									(ADDCVDBondBlockDetails != null ? ADDCVDBondBlockDetails.SuretyCompanyCode : ZString.Empty),
									dutyPercentageStrings,
									docData,
									docDataForChildLines,
									EntryType
								);
					entryPrintLines.Add(line);

					previousInvoicePK = docData.US_InvoicePK;
				}
			}

			return entryPrintLines;
		}

		//Checks if a current line is an ending line of an invoice 
		bool ShouldPrintInvoiceDetails(AENS40 currentENS40, ZGuid invoiceLinePK)
		{
			return DetermineShouldPrintInvoiceDetailsFlag(invoiceLinePK, () => aens40Lines.Find(x => CusEntryLine.GetNumericLineNumber(x.LineItemIdentifier) == CusEntryLine.GetNumericLineNumber(currentENS40.LineItemIdentifier) + 1));
		}

		protected override bool IsNAFTAReconIndicator
		{
			get { return aens10.TradeAgreementReconciliationIndicator == "Y"; }
		}

		protected override ZString ENSOtherIssueCode
		{
			get { return aens10.ReconciliationIssueCode; }
		}

		protected override ZString ENSOtherIssueCodeDescription
		{
			get { return Factory.GetCachedValue<ReconIssueCodeList>().GetDescriptionFromCode(ReconIssueCodeList.ConvertFromENSIssueCode(aens10.ReconciliationIssueCode)); }
		}

		#endregion
	}
}
